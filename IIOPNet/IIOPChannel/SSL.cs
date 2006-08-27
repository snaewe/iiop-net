/* SSL.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 27.03.04  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
 * 
 * Copyright 2004 Dominic Ullmann
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
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Text;

using Ch.Elca.Iiop.Cdr;
using Ch.Elca.Iiop.Util;
using Ch.Elca.Iiop.Idl;
using Ch.Elca.Iiop.CorbaObjRef;
using omg.org.CORBA;

namespace Ch.Elca.Iiop.Security.Ssl {
    
    
    [Flags]
    public enum SecurityAssociationOptions {
        NoProtection = 1,
        Integrity = 2,
        Confidentiality = 4,
        DetectReplay = 8,
        DetectMisordering = 16,
        EstablishTrustInTarget = 32,
        EstablishTrustInClient = 64,
        NoDelegation = 128,
        SimpleDelegation = 256,
        CompositeDelegation = 512
    }    
        
    /// <summary>the tagged component data of the TAG_SSL_SEC_TRANS component</summary>
    [ExplicitSerializationOrdered()]
    [IdlStruct]
    public struct SSLComponentData {

        #region SFields
        
        private static Type s_assocOptionsType = typeof(SecurityAssociationOptions);
               
        public readonly static Type ClassType = typeof(SSLComponentData);              
        
        public readonly static omg.org.CORBA.TypeCode TypeCode =
            Repository.CreateTypeCodeForType(typeof(SSLComponentData),
                                             AttributeExtCollection.EmptyCollection);
        
        #endregion SFields
        #region IFields
        
        /// <summary>the association options for the target</summary>
        [ExplicitSerializationOrderNr(0)]
        private short    m_target_supports;
        /// <summary>the accociation options required for the target</summary>
        [ExplicitSerializationOrderNr(1)]
        private short    m_target_requires;
        
        /// <summary>the listening port</summary>
        /// <remarks>was mapped form an UShort, is negative for high ports
        [ExplicitSerializationOrderNr(2)]
        public short Port; // Port is mapped from an unsigned short -> cast back to ushort, before return
        
        #endregion IFields        
        #region IConstructors
        
        public SSLComponentData(short targetSupportedOptions,
                                short targetRequiredOptions, 
                                short port) {
            m_target_supports = targetSupportedOptions;
            m_target_requires = targetRequiredOptions;
            Port            = port;
        }
        
        public SSLComponentData(SecurityAssociationOptions targetSupportedOptions,
                                SecurityAssociationOptions targetRequiredOptions,
                                short port) : 
            this((short)targetSupportedOptions, (short)targetRequiredOptions, 
                 port) {
        }
        
        #endregion IConstructors
        #region IProperties
        
        public SecurityAssociationOptions TargetSupportedOptions {
            get {
                try {
                    return (SecurityAssociationOptions) Enum.ToObject(s_assocOptionsType, m_target_supports);
                } catch (Exception e) {
                    Debug.WriteLine("invalid target_supports associationOptions: " + m_target_supports + "; " + e);
                    throw new INV_POLICY(115, CompletionStatus.Completed_MayBe);                    
                }
            }
        }
        
        public SecurityAssociationOptions TargetRequiredOptions {
            get {
                try {
                    return (SecurityAssociationOptions) Enum.ToObject(s_assocOptionsType, m_target_requires);
                } catch (Exception e) {
                    Debug.WriteLine("invalid target_required associationOptions: " + m_target_requires + "; " + e);
                    throw new INV_POLICY(115, CompletionStatus.Completed_MayBe);                    
                }                
            }
        }
        
        #endregion IProperties
        #region IMethods

        public int GetPort() {
            return (ushort)Port;
        }

        #endregion IMethods
        
    }

    
}
