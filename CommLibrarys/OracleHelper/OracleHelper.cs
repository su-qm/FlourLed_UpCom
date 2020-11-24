using System;
using System.Data;
using System.Data.OracleClient;

namespace CommLibrarys.OracleHelper
{
    public sealed class OracleHelper
    {
        private enum OracleConnectionOwnership
        {
            Internal,
            External
        }
        private OracleHelper()
        {
        }
        private static void AttachParameters(OracleCommand command, OracleParameter[] commandParameters)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (commandParameters != null)
            {
                for (int i = 0; i < commandParameters.Length; i++)
                {
                    OracleParameter oracleParameter = commandParameters[i];
                    if (oracleParameter != null)
                    {
                        if ((oracleParameter.Direction == ParameterDirection.InputOutput || oracleParameter.Direction == ParameterDirection.Input) && oracleParameter.Value == null)
                        {
                            oracleParameter.Value = DBNull.Value;
                        }
                        command.Parameters.Add(oracleParameter);
                    }
                }
            }
        }
        private static void AssignParameterValues(OracleParameter[] commandParameters, DataRow dataRow)
        {
            if (commandParameters != null && dataRow != null)
            {
                int num = 0;
                for (int i = 0; i < commandParameters.Length; i++)
                {
                    OracleParameter oracleParameter = commandParameters[i];
                    if (oracleParameter.ParameterName == null || oracleParameter.ParameterName.Length <= 1)
                    {
                        throw new Exception(string.Format("Please provide a valid parameter name on the parameter #{0}, the ParameterName property has the following value: '{1}'.", num, oracleParameter.ParameterName));
                    }
                    if (dataRow.Table.Columns.IndexOf(oracleParameter.ParameterName.Substring(1)) != -1)
                    {
                        oracleParameter.Value = dataRow[oracleParameter.ParameterName.Substring(1)];
                    }
                    num++;
                }
            }
        }
        private static void AssignParameterValues(OracleParameter[] commandParameters, object[] parameterValues)
        {
            if (commandParameters != null && parameterValues != null)
            {
                if (commandParameters.Length != parameterValues.Length)
                {
                    throw new ArgumentException("Parameter count does not match Parameter Value count.");
                }
                int i = 0;
                int num = commandParameters.Length;
                while (i < num)
                {
                    if (parameterValues[i] is IDbDataParameter)
                    {
                        IDbDataParameter dbDataParameter = (IDbDataParameter)parameterValues[i];
                        if (dbDataParameter.Value == null)
                        {
                            commandParameters[i].Value = DBNull.Value;
                        }
                        else
                        {
                            commandParameters[i].Value = dbDataParameter.Value;
                        }
                    }
                    else
                    {
                        if (parameterValues[i] == null)
                        {
                            commandParameters[i].Value = DBNull.Value;
                        }
                        else
                        {
                            commandParameters[i].Value = parameterValues[i];
                        }
                    }
                    i++;
                }
            }
        }
        private static void PrepareCommand(OracleCommand command, OracleConnection connection, OracleTransaction transaction, CommandType commandType, string commandText, OracleParameter[] commandParameters, out bool mustCloseConnection)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (commandText == null || commandText.Length == 0)
            {
                throw new ArgumentNullException("commandText");
            }
            if (connection.State != ConnectionState.Open)
            {
                mustCloseConnection = true;
                connection.Open();
            }
            else
            {
                mustCloseConnection = false;
            }
            command.Connection = connection;
            command.CommandText = commandText;
            if (transaction != null)
            {
                if (transaction.Connection == null)
                {
                    throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
                }
                command.Transaction = transaction;
            }
            command.CommandType = commandType;
            if (commandParameters != null)
            {
                OracleHelper.AttachParameters(command, commandParameters);
            }
        }
        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
        {
            return OracleHelper.ExecuteNonQuery(connectionString, commandType, commandText, null);
        }
        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            int result;
            using (OracleConnection oracleConnection = new OracleConnection(connectionString))
            {
                oracleConnection.Open();
                result = OracleHelper.ExecuteNonQuery(oracleConnection, commandType, commandText, commandParameters);
            }
            return result;
        }
        public static int ExecuteNonQuery(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            int result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                OracleParameter[] spParameterSet = OracleHelperParameterCache.GetSpParameterSet(connectionString, spName);
                OracleHelper.AssignParameterValues(spParameterSet, parameterValues);
                result = OracleHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = OracleHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static int ExecuteNonQuery(OracleConnection connection, CommandType commandType, string commandText)
        {
            return OracleHelper.ExecuteNonQuery(connection, commandType, commandText, null);
        }
        public static int ExecuteNonQuery(OracleConnection connection, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            OracleCommand oracleCommand = new OracleCommand();
            bool flag = false;
            OracleHelper.PrepareCommand(oracleCommand, connection, null, commandType, commandText, commandParameters, out flag);
            int result = oracleCommand.ExecuteNonQuery();
            oracleCommand.Parameters.Clear();
            if (flag)
            {
                connection.Close();
            }
            return result;
        }
        public static int ExecuteNonQuery(OracleConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            int result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                OracleParameter[] spParameterSet = OracleHelperParameterCache.GetSpParameterSet(connection, spName);
                OracleHelper.AssignParameterValues(spParameterSet, parameterValues);
                result = OracleHelper.ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = OracleHelper.ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static int ExecuteNonQuery(OracleTransaction transaction, CommandType commandType, string commandText)
        {
            return OracleHelper.ExecuteNonQuery(transaction, commandType, commandText, null);
        }
        public static int ExecuteNonQuery(OracleTransaction transaction, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            OracleCommand oracleCommand = new OracleCommand();
            bool flag = false;
            OracleHelper.PrepareCommand(oracleCommand, transaction.Connection, transaction, commandType, commandText, commandParameters, out flag);
            int result = oracleCommand.ExecuteNonQuery();
            oracleCommand.Parameters.Clear();
            return result;
        }
        public static int ExecuteNonQuery(OracleTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            int result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                OracleParameter[] spParameterSet = OracleHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                OracleHelper.AssignParameterValues(spParameterSet, parameterValues);
                result = OracleHelper.ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = OracleHelper.ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText)
        {
            return OracleHelper.ExecuteDataset(connectionString, commandType, commandText, null);
        }
        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            DataSet result;
            using (OracleConnection oracleConnection = new OracleConnection(connectionString))
            {
                oracleConnection.Open();
                result = OracleHelper.ExecuteDataset(oracleConnection, commandType, commandText, commandParameters);
            }
            return result;
        }
        public static DataSet ExecuteDataset(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            DataSet result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                OracleParameter[] spParameterSet = OracleHelperParameterCache.GetSpParameterSet(connectionString, spName);
                OracleHelper.AssignParameterValues(spParameterSet, parameterValues);
                result = OracleHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = OracleHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static DataSet ExecuteDataset(OracleConnection connection, CommandType commandType, string commandText)
        {
            return OracleHelper.ExecuteDataset(connection, commandType, commandText, null);
        }
        public static DataSet ExecuteDataset(OracleConnection connection, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            OracleCommand oracleCommand = new OracleCommand();
            bool flag = false;
            OracleHelper.PrepareCommand(oracleCommand, connection, null, commandType, commandText, commandParameters, out flag);
            DataSet result;
            using (OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(oracleCommand))
            {
                DataSet dataSet = new DataSet();
                oracleDataAdapter.Fill(dataSet);
                oracleCommand.Parameters.Clear();
                if (flag)
                {
                    connection.Close();
                }
                result = dataSet;
            }
            return result;
        }
        public static DataSet ExecuteDataset(OracleConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            DataSet result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                OracleParameter[] spParameterSet = OracleHelperParameterCache.GetSpParameterSet(connection, spName);
                OracleHelper.AssignParameterValues(spParameterSet, parameterValues);
                result = OracleHelper.ExecuteDataset(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = OracleHelper.ExecuteDataset(connection, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static DataSet ExecuteDataset(OracleTransaction transaction, CommandType commandType, string commandText)
        {
            return OracleHelper.ExecuteDataset(transaction, commandType, commandText, null);
        }
        public static DataSet ExecuteDataset(OracleTransaction transaction, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            OracleCommand oracleCommand = new OracleCommand();
            bool flag = false;
            OracleHelper.PrepareCommand(oracleCommand, transaction.Connection, transaction, commandType, commandText, commandParameters, out flag);
            DataSet result;
            using (OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(oracleCommand))
            {
                DataSet dataSet = new DataSet();
                oracleDataAdapter.Fill(dataSet);
                oracleCommand.Parameters.Clear();
                result = dataSet;
            }
            return result;
        }
        public static DataSet ExecuteDataset(OracleTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            DataSet result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                OracleParameter[] spParameterSet = OracleHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                OracleHelper.AssignParameterValues(spParameterSet, parameterValues);
                result = OracleHelper.ExecuteDataset(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = OracleHelper.ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        private static OracleDataReader ExecuteReader(OracleConnection connection, OracleTransaction transaction, CommandType commandType, string commandText, OracleParameter[] commandParameters, OracleHelper.OracleConnectionOwnership connectionOwnership)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            bool flag = false;
            OracleCommand oracleCommand = new OracleCommand();
            OracleDataReader result;
            try
            {
                OracleHelper.PrepareCommand(oracleCommand, connection, transaction, commandType, commandText, commandParameters, out flag);
                OracleDataReader oracleDataReader;
                if (connectionOwnership == OracleHelper.OracleConnectionOwnership.External)
                {
                    oracleDataReader = oracleCommand.ExecuteReader();
                }
                else
                {
                    oracleDataReader = oracleCommand.ExecuteReader(CommandBehavior.CloseConnection);
                }
                bool flag2 = true;
                foreach (OracleParameter oracleParameter in oracleCommand.Parameters)
                {
                    if (oracleParameter.Direction != ParameterDirection.Input)
                    {
                        flag2 = false;
                    }
                }
                if (flag2)
                {
                    oracleCommand.Parameters.Clear();
                }
                result = oracleDataReader;
            }
            catch
            {
                if (flag)
                {
                    connection.Close();
                }
                throw;
            }
            return result;
        }
        public static OracleDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText)
        {
            return OracleHelper.ExecuteReader(connectionString, commandType, commandText, null);
        }
        public static OracleDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            OracleConnection oracleConnection = null;
            OracleDataReader result;
            try
            {
                oracleConnection = new OracleConnection(connectionString);
                oracleConnection.Open();
                result = OracleHelper.ExecuteReader(oracleConnection, null, commandType, commandText, commandParameters, OracleHelper.OracleConnectionOwnership.Internal);
            }
            catch
            {
                if (oracleConnection != null)
                {
                    oracleConnection.Close();
                }
                throw;
            }
            return result;
        }
        public static OracleDataReader ExecuteReader(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            OracleDataReader result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                OracleParameter[] spParameterSet = OracleHelperParameterCache.GetSpParameterSet(connectionString, spName);
                OracleHelper.AssignParameterValues(spParameterSet, parameterValues);
                result = OracleHelper.ExecuteReader(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = OracleHelper.ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static OracleDataReader ExecuteReader(OracleConnection connection, CommandType commandType, string commandText)
        {
            return OracleHelper.ExecuteReader(connection, commandType, commandText, null);
        }
        public static OracleDataReader ExecuteReader(OracleConnection connection, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            return OracleHelper.ExecuteReader(connection, null, commandType, commandText, commandParameters, OracleHelper.OracleConnectionOwnership.External);
        }
        public static OracleDataReader ExecuteReader(OracleConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            OracleDataReader result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                OracleParameter[] spParameterSet = OracleHelperParameterCache.GetSpParameterSet(connection, spName);
                OracleHelper.AssignParameterValues(spParameterSet, parameterValues);
                result = OracleHelper.ExecuteReader(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = OracleHelper.ExecuteReader(connection, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static OracleDataReader ExecuteReader(OracleTransaction transaction, CommandType commandType, string commandText)
        {
            return OracleHelper.ExecuteReader(transaction, commandType, commandText, null);
        }
        public static OracleDataReader ExecuteReader(OracleTransaction transaction, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            return OracleHelper.ExecuteReader(transaction.Connection, transaction, commandType, commandText, commandParameters, OracleHelper.OracleConnectionOwnership.External);
        }
        public static OracleDataReader ExecuteReader(OracleTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            OracleDataReader result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                OracleParameter[] spParameterSet = OracleHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                OracleHelper.AssignParameterValues(spParameterSet, parameterValues);
                result = OracleHelper.ExecuteReader(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = OracleHelper.ExecuteReader(transaction, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText)
        {
            return OracleHelper.ExecuteScalar(connectionString, commandType, commandText, null);
        }
        public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            object result;
            using (OracleConnection oracleConnection = new OracleConnection(connectionString))
            {
                oracleConnection.Open();
                result = OracleHelper.ExecuteScalar(oracleConnection, commandType, commandText, commandParameters);
            }
            return result;
        }
        public static object ExecuteScalar(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            object result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                OracleParameter[] spParameterSet = OracleHelperParameterCache.GetSpParameterSet(connectionString, spName);
                OracleHelper.AssignParameterValues(spParameterSet, parameterValues);
                result = OracleHelper.ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = OracleHelper.ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static object ExecuteScalar(OracleConnection connection, CommandType commandType, string commandText)
        {
            return OracleHelper.ExecuteScalar(connection, commandType, commandText, null);
        }
        public static object ExecuteScalar(OracleConnection connection, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            OracleCommand oracleCommand = new OracleCommand();
            bool flag = false;
            OracleHelper.PrepareCommand(oracleCommand, connection, null, commandType, commandText, commandParameters, out flag);
            object result = oracleCommand.ExecuteScalar();
            oracleCommand.Parameters.Clear();
            if (flag)
            {
                connection.Close();
            }
            return result;
        }
        public static object ExecuteScalar(OracleConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            object result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                OracleParameter[] spParameterSet = OracleHelperParameterCache.GetSpParameterSet(connection, spName);
                OracleHelper.AssignParameterValues(spParameterSet, parameterValues);
                result = OracleHelper.ExecuteScalar(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = OracleHelper.ExecuteScalar(connection, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static object ExecuteScalar(OracleTransaction transaction, CommandType commandType, string commandText)
        {
            return OracleHelper.ExecuteScalar(transaction, commandType, commandText, null);
        }
        public static object ExecuteScalar(OracleTransaction transaction, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            OracleCommand oracleCommand = new OracleCommand();
            bool flag = false;
            OracleHelper.PrepareCommand(oracleCommand, transaction.Connection, transaction, commandType, commandText, commandParameters, out flag);
            object result = oracleCommand.ExecuteScalar();
            oracleCommand.Parameters.Clear();
            return result;
        }
        public static object ExecuteScalar(OracleTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            object result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                OracleParameter[] spParameterSet = OracleHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                OracleHelper.AssignParameterValues(spParameterSet, parameterValues);
                result = OracleHelper.ExecuteScalar(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = OracleHelper.ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static void FillDataset(string connectionString, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }
            using (OracleConnection oracleConnection = new OracleConnection(connectionString))
            {
                oracleConnection.Open();
                OracleHelper.FillDataset(oracleConnection, commandType, commandText, dataSet, tableNames);
            }
        }
        public static void FillDataset(string connectionString, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params OracleParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }
            using (OracleConnection oracleConnection = new OracleConnection(connectionString))
            {
                oracleConnection.Open();
                OracleHelper.FillDataset(oracleConnection, commandType, commandText, dataSet, tableNames, commandParameters);
            }
        }
        public static void FillDataset(string connectionString, string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }
            using (OracleConnection oracleConnection = new OracleConnection(connectionString))
            {
                oracleConnection.Open();
                OracleHelper.FillDataset(oracleConnection, spName, dataSet, tableNames, parameterValues);
            }
        }
        public static void FillDataset(OracleConnection connection, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            OracleHelper.FillDataset(connection, commandType, commandText, dataSet, tableNames, null);
        }
        public static void FillDataset(OracleConnection connection, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params OracleParameter[] commandParameters)
        {
            OracleHelper.FillDataset(connection, null, commandType, commandText, dataSet, tableNames, commandParameters);
        }
        public static void FillDataset(OracleConnection connection, string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            if (parameterValues != null && parameterValues.Length > 0)
            {
                OracleParameter[] spParameterSet = OracleHelperParameterCache.GetSpParameterSet(connection, spName);
                OracleHelper.AssignParameterValues(spParameterSet, parameterValues);
                OracleHelper.FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, tableNames, spParameterSet);
            }
            else
            {
                OracleHelper.FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, tableNames);
            }
        }
        public static void FillDataset(OracleTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            OracleHelper.FillDataset(transaction, commandType, commandText, dataSet, tableNames, null);
        }
        public static void FillDataset(OracleTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params OracleParameter[] commandParameters)
        {
            OracleHelper.FillDataset(transaction.Connection, transaction, commandType, commandText, dataSet, tableNames, commandParameters);
        }
        public static void FillDataset(OracleTransaction transaction, string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            if (parameterValues != null && parameterValues.Length > 0)
            {
                OracleParameter[] spParameterSet = OracleHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                OracleHelper.AssignParameterValues(spParameterSet, parameterValues);
                OracleHelper.FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, tableNames, spParameterSet);
            }
            else
            {
                OracleHelper.FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, tableNames);
            }
        }
        private static void FillDataset(OracleConnection connection, OracleTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params OracleParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }
            OracleCommand oracleCommand = new OracleCommand();
            bool flag = false;
            OracleHelper.PrepareCommand(oracleCommand, connection, transaction, commandType, commandText, commandParameters, out flag);
            using (OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(oracleCommand))
            {
                if (tableNames != null && tableNames.Length > 0)
                {
                    string text = "Table";
                    for (int i = 0; i < tableNames.Length; i++)
                    {
                        if (tableNames[i] == null || tableNames[i].Length == 0)
                        {
                            throw new ArgumentException("The tableNames parameter must contain a list of tables, a value was provided as null or empty string.", "tableNames");
                        }
                        oracleDataAdapter.TableMappings.Add(text, tableNames[i]);
                        text += (i + 1).ToString();
                    }
                }
                oracleDataAdapter.Fill(dataSet);
                oracleCommand.Parameters.Clear();
            }
            if (flag)
            {
                connection.Close();
            }
        }
        public static void UpdateDataset(OracleCommand insertCommand, OracleCommand deleteCommand, OracleCommand updateCommand, DataSet dataSet, string tableName)
        {
            if (insertCommand == null)
            {
                throw new ArgumentNullException("insertCommand");
            }
            if (deleteCommand == null)
            {
                throw new ArgumentNullException("deleteCommand");
            }
            if (updateCommand == null)
            {
                throw new ArgumentNullException("updateCommand");
            }
            if (tableName == null || tableName.Length == 0)
            {
                throw new ArgumentNullException("tableName");
            }
            using (OracleDataAdapter oracleDataAdapter = new OracleDataAdapter())
            {
                oracleDataAdapter.UpdateCommand = updateCommand;
                oracleDataAdapter.InsertCommand = insertCommand;
                oracleDataAdapter.DeleteCommand = deleteCommand;
                oracleDataAdapter.Update(dataSet, tableName);
                dataSet.AcceptChanges();
            }
        }
        public static OracleCommand CreateCommand(OracleConnection connection, string spName, params string[] sourceColumns)
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
            if (sourceColumns != null && sourceColumns.Length > 0)
            {
                OracleParameter[] spParameterSet = OracleHelperParameterCache.GetSpParameterSet(connection, spName);
                for (int i = 0; i < sourceColumns.Length; i++)
                {
                    spParameterSet[i].SourceColumn = sourceColumns[i];
                }
                OracleHelper.AttachParameters(oracleCommand, spParameterSet);
            }
            return oracleCommand;
        }
        public static int ExecuteNonQueryTypedParams(string connectionString, string spName, DataRow dataRow)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            int result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                OracleParameter[] spParameterSet = OracleHelperParameterCache.GetSpParameterSet(connectionString, spName);
                OracleHelper.AssignParameterValues(spParameterSet, dataRow);
                result = OracleHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = OracleHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static int ExecuteNonQueryTypedParams(OracleConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            int result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                OracleParameter[] spParameterSet = OracleHelperParameterCache.GetSpParameterSet(connection, spName);
                OracleHelper.AssignParameterValues(spParameterSet, dataRow);
                result = OracleHelper.ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = OracleHelper.ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static int ExecuteNonQueryTypedParams(OracleTransaction transaction, string spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            int result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                OracleParameter[] spParameterSet = OracleHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                OracleHelper.AssignParameterValues(spParameterSet, dataRow);
                result = OracleHelper.ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = OracleHelper.ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static DataSet ExecuteDatasetTypedParams(string connectionString, string spName, DataRow dataRow)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            DataSet result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                OracleParameter[] spParameterSet = OracleHelperParameterCache.GetSpParameterSet(connectionString, spName);
                OracleHelper.AssignParameterValues(spParameterSet, dataRow);
                result = OracleHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = OracleHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static DataSet ExecuteDatasetTypedParams(OracleConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            DataSet result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                OracleParameter[] spParameterSet = OracleHelperParameterCache.GetSpParameterSet(connection, spName);
                OracleHelper.AssignParameterValues(spParameterSet, dataRow);
                result = OracleHelper.ExecuteDataset(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = OracleHelper.ExecuteDataset(connection, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static DataSet ExecuteDatasetTypedParams(OracleTransaction transaction, string spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            DataSet result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                OracleParameter[] spParameterSet = OracleHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                OracleHelper.AssignParameterValues(spParameterSet, dataRow);
                result = OracleHelper.ExecuteDataset(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = OracleHelper.ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static OracleDataReader ExecuteReaderTypedParams(string connectionString, string spName, DataRow dataRow)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            OracleDataReader result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                OracleParameter[] spParameterSet = OracleHelperParameterCache.GetSpParameterSet(connectionString, spName);
                OracleHelper.AssignParameterValues(spParameterSet, dataRow);
                result = OracleHelper.ExecuteReader(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = OracleHelper.ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static OracleDataReader ExecuteReaderTypedParams(OracleConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            OracleDataReader result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                OracleParameter[] spParameterSet = OracleHelperParameterCache.GetSpParameterSet(connection, spName);
                OracleHelper.AssignParameterValues(spParameterSet, dataRow);
                result = OracleHelper.ExecuteReader(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = OracleHelper.ExecuteReader(connection, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static OracleDataReader ExecuteReaderTypedParams(OracleTransaction transaction, string spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            OracleDataReader result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                OracleParameter[] spParameterSet = OracleHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                OracleHelper.AssignParameterValues(spParameterSet, dataRow);
                result = OracleHelper.ExecuteReader(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = OracleHelper.ExecuteReader(transaction, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static object ExecuteScalarTypedParams(string connectionString, string spName, DataRow dataRow)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            object result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                OracleParameter[] spParameterSet = OracleHelperParameterCache.GetSpParameterSet(connectionString, spName);
                OracleHelper.AssignParameterValues(spParameterSet, dataRow);
                result = OracleHelper.ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = OracleHelper.ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static object ExecuteScalarTypedParams(OracleConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            object result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                OracleParameter[] spParameterSet = OracleHelperParameterCache.GetSpParameterSet(connection, spName);
                OracleHelper.AssignParameterValues(spParameterSet, dataRow);
                result = OracleHelper.ExecuteScalar(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = OracleHelper.ExecuteScalar(connection, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static object ExecuteScalarTypedParams(OracleTransaction transaction, string spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            object result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                OracleParameter[] spParameterSet = OracleHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                OracleHelper.AssignParameterValues(spParameterSet, dataRow);
                result = OracleHelper.ExecuteScalar(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = OracleHelper.ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
            }
            return result;
        }
    }
}
