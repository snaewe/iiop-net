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

        #endregion SFields
        #region IConstructors
        
        private IiopUrlUtil() {
        }

        #endregion IConstructors
        #region SMethods
        
        /// <summary>
        /// extract port and hostname out of an channel uri
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        public static void ParseChanUri(string uri, out string hostname,
                                        out int port) {
            hostname = null;
            port = 0;
            if (uri == null) { return; }
            if (uri.StartsWith("iiop:")) {
                // parse URL
                uri = uri.Substring(7); // cut of iiop://
                if (uri.IndexOf(":") > 0) {
                    hostname = uri.Substring(0, uri.IndexOf(":"));
                    port = Convert.ToInt32(uri.Substring(uri.IndexOf(":")+1));
                } else {
                    hostname = uri;
                }
            }
        }
        
        /// <summary>checks if data is an URL for the IIOP-channel </summary>
        public static bool IsUrl(string data) {
            if ((data.StartsWith("iiop")) || (data.StartsWith("IOR"))) {
                return true;
            } else {
                return false;
            }
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
                string host;
                int port;
                GiopVersion version;
                string chanURI = IiopUrlUtil.ParseUrl(url, out objectURI);
                IiopUrlUtil.ParseChanUri(chanURI, out host, out port);
                byte[] objectKey = IiopUrlUtil.GetObjectInfoForObjUri(objectURI, out version);
                // now create an IOR with the above information
                InternetIiopProfile profile = new InternetIiopProfile(version, host,
                                                                     (ushort)port, objectKey);
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
        public static string ParseUrl(string url, out string objectUri) {
            string channelUri = null;
            objectUri = null;
            if (url == null) { 
                return null; 
            }

            if (url.StartsWith("iiop:")) {
                // parse URL
                url = url.Substring(7); // cut of iiop://
                if (url.IndexOf("/") >= 0) {
                    channelUri = "iiop://" + url.Substring(0, url.IndexOf("/"));
                    objectUri = url.Substring(url.IndexOf("/")+1);
                } else {
                    channelUri = url;
                }
            } else if (url.StartsWith("IOR:")) {
                // parse IOR
                Ior ior = new Ior(url);
                channelUri = "iiop://" + ior.HostName + ":" + ior.Port;
                objectUri = GetObjUriForObjectInfo(ior.ObjectKey, ior.Version);
            } else {
                // not possible
            }
            return channelUri;
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
                return IorStringifyUtil.Destringify(objectId);
            } else {
                byte[] result = StringUtil.GetByteArrRepForString(objectId);
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
        /// take a CORBA object Key and create a .NET obj-URI out of it
        /// </summary>
        /// <param name="objectKey"></param>
        /// <returns></returns>
        public static string GetObjUriForObjectInfo(byte[] objectKey, 
                                                    GiopVersion version) {
            if (CheckNonStringifyTag(objectKey)) {
                // an URI pointing to a native .NET remoting framework object, therefore the .NET URI must be
                // reproduced from which this objectKey was created
                byte[] objectId = new byte[objectKey.Length-6];
                Array.Copy((Array)objectKey, 0, (Array)objectId, 0, 
                           objectId.Length);
                return StringUtil.GetStringFromWideChar(objectId);
            } else {
                // stringify it
                return IorStringifyUtil.Stringify(objectKey) + "," + GIOP_ID + "=" + 
                                                  version.Major + "." + version.Minor + "," +
                                                  STRINGIFIED_ID + "=t"; 
            }
        }

        #endregion SMethods

    }
}
