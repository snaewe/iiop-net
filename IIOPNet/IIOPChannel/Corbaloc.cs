/* Corbaloc.cs
 *
 * Project: IIOP.NET
 * IIOPChannel
 *
 * WHEN      RESPONSIBLE
 * 09.08.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
 *
 * Copyright 2003 Dominic Ullmann
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
using Ch.Elca.Iiop.Security.Ssl;
using Ch.Elca.Iiop.Util;
using omg.org.CORBA;
using omg.org.IOP;

namespace Ch.Elca.Iiop.CorbaObjRef {

    /// <summary>
    /// This class parses a corbaloc url.
    /// </summary>
    internal class Corbaloc {

        #region SFields

        private readonly static object[] s_defaultComponents =
            new object[0];

        #endregion SFields
        #region IFields

        /// <summary>the key string as string</summary>
        private string m_keyString;
        private byte[] m_keyBytes;

        private CorbaLocObjAddr[] m_objAddrs;
        private IorProfile[] m_profiles;

        #endregion IFields
        #region IConstructors

        /// <summary>creates the corbaloc from a corbaloc url string</summary>
        internal Corbaloc(string corbalocUrl, Codec codec) : this(corbalocUrl, codec, s_defaultComponents) {
        }

        /// <summary>creates the corbaloc from a corbaloc url string</summary>
        internal Corbaloc(string corbalocUrl, Codec codec, IList /* TaggedComponent */ additionalComponents) {
            Parse(corbalocUrl, codec, additionalComponents);
        }

        #endregion IConstructors
        #region IProperties

        /// <summary>
        /// contains an ASCII representation of the object key; valid characters in
        /// this string are member of the ASCII charset ->
        ///  can represent octet values 0 - 127
        /// </summary>
        public string KeyString {
            get {
                return m_keyString;
            }
        }

        public string ObjectUri {
            get {
                // TODO: resolve %HexHex%
                return m_keyString;
            }
        }

        #endregion IProperties
        #region IMethods

        /// <summary>parses a corbaloc url according to section 13.6.10 in Corba standard</summary>
        private void Parse(string corbalocUrl, Codec codec,
                           IList /* TaggedComponent */ additionalComponents) {
            const string corbaloc = "corbaloc:";
            if (!corbalocUrl.StartsWith(corbaloc)) {
                throw new BAD_PARAM(7, CompletionStatus.Completed_No);
            }
            string addrPart;
            int slashIndex = corbalocUrl.IndexOf('/', corbaloc.Length);
            if (slashIndex >= 0) {
                m_keyString = corbalocUrl.Substring(slashIndex + 1);
                addrPart = corbalocUrl.Substring(corbaloc.Length, slashIndex - corbaloc.Length);
            }
            else {
                addrPart = corbalocUrl.Substring(corbaloc.Length);
            }
            if ((addrPart.StartsWith("iiop:") || addrPart.StartsWith(":")) &&
                (m_keyString == null)) {
                throw new BAD_PARAM(10, CompletionStatus.Completed_No);
            }
            // create key bytes from keystring
            CalculateKeyBytesFromKeyString();
            ParseAddrList(addrPart, codec, additionalComponents);
        }

        /// <summary>parses the addr list</summary>
        private void ParseAddrList(string addrList, Codec codec,
                                   IList /* TaggedComponent */ additionalComponents) {
            if (addrList == null) {
                throw new BAD_PARAM(8, CompletionStatus.Completed_No);
            }
            string[] parts = addrList.Split(',');
            // at least one!
            m_objAddrs = new CorbaLocObjAddr[parts.Length];
            m_profiles = new IorProfile[parts.Length];

            for (int i = 0; i < parts.Length; i++) {
                if (CorbaLocIiopAddr.IsResponsibleForProtocol(parts[i])) {
                    m_objAddrs[i] = new CorbaLocIiopAddr(parts[i]);
                } else if (CorbaLocIiopSslAddr.IsResponsibleForProtocol(parts[i])) {
                    m_objAddrs[i] = new CorbaLocIiopSslAddr(parts[i]);
                } else {
                    throw new BAD_PARAM(8, CompletionStatus.Completed_No);
                }
                m_profiles[i] = GetProfileFor(m_objAddrs[i], GetKeyAsByteArray(), codec,
                                              additionalComponents);
            }
        }

        private IorProfile GetProfileFor(CorbaLocObjAddr objAddr, byte[] objKey, Codec codec,
                                         IList /* TaggedComponent */ additionalComponents) {
            IorProfile addrProfile =
                objAddr.GetProfileForAddr(objKey, codec);
            for (int i = 0; i < additionalComponents.Count; i++) {
                addrProfile.AddTaggedComponent((TaggedComponent)additionalComponents[i]);
            }
            return addrProfile;
        }

        public IorProfile[] GetProfiles() {
            return m_profiles;
        }

        public Uri ParseUrl(out string objectUri, out GiopVersion version) {
            if (m_objAddrs.Length == 0) {
                throw new INTERNAL(8540, CompletionStatus.Completed_MayBe);
            }
            objectUri = ObjectUri;
            return m_objAddrs[0].ParseUrl(objectUri, out version);
        }

        private void CalculateKeyBytesFromKeyString() {
            string id = KeyString;
            // TODO: not really correct: need to resolve %HexHex escape sequences
            m_keyBytes = IorUtil.GetKeyBytesForId(id);
        }

        /// <summary>converts the key string to a byte array, resolving escape sequences</summary>
        public byte[] GetKeyAsByteArray() {
            return m_keyBytes;
        }

        #endregion IMethods

    }

    /// <summary>marker interface to mark a corbaloc obj addr</summary>
    internal interface CorbaLocObjAddr {
        /// <summary>
        /// converts this address to an IorProfile
        /// </summary>
        IorProfile GetProfileForAddr(byte[] objectKey, Codec codec);

        /// <summary>
        /// parses the address into a .NET usable form
        /// </summary>
        Uri ParseUrl(string objectUri, out GiopVersion version);
    }

    /// <summary>
    /// base class for iiop-addresses
    /// </summary>
    internal abstract class CorbaLocIiopAddrBase : CorbaLocObjAddr {

        #region IFields
       
        private GiopVersion m_version;
       
        private string m_host;
       
        private int m_port = 2809; // default is 2809, see CORBA standard, 13.6.10.3
       
        #endregion IFields
        #region IConstructors
       
        protected CorbaLocIiopAddrBase(string addr, int protocolPrefixLength) {
            ParseIiopAddr(addr, protocolPrefixLength);
        }
       
        #endregion IConstructors
        #region IProperties
       
        internal GiopVersion Version {
            get {
                return m_version;
            }
        }
       
        internal string Host {
            get {
                return m_host;
            }
        }
       
        internal int Port {
            get {
                return m_port;
            }
        }
       
        #endregion IProperties
        #region IMethods
       
        /// <summary>parses an iiop-addr string</summary>
        private void ParseIiopAddr(string iiopAddr, int protocolPrefixLength) {
            // version part
            int hostIndex;
            int atIndex = iiopAddr.IndexOf('@', protocolPrefixLength);
            if (atIndex >= 0) {
                // version spec
                string versionPart = iiopAddr.Substring(protocolPrefixLength, atIndex - protocolPrefixLength);
                string[] versionParts = versionPart.Split(new char[] { '.' }, 2);
                try {
                    byte major = System.Byte.Parse(versionParts[0]);
                    if (versionParts.Length != 2) {
                        throw new BAD_PARAM(9, CompletionStatus.Completed_No);
                    }
                    byte minor = System.Byte.Parse(versionParts[1]);
                    m_version = new GiopVersion(major, minor);
                } catch (Exception) {
                    throw new BAD_PARAM(9, CompletionStatus.Completed_No);
                }
                hostIndex = atIndex + 1;
            } else {
                m_version = new GiopVersion(1,0); // default
                hostIndex = protocolPrefixLength;
            }
           
            // host, port part
            int commaIndex = iiopAddr.IndexOf(':', hostIndex);
            if (commaIndex >= 0) {
                m_host = iiopAddr.Substring(hostIndex, commaIndex - hostIndex);
                try {
                    m_port = System.Int32.Parse(iiopAddr.Substring(commaIndex + 1));
                } catch (Exception) {
                    throw new BAD_PARAM(9, CompletionStatus.Completed_No);
                }
            }
            else {
                m_host = iiopAddr.Substring(hostIndex);
            }
            if (StringConversions.IsBlank(m_host)) {
                m_host = "localhost";
            }
        }
       
        public abstract IorProfile GetProfileForAddr(byte[] objectKey, Codec codec);
       
        public abstract Uri ParseUrl(string objectUri, out GiopVersion version);

        #endregion IMethods
    }

    /// <summary>represents an iiop obj_addr in a corbaloc</summary>
    internal class CorbaLocIiopAddr : CorbaLocIiopAddrBase {

        #region IConstructors

        public CorbaLocIiopAddr(string addr) : base(addr, ProtocolPrefixLength(addr)) {
        }

        #endregion IConstructors
        #region IMethods

        private static int ProtocolPrefixLength(string addr) {
            if (addr.StartsWith(":")) {
                return 1;
            } else {
                // cut off iiop:
                return 5;
            }
        }

        public override IorProfile GetProfileForAddr(byte[] objectKey, Codec codec) {
            InternetIiopProfile result = new InternetIiopProfile(Version, Host, (ushort)Port, objectKey);
            return result;
        }

        public override Uri ParseUrl(string objectUri, out GiopVersion version) {
            version = Version;
            return new Uri("iiop" +
                           version.Major + "." + version.Minor +
                           Uri.SchemeDelimiter + Host + ":" + Port);
        }

        #endregion IMethods
        #region SMethods

        /// <summary>
        /// returns true, if this class can handle the specified protocol in the address
        /// </summary>
        public static bool IsResponsibleForProtocol(string addrString) {
            return (addrString.StartsWith(":") || addrString.StartsWith("iiop:"));
        }

        #endregion SMethods
    }


    /// <summary>represents an iiop ssl obj_addr in a corbaloc</summary>
    internal class CorbaLocIiopSslAddr : CorbaLocIiopAddrBase {

        #region IFields
        #endregion IFields
        #region IConstructors

        // cut off is iiop-ssl: 9 chars long
        public CorbaLocIiopSslAddr(string addr)
            : base(addr, 9)
        {}

        #endregion IConstructors
        #region IMethods

        public override IorProfile GetProfileForAddr(byte[] objectKey, Codec codec) {
            InternetIiopProfile result = new InternetIiopProfile(Version, Host, 0, objectKey);
            SSLComponentData sslComp =
                new SSLComponentData(SecurityAssociationOptions.EstablishTrustInClient,
                                     SecurityAssociationOptions.EstablishTrustInTarget,
                                     (short)Port);
            TaggedComponent sslTaggedComp =
                new TaggedComponent(TAG_SSL_SEC_TRANS.ConstVal,
                                    codec.encode_value(sslComp));
            result.AddTaggedComponent(sslTaggedComp);
            return result;
        }

        public override Uri ParseUrl(string objectUri, out GiopVersion version) {
            version = Version;
            return new Uri("iiop-ssl" +
                           version.Major + "." + version.Minor +
                           Uri.SchemeDelimiter + Host + ":" + Port);

        }

        #endregion IMethods
        #region SMethods

        /// <summary>
        /// returns true, if this class can handle the specified protocol in the address
        /// </summary>
        public static bool IsResponsibleForProtocol(string addrString) {
            return addrString.StartsWith("iiop-ssl:");
        }

        #endregion SMethods
    }
}


