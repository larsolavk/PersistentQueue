using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PersistentQueue;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {

            long index = 11;
            long indexItemsPerPage = 10;

            var indexPageIndex = index / (indexItemsPerPage+1);


            //var queue = new PersistentQueue.PersistentQueue();
            //var s = "Dette er en test";

            //using (var stream = GetStream(s))
            //{
            //    queue.Enqueue(stream);
            //}
        }


        static Stream GetStream(string s)
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            sw.Write(s);
            sw.Flush();
            ms.Position = 0;
            return ms;
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
    }
}
