/* TestService.cs
 *
 * Project: IIOP.NET
 * IntegrationTests
 *
 * WHEN      RESPONSIBLE
 * 08.07.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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

namespace Ch.Elca.Iiop.IntegrationTests {

    [Serializable]
    public struct TestStructA {
        public System.Int32 X;
        public System.Int32 Y;
    }

    public class Adder : MarshalByRefObject {
        public System.Int32 Add(System.Int32 sum1, System.Int32 sum2) {
            return sum1 + sum2;
        }
    }

    public class TestService : MarshalByRefObject {

        public System.Byte TestIncByte(System.Byte arg) {
            return (System.Byte)(arg + 1);
        }

        public void TestVoid() {
            return;
        }        
        
        public Adder RetrieveAdder() {
            return new Adder();
        }

        public TestStructA TestEchoStruct(TestStructA arg) {
            return arg;
        }

        public bool BlockForTime(int timeInMilliSec) {
            System.Threading.Thread.Sleep(timeInMilliSec);
            return true;
        }
        
        public override object InitializeLifetimeService() {
            // live forever
            return null;
        }

    }

}
