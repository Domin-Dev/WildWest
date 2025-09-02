using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;


public static class AuthUtils
{
    public static FixedString128Bytes ComputeSha256(char[] input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            FixedString128Bytes hash = new FixedString128Bytes();
            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            System.Array.Clear(inputBytes, 0, inputBytes.Length);
            System.Array.Clear(input, 0, input.Length);
            System.Array.Clear(bytes, 0, bytes.Length);
            return hash;
        }
    }
    public static FixedString128Bytes GetSalt()
    {
        byte[] salt = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
        string saltHex = BitConverter.ToString(salt).Replace("-", "").ToLower();
        FixedString128Bytes saltFixed = new FixedString128Bytes();
        saltFixed.Append(saltHex);
        return saltFixed;
    }

    public static FixedString128Bytes GetSaltHash(FixedString128Bytes hash, FixedString128Bytes salt)
    {
        return AuthUtils.ComputeSha256((hash.ToString() + salt.ToString()).ToArray());
    }
}
