/* Serializer.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 14.01.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.ComponentModel;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Diagnostics;
using System.Collections;
using System.Reflection;
using Ch.Elca.Iiop.Cdr;
using Ch.Elca.Iiop.Util;
using Ch.Elca.Iiop.Idl;
using Ch.Elca.Iiop.CorbaObjRef;
using Corba;
using omg.org.CORBA;

namespace Ch.Elca.Iiop.Marshalling {

    /// <summary>
    /// base class for all Serializer.
    /// </summary>
    public abstract class Serialiser {

        #region IConstructors

        public Serialiser() {
        }

        #endregion IConstructors
        #region IMethods

        /// <summary>
        /// serializes the actual value into the given stream
        /// </summary>
        public abstract void Serialise(Type formal, object actual, AttributeExtCollection attributes, 
                                       CdrOutputStream targetStream);

        /// <summary>
        /// deserialize the value from the given stream
        /// </summary>
        /// <param name="formal">the formal type of the parameter/field/...</param>
        /// <param name="attributes">the attributes on the parameter/field/..., but not the attributes on the formal-type</param>
        /// <param name="sourceStream"></param>
        /// <returns></returns>
        public abstract object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream);

        #endregion IMethods

    }

    // **************************************************************************************************
    #region serializer for primitive types

    /// <summary>serializes instances of System.Byte</summary>
    public class ByteSerialiser : Serialiser {

        #region IMethods

        public override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            targetStream.WriteOctet((byte)actual);
        }

        public override object Deserialise(Type formal, AttributeExtCollection attributes, 
                                           CdrInputStream sourceStream) {
            return sourceStream.ReadOctet();
        }

        #endregion IMethods

    }

    /// <summary>serializes instances of System.Boolean</summary> 
    public class BooleanSerialiser : Serialiser {

        #region IMethods

        public override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            targetStream.WriteBool((bool)actual);
        }

        public override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
            return sourceStream.ReadBool();
        }

        #endregion IMethods

    }

    /// <summary>serializes instances of System.Int16</summary>
    public class Int16Serialiser : Serialiser {

        #region IMethods
        
        public override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            targetStream.WriteShort((short)actual);
        }

        public override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
            return sourceStream.ReadShort();
        }

        #endregion IMethods

    }
    
    /// <summary>serializes instances of System.Int32</summary>
    public class Int32Serialiser : Serialiser {

        #region IMethods
        
        public override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            targetStream.WriteLong((int)actual);
        }

        public override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
            return sourceStream.ReadLong();
        }

        #endregion IMethods

    }

    /// <summary>serializes instances of System.Int64</summary>
    public class Int64Serialiser : Serialiser {

        #region IMethods

        public override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            targetStream.WriteLongLong((long)actual);
        }

        public override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
            return sourceStream.ReadLongLong();
        }

        #endregion IMethods

    }

    /// <summary>serializes instances of System.Single</summary>
    public class SingleSerialiser : Serialiser {

        #region IMethods
    
        public override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            targetStream.WriteFloat((float)actual);
        }

        public override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
            return sourceStream.ReadFloat();
        }

        #endregion IMethods

    }

    /// <summary>serializes instances of System.Double</summary>
    public class DoubleSerialiser : Serialiser {

        #region IMethods

        public override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            targetStream.WriteDouble((double)actual);
        }

        public override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
            return sourceStream.ReadDouble();
        }

        #endregion IMethods

    }

    public abstract class CharStringBaseSer : Serialiser {

        #region IMethods 

        protected bool UseWideOk(AttributeExtCollection attributes) {
            bool useWide = true;
            WideCharAttribute wideAttr = (WideCharAttribute)attributes.GetAttributeForType(typeof(WideCharAttribute));
            if (wideAttr != null) 
            {
                useWide = wideAttr.IsAllowed;
            }
            return useWide;
        }

        #endregion IMethods

    }

    /// <summary>serializes instances of System.Char</summary>
    public class CharSerialiser : CharStringBaseSer {

        #region IMethods

        public override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            bool useWide = UseWideOk(attributes);
            if (useWide) {
                targetStream.WriteWChar((char)actual);
            } else {
                // the high 8 bits of the character is cut off
                targetStream.WriteChar((char)actual);
            }
        }

        public override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
            bool useWide = UseWideOk(attributes);
            char result;
            if (useWide) {
                result = sourceStream.ReadWChar();
            } else {
                result = sourceStream.ReadChar();
            }
            return result;
        }

        #endregion IMethods

    }

    /// <summary>serializes instances of System.String which are serialized as string values</summary>
    public class StringSerialiser : CharStringBaseSer {

        #region IMethods

        public override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            bool useWide = UseWideOk(attributes);
            if (actual == null) { 
                // string may not be null, if StringValueAttriubte is set"
                throw new BAD_PARAM(10040, CompletionStatus.Completed_MayBe);
            }
            if (useWide) {
                targetStream.WriteWString((string)actual);
            } else {
                // encode with selected encoder, this can throw an exception, if an illegal character is encountered
                targetStream.WriteString((string)actual);
            }
        }

        public override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
            bool useWide = UseWideOk(attributes);
            object result = "";
            if (useWide) {
                result = sourceStream.ReadWString();
            } else {
                result = sourceStream.ReadString();
            }
            return result;
        }

        #endregion IMethods

    }

    #endregion
    // **************************************************************************************************
    
    // **************************************************************************************************    
    #region serializer for marshalbyref types
    
    /// <summary>serializes object references</summary>
    public class ObjRefSerializer : Serialiser {

        #region IMethods
        
        public override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            if (actual == null) { 
                WriteNullReference(targetStream); // null must be handled specially
                return;
            }
            MarshalByRefObject target = (MarshalByRefObject) actual; // this could be a proxy or the server object
            // get or create the objRef for the target
            ObjRef refToTarget =  RemotingServices.Marshal(target);
            Debug.WriteLine("marshal object reference : " + refToTarget + 
                            ", with URI: " + refToTarget.URI);
            // get the target address and port
            string host; int port; GiopVersion version;
            byte[] objectKey = ExtractDataFromObjRef(refToTarget, out host, out port,
                                                     out version);
            if ((objectKey == null) || (host == null)) { 
                // the objRef: " + refToTarget + ", uri: " +
                // refToTarget.URI + is not serializable, because connection data is missing 
                // hostName=host, objectKey=objectKey
                throw new INV_OBJREF(1961, CompletionStatus.Completed_MayBe);
            }
            Debug.WriteLine("connection information for objRef, host: " + host + ", port: " +
                            port + ", objKey-length: " + objectKey.Length); 
            // get the repository id for the type of this MarshalByRefObject
            string repositoryID = Repository.GetRepositoryID(actual.GetType());
            if (actual.GetType().Equals(typeof(MarshalByRefObject))) { 
                repositoryID = ""; 
            } // CORBA::Object has "" repository id
            // now create an IOR with the above information
            InternetIiopProfile profile = new InternetIiopProfile(version, host,
                                                                  (ushort)port, objectKey);
            Ior ior = new Ior(repositoryID, new IorProfile[] { profile });
            // now write the IOR to the stream
            ior.WriteToStream(targetStream);
        }

        private void WriteNullReference(CdrOutputStream targetStream) {
            Ior ior = new Ior("", new IorProfile[0]);
            ior.WriteToStream(targetStream); // write the null reference to the stream
        }

        /// <summary>
        /// gets the address data for the the objRef.
        /// </summary>
        /// <returns>the object Key for this URI</returns>
        private byte[] ExtractDataFromObjRef(ObjRef objRef, out string host,
                                             out int port, out GiopVersion version) {
            // if the object pointed to by this objRef is in this appdomain, get the address-info out of the channel-data
            IiopChannelData serverData = GetIIOPChannelData(objRef); 
            if (serverData != null) {
                host = serverData.HostName;
                port = serverData.Port;
                return IiopUrlUtil.GetObjectInfoForObjUri(objRef.URI, out version);
            } else {
                // if objref is from a proxy, the connection info is in the URI:
                string objectURI;
                string chanURI = IiopUrlUtil.ParseUrl(objRef.URI, out objectURI);
                IiopUrlUtil.ParseChanUri(chanURI, out host, out port);
                return IiopUrlUtil.GetObjectInfoForObjUri(objectURI, out version);
            }
        }

        /// <summary>gets the IIOPchannel-data from an ObjRef.</summary>
        private IiopChannelData GetIIOPChannelData(ObjRef objRef) {
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

        public override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
            // reads the encoded IOR from this stream
            Ior ior = new Ior(sourceStream);
            if (ior.IsNullReference()) { 
                return null; 
            } // received a null reference, return null
            // create a url from this ior:
            String objectURI = IiopUrlUtil.GetObjUriForObjectInfo(ior.ObjectKey, ior.Version);
            string url = IiopUrlUtil.GetUrl(ior.HostName, ior.Port, objectURI);
            Type interfaceType;
            if (!ior.TypID.Equals("")) { // empty string stands for CORBA::Object
                interfaceType = Repository.GetTypeForId(ior.TypID);
            } else {
                interfaceType = typeof(MarshalByRefObject);
            }
            if (interfaceType == null) { 
                // unknown repository id encountered:  ior.TypID
                throw new INTF_REPOS(1414, CompletionStatus.Completed_MayBe);
            }

            if ((!(formal.Equals(typeof(MarshalByRefObject)))) && !(formal.IsAssignableFrom(interfaceType))) {
                // for formal-parameter MarshalByRefObject everything is possible, the other formal types must be checked    
                Trace.WriteLine("received obj-reference is not compatible with the required formal parameter, formal: " +
                                formal + ", received: " + interfaceType);
                throw new BAD_PARAM(20010, CompletionStatus.Completed_MayBe);
            }
            
            // create a proxy
            object proxy = RemotingServices.Connect(interfaceType, url);
            return proxy;
        }

        #endregion IMethods

    }

    #endregion

    // **************************************************************************************************
    // ********************************* Serializer for value types *************************************
    // **************************************************************************************************

    /// <summary>standard serializer for pass by value object</summary>
    /// <remarks>if a CLS struct should be serialized as IDL struct and not as ValueType, use the IDLStruct Serializer</remarks>
    public class ValueObjectSerializer : Serialiser {

        #region IMethods

        public override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            // standard value types
            ValueOutputStream serStream = null;            
            if (targetStream is ValueOutputStream) { // nested value
                serStream = (ValueOutputStream) targetStream;
            } else { 
                serStream = new ValueOutputStream((CdrOutputStreamImpl) targetStream);
            }
            serStream.WriteValue(actual, formal);
        }

        public override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
            ValueInputStream deserStream = null;
            if (sourceStream is ValueInputStream) { // nested value
                deserStream = (ValueInputStream) sourceStream;
            } else { 
                deserStream = new ValueInputStream((CdrInputStreamImpl) sourceStream);
            }
            object result = deserStream.ReadValue(formal);
            return result;
        }

        #endregion IMethods

    }

    /// <summary>serializes an non boxed value as an IDL boxed value and deserialize an IDL boxed value as an unboxed value</summary>
    /// <remarks>do not use this serializer with instances of BoxedValues which should not be boxed or unboxed</remarks>
    public class BoxedValueSerializer : Serialiser {

        #region IFields

        private ValueObjectSerializer m_valueSer = new ValueObjectSerializer();

        #endregion IFields
        #region IMethods
        
        public override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            if (!formal.IsSubclassOf(typeof(BoxedValueBase))) { 
                // BoxedValueSerializer can only serialize formal types, 
                // which are subclasses of BoxedValueBase
                throw new INTERNAL(10041, CompletionStatus.Completed_MayBe);
            }
            // perform a boxing
            object boxed = null;
            if (actual != null) {
                boxed = Activator.CreateInstance(formal, new object[] { actual } );
            }
            m_valueSer.Serialise(formal, boxed, attributes, targetStream);
        }

        public override object Deserialise(Type formal, AttributeExtCollection attributes, 
                                           CdrInputStream sourceStream) {
            Debug.WriteLine("deserialise boxed value, formal: " + formal);
            if (!formal.IsSubclassOf(typeof(BoxedValueBase))) { 
                // BoxedValueSerializer can only serialize formal types,
                // which are subclasses of BoxedValueBase
                throw new INTERNAL(10041, CompletionStatus.Completed_MayBe);
            }

            BoxedValueBase boxedResult = (BoxedValueBase) m_valueSer.Deserialise(formal, attributes, sourceStream);
            object result = null;
            if (boxedResult != null) {
                // perform an unboxing
                result = boxedResult.Unbox();
            }
            Debug.WriteLine("unboxed result of boxedvalue-ser: " + result);
            return result;
        }

        #endregion IMethods

    }

    /// <summary>
    /// this class serializes .NET structs, which were mapped from an IDL-struct
    /// </summary>
    public class IdlStructSerializer : Serialiser {

        #region IMethods
    
        public override object Deserialise(System.Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
            FieldInfo[] fields = formal.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance);
            Marshaller marshaller = Marshaller.GetSingleton();
                        
            object instance = Activator.CreateInstance(formal);
            foreach (FieldInfo info in fields) {
                object fieldVal = marshaller.Unmarshal(info.FieldType, AttributeExtCollection.ConvertToAttributeCollection(info.GetCustomAttributes(true)), sourceStream);
                info.SetValue(instance, fieldVal);
            }
            return instance;
        }

        public override void Serialise(System.Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            FieldInfo[] fields = formal.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance);
            Marshaller marshaller = Marshaller.GetSingleton();
            foreach (FieldInfo info in fields) {
                marshaller.Marshal(info.FieldType, AttributeExtCollection.ConvertToAttributeCollection(info.GetCustomAttributes(true)), info.GetValue(actual), targetStream);
            }
        }

        #endregion IMethods

    }

    /// <summary>serailizes an instances as IDL abstract-value</summary>
    public class AbstractValueSerializer : Serialiser {

        #region IFields

        private ValueObjectSerializer m_valObjectSer = new ValueObjectSerializer();

        #endregion IFields
        #region IMethods

        public override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            // check if actual parameter is an IDL-struct: 
            // this is an illegal parameter for IDL-abstract value parameters
            AttributeExtCollection attrs = AttributeExtCollection.ConvertToAttributeCollection(actual.GetType().GetCustomAttributes(true));
            if (attrs.IsInCollection(typeof(IdlStructAttribute))) {
                // IDL-struct illegal parameter for formal type abstract value (actual type: actual.GetType() )
                throw new MARSHAL(20011, CompletionStatus.Completed_MayBe);
            }
            // check if it's a value-type:
            if (!ClsToIdlMapper.IsDefaultMarshalByVal(actual.GetType())) {
                // only a value type is possible as acutal value for a formal type abstract value / value base, actual type: actual.GetType() )
                throw new MARSHAL(20012, CompletionStatus.Completed_MayBe);            	
            }
            // if actual parameter is ok, serialize as idl-value object
            m_valObjectSer.Serialise(formal, actual, attributes, targetStream);
        }

        public override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
            // deserialise as IDL-value-type
            return m_valObjectSer.Deserialise(formal, attributes, sourceStream);
        }

        #endregion IMethods

    }

    /// <summary>serializes an instance of the class System.Type</summary>
    public class TypeSerializer : Serialiser {

        #region IFields
        
        private ValueObjectSerializer m_valObjectSer = new ValueObjectSerializer();

        #endregion IFields
        #region IMethods

        public override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            string repId = "";
            if (actual != null) {
                repId = Repository.GetRepositoryID((Type)actual);
            }
            CorbaTypeDesc desc = new CorbaTypeDesc(repId);
            m_valObjectSer.Serialise(typeof(CorbaTypeDesc), desc, attributes, targetStream);
        }

        public override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
            // deserialise as IDL-value-type
            CorbaTypeDesc descRes = (CorbaTypeDesc)m_valObjectSer.Deserialise(typeof(CorbaTypeDesc), attributes, sourceStream);
            string repId = descRes.respositoryID;
            Type result = Repository.GetTypeForId(repId); // get the type for the id
            return result;
        }

        #endregion IMethods

    }
    
    // **************************************************************************************************
    // ********************************* Serializer for special types *********************************** 
    // **************************************************************************************************

    /// <summary>serializes enums</summary>
    public class EnumSerializer : Serialiser {
        
        #region IMethods

        public override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            // check for IDL-enum mapped to a .NET enum
            AttributeExtCollection attrs = AttributeExtCollection.ConvertToAttributeCollection(formal.GetCustomAttributes(true));
            if (attrs.IsInCollection(typeof(IdlEnumAttribute))) {
                // idl enum's are mapped to .NET enums with long base-type, therefore all possible 2^32 idl-values can be represented
                int enumVal = (int) actual;
                targetStream.WriteULong((uint)enumVal);
            } else {
                // map to the base-type of the enum, write the value of the enum
                Type underlyingType = Enum.GetUnderlyingType(formal);
                Marshaller marshaller = Marshaller.GetSingleton();
                // marshal the enum value
                marshaller.Marshal(underlyingType, attributes, actual, targetStream); 
            }
        
        }

        public override object Deserialise(Type formal, AttributeExtCollection attributes, 
                                           CdrInputStream sourceStream) {
            AttributeExtCollection attrs = AttributeExtCollection.ConvertToAttributeCollection(formal.GetCustomAttributes(true));
            if (attrs.IsInCollection(typeof(IdlEnumAttribute))) {
                uint enumVal = sourceStream.ReadULong();
                return Enum.ToObject(formal, enumVal);    
            } else {
                // .NET enum handled with .NET to IDL mapping
                Type underlyingType = Enum.GetUnderlyingType(formal);                
                Marshaller marshaller = Marshaller.GetSingleton();
                // unmarshal the enum-value
                object val = marshaller.Unmarshal(underlyingType, attributes, sourceStream);
                if (!Enum.IsDefined(formal, val)) { 
                    // illegal enum value for enum: formal, val: val
                    throw new BAD_PARAM(10041, CompletionStatus.Completed_MayBe);
                }
                return Enum.ToObject(formal, val);
            }
        }
    
        #endregion IMethods

    }

    /// <summary>serializes idl sequences</summary>
    public class IdlSequenceSerializer : Serialiser {

        #region IMethods
        
        public override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            if (attributes.IsInCollection(typeof(IdlSequenceAttribute))) {
                // mapped from an IDL-sequence or CLS to IDL mapping
                attributes.RemoveAttributeOfType(typeof(IdlSequenceAttribute)); // this attribute is handled --> remove it
                Array array = (Array) actual;
                targetStream.WriteULong((uint)array.Length);
                // get marshaller for elemtype
                Type elemType = formal.GetElementType();
                MarshallerForType marshaller = new MarshallerForType(elemType, attributes);
                for (int i = 0; i < array.Length; i++) {
                    // it's more efficient to not determine serialise for each element; instead use cached ser
                    marshaller.Marshal(array.GetValue(i), targetStream);
                }
            } else {
                // attribute is missing: IDLSequnce, IDLSequenceSerializer can only
                // serialize IDLSequences, no general CLS-arrays, use boxedValueser
                // instead
                throw new INTERNAL(10040, CompletionStatus.Completed_MayBe);
            }
        }

        public override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
            if (attributes.IsInCollection(typeof(IdlSequenceAttribute))) {
                // mapped from an IDL-sequence
                attributes.RemoveAttributeOfType(typeof(IdlSequenceAttribute));
                uint nrOfElements = sourceStream.ReadULong();
                Array result = Array.CreateInstance(formal.GetElementType(), (int)nrOfElements);
                // get marshaller for array element type
                Type elemType = formal.GetElementType();
                MarshallerForType marshaller = new MarshallerForType(elemType, attributes);
                for (int i = 0; i < nrOfElements; i++) {
                    // it's more efficient to not determine serialise for each element; instead use cached ser
                    object entry = marshaller.Unmarshal(sourceStream);
                    result.SetValue(entry, i);
                }
                return result;
            } else {
                // attribute is missing: IDLSequnce, IDLSequenceSerializer can
                // only serialize IDLSequences,
                // no general CLS-arrays, use boxedValueser instead
                throw new INTERNAL(10040, CompletionStatus.Completed_MayBe);
                
            }
        }

        #endregion IMethods

    }

    /// <summary>serializes an instance as IDL-any</summary>
    public class AnySerializer : Serialiser {

        #region IFields
        
        private TypeCodeSerializer m_typeCodeSer = new TypeCodeSerializer();

        #endregion IFields
        #region IMethods

        public override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            TypeCodeImpl typeCode = new NullTC();
            if (actual != null) {
                typeCode = Repository.CreateTypeCodeForType(actual.GetType(), attributes);
            }
            typeCode.WriteToStream(targetStream);
            if (actual != null) {
                Marshaller marshaller = Marshaller.GetSingleton();
                attributes.RemoveAttributeOfType(typeof(ObjectIdlTypeAttribute));
                marshaller.Marshal(actual.GetType(), attributes, actual, targetStream);
            }
        }

        public override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
            omg.org.CORBA.TypeCode typeCode = (omg.org.CORBA.TypeCode)m_typeCodeSer.Deserialise(formal, 
                                                                                                attributes, sourceStream);
            if (!(typeCode is NullTC)) {
                Type dotNetType = Repository.GetTypeForTypeCode(typeCode);
                Marshaller marshaller = Marshaller.GetSingleton();
                return marshaller.Unmarshal(dotNetType, new AttributeExtCollection(new Attribute[0]), sourceStream);
            } else {
                return null;
            }
        }
        
        #endregion IMethods

    }
    
    /// <summary>serializes an instance as IDL abstract-interface</summary>
    public class AbstractInterfaceSerializer : Serialiser {

        #region IFields

        private ObjRefSerializer m_objRefSer = new ObjRefSerializer();
        private ValueObjectSerializer m_valueSer = new ValueObjectSerializer();

        #endregion IFields
        #region IMethods

        public override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            // if actual is null it shall be encoded as a valuetype: 15.3.7
            if ((actual != null) && (ClsToIdlMapper.IsMarshalByRef(actual.GetType()))) {
                targetStream.WriteBool(true); // an obj-ref is serialized
                m_objRefSer.Serialise(formal, actual, attributes, targetStream);
            } else if ((actual == null) || (ClsToIdlMapper.IsDefaultMarshalByVal(actual.GetType()))) {
                targetStream.WriteBool(false); // a value-type is serialised
                m_valueSer.Serialise(formal, actual, attributes, targetStream);
            } else {
                // actual value ( actual ) with type: 
                // actual.GetType() is not serializable for the formal type
                // formal
                throw new BAD_PARAM(6, CompletionStatus.Completed_MayBe);
            }
        }

        public override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
            bool isObjRef = sourceStream.ReadBool();
            if (isObjRef) {
                object result = m_objRefSer.Deserialise(formal, attributes, sourceStream);    
                return result;
            } else {
                object result = m_valueSer.Deserialise(formal, attributes, sourceStream);
                return result;
            }
        }

        #endregion IMethods
    
    }

    
    /// <summary>serializes .NET exceptions as IDL-Exceptions</summary>
    public class ExceptionSerializer : Serialiser {

        #region IMethods

        public override object Deserialise(System.Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
            string repId = sourceStream.ReadString();
            Type exceptionType = Repository.GetTypeForId(repId);
            if (exceptionType == null) {
                throw new UnknownUserException("user exception not found for id: " + repId);
            } else if (exceptionType.IsSubclassOf(typeof(AbstractCORBASystemException))) {
                // system exceptions are deserialized specially, because no inheritance is possible for exceptions, see implementation of the system exceptions
                uint minor = sourceStream.ReadULong();
                CompletionStatus completion = (CompletionStatus)((int) sourceStream.ReadULong());
                return (Exception)Activator.CreateInstance(exceptionType, new object[] { (int)minor, completion } );
            } else {
                Exception exception = (Exception)Activator.CreateInstance(exceptionType);
                // deserialise fields
                FieldInfo[] fields = exceptionType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                Marshaller marshaller = Marshaller.GetSingleton();
                foreach (FieldInfo field in fields) {
                    object fieldVal = marshaller.Unmarshal(field.FieldType, 
                                                           Util.AttributeExtCollection.ConvertToAttributeCollection(field.GetCustomAttributes(true)), 
                                                           sourceStream);
                    field.SetValue(exception, fieldVal);
                }                
                return exception;
            }
        }

        public override void Serialise(System.Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            string repId = Repository.GetRepositoryID(formal);
            targetStream.WriteString(repId);

            if (formal.IsSubclassOf(typeof(AbstractCORBASystemException))) {
                // system exceptions are serialized specially, because no inheritance is possible for exceptions, see implementation of the system exceptions
                AbstractCORBASystemException sysEx = (AbstractCORBASystemException) actual;
                targetStream.WriteULong((uint)sysEx.Minor);
                targetStream.WriteULong((uint)sysEx.Status);
            } else {
                FieldInfo[] fields = formal.GetFields(BindingFlags.Public | BindingFlags.NonPublic | 
                                                      BindingFlags.Instance | BindingFlags.DeclaredOnly);
                Marshaller marshaller = Marshaller.GetSingleton();
                foreach (FieldInfo field in fields) {
                    object fieldVal = field.GetValue(actual);
                    marshaller.Marshal(field.FieldType, 
                                       Util.AttributeExtCollection.ConvertToAttributeCollection(field.GetCustomAttributes(true)), 
                                       fieldVal, targetStream);
                }
            }
        }

        #endregion IMethods

    }

}

#if UnitTest

namespace Ch.Elca.Iiop.Tests {
	
    using System.IO;
    using NUnit.Framework;
    using Ch.Elca.Iiop.Marshalling;
    using Ch.Elca.Iiop.Cdr;
    using Ch.Elca.Iiop.Util;
    using omg.org.CORBA;
    
    /// <summary>
    /// Unit-tests for the serialisers
    /// </summary>
    public class SerialiserTest : TestCase {
        
        public SerialiserTest() {
        }

        public void TestByteSerialise() {
			MemoryStream outStream = new MemoryStream();
            CdrOutputStream cdrOut = new CdrOutputStreamImpl(outStream, 0);
            Serialiser ser = new ByteSerialiser();
            ser.Serialise(typeof(Byte), (byte)11, new AttributeExtCollection(), cdrOut);
            ser.Serialise(typeof(Byte), (byte)12, new AttributeExtCollection(), cdrOut);
            outStream.Seek(0, SeekOrigin.Begin);
            Assertion.AssertEquals(11, outStream.ReadByte());
            Assertion.AssertEquals(12, outStream.ReadByte());
            outStream.Close();
        }
        
        public void TestByteDeserialise() {
        	MemoryStream inStream = new MemoryStream();
        	inStream.WriteByte(11);
        	inStream.WriteByte(12);
        	inStream.Seek(0, SeekOrigin.Begin);
        	CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(inStream);
        	cdrIn.ConfigStream(0, new GiopVersion(1, 2));
			Serialiser ser = new ByteSerialiser();			
        	Assertion.AssertEquals(11, ser.Deserialise(typeof(Byte), 
        	                                           new AttributeExtCollection(), cdrIn));        	
        	Assertion.AssertEquals(12, ser.Deserialise(typeof(Byte), 
        	                                           new AttributeExtCollection(), cdrIn));
        	inStream.Close();        	
        }
        
        public void TestBooleanSerialise() {
			MemoryStream outStream = new MemoryStream();
            CdrOutputStream cdrOut = new CdrOutputStreamImpl(outStream, 0);
            Serialiser ser = new BooleanSerialiser();
            ser.Serialise(typeof(Boolean), true, new AttributeExtCollection(), cdrOut);
            ser.Serialise(typeof(Boolean), false, new AttributeExtCollection(), cdrOut);
            outStream.Seek(0, SeekOrigin.Begin);
            Assertion.AssertEquals(1, outStream.ReadByte());
            Assertion.AssertEquals(0, outStream.ReadByte());
            outStream.Close();
        }
        
        public void TestBooleanDeserialise() {
        	MemoryStream inStream = new MemoryStream();
        	inStream.WriteByte(0);
        	inStream.WriteByte(1);
        	inStream.Seek(0, SeekOrigin.Begin);
        	CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(inStream);
        	cdrIn.ConfigStream(0, new GiopVersion(1, 2));
			Serialiser ser = new BooleanSerialiser();
        	Assertion.AssertEquals(false, ser.Deserialise(typeof(Boolean), 
        	                                              new AttributeExtCollection(), cdrIn));        	
        	Assertion.AssertEquals(true, ser.Deserialise(typeof(Boolean), 
        	                                             new AttributeExtCollection(), cdrIn));
        	inStream.Close();        	
        }
        
        [ExpectedException(typeof(BAD_PARAM))]
        public void TestBooleanDeserialiseInvalidValue() {
        	MemoryStream inStream = new MemoryStream();
        	inStream.WriteByte(2);
        	inStream.Seek(0, SeekOrigin.Begin);
        	CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(inStream);
        	cdrIn.ConfigStream(0, new GiopVersion(1, 2));
			Serialiser ser = new BooleanSerialiser();
            try {
                ser.Deserialise(typeof(Boolean), new AttributeExtCollection(), cdrIn);
            } catch (Exception e) {
                inStream.Close();
                throw e;
            }
        }



    }

}
   	
#endif
