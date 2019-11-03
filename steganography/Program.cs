using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace Steganography
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Perdorimi:");
                Console.WriteLine("stegano <path> read");
                Console.WriteLine("stegano <path> write <text>");
                return;
            }

            string path = args[0];
            string command = args[1];
            Bitmap img = OpenBitmap(path);
            switch (command)
            {
                case "read":
                    string message = ReadText(img);
                    if (message == null)
                    {
                        Console.WriteLine("Nuk ka text.");
                    }
                    else
                    {
                        Console.WriteLine("Messazhi i dekoduar: " + message);
                    }
                    return;
                case "write":
                    if (args.Length < 3)
                    {
                        Console.WriteLine("Jepni tekstin.");
                    }

                    string text = args[2];
                    var imgStegano = WriteText(img, text);
                    string originalDir = Path.GetDirectoryName(path);
                    string originalName = Path.GetFileNameWithoutExtension(path);
                    string saveLocation = Path.Combine(originalDir, originalName + ".stegano.png");
                    imgStegano.Save(saveLocation, ImageFormat.Png);
                    return;
                default:
                    Console.WriteLine("Komande e panjohur.");
                    return;
            }
        }

        static Bitmap WriteText(Bitmap bitmap, string teksti)
        {
            Bitmap clone = (Bitmap)bitmap.Clone();
            byte[] textBytes = Encoding.UTF8.GetBytes(teksti);
            byte[] data = Merge(
                Encoding.UTF8.GetBytes("MSG"),
                BitConverter.GetBytes(textBytes.Length),
                textBytes
            );

            WriteBytes(clone, data, 0);
            return clone;
        }

        static void WriteBytes(Bitmap bitmap, byte[] bytes, int offset)
        {
            int length = bytes.Length;
            int width = bitmap.Width;
            int offsetBits = offset * 8;
            int lengthBits = length * 8;
            int index = 0;
            for (int i = offsetBits; i < offsetBits + lengthBits; i++)
            {
                int pixel = i / 3;
                int channel = i % 3;
                int x = pixel % width;
                int y = pixel / width;
                int color = bitmap.GetPixel(x, y).ToArgb();
                bool bit = GetBit(bytes, index);
                color = SetBit(color, channel * 8, bit);
                bitmap.SetPixel(x, y, Color.FromArgb(color));
                index++;
            }
        }

        static string ReadText(Bitmap bitmap)
        {
            byte[] secretCode = ReadBytes(bitmap, 0, 3);
            if (Encoding.UTF8.GetString(secretCode) != "MSG")
            {
                return null;
            }

            byte[] lengthBytes = ReadBytes(bitmap, 3, 4);
            int length = BitConverter.ToInt32(lengthBytes, 0);
            byte[] messageBytes = ReadBytes(bitmap, 7, length);
            string message = Encoding.UTF8.GetString(messageBytes);
            return message;
        }

        static byte[] ReadBytes(Bitmap bitmap, int offset, int length)
        {
            byte[] result = new byte[length];
            int width = bitmap.Width;
            int offsetBits = offset * 8;
            int lengthBits = length * 8;
            int index = 0;
            for (int i = offsetBits; i < offsetBits + lengthBits; i++)
            {
                int pixel = i / 3;
                int channel = i % 3;
                int x = pixel % width;
                int y = pixel / width;
                int color = bitmap.GetPixel(x, y).ToArgb();
                bool bit = GetBit(color, channel * 8);
                SetBit(result, index, bit);
                index++;
            }

            return result;
        }

        static byte[] Merge(params byte[][] arrays)
        {
            return arrays.SelectMany(x => x).ToArray();
        }

        static bool GetBit(byte[] array, int index)
        {
            int byteIndex = index / 8;
            int bitIndex = index % 8;
            return GetBit(array[byteIndex], bitIndex);
        }

        static void SetBit(byte[] array, int index, bool bit)
        {
            int byteIndex = index / 8;
            int bitIndex = index % 8;
            int newValue = SetBit(array[byteIndex], bitIndex, bit);
            array[byteIndex] = (byte)(newValue & 0xFF);
        }

        static bool GetBit(int val, int index)
        {
            return (val & (1 << index)) != 0;
        }

        static int SetBit(int val, int index, bool bit)
        {
            if (bit)
            {
                return val | (1 << index);
            }
            else
            {
                return val & ~(1 << index);
            }
        }

        static Bitmap OpenBitmap(string path)
        {
            return new Bitmap(path);
        }
    }
}