#if UnitTest

namespace Ch.Elca.Iiop.Tests {

    using Ch.Elca.Iiop.CorbaObjRef;
    using Ch.Elca.Iiop.Interception;
    using Ch.Elca.Iiop.Marshalling;
    using Ch.Elca.Iiop.Services;
    using NUnit.Framework;
   
    /// <summary>
    /// Unit-test for class Corbaloc
    /// </summary>
    [TestFixture]
    public class CorbalocTest {

        private object m_defaultCodeSetTaggedComponent;
        private Codec m_codec;

        public CorbalocTest() {
        }

        [SetUp]
        public void SetUp() {
            SerializerFactory serFactory =
                new SerializerFactory();
            CodecFactory codecFactory =
                new CodecFactoryImpl(serFactory);
            m_codec =
                codecFactory.create_codec(
                    new Encoding(ENCODING_CDR_ENCAPS.ConstVal, 1, 2));
            serFactory.Initalize(new SerializerFactoryConfig(),
                IiopUrlUtil.Create(m_codec,
                                   new object[] {
                                        Services.CodeSetService.CreateDefaultCodesetComponent(m_codec) }));
            m_defaultCodeSetTaggedComponent =
                Services.CodeSetService.CreateDefaultCodesetComponent(m_codec);
        }

        [Test]
        public void TestSingleCompleteCorbaLocIiop() {
            string testCorbaLoc = "corbaloc:iiop:1.2@elca.ch:1234/test";
            Corbaloc parsed = new Corbaloc(testCorbaLoc, m_codec,
                                           new object[] { m_defaultCodeSetTaggedComponent });
            Assert.AreEqual("test", parsed.KeyString);

            Assert.AreEqual(1, parsed.GetProfiles().Length);
            Assert.AreEqual(typeof(InternetIiopProfile), parsed.GetProfiles()[0].GetType());
            InternetIiopProfile profile = (InternetIiopProfile)parsed.GetProfiles()[0];
            Assert.IsTrue(profile.TaggedComponents.ContainsTaggedComponent(
                                CodeSetService.SERVICE_ID));

            Assert.AreEqual(1, profile.Version.Major);
            Assert.AreEqual(2, profile.Version.Minor);
            Assert.AreEqual("elca.ch", profile.HostName);
            Assert.AreEqual(1234, profile.Port);

            // port > 32768
            testCorbaLoc = "corbaloc:iiop:1.2@elca.ch:61234/test";
            parsed = new Corbaloc(testCorbaLoc, m_codec,
                                  new object[] { m_defaultCodeSetTaggedComponent });
            Assert.AreEqual("test", parsed.KeyString);

            Assert.AreEqual(1, parsed.GetProfiles().Length);
            Assert.AreEqual(typeof(InternetIiopProfile), parsed.GetProfiles()[0].GetType());
            profile = (InternetIiopProfile)parsed.GetProfiles()[0];
            Assert.IsTrue(profile.TaggedComponents.ContainsTaggedComponent(
                                CodeSetService.SERVICE_ID));

            Assert.AreEqual(1, profile.Version.Major);
            Assert.AreEqual(2, profile.Version.Minor);
            Assert.AreEqual("elca.ch", profile.HostName);
            Assert.AreEqual(61234, profile.Port);

        }

