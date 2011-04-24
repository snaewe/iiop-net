/* IOUtil.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 17.01.04  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
    /// helps in Reading/Writing of streams
    /// </summary>
    internal sealed class IoUtil {
 
        /// <summary>
        /// specifies, how much should be read in one step
        /// </summary>
        private const int READ_CHUNK_SIZE = 8192;
 
        #if UnitTest
 
        internal static int BufferLength {
            get {
                return READ_CHUNK_SIZE;
            }
        }
 
        #endif
 
 
 
        /// <summary>copies nrOfBytes from source to target,
        /// if possible, else throw IOException</summary>
        public static void StreamCopyExactly(Stream source, Stream target,
                                       int nrOfBytesToCopy) {
            // for efficiency, almost a copy of code from ReadExaclty
 
            byte[] readBuffer = new byte[READ_CHUNK_SIZE];
 
            int bytesRead = 0;
            while (bytesRead < nrOfBytesToCopy) {
                // need more data
                int toRead = Math.Min(readBuffer.Length,
                                      nrOfBytesToCopy - bytesRead);
 
                // read either the whole buffer length or
                // the remaining nr of bytes: nrOfBytesToRead - bytesRead
                int readCurrent = source.Read(readBuffer, 0, toRead);
                if (readCurrent <= 0) {
                    throw new IOException("underlying stream not enough data");
                }
 
                target.Write(readBuffer, 0, readCurrent);
                bytesRead += readCurrent;
            }
 
        }

        /// <summary>Reads the specified nrOfBytes from source,
        /// if possible, else throw IOException</summary>
        public static void ReadExactly(Stream source, byte[] target,
                                       int targetOffset, int nrOfBytesToRead) {
            // for efficiency, almost a copy of code from StreamCopyExactly
 
            if (targetOffset + nrOfBytesToRead > target.Length) {
                throw new ArgumentException("target array to small");
            }
 
            int bytesRead = 0;
            while (bytesRead < nrOfBytesToRead) {
                // need more data
                int toRead = Math.Min(READ_CHUNK_SIZE,
                                      nrOfBytesToRead - bytesRead);

                // read either the whole buffer length or
                // the remaining nr of bytes: nrOfBytesToRead - bytesRead
                int readCurrent = source.Read(target, targetOffset + bytesRead,
                                              toRead);
                if (readCurrent <= 0) {
                    throw new IOException("underlying stream not enough data");
                }
                bytesRead += readCurrent;
            }
 
        }
 
 
    }


}


#if UnitTest

namespace Ch.Elca.Iiop.Tests {

    using System.Reflection;
    using System.Collections;
    using System.IO;
    using NUnit.Framework;
    using Ch.Elca.Iiop;
    using Ch.Elca.Iiop.Util;


    /// <summary>
    /// Unit-tests for testing IoUtil class
    /// </summary>
    [TestFixture]
    public class IoUtilTest {
 

        private void CheckStreamCopy(int nrOfBytes) {
            Stream source = new MemoryStream();
            Stream target = new MemoryStream();
 
            for (int i = 0; i < nrOfBytes; i++) {
                source.WriteByte((byte)(i % 255));
            }
 
            source.Seek(0, SeekOrigin.Begin);
            IoUtil.StreamCopyExactly(source, target, nrOfBytes);
 
            // check target
            target.Seek(0, SeekOrigin.Begin);
            source.Seek(0, SeekOrigin.Begin);
            for (int i = 0; i < nrOfBytes; i++) {
                int expected = source.ReadByte();
                Assert.AreEqual(expected, target.ReadByte());
            }
        }
 
        private void CheckStreamReadToArray(int nrOfBytes) {
            Stream source = new MemoryStream();
            byte[] target = new byte[nrOfBytes];
 
            for (int i = 0; i < nrOfBytes; i++) {
                source.WriteByte((byte)(i % 255));
            }
 
            source.Seek(0, SeekOrigin.Begin);
            IoUtil.ReadExactly(source, target, 0, nrOfBytes);
 
            // check target
            source.Seek(0, SeekOrigin.Begin);
            for (int i = 0; i < nrOfBytes; i++) {
                int expected = source.ReadByte();
                Assert.AreEqual(expected, target[i]);
            }
 
        }
 
        [Test]
        public void TestScLessThanBuffer() {
            int nrOfBytes = IoUtil.BufferLength - 2;
            CheckStreamCopy(nrOfBytes);
        }
 
        [Test]
        public void TestScEqualThanBuffer() {
            int nrOfBytes = IoUtil.BufferLength;
            CheckStreamCopy(nrOfBytes);
        }
 
        [Test]
        public void TestScMoreThanBuffer() {
            int nrOfBytes = IoUtil.BufferLength + 1;
            CheckStreamCopy(nrOfBytes);
 
            nrOfBytes = IoUtil.BufferLength * 2;
            CheckStreamCopy(nrOfBytes);

            nrOfBytes = (IoUtil.BufferLength * 2) + 1;
            CheckStreamCopy(nrOfBytes);

            nrOfBytes = (IoUtil.BufferLength * 5) - 4;
            CheckStreamCopy(nrOfBytes);

            nrOfBytes = (IoUtil.BufferLength * 5) + 3;
            CheckStreamCopy(nrOfBytes);

            nrOfBytes = (IoUtil.BufferLength * 7) - 1;
            CheckStreamCopy(nrOfBytes);

        }

        [Test]
        public void TestRLessThanBuffer() {
            int nrOfBytes = IoUtil.BufferLength - 2;
            CheckStreamReadToArray(nrOfBytes);
        }
 
        [Test]
        public void TestREqualThanBuffer() {
            int nrOfBytes = IoUtil.BufferLength;
            CheckStreamReadToArray(nrOfBytes);
        }
 
        [Test]
        public void TestRMoreThanBuffer() {
            int nrOfBytes = IoUtil.BufferLength + 1;
            CheckStreamReadToArray(nrOfBytes);
 
            nrOfBytes = IoUtil.BufferLength * 2;
            CheckStreamReadToArray(nrOfBytes);

            nrOfBytes = (IoUtil.BufferLength * 2) + 1;
            CheckStreamReadToArray(nrOfBytes);

            nrOfBytes = (IoUtil.BufferLength * 5) - 4;
            CheckStreamReadToArray(nrOfBytes);

            nrOfBytes = (IoUtil.BufferLength * 5) + 3;
            CheckStreamReadToArray(nrOfBytes);

            nrOfBytes = (IoUtil.BufferLength * 7) - 1;
            CheckStreamReadToArray(nrOfBytes);

        }
 
    }

}

#endif
