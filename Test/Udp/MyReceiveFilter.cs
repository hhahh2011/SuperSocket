using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Test.Udp
{
    class MyReceiveFilter : IReceiveFilter<MyUdpRequestInfo>
    {
        public int LeftBufferSize
        {
            get { return 0; }
        }

        public IReceiveFilter<MyUdpRequestInfo> NextReceiveFilter
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the filter state.
        /// </summary>
        /// <value>
        /// The filter state.
        /// </value>
        public FilterState State { get; private set; }

        public void Reset()
        {
            if (State != FilterState.Normal)
                State = FilterState.Normal;
        }

        public MyUdpRequestInfo Filter(ReceivedData data, out int rest)
        {
            rest = 0;

            var segment = data.Current;

            if (segment.Count <= 40)
                return null;

            var key = Encoding.ASCII.GetString(segment.Array, segment.Offset, 4);
            var sessionID = Encoding.ASCII.GetString(segment.Array, segment.Offset + 4, 36);

            return new MyUdpRequestInfo(key, sessionID) { Value = Encoding.UTF8.GetString(segment.Array, segment.Offset + 40, segment.Count - 40) };
        }
    }
}
