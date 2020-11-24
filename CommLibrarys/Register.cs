using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using CommLibrarys.Encrypt;

namespace CommLibrarys
{
    public class Register
    {
        public static string ReadRegCode()
        {
            string result;
            try
            {
                ClassEncrypt classEncrypt = new ClassEncrypt();
                RegistryKey currentUser = Registry.CurrentUser;
                RegistryKey registryKey = currentUser.OpenSubKey("system");
                RegistryKey registryKey2 = registryKey.OpenSubKey("config");
                string text = classEncrypt.Decrypt(registryKey2.GetValue("regcode").ToString());
                result = text;
            }
            catch
            {
                result = "未注册";
            }
            return result;
        }
        public static string ReadRegValue(string key, bool flag)
        {
            string result;
            try
            {
                ClassEncrypt classEncrypt = new ClassEncrypt();
                RegistryKey currentUser = Registry.CurrentUser;
                RegistryKey registryKey = currentUser.CreateSubKey("system");
                RegistryKey registryKey2 = registryKey.CreateSubKey("config");
                string text = "";
                if (flag)
                {
                    text = classEncrypt.Decrypt(registryKey2.GetValue(key).ToString());
                }
                else
                {
                    text = registryKey2.GetValue(key).ToString();
                }
                result = text;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }
        public static bool WriteRegValue(string _key, string _value)
        {
            bool result;
            try
            {
                RegistryKey currentUser = Registry.CurrentUser;
                RegistryKey registryKey = currentUser.CreateSubKey("system");
                RegistryKey registryKey2 = registryKey.CreateSubKey("config");
                registryKey2.SetValue(_key, _value);
                result = true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }
        public static bool WriteRegValue(string key1, string key2, string _key, string _value)
        {
            bool result;
            try
            {
                RegistryKey currentUser = Registry.CurrentUser;
                RegistryKey registryKey = currentUser.CreateSubKey(key1);
                RegistryKey registryKey2 = registryKey.CreateSubKey(key2);
                registryKey2.SetValue(_key, _value);
                result = true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }
    }
}
