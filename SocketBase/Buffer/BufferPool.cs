using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;

namespace SuperSocket.SocketBase.Buffer
{
    class BufferPool : IBufferPool
    {
        private ConcurrentStack<byte[]> m_Store;

        private ConcurrentDictionary<int, byte> m_BufferDict;

        public int BufferSize { get; private set; }

        private int m_TotalCount;

        public int TotalCount
        {
            get { return m_TotalCount; }
        }

        private int m_AvailableCount;

        public int AvailableCount
        {
            get { return m_AvailableCount; }
        }

        public BufferPool(int bufferSize, int initialCount)
        {
            byte[][] list = new byte[initialCount][];

            for(var i = 0; i < initialCount; i++)
            {
                list[i] = new byte[bufferSize];
            }

            m_Store = new ConcurrentStack<byte[]>(list);
            m_BufferDict = new ConcurrentDictionary<int, byte>(list.Select(l => new KeyValuePair<int, byte>(l.GetHashCode(), 1)));

            BufferSize = bufferSize;
            m_TotalCount = initialCount;
            m_AvailableCount = initialCount;
        }

        public byte[] GetBuffer()
        {
            byte[] buffer;

            if (!m_Store.TryPop(out buffer))
            {
                Expand();
                return GetBuffer();
            }

            Interlocked.Decrement(ref m_AvailableCount);
            return buffer;
        }

        void Expand()
        {

        }

        void Shrink()
        {

        }

        public void ReturnBuffer(byte[] buffer)
        {
            m_Store.Push(buffer);
            Interlocked.Increment(ref m_AvailableCount);
        }
    }
}
