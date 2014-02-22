using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using System.Runtime.InteropServices;

namespace SuperSocket.SocketBase.Buffer
{
    class BufferPool : IBufferPool
    {
        private ConcurrentStack<byte[]> m_Store;

        private byte m_CurrentGeneration = 0;

        private ConcurrentDictionary<long, byte> m_BufferDict;

        private ConcurrentDictionary<long, byte> m_RemovedBufferDict;

        private int m_NextExpandThreshold;

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

        private int m_InExpanding = 0;

        public BufferPool(int bufferSize, int initialCount)
        {
            byte[][] list = new byte[initialCount][];

            m_BufferDict = new ConcurrentDictionary<long, byte>();

            for(var i = 0; i < initialCount; i++)
            {
                var buffer = new byte[bufferSize];
                GCHandle.Alloc(buffer, GCHandleType.Pinned); //Pinned the buffer in the memory
                list[i] = buffer;
                m_BufferDict.TryAdd(GetBytesAddress(buffer), m_CurrentGeneration);
            }

            m_Store = new ConcurrentStack<byte[]>(list);

            BufferSize = bufferSize;
            m_TotalCount = initialCount;
            m_AvailableCount = m_TotalCount;
            UpdateNextExpandThreshold();
        }

        private void UpdateNextExpandThreshold()
        {
            m_NextExpandThreshold = m_TotalCount / 5; //if only 20% buffer left, we can expand the buffer count
        }

        private long GetBytesAddress(byte[] buffer)
        {
            unsafe
            {
                fixed (byte* bytes = buffer)
                {
                    var ptr = new IntPtr(bytes);
                    return ptr.ToInt64();
                }
            }
        }

        public byte[] GetBuffer()
        {
            byte[] buffer;

            if (m_Store.TryPop(out buffer))
            {
                Interlocked.Decrement(ref m_AvailableCount);

                if (m_AvailableCount <= m_NextExpandThreshold && m_InExpanding == 0)
                    ThreadPool.QueueUserWorkItem(w => TryExpand());
                 
                return buffer;
            }

            //In expanding
            if (m_InExpanding == 1)
            {
                var spinWait = new SpinWait();

                while (true)
                {
                    spinWait.SpinOnce();

                    if (m_Store.TryPop(out buffer))
                    {
                        Interlocked.Decrement(ref m_AvailableCount);
                        return buffer;
                    }

                    if (m_InExpanding != 1)
                        return GetBuffer();
                }
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

            for (var i = 0; i < totalCount; i++)
            {
                var buffer = new byte[BufferSize];
                GCHandle.Alloc(buffer, GCHandleType.Pinned); //Pinned the buffer in the memory
                m_Store.Push(buffer);
                Interlocked.Increment(ref m_AvailableCount);
                m_BufferDict.TryAdd(buffer.GetHashCode(), m_CurrentGeneration);
            }

            m_CurrentGeneration++;

            m_TotalCount += totalCount;
            UpdateNextExpandThreshold();
            m_InExpanding = 0;
        }

        public void Shrink()
        {
            var generation = m_CurrentGeneration;
            if (generation == 0)
                return;

            var shrinThreshold = m_TotalCount * 3 / 4;

            if (m_AvailableCount <= shrinThreshold)
                return;

            m_CurrentGeneration = (byte)(generation - 1);

            var toBeRemoved = new List<long>(m_TotalCount / 2);

            foreach (var item in m_BufferDict)
            {
                if (item.Value == generation)
                {
                    toBeRemoved.Add(item.Key);
                }
            }

            if (m_RemovedBufferDict == null)
                m_RemovedBufferDict = new ConcurrentDictionary<long, byte>();

            foreach (var item in toBeRemoved)
            {
                byte value;
                m_BufferDict.TryRemove(item, out value);
                m_RemovedBufferDict.TryAdd(item, 0);
            }
        }

        public void ReturnBuffer(byte[] buffer)
        {
            var key = GetBytesAddress(buffer);

            if (m_BufferDict.ContainsKey(key))
            {
                m_Store.Push(buffer);
                Interlocked.Increment(ref m_AvailableCount);
                return;
            }

            if (m_RemovedBufferDict == null)
                return;

            byte value;

            if (m_RemovedBufferDict.TryRemove(key, out value))
            {
                Interlocked.Decrement(ref m_TotalCount);
                GCHandle.Alloc(buffer, GCHandleType.Normal); //Change the buffer to Normal from Pinned in the memory
            }
        }
    }
}
