/* CORBAInitService.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 27.01.03  Dominic Ullmann (DUL), dul@elca.ch
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
using System.Runtime.Remoting;
using Ch.Elca.Iiop.Idl;
using omg.org.CosNaming;
using omg.org.CORBA;

namespace Ch.Elca.Iiop.Services {

    /// <summary>
    /// specifies the operation on the INIT-object
    /// </summary>
    [InterfaceTypeAttribute(IdlTypeInterface.ConcreteInterface)]
    internal interface CORBAInitService : IIdlEntity {

        #region IMethods

        NamingContext _get([WideCharAttribute(false)][StringValueAttribute] string serviceName);

        #endregion IMethods

    }

    
    /// <summary>
    /// This class represents the entry point for a CORBA-client. The CORBAInitService can 
    /// return references to the important CORBA-services.
    /// </summary>
    internal class CORBAInitServiceImpl : MarshalByRefObject, CORBAInitService {

        #region Constants

        internal const string NAMESERVICE_NAME = "NameService";

        #endregion Constants
        #region SFields
        
        internal static byte[] s_corbaObjKey = new byte[] { 0x49, 0x4E, 0x49, 0x54 };

        private static CORBAInitServiceImpl s_corbaInitService;

        private static object s_lockObject = new Object();

        #endregion SFields
        #region IConstructors

        private CORBAInitServiceImpl() {
        }

        #endregion IConstructors
        #region SMethods
        
        public static void Publish() {
            lock(s_lockObject) {
                if (s_corbaInitService == null) {    
                    s_corbaInitService = new CORBAInitServiceImpl();
                    RemotingServices.Marshal(s_corbaInitService, 
                                             IiopUrlUtil.GetObjUriForObjectInfo(s_corbaObjKey,
                                                                                new GiopVersion(1, 0)));
                }
            }
        }

        #endregion SMethods
        #region IMethods

        public NamingContext _get([WideCharAttribute(false)][StringValueAttribute] string serviceName) {
            if (serviceName.Equals(NAMESERVICE_NAME)) {
                COSNamingContextImpl nameingContext = new COSNamingContextImpl();
                return nameingContext;
            }
            // unknown serivce: serviceName
            throw new OBJECT_NOT_EXIST(9700, CompletionStatus.Completed_MayBe);
        }

        public override Object InitializeLifetimeService() {
            // this should live forever
            return null;
        }

        #endregion IMethods
    
    }

}
