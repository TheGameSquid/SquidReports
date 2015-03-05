using System;
using System.Runtime.Serialization.Formatters;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace SquidReports.DataCollector.Helpers
{
    public class Crypto
    {
        public static string GetMD5HashFromString(string input)
        {
            // Step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // Step 2, convert byte array to Hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        public static string GetMD5HashFromObject(object o)
        {
            string objectString = JsonConvert.SerializeObject(o, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
            });

            return GetMD5HashFromString(objectString);
        }
    }
}
