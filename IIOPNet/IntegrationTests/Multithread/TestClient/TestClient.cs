/* TestClient.cs
 *
 * Project: IIOP.NET
 * IntegrationTests
 *
 * WHEN      RESPONSIBLE
 * 08.07.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Collections;
using System.Threading;
using System.Text;
using NUnit.Framework;
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Services;
using omg.org.CosNaming;

namespace Ch.Elca.Iiop.IntegrationTests {
    
    
    public class TestFailedException : Exception {
        
        public TestFailedException(string msg) : base(msg) {
        }
        
    }
    

    [TestFixture]
    public class TestClient {

        #region IFields

        private IiopClientChannel m_channel;

        private TestService m_testService1;
        private TestService m_testService2;
        
        private Random m_random = new Random();


        #endregion IFields

        [SetUp]
        public void SetupEnvironment() {
            // register the channel
            m_channel = new IiopClientChannel();
            ChannelServices.RegisterChannel(m_channel, false);

            // get the reference to the test-service
            m_testService1 = (TestService)RemotingServices.Connect(typeof(TestService), "iiop://localhost:8087/test1");
            m_testService2 = (TestService)RemotingServices.Connect(typeof(TestService), "iiop://localhost:8087/test2");
        }

        [TearDown]
        public void TearDownEnvironment() {
            m_testService1 = null;
            m_testService2 = null;
            // unregister the channel            
            ChannelServices.UnregisterChannel(m_channel);
        }
        
        private void RunMultithreaded(PerformCallDelegate remoteMethodCaller, int nrOfCalls, int nrOfThreads) {
            ArrayList methodRunners = new ArrayList();
            ArrayList threads = new ArrayList();
            for (int i = 0; i < nrOfThreads; i++) {
                TimeSpan delay = TimeSpan.FromMilliseconds(m_random.Next(6));
                RepeatedMethodCaller rmc1 = new RepeatedMethodCaller(nrOfCalls,
                                                                     delay, remoteMethodCaller,
                                                                     m_testService1);
                methodRunners.Add(rmc1);
                Thread sv1 = new Thread(new ThreadStart(rmc1.PerformCalls));
                threads.Add(sv1);
                sv1.Start();
                
                delay = TimeSpan.FromMilliseconds(m_random.Next(3));
                RepeatedMethodCaller rmc2 = new RepeatedMethodCaller(nrOfCalls,
                                                                     delay, remoteMethodCaller,
                                                                     m_testService2);
                methodRunners.Add(rmc2);
                Thread sv2 = new Thread(new ThreadStart(rmc2.PerformCalls));
                threads.Add(sv2);
                sv2.Start();
            }
            
            foreach (Thread t in threads) {
                t.Join(); // wait for thread to finish
            }
            
            StringBuilder exceptionMsg = new StringBuilder();
            bool foundEx = false;
            foreach (RepeatedMethodCaller rmc in methodRunners) {
                Exception[] exceptions = rmc.ExceptionsEncountered;
                if (exceptions.Length > 0) {
                    foundEx = true;
                    foreach (Exception ex in exceptions) {
                        exceptionMsg.Append(ex.ToString() + "\n");    
                    }
                }                
            }
            if (foundEx) {
                throw new TestFailedException("the following exceptions were encountered while running the test:\n" + 
                                              exceptionMsg.ToString());
            }

        }

        private void RunMultithreaded(PerformCallDelegate remoteMethodCaller) {
            RunMultithreaded(remoteMethodCaller, 150, 35); // on the server side max 20 threads for multiplexed con.
            RunMultithreaded(remoteMethodCaller, 250, 4); // on the server side max 20 threads for multiplexed con.
        }

        [Test]
        public void TestWithNoParamsSync() {                        
            RunMultithreaded(new PerformCallDelegate(this.CallNoParamsMethodSync));                
        }
        
        [Test]
        public void TestWithNoParamsAsync() {
            RunMultithreaded(new PerformCallDelegate(this.CallNoParamsMethodAsync));
        }
        
        private void CallNoParamsMethodSync(TestService serviceToUse) {
            serviceToUse.TestVoid();
        }
        
        private delegate void NoParamsCallDelegate();
        
        private void CallNoParamsMethodAsync(TestService serviceToUse) {
            NoParamsCallDelegate npcd = new NoParamsCallDelegate(serviceToUse.TestVoid);
            // async call
            IAsyncResult ar = npcd.BeginInvoke(null, null);
            // do not wait for response            
        }
        

        [Test]
        public void TestWithOctetParamsSync() {
            RunMultithreaded(new PerformCallDelegate(this.CallOctetParamsMethod));          
        }
        
        [Test]
        public void TestWithOctetParamsAsync() {
            RunMultithreaded(new PerformCallDelegate(this.CallOctetParamsMethodAsync));         
        }
        
        private void CallOctetParamsMethod(TestService serviceToUse) {
            System.Byte arg = (byte)m_random.Next(100);
            System.Byte result = serviceToUse.TestIncByte(arg);
            Assertion.AssertEquals((System.Byte)(arg + 1), result);
        }
        
        private delegate byte ByteParamsCallDelegate(byte arg);
        
        private void CallOctetParamsMethodAsync(TestService serviceToUse) {
            System.Byte arg = (byte)m_random.Next(100);
            
            ByteParamsCallDelegate bcd = new ByteParamsCallDelegate(serviceToUse.TestIncByte);
            // async call
            IAsyncResult ar = bcd.BeginInvoke(arg, null, null);
            // wait for response
            System.Byte result = bcd.EndInvoke(ar);
            Assertion.AssertEquals((System.Byte)(arg + 1), result);
        }


        [Test]
        public void TestWithRemoteCreatedObjectsSync() {
            RunMultithreaded(new PerformCallDelegate(this.CallWithCreateObject));
        }
        
        [Test]
        public void TestWithRemoteCreatedObjectsAsync() {
            RunMultithreaded(new PerformCallDelegate(this.CallWithCreateObjectAsync));
        }
        
        private void CallWithCreateObject(TestService serviceToUse) {
            Adder adder = serviceToUse.RetrieveAdder();
            System.Int32 arg1 = m_random.Next(100);
            System.Int32 arg2 = m_random.Next(100);
            System.Int32 result = adder.Add(arg1, arg2);
            Assertion.AssertEquals((System.Int32) arg1 + arg2, result);
        }
        
        private delegate Adder RetrieveAdderDelegate();
        private delegate int AddCallDelegate(int arg1, int arg2);
        
        private void CallWithCreateObjectAsync(TestService serviceToUse) {
            RetrieveAdderDelegate rad = new RetrieveAdderDelegate(serviceToUse.RetrieveAdder);
            // async call
            IAsyncResult ar1 = rad.BeginInvoke(null, null);
            
            System.Int32 arg1 = m_random.Next(100);
            System.Int32 arg2 = m_random.Next(100);
            
            // wait for response
            Adder result1 = rad.EndInvoke(ar1);
            Assertion.AssertNotNull(result1);
                        
            AddCallDelegate acd = new AddCallDelegate(result1.Add);
            // async call
            IAsyncResult ar2 = acd.BeginInvoke(arg1, arg2, null, null);
            // wait for response
            System.Int32 result2 = acd.EndInvoke(ar2);                                                         
            Assertion.AssertEquals((System.Int32) arg1 + arg2, result2);
        }

        private void CallLongBlocking(TestService serviceToUse) {
            serviceToUse.BlockForTime(350);
        }       

        [Test]
        public void TestWithStructParamsSync() {
            RunMultithreaded(new PerformCallDelegate(this.CallStructParamsMethod));         
        }
        
        [Test]
        public void TestWithStructParamsAsync() {
            RunMultithreaded(new PerformCallDelegate(this.CallStructParamsMethodAsync));            
        }

        [Test]
        public void TestLongBlockingCalls() {
            // on the server side max 20 threads for multiplexed con.
            RunMultithreaded(new PerformCallDelegate(this.CallLongBlocking), 40, 35); 
        }
        
        private void CallStructParamsMethod(TestService serviceToUse) {
            TestStructA arg = new TestStructAImpl();
            arg.X = m_random.Next(100);
            arg.Y = -1 * m_random.Next(100);
            TestStructA result = m_testService1.TestEchoStruct(arg);
            Assertion.AssertEquals(arg.X, result.X);
            Assertion.AssertEquals(arg.Y, result.Y);
        }
        
        private delegate TestStructA StructParamsCallDelegate(TestStructA arg);

        private void CallStructParamsMethodAsync(TestService serviceToUse) {
            TestStructA arg = new TestStructAImpl();
            arg.X = m_random.Next(100);
            arg.Y = -1 * m_random.Next(100);
            
            StructParamsCallDelegate scd = new StructParamsCallDelegate(serviceToUse.TestEchoStruct);
            // async call
            IAsyncResult ar = scd.BeginInvoke(arg, null, null);
            // wait for response
            TestStructA result = scd.EndInvoke(ar);
            
            Assertion.AssertEquals(arg.X, result.X);
            Assertion.AssertEquals(arg.Y, result.Y);
        }

    }
    
    
    delegate void PerformCallDelegate(TestService serviceToUse);
    
    
    internal class RepeatedMethodCaller {
        
        #region IFields
        
        private TimeSpan m_delay;
        private int m_nrOfCalls;
        private PerformCallDelegate m_callPerformer;
        private TestService m_serviceToUse;
        
        private ArrayList m_exceptionsEncountered = new ArrayList();
        
        #endregion IFields
        #region IConstructors
        
        public RepeatedMethodCaller(int nrOfCalls, TimeSpan delay,
                                    PerformCallDelegate callPerformer,
                                    TestService serviceToUse) {
            m_delay = delay;
            m_nrOfCalls = nrOfCalls;
            m_callPerformer = callPerformer;
            m_serviceToUse = serviceToUse;
        }
        
        #endregion IConstructors
        #region IProperties
        
        public Exception[] ExceptionsEncountered {
            get {
                lock(m_exceptionsEncountered.SyncRoot) {
                    return (Exception[])m_exceptionsEncountered.ToArray(typeof(Exception));
                }                
            }
        }
        
        #endregion IProperties
        #region IMethods
        
        public void PerformCalls() {
            for (int i = 0; i < m_nrOfCalls; i++) {
                try {
                    m_callPerformer(m_serviceToUse);
                } catch (Exception e) {
                    lock (m_exceptionsEncountered.SyncRoot) {
                        m_exceptionsEncountered.Add(e);
                    }
                }
                Thread.Sleep((int)m_delay.TotalMilliseconds);
            }
        }
        
        #endregion IMethods
        
    }

}
