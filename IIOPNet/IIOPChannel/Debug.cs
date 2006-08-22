/* IIOPTranport.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 06.11.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Diagnostics;
using System.IO;

namespace Ch.Elca.Iiop {

    public sealed class OutputHelper {
        private OutputHelper() {
        }

        [Conditional("TRACE")]
        [Conditional("DEBUG")]
        public static void DebugBuffer(byte[] data) {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++) {
                String msg = String.Format("{0:X2} ", data[i]);
                char ch = (char)data[i];
                sb = sb.Append(((ch > 0x20) ? ch.ToString() : " "));
                Debug.Write(msg);
                if (i % 16 == 15) {
                    Debug.WriteLine("  |   " +sb.ToString()); 
                    sb = new StringBuilder();
                }
            }
            if (data.Length % 16 != 0) {
                for (int i = data.Length % 16; i < 16; i++) {
                    Debug.Write("   ");
                }
                Debug.WriteLine("  |   " +sb.ToString());
            }
        }

        [Conditional("TRACE")]
        [Conditional("DEBUG")]        
        public static void LogStream(Stream stream) {
            stream.Seek(0, SeekOrigin.Begin); // assure stream is read from beginning
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, (int)stream.Length);
            OutputHelper.DebugBuffer(data);
        }
        
    }

}
