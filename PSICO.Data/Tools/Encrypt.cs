using System.Security.Cryptography;
using System.Text;

namespace PSICO.Data.Tools
{
    public class Encrypt
    {
        public static string GenerateSeed()
        {
            Random random = new Random();

            int length = 10;

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789*";

            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GenerateUniqueId(int length = 7)
        {
            Random random = new Random();

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789*";

            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GenerateToken()
        {
            Random random = new Random();

            int length = 6;

            const string chars = "0123456789";

            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        #region SHA256
        public static string GetSHA256(string str)
        {
            SHA256 sha256 = SHA256.Create();

            ASCIIEncoding encoding = new ASCIIEncoding();

            byte[] stream = null!;

            StringBuilder sb = new StringBuilder();

            stream = sha256.ComputeHash(encoding.GetBytes(str));

            for (int i = 0; i < stream.Length; i++)
                sb.AppendFormat("{0:x2}", stream[i]);

            return sb.ToString();
        }

        public static (string, string) StringToEncrypt(string stringText)
        {
            //Generamos una llave para la encriptacion-desencriptacion
            var stringSeed = GenerateSeed();

            //Encriptamos la cadena
            var stringEncrypted = GetSHA256(stringSeed + stringText);

            // Se retorna la llave y la cadena encriptada
            return (stringSeed, stringEncrypted);
        }

        public static bool CompareStringEncrypted(string stringSeed, string stringText, string stringToCompare)
        {
            //Encriptamos la cadena
            var stringEncrypted = GetSHA256(stringSeed + stringText);

            // Validacion de que el hash encriptado sea igual
            return stringEncrypted.Equals(stringToCompare);
        }

        #endregion

        #region Encriptacion - Desencriptacion

        /* AES si es un metodo de ENCRIPTACION:
             * Puede desencriptarse con una clave.
             * Puede usarse para la encriptacion de cadenas de texto que despues se deseen usar.
        */

        public static string EncryptRijndael(string clearText, string password)
        {
            string EncryptionKey = password;
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);

            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey,
                    new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 }
                );

                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }

                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }

            return clearText;
        }

        public static string DecryptRijndael(string cipherText, string password)
        {
            string EncryptionKey = password;
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey,
                    new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 }
                );

                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }

                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }

            return cipherText;
        }

        #endregion
    }
}
