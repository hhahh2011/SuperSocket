using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Protocol
{
    public interface IReceivedDataResolver
    {
        ResolveState Process(ArraySegment<byte> rawData);

        string ErrorMessage { get; }
    }
}
