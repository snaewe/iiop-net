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

        #endregion IFields
        #region IConstructors

        public CdrStreamBigEndianReadOP(CdrInputStream stream, GiopVersion version) {
            m_stream = stream;
            m_version = version;
        }

        #endregion IConstructors
        #region IMethods
        
        #region read methods depenant on byte ordering
        public short ReadShort() {
            // in stream: two-complement representation
            ushort numberTwoCRepr = ReadUShort();

            byte msbit = (byte)((numberTwoCRepr & 0x8000) >> 15);

            short result;
            if (msbit == 1) {
                // a negative number
                ushort invtwoCNumber = (ushort)(numberTwoCRepr ^ 0xFFFF);
                result = (short)((invtwoCNumber + 1) * -1);
            } else {
                // a positive number
                result = (short)(numberTwoCRepr);
            }
            return result;
        }

        public ushort ReadUShort() {
            m_stream.ForceReadAlign(Aligns.Align2);
            return (ushort) ((m_stream.ReadOctet() << 8) | m_stream.ReadOctet());
        }

        public int ReadLong() {
            // in stream: two-complement representation
            uint numberTwoCRepr = ReadULong();

            byte msbit = (byte)((numberTwoCRepr & 0x80000000) >> 31);

            int result;
            if (msbit == 1) {
                // a negative number
                uint invtwoCNumber = (uint)(numberTwoCRepr ^ 0xFFFFFFFF);
                result = (int)((invtwoCNumber + 1) * -1);
            } else {
                // a positive number
                result = (int)(numberTwoCRepr);
            }
            return result;
        }

        public uint ReadULong() {
            m_stream.ForceReadAlign(Aligns.Align4);
            return (
                (((uint)m_stream.ReadOctet()) << 24) | (((uint)m_stream.ReadOctet()) << 16) | 
                (((uint)m_stream.ReadOctet()) << 8) | ((uint)m_stream.ReadOctet())
            );
        }

        public long ReadLongLong() {
            // in stream: two-complement representation
            ulong numberTwoCRepr = ReadULongLong();

            byte msbit = (byte)((numberTwoCRepr & 0x8000000000000000) >> 63);
            long result;
            if (msbit == 1) {
                // a negative number
                ulong invtwoCNumber = (ulong)(numberTwoCRepr ^ 0xFFFFFFFFFFFFFFFF);
                result = (long)(0 - (invtwoCNumber + 1));
            } else {
                // a positive number
                result = (long)(numberTwoCRepr);
            }
            return result;
        }

        public ulong ReadULongLong() {
            m_stream.ForceReadAlign(Aligns.Align8);
            return (ulong)(
                (((ulong)m_stream.ReadOctet()) << 56) | (((ulong)m_stream.ReadOctet()) << 48)  | 
                (((ulong)m_stream.ReadOctet()) << 40) | (((ulong)m_stream.ReadOctet()) << 32) |                 
                (((ulong)m_stream.ReadOctet()) << 24) | (((ulong)m_stream.ReadOctet()) << 16) | 
                (((ulong)m_stream.ReadOctet()) << 8) | ((ulong)m_stream.ReadOctet()) 
            );

        }

        public float ReadFloat() {
            m_stream.ForceReadAlign(Aligns.Align4);
            byte[] data = m_stream.ReadOpaque(4);
            Array.Reverse((Array)data); // BitConverter wants little endian
            float result = BitConverter.ToSingle(data, 0);
            return result;
        }

        public double ReadDouble() {
            m_stream.ForceReadAlign(Aligns.Align8);
            byte[] data = m_stream.ReadOpaque(8);
            // BitConverter takes an 8 byte array, containing the litte endian representation of the double
            Array.Reverse((Array)data);
            double result = BitConverter.ToDouble(data, 0);
            return result;
        }
        
        private byte[] AppendChar(byte[] oldData) {
            byte[] newData = new byte[oldData.Length+1];
            Array.Copy(oldData, 0, newData, 0, oldData.Length);
            newData[oldData.Length] = m_stream.ReadOctet();
            return newData;
        }
        
        public char ReadWChar() {
            Encoding encoding = CdrStreamHelper.GetWCharEncoding(m_stream.WCharSet, 
                                    CodeSetConversionRegistryBigEndian.GetRegistry());
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
            Encoding encoding = CdrStreamHelper.GetWCharEncoding(m_stream.WCharSet,
                                    CodeSetConversionRegistryBigEndian.GetRegistry());
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
        
        public void WriteShort(short data) {
            m_stream.ForceWriteAlign(Aligns.Align2);
            if (data < 0) {
                // calculate two complement
                ushort positiveNumber = (ushort)(data * -1);
                ushort invNumber = (ushort)(positiveNumber ^ 0xFFFF);
                WriteUShort((ushort)(invNumber + 1));
            } else {
                WriteUShort((ushort)data);
            }
        }

        public void WriteUShort(ushort data) {
            m_stream.ForceWriteAlign(Aligns.Align2);
            m_stream.WriteOctet((byte) ((data & 0xFF00) >> 8));
            m_stream.WriteOctet((byte) ( data & 0x00FF));
        }

        public void WriteLong(int data) {
            m_stream.ForceWriteAlign(Aligns.Align4);
            if (data < 0) {
                // calculate two complement
                uint positiveNumber = (uint)(data * -1);
                uint invNumber = (uint)(positiveNumber ^ 0xFFFFFFFF);
                WriteULong((uint)(invNumber + 1));
            } else {
                WriteULong((uint)data);
            }
        }

        public void WriteULong(uint data) {
            m_stream.ForceWriteAlign(Aligns.Align4);
            m_stream.WriteOctet((byte) ((data & 0xFF000000) >> 24));
            m_stream.WriteOctet((byte) ((data & 0x00FF0000) >> 16));            
            m_stream.WriteOctet((byte) ((data & 0x0000FF00) >> 8));
            m_stream.WriteOctet((byte) ( data & 0x000000FF));
        }

        public void WriteLongLong(long data) {
            m_stream.ForceWriteAlign(Aligns.Align8);
            if (data < 0) {
                // calculate two complement
                ulong positiveNumber = (ulong)(data * -1);
                ulong invNumber = (ulong)(positiveNumber ^ 0xFFFFFFFFFFFFFFFF);
                WriteULongLong((ulong)(invNumber + 1));
            } else {
                WriteULongLong((ulong)data);
            }
        }

        public void WriteULongLong(ulong data) {
            m_stream.ForceWriteAlign(Aligns.Align8);
            m_stream.WriteOctet((byte) ((data & 0xFF00000000000000) >> 56));
            m_stream.WriteOctet((byte) ((data & 0x00FF000000000000) >> 48));
            m_stream.WriteOctet((byte) ((data & 0x0000FF0000000000) >> 40));
            m_stream.WriteOctet((byte) ((data & 0x000000FF00000000) >> 32));
            m_stream.WriteOctet((byte) ((data & 0x00000000FF000000) >> 24));
            m_stream.WriteOctet((byte) ((data & 0x0000000000FF0000) >> 16));            
            m_stream.WriteOctet((byte) ((data & 0x000000000000FF00) >> 8));
            m_stream.WriteOctet((byte) ( data & 0x00000000000000FF));
        }

        public void WriteFloat(float data) {
            m_stream.ForceWriteAlign(Aligns.Align4);
            byte[] byteRep = BitConverter.GetBytes(data);
            Array.Reverse((Array)byteRep);
            m_stream.WriteOpaque(byteRep);
        }

        public void WriteDouble(double data) {
            m_stream.ForceWriteAlign(Aligns.Align8);
            byte[] byteRep = BitConverter.GetBytes(data); // create the little endian representation of the double
            Array.Reverse((Array)byteRep);
            m_stream.WriteOpaque(byteRep);
        }

        public void WriteWChar(char data) {
            Encoding encoding = CdrStreamHelper.GetWCharEncoding(m_stream.WCharSet,
                                                                 CodeSetConversionRegistryBigEndian.GetRegistry());
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
            Encoding encoding = CdrStreamHelper.GetWCharEncoding(m_stream.WCharSet,
                                                                 CodeSetConversionRegistryBigEndian.GetRegistry());
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

        #endregion IFields
        #region IConstructors

        public CdrStreamLittleEndianReadOP(CdrInputStream stream, GiopVersion version) {
            m_stream = stream;
            m_version = version;
        }

        #endregion IConstructors
        #region IMethods
        
        #region read methods depenant on byte ordering
        public short ReadShort() {
            // in stream: two-complement representation
            ushort numberTwoCRepr = ReadUShort();

            byte msbit = (byte)((numberTwoCRepr & 0x8000) >> 15);

            short result;
            if (msbit == 1) {
                // a negative number
                ushort invtwoCNumber = (ushort)(numberTwoCRepr ^ 0xFFFF);
                result = (short)((invtwoCNumber + 1) * -1);
            } else {
                // a positive number
                result = (short)(numberTwoCRepr);
            }
            return result;
        }

        public ushort ReadUShort() {
            m_stream.ForceReadAlign(Aligns.Align2);
            return (ushort) (m_stream.ReadOctet() | (m_stream.ReadOctet() << 8));
        }

        public int ReadLong() {
            // in stream: two-complement representation
            uint numberTwoCRepr = ReadULong();

            byte msbit = (byte)((numberTwoCRepr & 0x80000000) >> 31);

            int result;
            if (msbit == 1) {
                // a negative number
                uint invtwoCNumber = (uint)(numberTwoCRepr ^ 0xFFFFFFFF);
                result = (int)((invtwoCNumber + 1) * -1);
            } else {
                // a positive number
                result = (int)(numberTwoCRepr);
            }
            return result;
        }

        public uint ReadULong() {
            m_stream.ForceReadAlign(Aligns.Align4);
            return (
                ((uint)m_stream.ReadOctet()) | (((uint)m_stream.ReadOctet()) << 8) | 
                (((uint)m_stream.ReadOctet()) << 16) | (((uint)m_stream.ReadOctet()) << 24)
            );
        }

        public long ReadLongLong() {
            // in stream: two-complement representation
            ulong numberTwoCRepr = ReadULongLong();

            byte msbit = (byte)((numberTwoCRepr & 0x8000000000000000) >> 63);
            long result;
            if (msbit == 1) {
                // a negative number
                ulong invtwoCNumber = (ulong)(numberTwoCRepr ^ 0xFFFFFFFFFFFFFFFF);
                result = (long)(0 - (invtwoCNumber + 1));
            } else {
                // a positive number
                result = (long)(numberTwoCRepr);
            }
            return result;
        }

        public ulong ReadULongLong() {
            m_stream.ForceReadAlign(Aligns.Align8);
            return (ulong)(
                ((ulong)m_stream.ReadOctet()) | (((ulong)m_stream.ReadOctet()) << 8)  | 
                (((ulong)m_stream.ReadOctet()) << 16) | (((ulong)m_stream.ReadOctet()) << 24) |                 
                (((ulong)m_stream.ReadOctet()) << 32) | (((ulong)m_stream.ReadOctet()) << 40) | 
                (((ulong)m_stream.ReadOctet()) << 48) | (((ulong)m_stream.ReadOctet()) << 56)
            );

        }

        public float ReadFloat() {
            m_stream.ForceReadAlign(Aligns.Align4);
            byte[] data = m_stream.ReadOpaque(4);
            // BitConverter wants little endian
            float result = BitConverter.ToSingle(data, 0);
            return result;
        }

        public double ReadDouble() {
            m_stream.ForceReadAlign(Aligns.Align8);
            byte[] data = m_stream.ReadOpaque(8);
            // BitConverter takes an 8 byte array, containing the litte endian representation of the double            
            double result = BitConverter.ToDouble(data, 0);
            return result;
        }
        
        private byte[] AppendChar(byte[] oldData) {
            byte[] newData = new byte[oldData.Length+1];
            Array.Copy(oldData, 0, newData, 0, oldData.Length);
            newData[oldData.Length] = m_stream.ReadOctet();
            return newData;
        }
        
        public char ReadWChar() {
            Encoding encoding = CdrStreamHelper.GetWCharEncoding(m_stream.WCharSet, 
                                    CodeSetConversionRegistryLittleEndian.GetRegistry());
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
            Encoding encoding = CdrStreamHelper.GetWCharEncoding(m_stream.WCharSet,
                                    CodeSetConversionRegistryLittleEndian.GetRegistry());
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
        
        public void WriteShort(short data) {
            m_stream.ForceWriteAlign(Aligns.Align2);
            if (data < 0) {
                // calculate two complement
                ushort positiveNumber = (ushort)(data * -1);
                ushort invNumber = (ushort)(positiveNumber ^ 0xFFFF);
                WriteUShort((ushort)(invNumber + 1));
            } else {
                WriteUShort((ushort)data);
            }
        }

        public void WriteUShort(ushort data) {
            m_stream.ForceWriteAlign(Aligns.Align2);
            m_stream.WriteOctet((byte) ( data & 0x00FF));
            m_stream.WriteOctet((byte) ((data & 0xFF00) >> 8));
        }

        public void WriteLong(int data) {
            m_stream.ForceWriteAlign(Aligns.Align4);
            if (data < 0) {
                // calculate two complement
                uint positiveNumber = (uint)(data * -1);
                uint invNumber = (uint)(positiveNumber ^ 0xFFFFFFFF);
                WriteULong((uint)(invNumber + 1));
            } else {
                WriteULong((uint)data);
            }
        }

        public void WriteULong(uint data) {
            m_stream.ForceWriteAlign(Aligns.Align4);                        
            m_stream.WriteOctet((byte) ( data & 0x000000FF));
            m_stream.WriteOctet((byte) ((data & 0x0000FF00) >> 8));
            m_stream.WriteOctet((byte) ((data & 0x00FF0000) >> 16));
            m_stream.WriteOctet((byte) ((data & 0xFF000000) >> 24));
        }

        public void WriteLongLong(long data) {
            m_stream.ForceWriteAlign(Aligns.Align8);
            if (data < 0) {
                // calculate two complement
                ulong positiveNumber = (ulong)(data * -1);
                ulong invNumber = (ulong)(positiveNumber ^ 0xFFFFFFFFFFFFFFFF);
                WriteULongLong((ulong)(invNumber + 1));
            } else {
                WriteULongLong((ulong)data);
            }
        }

        public void WriteULongLong(ulong data) {
            m_stream.ForceWriteAlign(Aligns.Align8);
            m_stream.WriteOctet((byte) ( data & 0x00000000000000FF));
            m_stream.WriteOctet((byte) ((data & 0x000000000000FF00) >> 8));
            m_stream.WriteOctet((byte) ((data & 0x0000000000FF0000) >> 16));            
            m_stream.WriteOctet((byte) ((data & 0x00000000FF000000) >> 24));
            m_stream.WriteOctet((byte) ((data & 0x000000FF00000000) >> 32));
            m_stream.WriteOctet((byte) ((data & 0x0000FF0000000000) >> 40));
            m_stream.WriteOctet((byte) ((data & 0x00FF000000000000) >> 48));
            m_stream.WriteOctet((byte) ((data & 0xFF00000000000000) >> 56));
        }

        public void WriteFloat(float data) {
            m_stream.ForceWriteAlign(Aligns.Align4);
            byte[] byteRep = BitConverter.GetBytes(data);
            // byteRep is in little endian order
            m_stream.WriteOpaque(byteRep);
        }

        public void WriteDouble(double data) {
            m_stream.ForceWriteAlign(Aligns.Align8);
            byte[] byteRep = BitConverter.GetBytes(data); // create the little endian representation of the double
            // byteRep is in little endian order
            m_stream.WriteOpaque(byteRep);
        }

        public void WriteWChar(char data) {
            Encoding encoding = CdrStreamHelper.GetWCharEncoding(m_stream.WCharSet,
                                                                 CodeSetConversionRegistryLittleEndian.GetRegistry());
            byte[] toSend = encoding.GetBytes(new char[] { data } );
            if (!((m_version.Major == 1) && (m_version.Minor <= 1))) { // GIOP 1.2
                m_stream.WriteOctet((byte)toSend.Length);
            }
            m_stream.WriteOpaque(toSend);
        }

        public void WriteWString(string data) {
            Encoding encoding = CdrStreamHelper.GetWCharEncoding(m_stream.WCharSet,
                                                                 CodeSetConversionRegistryLittleEndian.GetRegistry());
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