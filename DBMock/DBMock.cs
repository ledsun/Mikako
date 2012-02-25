using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Ledsun.Alhambra.Db;
using NUnit.Framework;

namespace Ledsun.Util
{
    //XMLからDataRowAccessorを読み込むクラスです。
    public class DBMock
    {
        private readonly string _schemaFileName;

        //スキーマファイルを指定してインスタンス化します。
        public DBMock(string schemaFileName)
        {
            if (String.IsNullOrEmpty(schemaFileName))
                throw new ArgumentException("スキーマファイルを指定して下さい。");

            if (File.Exists(schemaFileName))
                _schemaFileName = schemaFileName;
            else
                throw new ArgumentException("スキーマファイルがありません。");
        }

        //データファイルからデータを読み込みます。
        public List<DataRowAccessor> Select(string dataFileName)
        {
            if (String.IsNullOrEmpty(dataFileName))
                throw new ArgumentException("データファイルを指定して下さい。");

            if (!File.Exists(dataFileName))
                throw new ArgumentException("データファイルがありません。");

            DataSet ds = new DataSet();
            ds.ReadXmlSchema(_schemaFileName);
            ds.ReadXml(dataFileName);

            List<DataRowAccessor> l = new List<DataRowAccessor>();
            foreach (DataRow r in ds.Tables[0].Rows)
            {
                l.Add(new DataRowAccessor(r));
            }
            return l;
        }

        #region テスト
        [TestFixture]
        public class Test
        {
            [Test]
            public void スキーマとデータをファイルで指定してDataRowAccessorのリストを取得できます()
            {
                DBMock m = new DBMock("TEST_SCHEMA.xsd");
                List<DataRowAccessor> l = m.Select("TEST_DATA.xml");
                Assert.That(l[0]["ID"].Int, Is.EqualTo(81));
                Assert.That(l[0]["NAME"].String, Is.EqualTo("ABCDEFG"));
                Assert.That(l[0]["VALUE"].Int, Is.EqualTo(100));
                Assert.That(l[1]["ID"].Int, Is.EqualTo(82));
                Assert.That(l[1]["NAME"].String, Is.EqualTo(String.Empty));
                Assert.That(l[1]["VALUE"].Int, Is.EqualTo(0));
            }

            [Test]
            [ExpectedException(typeof(ArgumentException), ExpectedMessage = "スキーマファイルを指定して下さい。")]
            public void スキーマファイルは空ではダメです()
            {
                DBMock m = new DBMock("");
            }

            [Test]
            [ExpectedException(typeof(ArgumentException), ExpectedMessage = "スキーマファイルがありません。")]
            public void スキーマファイルがあること()
            {
                DBMock m = new DBMock("aaaaa");
            }

            [Test]
            [ExpectedException(typeof(ArgumentException), ExpectedMessage = "データファイルを指定して下さい。")]
            public void データファイルは空ではダメです()
            {
                DBMock m = new DBMock("TEST_SCHEMA.xsd");
                m.Select("");
            }

            [Test]
            [ExpectedException(typeof(ArgumentException), ExpectedMessage = "データファイルがありません。")]
            public void データファイルがあること()
            {
                DBMock m = new DBMock("TEST_SCHEMA.xsd");
                m.Select("aaaaa");
            }
        }
        #endregion
    }
}
