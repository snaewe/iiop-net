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
using omg.org.CORBA;

namespace Ch.Elca.Iiop.Services {

    /// <summary>
    /// This service handles code-set conversion.
    /// </summary>
    public class CodeSetService : CorbaService {

        #region Constants

        public const uint SERVICE_ID = 1;

        internal const uint UTF16_SET = 0x10109;
        internal const uint LATIN1_SET = 0x10001;
        internal const uint UTF8_SET = 0x5010001;

        internal const uint ISO646IEC_MULTI = 0x10100; // compatible with UTF-16
        internal const uint ISO646IEC_SINGLE = 0x10020; // compatible with ASCII

        public const uint DEFAULT_CHAR_SET = LATIN1_SET;
        public const uint DEFAULT_WCHAR_SET = UTF16_SET;


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

        public override void HandleContextForReceivedReply(ServiceContext context) {
            // nothing to do at the moment
        }

        private void CheckCodeSetCompatible(uint charSet, uint wcharSet) {
            // check if acceptable: at the moment, the code-set establishment algorithm is not implemented
            if (!IsCharSetCompatible(charSet)) { 
                throw new CODESET_INCOMPATIBLE(9501, CompletionStatus.Completed_No); 
            }
            if (!IsWCharSetCompatible(wcharSet)) { 
                throw new CODESET_INCOMPATIBLE(9502, CompletionStatus.Completed_No); 
            }            
        }
        
        private bool IsWCharSetCompatible(uint wcharSet) {
            if ((wcharSet == UTF16_SET) || (wcharSet == ISO646IEC_MULTI)) { 
                return true;
            } else {
                return false;
            }
        }
        
        private bool IsCharSetCompatible(uint charSet) {
            if ((charSet == LATIN1_SET) || (charSet == ISO646IEC_SINGLE) || (charSet == UTF8_SET)) {
                return true;
            } else {
                return false;
            }
        }
        
        private CodeSetComponent FindCodeSetComponent(IorProfile[] profiles) {
            foreach (IorProfile profile in profiles) {
                TaggedComponent[] components = profile.TaggedComponents;
                foreach (TaggedComponent taggedComp in components) {
                    if (taggedComp is CodeSetComponent) {
                    	return (CodeSetComponent)taggedComp;
                    }
                }
            }
            return null;
        }

        public override void HandleContextForReceivedRequest(ServiceContext context) {
            if (context == null) { 
                return; 
            }
            uint charSet = ((CodeSetServiceContext) context).CharSet;
            uint wcharSet = ((CodeSetServiceContext) context).WCharSet;
            CheckCodeSetCompatible(charSet, wcharSet);

            // TODO: implement code set establishment-alg
            GiopConnectionContext conContext = IiopConnectionManager.GetCurrentConnectionContext();
            conContext.CharSet = charSet;
            conContext.WCharSet = wcharSet;
        }
        
        private uint ChooseCharSet(CodeSetComponent codeSetComponent) {
            if (codeSetComponent.NativeCharSet == DEFAULT_CHAR_SET) {
                // the same native char sets
                return DEFAULT_CHAR_SET;
            }
            if (IsCharSetCompatible(codeSetComponent.NativeCharSet)) {
                // client converts to server's native char set
                return codeSetComponent.NativeCharSet;
            }
            uint[] serverConvSets = codeSetComponent.CharConvSet;
            foreach (uint serverConvSet in serverConvSets) {
                if (serverConvSet == DEFAULT_CHAR_SET) {
                    // server convert's from client's native char set
                    return DEFAULT_CHAR_SET;
                }
            }
            foreach (uint serverConvSet in serverConvSets) {
                if (IsCharSetCompatible(serverConvSet)) {
                    // use a conversion code set which is available for client and server
                    return serverConvSet;
                }
            }
            throw new CODESET_INCOMPATIBLE(9501, CompletionStatus.Completed_No);
        }
        
        private uint ChooseWCharSet(CodeSetComponent codeSetComponent) {
            if (codeSetComponent.NativeWCharSet == DEFAULT_WCHAR_SET) {
                // the same native wchar sets
                return DEFAULT_WCHAR_SET;
            }
            if (IsWCharSetCompatible(codeSetComponent.NativeWCharSet)) {
                // client converts to server's native wchar set
                return codeSetComponent.NativeWCharSet;
            }
            uint[] serverConvSets = codeSetComponent.WCharConvSet;
            foreach (uint serverConvSet in serverConvSets) {
                if (serverConvSet == DEFAULT_WCHAR_SET) {
                    // server convert's from client's native wchar set
                    return DEFAULT_WCHAR_SET;
                }
            }
            foreach (uint serverConvSet in serverConvSets) {
                if (IsWCharSetCompatible(serverConvSet)) {
                    // use a conversion code set which is available for client and server
                    return serverConvSet;
                }
            }
            throw new CODESET_INCOMPATIBLE(9502, CompletionStatus.Completed_No);
        }

        public override ServiceContext InsertContextForReplyToSend() {
            // nothing to do ?
            return null;
        }

        public override ServiceContext InsertContextForRequestToSend(IMethodCallMessage msg, Ior targetIor,
                                                                     GiopConnectionContext conContext) {
            uint charSet = DEFAULT_CHAR_SET;
            uint wcharSet = DEFAULT_WCHAR_SET;
            
            CodeSetComponent codeSetComponent = FindCodeSetComponent(targetIor.Profiles);
            if (codeSetComponent != null) {
                charSet = ChooseCharSet(codeSetComponent);
                wcharSet = ChooseWCharSet(codeSetComponent);
            }
            
            conContext.CharSet = charSet;
            conContext.WCharSet = wcharSet;
            return new CodeSetServiceContext(charSet, wcharSet);
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
