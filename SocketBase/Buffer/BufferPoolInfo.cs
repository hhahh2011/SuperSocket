using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Buffer
{
    public class BufferPoolInfo
    {
        public int BufferSize { get; set; }

        public int InitialCount { get; set; }

        public BufferPoolInfo(int bufferSize, int initialCount)
        {
            BufferSize = bufferSize;
            InitialCount = initialCount;
        }
    }
}
