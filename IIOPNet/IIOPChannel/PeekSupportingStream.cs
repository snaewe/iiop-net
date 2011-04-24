/* PeekSupportingStream.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 21.01.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
 * 
 * Copyright 2003 Dominic Ullmann
 *
 * Copyright 2003 ELCA Informatique SA
 * Av. de la Harpe 22-24, 1000 Lausanne 13, Switzerland
 * www.elca.ch
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System;
using System.IO;

namespace Ch.Elca.Iiop.Util {

    /// <summary>
    /// This stream supports peeking, without relaying on the underlying streams seek functionality
    /// </summary>
    /// <remarks>
    /// This stream doesn't support other operations as Read / ReadByte while peeking.
    /// Without peeking, all operations, the underlying stream supports, are supported too.
    /// </remarks>
    public class PeekSupportingStream : Stream {

        #region IFields

        private Stream m_stream;

        private bool m_isPeeking = false;
        private MemoryStream m_peekBuffer = null;
        /// <summary>stores the position in the peek buffer distributed last to a reader, when reading without peek</summary>
        private long m_upToPosInPeekBufferRead = 0;

        #endregion IFields
        #region IConstructors
 
        public PeekSupportingStream(Stream stream) {
            m_stream = stream;
        }

        #endregion IConstructors
        #region IProperties

        public override bool CanRead {
            get {
                return true;
            }
        }

        /// <summary>can seek, if underlying stream can seek, but only if not peeking at the moment</summary>
        public override bool CanSeek {
            get {
                return m_stream.CanSeek;
            }
        }

        public override bool CanWrite {
            get {
                return m_stream.CanWrite;
            }
        }

        public override long Length {
            get {
                return m_stream.Length;
            }
        }

        public override long Position {
            get {
                throw new NotSupportedException();
            }
            set {
                throw new NotSupportedException();
            }
        }

        #endregion IProperties
        #region IMethods

        public override void Close() {
            m_stream.Close();
        }

        /// <returns>
        /// count,
        /// if possible to read the requested nr of bytes
        /// (end of stream not reached in the middle),
        /// otherwise
        /// throws an IOException
        /// </returns>
        public override int Read(byte[] buffer, int offset, int count) {
            lock(this) {
 
                if (offset + count > buffer.Length) {
                    throw new ArgumentException("buffer is not large enough");
                }
 
                if (m_isPeeking) {
                    // not efficient, but ok for small number of reads in peeking mode
                    for (int i = 0; i < count; i++) {
                        int result = ReadByte(); // throws exception if not available
                        buffer[offset+i] = (byte)result;
                    }
                    return count;
                } else {
                    // efficient reading
                    int readTotal = 0;
                    int currentOffset = offset;
                    while (readTotal < count) {
                        if (m_peekBuffer != null) {
                            // read form peek-buffer
                            buffer[currentOffset] = (byte)InternalReadByte(); // throws Exception, if not available
                            readTotal++;
                            currentOffset++;
                        } else {
                            IoUtil.ReadExactly(m_stream, buffer, currentOffset,
                                               count - readTotal); // throws Exception, if not available
                            break; // read completed
                        }
                    }
                    // always read count bytes (or throws exception if not possible)
                    return count;
                }
            } // end lock
        }

        public override int ReadByte() {
            int result = 0;
            lock(this)
                result = m_isPeeking ? PeekByte() : InternalReadByte();

            if (result < 0)
                throw new IOException("underlying stream has not enough data");
 
            return result;
        }

        private int InternalReadByte() {
            // if peek-buffer isn't empty, read from it
            if (m_peekBuffer != null) {
                int result =  m_peekBuffer.ReadByte();
                m_upToPosInPeekBufferRead++;
                if (m_peekBuffer.Position == m_peekBuffer.Length) {
                    m_peekBuffer = null; // peek buffer now read, next operation uses stream
                }
                return result;
            } else {
                // no data in peek buffer, read from stream
                return m_stream.ReadByte();
            }
        }
 
        private int PeekByte() {
            if (m_peekBuffer.Position == m_peekBuffer.Length) {
                int result = m_stream.ReadByte();
                // store it in the peekBuffer
                m_peekBuffer.WriteByte((byte)result);
                return result;
            } else {
                // this is needed, because startPeeking can be called before all the peeked data of a previous
                // peek is read out of the peek buffer.
                int result = m_peekBuffer.ReadByte();
                return result;
            }
        }

        /// <summary>
        /// switch to peeking mode. When ending the peeking mode, reading continues at
        /// the position before starting peeking
        /// </summary>
        public void StartPeeking() {
            lock(this) {
                if (m_isPeeking) { return; }
                m_isPeeking = true;
                if (m_peekBuffer == null) {
                    m_upToPosInPeekBufferRead = 0; // nothing in peek-buffer distributed
                    m_peekBuffer = new MemoryStream();
                }
            }
        }

        /// <summary>stops peeking, next read-operation advanced position in the stream.</summary>
        public void EndPeeking() {
            lock(this) {
                if (!m_isPeeking) {
                    throw new InvalidOperationException("not in peeking mode");
                }
                m_isPeeking = false;
                m_peekBuffer.Seek(m_upToPosInPeekBufferRead, SeekOrigin.Begin); // reset the buffer to the current position for reading
                if (m_peekBuffer.Length == 0) {
                    m_peekBuffer = null;
                }
            }
        }

        public bool IsPeeking() {
            lock (this) {
                return (m_isPeeking);
            }
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin) {
            lock(this) {
                if (m_isPeeking) {
                    throw new NotSupportedException("seeking not supported, while peeking");
                } else {
                    long result = m_stream.Seek(offset, origin);
                    m_peekBuffer.Close();
                    m_peekBuffer = null; // peek-buffer useless after seek
                    return result;
                }
            }
        }

        public override void SetLength(long value) {
            m_stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count) {
            lock(this) {
                if (!(m_isPeeking)) {
                    m_stream.Write(buffer, offset, count);
                } else {
                    throw new NotSupportedException("writeing is not supported, while peeking");
                }
            }
        }

        public override void WriteByte(byte value) {
            lock(this) {
                if (!(m_isPeeking)) {
                    m_stream.WriteByte(value);
                } else {
                    throw new NotSupportedException("writeing is not supported, while peeking");
                }
            }
        }

        public override void Flush() {
            lock(this) {
                if (!(m_isPeeking)) {
                    m_stream.Flush();
                } else {
                    throw new NotSupportedException("flushing is not supported, while peeking");
                }
            }
        }

        #endregion IMethods

    }
}

#if UnitTest

namespace Ch.Elca.Iiop.Tests {

    using NUnit.Framework;
    using Ch.Elca.Iiop.Util;

    /// <summary>
    /// Unit test for PeekSupportingStrem
    /// </summary>
    [TestFixture]
    public class TestPeekSupport {

        public TestPeekSupport() {
        }

        [Test]
        public void TestPeekSupportReadByteNoPeek() {
            MemoryStream stream = new MemoryStream();
            for (int i = 0; i < 10; i++) {
                stream.WriteByte((byte)i);
            }
            stream.Seek(0, SeekOrigin.Begin);

            PeekSupportingStream peekSup = new PeekSupportingStream(stream);
            for (int i = 0; i < 10; i++) {
                Assert.AreEqual(i, peekSup.ReadByte());
            }
        }

        [Test]
        public void TestPeekSupportReadBytePeek() {
            MemoryStream stream = new MemoryStream();
            for (int i = 0; i < 10; i++) {
                stream.WriteByte((byte)i);
            }
            stream.Seek(0, SeekOrigin.Begin);

            PeekSupportingStream peekSup = new PeekSupportingStream(stream);
            peekSup.StartPeeking();
            for (int i = 0; i < 7; i++) {
                Assert.AreEqual(i, peekSup.ReadByte());
            }
            peekSup.EndPeeking();

            // read the whole content after end of peeking
            for (int i = 0; i < 10; i++) {
                Assert.AreEqual(i, peekSup.ReadByte());
            }
        }

        [Test]
        public void TestPeekSupportMulitplePeek() {
            MemoryStream stream = new MemoryStream();
            for (int i = 0; i < 10; i++) {
                stream.WriteByte((byte)i);
            }
            stream.Seek(0, SeekOrigin.Begin);

            PeekSupportingStream peekSup = new PeekSupportingStream(stream);
            peekSup.StartPeeking();
            for (int i = 0; i < 7; i++) {
                Assert.AreEqual(i, peekSup.ReadByte());
            }
            peekSup.EndPeeking();

            // now read something, then peek anew
            // read the whole content after end of peeking
            for (int i = 0; i < 3; i++) {
                Assert.AreEqual(i, peekSup.ReadByte());
            }

            peekSup.StartPeeking();
            for (int i = 3; i < 9; i++) {
                Assert.AreEqual(i, peekSup.ReadByte());
            }
            peekSup.EndPeeking();

            // now read the rest of the stream
            for (int i = 3; i < 10; i++) {
                Assert.AreEqual(i, peekSup.ReadByte());
            }
        }

        [Test]
        public void TestPeekSupportReadArrayNoPeek() {
            MemoryStream stream = new MemoryStream();
            for (int i = 0; i < 10; i++) {
                stream.WriteByte((byte)i);
            }
            stream.Seek(0, SeekOrigin.Begin);

            PeekSupportingStream peekSup = new PeekSupportingStream(stream);
            byte[] result = new byte[10];
            peekSup.Read(result, 0, 10);

            for (int i = 0; i < 10; i++) {
                Assert.AreEqual(i, result[i]);
            }
        }
 
        [Test]
        public void TestEmptyPeek() {
            MemoryStream stream = new MemoryStream();
            for (int i = 0; i < 10; i++) {
                stream.WriteByte((byte)i);
            }
            stream.Seek(0, SeekOrigin.Begin);
            PeekSupportingStream peekSup = new PeekSupportingStream(stream);
            peekSup.StartPeeking();
            peekSup.EndPeeking();
            int res = peekSup.ReadByte();
            Assert.AreEqual(0, res);
        }
    }

}

#endif
