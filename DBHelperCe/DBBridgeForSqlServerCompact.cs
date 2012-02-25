using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using Ledsun.Alhambra.Db;

namespace Ledsun.DbCe
{
    public class DBBridgeForSqlServerCompact : AbstractDBBridge
    {
        private static bool NoUpdate = false;

        public DBBridgeForSqlServerCompact()
            : base(GetConnectionString(), 0)
        {
            Update();
        }

        private static void Update()
        {
            if (NoUpdate)
                return;
            SqlCeEngine engine = new SqlCeEngine(GetConnectionString());
            try
            {
                NoUpdate = true;
                engine.Upgrade();
            }
            catch (SqlCeException ceException)
            {
                System.Diagnostics.Debug.WriteLine(ceException.Message);
                System.Diagnostics.Debug.WriteLine("アップデートする必要はありませんでした。処理を続行します。");
            }
        }

        protected override IDbConnection CreateConnection()
        {
            Update();
            return new SqlCeConnection();
        }

        protected override IDbDataAdapter CreateAdapter(string sql, IDbConnection con)
        {
            return new SqlCeDataAdapter(sql, con as SqlCeConnection);
        }

        private static string GetConnectionString()
        {
            return Config.Value.DbCeConnectionString;
        }

        public void FastInsert(string table, List<DataRowAccessor> rows)
        {
            _cmd.CommandText = table;
            _cmd.CommandType = CommandType.TableDirect;

            using (SqlCeResultSet rs = (_cmd as SqlCeCommand).ExecuteResultSet(ResultSetOptions.Updatable))
            {
                SqlCeUpdatableRecord rec = rs.CreateRecord();
                foreach (DataRowAccessor row in rows)
                {
                    for (int i = 0; i < row.Columns.Count; i++)
                    {
                        DataColumn col = row.Columns[i];
                        SetData(rec, i, col.DataType, row[col.ColumnName]);
                    }
                    rs.Insert(rec);
                }
            }
        }

        private static void SetData(SqlCeUpdatableRecord rec, int index, Type type, TypeConvertableWrapper col)
        {
            switch (Type.GetTypeCode(type.GetType()))
            {
                case TypeCode.String:
                    rec.SetString(index, col.String);
                    break;
                case TypeCode.Int16:
                    rec.SetInt16(index, col.Int16);
                    break;
                case TypeCode.Int32:
                    rec.SetInt32(index, col.Int);
                    break;
                case TypeCode.Int64:
                    rec.SetInt64(index, col.Int);
                    break;
                case TypeCode.Byte:
                    rec.SetSqlByte(index, col.Byte);
                    break;
                case TypeCode.DateTime:
                    rec.SetSqlDateTime(index, col.DateTime);
                    break;
                case TypeCode.Decimal:
                    rec.SetDecimal(index, col.Decimal);
                    break;
                default:
                    throw new ArgumentException(Type.GetTypeCode(type.GetType()).ToString() + "型には対応していません。");
            }
        }
    }
}
