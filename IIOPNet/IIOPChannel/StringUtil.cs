/* StringUtil.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 15.01.03  Dominic Ullmann (DUL), dul@elca.ch
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

namespace Ch.Elca.Iiop.Util {

     
    /// <summary>
    /// Provides some string to byte[] and byte[] to string conversion methods
    /// </summary>
    // used by UrlUtil, Ior
    public class StringUtil {

        #region IConstructors

        private StringUtil() {
        }

        #endregion IConstructors
        #region SMethods

        /// <summary>
        /// cut off the high 8 bits of every character in the string and return a byte[] representation
        /// of this 8bit character string
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] GetCutOffByteArrForString(string data) {
            if (data == null) { 
                return new byte[0]; 
            }
            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++) {
                result[i] = (byte)data[i];
            }
            return result;
        }

        /// <summary>
        /// creates a byte arr representation for a string, without loosing information:
        /// one char is encoded as two bytes
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] GetByteArrRepForString(string data) {
            if (data == null) { 
                return new byte[0]; 
            }
            byte[] result = new byte[data.Length * 2];
            for (int i = 0; i < data.Length; i++) {
                result[2*i] = (byte)((data[i] & 0xFF00) >> 8);
                result[2*i+1] = (byte)(data[i] & 0x00FF);
            }
            return result;
        }

        /// <summary>
        /// creates a string from an array of short characters
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string GetStringFromShortChar(byte[] data) {
            if (data == null) { 
                return ""; 
            }
            
            string result = "";
            for (int i = 0; i < data.Length; i++) {
                result += (char)data[i];
            }            
            return result;
        }

        /// <summary>
        /// creates a string from a byte array containing 2 byte characters
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string GetStringFromWideChar(byte[] data) {
            if (data == null) { 
            	return ""; 
            }
            if ((data.Length % 2) != 0) { 
                throw new ArgumentException("data length must contain 2 byte characters"); 
            }
            string result = "";
            for (int i = 0; i < (data.Length / 2); i++) {
                char resChar = (char)((data[2*i] << 8) | (data[2*i + 1]));
                result += resChar;
            }
            return result;
        }
        
        #endregion SMethods

    }
    
}
