/* GiopHeader.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 16.01.03  Dominic Ullmann (DUL), dul@elca.ch
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
        
        #endregion Constants
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

        internal GiopHeader(CdrInputStreamImpl stream) {
            byte[] readBuffer = stream.ReadOpaque(4);
            if (!((readBuffer[0] == m_giop_magic[0]) && (readBuffer[1] == m_giop_magic[1]) && 
                (readBuffer[2] == m_giop_magic[2]) && (readBuffer[3] == m_giop_magic[3]))) {
                // no GIOP
                throw new IOException("no GIOP-Message");
            } else {
                Trace.WriteLine("GIOP-message starting");
                m_version = new GiopVersion(stream.ReadOctet(), stream.ReadOctet());
                Debug.WriteLine("Version: " + m_version);
                if (m_version.Major != 1) {
                    throw new IOException("unknown GIOP Verision: " + m_version);
                }
            }

            m_flags = stream.ReadOctet();
            m_type = ConvertType((byte) stream.ReadOctet());
            stream.ConfigStream(m_flags, m_version);
            m_msgLength = stream.ReadULong();
            stream.SetMaxLength(m_msgLength);
        }

        #endregion IConstructors
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
            Trace.WriteLine("\nGIOP-message header starting: ");
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
        internal void WriteToStream(Stream stream, uint msgLength) {
        	CdrOutputStream target = new CdrOutputStreamImpl(stream, GiopFlags,
        	                                                 Version);
        	WriteToStream(target, msgLength);
        }

        private GiopMsgTypes ConvertType(byte type) {
            switch (type) {
                case 0: 
                    return GiopMsgTypes.Request;
                case 1: 
                    return GiopMsgTypes.Reply;
                case 2: 
                    return GiopMsgTypes.CancelRequest;
                case 3: 
                    return GiopMsgTypes.LocateRequest;
                case 4: 
                    return GiopMsgTypes.LocateReply;
                case 5: 
                    return GiopMsgTypes.CloseConnection;
                case 6: 
                    return GiopMsgTypes.MessageError;
                case 7: 
                    return GiopMsgTypes.Fragment;
                default:
                    throw new Exception("unknown Giop_msg_type: " + type);
            }
        }

        #endregion IMethods
        
    }
}
