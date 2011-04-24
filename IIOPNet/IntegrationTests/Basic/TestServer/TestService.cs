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
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Ch.Elca.Iiop.Idl;
using omg.org.CORBA;

namespace CCE {

    public interface Assembly {
    }

    [SupportedInterface(typeof(Assembly))]
    public class AssemblyImpl : MarshalByRefObject, Assembly {
    }

    public interface _Assembly {
    }

    [SupportedInterface(typeof(_Assembly))]
    public class _AssemblyImpl : MarshalByRefObject, _Assembly {
    }

}

namespace Ch.Elca.Iiop.IntegrationTests {

    [IdlEnum]
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
    [IdlStruct]
    public struct TestStructAIdl {
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

    [IdlStruct]
    public struct SSensi { 
        public int ICode; 
        public int IDev; 
        [IdlSequence(0L)]
        public long[] Sensibilites;
    }


    [IdlStruct]
    public struct IdlArrayContainer {
        [IdlArray(0L, 5)]
        public int[] OneDimIntArray5;

        [IdlArray(0L, 2)][IdlArrayDimension(0L, 1, 2)] 
        public int[,] TwoDimIntArray2x2;
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
    
    /// <summary>don't use supported interface here, to check if client is able to detect,
    /// that the impl class is compatible with interface</summary>
    public class TestUnknownEchoInterfaceImpl : MarshalByRefObject, TestEchoInterface {
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

        public System.UInt16 TestIncUInt16(System.UInt16 arg) {
            return (System.UInt16)(arg + 1);
        }

        public System.UInt32 TestIncUInt32(System.UInt32 arg) {
            return arg + (System.UInt32)1;
        }

        public System.UInt64 TestIncUInt64(System.UInt64 arg) {
            return arg + (System.UInt64)1;
        }

