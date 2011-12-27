using System;
using System.Configuration;
using System.Reflection;

namespace Com.Luxiar.Mikako.ConfigUtil
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
        }
    }

    //���ʃ��C�u�����p�̐ݒ�l�͂����ɏ����܂��B
    public class ConfigValueBase
    {
        private static AppSettingsReader _reader = new AppSettingsReader();

        public string DbConnectionString
        {
            get
            {
                try
                {
                    return GetValueString(MethodBase.GetCurrentMethod().Name.Substring(4));
                }
                catch (InvalidOperationException e)
                {
                    throw new ApplicationException("web.config��DbConnectionString���w�肵�ĉ������B", e);
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

        public int SqlCeCommandTimeout
        {
            get { return GetValueInt(MethodBase.GetCurrentMethod().Name.Substring(4)); }
        }

        public int PerfomanceTimerThreshold
        {
            get { return GetValueInt(MethodBase.GetCurrentMethod().Name.Substring(4)); }
        }

        public bool LogOutput
        {
            get { return GetValueBool(MethodBase.GetCurrentMethod().Name.Substring(4)); }
        }

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