/* TestServiceCommon.cs
 *
 * Project: IIOP.NET
 * IntegrationTests
 *
 * WHEN      RESPONSIBLE
 * 25.07.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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

    public enum TestEnum {
        A, B, C, D
    }

    [Flags]
    public enum TestFlags {
        A1 = 0x01, B1 = 0x02, C1 = 0x04, D1 = 0x08, All = A1 | B1 | C1 | D1
    }

    public enum TestEnumBI16 : short {
        A1, B1, C1
    }

    public enum TestEnumBI64 : long {
        AL = Int64.MaxValue, BL = 1000
    }

    public enum TestEnumUI32 : uint {
        A2, B2 = 10, C2 = 20
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

        System.SByte TestIncSByte(System.SByte arg);

        System.UInt16 TestIncUInt16(System.UInt16 arg);

        System.UInt32 TestIncUInt32(System.UInt32 arg);

        System.UInt64 TestIncUInt64(System.UInt64 arg);

        System.Boolean TestNegateBoolean(System.Boolean arg);

        void TestVoid();
        
        System.Char TestEchoChar(System.Char arg);

        System.String TestAppendString(System.String basic, System.String toAppend);

        TestEnum TestEchoEnumVal(TestEnum arg);

        TestEnumBI16 TestEchoEnumI16Val(TestEnumBI16 arg);

        TestEnumBI64 TestEchoEnumI64Val(TestEnumBI64 arg);        

        TestEnumUI32 TestEchoEnumUI32Val(TestEnumUI32 arg);

        TestFlags TestEchoFlagsVal(TestFlags arg);

        System.Byte[] TestAppendElementToByteArray(System.Byte[] arg, System.Byte toAppend);

        System.String[] TestAppendElementToStringArray(System.String[] arg, System.String toAppend);

        System.String[] CreateTwoElemStringArray(System.String arg1, System.String arg2);
        
        System.Int32[][] EchoJaggedIntArray(System.Int32[][] arg);
        
        System.String[][] EchoJaggedStringArray(System.String[][] arg);
        
        System.Byte[][][] EchoJaggedByteArray(System.Byte[][][] arg);
        
        System.Int32[,] EchoMultiDimIntArray(System.Int32[,] arg);
        
        System.Byte[,,] EchoMultiDimByteArray(System.Byte[,,] arg);
        
        System.String[,] EchoMultiDimStringArray(System.String[,] arg);

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

        Any EchoAnythingContainer(Any arg);

        System.Int32 TestRef(ref System.Int32 argRef);

        System.Int32 TestOut(System.Int32 inArg, out System.Int32 argOut);
        
        void Assign5ToOut(out System.Int32 argOut);

        int AddOverloaded(int arg1, int arg2);

        double AddOverloaded(double arg1, double arg2);

        int AddOverloaded(int arg1, int arg2, int arg3);

        /// <summary>
        /// a property with a name, which clashes with an IDL keyword
        /// </summary>
        System.Int32 context {
            get;
            set;                
        }

        /// <summary>
        /// a method with a name, which clashes with an IDL keyword
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        System.Int32 custom(System.Int32 arg);

        /// <summary>
        /// used to check special case mapping for ids, starting with _
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        System.Int32 _echoInt(System.Int32 arg);
        
        
        [return: StringValue]
        [return: WideChar(false)]
        String CheckParamAttrs([StringValue][WideChar(false)]String arg);

        TestUnion EchoUnion(TestUnion arg);

        TestUnionULong EchoUnionULong(TestUnionULong arg);

        /// <summary>
        /// echos a union, which has an enumeration discriminator
        /// </summary>
        TestUnionE EchoUnionE(TestUnionE arg);

        object RetrieveUnknownUnionAsAny();
        
        /// <summary>
        /// used to check, if a reference passed is equal to this object itself.
        /// </summary>
        bool CheckEqualityWithService(MarshalByRefObject toCheck);
        
        bool CheckEqualityWithServiceV2(TestService toCheck);

        TestSimpleInterface1 GetSimpleService1();
        TestSimpleInterface2 GetSimpleService2();

        TestSimpleInterface1 GetWhenSuppIfMissing();
        
        Adder CreateNewWithSystemID();
        Adder CreateNewWithUserID(string userId);

        /// <summary>returns the IOR for the instance of MBR implementing this interface</summary>
        string GetIorStringForThisObject();

        /// <summary>BAD_OPERATION ERROR REPORT</summary>
        void GetAllUsagerType();

        Type EchoType(Type arg);
        
    }

    
    /// <summary>Simple interface used to check obj-ref deserialisation, if compatibility is not
    /// checkable without server object help</summary>
    public interface TestSimpleInterface1 {
        bool ReturnTrue();
    }

    /// <summary>Simple interface used to check obj-ref deserialisation, if compatibility is not
    /// checkable without server object help</summary>
    public interface TestSimpleInterface2 {
        bool ReturnFalse();
    }

}

