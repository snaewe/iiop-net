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
using System.Runtime.InteropServices;

namespace Ch.Elca.Iiop.Cdr
{

    /// <summary>
    /// this is the endian implementation for the endian dependent operation for CDRInput-streams
    /// to use, when the stream endian is different than the plattform endian (see BitConverter.IsLittleEndian)
    /// </summary>
    /// <remarks>
    /// this class is not intended for use by CDRStream users
    /// </remarks>
    internal sealed class CdrStreamNonNativeEndianReadOP : CdrEndianDepInputStreamOp {

        #region IFields

        private CdrInputStream m_stream;
        private GiopVersion m_version;
        private byte[] m_buf = new byte[8];

        #endregion IFields
        #region IConstructors

        public CdrStreamNonNativeEndianReadOP(CdrInputStream stream, GiopVersion version) {
            m_stream = stream;
            m_version = version;
        }

        #endregion IConstructors
        #region IMethods

        private void Read(int size, Aligns align) {
            m_stream.ForceReadAlign(align);
            m_stream.ReadBytes(m_buf, 0, size);
        }

        public short ReadShort() {
            Read(2, Aligns.Align2);
            return BitConverterUtils.Reverse(BitConverter.ToInt16(m_buf, 0));
        }

        public ushort ReadUShort() {
            Read(2, Aligns.Align2);
            return BitConverterUtils.Reverse(BitConverter.ToUInt16(m_buf, 0));
        }

        public int ReadLong() {
            Read(4, Aligns.Align4);
            return BitConverterUtils.Reverse(BitConverter.ToInt32(m_buf, 0));
        }

        public uint ReadULong() {
            Read(4, Aligns.Align4);
            return BitConverterUtils.Reverse(BitConverter.ToUInt32(m_buf, 0));
        }

        public long ReadLongLong() {
            Read(8, Aligns.Align8);
            return BitConverterUtils.Reverse(BitConverter.ToInt64(m_buf, 0));
        }

        public ulong ReadULongLong() {
            Read(8, Aligns.Align8);
            return BitConverterUtils.Reverse(BitConverter.ToUInt64(m_buf, 0));
        }

        public float ReadFloat() {
            Read(4, Aligns.Align4);
            return BitConverterUtils.Reverse(BitConverter.ToSingle(m_buf, 0));
        }

        public double ReadDouble() {
            Read(8, Aligns.Align8);
            return BitConverterUtils.Reverse(BitConverter.ToDouble(m_buf, 0));
        }

        public T[] ReadPrimitiveTypeArray<T>(int elemCount) {
            int elemSize = Marshal.SizeOf(typeof(T));
            if(elemCount > 0)
                m_stream.ForceReadAlign((Aligns)elemSize);
            if(m_buf.Length < elemSize * elemCount)
                m_buf = new byte[elemSize * elemCount * 2];
            m_stream.ReadBytes(m_buf, 0, elemSize * elemCount);
            return BitConverterUtils.BytesToArrayReverse<T>(elemCount, m_buf);
        }

        private byte[] AppendChar(byte[] oldData) {
            byte[] newData = new byte[oldData.Length + 1];
            Array.Copy(oldData, 0, newData, 0, oldData.Length);
            newData[oldData.Length] = m_stream.ReadOctet();
            return newData;
        }

        public char ReadWChar() {
            Encoding encoding = CodeSetService.GetCharEncoding(m_stream.WCharSet, true,
                                                               !BitConverter.IsLittleEndian);
            byte[] data;
            if ((m_version.Major == 1) && (m_version.Minor <= 1)) {
                // GIOP 1.1 / 1.0
                data = new byte[] { m_stream.ReadOctet() };
                while (encoding.GetCharCount(data) < 1)
                {
                    data = AppendChar(data);
                }
            }
            else {
                // GIOP 1.2 or above
                byte count = m_stream.ReadOctet();
                data = m_stream.ReadOpaque(count);
            }
            char[] result = encoding.GetChars(data);
            return result[0];
        }

        public string ReadWString() {
            Encoding encoding = CodeSetService.GetCharEncoding(m_stream.WCharSet, true,
                                                               !BitConverter.IsLittleEndian);
            uint length = ReadULong();
            byte[] data;
            if ((m_version.Major == 1) && (m_version.Minor <= 1)) {
                // GIOP 1.1 / 1.0
                length = (length * 2); // only 2 bytes fixed size characters supported
                data = m_stream.ReadOpaque((int)length - 2); // exclude trailing zero
                m_stream.ReadOctet(); // read trailing zero: a wide character
                m_stream.ReadOctet(); // read trailing zero: a wide character
            }
            else {
                data = m_stream.ReadOpaque((int)length);
            }
            char[] result = encoding.GetChars(data);

            return new string(result);
        }

        #endregion IMethods

    }


    /// <summary>
    /// this is the endian implementation for the endian dependent operation for CDROutput-streams
    /// to use, when the stream endian is different than the plattform endian (see BitConverter.IsLittleEndian)
    /// </summary>
    /// <remarks>
    /// this class is not intended for use by CDRStream users
    /// </remarks>
    internal sealed class CdrStreamNonNativeEndianWriteOP : CdrEndianDepOutputStreamOp {

        #region IFields

        private CdrOutputStream m_stream;
        private GiopVersion m_version;

        #endregion IFields
        #region IConstructors
        
        public CdrStreamNonNativeEndianWriteOP(CdrOutputStream stream, GiopVersion version) {
            m_stream = stream;
            m_version = version;
        }

        #endregion IConstructors
        #region IMethods

        #region write methods dependant on byte ordering

        private void Write(byte[] data, int count, Aligns align) {
            m_stream.ForceWriteAlign(align);
            m_stream.WriteBytes(data, 0, count);
        }
        
        public void WriteShort(short data) {
            Write(BitConverter.GetBytes(BitConverterUtils.Reverse(data)), 2, Aligns.Align2);
        }

        public void WriteUShort(ushort data) {
            Write(BitConverter.GetBytes(BitConverterUtils.Reverse(data)), 2, Aligns.Align2);
        }

