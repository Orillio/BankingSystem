using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem
{
    public interface IDataBase
    {
        SqlConnection Connection { get; set; }
        SqlDataAdapter Adapter { get; set; }
        DataTable Table { get; set; }

    }
}
