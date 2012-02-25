using System;
using System.Configuration;
using System.Reflection;

namespace Ledsun.Mikako.ConfigUtil
{
    //XXX.Config�̐ݒ�l��ǂݎ��N���X�ł��B
    //�e�v���W�F�N�g���Ƃ�Config�N���X���ۂ��ƃR�s�[��
    //�v���W�F�N�g�ŗL�̐ݒ��Config.ConfigValue�N���X���Ɏ������Ă��������B
    internal class Config
    {
        private static ConfigValue value = new ConfigValue();
        public static ConfigValue Value
        {
            get { return value; }
        }

        public class ConfigValue : ConfigValueBase
        {
            //�v���W�F�N�g�ŗL�̐ݒ�l�͂��̃N���X�̃v���p�e�B�Ƃ��Ď������܂��B
            //public string Sample
            //{
            //    get { return GetValueString(MethodBase.GetCurrentMethod().Name.Substring(4)); }
            //}
            public string DbConnectionString
            {
                get
                {
                    var css = ConfigurationManager.ConnectionStrings["DBHelper"];
                    if (css != null && !String.IsNullOrEmpty(css.ConnectionString))
                    {

                        return css.ConnectionString;
                    }
                    else
                    {
                        throw new ApplicationException("config�t�@�C����ConnectionString��DBHeler�̐ڑ���������w�肵�ĉ������B");
                    }
                }
            }

            public string DBPrefix
            {
                get { return GetValueString(MethodBase.GetCurrentMethod().Name.Substring(4)); }
            }

            public int SqlCommandTimeout
            {
                get
                {
                    try
                    {
                        return GetValueInt(MethodBase.GetCurrentMethod().Name.Substring(4));
                    }
                    catch
                    {
                        return 30;
                    }
                }
            }
        }
    }

    //���ʃ��C�u�����p�̐ݒ�l�͂����ɏ����܂��B
    public class ConfigValueBase
    {
        private static AppSettingsReader _reader = new AppSettingsReader();

        #region protected
        protected string GetValueString(string arg)
        {
            return (string)_reader.GetValue(arg, typeof(System.String));
        }

        protected int GetValueInt(string arg)
        {
            return (int)_reader.GetValue(arg, typeof(System.Int32));
        }

        protected bool GetValueBool(string arg)
        {
            return (bool)_reader.GetValue(arg, typeof(System.Boolean));
        }
        #endregion
    }
}