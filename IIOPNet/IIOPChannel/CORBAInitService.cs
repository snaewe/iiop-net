/* CORBAInitService.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 27.01.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using Ch.Elca.Iiop.Util;
using omg.org.CosNaming;
using omg.org.CORBA;

namespace Ch.Elca.Iiop.Services {

    /// <summary>
    /// specifies the operation on the INIT-object
    /// </summary>
    [InterfaceTypeAttribute(IdlTypeInterface.ConcreteInterface)]
    [CLSCompliant(false)]
    public interface CORBAInitService : IIdlEntity {

        #region IMethods

        [FromIdlName("get")]
        MarshalByRefObject _get([WideCharAttribute(false)][StringValueAttribute] string serviceName);

        #endregion IMethods

    }

    
    /// <summary>
    /// This class represents the entry point for a CORBA-client. The CORBAInitService can 
    /// return references to the important CORBA-services.
    /// </summary>
    internal class CORBAInitServiceImpl : MarshalByRefObject, CORBAInitService {

        #region Constants

        internal const string NAMESERVICE_NAME = "NameService";
        internal const string INITSERVICE_NAME = "INIT";

        #endregion Constants
        #region SFields
        
        private static CORBAInitServiceImpl s_corbaInitService;

        private static object s_lockObject = new Object();

        #endregion SFields
        #region IFields

        private InitialCOSNamingContextImpl m_initalContext;

        #endregion IFields
        #region IConstructors

        private CORBAInitServiceImpl() {
            PublishNameService();
        }

        #endregion IConstructors
        #region SMethods
        
        internal static void Publish() {
            lock(s_lockObject) {
                if (s_corbaInitService == null) {

                    // create the init service, which provides access to other service (for JDK orbs)
                    s_corbaInitService = new CORBAInitServiceImpl();
                    RemotingServices.Marshal(s_corbaInitService, 
                                             INITSERVICE_NAME);
                }
            }
        }

        #endregion SMethods
        #region IMethods
        
        private void PublishNameService() {
            // create root naming context and publish it
            m_initalContext = new InitialCOSNamingContextImpl();
            RemotingServices.Marshal(m_initalContext,
                                     InitialCOSNamingContextImpl.INITIAL_NAMING_OBJ_NAME);
        }

        public MarshalByRefObject _get([WideCharAttribute(false)][StringValueAttribute] string serviceName) {
            if (serviceName.Equals(NAMESERVICE_NAME)) {
                return m_initalContext;
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
