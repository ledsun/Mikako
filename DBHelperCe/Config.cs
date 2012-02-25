using System.Reflection;
using Ledsun.Alhambra.ConfigUtil;

namespace Ledsun.DbCe
{
    internal class Config
    {
        private static ConfigValue value = new ConfigValue();
        internal static ConfigValue Value
        {
            get { return value; }
        }

        internal class ConfigValue : ConfigValueBase
        {
            public string DbCeConnectionString
            {
                get { return GetValueString(MethodBase.GetCurrentMethod().Name.Substring(4)); }
            }

            public int SqlCeCommandTimeout
            {
                get { return GetValueInt(MethodBase.GetCurrentMethod().Name.Substring(4)); }
            }
        }
    }
}
