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
using omg.org.CORBA;

namespace Ch.Elca.Iiop.CorbaObjRef {

	internal class Corbaloc {
    	    	
    	#region IFields
    		
		/// <summary>the key string as string</summary>
		private string m_keyString;
		
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
    	
    	public string KeyString {
    		get {
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
	    		if (parts[i].StartsWith(":") || parts[i].StartsWith("iiop:")) {
	    			m_objAddrs[i] = new CorbaLocIiopAddr(parts[i]);
	    		} else if (parts[i].StartsWith("rir:")) {
	    			ParseRirAddr(parts[i]);
	    		} else {
	    			throw new BAD_PARAM(8, CompletionStatus.Completed_No);
	    		}
	    	}	    	
	    }
	    	    
	    /// <summary>parse rir protocol addr</summary>
	    private void ParseRirAddr(string rirAddr) {
	    	// not supported yet
	    	throw new BAD_PARAM(11, CompletionStatus.Completed_No);
	    }
    
    	#endregion IMethods
    
	}
	
	/// <summary>marker interface to mark a corbaloc obj addr</summary>
	internal interface CorbaLocObjAddr {		
	}
	
	/// <summary>represents an iiop obj_addr in a corbaloc</summary>
	internal class CorbaLocIiopAddr : CorbaLocObjAddr {
		
		#region IFields
		
		private GiopVersion m_version;
		
		private string m_host;
		
		private int m_port = 2809; // default is 2809, see CORBA standard, 13.6.10.3
		
		#endregion IFields
		#region IConstructors
		
		public CorbaLocIiopAddr(string addr) {
			ParseIiopAddr(addr);	
		}
		
		#endregion IConstructors
		#region IProperties
		
		public GiopVersion Version {
			get {
				return m_version;
			}
		}
		
		public string Host {
			get {
				return m_host;
			}
		}
		
		public int Port {
			get {
				return m_port;
			}
		}
		
		#endregion IProperties
		#region IMethods
		
		/// <summary>parses an iiop-addr string</summary>
		private void ParseIiopAddr(string iiopAddr) {
	    	string specificPart;
	    	if (iiopAddr.StartsWith(":")) {
	    		specificPart = iiopAddr.Substring(1);
	    	} else {
	    		// cut off iiop
	    		specificPart = iiopAddr.Substring(5);
	    	}
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
	    
	    #endregion IMethods

		
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
        	CorbaLocIiopAddr addr = (CorbaLocIiopAddr)(parsed.ObjAddrs[0]);
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
        }
        
    }

}

#endif
