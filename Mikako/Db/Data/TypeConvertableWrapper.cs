using System;
using System.Data;
using System.Data.SqlTypes;
using System.Globalization;
using NUnit.Framework;

namespace Com.Luxiar.Mikako.Db
{
    /// <summary>
    /// �^�ϊ��@�\�����w���p�[�N���X
    /// DB����擾�����l�̌^�ϊ��Ɏg���܂��B
    /// </summary>
    /// <example>
    /// �^���̃v���p�e�B�œ��Y�^�Ŏ擾�ł��܂��B
    ///     new TypeConvertableWrapper("1").Int
    /// String�^�ւ͈Öٌ^�ϊ����\�ł��B
    ///     string hoge = new TypeConvertableWrapper("1");
    /// </example>
    public class TypeConvertableWrapper
    {
        private readonly Object _rawData;

        public TypeConvertableWrapper(object rawData)
        {
            _rawData = rawData;
        }

        //�����I�Ȍ^���w�肵���v���p�e�B
        public uint UInt { get { return To.UInt(_rawData); } }
        public int Int { get { return To.Int(_rawData); } }
        public Int16 Int16 { get { return To.Int16(_rawData); } }
        public Int64 Int64 { get { return To.Int64(_rawData); } }
        public Byte Byte { get { return To.Byte(_rawData); } }
        public string String { get { return To.String(_rawData); } }
        public decimal Decimal { get { return To.Decimal(_rawData); } }
        public DateTime DateTime { get { return To.DateTime(_rawData); } }
        public double Double { get { return To.Double(_rawData); } }

        /// <summary>
        /// 0��False����ȊO�̐�����True
        /// ������̏ꍇ��True�AFalse�͕ϊ��\�i�啶������������ʂ��Ȃ��j�B����ȊO�͗�O���o���B
        /// </summary>
        public bool Bool { get { return To.Bool(_rawData); } }

        //String�Ɋւ��Ă͈Öق̌^�ϊ����\
        public static implicit operator string(TypeConvertableWrapper value)
        {
            return value.String;
        }

        public override string ToString()
        {
            return String;
        }

        #region Test
        [TestFixture]
        public class Test
        {
            TypeConvertableWrapper _DateTimeData;
            TypeConvertableWrapper _DecimalData;
            TypeConvertableWrapper _DoubleData;
            TypeConvertableWrapper _IntData;
            TypeConvertableWrapper _StringData;
            TypeConvertableWrapper _UIntData;

            [SetUp]
            public void SetUp()
            {
                _DateTimeData = new TypeConvertableWrapper(new DateTime(2009, 4, 7));
                _DecimalData = new TypeConvertableWrapper(new Decimal(123.456));
                _DoubleData = new TypeConvertableWrapper(456.789d);
                _IntData = new TypeConvertableWrapper(-100);
                _StringData = new TypeConvertableWrapper("ABC");
                _UIntData = new TypeConvertableWrapper(100);
            }

            [Test]
            public void �v���p�e�B�ɂ��^�ϊ�()
            {
                Assert.That(_DateTimeData.DateTime, Is.EqualTo(new DateTime(2009, 4, 7)));
                Assert.That(_DecimalData.Decimal, Is.EqualTo(123.456));
                Assert.That(_DoubleData.Double, Is.EqualTo(456.789));
                Assert.That(_IntData.Int, Is.EqualTo(-100));
                Assert.That(_StringData.String, Is.EqualTo("ABC"));
                Assert.That(_UIntData.UInt, Is.EqualTo(100));
            }