        [Test]
        public void TestMultipleCompleteCorbaLocIiop() {
            string testCorbaLoc = "corbaloc:iiop:1.2@elca.ch:1234,:1.2@elca.ch:1235,:1.2@elca.ch:1236/test";
            Corbaloc parsed = new Corbaloc(testCorbaLoc, m_codec,
                                           new object[] { m_defaultCodeSetTaggedComponent });
            Assert.AreEqual("test", parsed.KeyString);

            IorProfile[] profiles = parsed.GetProfiles();
            Assert.AreEqual(3, profiles.Length);
            Assert.AreEqual(typeof(InternetIiopProfile), profiles[0].GetType());
            Assert.AreEqual(typeof(InternetIiopProfile), profiles[1].GetType());
            Assert.AreEqual(typeof(InternetIiopProfile), profiles[2].GetType());

            InternetIiopProfile prof = (InternetIiopProfile)(profiles[0]);
            Assert.AreEqual(1, prof.Version.Major);
            Assert.AreEqual(2, prof.Version.Minor);
            Assert.AreEqual("elca.ch", prof.HostName);
            Assert.AreEqual(1234, prof.Port);

            prof = (InternetIiopProfile)(profiles[1]);
            Assert.AreEqual(1, prof.Version.Major);
            Assert.AreEqual(2, prof.Version.Minor);
            Assert.AreEqual("elca.ch", prof.HostName);
            Assert.AreEqual(1235, prof.Port);

            prof = (InternetIiopProfile)(profiles[2]);
            Assert.AreEqual(1, prof.Version.Major);
            Assert.AreEqual(2, prof.Version.Minor);
            Assert.AreEqual("elca.ch", prof.HostName);
            Assert.AreEqual(1236, prof.Port);
        }

