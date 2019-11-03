using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace aes
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Jepnit te gjitha argumentet");
                return;
            }

            var command = args[0];
            var key = BytesOf(args.Length > 1 ? args[1] : "1234123412341234");
            var data = args.Length > 2 ? args[2] : Console.In.ReadToEnd();

            switch (command)
            {
                case "encrypt":
                    Console.WriteLine(Encrypt(data, key));
                    return;
                case "encrypt-file":
                    Encrypt(
                        File.OpenRead(data),
                        File.OpenWrite("C:/Users/Edon/Desktop/fajlli.enc"),
                        key);
                    return;
                case "decrypt":
                    Console.WriteLine(Decrypt(data, key));
                    return;
                case "decrypt-file":
                    Decrypt(
                        File.OpenRead(data),
                        Console.OpenStandardOutput(),
                        key);
                    return;
                default:
                    Console.WriteLine("Komande jo valide");
                    return;
            }
        }

        static byte[] BytesOf(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        static string Encrypt(string plaintext, byte[] key)
        {
            var cipherBytes = Encrypt(
                Encoding.UTF8.GetBytes(plaintext),
                key);
            return Convert.ToBase64String(cipherBytes);
        }

        static byte[] Encrypt(byte[] plaintext, byte[] key)
        {
            var input = new MemoryStream(plaintext)
            {
                Position = 0
            };
            var output = new MemoryStream();
            Encrypt(input, output, key);
            return output.ToArray();
        }

        static void Encrypt(Stream input, Stream output, byte[] key)
        {
            var aes = new AesManaged
            {
                BlockSize = 128,
                Key = key,
                Mode = CipherMode.ECB
            };

            using (var encryptor = aes.CreateEncryptor())
            using (var cs = new CryptoStream(
                output,
                encryptor,
                CryptoStreamMode.Write))
            {
                input.CopyTo(cs);
            }
        }

        static string Decrypt(string ciphertext, byte[] key)
        {
            var cipherBytes = Decrypt(
                Convert.FromBase64String(ciphertext),
                key);
            return Encoding.UTF8.GetString(cipherBytes);
        }

        static byte[] Decrypt(byte[] ciphertext, byte[] key)
        {
            var input = new MemoryStream(ciphertext)
            {
                Position = 0
            };
            var output = new MemoryStream();
            Decrypt(input, output, key);
            return output.ToArray();
        }

        static void Decrypt(Stream input, Stream output, byte[] key)
        {
            var aes = new AesManaged
            {
                BlockSize = 128,
                Key = key,
                Mode = CipherMode.ECB
            };

            using (var decryptor = aes.CreateDecryptor())
            using (var cs = new CryptoStream(
                input,
                decryptor,
                CryptoStreamMode.Read))
            {
                cs.CopyTo(output);
            }
        }

    }
}
