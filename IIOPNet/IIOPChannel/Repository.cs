/* Repository.cs
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
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using Ch.Elca.Iiop.Util;
using Corba;
using omg.org.CORBA;

namespace Ch.Elca.Iiop.Idl {

    /// <summary>
    /// Summary description for Repository.
    /// </summary>
    public class Repository {

        /// <summary>cache last loaded types</summary>
        private class TypeCache {
            
            #region IFields

            private Type m_place1 = null;
            private Type m_place2 = null;

            #endregion IFields
            #region IConstructors
                        
            public TypeCache() {
            }

            #endregion IConstructors
            #region IMethods

            public void Cache(Type type) {
                lock(this) {
                    m_place2 = m_place1;
                    m_place1 = type;
                }
            }

            public Type GetType(string clsName) {
                lock(this) {
                    if ((m_place1 != null) && (m_place1.FullName.Equals(clsName))) { 
                        return m_place1; 
                    }
                    if ((m_place2 != null) && (m_place2.FullName.Equals(clsName))) { 
                        Type result = m_place2;
                        m_place2 = m_place1;
                        m_place1 = result;
                        return result; 
                    }
                }
                return null;
            }

            #endregion IMethods

        }

        #region IConstructors

        private Repository() {
        }

        #endregion IConstructors
        #region SFields

        private static AssemblyCache s_asmCache = AssemblyCache.GetSingleton();
        private static TypeCache s_typeCache = new TypeCache();

        // for efficiency reason: the evaluation of the following expressions is cached
        private static Type s_repIdAttrType = typeof(RepositoryIDAttribute);
        private static Type s_supInterfaceAttrType = typeof(SupportedInterfaceAttribute);




        #endregion SFields
        #region SMethods

        #region rep-id parsing

        /// <summary>
        /// gets a CLS Type for the repository-id
        /// </summary>
        public static Type GetTypeForId(string repId) {
            if ((repId == String.Empty) ||
                (repId == "IDL:omg.org/CORBA/Object:1.0")) {
                return ReflectionHelper.MarshalByRefObjectType;
            }           
            
            string typeName = GetTypeNameForId(repId);
            if (typeName != null) {
                // now try to load the type:
                Type result = LoadType(typeName);
                if ((result == null) && (repId.StartsWith("IDL"))) {
                    // check, if type can be found, if it's assumed, that repId represents a type, which was
                    // mapped from IDL to CLS and a name mapping special case prevented the type from being found.
                    string alternativeTypeName = GetTypeNameForIDLId(repId, true);
                    result = LoadType(alternativeTypeName);
                }
                return result;
            } else {
                return null;
            }
        }

        /// <summary>
        /// gets the fully qualified type name for the repository-id
        /// </summary>
        internal static string GetTypeNameForId(string repId) {
            if (repId.StartsWith("RMI")) {
                return GetTypeNameForRMIId(repId);
            } else if (repId.StartsWith("IDL")) {
                return GetTypeNameForIDLId(repId, false);
            } else {
                return null; // unknown
            }
        }

        /// <summary>
        /// gets the CLS type name represented by the IDL-id
        /// </summary>
        /// <param name="idlID"></param>
        /// <param name="assumeMappedFromIdl">should assume, that type represented by idlid is mapped from IDL to CLS</param>
        /// <returns>
        /// The typename for the idlID
        /// </returns>
        private static string GetTypeNameForIDLId(string idlID, bool assumeMappedFromIdl) {
            idlID = idlID.Substring(4);
            if (idlID.IndexOf(":") < 0) { 
                // invalid repository id: idlID
                throw new INV_IDENT(9901, CompletionStatus.Completed_MayBe);
            }
            string typeName = idlID.Substring(0, idlID.IndexOf(":"));
            typeName = IdlNaming.MapIdlRepIdTypePartToClsName(typeName, assumeMappedFromIdl);
            return typeName;
        }
        /// <summary>
        /// gets the CLS type name represented by the RMI-id
        /// </summary>
        /// <param name="rmiID"></param>
        /// <returns></returns>
        private static string GetTypeNameForRMIId(string rmiID) {
            rmiID = rmiID.Substring(4);
            string typeName = "";
            if (rmiID.IndexOf(":") >= 0) {
                typeName = rmiID.Substring(0, rmiID.IndexOf(":"));
            } else {
                typeName = rmiID.Substring(0);
            }
            // check for array type
            if (typeName.StartsWith("[")) {
                string elemType = typeName.TrimStart(Char.Parse("["));
                if ((elemType == null) || (elemType.Length == 0)) { 
                    // invalid rmi-repository-id: typeName
                    throw new INV_IDENT(10002, CompletionStatus.Completed_MayBe);
                }
                int arrayRank = typeName.Length - elemType.Length; // array rank = number of [ - characters
                // parse the elem-type, which is in RMI-ID format
                string elemNameSpace;
                string unqualElemType = ParseRMIArrayElemType(elemType, out elemNameSpace);
                if (elemNameSpace.Length > 0) { 
                    elemNameSpace = "." + elemNameSpace; 
                } 
                unqualElemType = ResolveRmiInnerTypeMapping(unqualElemType);
                // determine name of boxed value type
                typeName = "org.omg.boxedRMI" + elemNameSpace + ".seq" + arrayRank + "_" + unqualElemType;
                Debug.WriteLine("mapped rmi id to boxed value type name:" + typeName);
            } else {
                if (typeName.LastIndexOf(".") >= 0)  {
                    int lastPIndex = typeName.LastIndexOf(".");
                    string elemNamespace = typeName.Substring(0, lastPIndex);
                    string unqualName = typeName.Substring(lastPIndex + 1);
                    if (unqualName.StartsWith("_")) {
                        // rmi mapping adds a J before a class name starting with _
                        typeName = elemNamespace + ".J" + unqualName;
                    }
                } else {
                    if (typeName.StartsWith("_")) {
                        // rmi mapping adds a J before a class name starting with _
                        typeName = "J" + typeName;
                    }
                }
                typeName = ResolveRmiInnerTypeMapping(typeName);
                
                // do name mapping (e.g. resolve clashes with CLS keywords)
                typeName = IdlNaming.MapRmiNameToClsName(typeName);
            }
            
            return typeName;
        }
        
        private static string ResolveRmiInnerTypeMapping(string typeName) {            
            return typeName.Replace(@"\U0024", "__");            
        }
        
        /// <param name="elemNamespace">the namespace of the elementType</param>
        /// <returns> the unqualified elem-type name </returns>
        private static string ParseRMIArrayElemType(string rmiElemType, out string elemNamespace) {
            // first character in elemType determines what kind of type
            char firstChar = Char.Parse(rmiElemType.Substring(0, 1));
            elemNamespace = ""; // for primitve types, this is the correct namespace
            switch (firstChar) {
                case 'I':
                    return "long";
                case 'Z':
                    return "boolean";
                case 'B':
                    return "octet";
                case 'C':
                    return "wchar";
                case 'D':
                    return "double";
                case 'F':
                    return "float";
                case 'J':
                    return "long_long";
                case 'S':
                    return "short";
                case 'L':
                    if (rmiElemType.Length <= 1) { 
                        // invalid element type in RMI array repository id"
                        throw new INV_IDENT(10004, CompletionStatus.Completed_MayBe);
                    }
                    string elemTypeName = rmiElemType.Substring(1);
                    elemTypeName = elemTypeName.TrimEnd(Char.Parse(";"));
                    string unqualName = "";
                    if (elemTypeName.LastIndexOf(".") < 0)  {
                        elemNamespace = "";
                        unqualName = elemTypeName;
                    } else {
                        int lastPIndex = elemTypeName.LastIndexOf(".");
                        elemNamespace = elemTypeName.Substring(0, lastPIndex);
                        unqualName = elemTypeName.Substring(lastPIndex + 1);
                    }
                    if (elemNamespace.Equals("java.lang") && (unqualName.Equals("String"))) {
                        // special case: map to CORBA.WStringValue
                        elemNamespace = "CORBA";
                        unqualName = "WStringValue";
                    } else {
                        // map rmi name to cls name, handle e.g. clashes with cls keywords
                        if (unqualName.StartsWith("_")) {
                            unqualName = "J" + unqualName; // rmi mapping adds a J before a class name starting with _
                        }
                        unqualName = IdlNaming.MapRmiNameToClsName(unqualName);
                        elemNamespace = IdlNaming.MapRmiNameToClsName(elemNamespace);
                        if (unqualName.StartsWith("_")) {
                            // remove _ added by IDL to CLS mapping, because class name follows a seq<n>_, therefore _ not added
                            // in IDL to CLS mapping
                            unqualName = unqualName.Substring(1);
                        }
                    }
                    return unqualName;
                default:
                    // invalid element type identifier in RMI array repository id: firstChar
                    throw new INV_IDENT(10003, CompletionStatus.Completed_MayBe);
            }
        }

        #endregion rep-id parsing
        #region rep-id creation
        /// <summary>
        /// gets the repository id for a CLS type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetRepositoryID(Type type) {
            object[] attr = type.GetCustomAttributes(s_repIdAttrType, true);    
            if (attr != null && attr.Length > 0) {
                RepositoryIDAttribute repIDAttr = (RepositoryIDAttribute) attr[0];
                return repIDAttr.Id;
            }
            attr = type.GetCustomAttributes(s_supInterfaceAttrType, true);
            if (attr != null && attr.Length > 0) {
                SupportedInterfaceAttribute repIDFrom = (SupportedInterfaceAttribute) attr[0];
                Type fromType = repIDFrom.FromType;
                if (fromType.Equals(type)) { 
                    throw new INTERNAL(1701, CompletionStatus.Completed_MayBe); 
                }
                return GetRepositoryID(fromType);
            }
            
            // no Repository ID attribute on type, have to create an ID:
            string idlName = IdlNaming.MapFullTypeNameToIdlRepIdTypePart(type);
            return "IDL:" + idlName + ":1.0"; // TODO: versioning?
        }

        #endregion rep-id creation
        #region loading types



        /// <summary>
        /// searches for the CLS type with the specified fully qualified name 
        /// in all accessible assemblies
        /// </summary>
        /// <param name="clsTypeName">the fully qualified CLS type name</param>
        /// <returns></returns>
        public static Type LoadType(string clsTypeName) {
            Type foundType = s_typeCache.GetType(clsTypeName);
            if (foundType != null) { 
                return foundType; 
            }
            // not in cache, load from asm
            foundType = LoadTypeFromAssemblies(clsTypeName);
            if (foundType == null) { // check for nested type
                foundType = LoadNested(clsTypeName);
            }
            if (foundType == null) { // check if accessible with Type.GetType
                foundType = Type.GetType(clsTypeName, false);
            }

            if (foundType != null) {
                s_typeCache.Cache(foundType);
            }
            return foundType;
        }

        private static Type LoadTypeFromAssemblies(string clsTypeName) {
            Type foundType = null;
            Assembly[] cachedAsms = s_asmCache.CachedAssemblies;
            for (int i = 0; i < cachedAsms.Length; i++) {
                foundType = cachedAsms[i].GetType(clsTypeName);
                if (foundType != null) { 
                    break; 
                }
            }
            if (foundType == null) {
                // check if it's a dynamically created type for a CLS type which is mapped to a boxed value type
                BoxedValueRuntimeTypeGenerator singleton = BoxedValueRuntimeTypeGenerator.GetSingleton();
                foundType = singleton.RetrieveType(clsTypeName);
                if (foundType == null) {
                    // check in Types created dynamically for type-codes:
                    TypeFromTypeCodeRuntimeGenerator typeCodeGen = TypeFromTypeCodeRuntimeGenerator.GetSingleton();
                    foundType = typeCodeGen.RetrieveType(clsTypeName);
                }
            }
            
            return foundType;
        }    


        private static Type LoadNested(string clsTypeName) {
            if (clsTypeName.IndexOf(".") < 0) { return null; }
            string nesterTypeName = clsTypeName.Substring(0, clsTypeName.LastIndexOf("."));
            string nestedType = clsTypeName.Substring(clsTypeName.LastIndexOf(".")+1);
            string name = nesterTypeName + "_package." + nestedType;
            Type foundType = LoadTypeFromAssemblies(name);
            if (foundType == null) { // check access via Type.GetType
                Type.GetType(name, false);
            }
            return foundType;
        }
        
        /// <summary>
        /// loads the boxed value type for the BoxedValueAttribute
        /// </summary>
        public static Type GetBoxedValueType(BoxedValueAttribute attr) {
            string repId = attr.RepositoryId; 
            Debug.WriteLine("getting boxed value type: " + repId);
            Type resultType = GetTypeForId(repId);
            return resultType;
        }

        /// <summar>load or create a boxed value type for a .NET array, which is mapped to an IDL boxed value type through the CLS to IDL mapping</summary>
        /// <remarks>this method is not called for IDL Boxed value types, mapped to a CLS array, for those the getBoxedValueType method is responsible</remarks>
        public static Type GetBoxedArrayType(Type clsArrayType) {
            BoxedValueRuntimeTypeGenerator gen = BoxedValueRuntimeTypeGenerator.GetSingleton();
            // convert a .NET true moredim array type to an array of array of ... type
            if (clsArrayType.GetArrayRank() > 1) {
                clsArrayType = BoxedArrayHelper.CreateNestedOneDimType(clsArrayType);
            }

            return gen.GetOrCreateBoxedTypeForArray(clsArrayType);
        }
        
        #endregion loading types 
        #region for typecodes

        /// <summary>
        /// creates a CORBA type code for a CLS type
        /// </summary>
        /// <returns>the typecode for the CLS type</returns>
        internal static TypeCodeImpl CreateTypeCodeForType(Type forType,  
                                                          AttributeExtCollection attributes) {
            return CreateTypeCodeForTypeInternal(forType, attributes, new TypeCodeCreater());
        }
        
        /// <summary>used by type code creating methods</summary>
        internal static TypeCodeImpl CreateTypeCodeForTypeInternal(Type forType, AttributeExtCollection attributes,
                                                                   TypeCodeCreater typeCodeCreator) {
            if (forType != null) {                        
                ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
                return (TypeCodeImpl)mapper.MapClsType(forType, attributes, typeCodeCreator);
            } else {
                // if no type info present, map to null typecode; the case can't be handled by cls to idl mapper
                return new NullTC();
            }
        }

        /// <summary>gets the CLS type for the Typecode</summary>
        public static Type GetTypeForTypeCode(omg.org.CORBA.TypeCode typeCode) {
            if (!(typeCode is omg.org.CORBA.TypeCodeImpl)) { 
                throw new INTERNAL(567, CompletionStatus.Completed_MayBe); 
            } else {
                return (typeCode as TypeCodeImpl).GetClsForTypeCode();
            }
        }
        
        public static AttributeExtCollection GetAttrsForTypeCode(omg.org.CORBA.TypeCode typeCode) {
            if (!(typeCode is omg.org.CORBA.TypeCodeImpl)) { 
                return AttributeExtCollection.EmptyCollection; 
            } else {
                return (typeCode as TypeCodeImpl).GetClsAttributesForTypeCode();
            }
        }
        #endregion for typecodes
        #endregion SMethods

    }

    /// <summary>
    /// create a type-code for the cls-Type mapped to the specified IDL-type
    /// </summary>
    internal class TypeCodeCreater : MappingAction {
        
        #region Types
        
        private class TypecodeForTypeKey {
        
            #region IFields
            
            private Type m_type;
            private AttributeExtCollection m_attributes;
            
            #endregion IFields
            #region IConstructors
        
            public TypecodeForTypeKey(Type forType, AttributeExtCollection
                                      attrs) {
                if ((forType == null) || (attrs == null)) {
                    throw new INTERNAL(801, CompletionStatus.Completed_MayBe);
                }
                m_type = forType;
                m_attributes = attrs;
            }
        
            #endregion IConstructors
            #region IProperties
            
            public Type ForType {
                get {
                    return m_type;                    
                }
            }
            
            public AttributeExtCollection Attributes {
                get {
                    return m_attributes;
                }
            }
            
            #endregion IProperties
            #region IMethods
        
            public override bool Equals(object other) {
                if (!(other is TypecodeForTypeKey)) {
                    return false;
                }
                return (m_type.Equals(((TypecodeForTypeKey)other).m_type) &&
                        m_attributes.Equals(((TypecodeForTypeKey)other).m_attributes));
            }
            
            public override int GetHashCode() {
                return m_type.GetHashCode() ^ 
                       m_attributes.GetHashCode();
            }
            
            #endregion IMethods
        
        }
        
        #endregion Types

        #region Constants

        private const short CONCRETE_VALUE_MOD = 0;
        private const short ABSTRACT_VALUE_MOD = 2;

        private const short VISIBILITY_PRIVATE = 0;
        private const short VISIBILITY_PUBLIC = 1;

        #endregion Constants
        #region IFields
        
        private IDictionary m_alreadyCreatedTypeCodes = new Hashtable();
        
        #endregion IFields
        #region IMethods
        
        private TypeCodeImpl CreateOrGetTypeCodeForType(Type forType,
                                                        AttributeExtCollection attributes) {
            TypecodeForTypeKey key = new TypecodeForTypeKey(forType, attributes);
            if (m_alreadyCreatedTypeCodes[key] == null) {
                return Repository.CreateTypeCodeForTypeInternal(forType, attributes, this);
            } else {
                return (TypeCodeImpl)m_alreadyCreatedTypeCodes[key];
            }
        }
        
        /// <summary>
        /// used for recursive type code creation
        /// </summary>
        private void RegisterCreatedTypeCodeForType(Type forType,
                                                    AttributeExtCollection attributes,
                                                    TypeCodeImpl typeCode) {
            TypecodeForTypeKey key = new TypecodeForTypeKey(forType, attributes);    
            m_alreadyCreatedTypeCodes[key] = typeCode;
        }
        
        #region Implementation of MappingAction
        public object MapToIdlStruct(Type clsType) {
            StructTC result = new StructTC();
            RegisterCreatedTypeCodeForType(clsType, AttributeExtCollection.EmptyCollection,
                                           result);
            
            FieldInfo[] members = ReflectionHelper.GetAllDeclaredInstanceFields(clsType);
            StructMember[] structMembers = new StructMember[members.Length];
            for (int i = 0; i < members.Length; i++) {                
                omg.org.CORBA.TypeCode memberType = 
                    CreateOrGetTypeCodeForType(members[i].FieldType,
                                               ReflectionHelper.GetCustomAttriutesForField(members[i], 
                                                                                           true));
                structMembers[i] = new StructMember(members[i].Name, memberType);
            }
            result.Initalize(Repository.GetRepositoryID(clsType), 
                             IdlNaming.ReverseIdlToClsNameMapping(clsType.Name),
                             structMembers);
            return result;
        }
        public object MapToIdlUnion(Type clsType) {
            UnionTC result = new UnionTC();            
            RegisterCreatedTypeCodeForType(clsType, AttributeExtCollection.EmptyCollection,
                                           result);
            
            // first get discriminator type
            FieldInfo discriminator = clsType.GetField(UnionGenerationHelper.DISCR_FIELD_NAME, 
                                                       BindingFlags.Instance | 
                                                       BindingFlags.DeclaredOnly |
                                                       BindingFlags.NonPublic);
            omg.org.CORBA.TypeCode discrTypeCode = 
                CreateOrGetTypeCodeForType(discriminator.FieldType, 
                                           ReflectionHelper.GetCustomAttriutesForField(discriminator,
                                                                                       true));            
            // get the methods used for typecode creation
            MethodInfo getCoveredDiscrMethod = clsType.GetMethod(UnionGenerationHelper.GET_COVERED_DISCR_VALUES,
                                                                 BindingFlags.Static | BindingFlags.NonPublic |
                                                                 BindingFlags.DeclaredOnly);
            MethodInfo getDefaultFieldMethod = clsType.GetMethod(UnionGenerationHelper.GET_DEFAULT_FIELD,
                                                                 BindingFlags.Static | BindingFlags.NonPublic |
                                                                 BindingFlags.DeclaredOnly);
            MethodInfo getFieldForDiscrVal = clsType.GetMethod(UnionGenerationHelper.GET_FIELD_FOR_DISCR_METHOD,
                                                               BindingFlags.Static | BindingFlags.NonPublic |
                                                               BindingFlags.DeclaredOnly);

            // get all discriminator values used in switch-cases
            object[] coveredDiscrs = (object[])getCoveredDiscrMethod.Invoke(null, new object[0]);
            if (coveredDiscrs == null) {
                throw new INTERNAL(898, CompletionStatus.Completed_MayBe);
            }
                        
            FieldInfo defaultField = (FieldInfo)getDefaultFieldMethod.Invoke(null, new object[0]);
            
            UnionSwitchCase[] cases = null;
            int defaultCaseNumber = -1; // no default case
            if (defaultField != null) {
                cases = new UnionSwitchCase[coveredDiscrs.Length + 1];
                omg.org.CORBA.TypeCode elemType = 
                    CreateOrGetTypeCodeForType(defaultField.FieldType, 
                                               ReflectionHelper.GetCustomAttriutesForField(defaultField, 
                                                                                           true));
                // create a default value of type discriminiator type, because of possible discriminator types, this 
                // is possible with Activator.CreateInstance ...
                object dummyValue = null;
                try {
                    dummyValue = Activator.CreateInstance(discriminator.FieldType);
                } catch (Exception) {
                    throw new MARSHAL(881, CompletionStatus.Completed_MayBe);
                }
                cases[coveredDiscrs.Length] = new UnionSwitchCase(dummyValue, defaultField.Name.Substring(2),
                                                                  elemType);
                defaultCaseNumber = coveredDiscrs.Length;
            } else {
                cases = new UnionSwitchCase[coveredDiscrs.Length];
            }
            
            // add a UnionSwitchCase to typecode for every discriminator value used
            for (int i = 0; i < coveredDiscrs.Length; i++) {
                FieldInfo caseField = (FieldInfo)getFieldForDiscrVal.Invoke(null, new object[] { coveredDiscrs[i] });
                if (caseField == null) {
                    throw new INTERNAL(898, CompletionStatus.Completed_MayBe);
                }
                omg.org.CORBA.TypeCode elemType = 
                    CreateOrGetTypeCodeForType(caseField.FieldType, 
                                               ReflectionHelper.GetCustomAttriutesForField(caseField, true));
                // extract name of element field: strip m_
                UnionSwitchCase switchCase = new UnionSwitchCase(coveredDiscrs[i], caseField.Name.Substring(2),
                                                                 elemType);
                cases[i] = switchCase;
            }                                                                                                
            result.Initalize(Repository.GetRepositoryID(clsType),            
                             IdlNaming.ReverseIdlToClsNameMapping(clsType.Name),
                             discrTypeCode, defaultCaseNumber, cases);
            return result;
        }
        
        public object MapToIdlAbstractInterface(Type clsType) {
            AbstractIfTC result =
                new AbstractIfTC(Repository.GetRepositoryID(clsType), 
                                 IdlNaming.ReverseIdlToClsNameMapping(clsType.Name));
            RegisterCreatedTypeCodeForType(clsType, AttributeExtCollection.EmptyCollection,
                                           result);
            return result;
            
        }
        public object MapToIdlLocalInterface(Type clsType) {
            LocalIfTC result = new LocalIfTC(Repository.GetRepositoryID(clsType), 
                                             IdlNaming.ReverseIdlToClsNameMapping(clsType.Name));
            RegisterCreatedTypeCodeForType(clsType, AttributeExtCollection.EmptyCollection,
                                           result);
            return result;            
        }
        
        public object MapToIdlConcreteInterface(Type clsType) {
            ObjRefTC result = new ObjRefTC(Repository.GetRepositoryID(clsType),
                                           IdlNaming.ReverseIdlToClsNameMapping(clsType.Name));
            RegisterCreatedTypeCodeForType(clsType, AttributeExtCollection.EmptyCollection,
                                           result);
            return result;            
        }
        public object MapToIdlConcreateValueType(Type clsType) {
            omg.org.CORBA.TypeCode baseTypeCode;
            if (clsType.BaseType.Equals(ReflectionHelper.ObjectType) || 
                clsType.BaseType.Equals(typeof(System.ComponentModel.MarshalByValueComponent))) {
                baseTypeCode = new NullTC();
            } else {
                baseTypeCode = CreateOrGetTypeCodeForType(clsType.BaseType, 
                                                          AttributeExtCollection.EmptyCollection);
            }
            ValueTypeTC result = new ValueTypeTC();
            RegisterCreatedTypeCodeForType(clsType, AttributeExtCollection.EmptyCollection,
                                           result);                        
            
            // create the TypeCodes for the members
            FieldInfo[] members = ReflectionHelper.GetAllDeclaredInstanceFields(clsType);
            ValueTypeMember[] valueMembers = new ValueTypeMember[members.Length];
            for (int i = 0; i < members.Length; i++) {
                omg.org.CORBA.TypeCode memberType = CreateOrGetTypeCodeForType(members[i].FieldType, 
                                                        ReflectionHelper.GetCustomAttriutesForField(members[i], 
                                                                                                    true));
                short visibility;
                if (members[i].IsPrivate) { 
                    visibility = VISIBILITY_PRIVATE; 
                } else { 
                    visibility = VISIBILITY_PUBLIC; 
                }
                valueMembers[i] = new ValueTypeMember(members[i].Name, memberType, visibility);
            }
            result.Initalize(Repository.GetRepositoryID(clsType),
                             IdlNaming.ReverseIdlToClsNameMapping(clsType.Name),
                             valueMembers, baseTypeCode, CONCRETE_VALUE_MOD);
            return result;
        }
        public object MapToIdlAbstractValueType(Type clsType) {                       
            omg.org.CORBA.TypeCode baseTypeCode;
            if (clsType.BaseType.Equals(ReflectionHelper.ObjectType) || 
                clsType.BaseType.Equals(typeof(System.ComponentModel.MarshalByValueComponent))) {
                baseTypeCode = new NullTC();
            } else {
                baseTypeCode = CreateOrGetTypeCodeForType(clsType.BaseType, 
                                   AttributeExtCollection.EmptyCollection);
            }
            ValueTypeTC result = new ValueTypeTC();            
            RegisterCreatedTypeCodeForType(clsType, AttributeExtCollection.EmptyCollection,
                                           result);
            result.Initalize(Repository.GetRepositoryID(clsType),
                             IdlNaming.ReverseIdlToClsNameMapping(clsType.Name), 
                             new ValueTypeMember[0],
                             baseTypeCode, ABSTRACT_VALUE_MOD);
            return result;
        }
        private object MapToIdlBoxedValueType(Type clsType, bool isAlreadyBoxed,
                                              bool boxInAny) {
            // dotNetType is subclass of BoxedValueBase
            if (!clsType.IsSubclassOf(ReflectionHelper.BoxedValueBaseType)) {
                // mapper error: MapToIdlBoxedValue found incorrect type
                throw new INTERNAL(1929, CompletionStatus.Completed_MayBe);
            }
            Type boxedType;
            object[] attributesOnBoxed = new object[0];
            try {
                boxedType = (Type)clsType.InvokeMember(BoxedValueBase.GET_BOXED_TYPE_METHOD_NAME,
                                                       BindingFlags.InvokeMethod | BindingFlags.Public |
                                                       BindingFlags.NonPublic | BindingFlags.Static | 
                                                       BindingFlags.DeclaredOnly, 
                                                       null, null, new object[0]);

                attributesOnBoxed = (object[])clsType.InvokeMember(BoxedValueBase.GET_BOXED_TYPE_ATTRIBUTES_METHOD_NAME,
                                                            BindingFlags.InvokeMethod | BindingFlags.Public |
                                                            BindingFlags.NonPublic | BindingFlags.Static | 
                                                            BindingFlags.DeclaredOnly, 
                                                            null, null, new object[0]);
            } 
            catch (Exception) {
                // invalid type: clsType
                // static method missing or not callable:
                // BoxedValueBase.GET_BOXED_TYPE_METHOD_NAME
                throw new INTERNAL(1930, CompletionStatus.Completed_MayBe);
            }
            if (boxInAny) {
                omg.org.CORBA.TypeCode boxed = CreateOrGetTypeCodeForType(boxedType, 
                                                                          AttributeExtCollection.ConvertToAttributeCollection(attributesOnBoxed));
            
                ValueBoxTC result = new ValueBoxTC(Repository.GetRepositoryID(clsType), 
                                                   IdlNaming.ReverseIdlToClsNameMapping(clsType.Name),
                                                   boxed);
                RegisterCreatedTypeCodeForType(clsType, AttributeExtCollection.EmptyCollection, result);
                return result;                                
            } else {
                // don't use boxed form
                // therefore create a typecode for the type boxed inside
                // e.g. in case of a boxed sequence of int, there will be a idlsequence typecode with int as element type created.
                // e.g. in case, where a sequence of boxed valuetype is boxed, an idl sequence will be created containing a typecode
                // for the boxed type.
                omg.org.CORBA.TypeCodeImpl forBoxed = CreateOrGetTypeCodeForType(boxedType, 
                                                                                 AttributeExtCollection.ConvertToAttributeCollection(attributesOnBoxed));
                return forBoxed;                
            }                                                              
        }
        public object MapToIdlBoxedValueType(Type clsType, bool isAlreadyBoxed) {
            return MapToIdlBoxedValueType(clsType, isAlreadyBoxed,
                                          MappingConfiguration.Instance.UseBoxedInAny);
        }
        public object MapToIdlSequence(Type clsType, int bound, AttributeExtCollection allAttributes, AttributeExtCollection elemTypeAttributes) {
            // sequence should not contain itself! -> do not register typecode
            omg.org.CORBA.TypeCode elementTC = CreateOrGetTypeCodeForType(clsType.GetElementType(),
                                                   elemTypeAttributes);
            return new SequenceTC(elementTC, bound);
        }
        public object MapToIdlAny(Type clsType) {
            return new AnyTC();
        }
        public object MapToAbstractBase(Type clsType) {
            // no CLS type mapped to CORBA::AbstractBase
            throw new INTERNAL(1940, CompletionStatus.Completed_MayBe);
        }
        public object MapToValueBase(Type clsType) {
            // no CLS type mapped to CORBA::ValueBase
            throw new INTERNAL(1940, CompletionStatus.Completed_MayBe);
        }
        public object MapToWStringValue(Type clsType) {
            if (MappingConfiguration.Instance.UseBoxedInAny) {
                return MapToIdlBoxedValueType(clsType, false, true);
            } else {
                // don't use boxed form
                return new WStringTC(0);
            }
        }
        public object MapToStringValue(Type clsType) {
            if (MappingConfiguration.Instance.UseBoxedInAny) {
                return MapToIdlBoxedValueType(clsType, false, true);
            } else {                
                // don't use boxed form
                return new StringTC(0);
            }
        }
        public object MapException(Type clsType) {
        	// TODO: check this, generic user exception handling ...
            ExceptTC result = new ExceptTC();
            RegisterCreatedTypeCodeForType(clsType, AttributeExtCollection.EmptyCollection,
                                           result);
            
            FieldInfo[] members = ReflectionHelper.GetAllDeclaredInstanceFields(clsType);
            StructMember[] exMembers = new StructMember[members.Length];
            for (int i = 0; i < members.Length; i++) {                
                omg.org.CORBA.TypeCode memberType = CreateOrGetTypeCodeForType(members[i].FieldType,
                                                        ReflectionHelper.GetCustomAttriutesForField(members[i], 
                                                                                                    true));
                exMembers[i] = new StructMember(members[i].Name, memberType);
            }
            result.Initalize(Repository.GetRepositoryID(clsType), 
                             IdlNaming.ReverseIdlToClsNameMapping(clsType.Name),
                             exMembers);
            return result;
        }
        public object MapToIdlEnum(Type clsType) {                        
            string[] names = Enum.GetNames(clsType);
            EnumTC result = new EnumTC(Repository.GetRepositoryID(clsType), 
                                       IdlNaming.ReverseIdlToClsNameMapping(clsType.Name),
                                       names);
            RegisterCreatedTypeCodeForType(clsType, AttributeExtCollection.EmptyCollection,
                                           result);                        
            return result;

        }
        public object MapToIdlBoolean(Type clsType) {
            return new BooleanTC();
        }
        public object MapToIdlFloat(Type clsType) {
            return new FloatTC();
        }
        public object MapToIdlDouble(Type clsType) {
            return new DoubleTC();
        }
        public object MapToIdlShort(Type clsType) {
            return new ShortTC();
        }
        public object MapToIdlUShort(Type clsType) {
            return new UShortTC();
        }
        public object MapToIdlLong(Type clsType) {
            return new LongTC();
        }
        public object MapToIdlULong(Type clsType) {
            return new ULongTC();
        }
        public object MapToIdlLongLong(Type clsType) {
            return new LongLongTC();
        }
        public object MapToIdlULongLong(Type clsType) {
            return new ULongLongTC();
        }
        public object MapToIdlOctet(Type clsType) {
            return new OctetTC();
        }
        public object MapToIdlVoid(Type clsType) {
            return new VoidTC();
        }
        public object MapToIdlWChar(Type clsType) {
            return new WCharTC();
        }
        public object MapToIdlWString(Type clsType) {
            return new WStringTC(0); // no bound specified
        }
        public object MapToIdlChar(Type clsType) {
            return new CharTC();
        }
        /// <returns>an optional result of the mapping, null may be possible</returns>
        public object MapToIdlString(Type clsType) {
            return new StringTC(0); // no bound specified
        }
        
        public object MapToTypeDesc(Type clsType) {
            return new omg.org.CORBA.TypeCodeTC();
        }

        public object MapToTypeCode(Type clsType) {
            return new omg.org.CORBA.TypeCodeTC();
        }

        #endregion
        #endregion IMethods

    }
}
