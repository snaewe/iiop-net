/* TestServiceImpl.java
 *
 * Project: IIOP.NET
 * IntegrationTests
 *
 * WHEN      RESPONSIBLE
 * 14.07.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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

package Ch.Elca.Iiop.IntegrationTests;

import java.rmi.RemoteException;
import javax.rmi.PortableRemoteObject;
    

public class TestServiceImpl extends PortableRemoteObject implements TestService {

    private int m_val = 0;
    private int[] m_intSequence = new int[0];

    public TestServiceImpl() throws java.rmi.RemoteException {
        super(); // invoke rmi linking and remote object initialization
    }

    public double TestIncDouble(double arg) throws RemoteException {
        return arg + 1;
    }

    public float TestIncFloat(float arg) throws RemoteException {
        return arg + 1;
    }

    public byte TestIncByte(byte arg) throws RemoteException {
        return (byte)(arg + 1);
    }

    public short TestIncInt16(short arg) throws RemoteException {
        return (short)(arg + 1);
    }

    public int TestIncInt32(int arg) throws RemoteException {
        return arg + 1;
    }

    public long TestIncInt64(long arg) throws RemoteException {
        return arg + 1;
    }

    public boolean TestNegateBoolean(boolean arg) throws RemoteException {
        return ! arg;
    }

    public void TestVoid() throws RemoteException {
    }
        
    public char TestEchoChar(char arg) throws RemoteException {
        return arg;
    }

    public String TestAppendString(String basic, String toAppend) throws RemoteException {
        String result = "";
        if (basic != null) {
            result += basic;
        }
        if (toAppend != null) {
            result += toAppend;
        }
        return result;
    }

    public byte[] TestAppendElementToByteArray(byte[] arg, byte toAppend) throws RemoteException {
        byte[] result;
        if (arg != null) {
            result = new byte[arg.length + 1];
            System.arraycopy(arg, 0, result, 0, arg.length);
        } else {
            result = new byte[1];
        }
        result[result.length - 1] = toAppend;
        return result;
    }

    public long[] TestAppendElementToLongArray(long[] arg, long toAppend) throws RemoteException {
        long[] result;
        if (arg != null) {
            result = new long[arg.length + 1];
            System.arraycopy(arg, 0, result, 0, arg.length);
        } else {
            result = new long[1];
        }
        result[result.length - 1] = toAppend;
        return result;
    }

    public String[] TestAppendElementToStringArray(String[] arg, String toAppend) throws RemoteException {
        String[] result;
        if (arg != null) {
            result = new String[arg.length + 1];
            System.arraycopy(arg, 0, result, 0, arg.length);
        } else {
            result = new String[1];
        }
        result[result.length - 1] = toAppend;
        return result;
    }

    public String[] CreateTwoElemStringArray(String arg1, String arg2) throws RemoteException {
        String[] result = new String[2];
        result[0] = arg1;
        result[1] = arg2;
        return result;
    }
        
    public int[][] EchoJaggedIntArray(int[][] arg) throws RemoteException {
        return arg;
    }
        
    public String[][] EchoJaggedStringArray(String[][] arg) throws RemoteException {
        return arg;
    }
        
    public byte[][][] EchoJaggedByteArray(byte[][][] arg) throws RemoteException {
        return arg;
    }

    public NamedValue[] TestAppendElementToNamedValueArray(NamedValue[] arg, NamedValue toAppend) throws RemoteException {
        NamedValue[] result;
        if (arg != null) {
            result = new NamedValue[arg.length + 1];
            System.arraycopy(arg, 0, result, 0, arg.length);
        } else {
            result = new NamedValue[1];
        }
        result[result.length - 1] = toAppend;
        return result;        
    }
        
    public Adder RetrieveAdder() throws RemoteException {
        return new AdderImpl();
    }

    public int AddWithAdder(Adder adder, int sum1, int sum2) throws RemoteException {
        return adder.Add(sum1, sum2);
    }

    public TestSerializableClassB2 TestChangeSerializableB2(TestSerializableClassB2 arg, String detail) throws RemoteException {
        arg.DetailedMsg = detail;
        return arg;
    }

    public TestSerializableClassC TestEchoSerializableC(TestSerializableClassC arg) throws RemoteException {
        return arg;
    }
        
    public TestNonSerializableBaseClass TestAbstractValueTypeEcho(TestNonSerializableBaseClass arg) throws RemoteException {
        return arg;
    }

    public TestSerializableClassD TestChangeSerilizableD(TestSerializableClassD arg, String newMessage) throws RemoteException {
        arg.val1.Msg = newMessage;
        return arg;
    }
    
    public TestSerializableClassE TestEchoSerializableE(TestSerializableClassE arg) throws RemoteException {
        return arg;
    }

    public TestSerWithInner TestEchoWithInner(TestSerWithInner arg) throws RemoteException {
        return arg;
    }

    public TestSerializableMixedValAndBase TestMixedSerType(boolean arg1, short arg2, int arg3, String arg4) throws RemoteException {
        TestSerializableMixedValAndBase result = new TestSerializableMixedValAndBase();
        result.basicVal1 = arg1;
        result.basicVal2 = arg2;
        result.basicVal3 = arg3;
        result.val1 = new TestSerializableClassB1();
        result.val1.Msg = arg4;
        result.val2 = new TestSerializableClassB1();
        result.val2.Msg = arg4;
        result.val3 = new TestSerializableClassB1();
        result.val3.Msg = arg4;
        return result;
    }

    public TestSerializableClassD TestMixedSerTypeFormalIsBase(boolean arg1, short arg2, int arg3, String arg4) throws RemoteException {
        return TestMixedSerType(arg1, arg2, arg3, arg4);
    }

    public Object EchoAnything(Object arg) throws RemoteException {
        return arg;
    }

    public Object GetDoubleAsAny(double val) throws RemoteException {
        return new Double(val);
    }

    public int getTestProp() throws RemoteException {
        return m_val;
    }

    public void setTestProp(int val) throws RemoteException {
        m_val = val;
    }
    
    public TestRecursiveValType TestRecursiveValueType(int nrOfChildren) throws RemoteException {
        TestRecursiveValType result = new TestRecursiveValType(nrOfChildren);
        return result;
    }
    
    public In[] TestArrayWithIdlConflictingElemType(int nrOfElems, int val) throws RemoteException {
        In[] result = new In[nrOfElems];
        for (int i = 0; i < result.length; i++) {
            result[i] = new In(val);
        }
        return result;
    }
    
    public _TestStartByUnderscore[] TestArrayWithElemTypeNameStartByUnderscore(int nrOfElems, int val) throws RemoteException {
        _TestStartByUnderscore[] result = new _TestStartByUnderscore[nrOfElems];
        for (int i = 0; i < result.length; i++) {
            result[i] = new _TestStartByUnderscore(val);
        }
        return result;
    }

    public TestSimpleInterface1 GetSimpleService1() throws RemoteException {
        return new TestSimpleIfImpl();        
    }
    
    public TestSimpleInterface2 GetSimpleService2() throws RemoteException {
        return new TestSimpleIfImpl();        
    }
    
    /** sequence is an idl keyword, check _ removal during transmission */
    public int[] getSequence() throws RemoteException {
        return m_intSequence;
    }
    
    /** sequence is an idl keyword, check _ removal during transmission */
    public void setSequence(int[] arg) throws RemoteException {
        m_intSequence = arg;
    }
    
    /** inout is an idl keyword, check _ removal during transmission */
    public byte octet(byte arg) throws RemoteException {
        return arg;
    }
        
}
