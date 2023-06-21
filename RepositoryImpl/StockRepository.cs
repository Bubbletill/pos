using BT_COMMONS.Database;
using BT_COMMONS;
using BT_COMMONS.DataRepositories;
using BT_COMMONS.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BT_POS.RepositoryImpl;

public class StockRepository : IStockRepository
{
    private readonly DatabaseAccess _database;
    private readonly APIAccess _api;

    public StockRepository(DatabaseAccess database, APIAccess api)
    {
        _database = database;
        _api = api;
    }

    public async Task<BasketItem?> GetItem(int code)
    {
        var items = await _database.LoadData<BasketItem, dynamic>("SELECT * FROM `stock` WHERE `code`=?;", new { code });
        if (items.Count == 0)
        {
            return null;
        }

        return items[0];
    }
}
