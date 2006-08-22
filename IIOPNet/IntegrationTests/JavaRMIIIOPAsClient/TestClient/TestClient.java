import junit.framework.*;
import javax.naming.NamingException;
import javax.naming.InitialContext;
import javax.naming.Context;
import javax.rmi.PortableRemoteObject;
import Ch.Elca.Iiop.IntegrationTests.*;


/**
 * Integration test for IIOP.NET.
 *
 */
public class TestClient extends TestCase {

    private TestService m_testService;

    public static void main (String[] args) {
        junit.textui.TestRunner.run (suite());
    }
    protected void setUp() throws Exception {
        Context ic = new InitialContext();
        Object objRef = ic.lookup("test");
        m_testService = (TestService) PortableRemoteObject.narrow(objRef, TestService.class);
    }

    protected void tearDown() {
        m_testService = null;     
    }

    public static Test suite() {
        return new TestSuite(TestClient.class);
    }

        
    public void testDouble() throws Exception {
        double arg = 1.23;
        double result = m_testService.TestIncDouble(arg);
        assertTrue(((double)(arg + 1)) == result);
    }

    public void testFloat() throws Exception {
        float arg = 1.23f;
        float result = m_testService.TestIncFloat(arg);
        assertTrue(((float)(arg + 1)) == result);
    }
        
    public void testByte() throws Exception {
        byte arg = 1;
        byte result = m_testService.TestIncByte(arg);
        assertEquals(((byte)(arg + 1)), result);
    }

    public void testInt16() throws Exception {
        short arg = 1;
        short result = m_testService.TestIncInt16(arg);
        assertEquals(((short)(arg + 1)), result);
        arg = -11;
        result = m_testService.TestIncInt16(arg);
        assertEquals(((short)(arg + 1)), result);
    }

    public void testInt32() throws Exception {
        int arg = 1;
        int result = m_testService.TestIncInt32(arg);
        assertEquals(((int)(arg + 1)), result);
        arg = -11;
        result = m_testService.TestIncInt32(arg);
        assertEquals(((int)(arg + 1)), result);
    }

    public void testInt64() throws Exception {
        long arg = 1;
        long result = m_testService.TestIncInt64(arg);
        assertEquals((long)(arg + 1), result);
        arg = -11;
        result = m_testService.TestIncInt64(arg);
        assertEquals((long)(arg + 1), result);
    }

    public void testUInt16() throws Exception {
        short arg = 1;
        short result = m_testService.TestIncUInt16(arg);
        assertEquals(((short)(arg + 1)), result);
        arg = -11;
        result = m_testService.TestIncUInt16(arg);
        assertEquals(((short)(arg + 1)), result);
    }

    public void testUInt32() throws Exception {
        int arg = 1;
        int result = m_testService.TestIncUInt32(arg);
        assertEquals(((int)(arg + 1)), result);
        arg = -11;
        result = m_testService.TestIncUInt32(arg);
        assertEquals(((int)(arg + 1)), result);
    }

    public void testUInt64() throws Exception {
        long arg = 1;
        long result = m_testService.TestIncUInt64(arg);
        assertEquals((long)(arg + 1), result);
        arg = -11;
        result = m_testService.TestIncUInt64(arg);
        assertEquals((long)(arg + 1), result);
    }

    public void testBoolean() throws Exception {
         boolean arg = true;
         boolean result = m_testService.TestNegateBoolean(arg);
         assertEquals(false, result);
    }

    public void testVoid() throws Exception {
        m_testService.TestVoid();
    }
        
    public void testChar() throws Exception {
        char arg = 'a';
        char result = m_testService.TestEchoChar(arg);
        assertEquals(arg, result);
        arg = '0';
        result = m_testService.TestEchoChar(arg);
        assertEquals(arg, result);
    }
        
    public void testString() throws Exception {
        String arg = "test";
        String toAppend = "toAppend";
        String result = m_testService.TestAppendString(arg, toAppend);
        assertEquals((arg + toAppend), result);
        arg = "test";
        toAppend = null;
        result = m_testService.TestAppendString(arg, toAppend);
        assertEquals(arg, result);
    }

    public void testByteArray() throws Exception {
        byte[] arg = new byte[1];
        arg[0] = 1;
        byte toAppend = 2;
        byte[] result = m_testService.TestAppendElementToByteArray(arg, toAppend);
        assertEquals(2, result.length);
        assertEquals((byte) 1, result[0]);
        assertEquals((byte) 2, result[1]);

        arg = null;
        toAppend = 3;
        result = m_testService.TestAppendElementToByteArray(arg, toAppend);
        assertEquals(1, result.length);
        assertEquals((byte) 3, result[0]);
    }

