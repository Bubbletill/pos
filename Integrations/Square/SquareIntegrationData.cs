using Square;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BT_POS.Integrations.Square;

public class SquareIntegrationData
{
    public SquareClient Client { get; set; }
    public string APIKey { get; set; }
    public string TerminalDeviceCode { get; set; }
    public string TerminalDeviceId { get; set; }
}
