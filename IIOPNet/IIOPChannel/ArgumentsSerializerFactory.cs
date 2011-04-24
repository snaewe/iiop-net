/* ArgumentsSerilizerFactory.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 30.12.05  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
 * 
 * Copyright 2005 Dominic Ullmann
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

namespace Ch.Elca.Iiop.Marshalling {
 
    /// <summary>
    /// Creates and caches ArgumentsSerializer for remote interfaces.
    /// </summary>
    internal class ArgumentsSerializerFactory {
 
        #region IFields
 
        private IDictionary /* <Type, ArgumentsSerializer> */ m_serializers = new Hashtable();
        private SerializerFactory m_serFactory;
 
        #endregion IFields
        #region IProperties
 
        /// <summary>
        /// the SerializerFactory used by and to use togheter with
        /// ArgumentSerializerFactory.
        /// </summary>
        internal SerializerFactory SerializerFactory {
            get {
                return m_serFactory;
            }
        }
 
        #endregion IProperties
        #region IConstructors
 
        internal ArgumentsSerializerFactory(SerializerFactory serFactory) {
            m_serFactory = serFactory;
        }
 
        #endregion IConstructors
        #region IMethods
 
        /// <summary>
        /// Creates or retrieve cached ArgumentsSerializer for the given (marshal by ref, e.g. interface) Type.
        /// </summary>
        internal ArgumentsSerializer Create(Type forType) {
            lock(this) {
                ArgumentsSerializer result = (ArgumentsSerializer)m_serializers[forType];
                if (result == null) {
                    result = new ArgumentsSerializer(forType, m_serFactory);
                    m_serializers[forType] = result;
                }
                return result;
            }
        }
 
        #endregion IMethods
 
    }


}

#if UnitTest

namespace Ch.Elca.Iiop.Tests {
 
    using System;
    using System.Reflection;
    using System.IO;
    using NUnit.Framework;
    using Ch.Elca.Iiop;
    using Ch.Elca.Iiop.Idl;
    using Ch.Elca.Iiop.Util;
    using Ch.Elca.Iiop.Marshalling;
    using Ch.Elca.Iiop.Cdr;
    using omg.org.CORBA;
 

    public class ParameterMarshallerTestRemote : MarshalByRefObject {
 
        public int TestSomeInts(int a1, int a2, int a3, int a4, int a5) {
            return a1 + a2 + a3 + a4 + a5; // unimportant for test
        }
 
    }

    /// <summary>
    /// Unit-tests for testing request/reply serialisation/deserialisation
    /// </summary>
    [TestFixture]
    public class ArgumentsSerializerFactoryTest {
 
        private ArgumentsSerializerFactory m_argSerFactory;
 
        [SetUp]
        public void SetUp() {
            SerializerFactory serFactory = new SerializerFactory();
            omg.org.IOP.CodecFactory codecFactory =
                new Ch.Elca.Iiop.Interception.CodecFactoryImpl(serFactory);
            omg.org.IOP.Codec codec =
                codecFactory.create_codec(
                    new omg.org.IOP.Encoding(omg.org.IOP.ENCODING_CDR_ENCAPS.ConstVal, 1, 2));
            serFactory.Initalize(new SerializerFactoryConfig(), IiopUrlUtil.Create(codec));
            m_argSerFactory = new ArgumentsSerializerFactory(serFactory);
        }
 
        private MethodInfo GetTestSomeIntMethod() {
            Type parameterMarshallerTestRemoteType = typeof(ParameterMarshallerTestRemote);
            return parameterMarshallerTestRemoteType.GetMethod("TestSomeInts", BindingFlags.Instance | BindingFlags.Public);
        }
 
        private void CheckArrayEqual(object[] a1, object[] a2) {
            Assert.AreEqual(a1.Length, a2.Length);
            for (int i = 0; i < a1.Length; i++) {
                Assert.AreEqual(a1[i], a2[i]);
            }
        }
 
        [Test]
        public void TestRequestArguments() {
            MethodInfo testMethod = GetTestSomeIntMethod();
 
            object[] actual = new object[] { 1 , 2, 3, 4, 5 };
            for (int j = 0; j < 10; j++) { // test more than one call
                object[] deser = MarshalAndUnmarshalRequestArgsOnce(testMethod, actual);
                CheckArrayEqual(actual, deser);
            }
        }
 
        private object[] MarshalAndUnmarshalRequestArgsOnce(MethodInfo testMethod, object[] actual) {
            ArgumentsSerializerFactory serFactory = m_argSerFactory;
            ArgumentsSerializer ser = serFactory.Create(testMethod.DeclaringType);
 
            MemoryStream data = new MemoryStream();
            GiopVersion version = new GiopVersion(1, 2);
            byte endian = 0;
            CdrOutputStream targetStream = new CdrOutputStreamImpl(data, endian, version);
            ser.SerializeRequestArgs(testMethod.Name, actual, targetStream, null);
 
            data.Seek(0, SeekOrigin.Begin);
            CdrInputStreamImpl sourceStream = new CdrInputStreamImpl(data);
            sourceStream.ConfigStream(endian, version);
            IDictionary contextElements;
            object[] deser = ser.DeserializeRequestArgs(testMethod.Name, sourceStream, out contextElements);
            return deser;
        }
 
        [Test]
        public void TestReplyArguments() {
            MethodInfo testMethod = GetTestSomeIntMethod();
 
            object returnValue = 9876;
            object[] outArgs = new object[0];
            for (int j = 0; j < 10; j++) { // check more than one call
                object[] deserOut;
                object deser = MarshalAndUnmarshalResponeArgsOnce(testMethod, returnValue, outArgs,
                                                                  out deserOut);
                Assert.AreEqual(returnValue, deser);
                CheckArrayEqual(outArgs, deserOut);
            }
 
        }
 
        private object MarshalAndUnmarshalResponeArgsOnce(MethodInfo testMethod, object returnValue,
                                                          object[] outArgs, out object[] deserOutArgs) {
            ArgumentsSerializerFactory serFactory =
                m_argSerFactory;
            ArgumentsSerializer ser = serFactory.Create(testMethod.DeclaringType);
 
            MemoryStream data = new MemoryStream();
            GiopVersion version = new GiopVersion(1, 2);
            byte endian = 0;
            CdrOutputStream targetStream = new CdrOutputStreamImpl(data, endian, version);
            ser.SerializeResponseArgs(testMethod.Name, returnValue, outArgs, targetStream);
 
            data.Seek(0, SeekOrigin.Begin);
            CdrInputStreamImpl sourceStream = new CdrInputStreamImpl(data);
            sourceStream.ConfigStream(endian, version);
            object returnValueDeser = ser.DeserializeResponseArgs(testMethod.Name, out deserOutArgs,
                                                                  sourceStream);
            return returnValueDeser;
        }
 
 
    }
 
}

#endif