        /// <summary>test corba loc with iiop addrs, check the defaults</summary>
        [Test]
        public void TestIncompleteCorbaLocIiop() {
            string testCorbaLoc = "corbaloc::/test";
            Corbaloc parsed = new Corbaloc(testCorbaLoc, m_codec,
                                           new object[] { m_defaultCodeSetTaggedComponent });
            Assert.AreEqual("test", parsed.KeyString);
            IorProfile[] profiles = parsed.GetProfiles();
            Assert.AreEqual(1, profiles.Length);
            Assert.AreEqual(typeof(InternetIiopProfile), profiles[0].GetType());
            InternetIiopProfile prof = (InternetIiopProfile)(profiles[0]);
            Assert.AreEqual(1, prof.Version.Major);
            Assert.AreEqual(0, prof.Version.Minor);
            Assert.AreEqual("localhost", prof.HostName);
            Assert.AreEqual(2809, prof.Port);

            testCorbaLoc = "corbaloc::elca.ch/test";
            parsed = new Corbaloc(testCorbaLoc, m_codec,
                                  new object[] { m_defaultCodeSetTaggedComponent });
            Assert.AreEqual("test", parsed.KeyString);
            profiles = parsed.GetProfiles();
            Assert.AreEqual(1, profiles.Length);
            Assert.AreEqual(typeof(InternetIiopProfile), profiles[0].GetType());
            prof = (InternetIiopProfile)(profiles[0]);
            Assert.AreEqual(1, prof.Version.Major);
            Assert.AreEqual(0, prof.Version.Minor);
            Assert.AreEqual("elca.ch", prof.HostName);
            Assert.AreEqual(2809, prof.Port);

            testCorbaLoc = "corbaloc:iiop:1.2@elca.ch/test";
            parsed = new Corbaloc(testCorbaLoc, m_codec,
                                  new object[] { m_defaultCodeSetTaggedComponent });
            Assert.AreEqual("test", parsed.KeyString);
            profiles = parsed.GetProfiles();
            Assert.AreEqual(1, profiles.Length);
            Assert.AreEqual(typeof(InternetIiopProfile), profiles[0].GetType());
            prof = (InternetIiopProfile)(profiles[0]);
            Assert.AreEqual(1, prof.Version.Major);
            Assert.AreEqual(2, prof.Version.Minor);
            Assert.AreEqual("elca.ch", prof.HostName);
            Assert.AreEqual(2809, prof.Port);

            testCorbaLoc = "corbaloc::elca.ch:1234/test";
            parsed = new Corbaloc(testCorbaLoc, m_codec,
                                  new object[] { m_defaultCodeSetTaggedComponent });
            Assert.AreEqual("test", parsed.KeyString);
            profiles = parsed.GetProfiles();
            Assert.AreEqual(1, profiles.Length);
            Assert.AreEqual(typeof(InternetIiopProfile), profiles[0].GetType());
            prof = (InternetIiopProfile)(profiles[0]);
            Assert.AreEqual(1, prof.Version.Major);
            Assert.AreEqual(0, prof.Version.Minor);
            Assert.AreEqual("elca.ch", prof.HostName);
            Assert.AreEqual(1234, prof.Port);
        }