        public void WriteLong(int data) {
            Write(BitConverter.GetBytes(BitConverterUtils.Reverse(data)), 4, Aligns.Align4);
        }

        public void WriteULong(uint data) {
            Write(BitConverter.GetBytes(BitConverterUtils.Reverse(data)), 4, Aligns.Align4);
        }

        public void WriteLongLong(long data) {
            Write(BitConverter.GetBytes(BitConverterUtils.Reverse(data)), 8, Aligns.Align8);
        }

        public void WriteULongLong(ulong data) {
            Write(BitConverter.GetBytes(BitConverterUtils.Reverse(data)), 8, Aligns.Align8);
        }

        public void WriteFloat(float data) {
            Write(BitConverter.GetBytes(BitConverterUtils.Reverse(data)), 4, Aligns.Align4);
        }

        public void WriteDouble(double data) {
            Write(BitConverter.GetBytes(BitConverterUtils.Reverse(data)), 8, Aligns.Align8);
        }

        public void WritePrimitiveTypeArray<T>(T[] a) {
            if(a.Length == 0)
                return;

            byte[] bytes = BitConverterUtils.ArrayToBytesReverse(a);
            int elSize = Marshal.SizeOf(typeof(T));
            m_stream.ForceWriteAlign((Aligns)elSize);
            m_stream.WriteBytes(bytes, 0, bytes.Length);
        }

        public void WriteWChar(char data) {
            Encoding encoding = CodeSetService.GetCharEncoding(m_stream.WCharSet, true,
                                                               !BitConverter.IsLittleEndian);
            byte[] toSend = encoding.GetBytes(new char[] { data });
            if (!((m_version.Major == 1) && (m_version.Minor <= 1))) {
                // GIOP 1.2
                m_stream.WriteOctet((byte)toSend.Length);
            }
            m_stream.WriteOpaque(toSend);
        }

