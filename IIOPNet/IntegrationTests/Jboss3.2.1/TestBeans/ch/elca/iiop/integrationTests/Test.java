/* Test.java
 *
 * Project: IIOP.NET
 * Examples
 *
 * WHEN      RESPONSIBLE
 * 16.07.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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

package ch.elca.iiop.integrationTests;

import javax.ejb.*;
import java.rmi.RemoteException;

/**
 * The methods in this interface are the public face of the Test-Bean.
 */
public interface Test extends EJBObject {

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

    public String[] TestAppendElementToStringArray(String[] arg, String toAppend) throws RemoteException;

    public String[] CreateTwoElemStringArray(String arg1, String arg2) throws RemoteException;
        
    public int[][] EchoJaggedIntArray(int[][] arg) throws RemoteException;
        
    public String[][] EchoJaggedStringArray(String[][] arg) throws RemoteException;
        
    public byte[][][] EchoJaggedByteArray(byte[][][] arg) throws RemoteException;      
        
    public IntAdder RetrieveAdder() throws RemoteException; 

    public int AddWithAdder(IntAdder adder, int sum1, int sum2) throws RemoteException;

    public TestSerializableClassB2 TestChangeSerializableB2(TestSerializableClassB2 arg, String detail) throws RemoteException;

    public TestSerializableClassC TestEchoSerializableC(TestSerializableClassC arg) throws RemoteException;        

    public TestSerializableClassD TestChangeSerilizableD(TestSerializableClassD arg, String newMessage) throws RemoteException;
    
    public int getTestProp() throws RemoteException;

    public static final int TEST_PROP_INIT_VAL = 11;

    public Object EchoAnything(Object arg) throws RemoteException;

}
