using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Protocol
{
    public enum ResolveState : byte
    {
        Found,
        Pending,
        Error
    }
}
