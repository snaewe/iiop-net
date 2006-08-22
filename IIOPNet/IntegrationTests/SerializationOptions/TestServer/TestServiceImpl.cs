/* TestServiceImpl.cs
 *
 * Project: IIOP.NET
 * IntegrationTests
 *
 * WHEN      RESPONSIBLE
 * 04.06.05  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using omg.org.CORBA;

namespace Ch.Elca.Iiop.IntegrationTests {

    
    [SupportedInterfaceAttribute(typeof(TestService))]
    public class TestServiceImpl : MarshalByRefObject, TestService {

        public string EchoString(string arg) {
            return arg;
        }

        [return: IdlSequence(0L)]
        public int[] EchoSequence([IdlSequence(0L)] int[] arg) {
            return arg;
        }

        [return: IdlArray(0L, 2)]
        [return: IdlArrayDimension(0L, 1, 3)]
        public int[,] EchoArray([IdlArray(0L, 2)][IdlArrayDimension(0L, 1, 3)] int[,] arg) {
            return arg;
        }

        public override object InitializeLifetimeService() {
            // live forever
            return null;
        }

    }

}