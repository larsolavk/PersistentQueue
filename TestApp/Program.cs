using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PersistentQueue;
using System.Diagnostics;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("TADA");

            var q = new PersistentQueue.PersistentQueue(@"c:\temp\PersistentQueue", 10*1024*1024);
            int items = 50000;
            var swOuter = new Stopwatch();
            var swInner = new Stopwatch();
            swOuter.Start();
            for (int i = 0; i < items; i++)
            {
                using (var s = GetStream(String.Format("This is line number {0}. Just adding some more text to grow the item size", i)))
                {
                    swInner.Start();
                    q.Enqueue(s);
                    swInner.Stop();
                }
            }
            swOuter.Stop();

            Console.WriteLine("Enqueued {0} items in {1} ms ({2:0} items/s). Inner: {3} ms ({4:0} items/s)",
                items,
                swOuter.ElapsedMilliseconds,
                ((double)items / swOuter.ElapsedMilliseconds) * 1000,
                swInner.ElapsedMilliseconds,
                ((double)items / swInner.ElapsedMilliseconds) * 1000);



            Stream stream;
            swOuter.Reset();
            swInner.Reset();
            swOuter.Start();
            items = 0;

            // Get n number of items from the queue
            //var n = 2;
            //for (int i = 0; i < n; i++)
            //{
            //    if (null == (stream = q.Dequeue()))
            //        break;

            //    items++;
            //    using (var br = new BinaryReader(stream))
            //    {
            //        var s = new string(br.ReadChars((int)stream.Length));
            //        Console.WriteLine(s);
            //    }
            //    stream.Dispose();
            //}


            // Get all items from queue
            do
            {
                swInner.Start();
                using (stream = q.Dequeue())
                {
                    swInner.Stop();
                    if (stream != null)
                    {
                        items++;

                        using (var br = new BinaryReader(stream))
                        {
                            var s = new string(br.ReadChars((int)stream.Length));
                            //Console.WriteLine(s);
                        }
                    }
                }
            }
            while (stream != null);
                
            Console.WriteLine("Dequeued {0} items in {1} ms ({2:0} items/s). Inner: {3} ms ({4:0} items/s)",
                items,
                swOuter.ElapsedMilliseconds,
                ((double)items / swOuter.ElapsedMilliseconds) * 1000,
                swInner.ElapsedMilliseconds,
                ((double)items / swInner.ElapsedMilliseconds) * 1000);

            //Console.ReadLine();
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
