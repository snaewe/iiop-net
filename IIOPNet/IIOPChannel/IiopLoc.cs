/* IiopLoc.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 24.12.04  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
 * 
 * Copyright 2004 Dominic Ullmann
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
using System.Net;
using System.Text;
using Ch.Elca.Iiop.Security.Ssl;
using Ch.Elca.Iiop.Util;
using omg.org.CORBA;


namespace Ch.Elca.Iiop.CorbaObjRef {


   /// <summary>
    /// handles addresses of the form iiop://host:port/objectKey
    /// </summary>
    internal class IiopLoc {
        
        #region IFields
        
        private string m_objectUri;
        private byte[] m_keyBytes;        
        private IiopLocObjAddr m_objAddr;        
        
        #endregion IFields        
        #region SFields
        
        private static ASCIIEncoding s_asciiEncoder = new ASCIIEncoding();
        
        #endregion SFields
        #region IConstructors
        
        /// <summary>creates the corbaloc from a corbaloc url string</summary>
        public IiopLoc(string iiopUrl) {            
            Parse(iiopUrl);
        }

        #endregion IConstructors
        #region IProperties
                
        /// <summary>
        /// the string representation of the object uri
        /// </summary>
        public string ObjectUri {
            get {
                return m_objectUri;
            }
        }
        
        /// <summary>the obj addr</summary>
        public IiopLocObjAddr ObjAddr {
            get {
                return m_objAddr;
            }
        }        
                
        #endregion IProperties
        #region IMethods
        
        private void Parse(string iiopUrl) {
            Uri uri = new Uri(iiopUrl);            
            if (IiopLocIiopAddr.IsResponsibleForProtocol(uri.Scheme)) {
                m_objAddr = new IiopLocIiopAddr(uri.Scheme, uri.Host, uri.Port);        
            } else if (IiopLocIiopSslAddr.IsResponsibleForProtocol(uri.Scheme)) {
                m_objAddr = new IiopLocIiopSslAddr(uri.Scheme, uri.Host, uri.Port);        
            } else {            
                throw new INTERNAL(145, CompletionStatus.Completed_MayBe);
            }
            try {
                m_objectUri = uri.PathAndQuery;
                if ((m_objectUri != null) && (m_objectUri.StartsWith("/"))) {
                    m_objectUri = m_objectUri.Substring(1);
                    string escaped = IiopUrlUtil.EscapeNonAscii(m_objectUri);
                    m_keyBytes = IiopUrlUtil.GetKeyBytesForId(escaped);
                }                
            } catch (Exception) {
                throw new INV_OBJREF(146, CompletionStatus.Completed_MayBe);
            }
        }
        
        /// <summary>
        /// get the byte representation of the corba object key.
        /// </summary>
        /// <returns></returns>
        public byte[] GetKeyAsByteArray() {
            return m_keyBytes;
        }        
        
        public IorProfile[] GetProfiles() {
            IorProfile[] result = new IorProfile[] { 
                m_objAddr.GetProfileForAddr(GetKeyAsByteArray()) };
            return result;
        }
        
        public Uri ParseUrl(out string objectUri, out GiopVersion version) {
            objectUri = ObjectUri;
            return m_objAddr.ParseUrl(objectUri, out version);
        }

        #endregion IMethods

    }
    
    /// <summary>marker interface to mark a corbaloc obj addr</summary>
    internal interface IiopLocObjAddr {
        /// <summary>
        /// converts the address to IorProfiles
        /// </summary>
        IorProfile GetProfileForAddr(byte[] objectKey);
        
        /// <summary>
        /// parses the address into a .NET usable form
        /// </summary>
        Uri ParseUrl(string objectUri, out GiopVersion version);
    }
    
    /// <summary>
    /// base class for iiop-addresses
    /// </summary>
    internal abstract class IiopLocIiopAddrBase : IiopLocObjAddr {

        #region IFields
        
        private GiopVersion m_version;
        
        private string m_host;        
        private int m_port;
        
        #endregion IFields
        #region IConstructors
        
        public IiopLocIiopAddrBase(string scheme, string host, int port) {
            m_host = host;
            m_port = port;
            ParseIiopScheme(scheme);
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

        /// <summary>
        /// returns the length of the protocol prefix, e.g. 4 for iiop
        /// </summary>
        protected abstract int ProtocolPrefixLength(string scheme);
        
        /// <summary>parses a scheme inside an iiop-url</summary>
        private void ParseIiopScheme(string scheme) {
            // cut off protocol part
            string specificPart = scheme.Substring(ProtocolPrefixLength(scheme));
            // version part
            if (specificPart.Length > 0) {
                // version spec
                try {
                    // parse version string
                    byte major = Byte.Parse(specificPart[0].ToString());
                    byte minor = Byte.Parse(specificPart[2].ToString());
                    m_version = new GiopVersion(major, minor);
                } catch (Exception) {
                    throw new BAD_PARAM(9, CompletionStatus.Completed_No);                    
                }
            } else {
                m_version = new GiopVersion(1,2); // default
            }
        }
        
        public abstract IorProfile GetProfileForAddr(byte[] objectKey);
        
        public abstract Uri ParseUrl(string objectUri, out GiopVersion version);

        #endregion IMethods

    }
    
    /// <summary>represents an iiop addr</summary>
    internal class IiopLocIiopAddr : IiopLocIiopAddrBase {
        
        #region IConstructors

        public IiopLocIiopAddr(string scheme, string host, int port) : base(scheme, host, port) {
        }
        
        #endregion IConstructors
        #region IMethods
    
        protected override int ProtocolPrefixLength(string scheme) {
            return 4;
        }

        public override IorProfile GetProfileForAddr(byte[] objectKey) {
            InternetIiopProfile result = new InternetIiopProfile(Version, Host, (short)Port, objectKey);
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
        /// returns true, if this class can handle the specified protocol in the address scheme
        /// </summary>
        public static bool IsResponsibleForProtocol(string scheme) {
            return scheme.StartsWith("iiop") && (!scheme.StartsWith("iiop-"));
        }

        #endregion SMethods


    }


    /// <summary>represents an iiop ssl addr</summary>
    internal class IiopLocIiopSslAddr : IiopLocIiopAddrBase {

        #region IConstructors

        public IiopLocIiopSslAddr(string scheme, string host, int port) : base(scheme, host, port) {
        }

        #endregion IConstructors
        #region IMethods
    
        protected override int ProtocolPrefixLength(string addr) {
            // cut off iiop-ssl
            return 8;
        }
        
        public override IorProfile GetProfileForAddr(byte[] objectKey) {
            InternetIiopProfile result = new InternetIiopProfile(Version, Host, 0, objectKey);
            result.AddTaggedComponents(new ITaggedComponent[] { new TaggedComponent(TaggedComponentIds.TAG_SSL_SEC_TRANS,
                                                                                    new SSLComponentData(SecurityAssociationOptions.EstablishTrustInClient,
                                                                                                         SecurityAssociationOptions.EstablishTrustInTarget,
                                                                                                         (short)Port)) });
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
        /// returns true, if this class can handle the specified protocol in the address scheme
        /// </summary>
        public static bool IsResponsibleForProtocol(string scheme) {
            return scheme.StartsWith("iiop-ssl");
        }

        #endregion SMethods



    }


}


#if UnitTest

namespace Ch.Elca.Iiop.Tests {
	
    using NUnit.Framework;
    using Ch.Elca.Iiop.CorbaObjRef;
    
    /// <summary>
    /// Unit-test for class Corbaloc
    /// </summary>
    public class IioplocTest : TestCase {
        
        public IioplocTest() {
        }

        
        public void TestIiopLoc() {
            string testIiopLoc = "iiop://elca.ch:1234/test";
            IiopLoc parsed = new IiopLoc(testIiopLoc);
            Assertion.AssertEquals("test", parsed.ObjectUri);
            Assertion.AssertEquals(typeof(IiopLocIiopAddr), parsed.ObjAddr.GetType());
            IiopLocIiopAddr addr = (IiopLocIiopAddr)(parsed.ObjAddr);
            Assertion.AssertEquals(1, addr.Version.Major);
            Assertion.AssertEquals(2, addr.Version.Minor);
            Assertion.AssertEquals("elca.ch", addr.Host);
            Assertion.AssertEquals(1234, addr.Port);
            
            testIiopLoc = "iiop1.1://elca.ch:1234/test";
            parsed = new IiopLoc(testIiopLoc);
            Assertion.AssertEquals("test", parsed.ObjectUri);
            Assertion.AssertEquals(typeof(IiopLocIiopAddr), parsed.ObjAddr.GetType());
            addr = (IiopLocIiopAddr)(parsed.ObjAddr);
            Assertion.AssertEquals(1, addr.Version.Major);
            Assertion.AssertEquals(1, addr.Version.Minor);
            Assertion.AssertEquals("elca.ch", addr.Host);
            Assertion.AssertEquals(1234, addr.Port);            
        }
        
        public void TestIiopSslLoc() {
            string testIiopLoc = "iiop-ssl://elca.ch:1234/test";
            IiopLoc parsed = new IiopLoc(testIiopLoc);
            Assertion.AssertEquals("test", parsed.ObjectUri);
            Assertion.AssertEquals(typeof(IiopLocIiopSslAddr), parsed.ObjAddr.GetType());
            IiopLocIiopSslAddr addr = (IiopLocIiopSslAddr)(parsed.ObjAddr);
            Assertion.AssertEquals(1, addr.Version.Major);
            Assertion.AssertEquals(2, addr.Version.Minor);
            Assertion.AssertEquals("elca.ch", addr.Host);
            Assertion.AssertEquals(1234, addr.Port);
            
            testIiopLoc = "iiop-ssl1.1://elca.ch:1234/test";
            parsed = new IiopLoc(testIiopLoc);
            Assertion.AssertEquals("test", parsed.ObjectUri);
            Assertion.AssertEquals(typeof(IiopLocIiopSslAddr), parsed.ObjAddr.GetType());
            addr = (IiopLocIiopSslAddr)(parsed.ObjAddr);
            Assertion.AssertEquals(1, addr.Version.Major);
            Assertion.AssertEquals(1, addr.Version.Minor);
            Assertion.AssertEquals("elca.ch", addr.Host);
            Assertion.AssertEquals(1234, addr.Port);            
        }
        
    }

}

#endif