        public void WriteWString(string data) {
            Encoding encoding = CodeSetService.GetCharEncoding(m_stream.WCharSet, true,
                                                               !BitConverter.IsLittleEndian);
            byte[] toSend = encoding.GetBytes(data);
            if ((m_version.Major == 1) && (m_version.Minor <= 1)) {
                // GIOP 1.0, 1.1
                byte[] sendNew = new byte[toSend.Length + 2];
                Array.Copy((Array)toSend, 0, (Array)sendNew, 0, toSend.Length);
                sendNew[toSend.Length] = 0; // trailing zero: a wide char
                sendNew[toSend.Length + 1] = 0; // trailing zero: a wide char
                m_stream.WriteULong(((uint)toSend.Length / 2) + 1); // number of chars instead of number of bytes, only 2 bytes character supported
                m_stream.WriteOpaque(sendNew);
            }
            else {
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
    internal sealed class CdrStreamNativeEndianReadOP : CdrEndianDepInputStreamOp {
        
        #region IFields

        private CdrInputStream m_stream;
        private GiopVersion m_version;
        private byte[] m_buf = new byte[8];

        #endregion IFields
        #region IConstructors

        public CdrStreamNativeEndianReadOP(CdrInputStream stream, GiopVersion version) {
            m_stream = stream;
            m_version = version;
        }

        #endregion IConstructors
        #region IMethods
        
        #region read methods depenant on byte ordering

        private void Read(int size, Aligns align) {
            m_stream.ForceReadAlign(align);
            m_stream.ReadBytes(m_buf, 0, size);
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
            return BitConverter.ToSingle(m_buf, 0);
        }

        public double ReadDouble() {
            Read(8, Aligns.Align8);
            return BitConverter.ToDouble(m_buf, 0);
        }

        public T[] ReadPrimitiveTypeArray<T>(int elemCount) {
            int elemSize = Marshal.SizeOf(typeof(T));
            if(elemCount > 0)
                m_stream.ForceReadAlign((Aligns)elemSize);
            if(m_buf.Length < elemSize * elemCount)
                m_buf = new byte[elemSize * elemCount * 2];
            m_stream.ReadBytes(m_buf, 0, elemSize * elemCount);
            return BitConverterUtils.BytesToArray<T>(elemCount, m_buf);
        }

        private byte[] AppendChar(byte[] oldData) {
            byte[] newData = new byte[oldData.Length + 1];
            Array.Copy(oldData, 0, newData, 0, oldData.Length);
            newData[oldData.Length] = m_stream.ReadOctet();
            return newData;
        }

        public char ReadWChar() {
            Encoding encoding = CodeSetService.GetCharEncoding(m_stream.WCharSet, true,
                                                               BitConverter.IsLittleEndian);
            byte[] data;
            if ((m_version.Major == 1) && (m_version.Minor <= 1)) {
                // GIOP 1.1 / 1.0
                data = new byte[] { m_stream.ReadOctet() };
                while (encoding.GetCharCount(data) < 1)
                {
                    data = AppendChar(data);
                }
            }
            else {
                // GIOP 1.2 or above
                byte count = m_stream.ReadOctet();
                data = m_stream.ReadOpaque(count);
            }
            char[] result = encoding.GetChars(data);
            return result[0];
        }

        public string ReadWString() {
            Encoding encoding = CodeSetService.GetCharEncoding(m_stream.WCharSet, true,
                                                               BitConverter.IsLittleEndian);
            uint length = ReadULong();
            byte[] data;
            if ((m_version.Major == 1) && (m_version.Minor <= 1)) {
                // GIOP 1.1 / 1.0
                length = (length * 2); // only 2 bytes fixed size characters supported
                data = m_stream.ReadOpaque((int)length - 2); // exclude trailing zero
                m_stream.ReadOctet(); // read trailing zero: a wide character
                m_stream.ReadOctet(); // read trailing zero: a wide character
            }
            else {
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
    internal sealed class CdrStreamNativeEndianWriteOP : CdrEndianDepOutputStreamOp {

        #region IFields

        private CdrOutputStream m_stream;
        private GiopVersion m_version;

        #endregion IFields
        #region IConstructors
        
        public CdrStreamNativeEndianWriteOP(CdrOutputStream stream, GiopVersion version) {
            m_stream = stream;
            m_version = version;
        }

        #endregion IConstructors
        #region IMethods

        #region write methods dependant on byte ordering

        private void Write(byte[] data, int count, Aligns align) {
            m_stream.ForceWriteAlign(align);
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

        public void WritePrimitiveTypeArray<T>(T[] a) {
            if(a.Length == 0)
                return;

            byte[] bytes = BitConverterUtils.ArrayToBytes(a);
            int elSize = Marshal.SizeOf(typeof(T));
            m_stream.ForceWriteAlign((Aligns)elSize);
            m_stream.WriteBytes(bytes, 0, bytes.Length);
        }

        public void WriteWChar(char data) {
            Encoding encoding = CodeSetService.GetCharEncoding(m_stream.WCharSet, true,
                                                               BitConverter.IsLittleEndian);
            byte[] toSend = encoding.GetBytes(new char[] { data });
            if (!((m_version.Major == 1) && (m_version.Minor <= 1))) {
                // GIOP 1.2
                m_stream.WriteOctet((byte)toSend.Length);
            }
            m_stream.WriteOpaque(toSend);
        }

        public void WriteWString(string data) {
            Encoding encoding = CodeSetService.GetCharEncoding(m_stream.WCharSet, true,
                                                               BitConverter.IsLittleEndian);
            byte[] toSend = encoding.GetBytes(data);
            if ((m_version.Major == 1) && (m_version.Minor <= 1)) {
                // GIOP 1.0, 1.1
                byte[] sendNew = new byte[toSend.Length + 2];
                Array.Copy((Array)toSend, 0, (Array)sendNew, 0, toSend.Length);
                sendNew[toSend.Length] = 0; // trailing zero: a wide char
                sendNew[toSend.Length + 1] = 0; // trailing zero: a wide char
                m_stream.WriteULong(((uint)toSend.Length / 2) + 1); // number of chars instead of number of bytes, only 2 bytes character supported
                m_stream.WriteOpaque(sendNew);
            }
            else {
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
    internal sealed class CdrEndianOpNotSpecified : CdrEndianDepInputStreamOp, CdrEndianDepOutputStreamOp
    {

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

        public T[] ReadPrimitiveTypeArray<T>(int elemCount) {
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

        public void WritePrimitiveTypeArray<T>(T[] data) {
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

namespace Ch.Elca.Iiop.Tests
{

    using System;
    using System.Reflection;
    using Ch.Elca.Iiop.Cdr;
    using NUnit.Framework;

    /// <summary>
    /// Unit-tests for testing CdrEndianDepOp Tests.
    /// </summary>
    [TestFixture]
    public class CdrEndianDepOpTest
    {

        private const byte STREAM_BIG_ENDIAN_FLAG = 0;
        private const byte STREAM_LITTLE_ENDIAN_FLAG = 1;

        [Test]
        public void TestInt16WBEWToS()
        {
            MemoryStream stream = new MemoryStream(new byte[] { 0, 1,
                                                                1, 2,
                                                                0x7F, 0xFF,
                                                                0x80, 0x00 });
            CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
            cdrIn.ConfigStream(STREAM_BIG_ENDIAN_FLAG, new GiopVersion(1, 2));

            System.Int16 result = cdrIn.ReadShort();
            Assert.AreEqual(1, result, "read wbe int 16");
            result = cdrIn.ReadShort();
            Assert.AreEqual(258, result, "read wbe int 16 (2)");
            result = cdrIn.ReadShort();
            Assert.AreEqual(Int16.MaxValue, result, "read wbe int 16 (3)");
            result = cdrIn.ReadShort();
            Assert.AreEqual(Int16.MinValue, result, "read wbe int 16 (4)");
        }

        [Test]
        public void TestInt16WLEWToS()
        {
            MemoryStream stream = new MemoryStream(new byte[] { 1, 0,
                                                                2, 1,
                                                                0xFF, 0x7F,
                                                                0x00, 0x80 });
            CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
            cdrIn.ConfigStream(STREAM_LITTLE_ENDIAN_FLAG, new GiopVersion(1, 2));

            System.Int16 result = cdrIn.ReadShort();
            Assert.AreEqual(1, result, "read wle int 16");
            result = cdrIn.ReadShort();
            Assert.AreEqual(258, result, "read wle int 16 (2)");
            result = cdrIn.ReadShort();
            Assert.AreEqual(Int16.MaxValue, result, "read wle int 16 (3)");
            result = cdrIn.ReadShort();
            Assert.AreEqual(Int16.MinValue, result, "read wle int 16 (4)");
        }

        [Test]
        public void TestInt16WBESToW()
        {
            MemoryStream stream = new MemoryStream();
            CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_BIG_ENDIAN_FLAG);
            cdrOut.WriteShort((short)1);
            cdrOut.WriteShort((short)258);
            cdrOut.WriteShort(Int16.MaxValue);
            cdrOut.WriteShort(Int16.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            byte[] result = new byte[2];
            stream.Read(result, 0, 2);
            Assert.AreEqual(new byte[] { 0, 1 }, result, "converted wbe int 16");
            stream.Read(result, 0, 2);
            Assert.AreEqual(new byte[] { 1, 2 }, result, "converted wbe int 16 (2)");
            stream.Read(result, 0, 2);
            Assert.AreEqual(new byte[] { 0x7F, 0xFF }, result, "converted wbe int 16 (3)");
            stream.Read(result, 0, 2);
            Assert.AreEqual(new byte[] { 0x80, 0x00 }, result, "converted wbe int 16 (4)");
        }

        [Test]
        public void TestInt16WLESToW()
        {
            MemoryStream stream = new MemoryStream();
            CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_LITTLE_ENDIAN_FLAG);
            cdrOut.WriteShort((short)1);
            cdrOut.WriteShort((short)258);
            cdrOut.WriteShort(Int16.MaxValue);
            cdrOut.WriteShort(Int16.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            byte[] result = new byte[2];
            stream.Read(result, 0, 2);
            Assert.AreEqual(new byte[] { 1, 0 }, result, "converted lbe int 16");
            stream.Read(result, 0, 2);
            Assert.AreEqual(new byte[] { 2, 1 }, result, "converted lbe int 16 (2)");
            stream.Read(result, 0, 2);
            Assert.AreEqual(new byte[] { 0xFF, 0x7F }, result, "converted lbe int 16 (3)");
            stream.Read(result, 0, 2);
            Assert.AreEqual(new byte[] { 0x00, 0x80 }, result, "converted lbe int 16 (4)");
        }

        [Test]
        public void TestUInt16WBEWToS()
        {
            MemoryStream stream = new MemoryStream(new byte[] { 0, 1,
                                                                1, 2,
                                                                0xFF, 0xFF,
                                                                0x00, 0x00 });
            CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
            cdrIn.ConfigStream(STREAM_BIG_ENDIAN_FLAG, new GiopVersion(1, 2));

            System.UInt16 result = cdrIn.ReadUShort();
            Assert.AreEqual(1, result, "read wbe uint 16");
            result = cdrIn.ReadUShort();
            Assert.AreEqual(258, result, "read wbe uint 16 (2)");
            result = cdrIn.ReadUShort();
            Assert.AreEqual(UInt16.MaxValue, result, "read wbe uint 16 (3)");
            result = cdrIn.ReadUShort();
            Assert.AreEqual(UInt16.MinValue, result, "read wbe uint 16 (4)");
        }

        [Test]
        public void TestUInt16WLEWToS()
        {
            MemoryStream stream = new MemoryStream(new byte[] { 1, 0,
                                                                2, 1,
                                                                0xFF, 0xFF,
                                                                0x00, 0x00 });
            CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
            cdrIn.ConfigStream(STREAM_LITTLE_ENDIAN_FLAG, new GiopVersion(1, 2));

            System.UInt16 result = cdrIn.ReadUShort();
            Assert.AreEqual(1, result, "read wle uint 16");
            result = cdrIn.ReadUShort();
            Assert.AreEqual(258, result, "read wle uint 16 (2)");
            result = cdrIn.ReadUShort();
            Assert.AreEqual(UInt16.MaxValue, result, "read wle uint 16 (3)");
            result = cdrIn.ReadUShort();
            Assert.AreEqual(UInt16.MinValue, result, "read wle uint 16 (4)");
        }

        [Test]
        public void TestUInt16WBESToW()
        {
            MemoryStream stream = new MemoryStream();
            CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_BIG_ENDIAN_FLAG);
            cdrOut.WriteUShort((ushort)1);
            cdrOut.WriteUShort((ushort)258);
            cdrOut.WriteUShort(UInt16.MaxValue);
            cdrOut.WriteUShort(UInt16.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            byte[] result = new byte[2];
            stream.Read(result, 0, 2);
            Assert.AreEqual(new byte[] { 0, 1 }, result, "converted wbe uint 16");
            stream.Read(result, 0, 2);
            Assert.AreEqual(new byte[] { 1, 2 }, result, "converted wbe uint 16 (2)");
            stream.Read(result, 0, 2);
            Assert.AreEqual(new byte[] { 0xFF, 0xFF }, result, "converted wbe uint 16 (3)");
            stream.Read(result, 0, 2);
            Assert.AreEqual(new byte[] { 0x00, 0x00 }, result, "converted wbe uint 16 (4)");
        }

        [Test]
        public void TestUInt16WLESToW()
        {
            MemoryStream stream = new MemoryStream();
            CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_LITTLE_ENDIAN_FLAG);
            cdrOut.WriteUShort((ushort)1);
            cdrOut.WriteUShort((ushort)258);
            cdrOut.WriteUShort(UInt16.MaxValue);
            cdrOut.WriteUShort(UInt16.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            byte[] result = new byte[2];
            stream.Read(result, 0, 2);
            Assert.AreEqual(new byte[] { 1, 0 }, result, "converted lbe uint 16");
            stream.Read(result, 0, 2);
            Assert.AreEqual(new byte[] { 2, 1 }, result, "converted lbe uint 16 (2)");
            stream.Read(result, 0, 2);
            Assert.AreEqual(new byte[] { 0xFF, 0xFF }, result, "converted lbe uint 16 (3)");
            stream.Read(result, 0, 2);
            Assert.AreEqual(new byte[] { 0x00, 0x00 }, result, "converted lbe uint 16 (4)");
        }

        [Test]
        public void TestInt32WBEWToS()
        {
            MemoryStream stream = new MemoryStream(new byte[] { 0, 0, 0, 1,
                                                                0, 0, 1, 2,
                                                                0x7F, 0xFF, 0xFF, 0xFF,
                                                                0x80, 0x00, 0x00, 0x00 });
            CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
            cdrIn.ConfigStream(STREAM_BIG_ENDIAN_FLAG, new GiopVersion(1, 2));

            System.Int32 result = cdrIn.ReadLong();
            Assert.AreEqual((int)1, result, "converted wbe int 32");
            result = cdrIn.ReadLong();
            Assert.AreEqual((int)258, result, "converted wbe int 32 (2)");
            result = cdrIn.ReadLong();
            Assert.AreEqual(Int32.MaxValue, result, "converted wbe int 32 (3)");
            result = cdrIn.ReadLong();
            Assert.AreEqual(Int32.MinValue, result, "converted wbe int 32 (4)");
        }

        [Test]
        public void TestInt32WLEWToS()
        {
            MemoryStream stream = new MemoryStream(new byte[] { 1, 0, 0, 0,
                                                                2, 1, 0, 0,
                                                                0xFF, 0xFF, 0xFF, 0x7F,
                                                                0x00, 0x00, 0x00, 0x80 });
            CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
            cdrIn.ConfigStream(STREAM_LITTLE_ENDIAN_FLAG, new GiopVersion(1, 2));

            System.Int32 result = cdrIn.ReadLong();
            Assert.AreEqual((int)1, result, "converted wbe int 32");
            result = cdrIn.ReadLong();
            Assert.AreEqual((int)258, result, "converted wbe int 32 (2)");
            result = cdrIn.ReadLong();
            Assert.AreEqual(Int32.MaxValue, result, "converted wbe int 32 (3)");
            result = cdrIn.ReadLong();
            Assert.AreEqual(Int32.MinValue, result, "converted wbe int 32 (4)");
        }

        [Test]
        public void TestInt32WBESToW()
        {
            MemoryStream stream = new MemoryStream();
            CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_BIG_ENDIAN_FLAG);
            cdrOut.WriteLong((int)1);
            cdrOut.WriteLong((int)258);
            cdrOut.WriteLong(Int32.MaxValue);
            cdrOut.WriteLong(Int32.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            byte[] result = new byte[4];
            stream.Read(result, 0, 4);
            Assert.AreEqual(new byte[] { 0, 0, 0, 1 }, result, "converted wbe int 32");
            stream.Read(result, 0, 4);
            Assert.AreEqual(new byte[] { 0, 0, 1, 2 }, result, "converted wbe int 32 (2)");
            stream.Read(result, 0, 4);
            Assert.AreEqual(new byte[] { 0x7F, 0xFF, 0xFF, 0xFF }, result, "converted wbe int 32 (3)");
            stream.Read(result, 0, 4);
            Assert.AreEqual(new byte[] { 0x80, 0x00, 0x00, 0x00 }, result, "converted wbe int 32 (4)");
        }

        [Test]
        public void TestInt32WLESToW()
        {
            MemoryStream stream = new MemoryStream();
            CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_LITTLE_ENDIAN_FLAG);
            cdrOut.WriteLong((int)1);
            cdrOut.WriteLong((int)258);
            cdrOut.WriteLong(Int32.MaxValue);
            cdrOut.WriteLong(Int32.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            byte[] result = new byte[4];
            stream.Read(result, 0, 4);
            Assert.AreEqual( new byte[] { 1, 0, 0, 0 }, result,"converted lbe int 32");
            stream.Read(result, 0, 4);
            Assert.AreEqual(new byte[] { 2, 1, 0, 0 }, result, "converted lbe int 32 (2)");
            stream.Read(result, 0, 4);
            Assert.AreEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0x7F }, result, "converted lbe int 32 (3)");
            stream.Read(result, 0, 4);
            Assert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x80 }, result, "converted lbe int 32 (4)");
        }

        [Test]
        public void TestUInt32WBEWToS()
        {
            MemoryStream stream = new MemoryStream(new byte[] { 0, 0, 0, 1,
                                                                0, 0, 1, 2,
                                                                0xFF, 0xFF, 0xFF, 0xFF,
                                                                0x00, 0x00, 0x00, 0x00 });
            CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
            cdrIn.ConfigStream(STREAM_BIG_ENDIAN_FLAG, new GiopVersion(1, 2));

            System.UInt32 result = cdrIn.ReadULong();
            Assert.AreEqual((uint)1, result,"converted wbe uint 32");
            result = cdrIn.ReadULong();
            Assert.AreEqual((uint)258, result,"converted wbe uint 32 (2)");
            result = cdrIn.ReadULong();
            Assert.AreEqual(UInt32.MaxValue, result, "converted wbe uint 32 (3)");
            result = cdrIn.ReadULong();
            Assert.AreEqual(UInt32.MinValue, result,"converted wbe uint 32 (4)");
        }

        [Test]
        public void TestUInt32WLEWToS()
        {
            MemoryStream stream = new MemoryStream(new byte[] { 1, 0, 0, 0,
                                                                2, 1, 0, 0,
                                                                0xFF, 0xFF, 0xFF, 0xFF,
                                                                0x00, 0x00, 0x00, 0x00 });
            CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
            cdrIn.ConfigStream(STREAM_LITTLE_ENDIAN_FLAG, new GiopVersion(1, 2));

            System.UInt32 result = cdrIn.ReadULong();
            Assert.AreEqual((uint)1, result, "converted wbe uint 32");
            result = cdrIn.ReadULong();
            Assert.AreEqual((uint)258, result, "converted wbe uint 32 (2)");
            result = cdrIn.ReadULong();
            Assert.AreEqual(UInt32.MaxValue, result, "converted wbe uint 32 (3)");
            result = cdrIn.ReadULong();
            Assert.AreEqual(UInt32.MinValue, result, "converted wbe uint 32 (4)");
        }

        [Test]
        public void TestUInt32WBESToW()
        {
            MemoryStream stream = new MemoryStream();
            CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_BIG_ENDIAN_FLAG);
            cdrOut.WriteULong((uint)1);
            cdrOut.WriteULong((uint)258);
            cdrOut.WriteULong(UInt32.MaxValue);
            cdrOut.WriteULong(UInt32.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            byte[] result = new byte[4];
            stream.Read(result, 0, 4);
            Assert.AreEqual(new byte[] { 0, 0, 0, 1 }, result, "converted wbe uint 32");
            stream.Read(result, 0, 4);
            Assert.AreEqual(new byte[] { 0, 0, 1, 2 }, result, "converted wbe uint 32 (2)");
            stream.Read(result, 0, 4);
            Assert.AreEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, result, "converted wbe uint 32 (3)");
            stream.Read(result, 0, 4);
            Assert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00 }, result, "converted wbe uint 32 (4)");
        }

        [Test]
        public void TestUInt32WLESToW()
        {
            MemoryStream stream = new MemoryStream();
            CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_LITTLE_ENDIAN_FLAG);
            cdrOut.WriteULong((uint)1);
            cdrOut.WriteULong((uint)258);
            cdrOut.WriteULong(UInt32.MaxValue);
            cdrOut.WriteULong(UInt32.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            byte[] result = new byte[4];
            stream.Read(result, 0, 4);
            Assert.AreEqual(new byte[] { 1, 0, 0, 0 }, result, "converted lbe uint 32");
            stream.Read(result, 0, 4);
            Assert.AreEqual(new byte[] { 2, 1, 0, 0 }, result,"converted lbe uint 32 (2)");
            stream.Read(result, 0, 4);
            Assert.AreEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, result,"converted lbe uint 32 (3)");
            stream.Read(result, 0, 4);
            Assert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00 }, result, "converted lbe uint 32 (4)");
        }

        [Test]
        public void TestInt64WBEWToS()
        {
            MemoryStream stream = new MemoryStream(new byte[] { 0, 0, 0, 0, 0, 0, 0, 1,
                                                                0, 0, 0, 0, 0, 0, 1, 2,
                                                                0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                                                                0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
            cdrIn.ConfigStream(STREAM_BIG_ENDIAN_FLAG, new GiopVersion(1, 2));
            System.Int64 result = cdrIn.ReadLongLong();
            Assert.AreEqual( 1, result,"converted wbe int 64");
            result = cdrIn.ReadLongLong();
            Assert.AreEqual(258, result, "converted wbe int 64 (2)");
            result = cdrIn.ReadLongLong();
            Assert.AreEqual(Int64.MaxValue, result, "converted wbe int 64 (3)");
            result = cdrIn.ReadLongLong();
            Assert.AreEqual(Int64.MinValue, result, "converted wbe int 64 (4)");
        }

        [Test]
        public void TestInt64WLEWToS()
        {
            MemoryStream stream = new MemoryStream(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0,
                                                                2, 1, 0, 0, 0, 0, 0, 0,
                                                                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F,
                                                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 });
            CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
            cdrIn.ConfigStream(STREAM_LITTLE_ENDIAN_FLAG, new GiopVersion(1, 2));

            System.Int64 result = cdrIn.ReadLongLong();
            Assert.AreEqual((long)1, result, "converted wbe int");
            result = cdrIn.ReadLongLong();
            Assert.AreEqual((long)258, result,"converted wbe int (2)");
            result = cdrIn.ReadLongLong();
            Assert.AreEqual(Int64.MaxValue, result, "converted wbe int (3)");
            result = cdrIn.ReadLongLong();
            Assert.AreEqual(Int64.MinValue, result, "converted wbe int (4)");
        }

        [Test]
        public void TestInt64WBESToW()
        {
            MemoryStream stream = new MemoryStream();
            CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_BIG_ENDIAN_FLAG);
            cdrOut.WriteLongLong((long)1);
            cdrOut.WriteLongLong((long)258);
            cdrOut.WriteLongLong(Int64.MaxValue);
            cdrOut.WriteLongLong(Int64.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            byte[] result = new byte[8];
            stream.Read(result, 0, 8);
            Assert.AreEqual(new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }, result, "converted wbe int");
            stream.Read(result, 0, 8);
            Assert.AreEqual(new byte[] { 0, 0, 0, 0, 0, 0, 1, 2 }, result, "converted wbe int (2)");
            stream.Read(result, 0, 8);
            Assert.AreEqual(new byte[] { 0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, result, "converted wbe int (3)");
            stream.Read(result, 0, 8);
            Assert.AreEqual(new byte[] { 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, result, "converted wbe int (4)");
        }

        [Test]
        public void TestInt64WLESToW()
        {
            MemoryStream stream = new MemoryStream();
            CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_LITTLE_ENDIAN_FLAG);
            cdrOut.WriteLongLong((long)1);
            cdrOut.WriteLongLong((long)258);
            cdrOut.WriteLongLong(Int64.MaxValue);
            cdrOut.WriteLongLong(Int64.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            byte[] result = new byte[8];
            stream.Read(result, 0, 8);
            Assert.AreEqual(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, result, "converted lbe int64");
            stream.Read(result, 0, 8);
            Assert.AreEqual(new byte[] { 2, 1, 0, 0, 0, 0, 0, 0 }, result, "converted lbe int64 (2)");
            stream.Read(result, 0, 8);
            Assert.AreEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F }, result, "converted lbe int64 (3)");
            stream.Read(result, 0, 8);
            Assert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 }, result, "converted lbe int64 (4)");
        }

        [Test]
        public void TestUInt64WBEWToS()
        {
            MemoryStream stream = new MemoryStream(new byte[] { 0, 0, 0, 0, 0, 0, 0, 1,
                                                                0, 0, 0, 0, 0, 0, 1, 2,
                                                                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                                                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
            cdrIn.ConfigStream(STREAM_BIG_ENDIAN_FLAG, new GiopVersion(1, 2));
            System.UInt64 result = cdrIn.ReadULongLong();
            Assert.AreEqual(1, result, "converted wbe uint 64");
            result = cdrIn.ReadULongLong();
            Assert.AreEqual(258, result, "converted wbe uint 64 (2)");
            result = cdrIn.ReadULongLong();
            Assert.AreEqual(UInt64.MaxValue, result,"converted wbe uint 64 (3)");
            result = cdrIn.ReadULongLong();
            Assert.AreEqual(UInt64.MinValue, result, "converted wbe uint 64 (4)");
        }

        [Test]
        public void TestUInt64WLEWToS()
        {
            MemoryStream stream = new MemoryStream(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0,
                                                                2, 1, 0, 0, 0, 0, 0, 0,
                                                                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                                                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
            cdrIn.ConfigStream(STREAM_LITTLE_ENDIAN_FLAG, new GiopVersion(1, 2));

            System.UInt64 result = cdrIn.ReadULongLong();
            Assert.AreEqual((ulong)1, result,"converted wbe uint");
            result = cdrIn.ReadULongLong();
            Assert.AreEqual((ulong)258, result,"converted wbe uint (2)");
            result = cdrIn.ReadULongLong();
            Assert.AreEqual(UInt64.MaxValue, result,"converted wbe uint (3)" );
            result = cdrIn.ReadULongLong();
            Assert.AreEqual(UInt64.MinValue, result,"converted wbe uint (4)");
        }

        [Test]
        public void TestUInt64WBESToW()
        {
            MemoryStream stream = new MemoryStream();
            CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_BIG_ENDIAN_FLAG);
            cdrOut.WriteULongLong((ulong)1);
            cdrOut.WriteULongLong((ulong)258);
            cdrOut.WriteULongLong(UInt64.MaxValue);
            cdrOut.WriteULongLong(UInt64.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            byte[] result = new byte[8];
            stream.Read(result, 0, 8);
            Assert.AreEqual(new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }, result, "converted wbe uint");
            stream.Read(result, 0, 8);
            Assert.AreEqual(new byte[] { 0, 0, 0, 0, 0, 0, 1, 2 }, result,"converted wbe uint (2)");
            stream.Read(result, 0, 8);
            Assert.AreEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, result, "converted wbe uint (3)");
            stream.Read(result, 0, 8);
            Assert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, result, "converted wbe uint (4)");
        }

        [Test]
        public void TestUInt64WLESToW()
        {
            MemoryStream stream = new MemoryStream();
            CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_LITTLE_ENDIAN_FLAG);
            cdrOut.WriteULongLong((ulong)1);
            cdrOut.WriteULongLong((ulong)258);
            cdrOut.WriteULongLong(UInt64.MaxValue);
            cdrOut.WriteULongLong(UInt64.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            byte[] result = new byte[8];
            stream.Read(result, 0, 8);
            Assert.AreEqual(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, result, "converted lbe uint64");
            stream.Read(result, 0, 8);
            Assert.AreEqual(new byte[] { 2, 1, 0, 0, 0, 0, 0, 0 }, result, "converted lbe uint64 (2)");
            stream.Read(result, 0, 8);
            Assert.AreEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, result, "converted lbe uint64 (3)");
            stream.Read(result, 0, 8);
            Assert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, result, "converted lbe uint64 (4)");

        }


