using BT_COMMONS.Database;
using BT_COMMONS;
using BT_COMMONS.DataRepositories;
using BT_COMMONS.Transactions;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Net.Sockets;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Diagnostics;

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
        var transactions = await _database.LoadData<TransactionEntry, dynamic>("SELECT * FROM `transactions` WHERE `store`=@Store AND `register`=@Register AND `transactionid`=@Id AND `date`=@Date LIMIT 1;", new { @Store=store, @Register=register, @Id=transId, @Date=date.ToString("yyyy-MM-dd") });
        if (transactions == null || transactions.Count == 0)
            return null;

        TransactionEntry entry = transactions[0];
        Transaction trxn = new Transaction();
        trxn.Store = entry.Store;
        trxn.Register = entry.Register;
        trxn.DateTime = entry.Date;
        trxn.TransactionId = entry.TransactionId;
        trxn.Type = entry.Type;
        trxn.Basket = JsonConvert.DeserializeObject<List<BasketItem>>(entry.Basket);
        trxn.Tenders = JsonConvert.DeserializeObject<Dictionary<TransactionTender, float>>(entry.Tenders);
        trxn.PostTransType = trxn.PostTransType;
        return trxn;
    }

    public async Task<bool> SubmitTransaction(Transaction trans, TransactionType? postTrans = null)
    {
        try
        {
            await _database.SaveData("INSERT INTO transactions (store, register, date, time, transactionid, type, operatorid, amount, basket, tenders, posttranstype) " +
                "VALUES (@Store, @Register, @Date, @Time, @TransId, @Type, @Oper, @Amount, @Basket, @Tenders, @PTT);",
                new { @Store = trans.Store, @Register = trans.Register, @Date = trans.DateTime.Date, @Time = trans.DateTime.ToLongTimeString(), @TransId = trans.TransactionId, @Type = trans.Type, @Oper = trans.Operator.OperatorId, @Amount = trans.GetTotal(), @Basket = JsonConvert.SerializeObject(trans.Basket), @Tenders = JsonConvert.SerializeObject(trans.Tenders), @PTT = (postTrans == null ? trans.Type : postTrans) });

            await _database.SaveData("INSERT INTO transaction_logs (store, register, date, transactionid, data) " +
    "VALUES (@Store, @Register, @Date, @TransId, @Data);",
    new { @Store = trans.Store, @Register = trans.Register, @Date = trans.DateTime.Date, @TransId = trans.TransactionId, @Data = JsonConvert.SerializeObject(trans.Logs) });

            if (trans.Type == TransactionType.RETURN || trans.Type == TransactionType.EXCHANGE)
            {
                foreach (var item in trans.ReturnBasket)
                {
                    item.Value.Locked = false;
                    await UpdateReturnEntry(item.Value);
                }
            }

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

    public async Task<ReturnEntry?> GetReturnEntry(int store, int register, int transId, DateOnly date)
    {
        try
        {
            var transaction = await _database.LoadData<ReturnEntry, dynamic>("SELECT * FROM `returns` WHERE `store`=@Store AND `register`=@Register AND `transactionid`=@Id AND `date`=@Date LIMIT 1;", new { @Store=store, @Register=register, @Id=transId, @Date= date.ToString("yyyy-MM-dd") });
            if (transaction == null || transaction.Count == 0)
                return null;

            transaction[0].ParsedBasket = JsonConvert.DeserializeObject<List<BasketItem>>(transaction[0].Basket);
            return transaction[0];
        } 
        catch (Exception ex) 
        {
            Console.WriteLine(ex.StackTrace);
            return null;
        }
    }

    public async Task<ReturnEntry?> GetReturnEntry(int urid)
    {
        try
        {
            var transaction = await _database.LoadData<ReturnEntry, dynamic>("SELECT * FROM `returns` WHERE `urid`=? LIMIT 1;", new { urid });
            if (transaction == null || transaction.Count == 0)
                return null;

            transaction[0].ParsedBasket = JsonConvert.DeserializeObject<List<BasketItem>>(transaction[0].Basket);
            return transaction[0];
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            return null;
        }
    }

    public async Task<int> SubmitReturnEntry(ReturnEntry entry)
    {
        try
        {
            await _database.SaveData("INSERT INTO returns (store, register, date, transactionid, basket, locked) " +
                "VALUES (@Store, @Register, @Date, @TransId, @Basket, @Locked);",
                new { @Store = entry.Store, @Register = entry.Register, @Date = entry.Date.ToString("yyyy-MM-dd"), @TransId = entry.TransactionId, @Basket = JsonConvert.SerializeObject(entry.ParsedBasket), @Locked = entry.Locked});

            ReturnEntry? addedEntry = await GetReturnEntry(entry.Store, entry.Register, entry.TransactionId, DateOnly.FromDateTime(entry.Date));
            if (addedEntry == null)
            {
                return -1;
            }

            return addedEntry.Urid;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);

            return -1;
        }
    }

    public async Task<bool> ToggleReturnLock(int urid, bool locked)
    {
        try
        {
            await _database.SaveData("UPDATE returns SET locked=? WHERE urid=?;",
                new { locked, urid });

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);

            return false;
        }
    }

    public async Task<bool> UpdateReturnEntry(ReturnEntry entry)
    {
        try
        {
            await _database.SaveData("UPDATE returns SET basket=@Basket, locked=@Locked WHERE urid=@Urid",
                new { @Basket = JsonConvert.SerializeObject(entry.ParsedBasket), @Locked = entry.Locked, @Urid = entry.Urid });

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);

            return false;
        }
    }
}
