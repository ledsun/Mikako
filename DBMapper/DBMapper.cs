using System;
using System.Data;
using System.Reflection;
using System.Xml;
using Com.Luxiar.Mikako.Db;
using NUnit.Framework;
using Ledsun.Xml;

namespace Ledsun.Util
{
    //フィールドがPKであると示する属性です。
    [AttributeUsage(AttributeTargets.Field)]
    public class PrimaryKeyAttribute : Attribute { };

    //フィールドがSequenceであると示する属性です。
    [AttributeUsage(AttributeTargets.Field)]
    public class SequenceAttribute : PrimaryKeyAttribute { };

    //テーブルにUPDATE_DATEカラムを持ち、値がGETDATE()であることを示す属性です。
    [AttributeUsage(AttributeTargets.Class)]
    public class UpdateDateAttribute : Attribute { };

    public static class DBMapper
    {
        //SELECT結果をXMLで返します。ノード名は指定可能、属性名はカラム名固定です。
        public static XmlDocument SelectToXml(string nodeName, string sql)
        {
            XMLMaker maker = new XMLMaker(nodeName + "s");
            DBHelper.Select(sql).ForEach(delegate(DataRowAccessor row)
            {
                XMLMaker.XmlMaterial xm = new XMLMaker.XmlMaterial(nodeName);
                foreach (DataColumn col in row.Columns)
                {
                    xm.Attrs[col.ColumnName] = row[col.ColumnName];
                }
                maker.Add(xm);
            });
            return maker;
        }

        //データクラスをテーブルにマッピングしてDBを更新します。
        public static DataRowAccessor InsertOrUpdateByDataClass(Object obj)
        {
            ObjectSqlizer os = new ObjectSqlizer(obj);
            if (IsExist(os))
            {
                DBHelper.Execute(os.UpdateSql);
            }
            else
            {
                using (DBHelperWithTransaction db = new DBHelperWithTransaction())
                {
                    db.Execute(os.InsertSql);
                    if (os.Sequence == 0) //Sequence使ってて且つ新規の場合
                    {
                        os.Sequence = db.Select(
                                new DBSqlStatement("SELECT IDENT_CURRENT('@DB@@TABLE_NAME@') SEQUENCE")
                                .ReplaceStripString("TABLE_NAME", os.TableName)
                            )[0]["SEQUENCE"].Int;
                    }
                    db.Commit();
                }
            }
            //TODO 0件しか取れなかったらどうするか考える
            return DBHelper.Select(os.SelectSql)[0];
        }

        //複数のデータクラスを1トランザクションでDB更新します。
        public static XmlDocument InsertOrUpdateByDataClass(Object[] objs)
        {
            using (DBHelperWithTransaction db = new DBHelperWithTransaction())
            {
                foreach (Object obj in objs)
                {
                    ObjectSqlizer os = new ObjectSqlizer(obj);
                    if (IsExistWithTrans(os, db))
                    {
                        db.Execute(os.UpdateSql);
                    }
                    else
                    {
                        db.Execute(os.InsertSql);
                    }
                }
                db.Commit();
            }
            return new SuccessDocument();
        }

        public static XmlDocument DeleteByDataClass(object obj)
        {
            ObjectSqlizer os = new ObjectSqlizer(obj);
            DBHelper.Execute(os.DeleteSql);
            return new SuccessDocument();
        }

        /// <summary>
        /// PKを使って既存のレコードがあるかどうか判定します。
        /// Sequenceがあればレコードを一つに特定出来るので、PKで検索せずにTrueを返します。
        /// </summary>
        /// <param name="os"></param>
        /// <returns></returns>
        private static bool IsExist(ObjectSqlizer os)
        {
            return os.Sequence > 0 || DBHelper.Select(os.CountByPKSql)[0]["CT"].Int != 0;
        }

        private static bool IsExistWithTrans(ObjectSqlizer os, DBHelperWithTransaction db)
        {
            return os.Sequence > 0 || db.Select(os.CountByPKSql)[0]["CT"].Int != 0;
        }

        #region テスト
        [TestFixture]
        public class Test
        {
            [Test]
            public void 取得結果をXML化()
            {
                string sql = new DBSqlStatement(@"
                    SELECT
                        'ABC' abc
                        ,123 num
                    UNION
                    SELECT
                        'XYZ' abc
                        ,999 num
                    ");
                Assert.That(DBMapper.SelectToXml("syozoku", sql).InnerXml, Is.EqualTo("<syozokus><syozoku abc=\"ABC\" num=\"123\" /><syozoku abc=\"XYZ\" num=\"999\" /></syozokus>"));
            }
        }
        #endregion

