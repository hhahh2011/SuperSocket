using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SuperSocket.Protocol
{
    public sealed class BufferSegmentStream : Stream
    {
        private IList<ArraySegment<byte>> m_Segments;

        private long m_Position;

        private long m_Length;

        public BufferSegmentStream(IList<ArraySegment<byte>> segments)
        {
            m_Segments = segments;

            long length = 0;

            for (var i = 0; i < segments.Count; i++)
            {
                length += segments[i].Count;
            }

            m_Length = length;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Length
        {
            get { return m_Length; }
        }

        public override long Position
        {
            get
            {
                return m_Position;
            }
            set
            {
                m_Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
