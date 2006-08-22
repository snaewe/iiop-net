/* TestServiceImpl.java
 *
 * Project: IIOP.NET
 * IntegrationTests
 *
 * WHEN      RESPONSIBLE
 * 17.08.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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

package Ch.Elca.Iiop.IntegrationTests.MappingPlugin;

import java.rmi.RemoteException;
import javax.rmi.PortableRemoteObject;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Date;
    

public class TestServiceImpl extends PortableRemoteObject implements TestService {

    public TestServiceImpl() throws java.rmi.RemoteException {
        super(); // invoke rmi linking and remote object initialization
    }

    public ArrayList createShortList(short val, int nrOfElems) throws RemoteException {
        ArrayList result = new ArrayList();
        for (int i = 0; i < nrOfElems; i++) {
            result.add(new Short(val));
        }
        return result;
    }

    public ArrayList createIntList(int val, int nrOfElems) throws RemoteException {
        ArrayList result = new ArrayList();
        for (int i = 0; i < nrOfElems; i++) {
            result.add(new Integer(val));
        }
        return result;
    }
    
    public ArrayList createLongList(long val, int nrOfElems) throws RemoteException {
        ArrayList result = new ArrayList();
        for (int i = 0; i < nrOfElems; i++) {
            result.add(new Long(val));
        }
        return result;
    }

    public ArrayList createByteList(byte val, int nrOfElems) throws RemoteException {
        ArrayList result = new ArrayList();
        for (int i = 0; i < nrOfElems; i++) {
            result.add(new Byte(val));
        }
        return result;
    }

    public ArrayList createCharList(char val, int nrOfElems) throws RemoteException {
        ArrayList result = new ArrayList();
        for (int i = 0; i < nrOfElems; i++) {
            result.add(new Character(val));
        }
        return result;
    }

    public ArrayList createBooleanList(boolean val, int nrOfElems) throws RemoteException {
        ArrayList result = new ArrayList();
        for (int i = 0; i < nrOfElems; i++) {
            result.add(new Boolean(val));
        }
        return result;
    }

    public ArrayList createFloatList(float val, int nrOfElems) throws RemoteException {
        ArrayList result = new ArrayList();
        for (int i = 0; i < nrOfElems; i++) {
            result.add(new Float(val));
        }
        return result;
    }

    public ArrayList createDoubleList(double val, int nrOfElems) throws RemoteException {
        ArrayList result = new ArrayList();
        for (int i = 0; i < nrOfElems; i++) {
            result.add(new Double(val));
        }
        return result;
    }
    
    public ArrayList createValTypeList(String msg, int nrOfElems) throws RemoteException {
        ArrayList result = new ArrayList();
        for (int i = 0; i < nrOfElems; i++) {
            TestSerializableClassB1 entry = new TestSerializableClassB1();
            entry.Msg = msg;
            result.add(entry);
        }
        return result;
    }
    
    public ArrayList createByRefTypeList(int nrOfElems) throws RemoteException {
        ArrayList result = new ArrayList();
        for (int i = 0; i < nrOfElems; i++) {
            result.add(new TestServiceImpl());
        }
        return result;
    }

    public ArrayList echoList(ArrayList arg) throws RemoteException {
        return arg;
    }

    public HashMap createHashMapWithShortVals(short val, int nrOfElems) throws RemoteException {
        HashMap result = new HashMap();
        for (int i = 0; i < nrOfElems; i++) {
            result.put(new Integer(i), new Short(val));
        }
        return result;
    }

    public HashMap createHashMapWithIntVals(int val, int nrOfElems) throws RemoteException {
        HashMap result = new HashMap();
        for (int i = 0; i < nrOfElems; i++) {
            result.put(new Integer(i), new Integer(val));
        }
        return result;
    }
    
    public HashMap createHashMapWithLongVals(long val, int nrOfElems) throws RemoteException {
        HashMap result = new HashMap();
        for (int i = 0; i < nrOfElems; i++) {
            result.put(new Integer(i), new Long(val));
        }
        return result;
    }

    public HashMap createHashMapWithByteVals(byte val, int nrOfElems) throws RemoteException {
        HashMap result = new HashMap();
        for (int i = 0; i < nrOfElems; i++) {
            result.put(new Integer(i), new Byte(val));
        }
        return result;
    }

    public HashMap createHashMapWithCharVals(char val, int nrOfElems) throws RemoteException {
        HashMap result = new HashMap();
        for (int i = 0; i < nrOfElems; i++) {
            result.put(new Integer(i), new Character(val));
        }
        return result;
    }

    public HashMap createHashMapWithBooleanVals(boolean val, int nrOfElems) throws RemoteException {
        HashMap result = new HashMap();
        for (int i = 0; i < nrOfElems; i++) {
            result.put(new Integer(i), new Boolean(val));
        }
        return result;
    }

    public HashMap createHashMapWithFloatVals(float val, int nrOfElems) throws RemoteException {
        HashMap result = new HashMap();
        for (int i = 0; i < nrOfElems; i++) {
            result.put(new Integer(i), new Float(val));
        }
        return result;
    }

    public HashMap createHashMapWithDoubleVals(double val, int nrOfElems) throws RemoteException {
        HashMap result = new HashMap();
        for (int i = 0; i < nrOfElems; i++) {
            result.put(new Integer(i), new Double(val));
        }
        return result;
    }

    public HashMap createHashMapWithValTypeVals(String msg, int nrOfElems) throws RemoteException {
        HashMap result = new HashMap();
        for (int i = 0; i < nrOfElems; i++) {
            TestSerializableClassB1 entry = new TestSerializableClassB1();
            entry.Msg = msg;
            result.put(new Integer(i), entry);
        }
        return result;
    }

    public HashMap createHashMapWithByRefVals(int nrOfElems) throws RemoteException {
        HashMap result = new HashMap();
        for (int i = 0; i < nrOfElems; i++) {
            result.put(new Integer(i), new TestServiceImpl());
        }
        return result;
    }

    public HashMap echoHashMap(HashMap arg) throws RemoteException {
        return arg;
    }
    
        
    public Date echoDate(Date arg) throws RemoteException {
        return arg;
    }

    public Date receiveCurrentDate() throws RemoteException {
        return new Date();
    }

    public CustomMappedSerializable echoCustomMappedSer(CustomMappedSerializable arg) throws RemoteException {
        return arg;
    }
        
}
