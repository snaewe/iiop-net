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
using System.Runtime.Remoting;
using Ch.Elca.Iiop.Idl;
using omg.org.CORBA;

namespace Ch.Elca.Iiop.IntegrationTests {

    [SupportedInterfaceAttribute(typeof(TestEchoInterface))]
    public class TestAbstrInterfaceImplByMarshalByRef : MarshalByRefObject, TestEchoInterface {
        public System.Int32 EchoInt(System.Int32 arg) {
            return arg;
        }
    }

    [SupportedInterfaceAttribute(typeof(TestService))]
    public class TestServiceImpl : MarshalByRefObject, TestService {

        private System.Double m_propValue = 0;

        private System.Int32 m_context = 0;
        
        public System.Double TestProperty {
            get {
                return m_propValue;
            }
            set {
                m_propValue = value;
            }
        }
        
        public System.Double TestReadOnlyPropertyReturningZero {
            get {
                return 0;
            }
        }
        
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

        public System.SByte TestIncSByte(System.SByte arg) {
            return (System.SByte)(arg + 1);
        }

        public System.UInt16 TestIncUInt16(System.UInt16 arg) {
            return (System.UInt16)(arg + 1);
        }

        public System.UInt32 TestIncUInt32(System.UInt32 arg) {
            return arg + 1;
        }

