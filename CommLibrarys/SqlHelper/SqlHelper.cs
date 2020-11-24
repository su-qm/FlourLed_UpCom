using System;
using System.Data;
using System.Data.SqlClient;
using System.Xml;

namespace CommLibrarys.SqlHelper
{
    public sealed class SqlHelper
    {
        private enum SqlConnectionOwnership
        {
            Internal,
            External
        }
        private SqlHelper()
        {
        }
        private static void AttachParameters(SqlCommand command, SqlParameter[] commandParameters)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (commandParameters != null)
            {
                for (int i = 0; i < commandParameters.Length; i++)
                {
                    SqlParameter sqlParameter = commandParameters[i];
                    if (sqlParameter != null)
                    {
                        if ((sqlParameter.Direction == ParameterDirection.InputOutput || sqlParameter.Direction == ParameterDirection.Input) && sqlParameter.Value == null)
                        {
                            sqlParameter.Value = DBNull.Value;
                        }
                        command.Parameters.Add(sqlParameter);
                    }
                }
            }
        }
        private static void AssignParameterValues(SqlParameter[] commandParameters, DataRow dataRow)
        {
            if (commandParameters != null && dataRow != null)
            {
                int num = 0;
                for (int i = 0; i < commandParameters.Length; i++)
                {
                    SqlParameter sqlParameter = commandParameters[i];
                    if (sqlParameter.ParameterName == null || sqlParameter.ParameterName.Length <= 1)
                    {
                        throw new Exception(string.Format("Please provide a valid parameter name on the parameter #{0}, the ParameterName property has the following value: '{1}'.", num, sqlParameter.ParameterName));
                    }
                    if (dataRow.Table.Columns.IndexOf(sqlParameter.ParameterName.Substring(1)) != -1)
                    {
                        sqlParameter.Value = dataRow[sqlParameter.ParameterName.Substring(1)];
                    }
                    num++;
                }
            }
        }
        private static void AssignParameterValues(SqlParameter[] commandParameters, object[] parameterValues)
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
        private static void PrepareCommand(SqlCommand command, SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, SqlParameter[] commandParameters, out bool mustCloseConnection)
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
                SqlHelper.AttachParameters(command, commandParameters);
            }
        }
        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
        {
            return SqlHelper.ExecuteNonQuery(connectionString, commandType, commandText, null);
        }
        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            int result;
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                result = SqlHelper.ExecuteNonQuery(sqlConnection, commandType, commandText, commandParameters);
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
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                SqlHelper.AssignParameterValues(spParameterSet, parameterValues);
                result = SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static int ExecuteNonQuery(SqlConnection connection, CommandType commandType, string commandText)
        {
            return SqlHelper.ExecuteNonQuery(connection, commandType, commandText, null);
        }
        public static int ExecuteNonQuery(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            SqlCommand sqlCommand = new SqlCommand();
            bool flag = false;
            SqlHelper.PrepareCommand(sqlCommand, connection, null, commandType, commandText, commandParameters, out flag);
            int result = sqlCommand.ExecuteNonQuery();
            sqlCommand.Parameters.Clear();
            if (flag)
            {
                connection.Close();
            }
            return result;
        }
        public static int ExecuteNonQuery(SqlConnection connection, string spName, params object[] parameterValues)
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
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                SqlHelper.AssignParameterValues(spParameterSet, parameterValues);
                result = SqlHelper.ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = SqlHelper.ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static int ExecuteNonQuery(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            return SqlHelper.ExecuteNonQuery(transaction, commandType, commandText, null);
        }
        public static int ExecuteNonQuery(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            SqlCommand sqlCommand = new SqlCommand();
            bool flag = false;
            SqlHelper.PrepareCommand(sqlCommand, transaction.Connection, transaction, commandType, commandText, commandParameters, out flag);
            int result = sqlCommand.ExecuteNonQuery();
            sqlCommand.Parameters.Clear();
            return result;
        }
        public static int ExecuteNonQuery(SqlTransaction transaction, string spName, params object[] parameterValues)
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
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                SqlHelper.AssignParameterValues(spParameterSet, parameterValues);
                result = SqlHelper.ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = SqlHelper.ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText)
        {
            return SqlHelper.ExecuteDataset(connectionString, commandType, commandText, null);
        }
        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            DataSet result;
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                result = SqlHelper.ExecuteDataset(sqlConnection, commandType, commandText, commandParameters);
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
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                SqlHelper.AssignParameterValues(spParameterSet, parameterValues);
                result = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static DataSet ExecuteDataset(SqlConnection connection, CommandType commandType, string commandText)
        {
            return SqlHelper.ExecuteDataset(connection, commandType, commandText, null);
        }
        public static DataSet ExecuteDataset(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            SqlCommand sqlCommand = new SqlCommand();
            bool flag = false;
            SqlHelper.PrepareCommand(sqlCommand, connection, null, commandType, commandText, commandParameters, out flag);
            DataSet result;
            using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand))
            {
                DataSet dataSet = new DataSet();
                sqlDataAdapter.Fill(dataSet);
                sqlCommand.Parameters.Clear();
                if (flag)
                {
                    connection.Close();
                }
                result = dataSet;
            }
            return result;
        }
        public static DataSet ExecuteDataset(SqlConnection connection, string spName, params object[] parameterValues)
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
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                SqlHelper.AssignParameterValues(spParameterSet, parameterValues);
                result = SqlHelper.ExecuteDataset(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = SqlHelper.ExecuteDataset(connection, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static DataSet ExecuteDataset(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            return SqlHelper.ExecuteDataset(transaction, commandType, commandText, null);
        }
        public static DataSet ExecuteDataset(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            SqlCommand sqlCommand = new SqlCommand();
            bool flag = false;
            SqlHelper.PrepareCommand(sqlCommand, transaction.Connection, transaction, commandType, commandText, commandParameters, out flag);
            DataSet result;
            using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand))
            {
                DataSet dataSet = new DataSet();
                sqlDataAdapter.Fill(dataSet);
                sqlCommand.Parameters.Clear();
                result = dataSet;
            }
            return result;
        }
        public static DataSet ExecuteDataset(SqlTransaction transaction, string spName, params object[] parameterValues)
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
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                SqlHelper.AssignParameterValues(spParameterSet, parameterValues);
                result = SqlHelper.ExecuteDataset(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = SqlHelper.ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        private static SqlDataReader ExecuteReader(SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, SqlParameter[] commandParameters, SqlHelper.SqlConnectionOwnership connectionOwnership)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            bool flag = false;
            SqlCommand sqlCommand = new SqlCommand();
            SqlDataReader result;
            try
            {
                SqlHelper.PrepareCommand(sqlCommand, connection, transaction, commandType, commandText, commandParameters, out flag);
                SqlDataReader sqlDataReader;
                if (connectionOwnership == SqlHelper.SqlConnectionOwnership.External)
                {
                    sqlDataReader = sqlCommand.ExecuteReader();
                }
                else
                {
                    sqlDataReader = sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
                }
                bool flag2 = true;
                foreach (SqlParameter sqlParameter in sqlCommand.Parameters)
                {
                    if (sqlParameter.Direction != ParameterDirection.Input)
                    {
                        flag2 = false;
                    }
                }
                if (flag2)
                {
                    sqlCommand.Parameters.Clear();
                }
                result = sqlDataReader;
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
        public static SqlDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText)
        {
            return SqlHelper.ExecuteReader(connectionString, commandType, commandText, null);
        }
        public static SqlDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            SqlConnection sqlConnection = null;
            SqlDataReader result;
            try
            {
                sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();
                result = SqlHelper.ExecuteReader(sqlConnection, null, commandType, commandText, commandParameters, SqlHelper.SqlConnectionOwnership.Internal);
            }
            catch
            {
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                }
                throw;
            }
            return result;
        }
        public static SqlDataReader ExecuteReader(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            SqlDataReader result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                SqlHelper.AssignParameterValues(spParameterSet, parameterValues);
                result = SqlHelper.ExecuteReader(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = SqlHelper.ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static SqlDataReader ExecuteReader(SqlConnection connection, CommandType commandType, string commandText)
        {
            return SqlHelper.ExecuteReader(connection, commandType, commandText, null);
        }
        public static SqlDataReader ExecuteReader(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            return SqlHelper.ExecuteReader(connection, null, commandType, commandText, commandParameters, SqlHelper.SqlConnectionOwnership.External);
        }
        public static SqlDataReader ExecuteReader(SqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            SqlDataReader result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                SqlHelper.AssignParameterValues(spParameterSet, parameterValues);
                result = SqlHelper.ExecuteReader(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = SqlHelper.ExecuteReader(connection, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static SqlDataReader ExecuteReader(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            return SqlHelper.ExecuteReader(transaction, commandType, commandText, null);
        }
        public static SqlDataReader ExecuteReader(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            return SqlHelper.ExecuteReader(transaction.Connection, transaction, commandType, commandText, commandParameters, SqlHelper.SqlConnectionOwnership.External);
        }
        public static SqlDataReader ExecuteReader(SqlTransaction transaction, string spName, params object[] parameterValues)
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
            SqlDataReader result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                SqlHelper.AssignParameterValues(spParameterSet, parameterValues);
                result = SqlHelper.ExecuteReader(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = SqlHelper.ExecuteReader(transaction, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText)
        {
            return SqlHelper.ExecuteScalar(connectionString, commandType, commandText, null);
        }
        public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            object result;
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                result = SqlHelper.ExecuteScalar(sqlConnection, commandType, commandText, commandParameters);
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
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                SqlHelper.AssignParameterValues(spParameterSet, parameterValues);
                result = SqlHelper.ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = SqlHelper.ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static object ExecuteScalar(SqlConnection connection, CommandType commandType, string commandText)
        {
            return SqlHelper.ExecuteScalar(connection, commandType, commandText, null);
        }
        public static object ExecuteScalar(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            SqlCommand sqlCommand = new SqlCommand();
            bool flag = false;
            SqlHelper.PrepareCommand(sqlCommand, connection, null, commandType, commandText, commandParameters, out flag);
            object result = sqlCommand.ExecuteScalar();
            sqlCommand.Parameters.Clear();
            if (flag)
            {
                connection.Close();
            }
            return result;
        }
        public static object ExecuteScalar(SqlConnection connection, string spName, params object[] parameterValues)
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
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                SqlHelper.AssignParameterValues(spParameterSet, parameterValues);
                result = SqlHelper.ExecuteScalar(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = SqlHelper.ExecuteScalar(connection, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static object ExecuteScalar(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            return SqlHelper.ExecuteScalar(transaction, commandType, commandText, null);
        }
        public static object ExecuteScalar(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            SqlCommand sqlCommand = new SqlCommand();
            bool flag = false;
            SqlHelper.PrepareCommand(sqlCommand, transaction.Connection, transaction, commandType, commandText, commandParameters, out flag);
            object result = sqlCommand.ExecuteScalar();
            sqlCommand.Parameters.Clear();
            return result;
        }
        public static object ExecuteScalar(SqlTransaction transaction, string spName, params object[] parameterValues)
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
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                SqlHelper.AssignParameterValues(spParameterSet, parameterValues);
                result = SqlHelper.ExecuteScalar(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = SqlHelper.ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static XmlReader ExecuteXmlReader(SqlConnection connection, CommandType commandType, string commandText)
        {
            return SqlHelper.ExecuteXmlReader(connection, commandType, commandText, null);
        }
        public static XmlReader ExecuteXmlReader(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            bool flag = false;
            SqlCommand sqlCommand = new SqlCommand();
            XmlReader result;
            try
            {
                SqlHelper.PrepareCommand(sqlCommand, connection, null, commandType, commandText, commandParameters, out flag);
                XmlReader xmlReader = sqlCommand.ExecuteXmlReader();
                sqlCommand.Parameters.Clear();
                result = xmlReader;
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
        public static XmlReader ExecuteXmlReader(SqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            XmlReader result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                SqlHelper.AssignParameterValues(spParameterSet, parameterValues);
                result = SqlHelper.ExecuteXmlReader(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = SqlHelper.ExecuteXmlReader(connection, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static XmlReader ExecuteXmlReader(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            return SqlHelper.ExecuteXmlReader(transaction, commandType, commandText, null);
        }
        public static XmlReader ExecuteXmlReader(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            SqlCommand sqlCommand = new SqlCommand();
            bool flag = false;
            SqlHelper.PrepareCommand(sqlCommand, transaction.Connection, transaction, commandType, commandText, commandParameters, out flag);
            XmlReader result = sqlCommand.ExecuteXmlReader();
            sqlCommand.Parameters.Clear();
            return result;
        }
        public static XmlReader ExecuteXmlReader(SqlTransaction transaction, string spName, params object[] parameterValues)
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
            XmlReader result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                SqlHelper.AssignParameterValues(spParameterSet, parameterValues);
                result = SqlHelper.ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = SqlHelper.ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName);
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
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                SqlHelper.FillDataset(sqlConnection, commandType, commandText, dataSet, tableNames);
            }
        }
        public static void FillDataset(string connectionString, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params SqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                SqlHelper.FillDataset(sqlConnection, commandType, commandText, dataSet, tableNames, commandParameters);
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
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                SqlHelper.FillDataset(sqlConnection, spName, dataSet, tableNames, parameterValues);
            }
        }
        public static void FillDataset(SqlConnection connection, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            SqlHelper.FillDataset(connection, commandType, commandText, dataSet, tableNames, null);
        }
        public static void FillDataset(SqlConnection connection, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params SqlParameter[] commandParameters)
        {
            SqlHelper.FillDataset(connection, null, commandType, commandText, dataSet, tableNames, commandParameters);
        }
        public static void FillDataset(SqlConnection connection, string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
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
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                SqlHelper.AssignParameterValues(spParameterSet, parameterValues);
                SqlHelper.FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, tableNames, spParameterSet);
            }
            else
            {
                SqlHelper.FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, tableNames);
            }
        }
        public static void FillDataset(SqlTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            SqlHelper.FillDataset(transaction, commandType, commandText, dataSet, tableNames, null);
        }
        public static void FillDataset(SqlTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params SqlParameter[] commandParameters)
        {
            SqlHelper.FillDataset(transaction.Connection, transaction, commandType, commandText, dataSet, tableNames, commandParameters);
        }
        public static void FillDataset(SqlTransaction transaction, string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
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
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                SqlHelper.AssignParameterValues(spParameterSet, parameterValues);
                SqlHelper.FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, tableNames, spParameterSet);
            }
            else
            {
                SqlHelper.FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, tableNames);
            }
        }
        private static void FillDataset(SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params SqlParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }
            SqlCommand sqlCommand = new SqlCommand();
            bool flag = false;
            SqlHelper.PrepareCommand(sqlCommand, connection, transaction, commandType, commandText, commandParameters, out flag);
            using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand))
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
                        sqlDataAdapter.TableMappings.Add(text, tableNames[i]);
                        text += (i + 1).ToString();
                    }
                }
                sqlDataAdapter.Fill(dataSet);
                sqlCommand.Parameters.Clear();
            }
            if (flag)
            {
                connection.Close();
            }
        }
        public static void UpdateDataset(SqlCommand insertCommand, SqlCommand deleteCommand, SqlCommand updateCommand, DataSet dataSet, string tableName)
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
            using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter())
            {
                sqlDataAdapter.UpdateCommand = updateCommand;
                sqlDataAdapter.InsertCommand = insertCommand;
                sqlDataAdapter.DeleteCommand = deleteCommand;
                sqlDataAdapter.Update(dataSet, tableName);
                dataSet.AcceptChanges();
            }
        }
        public static SqlCommand CreateCommand(SqlConnection connection, string spName, params string[] sourceColumns)
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
            if (sourceColumns != null && sourceColumns.Length > 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                for (int i = 0; i < sourceColumns.Length; i++)
                {
                    spParameterSet[i].SourceColumn = sourceColumns[i];
                }
                SqlHelper.AttachParameters(sqlCommand, spParameterSet);
            }
            return sqlCommand;
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
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                SqlHelper.AssignParameterValues(spParameterSet, dataRow);
                result = SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static int ExecuteNonQueryTypedParams(SqlConnection connection, string spName, DataRow dataRow)
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
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                SqlHelper.AssignParameterValues(spParameterSet, dataRow);
                result = SqlHelper.ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = SqlHelper.ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static int ExecuteNonQueryTypedParams(SqlTransaction transaction, string spName, DataRow dataRow)
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
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                SqlHelper.AssignParameterValues(spParameterSet, dataRow);
                result = SqlHelper.ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = SqlHelper.ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
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
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                SqlHelper.AssignParameterValues(spParameterSet, dataRow);
                result = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static DataSet ExecuteDatasetTypedParams(SqlConnection connection, string spName, DataRow dataRow)
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
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                SqlHelper.AssignParameterValues(spParameterSet, dataRow);
                result = SqlHelper.ExecuteDataset(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = SqlHelper.ExecuteDataset(connection, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static DataSet ExecuteDatasetTypedParams(SqlTransaction transaction, string spName, DataRow dataRow)
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
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                SqlHelper.AssignParameterValues(spParameterSet, dataRow);
                result = SqlHelper.ExecuteDataset(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = SqlHelper.ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static SqlDataReader ExecuteReaderTypedParams(string connectionString, string spName, DataRow dataRow)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            SqlDataReader result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                SqlHelper.AssignParameterValues(spParameterSet, dataRow);
                result = SqlHelper.ExecuteReader(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = SqlHelper.ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static SqlDataReader ExecuteReaderTypedParams(SqlConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            SqlDataReader result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                SqlHelper.AssignParameterValues(spParameterSet, dataRow);
                result = SqlHelper.ExecuteReader(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = SqlHelper.ExecuteReader(connection, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static SqlDataReader ExecuteReaderTypedParams(SqlTransaction transaction, string spName, DataRow dataRow)
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
            SqlDataReader result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                SqlHelper.AssignParameterValues(spParameterSet, dataRow);
                result = SqlHelper.ExecuteReader(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = SqlHelper.ExecuteReader(transaction, CommandType.StoredProcedure, spName);
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
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                SqlHelper.AssignParameterValues(spParameterSet, dataRow);
                result = SqlHelper.ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = SqlHelper.ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static object ExecuteScalarTypedParams(SqlConnection connection, string spName, DataRow dataRow)
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
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                SqlHelper.AssignParameterValues(spParameterSet, dataRow);
                result = SqlHelper.ExecuteScalar(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = SqlHelper.ExecuteScalar(connection, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static object ExecuteScalarTypedParams(SqlTransaction transaction, string spName, DataRow dataRow)
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
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                SqlHelper.AssignParameterValues(spParameterSet, dataRow);
                result = SqlHelper.ExecuteScalar(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = SqlHelper.ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static XmlReader ExecuteXmlReaderTypedParams(SqlConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            XmlReader result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
                SqlHelper.AssignParameterValues(spParameterSet, dataRow);
                result = SqlHelper.ExecuteXmlReader(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = SqlHelper.ExecuteXmlReader(connection, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        public static XmlReader ExecuteXmlReaderTypedParams(SqlTransaction transaction, string spName, DataRow dataRow)
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
            XmlReader result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);
                SqlHelper.AssignParameterValues(spParameterSet, dataRow);
                result = SqlHelper.ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }
            else
            {
                result = SqlHelper.ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName);
            }
            return result;
        }
    }
}
