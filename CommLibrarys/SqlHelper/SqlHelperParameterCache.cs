using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;

namespace CommLibrarys.SqlHelper
{
    public sealed class SqlHelperParameterCache
    {

        private static Hashtable paramCache = Hashtable.Synchronized(new Hashtable());
        private SqlHelperParameterCache()
        {
        }
        private static SqlParameter[] DiscoverSpParameterSet(SqlConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            SqlCommand sqlCommand = new SqlCommand(spName, connection);
            sqlCommand.CommandType = CommandType.StoredProcedure;
            connection.Open();
            SqlCommandBuilder.DeriveParameters(sqlCommand);
            connection.Close();
            if (!includeReturnValueParameter)
            {
                sqlCommand.Parameters.RemoveAt(0);
            }
            SqlParameter[] array = new SqlParameter[sqlCommand.Parameters.Count];
            sqlCommand.Parameters.CopyTo(array, 0);
            SqlParameter[] array2 = array;
            for (int i = 0; i < array2.Length; i++)
            {
                SqlParameter sqlParameter = array2[i];
                sqlParameter.Value = DBNull.Value;
            }
            return array;
        }
        private static SqlParameter[] CloneParameters(SqlParameter[] originalParameters)
        {
            SqlParameter[] array = new SqlParameter[originalParameters.Length];
            int i = 0;
            int num = originalParameters.Length;
            while (i < num)
            {
                array[i] = (SqlParameter)((ICloneable)originalParameters[i]).Clone();
                i++;
            }
            return array;
        }
        public static void CacheParameterSet(string connectionString, string commandText, params SqlParameter[] commandParameters)
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
            SqlHelperParameterCache.paramCache[key] = commandParameters;
        }
        public static SqlParameter[] GetCachedParameterSet(string connectionString, string commandText)
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
            SqlParameter[] array = SqlHelperParameterCache.paramCache[key] as SqlParameter[];
            SqlParameter[] result;
            if (array == null)
            {
                result = null;
            }
            else
            {
                result = SqlHelperParameterCache.CloneParameters(array);
            }
            return result;
        }
        public static SqlParameter[] GetSpParameterSet(string connectionString, string spName)
        {
            return SqlHelperParameterCache.GetSpParameterSet(connectionString, spName, false);
        }
        public static SqlParameter[] GetSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            SqlParameter[] spParameterSetInternal;
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                spParameterSetInternal = SqlHelperParameterCache.GetSpParameterSetInternal(sqlConnection, spName, includeReturnValueParameter);
            }
            return spParameterSetInternal;
        }
        internal static SqlParameter[] GetSpParameterSet(SqlConnection connection, string spName)
        {
            return SqlHelperParameterCache.GetSpParameterSet(connection, spName, false);
        }
        internal static SqlParameter[] GetSpParameterSet(SqlConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            SqlParameter[] spParameterSetInternal;
            using (SqlConnection sqlConnection = (SqlConnection)((ICloneable)connection).Clone())
            {
                spParameterSetInternal = SqlHelperParameterCache.GetSpParameterSetInternal(sqlConnection, spName, includeReturnValueParameter);
            }
            return spParameterSetInternal;
        }
        private static SqlParameter[] GetSpParameterSetInternal(SqlConnection connection, string spName, bool includeReturnValueParameter)
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
            SqlParameter[] array = SqlHelperParameterCache.paramCache[key] as SqlParameter[];
            if (array == null)
            {
                SqlParameter[] array2 = SqlHelperParameterCache.DiscoverSpParameterSet(connection, spName, includeReturnValueParameter);
                SqlHelperParameterCache.paramCache[key] = array2;
                array = array2;
            }
            return SqlHelperParameterCache.CloneParameters(array);
        }
    }
}
