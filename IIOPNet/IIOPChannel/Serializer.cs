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
    internal abstract class Serializer {

        #region IConstructors

        internal Serializer() {
        }

        #endregion IConstructors
        #region IMethods

        /// <summary>
        /// serializes the actual value into the given stream
        /// </summary>
        internal abstract void Serialize(object actual, 
                                         CdrOutputStream targetStream);

        /// <summary>
        /// deserialize the value from the given stream
        /// </summary>
        /// <param name="sourceStream"></param>
        /// <returns></returns>
        internal abstract object Deserialize(CdrInputStream sourceStream);

        /// <summary>
        /// Creates a serializer for serialising/deserialing a field
        /// </summary>
        protected static Serializer CreateSerializerForField(FieldInfo fieldToSer, SerializerFactory serFactory) {
            Type fieldType = fieldToSer.FieldType;
            AttributeExtCollection fieldAttrs = 
                ReflectionHelper.GetCustomAttriutesForField(fieldToSer, true);
            return serFactory.Create(fieldType, fieldAttrs);
        }

        /// <summary>
        /// serialises a field of a value-type
        /// </summary>
        /// <param name="fieldToSer"></param>
        protected static void SerializeField(FieldInfo fieldToSer, object actual, Serializer ser,
                                      CdrOutputStream targetStream) {
            ser.Serialize(fieldToSer.GetValue(actual), targetStream);
        }

        /// <summary>
        /// deserialises a field of a value-type and sets the value
        /// </summary>
        /// <returns>the deserialised value</returns>
        protected static object DeserializeField(FieldInfo fieldToDeser, object actual, Serializer ser,
                                          CdrInputStream sourceStream) {
            object fieldVal = ser.Deserialize(sourceStream);
            fieldToDeser.SetValue(actual, fieldVal);
            return fieldVal;
        }

        protected void CheckActualNotNull(object actual) {
            if (actual == null) {
                // not allowed
                throw new BAD_PARAM(3433, CompletionStatus.Completed_MayBe);
            }
            // ok
        }

        #endregion IMethods

    }

    // **************************************************************************************************
    #region serializer for primitive types

    /// <summary>serializes instances of System.Byte</summary>
    internal class ByteSerializer : Serializer {

        #region IMethods

        internal override void Serialize(object actual, 
                                       CdrOutputStream targetStream) {
            targetStream.WriteOctet((byte)actual);
        }

        internal override object Deserialize(CdrInputStream sourceStream) {
            return sourceStream.ReadOctet();
        }

        #endregion IMethods

    }

    /// <summary>serializes instances of System.SByte</summary>
    internal class SByteSerializer : Serializer {

        #region IMethods

        internal override void Serialize(object actual, 
                                       CdrOutputStream targetStream) {
            sbyte toSer = (sbyte)actual;
            targetStream.WriteOctet(
                unchecked((byte)toSer)); // do an unchecked cast, overflow no issue here
        }

        internal override object Deserialize(CdrInputStream sourceStream) {
            byte deser = sourceStream.ReadOctet();
            return unchecked((sbyte)deser); // do an unchecked cast, overflow no issue here
        }

        #endregion IMethods

    }

    /// <summary>serializes instances of System.Boolean</summary> 
    internal class BooleanSerializer : Serializer {

        #region IMethods

        internal override void Serialize(object actual,
                                       CdrOutputStream targetStream) {
            targetStream.WriteBool((bool)actual);
        }

        internal override object Deserialize(CdrInputStream sourceStream) {
            return sourceStream.ReadBool();
        }

        #endregion IMethods

    }

    /// <summary>serializes instances of System.Int16</summary>
    internal class Int16Serializer : Serializer {

        #region IMethods

        internal override void Serialize(object actual,
                                       CdrOutputStream targetStream) {
            targetStream.WriteShort((short)actual);
        }

        internal override object Deserialize(CdrInputStream sourceStream) {
            return sourceStream.ReadShort();
        }

        #endregion IMethods

    }

    /// <summary>serializes instances of System.UInt16</summary>
    internal class UInt16Serializer : Serializer {

        #region IMethods

        internal override void Serialize(object actual,
                                       CdrOutputStream targetStream) {
            targetStream.WriteUShort((ushort)actual);
        }

        internal override object Deserialize(CdrInputStream sourceStream) {
            return sourceStream.ReadUShort();
        }

        #endregion IMethods

    }

    /// <summary>serializes instances of System.Int32</summary>
    internal class Int32Serializer : Serializer {

        #region IMethods

        internal override void Serialize(object actual, 
                                       CdrOutputStream targetStream) {
            targetStream.WriteLong((int)actual);
        }

        internal override object Deserialize(CdrInputStream sourceStream) {
            return sourceStream.ReadLong();
        }

        #endregion IMethods

    }

    /// <summary>serializes instances of System.UInt32</summary>
    internal class UInt32Serializer : Serializer {

        #region IMethods

        internal override void Serialize(object actual, 
                                       CdrOutputStream targetStream) {
            targetStream.WriteULong((uint)actual);
        }

        internal override object Deserialize(CdrInputStream sourceStream) {
            return sourceStream.ReadULong();
        }

        #endregion IMethods

    }

    /// <summary>serializes instances of System.Int64</summary>
    internal class Int64Serializer : Serializer {

        #region IMethods

        internal override void Serialize(object actual,
                                       CdrOutputStream targetStream) {
            targetStream.WriteLongLong((long)actual);
        }

        internal override object Deserialize(CdrInputStream sourceStream) {
            return sourceStream.ReadLongLong();
        }

        #endregion IMethods

    }

    /// <summary>serializes instances of System.UInt64</summary>
    internal class UInt64Serializer : Serializer {

        #region IMethods

        internal override void Serialize(object actual,
                                       CdrOutputStream targetStream) {
            targetStream.WriteULongLong((ulong)actual);
        }

        internal override object Deserialize(CdrInputStream sourceStream) {
            return sourceStream.ReadULongLong();
        }

        #endregion IMethods

    }

    /// <summary>serializes instances of System.Single</summary>
    internal class SingleSerializer : Serializer {

        #region IMethods

        internal override void Serialize(object actual,
                                       CdrOutputStream targetStream) {
            targetStream.WriteFloat((float)actual);
        }

        internal override object Deserialize(CdrInputStream sourceStream) {
            return sourceStream.ReadFloat();
        }

        #endregion IMethods

    }

    /// <summary>serializes instances of System.Double</summary>
    internal class DoubleSerializer : Serializer {

        #region IMethods

        internal override void Serialize(object actual,
                                       CdrOutputStream targetStream) {
            targetStream.WriteDouble((double)actual);
        }

        internal override object Deserialize(CdrInputStream sourceStream) {
            return sourceStream.ReadDouble();
        }

        #endregion IMethods

    }

    /// <summary>serializes instances of System.Char</summary>
    internal class CharSerializer : Serializer {

        #region IFields

        private bool m_useWide;

        #endregion IFields
        #region IConstructors

        public CharSerializer(bool useWide) {
            m_useWide = useWide;
        }

        #endregion IConstructors
        #region IMethods

        internal override void Serialize(object actual,
                                       CdrOutputStream targetStream) {
            if (m_useWide) {
                targetStream.WriteWChar((char)actual);
            } else {
                // the high 8 bits of the character is cut off
                targetStream.WriteChar((char)actual);
            }
        }

        internal override object Deserialize(CdrInputStream sourceStream) {
            char result;
            if (m_useWide) {
                result = sourceStream.ReadWChar();
            } else {
                result = sourceStream.ReadChar();
            }
            return result;
        }

        #endregion IMethods

    }

    /// <summary>serializes instances of System.String which are serialized as string values</summary>
    internal class StringSerializer : Serializer {

        #region IFields

        private bool m_useWide;
        private bool m_allowNull;

        #endregion IFields
        #region IConstructors

        /// <summary>
        /// default constructor.
        /// </summary>
        /// <param name="useWide">Serialize as non-wide of wide-char.</param>
        /// <param name="allowNull">Allow to serialize null or not. If allowed, serialize it
        /// as empty string.</param>
        public StringSerializer(bool useWide, bool allowNull) {
            m_useWide = useWide;
            m_allowNull = allowNull;
        }

        #endregion IConstructors
        #region IMethods

        internal override void Serialize(object actual,
                                       CdrOutputStream targetStream) {
            // string may not be null by default, if StringValueAttriubte is set
            if (m_allowNull && actual == null) {
                actual = String.Empty;
            }
            CheckActualNotNull(actual);
            if (m_useWide) {
                targetStream.WriteWString((string)actual);
            } else {
                // encode with selected encoder, this can throw an exception, if an illegal character is encountered
                targetStream.WriteString((string)actual);
            }
        }

        internal override object Deserialize(CdrInputStream sourceStream) {
            object result = "";
            if (m_useWide) {
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
    internal class ObjRefSerializer : Serializer {

        #region IFields

        private Type m_forType;
        private IiopUrlUtil m_iiopUrlUtil;
        private bool m_serializeUsingConcreteType;

        #endregion IFields
        #region IConstructors

        public ObjRefSerializer(Type forType, IiopUrlUtil iiopUrlUtil,
                                bool serializeUsingConcreteType) {
            m_forType = forType;
            m_iiopUrlUtil = iiopUrlUtil;
            m_serializeUsingConcreteType = serializeUsingConcreteType;
        }

        #endregion IConstructors
        #region IMethods

        internal override void Serialize(object actual, CdrOutputStream targetStream) {
            if (actual == null) {
                Ior.WriteNullToStream(targetStream); // null must be handled specially
                return;
            }
            MarshalByRefObject target = (MarshalByRefObject) actual; // this could be a proxy or the server object

            // create the IOR for this URI, possibilities:
            // is a server object -> create ior from key and channel-data
            // is a proxy --> create IOR from url
            //                url possibilities: IOR:--hex-- ; iiop://addr/key ; corbaloc::addr:key ; ...
            Ior ior = null;
            if (RemotingServices.IsTransparentProxy(target)) {
                // proxy
                string url = RemotingServices.GetObjectUri(target);
                Debug.WriteLine("marshal object reference (from a proxy) with url " + url);
                Type actualType = actual.GetType();
                if (actualType.Equals(ReflectionHelper.MarshalByRefObjectType) &&
                    m_forType.IsInterface && m_forType.IsInstanceOfType(actual)) {
                    // when marshalling a proxy, without having adequate type information from an IOR
                    // and formal is an interface, use interface type instead of MarshalByRef to
                    // prevent problems on server
                    actualType = m_forType;
                }
                // get the repository id for the type of this MarshalByRef object
                string repositoryID = actualType == ReflectionHelper.MarshalByRefObjectType
                    ? "" // CORBA::Object has "" repository id
                    : Repository.GetRepositoryID(actualType);
                ior = m_iiopUrlUtil.CreateIorForUrl(url, repositoryID);
            } else {
                // server object
                // If an interface is expected we are going to make the target looks like the interface
                // so that it can be used even if this interface is explicitely implemented or if the
                // implementing type is not public.
                ior = IorUtil.CreateIorForObjectFromThisDomain(target,
                                                               m_forType.IsInterface ? m_forType : target.GetType(),
                                                               m_forType.IsInterface && !m_serializeUsingConcreteType);
            }

            Debug.WriteLine("connection information for objRef, nr of profiles: " + ior.Profiles.Length);

            // now write the IOR to the stream
            ior.WriteToStream(targetStream);
        }

        internal override object Deserialize(CdrInputStream sourceStream) {
            // reads the encoded IOR from this stream
            Ior ior = new Ior(sourceStream);
            if (ior.IsNullReference()) { 
                return null; 
            } // received a null reference, return null
            // create a url from this ior:
            string url = ior.ToString(); // use stringified form of IOR as url --> do not lose information
            Type interfaceType;
            if (!Repository.IsInterfaceCompatible(m_forType, ior.TypID, out interfaceType)) {
                // will be checked on first call remotely with is_a; don't do a remote check here, 
                // because not an appropriate place for a remote call; also safes call if ior not used.
                Trace.WriteLine(String.Format("ObjRef deser, not locally verifiable, that ior type-id " +
                                              "{0} is compatible to required formal type {1}. " + 
                                              "Remote check will be done on first call to this ior.",
                                              ior.TypID, m_forType.FullName));
            }
            // create a proxy
            //Console.WriteLine("Type for IOR with URL {0} is {1} ({2}), interface type: {3}",
            //                  url, ior.Type, ior.TypID, interfaceType);
            object proxy = RemotingServices.Connect(interfaceType, url);
            //Console.WriteLine("Connected to proxy of type {0} with type {1} from {2}",
            //                  proxy.GetType(), interfaceType, new StackTrace());
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
    internal class ValueObjectSerializer : Serializer {

        #region Types

        /// <summary>
        /// Serialises/deserialises a concrete instance of a value type.
        /// This is a helper to improve performance, and is only used by the ValueObjectSerializer.
        /// It's not directly selected by the SerializerFactory as Serializer.
        /// </summary>
        /// <remarks>
        /// This class can't inherit from the Serializer base class, because additional
        /// context information are needed.
        /// This class additionally doesn't inherit from Serializer base class, because 
        /// it should not be used like other Serializers.
        /// </remarks>
        internal class ValueConcreteInstanceSerializer {

            private Type m_forConcreteType;
            private Type m_forConcreteInstanceType;
            private bool m_isCustomMarshalled;
            private FieldInfo[] m_fieldInfos;
            private Serializer[] m_fieldSerializers;
            private SerializerFactory m_serFactory;
            private bool m_initalized;
            private string m_repositoryIDOfType;

            internal ValueConcreteInstanceSerializer(Type concreteType, SerializerFactory serFactory) {
                m_forConcreteType = concreteType;
                m_serFactory = serFactory;
                // determine the repository ID of this type
                m_repositoryIDOfType = DetermineRepositoryID(concreteType);
                // determine instance to instantiate for concreteType: 
                // check for a value type implementation class
                m_forConcreteInstanceType = DetermineInstanceToCreateType(m_forConcreteType);
                m_isCustomMarshalled = CheckForCustomMarshalled(m_forConcreteType);
            }

            private Type DetermineInstanceToCreateType(Type concreteType) {
                Type result = concreteType;
                object[] implAttr =
                    result.GetCustomAttributes(ReflectionHelper.ImplClassAttributeType, false);
                if ((implAttr != null) && (implAttr.Length > 0)) {
                    if (implAttr.Length > 1) {
                        // invalid type: actualType, only one ImplClassAttribute allowed
                        throw new INTERNAL(923, CompletionStatus.Completed_MayBe);
                    }
                    ImplClassAttribute implCl = (ImplClassAttribute)implAttr[0];
                    // get the type
                    result = Repository.GetValueTypeImplClass(implCl.ImplClass);
                    if (result == null) {
                        Trace.WriteLine("implementation class : " + implCl.ImplClass +
                                        " of value-type: " + concreteType + " couldn't be found");
                        throw new NO_IMPLEMENT(1, CompletionStatus.Completed_MayBe, implCl.ImplClass);
                    }
                }
                // type must not be abstract for beeing instantiable
                if (result.IsAbstract) {
                    // value-type couln't be instantiated: actualType
                    throw new NO_IMPLEMENT(931, CompletionStatus.Completed_MayBe);
                }
                return result;
            }

            /// <summary>checks, if custom marshalling must be used</summary>
            private bool CheckForCustomMarshalled(Type forType) {
                // subclasses of a custom marshalled type are automatically also custom marshalled: CORBA-spec-99-10-07: page 3-27
                return ReflectionHelper.ICustomMarshalledType.IsAssignableFrom(forType);
            }

            /// <summary>checks, if the type is an implementation of a value-type</summary>
            /// <remarks>fields of implementation classes are not serialized/deserialized</remarks>
            private bool IsImplClass(Type forType) {
                Type baseType = forType.BaseType;
                if (baseType != null) {
                    AttributeExtCollection attrs = AttributeExtCollection.ConvertToAttributeCollection(
                                                        baseType.GetCustomAttributes(false));
                    if (attrs.IsInCollection(ReflectionHelper.ImplClassAttributeType)) {
                        ImplClassAttribute implAttr = (ImplClassAttribute)
                                                      attrs.GetAttributeForType(ReflectionHelper.ImplClassAttributeType);
                        Type implClass = Repository.GetValueTypeImplClass(implAttr.ImplClass);
                        if (implClass == null) {
                            Trace.WriteLine("implementation class : " + implAttr.ImplClass +
                                        " of value-type: " + baseType + " couldn't be found");
                            throw new NO_IMPLEMENT(1, CompletionStatus.Completed_MayBe, implAttr.ImplClass);
                        }
                        if (implClass.Equals(forType)) {
                            return true;
                        }
                    }
                }
                return false;
            }

            /// <summary>
            /// initalize the serializer for usage. Before, the serializer is non-usable
            /// </summary>
            internal void Initalize() {
                if (m_initalized) {
                    throw new BAD_INV_ORDER(1678, CompletionStatus.Completed_MayBe);
                }
                if (!m_isCustomMarshalled) {
                    // could map fields also in consturctor, because no recursive
                    // occurence of ValueConcreteInstanceSerializer:
                    // ValueObjectSerializers are always created in between breaking the
                    // possible recursive chain
                    // but to be consistent with IdlStruct, do it this way.
                    DetermineFieldSerializers(m_serFactory);
                }
                m_initalized = true;
            }

            private void CheckInitalized() {
                if (!m_initalized) {
                    throw new BAD_INV_ORDER(1678, CompletionStatus.Completed_MayBe);
                }
            }

            private void DetermineFieldSerializers(SerializerFactory serFactory) {
                ArrayList allFields = new ArrayList();
                ArrayList allSerializers = new ArrayList();
                Stack typeHierarchy = CreateTypeHirarchyStack(m_forConcreteType);
                while (typeHierarchy.Count > 0) {
                    Type demarshalType = (Type)typeHierarchy.Pop();
                    // reads all fields declared in the Type: no inherited fields
                    FieldInfo[] fields = ReflectionHelper.GetAllDeclaredInstanceFieldsOrdered(demarshalType);
                    allFields.AddRange(fields);
                    for (int i = 0; i < fields.Length; i++) {
                        allSerializers.Add(CreateSerializerForField(fields[i], serFactory));
                    }
                }
                m_fieldInfos = (FieldInfo[])allFields.ToArray(typeof(FieldInfo));
                m_fieldSerializers =(Serializer[]) allSerializers.ToArray(typeof(Serializer));
            }

            /// <summary>
            /// creates a Stack with the inheritance information for the Type forType.
            /// </summary>
            private Stack CreateTypeHirarchyStack(Type forType) {
                Stack typeHierarchy = new Stack();
                Type currentType = forType;
                while (currentType != null) {
                    if (!IsImplClass(currentType)) { // ignore impl-classes in serialization code
                        typeHierarchy.Push(currentType);
                    }

                    currentType = currentType.BaseType;
                    if (currentType == ReflectionHelper.ObjectType || currentType == ReflectionHelper.ValueTypeType ||
                       (ClsToIdlMapper.IsMappedToAbstractValueType(currentType,
                                                                   AttributeExtCollection.EmptyCollection))) { // abstract value types are not serialized
                        break;
                    }
                }
                return typeHierarchy;
            }

            private string DetermineRepositoryID(Type forType) {
                string repId;
                if (!IsImplClass(forType)) {
                    repId = Repository.GetRepositoryID(forType);
                } else { // an impl-class is not serialized, because it's not known at the receiving ORB
                    repId = Repository.GetRepositoryID(forType.BaseType);
                }
                return repId;
            }

            /// <summary>writes all the fields of the instance</summary>
            private void WriteFields(object instance, 
                                     CdrOutputStream targetStream) {
                for (int i = 0; i < m_fieldInfos.Length; i++) {
                    if (!m_fieldInfos[i].IsNotSerialized) { // do not serialize transient fields
                        SerializeField(m_fieldInfos[i], instance, m_fieldSerializers[i],
                                       targetStream);
                    }
                }
            }

            /// <summary>reads and sets the all the fields of the instance</summary>
            private void ReadFields(object instance,
                                    CdrInputStream sourceStream) {
                for (int i = 0; i < m_fieldInfos.Length; i++) {
                    if (!m_fieldInfos[i].IsNotSerialized) { // do not serialize transient fields
                        DeserializeField(m_fieldInfos[i], instance, m_fieldSerializers[i],
                                         sourceStream);
                    }
                }
            }

            /// <summary>
            /// Serialize an instance of the type, this concrete serializer is for,
            /// i.e. actual.GetType() is the same type as the one passed to the constructor
            /// of this serializer.
            /// </summary>
            internal void Serialize(object actual, CdrOutputStream targetStream) {
                CheckInitalized();
                uint valueTag = CdrStreamHelper.MIN_VALUE_TAG; // value-tag with no option set
                // attentition here: if formal type represents an IDL abstract interface, writing no type information is not ok.
                // do not use no typing information option, because java orb can't handle it
                valueTag = valueTag | 0x00000002;
                StreamPosition indirPos = targetStream.WriteIndirectableInstanceTag(valueTag);
                targetStream.WriteIndirectableString(m_repositoryIDOfType, IndirectionType.IndirRepId,
                                                     IndirectionUsage.ValueType);

                // add instance to indirection table
                targetStream.StoreIndirection(actual,
                                              new IndirectionInfo(indirPos.GlobalPosition,
                                                                  IndirectionType.IndirValue,
                                                                  IndirectionUsage.ValueType));

                // value content
                if (!m_isCustomMarshalled) {
                    WriteFields(actual, targetStream);
                } else {
                    // custom marshalled
                    if (!(actual is ICustomMarshalled)) {
                        // can't deserialise custom value type, because ICustomMarshalled not implented
                        throw new INTERNAL(909, CompletionStatus.Completed_MayBe);
                    }
                    ((ICustomMarshalled)actual).Serialize(
                        new DataOutputStreamImpl(targetStream, m_serFactory));
                }
            }

            internal object Deserialize(CdrInputStream sourceStream,
                                        StreamPosition instanceStartPos, uint valueTag) {
                CheckInitalized();
                object result = Activator.CreateInstance(m_forConcreteInstanceType);
                // store indirection info for this instance, if another instance contains a reference to this one
                sourceStream.StoreIndirection(new IndirectionInfo(instanceStartPos.GlobalPosition,
                                                                  IndirectionType.IndirValue,
                                                                  IndirectionUsage.ValueType), 
                                              result);

                // now the value fields follow
                sourceStream.BeginReadValueBody(valueTag);

                // value content
                if (!m_isCustomMarshalled) {
                    ReadFields(result, 
                               sourceStream);
                } else {
                    // custom marshalled
                    if (!(result is ICustomMarshalled)) {
                        // can't deserialise custom value type, because ICustomMarshalled not implented
                        throw new INTERNAL(909, CompletionStatus.Completed_MayBe);
                    }
                    ((ICustomMarshalled)result).Deserialise(
                        new DataInputStreamImpl(sourceStream, m_serFactory));
                }

                sourceStream.EndReadValue(valueTag);
                return result;
            }


        }

        #endregion Types

        #region IFields

        private Type m_forType;
        private SerializerFactory m_serFactory;

        #endregion IFields
        #region IConstructors

        internal ValueObjectSerializer(Type forType, 
                                       SerializerFactory serFactory) {
            m_forType = forType;
            m_serFactory = serFactory;
        }

        #endregion IConstructors
        #region IMethods


        internal override void Serialize(object actual,
                                         CdrOutputStream targetStream) {
            if (actual == null) {
                targetStream.WriteULong(0); // write a null-value
                return;
            }

            // if value is already in indirection table, write indirection
            if (targetStream.IsPreviouslyMarshalled(actual,
                                                    IndirectionType.IndirValue,
                                                    IndirectionUsage.ValueType)) {
                // write indirection
                targetStream.WriteIndirection(actual);
                return; // write completed
            } else {
                // serialize a concrete instance
                ValueConcreteInstanceSerializer valConSer =
                    m_serFactory.CreateConcreteValueTypeSer(actual.GetType());
                valConSer.Serialize(actual, targetStream);
            }
        }

        internal override object Deserialize(CdrInputStream sourceStream) {
            sourceStream.BeginReadNewValue();
            StreamPosition instanceStartPos;
            bool isIndirection;
            uint valueTag = sourceStream.ReadInstanceOrIndirectionTag(out instanceStartPos, 
                                                                      out isIndirection);
            if (isIndirection) {
                // return indirected value
                // resolve indirection:
                StreamPosition indirectionPosition = sourceStream.ReadIndirectionOffset();
                return sourceStream.GetObjectForIndir(new IndirectionInfo(indirectionPosition.GlobalPosition,
                                                                          IndirectionType.IndirValue,
                                                                          IndirectionUsage.ValueType),
                                                      true);
            } else {
                if (IsNullValue(valueTag)) {
                    return null;
                }

                // non-null value
                if (HasCodeBaseUrl(valueTag)) {
                    HandleCodeBaseUrl(sourceStream);
                }

                Type actualType = GetActualType(m_forType, sourceStream, valueTag);
                if (!m_forType.IsAssignableFrom(actualType)) {
                    // invalid implementation class of value type: 
                    // instance.GetType() is incompatible with: formal
                    throw new BAD_PARAM(903, CompletionStatus.Completed_MayBe);
                }
                ValueConcreteInstanceSerializer valConSer =
                    m_serFactory.CreateConcreteValueTypeSer(actualType);
                return valConSer.Deserialize(sourceStream, instanceStartPos, valueTag);
            }
        }

        private bool IsNullValue(uint valueTag) {
            return valueTag == 0;
        }

        private bool HasCodeBaseUrl(uint valueTag) {
            return ((valueTag & 0x00000001) > 0);
        }

        private void HandleCodeBaseUrl(CdrInputStream sourceStream) {
            sourceStream.ReadIndirectableString(IndirectionType.CodeBaseUrl,
                                                IndirectionUsage.ValueType,
                                                false);
        }

        /// <summary>
        /// gets the type of which the actual parameter is / should be ...
        /// </summary>
        private Type GetActualType(Type formal, CdrInputStream sourceStream, uint valueTag) {
            Type actualType = null;
            switch (valueTag & 0x00000006) {
                case 0: 
                    // actual = formal-type
                    actualType = formal;
                    break;
                case 2:
                    // single repository-id follows
                    string repId = sourceStream.ReadIndirectableString(IndirectionType.IndirRepId,
                                                                       IndirectionUsage.ValueType,
                                                                       false);
                    actualType = Repository.GetTypeForId(repId);
                    if (actualType == null) { 
                        // repository id used is unknown: repId
                        throw new NO_IMPLEMENT(941, CompletionStatus.Completed_MayBe, repId);
                    }
                    break;
                case 6:
                    // TODO: handle indirections here
                    // a list of repository-id's
                    int nrOfIds = sourceStream.ReadLong();
                    if (nrOfIds == 0) { 
                        // a list of repository-id's for type-information must contain at least one element
                        throw new MARSHAL(935, CompletionStatus.Completed_MayBe);
                    }
                    string mostDerived = sourceStream.ReadString(); // use only the most derived type, no truncation allowed
                    for (int i = 1; i < nrOfIds; i++) { 
                        sourceStream.ReadString(); 
                    }
                    actualType = Repository.GetTypeForId(mostDerived);
                    break;
                default:
                    // invalid value-tag found: " + valueTag
                    throw new MARSHAL(937, CompletionStatus.Completed_MayBe);
            }
            if (ClsToIdlMapper.IsInterface(actualType)) { 
                // can't instantiate value-type of type: actualType
                throw new NO_IMPLEMENT(945, CompletionStatus.Completed_MayBe);
            }
            return actualType;
        }

        #endregion IMethods

    }

    /// <summary>serializes an non boxed value as an IDL boxed value and deserialize an IDL boxed value as an unboxed value</summary>
    /// <remarks>do not use this serializer with instances of BoxedValues which should not be boxed or unboxed</remarks>
    internal class BoxedValueSerializer : Serializer {

        #region IFields

        private ValueObjectSerializer m_valueSer;
        private bool m_convertMultiDimArray = false;
        private Type m_forType;

        #endregion IFields
        #region IConstructors

        public BoxedValueSerializer(Type forType, bool convertMultiDimArray,
                                    SerializerFactory serFactory) {
            CheckFormalIsBoxedValueType(forType);
            m_forType = forType;
            m_convertMultiDimArray = convertMultiDimArray;
            m_valueSer = new ValueObjectSerializer(forType, serFactory);
        }

        #endregion IConstructors
        #region IMethods

        private void CheckFormalIsBoxedValueType(Type formal) {
            if (!formal.IsSubclassOf(ReflectionHelper.BoxedValueBaseType)) { 
                // BoxedValueSerializer can only serialize formal types, 
                // which are subclasses of BoxedValueBase
                throw new INTERNAL(10041, CompletionStatus.Completed_MayBe);
            }
        }

        internal override void Serialize(object actual,
                                         CdrOutputStream targetStream) {
            Debug.WriteLine("Begin serialization of boxed value type");
            // perform a boxing
            object boxed = null;
            if (actual != null) {
                if (m_convertMultiDimArray) {
                    // actual is a multi dimensional array, which must be first converted to a jagged array
                    actual = 
                        BoxedArrayHelper.ConvertMoreDimToNestedOneDimChecked(actual);
                }
                boxed = Activator.CreateInstance(m_forType, new object[] { actual } );
            }
            m_valueSer.Serialize(boxed, targetStream);
        }

        internal override object Deserialize(CdrInputStream sourceStream) {
            Debug.WriteLine("Begin deserialization of boxed value type");
            BoxedValueBase boxedResult = (BoxedValueBase) 
                m_valueSer.Deserialize(sourceStream);
            object result = null;
            if (boxedResult != null) {
                // perform an unboxing
                result = boxedResult.Unbox();
            if (m_convertMultiDimArray) {
                // result is a jagged arary, which must be converted to a true multidimensional array
                    result = BoxedArrayHelper.ConvertNestedOneDimToMoreDimChecked(result);
                }
            }

            Debug.WriteLine("unboxed result of boxedvalue-ser: " + result);
            return result;
        }

        #endregion IMethods

    }

    /// <summary>
    /// this class serializes .NET structs, which were mapped from an IDL-struct
    /// </summary>
    internal class IdlStructSerializer : Serializer {

        #region IFields

        private Serializer[] m_fieldSerializers;
        private FieldInfo[] m_fields;
        private Type m_forType;
        private bool m_initalized = false;
        private SerializerFactory m_serFactory;

        #endregion IFields
        #region IConstructors

        /// <summary>
        /// creates the idl struct serializer. To prevent issues with
        /// recurive elements, the serializer must be initalized in
        /// an additional step by calling Initalize.
        /// The serializer must be cached to return it for recursive requests.
        /// </summary>
        internal IdlStructSerializer(Type forType, SerializerFactory serFactory) : base() {
            m_forType = forType;
            m_serFactory = serFactory;
        }

        #endregion IConstructors
        #region IMethods

        /// <summary>
        /// initalize the serializer for usage. Before, the serializer is non-usable
        /// </summary>
        internal void Initalize() {
            if (m_initalized) {
                throw new BAD_INV_ORDER(1678, CompletionStatus.Completed_MayBe);
            }
            DetermineFieldMapping();
            m_initalized = true;
        }

        private void CheckInitalized() {
            if (!m_initalized) {
                throw new BAD_INV_ORDER(1678, CompletionStatus.Completed_MayBe);
            }
        }

        private void DetermineFieldMapping() {
            m_fields = ReflectionHelper.GetAllDeclaredInstanceFieldsOrdered(m_forType);
            m_fieldSerializers = new Serializer[m_fields.Length];
            for (int i = 0; i < m_fields.Length; i++) {
                m_fieldSerializers[i] = CreateSerializerForField(m_fields[i], m_serFactory);
            }
        }

        internal override object Deserialize(CdrInputStream sourceStream) {
            CheckInitalized();
            object instance = Activator.CreateInstance(m_forType);
            for (int i = 0; i < m_fieldSerializers.Length; i++) {
                DeserializeField(m_fields[i], instance, m_fieldSerializers[i], sourceStream);
            }
            return instance;
        }

        internal override void Serialize(object actual, CdrOutputStream targetStream) {
            CheckInitalized();
            for (int i = 0; i < m_fieldSerializers.Length; i++) {
                SerializeField(m_fields[i], actual, m_fieldSerializers[i], targetStream);
            }
        }

        #endregion IMethods

    }

    internal class IdlUnionSerializer : Serializer {

        #region Constants

        private const string GET_FIELD_FOR_DISCR_METHOD_NAME = UnionGenerationHelper.GET_FIELD_FOR_DISCR_METHOD;

        private const string DISCR_FIELD_NAME = UnionGenerationHelper.DISCR_FIELD_NAME;

        private const string INITALIZED_FIELD_NAME = UnionGenerationHelper.INIT_FIELD_NAME;

        #endregion Constants
        #region IFields

        private Type m_forType;
        private FieldInfo m_discrField;
        private FieldInfo m_initField;
        private Serializer m_discrSerializer;
        private SerializerFactory m_serFactory;

        #endregion IFields
        #region IConstructors

        internal IdlUnionSerializer(Type forType, SerializerFactory serFactory) {
            m_forType = forType;
            // disciminator can't be the same type then the union -> therefore no recursive problem here.
            m_discrField = GetDiscriminatorField(m_forType);
            m_discrSerializer = CreateSerializerForField(m_discrField, serFactory);
            m_initField = GetInitalizedField(m_forType);
            m_serFactory = serFactory;
        }

        #endregion IConstructors
        #region IMethods

        private FieldInfo GetDiscriminatorField(Type formal) {
            FieldInfo discrValField = formal.GetField(DISCR_FIELD_NAME, 
                                                      BindingFlags.Instance | BindingFlags.NonPublic);
            if (discrValField == null) {
                throw new INTERNAL(898, CompletionStatus.Completed_MayBe);
            }
            return discrValField;
        }

        private FieldInfo GetValFieldForDiscriminator(Type formal, object discrValue) {
            MethodInfo getCurrentField = formal.GetMethod(GET_FIELD_FOR_DISCR_METHOD_NAME, 
                                                          BindingFlags.Static | BindingFlags.NonPublic);
            if (getCurrentField == null) {
                throw new INTERNAL(898, CompletionStatus.Completed_MayBe);
            }
            return (FieldInfo)getCurrentField.Invoke(null, new object[] { discrValue });
        }

        private FieldInfo GetInitalizedField(Type formal) {
            FieldInfo initalizedField = formal.GetField(INITALIZED_FIELD_NAME, 
                                                        BindingFlags.Instance | BindingFlags.NonPublic);
            if (initalizedField == null) {
                throw new INTERNAL(898, CompletionStatus.Completed_MayBe);
            }
            return initalizedField;
        }

        internal override object Deserialize(CdrInputStream sourceStream) {
            // instantiate the resulting union
            object result = Activator.CreateInstance(m_forType);
            // deserialise discriminator value
            object discrVal = DeserializeField(m_discrField, result, m_discrSerializer, sourceStream);

            // determine value to deser
            FieldInfo curField = GetValFieldForDiscriminator(m_forType, discrVal);
            if (curField != null) {
                // deserialise value
                Serializer curFieldSer = CreateSerializerForField(curField, m_serFactory);
                DeserializeField(curField, result, curFieldSer, sourceStream);
            }
            m_initField.SetValue(result, true);
            return result;
        }

        internal override void Serialize(object actual, CdrOutputStream targetStream) {
            bool isInit = (bool)m_initField.GetValue(actual);
            if (isInit == false) {
                throw new BAD_PARAM(34, CompletionStatus.Completed_MayBe);
            }
            // determine value of the discriminator
            object discrVal = m_discrField.GetValue(actual);
            // get the field matching the current discriminator
            FieldInfo curField = GetValFieldForDiscriminator(m_forType, discrVal);

            m_discrSerializer.Serialize(discrVal, targetStream);
            if (curField != null) {
                // seraialise value
                Serializer curFieldSer = CreateSerializerForField(curField, m_serFactory);
                SerializeField(curField, actual, curFieldSer, targetStream);
            } 
            // else:  case outside covered discr range, do not serialise value, only discriminator
        }

        #endregion IMethods
    }

    /// <summary>serailizes an instances as IDL abstract-value</summary>
    internal class AbstractValueSerializer : Serializer {

        #region IFields

        private ValueObjectSerializer m_valObjectSer;

        #endregion IFields
        #region IConstructors

        internal AbstractValueSerializer(Type forType, SerializerFactory serFactory) {
            m_valObjectSer = new ValueObjectSerializer(forType, serFactory);
        }

        #endregion IConstructors
        #region IMethods

        internal override void Serialize(object actual,
                                         CdrOutputStream targetStream) {
            if (actual != null) {
                // check if actual parameter is an IDL-struct: 
                // this is an illegal parameter for IDL-abstract value parameters
                if (ClsToIdlMapper.IsMarshalledAsStruct(actual.GetType())) {
                    // IDL-struct illegal parameter for formal type abstract value (actual type: actual.GetType() )
                    throw new MARSHAL(20011, CompletionStatus.Completed_MayBe);
                }
                // check if it's a concrete value-type:
                if (!ClsToIdlMapper.IsMappedToConcreteValueType(actual.GetType())) {
                    // only a value type is possible as acutal value for a formal type abstract value / value base, actual type: actual.GetType() )
                    throw new MARSHAL(20012, CompletionStatus.Completed_MayBe);
                }
            }
            // if actual parameter is ok, serialize as idl-value object
            m_valObjectSer.Serialize(actual, targetStream);
        }

        internal override object Deserialize(CdrInputStream sourceStream) {
            // deserialise as IDL-value-type
            return m_valObjectSer.Deserialize(sourceStream);
        }

        #endregion IMethods

    }

    /// <summary>serializes an instance of the class System.Type</summary>
    internal class TypeSerializer : Serializer {

        #region IFields

        private TypeCodeSerializer m_typeCodeSer;

        #endregion IFields
        #region IConstructors

        internal TypeSerializer(SerializerFactory serFactory) {
            m_typeCodeSer = new TypeCodeSerializer(serFactory);
        }

        #endregion IConstructors
        #region IMethods

        internal override void Serialize(object actual, CdrOutputStream targetStream) {
            omg.org.CORBA.TypeCode tc;
            tc = Repository.CreateTypeCodeForType((Type)actual, AttributeExtCollection.EmptyCollection);
            m_typeCodeSer.Serialize(tc, targetStream);
        }

        internal override object Deserialize(CdrInputStream sourceStream) {
            omg.org.CORBA.TypeCode tc = 
                (omg.org.CORBA.TypeCode)m_typeCodeSer.Deserialize(sourceStream);
            Type result = null;
            if (!(tc is NullTC)) {
                result = Repository.GetTypeForTypeCode(tc);
            }
            return result;
        }

        #endregion IMethods

    }

    /// <summary>
    /// base class for enum serializers
    /// </summary>
    internal abstract class EnumSerializerBase : Serializer {

        #region IFields

        protected Type m_forType;

        #endregion IFields
        #region IConstructors

        protected EnumSerializerBase(Type forType) {
            m_forType = forType;
        }

        #endregion IConstructors
        #region IMethods

        protected object CheckedConvertToEnum(object enumBaseVal) {
            if (!Enum.IsDefined(m_forType, enumBaseVal)) { 
                // illegal enum value for enum: formal, val: val
                throw CreateInvalidEnumValException(enumBaseVal);
            }
            return Enum.ToObject(m_forType, enumBaseVal);
        }

        protected Exception CreateInvalidEnumValException(object val) {
            return new BAD_PARAM(10041, CompletionStatus.Completed_MayBe, "val: " + val);
        }

        #endregion IMethods

    }

    /// <summary>serializes enums mapped from idl to cls</summary>
    internal class IdlEnumSerializer : EnumSerializerBase {

        #region IConstructors

        internal IdlEnumSerializer(Type forType) : base(forType) {
        }

        #endregion IConstructors
        #region IMethods

        internal override void Serialize(object actual,
                                         CdrOutputStream targetStream) {
            // all possible 2^32 values of an int based enum can be represented in idl enum range
            int enumVal = (int)actual;
            targetStream.WriteULong((uint)enumVal);
        }

        internal override object Deserialize(CdrInputStream sourceStream) {
            int valAsInt = (int)sourceStream.ReadULong();
            return CheckedConvertToEnum(valAsInt);
        }

        #endregion IMethods

    }

    /// <summary>serializes enums by mapping the cls range to idl range in
    /// the following way: </summary>
    internal class EnumMapClsToIdlRangeSerializer : EnumSerializerBase {

        #region IFields

        private Array m_enumVals;

        #endregion IFields
        #region IConstructors

        internal EnumMapClsToIdlRangeSerializer(Type forType) : base(forType) {
            m_enumVals = (Array)Enum.GetValues(forType);
        }

        #endregion IConstructors
        #region IMethods

        private int MapEnumValToIndexVal(object enumVal) {
            return Array.IndexOf(m_enumVals, enumVal);
        }

        internal override void Serialize(object actual,
                                         CdrOutputStream targetStream) {
            int mappedVal = MapEnumValToIndexVal(actual);
            targetStream.WriteULong((uint)mappedVal);
        }

        internal override object Deserialize(CdrInputStream sourceStream) {
            int index = (int)sourceStream.ReadULong();
            object val;
            if (index >= 0 && index < m_enumVals.Length) {
                val = m_enumVals.GetValue(index);
            } else {
                throw CreateInvalidEnumValException(index);
            }
            return CheckedConvertToEnum(val);
        }

        #endregion IMethods

    }

    /// <summary>
    /// Serializer for cls flags mapped to idl equivalents
    /// </summary>
    internal class FlagsSerializer : Serializer {

        #region IFields

        private Serializer m_netFlagsValSerializer;
        private Type m_forType;

        #endregion IFields
        #region IConstructors

        internal FlagsSerializer(Type forType, SerializerFactory serFactory) {
            m_forType = forType;
            Type underlyingType = Enum.GetUnderlyingType(m_forType);
            // undelying type is not the same than the flags enum -> no problem with recursive serializers
            // flags are mapped to the corresponding underlying type in idl, because no
            // flags concept in idl
            m_netFlagsValSerializer =
                serFactory.Create(underlyingType, AttributeExtCollection.EmptyCollection);
        }

        #endregion IConstructors
        #region IMethods

        internal override void Serialize(object actual,
                                         CdrOutputStream targetStream) {
            // map to the base-type of the enum, write the value of the enum
            m_netFlagsValSerializer.Serialize(actual, targetStream);
        }


        internal override object Deserialize(CdrInputStream sourceStream) {
            // .NET flags handled with .NET to IDL mapping
            object val = m_netFlagsValSerializer.Deserialize(sourceStream);
            // every value is allowed for flags -> therefore no checks
            return Enum.ToObject(m_forType, val);
        }

        #endregion IMethods

    }

    /// <summary>serializes idl sequences</summary>
    internal class IdlSequenceSerializer<T> : Serializer {

        #region IFields

        private int m_bound;
        private bool m_allowNull;
        private Type m_forTypeElemType;
        private Serializer m_elementSerializer;
        private bool isPrimitiveTypeArray;

        #endregion IFields
        #region IConstructors

        /// <summary>
        /// default constructor.
        /// </summary>
        /// <param name="elemAttrs">The sequence element type attributes</param>
        /// <param name="bound">the number of elements allowed maximally</param>
        /// <param name="allowNull">Allow to serialize null or not. If allowed, serialize it
        /// as empty sequence.</param>
        /// <param name="serFactory">The serializer factory created this serializer</param>
        public IdlSequenceSerializer(
                                     AttributeExtCollection elemAttrs,
                                     int bound, bool allowNull, SerializerFactory serFactory) {
            m_allowNull = allowNull;
            m_forTypeElemType = typeof(T);
            m_bound = bound;
            isPrimitiveTypeArray = (typeof(T) == typeof(byte))
                                || (typeof(T) == typeof(short)) || (typeof(T) == typeof(ushort)) 
                                || (typeof(T) == typeof(int)) || (typeof(T) == typeof(uint))
                                || (typeof(T) == typeof(long)) || (typeof(T) == typeof(ulong))
                                || (typeof(T) == typeof(float)) || (typeof(T) == typeof(double));
            // element is not the same than the sequence -> therefore no problems with recursion
            DetermineElementSerializer(m_forTypeElemType, elemAttrs, serFactory);
        }

        #endregion IConstructors
        #region IMethods

        private void DetermineElementSerializer(Type elemType,
                                                AttributeExtCollection elemAttrs,
                                                SerializerFactory serFactory) {
            m_elementSerializer =
                serFactory.Create(elemType, elemAttrs);
        }

        /// <summary>
        /// checks, if parameter to serialise does not contain more elements than allowed
        /// </summary>
        private void CheckBound(uint sequenceLength) {
            if (IdlSequenceAttribute.IsBounded(m_bound) && (sequenceLength > m_bound)) {
                throw new BAD_PARAM(3434, CompletionStatus.Completed_MayBe);
            }
        }

        internal override void Serialize(object actual, CdrOutputStream targetStream) {
            if (m_allowNull && actual == null) {
                // if null allowed, handle null as empty sequence.
                targetStream.WriteULong(0);
                return;
            }
            T[] array = (T[]) actual;
            // not allowed for a sequence:
            CheckActualNotNull(array);
            CheckBound((uint)array.Length);
            targetStream.WriteULong((uint)array.Length);
            
            if(isPrimitiveTypeArray) {
                if(typeof(T) == typeof(byte)) {                         // hack
                    targetStream.WriteOpaque((byte[])(object)array);
                    return;
                }
                targetStream.WritePrimitiveTypeArray(array);
            }
            else {
                // serialize sequence elements
                for (int i = 0; i < array.Length; i++) {
                    // it's more efficient to not determine serialise for each element; instead use cached ser
                    m_elementSerializer.Serialize(
                                                  array[i],
                                                  targetStream);
                }
            }
        }

        internal override object Deserialize(CdrInputStream sourceStream) {
            // mapped from an IDL-sequence
            uint nrOfElements = sourceStream.ReadULong();
            CheckBound(nrOfElements);

            if(isPrimitiveTypeArray) {
                if(typeof(T) == typeof(byte))                           // generics specialization could be very helpful =\
                    return sourceStream.ReadOpaque((int)nrOfElements);

                return sourceStream.ReadPrimitiveTypeArray<T>((int)nrOfElements);
            }

            T[] result = new T[nrOfElements];
            // serialize sequence elements
            for (int i = 0; i < nrOfElements; i++) {
                // it's more efficient to not determine serialise for each element; instead use cached ser
                object entry = m_elementSerializer.Deserialize(sourceStream);
                result[i] = (T) entry;
            }
            return result;
        }

        #endregion IMethods

    }

    /// <summary>serialises IDL-arrays</summary>
    internal class IdlArraySerializer : Serializer {

        #region IFields

        private int[] m_dimensions;
        private Type m_forTypeElemType;
        private Serializer m_elementSer;
        private bool m_allowNull;

        #endregion IFields
        #region IConstructors

        /// <summary>
        /// default constructor.
        /// </summary>
        /// <param name="forType">The idl array type</param>
        /// <param name="elemAttrs">The array element type attributes</param>
        /// <param name="dimensions">the fixed array dimensions</param>
        /// <param name="allowNull">Allow to serialize null or not. If allowed, serialize it
        /// as array with default elements.</param>
        /// <param name="serFactory">The serializer factory created this serializer</param>
        public IdlArraySerializer(Type forType, AttributeExtCollection elemAttributes, 
                                  int[] dimensions, bool allowNull,
                                  SerializerFactory serFactory) {
            m_dimensions = dimensions;
            m_forTypeElemType = forType.GetElementType();
            // element is not the same than the sequence -> therefore problems with recursion
            m_elementSer = serFactory.Create(m_forTypeElemType, elemAttributes);
            m_allowNull = allowNull;
        }

        #endregion IConstructors
        #region IMethods

        private void CheckInstanceDimensions(Array array) {
            if (m_dimensions.Length != array.Rank) {
                throw new BAD_PARAM(3436, CompletionStatus.Completed_MayBe);
            }
            for (int i = 0; i < m_dimensions.Length; i++) {
                if (m_dimensions[i] != array.GetLength(i)) {
                    throw new BAD_PARAM(3437, CompletionStatus.Completed_MayBe);
                }
            }
        } 


        private void SerialiseDimension(Array array, Serializer elementSer, CdrOutputStream targetStream,
                                        int[] indices, int currentDimension) {
            if (currentDimension == m_dimensions.Length) {
                object value = array.GetValue(indices);
                elementSer.Serialize(value, targetStream);
            } else {
                // the first dimension index in the array is increased slower than the second and so on ...
                for (int j = 0; j < m_dimensions[currentDimension]; j++) {
                    indices[currentDimension] = j;
                    SerialiseDimension(array, elementSer, targetStream, indices, currentDimension + 1);
                }
            }
        }

        internal override void Serialize(object actual, CdrOutputStream targetStream) {
            Array array = (Array) actual;
            // null not allowed for an idl array by default:
            if (m_allowNull && actual == null) {
                array = Array.CreateInstance(m_forTypeElemType,
                                             m_dimensions);
            }
            CheckActualNotNull(array);
            CheckInstanceDimensions(array);
            // get marshaller for elemtype
            SerialiseDimension(array, m_elementSer, targetStream, new int[m_dimensions.Length], 0);
        }

        private void DeserialiseDimension(Array array, Serializer elementSer, CdrInputStream sourceStream,
                                          int[] indices, int currentDimension) {
            if (currentDimension == array.Rank) {
                object entry = elementSer.Deserialize(sourceStream);
                array.SetValue(entry, indices);
            } else {
                // the first dimension index in the array is increased slower than the second and so on ...
                for (int j = 0; j < m_dimensions[currentDimension]; j++) {
                    indices[currentDimension] = j;
                    DeserialiseDimension(array, elementSer, sourceStream, indices, currentDimension + 1);
                }
            }
        }

        internal override object Deserialize(CdrInputStream sourceStream) {
            Array result = Array.CreateInstance(m_forTypeElemType, m_dimensions);
            // get marshaller for array element type
            DeserialiseDimension(result, m_elementSer, sourceStream, new int[m_dimensions.Length], 0);
            return result;
        }

        #endregion IMethods

    }

    /// <summary>serializes an instance as IDL-any</summary>
    internal class AnySerializer : Serializer {

        #region SFields

        private static Type s_supInterfaceAttrType = typeof(SupportedInterfaceAttribute);

        #endregion SFields
        #region IFields

        private TypeCodeSerializer m_typeCodeSer;
        private SerializerFactory m_serFactory;
        private bool m_formalIsAnyContainer;
        private bool m_unboxAnyToCls = true;

        #endregion IFields
        #region IConstructors

        internal AnySerializer(SerializerFactory serFactory, bool formalIsAnyContainer) : base() {
            m_serFactory = serFactory;
            m_formalIsAnyContainer = formalIsAnyContainer;
            m_typeCodeSer = new TypeCodeSerializer(serFactory);
        }

        #endregion IConstructors
        #region IMethods

        /// <summary>
        /// get the type to use for serialisation.
        /// </summary>
        /// <remarks>
        /// If a supported-interface attr is present on a MarshalByRefObject, then, the serialisation
        /// must be done for the interface type and not for the implementation type of the MarshalByRefObject,
        /// because otherwise the deserialisation would result into a problem, because only the sup-if type
        /// is know at deser.
        /// </remarks>
        /// <param name="actual"></param>
        /// <returns></returns>
        private Type DetermineTypeToUse(object actual) {
            if (actual == null) {
                return null;
            }
            Type result = actual.GetType();
            object[] attr = actual.GetType().GetCustomAttributes(s_supInterfaceAttrType, true);
            if (attr != null && attr.Length > 0) {
                SupportedInterfaceAttribute ifType = (SupportedInterfaceAttribute) attr[0];
                result = ifType.FromType;
            }
            return result;
        }


        internal override void Serialize(object actual,
                                       CdrOutputStream targetStream) {
            TypeCodeImpl typeCode = new NullTC();
            object actualToSerialise = actual;
            Type actualType = null;
            if (actual != null) {
                if (actual.GetType().Equals(ReflectionHelper.AnyType)) {
                    // use user defined type code
                    typeCode = ((Any)actual).Type as TypeCodeImpl;
                    if (typeCode == null) {
                        throw new INTERNAL(457, CompletionStatus.Completed_MayBe);
                    }
                    // type, which should be used to serialise value is determined by typecode!
                    if ((!(typeCode is NullTC)) && (!(typeCode is VoidTC))) {
                        actualType = Repository.GetTypeForTypeCode(typeCode); // no .NET type for null-tc, void-tc
                    }
                    actualToSerialise = ((Any)actual).ValueInternalRepresenation;
                } else {
                    // automatic type code creation
                    actualType = DetermineTypeToUse(actual);
                    typeCode = Repository.CreateTypeCodeForType(actualType, 
                                                                AttributeExtCollection.EmptyCollection);
                }
            }
            m_typeCodeSer.Serialize(typeCode, targetStream);
            if (actualType != null) {
                AttributeExtCollection typeAttributes = Repository.GetAttrsForTypeCode(typeCode);
                Serializer actualSer = 
                    m_serFactory.Create(actualType, typeAttributes);
                actualSer.Serialize(actualToSerialise, targetStream);
            }
        }

        internal override object Deserialize(CdrInputStream sourceStream) {
            TypeCodeImpl typeCode = 
                (omg.org.CORBA.TypeCodeImpl)m_typeCodeSer.Deserialize(sourceStream);
            object result;
            // when returning 0 in a mico-server for any, the typecode used is VoidTC
            if ((!(typeCode is NullTC)) && (!(typeCode is VoidTC))) {
                Type dotNetType = Repository.GetTypeForTypeCode(typeCode);
                AttributeExtCollection typeAttributes = Repository.GetAttrsForTypeCode(typeCode);
                Serializer actualSer = 
                    m_serFactory.Create(dotNetType, typeAttributes);
                result = actualSer.Deserialize(sourceStream);
                // e.g. for boxed valueTypes, do an unbox here; for non cls complian types, 
                // perform a conversion, if m_unboxAnyToCls.
                result = typeCode.ConvertToExternalRepresentation(result, m_unboxAnyToCls);
            } else {
                result = null;
            }
            if (!m_formalIsAnyContainer) {
                return result;
            } else {
                return new Any(result, typeCode);
            }
        }

        #endregion IMethods

    }

    /// <summary>serializes a typecode</summary>
    internal class TypeCodeSerializer : Serializer {

        #region IFields

        private SerializerFactory m_serFactory;

        #endregion IFields
        #region IConstructors

        internal TypeCodeSerializer(SerializerFactory serFactory) : base() {
            m_serFactory = serFactory;
        }

        #endregion IConstructors
        #region IMethods

        internal override object Deserialize(CdrInputStream sourceStream) {

            bool isIndirection;
            StreamPosition indirPos;
            uint kindVal = (uint)sourceStream.ReadInstanceOrIndirectionTag(out indirPos, 
                                                                           out isIndirection);
            if (!isIndirection) {

                omg.org.CORBA.TCKind kind = (omg.org.CORBA.TCKind)Enum.ToObject(typeof(omg.org.CORBA.TCKind),
                                                                                (int)kindVal);
                omg.org.CORBA.TypeCodeImpl result;
                switch(kind) {
                    case omg.org.CORBA.TCKind.tk_abstract_interface :
                        result = new omg.org.CORBA.AbstractIfTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_alias:
                        result = new omg.org.CORBA.AliasTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_any:
                        result = new omg.org.CORBA.AnyTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_array:
                        result = new omg.org.CORBA.ArrayTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_boolean:
                        result = new omg.org.CORBA.BooleanTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_char:
                        result = new omg.org.CORBA.CharTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_double:
                        result = new omg.org.CORBA.DoubleTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_enum:
                        result = new omg.org.CORBA.EnumTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_except:
                        result = new omg.org.CORBA.ExceptTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_fixed:
                        throw new NotImplementedException("fixed not implemented");
                    case omg.org.CORBA.TCKind.tk_float:
                        result = new omg.org.CORBA.FloatTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_local_interface :
                        result = new omg.org.CORBA.LocalIfTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_long:
                        result = new omg.org.CORBA.LongTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_longdouble:
                        throw new NotImplementedException("long double not implemented");
                    case omg.org.CORBA.TCKind.tk_longlong:
                        result = new omg.org.CORBA.LongLongTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_native:
                        throw new NotSupportedException("native not supported");
                    case omg.org.CORBA.TCKind.tk_null:
                        result = new omg.org.CORBA.NullTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_objref:
                        result = new omg.org.CORBA.ObjRefTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_octet:
                        result = new omg.org.CORBA.OctetTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_Principal:
                        throw new NotImplementedException("Principal not implemented");
                    case omg.org.CORBA.TCKind.tk_sequence:
                        result = new omg.org.CORBA.SequenceTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_short:
                        result = new omg.org.CORBA.ShortTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_string:
                        result = new omg.org.CORBA.StringTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_struct:
                        result = new omg.org.CORBA.StructTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_TypeCode:
                        result = new omg.org.CORBA.TypeCodeTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_ulong:
                        result = new omg.org.CORBA.ULongTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_ulonglong:
                        result = new omg.org.CORBA.ULongLongTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_union:
                        result = new omg.org.CORBA.UnionTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_ushort:
                        result = new omg.org.CORBA.UShortTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_value:
                        result = new omg.org.CORBA.ValueTypeTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_value_box:
                        result = new omg.org.CORBA.ValueBoxTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_void:
                        result = new omg.org.CORBA.VoidTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_wchar:
                        result = new omg.org.CORBA.WCharTC();
                        break;
                    case omg.org.CORBA.TCKind.tk_wstring:
                        result = new omg.org.CORBA.WStringTC();
                        break;
                    default:
                        // unknown typecode: kind
                        throw new omg.org.CORBA.BAD_PARAM(1504, 
                                                          omg.org.CORBA.CompletionStatus.Completed_MayBe);
                }
                // store indirection
                IndirectionInfo indirInfo = new IndirectionInfo(indirPos.GlobalPosition, 
                                                                IndirectionType.TypeCode,
                                                                IndirectionUsage.TypeCode);
                sourceStream.StoreIndirection(indirInfo, result);
                // read additional parts of typecode, if present
                result.ReadFromStream(sourceStream, m_serFactory);
                return result;
            } else {
                // resolve indirection:
                StreamPosition indirectionPosition = sourceStream.ReadIndirectionOffset();
                return sourceStream.GetObjectForIndir(new IndirectionInfo(indirectionPosition.GlobalPosition,
                                                                          IndirectionType.TypeCode,
                                                                          IndirectionUsage.TypeCode), 
                                                      true);
            }
        }

        internal override void Serialize(object actual, CdrOutputStream targetStream) {
            if (!(actual is omg.org.CORBA.TypeCodeImpl)) { 
                // typecode not serializable
                throw new omg.org.CORBA.INTERNAL(1654, omg.org.CORBA.CompletionStatus.Completed_MayBe);
            }
            omg.org.CORBA.TypeCodeImpl tcImpl = actual as omg.org.CORBA.TypeCodeImpl;
            if (!targetStream.IsPreviouslyMarshalled(tcImpl, IndirectionType.TypeCode, IndirectionUsage.TypeCode)) {
                tcImpl.WriteToStream(targetStream, m_serFactory);
            } else {
                targetStream.WriteIndirection(tcImpl);
            }
        }

        #endregion IMethods

    }

    /// <summary>serializes an instance as IDL abstract-interface</summary>
    internal class AbstractInterfaceSerializer : Serializer {

        #region IFields

        private Type m_forType;
        private Serializer m_objRefSer;
        private Serializer m_valueSer;

        #endregion IFields
        #region IConstructors

        internal AbstractInterfaceSerializer(Type forType, SerializerFactory serFactory,
                                             IiopUrlUtil iiopUrlUtil,
                                             bool objSerializeUsingConcreteType) {
            m_forType = forType;
            m_objRefSer = new ObjRefSerializer(forType, iiopUrlUtil, objSerializeUsingConcreteType);
            m_valueSer = new ValueObjectSerializer(forType, serFactory);
        }

        #endregion IConstructors
        #region IMethods

        internal override void Serialize(object actual, CdrOutputStream targetStream) {
            // if actual is null it shall be encoded as a valuetype: 15.3.7
            if ((actual != null) && (ClsToIdlMapper.IsMappedToConcreteInterface(actual.GetType()))) {
                targetStream.WriteBool(true); // an obj-ref is serialized
                m_objRefSer.Serialize(actual, targetStream);
            } else if ((actual == null) || (ClsToIdlMapper.IsMappedToConcreteValueType(actual.GetType()))) {
                targetStream.WriteBool(false); // a value-type is serialised
                m_valueSer.Serialize(actual, targetStream);
            } else {
                // actual value ( actual ) with type: 
                // actual.GetType() is not serializable for the formal type
                // formal
                throw new BAD_PARAM(6, CompletionStatus.Completed_MayBe);
            }
        }

        internal override object Deserialize(CdrInputStream sourceStream) {
            bool isObjRef = sourceStream.ReadBool();
            if (isObjRef) {
                Type formal = m_forType;
                if (formal.Equals(ReflectionHelper.ObjectType)) {
                    // if in interface only abstract interface base type is used, set formal now
                    // to base type of all objref for deserialization
                    formal = ReflectionHelper.MarshalByRefObjectType;
                }
                object result = m_objRefSer.Deserialize(sourceStream);
                return result;
            } else {
                object result = m_valueSer.Deserialize(sourceStream);
                return result;
            }
        }

        #endregion IMethods

    }


    /// <summary>serializes .NET exceptions as IDL-Exceptions</summary>
    internal class ExceptionSerializer : Serializer {

        #region IFields

        private SerializerFactory m_serFactory;

        #endregion IFields
        #region IConstructors

        public ExceptionSerializer(SerializerFactory serFactory) {
            m_serFactory = serFactory;
        }

        #endregion IConstructors
        #region IMethods

        internal override object Deserialize(CdrInputStream sourceStream) {
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
                FieldInfo[] fields = ReflectionHelper.GetAllDeclaredInstanceFieldsOrdered(exceptionType);
                foreach (FieldInfo field in fields) {
                    Serializer ser = m_serFactory.Create(field.FieldType, 
                                                         ReflectionHelper.GetCustomAttriutesForField(field, true));
                    object fieldVal = ser.Deserialize(sourceStream);
                    field.SetValue(exception, fieldVal);
                }
                return exception;
            }
        }

        internal override void Serialize(object actual, CdrOutputStream targetStream) {
            string repId = Repository.GetRepositoryID(actual.GetType());
            targetStream.WriteString(repId);

            if (actual.GetType().IsSubclassOf(typeof(AbstractCORBASystemException))) {
                // system exceptions are serialized specially, because no inheritance is possible for exceptions, see implementation of the system exceptions
                AbstractCORBASystemException sysEx = (AbstractCORBASystemException) actual;
                targetStream.WriteULong((uint)sysEx.Minor);
                targetStream.WriteULong((uint)sysEx.Status);
            } else {
                FieldInfo[] fields = ReflectionHelper.GetAllDeclaredInstanceFieldsOrdered(actual.GetType());
                foreach (FieldInfo field in fields) {
                    object fieldVal = field.GetValue(actual);
                    Serializer ser = m_serFactory.Create(field.FieldType,
                                                         ReflectionHelper.GetCustomAttriutesForField(field, true));
                    ser.Serialize(fieldVal, targetStream);
                }
            }
        }

        #endregion IMethods

    }

    /// <summary>
    /// Serializer decorator for handling custom mapping.
    /// </summary>
    internal class CustomMappingDecorator : Serializer {

        #region IFields

        private CustomMappingDesc m_customMappingUsed;
        private Serializer m_decorated;

        #endregion IFields
        #region IConstructors

        internal CustomMappingDecorator(CustomMappingDesc customMappingUsed, Serializer decorated) {
            m_decorated = decorated;
            m_customMappingUsed = customMappingUsed;
        }

        #endregion IConstructors
        #region IMethods

        internal override void Serialize(object actual, CdrOutputStream targetStream) {
            if (actual != null) {
                CustomMapperRegistry cReg = CustomMapperRegistry.GetSingleton();
                // custom mapping maps the actual object to an instance of 
                // the idl formal type.
                actual = cReg.CreateIdlForClsInstance(actual, m_customMappingUsed.IdlType);
            }
            m_decorated.Serialize(actual, targetStream);
        }

        internal override object Deserialize(CdrInputStream sourceStream) {
            object result =
                m_decorated.Deserialize(sourceStream);

            // check for plugged special mappings, e.g. CLS ArrayList -> java.util.ArrayList
            // --> if present, need to convert instance after deserialising
            if (result != null) {
                CustomMapperRegistry cReg = CustomMapperRegistry.GetSingleton();
                result = cReg.CreateClsForIdlInstance(result, m_customMappingUsed.ClsType);
            }
            return result;
        }

        #endregion IMethods
    }

}

#if UnitTest

namespace Ch.Elca.Iiop.Tests {

    using System.IO;
    using System.Runtime.Remoting.Channels;
    using NUnit.Framework;
    using Ch.Elca.Iiop.Marshalling;
    using Ch.Elca.Iiop.Cdr;
    using Ch.Elca.Iiop.Util;
    using omg.org.CORBA;

    public enum TestEnumWithValueNotIndexBI32 : int {
        a1 = 10, b1 = 20, c1 = 30
    }

    /// <summary>
    /// constains helper methods for serializer tests.
    /// </summary>
    public class AbstractSerializerTest {

        internal void GenericSerTest(Serializer ser, object actual, byte[] expected) {
            using (MemoryStream outStream = new MemoryStream()) {
                CdrOutputStream cdrOut = new CdrOutputStreamImpl(outStream, 0);
                cdrOut.WCharSet = (int)Ch.Elca.Iiop.Services.WCharSet.UTF16;
                ser.Serialize(actual, cdrOut);
                outStream.Seek(0, SeekOrigin.Begin);
                byte[] result = outStream.ToArray();
                Assert.AreEqual(expected, result, "value " + actual + " incorrectly serialized.");
            }
        }

        internal object GenericDeserForTest(Serializer ser, byte[] actual) {
            using (MemoryStream inStream = new MemoryStream()) {
                inStream.Write(actual, 0, actual.Length);
                inStream.Seek(0, SeekOrigin.Begin);
                CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(inStream);
                cdrIn.ConfigStream(0, new GiopVersion(1, 2));
                cdrIn.WCharSet = (int)Ch.Elca.Iiop.Services.WCharSet.UTF16;
                return ser.Deserialize(cdrIn);
            }
        }

        internal void GenericDeserTest(Serializer ser, byte[] actual, object expected,
                                      out object deserialized) {
            deserialized = GenericDeserForTest(ser, actual);
            Assert.AreEqual(expected, deserialized,"value " + expected + " not deserialized.");
        }

        internal void GenericDeserTest(Serializer ser, byte[] actual, object expected) {
            object deser;
            GenericDeserTest(ser, actual, expected, out deser);
        }

    }

    /// <summary>
    /// Unit-tests for the serialisers
    /// </summary>
    [TestFixture]
    public class SerialiserTestBaseTypes : AbstractSerializerTest {

        public SerialiserTestBaseTypes() {
        }

        [Test]
        public void TestByteSerialise() {
            Serializer ser = new ByteSerializer();
            GenericSerTest(ser, (byte)0, new byte[] { 0 });
            GenericSerTest(ser, (byte)11, new byte[] { 11 });
            GenericSerTest(ser, (byte)12, new byte[] { 12 });
            GenericSerTest(ser, (byte)225, new byte[] { 225 });
        }

        [Test]
        public void TestByteDeserialise() {
            Serializer ser = new ByteSerializer();
            GenericDeserTest(ser, new byte[] { 0 }, (byte)0);
            GenericDeserTest(ser, new byte[] { 11 }, (byte)11);
            GenericDeserTest(ser, new byte[] { 12 }, (byte)12);
            GenericDeserTest(ser, new byte[] { 225 }, (byte)225);
        }

        [Test]
        public void TestSByteSerialise() {
            Serializer ser = new SByteSerializer();
            GenericSerTest(ser, (sbyte)0, new byte[] { 0 });
            GenericSerTest(ser, (sbyte)11, new byte[] { 11 });
            GenericSerTest(ser, (sbyte)12, new byte[] { 12 });
            GenericSerTest(ser, (sbyte)-1, new byte[] { 0xFF });
            GenericSerTest(ser, SByte.MaxValue, new byte[] { 0x7F });
            GenericSerTest(ser, SByte.MinValue, new byte[] { 0x80 });
        }

        [Test]
        public void TestSByteDeserialise() {
            Serializer ser = new SByteSerializer();
            GenericDeserTest(ser, new byte[] { 0 }, (sbyte)0);
            GenericDeserTest(ser, new byte[] { 11 }, (sbyte)11);
            GenericDeserTest(ser, new byte[] { 12 }, (sbyte)12);
            GenericDeserTest(ser, new byte[] { 0xFF }, (sbyte)-1);
            GenericDeserTest(ser, new byte[] { 0x7F }, SByte.MaxValue);
            GenericDeserTest(ser, new byte[] { 0x80 }, SByte.MinValue);
        }

        [Test]
        public void TestInt16Serialise() {
            Serializer ser = new Int16Serializer();
            GenericSerTest(ser, (short)0, new byte[] { 0, 0 });
            GenericSerTest(ser, (short)225, new byte[] { 0, 225 });
            GenericSerTest(ser, unchecked((short)-1), new byte[] { 0xFF, 0xFF });
            GenericSerTest(ser, Int16.MaxValue, new byte[] { 0x7F, 0xFF });
            GenericSerTest(ser, Int16.MinValue, new byte[] { 0x80, 0x00 });
        }

        [Test]
        public void TestInt16Deserialise() {
            Serializer ser = new Int16Serializer();
            GenericDeserTest(ser, new byte[] { 0, 0 }, (short)0);
            GenericDeserTest(ser, new byte[] { 0, 225 }, (short)225);
            GenericDeserTest(ser, new byte[] { 0xFF, 0xFF }, unchecked((short)-1));
            GenericDeserTest(ser, new byte[] { 0x7F, 0xFF }, Int16.MaxValue);
            GenericDeserTest(ser, new byte[] { 0x80, 0x00 }, Int16.MinValue);
        }

        [Test]
        public void TestUInt16Serialise() {
            Serializer ser = new UInt16Serializer();
            GenericSerTest(ser, (ushort)0, new byte[] { 0, 0 });
            GenericSerTest(ser, (ushort)225, new byte[] { 0, 225 });
            GenericSerTest(ser, UInt16.MaxValue, new byte[] { 0xFF, 0xFF });
        }

        [Test]
        public void TestUInt16Deserialise() {
            Serializer ser = new UInt16Serializer();
            GenericDeserTest(ser, new byte[] { 0, 0 }, (ushort)0);
            GenericDeserTest(ser, new byte[] { 0, 225 }, (ushort)225);
            GenericDeserTest(ser, new byte[] { 0xFF, 0xFF }, UInt16.MaxValue);
        }

        [Test]
        public void TestInt32Serialise() {
            Serializer ser = new Int32Serializer();
            GenericSerTest(ser, (int)0, new byte[] { 0, 0, 0, 0 });
            GenericSerTest(ser, (int)225, new byte[] { 0, 0, 0, 225 });
            GenericSerTest(ser, unchecked((int)-1), new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
            GenericSerTest(ser, Int32.MaxValue, new byte[] { 0x7F, 0xFF, 0xFF, 0xFF });
            GenericSerTest(ser, Int32.MinValue, new byte[] { 0x80, 0x00, 0x00, 0x00 });
        }

        [Test]
        public void TestInt32Deserialise() {
            Serializer ser = new Int32Serializer();
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 0 }, (int)0);
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 225 }, (int)225);
            GenericDeserTest(ser, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, unchecked((int)-1));
            GenericDeserTest(ser, new byte[] { 0x7F, 0xFF, 0xFF, 0xFF }, Int32.MaxValue);
            GenericDeserTest(ser, new byte[] { 0x80, 0x00, 0x00, 0x00 }, Int32.MinValue);
        }

        [Test]
        public void TestUInt32Serialise() {
            Serializer ser = new UInt32Serializer();
            GenericSerTest(ser, (uint)0, new byte[] { 0, 0, 0, 0 });
            GenericSerTest(ser, (uint)225, new byte[] { 0, 0, 0, 225 });
            GenericSerTest(ser, UInt32.MaxValue, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
        }

        [Test]
        public void TestUInt32Deserialise() {
            Serializer ser = new UInt32Serializer();
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 0 }, (uint)0);
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 225 }, (uint)225);
            GenericDeserTest(ser, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, UInt32.MaxValue);
        }

        [Test]
        public void TestInt64Serialise() {
            Serializer ser = new Int64Serializer();
            GenericSerTest(ser, (long)0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 });
            GenericSerTest(ser, (long)225, new byte[] { 0, 0, 0, 0, 0, 0, 0, 225 });
            GenericSerTest(ser, unchecked((long)-1), new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
            GenericSerTest(ser, Int64.MaxValue, new byte[] { 0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
            GenericSerTest(ser, Int64.MinValue, new byte[] { 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
        }

        [Test]
        public void TestInt64Deserialise() {
            Serializer ser = new Int64Serializer();
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, (long)0);
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 0, 0, 0, 0, 225 }, (long)225);
            GenericDeserTest(ser, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, unchecked((long)-1));
            GenericDeserTest(ser, new byte[] { 0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, Int64.MaxValue);
            GenericDeserTest(ser, new byte[] { 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, Int64.MinValue);
        }

        [Test]
        public void TestUInt64Serialise() {
            Serializer ser = new UInt64Serializer();
            GenericSerTest(ser, (ulong)0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 });
            GenericSerTest(ser, (ulong)225, new byte[] { 0, 0, 0, 0, 0, 0, 0, 225 });
            GenericSerTest(ser, UInt64.MaxValue, 
                           new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
        }

        [Test]
        public void TestUInt64Deserialise() {
            Serializer ser = new UInt64Serializer();
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, (ulong)0);
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 0, 0, 0, 0, 225 }, (ulong)225);
            GenericDeserTest(ser, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, 
                             UInt64.MaxValue);
        }

        [Test]
        public void TestBooleanSerialise() {
            Serializer ser = new BooleanSerializer();
            GenericSerTest(ser, false, new byte[] { 0 });
            GenericSerTest(ser, true, new byte[] { 1 });
        }

        [Test]
        public void TestBooleanDeserialise() {
            Serializer ser = new BooleanSerializer();
            GenericDeserTest(ser, new byte[] { 0 }, false);
            GenericDeserTest(ser, new byte[] { 1 }, true);
        }

        [Test]
        [ExpectedException(typeof(BAD_PARAM))]
        public void TestBooleanDeserialiseInvalidValue() {
            Serializer ser = new BooleanSerializer();
            GenericDeserTest(ser, new byte[] { 2 }, null);
        }

        [Test]
        public void TestDeserializeEmptyStringNonWide() {
            Serializer ser = new StringSerializer(false, false);
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 1, 0 }, String.Empty);
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 0 }, String.Empty); // visibroker interop
        }

        [Test]
        public void TestSerializeEmptyStringNonWide() {
            Serializer ser = new StringSerializer(false, false);
            GenericSerTest(ser, String.Empty, new byte[] { 0, 0, 0, 1, 0 });
        }

        [Test]
        public void TestDeserializeEmptyStringWide() {
            Serializer ser = new StringSerializer(true, false);
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 0 }, String.Empty); // giop 1.2, no terminating null char
        }

        [Test]
        public void TestSerializeEmptyStringWide() {
            Serializer ser = new StringSerializer(true, false);
            GenericSerTest(ser, String.Empty, new byte[] { 0, 0, 0, 0 }); // giop 1.2, no terminating null char
        }

        [Test]
        public void TestDeserializeStringNonWide() {
            Serializer ser = new StringSerializer(false, false);
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 9, 73, 73, 79, 80, 46, 78, 69, 84, 0}, "IIOP.NET");
        }

        [Test]
        public void TestSerializeStringNonWide() {
            Serializer ser = new StringSerializer(false, false);
            GenericSerTest(ser, "IIOP.NET", new byte[] { 0, 0, 0, 9, 73, 73, 79, 80, 46, 78, 69, 84, 0});
        }

        [Test]
        public void TestDeserializeStringWide() {
            Serializer ser = new StringSerializer(true, false);
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 16, 0, 73, 0, 73, 0, 79, 0, 80, 0, 46, 0, 78, 0, 69, 0, 84 }, 
                             "IIOP.NET");
        }

        [Test]
        public void TestSerializeStringWide() {
            Serializer ser = new StringSerializer(true, false);
            GenericSerTest(ser, "IIOP.NET", 
                           new byte[] { 0, 0, 0, 16, 0, 73, 0, 73, 0, 79, 0, 80, 0, 46, 0, 78, 0, 69, 0, 84 });
        }

    }

    /// <summary>
    /// Serializer tests for WString / String Serialization.
    /// </summary>
    /// <remarks>move later all string ser tests here.</remarks>
    [TestFixture]
    public class AdvancedStringSerializerTest : AbstractSerializerTest {


        [Test]
        [ExpectedException(typeof(BAD_PARAM))]
        public void TestNotAllowNullStringForBasicStrings() {
            StringSerializer stringSer = new StringSerializer(false, false);
            using (MemoryStream outStream = new MemoryStream()) {
                CdrOutputStream cdrOut = new CdrOutputStreamImpl(outStream, 0);
                stringSer.Serialize(null, cdrOut);
            }
        }

        [Test]
        public void TestAllowNullStringForBasicStringsIfConfigured() {
            StringSerializer stringSer = new StringSerializer(false, true);
            GenericSerTest(stringSer, null, new byte[] { 0, 0, 0, 1, 0 });
        }

    }

    /// <summary>
    /// Serializer tests for enum / flags.
    /// </summary>
    [TestFixture]
    public class SerializerTestEnumFalgs : AbstractSerializerTest {

        private SerializerFactory m_serFactory;

        [SetUp]
        public void SetUp() {
            m_serFactory = new SerializerFactory();
            omg.org.IOP.CodecFactory codecFactory =
                new Ch.Elca.Iiop.Interception.CodecFactoryImpl(m_serFactory);
            omg.org.IOP.Codec codec = 
                codecFactory.create_codec(
                    new omg.org.IOP.Encoding(omg.org.IOP.ENCODING_CDR_ENCAPS.ConstVal, 1, 2));
            m_serFactory.Initalize(new SerializerFactoryConfig(), IiopUrlUtil.Create(codec));
        }

        private void FlagsGenericSerTest(Type flagsType, object actual, byte[] expected) {
            GenericSerTest(new FlagsSerializer(flagsType, m_serFactory),
                           actual, expected);
        }

        private void FlagsGenericDeserTest(Type flagsType, byte[] actual, object expected) {
            GenericDeserTest(new FlagsSerializer(flagsType, m_serFactory),
                             actual, expected);
        }

        [Test]
        public void TestIdlEnumSerialise() {
            Serializer ser = new IdlEnumSerializer(typeof(TestIdlEnumBI32));
            GenericSerTest(ser, TestIdlEnumBI32.IDL_A, new byte[] { 0, 0, 0, 0});
            GenericSerTest(ser, TestIdlEnumBI32.IDL_B, new byte[] { 0, 0, 0, 1});
            GenericSerTest(ser, TestIdlEnumBI32.IDL_C, new byte[] { 0, 0, 0, 2});
        }

        [Test]
        public void TestIdlEnumDeserialise() {
            Serializer ser = new IdlEnumSerializer(typeof(TestIdlEnumBI32));
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 0}, TestIdlEnumBI32.IDL_A);
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 1}, TestIdlEnumBI32.IDL_B);
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 2}, TestIdlEnumBI32.IDL_C);
        }

        [Test]
        public void TestEnumBI16Serialise() {
            Serializer ser = new EnumMapClsToIdlRangeSerializer(typeof(TestEnumBI16));
            GenericSerTest(ser, TestEnumBI16.a2, new byte[] { 0, 0, 0, 0});
            GenericSerTest(ser, TestEnumBI16.b2, new byte[] { 0, 0, 0, 1});
            GenericSerTest(ser, TestEnumBI16.c2, new byte[] { 0, 0, 0, 2});
        }

        [Test]
        public void TestEnumBI16Deserialise() {
            Serializer ser = new EnumMapClsToIdlRangeSerializer(typeof(TestEnumBI16));
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 0}, TestEnumBI16.a2);
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 1}, TestEnumBI16.b2);
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 2}, TestEnumBI16.c2);
        }

        [Test]
        public void TestEnumBI32Serialise() {
            Serializer ser = new EnumMapClsToIdlRangeSerializer(typeof(TestEnumBI32));
            GenericSerTest(ser, TestEnumBI32.a1, new byte[] { 0, 0, 0, 0});
            GenericSerTest(ser, TestEnumBI32.b1, new byte[] { 0, 0, 0, 1});
            GenericSerTest(ser, TestEnumBI32.c1, new byte[] { 0, 0, 0, 2});
        }

        [Test]
        public void TestEnumBI32Deserialise() {
            Serializer ser = new EnumMapClsToIdlRangeSerializer(typeof(TestEnumBI32));
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 0}, TestEnumBI32.a1);
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 1}, TestEnumBI32.b1);
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 2}, TestEnumBI32.c1);
        }

        [Test]
        public void TestEnumBI32ValueNotIndexSerialise() {
            Serializer ser = new EnumMapClsToIdlRangeSerializer(typeof(TestEnumWithValueNotIndexBI32));
            GenericSerTest(ser, TestEnumWithValueNotIndexBI32.a1, new byte[] { 0, 0, 0, 0});
            GenericSerTest(ser, TestEnumWithValueNotIndexBI32.b1, new byte[] { 0, 0, 0, 1});
            GenericSerTest(ser, TestEnumWithValueNotIndexBI32.c1, new byte[] { 0, 0, 0, 2});
        }

        [Test]
        public void TestEnumBI32ValueNotIndexDeserialise() {
            Serializer ser = new EnumMapClsToIdlRangeSerializer(typeof(TestEnumWithValueNotIndexBI32));
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 0}, TestEnumWithValueNotIndexBI32.a1);
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 1}, TestEnumWithValueNotIndexBI32.b1);
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 2}, TestEnumWithValueNotIndexBI32.c1);
        }

        [Test]
        public void TestEnumBBSerialise() {
            Serializer ser = new EnumMapClsToIdlRangeSerializer(typeof(TestEnumBB));
            GenericSerTest(ser, TestEnumBB.a4, new byte[] { 0, 0, 0, 0});
            GenericSerTest(ser, TestEnumBB.b4, new byte[] { 0, 0, 0, 1});
            GenericSerTest(ser, TestEnumBB.c4, new byte[] { 0, 0, 0, 2});
        }

        [Test]
        public void TestEnumBBDeserialise() {
            Serializer ser = new EnumMapClsToIdlRangeSerializer(typeof(TestEnumBB));
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 0}, TestEnumBB.a4);
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 1}, TestEnumBB.b4);
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 2}, TestEnumBB.c4);
        }

        [Test]
        public void TestEnumI64Serialise() {
            Serializer ser = new EnumMapClsToIdlRangeSerializer(typeof(TestEnumBI64));
            GenericSerTest(ser, TestEnumBI64.a3, new byte[] { 0, 0, 0, 0});
            GenericSerTest(ser, TestEnumBI64.b3, new byte[] { 0, 0, 0, 1});
            GenericSerTest(ser, TestEnumBI64.c3, new byte[] { 0, 0, 0, 2});
        }

        [Test]
        public void TestEnumI64Deserialise() {
            Serializer ser = new EnumMapClsToIdlRangeSerializer(typeof(TestEnumBI64));
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 0}, TestEnumBI64.a3);
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 1}, TestEnumBI64.b3);
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 2}, TestEnumBI64.c3);
        }

        [Test]
        public void TestEnumBUI16Serialise() {
            Serializer ser = new EnumMapClsToIdlRangeSerializer(typeof(TestEnumBUI16));
            GenericSerTest(ser, TestEnumBUI16.a6, new byte[] { 0, 0, 0, 0});
            GenericSerTest(ser, TestEnumBUI16.b6, new byte[] { 0, 0, 0, 1});
            GenericSerTest(ser, TestEnumBUI16.c6, new byte[] { 0, 0, 0, 2});
        }

        [Test]
        public void TestEnumBUI16Deserialise() {
            Serializer ser = new EnumMapClsToIdlRangeSerializer(typeof(TestEnumBUI16));
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 0}, TestEnumBUI16.a6);
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 1}, TestEnumBUI16.b6);
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 2}, TestEnumBUI16.c6);
        }

        [Test]
        public void TestEnumBUI32Serialise() {
            Serializer ser = new EnumMapClsToIdlRangeSerializer(typeof(TestEnumBUI32));
            GenericSerTest(ser, TestEnumBUI32.a7, new byte[] { 0, 0, 0, 0});
            GenericSerTest(ser, TestEnumBUI32.b7, new byte[] { 0, 0, 0, 1});
            GenericSerTest(ser, TestEnumBUI32.c7, new byte[] { 0, 0, 0, 2});
        }

        [Test]
        public void TestEnumBUI32Deserialise() {
            Serializer ser = new EnumMapClsToIdlRangeSerializer(typeof(TestEnumBUI32));
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 0}, TestEnumBUI32.a7);
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 1}, TestEnumBUI32.b7);
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 2}, TestEnumBUI32.c7);
        }

        [Test]
        public void TestEnumBSBSerialise() {
            Serializer ser = new EnumMapClsToIdlRangeSerializer(typeof(TestEnumBSB));
            GenericSerTest(ser, TestEnumBSB.a5, new byte[] { 0, 0, 0, 0});
            GenericSerTest(ser, TestEnumBSB.b5, new byte[] { 0, 0, 0, 1});
            GenericSerTest(ser, TestEnumBSB.c5, new byte[] { 0, 0, 0, 2});
        }

        [Test]
        public void TestEnumBSBDeserialise() {
            Serializer ser = new EnumMapClsToIdlRangeSerializer(typeof(TestEnumBSB));
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 0}, TestEnumBSB.a5);
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 1}, TestEnumBSB.b5);
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 2}, TestEnumBSB.c5);
        }

        [Test]
        public void TestEnumUI64Serialise() {
            Serializer ser = new EnumMapClsToIdlRangeSerializer(typeof(TestEnumBUI64));
            GenericSerTest(ser, TestEnumBUI64.a8, new byte[] { 0, 0, 0, 0});
            GenericSerTest(ser, TestEnumBUI64.b8, new byte[] { 0, 0, 0, 1});
            GenericSerTest(ser, TestEnumBUI64.c8, new byte[] { 0, 0, 0, 2});
        }

        [Test]
        public void TestEnumUI64Deserialise() {
            Serializer ser = new EnumMapClsToIdlRangeSerializer(typeof(TestEnumBUI64));
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 0}, TestEnumBUI64.a8);
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 1}, TestEnumBUI64.b8);
            GenericDeserTest(ser, new byte[] { 0, 0, 0, 2}, TestEnumBUI64.c8);
        }

        [Test]
        public void TestFlagsBI16Serialise() {
            FlagsGenericSerTest(typeof(TestFlagsBI16), TestFlagsBI16.a2, new byte[] { 0, 0});
            FlagsGenericSerTest(typeof(TestFlagsBI16), TestFlagsBI16.b2, new byte[] { 0, 1});
            FlagsGenericSerTest(typeof(TestFlagsBI16), TestFlagsBI16.c2, new byte[] { 0, 2});
        }

        [Test]
        public void TestFlagsBI16Deserialise() {
            FlagsGenericDeserTest(typeof(TestFlagsBI16), new byte[] { 0, 0}, TestFlagsBI16.a2);
            FlagsGenericDeserTest(typeof(TestFlagsBI16), new byte[] { 0, 1}, TestFlagsBI16.b2);
            FlagsGenericDeserTest(typeof(TestFlagsBI16), new byte[] { 0, 2}, TestFlagsBI16.c2);
        }

        [Test]
        public void TestFlagsBI32Serialise() {
            FlagsGenericSerTest(typeof(TestFlagsBI32), TestFlagsBI32.a1, new byte[] { 0, 0, 0, 0});
            FlagsGenericSerTest(typeof(TestFlagsBI32), TestFlagsBI32.b1, new byte[] { 0, 0, 0, 1});
            FlagsGenericSerTest(typeof(TestFlagsBI32), TestFlagsBI32.c1, new byte[] { 0, 0, 0, 2});
        }

        [Test]
        public void TestFlagsBI32Deserialise() {
            FlagsGenericDeserTest(typeof(TestFlagsBI32), new byte[] { 0, 0, 0, 0}, TestFlagsBI32.a1);
            FlagsGenericDeserTest(typeof(TestFlagsBI32), new byte[] { 0, 0, 0, 1}, TestFlagsBI32.b1);
            FlagsGenericDeserTest(typeof(TestFlagsBI32), new byte[] { 0, 0, 0, 2}, TestFlagsBI32.c1);
        }

        [Test]
        public void TestFlagsBBSerialise() {
            FlagsGenericSerTest(typeof(TestFlagsBB), TestFlagsBB.a4, new byte[] { 0});
            FlagsGenericSerTest(typeof(TestFlagsBB), TestFlagsBB.b4, new byte[] { 1});
            FlagsGenericSerTest(typeof(TestFlagsBB), TestFlagsBB.c4, new byte[] { 2});
        }

        [Test]
        public void TestFlagsBBDeserialise() {
            FlagsGenericDeserTest(typeof(TestFlagsBB), new byte[] { 0}, TestFlagsBB.a4);
            FlagsGenericDeserTest(typeof(TestFlagsBB), new byte[] { 1}, TestFlagsBB.b4);
            FlagsGenericDeserTest(typeof(TestFlagsBB), new byte[] { 2}, TestFlagsBB.c4);
        }

        [Test]
        public void TestFlagsI64Serialise() {
            FlagsGenericSerTest(typeof(TestFlagsBI64), TestFlagsBI64.a3, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0});
            FlagsGenericSerTest(typeof(TestFlagsBI64), TestFlagsBI64.b3, new byte[] { 0, 0, 0, 0, 0, 0, 0, 1});
            FlagsGenericSerTest(typeof(TestFlagsBI64), TestFlagsBI64.c3, new byte[] { 0, 0, 0, 0, 0, 0, 0, 2});
        }

        [Test]
        public void TestFlagsI64Deserialise() {
            FlagsGenericDeserTest(typeof(TestFlagsBI64), new byte[] { 0, 0, 0, 0, 0, 0, 0, 0}, TestFlagsBI64.a3);
            FlagsGenericDeserTest(typeof(TestFlagsBI64), new byte[] { 0, 0, 0, 0, 0, 0, 0, 1}, TestFlagsBI64.b3);
            FlagsGenericDeserTest(typeof(TestFlagsBI64), new byte[] { 0, 0, 0, 0, 0, 0, 0, 2}, TestFlagsBI64.c3);
        }

        [Test]
        public void TestFlagsBUI16Serialise() {
            FlagsGenericSerTest(typeof(TestFlagsBUI16), TestFlagsBUI16.a6, new byte[] { 0, 0});
            FlagsGenericSerTest(typeof(TestFlagsBUI16), TestFlagsBUI16.b6, new byte[] { 0, 1});
            FlagsGenericSerTest(typeof(TestFlagsBUI16), TestFlagsBUI16.c6, new byte[] { 0, 2});
        }

        [Test]
        public void TestFlagsBUI16Deserialise() {
            FlagsGenericDeserTest(typeof(TestFlagsBUI16), new byte[] { 0, 0}, TestFlagsBUI16.a6);
            FlagsGenericDeserTest(typeof(TestFlagsBUI16), new byte[] { 0, 1}, TestFlagsBUI16.b6);
            FlagsGenericDeserTest(typeof(TestFlagsBUI16), new byte[] { 0, 2}, TestFlagsBUI16.c6);
        }

        [Test]
        public void TestFlagsBUI32Serialise() {
            FlagsGenericSerTest(typeof(TestFlagsBUI32), TestFlagsBUI32.a7, new byte[] { 0, 0, 0, 0});
            FlagsGenericSerTest(typeof(TestFlagsBUI32), TestFlagsBUI32.b7, new byte[] { 0, 0, 0, 1});
            FlagsGenericSerTest(typeof(TestFlagsBUI32), TestFlagsBUI32.c7, new byte[] { 0, 0, 0, 2});
        }

        [Test]
        public void TestFlagsBUI32Deserialise() {
            FlagsGenericDeserTest(typeof(TestFlagsBUI32), new byte[] { 0, 0, 0, 0}, TestFlagsBUI32.a7);
            FlagsGenericDeserTest(typeof(TestFlagsBUI32), new byte[] { 0, 0, 0, 1}, TestFlagsBUI32.b7);
            FlagsGenericDeserTest(typeof(TestFlagsBUI32), new byte[] { 0, 0, 0, 2}, TestFlagsBUI32.c7);
        }

        [Test]
        public void TestFlagsBSBSerialise() {
            FlagsGenericSerTest(typeof(TestFlagsBSB), TestFlagsBSB.a5, new byte[] { 0});
            FlagsGenericSerTest(typeof(TestFlagsBSB), TestFlagsBSB.b5, new byte[] { 1});
            FlagsGenericSerTest(typeof(TestFlagsBSB), TestFlagsBSB.c5, new byte[] { 2});
        }

        [Test]
        public void TestFlagsBSBDeserialise() {
            FlagsGenericDeserTest(typeof(TestFlagsBSB), new byte[] { 0 }, TestFlagsBSB.a5);
            FlagsGenericDeserTest(typeof(TestFlagsBSB), new byte[] { 1 }, TestFlagsBSB.b5);
            FlagsGenericDeserTest(typeof(TestFlagsBSB), new byte[] { 2 }, TestFlagsBSB.c5);
        }

        [Test]
        public void TestFlagsUI64Serialise() {
            FlagsGenericSerTest(typeof(TestFlagsBUI64), TestFlagsBUI64.a8, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0});
            FlagsGenericSerTest(typeof(TestFlagsBUI64), TestFlagsBUI64.b8, new byte[] { 0, 0, 0, 0, 0, 0, 0, 1});
            FlagsGenericSerTest(typeof(TestFlagsBUI64), TestFlagsBUI64.c8, new byte[] { 0, 0, 0, 0, 0, 0, 0, 2});
        }

        [Test]
        public void TestFlagsUI64Deserialise() {
            FlagsGenericDeserTest(typeof(TestFlagsBUI64), new byte[] { 0, 0, 0, 0, 0, 0, 0, 0}, TestFlagsBUI64.a8);
            FlagsGenericDeserTest(typeof(TestFlagsBUI64), new byte[] { 0, 0, 0, 0, 0, 0, 0, 1}, TestFlagsBUI64.b8);
            FlagsGenericDeserTest(typeof(TestFlagsBUI64), new byte[] { 0, 0, 0, 0, 0, 0, 0, 2}, TestFlagsBUI64.c8);
        }
    }

    /// <summary>
    /// SerializerTests for obj ref.
    /// </summary>
    [TestFixture]
    public class SerializerTestObjRef : AbstractSerializerTest {

        private omg.org.IOP.Codec m_codec;
        private SerializerFactory m_serFactory;
        private IiopUrlUtil m_iiopUrlUtil;

        [SetUp]
        public void SetUp() {
            m_serFactory = 
                new SerializerFactory();
            omg.org.IOP.CodecFactory codecFactory =
                new Ch.Elca.Iiop.Interception.CodecFactoryImpl(m_serFactory);
            m_codec = 
                codecFactory.create_codec(
                    new omg.org.IOP.Encoding(omg.org.IOP.ENCODING_CDR_ENCAPS.ConstVal,
                                             1, 2));
            m_iiopUrlUtil = 
                IiopUrlUtil.Create(m_codec, new object[] { 
                    Services.CodeSetService.CreateDefaultCodesetComponent(m_codec)});
            m_serFactory.Initalize(new SerializerFactoryConfig(), m_iiopUrlUtil);
        }

        [Test]
        public void TestIorDeserialisation() {
            IiopClientChannel testChannel = new IiopClientChannel();
            ChannelServices.RegisterChannel(testChannel, false);

            try {
                byte[] testIor = new byte[] {
                    0x00, 0x00, 0x00, 0x28, 0x49, 0x44, 0x4C, 0x3A,
                    0x6F, 0x6D, 0x67, 0x2E, 0x6F, 0x72, 0x67, 0x2F,
                    0x43, 0x6F, 0x73, 0x4E, 0x61, 0x6D, 0x69, 0x6E,
                    0x67, 0x2F, 0x4E, 0x61, 0x6D, 0x69, 0x6E, 0x67,
                    0x43, 0x6F, 0x6E, 0x74, 0x65, 0x78, 0x74, 0x3A,
                    0x31, 0x2E, 0x30, 0x00, 0x00, 0x00, 0x00, 0x01,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x74,
                    0x00, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x0A,
                    0x31, 0x32, 0x37, 0x2E, 0x30, 0x2E, 0x30, 0x2E,
                    0x31, 0x00, 0x04, 0x19, 0x00, 0x00, 0x00, 0x30,
                    0xAF, 0xAB, 0xCB, 0x00, 0x00, 0x00, 0x00, 0x22,
                    0x00, 0x00, 0x03, 0xE8, 0x00, 0x00, 0x00, 0x01,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01,
                    0x00, 0x00, 0x00, 0x0C, 0x4E, 0x61, 0x6D, 0x65,
                    0x53, 0x65, 0x72, 0x76, 0x69, 0x63, 0x65, 0x00,
                    0x00, 0x00, 0x00, 0x03, 0x4E, 0x43, 0x30, 0x0A,
                    0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
                    0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x02,
                    0x05, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x20,
                    0x00, 0x01, 0x01, 0x09, 0x00, 0x00, 0x00, 0x01,
                    0x00, 0x01, 0x01, 0x00
                };
                MemoryStream inStream = new MemoryStream(testIor);
                CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(inStream);
                cdrIn.ConfigStream(0, new GiopVersion(1, 2));

                Serializer ser = new ObjRefSerializer(typeof(omg.org.CosNaming.NamingContext),
                                                      m_iiopUrlUtil, false);
                object result = ser.Deserialize(cdrIn);
                Assert.NotNull(result, "not correctly deserialised proxy for ior");
                Assert.IsTrue(RemotingServices.IsTransparentProxy(result));
                
                Assert.AreEqual("IOR:000000000000002849444C3A6F6D672E6F72672F436F734E616D696E672F4E616D696E67436F6E746578743A312E3000000000010000000000000074000102000000000A3132372E302E302E3100041900000030AFABCB0000000022000003E80000000100000000000000010000000C4E616D655365727669636500000000034E43300A0000000100000001000000200000000000010001000000020501000100010020000101090000000100010100",
                                       RemotingServices.GetObjectUri((MarshalByRefObject)result));
            }
            finally {
                ChannelServices.UnregisterChannel(testChannel);
            }

        }

    }

    /// <summary>
    /// Serializer tests for any.
    /// </summary>
    [TestFixture]
    public class SerializerTestAny : AbstractSerializerTest {

        private SerializerFactory m_serFactory;

        [SetUp]
        public void SetUp() {
            m_serFactory =
                new SerializerFactory();
            omg.org.IOP.CodecFactory codecFactory =
                new Ch.Elca.Iiop.Interception.CodecFactoryImpl(m_serFactory);
            omg.org.IOP.Codec codec = 
                codecFactory.create_codec(
                    new omg.org.IOP.Encoding(omg.org.IOP.ENCODING_CDR_ENCAPS.ConstVal,
                                             1, 2));
            IiopUrlUtil iiopUrlUtil = 
                IiopUrlUtil.Create(codec, new object[] { 
                    Services.CodeSetService.CreateDefaultCodesetComponent(codec)});
            m_serFactory.Initalize(new SerializerFactoryConfig(), iiopUrlUtil);
        }

        [Test]
        public void TestSerLongAsAnyNoAnyContainer() {
            int val = 2;
            AnySerializer anySer = new AnySerializer(m_serFactory, false);
            GenericSerTest(anySer, val, new byte[] { 0, 0, 0, 3, 0, 0, 0, 2 });
        }

        [Test]
        public void TestDeSerLongAsAnyNoAnyContainer() {
            AnySerializer anySer = new AnySerializer(m_serFactory, false);
            GenericDeserTest(anySer, new byte[] { 0, 0, 0, 3, 0, 0, 0, 2 }, (int)2);
        }

        [Test]
        public void TestSerULongAsAnyNoAnyContainer() {
            uint val = 4;
            AnySerializer anySer = new AnySerializer(m_serFactory, false);
            GenericSerTest(anySer, val, new byte[] { 0, 0, 0, 5, 0, 0, 0, 4 });
        }

        [Test]
        public void TestDeSerULongAsAnyNoAnyContainer() {
            AnySerializer anySer = new AnySerializer(m_serFactory, false);
            object deser;
            GenericDeserTest(anySer, new byte[] { 0, 0, 0, 5, 0, 0, 0, 4 }, (int)4,
                             out deser);
            Assert.AreEqual(ReflectionHelper.Int32Type,
                                   deser.GetType(), "deser type");
        }

        [Test]
        public void TestSerSbyteAsAnyNoAnyContainer() {
            sbyte val = 2;
            AnySerializer anySer = new AnySerializer(m_serFactory, false);
            GenericSerTest(anySer, val, new byte[] { 0, 0, 0, 10, 2 });
        }

        [Test]
        public void TestDeSerSByteAsAnyNoAnyContainer() {
            AnySerializer anySer = new AnySerializer(m_serFactory, false);
            object deser;
            GenericDeserTest(anySer, new byte[] { 0, 0, 0, 10, 2 }, (sbyte)2,
                             out deser);
            Assert.AreEqual(ReflectionHelper.ByteType,
                                   deser.GetType(), "deser type");
        }

        [Test]
        public void TestSerBoxedStringAsAnyNoAnyContainer1() {
            bool oldUseWideChar = MappingConfiguration.Instance.UseWideCharByDefault;
            MappingConfiguration.Instance.UseWideCharByDefault = true;

            string val = "test";
            AnySerializer anySer = new AnySerializer(m_serFactory, false);
            GenericSerTest(anySer, val, new byte[] {   0,   0,   0,  30,                     // tc-kind
                                                       0,   0,   0,  72,                     // encap length
                                                       0,   0,   0,   0,                     // flags
                                                       0,   0,   0,  35,  73,  68,  76,  58, // rep-id:          <35>"IDL:"
                                                     111, 109, 103,  46, 111, 114, 103,  47, //                  "omg.org/"
                                                      67,  79,  82,  66,  65,  47,  87,  83, //                  "CORBA/WS"
                                                     116, 114, 105, 110, 103,  86,  97, 108, //                  "tringVal"
                                                     117, 101,  58,  49,  46,  48,   0,   0, //                  "ue:1.0\0"<padding>
                                                       0,   0,   0,  13,  87,  83, 116, 114, // name
                                                     105, 110, 103,  86,  97, 108, 117, 101,
                                                       0,   0,   0,   0,   0,   0,   0,  27, // boxed value tc for wstring
                                                       0,   0,   0,   0, 127, 255, 255,   2, // bound, value
                                                       0,   0,   0,  35,  73,  68,  76,  58, // rep-id of value: <35>"IDL:"
                                                     111, 109, 103,  46, 111, 114, 103,  47, //                  "omg.org/"
                                                      67,  79,  82,  66,  65,  47,  87,  83, //                  "CORBA/WS"
                                                     116, 114, 105, 110, 103,  86,  97, 108, //                  "tringVal"
                                                     117, 101,  58,  49,  46,  48,   0,   0, //                  "ue:1.0\0"<padding>
                                                       0,   0,   0,   8,   0, 116,   0, 101, // value:           <8>"test"
                                                       0, 115,   0, 116 });                  // "For GIOP version 1.2 and 1.3 a wstring is not terminated by a null character"

            MappingConfiguration.Instance.UseWideCharByDefault = oldUseWideChar;
        }

        [Test]
        public void TestSerBoxedStringAsAnyNoAnyContainer2() {
            bool oldUseWideChar = MappingConfiguration.Instance.UseWideCharByDefault;
            MappingConfiguration.Instance.UseWideCharByDefault = false;

            string val = "test";
            AnySerializer anySer = new AnySerializer(m_serFactory, false);
            GenericSerTest(anySer, val, new byte[] {   0,   0,   0,  30,                     // tc-kind
                                                       0,   0,   0,  68,                     // encap length
                                                       0,   0,   0,   0,                     // flags
                                                       0,   0,   0,  34,  73,  68,  76,  58, // rep-id:          <34>"IDL:"
                                                     111, 109, 103,  46, 111, 114, 103,  47, //                  "omg.org/"
                                                      67,  79,  82,  66,  65,  47,  83, 116, //                  "CORBA/S"
                                                     114, 105, 110, 103,  86,  97, 108, 117, //                  "tringVal"
                                                     101,  58,  49,  46,  48,   0,   0,   0, //                  "ue:1.0\0"<padding>
                                                       0,   0,   0,  12,  83, 116, 114, 105, // name:            <12>"Stri"
                                                     110, 103,  86,  97, 108, 117, 101,   0, //                  "ngValue"
                                                       0,   0,   0,  18,                     // boxed value tc for string
                                                       0,   0,   0,   0, 127, 255, 255,   2, // bound, value
                                                       0,   0,   0,  34,  73,  68,  76,  58, // rep-id of value: <34>"IDL:"
                                                     111, 109, 103,  46, 111, 114, 103,  47, //                  "omg.org/"
                                                      67,  79,  82,  66,  65,  47,  83, 116, //                  "CORBA/S"
                                                     114, 105, 110, 103,  86,  97, 108, 117, //                  "tringVal"
                                                     101,  58,  49,  46,  48,   0,   0,   0, //                  "ue:1.0\0"<padding>
                                                       0,   0,   0,   5, 116, 101, 115, 116, // value:           <4>"test"
                                                       0                                     //                  "\0"
                                                     });                                     // "The string contents include a single terminating null character."

            MappingConfiguration.Instance.UseWideCharByDefault = oldUseWideChar;
        }

        [Test]
        public void TestDeSerBoxedStringAsAnyNoAnyContainer1() {
            AnySerializer anySer = new AnySerializer(m_serFactory, false);
            object deser;
            GenericDeserTest(anySer, new byte[] {      0,   0,   0,  30,                     // tc-kind
                                                       0,   0,   0,  72,                     // encap length
                                                       0,   0,   0,   0,                     // flags
                                                       0,   0,   0,  35,  73,  68,  76,  58, // rep-id:          <35>"IDL:"
                                                     111, 109, 103,  46, 111, 114, 103,  47, //                  "omg.org/"
                                                      67,  79,  82,  66,  65,  47,  87,  83, //                  "CORBA/WS"
                                                     116, 114, 105, 110, 103,  86,  97, 108, //                  "tringVal"
                                                     117, 101,  58,  49,  46,  48,   0,   0, //                  "ue:1.0\0"<padding>
                                                       0,   0,   0,  13,  87,  83, 116, 114, // name
                                                     105, 110, 103,  86,  97, 108, 117, 101,
                                                       0,   0,   0,   0,   0,   0,   0,  27, // boxed value tc for wstring
                                                       0,   0,   0,   0, 127, 255, 255,   2, // bound, value
                                                       0,   0,   0,  35,  73,  68,  76,  58, // rep-id of value: <35>"IDL:"
                                                     111, 109, 103,  46, 111, 114, 103,  47, //                  "omg.org/"
                                                      67,  79,  82,  66,  65,  47,  87,  83, //                  "CORBA/WS"
                                                     116, 114, 105, 110, 103,  86,  97, 108, //                  "tringVal"
                                                     117, 101,  58,  49,  46,  48,   0,   0, //                  "ue:1.0\0"<padding>
                                                       0,   0,   0,   8,   0, 116,   0, 101, // value:           <8>"test"
                                                       0, 115,   0, 116 },                  // "For GIOP version 1.2 and 1.3 a wstring is not terminated by a null character"
                             "test",
                             out deser);
            Assert.AreEqual(ReflectionHelper.StringType,
                                   deser.GetType(), "deser type");
        }

        [Test]
        public void TestDeSerBoxedStringAsAnyNoAnyContainer2() {
            AnySerializer anySer = new AnySerializer(m_serFactory, false);
            object deser;
            GenericDeserTest(anySer, new byte[] {      0,   0,   0,  30,                     // tc-kind
                                                       0,   0,   0,  68,                     // encap length
                                                       0,   0,   0,   0,                     // flags
                                                       0,   0,   0,  34,  73,  68,  76,  58, // rep-id:          <34>"IDL:"
                                                     111, 109, 103,  46, 111, 114, 103,  47, //                  "omg.org/"
                                                      67,  79,  82,  66,  65,  47,  83, 116, //                  "CORBA/S"
                                                     114, 105, 110, 103,  86,  97, 108, 117, //                  "tringVal"
                                                     101,  58,  49,  46,  48,   0,   0,   0, //                  "ue:1.0\0"<padding>
                                                       0,   0,   0,  12,  83, 116, 114, 105, // name:            <12>"Stri"
                                                     110, 103,  86,  97, 108, 117, 101,   0, //                  "ngValue"
                                                       0,   0,   0,  18,                     // boxed value tc for string
                                                       0,   0,   0,   0, 127, 255, 255,   2, // bound, value
                                                       0,   0,   0,  34,  73,  68,  76,  58, // rep-id of value: <34>"IDL:"
                                                     111, 109, 103,  46, 111, 114, 103,  47, //                  "omg.org/"
                                                      67,  79,  82,  66,  65,  47,  83, 116, //                  "CORBA/S"
                                                     114, 105, 110, 103,  86,  97, 108, 117, //                  "tringVal"
                                                     101,  58,  49,  46,  48,   0,   0,   0, //                  "ue:1.0\0"<padding>
                                                       0,   0,   0,   5, 116, 101, 115, 116, // value:           <4>"test"
                                                       0                                     //                  "\0"
                                                     },                                      // "The string contents include a single terminating null character."
                             "test",
                             out deser);
            Assert.AreEqual(ReflectionHelper.StringType,
                                   deser.GetType(), "deser type");
        }

        [Test]
        public void TestSerNullAsAnyNoAnyContainer() {
            AnySerializer anySer = new AnySerializer(m_serFactory, false);
            object val = null;
            GenericSerTest(anySer, val, new byte[] { 0, 0, 0, 0 });
        }

        [Test]
        public void TestDeSerNullAsAnyNoAnyContainer() {
            AnySerializer anySer = new AnySerializer(m_serFactory, false);
            object val = null;
            GenericDeserTest(anySer, new byte[] { 0, 0, 0, 0 }, val);
        }

        [Test]
        public void TestSerLongAsAnyAnyContainer() {
            Any any = new Any((int)2);
            AnySerializer anySer = new AnySerializer(m_serFactory, true);
            GenericSerTest(anySer, any, new byte[] { 0, 0, 0, 3, 0, 0, 0, 2 });
        }

        [Test]
        public void TestDeSerLongAsAnyAnyContainer() {
            AnySerializer anySer = new AnySerializer(m_serFactory, true);
            Any any = new Any((int)2);
            GenericDeserTest(anySer, new byte[] { 0, 0, 0, 3, 0, 0, 0, 2 }, any);
        }

        [Test]
        public void TestSerULongAsAnyAnyContainer() {
            AnySerializer anySer = new AnySerializer(m_serFactory, true);
            Any any = new Any((uint)4);
            GenericSerTest(anySer, any, new byte[] { 0, 0, 0, 5, 0, 0, 0, 4 });
        }

        [Test]
        public void TestDeSerULongAsAnyAnyContainer() {
            AnySerializer anySer = new AnySerializer(m_serFactory, true);
            Any any = new Any((uint)4);
            GenericDeserTest(anySer, new byte[] { 0, 0, 0, 5, 0, 0, 0, 4 }, any);
        }

        [Test]
        public void TestSerNullAsAnyAnyContainer() {
            AnySerializer anySer = new AnySerializer(m_serFactory, true);
            Any any = new Any(null);
            GenericSerTest(anySer, any, new byte[] { 0, 0, 0, 0 });
        }

        [Test]
        public void TestDeSerNullAsAnyAnyContainer() {
            AnySerializer anySer = new AnySerializer(m_serFactory, true);
            Any any = new Any(null);
            GenericDeserTest(anySer, new byte[] { 0, 0, 0, 0 }, any);
        }

        [Test]
        public void TestSerNullAsAnyWithVoidTcAnyContainer() {
            AnySerializer anySer = new AnySerializer(m_serFactory, true);
            Any any = new Any(null, new VoidTC());
            GenericSerTest(anySer, any, new byte[] { 0, 0, 0, 1 });
        }

        [Test]
        public void TestDeSerNullAsAnyWithVoidTcAnyContainer() {
            AnySerializer anySer = new AnySerializer(m_serFactory, true);
            Any any = new Any(null, new VoidTC());
            GenericDeserTest(anySer, new byte[] { 0, 0, 0, 1 }, any);
        }

        [Test]
        public void TestSerBoxedStringAsAnyAnyContainer1() {
            bool oldUseWideChar = MappingConfiguration.Instance.UseWideCharByDefault;
            MappingConfiguration.Instance.UseWideCharByDefault = true;

            Any val = new Any("test");
            AnySerializer anySer = new AnySerializer(m_serFactory, true);
            GenericSerTest(anySer, val, new byte[] {   0,   0,   0,  30,                     // tc-kind
                                                       0,   0,   0,  72,                     // encap length
                                                       0,   0,   0,   0,                     // flags
                                                       0,   0,   0,  35,  73,  68,  76,  58, // rep-id:          <35>"IDL:"
                                                     111, 109, 103,  46, 111, 114, 103,  47, //                  "omg.org/"
                                                      67,  79,  82,  66,  65,  47,  87,  83, //                  "CORBA/WS"
                                                     116, 114, 105, 110, 103,  86,  97, 108, //                  "tringVal"
                                                     117, 101,  58,  49,  46,  48,   0,   0, //                  "ue:1.0\0"<padding>
                                                       0,   0,   0,  13,  87,  83, 116, 114, // name
                                                     105, 110, 103,  86,  97, 108, 117, 101,
                                                       0,   0,   0,   0,   0,   0,   0,  27, // boxed value tc for wstring
                                                       0,   0,   0,   0, 127, 255, 255,   2, // bound, value
                                                       0,   0,   0,  35,  73,  68,  76,  58, // rep-id of value: <35>"IDL:"
                                                     111, 109, 103,  46, 111, 114, 103,  47, //                  "omg.org/"
                                                      67,  79,  82,  66,  65,  47,  87,  83, //                  "CORBA/WS"
                                                     116, 114, 105, 110, 103,  86,  97, 108, //                  "tringVal"
                                                     117, 101,  58,  49,  46,  48,   0,   0, //                  "ue:1.0\0"<padding>
                                                       0,   0,   0,   8,   0, 116,   0, 101, // value:           <8>"test"
                                                       0, 115,   0, 116 });                  // "For GIOP version 1.2 and 1.3 a wstring is not terminated by a null character"

            MappingConfiguration.Instance.UseWideCharByDefault = oldUseWideChar;
        }

        [Test]
        public void TestSerBoxedStringAsAnyAnyContainer2() {
            bool oldUseWideChar = MappingConfiguration.Instance.UseWideCharByDefault;
            MappingConfiguration.Instance.UseWideCharByDefault = false;

            Any val = new Any("test");
            AnySerializer anySer = new AnySerializer(m_serFactory, true);
            GenericSerTest(anySer, val, new byte[] {   0,   0,   0,  30,                     // tc-kind
                                                       0,   0,   0,  68,                     // encap length
                                                       0,   0,   0,   0,                     // flags
                                                       0,   0,   0,  34,  73,  68,  76,  58, // rep-id:          <34>"IDL:"
                                                     111, 109, 103,  46, 111, 114, 103,  47, //                  "omg.org/"
                                                      67,  79,  82,  66,  65,  47,  83, 116, //                  "CORBA/S"
                                                     114, 105, 110, 103,  86,  97, 108, 117, //                  "tringVal"
                                                     101,  58,  49,  46,  48,   0,   0,   0, //                  "ue:1.0\0"<padding>
                                                       0,   0,   0,  12,  83, 116, 114, 105, // name:            <12>"Stri"
                                                     110, 103,  86,  97, 108, 117, 101,   0, //                  "ngValue"
                                                       0,   0,   0,  18,                     // boxed value tc for string
                                                       0,   0,   0,   0, 127, 255, 255,   2, // bound, value
                                                       0,   0,   0,  34,  73,  68,  76,  58, // rep-id of value: <34>"IDL:"
                                                     111, 109, 103,  46, 111, 114, 103,  47, //                  "omg.org/"
                                                      67,  79,  82,  66,  65,  47,  83, 116, //                  "CORBA/S"
                                                     114, 105, 110, 103,  86,  97, 108, 117, //                  "tringVal"
                                                     101,  58,  49,  46,  48,   0,   0,   0, //                  "ue:1.0\0"<padding>
                                                       0,   0,   0,   5, 116, 101, 115, 116, // value:           <4>"test"
                                                       0                                     //                  "\0"
                                                     });                                     // "The string contents include a single terminating null character."

            MappingConfiguration.Instance.UseWideCharByDefault = oldUseWideChar;
        }

        [Test]
        public void TestDeSerBoxedStringAsAnyAnyContainer1() {
            AnySerializer anySer = new AnySerializer(m_serFactory, true);
            Any val = new Any("test");
            object deser =
                GenericDeserForTest(anySer, new byte[] {   0,   0,   0,  30,                     // tc-kind
                                                           0,   0,   0,  72,                     // encap length
                                                           0,   0,   0,   0,                     // flags
                                                           0,   0,   0,  35,  73,  68,  76,  58, // rep-id:          <35>"IDL:"
                                                         111, 109, 103,  46, 111, 114, 103,  47, //                  "omg.org/"
                                                          67,  79,  82,  66,  65,  47,  87,  83, //                  "CORBA/WS"
                                                         116, 114, 105, 110, 103,  86,  97, 108, //                  "tringVal"
                                                         117, 101,  58,  49,  46,  48,   0,   0, //                  "ue:1.0\0"<padding>
                                                           0,   0,   0,  13,  87,  83, 116, 114, // name
                                                         105, 110, 103,  86,  97, 108, 117, 101,
                                                           0,   0,   0,   0,   0,   0,   0,  27, // boxed value tc for wstring
                                                           0,   0,   0,   0, 127, 255, 255,   2, // bound, value
                                                           0,   0,   0,  35,  73,  68,  76,  58, // rep-id of value: <35>"IDL:"
                                                         111, 109, 103,  46, 111, 114, 103,  47, //                  "omg.org/"
                                                          67,  79,  82,  66,  65,  47,  87,  83, //                  "CORBA/WS"
                                                         116, 114, 105, 110, 103,  86,  97, 108, //                  "tringVal"
                                                         117, 101,  58,  49,  46,  48,   0,   0, //                  "ue:1.0\0"<padding>
                                                           0,   0,   0,   8,   0, 116,   0, 101, // value:           <8>"test"
                                                           0, 115,   0, 116 });                  // "For GIOP version 1.2 and 1.3 a wstring is not terminated by a null character"

            Assert.AreEqual(val.Value,
                                   ((Any)deser).Value, "deser value");
            Assert.AreEqual(ReflectionHelper.StringType,
                                   ((Any)deser).Value.GetType(), "deser type");
        }

        [Test]
        public void TestDeSerBoxedStringAsAnyAnyContainer2() {
            AnySerializer anySer = new AnySerializer(m_serFactory, true);
            Any val = new Any("test");
            object deser =
                GenericDeserForTest(anySer, new byte[] {   0,   0,   0,  30,                     // tc-kind
                                                           0,   0,   0,  68,                     // encap length
                                                           0,   0,   0,   0,                     // flags
                                                           0,   0,   0,  34,  73,  68,  76,  58, // rep-id:          <34>"IDL:"
                                                         111, 109, 103,  46, 111, 114, 103,  47, //                  "omg.org/"
                                                          67,  79,  82,  66,  65,  47,  83, 116, //                  "CORBA/S"
                                                         114, 105, 110, 103,  86,  97, 108, 117, //                  "tringVal"
                                                         101,  58,  49,  46,  48,   0,   0,   0, //                  "ue:1.0\0"<padding>
                                                           0,   0,   0,  12,  83, 116, 114, 105, // name:            <12>"Stri"
                                                         110, 103,  86,  97, 108, 117, 101,   0, //                  "ngValue"
                                                           0,   0,   0,  18,                     // boxed value tc for string
                                                           0,   0,   0,   0, 127, 255, 255,   2, // bound, value
                                                           0,   0,   0,  34,  73,  68,  76,  58, // rep-id of value: <34>"IDL:"
                                                         111, 109, 103,  46, 111, 114, 103,  47, //                  "omg.org/"
                                                          67,  79,  82,  66,  65,  47,  83, 116, //                  "CORBA/S"
                                                         114, 105, 110, 103,  86,  97, 108, 117, //                  "tringVal"
                                                         101,  58,  49,  46,  48,   0,   0,   0, //                  "ue:1.0\0"<padding>
                                                           0,   0,   0,   5, 116, 101, 115, 116, // value:           <4>"test"
                                                           0                                     //                  "\0"
                                                         });                                     // "The string contents include a single terminating null character."

            Assert.AreEqual(val.Value,
                                   ((Any)deser).Value, "deser value");
            Assert.AreEqual(ReflectionHelper.StringType,
                                   ((Any)deser).Value.GetType(), "deser type");
        }

    }

    [Serializable]
    [ExplicitSerializationOrdered()]
    [RepositoryID("IDL:Ch/Elca/Iiop/Tests/SimpleValueTypeWith2Ints")]
    public class SimpleValueTypeWith2Ints {

        [ExplicitSerializationOrderNr(0)]
        private int m_val1;
        [ExplicitSerializationOrderNr(1)]
        private int m_val2;

        public SimpleValueTypeWith2Ints() {
        }

        public SimpleValueTypeWith2Ints(int val1, int val2) {
            Val1 = val1;
            Val2 = val2;
        }

        public int Val1 {
            get {
                return m_val1;
            }
            set {
                m_val1 = value;
            }
        }

        public int Val2 {
            get {
                return m_val2;
            }
            set {
                m_val2 = value;
            }
        }

        public override bool Equals(object obj) {
            SimpleValueTypeWith2Ints other =
                obj as SimpleValueTypeWith2Ints;
            if (other == null) {
                return false;
            }
            return other.Val1 == Val1 &&
                   other.Val2 == Val2;
        }

        public override int GetHashCode() {
            return Val1.GetHashCode() ^
                Val2.GetHashCode();
        }

    }

    [TestFixture]
    public class SerializerTestValueTypes : AbstractSerializerTest {

        private SerializerFactory m_serFactory;

        [SetUp]
        public void SetUp() {
            m_serFactory =
                new SerializerFactory();
            omg.org.IOP.CodecFactory codecFactory =
                new Ch.Elca.Iiop.Interception.CodecFactoryImpl(m_serFactory);
            omg.org.IOP.Codec codec = 
                codecFactory.create_codec(
                    new omg.org.IOP.Encoding(omg.org.IOP.ENCODING_CDR_ENCAPS.ConstVal,
                                             1, 2));
            m_serFactory.Initalize(new SerializerFactoryConfig(), 
                                   IiopUrlUtil.Create(codec));
        }

        [Test]
        public void TestSerWStringValue() {
            Serializer ser = new ValueObjectSerializer(typeof(WStringValue),
                                                       m_serFactory);
            string testVal = "test";
            WStringValue toSer = new WStringValue(testVal);
            GenericSerTest(ser, toSer, new byte[] { 127, 255, 255, 2, // start value tag
                                              0, 0, 0, 35, 73, 68, 76, 58, // rep-id of value
                                              111, 109, 103, 46, 111, 114, 103, 47, 
                                              67, 79, 82, 66, 65, 47, 87, 83,
                                              116, 114, 105, 110, 103, 86, 97, 108,
                                              117, 101, 58, 49, 46, 48, 0, 0,
                                              0, 0, 0, 8, 0, 116, 0, 101, // "test"
                                              0, 115, 0, 116 } );

        }

        [Test]
        public void TestDeSerWStringValue() {
            Serializer ser = new ValueObjectSerializer(typeof(WStringValue),
                                                       m_serFactory);
            string testVal = "test";
            WStringValue deser = (WStringValue)
                GenericDeserForTest(ser, new byte[] { 127, 255, 255, 2, // start value tag
                                              0, 0, 0, 35, 73, 68, 76, 58, // rep-id of value
                                              111, 109, 103, 46, 111, 114, 103, 47, 
                                              67, 79, 82, 66, 65, 47, 87, 83,
                                              116, 114, 105, 110, 103, 86, 97, 108,
                                              117, 101, 58, 49, 46, 48, 0, 0,
                                              0, 0, 0, 8, 0, 116, 0, 101, // "test"
                                              0, 115, 0, 116 } );
            Assert.AreEqual(
                                   testVal, deser.Unbox(), "deserialised value wrong");
        }

        [Test]
        public void TestSerBasicContainingValueType() {
            Serializer ser = new ValueObjectSerializer(typeof(SimpleValueTypeWith2Ints),
                                                       m_serFactory);

            SimpleValueTypeWith2Ints toSer = new SimpleValueTypeWith2Ints(1, 2);

            GenericSerTest(ser, toSer, new byte[] { 127, 255, 255, 2, // start value tag
                                              0, 0, 0, 48, 73, 68, 76, 58, // rep-id of value
                                              67, 104, 47, 69, 108, 99, 97, 47, 
                                              73, 105, 111, 112, 47, 84, 101, 115,
                                              116, 115, 47, 83, 105, 109, 112, 108,
                                              101, 86, 97, 108, 117, 101, 84, 121, 
                                              112, 101, 87, 105, 116, 104, 50, 73,
                                              110, 116, 115, 0,
                                              0, 0, 0, 1, // 1
                                              0, 0, 0, 2 } ); // 2
        }

        [Test]
        public void TestDeserBasicContainingValueType() {
            Serializer ser = new ValueObjectSerializer(typeof(SimpleValueTypeWith2Ints),
                                                       m_serFactory);

            SimpleValueTypeWith2Ints toDeser = new SimpleValueTypeWith2Ints(1, 2);

            GenericDeserTest(ser, new byte[] { 127, 255, 255, 2, // start value tag
                                              0, 0, 0, 48, 73, 68, 76, 58, // rep-id of value
                                              67, 104, 47, 69, 108, 99, 97, 47, 
                                              73, 105, 111, 112, 47, 84, 101, 115,
                                              116, 115, 47, 83, 105, 109, 112, 108,
                                              101, 86, 97, 108, 117, 101, 84, 121, 
                                              112, 101, 87, 105, 116, 104, 50, 73,
                                              110, 116, 115, 0,
                                              0, 0, 0, 1, // 1
                                              0, 0, 0, 2 }, // 2
                                              toDeser);
        }
    }

    /// <summary>
    /// Serializer test for idl sequences
    /// </summary>
    [TestFixture]
    public class SerializerTestSequence : AbstractSerializerTest {

        private SerializerFactory m_serFactory;
        private SerializerFactoryConfig m_config;
        private IiopUrlUtil m_iiopUrlUtil;
        private AttributeExtCollection m_seqAttributes;

        [SetUp]
        public void SetUp() {
            m_serFactory = new SerializerFactory();
            omg.org.IOP.CodecFactory codecFactory =
                new Ch.Elca.Iiop.Interception.CodecFactoryImpl(m_serFactory);
            omg.org.IOP.Codec codec = 
                codecFactory.create_codec(
                    new omg.org.IOP.Encoding(omg.org.IOP.ENCODING_CDR_ENCAPS.ConstVal, 1, 2));
            m_iiopUrlUtil = IiopUrlUtil.Create(codec);
            m_config = new SerializerFactoryConfig();

            m_seqAttributes = 
                AttributeExtCollection.
                    ConvertToAttributeCollection(
                        new object[] { new IdlSequenceAttribute(0) } );
        }

        private Serializer CreateSerializer(object forInstance) {
            Type seqType = typeof(int[]); // simplification for null test.
            if (forInstance != null) {
                seqType = forInstance.GetType();
            }
            m_serFactory.Initalize(m_config, m_iiopUrlUtil);

            Serializer ser =
                m_serFactory.Create(seqType, m_seqAttributes);

            Assert.NotNull(ser, "ser");
            Assert.AreEqual(typeof(IdlSequenceSerializer<>).MakeGenericType(seqType.GetElementType()),
                            ser.GetType(), "ser type");
            return ser;
        }

        private void AssertSerialization(object actual, byte[] expected) {
            Serializer ser = CreateSerializer(actual);
            GenericSerTest(ser, actual, expected);
        }

        private void AssertDeserialization(byte[] actual, Array expected) {
            Serializer ser = CreateSerializer(expected);

            object result = GenericDeserForTest(ser, actual);
            Assert.IsTrue(result.GetType().IsArray, "result is array");
            Array resultArray = (Array)result;
            Assert.AreEqual(expected.Length, resultArray.Length, "array length");

            for (int i = 0; i < resultArray.Length; i++) {
                object elemIExpected = expected.GetValue(i);
                object elemIResult = resultArray.GetValue(i);
                Assert.AreEqual(elemIExpected, elemIResult, "element i");
            }
        }

        [Test]
        [ExpectedException(typeof(BAD_PARAM))]
        public void TestNotAllowNullIdlSeuqnece() {
            m_config.SequenceSerializationAllowNull = false;
            AssertSerialization(null, new byte[0]);

        }

        [Test]
        public void TestAllowNullSequenceIfConfigured() {
            m_config.SequenceSerializationAllowNull = true;
            AssertSerialization(null, new byte[] { 0, 0, 0, 0 });
        }

        [Test]
        public void TestNotEmptySequenceNullNotAllowed() {
            m_config.SequenceSerializationAllowNull = false;
            AssertSerialization(new int[] { 1, 2 },
                                new byte[] { 0, 0, 0, 2, 0, 0, 0, 1, 0, 0, 0 , 2});
        }

        [Test]
        public void TestNotEmptySequenceNullAllowed() {
            m_config.SequenceSerializationAllowNull = true;
            AssertSerialization(new int[] { 1, 2 },
                                new byte[] { 0, 0, 0, 2, 0, 0, 0, 1, 0, 0, 0 , 2});
        }

        [Test]
        public void TestInt16SequenceSer() {
            AssertSerialization(new short[] { 1, 2 },
                                new byte[] { 0, 0, 0, 2, 0, 1, 0, 2});
        }

        [Test]
        public void TestInt16SequenceDeSer() {
            AssertDeserialization(new byte[] { 0, 0, 0, 2, 0, 1, 0 , 2},
                                  new short[] { 1, 2 });
        }

        [Test]
        public void TestInt32SequenceSer() {
            AssertSerialization(new int[] { 1, 2 },
                                new byte[] { 0, 0, 0, 2, 0, 0, 0, 1, 0, 0, 0, 2});
        }

        [Test]
        public void TestInt32SequenceDeSer() {
            AssertDeserialization(new byte[] { 0, 0, 0, 2, 0, 0, 0, 1, 0, 0, 0, 2},
                                  new int[] { 1, 2 });
        }

        [Test]
        public void TestInt64SequenceSer() {
            AssertSerialization(new long[] { 1, 2 },
                                new byte[] { 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 2});
        }

        [Test]
        public void TestInt64SequenceDeSer() {
            AssertDeserialization(new byte[] { 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 2},
                                  new long[] { 1, 2 });
        }

        [Test]
        public void TestUInt16SequenceSer() {
            AssertSerialization(new ushort[] { 1, UInt16.MaxValue },
                                new byte[] { 0, 0, 0, 2, 0, 1, 0xFF, 0xFF});
        }

        [Test]
        public void TestUInt16SequenceDeSer() {
            AssertDeserialization(new byte[] { 0, 0, 0, 2, 0, 1, 0xFF , 0xFF},
                                  new ushort[] { 1, UInt16.MaxValue });
        }

        [Test]
        public void TestUInt32SequenceSer() {
            AssertSerialization(new uint[] { 1, UInt32.MaxValue },
                                new byte[] { 0, 0, 0, 2, 0, 0, 0, 1, 0xFF, 0xFF, 0xFF, 0xFF});
        }

        [Test]
        public void TestUInt32SequenceDeSer() {
            AssertDeserialization(new byte[] { 0, 0, 0, 2, 0, 0, 0, 1, 0xFF, 0xFF, 0xFF, 0xFF},
                                  new uint[] { 1, UInt32.MaxValue });
        }

        [Test]
        public void TestUInt64SequenceSer() {
            AssertSerialization(new ulong[] { 1, UInt64.MaxValue },
                                new byte[] { 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 
                                             0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF});
        }

        [Test]
        public void TestUInt64SequenceDeSer() {
            AssertDeserialization(new byte[] { 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 
                                               0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF},
                                  new ulong[] { 1, UInt64.MaxValue });
        }

        [Test]
        public void TestSingleSequenceSer() {
            AssertSerialization(new float[] { 1.0f, 0.01f },
                                new byte[] { 0, 0, 0, 2, 0x3F, 0x80, 0x00, 0x00, 0x3C, 0x23, 0xD7, 0x0A});
        }

        [Test]
        public void TestSingleSequenceDeSer() {
            AssertDeserialization(new byte[] { 0, 0, 0, 2, 0x3F, 0x80, 0x00, 0x00, 0x3C, 0x23, 0xD7, 0x0A},
                                  new float[] { 1.0f, 0.01f });
        }

        [Test]
        public void TestDoubleSequenceSer() {
            AssertSerialization(new double[] { (double)1.0f, Double.MaxValue, 0.01 },
                                new byte[] { 0, 0, 0, 3, 0, 0, 0, 0, 0x3F, 0xF0, 0, 0, 0, 0, 0, 0, 
                                             0x7F, 0xEF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                                             0x3F, 0x84, 0x7A, 0xE1, 0x47, 0xAE, 0x14, 0x7B});
        }

        [Test]
        public void TestDoubleSequenceDeSer() {
            AssertDeserialization(new byte[] { 0, 0, 0, 3, 0, 0, 0, 0, 0x3F, 0xF0, 0, 0, 0, 0, 0, 0, 
                                               0x7F, 0xEF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
                                               0x3F, 0x84, 0x7A, 0xE1, 0x47, 0xAE, 0x14, 0x7B },
                                  new double[] { (double)1.0f, Double.MaxValue, 0.01 });
        }

        [Test]
        public void TestByteSequenceSer() {
            AssertSerialization(new byte[] { 1, 2 },
                                new byte[] { 0, 0, 0, 2, 1, 2});
        }

        [Test]
        public void TestByteSequenceDeSer() {
            AssertDeserialization(new byte[] { 0, 0, 0, 2, 1, 2},
                                  new byte[] { 1, 2 });
        }

        [Test]
        public void TestBoolSequenceSer() {
            AssertSerialization(new bool[] { true, false },
                                new byte[] { 0, 0, 0, 2, 1, 0});
        }

        [Test]
        public void TestBoolSequenceDeSer() {
            AssertDeserialization(new byte[] { 0, 0, 0, 2, 1, 0},
                                  new bool[] { true, false });
        }

        [Test]
        public void TestEnumSequenceSer() {
            AssertSerialization(new TestIdlEnumBI32[] { TestIdlEnumBI32.IDL_A, TestIdlEnumBI32.IDL_B, TestIdlEnumBI32.IDL_C },
                                new byte[] { 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 2 });
        }

        [Test]
        public void TestEnumSequenceDeSer() {
            AssertDeserialization(new byte[] { 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 2 },
                                  new TestIdlEnumBI32[] { TestIdlEnumBI32.IDL_A, TestIdlEnumBI32.IDL_B, TestIdlEnumBI32.IDL_C });
        }
    }


    /// <summary>
    /// Serializer test for idl sequences
    /// </summary>
    [TestFixture]
    public class SerializerTestArray : AbstractSerializerTest {

        private SerializerFactory m_serFactory;
        private SerializerFactoryConfig m_config;
        private IiopUrlUtil m_iiopUrlUtil;
        private Type m_arrayType;
        private AttributeExtCollection m_arrayAttributes;

        [SetUp]
        public void SetUp() {
            m_serFactory = new SerializerFactory();
            omg.org.IOP.CodecFactory codecFactory =
                new Ch.Elca.Iiop.Interception.CodecFactoryImpl(m_serFactory);
            omg.org.IOP.Codec codec = 
                codecFactory.create_codec(
                    new omg.org.IOP.Encoding(omg.org.IOP.ENCODING_CDR_ENCAPS.ConstVal, 1, 2));
            m_iiopUrlUtil = IiopUrlUtil.Create(codec);
            m_config = new SerializerFactoryConfig();

            m_arrayAttributes = 
                AttributeExtCollection.
                    ConvertToAttributeCollection(
                        new object[] { new IdlArrayAttribute(0, 2) } );
            m_arrayType = typeof(int[]);
        }


        private void AssertSerialization(object actual, byte[] expected) {
            m_serFactory.Initalize(m_config, m_iiopUrlUtil);

            Serializer ser =
                m_serFactory.Create(m_arrayType, m_arrayAttributes);

            Assert.NotNull(ser, "ser");
            Assert.AreEqual(typeof(IdlArraySerializer),
                                   ser.GetType(), "ser type");

            GenericSerTest(ser, actual, expected);
        }

        [Test]
        [ExpectedException(typeof(BAD_PARAM))]
        public void TestNotAllowNullIdlArray() {
            m_config.ArraySerializationAllowNull = false;
            AssertSerialization(null, new byte[0]);

        }

        [Test]
        public void TestAllowNullIdlArray() {
            m_config.ArraySerializationAllowNull = true;
            AssertSerialization(null, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 });
        }

        [Test]
        public void TestNotNullIdlArray1DimNotAllowNull() {
            m_config.ArraySerializationAllowNull = false;
            AssertSerialization(new int[] { 1, 2 },
                                new byte[] { 0, 0, 0, 1, 0, 0, 0 , 2});
        }

        [Test]
        public void TestNotNullIdlArray1DimAllowNull() {
            m_config.ArraySerializationAllowNull = true;
            AssertSerialization(new int[] { 1, 2 },
                                new byte[] { 0, 0, 0, 1, 0, 0, 0 , 2});
        }

        [Test]
        public void Test2DimArrayNotNull() {
            m_config.ArraySerializationAllowNull = false;
            m_arrayType = typeof(int[,]);
            m_arrayAttributes =
                AttributeExtCollection.
                    ConvertToAttributeCollection(
                        new object[] { new IdlArrayAttribute(0, 2),
                                       new IdlArrayDimensionAttribute(0, 1, 3) } );

            AssertSerialization(new int[,] { { 1, 2, 3 }, { 4, 5, 6 } },
                                new byte[] { 0, 0, 0, 1, 0, 0, 0, 2,  0, 0, 0, 3, 
                                             0, 0, 0, 4, 0, 0, 0, 5, 0, 0, 0, 6 });
        }

        [Test]
        public void Test2DimArrayNullAllowNull() {
            m_config.ArraySerializationAllowNull = true;
            m_arrayType = typeof(int[,]);
            m_arrayAttributes =
                AttributeExtCollection.
                    ConvertToAttributeCollection(
                        new object[] { new IdlArrayAttribute(0, 2),
                                       new IdlArrayDimensionAttribute(0, 1, 3) } );

            AssertSerialization(null,
                                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0,  0, 0, 0, 0, 
                                             0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
        }

    }


    [ExplicitSerializationOrdered()]
    [IdlStruct]
    public struct TestExplicitelyOrderedStruct {

        [ExplicitSerializationOrderNr(1)]
        public int FieldB;

        [ExplicitSerializationOrderNr(2)]
        public int FieldC;

        [ExplicitSerializationOrderNr(0)]
        public int FieldA;

        [ExplicitSerializationOrderNr(3)]
        public int FieldD;

        public TestExplicitelyOrderedStruct(int a, int b, int c, int d) {
            FieldA = a;
            FieldB = b;
            FieldC = c;
            FieldD = d;
        }

        public override string ToString() {
            return "a: " + FieldA + "; b: " + FieldB + "; c: " + FieldC +
                ";d: " + FieldD;
        }

    }

    [IdlStruct]
    public struct TestImplicitelyOrderedStruct {

        public int FieldB;

        public int FieldC;

        public int FieldA;

        public int FieldD;

        public TestImplicitelyOrderedStruct(int a, int b, int c, int d) {
            FieldA = a;
            FieldB = b;
            FieldC = c;
            FieldD = d;
        }

        public override string ToString() {
            return "a: " + FieldA + "; b: " + FieldB + "; c: " + FieldC +
                ";d: " + FieldD;
        }

    }


    /// <summary>
    /// Serializer tests for idl structs.
    /// </summary>
    [TestFixture]
    public class SerializerTestIdlStruct : AbstractSerializerTest {

        private SerializerFactory m_serFactory;
        private IdlStructSerializer m_explicitelyOrderedStructSer;
        private IdlStructSerializer m_implicitelyOrderedStructSer;

        [SetUp]
        public void SetUp() {
            m_serFactory = new SerializerFactory();
            omg.org.IOP.CodecFactory codecFactory =
                new Ch.Elca.Iiop.Interception.CodecFactoryImpl(m_serFactory);
            omg.org.IOP.Codec codec = 
                codecFactory.create_codec(
                    new omg.org.IOP.Encoding(omg.org.IOP.ENCODING_CDR_ENCAPS.ConstVal, 1, 2));
            m_serFactory.Initalize(new SerializerFactoryConfig(), IiopUrlUtil.Create(codec));

            m_explicitelyOrderedStructSer = 
                new IdlStructSerializer(typeof(TestExplicitelyOrderedStruct), 
                                        m_serFactory);
            m_explicitelyOrderedStructSer.Initalize();
            m_implicitelyOrderedStructSer =
                new IdlStructSerializer(typeof(TestImplicitelyOrderedStruct), 
                                        m_serFactory);
            m_implicitelyOrderedStructSer.Initalize();
        }

        [Test]
        public void TestExplicitelyOrderedStrutSer() {

            TestExplicitelyOrderedStruct toSer = new TestExplicitelyOrderedStruct(1, 2, 3, 4);
            GenericSerTest(m_explicitelyOrderedStructSer,
                           toSer, new byte[] { 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 3,
                               0, 0, 0, 4 });
        }

        [Test]
        public void TestExplicitelyOrderedStructDeser() {
            TestExplicitelyOrderedStruct toDeser = new TestExplicitelyOrderedStruct(1, 2, 3, 4);
            GenericDeserTest(m_explicitelyOrderedStructSer,
                             new byte[] { 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 3,
                                          0, 0, 0, 4 },
                             toDeser);
        }

        [Test]
        public void TestImplicitelyOrderedStrutSer() {

            TestImplicitelyOrderedStruct toSer = new TestImplicitelyOrderedStruct(1, 2, 3, 4);
            GenericSerTest(m_implicitelyOrderedStructSer,
                           toSer, new byte[] { 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 3,
                               0, 0, 0, 4 });
        }

        [Test]
        public void TestImplicitelyOrderedStructDeser() {
            TestImplicitelyOrderedStruct toDeser = new TestImplicitelyOrderedStruct(1, 2, 3, 4);
            GenericDeserTest(m_implicitelyOrderedStructSer,
                             new byte[] { 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 3,
                                          0, 0, 0, 4 },
                             toDeser);
        }

    }

}

#endif
