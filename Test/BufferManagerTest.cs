using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SuperSocket.SocketBase.Pool;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.Test
{
    [TestFixture]
    public class BufferManagerTest
    {
        [Test]
        public void CompareWithNoBuffer()
        {
            Stopwatch watch = new Stopwatch();

            var testRount = 10000;
            var bufferSize = 1024 * 4;

            var bufferList = new byte[testRount][];

            watch.Start();

            for (var i = 0; i < testRount; i++)
            {
                bufferList[i] = new byte[bufferSize];
            }

            watch.Stop();

            Console.WriteLine("No buffer: {0}", watch.ElapsedMilliseconds);

            watch.Reset();

            GC.Collect();
            GC.WaitForFullGCComplete();

            var bufferManager = new BufferManager(new BufferPoolConfig[] { new BufferPoolConfig(bufferSize, 1000) });

            watch.Start();

            for (var i = 0; i < testRount; i++)
            {
                bufferList[i] = bufferManager.GetBuffer(bufferSize);
            }

            watch.Stop();
            Console.WriteLine("BufferManager: {0}", watch.ElapsedMilliseconds);
        }

        [Test]
        public void CompareWithNoBufferConcurrent()
        {
            Stopwatch watch = new Stopwatch();

            var testRount = 10000;
            var bufferSize = 1024 * 4;

            var bufferList = new byte[testRount][];

            watch.Start();

            Parallel.For(0, testRount, (i) =>
                {
                    bufferList[i] = new byte[bufferSize];
                });

            watch.Stop();

            Console.WriteLine("No buffer: {0}", watch.ElapsedMilliseconds);

            watch.Reset();

            GC.Collect();
            GC.WaitForFullGCComplete();

            var bufferManager = new BufferManager(new BufferPoolConfig[] { new BufferPoolConfig(bufferSize, 20000) });

            watch.Start();

            Parallel.For(0, testRount, (i) =>
            {
                bufferList[i] = bufferManager.GetBuffer(bufferSize);
            });

            watch.Stop();
            Console.WriteLine("BufferManager: {0}", watch.ElapsedMilliseconds);
        }
    }
}
