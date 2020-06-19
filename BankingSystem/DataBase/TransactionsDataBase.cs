using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem.DataBase
{
    public class TransactionsDataBase : IDataBase
    {
        public SqlConnection Connection { get; set; }
        public SqlDataAdapter Adapter { get; set; }
        public DataTable Table { get; set; }
        public TransactionsDataBase(string select, string insert, string update, string delete)
        {
            #region DB init

            #region Строка подключения

            SqlConnectionStringBuilder con = new SqlConnectionStringBuilder
            {
                DataSource = @"(localdb)\MSSQLLocalDB",
                InitialCatalog = "LocalDB"
            };
            Connection = new SqlConnection(con.ConnectionString);
            Connection.Open();
            #endregion

            Table = new DataTable();
            Adapter = new SqlDataAdapter();

            #region SELECT Command

            Adapter.SelectCommand = new SqlCommand(select, Connection);

            #endregion

            #region INSERT Command

            Adapter.InsertCommand = new SqlCommand(insert, Connection);

            Adapter.InsertCommand.Parameters.Add("@Id", SqlDbType.Int, 4, "id").Direction = ParameterDirection.Output;
            Adapter.InsertCommand.Parameters.Add("@ClientId", SqlDbType.Int, 4, "ClientId");
            Adapter.InsertCommand.Parameters.Add("@NameTarget", SqlDbType.NVarChar, 20, "NameTarget");
            Adapter.InsertCommand.Parameters.Add("@LastnameTarget", SqlDbType.NVarChar, 20, "LastnameTarget");
            Adapter.InsertCommand.Parameters.Add("@PatronymicTarget", SqlDbType.NVarChar, 20, "PatronymicTarget");
            Adapter.InsertCommand.Parameters.Add("@CardTarget", SqlDbType.BigInt, 20, "CardTarget");
            Adapter.InsertCommand.Parameters.Add("@CheckingAccount", SqlDbType.NVarChar, 20, "CheckingAccount");
            Adapter.InsertCommand.Parameters.Add("@ClientTypeTarget", SqlDbType.NVarChar, 20, "ClientTypeTarget");
            Adapter.InsertCommand.Parameters.Add("@TransactionSum", SqlDbType.BigInt, 20, "TransactionSum");
            Adapter.InsertCommand.Parameters.Add("@Type", SqlDbType.Int, 4, "Type");

            #endregion

            #region UPDATE Command

            Adapter.UpdateCommand = new SqlCommand(update, Connection);

            Adapter.UpdateCommand.Parameters.Add("@Id", SqlDbType.Int, 4, "id").SourceVersion = DataRowVersion.Original;
            Adapter.UpdateCommand.Parameters.Add("@ClientId", SqlDbType.Int, 4, "ClientId");
            Adapter.UpdateCommand.Parameters.Add("@NameTarger", SqlDbType.NVarChar, 20, "NameTarger");
            Adapter.UpdateCommand.Parameters.Add("@LastnameTarget", SqlDbType.NVarChar, 20, "LastnameTarget");
            Adapter.UpdateCommand.Parameters.Add("@PatronymicTarget", SqlDbType.NVarChar, 20, "PatronymicTarget");
            Adapter.UpdateCommand.Parameters.Add("@CardTarget", SqlDbType.BigInt, 20, "CardTarget");
            Adapter.UpdateCommand.Parameters.Add("@CheckingAccount", SqlDbType.NVarChar, 20, "CheckingAccount");
            Adapter.UpdateCommand.Parameters.Add("@ClientTypeTarget", SqlDbType.NVarChar, 20, "ClientTypeTarget");
            Adapter.UpdateCommand.Parameters.Add("@TransactionSum", SqlDbType.BigInt, 20, "TransactionSum");
            Adapter.UpdateCommand.Parameters.Add("@Type", SqlDbType.Int, 4, "Type");
            #endregion

            #region DELETE Command

            Adapter.DeleteCommand = new SqlCommand(delete, Connection);
            Adapter.DeleteCommand.Parameters.Add("@Id", SqlDbType.Int, 4, "Id");

            #endregion

            Adapter.Fill(Table);

            #endregion
        }
    }
}