    public void testStringArray() throws Exception {
        String arg1 = "abc";
        String arg2 = "def";
        String[] result = m_testService.CreateTwoElemStringArray(arg1, arg2);
        assertEquals(arg1, result[0]);
        assertEquals(arg2, result[1]);
            
        String[] arg = new String[1];
        arg[0] = "abc";
        String toAppend = "def";
        result = m_testService.TestAppendElementToStringArray(arg, toAppend);
        assertEquals(2, result.length);
        assertEquals("abc", result[0]);
        assertEquals("def", result[1]);

        arg = null;
        toAppend = "hik";
        result = m_testService.TestAppendElementToStringArray(arg, toAppend);
        assertEquals(1, result.length);
        assertEquals("hik", result[0]);
    }
        
    public void testJaggedArrays() throws Exception {
        int[][] arg1 = new int[2][];
        arg1[0] = new int[] { 1 };
        arg1[1] = new int[] { 2, 3 };
        int[][] result1 = m_testService.EchoJaggedIntArray(arg1);
        assertEquals(2, result1.length);
        assertNotNull(result1[0]);
        assertNotNull(result1[1]);
        assertEquals(arg1[0][0], result1[0][0]);
        assertEquals(arg1[1][0], result1[1][0]);
        assertEquals(arg1[1][1], result1[1][1]);
             
        byte[][][] arg2 = new byte[3][][];
        arg2[0] = new byte[][] { new byte[] { 1 } };
        arg2[1] = new byte[][] { new byte[0] };
        arg2[2] = new byte[0][];
        byte[][][] result2 = m_testService.EchoJaggedByteArray(arg2);
        assertEquals(3, result2.length);
        assertNotNull(result2[0]);
        assertNotNull(result2[1]);
        assertNotNull(result2[2]);
        assertEquals(arg2[0][0][0], result2[0][0][0]);
    }

    public void testJaggedArraysWithNullElems() throws Exception {
        int[][] arg1 = null;
        int[][] result1 = m_testService.EchoJaggedIntArray(arg1);
        assertEquals(arg1, result1);

        int[][] arg2 = new int[2][];
        int[][] result2 = m_testService.EchoJaggedIntArray(arg2);
        assertNotNull(result2);

        String[][] arg3 = null;
        String[][] result3 = m_testService.EchoJaggedStringArray(arg3);
        assertEquals(arg3, result3);

        String[][] arg4 = new String[][] { null, new String[] { "abc", "def" } };
        String[][] result4 = m_testService.EchoJaggedStringArray(arg4);
        assertNotNull(result4);
        assertNull(result4[0]);
        assertNotNull(result4[1]);
        assertEquals(result4[1][0], arg4[1][0]);
        assertEquals(result4[1][1], arg4[1][1]);
    }

        
    public void testJaggedStringArrays() throws Exception {
        String[][] arg1 = new String[2][];
        arg1[0] = new String[] { "test" };
        arg1[1] = new String[] { "test2", "test3" };
        String[][] result1 = m_testService.EchoJaggedStringArray(arg1);
        assertEquals(2, result1.length);
        assertNotNull(result1[0]);
        assertNotNull(result1[1]);
        assertEquals(arg1[0][0], result1[0][0]);
        assertEquals(arg1[1][0], result1[1][0]);
        assertEquals(arg1[1][1], result1[1][1]);
    }
        
    public void testRemoteObjects() throws Exception {
        Adder adder = m_testService.RetrieveAdder();
        int arg1 = 1;
        int arg2 = 2;
        int result = adder.Add(1, 2);
        assertEquals((int) arg1 + arg2, result);            
    }

    public void testSendRefOfAProxy() throws Exception {
        Adder adder = m_testService.RetrieveAdder();
        int arg1 = 1;
        int arg2 = 2;
        int result = m_testService.AddWithAdder(adder, arg1, arg2);
        assertEquals((int) arg1 + arg2, result);
    }

    /// <summary>
    /// Checks, if the repository id of the value-type itself is used and not the rep-id 
    /// for the implementation class
    /// </summary>
    public void testTypeOfValueTypePassed() throws Exception {
        TestSerializableClassB2Impl arg = new TestSerializableClassB2Impl();
        arg.Msg = "msg";            
        TestSerializableClassB2 result = m_testService.TestChangeSerializableB2(arg, arg.DetailedMsg);
        assertEquals(result.Msg, arg.Msg);
    }
        
    /// <summary>
    /// Checks, if the fields of a super-type are serilised too
    /// </summary>
    public void testValueTypeInheritance() throws Exception {
        TestSerializableClassB2 arg = new TestSerializableClassB2Impl();
        arg.Msg = "msg";
        String newDetail = "new detail";
        TestSerializableClassB2 result = m_testService.TestChangeSerializableB2(arg, newDetail);
        assertEquals(newDetail, result.DetailedMsg);
        assertEquals(arg.Msg, result.Msg);
    }
    
