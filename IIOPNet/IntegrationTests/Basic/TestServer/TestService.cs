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

    public enum TestEnum {
        A, B, C, D
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

    public class TestService : MarshalByRefObject {

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
        
        public TestSerializableClassE TestEchoSerializableE(TestSerializableClassE arg) {
            return arg;
        }
        
        
        public TestNonSerializableBaseClass TestAbstractValueTypeEcho(TestNonSerializableBaseClass arg) {
            return arg;
        }

        public TestSerializableClassD TestChangeSerilizableD(TestSerializableClassD arg, System.String newMessage) {
            arg.val1.Msg = newMessage;
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

        public System.Int32 TestRef(ref System.Int32 argRef) {
            argRef += 1;
            return argRef;
        }

        public System.Int32 TestOut(System.Int32 inArg, out System.Int32 argOut) {
            argOut = inArg;
            return inArg;
        }
        
        /// <param name="outArg">result is inarg + 1</param>
        /// <param name="outArg2">result is inarg2 + 1</param>
        /// <param name="inoutArg">result is inoutArg * 2</param>
        /// <param name="inoutArg2">result is inoutArg2 * 2</param>
        /// <returns>inarg + inarg2</returns>
        public System.Int32 TestInOutRef(System.Int32 inarg, out System.Int32 outArg, ref System.Int32 inoutArg,
                                         System.Int32 inarg2, ref System.Int32 inoutArg2, out System.Int32 outArg2) {
            
            outArg = inarg + 1;
            outArg2 = inarg2 + 1;
            inoutArg = inoutArg * 2;
            inoutArg2 = inoutArg2 * 2;
            return inarg + inarg2;
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
        
        /// <summary>
        /// a property with a name, which clashes with an IDL keyword
        /// </summary>
        public System.Int32 context {
            get {
                return m_context;
            }
            set {
                m_context = value;
            }
        }

        /// <summary>
        /// a method with a name, which clashes with an IDL keyword
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public System.Int32 custom(System.Int32 arg) {
            return arg;
        }

        /// <summary>
        /// used to check special case mapping for ids, starting with _
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public System.Int32 _echoInt(System.Int32 arg) {
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

        public TestSimpleInterface1 GetSimpleService1() {
            return new TestSimpleIfImpl();
        }

        public TestSimpleInterface2 GetSimpleService2() {
            return new TestSimpleIfImpl();            
        }

        public TestSimpleInterface1 GetWhenSuppIfMissing() {
            return new TestSimpleIfImplMissingSupIf();            
        }

        public Type EchoType(Type arg) {
            return arg;
        }
        
        public override object InitializeLifetimeService() {
            // live forever
            return null;
        }
        
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

    [SupportedInterface(typeof(TestExceptionService))]
    public class TestExceptionServiceImpl : MarshalByRefObject, TestExceptionService {
        
        public bool ThrowTestException() {
            TestException result = new TestException();
            result.Msg = "test-msg";
            throw result;
        }
        
        public bool ThrowDotNetException() {
            throw new Exception("dot-net-exception");
        }
        
        public bool ThrowSystemException() {
            throw new omg.org.CORBA.NO_IMPLEMENT(9, 
                                                 omg.org.CORBA.CompletionStatus.Completed_Yes);
        }        
        
        public override object InitializeLifetimeService() {
            // live forever
            return null;
        }
        
    }

}
