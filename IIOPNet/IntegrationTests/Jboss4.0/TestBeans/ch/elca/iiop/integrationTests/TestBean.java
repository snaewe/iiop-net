/* TestBean.java
 *
 * Project: IIOP.NET
 * Examples
 *
 * WHEN      RESPONSIBLE
 * 17.07.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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

import javax.ejb.CreateException;
import javax.ejb.SessionBean;
import javax.ejb.SessionContext;

import javax.naming.InitialContext;
import javax.rmi.PortableRemoteObject;



/**
 * TestBean is a stateless Session Bean. It implements the Test functionality.
 */
public class TestBean implements SessionBean {

    private SessionContext m_ctx;

 
    /**
     * This method is required by the EJB Specification,
     * but is not used here.
     *
     */
    public void ejbActivate() {
    }

    /**
     * This method is required by the EJB Specification,
     * but is not used here.
     *
     */
    public void ejbRemove() {
    }

    /**
     * This method is required by the EJB Specification,
     * but is not used by this example.
     *
     */
    public void ejbPassivate() {
    }

    /**
     * Sets the session context.
     *
     * @param ctx               SessionContext Context for session
     */
    public void setSessionContext(SessionContext ctx) {
        m_ctx = ctx;
    }

    /**
     * This method corresponds to the create method in the home interface
     * "TestHome.java".
     *
     */
    public void ejbCreate () throws CreateException {
        // nothing special to do here
    }


    public double TestIncDouble(double arg) {
        return arg + 1;
    }

    public float TestIncFloat(float arg) {
        return arg + 1;
    }

    public byte TestIncByte(byte arg) {
        return (byte)(arg + 1);
    }

    public short TestIncInt16(short arg) {
        return (short)(arg + 1);
    }

    public int TestIncInt32(int arg) {
        return arg + 1;
    }

    public long TestIncInt64(long arg) {
        return arg + 1;
    }

    public boolean TestNegateBoolean(boolean arg) {
        return ! arg;
    }

    public void TestVoid() {
    }
        
    public char TestEchoChar(char arg) {
        return arg;
    }

    public String TestAppendString(String basic, String toAppend) {
        String result = "";
        if (basic != null) {
            result += basic;
        }
        if (toAppend != null) {
            result += toAppend;
        }
        return result;
    }

    public byte[] TestAppendElementToByteArray(byte[] arg, byte toAppend) {
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

    public String[] TestAppendElementToStringArray(String[] arg, String toAppend) {
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

    public String[] CreateTwoElemStringArray(String arg1, String arg2) {
        String[] result = new String[2];
        result[0] = arg1;
        result[1] = arg2;
        return result;
    }
        
    public int[][] EchoJaggedIntArray(int[][] arg) {
        return arg;
    }
        
    public String[][] EchoJaggedStringArray(String[][] arg) {
        return arg;
    }
        
    public byte[][][] EchoJaggedByteArray(byte[][][] arg) {
        return arg;
    }
        
    public IntAdder RetrieveAdder() {
        IntAdder result = null;
        try {
            InitialContext ic = new InitialContext();
            Object obj = ic.lookup("iiop/IntegrationTest/intadder");
            IntAdderHome home = (IntAdderHome) PortableRemoteObject.narrow(obj, IntAdderHome.class);
            result = home.create();
            
        } catch (Exception e) {
            System.err.println("error: " + e);
        }
        return result;
            
    }

    public int AddWithAdder(IntAdder adder, int sum1, int sum2) {       
        try {
            int result = adder.add(sum1, sum2);
            return result;
        } catch (java.rmi.RemoteException e) {
            throw new javax.ejb.EJBException("exception encountered, while trying to add with adder");
        }
    }

    public TestSerializableClassB2 TestChangeSerializableB2(TestSerializableClassB2 arg, String detail) {
        arg.DetailedMsg = detail;
        return arg;
    }

    public TestSerializableClassC TestEchoSerializableC(TestSerializableClassC arg) {
        return arg;
    }
        
    public TestSerializableClassD TestChangeSerilizableD(TestSerializableClassD arg, String newMessage) {
        arg.val1.Msg = newMessage;
        return arg;
    }

    public Object EchoAnything(Object arg) {
        return arg;
    }
    
    public int getTestProp() {
        return Test.TEST_PROP_INIT_VAL;
    }
       
    /// <summary>
    /// used to check, if a reference passed is equal to this object itself.
    /// </summary>
    public boolean CheckEqualityWith(Test toCheck) {
        return toCheck.equals(this);
    }
    
}

