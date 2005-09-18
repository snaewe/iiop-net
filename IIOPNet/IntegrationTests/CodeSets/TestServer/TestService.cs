/* TestService.cs
 *
 * Project: IIOP.NET
 * IntegrationTests
 *
 * WHEN      RESPONSIBLE
 * 18.09.05  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using Ch.Elca.Iiop.Idl;
using omg.org.CORBA;

namespace Ch.Elca.Iiop.IntegrationTests {


    public class TestService : MarshalByRefObject {

       
        [return: WideChar(false)]
        public System.Char TestEchoChar([WideChar(false)] System.Char arg) {
            return arg;
        }

        [return: StringValue()]
        [return: WideChar(false)]
        public System.String TestAppendString([StringValue][WideChar(false)] System.String basic, 
                                              [StringValue][WideChar(false)] System.String toAppend) {
            return basic + toAppend;
        }


        [return: WideChar(true)]
        public System.Char TestEchoWChar([WideChar(true)] System.Char arg) {
            return arg;
        }

        [return: StringValue()]
        [return: WideChar(true)]
        public System.String TestAppendWString([StringValue][WideChar(true)] System.String basic, 
                                               [StringValue][WideChar(true)] System.String toAppend) {
            return basic + toAppend;
        }

        public TestService ReturnNewTestService() {
            return new TestService();
        }

        
    }

}
