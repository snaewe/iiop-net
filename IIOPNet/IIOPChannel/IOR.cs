/* IOR.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 15.01.03  Dominic Ullmann (DUL), dul@elca.ch
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
using Ch.Elca.Iiop.Cdr;
using Ch.Elca.Iiop.Util;

namespace Ch.Elca.Iiop.CorbaObjRef {

    /// <summary>
    /// Interface supported by all tagged-components. 
    /// Tagged Components are used in IorProfiles.
    /// </summary>
    internal interface TaggedComponent {
        
        #region IProperties

        /// <summary>
        /// used for non-concrete tagged-components, may return null
        /// </summary>
        byte[] ByteData { get; set; }

        #endregion IProperties
        #region IMethods

        uint GetId();

        #endregion IMethods

    }


    /// <summary>
    /// This class represents a Corba IOR.
    /// </summary>
    public class Ior {

        #region IFields
        
        private IorProfile[] m_profiles;

        private byte[] m_objectKey;
        private GiopVersion m_version;
        private string m_typID;
        private string m_hostName;
        private ushort m_port;
        
        #endregion IFields
        #region IConstructors

        /// <summary>
        /// creates an IOR from the IOR stringified form
        /// </summary>
        public Ior(string iorAsString) {
            // iorAsString contains only characters 0-9, A-F and IOR --> all of this are short characters
            byte[] asByteArray = StringUtil.GetCutOffByteArrForString(iorAsString);
            MemoryStream memStream = new MemoryStream(asByteArray);
            IorStream iorStream = new IorStream(memStream);
            ParseIOR(iorStream);
        }

        /// <summary>creates an IOR from an IORStream</summary>
        public Ior(IorStream iorStream) {
            ParseIOR(iorStream);
        }

        /// <summary>
        /// parses an IOR embedded in a GIOP message
        /// </summary>
        public Ior(CdrInputStream cdrStream) {
            ParseIOR(cdrStream);
        }

        /// <summary>
        /// creates an IOR from the typeName and the profiles
        /// </summary>
        public Ior(string typeName, IorProfile[] profiles) {
            if (profiles == null) { 
                profiles = new IorProfile[0]; 
            }
            m_profiles = profiles;
            m_typID = typeName;

            if (profiles.Length > 0) { // if profiles are present, an InternetIIOPProfile is required
                IorProfile profile = SearchInternetIIOPProfile(profiles);
                if (profile == null) { 
                    throw new ArgumentException("no InternetIIOPProfile found in the IORProfiles; other profiles are not usable with this implementation"); 
                }
                AssignDefaultFromProfile(profile);
            }
        }

        #endregion IConstructors
        #region IProperties

        public IorProfile[] Profiles {
            get { 
                return m_profiles; 
            }
        }

        /// <summary>the GIOP version of the default InternetIIOPProfile</summary>
        public GiopVersion Version {
            get { 
                return m_version; 
            }
        }
        
        /// <summary>the TypeID of this IOR</summary>
        public string TypID {
            get { 
                return m_typID; 
            }
        }

        /// <summary>the hostname of the default InternetIIOPProfile</summary>
        public string HostName {
            get { 
                return m_hostName; 
            }
        }

        /// <summary>the port of the default InternetIIOPProfile</summary>
        public ushort Port {
            get { 
                return m_port; 
            }
        }

        /// <summary>
        /// the object key of the object pointed to by the default InternetIIOPProfile
        /// </summary>
        /// <remarks>
        /// the returned object key is not IOR-stringified!
        /// </remarks>
        public byte[] ObjectKey {
            get { 
                return m_objectKey; 
            }
        }

        #endregion IProperties
        #region IMethods

        /// <summary>
        /// serach for the InternetIIOPProfile in the profiles
        /// </summary>
        private IorProfile SearchInternetIIOPProfile(IorProfile[] profiles) {
            for (int i = 0; i < profiles.Length; i++) {
                if (profiles[i] is InternetIiopProfile) { 
                    return profiles[i]; 
                }
            }
            return null;
        }

        /// <summary>
        /// assigns the giop-version, type-id, object-key from the profile
        /// </summary>
        /// <param name="profile"></param>
        private void AssignDefaultFromProfile(IorProfile profile) {
            m_version = profile.Version;
            m_hostName = profile.HostName;
            m_port = profile.Port;
            m_objectKey = profile.ObjectKey;
        }

        /// <summary>returns true, if this IOR represents a null reference</summary>
        /// <returns>true, if a IOR represents null reference, otherwise false</returns>
        public bool IsNullReference() {
            return (m_typID.Equals("") && (m_profiles.Length == 0));
        }

        /// <summary>get the IOR data out of a stream containing a stringified IOR.</summary>
        private void ParseIOR(IorStream iorStream) {
            CdrInputStreamImpl cdrStream = new CdrInputStreamImpl(iorStream);
            byte flags = cdrStream.ReadOctet();
            cdrStream.ConfigStream(flags, new GiopVersion(1,2)); // giop dep operation are not used for IORs
            ParseIOR(cdrStream);
        }

        private void ParseIOR(CdrInputStream cdrStream) {
            m_typID = cdrStream.ReadString();
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
            ulong profileType = cdrStream.ReadULong(); 
            CdrEncapsulationInputStream encapStream = cdrStream.ReadEncapsulation();
            switch (profileType) {
                case 0: 
                    IorProfile result = new InternetIiopProfile(encapStream);
                    AssignDefaultFromProfile(result);
                    return result;
                default: 
                    throw new Exception("unparsable profile: " + profileType);
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
            MemoryStream encodedStream = new MemoryStream();
            IorStream iorStream = new IorStream(encodedStream);
            byte[] data = content.ToArray();
            iorStream.Write(data, 0, data.Length);
            // now create a string from the IOR-Data, IORData consist of short characters
            string result = StringUtil.GetStringFromShortChar(encodedStream.ToArray());
            return result;
        }

        /// <summary>
        /// write this IOR to a CDR-Stream in non-strignified form
        /// </summary>
        public void WriteToStream(CdrOutputStream cdrStream) {
            cdrStream.WriteString(m_typID);
            cdrStream.WriteULong((uint)m_profiles.Length); // nr of profiles
            for (int i = 0; i < m_profiles.Length; i++) {
                m_profiles[i].WriteToStream(cdrStream);
            }
        }

        #endregion IMethods

    }


    /// <summary>This class represents a profile in a CORBA-IOR</summary>
    public abstract class IorProfile {
        
        #region IFields

        protected GiopVersion m_giopVersion;

        protected string m_hostName;
        protected ushort m_port;

        protected byte[] m_objectKey;

        #endregion IFields
        #region IConstructors

        /// <summary>
        /// creates an IOR from the data in an encapsulation
        /// </summary>
        /// <param name="encapsulation"></param>
        protected IorProfile(CdrEncapsulationInputStream encapsulation) {
            ReadFromEncapsulation(encapsulation);
        }

        /// <summary>
        /// creates a profile from the standard data needed
        /// </summary>
        public IorProfile(GiopVersion version, string hostName, ushort port, byte[] objectKey) {
            m_giopVersion = version;
            m_hostName = hostName;
            m_port = port;
            m_objectKey = objectKey;
        }

        #endregion IConstructors
        #region IProperties

        public GiopVersion Version {
            get { 
                return m_giopVersion; 
            }
        }
       
        public string HostName {
            get { 
                return m_hostName; 
            }
        }

        public ushort Port {
            get { 
                return m_port; 
            }
        }

        public byte[] ObjectKey {
            get {
                return m_objectKey; 
            }
        }

        /// <summary>
        /// the id of the profile
        /// </summary>
        public abstract ulong ProfileId {
            get;
        }

        #endregion IProperties
        #region IMethods

        /// <summary>writes this profile into an encapsulation</summary>
        public abstract void WriteToStream(CdrOutputStream cdrStream);

        /// <summary>reads this profile from an encapsulation</summary>
        protected abstract void ReadFromEncapsulation(CdrEncapsulationInputStream cdrStream);

        #endregion IMethods
    
    }

    /// <summary>
    /// the profile for IIOP connections
    /// </summary>
    public class InternetIiopProfile : IorProfile {
        
        #region IFields

        /// <summary>the tagged components in this profile</summary>
        private TaggedComponent[] m_taggedComponents;        

        #endregion IFields
        #region IConstructors

        public InternetIiopProfile(GiopVersion version, string hostName, ushort port, byte[] objectKey) : base(version, hostName, port, objectKey) {
            // set the default tagged components
            m_taggedComponents = new TaggedComponent[1];
            // default codesetComponent
            m_taggedComponents[0] = new CodeSetComponent(Services.CodeSetService.DEFAULT_CHAR_SET,
                                                         new uint[] {Services.CodeSetService.ISO646IEC_SINGLE },
                                                         Services.CodeSetService.DEFAULT_WCHAR_SET,
                                                         new uint[] { Services.CodeSetService.ISO646IEC_MULTI });
        }

        /// <summary>
        /// reads an InternetIIOPProfile from an encapsulation (created with the method readEncapsulation in
        /// CDRStream) // not very nice, to change
        /// </summary>
        /// <param name="encapsulation"></param>
        public InternetIiopProfile(CdrEncapsulationInputStream encapsulation) : base(encapsulation) {
            
        }

        #endregion IConstructors
        #region IProperties

        /// <summary>the tagged components in this profile</summary>
        internal TaggedComponent[] TaggedComponents {
            get { 
                return m_taggedComponents; 
            }
            set {
                m_taggedComponents = value; 
            }
        }

        /// <summary>returns the profile-id for this profile</summary>
        public override ulong ProfileId {
            get { 
                return 0; 
            }
        }

        #endregion IProperties
        #region IMethods

        protected override void ReadFromEncapsulation(CdrEncapsulationInputStream encapsulation) {
            Debug.WriteLine("parse Internet IIOP Profile");
            byte giopMajor = encapsulation.ReadOctet();
            byte giopMinor = encapsulation.ReadOctet();
            m_giopVersion = new GiopVersion(giopMajor, giopMinor);

            Debug.WriteLine("giop-verion: " + m_giopVersion);
            m_hostName = encapsulation.ReadString();
            Debug.WriteLine("hostname: " + m_hostName);
            m_port = encapsulation.ReadUShort();
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
                uint nrOfComponent = encapsulation.ReadULong();
                Debug.WriteLine("nr of tagged-components in this profile: " + nrOfComponent);
                m_taggedComponents = new TaggedComponent[nrOfComponent];
                for (int i = 0; i < nrOfComponent; i++) {
                    uint id = encapsulation.ReadULong();
                    TaggedComponentSerializer ser = TaggedComponentSerRegistry.GetSerializer(id);
                    m_taggedComponents[i] = ser.ReadFromStream(encapsulation);
                }
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
            
            byte flags = 0;
            CdrEncapsulationOutputStream encapStream = new CdrEncapsulationOutputStream(flags);
            encapStream.WriteOctet(m_giopVersion.Major);
            encapStream.WriteOctet(m_giopVersion.Minor);
            encapStream.WriteString(m_hostName);
            encapStream.WriteUShort(m_port);
            encapStream.WriteULong((uint)m_objectKey.Length);
            encapStream.WriteOpaque(m_objectKey);
            // the tagged components            
            if (!(m_giopVersion.Major == 1 && m_giopVersion.Minor == 0)) { // for GIOP >= 1.1, tagged components are possible
                encapStream.WriteULong((uint)m_taggedComponents.Length);
                foreach (TaggedComponent comp in m_taggedComponents) {
                    TaggedComponentSerializer ser = TaggedComponentSerRegistry.GetSerializer(comp.GetId());
                    ser.WriteToStream(comp, encapStream);
                }
            }
            // write the whole encapsulation to the stream
            cdrStream.WriteEncapsulation(encapStream);
        }

        #endregion IMethods

    }


    /// <summary>
    /// generic tagged component
    /// </summary>
    internal class GenericTaggedComponent : TaggedComponent {
        
        #region IFields
        
        private byte[] m_data;
        private uint m_id;

        #endregion IFields
        #region IConstructors

        public GenericTaggedComponent(uint id, byte[] data) {
            m_data = data;
            m_id = id;
        }

        #endregion IConstructors
        #region IProperties

        public byte[] ByteData { 
            get { 
                return m_data; 
            } 
            set { 
                m_data = value; 
            } 
        }

        #endregion IProperties
        #region IMethods

        public uint GetId() { return m_id; }

        #endregion IMethods

    }


    /// <summary>
    /// this tagged-component in an IOR responsible for CodeSet-Information
    /// </summary>
    internal class CodeSetComponent : TaggedComponent {
    
        #region Constants

        internal const uint CODESET_COMPONENT_ID = 0x0001;        

        #endregion Constants
        #region IFields

        private uint m_nativeCharSet;
        private uint m_nativeWCharSet;

        private uint[] m_charConvSet;
        private uint[] m_wcharConvSet;

        #endregion IFields
        #region IConstructors

        public CodeSetComponent(uint nativeCharSet, uint[] charConvSet,
                                uint nativeWCharSet, uint[] wcharConvSet) {
            m_nativeCharSet = nativeCharSet;
            m_nativeWCharSet = nativeWCharSet;
            
            m_charConvSet = charConvSet;
            if (m_charConvSet == null) { 
                m_charConvSet = new uint[0]; 
            }
            m_wcharConvSet = wcharConvSet;
            if (m_wcharConvSet == null) { 
                m_wcharConvSet = new uint[0]; 
            }
        }

        #endregion IConstructors
        #region IProperties

        public uint NativeCharSet { 
            get {
                return m_nativeCharSet; 
            }
        }

        public uint NativeWCharSet { 
            get { 
                return m_nativeWCharSet; 
            }
        }

        public uint[] CharConvSet { 
            get { 
                return m_charConvSet; 
            }
        }
        
        public uint[] WCharConvSet { 
            get { 
                return m_wcharConvSet; 
            }
        }

        public byte[] ByteData { 
            get { 
                return null; 
            } 
            set {
            }
        } // not used here

        #endregion IProperties
        #region IMethods     

        public uint GetId() {
            return CODESET_COMPONENT_ID;
        }

        #endregion IMethods

    }


    /// <summary>
    /// registry managing serializer for tagged components
    /// </summary>
    internal class TaggedComponentSerRegistry {
        
        #region SFields
        
        private static Hashtable s_taggedComponetsSer = new Hashtable();

        #endregion SFields
        #region SConstructor
        
        static TaggedComponentSerRegistry() {
            AddTaggedComponentSer(new CodeSetComponentSer());
        }

        #endregion SConstructor
        #region SMethods

        private static void AddTaggedComponentSer(TaggedComponentSerializer ser) {
            s_taggedComponetsSer.Add(ser.GetId(), ser);
        }

        public static TaggedComponentSerializer GetSerializer(uint id) {
            TaggedComponentSerializer ser = (TaggedComponentSerializer)s_taggedComponetsSer[id];
            if (ser == null) {
                ser = new GenericTaggedComponentSer(id);
            }
            return ser;
        }

        #endregion SMethods

    }
    

    /// <summary>
    /// base class for serializer for tagged components
    /// </summary>
    internal abstract class TaggedComponentSerializer {
        
        #region IMethods

        public abstract void WriteToStream(TaggedComponent toSer, CdrOutputStream cdrStream);
        public abstract TaggedComponent ReadFromStream(CdrInputStream cdrStream);

        public abstract uint GetId();

        #endregion IMethods

    }

    /// <summary>
    /// generic serializer for unknown tagged components
    /// </summary>
    internal class GenericTaggedComponentSer : TaggedComponentSerializer {
        
        #region IFields

        private uint m_id;

        #endregion IFields
        #region IConstructors

        public GenericTaggedComponentSer(uint id) {
            m_id = id;
        }

        #endregion IConstructors
        #region IMethods
        
        public override void WriteToStream(TaggedComponent toSer, CdrOutputStream cdrStream) {
            cdrStream.WriteULong(toSer.GetId());
            cdrStream.WriteULong((uint)toSer.ByteData.Length);
            cdrStream.WriteOpaque(toSer.ByteData);
        }
        
        public override TaggedComponent ReadFromStream(CdrInputStream cdrStream) {
            uint bytesToFollow = cdrStream.ReadULong();
            byte[] data = cdrStream.ReadOpaque((int)bytesToFollow);
            return new GenericTaggedComponent(m_id, data);
        }

        public override uint GetId() {
            return m_id;
        }

        #endregion IMethods

    }

    /// <summary>
    /// tagged components serializer for codesetcomponent
    /// </summary>
    internal class CodeSetComponentSer : TaggedComponentSerializer {

        #region IMethods

        public override uint GetId() {
            return CodeSetComponent.CODESET_COMPONENT_ID;
        }

        public override void WriteToStream(TaggedComponent toSer, CdrOutputStream cdrStream) {
            CodeSetComponent asCodeSetComp = (CodeSetComponent) toSer;
            cdrStream.WriteULong(toSer.GetId());
            CdrEncapsulationOutputStream encap = new CdrEncapsulationOutputStream(0);

            encap.WriteULong(asCodeSetComp.NativeCharSet);
            encap.WriteULong((uint)asCodeSetComp.CharConvSet.Length);
            foreach (uint convSet in asCodeSetComp.CharConvSet) {
                encap.WriteULong(convSet);
            }

            encap.WriteULong(asCodeSetComp.NativeWCharSet);
            encap.WriteULong((uint)asCodeSetComp.WCharConvSet.Length);
            foreach (uint convSet in asCodeSetComp.WCharConvSet) {
                encap.WriteULong(convSet);
            }

            // write encapsulation to the stream
            cdrStream.WriteEncapsulation(encap);
        }

        public override TaggedComponent ReadFromStream(CdrInputStream cdrStream) {
            CdrEncapsulationInputStream encap = cdrStream.ReadEncapsulation();

            uint nativeCharCodeSet = encap.ReadULong();
            uint nrOfConvSets = encap.ReadULong();
            uint[] charConvSet = new uint[nrOfConvSets];
            for (int i = 0; i < nrOfConvSets; i++) {
                charConvSet[i] = encap.ReadULong();
            }

            uint nativeWCharCodeSet = encap.ReadULong();
            uint nrOfWCharConvSets = encap.ReadULong();
            uint[] wcharConvSet = new uint[nrOfWCharConvSets];
            for (int i = 0; i< nrOfWCharConvSets; i++) {
                wcharConvSet[i] = encap.ReadULong();
            }

            return new CodeSetComponent(nativeCharCodeSet, charConvSet, 
                                        nativeWCharCodeSet, wcharConvSet);
        }

        #endregion IMethods

    }

}
