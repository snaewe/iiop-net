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

        private const byte DEFAULT_GIOP_MAJOR = 1;
        private const byte DEFAULT_GIOP_MINOR = 2;

        private const string STRINGIFIED_ID = "str";
        private const string GIOP_ID = "GIOP";

        #endregion Constants
        #region SFields

        private static byte[] s_nonStringifyTag = new byte[] { 44, 115, 116, 114, 61, 102 };
        private static UnicodeEncoding s_cachedEncoder = new UnicodeEncoding(false, false);

        #endregion SFields
        #region IConstructors
        
        private IiopUrlUtil() {
        }

        #endregion IConstructors
        #region SMethods
        
        /// <summary>checks if data is an URL for the IIOP-channel </summary>
        public static bool IsUrl(string data) {
            return (data.StartsWith("iiop") || data.StartsWith("IOR"));
        }
        
        /// <summary>creates an IOR for the object described by the Url url</summary>
        /// <param name="url">an url of the form IOR:--hex-- or iiop://addr/key</param>
        /// <param name="targetType">if the url contains no info about the target type, use this type</param>
        public static Ior CreateIorForUrl(string url, string repositoryId) {
            Ior ior = null;
            if (url.StartsWith("IOR")) {
                ior = new Ior(url);                    
            } else if (url.StartsWith("iiop")) {
                string objectURI;
                GiopVersion version;

                Uri chanUri = IiopUrlUtil.ParseUrl(url, out objectURI);
                byte[] objectKey = IiopUrlUtil.GetObjectInfoForObjUri(objectURI, out version);
                // now create an IOR with the above information
                InternetIiopProfile profile = new InternetIiopProfile(version, chanUri.Host,
                                                                     (ushort)chanUri.Port, objectKey);
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
        public static Uri ParseUrl(string url, out string objectUri) {
            Uri uri = null;
            if (url.StartsWith("iiop:")) {
                // skip
                uri = new Uri(url);
                objectUri = uri.PathAndQuery;
                if ((objectUri != null) && (objectUri.StartsWith("/"))) {
                    objectUri = objectUri.Substring(1);
                }
            } else if (url.StartsWith("IOR:")) {
                Ior ior = new Ior(url);
                uri = new Uri("iiop://"+ior.HostName+":"+ior.Port);
                objectUri = GetObjUriForObjectInfo(ior.ObjectKey, ior.Version);
            } else if (url.StartsWith("corbaloc:")) {
            	Corbaloc loc = new Corbaloc(url);
            	CorbaLocIiopAddr addr = loc.GetIiopAddr();            	
            	if (addr == null) {
            		throw new INTERNAL(8540, CompletionStatus.Completed_MayBe);
            	}
            	uri = new Uri("iiop://" + addr.Host + ":" + addr.Port);
            	byte[] objectKey = loc.GetKeyAsByteArray();
            	objectUri = GetObjUriForObjectInfo(objectKey, addr.Version);
            }else {
                // not possible
                uri = null;
                objectUri = null;
            }
            return uri;
        }

        /// <summary>
        /// creates an URL from host, port and objectURI
        /// </summary>
        public static string GetUrl(string host, int port, string objectUri) {
            return "iiop://" + host + ":" + port + "/" + objectUri;
        }

        /// <summary>takes an objectURI and extracts the information needed
        /// for CORBA call out of it
        /// </summary>
        /// <returns>the CORBA object key</returns>
        public static byte[] GetObjectInfoForObjUri(string objectUri,
                                                    out GiopVersion version) {
            version = new GiopVersion(DEFAULT_GIOP_MAJOR, DEFAULT_GIOP_MINOR);
            if (objectUri == null) { 
                return null; 
            }
            bool stringified = false;
            string objectId = objectUri;
            
            if (objectUri.IndexOf(",") > 0) { 
                objectId = objectUri.Substring(0, objectUri.IndexOf(",")); 
            }
            if (objectUri.IndexOf(STRINGIFIED_ID) > 0) {
                string isStr = objectUri.Substring(objectUri.IndexOf(STRINGIFIED_ID) +
                                                   STRINGIFIED_ID.Length, 2);
                if (isStr.StartsWith("=t")) { 
                    stringified = true; 
                }
            }
            if (objectUri.IndexOf(GIOP_ID) > 0) {
                string versionStr = objectUri.Substring(objectUri.IndexOf(GIOP_ID) +
                                                        GIOP_ID.Length, 4);
                if (!(versionStr.StartsWith("="))) { 
                    // uri contains malfromed giop-version-info: versionStr
                    throw new INV_OBJREF(9401, CompletionStatus.Completed_No);
                }
                byte giopMajor = Convert.ToByte(versionStr.Substring(1, 1));
                byte giopMinor = Convert.ToByte(versionStr.Substring(3, 1));
                version = new GiopVersion(giopMajor, giopMinor);
            }

            if (stringified) {
                // check for leading appdomain-id in objectId and remove it (e.g. for initial nameing context, this is the case)
                if ((objectId.IndexOf("/") >= 0) && 
                    (objectId.LastIndexOf("/") + 1 < objectId.Length)) {
                    objectId = objectId.Substring(objectId.LastIndexOf("/") + 1);
                }
                return StringConversions.Destringify(objectId);
            } else {
                byte[] result = s_cachedEncoder.GetBytes(objectId);
                return AppendNonStringifyTag(result); // append a tag to indicate that this object key should not be stringified
            }
        }

        public static byte[] GetObjectKeyForObjUri(string objectUri) {
            GiopVersion version;
            return GetObjectInfoForObjUri(objectUri, out version);
        }

        private static byte[] AppendNonStringifyTag(byte[] objKey) {
            byte[] extObjKey = new byte[objKey.Length + 6];
            Array.Copy((Array)objKey, 0, (Array)extObjKey, 0, objKey.Length);
            Array.Copy((Array)s_nonStringifyTag, 0, (Array)extObjKey,
                       extObjKey.Length-6, s_nonStringifyTag.Length);
            return extObjKey;
        }

        /// <summary>
        /// checks if the non-stringify tag is set in the object key
        /// </summary>
        /// <remarks>
        /// if this tag is set, the objectURI is created without stringify the object-key,
        /// it's interpreted as 2 byte characters array
        /// </remarks>
        private static bool CheckNonStringifyTag(byte[] objectKey) {
            if (objectKey.Length < 6) { return false; }
            for (int i = 0; i < 6; i++) {
                if (objectKey[objectKey.Length-6+i] != s_nonStringifyTag[i]) { 
                    return false; 
                }
            }
            return true;
        }

        
        /// <summary>
        /// take a CORBA object Key and the GIOP-Version
        /// and create a .NET obj-URI out of it
        /// If giopVersion is null -> do not include version info into uri.
        /// </summary>
        /// <param name="objectKey"></param>
        /// <returns></returns>
        private static string GetObjUriForObjectKeyAndOptInfo(byte[] objectKey, object giopVersion) {
            if (CheckNonStringifyTag(objectKey)) {
                // an URI pointing to a native .NET remoting framework object, therefore the .NET URI must be
                // reproduced from which this objectKey was created
                return s_cachedEncoder.GetString(objectKey, 0, objectKey.Length-6);
            } else {
                // stringify it
                string result = StringConversions.Stringify(objectKey);
                if (giopVersion != null) {
                    result += "," + GIOP_ID + "=" + ((GiopVersion)giopVersion).Major + "." +
                              ((GiopVersion)giopVersion).Minor;
                }                
                return  result + "," + STRINGIFIED_ID + "=t";
            }            
        }

        /// <summary>
        /// take a CORBA object Key and the GIOP-Version
        /// and create a .NET obj-URI out of it
        /// </summary>
        /// <param name="objectKey"></param>
        /// <returns></returns>
        public static string GetObjUriForObjectInfo(byte[] objectKey, 
                                                    GiopVersion version) {
            return GetObjUriForObjectKeyAndOptInfo(objectKey, version);
        }

        /// <summary>
        /// take a CORBA object Key and create a .NET obj-URI out of it
        /// </summary>
        /// <param name="objectKey"></param>
        /// <returns></returns>
        public static string GetObjUriForObjectKey(byte[] objectKey) {
            return GetObjUriForObjectKeyAndOptInfo(objectKey, null);
        }        

        #endregion SMethods

    }
}
