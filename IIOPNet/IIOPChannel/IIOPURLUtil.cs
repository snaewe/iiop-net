/* IIOPURLUtil.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 16.01.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Text;
using System.Runtime.Remoting;
using System.Diagnostics;
using System.Globalization;
using omg.org.CORBA;
using Ch.Elca.Iiop.CorbaObjRef;

namespace Ch.Elca.Iiop.Util {

    /// <summary>
    /// This class is able to handle urls, object uris for the IIOP-channel
    /// </summary>
    /// <remarks>
    /// This class is used to parse url's, parse object uri's, ... 
    /// This is a helper class for the IIOP-channel
    /// </remarks>
    public class IiopUrlUtil {

        #region Constants

        #endregion Constants
        #region SFields

        private static ASCIIEncoding s_asciiEncoder = new ASCIIEncoding();
        
        private static SystemIDGenerator s_sysIdGenerator = new SystemIDGenerator();

        #endregion SFields
        #region IConstructors
        
        private IiopUrlUtil() {
        }

        #endregion IConstructors
        #region SMethods
        
        /// <summary>checks if data is an URL for the IIOP-channel </summary>
        public static bool IsUrl(string data) {
            return (data.StartsWith("iiop") || IsIorString(data) ||
                    data.StartsWith("corbaloc"));
        }

        public static bool IsIorString(string url) {
            return url.StartsWith("IOR");
        }
        
        /// <summary>creates an IOR for the object described by the Url url</summary>
        /// <param name="url">an url of the form IOR:--hex-- or iiop://addr/key</param>
        /// <param name="targetType">if the url contains no info about the target type, use this type</param>
        public static Ior CreateIorForUrl(string url, string repositoryId) {
            Ior ior = null;
            if (url.StartsWith("IOR")) {
                ior = new Ior(url);                    
            } else if (url.StartsWith("iiop")) {
                // iiop1.0, iiop1.1, iiop1.2 (=iiop); extract version in protocol tag

                IiopLoc iiopLoc = new IiopLoc(url);                                
                // now create an IOR with the above information
                InternetIiopProfile profile = new InternetIiopProfile(iiopLoc.Version, 
                                                                      iiopLoc.ChannelUri.Host,                                                                      
                                                                     (ushort)iiopLoc.ChannelUri.Port, 
                                                                     iiopLoc.GetKeyAsByteArray());
                ior = new Ior(repositoryId, new IorProfile[] { profile });
            } else if (url.StartsWith("corbaloc")) {
            	Corbaloc loc = new Corbaloc(url);
            	CorbaLocIiopAddr addr = loc.GetIiopAddr();
            	if (addr == null) {
            		throw new INV_OBJREF(8421, CompletionStatus.Completed_MayBe);
            	}
            	byte[] objectKey = loc.GetKeyAsByteArray();
            	InternetIiopProfile profile = new InternetIiopProfile(addr.Version, addr.Host,
                                                                     (ushort)addr.Port, objectKey);            	
            	ior = new Ior(repositoryId, new IorProfile[] { profile });
            } else {
        	    throw new INV_OBJREF(1963, CompletionStatus.Completed_MayBe);
        	}
            return ior;
        }
        
        /// <summary>
        /// This method parses an url for the IIOP channel. 
        /// It extracts the channel URI and the objectURI
        /// </summary>
        /// <param name="url">the url to parse</param>
        /// <param name="objectURI">the objectURI</param>
        /// <returns>the channel-Uri</returns>
        internal static Uri ParseUrl(string url, out string objectUri, 
                                     out GiopVersion version) {
            Uri uri = null;
            if (url.StartsWith("iiop")) {
                IiopLoc iiopLoc = new IiopLoc(url);
                uri = iiopLoc.ChannelUri;
                objectUri = iiopLoc.ObjectUri;
                version = iiopLoc.Version;
            } else if (url.StartsWith("IOR")) {
                Ior ior = new Ior(url);
                uri = new Uri("iiop" + ior.Version.Major + "." + ior.Version.Minor + 
                              Uri.SchemeDelimiter + ior.HostName+":"+ior.Port);
                objectUri = GetObjectUriForObjectKey(ior.ObjectKey);
                version = ior.Version;
            } else if (url.StartsWith("corbaloc")) {
            	Corbaloc loc = new Corbaloc(url);
            	CorbaLocIiopAddr addr = loc.GetIiopAddr();            	
            	if (addr == null) {
            		throw new INTERNAL(8540, CompletionStatus.Completed_MayBe);
            	}
                objectUri = loc.ObjectUri;
                version = addr.Version;
                uri = new Uri("iiop" + 
                              addr.Version.Major + "." + addr.Version.Minor + 
                              Uri.SchemeDelimiter + addr.Host + ":" + addr.Port);

            } else {
                // not possible
                uri = null;
                objectUri = null;
                version = new GiopVersion(1,0);
            }
            return uri;
        }

        /// <summary>
        /// creates an URL from host, port and objectURI
        /// </summary>
        internal static string GetUrl(string host, int port, string objectUri) {
            return "iiop" + Uri.SchemeDelimiter + host + ":" + port + "/" + objectUri;
        }

        /// <summary>
        /// takes a marshalled object and calculate the corba object key to send from it
        /// </summary>
        /// <param name="objectUri"></param>
        /// <returns></returns>
        internal static byte[] GetObjectKeyForObj(MarshalByRefObject mbr) {
            string objectUri = RemotingServices.GetObjectUri(mbr);
            if (objectUri == null) { 
                throw new INTERNAL(57, CompletionStatus.Completed_MayBe);
            }
            
            if (!s_sysIdGenerator.IsSystemId(objectUri)) {
                // remove appdomain-id in front of uri which is automatically appended 
                // (see comment for RemotingServices.SetObjectUriForMarshal);
                // to support user-id policy, this appdomain-guid must be removed!
                if (objectUri.StartsWith("/")) {
                    objectUri = objectUri.Substring(1);
            }
                int appdomainGuidEndIndex = objectUri.IndexOf("/");
                if (appdomainGuidEndIndex >= 0) {
                    // remove appdomain-guid part
                    objectUri = objectUri.Substring(appdomainGuidEndIndex + 1);
                } else {
                    Debug.WriteLine("warning, appdomain guid not found in object-uri: " +
                                    objectUri);
                }
            }
            // For System-id policy, don't remove the appdomain id, because after
            // a restart of the application, appdomain id-guid prevents the 
            // generation of the same ids
            
            // use ASCII-encoder + unicode escaped to encode uri string
            objectUri = EscapeNonAscii(objectUri);
            return s_asciiEncoder.GetBytes(objectUri);
                }
        
        /// <summary>
        /// generates an IIOP.NET CORBA System-ID
        /// </summary>
        internal static string GenerateSystemId() {
            return s_sysIdGenerator.GenerateId();
            }

        /// <summary>
        /// get the key-bytes for the id; doesn't any more transformations
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal static byte[] GetKeyBytesForId(string id) {
            return s_asciiEncoder.GetBytes(id);
                }
        
        /// <summary>
        /// escape characters, which are not part of the ASCII set
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        internal static string EscapeNonAscii(string uri) {
            StringBuilder result = new StringBuilder();
            string escaped = uri.Replace(@"\u", @"\\u");  // escape \u            
            for (int i = 0; i < escaped.Length; i++) {
                // replace non-ascii char by \u****
                if (escaped[i] <= 0x7f) {
                    result.Append(escaped[i]);    
            } else {
                    result.Append(@"\u" + 
                                  String.Format("{0:X4}", 
                                                Convert.ToInt32(escaped[i])));
                }                
            }
            return result.ToString();
        }

        internal static string GetObjectUriForObjectKey(byte[] objectKey) {
            string result = s_asciiEncoder.GetString(objectKey);
            return UnescapeNonAscii(result);
        }

        /// <summary>
        /// reverse the effect of EscapeNonAscii
        /// </summary>
        private static string UnescapeNonAscii(string uri) {
            StringBuilder result = new StringBuilder();
            StringBuilder potentialEscapeChar = new StringBuilder();            
            for (int i = 0; i < uri.Length; i++) {
                
                if ((uri[i] != '\\') && (potentialEscapeChar.Length == 0)) {
                    result.Append(uri[i]);    
                } else {
                    // either new escape sequence starting with \ or continue of a sequence
                    potentialEscapeChar.Append(uri[i]);
                    if (!IsPotentiallyEscapedCharacterRepresentation(potentialEscapeChar.ToString())) {
                        // no escape sequence, add string directly to result
                        result.Append(potentialEscapeChar.ToString());
                        potentialEscapeChar.Remove(0, potentialEscapeChar.Length);
                    } else if (potentialEscapeChar.Length == 6) {
                        // it's an escape char in form \uQRST
                        int charNr = Int32.Parse(potentialEscapeChar.ToString().Substring(2),
                                                 NumberStyles.HexNumber);
                        char unescaped = Convert.ToChar(charNr);
                        result.Append(unescaped);
                        // chars are handled, remove
                        potentialEscapeChar.Remove(0, potentialEscapeChar.Length);
                    }
                }                
            }
            // undo \u transformation for non-ascii
            result = result.Replace(@"\\u", @"\u");
            return result.ToString();
        }

        /// <summary>
        /// checks, if a given candidate sequence may represent an escaped character
        /// </summary>
        /// <returns>true, if possible, otherwise false</returns>
        private static bool IsPotentiallyEscapedCharacterRepresentation(string candidate) {
            // look for \uQRST
            if (candidate.Length > 6) {
                return false;
            }
            if (!candidate.StartsWith(@"\")) {
                    return false; 
                }
            if ((candidate.Length > 1) && (candidate[1] != 'u')) {
                return false;
            }                
            if (candidate.Length > 2) {
                for (int i = 2; i < candidate.Length; i++) {
                    if (!Char.IsDigit(candidate[i])) {
                        return false;
                    }
                }
            }
            return true;
        }

        #endregion SMethods

    }
        
        /// <summary>
    /// helper class, which supports generation of system id's
        /// </summary>
    internal class SystemIDGenerator {

        #region Constants

        private const string SYSTEM_ID_MARKER = "IIOPNET_SYSTEM_ID/";
        private const int RND_PART_LENGTH = 16;
        
        #endregion Constants
        #region IFields
        
        private long m_seqNr;        
        
        private System.Security.Cryptography.RandomNumberGenerator m_rndGen;
        
        #endregion IFields
        #region IConstructors
        
        internal SystemIDGenerator() {            
            m_seqNr = 0;
            m_rndGen = new System.Security.Cryptography.RNGCryptoServiceProvider();            
        }
        
        #endregion IConstructors
        #region IMethods
        
        private long GetNextSeqNr() {            
            return System.Threading.Interlocked.Increment(ref m_seqNr);
                }                
        
        public string GenerateId() {            
            byte[] rndPart = new byte[RND_PART_LENGTH];
            m_rndGen.GetNonZeroBytes(rndPart);
            string rndString = Convert.ToBase64String(rndPart);
            rndString.Replace('/', '_');
            return SYSTEM_ID_MARKER + rndString + "_" + GetNextSeqNr();
            }            
        
        public bool IsSystemId(string id) {
            return (id.IndexOf(SYSTEM_ID_MARKER) >= 0);
        }

        #endregion IMethods
        
    }
    
    
        /// <summary>
    /// handles addresses of the form iiop://host:port/objectKey
        /// </summary>
    internal class IiopLoc {
        
        #region IFields
        
        private string m_objectUri;
        private Uri m_channelUri;
        private GiopVersion m_version = new GiopVersion(1,2); // default for iiop
        private byte[] m_keyBytes;
        private string m_iiopUrl;
        
        #endregion IFields        
        #region SFields
        
        private static ASCIIEncoding s_asciiEncoder = new ASCIIEncoding();
        
        #endregion SFields
        #region IConstructors
        
        /// <summary>creates the corbaloc from a corbaloc url string</summary>
        public IiopLoc(string iiopUrl) {
            m_iiopUrl = iiopUrl;
            Parse(iiopUrl);
        }

        #endregion IConstructors
        #region IProperties
        
        /// <summary>
        /// gets the url, which is represented by this instance.
        /// </summary>
        public string Url {
            get {
                return m_iiopUrl;
            }
        }
        
        /// <summary>
        /// the string representation of the object uri
        /// </summary>
        public string ObjectUri {
            get {
                return m_objectUri;
            }
        }
        
        /// <summary>
        /// the channel uri: connection information (host, port)
        /// </summary>
        public Uri ChannelUri {
            get {                
                return m_channelUri;
            }
        }

        /// <summary>
        /// the giop protocol version
        /// </summary>
        public GiopVersion Version {
            get {
                return m_version;
            }
        }
        
        #endregion IProperties
        #region IMethods
        
        private void Parse(string iiopUrl) {
            Uri uri = new Uri(iiopUrl);
            string iiopSchema = uri.Scheme;
            if (!iiopSchema.StartsWith("iiop")) {
                throw new INTERNAL(145, CompletionStatus.Completed_MayBe);
            }
            try {
                if (iiopSchema.Length > 4) {
                    // a giop-version is specified in the schema
                    string versionString = iiopSchema.Substring(4);

                    byte major = Byte.Parse(versionString[0].ToString());
                    byte minor = Byte.Parse(versionString[2].ToString());
                    m_version = new GiopVersion(major, minor);                    
                } 
                m_channelUri = new Uri(uri.Scheme + Uri.SchemeDelimiter + 
                                       uri.Host + ":" + uri.Port);
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

        #endregion IMethods


    }
    
    
}
