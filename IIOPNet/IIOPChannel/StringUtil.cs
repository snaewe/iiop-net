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
    public class StringConversions {

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
        public static String Stringify(byte[] array) {
            Char[] res = new Char[array.Length*2];

            for (int i = 0; i < array.Length; i++) {
                int val = array[i];
                res[i*2] = s_hexMap[val >> 4];
                res[i*2+1] = s_hexMap[val % 16];
            }
            return new String(res);
        }

        /// <summary>
        /// Convert a string containing a byte array in literal form to a byte array
        /// </summary>
        /// <param name="s">String to be parsed</param>
        /// <returns></returns>
        public static byte[] Destringify(String s) {
            if (s.Length % 2 == 1) {
                throw new ArgumentException("String length must be even");
            }
            byte[] res = new byte[s.Length >> 1];

            for (int i = 0; i < res.Length; i++) {
                String sub = s.Substring(i*2, 2);
                res[i] = byte.Parse(sub, System.Globalization.NumberStyles.HexNumber);
            }
            return res;
        }

        #endregion SMethods
    }    
}
