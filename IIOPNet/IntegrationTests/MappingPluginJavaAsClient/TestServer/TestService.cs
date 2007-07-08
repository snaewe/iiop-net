/* TestService.cs
 *
 * Project: IIOP.NET
 * IntegrationTests
 *
 * WHEN      RESPONSIBLE
 * 21.08.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Collections;
using System.Runtime.Remoting.Messaging;
using Ch.Elca.Iiop.Idl;

namespace Ch.Elca.Iiop.IntegrationTests.MappingPlugin {

    [Serializable]
    public class TestSerializableClassB1 {
        public System.String Msg;
    }

    [Serializable]
    public class ByteArrayContainer {

        public ByteArrayContainer() {
        }

        public ByteArrayContainer(byte[] content) {
            Content = content;
        }

        [IdlSequence(0L)]
        public byte[] Content;
    }
    

    [SupportedInterface(typeof(TestService))]
    public class TestServiceImpl : MarshalByRefObject, TestService {


        public ArrayList CreateShortList(short val, int nrOfElems) {
            ArrayList result = new ArrayList();
            for (int i = 0; i < nrOfElems; i++) {
                result.Add(val);
            }
            return result;
        }

        public ArrayList CreateIntList(int val, int nrOfElems) {
            ArrayList result = new ArrayList();
            for (int i = 0; i < nrOfElems; i++) {
                result.Add(val);
            }
            return result;
        }
        
        public ArrayList CreateLongList(long val, int nrOfElems) {
            ArrayList result = new ArrayList();
            for (int i = 0; i < nrOfElems; i++) {
                result.Add(val);
            }
            return result;
        }

        public ArrayList CreateByteList(byte val, int nrOfElems) {
            ArrayList result = new ArrayList();
            for (int i = 0; i < nrOfElems; i++) {
                result.Add(val);
            }
            return result;
        }

        public ArrayList CreateCharList(char val, int nrOfElems) {
            ArrayList result = new ArrayList();
            for (int i = 0; i < nrOfElems; i++) {
                result.Add(val);
            }
            return result;
        }

        public ArrayList CreateBooleanList(bool val, int nrOfElems) {
            ArrayList result = new ArrayList();
            for (int i = 0; i < nrOfElems; i++) {
                result.Add(val);
            }
            return result;
        }

        public ArrayList CreateFloatList(float val, int nrOfElems) {
            ArrayList result = new ArrayList();
            for (int i = 0; i < nrOfElems; i++) {
                result.Add(val);
            }
            return result;
        }

        public ArrayList CreateDoubleList(double val, int nrOfElems) {
            ArrayList result = new ArrayList();
            for (int i = 0; i < nrOfElems; i++) {
                result.Add(val);
            }
            return result;
        }
    
        public ArrayList CreateValTypeList(String msg, int nrOfElems) {
            ArrayList result = new ArrayList();
            for (int i = 0; i < nrOfElems; i++) {
                TestSerializableClassB1 entry = new TestSerializableClassB1();
                entry.Msg = msg;
                result.Add(entry);
            }
            return result;
        }
    
        public ArrayList CreateByRefTypeList(int nrOfElems) {
            ArrayList result = new ArrayList();
            for (int i = 0; i < nrOfElems; i++) {
                result.Add(new TestServiceImpl());
            }
            return result;
        }

        public ArrayList EchoList(ArrayList arg) {
            return arg;
        }


        public Hashtable EchoHashtable(Hashtable arg) {
            return arg;
        }

        public Hashtable CreateHashtableWithIntElems(int val, int nrOfElems) {
            Hashtable result = new Hashtable();
            for (int i = 0; i <nrOfElems; i++) {
                result[i] = val;
            }
            return result;
        }

        public Hashtable CreateHashtableWithValTypeElems(String msg, int nrOfElems) {
            Hashtable result = new Hashtable();
            for (int i = 0; i <nrOfElems; i++) {
                TestSerializableClassB1 entry = new TestSerializableClassB1();
                entry.Msg = msg;
                result[i] = entry;
            }
            return result;
        }

        public Hashtable CreateHashtableWithByRefElems(int nrOfElems) {
            Hashtable result = new Hashtable();
            for (int i = 0; i <nrOfElems; i++) {
                result[i] = new TestServiceImpl();
            }
            return result;
        }

        public Hashtable CreateHashtableWithByteArrayAndStringElement(byte[] content1, string content2) {
            Hashtable result = new Hashtable();
            //result["content1"] = content1; // doesn't work, because of java behaviour
            result["content1"] = new ByteArrayContainer(content1);
            result["content2"] = content2;
            return result;
        }
        
        public DateTime EchoDateTime(DateTime arg) {
            return arg;
        }        
        
        public override object InitializeLifetimeService() {
            // live forever
            return null;
        }

    }

    public interface TestService {
 

        ArrayList CreateShortList(short val, int nrOfElems);

        ArrayList CreateIntList(int val, int nrOfElems);
        
        ArrayList CreateLongList(long val, int nrOfElems);

        ArrayList CreateByteList(byte val, int nrOfElems);

        ArrayList CreateCharList(char val, int nrOfElems);

        ArrayList CreateBooleanList(bool val, int nrOfElems);

        ArrayList CreateFloatList(float val, int nrOfElems);

        ArrayList CreateDoubleList(double val, int nrOfElems);

        ArrayList CreateValTypeList(String msg, int nrOfElems);

        ArrayList CreateByRefTypeList(int nrOfElems);

        ArrayList EchoList(ArrayList arg);

        Hashtable EchoHashtable(Hashtable arg);

        Hashtable CreateHashtableWithIntElems(int val, int nrOfElems);

        Hashtable CreateHashtableWithValTypeElems(String msg, int nrOfElems);

        Hashtable CreateHashtableWithByRefElems(int nrOfElems);

        Hashtable CreateHashtableWithByteArrayAndStringElement(byte[] content1, string content2);

        DateTime  EchoDateTime(DateTime arg);
                        
    }


}