        public System.SByte TestIncSByte(System.SByte arg) {
            return (System.SByte)(arg + 1);
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

        [return: IdlSequence(0L)]
        public System.Int32[] EchoIdlLongSequence([IdlSequence(0L)] System.Int32[] arg) {
            return arg;
        }

        [return: IdlSequence(0L, 10L)]
        [return: IdlSequence(1L)]
        public System.Int32[][] EchoIdlLongSequenceOfBoundedSequence([IdlSequence(0L, 10L)] [IdlSequence(1L)] System.Int32[][] arg) {
            return arg;
        }

        [return: IdlSequence(0L)]
        [return: IdlSequence(1L)]
        public System.Int32[][] EchoIdlLongSequenceOfSequence([IdlSequence(0L)] [IdlSequence(1L)] System.Int32[][] arg) {
            return arg;
        }

        [return: IdlSequence(0L)]
        public System.Int32[] AppendToIdlLongSequence([IdlSequence(0L)] System.Int32[] arg, System.Int32 toAppend) {
            System.Int32[] result = new System.Int32[arg.Length + 1]; // arg is not null, because not allowed for idl seq
            Array.Copy(arg, 0, result, 0, arg.Length);
            result[arg.Length] = toAppend;
            return result;
            
        }

        [return: IdlSequence(0L)]
        [return: StringValue()]
        [return: WideChar(false)]
        public System.String[] EchoIdlStringSequence([IdlSequence(0L)] [StringValue()] [WideChar(false)] System.String[] arg) {
            return arg;
        }

        [return: IdlSequence(0L)]
        [return: StringValue()]
        [return: WideChar(true)]
        public System.String[] EchoIdlWStringSequence([IdlSequence(0L)] [StringValue()] [WideChar(true)] System.String[] arg) {
            return arg;
        }

        [return: IdlSequence(0L)]
        [return: StringValue()]
        [return: WideChar(false)]
        public System.String[] AppendToIdlStringSequence([IdlSequence(0L)] [StringValue()] [WideChar(false)] System.String[] arg, 
                                                         [StringValue()] [WideChar(false)] System.String toAppend) {
            System.String[] result = new System.String[arg.Length + 1]; // arg is not null, because not allowed for idl seq
            Array.Copy(arg, 0, result, 0, arg.Length);
            result[arg.Length] = toAppend;
            return result;
            
        }

        [return: IdlArray(0L, 5)]
        public System.Int32[] EchoIdlLongArrayFixedSize5([IdlArray(0L, 5)] System.Int32[] arg) {
            return arg;
        }

        [return: IdlArray(0L, 5)]
        [return: IdlArrayDimension(0L, 1, 3)]
        public System.Int32[,] EchoIdlLongArray5times3([IdlArray(0L, 5)][IdlArrayDimension(0L, 1, 3)] System.Int32[,] arg) {
            return arg;
        }

        public IdlArrayContainer EchoIdlArrayContainer(IdlArrayContainer arrayContainer) {
            return arrayContainer;            
        }

        public object RetrieveIdlIntArrayAsAny([IdlArray(0L, 5)] int[] arg) {
            // test with explicit typecode-creation
            IOrbServices orbServices = OrbServices.GetSingleton();
            omg.org.CORBA.TypeCode arrayTC = 
                orbServices.create_array_tc(5, orbServices.create_tc_for_type(typeof(int)));
            Any arrayAsAny = new Any(arg, arrayTC);
            return arrayAsAny;
        }

        public object RetrieveIdlInt2DimArray2x2AsAny([IdlArray(0L, 2)][IdlArrayDimension(0L, 1, 2)] System.Int32[,] arg) {
            // test with explicit typecode-creation
            IOrbServices orbServices = OrbServices.GetSingleton();
            omg.org.CORBA.TypeCode innerArrayTC = 
                orbServices.create_array_tc(2, orbServices.create_tc_for_type(typeof(int)));
            omg.org.CORBA.TypeCode arrayTC = 
                orbServices.create_array_tc(2, innerArrayTC);
            Any arrayAsAny = new Any(arg, arrayTC);
            return arrayAsAny;
        }

        public object RetrieveIdlInt3DimArray2x2x3AsAny([IdlArray(0L, 2)][IdlArrayDimension(0L, 1, 2)][IdlArrayDimension(0L, 2, 3)] System.Int32[,,] arg) {
            // test with explicit typecode-creation
            IOrbServices orbServices = OrbServices.GetSingleton();
            omg.org.CORBA.TypeCode arrayTC = 
                orbServices.create_array_tc(3, orbServices.create_tc_for_type(typeof(int)));
            arrayTC = orbServices.create_array_tc(2, arrayTC);
            arrayTC = orbServices.create_array_tc(2, arrayTC);
            Any arrayAsAny = new Any(arg, arrayTC);
            return arrayAsAny;
        }

        public object[] EchoObjectArray(object[] arg) {
            return arg;
        }

        public Adder RetrieveAdder() {
            return new Adder();
        }

        public object RetrieveAdderAsAny() {
            return RetrieveAdder();
        }

        [return: ObjectIdlTypeAttribute(IdlTypeObject.AbstractBase)]
        public object RetrieveAdderForAbstractInterfaceBase() {
            return RetrieveAdder();
        }

        public System.Int32 AddWithAdder(Adder adder, System.Int32 sum1, System.Int32 sum2) {
            return adder.Add(sum1, sum2);
        }

        public TestStructA TestEchoStruct(TestStructA arg) {
            return arg;
        }

        public TestStructAIdl TestEchoIdlStruct(TestStructAIdl arg) {
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

        public TestEchoInterface RetrieveUnknownEchoInterfaceImplementor() {
            return new TestUnknownEchoInterfaceImpl();
        }

        public object RetrieveUnknownEchoInterfaceImplementorAsAny() {
            return RetrieveUnknownEchoInterfaceImplementor();
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

        [ThrowsIdlException(typeof(TestNetExceptionMappedToIdl))]
        public void ThrowKnownException() {
            TestNetExceptionMappedToIdl testEx = new TestNetExceptionMappedToIdl(10);            
            throw testEx;
        }
        
        [ThrowsIdlException(typeof(TestNetExceptionMappedToIdl))]        
        public void ThrowUnKnownException() {
            throw new NotSupportedException("test-ex");
        }

        [ContextElement("element1")]
        [return: StringValue()]
        [return: WideChar(false)]
        public string TestContextElementPassing() {
            string result = "";
            CorbaContextElement elem = CallContext.GetData("element1") as CorbaContextElement;
            if (elem != null) {
                result = elem.ElementValue;
            }
            return result;
        }

        public bool TestPropWithGetUserException {
            get {
                TestException testEx = new TestException();
                testEx.Msg = "test-msg";
                throw testEx;
            }
        }

        public bool TestPropWithGetSystemException {
            get {
                throw new omg.org.CORBA.INTERNAL(29, 
                                           omg.org.CORBA.CompletionStatus.Completed_Yes);
            }
        }

        public void TestDuplicateSeqOfSeqInOut([IdlSequence(0L)] ref SSensi[] arg) {
            if (arg != null) {
               SSensi[] result = new SSensi[arg.Length * 2];
               for (int i = 0; i < arg.Length; i++) {
                   result[i*2] = arg[i];
                   result[i*2 + 1] = arg[i];
               }
               arg = result;
            }
        }

        public CCE.Assembly CreateAsm() {
            return new CCE.AssemblyImpl();
        }

        public CCE._Assembly Create_Asm() {
            return new CCE._AssemblyImpl();
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
            result.Code = 1;
            throw result;
        }
        
        public bool ThrowDotNetException() {
            throw new Exception("dot-net-exception");
        }
        
        public bool ThrowSystemException() {
            throw new omg.org.CORBA.NO_IMPLEMENT(9, 
                                                 omg.org.CORBA.CompletionStatus.Completed_Yes);
        }        

        public bool TestAttrWithException {
            get {
                TestException result = new TestException();
                result.Msg = "test-msg";
                throw result;                             
            }
            set {
                throw new omg.org.CORBA.NO_IMPLEMENT(10, 
                                                     omg.org.CORBA.CompletionStatus.Completed_Yes);
            }
        }
        
        public override object InitializeLifetimeService() {
            // live forever
            return null;
        }
        
    }


    public class TestNetExceptionMappedToIdl : AbstractUserException {

        private int code;

        public TestNetExceptionMappedToIdl() : this(0) {
        }

        public TestNetExceptionMappedToIdl(int code) {
            this.code = code;
        }

    }


    [SupportedInterface(typeof(TestIdlTypesService))]
    public class TestIdlTypesServiceImpl : MarshalByRefObject, TestIdlTypesService {

        
        [return: BoxedValueAttribute("IDL:Ch.Elca.Iiop.IntegrationTests.boxed_string:1.0")]
        public string EchoBoxedString([BoxedValueAttribute("IDL:Ch.Elca.Iiop.IntegrationTests.boxed_string:1.0")] string arg) {
            return arg;
        }

        [return: BoxedValueAttribute("IDL:Ch.Elca.Iiop.IntegrationTests.boxed_TestStruct:1.0")]
        public TestStructWB EchoBoxedStruct([BoxedValueAttribute("IDL:Ch.Elca.Iiop.IntegrationTests.boxed_TestStruct:1.0")] TestStructWB arg) {
            return arg;
        }

        

        [return: BoxedValueAttribute("IDL:Ch/Elca/Iiop/IntegrationTests/ValidListSeq:1.0")]
        public Int32[]  EchoBoxedSeq([BoxedValueAttribute("IDL:Ch/Elca/Iiop/IntegrationTests/ValidListSeq:1.0")] Int32[] arg) {
            return arg;
        }

        public TestUnionLD EchoLDUnion(TestUnionLD arg) {
            return arg;
        }

        public void TestEmptySeqAlignment(ref int a, out long[] b, out long[] c, out string[] d) {
            b = new long[a];
            c = new long[a];
            d = new string[a];

            for(int i = 0; i < a; ++i) {
                b[i] = 0x797065656c536d49L;
                c[i] = 0x002da715A900dDa7L;
                d[i] = i.ToString();
            }

            a = 42;
        }
        
        public override object InitializeLifetimeService() {
            // live forever
            return null;
        }
        
    }


    [SupportedInterface(typeof(TestOneWayService))]
    public class TestOneWayServiceImpl : MarshalByRefObject, TestOneWayService {

        private int m_setWithVoid;
        private int m_setWithOneWay;


        public void SetArgumentVoid(int arg) {
            m_setWithVoid = arg;
        }
        
        public int GetArgumentVoid() {
            return m_setWithVoid;
        }

        [OneWay]
        public void SetArgumentOneWay(int arg) {
            m_setWithOneWay = arg;
        }

        public int GetArgumentOneWay() {
            return m_setWithOneWay;
        }
        
        public override object InitializeLifetimeService() {
            // live forever
            return null;
        }
        
    }

    public class TestServiceWithCallbackImpl : MarshalByRefObject, TestServiceWithCallback1
                                                                 /*, TestServiceWithCallback2 */ {
        private Timer asyncPingTimer;

        public void Ping1(int code, Callback1 callback) {
            callback.Pong(code);
        }
        
        // void TestServiceWithCallback2.Ping1(int code, Callback1 callback) {
            // callback.Pong(code * 2);
        // }

        public void Ping2(int code, Callback2 callback) {
            callback.Pong(code);
        }

        public void AsyncPing(int delayInSecs, Callback1 callback) {
            if (this.asyncPingTimer == null) {
                this.asyncPingTimer = new Timer(this.OnAsyncPingTime, callback, delayInSecs * 1000, Timeout.Infinite);
            }
        }

        private void OnAsyncPingTime(object state) {
            Callback1 callback = (Callback1)state;
            callback.Pong(0);
        }

        public override object InitializeLifetimeService() {
            // live forever
            return null;
        }
    }
}
