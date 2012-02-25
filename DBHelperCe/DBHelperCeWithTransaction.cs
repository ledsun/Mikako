using System;
using System.Collections.Generic;
using Ledsun.Alhambra.Db;

namespace Ledsun.DbCe
{
    public class DBHelperCeWithTransaction : IDisposable
    {
        private DBBridgeForSqlServerCompact _bridg;

        public DBHelperCeWithTransaction()
        {
            _bridg = new DBBridgeForSqlServerCompact();
            _bridg.BeginTransaction();
        }

        public void Commit()
        {
            _bridg.Commit();
        }

        public void Rollback()
        {
            _bridg.Rollback();
        }

        public void Execute(string sql)
        {
            _bridg.Execute(sql);
        }

        public List<DataRowAccessor> Select(string sql)
        {
            return _bridg.Select(sql);
        }

        public TypeConvertableWrapper SelectOne(string sql)
        {
            return _bridg.SelectOne(sql);
        }

        public void Dispose()
        {
            _bridg.Dispose();
        }
    }
}