        #region ObjectSqlizer
        /// <summary>
        /// データクラスをもとにDBを更新するSQLを生成するクラスです。
        /// クラス名をテーブル名、フィールド名をカラム名として扱います。
        /// PrimaryKeyAttributeがついたフィールドをPKとして扱います。
        /// </summary>
        private class ObjectSqlizer
        {
            private readonly object _dataObject;
            private readonly Type _type;
            private readonly FieldInfo[] _fields;

            public ObjectSqlizer(object dataObject)
            {
                _dataObject = dataObject;
                _type = _dataObject.GetType();
                _fields = _type.GetFields();

                if (_fields.Length == 0) throw new ApplicationException("最低1つのフィールドが必要です。");
            }

            public string TableName
            {
                get
                {
                    return _type.Name;
                }
            }

            public string CountByPKSql
            {
                get
                {
                    return ReplaceTable(new DBSqlStatement("SELECT COUNT(*) CT FROM @DB@ @TABLE@")) + PKCondition;
                }
            }

            public string DeleteSql
            {
                get
                {
                    return ReplaceTable(new DBSqlStatement("DELETE FROM @DB@ @TABLE@")) + PKCondition;
                }
            }

            //TODO まだリファクタリングの余地がある。
            private string PKCondition
            {
                get
                {
                    string condition = "";
                    foreach (FieldInfo f in _type.GetFields())
                    {
                        if (IsPK(f))
                            condition = ReplaceByType(new SqlStatement(condition + f.Name + " = @VALUE@ AND"), _dataObject, f);
                    }

                    if (condition == "") throw new ApplicationException("最低1つのPKが必要です。");
                    return " WHERE " + CutHip(condition, 3); ;
                }
            }

            public string InsertSql
            {
                get
                {
                    string columnNames = "";
                    string columnValues = "";
                    foreach (FieldInfo f in _type.GetFields())
                    {
                        if (!IsSequence(f))
                        {
                            columnNames += f.Name + ",";
                            columnValues = ReplaceByType(new SqlStatement(columnValues + "@VALUE@,"), _dataObject, f);
                        }
                    }

                    if (HasUpdateDate)
                    {
                        columnNames += "UPDATE_DATE,";
                        columnValues += "GETDATE(),";
                    }

                    return ReplaceTable(new DBSqlStatement("INSERT INTO @DB@ @TABLE@")) + "(" + CutHip(columnNames, 1) + ") VALUES(" + CutHip(columnValues, 1) + ")";
                }
            }

            public string UpdateSql
            {
                get
                {
                    string body = "";
                    string condition = "";
                    foreach (FieldInfo f in _type.GetFields())
                    {
                        if (IsPK(f))
                            condition = ReplaceByType(new SqlStatement(condition + f.Name + " = @VALUE@ AND"), _dataObject, f);
                        else
                            body = ReplaceByType(new SqlStatement(body + f.Name + " = @VALUE@,"), _dataObject, f);
                    }

                    if (HasUpdateDate)
                    {
                        body += "UPDATE_DATE = GETDATE(),";
                    }

                    if (condition == "") throw new ApplicationException("最低1つのPKが必要です。");
                    return ReplaceTable(new DBSqlStatement("UPDATE @DB@ @TABLE@ SET ")) + CutHip(body, 1) + " WHERE " + CutHip(condition, 3);
                }
            }

            public string SelectSql
            {
                get
                {
                    return ReplaceTable(new DBSqlStatement("SELECT * FROM @DB@ @TABLE@" + PKCondition));
                }
            }

            public int Sequence
            {
                get
                {
                    foreach (FieldInfo f in _type.GetFields())
                    {
                        if (IsSequence(f)) return (int)f.GetValue(_dataObject);
                    }
                    return -1;
                }
                set
                {
                    foreach (FieldInfo f in _type.GetFields())
                    {
                        if (IsSequence(f)) f.SetValue(_dataObject, value);
                    }
                }
            }

            private string ReplaceTable(DBSqlStatement statement)
            {
                return statement.ReplaceStripString("TABLE", _type.Name);
            }

            private bool IsPK(FieldInfo f)
            {
                return f.GetCustomAttributes(typeof(PrimaryKeyAttribute), false).Length > 0;
            }

            private bool IsSequence(FieldInfo f)
            {
                return f.GetCustomAttributes(typeof(SequenceAttribute), false).Length > 0;
            }

            private bool HasUpdateDate
            {
                get
                {
                    return _type.GetCustomAttributes(typeof(UpdateDateAttribute), false).Length > 0;
                }
            }