        public System.UInt64 TestIncUInt64(System.UInt64 arg) {
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

        public TestFlags TestEchoFlagsVal(TestFlags arg) {
            return arg;
        }

        public TestEnumBI16 TestEchoEnumI16Val(TestEnumBI16 arg) {
            return arg;
        }

        public TestEnumBI64 TestEchoEnumI64Val(TestEnumBI64 arg) {
            return arg;
        }

        public TestEnumUI32 TestEchoEnumUI32Val(TestEnumUI32 arg) {
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

        public System.String[] CreateTwoElemStringArray(System.String arg1, System.String arg2) {
            System.String[] result = new System.String[2];
            result[0] = arg1;
            result[1] = arg2;
            return result;
        }
        
        public System.Int32[][] EchoJaggedIntArray(System.Int32[][] arg) {
            return arg;
        }
        
        public System.String[][] EchoJaggedStringArray(System.String[][] arg) {
            return arg;
        }
        
        public System.Byte[][][] EchoJaggedByteArray(System.Byte[][][] arg) {
            return arg;
        }
        
        public System.Int32[,] EchoMultiDimIntArray(System.Int32[,] arg) {
            return arg;
        }
        
        public System.Byte[,,] EchoMultiDimByteArray(System.Byte[,,] arg) {
            return arg;
        }
        
        public System.String[,] EchoMultiDimStringArray(System.String[,] arg) {
            return arg;
        }

        public Adder RetrieveAdder() {
            return new Adder();
        }

        public System.Int32 AddWithAdder(Adder adder, System.Int32 sum1, System.Int32 sum2) {
            return adder.Add(sum1, sum2);
        }

        public TestStructA TestEchoStruct(TestStructA arg) {
            return arg;
        }

        public TestSerializableClassB2 TestChangeSerializableB2(TestSerializableClassB2 arg, System.String detail) {
            arg.DetailedMsg = detail;
            return arg;
        }

        public TestSerializableClassC TestEchoSerializableC(TestSerializableClassC arg) {
            return arg;
        }
        
        public TestNonSerializableBaseClass TestAbstractValueTypeEcho(TestNonSerializableBaseClass arg) {
            return arg;
        }

        public TestSerializableClassD TestChangeSerilizableD(TestSerializableClassD arg, System.String newMessage) {
            arg.val1.Msg = newMessage;
            return arg;
        }
        
        public TestSerializableClassE TestEchoSerializableE(TestSerializableClassE arg) {
            return arg;
        }

        public TestEchoInterface RetrieveEchoInterfaceImplementor() {
            return new TestAbstrInterfaceImplByMarshalByRef();
        }

        public TestInterfaceA RetrieveTestInterfaceAImplementor(System.String initialMsg) {
            return new TestAbstrInterfaceImplByMarshalByVal(initialMsg);
        }

        public System.String ExtractMsgFromInterfaceAImplmentor(TestInterfaceA arg) {
            return arg.Msg;
        }

        public TestAbstrInterfaceImplByMarshalByVal RetriveTestInterfaceAImplemtorTheImpl(System.String initialMsg) {
            return new TestAbstrInterfaceImplByMarshalByVal(initialMsg);
        }

        public object EchoAnything(object arg) {
            return arg;
        }

        public Any EchoAnythingContainer(Any arg) {
            return arg;
        }

        public System.Int32 TestRef(ref System.Int32 argRef) {
            argRef += 1;
            return argRef;
        }

        public System.Int32 TestOut(System.Int32 inArg, out System.Int32 argOut) {
            argOut = inArg;
            return inArg;
        }
        
        public void Assign5ToOut(out System.Int32 argOut) {
            argOut = 5;
        }

        public int AddOverloaded(int arg1, int arg2) {
            return arg1 + arg2;
        }

        public double AddOverloaded(double arg1, double arg2) {
            return arg1 + arg2;
        }

        public int AddOverloaded(int arg1, int arg2, int arg3) {
            return arg1 + arg2 + arg3;
        }

        public System.Int32 context {
            get {
                return m_context;
            }
            set {
                m_context = value;
            }
        }

        public System.Int32 custom(System.Int32 arg) {
            return arg;
        }

        public System.Int32 _echoInt(System.Int32 arg) {
            return arg;
        }
        
        /// <summary>checks, if inherited parameter attributes are considered correctly</summary>
        public System.String CheckParamAttrs(System.String arg) {
            return arg;
        }

        public TestUnion EchoUnion(TestUnion arg) {
            return arg;
        }

        public TestUnionULong EchoUnionULong(TestUnionULong arg) {
            return arg;
        }

        public TestUnionE EchoUnionE(TestUnionE arg) {
            return arg;
        }

        public object RetrieveUnknownUnionAsAny() {
            TestUnionE2 arg = new TestUnionE2();
            short case0Val = 11;
            arg.SetvalE0(case0Val);
            return arg;
        }

        
        /// <summary>
        /// used to check, if a reference passed is equal to this object itself.
        /// </summary>
        public bool CheckEqualityWithService(MarshalByRefObject toCheck) {
            return toCheck.Equals(this);
        }
        
        public bool CheckEqualityWithServiceV2(TestService toCheck) {
            return toCheck.Equals(this);
        }
        
        public override object InitializeLifetimeService() {
            // live forever
            return null;
        }

        public TestSimpleInterface1 GetSimpleService1() {
            return new TestSimpleIfImpl();
        }

        public TestSimpleInterface2 GetSimpleService2() {
            return new TestSimpleIfImpl();            
        }

        public TestSimpleInterface1 GetWhenSuppIfMissing() {
            return new TestSimpleIfImplMissingSupIf();            
        }

        public Adder CreateNewWithSystemID() {
            return new Adder();
        }
        
        public Adder CreateNewWithUserID(string userId) {
            Adder result = new Adder();
            RemotingServices.Marshal(result, userId);
            return result;
        }

        public string GetIorStringForThisObject() {
            OrbServices orbServices = OrbServices.GetSingleton();
            return orbServices.object_to_string(this);
        }

        public void GetAllUsagerType() {
        }

        public Type EchoType(Type arg) {
            return arg;
        }
    }

    [SupportedInterfaceAttribute(typeof(TestSimpleInterface1))]
    public class TestSimpleIfImpl : MarshalByRefObject, TestSimpleInterface1, TestSimpleInterface2 {

        public bool ReturnTrue() {
            return true;
        }

        public bool ReturnFalse() {
            return false;
        }

    }

    public class TestSimpleIfImplMissingSupIf : MarshalByRefObject, TestSimpleInterface1, TestSimpleInterface2 {

        public bool ReturnTrue() {
            return true;
        }

        public bool ReturnFalse() {
            return false;
        }

    }



}
