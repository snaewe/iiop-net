/* TestService.cs
 *
 * Project: IIOP.NET
 * IntegrationTests
 *
 * WHEN      RESPONSIBLE
 * 31.07.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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

namespace Ch.Elca.Iiop.IntegrationTests {

    public enum TestEnum {
        A, B, C, D
    }

    [Flags]
    public enum TestFlags {
        AF = 0x01, BF = 0x02, CF = 0x04
    }

    public enum TestEnumBI16 : short {
        A1, B1, C1
    }

    public enum TestEnumBI64 : long {
        AL = Int64.MaxValue, BL = 1000
    }

    public enum TestEnumUI32 : uint {
        A2, B2, C2
    }

    [Serializable]
    public struct TestStructA {
        public System.Int32 X;
        public System.Int32 Y;
    }

    [Serializable]
    public class TestSerializableClassB1 {
        public System.String Msg;
    }
    
    [Serializable]
    public class TestSerializableClassB2 : TestSerializableClassB1 {
        public System.String DetailedMsg;
    }

    public abstract class TestNonSerializableBaseClass {
        public abstract System.String Format();
    }

    [Serializable]
    public class TestSerializableClassC : TestNonSerializableBaseClass {
        
        public String Msg;
        
        public override System.String Format() {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    public class TestSerializableClassD {
        public TestSerializableClassB1 val1;
        public TestSerializableClassB1 val2;
    }
    
    [Serializable]
    public class TestSerializableClassE {
        public TestSerializableClassE[] RecArrEntry;
    }

    public class Adder : MarshalByRefObject {
        public System.Int32 Add(System.Int32 sum1, System.Int32 sum2) {
            return sum1 + sum2;
        }
    }

    public interface TestEchoInterface {
        System.Int32 EchoInt(System.Int32 arg);
    }

    [SupportedInterfaceAttribute(typeof(TestEchoInterface))]
    public class TestAbstrInterfaceImplByMarshalByRef : MarshalByRefObject, TestEchoInterface {
        public System.Int32 EchoInt(System.Int32 arg) {
            return arg;
        }
    }

    public interface TestInterfaceA {
        System.String Msg {
            get;
        }
    }

    [Serializable]
    public class TestAbstrInterfaceImplByMarshalByVal : TestInterfaceA {
        
        private System.String m_msg = "standard";

        public TestAbstrInterfaceImplByMarshalByVal() {
        }

        public TestAbstrInterfaceImplByMarshalByVal(System.String msg) {
            m_msg = msg;
        }

        public System.String Msg {
            get {
                return m_msg;
            }
        }

    }

    [SupportedInterface(typeof(TestService))]
    public class TestServiceImpl : MarshalByRefObject, TestService {

        private System.Double m_propValue = 0;

        private System.Int32 m_lastOneWay;
        
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

        public System.UInt16 TestIncUInt16(System.UInt16 arg) {
            return (System.UInt16)(arg + 1);
        }

        public System.UInt32 TestIncUInt32(System.UInt32 arg) {
            return arg + (System.UInt32)1;
        }

        public System.UInt64 TestIncUInt64(System.UInt64 arg) {
            return arg + (System.UInt64)1;
        }

        public System.Boolean TestNegateBoolean(System.Boolean arg) {
            return ! arg;
        }

        public void TestVoid() {
            return;
        }

        [OneWay]
        public void TestOneWay(System.Int32 arg) {
            m_lastOneWay = arg;
            return;
        }

        public System.Int32 LastTestOneWayArg() {
            return m_lastOneWay;
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

        /// <summary>checks, if inherited parameter attributes are considered correctly</summary>
        public System.String CheckParamAttrs(System.String arg) {
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

        public void EchoByOut(string arg, out string result) {
            result = arg;
        }

//        public void EchoByRef(ref string result) {
//        }

        public void EchoIntByOut(int arg, out int result) {
            result = arg;
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
        
        public override object InitializeLifetimeService() {
            // live forever
            return null;
        }

    }

    public interface TestService {
 
        System.Double TestProperty {
            get;
            set;
        }
        
        System.Double TestReadOnlyPropertyReturningZero {
            get;
        }
        
        System.Double TestIncDouble(System.Double arg);

        System.Single TestIncFloat(System.Single arg);

        System.Byte TestIncByte(System.Byte arg);

        System.Int16 TestIncInt16(System.Int16 arg);

        System.Int32 TestIncInt32(System.Int32 arg);

        System.Int64 TestIncInt64(System.Int64 arg);

        System.UInt16 TestIncUInt16(System.UInt16 arg);

        System.UInt32 TestIncUInt32(System.UInt32 arg);

        System.UInt64 TestIncUInt64(System.UInt64 arg);

        System.Boolean TestNegateBoolean(System.Boolean arg);

        void TestVoid();

        [OneWay]
        void TestOneWay(System.Int32 arg);

        System.Int32 LastTestOneWayArg();
        
        System.Char TestEchoChar(System.Char arg);

        System.String TestAppendString(System.String basic, System.String toAppend);

        TestEnum TestEchoEnumVal(TestEnum arg);

        System.Byte[] TestAppendElementToByteArray(System.Byte[] arg, System.Byte toAppend);

        System.String[] TestAppendElementToStringArray(System.String[] arg, System.String toAppend);

        System.String[] CreateTwoElemStringArray(System.String arg1, System.String arg2);
       
        System.Int32[][] EchoJaggedIntArray(System.Int32[][] arg);
        
        System.String[][] EchoJaggedStringArray(System.String[][] arg);
        
        System.Byte[][][] EchoJaggedByteArray(System.Byte[][][] arg);
       
        Adder RetrieveAdder();

        System.Int32 AddWithAdder(Adder adder, System.Int32 sum1, System.Int32 sum2);

        TestStructA TestEchoStruct(TestStructA arg);

        TestSerializableClassB2 TestChangeSerializableB2(TestSerializableClassB2 arg, System.String detail);

        TestSerializableClassC TestEchoSerializableC(TestSerializableClassC arg);
        
        TestNonSerializableBaseClass TestAbstractValueTypeEcho(TestNonSerializableBaseClass arg);

        TestSerializableClassD TestChangeSerilizableD(TestSerializableClassD arg, System.String newMessage);

        TestSerializableClassE TestEchoSerializableE(TestSerializableClassE arg);

        TestEchoInterface RetrieveEchoInterfaceImplementor();

        TestInterfaceA RetrieveTestInterfaceAImplementor(System.String initialMsg);

        System.String ExtractMsgFromInterfaceAImplmentor(TestInterfaceA arg);

        TestAbstrInterfaceImplByMarshalByVal RetriveTestInterfaceAImplemtorTheImpl(System.String initialMsg);

        object EchoAnything(object arg);

        /// <summary>checks, if inherited parameter attributes are considered correctly</summary>        
        [return: StringValue]
        [return: WideChar(false)]
        System.String CheckParamAttrs([StringValue][WideChar(false)]System.String arg);
        
        void EchoByOut([StringValue] string arg, [StringValue] out string result);

        // the following leads to java idlj problem, because of string byref argument
        // void EchoByRef(ref string result);

        void EchoIntByOut(int arg, out int result);

        TestFlags TestEchoFlagsVal(TestFlags arg);

        TestEnumBI16 TestEchoEnumI16Val(TestEnumBI16 arg);

        TestEnumBI64 TestEchoEnumI64Val(TestEnumBI64 arg);        

        TestEnumUI32 TestEchoEnumUI32Val(TestEnumUI32 arg);


    }


}
