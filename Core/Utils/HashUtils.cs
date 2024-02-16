using System;
using System.Linq;
using System.Text;

namespace Core.Utils
{
    public static class HashUtils
    {
        public static string SHA1(string value)
        {
            return ComputeStringHash<System.Security.Cryptography.SHA1>(value);
        }

        public static string SHA256(string value)
        {
            return ComputeStringHash<System.Security.Cryptography.SHA256>(value);
        }

        private static string ComputeStringHash<THashAlgo>(string value)
            where THashAlgo : System.Security.Cryptography.HashAlgorithm
        {
            var CreateAlgo = typeof(THashAlgo).GetMethod("Create", new Type[] { });

            using (var hashAlgo = (THashAlgo)CreateAlgo.Invoke(null, null))
            {
                var byteHash = hashAlgo.ComputeHash(Encoding.UTF8.GetBytes(value));

                return BytesToString(byteHash);
            }
        }

        private static string BytesToString(byte[] bytes)
        {
            return string.Concat(bytes.Select(b => b.ToString("X2")).ToArray());
        }
    }
}
