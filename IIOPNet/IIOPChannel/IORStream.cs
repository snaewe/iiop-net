/* IORStream.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 15.01.03  Dominic Ullmann (DUL), dul@elca.ch
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
using omg.org.CORBA;

namespace Ch.Elca.Iiop.CorbaObjRef {


    /// <summary>this class provides utility funcionality for stringify and unstringify in the CORBA-IOR manner</summary>
    public class IorStringifyUtil {
    
        #region SMethods

        /// <summary>gets the stringified representation for the byte data</summary>
        public static string Stringify(byte[] data) {
            if (data == null) { return ""; }
            string result = "";
            for (int i = 0; i < data.Length; i++) {
                byte highChar, lowChar;
                ConvertToHexRep(data[i], out highChar, out lowChar);
                result += ((char)highChar); // add the the character representation of highChar
                result += ((char)lowChar); // add the character representation of lowChar
            }
            return result;
        }

        /// <summary>unstringify the stringified data in the string.</summary>
        public static byte[] Destringify(string data) {
            if (data == null) { 
            	return new byte[0]; 
            }
            if ((data.Length % 2) != 0) { 
                // data is not a valid stringified representation
                throw new INV_OBJREF(9405, CompletionStatus.Completed_No);
            }
            byte[] result = new byte[data.Length / 2];
            for (int i = 0; i < (data.Length / 2); i++) {
                result[i] = ConvertFromHexRep((byte)data[2*i], (byte)data[2*i+1]);
            }
            return result;
        }


        /// <summary>get the hex representation for the data.</summary>
        /// <param name="highChar">the high char in the stringified rep for a byte</param>
        /// <param name="lowChar">the low char in the stringified rep for a byte</param>
        internal static void ConvertToHexRep(byte data, out byte highChar, out byte lowChar) {
            byte highBits = GetHighBits(data);
            byte lowBits = GetLowBits(data);

            highChar = ToHexChar(highBits);
            lowChar = ToHexChar(lowBits);                
        }

        /// <summary>destringify the byte in stringified representation</summary>
        internal static byte ConvertFromHexRep(byte highChar, byte lowChar) {
            if ((!CheckData(highChar)) || (!CheckData(lowChar))) {
                // invalid data to destringify from: + 
                // highChar: highChar lowChar: lowChar
                throw new INV_OBJREF(9407, CompletionStatus.Completed_No);
            }

            string hexByte = Convert.ToChar(highChar).ToString(); // the high four bits of the byte encoded as hex digit
            hexByte += Convert.ToChar(lowChar); // the low four bits of the byte encoded as hex digit
            // now get the number represented by the hexString hexByte
            return Convert.ToByte(hexByte, 16); // parse hex string
        }

        /// <summary>check if the given character data is possible in a stringified string</summary>
        private static bool CheckData(byte data) {
            if (((data >= 0x30) && (data <= 0x39)) ||     // 0-9
                ((data >= 0x41) && (data <= 0x46)) ||     // A-F
                ((data >= 0x61) && (data <= 0x66))) {     // a-f
                return true;
            } else {
                return false;
            }
        }

        /// <summary>gets the high 4 bits in a byte.</summary>
        private static byte GetHighBits(byte data) {
            return (byte)((data & 0xF0) >> 4);
        }

        /// <summary>gets the high 4 bits in a byte.</summary>
        private static byte GetLowBits(byte data) {
            return (byte)(data & 0x0F);
        }

        /// <summary>get the hex string representation for the byte data</summary>
        private static byte ToHexChar(byte data) {
            switch (data) {
                case 0: 
                    return 0x30;
                case 1: 
                    return 0x31;
                case 2: 
                    return 0x32;
                case 3: 
                    return 0x33;
                case 4: 
                    return 0x34;
                case 5: 
                    return 0x35;
                case 6: 
                    return 0x36;
                case 7:
                    return 0x37;
                case 8:
                    return 0x38;
                case 9:
                    return 0x39;
                case 10:
                    return 0x41;
                case 11:
                    return 0x42;
                case 12:
                    return 0x43;
                case 13:
                    return 0x44;
                case 14:
                    return 0x45;
                case 15:
                    return 0x46;
                default:
                    // toHexChar: data must be between 0 and 15
                    throw new INV_OBJREF(9413, CompletionStatus.Completed_No);
            }
        }

        #endregion SMethods

    }
    
    /// <summary>
    /// This stream can write stringified content and can destringify stringified content.
    /// </summary>
    public class IorStream : Stream {
        
        #region IFields
        
        /// <summary>the underlying stream</summary>
        private Stream m_stream;
        
        private bool m_prefixRead = false;
        private bool m_prefixWritten = false;
        
        // byre rep for IOR:
        private byte[] m_iorMagic = { 0x49, 0x4F, 0x52, 0x3A };

        #endregion IFields
        #region IConstructors

        /// <summary>
        /// constructs a new IORStream based on the Stream stream, the stream must start with IOR:, and
        /// then contain only IOR stringified content, else it's invalid
        /// </summary>
        /// <param name="stream">the stream, containing the stringified data</param>
        public IorStream(Stream stream) {
            m_stream = stream;
        }

        #endregion IConstructors
        #region IProperties

        public override bool CanRead {
            get { 
                return true; 
            }
        }

        public override bool CanWrite {
            get {
                return true; 
            }
        }

        public override bool CanSeek {
            get { 
                return false; 
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

        /// <summary>
        /// reads a stringified representation fo a single byte from the stream
        /// </summary>
        /// <remarks>
        /// IOR: is read automatically, when ReadByte is called the first time. If the stream doesn't start with IOR:
        /// than an Exception is thrown.
        /// The first byte returned is the first byte after IOR:
        /// </remarks>
        /// <returns>the destringified byte</returns>
        public override int ReadByte() {
            CheckReadPrefix(); // read prefix if ReadByte is called the first time
            int highByte = m_stream.ReadByte();
            if (highByte == -1) { return -1; }
            int lowByte = m_stream.ReadByte();
            if (lowByte == -1) { 
            	// IORStream.ReadByte: the stream read from ended inside a hexdigit
            	throw new INV_OBJREF(9416, CompletionStatus.Completed_No);
            }
            
            return IorStringifyUtil.ConvertFromHexRep((byte)highByte, (byte)lowByte);
        }

        /// <summary>
        /// write the stringified representation for a single byte to the stream
        /// </summary>
        /// <remarks>
        /// IOR: is automatically written when WriteByte is called the first time.
        /// </remarks>
        /// <param name="data">the byte to write</param>
        public override void WriteByte(byte data) {
            CheckWritePrefix(); // write prefix if WriteByte is called the first time
            byte highChar;
            byte lowChar;
            IorStringifyUtil.ConvertToHexRep(data, out highChar, out lowChar);

            m_stream.WriteByte(highChar);
            m_stream.WriteByte(lowChar);
        }
        
        /// <summary>
        /// reads the IOR magic, if not already read
        /// </summary>
        private void CheckReadPrefix() {
            if (!m_prefixRead) {
                for (int i = 0; i < m_iorMagic.Length; i++) {
                    if (m_stream.ReadByte() != m_iorMagic[i]) {
                        // invalid ior-stream, must start with IOR
                        throw new INV_OBJREF(9420, CompletionStatus.Completed_No);
                    }
                }
                m_prefixRead = true;
            }
        }

        /// <summary>
        /// writes the IOR magic, if not already written
        /// </summary>
        private void CheckWritePrefix() {
            if (!m_prefixWritten) {
                for (int i = 0; i < m_iorMagic.Length; i++) {
                    m_stream.WriteByte(m_iorMagic[i]);
                }
                m_prefixWritten = true;
            }
        }

        public override int Read(byte[] buffer, int offset, int count) {
            for (int i = 0; i < count; i++) {
                buffer[offset+i] = (byte)ReadByte();
            }
            return count;
        }

        public override void Write(byte[] buffer, int offset, int count) {
            for (int i = 0; i < count; i++) {
                WriteByte(buffer[offset+i]);
            }
        }

        public override void Flush() {
            m_stream.Flush();
        }

        public override long Length {
            get { 
            	throw new NotSupportedException(); 
            }
        }

        public override void SetLength(long length) {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin) {
            throw new NotSupportedException();
        }

        #endregion IMethods

    }

}

#if UnitTest

namespace Ch.Elca.Iiop.Tests {
	
    using NUnit.Framework;    
    using System;
	using System.IO;
	using Ch.Elca.Iiop.CorbaObjRef;
	using omg.org.CORBA;
	
    /// <summary>
    /// Unit test for testing IorStream
    /// </summary>
    public class IorTest : TestCase {
        
        public IorTest() {
        }

        public void TestIorDeStringification() {
            MemoryStream memStream = new MemoryStream();
            
            // IOR:
            memStream.WriteByte(0x49);
            memStream.WriteByte(0x4F);
            memStream.WriteByte(0x52);
            memStream.WriteByte(0x3A);
            
            // content:
            memStream.WriteByte(0x35);
            memStream.WriteByte(0x33);
            
            memStream.WriteByte(0x36);
            memStream.WriteByte(0x31);
            
            memStream.WriteByte(0x37);
            memStream.WriteByte(0x39);
            
            memStream.WriteByte(0x34);
            memStream.WriteByte(0x38);
            
            memStream.WriteByte(0x36);
            memStream.WriteByte(0x35);

            memStream.WriteByte(0x36);
            memStream.WriteByte(0x43);		

            memStream.WriteByte(0x36);
            memStream.WriteByte(0x43);

            memStream.WriteByte(0x36);
            memStream.WriteByte(0x46);
            
            memStream.Seek(0, SeekOrigin.Begin);
            
            IorStream iorStream = new IorStream(memStream);
            Assertion.AssertEquals(83, iorStream.ReadByte());
            Assertion.AssertEquals(97, iorStream.ReadByte());
            Assertion.AssertEquals(121, iorStream.ReadByte());
            Assertion.AssertEquals(72, iorStream.ReadByte());
            Assertion.AssertEquals(101, iorStream.ReadByte());
            Assertion.AssertEquals(108, iorStream.ReadByte());
            Assertion.AssertEquals(108, iorStream.ReadByte());
            Assertion.AssertEquals(111, iorStream.ReadByte());
        }

        public void TestIorStringification() {
            MemoryStream memStream = new MemoryStream();

            IorStream iorStream = new IorStream(memStream);
            iorStream.WriteByte(83);
            iorStream.WriteByte(97);
            iorStream.WriteByte(121);
            iorStream.WriteByte(72);
            iorStream.WriteByte(101);
            iorStream.WriteByte(108);
            iorStream.WriteByte(108);
            iorStream.WriteByte(111);
            
            memStream.Seek(0, SeekOrigin.Begin);
            
            // IOR magic
            Assertion.AssertEquals(0x49, memStream.ReadByte());
            Assertion.AssertEquals(0x4F, memStream.ReadByte());
            Assertion.AssertEquals(0x52, memStream.ReadByte());
            Assertion.AssertEquals(0x3A, memStream.ReadByte());

            // content
            Assertion.AssertEquals(0x35, memStream.ReadByte());
            Assertion.AssertEquals(0x33, memStream.ReadByte());
            
            Assertion.AssertEquals(0x36, memStream.ReadByte());
            Assertion.AssertEquals(0x31, memStream.ReadByte());

            Assertion.AssertEquals(0x37, memStream.ReadByte());
            Assertion.AssertEquals(0x39, memStream.ReadByte());

            Assertion.AssertEquals(0x34, memStream.ReadByte());
            Assertion.AssertEquals(0x38, memStream.ReadByte());

            Assertion.AssertEquals(0x36, memStream.ReadByte());
            Assertion.AssertEquals(0x35, memStream.ReadByte());

            Assertion.AssertEquals(0x36, memStream.ReadByte());
            Assertion.AssertEquals(0x43, memStream.ReadByte());

            Assertion.AssertEquals(0x36, memStream.ReadByte());
            Assertion.AssertEquals(0x43, memStream.ReadByte());

            Assertion.AssertEquals(0x36, memStream.ReadByte());
            Assertion.AssertEquals(0x46, memStream.ReadByte());

            Assertion.AssertEquals(-1, memStream.ReadByte());
		}

        [ExpectedException(typeof(INV_OBJREF))]
        public void TestInvalidDataInIorStream() {
            MemoryStream stream = new MemoryStream();
            stream.WriteByte(0x01);
            stream.WriteByte(0x02);
            stream.Seek(0, SeekOrigin.Begin);
			IorStream iorStream = new IorStream(stream);
            byte data = (byte)iorStream.ReadByte();
        }

    }

}

#endif
