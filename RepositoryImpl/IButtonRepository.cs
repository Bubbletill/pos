using BT_COMMONS.Transactions;
using BT_POS.Buttons.Admin;
using BT_POS.Buttons.Admin.CashMngt;
using BT_POS.Buttons.ItemMod;
using BT_POS.Buttons.Menu;
using BT_POS.Buttons.TransMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BT_POS.RepositoryImpl;

public interface IButtonRepository
{
    Task<List<TransactionTender>?> GetTenderTypes();

    Task<List<HomeButton>?> GetHomeButtons();
    Task<List<HomeButton>?> GetHomeTransButtons();
    Task<List<ItemModButton>?> GetItemModButtons();
    Task<List<TransModButton>?> GetTransModButtons();
    Task<List<AdminButton>?> GetAdminButtons();
    Task<List<CashManagementButton>?> GetAdminCashManagementButtons();
}
