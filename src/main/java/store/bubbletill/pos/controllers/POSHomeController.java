package store.bubbletill.pos.controllers;

import javafx.application.Platform;
import javafx.fxml.FXML;
import javafx.fxml.FXMLLoader;
import javafx.scene.Scene;
import javafx.scene.control.*;
import javafx.scene.control.cell.PropertyValueFactory;
import javafx.scene.input.KeyCombination;
import javafx.scene.input.KeyEvent;
import javafx.scene.layout.Pane;
import javafx.scene.text.Font;
import javafx.stage.Stage;
import org.apache.http.HttpResponse;
import org.apache.http.client.HttpClient;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.entity.ContentType;
import org.apache.http.entity.StringEntity;
import org.apache.http.impl.client.HttpClientBuilder;
import org.apache.http.util.EntityUtils;
import store.bubbletill.pos.POSApplication;
import store.bubbletill.commons.*;
import store.bubbletill.pos.views.AdminView;
import store.bubbletill.pos.views.HomeTenderView;
import store.bubbletill.pos.views.OpeningFloatView;
import store.bubbletill.pos.views.ResumeView;

import java.time.LocalDateTime;
import java.util.Map;
import java.util.Timer;
import java.util.TimerTask;

public class POSHomeController {

    // Home Tender View
    @FXML private Pane mainHome;
    @FXML private Pane preTransButtons;
    @FXML private Pane transStartedButtons;
    @FXML private Pane tenderButtons;
    @FXML private Pane transModButtons;
    @FXML private Label categoryInputLabel;
    @FXML private TextField categoryInputField;
    @FXML private TextField itemcodeInputField;
    @FXML private Pane homeItemInputPane;
    @FXML private ListView<String> basketListView;

    @FXML private Pane homeCostsPane;
    @FXML private Pane homeCostsTenderPane;
    @FXML private Label homeTenderTotalLabel;
    @FXML private Label homeTenderTenderLabel;
    @FXML private Label homeTenderRemainLabel;

    @FXML private Button tenderButton;
    @FXML private Button tenderCashButton;
    @FXML private Button tenderCardButton;
    @FXML private Button tenderBackButton;
    @FXML private Button itemModButton;
    @FXML private Button transModButton;
    @FXML private Button suspendButton;
    @FXML private Button transModVoidButton;
    @FXML private Button transModBackButton;
    @FXML private Button logoutButton;
    @FXML private Button adminButton;

    // Resume View
    @FXML private Pane resumeTrans;
    @FXML private TableView<SuspendedListData> resumeTable;

    @FXML private Button homeResumeButton;
    @FXML private Button rtResumeButton;
    @FXML private Button rtBackButton;

    // Opening float view
    @FXML private Pane declareOpeningFloat;
    @FXML private Pane dofPrompt;
    @FXML private Pane dofDeclare;
    @FXML private TextField dof50;
    @FXML private TextField dof20;
    @FXML private TextField dof10;
    @FXML private TextField dof5;
    @FXML private TextField dof1;
    @FXML private TextField dof50p;
    @FXML private TextField dof20p;
    @FXML private TextField dof10p;
    @FXML private TextField dof5p;
    @FXML private TextField dof2p;
    @FXML private TextField dof1p;

    @FXML private Button openingFloatYesButton;
    @FXML private Button openingFloatNoButton;
    @FXML private Button openingFloatSubmitButton;

    // Admin View
    @FXML private Pane adminPane;
    @FXML private Button adminNoSaleButton;
    @FXML private Button adminPostVoidButton;
    @FXML private Button adminXReadButton;
    @FXML private Button adminResyncDatabaseButton;
    @FXML private Button adminBackButton;

    // Top status bar
    @FXML private Label dateTimeLabel;
    @FXML private Label statusLabel;
    @FXML private Label registerLabel;
    @FXML private Label transactionLabel;
    @FXML private Label operatorLabel;
    @FXML private Pane errorPane;
    @FXML private Label errorLabel;

    // Views
    public HomeTenderView homeTenderView;
    public OpeningFloatView openingFloatView;
    public ResumeView resumeView;
    public AdminView adminView;

    private POSApplication app;

