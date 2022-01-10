using System;
using System.Security.Cryptography;
using System.Text;

namespace CardGame
{
    class Crypto
    {
        public static string EncryptPassword(string plainText)
        {
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            //set the secret key for the tripleDES algorithm
            tdes.Key = Encoding.Default.GetBytes("D_phmpds_6k8oRPm");

            tdes.Mode = CipherMode.ECB;
            //padding mode(if any extra byte added)
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            string textToEncrypt = plainText;
            //transform the specified region of bytes array to resultArray
            byte[] resultArray =
              cTransform.TransformFinalBlock(
                  System.Text.ASCIIEncoding.Default.GetBytes(textToEncrypt),
                  0,
                  textToEncrypt.Length);
            //Release resources held by TripleDes Encryptor
            tdes.Clear();
            //Return the encrypted data
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }
    }
}
