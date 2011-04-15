/* TestClient.cs
 *
 * Project: IIOP.NET
 * Benchmarks
 *
 * WHEN      RESPONSIBLE
 * 20.05.04  Patrik Reali (PRR), patrik.reali -at- elca.ch
 * 20.05.04  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Services;
using Ch.Elca.Iiop.Idl;
using omg.org.CosNaming;
using System.Collections;

namespace Ch.Elca.Iiop.Benchmarks {
    
    
    [SupportedInterface(typeof(RefType))]
    public class RefTypeLocalImpl : MarshalByRefObject, RefType {
    }
    
      
      
    public class TestClient {
        
        #region IFields
    
        private IiopChannel m_channel;
      
        private TestService m_testService;
        private TestService m_testServiceIorUrl;
        private TestService m_testServiceFromNs;
      
        private int  m_count;
        private TimeSpan m_totalTime = new TimeSpan(0);      
        private TimeSpan m_referenceTime = new TimeSpan(0); 
      
        private RefType m_localRT;
        private RefType m_remoteRT;
      
      
        #endregion IFields
      

        public TestClient(int count) {
            m_count = count;
        }

        public void SetupEnvironment(string serviceUrl, string nsUrl, NameComponent[] name,
                                     string endian) {
            // register the channel
            int port = 0;
            IDictionary dict = new Hashtable();
            dict["port"] = port;
            dict["endian"] = endian;
            m_channel = new IiopChannel(dict);
            ChannelServices.RegisterChannel(m_channel, false);

            m_testService = (TestService)RemotingServices.Connect(typeof(TestService), serviceUrl);
            m_testServiceIorUrl = (TestService)
                omg.org.CORBA.OrbServices.GetSingleton().string_to_object(serviceUrl);

            m_testServiceFromNs = TryGetServiceFromNs(nsUrl, name);
        }

        private TestService TryGetServiceFromNs(string nsUrl, NameComponent[] name) {
            try {
                // access COS nameing service
                NamingContext nameService = (NamingContext)
                    RemotingServices.Connect(typeof(NamingContext), nsUrl);
                // get the reference to the test-service
                return (TestService)nameService.resolve(name);
            } catch {
                return null;
            }
        }

        public void TearDownEnvironment() {
            m_testService = null;
            // unregister the channel            
            ChannelServices.UnregisterChannel(m_channel);
        }


        void Dummy() {
            // do nothing
        }

        void CallVoid() {
            m_testService._Void();
        }

        void CallVoidIorUrl() {
            m_testServiceIorUrl._Void();
        }

        void CallVoidIorFromNs() {
            if (m_testServiceFromNs != null) {
                m_testServiceFromNs._Void();
            }
        }

        void CallVI() {
            m_testService.VI(1);
        }

        void CallVII() {
            m_testService.VII(1, 2);
        }

        void CallVIIIII() {
            m_testService.VIIIII(1, 2, 3, 4, 5);
        }

        void CallII() {
            int i = m_testService.II(23);
        }

        void CallIIIIII() {
            int i = m_testService.IIIIII(9,8,7,6,5);
        }

        void CallStSt() {
            string r = m_testService.StSt("abcdefg");
        }

        void CallStStStSt() {
            string r = m_testService.StStStSt("abcdefg", "hijklmnop", "qrstuvw");
        }

        void CallVD() {
            m_testService.VD(1.234567);
        }

        void callDDDDDD() {
            double r = m_testService.DDDDDD(1.0, 2.1, 3.2, 4.3, 5.4);
        }

        void CallVLocalRef() {
            m_testService.VRef(m_localRT);
        }

        void CallVRemoteRef() {
            m_testService.VRef(m_remoteRT);
        }

        void CallRemoteRefRef() {
            RefType rt = m_testService.RefRef(m_remoteRT);
        }

        void CallRemoteLocalRefRef() {
            RefType rt = m_testService.RefRefLocal(m_remoteRT);
        }

        void CallVal1() {
            ValType1 vt = m_testService.Val1();
        }

        void CallVal1Val1() {
            ValType1 vt = new ValType1Impl(23, 29, 31);
            vt = m_testService.Val1Val1(vt);
        }

        void CallVVal1() {
            ValType1 vt = new ValType1Impl(23, 29, 31);
            m_testService.VVal1(vt);
        }

        void CallVal2() {
            ValType2 vt = m_testService.Val2(false);
        }

        void CallVal2Rep() {
            ValType2 vt = m_testService.Val2(true);
        }

        void CallVal2Val2() {
            ValType2 vt = new ValType2Impl(false, 100, 23, 29, 31);
            vt = m_testService.Val2Val2(vt);
        }

        void CallVal2Val2Rep() {
            ValType2 vt = new ValType2Impl(true, 100, 23, 29, 31);
            vt = m_testService.Val2Val2(vt);
        }

        void CallVVal2() {
            ValType2 vt = new ValType2Impl(false, 100, 23, 29, 31);
            m_testService.VVal2(vt);
        }

        void CallVVal2Rep() {
            ValType2 vt = new ValType2Impl(true, 100, 23, 29, 31);
            m_testService.VVal2(vt);
        }

        void CallDoulbeArrCreate() {
            double[] result = m_testService.DoulbeArrCreate(5000);
        }

        void CallDoubleArrEcho() {
            double[] arg = new double[5000];
            m_testService.DoubleArrEcho(arg);
        }

        void CallDoubleSeqEcho() {
            double[] arg = new double[5000];
            m_testService.DoubleIdlSeqEcho(arg);
        }

        void CallIntSeqEcho() {
            int[] arg = new int[5000];
            m_testService.IntIdlSeqEcho(arg);
        }

        void CallBigIntSeqEcho() {
            int[] arg = new int[40*400000];
            m_testService.IntIdlSeqEcho(arg);
        }

        void CallDoubleArrCountElems() {
            double[] arg = new double[5000];
            m_testService.DoubleArrCountElems(arg);
        }


        void CallEnumEcho() {
            EnumA arg = EnumA.EnumA_C;
            m_testService.EchoEnum(arg);
        }

        void CallIdlStructEcho() {
            IdlStructA arg = new IdlStructA(1,2,3,10,11,12);
            m_testService.EchoStruct(arg);
        }

        void CallIdlStructSeqEcho() {
            IdlStructA[] arg = new IdlStructA[10000];
            m_testService.EchoStructSeq(arg);
        }

        void CallIdlAnySeqEcho() {
            object[] arg = new object[10000];
            for(int i = 0; i < arg.Length; ++i)
            	arg[i] = new IdlStructA(9, 8, 7, 6, 5, 4);
            m_testService.EchoAnySeq(arg);
        }

        void CallEnumSeqEcho() {
            EnumA[] arg = new EnumA[1000];
            m_testService.EnumIdlSeqEcho(arg);
        }

        void CallIdlArrayEcho() {
            int[,] arg = new int[500,3];
            m_testService.IdlLongArray5times3Echo(arg);
        }

        void CallBigSingleSeqEcho() {
            float[] arg = new float[40*400000];
            m_testService.SingleIdlSeqEcho(arg);
        }


        void CallIdlArrayBigSingleEcho() {
            float[,] arg = new float[40,400000];
            m_testService.IdlFloatArray40times400000Echo(arg);
        }

        void CallIdlArrayBigByteEcho() {
            byte[] arg = new byte[4*40*400000];
            m_testService.ByteIdlSeqEcho(arg);
        }

        delegate void TestProcedure();

        private void ExecuteTest(bool addtoref, String msg, TestProcedure t, int countreductionDivisor) {
            try {
                Console.Write("{0,-25}", msg);
                int nrOfRuns = m_count / countreductionDivisor;
                nrOfRuns = (nrOfRuns > 0 ? nrOfRuns : 1);
                Stopwatch stopWatch = Stopwatch.StartNew();
                for (int i=0; i<nrOfRuns; i++) {
                    t();
                }
                stopWatch.Stop();
                if (addtoref) {
                    m_referenceTime += stopWatch.Elapsed;
                }
                Console.WriteLine("{0,-10} ms for {1} calls, per call : {2,-10} ms", 
                                  stopWatch.Elapsed.TotalMilliseconds, nrOfRuns, 
                                  stopWatch.Elapsed.TotalMilliseconds / nrOfRuns);
                m_totalTime += stopWatch.Elapsed;
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        private void ExecuteTest(bool addtoref, String msg, TestProcedure t) {
            ExecuteTest(addtoref, msg, t, 1);
        }

        static public void Main(String[] args) {
            int count = 100;

            if (args.Length > 0) {
                count = Int32.Parse(args[0]);
                Console.WriteLine("Count overridden, set to {0}", count);
            }
            TestClient tc = new TestClient(count);
    
	    string serviceUrl = "corbaloc:iiop:1.2@localhost:8087/test";
            string nsUrl = "corbaloc:iiop:1.0@localhost:8087/NameService";
            string endian = "BigEndian";
            if (args.Length > 1) {
                endian = args[1];
            }
            tc.SetupEnvironment(serviceUrl, nsUrl, 
                                new NameComponent[] { new NameComponent("test", "") },
                                endian);
            tc.m_localRT = new RefTypeLocalImpl();
            tc.m_remoteRT = tc.m_testService.RefLocal();
    
            tc.ExecuteTest(false, "Reference", new TestProcedure(tc.Dummy));
            tc.ExecuteTest(true, "Void", new TestProcedure(tc.CallVoid));
            tc.ExecuteTest(false, "VoidUrlIor", new TestProcedure(tc.CallVoidIorUrl));
            tc.ExecuteTest(false, "VoidUrlFNs", new TestProcedure(tc.CallVoidIorFromNs));
            tc.ExecuteTest(false, "(I)V", new TestProcedure(tc.CallVI));
            tc.ExecuteTest(false, "(II)V", new TestProcedure(tc.CallVII));
            tc.ExecuteTest(false, "(IIIII)V", new TestProcedure(tc.CallVIIIII));
            tc.ExecuteTest(false, "(I)I", new TestProcedure(tc.CallII));
            tc.ExecuteTest(false, "(IIIII)I", new TestProcedure(tc.CallIIIIII));
            tc.ExecuteTest(false, "(St)St", new TestProcedure(tc.CallStSt));
            tc.ExecuteTest(false, "(St)StStSt", new TestProcedure(tc.CallStStStSt));
            tc.ExecuteTest(false, "()D", new TestProcedure(tc.CallVD));
            tc.ExecuteTest(false, "(D)DDD", new TestProcedure(tc.callDDDDDD));
            tc.ExecuteTest(false, "(RT) Local", new TestProcedure(tc.CallVLocalRef));
            tc.ExecuteTest(false, "(RT) Remote", new TestProcedure(tc.CallVRemoteRef));
            tc.ExecuteTest(false, "(RT)RT Remote", new TestProcedure(tc.CallRemoteRefRef));
            tc.ExecuteTest(false, "(RT)RT(loc) Remote", new TestProcedure(tc.CallRemoteLocalRefRef));
            tc.ExecuteTest(false, "(Val1)V", new TestProcedure(tc.CallVVal1));
            tc.ExecuteTest(false, "(Val1)Val1", new TestProcedure(tc.CallVal1Val1));
            tc.ExecuteTest(false, "()Val1", new TestProcedure(tc.CallVal1));
            tc.ExecuteTest(false, "(Val2)V", new TestProcedure(tc.CallVVal2));
            tc.ExecuteTest(false, "(Val2Rep)V", new TestProcedure(tc.CallVVal2Rep));
            tc.ExecuteTest(false, "(Val2)Val2", new TestProcedure(tc.CallVal2Val2));
            tc.ExecuteTest(false, "(Val2Rep)Val2", new TestProcedure(tc.CallVal2Val2Rep));
            tc.ExecuteTest(false, "()Val2", new TestProcedure(tc.CallVal2));
            tc.ExecuteTest(false, "()Val2Rep", new TestProcedure(tc.CallVal2Rep));
            tc.ExecuteTest(false, "()double[]", new TestProcedure(tc.CallDoulbeArrCreate));
            tc.ExecuteTest(false, "(double[])double[]", new TestProcedure(tc.CallDoubleArrEcho));
            tc.ExecuteTest(false, "(double[])V", new TestProcedure(tc.CallDoubleArrCountElems));
            tc.ExecuteTest(false, "(double_sq)double_sq", new TestProcedure(tc.CallDoubleSeqEcho));
            tc.ExecuteTest(false, "(int_sq)int_sq", new TestProcedure(tc.CallIntSeqEcho));
            tc.ExecuteTest(false, "(EnumA)EnumA", new TestProcedure(tc.CallEnumEcho));
            tc.ExecuteTest(false, "(IdlStructA)IdlStructA", new TestProcedure(tc.CallIdlStructEcho));
            tc.ExecuteTest(false, "(IdlStruct[])IdlStruct[]", new TestProcedure(tc.CallIdlStructSeqEcho), 100);
            tc.ExecuteTest(false, "(any[])any[]", new TestProcedure(tc.CallIdlAnySeqEcho), 100);
            tc.ExecuteTest(false, "(enum_sq)enum_sq", new TestProcedure(tc.CallEnumSeqEcho));
            tc.ExecuteTest(false, "(int_ar2d)int_ar2d", new TestProcedure(tc.CallIdlArrayEcho));
            
            tc.ExecuteTest(false, "(sng_seq)sng_seq", new TestProcedure(tc.CallBigSingleSeqEcho), 1000);
            tc.ExecuteTest(false, "(sng_ar2d)sng_ar2d", new TestProcedure(tc.CallIdlArrayBigSingleEcho), 1000);
            tc.ExecuteTest(false, "(int_sq)int_sq", new TestProcedure(tc.CallBigIntSeqEcho), 1000);
            tc.ExecuteTest(false, "(byte_sq)byte_sq", new TestProcedure(tc.CallIdlArrayBigByteEcho), 1000);

            tc.TearDownEnvironment();

            Console.WriteLine(String.Format("Total time = {0} s", tc.m_totalTime.TotalSeconds));
        
        }

    }

}
