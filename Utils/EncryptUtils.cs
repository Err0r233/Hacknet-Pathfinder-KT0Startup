
using System.Text;
using Hacknet;
using System.Security.Cryptography;

/*
 * Inspiration from 0DTK
 */

namespace KT0Mods.Utils
{
    public class EncryptUtils
    {
        public const string DELIM = "//";
        
        private const string FILE_HEADER = "SZIP::ENC v2.7------------";

        private const string DEFAULT_KEY = "default_32byte_key_1234567890abc!";

        public static string GenerateEncryptString(Folder folder)
        {
            string ret = folder.name + "\n";
            foreach (var target in folder.folders)
            {
                ret += GenerateEncryptString(target) + "\n";
            }

            foreach (var fileEntry in folder.files)
            {
                ret += GenerateEncryptFileString(fileEntry) + "\n";
            }

            return ret + DELIM;
        }

        public static string GenerateEncryptFileString(FileEntry file)
        {
            Thread.Sleep(200);
            return file.name + DELIM + file.data;
        }

        public static string EncryptMain(string data, string key = "default_32byte_key_1234567890abc!")
        {
            byte[] fileBytes = Encoding.UTF8.GetBytes(data);
            byte[] keyBytes = GenerateKey(key);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = keyBytes;
                aesAlg.GenerateIV();
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;
                
                using (ICryptoTransform encryptor = aesAlg.CreateEncryptor())
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(fileBytes, 0, fileBytes.Length);
                        csEncrypt.FlushFinalBlock();
                    }

                    byte[] encryptedData = msEncrypt.ToArray();

                    string hexData = ByteArrayToHexString(encryptedData);
                    return FILE_HEADER + hexData;
                }
            }
        }

        private static string ByteArrayToHexString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                sb.AppendFormat("{0:X2}", b);
            }

            return sb.ToString();
        }

        private static byte[] GenerateKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Console.WriteLine("[WARNING] Using default key! Not secure for production use.");
                key = DEFAULT_KEY;
            }

            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
            }
        }

        public static string DecryptMain(string data, string key = DEFAULT_KEY)
        {
            string hexData = data.Substring(FILE_HEADER.Length).Trim();
            byte[] encryptedBytes = HexStringToByteArray(hexData);
            byte[] keyBytes = GenerateKey(key);

            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    byte[] iv = new byte[16];
                    Array.Copy(encryptedBytes, 0, iv, 0, iv.Length);
                    aesAlg.Key = keyBytes;
                    aesAlg.IV = iv;
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Padding = PaddingMode.PKCS7;

                    int encryptedDataLength = encryptedBytes.Length - iv.Length;
                    byte[] cipherBytes = new byte[encryptedDataLength];
                    Array.Copy(encryptedBytes, iv.Length, cipherBytes, 0, encryptedDataLength);
                    using (ICryptoTransform decryptor = aesAlg.CreateDecryptor())
                    using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt, Encoding.UTF8))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                return "Decrypt Error";
            }
            
        }
        
        private static byte[] HexStringToByteArray(string hex)
        {
            if (hex.Length % 2 != 0)
            {
                throw new ArgumentException("Hex string must have even length");
            }

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }
    }
}

