using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace EPMI.Core.Encryption
{
    public class AES
    {
        const string SALTCHARS = "abcdefhijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

        public static string EncryptionKey
        {
            set;
            private get;
        }

        static string GenerateSalt()
        {
            StringBuilder salt = new StringBuilder();
            Random rnd = new Random();
            while (salt.Length < 10)
            {
                int index = (int)(rnd.Next(SALTCHARS.Length));
                salt.Append(SALTCHARS.Substring(index, 1));
            }
            return salt.ToString();
        }

        static string[] SplitSalt(string encryptedText)
        {
            int index = encryptedText.IndexOf('$');
            List<string> parts = new List<string>();
            parts.Add(encryptedText.Substring(0, index));
            parts.Add(encryptedText.Substring(index + 1));
            return parts.ToArray();
        }

        public static string EncryptString(string plainText)
        {
            string encryptedString = string.Empty;
            RijndaelManaged algo = new RijndaelManaged();
            try
            {
                // for Convenience, use some static string as salt.
                string _salt = GenerateSalt();
                byte[] salt = Encoding.ASCII.GetBytes(_salt);
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(EncryptionKey, salt);
                algo.Key = key.GetBytes(algo.KeySize / 8);
                algo.IV = key.GetBytes(algo.BlockSize / 8);
                ICryptoTransform encryptor = algo.CreateEncryptor();
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream encryptStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(encryptStream))
                        {
                            sw.Write(plainText);
                            //encryptStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                        }
                    }
                    encryptedString = string.Format("{0}${1}", _salt, Convert.ToBase64String(ms.ToArray()));
                }
            }
            finally
            {
                if (algo != null)
                    algo.Clear();
            }
            return encryptedString;
        }

        public static string DecryptString(string encryptedText)
        {
            string plainString = string.Empty;
            RijndaelManaged algo = new RijndaelManaged();
            try
            {
                string[] parts = SplitSalt(encryptedText);
                // make sure that you use the same salt.
                byte[] salt = Encoding.ASCII.GetBytes(parts[0]);
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(EncryptionKey, salt);
                algo.Key = key.GetBytes(algo.KeySize / 8);
                algo.IV = key.GetBytes(algo.BlockSize / 8);
                ICryptoTransform decryptor = algo.CreateDecryptor();
                byte[] bytes = Convert.FromBase64String(parts[1]);
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    using (CryptoStream decryptStream = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(decryptStream))
                        {
                            plainString = sr.ReadToEnd();
                        }
                    }
                }
            }
            finally
            {
                if (algo != null)
                    algo.Clear();
            }
            return plainString;
        }
        public static string Decode(string value)
        {
            return System.Text.RegularExpressions.Regex.Replace(value,
                @"\{\{(?<password>[^\}]+)\}\}",
                delegate(System.Text.RegularExpressions.Match m) { return DecryptString(m.Groups["password"].ToString()); }
                );
        }
    }
}
