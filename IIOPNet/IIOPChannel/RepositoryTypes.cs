/* RepositoryTypes.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 26.03.06  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using Ch.Elca.Iiop.Util;
using Ch.Elca.Iiop.Idl;

namespace omg.org.CORBA {

    // this file contains the corba mappings for interface repostiory
    
    [Serializable]
    [RepositoryID("IDL:omg.org/CORBA/DefinitionKind:1.0")]
    [IdlEnum]
    public enum DefinitionKind {
        dk_none, dk_all,
        dk_Attribute, dk_Constant, dk_Exception, dk_Interface,
        dk_Module, dk_Operation, dk_Typedef,
        dk_Alias, dk_Struct, dk_Union, dk_Enum,
        dk_Primitive, dk_String, dk_Sequence, dk_Array,
        dk_Repository,
        dk_Wstring, dk_Fixed,
        dk_Value, dk_ValueBox, dk_ValueMember,
        dk_Native, dk_AbstractInterface,
        dk_LocalInterface
    };
    
    [InterfaceTypeAttribute(IdlTypeInterface.ConcreteInterface)]
    [RepositoryID("IDL:omg.org/CORBA/IRObject:1.0")]
    public interface IRObject : IIdlEntity {
        
        DefinitionKind def_kind {
            get;
        }                
        
        // not needed
        // void destroy();
        
    }
    
    [InterfaceTypeAttribute(IdlTypeInterface.ConcreteInterface)]
    [RepositoryID("IDL:omg.org/CORBA/IDLType:1.0")]
    public interface IDLType : IIdlEntity, IRObject {
        
        omg.org.CORBA.TypeCode type {
            get;
        }
    }
    
    [ExplicitSerializationOrdered()]
    [RepositoryID("IDL:omg.org/CORBA/StructMember:1.0")]
    [Serializable]
    [IdlStruct]
    public struct StructMember : IIdlEntity {
        
        /// <summary>
        /// constructor used for type code operations.
        /// </summary>
        /// <param name="name">the name of this member</param>
        /// <param name="type">the typecode for this member</param>
        public StructMember(string name, TypeCode type) {
            this.name = name;
            this.type = type;
            this.type_def = null;
        }
        
        [ExplicitSerializationOrderNr(0)]
        [StringValue()]
        [WideCharAttribute(false)]
        public string name;
        
        [ExplicitSerializationOrderNr(1)]
        public omg.org.CORBA.TypeCode type;
        
        [ExplicitSerializationOrderNr(2)]
        /// <remarks>not used for typecode opertions</remarks>
        public IDLType type_def;

    }
    
    [ExplicitSerializationOrdered()]
    [RepositoryID("IDL:omg.org/CORBA/ValueMember:1.0")]
    [Serializable]
    [IdlStruct]
    public struct ValueMember : IIdlEntity {

        /// <summary>
        /// constructor used for type code operations.
        /// </summary>
        public ValueMember(string name, TypeCode type, short access) {
            this.name = name;
            this.type = type;
            this.access = access;
            this.type_def = null;
            this.version = null;
            this.defined_in = null;
            this.id = null;
        }
        
        /// <summary>
        /// the name of this member
        /// </summary>
        [ExplicitSerializationOrderNr(0)]
        [StringValue()]
        [WideCharAttribute(false)]
        public string name;
        
        /// <summary>
        /// the type of this member
        /// </summary>
        [ExplicitSerializationOrderNr(1)]
        public omg.org.CORBA.TypeCode type;
        
        /// <summary>
        /// the visibility of this value member
        /// </summary>
        [ExplicitSerializationOrderNr(2)]
        public short access;
        
        /// <remarks>not used for typecode opertions</remarks>
        [ExplicitSerializationOrderNr(3)]
        public IDLType type_def;
        
        /// <remarks>not used for typecode opertions</remarks>
        [ExplicitSerializationOrderNr(4)]
        public string version;
        
        /// <remarks>not used for typecode opertions</remarks>
        [ExplicitSerializationOrderNr(5)]
        public string defined_in;
        
        /// <remarks>not used for typecode opertions</remarks>
        [ExplicitSerializationOrderNr(6)]
        public string id;
        
    }        

    
}
