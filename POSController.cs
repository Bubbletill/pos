using BT_COMMONS.DataRepositories;
using BT_COMMONS.Operators;
using BT_COMMONS.Transactions;
using BT_POS.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace BT_POS;

public class POSController
{

    private readonly IOperatorRepository _operatorRepository;
    private readonly ITransactionRepository _transactionRepository;
    public string? ControllerAuthenticationToken { get; set; }
    public bool GotInitialControllerData { get; set; } = false;

    public bool OnlineToController { get; set; }

    public readonly int StoreNumber;
    public readonly int RegisterNumber;

    public Operator? CurrentOperator { get; set; }
    public Transaction? CurrentTransaction { get; set; }
    public int CurrentTransId = 0;

    public POSController(IOperatorRepository operatorRepository, ITransactionRepository transactionRepository)
    {
        _operatorRepository = operatorRepository;
        _transactionRepository = transactionRepository;
        StoreNumber = 1;
        RegisterNumber = 1;
    }

    public void HeaderError(string? error = null)
    {
        MainWindow mw = App.AppHost.Services.GetRequiredService<MainWindow>();
        mw.HeaderError(error);
    }

    public async Task<bool> CompleteLogin()
    {
        App.SetAPIToken(ControllerAuthenticationToken);

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(ControllerAuthenticationToken);
        var id = token.Claims.FirstOrDefault(c => c.Type == "id")?.Value;

        var oper = await _operatorRepository.GetOperator(Int32.Parse(id));
        if (oper == null)
        {
            return false;
        }

        if (!GotInitialControllerData)
        {
            int? prevTrans = await _transactionRepository.GetPreviousTransactionId(StoreNumber, RegisterNumber);

            if (prevTrans == null)
                return false;

            CurrentTransId = prevTrans.Value;
        }

        CurrentOperator = oper;

        return true;
    }

    public void AddItemToBasket(BasketItem item)
    {
        if (CurrentTransaction == null)
        {
            CurrentTransId++;
            CurrentTransaction = new Transaction();
            CurrentTransaction.Init(StoreNumber, RegisterNumber, DateOnly.FromDateTime(DateTime.Now), TimeOnly.FromDateTime(DateTime.Now), CurrentTransId, TransactionType.SALE);

            MainWindow mw = App.AppHost.Services.GetRequiredService<MainWindow>();
            mw.POSParentHeader_Trans.Text = "Transaction# " + CurrentTransId;
        }

        CurrentTransaction.AddToBasket(item);
    }
}
