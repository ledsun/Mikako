using System.Collections.Generic;
using Com.Luxiar.Mikako.Db;
using NUnit.Framework;

namespace Com.Luxiar.DbCe
{
    public class DBHelperCe
    {
        public static void Execute(string sql)
        {
            using (DBBridgeForSqlServerCompact db = new DBBridgeForSqlServerCompact())
            {
                db.Execute(sql);
            }
        }

        public static TypeConvertableWrapper SelectOne(string sql)
        {
            using (DBBridgeForSqlServerCompact db = new DBBridgeForSqlServerCompact())
            {
                return db.SelectOne(sql);
            }
        }

        public static List<DataRowAccessor> Select(string sql)
        {
            using (DBBridgeForSqlServerCompact db = new DBBridgeForSqlServerCompact())
            {
                return db.Select(sql);
            }
        }

        public static void Insert(string table, List<DataRowAccessor> rows)
        {
            using (DBBridgeForSqlServerCompact db = new DBBridgeForSqlServerCompact())
            {
                db.FastInsert(table, rows);
            }
        }

        [TestFixture]
        public class Test
        {
            [Test]
            public void SelectOne()
            {
                Assert.That(DBHelperCe.SelectOne("SELECT 1").Int, Is.EqualTo(1));
            }
        }
    }
}
