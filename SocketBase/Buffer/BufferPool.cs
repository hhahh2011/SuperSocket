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

        private byte m_CurrentGeneration = 0;

        private ConcurrentDictionary<int, byte> m_BufferDict;

        private ConcurrentDictionary<int, byte> m_RemovedBufferDict;

        private int m_NextExpandThreshold;

        public int BufferSize { get; private set; }

        private int m_TotalCount;

        public int TotalCount
        {
            get { return m_TotalCount; }
        }

        public int AvailableCount
        {
            get { return m_Store.Count; }
        }

        private int m_InExpanding = 0;

        public BufferPool(int bufferSize, int initialCount)
        {
            byte[][] list = new byte[initialCount][];

            for(var i = 0; i < initialCount; i++)
            {
                list[i] = new byte[bufferSize];
            }

            m_Store = new ConcurrentStack<byte[]>(list);
            m_BufferDict = new ConcurrentDictionary<int, byte>(list.Select(l => new KeyValuePair<int, byte>(l.GetHashCode(), m_CurrentGeneration)));

            BufferSize = bufferSize;
            m_TotalCount = initialCount;
            UpdateNextExpandThreshold();
        }

        private void UpdateNextExpandThreshold()
        {
            m_NextExpandThreshold = m_TotalCount / 5; //if only 20% buffer left, we can expand the buffer count
        }

        public byte[] GetBuffer()
        {
            byte[] buffer;

            if (m_Store.TryPop(out buffer))
            {
                if (m_Store.Count <= m_NextExpandThreshold && m_InExpanding == 0)
                    ThreadPool.QueueUserWorkItem(w => TryExpand());
                 
                return buffer;
            }

            //In expanding
            if (m_InExpanding == 1)
            {
                var spinWait = new SpinWait();

                for (var i = 0; i < 100; i++)
                {
                    spinWait.SpinOnce();

                    if (m_Store.TryPop(out buffer))
                    {
                        return buffer;
                    }
                }

                Console.WriteLine("Failed to GetBuffer");
                throw new Exception("Failed to GetBuffer");
            }
            else
            {
                TryExpand();
                return GetBuffer();
            }
        }

        bool TryExpand()
        {
            if (Interlocked.CompareExchange(ref m_InExpanding, 1, 0) != 0)
                return false;

            Expand();
            return true;
        }

        void Expand()
        {
            var totalCount = m_TotalCount;

            //double the size
            byte[][] list = new byte[totalCount][];

            for (var i = 0; i < totalCount; i++)
            {
                var buffer = new byte[BufferSize];
                m_Store.Push(buffer);
                m_BufferDict.TryAdd(buffer.GetHashCode(), m_CurrentGeneration);
            }

            m_CurrentGeneration++;

            m_TotalCount += totalCount;
            Console.WriteLine("Expanding: {0}", m_TotalCount);
            UpdateNextExpandThreshold();
            m_InExpanding = 0;
        }

        public void Shrink()
        {
            var generation = m_CurrentGeneration;
            if (generation == 0)
                return;

            var shrinThreshold = m_TotalCount * 3 / 4;

            if (m_Store.Count <= shrinThreshold)
                return;

            m_CurrentGeneration = (byte)(generation - 1);

            var toBeRemoved = new List<int>(m_TotalCount / 2);

            foreach (var item in m_BufferDict)
            {
                if (item.Value == generation)
                {
                    toBeRemoved.Add(item.Key);
                }
            }

            if (m_RemovedBufferDict == null)
                m_RemovedBufferDict = new ConcurrentDictionary<int, byte>();

            foreach (var item in toBeRemoved)
            {
                byte value;
                m_BufferDict.TryRemove(item, out value);
                m_RemovedBufferDict.TryAdd(item, 0);
            }
        }

        public void ReturnBuffer(byte[] buffer)
        {
            var key = buffer.GetHashCode();

            if (m_BufferDict.ContainsKey(key))
            {
                m_Store.Push(buffer);
                return;
            }

            if (m_RemovedBufferDict == null)
                return;

            byte value;

            if (m_RemovedBufferDict.TryRemove(key, out value))
            {
                Interlocked.Decrement(ref m_TotalCount);
            }
        }
    }
}
