/* StringUtil.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 15.01.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Text;

namespace Ch.Elca.Iiop.Util {


    /// <summary>
    /// Summary description for Conversions.
    /// </summary>
    public static class StringConversions {

        #region SFields

        private static Char[] s_hexMap = new Char[]{'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};

        #endregion SFields
        #region SMethods

        /// <summary>
        /// Convert a byte array to its literal string representation
        /// ByteArrayToHexString({12,34,5A,BC}) => "12345ABC"
        /// </summary>
        /// <param name="array">the array to be converted</param>
        /// <returns>the result</returns>
        public static string Stringify(string header, byte[] array, int count) {
            StringBuilder builder = new StringBuilder(header, header.Length + count * 2);

            for (int i = 0; i < count; ++i) {
                int val = array[i];
                builder.Append(s_hexMap[val >> 4]);
                builder.Append(s_hexMap[val % 16]);
            }
            return builder.ToString();
        }

        public static int Parse(string s, int startIndex, int length) {
            int result = 0;
            for (int i = 0; i != length; ++i) {
                char c = s[startIndex + i];
                if (c >= '0' && c <= '9') {
                    result = (result << 4) + (c - '0');
                }
                else if (c >= 'A' && c <= 'F') {
                    result = (result << 4) + 10 + (c - 'A');
                }
                else if (c >= 'a' && c <= 'f') {
                    result = (result << 4) + 10 + (c - 'a');
                }
                else {
                    throw new FormatException(string.Format("Unexpected '{0}' while parsing an hexadecimal number", c));
                }
            }
            return result;
        }

        /// <summary>
        /// Convert a string containing a byte array in literal form to a byte array
        /// </summary>
        /// <param name="s">String to be parsed</param>
        /// <returns></returns>
        public static byte[] Destringify(string s, int startIndex) {
            int count = s.Length - startIndex;
            if (count % 2 == 1) {
                throw new ArgumentException("String length must be even");
            }
            byte[] res = new byte[count >> 1];

            for (int i = 0; i != res.Length; ++i) {
                res[i] = Convert.ToByte(Parse(s, startIndex + i * 2, 2));
            }
            return res;
        }

        public static bool IsBlank(string s) {
            for (int i = 0; i != s.Length; ++i) {
                if (!Char.IsWhiteSpace(s[i])) {
                    return false;
                }
            }
            return true;
        }
        #endregion SMethods
    }    
}
