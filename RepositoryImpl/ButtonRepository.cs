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

    public async Task<List<POSMenuButton>?> GetHomeButtons()
    {
        var tables = await _database.LoadData<string, dynamic>("SELECT buttons FROM `buttons` WHERE `menu`=\"home\";", new { });
        if (tables.Count == 0)
        {
            return null;
        }

        Trace.WriteLine(tables[0]);
        var buttons = JsonConvert.DeserializeObject<List<POSMenuButton>>(tables[0]);
        return buttons;
    }

    public async Task<List<POSMenuButton>?> GetHomeTransButtons()
    {
        var tables = await _database.LoadData<string, dynamic>("SELECT buttons FROM `buttons` WHERE `menu`=\"home_trans\";", new { });
        if (tables.Count == 0)
        {
            return null;
        }

        Trace.WriteLine(tables[0]);
        var buttons = JsonConvert.DeserializeObject<List<POSMenuButton>>(tables[0]);
        return buttons;
    }
}
