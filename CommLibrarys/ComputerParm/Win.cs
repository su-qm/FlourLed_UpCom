using System;
using System.Management;
using System.Net;

namespace CommLibrarys.ComputerParm
{
    public class Win
    {
        public static string[] GetMAC()
        {
            ManagementClass managementClass = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection instances = managementClass.GetInstances();
            string[] array = new string[instances.Count];
            int num = 0;
            using (ManagementObjectCollection.ManagementObjectEnumerator enumerator = instances.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    ManagementObject managementObject = (ManagementObject)enumerator.Current;
                    if ((bool)managementObject["IPEnabled"])
                    {
                        string text = managementObject["MacAddress"].ToString();
                        array[num] = text;
                        num++;
                    }
                    managementObject.Dispose();
                }
            }
            string[] array2 = new string[num];
            for (int i = 0; i < num; i++)
            {
                array2[i] = array[i];
            }
            return array2;
        }
        public static string[] GetCPU()
        {
            ManagementClass managementClass = new ManagementClass("Win32_Processor");
            ManagementObjectCollection instances = managementClass.GetInstances();
            string[] array = new string[instances.Count];
            int num = 0;
            using (ManagementObjectCollection.ManagementObjectEnumerator enumerator = instances.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    ManagementObject managementObject = (ManagementObject)enumerator.Current;
                    array[num] = managementObject.Properties["ProcessorId"].Value.ToString();
                    num++;
                }
            }
            return array;
        }
        public static string[] DiskID()
        {
            ManagementClass managementClass = new ManagementClass("Win32_DiskDrive");
            ManagementObjectCollection instances = managementClass.GetInstances();
            string[] array = new string[instances.Count];
            int num = 0;
            using (ManagementObjectCollection.ManagementObjectEnumerator enumerator = instances.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    ManagementObject managementObject = (ManagementObject)enumerator.Current;
                    array[num] = (string)managementObject.Properties["Model"].Value;
                }
            }
            return array;
        }
        public static string GetHostName()
        {
            return Dns.GetHostName();
        }
        public static string[] GetHostIP()
        {
            IPAddress[] hostAddresses = Dns.GetHostAddresses(Win.GetHostName());
            string[] array = new string[hostAddresses.Length];
            for (int i = 0; i < hostAddresses.Length; i++)
            {
                array[i] = hostAddresses[i].ToString();
            }
            return array;
        }
    }
}
