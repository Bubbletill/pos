using BT_COMMONS;
using BT_COMMONS.Database;
using BT_COMMONS.DataRepositories;
using BT_COMMONS.Operators;
using BT_COMMONS.Operators.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BT_POS.RepositoryImpl;

public class OperatorRepository : IOperatorRepository
{
    private readonly DatabaseAccess _database;
    private readonly APIAccess _api;

    public OperatorRepository(DatabaseAccess database, APIAccess api)
    {
        _database = database;
        _api = api;
    }

    public async Task<Operator?> GetOperator(int id)
    {
        var opers = await _database.LoadData<Operator, dynamic>("SELECT id, isactive, operatorid, operatorpassword, istemppassword, firstname, nickname, lastname, groupsid, dateofbirth, hiredate, terminationdate FROM `operators` WHERE `id`=?;", new { id });
        if (opers.Count == 0)
        {
            return null;
        }

        var oper = opers[0].parse();

        return oper;
    }

    public async Task<OperatorLoginResponse> OperatorLogin(OperatorLoginRequest request)
    {
        //APIResponse<OperatorLoginResponse> loginResponse = await _api.Post<OperatorLoginResponse, OperatorLoginRequest>("operator/login", request);
        //return loginResponse.Data;

        var opers = await _database.LoadData<Operator, dynamic>("SELECT id, isactive, operatorid, operatorpassword, groupsid FROM `operators` WHERE `operatorid`=?;", new { request.Id });
        if (opers.Count == 0)
        {
            return new OperatorLoginResponse
            {
                ID = null,
                Message = "Invalid operator id or password."
            };
        }

        var oper = opers[0].parse();

        if (!oper.IsActive)
        {
            return new OperatorLoginResponse
            {
                ID = null,
                Message = "Operator is disabled."
            };
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, oper.OperatorPassword))
        {
            return new OperatorLoginResponse
            {
                ID = null,
                Message = "Invalid operator id or password."
            };
        }

        return new OperatorLoginResponse
        {
            ID = oper.Id,
            Message = string.Empty
        };
    }

    public async Task<List<OperatorGroup>> GetOperatorGroups()
    {
        List<OperatorGroup> operGroups = await _database.LoadData<OperatorGroup, dynamic>("SELECT * FROM `groups`;", new { });
        return operGroups;
    }
}
