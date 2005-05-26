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
using System.Threading;
using omg.org.CORBA;
using omg.org.PortableInterceptor;
using Ch.Elca.Iiop.CorbaObjRef;
using Ch.Elca.Iiop.Idl;
using Ch.Elca.Iiop.Interception;

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
                ior = new Ior(repositoryId, iiopLoc.GetProfiles());
            } else if (url.StartsWith("corbaloc")) {
                Corbaloc loc = new Corbaloc(url);
                IorProfile[] profiles = loc.GetProfiles();
                ior = new Ior(repositoryId, profiles);
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
                uri = iiopLoc.ParseUrl(out objectUri, out version);
            } else if (url.StartsWith("IOR")) {
                Ior ior = new Ior(url);
                IInternetIiopProfile profile = ior.FindInternetIiopProfile();
                if (profile != null) {
                    uri = new Uri("iiop" + profile.Version.Major + "." + profile.Version.Minor + 
                              Uri.SchemeDelimiter + profile.HostName+":"+profile.Port);
                    objectUri = GetObjectUriForObjectKey(profile.ObjectKey);
                    version = profile.Version;
                } else {
                    uri = null;
                    objectUri = null;
                    version = new GiopVersion(1,0);
                }                
            } else if (url.StartsWith("corbaloc")) {
                Corbaloc loc = new Corbaloc(url);
                uri = loc.ParseUrl(out objectUri, out version);
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
        /// checks if a uri is assigned by .NET remoting -> this will be marshalled as
        /// CORBA system-id; user-assigned id's will be marshalled as CORBA user-id's.
        /// </summary>
        private static bool IsSystemGeneratedId(string uri) {
            // checks if uri endswith _seqNr.rem, where seqNr is a base 10 number
            bool result = true;
            if (uri == null) {
                throw new INTERNAL(58, CompletionStatus.Completed_MayBe);
            } else {
                int endPartIndex = uri.LastIndexOf("_");
                if ((endPartIndex >= 0) && (uri.EndsWith(".rem"))) {
                    string lastPart = uri.Substring(endPartIndex + 1);
                    for (int i = 0; i < lastPart.Length - 4; i++) {
                        if (!Char.IsDigit(lastPart, i)) {
                            result = false;
                        }
                    }              
                } else {
                    result = false;
                }
            }
            return result;
        }

        /// <summary>
        /// takes a marshalled object and calculate the corba object key to send from it
        /// </summary>
        /// <param name="mbr">the object for which to get the object key</param>
        /// <returns></returns>
        internal static byte[] GetObjectKeyForObj(MarshalByRefObject mbr) {
            string objectUri = RemotingServices.GetObjectUri(mbr);
            if (objectUri == null) { 
                throw new INTERNAL(57, CompletionStatus.Completed_MayBe);
            }
            
            if (!IsSystemGeneratedId(objectUri)) {
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

        #region ServerSide
        
        /// <summary>
        /// creates an IOR for an object hosted in the local appdomain.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal static Ior CreateIorForObjectFromThisDomain(MarshalByRefObject obj) {
            ObjRef objRef = RemotingServices.Marshal(obj); // make sure, the object is marshalled and get obj-ref
            byte[] objectKey = IiopUrlUtil.GetObjectKeyForObj(obj);
            IiopChannelData serverData = GetIiopChannelData(objRef);
            if (serverData != null) {
                string host = serverData.HostName;
                int port = serverData.Port;
                if ((objectKey == null) || (host == null)) { 
                    // the objRef: " + refToTarget + ", uri: " +
                    // refToTarget.URI + is not serialisable, because connection data is missing 
                    // hostName=host, objectKey=objectKey
                    throw new INV_OBJREF(1961, CompletionStatus.Completed_MayBe);
                }
                string repositoryID = Repository.GetRepositoryID(obj.GetType());
                if (obj.GetType().Equals(ReflectionHelper.MarshalByRefObjectType)) {
                    repositoryID = "";
                }
                // this server support GIOP 1.2 --> create an GIOP 1.2 profile
                InternetIiopProfile profile = new InternetIiopProfile(new GiopVersion(1, 2), host,
                                                                      (short)port, objectKey);
                // add additional tagged components according to the channel options, e.g. for SSL
                profile.AddTaggedComponents(serverData.AdditionalTaggedComponents);
                // add additional tagged components according to registered interceptors:
                AddProfileComponentsFromIorInterceptors(profile);
                
                Ior ior = new Ior(repositoryID, new IorProfile[] { profile });
                return ior;                
            } else {
                Debug.WriteLine("ERROR: no server-channel information found!");
                Debug.WriteLine("Please make sure, that an IIOPChannel has been created with specifying a listen port number (0 for automatic)!");
                Debug.WriteLine("e.g. IIOPChannel chan = new IIOPChannel(0);");
                throw new INTERNAL(1960, CompletionStatus.Completed_MayBe);
            }
        }
        
        /// <summary>gets the IIOPchannel-data from an ObjRef.</summary>
        private static IiopChannelData GetIiopChannelData(ObjRef objRef) {
            IChannelInfo info = objRef.ChannelInfo;
            if ((info == null) || (info.ChannelData == null)) { 
                return null; 
            }
            
            foreach (object chanData in info.ChannelData) {
                if (chanData is IiopChannelData) {
                    Debug.WriteLine("chan-data for IIOP-channel found: " + chanData);
                    return (IiopChannelData)chanData; // the IIOP-channel data
                }
            }
            // no IIOPChannelData found
            return null; 
        }
        
        private static void AddProfileComponentsFromIorInterceptors(InternetIiopProfile profile) {
            IORInterceptor[] interceptors = OrbServices.GetSingleton().InterceptorManager.GetIorInterceptors();
            if (interceptors.Length > 0) {
                IORInfo info = new IORInfoImpl(profile);
                for (int i = 0; i < interceptors.Length; i++) {
                    try {                    
                        interceptors[i].establish_components(info);
                    } catch (ThreadAbortException) {
                        throw;
                    } catch (Exception e) {
                        // ignore exceptions
                        Trace.WriteLine("warning: ior interceptor thrown exception: " + e);
                    }
                }
            }
        }
        
        #endregion ServerSide

        #endregion SMethods

    }
        
 
     
}
