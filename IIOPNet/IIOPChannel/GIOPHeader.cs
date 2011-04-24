/* GiopHeader.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 16.01.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Diagnostics;
using System.IO;
using Ch.Elca.Iiop.Cdr;

namespace Ch.Elca.Iiop {
    
    /// <summary>
    /// specifies the endian.
    /// </summary>
    public enum Endian {
        LittleEndian, BigEndian
    }
    
    public struct GiopVersion {
        
        #region IFields
        
        private byte m_major;
        private byte m_minor;

        #endregion IFields
        #region IConstructors
        
        public GiopVersion(byte major, byte minor) {
            m_major = major;
            m_minor = minor;
        }

        #endregion IConstructors
        #region IProperties
        
        public byte Major { 
            get { 
                return m_major; 
            } 
        }
        public byte Minor {
            get { 
                return m_minor; 
            } 
        }

        #endregion IProperties
        #region IMethods

        public override string ToString() {
            return "GIOP-version: " + m_major + "." + m_minor; 
        }
        
        /// <summary>
        /// returns true, if the protocl version is 1.0 or 1.1, otherwise false
        /// </summary>        
        public bool IsBeforeGiop1_2() {
            return ((Major == 1) && (Minor <= 1));
        }

        /// <summary>
        /// returns true, if the protocl version is bigger than 1.0, otherwise false
        /// </summary>                
        public bool IsAfterGiop1_0() {
            return ((Major == 1) && (Minor > 0)) ||
                    Major > 1;
        }

        #endregion IMethods
    }
    
    /// <summary>
    /// This class represents the header data in the GIOP header
    /// </summary>
    public class GiopHeader {

        #region Constants
        
        /// <summary>the length of the header in bytes</summary>
        internal const int HEADER_LENGTH = 12;
    
        internal const byte FRAGMENT_MASK = 0x02;
        private const byte ENDIAN_MASK = 0x01;
        
        #endregion Constants
        #region SFields
        
        private static Type s_giopMsgTypes = typeof(GiopMsgTypes);
        
        #endregion SFields
        #region IFields

        private GiopVersion m_version;
        private byte m_flags;
        private uint m_msgLength = 0;
        private GiopMsgTypes m_type;
        internal byte[] m_giop_magic = { 71, 73, 79, 80 };        
        
        #endregion IFields
        #region IConstructors

        internal GiopHeader(byte GIOP_major, byte GIOP_minor, byte flags, GiopMsgTypes type) {
            m_version = new GiopVersion(GIOP_major, GIOP_minor);
            m_flags = flags;
            m_type = type;
        }

        internal GiopHeader(CdrInputStreamImpl stream) : this(stream.ReadOpaque(12)) {            
            stream.ConfigStream(m_flags, m_version);
            stream.SetMaxLength(m_msgLength);
        }
        
        /// <summary>
        /// creates a giop header from buffer
        /// </summary>
        /// <param name="buffer">first 12 byte of array are used to parse a giop-header from</param>
        internal GiopHeader(byte[] readBuffer) {
            if ((readBuffer == null) || (readBuffer.Length < 12)) {
                throw new ArgumentException("can't create giop header from buffer");
            }
            if (!((readBuffer[0] == m_giop_magic[0]) && (readBuffer[1] == m_giop_magic[1]) && 
                (readBuffer[2] == m_giop_magic[2]) && (readBuffer[3] == m_giop_magic[3]))) {
                // no GIOP
                Trace.WriteLine("received non GIOP-Message");
                throw new omg.org.CORBA.MARSHAL(19, omg.org.CORBA.CompletionStatus.Completed_No);
            }
            m_version = new GiopVersion(readBuffer[4], readBuffer[5]);
            if (m_version.Major != 1) {
                Trace.WriteLine("unknown GIOP Verision: " + m_version);
                throw new omg.org.CORBA.MARSHAL(20, omg.org.CORBA.CompletionStatus.Completed_No);
            }
            m_flags = readBuffer[6];
            m_type = ConvertType(readBuffer[7]);            
            if (BitConverter.IsLittleEndian == IsLittleEndian()) {
                m_msgLength = BitConverter.ToUInt32(readBuffer, 8);    
            } else {
                // BitConverter uses a different endian, convert to other endian variant
                byte[] msgLengthBuffer = new byte[4]; // make sure to not change input array
                msgLengthBuffer[0] = readBuffer[11];
                msgLengthBuffer[1] = readBuffer[10];
                msgLengthBuffer[2] = readBuffer[9];
                msgLengthBuffer[3] = readBuffer[8];
                m_msgLength = BitConverter.ToUInt32(msgLengthBuffer, 0);
            }            
        }

        #endregion IConstructors
        #region SProperties                
        #endregion SProperties
        #region IProperties

        internal GiopVersion Version {
            get { 
                return m_version; 
            }
        }

        internal byte GiopFlags {
            get    { 
                return m_flags; 
            }
        }

        internal GiopMsgTypes GiopType {
            get { 
                return m_type; 
            }
        }

        internal uint ContentMsgLength {
            get { 
                return m_msgLength; 
            }
        }
        
        #endregion IProperties
        #region IMethods

        /// <summary>
        /// writes this message header to a stream
        /// </summary>
        /// <param name="stream">the stream to write to</param>
        /// <param name="msgLength">the length of the msg content</param>
        internal void WriteToStream(CdrOutputStream stream, uint msgLength) {
            Debug.WriteLine("\nGIOP-message header starting: ");
            // write magic
            for (int i = 0; i < m_giop_magic.Length; i++) {
                Debug.Write(m_giop_magic[i] + " ");    
                stream.WriteOctet(m_giop_magic[i]);    
            }
            // write GIOP_Version
            Debug.Write(m_version.Major + " ");
            stream.WriteOctet(m_version.Major);
            Debug.Write(m_version.Minor + " ");
            stream.WriteOctet(m_version.Minor);
            // writing GIOP_flags
            Debug.Write(m_flags + " ");
            stream.WriteOctet(m_flags);
            Debug.Write((byte)m_type + " ");
            stream.WriteOctet((byte)m_type); // the message type
            Debug.Write("\nMessage-length: " + msgLength + "\n");
            stream.WriteULong(msgLength);
        }
                
        /// <summary>
        /// writes this message header to a stream, using msgLength as 
        /// message Length
        /// </summary>
        /// <returns>the outputstream used, positioned just after the header</returns>
        internal CdrOutputStream WriteToStream(Stream stream, uint msgLength) {
            CdrOutputStream target = new CdrOutputStreamImpl(stream, GiopFlags,
                                                             Version);
            WriteToStream(target, msgLength);
            return target;
        }        

        private GiopMsgTypes ConvertType(int type) {            
            if (!Enum.IsDefined(s_giopMsgTypes, type)) {
                throw new omg.org.CORBA.MARSHAL(15, omg.org.CORBA.CompletionStatus.Completed_No);
            }
            return (GiopMsgTypes)Enum.ToObject(s_giopMsgTypes, type);
        }

        /// <summary>
        /// returns true, if the fragmented bit in the flags is set; otherwise false.
        /// </summary>        
        internal bool IsFragmentedBitSet() {
            return (GiopFlags & GiopHeader.FRAGMENT_MASK) > 0;
        }
        
        /// <summary>
        /// returns true, if the endian bit in the flags is set (=1 -> little endian); otherwise false.
        /// </summary>        
        private bool IsLittleEndian() {
            return (GiopFlags & GiopHeader.ENDIAN_MASK) > 0;
        }

        
        #endregion IMethods
        #region SMethods
        
        /// <summary>
        /// Gets the default header flags for the given endian.
        /// </summary>
        internal static byte GetDefaultHeaderFlagsForEndian(Endian endian) {
            if (endian == Endian.BigEndian) {
                return 0;
            } else {
                return 1;
            }
        }
        
        /// <summary>
        /// Gets the default header flags for the platform endian.
        /// </summary>
        internal static byte GetDefaultHeaderFlagsForPlatform() {
            if (BitConverter.IsLittleEndian) {
                return 1;
            } else {
                return 0;
            }
        }
        
        #endregion SMethods
    }
}
#if UnitTest

namespace Ch.Elca.Iiop.Tests {
    
    using System.Reflection;
    using NUnit.Framework;
    using Ch.Elca.Iiop;

    /// <summary>
    /// Unit-tests for testing request/reply serialisation/deserialisation
    /// </summary>
    [TestFixture]
    public class GiopHeaderTest {
        
        [Test]
        public void TestGetHeaderFlagsForBigEndian() {
            Assert.AreEqual(0, GiopHeader.GetDefaultHeaderFlagsForEndian(Endian.BigEndian));
        }
        
        [Test]
        public void TestGetHeaderFlagsForLittleEndian() {
            Assert.AreEqual(1, GiopHeader.GetDefaultHeaderFlagsForEndian(Endian.LittleEndian));
        }        
        
        [Test]
        public void TestGetHeaderFlagsForPlatform() {
            byte expected;
            if (BitConverter.IsLittleEndian) {
                expected = 1;
            } else {
                expected = 0;
            }
            Assert.AreEqual(expected,
                                   GiopHeader.GetDefaultHeaderFlagsForPlatform());
        }
        
    }
    
}

#endif
