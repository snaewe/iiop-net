/* PeekSupportingStream.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 21.01.03  Dominic Ullmann (DUL), dul@elca.ch
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

        public override int Read(byte[] buffer, int offset, int count) {
            lock(this) {
                int read = count;
                if (offset + count > buffer.Length) { 
                    throw new InvalidOperationException("buffer is not large enough"); 
                }
                for (int i = 0; i < count; i++) {
                    int result = ReadByte();
                    if (result < 0) { read = i; break; }
                    buffer[offset+i] = (byte)result;
                }
                return read;
            }
        }

        public override int ReadByte() {
            int result = ReadOrPeekByte();
            if (result < 0) { 
                throw new IOException("underlaying stream not enough data"); 
            }
            return result;
        }

        private int ReadOrPeekByte() {
            lock(this) {    
                if (m_isPeeking) {
                    return PeekByte();
                } else {
                    return InternalReadByte();
                }
            }
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
