/* TestService.java
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
    

public interface TestService extends java.rmi.Remote {

    public double TestIncDouble(double arg) throws RemoteException;

    public float TestIncFloat(float arg) throws RemoteException;

    public byte TestIncByte(byte arg) throws RemoteException; 

    public short TestIncInt16(short arg) throws RemoteException;

    public int TestIncInt32(int arg) throws RemoteException;

    public long TestIncInt64(long arg) throws RemoteException;

    public boolean TestNegateBoolean(boolean arg) throws RemoteException;

    public void TestVoid() throws RemoteException;
        
    public char TestEchoChar(char arg) throws RemoteException;

    public String TestAppendString(String basic, String toAppend) throws RemoteException; 

    public byte[] TestAppendElementToByteArray(byte[] arg, byte toAppend) throws RemoteException;

    public long[] TestAppendElementToLongArray(long[] arg, long toAppend) throws RemoteException;

    public String[] TestAppendElementToStringArray(String[] arg, String toAppend) throws RemoteException;

    public String[] CreateTwoElemStringArray(String arg1, String arg2) throws RemoteException;
        
    public int[][] EchoJaggedIntArray(int[][] arg) throws RemoteException;
        
    public String[][] EchoJaggedStringArray(String[][] arg) throws RemoteException;
        
    public byte[][][] EchoJaggedByteArray(byte[][][] arg) throws RemoteException;   

    public NamedValue[] TestAppendElementToNamedValueArray(NamedValue[] arg, NamedValue toAppend) throws RemoteException;
        
    public Adder RetrieveAdder() throws RemoteException; 

    public int AddWithAdder(Adder adder, int sum1, int sum2) throws RemoteException;

    public TestSerializableClassB2 TestChangeSerializableB2(TestSerializableClassB2 arg, String detail) throws RemoteException;

    public TestSerializableClassC TestEchoSerializableC(TestSerializableClassC arg) throws RemoteException;
        
    public TestNonSerializableBaseClass TestAbstractValueTypeEcho(TestNonSerializableBaseClass arg) throws RemoteException;

    public TestSerializableClassD TestChangeSerilizableD(TestSerializableClassD arg, String newMessage) throws RemoteException;
    
    public TestSerializableClassE TestEchoSerializableE(TestSerializableClassE arg) throws RemoteException;

    public TestSerWithInner TestEchoWithInner(TestSerWithInner arg) throws RemoteException;

    public TestSerializableMixedValAndBase TestMixedSerType(boolean arg1, short arg2, int arg3, String arg4) throws RemoteException;

    public TestSerializableClassD TestMixedSerTypeFormalIsBase(boolean arg1, short arg2, int arg3, String arg4) throws RemoteException;

    public Object GetDoubleAsAny(double val) throws RemoteException;

    public Object EchoAnything(Object arg) throws RemoteException;

    public int getTestProp() throws RemoteException;

    public void setTestProp(int val) throws RemoteException;
    
    public TestRecursiveValType TestRecursiveValueType(int nrOfChildren) throws RemoteException;
    
    public In[] TestArrayWithIdlConflictingElemType(int nrOfElems, int val) throws RemoteException;
    
    public _TestStartByUnderscore[] TestArrayWithElemTypeNameStartByUnderscore(int nrOfElems, int val) throws RemoteException;

    public TestSimpleInterface1 GetSimpleService1() throws RemoteException;
    public TestSimpleInterface2 GetSimpleService2() throws RemoteException;

    /** sequence is an idl keyword, check _ removal during transmission */
    public int[] getSequence() throws RemoteException;
    
    /** sequence is an idl keyword, check _ removal during transmission */
    public void setSequence(int[] arg) throws RemoteException;
    
    /** octet is an idl keyword, check _ removal during transmission */
    public byte octet(byte arg) throws RemoteException;    

}
