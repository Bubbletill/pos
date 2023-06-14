using BT_COMMONS.DataRepositories;
using BT_COMMONS.Operators;
using BT_COMMONS.Transactions;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace BT_POS;

public class POSController
{

    private readonly IOperatorRepository _operatorRepository;
    public string? ControllerAuthenticationToken { get; set; }

    public bool OnlineToController { get; set; }

    public readonly int RegisterNumber;

    public Operator? CurrentOperator { get; set; }
    public Transaction? CurrentTransaction { get; set; }

    public POSController(IOperatorRepository operatorRepository)
    {
        _operatorRepository = operatorRepository;
        RegisterNumber = 1;
    }

    public async Task<bool> CompleteLogin()
    {
        App.ControllerToken = ControllerAuthenticationToken;

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(ControllerAuthenticationToken);
        var id = token.Claims.FirstOrDefault(c => c.Type == "id")?.Value;

        var oper = await _operatorRepository.GetOperator(Int32.Parse(id));
        if (oper == null)
        {
            return false;
        }

        CurrentOperator = oper;

        return true;
    }
}