        [Test]
        public void TestSingleWBEWToS()
        {
            MemoryStream stream = new MemoryStream(new byte[] { 0x3F, 0x80, 0x00, 0x00,
                                                                0x3C, 0x23, 0xD7, 0x0A,
                                                                0x7F, 0x7F, 0xFF, 0xFF,
                                                                0xFF, 0x7F, 0xFF, 0xFF });
            CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
            cdrIn.ConfigStream(STREAM_BIG_ENDIAN_FLAG, new GiopVersion(1, 2));

            System.Single result = cdrIn.ReadFloat();
            Assert.AreEqual((float)1.0f, result, "converted wbe single");
            result = cdrIn.ReadFloat();
            Assert.AreEqual((float)0.01f, result, "converted wbe single (2)");
            result = cdrIn.ReadFloat();
            Assert.AreEqual(Single.MaxValue, result, "converted wbe single (3)");
            result = cdrIn.ReadFloat();
            Assert.AreEqual(Single.MinValue, result, "converted wbe single (4)");
        }

        [Test]
        public void TestSingleWLEWToS()
        {
            MemoryStream stream = new MemoryStream(new byte[] { 0x00, 0x00, 0x80, 0x3F,
                                                                0x0A, 0xD7, 0x23, 0x3C,
                                                                0xFF, 0xFF, 0x7F, 0x7F,
                                                                0xFF, 0xFF, 0x7F, 0xFF });
            CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
            cdrIn.ConfigStream(STREAM_LITTLE_ENDIAN_FLAG, new GiopVersion(1, 2));

            System.Single result = cdrIn.ReadFloat();
            Assert.AreEqual((float)1.0f, result, "converted wbe single");
            result = cdrIn.ReadFloat();
            Assert.AreEqual((float)0.01f, result, "converted wbe single (2)");
            result = cdrIn.ReadFloat();
            Assert.AreEqual(Single.MaxValue, result, "converted wbe single (3)");
            result = cdrIn.ReadFloat();
            Assert.AreEqual(Single.MinValue, result, "converted wbe single (4)");
        }