    @FXML
    private void initialize() {
        app = POSApplication.getInstance();

        // Set up the views!
        homeTenderView = new HomeTenderView(app, this, transactionLabel, mainHome, preTransButtons,
                transStartedButtons, tenderButtons, transModButtons, categoryInputLabel, categoryInputField,
                itemcodeInputField, homeItemInputPane, basketListView, homeCostsPane, homeCostsTenderPane,
                homeTenderTotalLabel, homeTenderTenderLabel, homeTenderRemainLabel, tenderCashButton, tenderCardButton,
                tenderBackButton, tenderButton, itemModButton, transModButton, suspendButton, transModVoidButton,
                transModBackButton, logoutButton, homeResumeButton, adminButton);

        openingFloatView = new OpeningFloatView(app, this, declareOpeningFloat, dofPrompt, dofDeclare, dof50,
                dof20, dof10, dof5, dof1, dof50p, dof20p, dof10p, dof5p, dof2p, dof1p, openingFloatYesButton,
                openingFloatNoButton, openingFloatSubmitButton);

        resumeView = new ResumeView(app, this, resumeTrans, resumeTable, rtResumeButton, rtBackButton);

        adminView = new AdminView(app, this, adminPane, adminNoSaleButton, adminPostVoidButton,
                adminXReadButton, adminResyncDatabaseButton, adminBackButton);

        if (app.cashInDraw == -9999) {
            homeTenderView.hide();
            openingFloatView.show();
            POSApplication.buzzer("double");
        } else {
            homeTenderView.show();
            openingFloatView.hide();
        }

        resumeView.hide();
        adminView.hide();

        errorPane.setVisible(false);


        if (app.dateTimeTimer != null)
            app.dateTimeTimer.cancel();

        app.dateTimeTimer = new Timer();
        app.dateTimeTimer.scheduleAtFixedRate(new TimerTask() {
            @Override
            public void run() {
                Platform.runLater(() -> {
                    dateTimeLabel.setText(Formatters.dateTimeFormatter.format(LocalDateTime.now()));
                });
            }
        }, 0, 5000);

        statusLabel.setText((app.workingOnline ? "Online" : "Offline"));
        registerLabel.setText("" + app.register);
        transactionLabel.setText("" + app.transNo);
        operatorLabel.setText(app.operator.getOperatorId());


        // Resume trans?
        if (app.transaction != null) {
            app.transaction.log("Transaction resumed at " + Formatters.dateTimeFormatter.format(LocalDateTime.now()));
            for (StockData stockData : app.transaction.getBasket()) {
                basketListView.getItems().add("[" + app.getCategory(stockData.getCategory()).getMessage() + "] " + stockData.getDescription() + " - ??" + Formatters.decimalFormatter.format(stockData.getPrice()));
            }
            homeTenderTotalLabel.setText("??" + Formatters.decimalFormatter.format(app.transaction.getBasketTotal()));
        }
    }

    public Stage getStage() {
        return (Stage) dateTimeLabel.getScene().getWindow();
    }

    public void showError(String error) {
        showError(error, true);
    }

    public void showError(String error, boolean buzzer) {
        if (error == null) {
            errorPane.setVisible(false);
            return;
        }

        errorPane.setVisible(true);
        errorLabel.setText(error);
        if (buzzer)
            POSApplication.buzzer("double");
    }

    public void resumeTransaction(int uniqueSuspendedId) {
        Transaction resumeData;
        try {
            HttpClient httpClient = HttpClientBuilder.create().build();

            StringEntity requestEntity = new StringEntity(
                    "{\"usid\":\"" + uniqueSuspendedId + "\", \"token\":\"" + POSApplication.getInstance().accessToken + "\"}",
                    ContentType.APPLICATION_JSON);

            HttpPost postMethod = new HttpPost(POSApplication.backendUrl + "/pos/resume");
            postMethod.setEntity(requestEntity);

            HttpResponse rawResponse = httpClient.execute(postMethod);
            String out = EntityUtils.toString(rawResponse.getEntity());

            resumeData = POSApplication.gson.fromJson(out, Transaction.class);
        } catch (Exception e) {
            e.printStackTrace();
            return;
        }

        resumeTrans.setVisible(false);
        mainHome.setVisible(true);
        app.transNo++;
        app.transaction = new Transaction(app.transNo);
        app.transaction.setBasket(resumeData.getBasket());
        app.transaction.setLogs(resumeData.getLogs());
        app.transaction.log("Transaction resumed at " + Formatters.dateTimeFormatter.format(LocalDateTime.now()));
        transactionLabel.setText("" + app.transNo);
        transStartedButtons.setVisible(true);
        preTransButtons.setVisible(false);
        homeTenderTotalLabel.setText("??" + Formatters.decimalFormatter.format(app.transaction.getBasketTotal()));

        for (StockData stockData : resumeData.getBasket()) {
            basketListView.getItems().add("[" + app.getCategory(stockData.getCategory()).getMessage() + "] " + stockData.getDescription() + " - ??" + Formatters.decimalFormatter.format(stockData.getPrice()) + "\n" + stockData.getCategory() + " / " + stockData.getItemCode());
        }


    }

