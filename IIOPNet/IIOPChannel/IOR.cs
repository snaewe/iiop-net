/* IOR.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 15.01.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Text;

using Ch.Elca.Iiop.Cdr;
using Ch.Elca.Iiop.Util;
using Ch.Elca.Iiop.Idl;
using Ch.Elca.Iiop.Marshalling;
using omg.org.CORBA;
using omg.org.IOP;

namespace Ch.Elca.Iiop.CorbaObjRef {
   

    /// <summary>
    /// This class represents a Corba IOR.
    /// </summary>
    public class Ior {

        #region IFields
        
        private IorProfile[] m_profiles;
        private string m_typId;
        
        #endregion IFields
        #region IConstructors

        /// <summary>
        /// creates an IOR from the IOR stringified form
        /// </summary>
        public Ior(string iorAsString) {
            // iorAsString contains only characters 0-9, A-F and IOR --> all of this are short characters
            if (iorAsString.StartsWith("IOR:")) {
                string tmp = iorAsString.Substring(4);
                MemoryStream memStream = new MemoryStream(StringConversions.Destringify(tmp));
                CdrInputStreamImpl cdrStream = new CdrInputStreamImpl(memStream);
                byte flags = cdrStream.ReadOctet();
                cdrStream.ConfigStream(flags, new GiopVersion(1,2)); // giop dep operation are not used for IORs
                ParseIOR(cdrStream);
            } else {
                throw new INV_OBJREF(9420, CompletionStatus.Completed_No);
            }
        }

        /// <summary>
        /// parses an IOR embedded in a GIOP message
        /// </summary>
        internal Ior(CdrInputStream cdrStream) {
            ParseIOR(cdrStream);
        }

        /// <summary>
        /// creates an IOR from the typeName and the profiles
        /// </summary>        
        internal Ior(string typeName, IorProfile[] profiles) {
            if (profiles == null) { 
                profiles = new IorProfile[0]; 
            }
            m_profiles = profiles;
            m_typId = typeName;            
        }

        #endregion IConstructors
        #region IProperties
        
        /// <summary>
        /// all profiles inside this ior.
        /// </summary>
        public IIorProfile[] Profiles {
            get { 
                return m_profiles; 
            }
        }
        
        /// <summary>the TypeID of this IOR</summary>
        public string TypID {
            get { 
                return m_typId; 
            }
        }
        
        /// <summary>the type represented by typeid</summary>
        public Type Type {
            get {
                return Repository.GetTypeForId(m_typId);
            }
        }

        #endregion IProperties
        #region IMethods
        
        /// <summary>returns true, if this IOR represents a null reference</summary>
        /// <returns>true, if a IOR represents null reference, otherwise false</returns>
        public bool IsNullReference() {
            return (m_typId.Equals("") && (m_profiles.Length == 0));
        }

        private void ParseIOR(CdrInputStream cdrStream) {
            m_typId = cdrStream.ReadString();
            ulong nrOfProfiles = cdrStream.ReadULong();
            m_profiles = new IorProfile[nrOfProfiles];
            for (ulong i = 0; i < nrOfProfiles; i++) {
                m_profiles[i] = ParseProfile(cdrStream);
            }            
        }

        /// <summary>
        /// parses an IOR-profile embedded in a CDRStream
        /// </summary>
        /// <param name="cdrStream"></param>
        /// <returns></returns>
        private IorProfile ParseProfile(CdrInputStream cdrStream) {
            int profileType = (int)cdrStream.ReadULong();             
            switch (profileType) {
                case 0: 
                    IorProfile result = new InternetIiopProfile(cdrStream);                    
                    return result;
                case 1:                    
                    return new MultipleComponentsProfile(cdrStream);
                default: 
                    // unsupported profile type
                    return new UnsupportedIorProfile(cdrStream, profileType);
            }
        }

        /// <summary>gets a stringified representation</summary>
        public override string ToString() {
            // encode the IOR to a CDR-strem, afterwards write it to the iorStream
            MemoryStream content = new MemoryStream();
            byte flags = 0;
            CdrOutputStream stream = new CdrOutputStreamImpl(content, flags);
            stream.WriteOctet(flags); // writing the flags before the IOR
            WriteToStream(stream);

            // write content to the IORStream
            content.Close();
            string result = "IOR:" + StringConversions.Stringify(content.ToArray());

            return result;
        }

        /// <summary>
        /// write this IOR to a CDR-Stream in non-strignified form
        /// </summary>
        internal void WriteToStream(CdrOutputStream cdrStream) {
            cdrStream.WriteString(m_typId);
            cdrStream.WriteULong((uint)m_profiles.Length); // nr of profiles
            for (int i = 0; i < m_profiles.Length; i++) {
                m_profiles[i].WriteToStream(cdrStream);
            }
        }
        
        /// <summary>
        /// tries to find an internet-iop profile inside this ior.
        /// </summary>
        public IInternetIiopProfile FindInternetIiopProfile() {
            for (int i = 0; i < Profiles.Length; i++) {
                if (Profiles[i].ProfileId == TAG_INTERNET_IOP.ConstVal) {
                    return (IInternetIiopProfile)Profiles[i];
                }
            }
            return null;
        }

        #endregion IMethods

    }

    
    /// <summary>
    /// the interface of all ior profiles.
    /// </summary>
    public interface IIorProfile {
        
        #region IProperties

        /// <summary>
        /// the id of the profile
        /// </summary>
        int ProfileId {
            get;
        }

        /// <summary>the list of tagged components</summary>
        TaggedComponentList TaggedComponents {
            get;
        }
        
        /// <summary>
        /// the giop version usable when connecting with this profile.
        /// </summary>
        GiopVersion Version {
            get;
        }
                        
        /// <summary>
        /// the key of the target object.
        /// </summary>
        byte[] ObjectKey {
            get;
        }
        
        #endregion IProperties
        #region IMethods
        
        /// <summary>
        /// creates a tagged profile from this profile.
        /// </summary>
        TaggedProfile CreateTaggedProfile();
        
        /// <summary>
        /// deserialise the component data for the contained component with the specified id; if component
        /// is not present, returns null.
        /// </summary>
        object GetTaggedComponentData(int tag, Type componentType);
                
        /// <summary>
        /// returns true, if at least one tagged component with the given tag is present.
        /// </summary>
        bool ContainsTaggedComponent(int tag);
        
        /// <summary>
        /// returns one tagged component with the given tag, if present. Otherwise throws exception.
        /// </summary>
        TaggedComponent GetTaggedComponent(int tag);
        
        /// <summary>
        /// returns all tagged components with the given tag. If not present, returns an empty array.
        /// </summary>
        TaggedComponent[] GetTaggedComponents(int tag);
        
        #endregion IMethods
        
    }
    
    /// <summary>
    /// interface for internet iiop profile.
    /// </summary>
    public interface IInternetIiopProfile : IIorProfile {
        
        string HostName {
            get;
        }
        
        /// <summary>
        /// the port, the server is listening on.
        /// </summary>
        int Port {
            get;
        }
        
        
    }
    

    /// <summary>This class represents a profile in a CORBA-IOR</summary>
    /// <remarks>This class is non CLS compliant</remarks>    
    internal abstract class IorProfile : IIorProfile {
        
        #region SFields
        
        private readonly static object s_defaultCodeSetTaggedComponent = 
            TaggedComponent.CreateTaggedComponent(TAG_CODE_SETS.ConstVal, 
                                                  new Services.CodeSetComponentData(Services.CodeSetService.DEFAULT_CHAR_SET,
                                                                           new int[] { Services.CodeSetService.ISO646IEC_SINGLE },
                                                                           Services.CodeSetService.DEFAULT_WCHAR_SET,
                                                                           new int[] { Services.CodeSetService.ISO646IEC_MULTI }));
        
        #endregion SFields
        #region IFields

        protected GiopVersion m_giopVersion;

        protected byte[] m_objectKey;
        
        /// <summary>the tagged components in this profile</summary>
        protected TaggedComponentList m_taggedComponents = new TaggedComponentList();

        #endregion IFields
        #region IConstructors

        /// <summary>
        /// creates an IOR from the data in a stream
        /// </summary>
        /// <param name="stream">the stream containing the profile data</param>
        protected IorProfile(CdrInputStream stream) {
            ReadDataFromStream(stream);
        }

        /// <summary>
        /// creates a profile from the standard data needed
        /// </summary>
        public IorProfile(GiopVersion version, byte[] objectKey) {
            m_giopVersion = version;
            m_objectKey = objectKey;
        }

        #endregion IConstructors
        #region IProperties

        public GiopVersion Version {
            get { 
                return m_giopVersion; 
            }
        }
       
        public byte[] ObjectKey {
            get {
                return m_objectKey; 
            }
        }
        
        /// <summary>the tagged components in this profile</summary>
        public TaggedComponentList TaggedComponents {
            get { 
                return m_taggedComponents; 
            }
        }

        /// <summary>
        /// the id of the profile
        /// </summary>
        public abstract int ProfileId {
            get;
        }

        #endregion IProperties
        #region IMethods  
        
        public void AddTaggedComponent(TaggedComponent component) {
            m_taggedComponents.AddComponent(component);
        }
        
        public void AddTaggedComponents(TaggedComponent[] components) {
            m_taggedComponents.AddComponents(components);
        }        
        
        public void AddTaggedComponentWithData(int tag, object componentData) {
            m_taggedComponents.AddComponentWithData(tag, componentData);
        }
        
        public object GetTaggedComponentData(int tag, Type componentType) {
            return m_taggedComponents.GetComponentData(tag, componentType);
        }
                
        public bool ContainsTaggedComponent(int tag) {
            return m_taggedComponents.ContainsTaggedComponent(tag);
        }
        
        public TaggedComponent GetTaggedComponent(int tag) {
            return m_taggedComponents.GetComponent(tag);
        }
        
        public TaggedComponent[] GetTaggedComponents(int tag) {
            return m_taggedComponents.GetComponents(tag);
        }
        
        /// <summary>writes this profile into an encapsulation</summary>
        public abstract void WriteToStream(CdrOutputStream cdrStream);

        /// <summary>reads this profile data from a stream</summary>
        protected abstract void ReadDataFromStream(CdrInputStream cdrStream);                
        
        /// <summary>creates a tagged profile representation for this profile.</summary>
        public abstract TaggedProfile CreateTaggedProfile();
        
        #endregion IMethods
        #region SMethods
        
        /// <summary>
        /// returns the codeset tagged component, which should be added to the profiles by default.
        /// </summary>        
        protected static TaggedComponent GetDefaultCodeSetTaggedComponent() {
            return (TaggedComponent)s_defaultCodeSetTaggedComponent;
        }
        
        #endregion SMethods
    
    }

    /// <summary>
    /// the profile for IIOP connections
    /// </summary>
    /// <remarks>This class is not CLSCompliant</remarks>
    internal sealed class InternetIiopProfile : IorProfile, IInternetIiopProfile {
        
        #region IFields
        
        private string m_hostName;
        private short m_port;
        
        #endregion IFields
        #region IConstructors

        public InternetIiopProfile(GiopVersion version, string hostName, short port, byte[] objectKey) : base(version, objectKey) {
            m_hostName = hostName;
            m_port = port;
            // default codesetComponent
            TaggedComponents.AddComponent(IorProfile.GetDefaultCodeSetTaggedComponent());
        }

        /// <summary>
        /// reads an InternetIIOPProfile from a cdr stream
        /// </summary>
        /// <param name="encapsulation"></param>
        public InternetIiopProfile(CdrInputStream dataStream) : base(dataStream) {
            
        }

        #endregion IConstructors
        #region IProperties

        /// <summary>returns the profile-id for this profile</summary>
        public override int ProfileId {
            get { 
                return TAG_INTERNET_IOP.ConstVal; 
            }
        }
        
        public string HostName {
            get { 
                return m_hostName; 
            }
        }

        public int Port {
            get { 
                return (ushort)m_port; // m_port is mapped from an unsigned short -> cast back to ushort, before return
            }
        }        

        #endregion IProperties
        #region IMethods

        protected override void ReadDataFromStream(CdrInputStream inputStream) {
            // internet-iiop profile is encapsulated
            CdrEncapsulationInputStream encapsulation = inputStream.ReadEncapsulation();
            Debug.WriteLine("parse Internet IIOP Profile");
            byte giopMajor = encapsulation.ReadOctet();
            byte giopMinor = encapsulation.ReadOctet();
            m_giopVersion = new GiopVersion(giopMajor, giopMinor);

            Debug.WriteLine("giop-verion: " + m_giopVersion);
            m_hostName = encapsulation.ReadString();
            Debug.WriteLine("hostname: " + m_hostName);
            m_port = (short)encapsulation.ReadUShort();
            Debug.WriteLine("port: " + m_port);
            uint objectKeyLength = encapsulation.ReadULong();
            m_objectKey = new byte[objectKeyLength];
            Debug.WriteLine("object key follows");
            for (uint i = 0; i < objectKeyLength; i++) {
                m_objectKey[i] = encapsulation.ReadOctet();
                Debug.Write(m_objectKey[i] + " ");
            }
            Debug.WriteLine("");
            // GIOP 1.1, 1.2:
            if (!(m_giopVersion.Major == 1 && m_giopVersion.Minor == 0)) {
                m_taggedComponents = new TaggedComponentList(encapsulation);
            }
            
            Debug.WriteLine("parsing Internet-IIOP-profile completed");
        }

        /// <summary>
        /// writes this profile to the cdrStream
        /// </summary>
        /// <param name="cdrStream"></param>
        /// <remarks>
        public override void WriteToStream(CdrOutputStream cdrStream) {
            // write the profile id of this profile
            cdrStream.WriteULong((uint)ProfileId);
            CdrEncapsulationOutputStream encapStream = GetProfileContentStream();
            // write the whole encapsulation to the stream
            cdrStream.WriteEncapsulation(encapStream);
        }
        
        private CdrEncapsulationOutputStream GetProfileContentStream() {
            byte flags = 0;
            CdrEncapsulationOutputStream encapStream = new CdrEncapsulationOutputStream(flags);
            encapStream.WriteOctet(m_giopVersion.Major);
            encapStream.WriteOctet(m_giopVersion.Minor);
            encapStream.WriteString(m_hostName);
            encapStream.WriteUShort((ushort)m_port);
            encapStream.WriteULong((uint)m_objectKey.Length);
            encapStream.WriteOpaque(m_objectKey);
            // the tagged components            
            if (!(m_giopVersion.Major == 1 && m_giopVersion.Minor == 0)) { // for GIOP >= 1.1, tagged components are possible
                m_taggedComponents.WriteTaggedComponentList(encapStream);
            }            
            return encapStream;
        }
        
        public override TaggedProfile CreateTaggedProfile() {
            TaggedProfile result = new TaggedProfile();
            result.tag = ProfileId;
            result.profile_data = GetProfileContentStream().GetEncapsulationData();
            return result;
        }        

        #endregion IMethods

    }

    /// <summary>
    /// The multiple component profile.    
    /// </summary>
    /// <remarks>
    /// This class is not CLSCompliant.
    /// </remarks>
    internal sealed class MultipleComponentsProfile : IorProfile {
    
        #region IConstructors

        public MultipleComponentsProfile() : base(new GiopVersion(1,2), null) {
            // default codesetComponent
            TaggedComponents.AddComponent(IorProfile.GetDefaultCodeSetTaggedComponent());        
        }

        /// <summary>
        /// reads a multiple component profile data from a stream
        /// </summary>
        /// <param name="encapsulation"></param>
        public MultipleComponentsProfile(CdrInputStream inputStream) : base(inputStream) {            
        }

        #endregion IConstructors
        #region IProperties

        /// <summary>returns the profile-id for this profile</summary>
        public override int ProfileId {
            get { 
                return TAG_MULTIPLE_COMPONENTS.ConstVal; 
            }
        }

        #endregion IProperties
        #region IMethods

        protected override void ReadDataFromStream(CdrInputStream inputStream) {
            // internet-iiop profile is encapsulated
            CdrEncapsulationInputStream encapsulation = inputStream.ReadEncapsulation();

            Debug.WriteLine("parse Multiple component Profile");
            m_taggedComponents = new TaggedComponentList(encapsulation);
            
            Debug.WriteLine("parsing multiple components profile completed");
        }

        /// <summary>
        /// writes this profile to the cdrStream
        /// </summary>
        /// <param name="cdrStream"></param>
        /// <remarks>
        public override void WriteToStream(CdrOutputStream cdrStream) {
            // write the profile id of this profile
            cdrStream.WriteULong((uint)ProfileId);
            CdrEncapsulationOutputStream encapStream = GetProfileContentStream();
            // write the whole encapsulation to the stream
            cdrStream.WriteEncapsulation(encapStream);
        }
        
        private CdrEncapsulationOutputStream GetProfileContentStream() {
            byte flags = 0;
            CdrEncapsulationOutputStream encapStream = new CdrEncapsulationOutputStream(flags);
            // the tagged components
            m_taggedComponents.WriteTaggedComponentList(encapStream);
            return encapStream;            
        }
        
        public override TaggedProfile CreateTaggedProfile() {
            TaggedProfile result = new TaggedProfile();
            result.tag = ProfileId;
            result.profile_data = GetProfileContentStream().GetEncapsulationData();
            return result;
        }

        #endregion IMethods
    
    }
    
    /// <summary>
    /// used to store information about an unsupported profile
    /// </summary>
    /// <remkars>This class is not CLSCompliant</remarks>
    internal sealed class UnsupportedIorProfile : IorProfile {
        
        #region IFields
                
        // that's equal to a omg.org.IOP.TaggedProfile
        private int m_profileId;
        private byte[] m_data;
               
        #endregion IFields
        #region IConstructors
    
        /// <summary>
        /// reads an unsupported profile from an encapsulation (created with the method readEncapsulation in
        /// CDRStream) 
        /// </summary>
        /// <param name="encapsulation"></param>
        public UnsupportedIorProfile(CdrInputStream inputStream, int profileId) : base(inputStream) {                
            m_profileId = profileId;
        }
        
        #endregion IConstructors        
        #region IProperties

        /// <summary>returns the profile-id for this profile</summary>
        public override int ProfileId {
            get { 
                return m_profileId; 
            }
        }

        #endregion IProperties
        #region IMethods

        protected override void ReadDataFromStream(CdrInputStream inputStream) {
            Debug.WriteLine("parse unsupported ior profile");
            uint length = inputStream.ReadULong();
            m_data = inputStream.ReadOpaque((int)length);
                        
            Debug.WriteLine("parsing unsupported profile completed");
        }

        /// <summary>
        /// writes this profile to the cdrStream
        /// </summary>
        /// <param name="cdrStream"></param>
        /// <remarks>
        public override void WriteToStream(CdrOutputStream cdrStream) {
            // write the profile id of this profile
            cdrStream.WriteULong((uint)ProfileId);
            uint length = (uint)m_data.Length;
            cdrStream.WriteULong(length);
            cdrStream.WriteOpaque(m_data);
        }
        
        public override TaggedProfile CreateTaggedProfile() {
            TaggedProfile result = new TaggedProfile();
            result.tag = ProfileId;
            result.profile_data = m_data;
            return result;
        }

        #endregion IMethods
    
    }    

}

#if UnitTest

namespace Ch.Elca.Iiop.Tests {
    
    using NUnit.Framework;
    using Ch.Elca.Iiop.CorbaObjRef;
    
    /// <summary>
    /// Unit-test for class Ior
    /// </summary>
    public class IorTest : TestCase {
        
        public IorTest() {
        }

        public void TestIorCreation() {
            string iorString = "IOR:0000000000000024524d493a48656c6c6f496e746572666163653a3030303030303030303030303030303000000000010000000000000050000102000000000c31302e34302e32302e3531001f9500000000000853617948656C6C6F0000000100000001000000200000000000010001000000020501000100010020000101090000000100010100";
            Ior ior = new Ior(iorString);
            Assertion.Assert("nr of profiles", ior.Profiles.Length > 0);
            Assertion.AssertEquals("first profile type", TAG_INTERNET_IOP.ConstVal, ior.Profiles[0].ProfileId);
            IInternetIiopProfile iiopProf = (IInternetIiopProfile)ior.Profiles[0];
            Assertion.AssertEquals("10.40.20.51", iiopProf.HostName);
            Assertion.AssertEquals(8085, iiopProf.Port);
            Assertion.AssertEquals(1, iiopProf.Version.Major);
            Assertion.AssertEquals(2, iiopProf.Version.Minor);
            byte[] oid = new byte[] { 0x53, 0x61, 0x79, 0x48, 0x65, 0x6C, 0x6C, 0x6F };
            CheckIorKey(oid, iiopProf.ObjectKey);
            
            
            iorString = "IOR:0000000000000024524d493a48656c6c6f496e746572666163653a3030303030303030303030303030303000000000010000000000000050000102000000000c31302e34302e32302e3531007f9500000000000853617948656C6C6F0000000100000001000000200000000000010001000000020501000100010020000101090000000100010100";
            ior = new Ior(iorString);
            Assertion.Assert("nr of profiles", ior.Profiles.Length > 0);
            Assertion.AssertEquals("first profile type", TAG_INTERNET_IOP.ConstVal, ior.Profiles[0].ProfileId);
            iiopProf = (IInternetIiopProfile)ior.Profiles[0];
            Assertion.AssertEquals("10.40.20.51", iiopProf.HostName);
            Assertion.AssertEquals(32661, iiopProf.Port);
            Assertion.AssertEquals(1, iiopProf.Version.Major);
            Assertion.AssertEquals(2, iiopProf.Version.Minor);
            oid = new byte[] { 0x53, 0x61, 0x79, 0x48, 0x65, 0x6C, 0x6C, 0x6F };
            CheckIorKey(oid, iiopProf.ObjectKey);
            
            iorString = "IOR:0000000000000024524d493a48656c6c6f496e746572666163653a3030303030303030303030303030303000000000010000000000000050000102000000000c31302e34302e32302e3531008f9500000000000853617948656C6C6F0000000100000001000000200000000000010001000000020501000100010020000101090000000100010100";
            ior = new Ior(iorString);
            Assertion.Assert("nr of profiles", ior.Profiles.Length > 0);
            Assertion.AssertEquals("first profile type", TAG_INTERNET_IOP.ConstVal, ior.Profiles[0].ProfileId);
            iiopProf = (IInternetIiopProfile)ior.Profiles[0];
            Assertion.AssertEquals("10.40.20.51", iiopProf.HostName);
            Assertion.AssertEquals(36757, iiopProf.Port);
            Assertion.AssertEquals(1, iiopProf.Version.Major);
            Assertion.AssertEquals(2, iiopProf.Version.Minor);
            oid = new byte[] { 0x53, 0x61, 0x79, 0x48, 0x65, 0x6C, 0x6C, 0x6F };
            CheckIorKey(oid, iiopProf.ObjectKey);            
            
            iorString = "IOR:0000000000000024524d493a48656c6c6f496e746572666163653a3030303030303030303030303030303000000000010000000000000050000102000000000c31302e34302e32302e353100ffff00000000000853617948656C6C6F0000000100000001000000200000000000010001000000020501000100010020000101090000000100010100";
            ior = new Ior(iorString);
            Assertion.Assert("nr of profiles", ior.Profiles.Length > 0);
            Assertion.AssertEquals("first profile type", TAG_INTERNET_IOP.ConstVal, ior.Profiles[0].ProfileId);
            iiopProf = (IInternetIiopProfile)ior.Profiles[0];
            Assertion.AssertEquals("10.40.20.51", iiopProf.HostName);
            Assertion.AssertEquals(65535, iiopProf.Port);
            Assertion.AssertEquals(1, iiopProf.Version.Major);
            Assertion.AssertEquals(2, iiopProf.Version.Minor);
            oid = new byte[] { 0x53, 0x61, 0x79, 0x48, 0x65, 0x6C, 0x6C, 0x6F };
            CheckIorKey(oid, iiopProf.ObjectKey);                        
        }
        
        private void CheckIorKey(byte[] expected, byte[] actual) {
            Assertion.AssertEquals("wrong id length", expected.Length, actual.Length);
            for (int i = 0; i <expected.Length; i++) {
                Assertion.AssertEquals("wrong element nr " + i, expected[i], actual[i]);
            }
        }
        
        public void TestNonUsableProfileIncluded() {
            string iorString = "IOR:000000000000001b49444c3a636d6956322f5573657241636365737356323a312e3000020000000210ca1000000000650000000800000008646576312d73660033de6f8e0000004d000000020000000855736572504f41000000001043415355736572416363657373563200c3fbedfb0000000e007c4c51000000fd57aacdaf801a0000000e007c4c51000000fd57aacdaf80120000009400000000000000980001023100000008646576312d736600200b00020000004d000000020000000855736572504f41000000001043415355736572416363657373563200c3fbedfb0000000e007c4c51000000fd57aacdaf801a0000000e007c4c51000000fd57aacdaf8012000000140000000200000002000000140000000400000001000000230000000400000001000000000000000800000000cb0e0001";            
            Ior ior = new Ior(iorString);
            Assertion.AssertEquals("wrong RepositoryId", "IDL:cmiV2/UserAccessV2:1.0", ior.TypID);
            IInternetIiopProfile iiopProf = ior.FindInternetIiopProfile();
            Assertion.AssertNotNull("iiop ior profile not found", iiopProf);
            Assertion.AssertEquals("wrong hostname", "dev1-sf", iiopProf.HostName);
            Assertion.AssertEquals("wrong port", 8203, iiopProf.Port);
            Assertion.AssertEquals("wrong major", 1, iiopProf.Version.Major);
            Assertion.AssertEquals("wrong minor", 2, iiopProf.Version.Minor);
            Assertion.AssertEquals("wrong number of profiles", 2, ior.Profiles.Length);
        }
        
        public void TestParseAndRecreate() {
            string iorString = "IOR:0000000000000024524d493a48656c6c6f496e746572666163653a3030303030303030303030303030303000000000010000000000000050000102000000000c31302e34302e32302e3531001f9500000000000853617948656C6C6F0000000100000001000000200000000000010001000000020501000100010020000101090000000100010100";
            Ior ior = new Ior(iorString);
            string recreated = ior.ToString();
            Assertion.AssertEquals("ior not recreated", iorString.ToLower(), 
                                   recreated.ToLower());
            
            string iorString2 = "IOR:000000000000001b49444c3a636d6956322f5573657241636365737356323a312e3000000000000210ca1000000000650000000800000008646576312d73660033de6f8e0000004d000000020000000855736572504f41000000001043415355736572416363657373563200c3fbedfb0000000e007c4c51000000fd57aacdaf801a0000000e007c4c51000000fd57aacdaf80120000000000000000000000980001020000000008646576312d736600200b00000000004d000000020000000855736572504f41000000001043415355736572416363657373563200c3fbedfb0000000e007c4c51000000fd57aacdaf801a0000000e007c4c51000000fd57aacdaf8012000000000000000200000002000000140000000400000001000000230000000400000001000000000000000800000000cb0e0001";            
            Ior ior2 = new Ior(iorString2);
            string recreated2 = ior2.ToString();
            Assertion.AssertEquals("ior2 not recreated", iorString2.ToLower(), 
                                   recreated2.ToLower());                        
        }
        
        public void TestWithSslComponent() {
            string iorString = "IOR:000000000000003749444C3A43682F456C63612F49696F702F5475746F7269616C2F47657474696E67537461727465642F4164646572496D706C3A312E30000000000001000000000000005C000102000000000D3139322E3136382E312E33370000000000000005616464657200000000000002000000010000001C0000000000010001000000010001002000010109000000010001010000000014000000080000006000601F97";
            Ior ior = new Ior(iorString);
            Assertion.AssertEquals("wrong number of profiles", 1, ior.Profiles.Length);
            Assertion.AssertEquals("first profile type", TAG_INTERNET_IOP.ConstVal, ior.Profiles[0].ProfileId);
            InternetIiopProfile iiopProf = (InternetIiopProfile)ior.Profiles[0];            
            Assertion.AssertEquals("wrong hostname", "192.168.1.37", iiopProf.HostName); 
            Assertion.AssertEquals("wrong major", 1, iiopProf.Version.Major);
            Assertion.AssertEquals("wrong minor", 2, iiopProf.Version.Minor);            
            Assertion.AssertEquals("wrong number of components in profile", 2, iiopProf.TaggedComponents.Count);
            Assertion.AssertNotNull("no ssl tagged component found",
                                    iiopProf.GetTaggedComponentData(TAG_SSL_SEC_TRANS.ConstVal,
                                                                           Ch.Elca.Iiop.Security.Ssl.SSLComponentData.ClassType));
        }
        
    }

}

#endif
