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
using omg.org.CORBA;
using Ch.Elca.Iiop.Util;
using Ch.Elca.Iiop.Security.Ssl;

namespace Ch.Elca.Iiop.CorbaObjRef {

	internal class Corbaloc {
    	    	
    	#region IFields
    		
		/// <summary>the key string as string</summary>
		private string m_keyString;
	    private byte[] m_keyBytes;
		
		private CorbaLocObjAddr[] m_objAddrs;
    	
    	#endregion IFields
    	#region IConstructors
    	
    	/// <summary>creates the corbaloc from a corbaloc url string</summary>
    	public Corbaloc(string corbalocUrl) {
            Parse(corbalocUrl);
    	}
    
    	#endregion IConstructors
    	#region IProperties
    	
    	/// <summary>the obj_addr_list</summary>
    	public CorbaLocObjAddr[] ObjAddrs {
    		get {
    			return m_objAddrs;
    		}
    	}
    	
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
    	private void Parse(string corbalocUrl) {
    		if (!corbalocUrl.StartsWith("corbaloc:")) {
    			throw new BAD_PARAM(7, CompletionStatus.Completed_No);
    		}
    		string corbaloc = corbalocUrl.Substring(9);
    		string addrPart = corbaloc;
    		if (corbaloc.IndexOf("/") >= 0) {
    		    m_keyString = corbaloc.Substring(corbaloc.IndexOf("/") + 1);
    			addrPart = corbaloc.Substring(0, corbaloc.IndexOf("/"));
    		}
    		if ((addrPart.StartsWith("iiop:") || addrPart.StartsWith(":")) &&
    		    (m_keyString == null)) {
    		    throw new BAD_PARAM(10, CompletionStatus.Completed_No);
    		}
    		ParseAddrList(addrPart);    		
    	    // create key bytes from keystring
    	    CalculateKeyBytesFromKeyString();
	    }
	    
	    /// <summary>parses the addr list</summary>
	    private void ParseAddrList(string addrList) {
	    	if (addrList == null) {
	    		throw new BAD_PARAM(8, CompletionStatus.Completed_No);
	    	}
	    	string[] parts = addrList.Split(',');
            // at least one!
            m_objAddrs = new CorbaLocObjAddr[parts.Length];
	    	for (int i = 0; i < parts.Length; i++) {
	    	    if (CorbaLocIiopAddr.IsResponsibleForProtocol(parts[i])) {
	    			m_objAddrs[i] = new CorbaLocIiopAddr(parts[i]);
	    		} else if (CorbaLocIiopSslAddr.IsResponsibleForProtocol(parts[i])) {
	    		    m_objAddrs[i] = new CorbaLocIiopSslAddr(parts[i]);
	    		} else {
	    			throw new BAD_PARAM(8, CompletionStatus.Completed_No);
	    		}
	    	}	    	
	    }

