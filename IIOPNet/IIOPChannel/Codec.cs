/* Policy.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 17.04.05  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
 * 
 * Copyright 2005 Dominic Ullmann
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
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Idl;

namespace omg.org.IOP {
 
    /// <summary>
    /// Encoding format: cdr encapsulation.
    /// </summary>
    public sealed class ENCODING_CDR_ENCAPS {
        
        #region Constants
        
        public const short ConstVal = 0;
        
        #endregion Constants
        #region IConstructors
        
        private ENCODING_CDR_ENCAPS() {
        }
        
        #endregion IConstructors        
        
    }
    
     
    /// <summary>
    /// The IORInfo allows IORInterceptor (on the server side) to components
    /// to an ior profile.
    /// </summary>
    [RepositoryID("IDL:omg.org/IOP/Codec:1.0")]
    [InterfaceType(IdlTypeInterface.LocalInterface)]
    public interface Codec {
        
        /// <summary>
        /// Convert the given any into an octet sequence based on the encoding format effective
        /// for this Codec.
        /// </summary>
        [ThrowsIdlException(typeof(omg.org.IOP.Codec_package.InvalidTypeForEncoding))]
        [return: IdlSequence(0L)]
        byte[] encode (object data);
        
        /// <summary>
        /// Decode the given octet sequence into an any based on the encoding format effective
        /// for this Codec.
        /// </summary>
        [ThrowsIdlException(typeof(omg.org.IOP.Codec_package.FormatMismatch))]
        object decode ([IdlSequence(0L)] byte[] data);

        /// <summary>
        /// Convert the given any into an octet sequence based on the encoding format effective
        /// for this Codec. Only the data from the any is encoded, not the TypeCode.
        /// </summary>
        [ThrowsIdlException(typeof(omg.org.IOP.Codec_package.InvalidTypeForEncoding))]
        [return: IdlSequence(0L)]        
        byte[] encode_value (object data);
        
        /// <summary>
        /// Decode the given octet sequence into an any based on the given TypeCode and the
        /// encoding format effective for this Codec.
        /// </summary>
        [ThrowsIdlException(typeof(omg.org.IOP.Codec_package.FormatMismatch))]
        [ThrowsIdlException(typeof(omg.org.IOP.Codec_package.TypeMismatch))]
        object decode_value ([IdlSequence(0L)] byte[] data,
                             omg.org.CORBA.TypeCode tc);
        
    }
    
    /// <summary>
    /// The Encoding structure defines the encoding format of a Codec. It details the
    /// encoding format, such as CDR Encapsulation encoding, and the major and minor
    /// versions of that format.
    /// </summary>    
    [Serializable()]
    [RepositoryID("IDL:omg.org/IOP/Encoding:1.0")]
    [IdlStruct()]
    [ExplicitSerializationOrdered()]
    public struct Encoding : IIdlEntity {
               
        [ExplicitSerializationOrderNr(0)]
        public short format;
        [ExplicitSerializationOrderNr(1)]
        public byte major_version;
        [ExplicitSerializationOrderNr(2)]
        public byte minor_version;
        
        public Encoding(short format, byte major_version, byte minor_version) {
            this.format = format;
            this.major_version = major_version;
            this.minor_version = minor_version;
        }        
        
    }
    
    /// <summary>
    /// Create a Codec of the given encoding.
    /// </summary>
    [RepositoryID("IDL:omg.org/IOP/CodecFactory:1.0")]
    [InterfaceType(IdlTypeInterface.LocalInterface)]    
    public interface CodecFactory {
        
        [ThrowsIdlException(typeof(omg.org.IOP.CodecFactory_package.UnknownEncoding))]
        Codec create_codec (Encoding enc);
        
    }
     
}
 
namespace omg.org.IOP.Codec_package {
 
    /// <summary>
    /// This exception is raised by encode or encode_value when the type is invalid for the
    /// encoding.
    /// </summary>
    [Serializable]
    [ExplicitSerializationOrdered()]
    public class InvalidTypeForEncoding : AbstractUserException {
     
        public InvalidTypeForEncoding() {
        }
        
        public InvalidTypeForEncoding(string reason) : base(reason) {
        }
        
        protected InvalidTypeForEncoding(System.Runtime.Serialization.SerializationInfo info,
                                         System.Runtime.Serialization.StreamingContext context) : base(info, context) {            
        }        
     
    }

     
    /// <summary>
    /// This exception is raised by decode or decode_value when the data in the octet
    /// sequence cannot be decoded into an any.
    /// </summary>
    [Serializable]
    [ExplicitSerializationOrdered()]
    public class FormatMismatch : AbstractUserException {
     
        public FormatMismatch() {
        }

        public FormatMismatch(string reason) : base(reason) {
        }
        
        protected FormatMismatch(System.Runtime.Serialization.SerializationInfo info,
                                 System.Runtime.Serialization.StreamingContext context) : base(info, context) {            
        }        
        
    }

     
    /// <summary>
    /// This exception is raised by decode_value when the given TypeCode does not match
    /// the given octet sequence.
    /// </summary>
    [Serializable]
    [ExplicitSerializationOrdered()]
    public class TypeMismatch : AbstractUserException {
     
        public TypeMismatch() {
        }

        public TypeMismatch(string reason) : base(reason) {
        }
        
        protected TypeMismatch(System.Runtime.Serialization.SerializationInfo info,
                               System.Runtime.Serialization.StreamingContext context) : base(info, context) {            
        }        
        
    }     
     
     
}
 
namespace omg.org.IOP.CodecFactory_package {
 
    /// <summary>
    /// raised, if the codec factory, cannot create a Codec of the given encoding.
    /// </summary>
    [Serializable]
    [ExplicitSerializationOrdered()]
    public class UnknownEncoding : AbstractUserException {
     
        public UnknownEncoding() {
        }
        
        public UnknownEncoding(string reason) : base(reason) {
        }
        
        protected UnknownEncoding(System.Runtime.Serialization.SerializationInfo info,
                                  System.Runtime.Serialization.StreamingContext context) : base(info, context) {            
        }        
         
    }
 
}
 
 
namespace Ch.Elca.Iiop.Interception {
    
    using omg.org.IOP;
    using Ch.Elca.Iiop.Cdr;
    using Ch.Elca.Iiop.Marshalling;
    using Ch.Elca.Iiop.Util;

    /// <summary>
    /// implementation of <see cref="omg.org.IOP.CodecFactory"></see>
    /// </summary>
    internal class CodecFactoryImpl : CodecFactory {
        
        private SerializerFactory m_serFactory;
        
        public CodecFactoryImpl(SerializerFactory serFactory) {
            m_serFactory = serFactory;
        }
        
        public Codec create_codec (Encoding enc) {
            GiopVersion version = new GiopVersion(enc.major_version, enc.minor_version);
            if (enc.format == omg.org.IOP.ENCODING_CDR_ENCAPS.ConstVal) {
                Codec impl = new CodecImplEncap(version, m_serFactory);
                return impl;
            } else {
                throw new omg.org.IOP.CodecFactory_package.UnknownEncoding();
            }
        }        
        
    }
    
    

    /// <summary>
    /// implementation of <see cref="omg.org.IOP.Codec"> for format ENCODING_CDR_ENCAPS.</see>
    /// </summary>
    internal class CodecImplEncap : Codec {
        
        #region IFields
        
        private GiopVersion m_version;
        private Serializer m_serializerForAnyType;
        private SerializerFactory m_serFactory;
        
        #endregion IFields
        #region IConstructors
        
        internal CodecImplEncap(GiopVersion version, SerializerFactory serFactory) {
            m_version = version;
            m_serFactory = serFactory;
            m_serializerForAnyType = 
                m_serFactory.Create(ReflectionHelper.ObjectType,
                                    AttributeExtCollection.EmptyCollection);
        }
        
        #endregion IConstructors
        #region IMethods
        
        /// <summary>
        /// <see cref="omg.org.IOP.Codec.encode"></see>
        /// </summary>
        [ThrowsIdlException(typeof(omg.org.IOP.Codec_package.InvalidTypeForEncoding))]
        [return: IdlSequence(0L)]
        public byte[] encode (object data) {
            CdrEncapsulationOutputStream outputStream = new CdrEncapsulationOutputStream(m_version);
            m_serializerForAnyType.Serialize(data, outputStream);
            return outputStream.GetEncapsulationData();
        }
        
        /// <summary>
        /// <see cref="omg.org.IOP.Codec.decode"></see>
        /// </summary>
        [ThrowsIdlException(typeof(omg.org.IOP.Codec_package.FormatMismatch))]
        public object decode ([IdlSequence(0L)] byte[] data) {
            CdrEncapsulationInputStream inputStream = new CdrEncapsulationInputStream(data, m_version);
            return m_serializerForAnyType.Deserialize(inputStream);
        }

        /// <summary>
        /// <see cref="omg.org.IOP.Codec.encode_value"></see>
        /// </summary>
        [ThrowsIdlException(typeof(omg.org.IOP.Codec_package.InvalidTypeForEncoding))]
        [return: IdlSequence(0L)]        
        public byte[] encode_value (object data) {
            CdrEncapsulationOutputStream outputStream = new CdrEncapsulationOutputStream(m_version);
            if (!(data is Any)) {
                Serializer ser =
                    m_serFactory.Create(data.GetType(), 
                                        AttributeExtCollection.EmptyCollection);
                ser.Serialize(data, outputStream);                                   
            } else {
                Type marshalAs = ((TypeCodeImpl)((Any)data).Type).GetClsForTypeCode();
                AttributeExtCollection marshalAsAttrs = 
                    ((TypeCodeImpl)((Any)data).Type).GetClsAttributesForTypeCode();
                Serializer ser =
                    m_serFactory.Create(marshalAs, 
                                        marshalAsAttrs);
                ser.Serialize(data, outputStream);
            }
            return outputStream.GetEncapsulationData();
        }
        
        /// <summary>
        /// <see cref="omg.org.IOP.Codec.decode_value"></see>
        /// </summary>
        [ThrowsIdlException(typeof(omg.org.IOP.Codec_package.FormatMismatch))]
        [ThrowsIdlException(typeof(omg.org.IOP.Codec_package.TypeMismatch))]
        public object decode_value ([IdlSequence(0L)] byte[] data,
                                    omg.org.CORBA.TypeCode tc) {
            CdrEncapsulationInputStream inputStream = new CdrEncapsulationInputStream(data, m_version);
            Type marshalAs = ((TypeCodeImpl)tc).GetClsForTypeCode();
            AttributeExtCollection marshalAsAttrs = 
                    ((TypeCodeImpl)tc).GetClsAttributesForTypeCode();            
            Serializer ser =
                    m_serFactory.Create(marshalAs, 
                                        marshalAsAttrs);                    
            return ser.Deserialize(inputStream);
        }

        #endregion IMethods
        
    }
     
     
}
