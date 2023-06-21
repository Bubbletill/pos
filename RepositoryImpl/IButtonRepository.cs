using BT_POS.Buttons.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BT_POS.RepositoryImpl;

public interface IButtonRepository
{
    Task<List<POSMenuButton>?> GetHomeButtons();
    Task<List<POSMenuButton>?> GetHomeTransButtons();
}
