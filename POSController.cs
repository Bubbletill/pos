using BT_COMMONS.Database;
using BT_COMMONS.Operators;
using BT_COMMONS.Transactions;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace BT_POS;

public class POSController
{

    private readonly IAPIAccess _apiAccess;
    public string? ControllerAuthenticationToken { get; set; }

    public bool OnlineToController { get; set; }

    public readonly int RegisterNumber;

    public Operator? CurrentOperator { get; set; }
    public Transaction? CurrentTransaction { get; set; }

    public POSController(IAPIAccess apiAccess)
    {
        _apiAccess = apiAccess;
        RegisterNumber = 1;
    }

    public async Task<bool> CompleteLogin()
    {
        _apiAccess.UpdateWithToken(ControllerAuthenticationToken);

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(ControllerAuthenticationToken);
        var id = token.Claims.FirstOrDefault(c => c.Type == "id")?.Value;

        var response = await _apiAccess.Get<Operator>("operator/" + id);
        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            CurrentOperator = response.Data;
        }

        if (CurrentOperator == default)
        {
            return false;
        }

        return true;
    }
}
