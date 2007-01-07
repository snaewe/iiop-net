/* CdrStreamEndianDepOp.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 28.05.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Text;
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Util;
using Ch.Elca.Iiop.Services;
using Ch.Elca.Iiop.CodeSet;
using omg.org.CORBA;

namespace Ch.Elca.Iiop.Cdr {
    
    /// <summary>
    /// Base class for endian read op base implementations
    /// </summary>
    internal abstract class CdrStreamEndianReadOpBase : CdrEndianDepInputStreamOp {
        
        #region IFields
        
        protected CdrInputStream m_stream;
        protected GiopVersion m_version;        
        
        #endregion IFields
        #region IConstructors
        
        protected CdrStreamEndianReadOpBase(CdrInputStream stream, GiopVersion version) {
            m_stream = stream;
            m_version = version;
        }
        
        #endregion IConstructors
        
    	
		public abstract short ReadShort();
		
		public void ReadShortArray(short[] array) {
		    for (int i = 0; i < array.Length; i++) {
		        array[i] = ReadShort();
		    }
		}
    	
		public abstract ushort ReadUShort();
		
		public void ReadUShortArray(ushort[] array) {
		    for (int i = 0; i < array.Length; i++) {
		        array[i] = ReadUShort();
		    }
		}		
    	
		public abstract int ReadLong();
		
		public void ReadLongArray(int[] array) {
		    for (int i = 0; i < array.Length; i++) {
		        array[i] = ReadLong();
		    }
		}				
    	
		public abstract uint ReadULong();
		
		public void ReadULongArray(uint[] array) {
		    for (int i = 0; i < array.Length; i++) {
		        array[i] = ReadULong();
		    }
		}						
    	
		public abstract long ReadLongLong();
		
		public void ReadLongLongArray(long[] array) {
		    for (int i = 0; i < array.Length; i++) {
		        array[i] = ReadLongLong();
		    }
		}								
    	
		public abstract ulong ReadULongLong();

		public void ReadULongLongArray(ulong[] array) {
		    for (int i = 0; i < array.Length; i++) {
		        array[i] = ReadULongLong();
		    }
		}										
    	
		public abstract float ReadFloat();
		
		public void ReadFloatArray(float[] array) {
		    for (int i = 0; i < array.Length; i++) {
		        array[i] = ReadFloat();
		    }
		}		
    	
		public abstract double ReadDouble();
    	
		public void ReadDoubleArray(double[] array) {
		    for (int i = 0; i < array.Length; i++) {
		        array[i] = ReadDouble();
		    }
		}
    	
		public abstract char ReadWChar();
    	
		public abstract string ReadWString();
    }

    /// <summary>
    /// Base class for endian write op base implementations
    /// </summary>    
    internal abstract class CdrStreamEndianWriteOpBase : CdrEndianDepOutputStreamOp {
        
        #region IFields

        protected CdrOutputStream m_stream;
        protected GiopVersion m_version;

        #endregion IFields
        #region IConstructors
        
        public CdrStreamEndianWriteOpBase(CdrOutputStream stream, GiopVersion version) {
            m_stream = stream;
            m_version = version;
        }        
        
        #endregion IConstructors
        #region IMethods
    	
		public abstract void WriteShort(short data);
		
		public void WriteShortArray(short[] array) {
		    for (int i = 0; i < array.Length; i++) {
		        WriteShort(array[i]);
		    }
		}
    	
		public abstract void WriteUShort(ushort data);
		
		public void WriteUShortArray(ushort[] array) {
		    for (int i = 0; i < array.Length; i++) {
		        WriteUShort(array[i]);
		    }
		}		
    	
		public abstract void WriteLong(int data);
		
		public void WriteLongArray(int[] array) {
		    for (int i = 0; i < array.Length; i++) {
		        WriteLong(array[i]);
		    }
		}				
    	
		public abstract void WriteULong(uint data);
		
		public void WriteULongArray(uint[] array) {
		    for (int i = 0; i < array.Length; i++) {
		        WriteULong(array[i]);
		    }
		}						
    	
		public abstract void WriteLongLong(long data);
		
		public void WriteLongLongArray(long[] array) {
		    for (int i = 0; i < array.Length; i++) {
		        WriteLongLong(array[i]);
		    }
		}
    	
		public abstract void WriteULongLong(ulong data);
		
		public void WriteULongLongArray(ulong[] array) {
		    for (int i = 0; i < array.Length; i++) {
		        WriteULongLong(array[i]);
		    }
		}
    	
		public abstract void WriteFloat(float data);
		
		public void WriteFloatArray(float[] array) {
		    for (int i = 0; i < array.Length; i++) {
		        WriteFloat(array[i]);
		    }
		}		
    	
		public abstract void WriteDouble(double data);
    	
		public void WriteDoubleArray(double[] array) {
		    for (int i = 0; i < array.Length; i++) {
		        WriteDouble(array[i]);
		    }
		}
    	
		public abstract void WriteWChar(char data);
    	
		public abstract void WriteWString(string data);
		
		#endregion IMethods
    }
    
    /// <summary>
    /// this is the endian implementation for the endian dependent operation for CDRInput-streams
    /// to use, when the stream endian is different than the plattform endian (see BitConverter.IsLittleEndian)
    /// </summary>
    /// <remarks>
    /// this class is not intended for use by CDRStream users
    /// </remarks>
    internal class CdrStreamNonNativeEndianReadOP : CdrStreamEndianReadOpBase, CdrEndianDepInputStreamOp {
        
        #region IFields

        private byte[] m_buf = new byte[8];

        #endregion IFields
        #region IConstructors

        public CdrStreamNonNativeEndianReadOP(CdrInputStream stream, GiopVersion version) : base(stream, version) {
        }

        #endregion IConstructors
        #region IMethods
                
        #region read methods depenant on byte ordering

        private void Read(int size, Aligns align) {
            m_stream.ForceReadAlign(align);
            m_stream.ReadBytes(m_buf, 0, size);
        }

        public override short ReadShort() {
            Read(2, Aligns.Align2);
            return NonNativeEndianSystemWireBitConverter.ToInt16(m_buf);
        }

        public override ushort ReadUShort() {
            Read(2, Aligns.Align2);
            return NonNativeEndianSystemWireBitConverter.ToUInt16(m_buf);
        }

        public override int ReadLong() {
            Read(4, Aligns.Align4);
            return NonNativeEndianSystemWireBitConverter.ToInt32(m_buf);
        }

        public override uint ReadULong() {
            Read(4, Aligns.Align4);
            return NonNativeEndianSystemWireBitConverter.ToUInt32(m_buf);
        }

        public override long ReadLongLong() {
            Read(8, Aligns.Align8);
            return NonNativeEndianSystemWireBitConverter.ToInt64(m_buf);
        }

        public override ulong ReadULongLong() {
            Read(8, Aligns.Align8);
            return NonNativeEndianSystemWireBitConverter.ToUInt64(m_buf);
        }

        public override float ReadFloat() {
            Read(4, Aligns.Align4);
            return NonNativeEndianSystemWireBitConverter.ToSingle(m_buf);
        }

        public override double ReadDouble() {
            Read(8, Aligns.Align8);
            return NonNativeEndianSystemWireBitConverter.ToDouble(m_buf);
        }
        
        private byte[] AppendChar(byte[] oldData) {
            byte[] newData = new byte[oldData.Length+1];
            Array.Copy(oldData, 0, newData, 0, oldData.Length);
            newData[oldData.Length] = m_stream.ReadOctet();
            return newData;
        }
        
        public override char ReadWChar() {
            Encoding encoding = CodeSetService.GetCharEncoding(m_stream.WCharSet, true, 
                                                               !BitConverter.IsLittleEndian);
            byte[] data;
            if ((m_version.Major == 1) && (m_version.Minor <= 1)) { // GIOP 1.1 / 1.0
                data = new byte[] { m_stream.ReadOctet() };
                while (encoding.GetCharCount(data) < 1) {
                    data = AppendChar(data);
                }
            } else { // GIOP 1.2 or above
                byte count = m_stream.ReadOctet();
                data = m_stream.ReadOpaque(count);
            }            
            char[] result = encoding.GetChars(data);
            return result[0];
        }

        public override string ReadWString()    {
            Encoding encoding = CodeSetService.GetCharEncoding(m_stream.WCharSet, true, 
                                                               !BitConverter.IsLittleEndian);
            uint length = ReadULong(); 
            byte[] data;
            if ((m_version.Major == 1) && (m_version.Minor <= 1)) { // GIOP 1.1 / 1.0
                length = (length * 2); // only 2 bytes fixed size characters supported
                data = m_stream.ReadOpaque((int)length - 2); // exclude trailing zero
                m_stream.ReadOctet(); // read trailing zero: a wide character
                m_stream.ReadOctet(); // read trailing zero: a wide character
            } else {
                data = m_stream.ReadOpaque((int)length);
            }
            char[] result = encoding.GetChars(data);
            
            return new string(result);
        }

        #endregion

        #endregion IMethods

    }


    /// <summary>
    /// this is the endian implementation for the endian dependent operation for CDROutput-streams
    /// to use, when the stream endian is different than the plattform endian (see BitConverter.IsLittleEndian)
    /// </summary>
    /// <remarks>
    /// this class is not intended for use by CDRStream users
    /// </remarks>
    internal class CdrStreamNonNativeEndianWriteOP : CdrStreamEndianWriteOpBase, CdrEndianDepOutputStreamOp {

        #region IConstructors
        
        public CdrStreamNonNativeEndianWriteOP(CdrOutputStream stream, GiopVersion version) : base(stream, version) {
        }

        #endregion IConstructors
        #region IMethods
                
        #region write methods dependant on byte ordering

    	private void Write(byte[] data, int count, Aligns align) {
	        m_stream.ForceWriteAlign(align);
		    m_stream.WriteBytes(data, 0, count);
	    }
        
        public override void WriteShort(short data) {
		    Write(NonNativeEndianSystemWireBitConverter.GetBytes(data), 2, Aligns.Align2);
        }

        public override void WriteUShort(ushort data) {
		    Write(NonNativeEndianSystemWireBitConverter.GetBytes(data), 2, Aligns.Align2);
        }

        public override void WriteLong(int data) {
		    Write(NonNativeEndianSystemWireBitConverter.GetBytes(data), 4, Aligns.Align4);
        }

        public override void WriteULong(uint data) {
		    Write(NonNativeEndianSystemWireBitConverter.GetBytes(data), 4, Aligns.Align4);
        }

        public override void WriteLongLong(long data) {
		    Write(NonNativeEndianSystemWireBitConverter.GetBytes(data), 8, Aligns.Align8);
        }

        public override void WriteULongLong(ulong data) {
		    Write(NonNativeEndianSystemWireBitConverter.GetBytes(data), 8, Aligns.Align8);
        }

        public override void WriteFloat(float data) {
		    Write(NonNativeEndianSystemWireBitConverter.GetBytes(data), 4, Aligns.Align4);
        }

        public override void WriteDouble(double data) {
		    Write(NonNativeEndianSystemWireBitConverter.GetBytes(data), 8, Aligns.Align8);
        }

        public override void WriteWChar(char data) {
            Encoding encoding = CodeSetService.GetCharEncoding(m_stream.WCharSet, true, 
                                                               !BitConverter.IsLittleEndian);
            byte[] toSend = encoding.GetBytes(new char[] { data } );
            if (!((m_version.Major == 1) && (m_version.Minor <= 1))) { // GIOP 1.2
                m_stream.WriteOctet((byte)toSend.Length);
            }
            m_stream.WriteOpaque(toSend);
        }

        public override void WriteWString(string data) {
            Encoding encoding = CodeSetService.GetCharEncoding(m_stream.WCharSet, true, 
                                                               !BitConverter.IsLittleEndian);
            byte[] toSend = encoding.GetBytes(data);
            if ((m_version.Major == 1) && (m_version.Minor <= 1)) { // GIOP 1.0, 1.1
                byte[] sendNew = new byte[toSend.Length + 2];
                Array.Copy((Array)toSend, 0, (Array)sendNew, 0, toSend.Length);
                sendNew[toSend.Length] = 0; // trailing zero: a wide char
                sendNew[toSend.Length + 1] = 0; // trailing zero: a wide char
                m_stream.WriteULong(((uint)toSend.Length / 2) + 1); // number of chars instead of number of bytes, only 2 bytes character supported
                m_stream.WriteOpaque(sendNew);
            } else {
                m_stream.WriteULong((uint)toSend.Length);
                m_stream.WriteOpaque(toSend);
            }
        }

        #endregion write methods dependant on byte ordering

        #endregion IMethods

    }


    /// <summary>
    /// this is the endian implementation for the endian dependent operation for CDRInput-streams
    /// to use, when the stream endian is the same as the plattform endian (see BitConverter.IsLittleEndian)
    /// </summary>
    /// <remarks>
    /// this class is not intended for use by CDRStream users
    /// </remarks>
    internal class CdrStreamNativeEndianReadOP : CdrStreamEndianReadOpBase, CdrEndianDepInputStreamOp {
        
        #region IFields

        private byte[] m_buf = new byte[8];

        #endregion IFields
        #region IConstructors

        public CdrStreamNativeEndianReadOP(CdrInputStream stream, GiopVersion version) : base(stream, version) {
        }

        #endregion IConstructors
        #region IMethods
                
        #region read methods depenant on byte ordering

        private void Read(int size, Aligns align) {
            m_stream.ForceReadAlign(align);
            m_stream.ReadBytes(m_buf, 0, size);
        }

        public override short ReadShort() {
            Read(2, Aligns.Align2);
            return BitConverter.ToInt16(m_buf, 0);
        }

        public override ushort ReadUShort() {
            Read(2, Aligns.Align2);
            return BitConverter.ToUInt16(m_buf, 0);
        }

        public override int ReadLong() {
            Read(4, Aligns.Align4);
            return BitConverter.ToInt32(m_buf, 0);
        }

        public override uint ReadULong() {
            Read(4, Aligns.Align4);
            return BitConverter.ToUInt32(m_buf, 0);
        }

        public override long ReadLongLong() {
            Read(8, Aligns.Align8);
            return BitConverter.ToInt64(m_buf, 0);
        }

        public override ulong ReadULongLong() {
            Read(8, Aligns.Align8);
            return BitConverter.ToUInt64(m_buf, 0);
        }

        public override float ReadFloat() {
            Read(4, Aligns.Align4);
            return BitConverter.ToSingle(m_buf, 0);
        }

        public override double ReadDouble() {
            Read(8, Aligns.Align8);
            return BitConverter.ToDouble(m_buf, 0);
        }
        
        private byte[] AppendChar(byte[] oldData) {
            byte[] newData = new byte[oldData.Length+1];
            Array.Copy(oldData, 0, newData, 0, oldData.Length);
            newData[oldData.Length] = m_stream.ReadOctet();
            return newData;
        }
        
        public override char ReadWChar() {
            Encoding encoding = CodeSetService.GetCharEncoding(m_stream.WCharSet, true, 
                                                               BitConverter.IsLittleEndian);
            byte[] data;
            if ((m_version.Major == 1) && (m_version.Minor <= 1)) { // GIOP 1.1 / 1.0
                data = new byte[] { m_stream.ReadOctet() };
                while (encoding.GetCharCount(data) < 1) {
                    data = AppendChar(data);
                }
            } else { // GIOP 1.2 or above
                byte count = m_stream.ReadOctet();
                data = m_stream.ReadOpaque(count);
            }
            char[] result = encoding.GetChars(data);            
            return result[0];
        }

        public override string ReadWString()    {
            Encoding encoding = CodeSetService.GetCharEncoding(m_stream.WCharSet, true, 
                                                               BitConverter.IsLittleEndian);
            uint length = ReadULong(); 
            byte[] data;
            if ((m_version.Major == 1) && (m_version.Minor <= 1)) { // GIOP 1.1 / 1.0
                length = (length * 2); // only 2 bytes fixed size characters supported
                data = m_stream.ReadOpaque((int)length - 2); // exclude trailing zero
                m_stream.ReadOctet(); // read trailing zero: a wide character
                m_stream.ReadOctet(); // read trailing zero: a wide character
            } else {
                data = m_stream.ReadOpaque((int)length);
            }
            char[] result = encoding.GetChars(data);
            
            return new string(result);
        }

        #endregion

        #endregion IMethods

    }


    /// <summary>
    /// this is the endian implementation for the endian dependent operation for CDROutput-streams
    /// to use, when the stream endian is the same as the plattform endian (see BitConverter.IsLittleEndian)
    /// </summary>
    /// <remarks>
    /// this class is not intended for use by CDRStream users
    /// </remarks>
    internal class CdrStreamNativeEndianWriteOP : CdrStreamEndianWriteOpBase, CdrEndianDepOutputStreamOp {

        #region IConstructors
        
        public CdrStreamNativeEndianWriteOP(CdrOutputStream stream, GiopVersion version) : base(stream, version) {
        }

        #endregion IConstructors
        #region IMethods
                
        #region write methods dependant on byte ordering

       	private void Write(byte[] data, int count, Aligns align) {
		    m_stream.ForceWriteAlign(align);
		    m_stream.WriteBytes(data, 0, count);
	    }
        
        public override void WriteShort(short data) {
		    Write(BitConverter.GetBytes(data), 2, Aligns.Align2);
        }

        public override void WriteUShort(ushort data) {
		    Write(BitConverter.GetBytes(data), 2, Aligns.Align2);
        }

        public override void WriteLong(int data) {
		    Write(BitConverter.GetBytes(data), 4, Aligns.Align4);
        }

        public override void WriteULong(uint data) {
		    Write(BitConverter.GetBytes(data), 4, Aligns.Align4);
        }

        public override void WriteLongLong(long data) {
		    Write(BitConverter.GetBytes(data), 8, Aligns.Align8);
        }

        public override void WriteULongLong(ulong data) {
		    Write(BitConverter.GetBytes(data), 8, Aligns.Align8);
        }

        public override void WriteFloat(float data) {
		    Write(BitConverter.GetBytes(data), 4, Aligns.Align4);
        }

        public override void WriteDouble(double data) {
		    Write(BitConverter.GetBytes(data), 8, Aligns.Align8);
        }

        public override void WriteWChar(char data) {
            Encoding encoding = CodeSetService.GetCharEncoding(m_stream.WCharSet, true,
                                                               BitConverter.IsLittleEndian);
            byte[] toSend = encoding.GetBytes(new char[] { data } );
            if (!((m_version.Major == 1) && (m_version.Minor <= 1))) { // GIOP 1.2
                m_stream.WriteOctet((byte)toSend.Length);
            }
            m_stream.WriteOpaque(toSend);
        }

        public override void WriteWString(string data) {
            Encoding encoding = CodeSetService.GetCharEncoding(m_stream.WCharSet, true, 
                                                               BitConverter.IsLittleEndian);
            byte[] toSend = encoding.GetBytes(data);
            if ((m_version.Major == 1) && (m_version.Minor <= 1)) { // GIOP 1.0, 1.1
                byte[] sendNew = new byte[toSend.Length + 2];
                Array.Copy((Array)toSend, 0, (Array)sendNew, 0, toSend.Length);
                sendNew[toSend.Length] = 0; // trailing zero: a wide char
                sendNew[toSend.Length + 1] = 0; // trailing zero: a wide char
                m_stream.WriteULong(((uint)toSend.Length / 2) + 1); // number of chars instead of number of bytes, only 2 bytes character supported
                m_stream.WriteOpaque(sendNew);
            } else {
                m_stream.WriteULong((uint)toSend.Length);
                m_stream.WriteOpaque(toSend);
            }
        }

        #endregion write methods dependant on byte ordering

        #endregion IMethods

    }


    /// <summary>
    /// An Instance of this class is used, if the endian flag is not yet specified in a CdrStream.
    /// </summary>
    internal class CdrEndianOpNotSpecified : CdrEndianDepInputStreamOp, CdrEndianDepOutputStreamOp {
        
        #region IMethods
        
        public short ReadShort() {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }
               
        public void ReadShortArray(short[] array) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);            
        }
        
        public ushort ReadUShort() {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }
        
        public void ReadUShortArray(ushort[] array) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);            
        }        

        public int ReadLong() {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }
        
        public void ReadLongArray(int[] array) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);            
        }                

        public uint ReadULong() {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);        
        }
        
        public void ReadULongArray(uint[] array) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);            
        }                       

        public long ReadLongLong() {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }
        
        public void ReadLongLongArray(long [] array) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);            
        }                               

        public ulong ReadULongLong() {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }
        
        public void ReadULongLongArray(ulong [] array) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);            
        }                                       

        public float ReadFloat() {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }
        
        public void ReadFloatArray(float[] array) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);            
        }                

        public double ReadDouble() {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        public void ReadDoubleArray(double[] array) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);            
        }        
        
        public char ReadWChar() {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        public string ReadWString() {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }
   

        public void WriteShort(short data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }
        
        public void WriteShortArray(short[] data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);            
        }

        public void WriteUShort(ushort data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }
        
        public void WriteUShortArray(ushort[] data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);            
        }        

        public void WriteLong(int data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }
        
        public void WriteLongArray(int[] data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);            
        }        
        
        public void WriteULong(uint data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }
        
        public void WriteULongArray(uint[] data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);            
        }               

        public void WriteLongLong(long data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }
        
        public void WriteLongLongArray(long[] data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);            
        }                       

        public void WriteULongLong(ulong data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }
        
        public void WriteULongLongArray(ulong[] data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);            
        }                               

        public void WriteFloat(float data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }
        
        public void WriteFloatArray(float[] array) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);            
        }        

        public void WriteDouble(double data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }
        
        public void WriteDoubleArray(double[] array) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);            
        }

        public void WriteWChar(char data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        public void WriteWString(string data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        #endregion IMethods
            
    }


}

#if UnitTest

namespace Ch.Elca.Iiop.Tests {
    
    using System;
    using System.Reflection;
    using Ch.Elca.Iiop.Cdr;
    using NUnit.Framework;
    
    /// <summary>
    /// Unit-tests for testing CdrEndianDepOp Tests.
    /// </summary>
    [TestFixture]
    public class CdrEndianDepOpTest {

        private const byte STREAM_BIG_ENDIAN_FLAG = 0;
        private const byte STREAM_LITTLE_ENDIAN_FLAG = 1;
    	
    	[Test]
    	public void TestInt16WBEWToS() {
    	    MemoryStream stream = new MemoryStream(new byte[] { 0, 1,
    	                                                        1, 2,
    	                                                        0x7F, 0xFF,
    	                                                        0x80, 0x00 });
    	    CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
    	    cdrIn.ConfigStream(STREAM_BIG_ENDIAN_FLAG, new GiopVersion(1, 2));
    	        	    
    		System.Int16 result = cdrIn.ReadShort();
    		Assertion.AssertEquals("read wbe int 16", 1, result);    		
    		result = cdrIn.ReadShort();
    		Assertion.AssertEquals("read wbe int 16 (2)", 258, result);
    		result = cdrIn.ReadShort();
    		Assertion.AssertEquals("read wbe int 16 (3)", Int16.MaxValue, result);
    		result = cdrIn.ReadShort();
    		Assertion.AssertEquals("read wbe int 16 (4)", Int16.MinValue, result);
    	}
    	
    	[Test]
    	public void TestInt16WLEWToS() {
    	    MemoryStream stream = new MemoryStream(new byte[] { 1, 0,
    	                                                        2, 1,
    	                                                        0xFF, 0x7F,
    	                                                        0x00, 0x80 });
    	    CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
    	    cdrIn.ConfigStream(STREAM_LITTLE_ENDIAN_FLAG, new GiopVersion(1, 2));
    	        	    
    		System.Int16 result = cdrIn.ReadShort();
    		Assertion.AssertEquals("read wle int 16", 1, result);    		
    		result = cdrIn.ReadShort();
    		Assertion.AssertEquals("read wle int 16 (2)", 258, result);
    		result = cdrIn.ReadShort();
    		Assertion.AssertEquals("read wle int 16 (3)", Int16.MaxValue, result);
    		result = cdrIn.ReadShort();
    		Assertion.AssertEquals("read wle int 16 (4)", Int16.MinValue, result);
    	}    	    	
    	
    	[Test]
    	public void TestInt16WBESToW() {
    	    MemoryStream stream = new MemoryStream();
    	    CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_BIG_ENDIAN_FLAG);
    	    cdrOut.WriteShort((short)1);
    	    cdrOut.WriteShort((short)258);
    	    cdrOut.WriteShort(Int16.MaxValue);
    	    cdrOut.WriteShort(Int16.MinValue);
    	    stream.Seek(0, SeekOrigin.Begin);
    	    byte[] result = new byte[2];
    	    stream.Read(result, 0, 2);
    	    ArrayAssertion.AssertByteArrayEquals("converted wbe int 16", new byte[] { 0, 1 }, result);
    	    stream.Read(result, 0, 2);    	        	        	        		
    		ArrayAssertion.AssertByteArrayEquals("converted wbe int 16 (2)", new byte[] { 1, 2 }, result);
    	    stream.Read(result, 0, 2);    	        	        	        		
    		ArrayAssertion.AssertByteArrayEquals("converted wbe int 16 (3)", new byte[] { 0x7F, 0xFF }, result);
    	    stream.Read(result, 0, 2);    	        	        	        		    		
    		ArrayAssertion.AssertByteArrayEquals("converted wbe int 16 (4)", new byte[] { 0x80, 0x00 }, result);    		
    	}
    	
    	[Test]
    	public void TestInt16WLESToW() {
    	    MemoryStream stream = new MemoryStream();
    	    CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_LITTLE_ENDIAN_FLAG);
    	    cdrOut.WriteShort((short)1);
    	    cdrOut.WriteShort((short)258);
    	    cdrOut.WriteShort(Int16.MaxValue);
    	    cdrOut.WriteShort(Int16.MinValue);
    	    stream.Seek(0, SeekOrigin.Begin);
    	    byte[] result = new byte[2];
    	    stream.Read(result, 0, 2);
    	    ArrayAssertion.AssertByteArrayEquals("converted lbe int 16", new byte[] { 1, 0 }, result);
    	    stream.Read(result, 0, 2);    	        	        	        		
    		ArrayAssertion.AssertByteArrayEquals("converted lbe int 16 (2)", new byte[] { 2, 1 }, result);
    	    stream.Read(result, 0, 2);    	        	        	        		
    		ArrayAssertion.AssertByteArrayEquals("converted lbe int 16 (3)", new byte[] { 0xFF, 0x7F }, result);
    	    stream.Read(result, 0, 2);    	        	        	        		    		
    		ArrayAssertion.AssertByteArrayEquals("converted lbe int 16 (4)", new byte[] { 0x00, 0x80 }, result);    		
    	}
    	
    	[Test]
    	public void TestUInt16WBEWToS() {
    	    MemoryStream stream = new MemoryStream(new byte[] { 0, 1,
    	                                                        1, 2,
    	                                                        0xFF, 0xFF,
    	                                                        0x00, 0x00 });
    	    CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
    	    cdrIn.ConfigStream(STREAM_BIG_ENDIAN_FLAG, new GiopVersion(1, 2));
    	        	    
    		System.UInt16 result = cdrIn.ReadUShort();
    		Assertion.AssertEquals("read wbe uint 16", 1, result);    		
    		result = cdrIn.ReadUShort();
    		Assertion.AssertEquals("read wbe uint 16 (2)", 258, result);
    		result = cdrIn.ReadUShort();
    		Assertion.AssertEquals("read wbe uint 16 (3)", UInt16.MaxValue, result);
    		result = cdrIn.ReadUShort();
    		Assertion.AssertEquals("read wbe uint 16 (4)", UInt16.MinValue, result);    	    
    	}
    	
    	[Test]
    	public void TestUInt16WLEWToS() {
    	    MemoryStream stream = new MemoryStream(new byte[] { 1, 0,
    	                                                        2, 1,
    	                                                        0xFF, 0xFF,
    	                                                        0x00, 0x00 });
    	    CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
    	    cdrIn.ConfigStream(STREAM_LITTLE_ENDIAN_FLAG, new GiopVersion(1, 2));
    	        	    
    		System.UInt16 result = cdrIn.ReadUShort();
    		Assertion.AssertEquals("read wle uint 16", 1, result);    		
    		result = cdrIn.ReadUShort();
    		Assertion.AssertEquals("read wle uint 16 (2)", 258, result);
    		result = cdrIn.ReadUShort();
    		Assertion.AssertEquals("read wle uint 16 (3)", UInt16.MaxValue, result);
    		result = cdrIn.ReadUShort();
    		Assertion.AssertEquals("read wle uint 16 (4)", UInt16.MinValue, result);
    	}

    	[Test]
    	public void TestUInt16WBESToW() {
    	    MemoryStream stream = new MemoryStream();
    	    CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_BIG_ENDIAN_FLAG);
    	    cdrOut.WriteUShort((ushort)1);
    	    cdrOut.WriteUShort((ushort)258);
    	    cdrOut.WriteUShort(UInt16.MaxValue);
    	    cdrOut.WriteUShort(UInt16.MinValue);
    	    stream.Seek(0, SeekOrigin.Begin);
    	    byte[] result = new byte[2];
    	    stream.Read(result, 0, 2);
    	    ArrayAssertion.AssertByteArrayEquals("converted wbe uint 16", new byte[] { 0, 1 }, result);
    	    stream.Read(result, 0, 2);    	        	        	        		
    		ArrayAssertion.AssertByteArrayEquals("converted wbe uint 16 (2)", new byte[] { 1, 2 }, result);
    	    stream.Read(result, 0, 2);    	        	        	        		
    		ArrayAssertion.AssertByteArrayEquals("converted wbe uint 16 (3)", new byte[] { 0xFF, 0xFF }, result);
    	    stream.Read(result, 0, 2);    	        	        	        		    		
    		ArrayAssertion.AssertByteArrayEquals("converted wbe uint 16 (4)", new byte[] { 0x00, 0x00 }, result);    		
    	}
    	
    	[Test]
    	public void TestUInt16WLESToW() {
    	    MemoryStream stream = new MemoryStream();
    	    CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_LITTLE_ENDIAN_FLAG);
    	    cdrOut.WriteUShort((ushort)1);
    	    cdrOut.WriteUShort((ushort)258);
    	    cdrOut.WriteUShort(UInt16.MaxValue);
    	    cdrOut.WriteUShort(UInt16.MinValue);
    	    stream.Seek(0, SeekOrigin.Begin);
    	    byte[] result = new byte[2];
    	    stream.Read(result, 0, 2);
    	    ArrayAssertion.AssertByteArrayEquals("converted lbe uint 16", new byte[] { 1, 0 }, result);
    	    stream.Read(result, 0, 2);    	        	        	        		
    		ArrayAssertion.AssertByteArrayEquals("converted lbe uint 16 (2)", new byte[] { 2, 1 }, result);
    	    stream.Read(result, 0, 2);    	        	        	        		
    		ArrayAssertion.AssertByteArrayEquals("converted lbe uint 16 (3)", new byte[] { 0xFF, 0xFF }, result);
    	    stream.Read(result, 0, 2);    	        	        	        		    		
    		ArrayAssertion.AssertByteArrayEquals("converted lbe uint 16 (4)", new byte[] { 0x00, 0x00 }, result);    	        
    	}    	
    	
    	[Test]
    	public void TestInt32WBEWToS() {
    	    MemoryStream stream = new MemoryStream(new byte[] { 0, 0, 0, 1,
    	                                                        0, 0, 1, 2,
    	                                                        0x7F, 0xFF, 0xFF, 0xFF,
    	                                                        0x80, 0x00, 0x00, 0x00 });
    	    CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
    	    cdrIn.ConfigStream(STREAM_BIG_ENDIAN_FLAG, new GiopVersion(1, 2));
    	    
    	    System.Int32 result = cdrIn.ReadLong();
    		Assertion.AssertEquals("converted wbe int 32", (int)1, result);
    		result = cdrIn.ReadLong();
    		Assertion.AssertEquals("converted wbe int 32 (2)", (int)258, result);
    		result = cdrIn.ReadLong();    		
    		Assertion.AssertEquals("converted wbe int 32 (3)", Int32.MaxValue, result);    		
    		result = cdrIn.ReadLong();
    		Assertion.AssertEquals("converted wbe int 32 (4)", Int32.MinValue, result);
    	}
    	
    	[Test]
    	public void TestInt32WLEWToS() {
    	    MemoryStream stream = new MemoryStream(new byte[] { 1, 0, 0, 0,
    	                                                        2, 1, 0, 0,
    	                                                        0xFF, 0xFF, 0xFF, 0x7F,
    	                                                        0x00, 0x00, 0x00, 0x80 });
    	    CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
    	    cdrIn.ConfigStream(STREAM_LITTLE_ENDIAN_FLAG, new GiopVersion(1, 2));
    	    
    	    System.Int32 result = cdrIn.ReadLong();
    		Assertion.AssertEquals("converted wbe int 32", (int)1, result);
    		result = cdrIn.ReadLong();			
			Assertion.AssertEquals("converted wbe int 32 (2)", (int)258, result);
            result = cdrIn.ReadLong();    		
    		Assertion.AssertEquals("converted wbe int 32 (3)", Int32.MaxValue, result);
            result = cdrIn.ReadLong();    		
    		Assertion.AssertEquals("converted wbe int 32 (4)", Int32.MinValue, result);
    	}
    	
    	[Test]
    	public void TestInt32WBESToW() {
    	    MemoryStream stream = new MemoryStream();
    	    CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_BIG_ENDIAN_FLAG);
    	    cdrOut.WriteLong((int)1);
    	    cdrOut.WriteLong((int)258);
    	    cdrOut.WriteLong(Int32.MaxValue);
    	    cdrOut.WriteLong(Int32.MinValue);
    	    stream.Seek(0, SeekOrigin.Begin);
    	    byte[] result = new byte[4];
    	    stream.Read(result, 0, 4);
    		ArrayAssertion.AssertByteArrayEquals("converted wbe int 32", new byte[] {  0, 0, 0, 1 }, result);
    		stream.Read(result, 0, 4);
    		ArrayAssertion.AssertByteArrayEquals("converted wbe int 32 (2)", new byte[] { 0, 0, 1, 2 }, result);
            stream.Read(result, 0, 4);
    		ArrayAssertion.AssertByteArrayEquals("converted wbe int 32 (3)", new byte[] { 0x7F, 0xFF, 0xFF, 0xFF }, result);
            stream.Read(result, 0, 4);
    		ArrayAssertion.AssertByteArrayEquals("converted wbe int 32 (4)", new byte[] { 0x80, 0x00, 0x00, 0x00 }, result);
    	}
    	
    	[Test]
    	public void TestInt32WLESToW() {
    	    MemoryStream stream = new MemoryStream();
    	    CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_LITTLE_ENDIAN_FLAG);
    	    cdrOut.WriteLong((int)1);
    	    cdrOut.WriteLong((int)258);
    	    cdrOut.WriteLong(Int32.MaxValue);
    	    cdrOut.WriteLong(Int32.MinValue);
    	    stream.Seek(0, SeekOrigin.Begin);
    	    byte[] result = new byte[4];
    	    stream.Read(result, 0, 4);
    		ArrayAssertion.AssertByteArrayEquals("converted lbe int 32", new byte[] { 1, 0, 0, 0 }, result);
            stream.Read(result, 0, 4);
    		ArrayAssertion.AssertByteArrayEquals("converted lbe int 32 (2)", new byte[] { 2, 1, 0, 0 }, result);
    	    stream.Read(result, 0, 4);
    		ArrayAssertion.AssertByteArrayEquals("converted lbe int 32 (3)", new byte[] { 0xFF, 0xFF, 0xFF, 0x7F }, result);
    	    stream.Read(result, 0, 4);
    		ArrayAssertion.AssertByteArrayEquals("converted lbe int 32 (4)", new byte[] { 0x00, 0x00, 0x00, 0x80 }, result);
    	}    	
    	
    	[Test]
    	public void TestUInt32WBEWToS() {
    	    MemoryStream stream = new MemoryStream(new byte[] { 0, 0, 0, 1,
    	                                                        0, 0, 1, 2,
    	                                                        0xFF, 0xFF, 0xFF, 0xFF,
    	                                                        0x00, 0x00, 0x00, 0x00 });
    	    CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
    	    cdrIn.ConfigStream(STREAM_BIG_ENDIAN_FLAG, new GiopVersion(1, 2));
    	    
    	    System.UInt32 result = cdrIn.ReadULong();
    		Assertion.AssertEquals("converted wbe uint 32", (uint)1, result);
    		result = cdrIn.ReadULong();
    		Assertion.AssertEquals("converted wbe uint 32 (2)", (uint)258, result);
    		result = cdrIn.ReadULong();    		
    		Assertion.AssertEquals("converted wbe uint 32 (3)", UInt32.MaxValue, result);    		
    		result = cdrIn.ReadULong();
    		Assertion.AssertEquals("converted wbe uint 32 (4)", UInt32.MinValue, result);
    	}

    	[Test]
    	public void TestUInt32WLEWToS() {
    	    MemoryStream stream = new MemoryStream(new byte[] { 1, 0, 0, 0,
    	                                                        2, 1, 0, 0,
    	                                                        0xFF, 0xFF, 0xFF, 0xFF,
    	                                                        0x00, 0x00, 0x00, 0x00 });
    	    CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
    	    cdrIn.ConfigStream(STREAM_LITTLE_ENDIAN_FLAG, new GiopVersion(1, 2));
    	    
    	    System.UInt32 result = cdrIn.ReadULong();
    		Assertion.AssertEquals("converted wbe uint 32", (uint)1, result);
    		result = cdrIn.ReadULong();			
			Assertion.AssertEquals("converted wbe uint 32 (2)", (uint)258, result);
            result = cdrIn.ReadULong();    		
    		Assertion.AssertEquals("converted wbe uint 32 (3)", UInt32.MaxValue, result);
            result = cdrIn.ReadULong();    		
    		Assertion.AssertEquals("converted wbe uint 32 (4)", UInt32.MinValue, result);
    	}

    	[Test]
    	public void TestUInt32WBESToW() {
    	    MemoryStream stream = new MemoryStream();
    	    CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_BIG_ENDIAN_FLAG);
    	    cdrOut.WriteULong((uint)1);
    	    cdrOut.WriteULong((uint)258);
    	    cdrOut.WriteULong(UInt32.MaxValue);
    	    cdrOut.WriteULong(UInt32.MinValue);
    	    stream.Seek(0, SeekOrigin.Begin);
    	    byte[] result = new byte[4];
    	    stream.Read(result, 0, 4);
    		ArrayAssertion.AssertByteArrayEquals("converted wbe uint 32", new byte[] {  0, 0, 0, 1 }, result);
    		stream.Read(result, 0, 4);
    		ArrayAssertion.AssertByteArrayEquals("converted wbe uint 32 (2)", new byte[] { 0, 0, 1, 2 }, result);
            stream.Read(result, 0, 4);
    		ArrayAssertion.AssertByteArrayEquals("converted wbe uint 32 (3)", new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, result);
            stream.Read(result, 0, 4);
    		ArrayAssertion.AssertByteArrayEquals("converted wbe uint 32 (4)", new byte[] { 0x00, 0x00, 0x00, 0x00 }, result);
    	}
    	
    	[Test]
    	public void TestUInt32WLESToW() {
    	    MemoryStream stream = new MemoryStream();
    	    CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_LITTLE_ENDIAN_FLAG);
    	    cdrOut.WriteULong((uint)1);
    	    cdrOut.WriteULong((uint)258);
    	    cdrOut.WriteULong(UInt32.MaxValue);
    	    cdrOut.WriteULong(UInt32.MinValue);
    	    stream.Seek(0, SeekOrigin.Begin);
    	    byte[] result = new byte[4];
    	    stream.Read(result, 0, 4);
    		ArrayAssertion.AssertByteArrayEquals("converted lbe uint 32", new byte[] { 1, 0, 0, 0 }, result);
            stream.Read(result, 0, 4);
    		ArrayAssertion.AssertByteArrayEquals("converted lbe uint 32 (2)", new byte[] { 2, 1, 0, 0 }, result);
    	    stream.Read(result, 0, 4);
    		ArrayAssertion.AssertByteArrayEquals("converted lbe uint 32 (3)", new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, result);
    	    stream.Read(result, 0, 4);
    		ArrayAssertion.AssertByteArrayEquals("converted lbe uint 32 (4)", new byte[] { 0x00, 0x00, 0x00, 0x00 }, result);
    	}    	
    	
    	[Test]
    	public void TestInt64WBEWToS() {
    	    MemoryStream stream = new MemoryStream(new byte[] { 0, 0, 0, 0, 0, 0, 0, 1,
    	                                                        0, 0, 0, 0, 0, 0, 1, 2,
    	                                                        0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
    	                                                        0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
    	    CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
    	    cdrIn.ConfigStream(STREAM_BIG_ENDIAN_FLAG, new GiopVersion(1, 2));
    	    System.Int64 result = cdrIn.ReadLongLong();
    	    Assertion.AssertEquals("converted wbe int 64", 1, result);
    		result = cdrIn.ReadLongLong();
    		Assertion.AssertEquals("converted wbe int 64 (2)", 258, result);
            result = cdrIn.ReadLongLong();    		
    		Assertion.AssertEquals("converted wbe int 64 (3)", Int64.MaxValue, result);
    		result = cdrIn.ReadLongLong();
    		Assertion.AssertEquals("converted wbe int 64 (4)", Int64.MinValue, result);
    	}
    	
    	[Test]
    	public void TestInt64WLEWToS() {
    	    MemoryStream stream = new MemoryStream(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0,
    	                                                        2, 1, 0, 0, 0, 0, 0, 0,
    	                                                        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F,
    	                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 });
    	    CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
    	    cdrIn.ConfigStream(STREAM_LITTLE_ENDIAN_FLAG, new GiopVersion(1, 2));
    	    
    	    System.Int64 result = cdrIn.ReadLongLong();
    		Assertion.AssertEquals("converted wbe int", (long)1, result);
    		result = cdrIn.ReadLongLong();			
			Assertion.AssertEquals("converted wbe int (2)", (long)258, result);
            result = cdrIn.ReadLongLong();    		
    		Assertion.AssertEquals("converted wbe int (3)", Int64.MaxValue, result);
            result = cdrIn.ReadLongLong();    		
    		Assertion.AssertEquals("converted wbe int (4)", Int64.MinValue, result);
    	}    	
    	
    	[Test]
    	public void TestInt64WBESToW() {
    	    MemoryStream stream = new MemoryStream();
    	    CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_BIG_ENDIAN_FLAG);
    	    cdrOut.WriteLongLong((long)1);
    	    cdrOut.WriteLongLong((long)258);
    	    cdrOut.WriteLongLong(Int64.MaxValue);
    	    cdrOut.WriteLongLong(Int64.MinValue);
    	    stream.Seek(0, SeekOrigin.Begin);
    	    byte[] result = new byte[8];
    	    stream.Read(result, 0, 8);
    		ArrayAssertion.AssertByteArrayEquals("converted wbe int", new byte[] {  0, 0, 0, 0, 0, 0, 0, 1 }, result);
    		stream.Read(result, 0, 8);
    		ArrayAssertion.AssertByteArrayEquals("converted wbe int (2)", new byte[] { 0, 0, 0, 0, 0, 0, 1, 2 }, result);
            stream.Read(result, 0, 8);
    		ArrayAssertion.AssertByteArrayEquals("converted wbe int (3)", new byte[] { 0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, result);
            stream.Read(result, 0, 8);
    		ArrayAssertion.AssertByteArrayEquals("converted wbe int (4)", new byte[] { 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, result);
    	}
    	
    	[Test]
    	public void TestInt64WLESToW() {
    	    MemoryStream stream = new MemoryStream();
    	    CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_LITTLE_ENDIAN_FLAG);
    	    cdrOut.WriteLongLong((long)1);
    	    cdrOut.WriteLongLong((long)258);
    	    cdrOut.WriteLongLong(Int64.MaxValue);
    	    cdrOut.WriteLongLong(Int64.MinValue);
    	    stream.Seek(0, SeekOrigin.Begin);
    	    byte[] result = new byte[8];
    	    stream.Read(result, 0, 8);
    		ArrayAssertion.AssertByteArrayEquals("converted lbe int64", new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, result);
            stream.Read(result, 0, 8);
    		ArrayAssertion.AssertByteArrayEquals("converted lbe int64 (2)", new byte[] { 2, 1, 0, 0, 0, 0, 0, 0 }, result);
    	    stream.Read(result, 0, 8);
    		ArrayAssertion.AssertByteArrayEquals("converted lbe int64 (3)", new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F }, result);
    	    stream.Read(result, 0, 8);
    		ArrayAssertion.AssertByteArrayEquals("converted lbe int64 (4)", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 }, result);
    	}    	
    	
    	[Test]
    	public void TestUInt64WBEWToS() {
    	    MemoryStream stream = new MemoryStream(new byte[] { 0, 0, 0, 0, 0, 0, 0, 1,
    	                                                        0, 0, 0, 0, 0, 0, 1, 2,
    	                                                        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
    	                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
    	    CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
    	    cdrIn.ConfigStream(STREAM_BIG_ENDIAN_FLAG, new GiopVersion(1, 2));
    	    System.UInt64 result = cdrIn.ReadULongLong();
    	    Assertion.AssertEquals("converted wbe uint 64", 1, result);
    		result = cdrIn.ReadULongLong();
    		Assertion.AssertEquals("converted wbe uint 64 (2)", 258, result);
            result = cdrIn.ReadULongLong();    		
    		Assertion.AssertEquals("converted wbe uint 64 (3)", UInt64.MaxValue, result);
    		result = cdrIn.ReadULongLong();
    		Assertion.AssertEquals("converted wbe uint 64 (4)", UInt64.MinValue, result);
    	}
    	
    	[Test]
    	public void TestUInt64WLEWToS() {
    	    MemoryStream stream = new MemoryStream(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0,
    	                                                        2, 1, 0, 0, 0, 0, 0, 0,
    	                                                        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
    	                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
    	    CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
    	    cdrIn.ConfigStream(STREAM_LITTLE_ENDIAN_FLAG, new GiopVersion(1, 2));
    	    
    	    System.UInt64 result = cdrIn.ReadULongLong();
    		Assertion.AssertEquals("converted wbe uint", (ulong)1, result);
    		result = cdrIn.ReadULongLong();			
			Assertion.AssertEquals("converted wbe uint (2)", (ulong)258, result);
            result = cdrIn.ReadULongLong();    		
    		Assertion.AssertEquals("converted wbe uint (3)", UInt64.MaxValue, result);
            result = cdrIn.ReadULongLong();    		
    		Assertion.AssertEquals("converted wbe uint (4)", UInt64.MinValue, result);    	        	    
    	}    	
    	
    	[Test]
    	public void TestUInt64WBESToW() {
    	    MemoryStream stream = new MemoryStream();
    	    CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_BIG_ENDIAN_FLAG);
    	    cdrOut.WriteULongLong((ulong)1);
    	    cdrOut.WriteULongLong((ulong)258);
    	    cdrOut.WriteULongLong(UInt64.MaxValue);
    	    cdrOut.WriteULongLong(UInt64.MinValue);
    	    stream.Seek(0, SeekOrigin.Begin);
    	    byte[] result = new byte[8];
    	    stream.Read(result, 0, 8);
    		ArrayAssertion.AssertByteArrayEquals("converted wbe uint", new byte[] {  0, 0, 0, 0, 0, 0, 0, 1 }, result);
    		stream.Read(result, 0, 8);
    		ArrayAssertion.AssertByteArrayEquals("converted wbe uint (2)", new byte[] { 0, 0, 0, 0, 0, 0, 1, 2 }, result);
            stream.Read(result, 0, 8);
    		ArrayAssertion.AssertByteArrayEquals("converted wbe uint (3)", new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, result);
            stream.Read(result, 0, 8);
    		ArrayAssertion.AssertByteArrayEquals("converted wbe uint (4)", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, result);    	        	    
    	}
    	
    	[Test]
    	public void TestUInt64WLESToW() {
    		MemoryStream stream = new MemoryStream();
    	    CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_LITTLE_ENDIAN_FLAG);
    	    cdrOut.WriteULongLong((ulong)1);
    	    cdrOut.WriteULongLong((ulong)258);
    	    cdrOut.WriteULongLong(UInt64.MaxValue);
    	    cdrOut.WriteULongLong(UInt64.MinValue);
    	    stream.Seek(0, SeekOrigin.Begin);
    	    byte[] result = new byte[8];
    	    stream.Read(result, 0, 8);
    		ArrayAssertion.AssertByteArrayEquals("converted lbe uint64", new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, result);
            stream.Read(result, 0, 8);
    		ArrayAssertion.AssertByteArrayEquals("converted lbe uint64 (2)", new byte[] { 2, 1, 0, 0, 0, 0, 0, 0 }, result);
    	    stream.Read(result, 0, 8);
    		ArrayAssertion.AssertByteArrayEquals("converted lbe uint64 (3)", new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, result);
    	    stream.Read(result, 0, 8);
    		ArrayAssertion.AssertByteArrayEquals("converted lbe uint64 (4)", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, result);
    		
    	}    	
    	
    	
    	[Test]
    	public void TestSingleWBEWToS() {
    	    MemoryStream stream = new MemoryStream(new byte[] { 0x3F, 0x80, 0x00, 0x00,
    	                                                        0x3C, 0x23, 0xD7, 0x0A,
    	                                                        0x7F, 0x7F, 0xFF, 0xFF,
    	                                                        0xFF, 0x7F, 0xFF, 0xFF });
    	    CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
    	    cdrIn.ConfigStream(STREAM_BIG_ENDIAN_FLAG, new GiopVersion(1, 2));
    	    
    	    System.Single result = cdrIn.ReadFloat();
    		Assertion.AssertEquals("converted wbe single", (float)1.0f, result);
    		result = cdrIn.ReadFloat();
    		Assertion.AssertEquals("converted wbe single (2)", (float)0.01f, result);
    		result = cdrIn.ReadFloat();    		
    		Assertion.AssertEquals("converted wbe single (3)", Single.MaxValue, result);    		
    		result = cdrIn.ReadFloat();
    		Assertion.AssertEquals("converted wbe single (4)", Single.MinValue, result);    		
    	}    	
    	
    	[Test]
    	public void TestSingleWLEWToS() {
    	    MemoryStream stream = new MemoryStream(new byte[] { 0x00, 0x00, 0x80, 0x3F,
    	                                                        0x0A, 0xD7, 0x23, 0x3C,
    	                                                        0xFF, 0xFF, 0x7F, 0x7F,
    	                                                        0xFF, 0xFF, 0x7F, 0xFF });
    	    CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
    	    cdrIn.ConfigStream(STREAM_LITTLE_ENDIAN_FLAG, new GiopVersion(1, 2));
    	    
    	    System.Single result = cdrIn.ReadFloat();
    		Assertion.AssertEquals("converted wbe single", (float)1.0f, result);
    		result = cdrIn.ReadFloat();			
			Assertion.AssertEquals("converted wbe single (2)", (float)0.01f, result);
            result = cdrIn.ReadFloat();    		
    		Assertion.AssertEquals("converted wbe single (3)", Single.MaxValue, result);
            result = cdrIn.ReadFloat();    		
    		Assertion.AssertEquals("converted wbe single (4)", Single.MinValue, result);
    	}    	    	    	    	
    	
    	[Test]
    	public void TestSingleWBESToW() {
    	    MemoryStream stream = new MemoryStream();
    	    CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_BIG_ENDIAN_FLAG);
    	    cdrOut.WriteFloat((float)1);
    	    cdrOut.WriteFloat((float)0.01);
    	    cdrOut.WriteFloat(Single.MaxValue);
    	    cdrOut.WriteFloat(Single.MinValue);
    	    stream.Seek(0, SeekOrigin.Begin);
    	    byte[] result = new byte[4];
    	    stream.Read(result, 0, 4);
    		ArrayAssertion.AssertByteArrayEquals("converted wbe single", new byte[] { 0x3F, 0x80, 0x00, 0x00 }, result);
    		stream.Read(result, 0, 4);
    		ArrayAssertion.AssertByteArrayEquals("converted wbe single (2)", new byte[] { 0x3C, 0x23, 0xD7, 0x0A }, result);
            stream.Read(result, 0, 4);
    		ArrayAssertion.AssertByteArrayEquals("converted wbe single (3)", new byte[] { 0x7F, 0x7F, 0xFF, 0xFF }, result);
            stream.Read(result, 0, 4);
    		ArrayAssertion.AssertByteArrayEquals("converted wbe single (4)", new byte[] { 0xFF, 0x7F, 0xFF, 0xFF }, result);
    	}
    	
    	[Test]
    	public void TestSingleWLESToW() {
    	    MemoryStream stream = new MemoryStream();
    	    CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_LITTLE_ENDIAN_FLAG);
    	    cdrOut.WriteFloat((float)1);
    	    cdrOut.WriteFloat((float)0.01);
    	    cdrOut.WriteFloat(Single.MaxValue);
    	    cdrOut.WriteFloat(Single.MinValue);
    	    stream.Seek(0, SeekOrigin.Begin);
    	    byte[] result = new byte[4];
    	    stream.Read(result, 0, 4);
    		ArrayAssertion.AssertByteArrayEquals("converted lbe single", new byte[] { 0x00, 0x00, 0x80, 0x3F }, result);
            stream.Read(result, 0, 4);
    		ArrayAssertion.AssertByteArrayEquals("converted lbe single (2)", new byte[] { 0x0A, 0xD7, 0x23, 0x3C }, result);
    	    stream.Read(result, 0, 4);
    		ArrayAssertion.AssertByteArrayEquals("converted lbe single (3)", new byte[] { 0xFF, 0xFF, 0x7F, 0x7F }, result);
    	    stream.Read(result, 0, 4);
    		ArrayAssertion.AssertByteArrayEquals("converted lbe single (4)", new byte[] { 0xFF, 0xFF, 0x7F, 0xFF }, result);
    	}    	
    	
    	[Test]
    	public void TestDoubleWBEWToS() {
    	    MemoryStream stream = new MemoryStream(new byte[] { 0x3F, 0xF0, 0, 0, 0, 0, 0, 0,
    	                                                        0x3F, 0x84, 0x7A, 0xE1, 0x47, 0xAE, 0x14, 0x7B,
    	                                                        0x7F, 0xEF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
    	                                                        0xFF, 0xEF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
    	    CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
    	    cdrIn.ConfigStream(STREAM_BIG_ENDIAN_FLAG, new GiopVersion(1, 2));
    	    double result = cdrIn.ReadDouble();
    	    Assertion.AssertEquals("converted wbe double", 1.0f, result);
    		result = cdrIn.ReadDouble();
    		Assertion.AssertEquals("converted wbe double (2)", 0.01f, result);
            result = cdrIn.ReadDouble();    		
    		Assertion.AssertEquals("converted wbe double (3)", Double.MaxValue, result);
    		result = cdrIn.ReadDouble();
    		Assertion.AssertEquals("converted wbe double (4)", Double.MinValue, result);
    	}    	
    	
    	[Test]
    	public void TestDoubleWLEWToS() {
    	    MemoryStream stream = new MemoryStream(new byte[] { 0, 0, 0, 0, 0, 0, 0xF0, 0x3F,
    	                                                        0x7B, 0x14, 0xAE, 0x47, 0xE1, 0x7A, 0x84, 0x3F,
    	                                                        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xEF, 0x7F,
    	                                                        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xEF, 0xFF });
    	    CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
    	    cdrIn.ConfigStream(STREAM_LITTLE_ENDIAN_FLAG, new GiopVersion(1, 2));
    	    double result = cdrIn.ReadDouble();
    		Assertion.AssertEquals("converted lbe double", 1.0f, result);
    		result = cdrIn.ReadDouble();
    		Assertion.AssertEquals("converted lbe double (2)", 0.01f, result);    		
    	    result = cdrIn.ReadDouble();    		
    		Assertion.AssertEquals("converted lbe double (3)", Double.MaxValue, result);
    	    result = cdrIn.ReadDouble();
    		Assertion.AssertEquals("converted lbe double (4)", Double.MinValue, result);
    	}    	    	
    	
    	[Test]
    	public void TestDoubleWBESToW() {
    	    MemoryStream stream = new MemoryStream();
    	    CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_BIG_ENDIAN_FLAG);
    	    cdrOut.WriteDouble((double)1);
    	    cdrOut.WriteDouble((double)0.01);
    	    cdrOut.WriteDouble(Double.MaxValue);
    	    cdrOut.WriteDouble(Double.MinValue);
    	    stream.Seek(0, SeekOrigin.Begin);
    	    byte[] result = new byte[8];
    	    stream.Read(result, 0, 8);
    		ArrayAssertion.AssertByteArrayEquals("converted wbe double", new byte[] { 0x3F, 0xF0, 0, 0, 0, 0, 0, 0 }, result);
            stream.Read(result, 0, 8);
    		ArrayAssertion.AssertByteArrayEquals("converted wbe double (2)", new byte[] { 0x3F, 0x84, 0x7A, 0xE1, 0x47, 0xAE, 0x14, 0x7B }, result);
            stream.Read(result, 0, 8);
    		ArrayAssertion.AssertByteArrayEquals("converted wbe double (3)", new byte[] { 0x7F, 0xEF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, result);
            stream.Read(result, 0, 8);
    		ArrayAssertion.AssertByteArrayEquals("converted wbe double (4)", new byte[] { 0xFF, 0xEF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, result);
    	}
    	
    	[Test]
    	public void TestDoubleWLESToW() {
    	    MemoryStream stream = new MemoryStream();
    	    CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_LITTLE_ENDIAN_FLAG);
    	    cdrOut.WriteDouble((double)1);
    	    cdrOut.WriteDouble((double)0.01);
    	    cdrOut.WriteDouble(Double.MaxValue);
    	    cdrOut.WriteDouble(Double.MinValue);
    	    stream.Seek(0, SeekOrigin.Begin);
    	    byte[] result = new byte[8];
    	    stream.Read(result, 0, 8);
    		ArrayAssertion.AssertByteArrayEquals("converted lbe double", new byte[] { 0, 0, 0, 0, 0, 0, 0xF0, 0x3F }, result);
    		stream.Read(result, 0, 8);
    		ArrayAssertion.AssertByteArrayEquals("converted lbe double (2)", new byte[] { 0x7B, 0x14, 0xAE, 0x47, 0xE1, 0x7A, 0x84, 0x3F }, result);
            stream.Read(result, 0, 8);
    		ArrayAssertion.AssertByteArrayEquals("converted lbe double (3)", new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xEF, 0x7F }, result);
            stream.Read(result, 0, 8);
    		ArrayAssertion.AssertByteArrayEquals("converted lbe double (4)", new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xEF, 0xFF }, result);
    	}    	    	
    	
    	
    	[Test]
    	public void TestWStringWBEWToS() {
    	    MemoryStream stream = new MemoryStream(new byte[] {0, 0, 0, 10, 0xFE, 0xFF, 0, 84, 0, 101, 0, 115, 0, 116 });
    	    CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
    	    cdrIn.ConfigStream(STREAM_BIG_ENDIAN_FLAG, new GiopVersion(1, 2));
    	    cdrIn.WCharSet = (int)WCharSet.UTF16;
    	    string result = cdrIn.ReadWString();
    	    Assertion.AssertEquals("converted wbe string", "Test", result);
    	}    	
    	
    	[Test]
    	public void TestWStringWLEWToS() {
    	    MemoryStream stream = new MemoryStream(new byte[] {10, 0, 0, 0, 0xFF, 0xFE, 84, 0, 101, 0, 115, 0, 116, 0 });
    	    CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
    	    cdrIn.ConfigStream(STREAM_LITTLE_ENDIAN_FLAG, new GiopVersion(1, 2));
    	    cdrIn.WCharSet = (int)WCharSet.UTF16;
    	    string result = cdrIn.ReadWString();
    		Assertion.AssertEquals("converted lbe string", "Test", result);
    	}    	    	
    	
    	[Test]
    	public void TestWStringWBESToW() {
    	    MemoryStream stream = new MemoryStream();
    	    CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_BIG_ENDIAN_FLAG);
    	    cdrOut.WCharSet = (int)WCharSet.UTF16;
    	    cdrOut.WriteWString("Test");
    	    stream.Seek(0, SeekOrigin.Begin);
    	    byte[] result = new byte[12];
    	    stream.Read(result, 0, 12);
    		ArrayAssertion.AssertByteArrayEquals("converted wbe string", new byte[] { 0, 0, 0, 8, 0, 84, 0, 101, 0, 115, 0, 116 }, result);
    	}
    	
    	[Test]
    	public void TestWStringWLESToW() {
    	    MemoryStream stream = new MemoryStream();
    	    CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_LITTLE_ENDIAN_FLAG);
    	    cdrOut.WCharSet = (int)WCharSet.UTF16;
    	    cdrOut.WriteWString("Test");
    	    stream.Seek(0, SeekOrigin.Begin);
    	    byte[] result = new byte[14];
    	    stream.Read(result, 0, 14);
    		ArrayAssertion.AssertByteArrayEquals("converted lbe string", new byte[] { 10, 0, 0, 0, 0xFF, 0xFE, 84, 0, 101, 0, 115, 0, 116, 0 }, result);
    	}    	    	

    	
    }
}

#endif
