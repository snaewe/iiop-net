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
using omg.org.IOP;

namespace Ch.Elca.Iiop.Util
{

    /// <summary>
    /// This class is able to handle urls for the IIOP-channel
    /// </summary>
    /// <remarks>
    /// This class is used to parse url's.
    /// This is a helper class for the IIOP-channel
    /// </remarks>
    public sealed class IiopUrlUtil
    {

        #region Constants

        #endregion Constants
        #region SFields

        private readonly static object[] s_emptyAdditionalTaggedComponents =
            new object[0];

        #endregion SFields
        #region IFields

        private object[] m_defaultAdditionalTaggedComponents;

        private Codec m_codec;

        #endregion IFields
        #region IConstructors

        /// <summary>
        /// Default constructor. To Create an instance,
        /// use the factory methods provided.
        /// </summary>
        private IiopUrlUtil(Codec codec)
        {
            m_codec = codec;
            m_defaultAdditionalTaggedComponents =
                s_emptyAdditionalTaggedComponents;
        }

        #endregion IConstructors
        #region IMethods

        /// <summary>
        /// This method instructs the IiopUrlUtil to add the given
        /// components, when creating an ior for a corbaloc or iiop url.
        /// </summary>
        private void SetDefaultComponents(object[] taggedComponents)
        {
            m_defaultAdditionalTaggedComponents = taggedComponents;
        }

        /// <summary>creates an IOR for the object described by the Url url</summary>
        /// <param name="url">an url of the form IOR:--hex-- or iiop://addr/key</param>
        /// <param name="targetType">if the url contains no info about the target type, use this type</param>
        public Ior CreateIorForUrl(string url, string repositoryId)
        {
            Ior ior = null;
            if (IsIorString(url))
            {
                ior = new Ior(url);
            }
            else if (url.StartsWith("iiop"))
            {
                // iiop1.0, iiop1.1, iiop1.2 (=iiop); extract version in protocol tag
                IiopLoc iiopLoc = new IiopLoc(url, m_codec,
                                              m_defaultAdditionalTaggedComponents);
                // now create an IOR with the above information
                ior = new Ior(repositoryId, iiopLoc.GetProfiles());
            }
            else if (url.StartsWith("corbaloc"))
            {
                Corbaloc loc = new Corbaloc(url, m_codec,
                                            m_defaultAdditionalTaggedComponents);
                IorProfile[] profiles = loc.GetProfiles();
                ior = new Ior(repositoryId, profiles);
            }
            else
            {
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
        internal Uri ParseUrl(string url, out string objectUri,
                              out GiopVersion version)
        {
            Uri uri = null;
            if (url.StartsWith("iiop"))
            {
                IiopLoc iiopLoc = new IiopLoc(url, m_codec,
                                              m_defaultAdditionalTaggedComponents);
                uri = iiopLoc.ParseUrl(out objectUri, out version);
            }
            else if (url.StartsWith("IOR"))
            {
                Ior ior = new Ior(url);
                IInternetIiopProfile profile = ior.FindInternetIiopProfile();
                if (profile != null)
                {
                    uri = new Uri("iiop" + profile.Version.Major + "." + profile.Version.Minor +
                              Uri.SchemeDelimiter + profile.HostName + ":" + profile.Port);
                    objectUri = IorUtil.GetObjectUriForObjectKey(profile.ObjectKey);
                    version = profile.Version;
                }
                else
                {
                    uri = null;
                    objectUri = null;
                    version = new GiopVersion(1, 0);
                }
            }
            else if (url.StartsWith("corbaloc"))
            {
                Corbaloc loc = new Corbaloc(url, m_codec,
                                            m_defaultAdditionalTaggedComponents);
                uri = loc.ParseUrl(out objectUri, out version);
            }
            else
            {
                // not possible
                uri = null;
                objectUri = null;
                version = new GiopVersion(1, 0);
            }
            return uri;
        }

        #endregion IMethods
        #region SMethods

        /// <summary>checks if data is an URL for the IIOP-channel </summary>
        public static bool IsUrl(string data)
        {
            return (data.StartsWith("iiop") || IsIorString(data) ||
                    data.StartsWith("corbaloc"));
        }

        public static bool IsIorString(string url)
        {
            return url.StartsWith("IOR");
        }

        /// <summary>
        /// creates an URL from host, port and objectURI
        /// </summary>
        internal static string GetUrl(string host, int port, string objectUri)
        {
            return "iiop" + Uri.SchemeDelimiter + host + ":" + port + "/" + objectUri;
        }

        /// <summary>
        /// Create an instance of IiopUrlUtil, which adds the given tagged components to
        /// created iors.
        /// </summary>
        public static IiopUrlUtil Create(Codec codec,
                                         object[] additionalComponents)
        {
            IiopUrlUtil result = new IiopUrlUtil(codec);
            result.SetDefaultComponents(additionalComponents);
            return result;
        }

        /// <summary>
        /// Create an instance of IiopUrlUtil, without adding any tagged components to
        /// created iors.
        /// </summary>        
        public static IiopUrlUtil Create(Codec codec)
        {
            return new IiopUrlUtil(codec);
        }

        #endregion SMethods

    }



}


#if UnitTest

namespace Ch.Elca.Iiop.Tests
{

