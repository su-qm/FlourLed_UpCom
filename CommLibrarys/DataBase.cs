using System;

namespace CommLibrarys
{
    public class DataBase
    {
        private string _server;
        private string _database;
        private string _userid;
        private string _password;
        public string server
        {
            get
            {
                return this._server;
            }
            set
            {
                this._server = value;
            }
        }
        public string database
        {
            get
            {
                return this._database;
            }
            set
            {
                this._database = value;
            }
        }
        public string userid
        {
            get
            {
                return this._userid;
            }
            set
            {
                this._userid = value;
            }
        }
        public string password
        {
            get
            {
                return this._password;
            }
            set
            {
                this._password = value;
            }
        }
    }
}
