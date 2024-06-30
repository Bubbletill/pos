using BT_COMMONS;
using BT_COMMONS.DataRepositories;
using BT_COMMONS.Transactions;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    public async Task<List<SuspendEntry>?> List(int store)
    {
        try
        {
            var transactions = await _database.LoadData<SuspendEntry, dynamic>("SELECT sid, data FROM suspends WHERE store=?;", new { store });
            if (transactions == null)
                return new List<SuspendEntry>();

            List<SuspendEntry> result = new List<SuspendEntry>();
            foreach (var item in transactions)
            {
                Transaction trans = JsonConvert.DeserializeObject<Transaction>(item.Data);
                item.Transaction = trans;
                result.Add(item);
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            return new List<SuspendEntry>();
        }
    }

    public async Task<bool> Delete(int suspendId)
    {
        try
        {
            await _database.SaveData("DELETE FROM suspends WHERE sid=?", new { suspendId });

            return true;
        } 
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            return false;
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

