using BT_COMMONS.Integrations.Square;
using BT_COMMONS.Transactions;
using BT_POS.Components;
using BT_POS.Views.Tender;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Square;
using Square.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BT_POS.Integrations.Square;

public class SquareIntegrationData
{
    public SquareClient Client { get; set; }
    public string APIKey { get; set; }
    public string TerminalDeviceCode { get; set; }
    public string TerminalDeviceId { get; set; }

    public SquareCardHandler? CardHandler { get; set; }
    public string CheckoutId { get; set; }

    public Process? IntegrationProcess { get; set; }

    public void RecieveCheckoutWebhook(string data)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            POSController controller = App.AppHost.Services.GetRequiredService<POSController>();
            dynamic dyn = JsonConvert.DeserializeObject(data);
            if (dyn == null || dyn.type == null)
            {
                controller.HeaderError("SQAPI Error: Invalid webhook data recieved.");
                return;
            }

            if (dyn.type == "terminal.checkout.created")
                TerminalCheckoutCreated(data, controller);
            else if (dyn.type == "terminal.checkout.updated")
                TerminalCheckoutUpdated(data, controller);
            else if (dyn.type == "payment.created" || dyn.type == "payment.updatd")
                PaymentCreatedUpdated(data, controller);
        });
    }

    private void TerminalCheckoutCreated(string data, POSController controller)
    {
        TerminalCheckoutWebhook tcw = JsonConvert.DeserializeObject<TerminalCheckoutWebhook>(data);
        if (CardHandler != null)
            return;

        TerminalCheckout checkout = tcw.data.obj.checkout;

        if (checkout.DeviceOptions.DeviceId != App.squareIntegrationData.TerminalDeviceId)
            return;

        CheckoutId = checkout.Id;
        ProcessedPaymentIds = new List<string>();
        CardHandler = new SquareCardHandler(CheckoutId);
        CardHandler.InfoText.Text = "Please following the instructions on the card reader.";
        CardHandler.CancelButton.IsEnabled = true;
        controller.CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.Hidden, "Started SquareUp Payment"));
        CardHandler.Show();
    }

    private void TerminalCheckoutUpdated(string data, POSController controller)
    {
        TerminalCheckoutWebhook tcw = JsonConvert.DeserializeObject<TerminalCheckoutWebhook>(data);
        if (CardHandler == null)
            return;

        TerminalCheckout checkout = tcw.data.obj.checkout;

        if (checkout.DeviceOptions.DeviceId != App.squareIntegrationData.TerminalDeviceId)
            return;

        if (checkout.Status == "COMPLETED")
        {
            CardHandler.InfoText.Text = "Approved. Please wait...";
            CardHandler.CancelButton.IsEnabled = false;

            string amt = checkout.AmountMoney.Amount.ToString();
            amt = amt.Insert(amt.Length - 2, ".");

            float amount = float.Parse(amt);
            try
            {
                if (checkout.PaymentIds[0] != null)
                    controller.CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.Hidden, "SquareUp: Payment ID " + checkout.PaymentIds[0]));

                GetPaymentResponse paymentResponse = App.squareIntegrationData.Client.PaymentsApi.GetPayment(checkout.PaymentIds[0]);
                Payment payment = paymentResponse.Payment;

                controller.CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.Tender, payment!.CardDetails.Card.CardBrand + " " + payment.CardDetails.Card.CardType + " XXXX XXXX XXXX " + payment.CardDetails.Card.Last4));
                controller.CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.Tender, "Auth Code: " + payment.CardDetails.AuthResultCode + " Receipt No: " + payment.ReceiptNumber));
                controller.CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.Tender, "AID: " + payment.CardDetails.ApplicationIdentifier));
                controller.CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.Tender, "Please retain for your records."));
            } 
            catch (Exception ex)
            {
                controller.CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.Tender, "SquareUp: failed to get payment information: " + ex.Message));
            }

            controller.AddTender(TransactionTender.SQUARE_CARD, amount);

            CardHandler.Close();
            CardHandler = null;
            CheckoutId = "";
        }
        else if (checkout.Status == "CANCELED")
        {
            CardHandler.Close();
            CardHandler = null;
            CheckoutId = "";
            MainWindow mainWindow = App.AppHost.Services.GetRequiredService<MainWindow>();
            TenderHomeView tenderHome = App.AppHost.Services.GetRequiredService<TenderHomeView>();
            mainWindow.POSViewContainer.Content = tenderHome;
            string amt = checkout.AmountMoney.Amount.ToString();
            amt = amt.Insert(amt.Length - 2, ".");
            controller.HeaderError("Transaction Cancelled: " + checkout.CancelReason);
            controller.CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.Hidden, "SquareUp Payment Cancelled: " + checkout.CancelReason));
        }
        else if (checkout.Status == "CANCEL_REQUESTED")
        {
            CardHandler.InfoText.Text = "Cancelling transaction...";
            CardHandler.CancelButton.IsEnabled = false;
        }
        else
        {
            CardHandler.InfoText.Text = "Please following the instructions on the card reader.";
            CardHandler.CancelButton.IsEnabled = true;
        }
    }

    private void PaymentCreatedUpdated(string data, POSController controller)
    {
        PaymentWebhook pw = JsonConvert.DeserializeObject<PaymentWebhook>(data);
        if (CardHandler == null)
            return;

        Payment payment = pw.data.obj.payment;
        if (payment.DeviceDetails == null || payment.DeviceDetails.DeviceId == null || payment.DeviceDetails.DeviceId != App.squareIntegrationData.TerminalDeviceId)
            return;

        if (payment.Status == "APPROVED")
        {
            CardHandler.InfoText.Text = "Card Approved\nPlease wait...";
            controller.CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.Hidden, "SquareUp Payment Approved"));
        } 

        if (payment.Status != "FAILED")
            return;

        if (payment.CardDetails.Errors.Count == 0)
            return;

        Error error = payment.CardDetails.Errors[0];
        controller.CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.Hidden, "SquareUp: " + error.Detail));
        switch (error.Code)
        {
            case "INSUFFICIENT_FUNDS":
                {
                    CardHandler.InfoText.Text = "Card Declined: insufficient funds.\nPlease following the instructions on the card reader.";
                    break;
                }
            case "CARD_DECLINED_VERIFICATION_REQUIRED":
                {
                    CardHandler.InfoText.Text = "Card Declined: pin verification required.\nPlease following the instructions on the card reader.";
                    break;
                }
            default:
                {
                    CardHandler.InfoText.Text = error.Detail + "\nPlease following the instructions on the card reader.";
                    break;
                }
        }
    }

}