	    public IorProfile[] GetProfiles() {
	        ArrayList resultList = new ArrayList();
	        foreach (CorbaLocObjAddr addr in m_objAddrs) {
	            resultList.AddRange(addr.GetProfilesForAddr(GetKeyAsByteArray()));        
	        }
	        IorProfile[] result = (IorProfile[])resultList.ToArray(ReflectionHelper.IorProfileType);
            if (result.Length == 0) {
                throw new INV_OBJREF(8421, CompletionStatus.Completed_MayBe);
            }
            return result;
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
            m_keyBytes = IiopUrlUtil.GetKeyBytesForId(id);
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
        /// converts the address to IorProfiles
        /// </summary>
        IorProfile[] GetProfilesForAddr(byte[] objectKey);
        
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
		
		public CorbaLocIiopAddrBase(string addr) {
			ParseIiopAddr(addr);
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
		/// returns the length of the protocol prefix, e.g. 5 for iiop:
		/// </summary>
		protected abstract int ProtocolPrefixLength(string addr);
		
		/// <summary>parses an iiop-addr string</summary>
		private void ParseIiopAddr(string iiopAddr) {
	    	// cut off protocol part
	    	string specificPart = iiopAddr.Substring(ProtocolPrefixLength(iiopAddr));
	    	// version part
	    	if (specificPart.IndexOf("@") >= 0) {
	    		// version spec
	    		string versionPart = specificPart.Substring(0, specificPart.IndexOf("@"));
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
	    		specificPart = specificPart.Substring(specificPart.IndexOf("@") + 1);	    		
	    	} else {
				m_version = new GiopVersion(1,0); // default	    	
	    	}
	    	
	    	// host, port part
	    	m_host = specificPart;
			if (specificPart.IndexOf(":") >= 0) {
				m_host = specificPart.Substring(0, specificPart.IndexOf(":"));
				try {
					m_port = System.Int32.Parse(specificPart.Substring(specificPart.IndexOf(":") + 1));
				} catch (Exception) {
					throw new BAD_PARAM(9, CompletionStatus.Completed_No);
				}
			}
			if (m_host.Trim().Equals("")) {
				m_host = "localhost";
			}
	    	
	    }
	    
	    public abstract IorProfile[] GetProfilesForAddr(byte[] objectKey);
	    
	    public abstract Uri ParseUrl(string objectUri, out GiopVersion version);

	    
	    #endregion IMethods

	}
	
    /// <summary>represents an iiop obj_addr in a corbaloc</summary>
    internal class CorbaLocIiopAddr : CorbaLocIiopAddrBase {
        
        #region IConstructors

        public CorbaLocIiopAddr(string addr) : base(addr) {
        }
        
        #endregion IConstructors
        #region IMethods
    
        protected override int ProtocolPrefixLength(string addr) {
            if (addr.StartsWith(":")) {
                return 1;
            } else {
                // cut off iiop:
                return 5;
            }
        }

        public override IorProfile[] GetProfilesForAddr(byte[] objectKey) {	        
            InternetIiopProfile result = new InternetIiopProfile(Version, Host, (short)Port, objectKey);
            return new IorProfile[] { result };
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

        #region IConstructors

        public CorbaLocIiopSslAddr(string addr) : base(addr) {
        }

        #endregion IConstructors
        #region IMethods
    
        protected override int ProtocolPrefixLength(string addr) {
            // cut off iiop-ssl:
            return 9;
        }
        
        public override IorProfile[] GetProfilesForAddr(byte[] objectKey) {
            InternetIiopProfile result = new InternetIiopProfile(Version, Host, 0, objectKey);
            result.AddTaggedComponents(new ITaggedComponent[] { new TaggedComponent(TaggedComponentIds.TAG_SSL_SEC_TRANS,
                                                                                    new SSLComponentData(SecurityAssociationOptions.EstablishTrustInClient,
                                                                                                         SecurityAssociationOptions.EstablishTrustInTarget,
                                                                                                         (short)Port)) });
            return new IorProfile[] { result };
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
	
    using NUnit.Framework;
    using Ch.Elca.Iiop.CorbaObjRef;
    
    /// <summary>
    /// Unit-test for class Corbaloc
    /// </summary>
    public class CorbalocTest : TestCase {
        
        public CorbalocTest() {
        }

        
        public void TestSingleCompleteCorbaLocIiop() {
			string testCorbaLoc = "corbaloc:iiop:1.2@elca.ch:1234/test";
        	Corbaloc parsed = new Corbaloc(testCorbaLoc);
        	Assertion.AssertEquals("test", parsed.KeyString);
        	Assertion.AssertEquals(1, parsed.ObjAddrs.Length);
        	Assertion.AssertEquals(typeof(CorbaLocIiopAddr), parsed.ObjAddrs[0].GetType());
        	CorbaLocIiopAddr addr = (CorbaLocIiopAddr)(parsed.ObjAddrs[0]);
        	Assertion.AssertEquals(1, addr.Version.Major);
        	Assertion.AssertEquals(2, addr.Version.Minor);
        	Assertion.AssertEquals("elca.ch", addr.Host);
        	Assertion.AssertEquals(1234, addr.Port);
        }
        
        public void TestMultipleCompleteCorbaLocIiop() {
        	string testCorbaLoc = "corbaloc:iiop:1.2@elca.ch:1234,:1.2@elca.ch:1235,:1.2@elca.ch:1236/test";
        	Corbaloc parsed = new Corbaloc(testCorbaLoc);
        	Assertion.AssertEquals("test", parsed.KeyString);
        	Assertion.AssertEquals(3, parsed.ObjAddrs.Length);
        	Assertion.AssertEquals(typeof(CorbaLocIiopAddr), parsed.ObjAddrs[0].GetType());
        	Assertion.AssertEquals(typeof(CorbaLocIiopAddr), parsed.ObjAddrs[1].GetType());
        	Assertion.AssertEquals(typeof(CorbaLocIiopAddr), parsed.ObjAddrs[2].GetType());
        	
        	CorbaLocIiopAddr addr = (CorbaLocIiopAddr)(parsed.ObjAddrs[0]);
        	Assertion.AssertEquals(1, addr.Version.Major);
        	Assertion.AssertEquals(2, addr.Version.Minor);
        	Assertion.AssertEquals("elca.ch", addr.Host);
        	Assertion.AssertEquals(1234, addr.Port);
        	
        	addr = (CorbaLocIiopAddr)(parsed.ObjAddrs[1]);
        	Assertion.AssertEquals(1, addr.Version.Major);
        	Assertion.AssertEquals(2, addr.Version.Minor);
        	Assertion.AssertEquals("elca.ch", addr.Host);
        	Assertion.AssertEquals(1235, addr.Port);
        	
        	addr = (CorbaLocIiopAddr)(parsed.ObjAddrs[2]);
        	Assertion.AssertEquals(1, addr.Version.Major);
        	Assertion.AssertEquals(2, addr.Version.Minor);
        	Assertion.AssertEquals("elca.ch", addr.Host);
        	Assertion.AssertEquals(1236, addr.Port);        	
        }
        
        /// <summary>test corba loc with iiop addrs, check the defaults</summary>
        public void TestIncompleteCorbaLocIiop() {
        	string testCorbaLoc = "corbaloc::/test";
        	Corbaloc parsed = new Corbaloc(testCorbaLoc);
        	Assertion.AssertEquals("test", parsed.KeyString);
        	Assertion.AssertEquals(1, parsed.ObjAddrs.Length);
        	Assertion.AssertEquals(typeof(CorbaLocIiopAddr), parsed.ObjAddrs[0].GetType());
        	CorbaLocIiopAddrBase addr = (CorbaLocIiopAddr)(parsed.ObjAddrs[0]);
        	Assertion.AssertEquals(1, addr.Version.Major);
        	Assertion.AssertEquals(0, addr.Version.Minor);
        	Assertion.AssertEquals("localhost", addr.Host);
        	Assertion.AssertEquals(2809, addr.Port);
        	
        	testCorbaLoc = "corbaloc::elca.ch/test";
        	parsed = new Corbaloc(testCorbaLoc);
        	Assertion.AssertEquals("test", parsed.KeyString);
        	Assertion.AssertEquals(1, parsed.ObjAddrs.Length);
        	Assertion.AssertEquals(typeof(CorbaLocIiopAddr), parsed.ObjAddrs[0].GetType());
        	addr = (CorbaLocIiopAddr)(parsed.ObjAddrs[0]);
        	Assertion.AssertEquals(1, addr.Version.Major);
        	Assertion.AssertEquals(0, addr.Version.Minor);
        	Assertion.AssertEquals("elca.ch", addr.Host);
        	Assertion.AssertEquals(2809, addr.Port);
        	
        	testCorbaLoc = "corbaloc:iiop:1.2@elca.ch/test";
        	 parsed = new Corbaloc(testCorbaLoc);
        	Assertion.AssertEquals("test", parsed.KeyString);
        	Assertion.AssertEquals(1, parsed.ObjAddrs.Length);
        	Assertion.AssertEquals(typeof(CorbaLocIiopAddr), parsed.ObjAddrs[0].GetType());
        	addr = (CorbaLocIiopAddr)(parsed.ObjAddrs[0]);
        	Assertion.AssertEquals(1, addr.Version.Major);
        	Assertion.AssertEquals(2, addr.Version.Minor);
        	Assertion.AssertEquals("elca.ch", addr.Host);
        	Assertion.AssertEquals(2809, addr.Port);
        	
        	testCorbaLoc = "corbaloc::elca.ch:1234/test";
        	parsed = new Corbaloc(testCorbaLoc);
        	Assertion.AssertEquals("test", parsed.KeyString);
        	Assertion.AssertEquals(1, parsed.ObjAddrs.Length);
        	Assertion.AssertEquals(typeof(CorbaLocIiopAddr), parsed.ObjAddrs[0].GetType());
        	addr = (CorbaLocIiopAddr)(parsed.ObjAddrs[0]);
        	Assertion.AssertEquals(1, addr.Version.Major);
        	Assertion.AssertEquals(0, addr.Version.Minor);
        	Assertion.AssertEquals("elca.ch", addr.Host);
        	Assertion.AssertEquals(1234, addr.Port);
            
        	testCorbaLoc = "corbaloc:iiop-ssl:elca.ch:1234/test";
        	parsed = new Corbaloc(testCorbaLoc);
        	Assertion.AssertEquals("test", parsed.KeyString);
        	Assertion.AssertEquals(1, parsed.ObjAddrs.Length);
        	Assertion.AssertEquals(typeof(CorbaLocIiopSslAddr), parsed.ObjAddrs[0].GetType());
        	addr = (CorbaLocIiopSslAddr)(parsed.ObjAddrs[0]);
        	Assertion.AssertEquals(1, addr.Version.Major);
        	Assertion.AssertEquals(0, addr.Version.Minor);
        	Assertion.AssertEquals("elca.ch", addr.Host);
        	Assertion.AssertEquals(1234, addr.Port);
            
        }
        
    }

}

#endif
