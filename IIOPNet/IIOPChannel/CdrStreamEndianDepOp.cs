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
    /// this is a big-endian implementation for the endian dependent operation for CDRInput-streams
    /// </summary>
    /// <remarks>
    /// this class is not intended for use by CDRStream users
    /// </remarks>
    internal class CdrStreamBigEndianReadOP : CdrEndianDepInputStreamOp {
        
        #region IFields

        private CdrInputStream m_stream;
        private GiopVersion m_version;
        private byte[] m_buf = new byte[8];

        #endregion IFields
        #region IConstructors

        public CdrStreamBigEndianReadOP(CdrInputStream stream, GiopVersion version) {
            m_stream = stream;
            m_version = version;
        }

        #endregion IConstructors
        #region IMethods
        
        #region read methods depenant on byte ordering

        private void Read(int size, Aligns align) {
            m_stream.ForceReadAlign(align);
            m_stream.ReadBytes(m_buf, 0, size);
            if (BitConverter.IsLittleEndian) { // need to reverse, because BitConverter uses other endian
                Array.Reverse(m_buf, 0, size);
            }
        }

        public short ReadShort() {
            Read(2, Aligns.Align2);
            return BitConverter.ToInt16(m_buf, 0);
        }

        public ushort ReadUShort() {
            Read(2, Aligns.Align2);
            return BitConverter.ToUInt16(m_buf, 0);
        }

        public int ReadLong() {
            Read(4, Aligns.Align4);
            return BitConverter.ToInt32(m_buf, 0);
        }

        public uint ReadULong() {
            Read(4, Aligns.Align4);
            return BitConverter.ToUInt32(m_buf, 0);
        }

        public long ReadLongLong() {
            Read(8, Aligns.Align8);
            return BitConverter.ToInt64(m_buf, 0);
        }

        public ulong ReadULongLong() {
            Read(8, Aligns.Align8);
            return BitConverter.ToUInt64(m_buf, 0);
        }

        public float ReadFloat() {
            Read(4, Aligns.Align4);
            float result = BitConverter.ToSingle(m_buf, 0);
            return result;
        }

        public double ReadDouble() {
            Read(8, Aligns.Align8);
            double result = BitConverter.ToDouble(m_buf, 0);
            return result;
        }
        
        private byte[] AppendChar(byte[] oldData) {
            byte[] newData = new byte[oldData.Length+1];
            Array.Copy(oldData, 0, newData, 0, oldData.Length);
            newData[oldData.Length] = m_stream.ReadOctet();
            return newData;
        }
        
        public char ReadWChar() {
            Encoding encoding = CodeSetService.GetCharEncodingBigEndian(m_stream.WCharSet, true);
            if (encoding == null) {
                throw new INTERNAL(987, CompletionStatus.Completed_MayBe);
            }
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

        public string ReadWString()    {
            Encoding encoding = CodeSetService.GetCharEncodingBigEndian(m_stream.WCharSet, true);
            if (encoding == null) {
                throw new INTERNAL(987, CompletionStatus.Completed_MayBe);
            }
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
    /// this is a big-endian implementation for the endian dependent operation for CDROutput-streams
    /// </summary>
    /// <remarks>
    /// this class is not intended for use by CDRStream users
    /// </remarks>
    internal class CdrStreamBigEndianWriteOP : CdrEndianDepOutputStreamOp {

        #region IFields

        private CdrOutputStream m_stream;
        private GiopVersion m_version;

        #endregion IFields
        #region IConstructors
        
        public CdrStreamBigEndianWriteOP(CdrOutputStream stream, GiopVersion version) {
            m_stream = stream;
            m_version = version;
        }

        #endregion IConstructors
        #region IMethods
        
        #region write methods dependant on byte ordering

    	private void Write(byte[] data, int count, Aligns align) {
	        m_stream.ForceWriteAlign(align);
            if (BitConverter.IsLittleEndian) { // need to reverse, because BitConverter uses other endian
    	        Array.Reverse(data, 0, count);
	        }
		    m_stream.WriteBytes(data, 0, count);
	    }
        
        public void WriteShort(short data) {
		    Write(BitConverter.GetBytes(data), 2, Aligns.Align2);
        }

        public void WriteUShort(ushort data) {
		    Write(BitConverter.GetBytes(data), 2, Aligns.Align2);
        }

        public void WriteLong(int data) {
		    Write(BitConverter.GetBytes(data), 4, Aligns.Align4);
        }

        public void WriteULong(uint data) {
		    Write(BitConverter.GetBytes(data), 4, Aligns.Align4);
        }

        public void WriteLongLong(long data) {
		    Write(BitConverter.GetBytes(data), 8, Aligns.Align8);
        }

        public void WriteULongLong(ulong data) {
		    Write(BitConverter.GetBytes(data), 8, Aligns.Align8);
        }

        public void WriteFloat(float data) {
		    Write(BitConverter.GetBytes(data), 4, Aligns.Align4);
        }

        public void WriteDouble(double data) {
		    Write(BitConverter.GetBytes(data), 8, Aligns.Align8);
        }

        public void WriteWChar(char data) {
            Encoding encoding = CodeSetService.GetCharEncodingBigEndian(m_stream.WCharSet, true);
            if (encoding == null) {
                throw new INTERNAL(987, CompletionStatus.Completed_MayBe);
            }            
            byte[] toSend = encoding.GetBytes(new char[] { data } );
            if (!((m_version.Major == 1) && (m_version.Minor <= 1))) { // GIOP 1.2
                m_stream.WriteOctet((byte)toSend.Length);
            }
            m_stream.WriteOpaque(toSend);
        }

        public void WriteWString(string data) {
            Encoding encoding = CodeSetService.GetCharEncodingBigEndian(m_stream.WCharSet, true);
            if (encoding == null) {
                throw new INTERNAL(987, CompletionStatus.Completed_MayBe);
            }
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
    /// this is a little-endian implementation for the endian dependent operation for CDRInput-streams
    /// </summary>
    /// <remarks>
    /// this class is not intended for use by CDRStream users
    /// </remarks>
    internal class CdrStreamLittleEndianReadOP : CdrEndianDepInputStreamOp {
        
        #region IFields

        private CdrInputStream m_stream;
        private GiopVersion m_version;
        private byte[] m_buf = new byte[8];

        #endregion IFields
        #region IConstructors

        public CdrStreamLittleEndianReadOP(CdrInputStream stream, GiopVersion version) {
            m_stream = stream;
            m_version = version;
        }

        #endregion IConstructors
        #region IMethods
        
        #region read methods depenant on byte ordering

        private void Read(int size, Aligns align) {
            m_stream.ForceReadAlign(align);
            m_stream.ReadBytes(m_buf, 0, size);
            if (!BitConverter.IsLittleEndian) { // need to reverse, because BitConverter uses other endian
                Array.Reverse(m_buf, 0, size);
            }            
        }

        public short ReadShort() {
            Read(2, Aligns.Align2);
            return BitConverter.ToInt16(m_buf, 0);
        }

        public ushort ReadUShort() {
            Read(2, Aligns.Align2);
            return BitConverter.ToUInt16(m_buf, 0);
        }

        public int ReadLong() {
            Read(4, Aligns.Align4);
            return BitConverter.ToInt32(m_buf, 0);
        }

        public uint ReadULong() {
            Read(4, Aligns.Align4);
            return BitConverter.ToUInt32(m_buf, 0);
        }

        public long ReadLongLong() {
            Read(8, Aligns.Align8);
            return BitConverter.ToInt64(m_buf, 0);
        }

        public ulong ReadULongLong() {
            Read(8, Aligns.Align8);
            return BitConverter.ToUInt64(m_buf, 0);
        }

        public float ReadFloat() {
            Read(4, Aligns.Align4);
            float result = BitConverter.ToSingle(m_buf, 0);
            return result;
        }

        public double ReadDouble() {
            Read(8, Aligns.Align8);
            double result = BitConverter.ToDouble(m_buf, 0);
            return result;
        }
        
        private byte[] AppendChar(byte[] oldData) {
            byte[] newData = new byte[oldData.Length+1];
            Array.Copy(oldData, 0, newData, 0, oldData.Length);
            newData[oldData.Length] = m_stream.ReadOctet();
            return newData;
        }
        
        public char ReadWChar() {
            Encoding encoding = CodeSetService.GetCharEncodingLittleEndian(m_stream.WCharSet, true);
            if (encoding == null) {
                throw new INTERNAL(987, CompletionStatus.Completed_MayBe);
            }

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

        public string ReadWString()    {
            Encoding encoding = CodeSetService.GetCharEncodingLittleEndian(m_stream.WCharSet, true);
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
    /// this is a little-endian implementation for the endian dependent operation for CDROutput-streams
    /// </summary>
    /// <remarks>
    /// this class is not intended for use by CDRStream users
    /// </remarks>
    internal class CdrStreamLittleEndianWriteOP : CdrEndianDepOutputStreamOp {

        #region IFields

        private CdrOutputStream m_stream;
        private GiopVersion m_version;

        #endregion IFields
        #region IConstructors
        
        public CdrStreamLittleEndianWriteOP(CdrOutputStream stream, GiopVersion version) {
            m_stream = stream;
            m_version = version;
        }

        #endregion IConstructors
        #region IMethods
        
        #region write methods dependant on byte ordering

       	private void Write(byte[] data, int count, Aligns align) {
		    m_stream.ForceWriteAlign(align);
            if (!BitConverter.IsLittleEndian) { // need to reverse, because BitConverter uses other endian
    	        Array.Reverse(data, 0, count);
	        }
		    m_stream.WriteBytes(data, 0, count);
	    }
        
        public void WriteShort(short data) {
		    Write(BitConverter.GetBytes(data), 2, Aligns.Align2);
        }

        public void WriteUShort(ushort data) {
		    Write(BitConverter.GetBytes(data), 2, Aligns.Align2);
        }

        public void WriteLong(int data) {
		    Write(BitConverter.GetBytes(data), 4, Aligns.Align4);
        }

        public void WriteULong(uint data) {
		    Write(BitConverter.GetBytes(data), 4, Aligns.Align4);
        }

        public void WriteLongLong(long data) {
		    Write(BitConverter.GetBytes(data), 8, Aligns.Align8);
        }

        public void WriteULongLong(ulong data) {
		    Write(BitConverter.GetBytes(data), 8, Aligns.Align8);
        }

        public void WriteFloat(float data) {
		    Write(BitConverter.GetBytes(data), 4, Aligns.Align4);
        }

        public void WriteDouble(double data) {
		    Write(BitConverter.GetBytes(data), 8, Aligns.Align8);
        }

        public void WriteWChar(char data) {
            Encoding encoding = CodeSetService.GetCharEncodingLittleEndian(m_stream.WCharSet, true);
            byte[] toSend = encoding.GetBytes(new char[] { data } );
            if (!((m_version.Major == 1) && (m_version.Minor <= 1))) { // GIOP 1.2
                m_stream.WriteOctet((byte)toSend.Length);
            }
            m_stream.WriteOpaque(toSend);
        }

        public void WriteWString(string data) {
            Encoding encoding = CodeSetService.GetCharEncodingLittleEndian(m_stream.WCharSet, true);
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
        
        public ushort ReadUShort() {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        public int ReadLong() {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        public uint ReadULong() {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);        
        }

        public long ReadLongLong() {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        public ulong ReadULongLong() {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        public float ReadFloat() {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        public double ReadDouble() {
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

        public void WriteUShort(ushort data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        public void WriteLong(int data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        public void WriteULong(uint data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        public void WriteLongLong(long data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        public void WriteULongLong(ulong data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        public void WriteFloat(float data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        public void WriteDouble(double data) {
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
