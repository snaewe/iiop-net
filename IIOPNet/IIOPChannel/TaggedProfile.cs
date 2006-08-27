/* TaggedProfile.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 24.04.05  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
 * 
 * Copyright 2005 Dominic Ullmann
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
using Ch.Elca.Iiop.Idl;

 
namespace omg.org.IOP {
  
    public sealed class TAG_INTERNET_IOP {
        
        #region Constants
        
        public const int ConstVal = 0;
        
        #endregion Constants
        #region IConstructors

        private TAG_INTERNET_IOP() {
        }        
        
        #endregion IConstructors
        
    }    

    
    public sealed class TAG_MULTIPLE_COMPONENTS {
        
        #region Constants
        
        public const int ConstVal = 1;
        
        #endregion Constants
        #region IConstructors

        private TAG_MULTIPLE_COMPONENTS() {
        }        
        
        #endregion IConstructors
        
    }    
    
    
    public sealed class TAG_SCCP_IOP {
        
        #region Constants
        
        public const int ConstVal = 2;
        
        #endregion Constants
        #region IConstructors

        private TAG_SCCP_IOP() {
        }        
        
        #endregion IConstructors
        
    }        
   
    
    [RepositoryID("IDL:omg.org/IOP/TaggedProfile:1.0")]
    [IdlStruct()]
    [Serializable]
    [ExplicitSerializationOrdered()]
    public struct TaggedProfile {
        [ExplicitSerializationOrderNr(0)]
        public int tag;
        [ExplicitSerializationOrderNr(1)]
        [IdlSequence(0L)]
        public byte[] profile_data;
    }
    
} 
