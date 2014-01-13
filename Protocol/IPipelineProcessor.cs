using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Protocol
{
    public interface IPipelineProcessor
    {
        ProcessState Process(ArraySegment<byte> rawData);
    }
}
