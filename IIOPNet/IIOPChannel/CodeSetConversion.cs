/* CodeSetConversion.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 28.01.03  Dominic Ullmann (DUL), dul@elca.ch
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

namespace Ch.Elca.Iiop.CodeSet {

    public class CodeSetConversionRegistry {

        #region SFields

        private static CodeSetConversionRegistry s_registry = new CodeSetConversionRegistry();

        #endregion SFields
        #region IFields
        
        /// <summary>stores the encodings</summary>
        private Hashtable m_knownCodeSets = new Hashtable();
        
        #endregion IFields
        #region IConstructors
        
        public CodeSetConversionRegistry() { 
            // add the non-endian dependant encodings here
            AddEncoding(CodeSetService.LATIN1_SET, new Latin1Encoding());
            AddEncoding(CodeSetService.ISO646IECSingle, new ASCIIEncoding());
        }

        #endregion IConstructors
        #region SMethods

        /// <summary>get the singleton registry</summary>
        /// <returns></returns>
        public static CodeSetConversionRegistry GetRegistry() { 
            return s_registry; 
        }

        #endregion SMethods
        #region IMethods


        protected void AddEncoding(uint id, Encoding encoding) {
            m_knownCodeSets.Add(id, encoding);    
        }

        public Encoding GetEncoding(uint id) {
            return (Encoding)m_knownCodeSets[id];
        }
    
        #endregion IMethods
    }
    
    
    /// <summary>
    /// This registry contains the known character encodings
    /// </summary>
    public class CodeSetConversionRegistryBigEndian : CodeSetConversionRegistry {
    
        #region SFields
        
        private static CodeSetConversionRegistryBigEndian s_registry = new CodeSetConversionRegistryBigEndian();

        #endregion SFields
        #region IConstructors

        private CodeSetConversionRegistryBigEndian() {
            AddEncoding(CodeSetService.UTF16_SET, new UnicodeEncodingExt(true, false)); // use big endian encoding here, put no unicode byte order mark
            AddEncoding(CodeSetService.ISO646IECMulti, new UnicodeEncodingExt(true, false));
        }

        #endregion IConsturctors
        #region SMethods

        /// <summary>get the singleton registry</summary>
        public static new CodeSetConversionRegistry GetRegistry() {
            return s_registry;
        }

        #endregion SMethods

    
    }

    // TODO: Little endian support


    public class Latin1Encoding : Encoding {
        
        #region IMethods
        
        public override int GetByteCount(char[] chars, int index, int count) {
            // one char results in one byte
            return count;
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex) {
            if ((bytes.Length - byteIndex) < charCount) { 
                throw new ArgumentException("bytes array is too small"); 
            }
            
            // mapping for latin-1: latin-1 value = unicode-value, for unicode values 0 - 0xFF, other values: exception, non latin-1
            for (int i = charIndex; i < charIndex + charCount; i++) {
                byte lowbits = (byte)(chars[i] & 0x00FF);
                byte highbits = (byte) ((chars[i] & 0xFF00) >> 8);
                if (highbits != 0) { 
                    throw new ArgumentException("character : " + chars[i] + 
                                                " can't be encoded, because it's a non-latin1 character"); 
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
                throw new ArgumentException("chars array is too small"); 
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
                int result = base.GetBytes(chars, charIndex, charCount, bytes, byteIndex+2);
                return result + 2;
            } else {
                return base.GetBytes(chars, charIndex, charCount, bytes, byteIndex);
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
                    throw new Exception("little endian not supported, if big endian specified");
                }
            } else if (bytes[index] == 255 && (bytes[index+1] == 254)) {
                if (m_bigEndian) {
                    throw new Exception("big endian not supported, if little endian specified");
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
                    return base.GetChars(bytes, byteIndex+2, byteCount-2, chars, charIndex);
                } else {
                    throw new Exception("little endian not supported, if big endian specified");
                }
            } else if (bytes[byteIndex] == 255 && (bytes[byteIndex+1] == 254)) {
                if (m_bigEndian) {
                    throw new Exception("big endian not supported, if little endian specified");
                } else {
                    return base.GetChars(bytes, byteIndex+2, byteCount-2, chars, charIndex);
                }
            } else {
                return base.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
            }
        }

        #endregion IMethods

    }

}
