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

using System;
using System.Collections;
using System.Text;
using Ch.Elca.Iiop.Services;
using omg.org.CORBA;

namespace Ch.Elca.Iiop.CodeSet {
    
    /// <summary>
    /// stores the mapping between codesets and encodings for realising the conversion to/from .NET chars
    /// </summary>
    internal class CodeSetConversionRegistry {

        #region IFields
        
        /// <summary>stores the encodings</summary>
        private Hashtable m_codeSetsEndianIndep = new Hashtable();
        private Hashtable m_codeSetsBigEndian = new Hashtable();
        private Hashtable m_codeSetsLittleEndian = new Hashtable();
        
        #endregion IFields
        #region IConstructors
        
        internal CodeSetConversionRegistry() { 
        }

        #endregion IConstructors
        #region IMethods


        /// <summary>
        /// adds an encoding for both endians (endian independant)
        /// </summary>
        internal void AddEncodingAllEndian(int id, System.Text.Encoding encoding) {
            m_codeSetsEndianIndep.Add(id,encoding);
            AddEncodingBigEndian(id, encoding);
            AddEncodingLittleEndian(id, encoding);
        }

        /// <summary>
        /// adds an encoding for only big endian
        /// </summary>            
        internal void AddEncodingBigEndian(int id, System.Text.Encoding encoding) {
            m_codeSetsBigEndian.Add(id, encoding);
        }            

        /// <summary>
        /// adds an encoding for little endian
        /// </summary>                        
        internal void AddEncodingLittleEndian(int id, System.Text.Encoding encoding) {
            m_codeSetsLittleEndian.Add(id, encoding);
        }            

        /// <summary>
        /// gets an endian independant encoding
        /// </summary>                                    
        internal System.Text.Encoding GetEncodingEndianIndependant(int id) {
            return (System.Text.Encoding)m_codeSetsEndianIndep[id];
        }

        /// <summary>
        /// gets an encoding usable with big endian
        /// </summary>                                    
        internal System.Text.Encoding GetEncodingBigEndian(int id) {
            return (System.Text.Encoding)m_codeSetsBigEndian[id];
        }
            
        /// <summary>
        /// gets an encoding usable with little endian
        /// </summary>                                    
        internal System.Text.Encoding GetEncodingLittleEndian(int id) {
            return (System.Text.Encoding)m_codeSetsLittleEndian[id];
        }

        #endregion IMethods
    }
                


    public class Latin1Encoding : Encoding {
        
        #region IMethods
        
        public override int GetByteCount(char[] chars, int index, int count) {
            // one char results in one byte
            return count;
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount,
                                     byte[] bytes, int byteIndex) {
            if ((bytes.Length - byteIndex) < charCount) { 
                // bytes array is too small
                throw new INTERNAL(9965, CompletionStatus.Completed_MayBe);
            }
            
            // mapping for latin-1: latin-1 value = unicode-value, for unicode values 0 - 0xFF, other values: exception, non latin-1
            for (int i = charIndex; i < charIndex + charCount; i++) {
                byte lowbits = (byte)(chars[i] & 0x00FF);
                byte highbits = (byte) ((chars[i] & 0xFF00) >> 8);
                if (highbits != 0) { 
                    // character : chars[i]
                    // can't be encoded, because it's a non-latin1 character
                    throw new BAD_PARAM(1919, CompletionStatus.Completed_MayBe);
                }
                bytes[byteIndex + (i - charIndex)] = lowbits;
            }
            return charCount;
        }

        public override int GetCharCount(byte[] bytes, int index, int count) {
            // one byte results in one char
            return count;
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex) {
            if ((chars.Length - charIndex) < byteCount) { 
                // chars array is too small
                throw new INTERNAL(9965, CompletionStatus.Completed_MayBe);
            }
            // mapping for latin-1: unicode-value = latin-1 value
            for (int i = byteIndex; i < byteIndex + byteCount; i++) {
                chars[charIndex + (i  - byteIndex)] = (char) bytes[i];
            }
            return byteCount;
        }

        public override int GetMaxByteCount(int charCount) {
            // one char results in one byte
            return charCount;
        }

        public override int GetMaxCharCount(int byteCount) {
            // one byte results in one char
            return byteCount;
        }

        #endregion IMethods

    }


