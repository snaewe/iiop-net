/* IIOPRequestNumberHelper.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 17.01.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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

namespace Ch.Elca.Iiop {

    /// <summary>
    /// this class is able to generate unique request id for a connection    
    /// </summary>
    internal class GiopRequestNumberGenerator {

        #region IFields

        private uint m_last = 5;
        
        private uint m_increment = 1;

        #endregion IFields
        #region IConstructors
        
        /// <summary>
        /// creates sequential numbers.
        /// </summary>
        internal GiopRequestNumberGenerator() {
        }
        
        /// <summary>
        /// creates only even or non-even numbers.
        /// </summary>        
        internal GiopRequestNumberGenerator(bool evenOrNonEven) {
            m_increment = 2;
            if (evenOrNonEven) {
                m_last = 6;
            }
        }        

        #endregion IConstructors
        #region IMethods
        
        /// <summary>generates the next request id</summary>
        /// <remarks>this operation isn't thread safe</remarks>
        internal uint GenerateRequestId() {
            if (IsAbleToGenerateNext()) {
                uint result = m_last;
                m_last = m_last + m_increment;
                return result;
            } else {
                // overflow
                throw new InvalidOperationException("RequestNumberGen: overflow occured");
            }
        }
                
        /// <summary>returns true, if reqNumberGen is able to generate a next request number</summary>
        /// <remarks>this operation isn't thread safe</remarks>
        internal bool IsAbleToGenerateNext() {
            return m_last < UInt32.MaxValue;
        }

        #endregion IMethods

    }
}