        [Test]
        public void TestSingleWBESToW()
        {
            MemoryStream stream = new MemoryStream();
            CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_BIG_ENDIAN_FLAG);
            cdrOut.WriteFloat((float)1);
            cdrOut.WriteFloat((float)0.01);
            cdrOut.WriteFloat(Single.MaxValue);
            cdrOut.WriteFloat(Single.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            byte[] result = new byte[4];
            stream.Read(result, 0, 4);
            Assert.AreEqual(new byte[] { 0x3F, 0x80, 0x00, 0x00 }, result, "converted wbe single");
            stream.Read(result, 0, 4);
            Assert.AreEqual(new byte[] { 0x3C, 0x23, 0xD7, 0x0A }, result, "converted wbe single (2)");
            stream.Read(result, 0, 4);
            Assert.AreEqual(new byte[] { 0x7F, 0x7F, 0xFF, 0xFF }, result, "converted wbe single (3)");
            stream.Read(result, 0, 4);
            Assert.AreEqual(new byte[] { 0xFF, 0x7F, 0xFF, 0xFF }, result, "converted wbe single (4)");
        }

        [Test]
        public void TestSingleWLESToW()
        {
            MemoryStream stream = new MemoryStream();
            CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_LITTLE_ENDIAN_FLAG);
            cdrOut.WriteFloat((float)1);
            cdrOut.WriteFloat((float)0.01);
            cdrOut.WriteFloat(Single.MaxValue);
            cdrOut.WriteFloat(Single.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            byte[] result = new byte[4];
            stream.Read(result, 0, 4);
            Assert.AreEqual(new byte[] { 0x00, 0x00, 0x80, 0x3F }, result, "converted lbe single");
            stream.Read(result, 0, 4);
            Assert.AreEqual(new byte[] { 0x0A, 0xD7, 0x23, 0x3C }, result, "converted lbe single (2)");
            stream.Read(result, 0, 4);
            Assert.AreEqual(new byte[] { 0xFF, 0xFF, 0x7F, 0x7F }, result, "converted lbe single (3)");
            stream.Read(result, 0, 4);
            Assert.AreEqual(new byte[] { 0xFF, 0xFF, 0x7F, 0xFF }, result, "converted lbe single (4)");
        }

