using BT_COMMONS;
using BT_COMMONS.Database;
using BT_COMMONS.DataRepositories;
using BT_COMMONS.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    public Operator GetOperator(int id)
    {
        throw new NotImplementedException();
    }

    public OperatorLoginResponse OperatorLogin(OperatorLoginRequest request)
    {
        throw new NotImplementedException();
    }
}
