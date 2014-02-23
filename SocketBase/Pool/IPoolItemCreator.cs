using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Pool
{
    interface IPoolItemCreator<T>
    {
        IEnumerable<T> Create(int count);
    }
}
