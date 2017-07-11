namespace SignatureApp
{
    using System.Text;

    public static class Extension
    {
        public static string ConvertToHexString(this byte[] bytes)
        {
            var result = new StringBuilder(bytes.Length * 2);

            for (int i = 0; i < bytes.Length; i++)
            {
                result.Append(bytes[i].ToString("x2"));
            }

            return result.ToString();
        }
    }
}