    /// <summary>
    /// This class is an extended version of the unicode-encoder:
    /// it encodes a byte-order-mark (if wished), removes a byte order mark on decoding
    /// </summary>
    public class UnicodeEncodingExt : UnicodeEncoding {

        #region IFields

        private bool m_bigEndian = true;
        private bool m_includeEndianMark = true;

        #endregion IFields
        #region IConstructors
        
        public UnicodeEncodingExt() : base() {
            
        }

        public UnicodeEncodingExt(bool bigEndian, bool mark) : base(bigEndian, mark) {
            m_bigEndian = bigEndian;
            m_includeEndianMark = mark;
            
        }

        #endregion IConsturctors
        #region IMethods

        public override int GetByteCount(string s) {
            char[] asCharArr = s.ToCharArray();
            return GetByteCount(asCharArr, 0, asCharArr.Length);
        }

        public override int GetByteCount(char[] chars, int index, int count) {
            int result = base.GetByteCount(chars, index, count);
            if (m_includeEndianMark) {
                result += 2;
            }
            return result;
        }

        public override int GetBytes(string s, int charIndex, int charCount,
                                     byte[] bytes, int byteIndex) {
            char[] asCharArr = s.ToCharArray();
            return GetBytes(asCharArr, charIndex, charCount, bytes, byteIndex);
        }

        public override byte[] GetBytes(string s) {
            char[] asCharArr = s.ToCharArray();
            byte[] result = new byte[GetByteCount(asCharArr, 0, asCharArr.Length)];
            GetBytes(asCharArr, 0, asCharArr.Length, result, 0);
            return result;
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount,
                                     byte[] bytes, int byteIndex) {
            if (m_includeEndianMark) {
                if (m_bigEndian) {
                    bytes[byteIndex] = 254;
                    bytes[byteIndex+1] = 255;
                } else {
                    bytes[byteIndex] = 255;
                    bytes[byteIndex+1] = 254;
                }
                int result = base.GetBytes(chars, charIndex, charCount,
                                           bytes, byteIndex+2);
                return result + 2;
            } else {
                return base.GetBytes(chars, charIndex, charCount,
                                     bytes, byteIndex);
            }
        }

        public override int GetCharCount(byte[] bytes, int index, int count) {
            // no endian mark possible if array too small
            if (bytes.Length <= 1) { 
                return base.GetCharCount(bytes, index, count); 
            }
            // check for endian mark
            if (bytes[index] == 254 && (bytes[index+1] == 255)) {
                if (m_bigEndian) {
                    return base.GetCharCount(bytes, index+2, count-2);
                } else {
                    // little endian not supported, if big endian specified
                    throw new BAD_PARAM(9923, CompletionStatus.Completed_MayBe);
                }
            } else if (bytes[index] == 255 && (bytes[index+1] == 254)) {
                if (m_bigEndian) {
                    // big endian not supported, if little endian specified
                    throw new BAD_PARAM(9924, CompletionStatus.Completed_MayBe);
                } else {
                    return base.GetCharCount(bytes, index+2, count-2);
                }
            } else {
                return base.GetCharCount(bytes, index, count); // no endian mark present
            }
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount,
                                     char[] chars, int charIndex) {
            // no big/little endian tag in byte array possible if array too small
            if (bytes.Length <= 1) { 
                return base.GetChars(bytes, byteIndex, byteCount, chars, charIndex); 
            }
            // check if big/little endian tag in byte array
            if (bytes[byteIndex] == 254 && (bytes[byteIndex+1] == 255)) {
                if (m_bigEndian) {
                    return base.GetChars(bytes, byteIndex+2, byteCount-2,
                                         chars, charIndex);
                } else {
                    // little endian not supported, if big endian specified
                   throw new BAD_PARAM(9923, CompletionStatus.Completed_MayBe);
                }
            } else if (bytes[byteIndex] == 255 && (bytes[byteIndex+1] == 254)) {
                if (m_bigEndian) {
                    // big endian not supported, if little endian specified
                    throw new BAD_PARAM(9924, CompletionStatus.Completed_MayBe);
                } else {
                    return base.GetChars(bytes, byteIndex+2, byteCount-2,
                                         chars, charIndex);
                }
            } else {
                return base.GetChars(bytes, byteIndex, byteCount,
                                     chars, charIndex);
            }
        }

        #endregion IMethods

    }

}