            private string ReplaceByType(SqlStatement statement, Object dataClass, FieldInfo f)
            {
                switch (f.FieldType.Name)
                {
                    case "String":
                        return statement.Replace("VALUE", (string)f.GetValue(dataClass));
                    case "Int32":
                        return statement.Replace("VALUE", (int)f.GetValue(dataClass));
                    case "Boolean":
                        return statement.Replace("VALUE", (bool)f.GetValue(dataClass));
                    case "DateTime":
                        return statement.Replace("VALUE", (DateTime)f.GetValue(dataClass));
                    case "Decimal":
                        return statement.Replace("VALUE", (decimal)f.GetValue(dataClass));
                    default:
                        throw new ArgumentException(f.FieldType.Name + "型には対応していません。");
                }
            }

            //指定した文字数お尻から削除します。
            private static string CutHip(string sql, int length)
            {
                return sql.Substring(0, sql.Length - length);
            }

            #region テスト
            [TestFixture]
            public class Test
            {
                [Test]
                public void 正常系()
                {
                    ObjectSqlizer zer = new ObjectSqlizer(new ABC());

                    Assert.That(zer.CountByPKSql, Is.EqualTo("SELECT COUNT(*) CT FROM ClassLibraryTest.dbo. ABC WHERE INT_COLUMN = 1234 "));
                    Assert.That(zer.UpdateSql, Is.EqualTo("UPDATE ClassLibraryTest.dbo. ABC SET TEXT_COLUMN = N'1234',TEST_COLUMN2 = N'ABCD',BOOL_COLUMN = 1,DATETIME_COLUMN = '2009/07/17 00:00:00' WHERE INT_COLUMN = 1234 "));
                    Assert.That(zer.InsertSql, Is.EqualTo("INSERT INTO ClassLibraryTest.dbo. ABC(INT_COLUMN,TEXT_COLUMN,TEST_COLUMN2,BOOL_COLUMN,DATETIME_COLUMN) VALUES(1234,N'1234',N'ABCD',1,'2009/07/17 00:00:00')"));
                }

                private class ABC
                {
                    [PrimaryKey]
                    public int INT_COLUMN = 1234;
                    public string TEXT_COLUMN = "1234";
                    public string TEST_COLUMN2 = "ABCD";
                    public bool BOOL_COLUMN = true;
                    public DateTime DATETIME_COLUMN = new DateTime(2009, 7, 17);
                }

                [Test]
                [ExpectedException(ExpectedMessage = "最低1つのフィールドが必要です。")]
                public void フィールドがなかったら例外()
                {
                    new ObjectSqlizer(new NoField());
                }

                private class NoField { }

                [Test]
                [ExpectedException(ExpectedMessage = "最低1つのPKが必要です。")]
                public void PKがないとSelect文を作れません()
                {
                    string sql = new ObjectSqlizer(new NoPK()).CountByPKSql;
                }

                [Test]
                [ExpectedException(ExpectedMessage = "最低1つのPKが必要です。")]
                public void PKがないとUpdate文を作れません()
                {
                    string sql = new ObjectSqlizer(new NoPK()).UpdateSql;
                }

                private class NoPK { public string TEXT_COLUMN = "1234"; }
                [Test]
                public void UPDATE_DATEの自動挿入()
                {
                    ObjectSqlizer zer = new ObjectSqlizer(new UPDATE());

                    Assert.That(zer.CountByPKSql, Is.EqualTo("SELECT COUNT(*) CT FROM ClassLibraryTest.dbo. UPDATE WHERE PK = 1234 "));
                    Assert.That(zer.UpdateSql, Is.EqualTo("UPDATE ClassLibraryTest.dbo. UPDATE SET UPDATE_DATE = GETDATE() WHERE PK = 1234 "));
                    Assert.That(zer.InsertSql, Is.EqualTo("INSERT INTO ClassLibraryTest.dbo. UPDATE(PK,UPDATE_DATE) VALUES(1234,GETDATE())"));
                }

                [UpdateDate]
                private class UPDATE
                {
                    [PrimaryKey]
                    public int PK = 1234;
                }

                [Test]
                public void SequenceはInsert分に含めない()
                {
                    ObjectSqlizer zer = new ObjectSqlizer(new Sequence());
                    Assert.That(zer.InsertSql, Is.EqualTo("INSERT INTO ClassLibraryTest.dbo. Sequence(VALUE) VALUES(N'ABCD')"));
                }

                private class Sequence
                {
                    [Sequence]
                    public int SEQ = 1234;
                    public string VALUE = "ABCD";
                }
            }
            #endregion
        }
        #endregion
    }
}