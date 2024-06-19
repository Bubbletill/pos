using BT_COMMONS.Database;
using BT_COMMONS;
using BT_POS.Buttons.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;
using BT_POS.Buttons.Admin;
using BT_POS.Buttons.Admin.CashMngt;
using BT_POS.Buttons.ItemMod;
using BT_POS.Buttons.TransMod;

namespace BT_POS.RepositoryImpl;

public class ButtonRepository : IButtonRepository
{
    private readonly DatabaseAccess _database;
    private readonly APIAccess _api;

    public ButtonRepository(DatabaseAccess database, APIAccess api)
    {
        _database = database;
        _api = api;
    }

    public async Task<List<HomeButton>?> GetHomeButtons()
    {
        var tables = await _database.LoadData<string, dynamic>("SELECT buttons FROM `buttons` WHERE `menu`=\"home\";", new { });
        if (tables.Count == 0)
        {
            return null;
        }

        Trace.WriteLine(tables[0]);
        var buttons = JsonConvert.DeserializeObject<List<HomeButton>>(tables[0]);
        return buttons;
    }

    public async Task<List<HomeButton>?> GetHomeTransButtons()
    {
        var tables = await _database.LoadData<string, dynamic>("SELECT buttons FROM `buttons` WHERE `menu`=\"home_trans\";", new { });
        if (tables.Count == 0)
        {
            return null;
        }

        Trace.WriteLine(tables[0]);
        var buttons = JsonConvert.DeserializeObject<List<HomeButton>>(tables[0]);
        return buttons;
    }

    public async Task<List<ItemModButton>?> GetItemModButtons()
    {
        var tables = await _database.LoadData<string, dynamic>("SELECT buttons FROM `buttons` WHERE `menu`=\"item_mod\";", new { });
        if (tables.Count == 0)
        {
            return null;
        }

        Trace.WriteLine(tables[0]);
        var buttons = JsonConvert.DeserializeObject<List<ItemModButton>>(tables[0]);
        return buttons;
    }

    public async Task<List<TransModButton>?> GetTransModButtons()
    {
        var tables = await _database.LoadData<string, dynamic>("SELECT buttons FROM `buttons` WHERE `menu`=\"trans_mod\";", new { });
        if (tables.Count == 0)
        {
            return null;
        }

        Trace.WriteLine(tables[0]);
        var buttons = JsonConvert.DeserializeObject<List<TransModButton>>(tables[0]);
        return buttons;
    }

    public async Task<List<AdminButton>?> GetAdminButtons()
    {
        var tables = await _database.LoadData<string, dynamic>("SELECT buttons FROM `buttons` WHERE `menu`=\"admin\";", new { });
        if (tables.Count == 0)
        {
            return null;
        }

        Trace.WriteLine(tables[0]);
        var buttons = JsonConvert.DeserializeObject<List<AdminButton>>(tables[0]);
        return buttons;
    }

    public async Task<List<CashManagementButton>?> GetAdminCashManagementButtons()
    {
        var tables = await _database.LoadData<string, dynamic>("SELECT buttons FROM `buttons` WHERE `menu`=\"admincashmngt\";", new { });
        if (tables.Count == 0)
        {
            return null;
        }

        Trace.WriteLine(tables[0]);
        var buttons = JsonConvert.DeserializeObject<List<CashManagementButton>>(tables[0]);
        return buttons;
    }
}
