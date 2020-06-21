using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem.DataBase
{
    public class InvestmentsDataBase : IDataBase
    {
        public SqlConnection Connection { get; set; } 
        public SqlDataAdapter Adapter { get; set; }
        public DataTable Table { get; set; }
        public InvestmentsDataBase(string select, string insert)
        {
            #region DB init

            #region Строка подключения

            SqlConnectionStringBuilder con = new SqlConnectionStringBuilder
            {
                DataSource = @"(localdb)\MSSQLLocalDB",
                InitialCatalog = "LocalDB"
            };
            Connection = new SqlConnection(con.ConnectionString);

            #endregion

            Table = new DataTable();
            Adapter = new SqlDataAdapter();
            Connection.Open();

            #region SELECT Command

            Adapter.SelectCommand = new SqlCommand(select, Connection);

            #endregion

            #region INSERT Command

            Adapter.InsertCommand = new SqlCommand(insert, Connection);

            Adapter.InsertCommand.Parameters.Add("@Id", SqlDbType.Int, 10,"Id" ).Direction = ParameterDirection.Output;
            Adapter.InsertCommand.Parameters.Add("@clientId", SqlDbType.Int, 10, "clientId");
            Adapter.InsertCommand.Parameters.Add("@investmentType", SqlDbType.NVarChar, 20, "investmentType");
            Adapter.InsertCommand.Parameters.Add("@investmentSum", SqlDbType.Int, 10, "investmentSum");
            Adapter.InsertCommand.Parameters.Add("@investmentDate", SqlDbType.NVarChar, 20, "investmentDate");
            Adapter.InsertCommand.Parameters.Add("@percentage", SqlDbType.Int, 20, "percentage");

            #endregion


            Adapter.Fill(Table);
            #endregion
        }
    }
}
