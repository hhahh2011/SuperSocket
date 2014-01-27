using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Buffer
{
    public class BufferManager : IBufferManager
    {
        private const int m_Magic = 4096;//4k

        private IBufferPool[] m_Pools;

        private IBufferPool m_LastHitPool;

        public BufferManager()
        {
            //4096 = 4k, 8192 = 8k, 16384 = 16k
            m_Pools = new IBufferPool[]
            {
                new BufferPool(4096, 100),
                new BufferPool(8192, 50),
                new BufferPool(16384, 10)
            };
        }

        public byte[] GetBuffer(int size)
        {
            var lastHitPool = m_LastHitPool;

            if (lastHitPool != null && lastHitPool.BufferSize >= size)
                return lastHitPool.GetBuffer();

            for(var i = 0; i < m_Pools.Length; i++)
            {
                var pool = m_Pools[i];

                if(pool.BufferSize >= size)
                {
                    m_LastHitPool = pool;
                    return pool.GetBuffer();
                }
            }

            //The size is large than any pool's buffer size, so create the buffer directly now
            return new byte[size];
        }

        public void ReturnBuffer(byte[] buffer)
        {
            var size = buffer.Length;

            var lastHitPool = m_LastHitPool;

            if (lastHitPool != null && lastHitPool.BufferSize == size)
            {
                lastHitPool.ReturnBuffer(buffer);
                return;
            }

            for(var i = 0; i < m_Pools.Length; i++)
            {
                var pool = m_Pools[i];

                if(pool.BufferSize == size)
                {
                    m_LastHitPool = pool;
                    pool.ReturnBuffer(buffer);
                    return;
                }
            }
        }

        public void Shrink()
        {
            for (var i = 0; i < m_Pools.Length; i++)
            {
                m_Pools[i].Shrink();
            }
        }
    }
}