        [Test]
        public void TestDoubleWBEWToS()
        {
            MemoryStream stream = new MemoryStream(new byte[] { 0x3F, 0xF0, 0, 0, 0, 0, 0, 0,
                                                                0x3F, 0x84, 0x7A, 0xE1, 0x47, 0xAE, 0x14, 0x7B,
                                                                0x7F, 0xEF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                                                                0xFF, 0xEF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
            CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
            cdrIn.ConfigStream(STREAM_BIG_ENDIAN_FLAG, new GiopVersion(1, 2));
            double result = cdrIn.ReadDouble();
            Assert.AreEqual(1.0, result, "converted wbe double");
            result = cdrIn.ReadDouble();
            Assert.AreEqual(0.01, result, "converted wbe double (2)");
            result = cdrIn.ReadDouble();
            Assert.AreEqual(Double.MaxValue, result, "converted wbe double (3)");
            result = cdrIn.ReadDouble();
            Assert.AreEqual(Double.MinValue, result, "converted wbe double (4)");
        }

        [Test]
        public void TestDoubleWLEWToS()
        {
            MemoryStream stream = new MemoryStream(new byte[] { 0, 0, 0, 0, 0, 0, 0xF0, 0x3F,
                                                                0x7B, 0x14, 0xAE, 0x47, 0xE1, 0x7A, 0x84, 0x3F,
                                                                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xEF, 0x7F,
                                                                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xEF, 0xFF });
            CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
            cdrIn.ConfigStream(STREAM_LITTLE_ENDIAN_FLAG, new GiopVersion(1, 2));
            double result = cdrIn.ReadDouble();
            Assert.AreEqual(1.0, result, "converted lbe double");
            result = cdrIn.ReadDouble();
            Assert.AreEqual(0.01, result, "converted lbe double (2)");
            result = cdrIn.ReadDouble();
            Assert.AreEqual(Double.MaxValue, result, "converted lbe double (3)");
            result = cdrIn.ReadDouble();
            Assert.AreEqual(Double.MinValue, result, "converted lbe double (4)");
        }

        [Test]
        public void TestDoubleWBESToW()
        {
            MemoryStream stream = new MemoryStream();
            CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_BIG_ENDIAN_FLAG);
            cdrOut.WriteDouble((double)1);
            cdrOut.WriteDouble((double)0.01);
            cdrOut.WriteDouble(Double.MaxValue);
            cdrOut.WriteDouble(Double.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            byte[] result = new byte[8];
            stream.Read(result, 0, 8);
            Assert.AreEqual(new byte[] { 0x3F, 0xF0, 0, 0, 0, 0, 0, 0 }, result, "converted wbe double");
            stream.Read(result, 0, 8);
            Assert.AreEqual(new byte[] { 0x3F, 0x84, 0x7A, 0xE1, 0x47, 0xAE, 0x14, 0x7B }, result, "converted wbe double (2)");
            stream.Read(result, 0, 8);
            Assert.AreEqual(new byte[] { 0x7F, 0xEF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, result, "converted wbe double (3)");
            stream.Read(result, 0, 8);
            Assert.AreEqual(new byte[] { 0xFF, 0xEF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, result, "converted wbe double (4)");
        }

        [Test]
        public void TestDoubleWLESToW()
        {
            MemoryStream stream = new MemoryStream();
            CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_LITTLE_ENDIAN_FLAG);
            cdrOut.WriteDouble((double)1);
            cdrOut.WriteDouble((double)0.01);
            cdrOut.WriteDouble(Double.MaxValue);
            cdrOut.WriteDouble(Double.MinValue);
            stream.Seek(0, SeekOrigin.Begin);
            byte[] result = new byte[8];
            stream.Read(result, 0, 8);
            Assert.AreEqual(new byte[] { 0, 0, 0, 0, 0, 0, 0xF0, 0x3F }, result, "converted lbe double");
            stream.Read(result, 0, 8);
            Assert.AreEqual(new byte[] { 0x7B, 0x14, 0xAE, 0x47, 0xE1, 0x7A, 0x84, 0x3F }, result, "converted lbe double (2)");
            stream.Read(result, 0, 8);
            Assert.AreEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xEF, 0x7F }, result, "converted lbe double (3)");
            stream.Read(result, 0, 8);
            Assert.AreEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xEF, 0xFF }, result, "converted lbe double (4)");
        }


