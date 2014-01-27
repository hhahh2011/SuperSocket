using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Buffer
{
    interface IBufferPool
    {
        int BufferSize { get; }

        int TotalCount { get; }

        int AvailableCount { get; }

        byte[] GetBuffer();

        void ReturnBuffer(byte[] buffer);

        void Shrink();
    }
}
