using System;
using System.Collections.Generic;
using System.Text;
using Com.Luxiar.Mikako.ConfigUtil;
using NUnit.Framework;

namespace Com.Luxiar.Mikako.Db
{
    //������DB���g�����������Ȃ����ꍇ�ɁA�V�����N���X������Ēu�������������ύX���Ă��������B
    public class DBSqlStatement : SqlStatement
    {
        public DBSqlStatement(string baseSql) : base(baseSql, Config.Value.DBPrefix) { }
    }

    //SQL�X�e�[�g�����g���쐬���邽�߂̃N���X�ł��B
    //@�ň͂񂾕�������A�w��̒l�ɒu�������Ă����Replace���\�b�h��񋟂��܂��B
    //�ȉ��̂悤�ɂ���SQL��������쐬���邱�Ƃ��ł��܂��B
    // new SqlStatement("SELECT * FROM TABLE WHERE ID = @ID@").Replace("ID", 100).ToString();
    public class SqlStatement
    {
        const string DATABASE = "@DB@";
        private readonly string _baseSql;

        public SqlStatement(string baseSql)
        {
            _baseSql = (string)baseSql.Clone();
        }

        public SqlStatement(string baseSql, string dbName)
            : this(baseSql)
        {
            _baseSql = _baseSql.Replace(DATABASE, dbName);
        }

        public SqlStatement Replace(string oldString, bool newValue)
        {
            return ReplaceByAtmark(oldString, newValue ? "1" : "0");
        }

        public SqlStatement Replace(string oldString, int newValue)
        {
            return ReplaceByAtmark(oldString, newValue.ToString());
        }

        public SqlStatement Replace(string oldString, Decimal newValue)
        {
            return ReplaceByAtmark(oldString, newValue.ToString());
        }

        public SqlStatement Replace(string oldString, DateTime newDate)
        {
            //�������������ꂽ�l���w�肳�ꂽ�ꍇ��NULL�ɂ��܂��B
            //DB��datetime�^�͎w�薳���������l�����Ȃ�����NULL�������܂��B
            string newString = newDate == new DateTime(0) ? "NULL" : newDate.ToString("\\'yyyy/MM/dd HH:mm:ss\\'");
            return ReplaceByAtmark(oldString, newString);
        }

        public SqlStatement Replace(string oldString, IEnumerable<string> newStrings)
        {
            var newString = "";
            foreach (string str in newStrings)
            {
                newString += "'" + Sanitize(str) + "',";
            }

            return ReplaceByAtmark(oldString, CutLastChar(newString));
        }

        public SqlStatement Replace(string oldString, IEnumerable<long> newStrings)
        {
            var newString = "";
            foreach (long l in newStrings)
            {
                newString += l.ToString() + ",";
            }

            return ReplaceByAtmark(oldString, CutLastChar(newString));
        }

        private static string CutLastChar(string val)
        {
            return val.Length > 0 ? val.Remove(val.Length - 1) : val;
        }

        //�V���O���N�H�[�g�ň͂ޔ�
        public SqlStatement Replace(string oldString, string newString)
        {
            return ReplaceByAtmark(oldString, "N'" + Sanitize(newString) + "'");
        }

        public SqlStatement ReplaceForPartialMatchRetrieval(string oldString, string newString)
        {
            return ReplaceByAtmark(oldString, "'%" + Sanitize(newString) + "%'");
        }

        public SqlStatement ReplaceStripString(string oldString, string newString)
        {
            return ReplaceByAtmark(oldString, Sanitize(newString));
        }

        #region ������ϊ�
        /// <summary>
        /// String�^�ւ̈Öٌ^�ϊ����Z�q
        /// ���̃��\�b�h���������邱�ƂŁAString�^�ւ̃L���X�g��SQL��������擾�ł��܂��B
        /// </summary>
        /// <param name="value"></param>
        /// <returns>�쐬����SQL������</returns>
        public static implicit operator string(SqlStatement value)
        {
            return value._baseSql;
        }

        /// <summary>
        /// �ÖٓI�Ȍ^�ϊ����T�|�[�g���Ă���̂ŃL���X�g�����SQL�����񂪎擾�ł��邽�߁A�����ɂ͖{���\�b�h�͕K�v����܂���B
        /// �������AToString���\�b�h��String�^�ւ̌^�ϊ��̌��ʂ��قȂ�ꍇ�A�������������ߓ������ʂ�Ԃ��܂��B
        /// </summary>
        /// <returns>�쐬����SQL������</returns>
        public override string ToString()
        {
            return this;
        }
        #endregion

        #region private_method
        private SqlStatement ReplaceByAtmark(string oldString, string newString)
        {
            return new SqlStatement(_baseSql.Replace("@" + oldString + "@", newString));
        }

        /// <summary>
        /// ������u������SQL�C���W�F�N�V�����΍�Ɋ댯�ȕ�����i�V���O���N�H�[�g�ƃp�[�Z���g�j���G�X�P�[�v���܂��B
        /// varchar�^����Null���w�肵�����ꍇ�́Anull�ł͂Ȃ�������"NULL"���w�肵�Ă��������B
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string Sanitize(string value)
        {
            //������null���󕶎��ɒu��������̂͂��������ł��B
            //�Ăяo������public���\�b�h�ň�����null���`�F�b�N��ArgumentNullException��Ԃ��ׂ��ł��B
            //�������A�ǂ̃v���W�F�N�g�Ŏg���Ă��邩�킩��Ȃ��̂ł����C�����܂���B
            if (value == null) return "";
            StringBuilder builder = new StringBuilder(value.Length);
            foreach (char c in value)
            {
                if (c == '\'')
                {
                    builder.Append('\'');
                }
                if (c == '%')
                {
                    builder.Append('\\');
                }
                builder.Append(c);
            }
            return builder.ToString();
        }
        #endregion

        #region test
        [TestFixture]
        public class Test
        {
            [Test]
            public void �z��̒u��()
            {
                string[] moto = { "aaa", "bbb", "ccc" };
                Assert.That((String)new SqlStatement("WHERE HAGE IN (@HAGES@)").Replace("HAGES", moto), Is.EqualTo("WHERE HAGE IN ('aaa','bbb','ccc')"));

                long[] moto1 = { 1, 2, 3 };
                Assert.That((String)new SqlStatement("WHERE HAGE IN (@HAGES@)").Replace("HAGES", moto1), Is.EqualTo("WHERE HAGE IN (1,2,3)"));
            }

            [Test]
            public void ToString�̃I�[�o�[���C�h()
            {
                Assert.That(new SqlStatement("SELECT 1").ToString(), Is.EqualTo("SELECT 1"));
                //�ÖٓI�ȕ�����ϊ��Ɠ������ʂ�Ԃ��܂��B
                Assert.That(new SqlStatement("SELECT 1").ToString(), Is.EqualTo((String)new SqlStatement("SELECT 1")));
            }
        }
        #endregion
    }
}