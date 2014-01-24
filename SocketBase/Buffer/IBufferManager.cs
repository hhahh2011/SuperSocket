using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Buffer
{
    public interface IBufferManager
    {
        byte[] GetBuffer(int size);

        void ReturnBuffer(byte[] buffer);
    }
}