            [Test]
            public void ToString���\�b�h���I�[�o�[���C�h���Ă��܂�()
            {
                Assert.That(_DateTimeData.ToString(), Is.EqualTo("2009/04/07 0:00:00"));
                Assert.That(_DecimalData.ToString(), Is.EqualTo("123.456"));
                Assert.That(_DoubleData.ToString(), Is.EqualTo("456.789"));
                Assert.That(_StringData.ToString(), Is.EqualTo("ABC"));
            }
        }
        #endregion

        #region �^�ϊ�����������N���X
        //staic�N���X�ł����e�X�g�����G�Ȃ��߁Aprivate���\�b�h�ɂ���private�N���X�Ƃ��Ă܂Ƃ߂ĕ������܂��B
        private static class To
        {
            /// <summary>
            /// �l����t�ɕϊ�����
            /// </summary>
            /// <param name="val">�ϊ����̒l</param>
            /// <returns>null�̏ꍇ��0�����̓��t�l�A����ȊO�͕ϊ����ꂽ�l</returns>
            internal static DateTime DateTime(object val)
            {
                return IsNull(val) ? System.DateTime.Parse("1/1/1753 12:00:00")//sqlserver compact �̐����i1/1/1753 12:00:00 AM ���� 12/31/9999 11:59:59 PM �܂ł̊ԂłȂ���΂Ȃ�܂���B�j
                    : val is SqlDateTime ? ((SqlDateTime)val).Value
                    : Convert.ToDateTime(val);
            }

            /// <summary>
            /// �l�𐔒l�ɕϊ�����
            /// </summary>
            /// <param name="val">�ϊ����̒l</param>
            /// <returns>null�̏ꍇ��0�A����ȊO�͕ϊ����ꂽ���l</returns>
            internal static Decimal Decimal(object val)
            {
                return IsNull(val) ? 0 : Convert.ToDecimal(val, NumberFormatInfo.CurrentInfo);
            }

            /// <summary>
            /// �l�𕂓������l�ɕϊ�����
            /// </summary>
            /// <param name="val">�ϊ����̒l</param>
            /// <returns>null�̏ꍇ��0�A����ȊO�͕ϊ����ꂽ�l</returns>
            internal static double Double(object val)
            {
                return IsNull(val) ? 0 : Convert.ToDouble(val);
            }

            /// <summary>
            /// �l�𐳐��ɕϊ�����
            /// </summary>
            /// <param name="val">�ϊ����̒l</param>
            /// <returns>null�̏ꍇ��0�A����ȊO�͕ϊ����ꂽ���l</returns>
            internal static uint UInt(object val)
            {
                return IsNull(val) ? 0 : Convert.ToUInt32(val, NumberFormatInfo.CurrentInfo);
            }

            /// <summary>
            /// �l�𐔒l�ɕϊ�����
            /// </summary>
            /// <param name="val">�ϊ����̒l</param>
            /// <returns>null�̏ꍇ��0�A����ȊO�͕ϊ����ꂽ���l</returns>
            internal static int Int(object val)
            {
                return IsNull(val) ? 0 : Convert.ToInt32(val, NumberFormatInfo.CurrentInfo);
            }

            /// <summary>
            /// �l�𐔒l�ɕϊ�����
            /// </summary>
            /// <param name="val">�ϊ����̒l</param>
            /// <returns>null�̏ꍇ��0�A����ȊO�͕ϊ����ꂽ���l</returns>
            internal static System.Int16 Int16(object val)
            {
                return IsNull(val) ? (Int16)0 : Convert.ToInt16(val, NumberFormatInfo.CurrentInfo);
            }

            /// <summary>
            /// �l�𐔒l�ɕϊ�����
            /// </summary>
            /// <param name="val">�ϊ����̒l</param>
            /// <returns>null�̏ꍇ��0�A����ȊO�͕ϊ����ꂽ���l</returns>
            internal static System.Int64 Int64(object val)
            {
                return IsNull(val) ? (Int64)0 : Convert.ToInt64(val, NumberFormatInfo.CurrentInfo);
            }

            /// <summary>
            /// �l�𐔒l�ɕϊ�����
            /// </summary>
            /// <param name="val">�ϊ����̒l</param>
            /// <returns>null�̏ꍇ��0�A����ȊO�͕ϊ����ꂽ���l</returns>
            internal static System.Byte Byte(object val)
            {
                return IsNull(val) ? (Byte)0 : Convert.ToByte(val, NumberFormatInfo.CurrentInfo);
            }

            /// <summary>
            /// �l�𕶎���ɕϊ�����
            /// </summary>
            /// <param name="val">�ϊ����̒l</param>
            /// <returns>null�̏ꍇ�͋󕶎��A����ȊO�͕ϊ����ꂽ����</returns>
            internal static string String(object val)
            {
                return IsNull(val) ? "" : val.ToString();
            }

            internal static bool Bool(object val)
            {
                return IsNull(val) ? false : Convert.ToBoolean(val);
            }

            /// <summary>
            /// �I�u�W�F�N�g��NULL��\�����̂��ǂ�����Ԃ�
            /// </summary>
            /// <param name="val">���肷��l</param>
            /// <returns>true:null false:null�ȊO</returns>
            private static bool IsNull(object val)
            {
                return (null == val || val is DBNull);
            }

            #region test
            [TestFixture]
            public class Test
            {
                #region DateTime
                [Test]
                public void DateTime()
                {
                    Assert.That(To.DateTime(null), Is.EqualTo(System.DateTime.Parse("1/1/1753 12:00:00")));
                    Assert.That(To.DateTime("100.1"), Is.EqualTo(new DateTime(100, 1, 1)));
                    Assert.That(To.DateTime(new DateTime(2009, 4, 7)), Is.EqualTo(new DateTime(2009, 4, 7)));

                    Assert.That(To.DateTime("2009/04/07 0:00:00"), Is.EqualTo(new DateTime(2009, 4, 7)));
                    Assert.That(To.DateTime(new SqlDateTime(2009, 4, 7)), Is.EqualTo(new DateTime(2009, 4, 7)));
                }

                [Test]
                [ExpectedException(typeof(InvalidCastException))]
                public void ������DateTime�ϊ����Ȃ�()
                {
                    To.DateTime(100);
                }

                [Test]
                [ExpectedException(typeof(InvalidCastException))]
                public void ������DateTime�ϊ����Ȃ�()
                {
                    To.DateTime(100.1);
                }

                [Test]
                [ExpectedException(typeof(FormatException))]
                public void �����������DateTime�ϊ����Ȃ�()
                {
                    To.DateTime("100");
                }

                [Test]
                [ExpectedException(typeof(FormatException))]
                public void �󕶎����DateTime�ϊ����Ȃ�()
                {
                    To.DateTime("");
                }

                [Test]
                [ExpectedException(typeof(FormatException))]
                public void �A���t�@�x�b�g�������DateTime�ϊ����Ȃ�()
                {
                    To.DateTime("ABC");
                }
                #endregion

                #region Decimal
                [Test]
                public void Decimal()
                {
                    Assert.That(To.Decimal(null), Is.EqualTo(0));
                    Assert.That(To.Decimal(100), Is.EqualTo(100));
                    Assert.That(To.Decimal(100.1), Is.EqualTo(100.1));
                    Assert.That(To.Decimal("100"), Is.EqualTo(100));
                    Assert.That(To.Decimal("100.1"), Is.EqualTo(100.1));
                }

                [Test]
                [ExpectedException(typeof(FormatException))]
                public void �󕶎���Decimal�ϊ����Ȃ�()
                {
                    To.Decimal("");
                }

                [Test]
                [ExpectedException(typeof(FormatException))]
                public void �A���t�@�x�b�g��Decimal�ɕϊ����Ȃ�()
                {
                    To.Decimal("ABC");
                }

                [Test]
                [ExpectedException(typeof(InvalidCastException))]
                public void DateTime��Decimal�ɕϊ����Ȃ�()
                {
                    To.Decimal(new DateTime());
                }
                #endregion Decimal

                #region Double
                [Test]
                public void Double()
                {
                    Assert.That(To.Double(null), Is.EqualTo(0));
                    Assert.That(To.Double(100), Is.EqualTo(100));
                    Assert.That(To.Double(100.1), Is.EqualTo(100.1));
                    Assert.That(To.Double("100"), Is.EqualTo(100));
                    Assert.That(To.Double("100.1"), Is.EqualTo(100.1));
                }

                [Test]
                [ExpectedException(typeof(FormatException))]
                public void �󕶎���Double�ɕϊ����Ȃ�()
                {
                    To.Double("");
                }

                [Test]
                [ExpectedException(typeof(FormatException))]
                public void �A���t�@�x�b�g��Double�ɕϊ����Ȃ�()
                {
                    To.Double("ABC");
                }

                [Test]
                [ExpectedException(typeof(InvalidCastException))]
                public void DateTime��Double�ɕϊ����Ȃ�()
                {
                    To.Double(new DateTime());
                }
                #endregion

                #region UInt
                [Test]
                public void UInt()
                {
                    Assert.That(To.UInt(null), Is.EqualTo(0));
                    Assert.That(To.UInt(100), Is.EqualTo(100));
                    Assert.That(To.UInt(100.1), Is.EqualTo(100));
                    Assert.That(To.UInt("100"), Is.EqualTo(100));
                }

                [Test]
                [ExpectedException(typeof(OverflowException))]
                public void ������UInt�ɕϊ����Ȃ�()
                {
                    To.UInt(-100);
                }

                [Test]
                [ExpectedException(typeof(FormatException))]
                public void �A���t�@�x�b�g��UInt�ɕϊ����Ȃ�()
                {
                    To.UInt("ABC");
                }

                [Test]
                [ExpectedException(typeof(InvalidCastException))]
                public void DateTime��UInt�ɕϊ����Ȃ�()
                {
                    To.UInt(new DateTime());
                }

                [Test]
                [ExpectedException(typeof(FormatException))]
                public void �󕶎���UInt�ɕϊ����Ȃ�()
                {
                    To.UInt("");
                }

                [Test]
                [ExpectedException(typeof(FormatException))]
                public void �����_�t���������UInt�ɕϊ����Ȃ�()
                {
                    To.UInt("100.1");
                }
                #endregion

                #region Int
                [Test]
                public void Int()
                {
                    Assert.That(To.Int(null), Is.EqualTo(0));
                    Assert.That(To.Int(100), Is.EqualTo(100));
                    Assert.That(To.Int(100.1), Is.EqualTo(100));
                    Assert.That(To.Int("100"), Is.EqualTo(100));
                }

                [Test]
                [ExpectedException(typeof(FormatException))]
                public void �A���t�@�x�b�g��Int�ɕϊ����Ȃ�()
                {
                    To.Int("ABC");
                }

                [Test]
                [ExpectedException(typeof(InvalidCastException))]
                public void DateTime��Int�ɕϊ����Ȃ�()
                {
                    To.Int(new DateTime());
                }

                [Test]
                [ExpectedException(typeof(FormatException))]
                public void �󕶎���Int�ɕϊ����Ȃ�()
                {
                    To.Int("");
                }

                [Test]
                [ExpectedException(typeof(FormatException))]
                public void �����_�t���������Int�ɕϊ����Ȃ�()
                {
                    To.Int("100.1");
                }
                #endregion

                [Test]
                public void String()
                {
                    Assert.That(To.String(null), Is.EqualTo(""));
                    Assert.That(To.String(100), Is.EqualTo("100"));
                    Assert.That(To.String(100.1), Is.EqualTo("100.1"));
                    Assert.That(To.String("100"), Is.EqualTo("100"));
                    Assert.That(To.String("100.1"), Is.EqualTo("100.1"));
                    Assert.That(To.String(""), Is.EqualTo(""));
                    Assert.That(To.String("ABC"), Is.EqualTo("ABC"));
                    Assert.That(To.String(new DateTime(2009, 4, 7)), Is.EqualTo("2009/04/07 0:00:00"));
                }

                [Test]
                public void Bool()
                {
                    Assert.That(To.Bool(null), Is.False);
                    Assert.That(To.Bool(-1), Is.True);
                    Assert.That(To.Bool(0), Is.False);
                    Assert.That(To.Bool(1), Is.True);
                    Assert.That(To.Bool(100), Is.True);
                    Assert.That(To.Bool("TRue"), Is.True);
                    Assert.That(To.Bool("falSe"), Is.False);
                    Assert.That(To.Bool(true), Is.True);
                    Assert.That(To.Bool(false), Is.False);
                }

                [Test]
                [ExpectedException(typeof(FormatException))]
                public void �^���l�ϊ��ł��Ȃ�������()
                {
                    To.Bool("x");
                }

                [Test]
                public void IsNull()
                {
                    Assert.That(To.IsNull(null), Is.True);
                    Assert.That(To.IsNull(DBNull.Value), Is.True);
                }
            }
            #endregion
        }
        #endregion
    }
}
