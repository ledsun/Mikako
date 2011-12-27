using System.Data;
using System.Data.SqlClient;
using Com.Luxiar.Mikako.ConfigUtil;

namespace Com.Luxiar.Mikako.Db
{
    public class DBBridgeForSqlServer : AbstractDBBridge
    {
        public DBBridgeForSqlServer() : base(Config.Value.DbConnectionString, Config.Value.SqlCommandTimeout) { }

        protected override IDbConnection CreateConnection()
        {
            return new SqlConnection();
        }

        protected override IDbDataAdapter CreateAdapter(string sql, IDbConnection con)
        {
            return new SqlDataAdapter(sql, con as SqlConnection);
        }
    }
}
