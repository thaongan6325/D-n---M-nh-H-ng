using System.Text;
using System.Security.Cryptography;

namespace Vivu_Xe.Helpers
{
    public class MyUtil
    {
        // Hàm mã hóa MD5
        public static string ToMd5(string str)
        {
            string result = "";
            byte[] buffer = Encoding.UTF8.GetBytes(str);
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            buffer = md5.ComputeHash(buffer);
            for (int i = 0; i < buffer.Length; i++)
            {
                result += buffer[i].ToString("x2");
            }
            return result;
        }
    }
}