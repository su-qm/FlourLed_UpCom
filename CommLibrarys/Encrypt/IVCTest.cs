using System;
using System.Collections.Generic;
using System.Text;

namespace CommLibrarys.Encrypt
{
    public interface IVCTest
    {
        string Encrypt(string str);
        string Decrypt(string str);
    }
}
