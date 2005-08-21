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
            Type result = null;
            string typeNameAssumeIdlMapped = GetTypeNameForId(repId, true);
            if (typeNameAssumeIdlMapped != null) {
                // now try to load the type:
                // check, if type with correct repository id can be found, 
                // if it's assumed, that repId represents a type, which was
                // mapped from IDL to CLS
                result = LoadType(typeNameAssumeIdlMapped);
                bool isRepIdCorrect = false;
                if (result != null) {
                    try {
                        isRepIdCorrect = GetRepositoryID(result) == repId;
                    } catch (Exception) {
                        // for types in construction, not supported -> ignore
                    }
                }
                if (result == null || !isRepIdCorrect) {
                    // check, if type can be found, if a native CLS type mapped to idl is assumed.
                    string typeNameAssumeCls = GetTypeNameForId(repId, false);
                    if (typeNameAssumeCls != null) {
                        result = LoadType(typeNameAssumeCls);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// gets the fully qualified type name for the repository-id
        /// </summary>
        internal static string GetTypeNameForId(string repId) {
            return GetTypeNameForId(repId, false);
        }

        /// <summary>
        /// gets the fully qualified type name for the repository-id
        /// </summary>
        private static string GetTypeNameForId(string repId, bool assumeMappedFromIdl) {
            if (repId.StartsWith("RMI")) {
                return GetTypeNameForRMIId(repId);
            } else if (repId.StartsWith("IDL")) {
                return GetTypeNameForIDLId(repId, assumeMappedFromIdl);
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
            object[] attr = type.GetCustomAttributes(ReflectionHelper.RepositoryIDAttributeType, true);    
            if (attr != null && attr.Length > 0) {
                RepositoryIDAttribute repIDAttr = (RepositoryIDAttribute) attr[0];
                return repIDAttr.Id;
            }
            attr = type.GetCustomAttributes(ReflectionHelper.SupportedInterfaceAttributeType, true);
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
            Debug.WriteLine("try to load type: " + clsTypeName);
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
                
        #endregion loading types 
        #region verifying types
        
        /// <summary>
        /// returns true, if interface type iorType is assignable to requiredType.
        /// If not verifable with static inheritance information, returns false.
        /// </summary>
        /// <param name="useTypeForId">returns the best local type to use for iorTypeId</param>
        public static bool IsInterfaceCompatible(Type requiredType, string iorTypeId, out Type useTypeForId) {            
            Type interfaceType;
            if (!iorTypeId.Equals(String.Empty)) { // empty string stands for CORBA::Object
                interfaceType = Repository.GetTypeForId(iorTypeId);
            } else {
                interfaceType = ReflectionHelper.MarshalByRefObjectType;
            }            
            // for requiredType MarshalByRefObject and omg.org.CORBA.IObject
            // everything is possible (i.e. every remote object type can be assigned to it),
            // the other requiredType types must be checked remote if not locally verifable
            if ((!requiredType.Equals(ReflectionHelper.MarshalByRefObjectType)) &&                
                (!requiredType.Equals(ReflectionHelper.IObjectType)) && 
                ((interfaceType == null) || (!requiredType.IsAssignableFrom(interfaceType)))) {
                // remote type not known or locally assignability not verifable
                useTypeForId = requiredType;
                return false;
            }
            // interface is assignable (or required type is compatible with all -> do a null check for this case)
            useTypeForId = (interfaceType != null ? interfaceType : requiredType);
            return true;            
        }
        
        #endregion verifying types
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

}
