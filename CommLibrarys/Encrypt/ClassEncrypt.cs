using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace CommLibrarys.Encrypt
{
    [ClassInterface(ClassInterfaceType.None)]
    public class ClassEncrypt
    {
        private readonly SymmetricAlgorithm mobjCryptoService;
        private readonly string key;
        public ClassEncrypt()
        {
            this.mobjCryptoService = new RijndaelManaged();
            this.key = "DT";
        }
        private byte[] GetLegalKey()
        {
            string text = this.key;
            this.mobjCryptoService.GenerateKey();
            byte[] array = this.mobjCryptoService.Key;
            int num = array.Length;
            if (text.Length > num)
            {
                text = text.Substring(0, num);
            }
            else
            {
                if (text.Length < num)
                {
                    text = text.PadRight(num, ' ');
                }
            }
            return Encoding.ASCII.GetBytes(text);
        }
        private byte[] GetLegalIV()
        {
            string text = "dt";
            this.mobjCryptoService.GenerateIV();
            byte[] iV = this.mobjCryptoService.IV;
            int num = iV.Length;
            if (text.Length > num)
            {
                text = text.Substring(0, num);
            }
            else
            {
                if (text.Length < num)
                {
                    text = text.PadRight(num, ' ');
                }
            }
            return Encoding.ASCII.GetBytes(text);
        }
        public string Encrypt(string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            MemoryStream memoryStream = new MemoryStream();
            this.mobjCryptoService.Key = this.GetLegalKey();
            this.mobjCryptoService.IV = this.GetLegalIV();
            ICryptoTransform transform = this.mobjCryptoService.CreateEncryptor();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
            cryptoStream.Write(bytes, 0, bytes.Length);
            cryptoStream.FlushFinalBlock();
            memoryStream.Close();
            byte[] inArray = memoryStream.ToArray();
            return Convert.ToBase64String(inArray);
        }
        public string Decrypt(string str)
        {
            byte[] array = Convert.FromBase64String(str);
            MemoryStream stream = new MemoryStream(array, 0, array.Length);
            this.mobjCryptoService.Key = this.GetLegalKey();
            this.mobjCryptoService.IV = this.GetLegalIV();
            ICryptoTransform transform = this.mobjCryptoService.CreateDecryptor();
            CryptoStream stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Read);
            StreamReader streamReader = new StreamReader(stream2);
            return streamReader.ReadToEnd();
        }
        public byte[] md5(string str)
        {
            MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
            UTF8Encoding uTF8Encoding = new UTF8Encoding();
            return mD5CryptoServiceProvider.ComputeHash(uTF8Encoding.GetBytes(str));
        }
        public string md5pwd(string str)
        {
            MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
            byte[] value = mD5CryptoServiceProvider.ComputeHash(Encoding.Default.GetBytes(str));
            string text = BitConverter.ToString(value);
            return text.Replace("-", "");
        }
    }
}
