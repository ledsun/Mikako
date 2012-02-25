using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Ledsun.Alhambra.Db;
using NUnit.Framework;

namespace Ledsun.Util
{
    //XML����DataRowAccessor��ǂݍ��ރN���X�ł��B
    public class DBMock
    {
        private readonly string _schemaFileName;

        //�X�L�[�}�t�@�C�����w�肵�ăC���X�^���X�����܂��B
        public DBMock(string schemaFileName)
        {
            if (String.IsNullOrEmpty(schemaFileName))
                throw new ArgumentException("�X�L�[�}�t�@�C�����w�肵�ĉ������B");

            if (File.Exists(schemaFileName))
                _schemaFileName = schemaFileName;
            else
                throw new ArgumentException("�X�L�[�}�t�@�C��������܂���B");
        }

        //�f�[�^�t�@�C������f�[�^��ǂݍ��݂܂��B
        public List<DataRowAccessor> Select(string dataFileName)
        {
            if (String.IsNullOrEmpty(dataFileName))
                throw new ArgumentException("�f�[�^�t�@�C�����w�肵�ĉ������B");

            if (!File.Exists(dataFileName))
                throw new ArgumentException("�f�[�^�t�@�C��������܂���B");

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

        #region �e�X�g
        [TestFixture]
        public class Test
        {
            [Test]
            public void �X�L�[�}�ƃf�[�^���t�@�C���Ŏw�肵��DataRowAccessor�̃��X�g���擾�ł��܂�()
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
            [ExpectedException(typeof(ArgumentException), ExpectedMessage = "�X�L�[�}�t�@�C�����w�肵�ĉ������B")]
            public void �X�L�[�}�t�@�C���͋�ł̓_���ł�()
            {
                DBMock m = new DBMock("");
            }

            [Test]
            [ExpectedException(typeof(ArgumentException), ExpectedMessage = "�X�L�[�}�t�@�C��������܂���B")]
            public void �X�L�[�}�t�@�C�������邱��()
            {
                DBMock m = new DBMock("aaaaa");
            }

            [Test]
            [ExpectedException(typeof(ArgumentException), ExpectedMessage = "�f�[�^�t�@�C�����w�肵�ĉ������B")]
            public void �f�[�^�t�@�C���͋�ł̓_���ł�()
            {
                DBMock m = new DBMock("TEST_SCHEMA.xsd");
                m.Select("");
            }

            [Test]
            [ExpectedException(typeof(ArgumentException), ExpectedMessage = "�f�[�^�t�@�C��������܂���B")]
            public void �f�[�^�t�@�C�������邱��()
            {
                DBMock m = new DBMock("TEST_SCHEMA.xsd");
                m.Select("aaaaa");
            }
        }
        #endregion
    }
}