    /// <summary>
    /// checks, if recursive values are serialised using an indirection
    /// </summary>
    public void testRecursiveValueType() throws Exception {
        TestSerializableClassE arg = new TestSerializableClassEImpl();
        arg.RecArrEntry = new TestSerializableClassE[1];
        arg.RecArrEntry[0] = arg;
        TestSerializableClassE result = m_testService.TestEchoSerializableE(arg);
        assertNotNull(result);
        assertNotNull(result.RecArrEntry);
        assertEquals(arg.RecArrEntry.length, result.RecArrEntry.length);
        assertTrue("invalid entry in recArrEntry", (result == result.RecArrEntry[0]));            
    }


/* java has problems with the following two tests, because it can't serialise Impl  */

//    /// <summary>
//    /// Checks, if a formal parameter type, which is not Serilizable works correctly,
//    /// if an instance of a Serializable subclass is passed.
//    /// </summary>
//    public void testNonSerilizableFormalParam() throws Exception {
//        TestNonSerializableBaseClass arg = new TestSerializableClassCImpl();
//        TestNonSerializableBaseClass result = m_testService.TestAbstractValueTypeEcho(arg);
//        assertTrue(result instanceof TestSerializableClassCImpl);
//    }

//    public void testBaseTypeNonSerializableParam() throws Exception {
//        TestSerializableClassC arg = new TestSerializableClassCImpl();
//        arg.Msg = "test";
//        TestSerializableClassC result = m_testService.TestEchoSerializableC(arg);
//        assertEquals(arg.Msg, result.Msg);
//        // check method implementation called
//        assertEquals(result.Msg, result.Format());
//    }

    /// <summary>
    /// Checks, if fields with reference semantics retain their semantic during serialisation / deserialisation
    /// </summary>
    public void testReferenceSematicForValueTypeField() throws Exception {
        TestSerializableClassD arg = new TestSerializableClassDImpl();
        arg.val1 = new TestSerializableClassB1Impl();
        arg.val1.Msg = "test";
        arg.val2 = arg.val1;
        String newMsg = "test-new";
        TestSerializableClassD result = m_testService.TestChangeSerilizableD(arg, newMsg);
        assertEquals(newMsg, result.val1.Msg);
        assertEquals(result.val1, result.val2);
        assertEquals(result.val1.Msg, result.val2.Msg);
        assertTrue(result.val1 == result.val2);
    }
       
    public void testProperty() throws Exception {
        double arg = 10;
        m_testService.TestProperty(arg);
        double newVal = m_testService.TestProperty();
        assertTrue(arg == newVal);
    }

    public void testFragments() throws Exception {
        // use a really big argument to force fragmentation at server side
        int size = 16000;
        byte[] hugeArg = new byte[size];
        for (int i = 0; i < size; i++) {
            hugeArg[i] = (byte)(i % 256);
        }
            
        byte[] result = m_testService.TestAppendElementToByteArray(hugeArg, (byte)(size % 256));
        assertEquals(result.length, size + 1);
        for (int i = 0; i < size + 1; i++) {
           assertEquals((byte)(i % 256), result[i]);
        }
    }

    public void testOneWay() throws Exception {
        int arg = 21;
        m_testService.TestOneWay(arg);
        // caution: no response -> wait some time to make sure call is complete
        Thread.sleep(100);
        int result = m_testService.LastTestOneWayArg();
        assertEquals(arg, result);
    }
    
    public void testCheckParamAttrs() throws Exception {
        String arg = "testArg";
        String result = m_testService.CheckParamAttrs(arg);
        assertEquals(arg, result);
    }

    public void testIntOutArg() throws Exception {
        int arg = 22;
        org.omg.CORBA.IntHolder result = new org.omg.CORBA.IntHolder();
        m_testService.EchoIntByOut(arg, result);
        assertEquals(arg, result.value);
    }

    public void testStringOutArg() throws Exception {
        String arg = "test1";
        org.omg.CORBA.StringHolder result = new org.omg.CORBA.StringHolder();
        m_testService.EchoByOut(arg, result);
        assertEquals(arg, result.value);
    }

    public void testFlags() throws Exception {
        int arg = 1;
        int result = m_testService.TestEchoFlagsVal(arg);
        assertEquals(arg, result);
        arg = 3;
        result = m_testService.TestEchoFlagsVal(arg);
        assertEquals(arg, result);
    }

    public void testEnum() throws Exception {
        TestEnum arg = TestEnum.TestEnum_A;
        TestEnum result = m_testService.TestEchoEnumVal(arg);
        assertEquals(arg, result);
    }

    public void testEnumBI16() throws Exception {
        TestEnumBI16 arg = TestEnumBI16.TestEnumBI16_B1;
        TestEnumBI16 result = m_testService.TestEchoEnumI16Val(arg);
        assertEquals(arg, result);
    }

    public void testEnumBUI32() throws Exception {
        TestEnumUI32 arg = TestEnumUI32.TestEnumUI32_C2;
        TestEnumUI32 result = m_testService.TestEchoEnumUI32Val(arg);
        assertEquals(arg, result);
    }

    public void testEnumBI64() throws Exception {
        TestEnumBI64 arg = TestEnumBI64.TestEnumBI64_AL;
        TestEnumBI64 result = m_testService.TestEchoEnumI64Val(arg);
        assertEquals(arg, result);

        arg = TestEnumBI64.TestEnumBI64_BL;
        result = m_testService.TestEchoEnumI64Val(arg);
        assertEquals(arg, result);
    }

}
