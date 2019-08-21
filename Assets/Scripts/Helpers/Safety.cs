using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;

static class Safety
{
    public static byte[] Hash(this byte[] data)
    {
        using (MD5 engine = MD5.Create())
        {
            byte[] buffer;
            buffer = engine.ComputeHash(data);
            return buffer;
        }
    }

    public static string HashToString(this byte[] data)
    {
        var buffer = Hash(data);
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

