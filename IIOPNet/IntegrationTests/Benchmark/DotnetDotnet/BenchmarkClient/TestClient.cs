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
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Services;
using Ch.Elca.Iiop.Idl;
using omg.org.CosNaming;

namespace Ch.Elca.Iiop.Benchmarks {
    
    
    [SupportedInterface(typeof(RefType))]
    public class RefTypeLocalImpl : MarshalByRefObject, RefType {
    }
    


    public class TestClient {
        
        #region IFields
    
        private IiopChannel m_channel;

        private TestService m_testService;

        private int  m_count;
        private TimeSpan m_totalTime = new TimeSpan(0);      
        private TimeSpan m_referenceTime = new TimeSpan(0); 

        private RefType m_localRT;
        private RefType m_remoteRT;


        #endregion IFields


        public TestClient(int count) {
            m_count = count;
        }

        public void SetupEnvironment() {
            // register the channel
            m_channel = new IiopChannel(0);
            ChannelServices.RegisterChannel(m_channel);

            // access COS nameing service
            CorbaInit init = CorbaInit.GetInit();
            NamingContext nameService = init.GetNameService("localhost", 8087);
            NameComponent[] name = new NameComponent[] { new NameComponent("test", "") };
            // get the reference to the test-service
            m_testService = (TestService)nameService.resolve(name);
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


        delegate void TestProcedure();

        private void ExecuteTest(bool addtoref, String msg, TestProcedure t) {
            try {
                Console.Write("{0,-20}", msg);
                PerformanceCounter counter = new PerformanceCounter();
                for (int i=0; i<m_count; i++) {
                    t();
                }
                counter.Stop();                
                if (addtoref) {
                    m_referenceTime += counter.Difference;
                }
                Console.WriteLine("{0,-10} ms for {1} calls, per call : {2,-10} ms", 
                                  counter.Difference.TotalMilliseconds, m_count, 
                                  counter.Difference.TotalMilliseconds / m_count);
                m_totalTime += counter.Difference;
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

        }

        static public void Main(String[] args) {
            int count = 100;

            if (args.Length > 0) {
                count = Int32.Parse(args[0]);
                Console.WriteLine("Count overridden, set to {0}", count);
            }
            TestClient tc = new TestClient(count);
    
            tc.SetupEnvironment();
            tc.m_localRT = new RefTypeLocalImpl();
            tc.m_remoteRT = tc.m_testService.RefLocal();
    
            tc.ExecuteTest(false, "Reference", new TestProcedure(tc.Dummy));
            tc.ExecuteTest(true, "Void", new TestProcedure(tc.CallVoid));
            tc.ExecuteTest(false, "(I)V", new TestProcedure(tc.CallVI));
            tc.ExecuteTest(false, "(II)V", new TestProcedure(tc.CallVII));
            tc.ExecuteTest(false, "(IIIII)V", new TestProcedure(tc.CallVIIIII));
            tc.ExecuteTest(false, "(I)I", new TestProcedure(tc.CallII));
            tc.ExecuteTest(false, "(IIIII)I", new TestProcedure(tc.CallIIIIII));
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
            tc.TearDownEnvironment();

            Console.WriteLine(String.Format("Total time = {0} s", tc.m_totalTime.TotalSeconds));
        
        }

    }

}
