using BT_COMMONS.Database;
using BT_COMMONS;
using BT_COMMONS.DataRepositories;
using BT_COMMONS.Transactions;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace BT_POS.RepositoryImpl;

public class TransactionRepository : ITransactionRepository
{
    private readonly DatabaseAccess _database;
    private readonly APIAccess _api;

    public TransactionRepository(DatabaseAccess database, APIAccess api)
    {
        _database = database;
        _api = api;
    }

    public async Task<int?> GetPreviousTransactionId(int store, int register)
    {
        var transactions = await _database.LoadData<Transaction, dynamic>("SELECT transactionid FROM `transactions` WHERE `store`=? AND `register`=? ORDER BY `utid` DESC LIMIT 1;", new { store, register });
        if (transactions.Count == 0)
        {
            return 0;
        }

        return transactions[0].TransactionId;
    }

    public async Task<Transaction?> GetTransaction(int store, int register, int transId, DateOnly date)
    {
        var transactions = await _database.LoadData<Transaction, dynamic>("SELECT * FROM `transactions` WHERE `store`=? AND `register`=? AND `transactionid`=? AND `date`=? LIMIT 1;", new { store, register, transId, date });
        return null;
    }

    public async Task<bool> SubmitTransaction(Transaction trans, TransactionType? postTrans = null)
    {
        try
        {
            await _database.SaveData("INSERT INTO transactions (store, register, date, time, transactionid, type, operator, amount, basket, tenders, logs, posttranstype) " +
                "VALUES (@Store, @Register, @Date, @Time, @TransId, @Type, @Oper, @Amount, @Basket, @Tenders, @Logs, @PTT);",
                new { @Store = trans.Store, @Register = trans.Register, @Date = trans.DateTime.Date, @Time = trans.DateTime.ToLongTimeString(), @TransId = trans.TransactionId, @Type = trans.Type, @Oper = trans.Operator.OperatorId, @Amount = trans.GetTotal(), @Basket = JsonConvert.SerializeObject(trans.Basket), @Tenders = JsonConvert.SerializeObject(trans.Tenders), @Logs = JsonConvert.SerializeObject(trans.Logs), @PTT = (postTrans == null ? trans.Type : postTrans) });
            
            return true;
        } 
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);

            trans.Logs.Add(new TransactionLog(TransactionLogType.Hidden, "Transaction failed to submit to controller: " + ex.Message));

            string json = JsonConvert.SerializeObject(trans);
            File.WriteAllText("C:\\bubbletill\\offlinetransactions\\" + trans.TransactionId + ".json", json);

            return false;
        }
    }
}
