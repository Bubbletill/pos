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
            TerminalCheckoutWebhook tcw = JsonConvert.DeserializeObject<TerminalCheckoutWebhook>(data);
            if (tcw.type == "terminal.checkout.created")
            {
                if (CardHandler != null)
                    return;

                TerminalCheckout checkout = tcw.data.obj.checkout;
                CheckoutId = checkout.Id;
                CardHandler = new SquareCardHandler(CheckoutId);
                CardHandler.InfoText.Text = "Please following the instructions on the card reader.";
                CardHandler.CancelButton.IsEnabled = true;
                controller.CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.Hidden, "Started SquareUp Payment"));
                CardHandler.Show();
            } 
            else if (tcw.type == "terminal.checkout.updated")
            {
                if (CardHandler == null)
                    return;

                TerminalCheckout checkout = tcw.data.obj.checkout;
                if (checkout.Status == "COMPLETED")
                {
                    CardHandler.InfoText.Text = "Approved. Please wait...";
                    CardHandler.CancelButton.IsEnabled = false;

                    string amt = checkout.AmountMoney.Amount.ToString();
                    amt = amt.Insert(amt.Length - 2, ".");

                    float amount = float.Parse(amt);


                    GetPaymentResponse paymentResponse = App.squareIntegrationData.Client.PaymentsApi.GetPayment(checkout.PaymentIds[0]);
                    Payment payment = paymentResponse.Payment;

                    controller.CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.PostTender, payment.CardDetails.Card.CardBrand + " " + payment.CardDetails.Card.CardType + " XXXX XXXX XXXX " + payment.CardDetails.Card.Last4));
                    controller.CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.PostTender, "Auth Code: " + payment.CardDetails.AuthResultCode + " Receipt No: " + payment.ReceiptNumber));
                    controller.CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.PostTender, "AID: " + payment.CardDetails.ApplicationIdentifier));
                    controller.CurrentTransaction!.Logs.Add(new TransactionLog(TransactionLogType.PostTender, "Please retain for your records."));

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
        });
    }

}
