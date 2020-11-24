using System;
using System.Collections;
using System.Data;
using System.Data.OracleClient;

namespace CommLibrarys.OracleHelper
{
    public sealed class OracleHelperParameterCache
    {
        private static Hashtable paramCache = Hashtable.Synchronized(new Hashtable());
        private OracleHelperParameterCache()
        {
        }
        private static OracleParameter[] DiscoverSpParameterSet(OracleConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            OracleCommand oracleCommand = new OracleCommand(spName, connection);
            oracleCommand.CommandType = CommandType.StoredProcedure;
            connection.Open();
            OracleCommandBuilder.DeriveParameters(oracleCommand);
            connection.Close();
            if (!includeReturnValueParameter)
            {
                oracleCommand.Parameters.RemoveAt(0);
            }
            OracleParameter[] array = new OracleParameter[oracleCommand.Parameters.Count];
            oracleCommand.Parameters.CopyTo(array, 0);
            OracleParameter[] array2 = array;
            for (int i = 0; i < array2.Length; i++)
            {
                OracleParameter oracleParameter = array2[i];
                oracleParameter.Value = DBNull.Value;
            }
            return array;
        }
        private static OracleParameter[] CloneParameters(OracleParameter[] originalParameters)
        {
            OracleParameter[] array = new OracleParameter[originalParameters.Length];
            int i = 0;
            int num = originalParameters.Length;
            while (i < num)
            {
                array[i] = (OracleParameter)((ICloneable)originalParameters[i]).Clone();
                i++;
            }
            return array;
        }
        public static void CacheParameterSet(string connectionString, string commandText, params OracleParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (commandText == null || commandText.Length == 0)
            {
                throw new ArgumentNullException("commandText");
            }
            string key = connectionString + ":" + commandText;
            OracleHelperParameterCache.paramCache[key] = commandParameters;
        }
        public static OracleParameter[] GetCachedParameterSet(string connectionString, string commandText)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (commandText == null || commandText.Length == 0)
            {
                throw new ArgumentNullException("commandText");
            }
            string key = connectionString + ":" + commandText;
            OracleParameter[] array = OracleHelperParameterCache.paramCache[key] as OracleParameter[];
            OracleParameter[] result;
            if (array == null)
            {
                result = null;
            }
            else
            {
                result = OracleHelperParameterCache.CloneParameters(array);
            }
            return result;
        }
        public static OracleParameter[] GetSpParameterSet(string connectionString, string spName)
        {
            return OracleHelperParameterCache.GetSpParameterSet(connectionString, spName, false);
        }
        public static OracleParameter[] GetSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            OracleParameter[] spParameterSetInternal;
            using (OracleConnection oracleConnection = new OracleConnection(connectionString))
            {
                spParameterSetInternal = OracleHelperParameterCache.GetSpParameterSetInternal(oracleConnection, spName, includeReturnValueParameter);
            }
            return spParameterSetInternal;
        }
        internal static OracleParameter[] GetSpParameterSet(OracleConnection connection, string spName)
        {
            return OracleHelperParameterCache.GetSpParameterSet(connection, spName, false);
        }
        internal static OracleParameter[] GetSpParameterSet(OracleConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            OracleParameter[] spParameterSetInternal;
            using (OracleConnection oracleConnection = (OracleConnection)((ICloneable)connection).Clone())
            {
                spParameterSetInternal = OracleHelperParameterCache.GetSpParameterSetInternal(oracleConnection, spName, includeReturnValueParameter);
            }
            return spParameterSetInternal;
        }
        private static OracleParameter[] GetSpParameterSetInternal(OracleConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            string key = connection.ConnectionString + ":" + spName + (includeReturnValueParameter ? ":include ReturnValue Parameter" : "");
            OracleParameter[] array = OracleHelperParameterCache.paramCache[key] as OracleParameter[];
            if (array == null)
            {
                OracleParameter[] array2 = OracleHelperParameterCache.DiscoverSpParameterSet(connection, spName, includeReturnValueParameter);
                OracleHelperParameterCache.paramCache[key] = array2;
                array = array2;
            }
            return OracleHelperParameterCache.CloneParameters(array);
        }
    }
}