        [Test]
        public void TestWStringWBEWToS()
        {
            MemoryStream stream = new MemoryStream(new byte[] { 0, 0, 0, 10, 0xFE, 0xFF, 0, 84, 0, 101, 0, 115, 0, 116 });
            CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
            cdrIn.ConfigStream(STREAM_BIG_ENDIAN_FLAG, new GiopVersion(1, 2));
            cdrIn.WCharSet = (int)WCharSet.UTF16;
            string result = cdrIn.ReadWString();
            Assert.AreEqual("Test", result, "converted wbe string");
        }

        [Test]
        public void TestWStringWLEWToS()
        {
            MemoryStream stream = new MemoryStream(new byte[] { 10, 0, 0, 0, 0xFF, 0xFE, 84, 0, 101, 0, 115, 0, 116, 0 });
            CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(stream);
            cdrIn.ConfigStream(STREAM_LITTLE_ENDIAN_FLAG, new GiopVersion(1, 2));
            cdrIn.WCharSet = (int)WCharSet.UTF16;
            string result = cdrIn.ReadWString();
            Assert.AreEqual("Test", result, "converted lbe string");
        }

        [Test]
        public void TestWStringWBESToW()
        {
            MemoryStream stream = new MemoryStream();
            CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_BIG_ENDIAN_FLAG);
            cdrOut.WCharSet = (int)WCharSet.UTF16;
            cdrOut.WriteWString("Test");
            stream.Seek(0, SeekOrigin.Begin);
            byte[] result = new byte[12];
            stream.Read(result, 0, 12);
            Assert.AreEqual(new byte[] { 0, 0, 0, 8, 0, 84, 0, 101, 0, 115, 0, 116 }, result, "converted wbe string");
        }

        [Test]
        public void TestWStringWLESToW()
        {
            MemoryStream stream = new MemoryStream();
            CdrOutputStreamImpl cdrOut = new CdrOutputStreamImpl(stream, STREAM_LITTLE_ENDIAN_FLAG);
            cdrOut.WCharSet = (int)WCharSet.UTF16;
            cdrOut.WriteWString("Test");
            stream.Seek(0, SeekOrigin.Begin);
            byte[] result = new byte[14];
            stream.Read(result, 0, 14);
            Assert.AreEqual(new byte[] { 10, 0, 0, 0, 0xFF, 0xFE, 84, 0, 101, 0, 115, 0, 116, 0 }, result, "converted lbe string");
        }


    }
}

#endif
