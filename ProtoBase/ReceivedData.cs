using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    public class ReceivedData
    {
        public ArraySegment<byte> Current { get; set; }

        public List<ArraySegment<byte>> PackageData { get; private set; }

        public ReceivedData()
        {
            PackageData = new List<ArraySegment<byte>>();
        }

        public int Total
        {
            get
            {
                var total = 0;

                for (var i = 0; i < PackageData.Count; i++)
                {
                    total += PackageData[i].Count;
                }

                return total;
            }
        }

        public void SetLastLength(int length)
        {
            var lastPos = PackageData.Count - 1;
            var last = PackageData[lastPos];
            PackageData[lastPos] = new ArraySegment<byte>(last.Array, last.Offset, length);
        }
    }
}
