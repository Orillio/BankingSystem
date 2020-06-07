using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem
{
    public class ClientsDataBase : IDataBase
    { 
        public SqlConnection Connection { get; set; }
        public SqlDataAdapter Adapter { get; set; }
        public DataTable Table { get; set; }

        public ClientsDataBase(string select, string insert, string update, string delete)
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

            #region SELECT Command

            Adapter.SelectCommand = new SqlCommand(select, Connection);

            #endregion

            #region INSERT Command

            Adapter.InsertCommand = new SqlCommand(insert, Connection);

            Adapter.InsertCommand.Parameters.Add("@clientName", SqlDbType., 20, "clientName");
            Adapter.InsertCommand.Parameters.Add("@clientLastname", SqlDbType.NVarChar, 20, "clientLastname");
            Adapter.InsertCommand.Parameters.Add("@clientPatronymic", SqlDbType.NVarChar, 20, "clientPatronymic");
            Adapter.InsertCommand.Parameters.Add("@clientAge", SqlDbType.Int, 4, "clientAge");
            Adapter.InsertCommand.Parameters.Add("@cardNumber", SqlDbType.Int, 4, "cardNumber");
            Adapter.InsertCommand.Parameters.Add("@bankBalance", SqlDbType.Int, 4, "bankBalance");

            #endregion

            #region UPDATE Command

            Adapter.UpdateCommand = new SqlCommand(update, Connection);

            Adapter.UpdateCommand.Parameters.Add("@clientName", SqlDbType.NVarChar, 20, "clientName");
            Adapter.UpdateCommand.Parameters.Add("@clientLastname", SqlDbType.NVarChar, 20, "clientLastname");
            Adapter.UpdateCommand.Parameters.Add("@clientPatronymic", SqlDbType.NVarChar, 20, "clientPatronymic");
            Adapter.UpdateCommand.Parameters.Add("@clientAge", SqlDbType.Int, 4, "clientAge");
            Adapter.UpdateCommand.Parameters.Add("@cardNumber", SqlDbType.Int, 4, "cardNumber");
            Adapter.UpdateCommand.Parameters.Add("@bankBalance", SqlDbType.Int, 4, "bankBalance");
            #endregion

            #region DELETE Command

            Adapter.DeleteCommand = new SqlCommand(delete, Connection);
            Adapter.DeleteCommand.Parameters.Add("@id",SqlDbType.Int, 4, "id");

            #endregion

            Adapter.Fill(Table);
            
            #endregion
        }
    }
}
