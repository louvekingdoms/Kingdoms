using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Newtonsoft.Json;


static class Safety
{
    public static string Hash(this object data)
    {
        using (MD5 engine = MD5.Create())
        {
            byte[] buffer;
            if (!data.GetType().IsSerializable)
                buffer = engine.ComputeHash(Encoding.UTF8.GetBytes(data.ToString()));
            else
            {
                var obj = JsonConvert.SerializeObject(data);
                buffer = engine.ComputeHash(Encoding.UTF8.GetBytes(obj));
            }

            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < buffer.Length; i++)
            {
                sBuilder.Append(buffer[i].ToString("X2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();

        }
    }
}

