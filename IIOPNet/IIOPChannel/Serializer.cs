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
    internal abstract class Serialiser {

        #region IConstructors

        internal Serialiser() {
        }

        #endregion IConstructors
        #region IMethods

        /// <summary>
        /// serializes the actual value into the given stream
        /// </summary>
        internal abstract void Serialise(Type formal, object actual, AttributeExtCollection attributes, 
                                       CdrOutputStream targetStream);

        /// <summary>
        /// deserialize the value from the given stream
        /// </summary>
        /// <param name="formal">the formal type of the parameter/field/...</param>
        /// <param name="attributes">the attributes on the parameter/field/..., but not the attributes on the formal-type</param>
        /// <param name="sourceStream"></param>
        /// <returns></returns>
        internal abstract object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream);

        /// <summary>
        /// serialises a field of a value-type
        /// </summary>
        /// <param name="fieldToSer"></param>
        protected void SerialiseField(FieldInfo fieldToSer, object actual, CdrOutputStream targetStream) {
            Marshaller marshaller = Marshaller.GetSingleton();
            marshaller.Marshal(fieldToSer.FieldType, 
                               ReflectionHelper.GetCustomAttriutesForField(fieldToSer, 
                                                                           true),
                               fieldToSer.GetValue(actual), targetStream);
        }

        /// <summary>
        /// deserialises a field of a value-type and sets the value
        /// </summary>
        /// <returns>the deserialised value</returns>
        protected object DeserialiseField(FieldInfo fieldToDeser, object actual, CdrInputStream sourceStream) {
            Marshaller marshaller = Marshaller.GetSingleton();
            object fieldVal = marshaller.Unmarshal(fieldToDeser.FieldType, 
                                                   ReflectionHelper.GetCustomAttriutesForField(fieldToDeser, 
                                                                                               true),
                                                   sourceStream);
            fieldToDeser.SetValue(actual, fieldVal);
            return fieldVal;
        }

        #endregion IMethods

    }

    // **************************************************************************************************
    #region serializer for primitive types

    /// <summary>serializes instances of System.Byte</summary>
    internal class ByteSerialiser : Serialiser {

        #region IMethods

        internal override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            targetStream.WriteOctet((byte)actual);
        }

        internal override object Deserialise(Type formal, AttributeExtCollection attributes, 
                                           CdrInputStream sourceStream) {
            return sourceStream.ReadOctet();
        }

        #endregion IMethods

    }

    /// <summary>serializes instances of System.Boolean</summary> 
    internal class BooleanSerialiser : Serialiser {

        #region IMethods

        internal override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            targetStream.WriteBool((bool)actual);
        }

        internal override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
            return sourceStream.ReadBool();
        }

        #endregion IMethods

    }

    /// <summary>serializes instances of System.Int16</summary>
    internal class Int16Serialiser : Serialiser {

        #region IMethods
        
        internal override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            targetStream.WriteShort((short)actual);
        }

        internal override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
            return sourceStream.ReadShort();
        }

        #endregion IMethods

    }
    
    /// <summary>serializes instances of System.Int32</summary>
    internal class Int32Serialiser : Serialiser {

        #region IMethods
        
        internal override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            targetStream.WriteLong((int)actual);
        }

        internal override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
            return sourceStream.ReadLong();
        }

        #endregion IMethods

    }

    /// <summary>serializes instances of System.Int64</summary>
    internal class Int64Serialiser : Serialiser {

        #region IMethods

        internal override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            targetStream.WriteLongLong((long)actual);
        }

        internal override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
            return sourceStream.ReadLongLong();
        }

        #endregion IMethods

    }

    /// <summary>serializes instances of System.Single</summary>
    internal class SingleSerialiser : Serialiser {

        #region IMethods
    
        internal override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            targetStream.WriteFloat((float)actual);
        }

        internal override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
            return sourceStream.ReadFloat();
        }

        #endregion IMethods

    }

    /// <summary>serializes instances of System.Double</summary>
    internal class DoubleSerialiser : Serialiser {

        #region IMethods

        internal override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            targetStream.WriteDouble((double)actual);
        }

        internal override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
            return sourceStream.ReadDouble();
        }

        #endregion IMethods

    }

    /// <summary>serializes instances of System.Char</summary>
    internal class CharSerialiser : Serialiser {

        #region IFields
        
        private bool m_useWide;
        
        #endregion IFields
        #region IConstructors
        
        public CharSerialiser(bool useWide) {
            m_useWide = useWide;
        }
        
        #endregion IConstructors
        #region IMethods

        internal override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {            
            if (m_useWide) {
                targetStream.WriteWChar((char)actual);
            } else {
                // the high 8 bits of the character is cut off
                targetStream.WriteChar((char)actual);
            }
        }

        internal override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {            
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
    internal class StringSerialiser : Serialiser {

        #region IFields
        
        private bool m_useWide;
        
        #endregion IFields
        #region IConstructors
        
        public StringSerialiser(bool useWide) {
            m_useWide = useWide;
        }
        
        #endregion IConstructors
        #region IMethods

        internal override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {            
            if (actual == null) { 
                // string may not be null, if StringValueAttriubte is set"
                throw new BAD_PARAM(10040, CompletionStatus.Completed_MayBe);
            }
            if (m_useWide) {
                targetStream.WriteWString((string)actual);
            } else {
                // encode with selected encoder, this can throw an exception, if an illegal character is encountered
                targetStream.WriteString((string)actual);
            }
        }

        internal override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
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
    internal class ObjRefSerializer : Serialiser {

        #region SFields
        
        private static Type s_iObjectType;
        private static MethodInfo s_isAMethod;
        
        #endregion SFields
        #region SConstructor
        
        static ObjRefSerializer() {
             s_iObjectType = typeof(omg.org.CORBA.IObject);
             s_isAMethod = s_iObjectType.GetMethod("_is_a", BindingFlags.Public |
                                                            BindingFlags.Instance);
        }
        
        #endregion SConstructor        
        #region IMethods
        
        internal override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            if (actual == null) { 
                WriteNullReference(targetStream); // null must be handled specially
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
                    formal.IsInterface && formal.IsInstanceOfType(actual)) {
                    // when marshalling a proxy, without having adequate type information from an IOR
                    // and formal is an interface, use interface type instead of MarshalByRef to
                    // prevent problems on server
                    actualType = formal;
                }
                // get the repository id for the type of this MarshalByRef object
                string repositoryID = Repository.GetRepositoryID(actualType);
                if (actualType.Equals(ReflectionHelper.MarshalByRefObjectType)) { 
                    repositoryID = ""; 
                } // CORBA::Object has "" repository id
                ior = IiopUrlUtil.CreateIorForUrl(url, repositoryID);
            } else {
                // server object
                ior = IiopUrlUtil.CreateIorForObjectFromThisDomain(target);
            }

            Debug.WriteLine("connection information for objRef, nr of profiles: " + ior.Profiles.Length);

            // now write the IOR to the stream
            ior.WriteToStream(targetStream);
        }

        private void WriteNullReference(CdrOutputStream targetStream) {
            Ior ior = new Ior("", new IorProfile[0]);
            ior.WriteToStream(targetStream); // write the null reference to the stream
        }
                


        internal override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
            // reads the encoded IOR from this stream
            Ior ior = new Ior(sourceStream);
            if (ior.IsNullReference()) { 
                return null; 
            } // received a null reference, return null
            // create a url from this ior:
            string url = ior.ToString(); // use stringified form of IOR as url --> do not lose information
            Type interfaceType;
            if (!ior.TypID.Equals("")) { // empty string stands for CORBA::Object
                interfaceType = Repository.GetTypeForId(ior.TypID);
            } else {
                interfaceType = ReflectionHelper.MarshalByRefObjectType;
            }
            if (interfaceType == null) { 
                // check, if formal can be assigned by remote object; for formal MarshalbyRefObject, every remote object is ok
                if (formal.Equals(ReflectionHelper.MarshalByRefObjectType) || CheckAssignableRemote(formal, url)) {
                    interfaceType = formal;
                } else {
                    Trace.WriteLine("unknown incompatible type-id in IOR: " + ior.TypID);
                    // unknown repository id encountered:  ior.TypID
                    // and is_a check failed
                    throw new INTF_REPOS(1414, CompletionStatus.Completed_MayBe);
                }
            }

            if ((!formal.Equals(ReflectionHelper.MarshalByRefObjectType)) && 
                (!formal.IsAssignableFrom(interfaceType)) &&
                (!formal.Equals(ReflectionHelper.IObjectType))) {
                // for formal-parameter MarshalByRefObject and omg.org.CORBA.IObject
                // everything is possible (i.e. every remote object type can be assigned to it),
                // the other formal types must be checked
                if (CheckAssignableRemote(formal, url)) {
                    interfaceType = formal;
                } else {
                    Trace.WriteLine("received obj-reference is not compatible with " + 
                                    "the required formal parameter, formal: " +
                                    formal + ", received: " + interfaceType);
                    throw new BAD_PARAM(20010, CompletionStatus.Completed_MayBe);
                }
            }
            
            // create a proxy
            object proxy = RemotingServices.Connect(interfaceType, url);
            return proxy;
        }
        
        /// <summary>if compatibility is not checkable with type information included in
        /// IOR, call _is_a method to check.</summary>
        private bool CheckAssignableRemote(Type formal, string url) {
            object proxy = RemotingServices.Connect(s_iObjectType, url);
            bool isAssignable = (bool)s_isAMethod.Invoke(proxy, 
                                                         new object[] { Repository.GetRepositoryID(formal)});
            return isAssignable;
        }

        #endregion IMethods

    }

    #endregion

    // **************************************************************************************************
    // ********************************* Serializer for value types *************************************
    // **************************************************************************************************

    /// <summary>standard serializer for pass by value object</summary>
    /// <remarks>if a CLS struct should be serialized as IDL struct and not as ValueType, use the IDLStruct Serializer</remarks>
    internal class ValueObjectSerializer : Serialiser {

        #region IMethods

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
                    Type implClass = Repository.LoadType(implAttr.ImplClass);
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
        /// creates a Stack with the inheritance information for the Type forType.
        /// </summary>
        private Stack CreateTypeHirarchyStack(Type forType) {
            Stack typeHierarchy = new Stack();
            Type currentType = forType;
            while (currentType != null) {
                if (!IsImplClass(currentType)) { // ignore impl-classes in serialization code
                    typeHierarchy.Push(currentType);
                    if (CheckForCustomMarshalled(currentType)) {
                        break;
                    }
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

        internal override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
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

                uint valueTag = CdrStreamHelper.MIN_VALUE_TAG; // value-tag with no option set
                // attentition here: if formal type represents an IDL abstract interface, writing no type information is not ok.
                // do not use no typing information option, because java orb can't handle it
                valueTag = valueTag | 0x00000002;
                StreamPosition indirPos = targetStream.WriteIndirectableInstanceTag(valueTag);
                string repId = "";
                if (!IsImplClass(actual.GetType())) {
                    repId = Repository.GetRepositoryID(actual.GetType());
                } else { // an impl-class is not serialized, because it's not known at the receiving ORB
                    repId = Repository.GetRepositoryID(actual.GetType().BaseType);
                }
                targetStream.WriteIndirectableString(repId, IndirectionType.IndirRepId,
                                                     IndirectionUsage.ValueType);

                // add instance to indirection table
                targetStream.StoreIndirection(actual,
                                              new IndirectionInfo(indirPos.GlobalPosition,
                                                                  IndirectionType.IndirValue,
                                                                  IndirectionUsage.ValueType));

                Stack typeHierarchy = CreateTypeHirarchyStack(actual.GetType());
                while (typeHierarchy.Count > 0) {
                    Type marshalType = (Type)typeHierarchy.Pop();
                    if (!CheckForCustomMarshalled(marshalType)) {
                        WriteFieldsForType(actual, marshalType, targetStream);
                    } else { // custom marshalled
                        if (!(actual is ICustomMarshalled)) {
                            // can't serialise custom value type, because ICustomMarshalled not implemented
                            throw new INTERNAL(909, CompletionStatus.Completed_MayBe);
                        }
                        ((ICustomMarshalled)actual).Serialize(new DataOutputStreamImpl(targetStream));
                    }
                }
            }
        }

        /// <summary>writes the fields delcared in the type ofType of the instance instance</summary>
        /// <param name="chunkedRep">use chunked representation</param>
        private void WriteFieldsForType(object instance, Type ofType, 
                                        CdrOutputStream targetStream) {
            FieldInfo[] fields = ReflectionHelper.GetAllDeclaredInstanceFields(ofType);
            foreach (FieldInfo fieldInfo in fields) {
                if (!fieldInfo.IsNotSerialized) { // do not serialize transient fields
                    WriteField(fieldInfo, instance, 
                               targetStream);
                }
            }
        }

        /// <summary>write the value of the field to the underlying stream</summary>
        private void WriteField(FieldInfo field, object instance,
                                CdrOutputStream targetStream) {
            Marshaller marshaller = Marshaller.GetSingleton();

            object fieldVal = field.GetValue(instance);
            AttributeExtCollection attrColl =
                ReflectionHelper.GetCustomAttriutesForField(field, true);
            // write value                
            marshaller.Marshal(field.FieldType, attrColl, fieldVal, 
                               targetStream);
        }


        internal override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
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

                Type actualType = GetActualType(formal, sourceStream, valueTag);
                object result = CreateInstance(actualType);
                if (!(formal.IsInstanceOfType(result))) {
                    // invalid implementation class of value type: 
                    // instance.GetType() is incompatible with: formal
                    throw new BAD_PARAM(903, CompletionStatus.Completed_MayBe);
                }
                // store indirection info for this instance, if another instance contains a reference to this one
                sourceStream.StoreIndirection(new IndirectionInfo(instanceStartPos.GlobalPosition,
                                                                  IndirectionType.IndirValue,
                                                                  IndirectionUsage.ValueType), 
                                              result);

                // now the value fields follow
                sourceStream.BeginReadValueBody(valueTag);
                DeserialiseValueBody(actualType, sourceStream, result);
                sourceStream.EndReadValue(valueTag);
                return result;
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

        private void DeserialiseValueBody(Type actualType, CdrInputStream sourceStream,
                                          object instance) {
            Stack typeHierarchy = CreateTypeHirarchyStack(actualType);
            while (typeHierarchy.Count > 0) {
                Type demarshalType = (Type)typeHierarchy.Pop();
                if (!CheckForCustomMarshalled(demarshalType)) {
                    ReadFieldsForType(instance, demarshalType, 
                                      sourceStream);
                } else { // custom marshalled
                    if (!(instance is ICustomMarshalled)) {
                        // can't deserialise custom value type, because ICustomMarshalled not implented
                        throw new INTERNAL(909, CompletionStatus.Completed_MayBe);
                    }
                    ((ICustomMarshalled)instance).Deserialise(new DataInputStreamImpl(sourceStream));
                }
            }
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

        /// <summary>creates an instance of the given type via reflection</summary>
        private object CreateInstance(Type actualType) {
            object[] implAttr = actualType.GetCustomAttributes(ReflectionHelper.ImplClassAttributeType, false);
            if ((implAttr != null) && (implAttr.Length > 0)) {
                if (implAttr.Length > 1) {
                    // invalid type: actualType, only one ImplClassAttribute allowed
                    throw new INTERNAL(923, CompletionStatus.Completed_MayBe);
                }
                ImplClassAttribute implCl = (ImplClassAttribute)implAttr[0];
                // get the type
                actualType = Repository.LoadType(implCl.ImplClass);
                if (actualType == null) {
                    Trace.WriteLine("implementation class : " + implCl.ImplClass +
                                    " of value-type: " + actualType + " couldn't be found");
                    throw new NO_IMPLEMENT(1, CompletionStatus.Completed_MayBe, implCl.ImplClass);
                }
            }
            // type must not be abstract for beeing instantiable
            if (actualType.IsAbstract) {
                // value-type couln't be instantiated: actualType
                throw new NO_IMPLEMENT(931, CompletionStatus.Completed_MayBe);
            }
            // instantiate            
            object instance = Activator.CreateInstance(actualType);
            return instance;
        }

        /// <summary>reads in a field in a value-type instance</summary>
        /// <param name="containingInstance">the instance, in which the field should be set</param>
        internal void ReadAndSetField(FieldInfo field, object containingInstance, 
                                      CdrInputStream sourceStream) {
            Marshaller marshaller = Marshaller.GetSingleton();
            AttributeExtCollection attrColl =
                ReflectionHelper.GetCustomAttriutesForField(field, true);
            object result = marshaller.Unmarshal(field.FieldType, attrColl, sourceStream);
            field.SetValue(containingInstance, result);
        }

        /// <summary>reads and sets the field declared in the type ofType.</summary>
        private void ReadFieldsForType(object instance, Type ofType, 
                                       CdrInputStream sourceStream) {
            // reads all fields declared in the Type: no inherited fields
            FieldInfo[] fields = ReflectionHelper.GetAllDeclaredInstanceFields(ofType);
            foreach (FieldInfo fieldInfo in fields) {
                if (!fieldInfo.IsNotSerialized) { // do not serialize transient fields
                    ReadAndSetField(fieldInfo, instance, sourceStream);
                }
            }
        }


        #endregion IMethods

    }

    /// <summary>serializes an non boxed value as an IDL boxed value and deserialize an IDL boxed value as an unboxed value</summary>
    /// <remarks>do not use this serializer with instances of BoxedValues which should not be boxed or unboxed</remarks>
    internal class BoxedValueSerializer : Serialiser {

        #region IFields

        private ValueObjectSerializer m_valueSer = new ValueObjectSerializer();
        private bool m_convertMultiDimArray = false;

        #endregion IFields
        #region IConstructors
        
        public BoxedValueSerializer(bool convertMultiDimArray) {
            m_convertMultiDimArray = convertMultiDimArray;
        }

        #endregion IConstructors
        #region IMethods
        
        internal override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                         CdrOutputStream targetStream) {
            if (!formal.IsSubclassOf(ReflectionHelper.BoxedValueBaseType)) { 
                // BoxedValueSerializer can only serialize formal types, 
                // which are subclasses of BoxedValueBase
                throw new INTERNAL(10041, CompletionStatus.Completed_MayBe);
            }

            if (m_convertMultiDimArray) {
                // actual is a multi dimensional array, which must be first converted to a jagged array
                if ((actual != null) && (!actual.GetType().IsArray) && (!(actual.GetType().GetArrayRank() > 1))) {
                    throw new BAD_PARAM(9004, CompletionStatus.Completed_MayBe);
                }
                actual = BoxedArrayHelper.ConvertMoreDimToNestedOneDim((Array)actual);
            }

            // perform a boxing
            object boxed = null;
            if (actual != null) {
                boxed = Activator.CreateInstance(formal, new object[] { actual } );
            }
            m_valueSer.Serialise(formal, boxed, attributes, targetStream);
        }

        internal override object Deserialise(Type formal, AttributeExtCollection attributes, 
                                             CdrInputStream sourceStream) {
            Debug.WriteLine("deserialise boxed value, formal: " + formal);
            if (!formal.IsSubclassOf(ReflectionHelper.BoxedValueBaseType)) { 
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

            if (m_convertMultiDimArray) {
                // result is a jagged arary, which must be converted to a true multidimensional array
                if ((result != null) && (!result.GetType().IsArray)) {
                    throw new BAD_PARAM(9004, CompletionStatus.Completed_MayBe);
                }
                result = BoxedArrayHelper.ConvertNestedOneDimToMoreDim((Array)result);
            }

            Debug.WriteLine("unboxed result of boxedvalue-ser: " + result);
            return result;
        }

        #endregion IMethods

    }

    /// <summary>
    /// this class serializes .NET structs, which were mapped from an IDL-struct
    /// </summary>
    internal class IdlStructSerializer : Serialiser {

        #region IMethods
    
        internal override object Deserialise(System.Type formal, AttributeExtCollection attributes,
                                             CdrInputStream sourceStream) {
            FieldInfo[] fields = ReflectionHelper.GetAllDeclaredInstanceFields(formal);
            Marshaller marshaller = Marshaller.GetSingleton();
                        
            object instance = Activator.CreateInstance(formal);
            foreach (FieldInfo info in fields) {
                object fieldVal = marshaller.Unmarshal(info.FieldType, 
                                                       ReflectionHelper.GetCustomAttriutesForField(info, true), 
                                                       sourceStream);
                info.SetValue(instance, fieldVal);
            }
            return instance;
        }

        internal override void Serialise(System.Type formal, object actual, AttributeExtCollection attributes,
                                         CdrOutputStream targetStream) {
            FieldInfo[] fields = ReflectionHelper.GetAllDeclaredInstanceFields(formal);
            Marshaller marshaller = Marshaller.GetSingleton();
            foreach (FieldInfo info in fields) {
                marshaller.Marshal(info.FieldType, 
                                   ReflectionHelper.GetCustomAttriutesForField(info, true),
                                   info.GetValue(actual), targetStream);
            }
        }

        #endregion IMethods

    }

    internal class IdlUnionSerializer : Serialiser {

        #region Constants

        private const string GET_FIELD_FOR_DISCR_METHOD_NAME = "GetFieldForDiscriminator";

        private const string DISCR_FIELD_NAME = "m_discriminator";

        private const string INITALIZED_FIELD_NAME = "m_initalized";

        #endregion Constants

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

        internal override object Deserialise(System.Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {            
            // instantiate the resulting union
            object result = Activator.CreateInstance(formal);
            // deserialise discriminator value
            FieldInfo discrField = GetDiscriminatorField(formal);
            object discrVal = DeserialiseField(discrField, result, sourceStream);
            
            // determine value to deser
            FieldInfo curField = GetValFieldForDiscriminator(formal, discrVal);
            if (curField != null) {
                // deserialise value
                DeserialiseField(curField, result, sourceStream);
            }
            FieldInfo initalizedField = GetInitalizedField(formal);
            initalizedField.SetValue(result, true);
            return result;
        }

        internal override void Serialise(System.Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {            
            FieldInfo initalizedField = GetInitalizedField(formal);
            bool isInit = (bool)initalizedField.GetValue(actual);
            if (isInit == false) {
                throw new BAD_OPERATION(34, CompletionStatus.Completed_MayBe);
            }
            // determine value of the discriminator
            FieldInfo discrValField = GetDiscriminatorField(formal);
            object discrVal = discrValField.GetValue(actual);
            // get the field matching the current discriminator
            FieldInfo curField = GetValFieldForDiscriminator(formal, discrVal);
            
            SerialiseField(discrValField, actual, targetStream);
            if (curField != null) {
                // seraialise value
                SerialiseField(curField, actual, targetStream);
            } 
            // else:  case outside covered discr range, do not serialise value, only discriminator
        }

        #endregion IMethods
    }

    /// <summary>serailizes an instances as IDL abstract-value</summary>
    internal class AbstractValueSerializer : Serialiser {

        #region IFields

        private ValueObjectSerializer m_valObjectSer = new ValueObjectSerializer();

        #endregion IFields
        #region IMethods

        internal override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
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
            m_valObjectSer.Serialise(formal, actual, attributes, targetStream);
        }

        internal override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
            // deserialise as IDL-value-type
            return m_valObjectSer.Deserialise(formal, attributes, sourceStream);
        }

        #endregion IMethods

    }

    /// <summary>serializes an instance of the class System.Type</summary>
    internal class TypeSerializer : Serialiser {

        #region IFields
        
        private TypeCodeSerializer m_typeCodeSer = new TypeCodeSerializer();

        #endregion IFields
        #region IMethods

        internal override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            omg.org.CORBA.TypeCode tc;
            tc = Repository.CreateTypeCodeForType((Type)actual, attributes);
            m_typeCodeSer.Serialise(ReflectionHelper.CorbaTypeCodeType, tc, attributes, targetStream);
        }

        internal override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {            
            omg.org.CORBA.TypeCode tc = 
                (omg.org.CORBA.TypeCode)m_typeCodeSer.Deserialise(ReflectionHelper.CorbaTypeCodeType, attributes, sourceStream);
            Type result = null;
            if (!(tc is NullTC)) {
                result = Repository.GetTypeForTypeCode(tc);
            }
            return result;
        }

        #endregion IMethods

    }
    
    /// <summary>serializes enums</summary>
    internal class EnumSerializer : Serialiser {
        
        #region IMethods

        internal override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            // check for IDL-enum mapped to a .NET enum
            AttributeExtCollection attrs = ReflectionHelper.GetCustomAttributesForType(formal, true);
            if (attrs.IsInCollection(ReflectionHelper.IdlEnumAttributeType)) {
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

        internal override object Deserialise(Type formal, AttributeExtCollection attributes, 
                                           CdrInputStream sourceStream) {
            AttributeExtCollection attrs = ReflectionHelper.GetCustomAttributesForType(formal, true);
            if (attrs.IsInCollection(ReflectionHelper.IdlEnumAttributeType)) {
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
    internal class IdlSequenceSerializer : Serialiser {
        
        #region IFields
        
        private int m_bound;
        
        #endregion IFields
        #region IConstructors
        
        public IdlSequenceSerializer(int bound) {
            m_bound = bound;    
        }
        
        #endregion IConstructors
        #region IMethods

        /// <summary>
        /// checks, if parameter to serialise does not contain more elements than allowed
        /// </summary>
        private void CheckBound(uint sequenceLength) {
            if (IdlSequenceAttribute.IsBounded(m_bound) && (sequenceLength > m_bound)) {
                throw new BAD_PARAM(3434, CompletionStatus.Completed_MayBe);
            }
        }
        
        internal override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            Array array = (Array) actual;
            if (array == null) {
                // not allowed for a sequence:
                throw new BAD_PARAM(3433, CompletionStatus.Completed_MayBe);
            }
            CheckBound((uint)array.Length);
            targetStream.WriteULong((uint)array.Length);
            // get marshaller for elemtype
            Type elemType = formal.GetElementType();
            MarshallerForType marshaller = new MarshallerForType(elemType, attributes);
            for (int i = 0; i < array.Length; i++) {
                // it's more efficient to not determine serialise for each element; instead use cached ser
                marshaller.Marshal(array.GetValue(i), targetStream);
            }
        }

        internal override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
            // mapped from an IDL-sequence
            uint nrOfElements = sourceStream.ReadULong();
            CheckBound(nrOfElements);
            
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
        }

        #endregion IMethods

    }


    /// <summary>serialises IDL-arrays</summary>
    internal class IdlArraySerialiser : Serialiser {

        #region IFields
        
        private int[] m_dimensions;
        
        #endregion IFields
        #region IConstructors
        
        public IdlArraySerialiser(int[] dimensions) {
            m_dimensions = dimensions;    
        }
        
        #endregion IConstructors
        #region IMethods

        private void CheckInstance(Array array) {
            if (m_dimensions.Length != array.Rank) {
                throw new BAD_PARAM(3436, CompletionStatus.Completed_MayBe);
            }
            for (int i = 0; i < array.Rank; i++) {
                if (m_dimensions[i] != array.GetLength(i)) {
                    throw new BAD_PARAM(3437, CompletionStatus.Completed_MayBe);
                }
            }
        } 


        private void SerialiseDimension(Array array, MarshallerForType marshaller, CdrOutputStream targetStream,
                                        int[] indices, int currentDimension) {
            if (currentDimension == array.Rank) {
                object value = array.GetValue(indices);
                marshaller.Marshal(value, targetStream);
            } else {
                // the first dimension index in the array is increased slower than the second and so on ...
                for (int j = 0; j < array.GetLength(currentDimension); j++) {
                    indices[currentDimension] = j;                    
                    SerialiseDimension(array, marshaller, targetStream, indices, currentDimension + 1);
                }
            }
        }
        
        internal override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                         CdrOutputStream targetStream) {
            Array array = (Array) actual;
            if (array == null) {
                // not allowed for an idl array:
                throw new BAD_PARAM(3433, CompletionStatus.Completed_MayBe);
            }
            CheckInstance(array);
            // get marshaller for elemtype
            Type elemType = formal.GetElementType();
            MarshallerForType marshaller = new MarshallerForType(elemType, attributes);
            SerialiseDimension(array, marshaller, targetStream, new int[array.Rank], 0);
        }

        private void DeserialiseDimension(Array array, MarshallerForType marshaller, CdrInputStream sourceStream,
                                          int[] indices, int currentDimension) {
            if (currentDimension == array.Rank) {
                object entry = marshaller.Unmarshal(sourceStream);
                array.SetValue(entry, indices);
            } else {
                // the first dimension index in the array is increased slower than the second and so on ...
                for (int j = 0; j < array.GetLength(currentDimension); j++) {                    
                    indices[currentDimension] = j;                    
                    DeserialiseDimension(array, marshaller, sourceStream, indices, currentDimension + 1);
                }
            }            
        }

        internal override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
           
            Array result = Array.CreateInstance(formal.GetElementType(), m_dimensions);
            // get marshaller for array element type
            Type elemType = formal.GetElementType();
            MarshallerForType marshaller = new MarshallerForType(elemType, attributes);
            DeserialiseDimension(result, marshaller, sourceStream, new int[result.Rank], 0);
            return result;
        }

        #endregion IMethods        

    }

    /// <summary>serializes an instance as IDL-any</summary>
    internal class AnySerializer : Serialiser {

        #region SFields

        private static Type s_supInterfaceAttrType = typeof(SupportedInterfaceAttribute);
        private static Type s_anyType = typeof(omg.org.CORBA.Any);

        #endregion SFields
        #region IFields
        
        private TypeCodeSerializer m_typeCodeSer = new TypeCodeSerializer();

        #endregion IFields
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
        
        
        internal override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            TypeCodeImpl typeCode = new NullTC();
            object actualToSerialise = actual;
            Type actualType = null;
            if (actual != null) {
                if (actual.GetType().Equals(s_anyType)) {
                    // use user defined type code
                    typeCode = ((Any)actual).Type as TypeCodeImpl;
                    if (typeCode == null) {
                        throw new INTERNAL(457, CompletionStatus.Completed_MayBe);
                    }
                    // type, which should be used to serialise value is determined by typecode!                    
                    if ((!(typeCode is NullTC)) && (!(typeCode is VoidTC))) {
                        actualType = Repository.GetTypeForTypeCode(typeCode); // no .NET type for null-tc, void-tc
                    }
                    actualToSerialise = ((Any)actual).Value;
                } else {
                    // automatic type code creation
                    actualType = DetermineTypeToUse(actual);
                    typeCode = Repository.CreateTypeCodeForType(actualType, attributes);
                }
            }
            m_typeCodeSer.Serialise(ReflectionHelper.CorbaTypeCodeType, typeCode, attributes, targetStream);
            if (actualType != null) {
                Marshaller marshaller = Marshaller.GetSingleton();               
                AttributeExtCollection typeAttributes = Repository.GetAttrsForTypeCode(typeCode);                
                marshaller.Marshal(actualType, typeAttributes, actualToSerialise, targetStream);
            }
        }

        internal override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
            omg.org.CORBA.TypeCode typeCode = (omg.org.CORBA.TypeCode)m_typeCodeSer.Deserialise(formal, 
                                                                                                attributes, sourceStream);
            object result;
            // when returning 0 in a mico-server for any, the typecode used is VoidTC
            if ((!(typeCode is NullTC)) && (!(typeCode is VoidTC))) {
                Type dotNetType = Repository.GetTypeForTypeCode(typeCode);
                AttributeExtCollection typeAttributes = Repository.GetAttrsForTypeCode(typeCode);
                Marshaller marshaller = Marshaller.GetSingleton();
                result = marshaller.Unmarshal(dotNetType, typeAttributes, sourceStream);
                if (result is BoxedValueBase) {
                    result = ((BoxedValueBase)result).Unbox(); // unboxing the boxed-value, because BoxedValueTypes are internal types, which should not be used by users
                }
            } else {
                result = null;
            }
            if (!formal.Equals(s_anyType)) {
                return result;
            } else {
                return new Any(result, typeCode);
            }
        }
        
        #endregion IMethods

    }

    /// <summary>serializes a typecode</summary>
    internal class TypeCodeSerializer : Serialiser {
        
        #region IMethods

        internal override object Deserialise(System.Type formal, AttributeExtCollection attributes, CdrInputStream sourceStream) {

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
                result.ReadFromStream(sourceStream);                                
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

        internal override void Serialise(System.Type formal, object actual, AttributeExtCollection attributes, CdrOutputStream targetStream) {
            if (!(actual is omg.org.CORBA.TypeCodeImpl)) { 
                // typecode not serializable
                throw new omg.org.CORBA.INTERNAL(1654, omg.org.CORBA.CompletionStatus.Completed_MayBe);
            }
            omg.org.CORBA.TypeCodeImpl tcImpl = actual as omg.org.CORBA.TypeCodeImpl;
            if (!targetStream.IsPreviouslyMarshalled(tcImpl, IndirectionType.TypeCode, IndirectionUsage.TypeCode)) {
                tcImpl.WriteToStream(targetStream);
            } else {
                targetStream.WriteIndirection(tcImpl);
            }
        }

        #endregion IMethods

    }
    
    /// <summary>serializes an instance as IDL abstract-interface</summary>
    internal class AbstractInterfaceSerializer : Serialiser {

        #region IFields

        private ObjRefSerializer m_objRefSer = new ObjRefSerializer();
        private ValueObjectSerializer m_valueSer = new ValueObjectSerializer();

        #endregion IFields
        #region IMethods

        internal override void Serialise(Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            // if actual is null it shall be encoded as a valuetype: 15.3.7
            if ((actual != null) && (ClsToIdlMapper.IsMappedToConcreteInterface(actual.GetType()))) {
                targetStream.WriteBool(true); // an obj-ref is serialized
                m_objRefSer.Serialise(formal, actual, attributes, targetStream);
            } else if ((actual == null) || (ClsToIdlMapper.IsMappedToConcreteValueType(actual.GetType()))) {
                targetStream.WriteBool(false); // a value-type is serialised
                m_valueSer.Serialise(formal, actual, attributes, targetStream);
            } else {
                // actual value ( actual ) with type: 
                // actual.GetType() is not serializable for the formal type
                // formal
                throw new BAD_PARAM(6, CompletionStatus.Completed_MayBe);
            }
        }

        internal override object Deserialise(Type formal, AttributeExtCollection attributes,
                                           CdrInputStream sourceStream) {
            bool isObjRef = sourceStream.ReadBool();
            if (isObjRef) {
                if (formal.Equals(ReflectionHelper.ObjectType)) {
                    // if in interface only abstract interface base type is used, set formal now
                    // to base type of all objref for deserialization
                    formal = ReflectionHelper.MarshalByRefObjectType;
                }
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
    internal class ExceptionSerializer : Serialiser {

        #region IMethods

        internal override object Deserialise(System.Type formal, AttributeExtCollection attributes,
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
                FieldInfo[] fields = ReflectionHelper.GetAllDeclaredInstanceFields(exceptionType);
                Marshaller marshaller = Marshaller.GetSingleton();
                foreach (FieldInfo field in fields) {
                    object fieldVal = marshaller.Unmarshal(field.FieldType, 
                                                           ReflectionHelper.GetCustomAttriutesForField(field, true),                                                           
                                                           sourceStream);
                    field.SetValue(exception, fieldVal);
                }                
                return exception;
            }
        }

        internal override void Serialise(System.Type formal, object actual, AttributeExtCollection attributes,
                                       CdrOutputStream targetStream) {
            string repId = Repository.GetRepositoryID(formal);
            targetStream.WriteString(repId);

            if (formal.IsSubclassOf(typeof(AbstractCORBASystemException))) {
                // system exceptions are serialized specially, because no inheritance is possible for exceptions, see implementation of the system exceptions
                AbstractCORBASystemException sysEx = (AbstractCORBASystemException) actual;
                targetStream.WriteULong((uint)sysEx.Minor);
                targetStream.WriteULong((uint)sysEx.Status);
            } else {
                FieldInfo[] fields = ReflectionHelper.GetAllDeclaredInstanceFields(formal);
                Marshaller marshaller = Marshaller.GetSingleton();
                foreach (FieldInfo field in fields) {
                    object fieldVal = field.GetValue(actual);
                    marshaller.Marshal(field.FieldType, 
                                       ReflectionHelper.GetCustomAttriutesForField(field, true),
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
    using System.Runtime.Remoting.Channels;
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
            ser.Serialise(ReflectionHelper.ByteType, (byte)11, new AttributeExtCollection(), cdrOut);
            ser.Serialise(ReflectionHelper.ByteType, (byte)12, new AttributeExtCollection(), cdrOut);
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
            Assertion.AssertEquals(11, ser.Deserialise(ReflectionHelper.ByteType, 
                                                       new AttributeExtCollection(), cdrIn));           
            Assertion.AssertEquals(12, ser.Deserialise(ReflectionHelper.ByteType, 
                                                       new AttributeExtCollection(), cdrIn));
            inStream.Close();           
        }
        
        public void TestBooleanSerialise() {
            MemoryStream outStream = new MemoryStream();
            CdrOutputStream cdrOut = new CdrOutputStreamImpl(outStream, 0);
            Serialiser ser = new BooleanSerialiser();
            ser.Serialise(ReflectionHelper.BooleanType, true, new AttributeExtCollection(), cdrOut);
            ser.Serialise(ReflectionHelper.BooleanType, false, new AttributeExtCollection(), cdrOut);
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
            Assertion.AssertEquals(false, ser.Deserialise(ReflectionHelper.BooleanType, 
                                                          new AttributeExtCollection(), cdrIn));            
            Assertion.AssertEquals(true, ser.Deserialise(ReflectionHelper.BooleanType, 
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
                ser.Deserialise(ReflectionHelper.BooleanType, new AttributeExtCollection(), cdrIn);
            } catch (Exception e) {
                inStream.Close();
                throw e;
            }
        }
        
        public void TestIorDeserialisation() {
            IiopClientChannel testChannel = new IiopClientChannel();
            ChannelServices.RegisterChannel(testChannel);
        
            MemoryStream inStream = new MemoryStream();
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
            inStream.Write(testIor, 0, testIor.Length);
            inStream.Seek(0, SeekOrigin.Begin);
            CdrInputStreamImpl cdrIn = new CdrInputStreamImpl(inStream);
            cdrIn.ConfigStream(0, new GiopVersion(1, 2));            
            
            Serialiser ser = new ObjRefSerializer();
            object result = ser.Deserialise(typeof(omg.org.CosNaming.NamingContext),
                                            new AttributeExtCollection(),
                                            cdrIn);
            Assertion.AssertNotNull("not correctly deserialised proxy for ior", result);
            Assertion.Assert(RemotingServices.IsTransparentProxy(result));
            Assertion.AssertEquals("IOR:000000000000002849444C3A6F6D672E6F72672F436F734E616D696E672F4E616D696E67436F6E746578743A312E3000000000010000000000000074000102000000000A3132372E302E302E3100041900000030AFABCB0000000022000003E80000000100000000000000010000000C4E616D655365727669636500000000034E43300A0000000100000001000000200000000000010001000000020501000100010020000101090000000100010100",
                                   RemotingServices.GetObjectUri((MarshalByRefObject)result));
            ChannelServices.UnregisterChannel(testChannel);
            
        }



    }

}
    
#endif
