using BT_COMMONS.Database;
using BT_COMMONS;
using BT_COMMONS.DataRepositories;
using BT_COMMONS.Hotshot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BT_POS.RepositoryImpl;

public class HotshotRepository : IHotshotRepository
{
    private readonly DatabaseAccess _database;

    public HotshotRepository(DatabaseAccess database)
    {
        _database = database;
    }

    public async Task<List<HotshotCategory>> GetHotshotCategories()
    {
        List<HotshotCategory> categories = await _database.LoadData<HotshotCategory, dynamic>("SELECT * FROM `hotshot_categories`;", new { });
        List<HotshotItem> items = await _database.LoadData<HotshotItem, dynamic>("SELECT * FROM `hotshot_items`;", new { });

        foreach (HotshotCategory cat in categories)
        {
            foreach (HotshotItem item in items)
            {
                if (item.catid == cat.hscatid)
                    cat.items.Add(item);
            }

            items.RemoveAll((item) => item.catid == cat.hscatid);
        }

        return categories;
    }
}
