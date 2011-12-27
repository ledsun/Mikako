using System;
using System.Configuration;
using System.Reflection;

namespace Com.Luxiar.Mikako.ConfigUtil
{
    //XXX.Configの設定値を読み取るクラスです。
    //各プロジェクトごとにConfigクラスを丸ごとコピーし
    //プロジェクト固有の設定をConfig.ConfigValueクラス内に実装してください。
    internal class Config
    {
        private static ConfigValue value = new ConfigValue();
        public static ConfigValue Value
        {
            get { return value; }
        }

        public class ConfigValue : ConfigValueBase
        {
            //プロジェクト固有の設定値はこのクラスのプロパティとして実装します。
            //public string Sample
            //{
            //    get { return GetValueString(MethodBase.GetCurrentMethod().Name.Substring(4)); }
            //}
        }
    }

    //共通ライブラリ用の設定値はここに書きます。
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
                    throw new ApplicationException("web.configにDbConnectionStringを指定して下さい。", e);
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