using System;

namespace CommLibrarys
{
    public class Global
    {
        private static int _id;
        private static string _userid;
        private static string _username;
        private static string _password;
        private static string _truename;
        private static string _question;
        private static string _answer;
        private static int _enable;
        public static int id
        {
            get
            {
                return Global._id;
            }
            set
            {
                Global._id = value;
            }
        }
        public static string userid
        {
            get
            {
                return Global._userid;
            }
            set
            {
                Global._userid = value;
            }
        }
        public static string username
        {
            get
            {
                return Global._username;
            }
            set
            {
                Global._username = value;
            }
        }
        public static string password
        {
            get
            {
                return Global._password;
            }
            set
            {
                Global._password = value;
            }
        }
        public static string truename
        {
            get
            {
                return Global._truename;
            }
            set
            {
                Global._truename = value;
            }
        }
        public static string question
        {
            get
            {
                return Global._question;
            }
            set
            {
                Global._question = value;
            }
        }
        public static string answer
        {
            get
            {
                return Global._answer;
            }
            set
            {
                Global._answer = value;
            }
        }
        public static int enable
        {
            get
            {
                return Global._enable;
            }
            set
            {
                Global._enable = value;
            }
        }
    }
}
