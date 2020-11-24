using System;
using System.Configuration;

namespace CommLibrarys.SysConfig
{
    public class ConfigStr
    {
        public string ConnectionStr()
        {
            return ConfigurationManager.AppSettings["SqlStr"];
        }
        public string OtherStr(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}
