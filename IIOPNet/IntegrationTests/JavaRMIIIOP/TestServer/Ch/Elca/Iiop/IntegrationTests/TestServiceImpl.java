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

    public Object EchoAnything(Object arg) throws RemoteException {
        return arg;
    }
        
    /// <summary>
    /// used to check, if a reference passed is equal to this object itself.
    /// </summary>
    public boolean CheckEqualityWithService(java.rmi.Remote toCheck) throws RemoteException {
        return toCheck.equals(this);
    }
         
    public boolean CheckEqualityWithServiceV2(TestService toCheck) throws RemoteException {
        return toCheck.equals(this);
    }

}