    using System.IO;
    using NUnit.Framework;
    using Ch.Elca.Iiop;
    using Ch.Elca.Iiop.Idl;
    using Ch.Elca.Iiop.Util;
    using Ch.Elca.Iiop.Services;
    using Ch.Elca.Iiop.Marshalling;
    using Ch.Elca.Iiop.Interception;
    using omg.org.CORBA;


    /// <summary>
    /// Unit-tests for testing request/reply serialisation/deserialisation
    /// </summary>
    [TestFixture]
    public class IiopUrlUtilTest
    {

        private omg.org.IOP.Codec m_codec;
        private IiopUrlUtil m_iiopUrlUtil;
        private SerializerFactory m_serFactory;

        [SetUp]
        public void SetUp()
        {
            m_serFactory =
                new SerializerFactory();
            omg.org.IOP.CodecFactory codecFactory =
                new CodecFactoryImpl(m_serFactory);
            m_codec =
                codecFactory.create_codec(
                    new omg.org.IOP.Encoding(omg.org.IOP.ENCODING_CDR_ENCAPS.ConstVal, 1, 2));
            m_iiopUrlUtil =
                IiopUrlUtil.Create(m_codec, new object[] { 
                    Services.CodeSetService.CreateDefaultCodesetComponent(m_codec)});
            m_serFactory.Initalize(new SerializerFactoryConfig(), m_iiopUrlUtil);
        }

        private void CheckIorForUrl(Ior iorForUrl, int expectedNumberOfComponents,
                                    bool shouldHaveCodeSetComponent)
        {
            Assert.AreEqual(1, iorForUrl.Profiles.Length, "number of profiles");
            Assert.AreEqual(typeof(MarshalByRefObject),
                                   iorForUrl.Type, "type");
            IIorProfile profile = iorForUrl.FindInternetIiopProfile();
            Assert.NotNull(profile, "internet iiop profile");
            Assert.AreEqual(
                                                 new byte[] { 116, 101, 115, 116 },
                                                 profile.ObjectKey, "profile object key");
            Assert.AreEqual(new GiopVersion(1, 2), profile.Version, "profile giop version");

            if (shouldHaveCodeSetComponent)
            {
                Assert.AreEqual(
                                       expectedNumberOfComponents,
                                       profile.TaggedComponents.Count, "number of components");
                Assert.IsTrue(profile.ContainsTaggedComponent(
                                     CodeSetService.SERVICE_ID), "code set component present");
                CodeSetComponentData data = (CodeSetComponentData)
                    profile.TaggedComponents.GetComponentData(CodeSetService.SERVICE_ID,
                                                              m_codec,
                                                              CodeSetComponentData.TypeCode);
                Assert.AreEqual(
                                       (int)CharSet.LATIN1,
                                       data.NativeCharSet, "code set component: native char set");
                Assert.AreEqual(
                                       (int)WCharSet.UTF16,
                                       data.NativeWCharSet, "code set component: native char set");
            }
            else
            {
                Assert.IsTrue(
                                 !profile.ContainsTaggedComponent(
                                     CodeSetService.SERVICE_ID), "code set component present");
            }
        }

        [Test]
        public void CreateIorForCorbaLocUrlWithCodeSetComponent()
        {
            string testCorbaLoc = "corbaloc:iiop:1.2@elca.ch:1234/test";
            Ior iorForUrl =
                m_iiopUrlUtil.CreateIorForUrl(testCorbaLoc, String.Empty);
            CheckIorForUrl(iorForUrl, 1, true);
        }

        [Test]
        public void CreateIorForIiopLocUrlWithCodeSetComponent()
        {
            string testIiopLoc = "iiop1.2://localhost:1234/test";
            Ior iorForUrl =
                m_iiopUrlUtil.CreateIorForUrl(testIiopLoc, String.Empty);
            CheckIorForUrl(iorForUrl, 1, true);
        }


        [Test]
        public void CreateIorForIorUrl()
        {
            string testIorLoc =
                "IOR:000000000000000100000000000000010000000000000050000102000000000A6C6F63616C686F73740004D2000000047465737400000001000000010000002800000000000100010000000300010001000100200501000100010109000000020001010000010109";
            Ior iorForUrl =
                m_iiopUrlUtil.CreateIorForUrl(testIorLoc, String.Empty);
            CheckIorForUrl(iorForUrl, 1, true);
        }

    }

}

#endif

