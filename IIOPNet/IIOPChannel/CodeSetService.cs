/* CodeSetService.cs
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
using System.Runtime.Remoting.Messaging;
using Ch.Elca.Iiop.Cdr;
using Ch.Elca.Iiop.CorbaObjRef;
using Ch.Elca.Iiop.Idl;
using omg.org.CORBA;

namespace Ch.Elca.Iiop.Services {
    
    
    [IdlStruct]
    public struct CodeSetComponentData : ITaggedComponentData {
        
        #region IFields
        
        public int NativeCharSet;
        [IdlSequence]
        public int[] CharConvSet;
        
        public int NativeWCharSet;
        [IdlSequence]
        public int[] WCharConvSet;
        
        #endregion IFields
        #region IConstructors        
        
        public CodeSetComponentData(int nativeCharSet, int[] charConvSet,
                                    int nativeWCharSet, int[] wcharConvSet) {
            NativeCharSet = nativeCharSet;            
            NativeWCharSet = nativeWCharSet;
            CharConvSet = charConvSet;
            if (CharConvSet == null) {
                CharConvSet = new int[0];
            }
            WCharConvSet = wcharConvSet;
            if (WCharConvSet == null) {
                WCharConvSet = new int[0];
            }            
        }
        
        #endregion IConstructors
                
    }
    

    /// <summary>
    /// This service handles code-set conversion.
    /// </summary>
    public class CodeSetService : CorbaService {

        #region Constants

        public const uint SERVICE_ID = 1;

        internal const int UTF16_SET = 0x10109;
        internal const int LATIN1_SET = 0x10001;
        internal const int UTF8_SET = 0x5010001;

        internal const int ISO646IEC_MULTI = 0x10100; // compatible with UTF-16
        internal const int ISO646IEC_SINGLE = 0x10020; // compatible with ASCII

        public const int DEFAULT_CHAR_SET = LATIN1_SET;
        public const int DEFAULT_WCHAR_SET = UTF16_SET;


        #endregion Constants
        #region IConstructors
        
        public CodeSetService() {
        }

        #endregion IConstructors
        #region IMethods

        public override uint GetServiceId() {
            return SERVICE_ID;
        }

        public override ServiceContext DeserialiseContext(CdrEncapsulationInputStream encap) {
            return new CodeSetServiceContext(encap);
        }

        public override void HandleContextForReceivedReply(ServiceContext context,
                                                           GiopConnectionDesc conDesc) {
            // nothing to do at the moment
        }

        private void CheckCodeSetCompatible(int charSet, int wcharSet) {
            // check if acceptable: at the moment, the code-set establishment algorithm is not implemented
            if (!IsCharSetCompatible(charSet)) { 
                throw new CODESET_INCOMPATIBLE(9501, CompletionStatus.Completed_No); 
            }
            if (!IsWCharSetCompatible(wcharSet)) { 
                throw new CODESET_INCOMPATIBLE(9502, CompletionStatus.Completed_No); 
            }            
        }
        
        private bool IsWCharSetCompatible(int wcharSet) {
            if ((wcharSet == UTF16_SET) || (wcharSet == ISO646IEC_MULTI)) { 
                return true;
            } else {
                return false;
            }
        }
        
        private bool IsCharSetCompatible(int charSet) {
            if ((charSet == LATIN1_SET) || (charSet == ISO646IEC_SINGLE) || (charSet == UTF8_SET)) {
                return true;
            } else {
                return false;
            }
        }
        
        private ITaggedComponent FindCodeSetComponent(IorProfile[] profiles) {
            foreach (IorProfile profile in profiles) {
                ITaggedComponent[] components = profile.TaggedComponents;
                foreach (ITaggedComponent taggedComp in components) {
                    if (taggedComp.Id == TaggedComponentIds.CODESET_COMPONENT_ID) {
                    	return taggedComp;
                    }
                }
            }
            return null;
        }

        public override void HandleContextForReceivedRequest(ServiceContext context,
                                                             GiopConnectionDesc conDesc) {
            if (context == null) { 
                return; 
            }
            uint charSet = ((CodeSetServiceContext) context).CharSet;
            uint wcharSet = ((CodeSetServiceContext) context).WCharSet;
            CheckCodeSetCompatible((int)charSet, (int)wcharSet);

            // TODO: implement code set establishment-alg
            conDesc.CharSet = charSet;
            conDesc.WCharSet = wcharSet;
        }
        
        private int ChooseCharSet(CodeSetComponentData codeSetComponent) {
            if (codeSetComponent.NativeCharSet == DEFAULT_CHAR_SET) {
                // the same native char sets
                return DEFAULT_CHAR_SET;
            }
            if (IsCharSetCompatible(codeSetComponent.NativeCharSet)) {
                // client converts to server's native char set
                return codeSetComponent.NativeCharSet;
            }
            int[] serverConvSets = codeSetComponent.CharConvSet;
            foreach (int serverConvSet in serverConvSets) {
                if (serverConvSet == DEFAULT_CHAR_SET) {
                    // server convert's from client's native char set
                    return DEFAULT_CHAR_SET;
                }
            }
            foreach (int serverConvSet in serverConvSets) {
                if (IsCharSetCompatible(serverConvSet)) {
                    // use a conversion code set which is available for client and server
                    return serverConvSet;
                }
            }
            throw new CODESET_INCOMPATIBLE(9501, CompletionStatus.Completed_No);
        }
        
        private int ChooseWCharSet(CodeSetComponentData codeSetComponent) {
            if (codeSetComponent.NativeWCharSet == DEFAULT_WCHAR_SET) {
                // the same native wchar sets
                return DEFAULT_WCHAR_SET;
            }
            if (IsWCharSetCompatible(codeSetComponent.NativeWCharSet)) {
                // client converts to server's native wchar set
                return codeSetComponent.NativeWCharSet;
            }
            int[] serverConvSets = codeSetComponent.WCharConvSet;
            foreach (int serverConvSet in serverConvSets) {
                if (serverConvSet == DEFAULT_WCHAR_SET) {
                    // server convert's from client's native wchar set
                    return DEFAULT_WCHAR_SET;
                }
            }
            foreach (int serverConvSet in serverConvSets) {
                if (IsWCharSetCompatible(serverConvSet)) {
                    // use a conversion code set which is available for client and server
                    return serverConvSet;
                }
            }
            throw new CODESET_INCOMPATIBLE(9502, CompletionStatus.Completed_No);
        }

        public override ServiceContext InsertContextForReplyToSend(GiopConnectionDesc conDesc) {
            // nothing to do ?
            return null;
        }

        public override ServiceContext InsertContextForRequestToSend(IMethodCallMessage msg, Ior targetIor,
                                                                     GiopConnectionDesc conDesc) {
            int charSet = DEFAULT_CHAR_SET;
            int wcharSet = DEFAULT_WCHAR_SET;
            
            ITaggedComponent codeSetComponent = FindCodeSetComponent(targetIor.Profiles);
            if (codeSetComponent != null) {
                charSet = ChooseCharSet((CodeSetComponentData)codeSetComponent.ComponentData);
                wcharSet = ChooseWCharSet((CodeSetComponentData)codeSetComponent.ComponentData);
            }
            
            // TODO: check for already established
            conDesc.CharSet = (uint)charSet;
            conDesc.WCharSet = (uint)wcharSet;
            return new CodeSetServiceContext((uint)charSet, (uint)wcharSet);
        }

        #endregion IMethods
        
    }


    /// <summary>
    /// the service context for the code set service
    /// </summary>
    public class CodeSetServiceContext : ServiceContext {
        
        #region Constants
        
        private const uint SERVICE_ID = CodeSetService.SERVICE_ID;

        #endregion Constants
        #region IFields
        
        private uint m_charSet;
        private uint m_wcharSet;

        #endregion IFields
        #region IConstructors

        public CodeSetServiceContext(uint charSet, uint wcharSet) : base(SERVICE_ID) {
            m_charSet = charSet;
            m_wcharSet = wcharSet;    
        }

        public CodeSetServiceContext(CdrEncapsulationInputStream encap) : base(encap, SERVICE_ID) {
        }


        #endregion IConstructors
        #region IProperties
        
        public uint CharSet {
            get {
                return m_charSet;
            }
        }

        public uint WCharSet {
            get {
                return m_wcharSet;
            }
        }

        #endregion IProperties
        #region IMethods

        public override string ToString() {
            return "CodeSetService: charset: " + m_charSet +
                   ", wcharset: " + m_wcharSet;
        }

        public override void Serialize(CdrOutputStream stream) {
            stream.WriteULong(SERVICE_ID);
            CdrEncapsulationOutputStream encapStream = new CdrEncapsulationOutputStream(0);
            encapStream.WriteULong(m_charSet);
            encapStream.WriteULong(m_wcharSet);
            stream.WriteEncapsulation(encapStream);
        }

        public override void Deserialize(CdrEncapsulationInputStream encap) {
            m_charSet = encap.ReadULong();
            m_wcharSet = encap.ReadULong();
        }

        #endregion IMethods
    }

}
