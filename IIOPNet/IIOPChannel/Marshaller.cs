/* Marshaller.cs
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
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using Ch.Elca.Iiop.Cdr;
using Ch.Elca.Iiop.Idl;
using Ch.Elca.Iiop.Util;
using omg.org.CORBA;

namespace Ch.Elca.Iiop.Marshalling {
    

    /// <summary>
    /// The Marshaller is responsible for marshalling/unmarshalling items
    /// </summary>
    /// <remarks>
    /// The class Marshaller is used by the ParameterMarshaller to marshal / unmarshal the parameters. 
    /// 
    /// The Marshaller uses serializer classes for accomplishing its task of marshalling / unmarshalling values.
    /// </remarks>
    internal class Marshaller {

        #region SFields
        
        private static Marshaller s_singleton = new Marshaller();

        /// <summary>returns the serializer responsible for a mapping</summary>
        private static SerializerDetermination s_serDetermination = new SerializerDetermination();
        /// <summary>is responsible for the mapping CLS to IDL</summary>
        private static ClsToIdlMapper s_mapper = ClsToIdlMapper.GetSingleton();

        #endregion SFields
        #region IConstructors

        protected Marshaller() {
        }

        #endregion IConstructors
        #region SMethods

        internal static Marshaller GetSingleton() {
            return s_singleton;
        }

        #endregion SMethods
        #region IMethods

        /// <summary>
        /// Marshals items
        /// </summary>
        /// <remarks>
        /// Marshal uses the formal type and the attributes to decide, how to serialise the actual object.
        /// The attributes contain the addional information from the IDL to .NET mapping, if such attributes are present.
        /// 
        /// The marshaller looks up the correct serializer for serialising a value compatible with the formal type
        /// and delegates the serialisation work to the found serializer.
        /// </remarks>
        /// <param name="formal"></param>
        /// <param name="attributes"></param>
        /// <param name="actual"></param>
        /// <param name="targetStream"></param>
        internal void Marshal(Type formal, AttributeExtCollection attributes, object actual,
                            CdrOutputStream targetStream) {
            Debug.WriteLine("marshal, formal: " + formal);
            // determine the serialiser
            Serialiser serialiser = DetermineSerialiser(ref formal, ref attributes);
            Marshal(formal, attributes, serialiser, actual, targetStream);
        }

        /// <summary>marshals a paramter/field, using the specified serialiser</summary>
        /// <param name="attributes">the attributes used for further processing; the ones used to determine the serialiser are not part of the collection</param>
        /// <remarks>this method is available for efficieny reason; normally other overloaded method is used</remarks>
        protected void Marshal(Type formal, AttributeExtCollection attributes, Serialiser serialiser,
                               object actual, CdrOutputStream targetStream) {
            
            // check for plugged special mappings, e.g. CLS ArrayList -> java.util.ArrayList
            // --> if present, need to convert instance before serialising
            CustomMapperRegistry cReg = CustomMapperRegistry.GetSingleton();
            if ((actual != null) && (cReg.IsCustomMappingPresentForCls(actual.GetType()))) {
                ICustomMapper mapper = cReg.GetMappingForCls(actual.GetType()).Mapper;
                actual = mapper.CreateIdlForClsInstance(actual);
                // check, if mapped is instance is assignable to formal -> otherwise will not work on other side ...
                if (!formal.IsAssignableFrom(actual.GetType())) {
                    throw new BAD_PARAM(12310, CompletionStatus.Completed_MayBe);
                }
            }
            
            // check for .NET true moredimensional arrays, the must be converted before serialized:
            if ((actual != null) && (actual.GetType().IsArray) && (actual.GetType().GetArrayRank() > 1)) {
                actual = BoxedArrayHelper.ConvertMoreDimToNestedOneDim((Array)actual);
            }
            serialiser.Serialise(formal, actual, attributes, targetStream);
        }

        /// <summary>determines the serialiser responsible for a specified formal type and the parameterattributes attributes</summary>
        /// <param name="formal">The formal type. If formal is modified through mapper, result is returned in this parameter</param>
        /// <param name="attributes">the parameter/field attributes</param>
        /// <returns></returns>
        protected Serialiser DetermineSerialiser(ref Type formal, ref AttributeExtCollection attributes) {
            Serialiser serialiser = (Serialiser)s_mapper.MapClsTypeWithTransform(ref formal, ref attributes, s_serDetermination); // formal can be transformed
            if (serialiser == null) {
                // no serializer present for Type: formal
                Trace.WriteLine("no serialiser for Type: " + formal);
                throw new BAD_PARAM(9001, CompletionStatus.Completed_MayBe);
            }
            Debug.WriteLine("serialize formal type " + formal + " with : " + serialiser);
            return serialiser;
        }

        /// <summary>
        /// Unmarshal items
        /// </summary>
        /// <remarks>
        /// Unmarshal uses the formal type and the attributes to decide, how to deserialise the serialised object
        /// contained in the CDRStream. 
        /// The attributes contain the additional information from the IDL to .NET mapping, if such attributes are present. 
        /// The marshaller looks up the correct serializer for the formal type and attributes and delegates the deserialisation work to the found serializer.
        /// </remarks>
        /// <param name="formal"></param>
        /// <param name="attributes"></param>
        /// <param name="sourceStream"></param>
        /// <returns></returns>
        internal object Unmarshal(Type formal, AttributeExtCollection attributes, CdrInputStream sourceStream) {
            Debug.WriteLine("unmarshal, formal: " + formal);
            Type formalNew = formal;
            // determine the serialiser
            Serialiser serialiser = DetermineSerialiser(ref formalNew, ref attributes);
            return Unmarshal(formalNew, formal, attributes, serialiser, sourceStream);
        }

        /// <summary>unmarshals a parameter/field</summary>
        /// <param name="formalSer">the type to unmarshal determined by mapper</param>
        /// <param name="formalSig">the type in signature/field declaration/...</param>
        /// <param name="serializer">the seriliazer to use</param>
        /// <param name="attributes">the attributes used for further processing; the ones used to determine the serialiser are not part of the collection</param>
        /// <remarks>this method is available for efficieny reason; normally other overloaded method is used</remarks>
        protected object Unmarshal(Type formalSer, Type formalSig, AttributeExtCollection attributes,
                                   Serialiser serialiser, CdrInputStream sourceStream) {
            object result = serialiser.Deserialise(formalSer, attributes, sourceStream);
            
            // check for plugged special mappings, e.g. CLS ArrayList -> java.util.ArrayList
            // --> if present, need to convert instance after deserialising
            CustomMapperRegistry cReg = CustomMapperRegistry.GetSingleton();
            if ((result != null) && (cReg.IsCustomMappingPresentForIdl(result.GetType()))) {
                ICustomMapper mapper = cReg.GetMappingForIdl(result.GetType()).Mapper;
                result = mapper.CreateClsForIdlInstance(result);
                // check, if mapped instance is assignable to formal in CLS signature -> otherwise will not work.
                if (!formalSig.IsAssignableFrom(result.GetType())) {
                    throw new BAD_PARAM(12311, CompletionStatus.Completed_MayBe);
                }
            }
            
            if ((formalSig.IsArray) && (formalSig.GetArrayRank() > 1)) { // a true .NET moredimensional array
                if ((result != null) && (!result.GetType().IsArray)) {
                    throw new BAD_PARAM(9004, CompletionStatus.Completed_MayBe);
                }
                result = BoxedArrayHelper.ConvertNestedOneDimToMoreDim((Array)result);
            }

            return result;            
        }

        #endregion IMethods

    }

    /// <summary>
    /// a marshaller only usable for one type. It's more efficient to use this class, if more than one parameter/field of the same formal type should be marshalled/unmarshalled
    /// </summary>
    internal class MarshallerForType : Marshaller {

        #region IFields
        
        private Type m_formal;
        private Type m_formalToSer;
        private AttributeExtCollection m_attributes;
        private Serialiser m_ser;

        #endregion IFields
        #region IConstructors

        internal MarshallerForType(Type formal, AttributeExtCollection attributes) {
            m_formal = formal;
            m_formalToSer = formal;            
            m_ser = DetermineSerialiser(ref m_formalToSer, ref attributes);
            m_attributes = attributes; // the attributes passed, without the ones considered for the serialiser determination
        }

        #endregion IConstructors
        #region IMethods

        internal void Marshal(object actual, CdrOutputStream targetStream) {
            base.Marshal(m_formalToSer, 
                         m_attributes, // attributecollection can't be modified -> can pass it without copying
                         m_ser, actual, targetStream);
        }

        internal object Unmarshal(CdrInputStream sourceStream) {
            return base.Unmarshal(m_formalToSer, m_formal, 
                                  m_attributes, // attributecollection can't be modified -> can pass it without copying
                                  m_ser, sourceStream);
        }

        #endregion IMethods
    
    }

    /// <summary>determines the correct serializer for the mapping</summary>
    internal class SerializerDetermination : MappingAction {

        #region IFields
    
        // default serializer
        private Serialiser m_anySer = new AnySerializer();
        private Serialiser m_typeSer = new TypeSerializer();
        private Serialiser m_typeCodeSer = new TypeCodeSerializer();
        private AbstractInterfaceSerializer m_abstrInterfaceSer = new AbstractInterfaceSerializer();
        private AbstractValueSerializer m_abstrValueSer = new AbstractValueSerializer();
        private Serialiser m_marshalByRefSer = new ObjRefSerializer();
        private Serialiser m_marshalByValSer = new ValueObjectSerializer();
        private Serialiser m_boxedValueSer = new BoxedValueSerializer();
        private Serialiser m_enumSer = new EnumSerializer();
        private Serialiser m_seqSer_unbounded = new IdlSequenceSerializer(0);
        private Serialiser m_structSer = new IdlStructSerializer();
        private Serialiser m_unionSer = new IdlUnionSerializer();
        private Serialiser m_exceptSer = new ExceptionSerializer();

        // base type serialiser
        private Serialiser m_wideCharSer = new CharSerialiser(true);
        private Serialiser m_nonWideCharSer = new CharSerialiser(false);
        
        private Serialiser m_wideStringSer = new StringSerialiser(true);
        private Serialiser m_nonWideStringSer = new StringSerialiser(false);        
        
        private Serialiser m_byteSer = new ByteSerialiser();
        private Serialiser m_boolSer = new BooleanSerialiser();
        private Serialiser m_int16Ser = new Int16Serialiser();
        private Serialiser m_int32Ser = new Int32Serialiser();
        private Serialiser m_int64Ser = new Int64Serialiser();
        private Serialiser m_singleSer = new SingleSerialiser();
		private Serialiser m_doubleSer = new DoubleSerialiser();
        
        #endregion IFields
        #region IConstructors

        public SerializerDetermination() {
        }

        #endregion IConstructors
        #region IMethods
        
        #region Implementation of MappingAction
        public object MapToIdlStruct(System.Type clsType) {
            return m_structSer;
        }
        public object MapToIdlUnion(System.Type clsType) {
            return m_unionSer;
        }
        public object MapToIdlAbstractInterface(System.Type clsType) {
            return m_abstrInterfaceSer;
        }
        public object MapToIdlConcreteInterface(System.Type clsType) {
            return m_marshalByRefSer;
        }
        public object MapToIdlLocalInterface(System.Type clsType) {
            // local interfaces are non-marshable
            throw new MARSHAL(4, CompletionStatus.Completed_MayBe);
        }
        public object MapToIdlConcreateValueType(System.Type clsType) {
            return m_marshalByValSer;
        }
        public object MapToIdlAbstractValueType(System.Type clsType) {
            return m_abstrValueSer;
        }
        public object MapToIdlBoxedValueType(System.Type clsType, bool isAlreadyBoxed) {
            if (!isAlreadyBoxed) {
                // need boxing / unboxing of values
                return m_boxedValueSer;
            } else {
                // do serialize as value type
                return m_marshalByValSer;
            }
        }
        public object MapToIdlSequence(System.Type clsType, int bound, AttributeExtCollection allAttributes, AttributeExtCollection elemTypeAttributes) {
        	if (bound == 0) {
        		return m_seqSer_unbounded;
        	} else {
        		return new IdlSequenceSerializer(bound);
        	}        	
        }
        public object MapToIdlAny(System.Type clsType) {
            return m_anySer;
        }
        public object MapToAbstractBase(System.Type clsType) {
            return m_abstrInterfaceSer;
        }
        public object MapToValueBase(System.Type clsType) {
            return m_abstrValueSer;
        }
        public object MapException(System.Type clsType) {
            return m_exceptSer;
        }
        public object MapToIdlEnum(System.Type clsType) {
            return m_enumSer;
        }
        public object MapToWStringValue(System.Type clsType) {
            return m_boxedValueSer;
        }
        public object MapToStringValue(System.Type clsType) {
            return m_boxedValueSer;
        }
        public object MapToTypeDesc(System.Type clsType) {
            return m_typeSer;
        }
        public object MapToTypeCode(System.Type clsType) {
            return m_typeCodeSer;
        }
        public object MapToIdlBoolean(System.Type clsType) {
            return m_boolSer;
        }
        public object MapToIdlFloat(System.Type clsType) {
            return m_singleSer;
        }
        public object MapToIdlDouble(System.Type clsType) {
            return m_doubleSer;
        }
        public object MapToIdlShort(System.Type clsType) {
            return m_int16Ser;
        }
        public object MapToIdlUShort(System.Type clsType) {
            // no CLS type is mapped to UShort
            throw new INTERNAL(8702, CompletionStatus.Completed_MayBe);
        }
        public object MapToIdlLong(System.Type clsType) {
            return m_int32Ser;
        }
        public object MapToIdlULong(System.Type clsType) {
            // no CLS type is mapped to ULong
            throw new INTERNAL(8703, CompletionStatus.Completed_MayBe);
        }
        public object MapToIdlLongLong(System.Type clsType) {
            return m_int64Ser;
        }
        public object MapToIdlULongLong(System.Type clsType) {
            // no CLS type is mapped to ULongLong
            throw new INTERNAL(8703, CompletionStatus.Completed_MayBe);
        }
        public object MapToIdlOctet(System.Type clsType) {
            return m_byteSer;
        }
        public object MapToIdlVoid(System.Type clsType) {
            // void is not serializable
            throw new INTERNAL(8704, CompletionStatus.Completed_MayBe);
        }
        public object MapToIdlWChar(System.Type clsType) {
            return m_wideCharSer;
        }
        public object MapToIdlWString(System.Type clsType) {
            return m_wideStringSer;
        }
        public object MapToIdlChar(System.Type clsType) {
            return m_nonWideCharSer;
        }
        public object MapToIdlString(System.Type clsType) {
            return m_nonWideStringSer;
        }
    
        #endregion Implementation of MappingAction

        #endregion IMethods

    }
}
