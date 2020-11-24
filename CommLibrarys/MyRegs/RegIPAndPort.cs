using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CommLibrarys
{
    public class RegIPAndPort
    {
        ///<summary>        

        /// 判断是否是Ip地址        

        /// </summary>        

        /// <param name="str1"></param>        

        /// <returns></returns>

        public static bool IsIPAddress(string ip)
        {
            if (string.IsNullOrEmpty(ip) || ip.Length < 7 || ip.Length > 15) return false;
            string regformat = @"^(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])$";
            Regex regex = new Regex(regformat, RegexOptions.IgnoreCase);

            return regex.IsMatch(ip);
        }



        public static bool IsIPPort(string port)
        {
            bool isPort = false;
            int portNum;
            isPort = Int32.TryParse(port, out portNum);
            if (isPort && portNum >= 0 && portNum <= 65535)
            {
                isPort = true;
            }
            else           
            {
                isPort = false;
            }

            return isPort;
        }

        public static bool IsIPPortFGSJ(string port)
        {
            bool isPort = false;
            int portNum;
            isPort = Int32.TryParse(port, out portNum);
            if (isPort && portNum >= 0 && portNum <= 1000)
            {
                isPort = true;
            }
            else
            {
                isPort = false;
            }

            return isPort;
        }
        public static bool IsIPPortFGYS(string port)
        {
            bool isPort = false;
            int portNum;
            isPort = Int32.TryParse(port, out portNum);
            if (isPort && portNum >= 0 && portNum <= 9000)
            {
                isPort = true;
            }
            else
            {
                isPort = false;
            }

            return isPort;
        }

        public static bool IsHefaValue(string tvalue)
        {
            bool isPort = false;
            int portNum;
            isPort = Int32.TryParse(tvalue, out portNum);
            if (isPort && portNum >= 0 && portNum <= 0xFFFFFF)
            {
                isPort = true;
            }
            else
            {
                isPort = false;
            }

            return isPort;
        }
    }
}
