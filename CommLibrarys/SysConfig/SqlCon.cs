using System;
using System.Data.SqlClient;

namespace CommLibrarys.SysConfig
{
    public class SqlCon
    {
        public SqlConnection con(string constr)
        {
            return new SqlConnection(constr);
        }
        public SqlConnection con()
        {
            ConfigStr configStr = new ConfigStr();
            return new SqlConnection(configStr.ConnectionStr());
        }
        public SqlConnection Regcon()
        {
            DataBase dataBase = new DataBase();
            dataBase.server = Register.ReadRegValue("server", true);
            dataBase.database = Register.ReadRegValue("database", true);
            dataBase.userid = Register.ReadRegValue("username", true);
            dataBase.password = Register.ReadRegValue("pwd", true);
            string connectionString = string.Concat(new string[]
			{
				"server=", 
				dataBase.server, 
				";database=", 
				dataBase.database, 
				";user id=", 
				dataBase.userid, 
				";password=", 
				dataBase.password, 
				";"
			});
            return new SqlConnection(connectionString);
        }
        public SqlConnection Regcon(DataBase db)
        {
            string connectionString = string.Concat(new string[]
			{
				"server=", 
				db.server, 
				";database=", 
				db.database, 
				";user id=", 
				db.userid, 
				";password=", 
				db.password, 
				";"
			});
            return new SqlConnection(connectionString);
        }
    }
}