        [Test]
        public void TestCorbaLocIiopSsl() {
            string testCorbaLoc = "corbaloc:iiop-ssl:elca.ch:1234/test";
            Corbaloc parsed = new Corbaloc(testCorbaLoc, m_codec,
                                           new object[] { m_defaultCodeSetTaggedComponent });
            Assert.AreEqual("test", parsed.KeyString);
            IorProfile[] profiles = parsed.GetProfiles();
            Assert.AreEqual(1, profiles.Length);
            Assert.AreEqual(typeof(InternetIiopProfile), profiles[0].GetType());
            InternetIiopProfile prof = (InternetIiopProfile)(profiles[0]);
            Assert.AreEqual(1, prof.Version.Major);
            Assert.AreEqual(0, prof.Version.Minor);
            Assert.AreEqual("elca.ch", prof.HostName);
            Assert.AreEqual(0, prof.Port);


            Assert.IsTrue(profiles[0].TaggedComponents.ContainsTaggedComponent(
                                 CodeSetService.SERVICE_ID));
            Assert.IsTrue(profiles[0].TaggedComponents.ContainsTaggedComponent(
                                 TAG_SSL_SEC_TRANS.ConstVal));
        }

        [ExpectedException(typeof(BAD_PARAM))]
        [Test]
        public void TestNoCorbaLoc() {
            string otherUrl = "iiop://localhost:8087/test";
            Corbaloc parsed = new Corbaloc(otherUrl, m_codec,
                                           new object[] { m_defaultCodeSetTaggedComponent });
        }

        [Test]
        public void TestParseUrl() {
            string testCorbaLoc = "corbaloc:iiop:1.2@elca.ch:1234/test";
            Corbaloc parsed = new Corbaloc(testCorbaLoc, m_codec,
                                           new object[] { m_defaultCodeSetTaggedComponent });
            string objectUri;
            GiopVersion version;
            Uri channelUri = parsed.ParseUrl(out objectUri, out version);
            Assert.AreEqual("test", objectUri, "object uri");
            Assert.AreEqual(1, version.Major, "version major");
            Assert.AreEqual(2, version.Minor, "version minor");
            Assert.AreEqual("iiop1.2://elca.ch:1234/",
                                   channelUri.AbsoluteUri, "channel uri");
        }

        [Test]
        public void TestParseUrlSsl() {
            string testCorbaLoc = "corbaloc:iiop-ssl:1.2@elca.ch:1234/test";
            Corbaloc parsed = new Corbaloc(testCorbaLoc, m_codec,
                                           new object[] { m_defaultCodeSetTaggedComponent });
            string objectUri;
            GiopVersion version;
            Uri channelUri = parsed.ParseUrl(out objectUri, out version);
            Assert.AreEqual("test", objectUri, "object uri");
            Assert.AreEqual(1, version.Major, "version major");
            Assert.AreEqual(2, version.Minor, "version minor");
            Assert.AreEqual("iiop-ssl1.2://elca.ch:1234/",
                                   channelUri.AbsoluteUri, "channel uri");
        }

    }

}

#endif
