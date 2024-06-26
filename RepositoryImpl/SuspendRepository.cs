using BT_COMMONS;
using BT_COMMONS.DataRepositories;
using BT_COMMONS.Transactions;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BT_POS.RepositoryImpl;

public class SuspendRepository : ISuspendRepository
{

    private readonly DatabaseAccess _database;

    public SuspendRepository(DatabaseAccess database)
    {
        _database = database;
    }

    public async Task<List<Transaction>?> List(int store)
    {
        try
        {
            var transactions = await _database.LoadData<Transaction, dynamic>("SELECT * FROM suspends WHERE store=?;", new { store });
            if (transactions == null)
                return new List<Transaction>();

            return transactions;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            return new List<Transaction>();
        }
    }

    public async Task<Transaction?> Resume(int suspendId)
    {
        try
        {
            var transactions = await _database.LoadData<Transaction, dynamic>("SELECT * FROM suspends WHERE sid=? LIMIT 1;", new { suspendId });
            if (transactions == null || transactions[0] == null)
                return null;

            return transactions[0];
        } 
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            return null;
        }
    }

    public async Task<bool> Suspend(Transaction transaction)
    {
        try
        {
            string json = JsonConvert.SerializeObject(transaction);
            await _database.SaveData("INSERT INTO suspends (store, data) VALUES (@Store, @Data);", new { @Store = transaction.Store, @Data = json });

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);

            transaction.Logs.Add(new TransactionLog(TransactionLogType.Hidden, "Transaction failed to suspend to controller: " + ex.Message));

            string json = JsonConvert.SerializeObject(transaction);
            File.WriteAllText("C:\\bubbletill\\offlinesuspends\\" + transaction.TransactionId + ".json", json);

            return false;
        }
    }
}

