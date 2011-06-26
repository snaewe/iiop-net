/* IORUtil.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 25.05.06  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
 * 
 * Copyright 2006 Dominic Ullmann
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
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using Ch.Elca.Iiop.CorbaObjRef;
using Ch.Elca.Iiop.Idl;
using Ch.Elca.Iiop.Interception;
using omg.org.CORBA;
using omg.org.PortableInterceptor;

namespace Ch.Elca.Iiop.Util {

    /// <summary>
    /// Class, containing methods for working with IOR and
    /// object keys.
    /// </summary>
    internal static class IorUtil {
         
        #region SMethods
         
         
        /// <summary>
        /// get the key-bytes for the id; doesn't any more transformations
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal static byte[] GetKeyBytesForId(string id) {
            return Encoding.ASCII.GetBytes(id);
        }

        internal static byte[] GetKeyBytesForId(string id, int startIndex) {
            return Encoding.ASCII.GetBytes(id.ToCharArray(startIndex, id.Length - startIndex));
        }
        
        internal static string GetObjectUriForObjectKey(byte[] objectKey) {
            string result = Encoding.ASCII.GetString(objectKey);
            return UnescapeNonAscii(result);
        }
         
        /// <summary>
        /// escape characters, which are not part of the ASCII set
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        internal static string EscapeNonAscii(string uri, ref int startIndex) {
            StringBuilder result = null;
            bool inEscapeMode = false;
            for (int i = startIndex; i != uri.Length; ++i) {
                if (uri[i] == '\\') {
                    inEscapeMode = true;
                }
                else if (inEscapeMode && uri[i] == 'u') {
                    inEscapeMode = false;
                    if (result == null) {
                        result = new StringBuilder(uri, startIndex, i, uri.Length - startIndex);
                    }
                    // escape \u in \\u
                    result.Append('\\');
                }
                else {
                    inEscapeMode = false;
                    if (uri[i] > 0x7f) {
                        if (result == null) {
                            result = new StringBuilder(uri, startIndex, i, uri.Length - startIndex);
                        }
                        // replace non-ascii char by \u****
                        result.Append(@"\u").Append(Convert.ToInt32(uri[i]).ToString("X4"));
                        continue;
                    }
                }
                if (result != null) {
                    result.Append(uri[i]);
                }
            }
            if (result != null) {
                startIndex = 0;
                return result.ToString();
            }
            // Nothing to escape
            return uri;
        }

        /// <summary>
        /// reverse the effect of EscapeNonAscii
        /// </summary>
        internal static string UnescapeNonAscii(string uri) {
            StringBuilder result = null;
            int escapeSequenceStartIndex = 0;
            for (int i = 0; i != uri.Length; ++i) {
                bool endOfSequence;
                if (IsPotentiallyEscapedCharacterRepresentation1(uri, i, i - escapeSequenceStartIndex, out endOfSequence)) {
                    // either new escape sequence starting with \ or continue of a sequence
                    if (endOfSequence) {
                        // it's an escape char in form \uQRST
                        if (result == null) {
                            result = new StringBuilder(uri, 0, escapeSequenceStartIndex, uri.Length);
                        }
                        int charNr = StringConversions.Parse(uri, escapeSequenceStartIndex + 2, 4);
                        result.Append(Convert.ToChar(charNr));
                        escapeSequenceStartIndex = i;
                    }
                    else {
                        continue;
                    }
                }
                else if (IsPotentiallyEscapedCharacterRepresentation2(uri, i, i - escapeSequenceStartIndex, out endOfSequence)) {
                    if (endOfSequence) {
                        // it's an escape char in form \\u
                        if (result == null) {
                            result = new StringBuilder(uri, 0, escapeSequenceStartIndex, uri.Length);
                        }
                        result.Append(@"\u");
                        escapeSequenceStartIndex = i;
                    }
                    else {
                        continue;
                    }
                }
                else {
                    // no escape sequence, add string directly to result
                    if (result != null) {
                        result.Append(uri, escapeSequenceStartIndex, i - escapeSequenceStartIndex + 1);
                    }
                    escapeSequenceStartIndex = i;
                }
                ++escapeSequenceStartIndex;
            }
            return result != null ? result.ToString() : uri;
        }

        /// <summary>
        /// checks, if a given candidate sequence may represent an escaped character
        /// </summary>
        /// <returns>true, if possible, otherwise false</returns>
        private static bool IsPotentiallyEscapedCharacterRepresentation1(string candidate, int index, int escapedCharRepIndex,
                                                                         out bool lastChar) {
            // look for '\uQRST'
            lastChar = escapedCharRepIndex == 5;
            switch (escapedCharRepIndex)
            {
                case 0:
                    return candidate[index] == '\\';
                case 1:
                    return candidate[index] == 'u';
                case 2:
                case 3:
                case 4:
                case 5:
                    return Char.IsDigit(candidate, index) ||
                           (candidate[index] >= 'A' && candidate[index] <= 'F') ||
                           (candidate[index] >= 'a' && candidate[index] <= 'f');
                default:
                    return false;
            }
        }

        private static bool IsPotentiallyEscapedCharacterRepresentation2(string candidate, int index, int escapedCharRepIndex,
                                                                         out bool lastChar) {
            lastChar = false;
            // look for \\u
            switch (escapedCharRepIndex)
            {
                case 0:
                case 1:
                    return candidate[index] == '\\';
                case 2:
                    lastChar = true;
                    return candidate[index] == 'u';
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// checks if a uri is assigned by .NET remoting -> this will be marshalled as
        /// CORBA system-id; user-assigned id's will be marshalled as CORBA user-id's.
        /// </summary>
        private static bool IsSystemGeneratedId(string uri) {
            // checks if uri endswith _seqNr.rem, where seqNr is a base 10 number
            if (uri == null) {
                throw new INTERNAL(58, CompletionStatus.Completed_MayBe);
            }

            int endPartIndex = uri.LastIndexOf("_");
            if ((endPartIndex >= 0) && (uri.EndsWith(".rem"))) {
                for (int i = endPartIndex + 1; i < uri.Length - 4; ++i) {
                    if (!Char.IsDigit(uri, i)) {
                        return false;
                    }
                }
            } else {
                return false;
            }
            return true;
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

            return GetObjectKeyForUri(objectUri);
        }

        private static byte[] GetObjectKeyForUri(string objectUri) {
            int startIndex = 0;
            if (!IsSystemGeneratedId(objectUri)) {
                // remove appdomain-id in front of uri which is automatically appended
                // (see comment for RemotingServices.SetObjectUriForMarshal);
                // to support user-id policy, this appdomain-guid must be removed!
                if (objectUri.StartsWith("/")) {
                    startIndex = 1;
                }
                int appdomainGuidEndIndex = objectUri.IndexOf('/', startIndex);
                if (appdomainGuidEndIndex >= 0) {
                    // remove appdomain-guid part
                    startIndex = appdomainGuidEndIndex + 1;
                } else {
                    Debug.WriteLine("warning, appdomain guid not found in object-uri: " +
                                    objectUri);
                }
            }
            // For System-id policy, don't remove the appdomain id, because after
            // a restart of the application, appdomain id-guid prevents the
            // generation of the same ids

            // use ASCII-encoder + unicode escaped to encode uri string
            objectUri = EscapeNonAscii(objectUri, ref startIndex);
            return Encoding.ASCII.GetBytes(objectUri.ToCharArray(startIndex, objectUri.Length - startIndex));
        }
        
        /// <summary>
        /// creates an IOR for an object hosted in the local appdomain.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal static Ior CreateIorForObjectFromThisDomain(MarshalByRefObject obj) {
            return CreateIorForObjectFromThisDomain(obj, obj.GetType(), false);
        }

        /// <summary>
        /// creates an IOR for an object hosted in the local appdomain.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal static Ior CreateIorForObjectFromThisDomain(MarshalByRefObject obj, Type forType, bool marshalUsingForType) {
            Console.WriteLine("Marshalling using for type: {0}", marshalUsingForType);
            ObjRef objRef = 
                marshalUsingForType ? RemotingServices.Marshal(obj, null, forType)
                                    : RemotingServices.Marshal(obj); // make sure, the object is marshalled and get obj-ref
            byte[] objectKey = GetObjectKeyForUri(objRef.URI);
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
                string repositoryID = forType == ReflectionHelper.MarshalByRefObjectType
                    ? "" // CORBA::Object has "" repository id
                    : Repository.GetRepositoryID(forType);
                // this server support GIOP 1.2 --> create an GIOP 1.2 profile
                InternetIiopProfile profile = new InternetIiopProfile(new GiopVersion(1, 2), host,
                                                                      (ushort)port, objectKey);
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
        #endregion SMethods
    }
}

#if UnitTest

namespace Ch.Elca.Iiop.Tests
{
    using NUnit.Framework;
    using Ch.Elca.Iiop.Util;

    [TestFixture]
    public class IORUtilTest
    {
        [Test]
        public void EscapeNonAsciiTest()
        {
            int startIndex = 0;
            string result = IorUtil.EscapeNonAscii("foo", ref startIndex);
            Assert.AreEqual(0, startIndex);
            Assert.AreEqual("foo", result);
            Assert.AreEqual(@"fran\u00E7ois", IorUtil.EscapeNonAscii("françois", ref startIndex));
            Assert.AreEqual("françois", IorUtil.UnescapeNonAscii(@"fran\u00E7ois"));
            Assert.AreEqual(@"fran\u00E7\\u00B9ois", IorUtil.EscapeNonAscii(@"franç\u00B9ois", ref startIndex));
            Assert.AreEqual(@"franç\u00B9ois", IorUtil.UnescapeNonAscii(@"fran\u00E7\\u00B9ois"));
        }
    }
}

#endif
