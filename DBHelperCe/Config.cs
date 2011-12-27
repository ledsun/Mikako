using System.Reflection;
using Com.Luxiar.Mikako.ConfigUtil;

namespace Com.Luxiar.DbCe
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
        }
    }
}
