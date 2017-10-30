namespace System.Security.Cryptography
{
    using System.Text;

    public static class _MD5
    {
        public static string MD5(this string[] data, bool sorted, System.Text.Encoding encoding = null)
        {
            StringBuilder buidler = new StringBuilder();
            if (data != null)
            {
                if (sorted)
                {
                    Array.Sort(data);
                }
                foreach (String str in data)
                {
                    buidler.Append(str);
                }
            }
            return MD5(buidler.ToString(), encoding);
        }

        public static string MD5(this string data, System.Text.Encoding encoding = null)
        {
            StringBuilder buidler = new StringBuilder();
            if (data == null)
            {
                data = "";
            }
            if (encoding == null)
            {
                encoding = System.Text.Encoding.UTF8;
            }
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            try
            {
                byte[] hashBytes = md5.ComputeHash(encoding.GetBytes(data));
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    buidler.Append(hashBytes[i].ToString("X2"));
                }
                return buidler.ToString().ToLower();
            } finally
            {
                md5.Dispose();
            }
        }
    }
}