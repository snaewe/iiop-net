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

namespace Ch.Elca.Iiop.IntegrationTests {

    public enum TestEnum {
        A, B, C, D
    }


    public class TestService : MarshalByRefObject {

        public System.Double TestIncDouble(System.Double arg) {
            return arg + 1;
        }

        public System.Single TestIncFloat(System.Single arg) {
            return arg + 1;
        }

        public System.Byte TestIncByte(System.Byte arg) {
            return (System.Byte)(arg + 1);
        }

        public System.Int16 TestIncInt16(System.Int16 arg) {
            return (System.Int16)(arg + 1);
        }

        public System.Int32 TestIncInt32(System.Int32 arg) {
            return arg + 1;
        }

        public System.Int64 TestIncInt64(System.Int64 arg) {
            return arg + 1;
        }

        public System.Boolean TestNegateBoolean(System.Boolean arg) {
            return ! arg;
        }

        public void TestVoid() {
            return;
        }
        
        public System.Char TestEchoChar(System.Char arg) {
            return arg;
        }

        public System.String TestAppendString(System.String basic, System.String toAppend) {
            return basic + toAppend;
        }

        public TestEnum TestEchoEnumVal(TestEnum arg) {
            return arg;
        }

        public System.Byte[] TestAppendElementToByteArray(System.Byte[] arg, System.Byte toAppend) {
            System.Byte[] result;
            if (arg != null) {
                result = new System.Byte[arg.Length + 1];
                Array.Copy((Array) arg, (Array) result, arg.Length);
            } else {
                result = new System.Byte[1];
            }
            result[result.Length - 1] = toAppend;
            return result;
        }

        public System.String[] TestAppendElementToStringArray(System.String[] arg, System.String toAppend) {
            System.String[] result;
            if (arg != null) {
                result = new System.String[arg.Length + 1];
                Array.Copy((Array) arg, (Array) result, arg.Length);
            } else {
                result = new System.String[1];
            }
            result[result.Length - 1] = toAppend;
            return result;
        }

        public override object InitializeLifetimeService() {
            // live forever
            return null;
        }

    }

}
