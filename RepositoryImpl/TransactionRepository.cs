using BT_COMMONS.Database;
using BT_COMMONS;
using BT_COMMONS.DataRepositories;
using BT_COMMONS.Transactions;
using BT_COMMONS.Transactions.API;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
        APIResponse<int> response = await _api.Get<int>("transaction/previous?store=" + store.ToString() + "&register=" + register.ToString());
        Trace.WriteLine(response.StatusCode);
        if (response.StatusCode != System.Net.HttpStatusCode.OK)
            return null;

        return response.Data;
    }

    public Task<bool> SubmitTransaction(Transaction transaction)
    {
        throw new NotImplementedException();
    }
}
