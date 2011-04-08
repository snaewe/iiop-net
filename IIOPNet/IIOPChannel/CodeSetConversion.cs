/* CodeSetConversion.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 28.01.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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

using System.Collections.Generic;
using System.Text;
using omg.org.CORBA;

namespace Ch.Elca.Iiop.CodeSet
{

    /// <summary>
    /// stores the mapping between codesets and encodings for realising the conversion to/from .NET chars
    /// </summary>
    internal class CodeSetConversionRegistry
    {

        #region IFields

        /// <summary>stores the encodings</summary>
        private IDictionary<int, Encoding> m_codeSetsEndianIndep = new SortedList<int, Encoding>();
        private IDictionary<int, Encoding> m_codeSetsBigEndian = new SortedList<int, Encoding>();
        private IDictionary<int, Encoding> m_codeSetsLittleEndian = new SortedList<int, Encoding>();

        #endregion IFields
        #region IConstructors

        internal CodeSetConversionRegistry()
        {}

        #endregion IConstructors
        #region IMethods


        /// <summary>
        /// adds an encoding for both endians (endian independant)
        /// </summary>
        internal void AddEncodingAllEndian(int id, Encoding encoding)
        {
            m_codeSetsEndianIndep.Add(id, encoding);
            AddEncodingBigEndian(id, encoding);
            AddEncodingLittleEndian(id, encoding);
        }

        /// <summary>
        /// adds an encoding for only big endian
        /// </summary>            
        internal void AddEncodingBigEndian(int id, Encoding encoding)
        {
            m_codeSetsBigEndian.Add(id, encoding);
        }

        /// <summary>
        /// adds an encoding for little endian
        /// </summary>                        
        internal void AddEncodingLittleEndian(int id, Encoding encoding)
        {
            m_codeSetsLittleEndian.Add(id, encoding);
        }

        /// <summary>
        /// gets an endian independant encoding
        /// </summary>                                    
        internal Encoding GetEncodingEndianIndependant(int id)
        {
            Encoding encoding;
            return m_codeSetsEndianIndep.TryGetValue(id, out encoding) ? encoding : null;
        }

        /// <summary>
        /// gets an encoding usable with big endian
        /// </summary>                                    
        internal Encoding GetEncodingBigEndian(int id)
        {
            Encoding encoding;
            return m_codeSetsBigEndian.TryGetValue(id, out encoding) ? encoding : null;
        }

        /// <summary>
        /// gets an encoding usable with little endian
        /// </summary>                                    
        internal Encoding GetEncodingLittleEndian(int id)
        {
            Encoding encoding;
            return m_codeSetsLittleEndian.TryGetValue(id, out encoding) ? encoding : null;
        }

        #endregion IMethods
    }



    public class Latin1Encoding : Encoding
    {

        #region IMethods

        public override int GetByteCount(char[] chars, int index, int count)
        {
            // one char results in one byte
            return count;
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount,
                                     byte[] bytes, int byteIndex)
        {
            if ((bytes.Length - byteIndex) < charCount)
            {
                // bytes array is too small
                throw new INTERNAL(9965, CompletionStatus.Completed_MayBe);
            }

            // mapping for latin-1: latin-1 value = unicode-value, for unicode values 0 - 0xFF, other values: exception, non latin-1
            for (int i = charIndex; i < charIndex + charCount; i++)
            {
                byte lowbits = (byte)(chars[i] & 0x00FF);
                byte highbits = (byte)((chars[i] & 0xFF00) >> 8);
                if (highbits != 0)
                {
                    // character : chars[i]
                    // can't be encoded, because it's a non-latin1 character
                    throw new BAD_PARAM(1919, CompletionStatus.Completed_MayBe);
                }
                bytes[byteIndex + (i - charIndex)] = lowbits;
            }
            return charCount;
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            // one byte results in one char
            return count;
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            if ((chars.Length - charIndex) < byteCount)
            {
                // chars array is too small
                throw new INTERNAL(9965, CompletionStatus.Completed_MayBe);
            }
            // mapping for latin-1: unicode-value = latin-1 value
            for (int i = byteIndex; i < byteIndex + byteCount; i++)
            {
                chars[charIndex + (i - byteIndex)] = (char)bytes[i];
            }
            return byteCount;
        }

        public override int GetMaxByteCount(int charCount)
        {
            // one char results in one byte
            return charCount;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            // one byte results in one char
            return byteCount;
        }

        #endregion IMethods

    }


    /// <summary>
    /// This class is an extended version of the unicode-encoder:
    /// it encodes a byte-order-mark for little endian, removes a byte order mark on decoding
    /// </summary>
    /// <remarks>This class implements the
    /// rules in CORBA 2.6 Chapter 15.3.1.6 releated to UTF 16</remarks>
    public class UnicodeEncodingExt : Encoding
    {

        #region SFields

        private static UnicodeEncoding s_unicodeEncodingBe =
            new UnicodeEncoding(true, false);

        // for little endian, a bom is required, because default is big endian.
        private static UnicodeEncoding s_unicodeEncodingLe =
            new UnicodeEncoding(false, true);

        #endregion SFields
        #region IFields

        /// <summary>
        /// the encoding instance used to convert char[] to byte[], for the reverse,
        /// the decision is based on bom in byte[].
        /// </summary>
        private readonly UnicodeEncoding m_encoderToUse;

        #endregion IFields
        #region IConstructors

        public UnicodeEncodingExt(bool encodeAsBigEndian)
        {
            m_encoderToUse = encodeAsBigEndian ? s_unicodeEncodingBe
                                               : s_unicodeEncodingLe;
        }

        #endregion IConsturctors
        #region IMethods

        public override int GetByteCount(char[] chars, int index, int count)
        {
            return m_encoderToUse.GetByteCount(chars, index, count) +
                   m_encoderToUse.GetPreamble().Length;
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount,
                                     byte[] bytes, int byteIndex)
        {
            m_encoderToUse.GetPreamble().CopyTo(bytes, byteIndex);
            return m_encoderToUse.GetBytes(chars, charIndex, charCount,
                                           bytes, byteIndex + m_encoderToUse.GetPreamble().Length) +
                   m_encoderToUse.GetPreamble().Length;
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            // no endian mark possible if array too small, default is big endian
            if (bytes.Length <= 1)
            {
                return s_unicodeEncodingBe.GetCharCount(bytes, index, count);
            }
            // check for endian mark, select correct encoding.                           
            if (bytes[index] == 254 && (bytes[index + 1] == 255))
            {
                return s_unicodeEncodingBe.GetCharCount(bytes, index + 2, count - 2);
            }
            if (bytes[index] == 255 && (bytes[index + 1] == 254))
            {
                return s_unicodeEncodingLe.GetCharCount(bytes, index + 2, count - 2);
            }
            // no endian mark present
            return s_unicodeEncodingBe.GetCharCount(bytes, index, count);
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount,
                                     char[] chars, int charIndex)
        {
            // no big/little endian tag in byte array possible if array too small
            if (bytes.Length <= 1)
            {
                return s_unicodeEncodingBe.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
            }
            // check for endian mark, select correct encoding.                           
            if (bytes[byteIndex] == 254 && (bytes[byteIndex + 1] == 255))
            {
                return s_unicodeEncodingBe.GetChars(bytes, byteIndex + 2, byteCount - 2,
                                                    chars, charIndex);
            }
            if (bytes[byteIndex] == 255 && (bytes[byteIndex + 1] == 254))
            {
                return s_unicodeEncodingLe.GetChars(bytes, byteIndex + 2, byteCount - 2,
                                                    chars, charIndex);
            }
            // no endian mark present
            return s_unicodeEncodingBe.GetChars(bytes, byteIndex, byteCount,
                                                chars, charIndex);
        }

        public override int GetMaxByteCount(int charCount)
        {
            // one char results in two byte; if a bom is encoded, add 2.
            return m_encoderToUse.GetMaxByteCount(charCount) +
                   m_encoderToUse.GetPreamble().Length;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            // two bytes results in one char; if a bom is encoded, this method returns too much,
            // but only a maximum is requested -> therefore ok.
            return m_encoderToUse.GetMaxCharCount(byteCount);
        }

        #endregion IMethods

    }

}

#if UnitTest

namespace Ch.Elca.Iiop.Tests
{

    using System.IO;
    using Ch.Elca.Iiop;
    using Ch.Elca.Iiop.Cdr;
    using NUnit.Framework;

    /// <summary>
    /// test the encoding/decoding of UTF 16 strings
    /// </summary>
    [TestFixture]
    public class TestUtf16StringsGiop1_2
    {

        private CdrInputStreamImpl CreateInputStream(byte[] content, bool isLittleEndian)
        {
            MemoryStream stream = new MemoryStream(content);
            CdrInputStreamImpl cdrStream = new CdrInputStreamImpl(stream);
            byte endianFlag = 0;
            if (isLittleEndian)
            {
                endianFlag = 1;
            }
            cdrStream.ConfigStream(endianFlag, new GiopVersion(1, 2));
            cdrStream.SetMaxLength((uint)content.Length);
            cdrStream.WCharSet = (int)Ch.Elca.Iiop.Services.WCharSet.UTF16;
            return cdrStream;
        }

        /// <summary>
        /// check, that an utf 16 encoded string can be read correctly from a 
        /// big endian stream, if no bom is placed. If no bom is placed, big endian
        /// is assumed. See CORBA 2.6, chapter 15.3.1
        /// </summary>
        [Test]
        public void TestDecodeNoBomBeStream()
        {
            byte[] encoded = new byte[] { 0, 0, 0, 8, 0, 84, 0, 101, 0, 115, 0, 116 }; // Test
            CdrInputStream cdrStream = CreateInputStream(encoded, false);
            Assert.AreEqual("Test", cdrStream.ReadWString(), "wrongly decoded");
        }

        /// <summary>
        /// check, that an utf 16 encoded string can be read correctly from a 
        /// little endian stream, if no bom is placed. If no bom is placed, big endian
        /// is assumed. See CORBA 2.6, chapter 15.3.1
        /// </summary>
        [Test]
        public void TestDecodeNoBomLeStream()
        {
            byte[] encoded = new byte[] { 8, 0, 0, 0, 0, 84, 0, 101, 0, 115, 0, 116 }; // Test
            CdrInputStream cdrStream = CreateInputStream(encoded, true);
            Assert.AreEqual("Test", cdrStream.ReadWString(), "wrongly decoded");
        }

        /// <summary>
        /// check, that an utf 16 encoded string can be read correctly
        /// from a big endian stream, if a little endian bom is placed.
        /// See CORBA 2.6, chapter 15.3.1
        /// </summary>
        [Test]
        public void TestDecodeLeBomBeStream()
        {
            byte[] encoded = new byte[] { 0, 0, 0, 10, 0xFF, 0xFE, 84, 0, 101, 0, 115, 0, 116, 0 }; // Test
            CdrInputStream cdrStream = CreateInputStream(encoded, false);
            Assert.AreEqual("Test", cdrStream.ReadWString(), "wrongly decoded");
        }

        /// <summary>
        /// check, that an utf 16 encoded string can be read correctly
        /// from a little endian stream, if a little endian bom is placed.
        /// See CORBA 2.6, chapter 15.3.1
        /// </summary>
        [Test]
        public void TestDecodeLeBomLeStream()
        {
            byte[] encoded = new byte[] { 10, 0, 0, 0, 0xFF, 0xFE, 84, 0, 101, 0, 115, 0, 116, 0 }; // Test
            CdrInputStream cdrStream = CreateInputStream(encoded, true);
            Assert.AreEqual("Test", cdrStream.ReadWString(), "wrongly decoded");
        }

        /// <summary>
        /// check, that an utf 16 encoded string can be read correctly
        /// from a big endian stream, if a big endian bom is placed.
        /// See CORBA 2.6, chapter 15.3.1
        /// </summary>
        [Test]
        public void TestDecodeBeBomBeStream()
        {
            byte[] encoded = new byte[] { 0, 0, 0, 10, 0xFE, 0xFF, 0, 84, 0, 101, 0, 115, 0, 116 }; // Test
            CdrInputStream cdrStream = CreateInputStream(encoded, false);
            Assert.AreEqual("Test", cdrStream.ReadWString(), "wrongly decoded");
        }

        /// <summary>
        /// check, that an utf 16 encoded string can be read correctly
        /// from a little endian stream, if a big endian bom is placed.
        /// See CORBA 2.6, chapter 15.3.1
        /// </summary>
        [Test]
        public void TestDecodeBeBomLeStream()
        {
            byte[] encoded = new byte[] { 10, 0, 0, 0, 0xFE, 0xFF, 0, 84, 0, 101, 0, 115, 0, 116 }; // Test
            CdrInputStream cdrStream = CreateInputStream(encoded, true);
            Assert.AreEqual("Test", cdrStream.ReadWString(), "wrongly decoded");
        }

        /// <summary>
        /// check, that a wstring is encoded as big-endian with big endian bom for a big endian stream.
        /// </summary>
        [Test]
        public void TestEncodeBeStream()
        {
            MemoryStream outStream = new MemoryStream();
            CdrOutputStreamImpl cdrStream = new CdrOutputStreamImpl(outStream, 0, new GiopVersion(1, 2));
            cdrStream.WCharSet = (int)Ch.Elca.Iiop.Services.WCharSet.UTF16;
            cdrStream.WriteWString("Test");
            AssertByteArrayEquals(new byte[] { 0, 0, 0, 8, 0, 84, 0, 101, 0, 115, 0, 116 },
                                  outStream.ToArray());

        }

        /// <summary>
        /// check, that a wstring is encoded as little-endian with little endian bom for a little endian stream.
        /// </summary>        
        [Test]
        public void TestEncodeLeStream()
        {
            MemoryStream outStream = new MemoryStream();
            CdrOutputStreamImpl cdrStream = new CdrOutputStreamImpl(outStream, 1, new GiopVersion(1, 2));
            cdrStream.WCharSet = (int)Ch.Elca.Iiop.Services.WCharSet.UTF16;
            cdrStream.WriteWString("Test");
            AssertByteArrayEquals(new byte[] { 10, 0, 0, 0, 0xFF, 0xFE, 84, 0, 101, 0, 115, 0, 116, 0 },
                                  outStream.ToArray());
        }

        private void AssertByteArrayEquals(byte[] arg1, byte[] arg2)
        {
            Assert.AreEqual(arg1.Length, arg2.Length, "Array length");
            for (int i = 0; i < arg1.Length; i++)
            {
                Assert.AreEqual(arg1[i], arg2[i], "array element number: " + i);
            }
        }


    }

    /// <summary>
    /// test the encoding/decoding of UTF 16 strings
    /// </summary>
    [TestFixture]
    public class TestUtf16StringsGiop1_1
    {

        private CdrInputStreamImpl CreateInputStream(byte[] content, bool isLittleEndian)
        {
            MemoryStream stream = new MemoryStream(content);
            CdrInputStreamImpl cdrStream = new CdrInputStreamImpl(stream);
            byte endianFlag = 0;
            if (isLittleEndian)
            {
                endianFlag = 1;
            }
            cdrStream.ConfigStream(endianFlag, new GiopVersion(1, 1));
            cdrStream.SetMaxLength((uint)content.Length);
            cdrStream.WCharSet = (int)Ch.Elca.Iiop.Services.WCharSet.UTF16;
            return cdrStream;
        }

        /// <summary>
        /// check, that an utf 16 encoded string can be read correctly from a 
        /// big endian stream, if no bom is placed. If no bom is placed, big endian
        /// is assumed. See CORBA 2.6, chapter 15.3.1
        /// </summary>
        [Test]
        public void TestDecodeNoBomBeStream()
        {
            byte[] encoded = new byte[] { 0, 0, 0, 5, 0, 84, 0, 101, 0, 115, 0, 116, 0, 0 }; // Test
            CdrInputStream cdrStream = CreateInputStream(encoded, false);
            Assert.AreEqual("Test", cdrStream.ReadWString(), "wrongly decoded");
        }

        /// <summary>
        /// check, that an utf 16 encoded string can be read correctly from a 
        /// little endian stream, if no bom is placed. If no bom is placed, big endian
        /// is assumed. See CORBA 2.6, chapter 15.3.1
        /// </summary>
        [Test]
        public void TestDecodeNoBomLeStream()
        {
            byte[] encoded = new byte[] { 5, 0, 0, 0, 0, 84, 0, 101, 0, 115, 0, 116, 0, 0 }; // Test
            CdrInputStream cdrStream = CreateInputStream(encoded, true);
            Assert.AreEqual("Test", cdrStream.ReadWString(), "wrongly decoded");
        }

        /// <summary>
        /// check, that an utf 16 encoded string can be read correctly
        /// from a big endian stream, if a little endian bom is placed.
        /// See CORBA 2.6, chapter 15.3.1
        /// </summary>
        [Test]
        public void TestDecodeLeBomBeStream()
        {
            byte[] encoded = new byte[] { 0, 0, 0, 6, 0xFF, 0xFE, 84, 0, 101, 0, 115, 0, 116, 0, 0, 0 }; // Test
            CdrInputStream cdrStream = CreateInputStream(encoded, false);
            Assert.AreEqual("Test", cdrStream.ReadWString(), "wrongly decoded");
        }

        /// <summary>
        /// check, that an utf 16 encoded string can be read correctly
        /// from a little endian stream, if a little endian bom is placed.
        /// See CORBA 2.6, chapter 15.3.1
        /// </summary>
        [Test]
        public void TestDecodeLeBomLeStream()
        {
            byte[] encoded = new byte[] { 6, 0, 0, 0, 0xFF, 0xFE, 84, 0, 101, 0, 115, 0, 116, 0, 0, 0 }; // Test
            CdrInputStream cdrStream = CreateInputStream(encoded, true);
            Assert.AreEqual("Test", cdrStream.ReadWString(), "wrongly decoded");
        }

        /// <summary>
        /// check, that an utf 16 encoded string can be read correctly
        /// from a big endian stream, if a big endian bom is placed.
        /// See CORBA 2.6, chapter 15.3.1
        /// </summary>
        [Test]
        public void TestDecodeBeBomBeStream()
        {
            byte[] encoded = new byte[] { 0, 0, 0, 6, 0xFE, 0xFF, 0, 84, 0, 101, 0, 115, 0, 116, 0, 0 }; // Test
            CdrInputStream cdrStream = CreateInputStream(encoded, false);
            Assert.AreEqual("Test", cdrStream.ReadWString(), "wrongly decoded");
        }

        /// <summary>
        /// check, that an utf 16 encoded string can be read correctly
        /// from a little endian stream, if a big endian bom is placed.
        /// See CORBA 2.6, chapter 15.3.1
        /// </summary>
        [Test]
        public void TestDecodeBeBomLeStream()
        {
            byte[] encoded = new byte[] { 6, 0, 0, 0, 0xFE, 0xFF, 0, 84, 0, 101, 0, 115, 0, 116, 0, 0 }; // Test
            CdrInputStream cdrStream = CreateInputStream(encoded, true);
            Assert.AreEqual("Test", cdrStream.ReadWString(), "wrongly decoded");
        }

        /// <summary>
        /// check, that a wstring is encoded as big-endian with big endian bom for a big endian stream.
        /// </summary>
        [Test]
        public void TestEncodeBeStream()
        {
            MemoryStream outStream = new MemoryStream();
            CdrOutputStreamImpl cdrStream = new CdrOutputStreamImpl(outStream, 0, new GiopVersion(1, 1));
            cdrStream.WCharSet = (int)Ch.Elca.Iiop.Services.WCharSet.UTF16;
            cdrStream.WriteWString("Test");
            AssertByteArrayEquals(new byte[] { 0, 0, 0, 5, 0, 84, 0, 101, 0, 115, 0, 116, 0, 0 },
                                  outStream.ToArray());

        }

        /// <summary>
        /// check, that a wstring is encoded as little-endian with little endian bom for a little endian stream.
        /// </summary>        
        [Test]
        public void TestEncodeLeStream()
        {
            MemoryStream outStream = new MemoryStream();
            CdrOutputStreamImpl cdrStream = new CdrOutputStreamImpl(outStream, 1, new GiopVersion(1, 1));
            cdrStream.WCharSet = (int)Ch.Elca.Iiop.Services.WCharSet.UTF16;
            cdrStream.WriteWString("Test");
            AssertByteArrayEquals(new byte[] { 6, 0, 0, 0, 0xFF, 0xFE, 84, 0, 101, 0, 115, 0, 116, 0, 0, 0 },
                                  outStream.ToArray());
        }

        private void AssertByteArrayEquals(byte[] arg1, byte[] arg2)
        {
            Assert.AreEqual(arg1.Length, arg2.Length, "Array length");
            for (int i = 0; i < arg1.Length; i++)
            {
                Assert.AreEqual(arg1[i], arg2[i], "array element number: " + i);
            }
        }

    }

}

#endif
