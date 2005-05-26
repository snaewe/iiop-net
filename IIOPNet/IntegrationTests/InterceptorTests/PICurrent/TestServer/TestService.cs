/* TestService.cs
 *
 * Project: IIOP.NET
 * IntegrationTests
 *
 * WHEN      RESPONSIBLE
 * 10.04.05  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Threading;
using Ch.Elca.Iiop.Idl;
using omg.org.CORBA;
using omg.org.PortableInterceptor;

namespace Ch.Elca.Iiop.IntegrationTests {

    public class TestServerSideException : AbstractUserException {

    }

    public class TestService : MarshalByRefObject {

        private int m_slotId;

        public TestService(int slotId) {
            m_slotId = slotId;
        }        

        public System.Int32 TestAddToContextData(System.Int32 arg) {
            ORB orb = OrbServices.GetSingleton();
            omg.org.PortableInterceptor.Current current = 
                (omg.org.PortableInterceptor.Current)orb.resolve_initial_references("PICurrent");
            int contextData = (int)current.get_slot(m_slotId);            
            int result = contextData + arg;
            current.set_slot(m_slotId, result);
            return result;
        }

        public System.Int32 TestReceiveReqNotChangeThreadScope(System.Int32 arg) {
            return TestAddToContextData(arg);
        }

        public System.Int32 TestReceiveReqChangeThreadScope(System.Int32 arg) {
            return TestAddToContextData(arg);
        }

        public bool NoValueInScope() {
            ORB orb = OrbServices.GetSingleton();
            omg.org.PortableInterceptor.Current current = 
                (omg.org.PortableInterceptor.Current)orb.resolve_initial_references("PICurrent");
            return (current.get_slot(m_slotId) == null);
        }

        public override object InitializeLifetimeService() {
            // live forever
            return null;
        }        
        
    }

}