    public void performXRead() {
        showError("Performing X Read...", false);
        TransactionListData[] listData;
        try {
            HttpClient httpClient = HttpClientBuilder.create().build();

            String todaysDate = Formatters.dateFormatter.format(LocalDateTime.now());
            StringEntity requestEntity = new StringEntity(
                    "{"
                            + "\"store\": \"" + app.store
                            + "\", \"startDate\": \"" + todaysDate
                            + "\", \"endDate\": \"" + todaysDate
                            + "\", \"startTime\": \"" + "00:00"
                            + "\", \"endTime\": \"" + "23:59"
                            + "\", \"register\": \"" + app.register
                            + "\", \"operator\": \"" + ""
                            + "\", \"startTotal\": \"" + Double.MIN_VALUE
                            + "\", \"endTotal\": \"" + Double.MAX_VALUE
                            + "\", \"token\" :\"" + app.accessToken
                            + "\"}",
                    ContentType.APPLICATION_JSON);

            HttpPost postMethod = new HttpPost(POSApplication.backendUrl + "/bo/listtransactions");
            postMethod.setEntity(requestEntity);

            HttpResponse rawResponse = httpClient.execute(postMethod);
            String out = EntityUtils.toString(rawResponse.getEntity());

            listData = POSApplication.gson.fromJson(out, TransactionListData[].class);

            if (listData.length == 0) {
                showError("Cannot perform X Read on 0 transactions");
                return;
            }
        } catch (Exception e) {
            e.printStackTrace();
            showError(e.getMessage());
            return;
        }

        XReadData data = new XReadData(app.store, app.register, app.operator.getOperatorId());

        data.setTransactionCount(listData.length);
        data.setSystemCashInDraw(app.cashInDraw);
        data.setRegOpened("NA");
        data.setRegClosed("NA");
        for (TransactionListData listItem : listData) {
            //Transaction items = POSApplication.gson.fromJson(listItem.getItems(), Transaction.class);
            Transaction items = new Transaction(9999);

            if (items.isVoided()) {
                data.incrementTransVoidTotal();

                data.getTotalPerTransactionType().putIfAbsent(listItem.getType(), 0.0);
                data.getTotalPerTransactionType().put(listItem.getType(), data.getTotalPerTransactionType().get(listItem.getType()) + listItem.getTotal());

                continue;
            }

            // Totals
            data.incrementGrandTotal(listItem.getTotal());
            data.incrementUnitsSold(items.getBasket().size());

            // Total per category
            for (StockData stockData : items.getBasket()) {
                data.getTotalPerCategory().putIfAbsent(stockData.getCategory(), 0.0);
                data.getTotalPerCategory().put(stockData.getCategory(), data.getTotalPerCategory().get(stockData.getCategory()) + stockData.getPrice());
            }

            // Total per payment type
            for (Map.Entry<PaymentType, Double> e : items.getTender().entrySet()) {
                data.getTotalPerPaymentType().putIfAbsent(e.getKey(), 0.0);
                data.getTotalPerPaymentType().put(e.getKey(), data.getTotalPerPaymentType().get(e.getKey()) + e.getValue());
            }

            // Total per transaction type
            data.getTotalPerTransactionType().putIfAbsent(listItem.getType(), 0.0);
            data.getTotalPerTransactionType().put(listItem.getType(), data.getTotalPerTransactionType().get(listItem.getType()) + listItem.getTotal());

            // Cash in draw
            double change = 0;
            if (items.getTender().containsKey(PaymentType.CASH)) {
                data.incrementCashInDraw(items.getTender().get(PaymentType.CASH));

                if (items.getTender().get(PaymentType.CASH) > items.getBasketTotal())
                    change = items.getTender().get(PaymentType.CASH) - items.getBasketTotal();
            }
            data.subtractCashInDraw(change);
        }

        String unformatted = POSApplication.gson.toJson(data);
        System.out.println(unformatted);
        try {
            HttpClient httpClient = HttpClientBuilder.create().build();
            String items = unformatted.replaceAll("\"", "\\\\\"");
            StringEntity requestEntity = new StringEntity(
                    "{"
                            + "\"data\": \"" + items
                            + "\"}",
                    ContentType.APPLICATION_JSON);

            HttpPost postMethod = new HttpPost("http://localhost:5001/print/xread");
            postMethod.setEntity(requestEntity);

            HttpResponse rawResponse = httpClient.execute(postMethod);
            showError(null);
        } catch (Exception e) {
            e.printStackTrace();
            showError(e.getMessage());
        }
    }
}
