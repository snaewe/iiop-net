/* IDLNaming.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Ch.Elca.Iiop.Util;

namespace Ch.Elca.Iiop.Idl {

    
    /// <summary>
    /// This class is responsible for realising the identifier (name) mapping
    ///  in the IDL to .NET and .NET to IDL mapping.
    /// </summary>
    public static class IdlNaming {

        #region SFields
        
        private static ArrayList s_clsKeywordList = new ArrayList();
        private static IDictionary<Type, string> s_clsMapSpecial = new Dictionary<Type, string>();
        
        private static ArrayList s_idlKeywordList = new ArrayList();

        private static IComparer s_keyWordComparer = new CaseInsensitiveComparer();
        
        /// <summary>used as helper to map type names to idl for Cls types; used inside method name mapping for overloaded methods and inside name construction for sequences typedef</summary>
        private static GenerationActionReference s_genIdlNameforClsTypeNoAnonSeq = new GenerationActionReference(false);
        
        #endregion SFields
        #region SConstructor

        static IdlNaming() {
            // creates the list of reserved CLS words
            InitClsKeyWordList();
            // creates the list of reserved IDL words
            InitIdlKeyWordList();
            // create mapping table for primitive types and special types
            InitMapClsToIdlSpecial();
        }

        #endregion SConstructor
        #region SMethods

        #region simple unqualified name mapping
        
        /// <summary>
        /// maps a simple Cls Name to an Idl name according to CLS to IDL spec.
        /// </summary>
        /// <param name="clsName">a simple unqualified cls name (e.g. a type-name or a method name)</param>
        /// <returns></returns>
        public static string MapClsNameToIdlName(string clsName) {
            string result = clsName;
            if (NameClashesWithIdlKeyWord(clsName)) {
                result = "_" + result;
            } else if (clsName.StartsWith("_")) {
                result = "N" + result;
            }
            // TODO: handle exception from chapter 2.2.4
            return result;
        }

        /// <summary>
        /// maps a simple Idl Name to a cls name according to IDL to CLS spec.
        /// </summary>
        /// <param name="clsName">a simple unqualified cls name (e.g. a type-name or a method name)</param>
        /// <returns></returns>
        public static string MapIdlNameToClsName(string idlName) {
            string result = idlName;
            if (NameClashesWithClsKeyWord(idlName)
                || !(char.IsLetter(idlName[0]) || idlName[0] == '_')) {
                result = "_" + result;
            }
            return result;
        }

        
        /// <summary>creates the Idl-name for a name, which was mapped from IDL to CLS</summary>
        internal static string ReverseIdlToClsNameMapping(string mappedNameInCls) {
            string result = mappedNameInCls;
            if (result.StartsWith("_")) {
                // remove leading underscore, according to section 3.2.3.1 in CORBA 2.3.1 standard
                // the underscore is not removed during name mapping, to handle idl id's which would be mapped
                // to a CLS keyword easily
                result = result.Substring(1);
            }
            return result;
        }
        
        /// <summary>creates the CLS-name for a name, which was mapped from CLS to IDL</summary>
        internal static string ReverseClsToIdlNameMapping(string mappedNameInIdl) {
            string result = mappedNameInIdl;
            if (result.StartsWith("_")) { 
                // for names conflicting with an IDL keyword
                // and because of property accessor methods: .NET accessor have no _ before get/set
                result = result.Substring(1); 
            } else if (result.StartsWith("N_")) {
                result = result.Substring(1);
            }
            return result;
        }                
        
        #endregion simple unqualified name mapping
        #region method name mapping

        /// <summary>
        /// Maps the method name for the CLS method to IDL
        /// </summary>
        public static string MapClsMethodNameToIdlName(MethodInfo method, bool isOverloaded) {                       
            string methodName = MapClsNameToIdlName(method.Name);
            if (isOverloaded) {
                // do the mangling                
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length == 0) {
                    methodName += "__";
                }
                ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
                foreach (ParameterInfo parameter in parameters) {
                    string mappedTypeName = (string)mapper.MapClsType(parameter.ParameterType,
                                                                      ReflectionHelper.CollectParameterAttributes(parameter, method),
                                                                      s_genIdlNameforClsTypeNoAnonSeq);
                    mappedTypeName = mappedTypeName.Replace(" ", "_");
                    mappedTypeName = mappedTypeName.Replace("::", "__");
                    methodName = methodName + "__" + mappedTypeName;
                }                
            }
            return methodName;
        }
        
        /// <summary>gets the request method name for attribute, if possible.</summary>
        private static string GetRequestMethodNameFromAttr(MethodInfo info) {
            AttributeExtCollection methodAttributes =
                ReflectionHelper.GetCustomAttriutesForMethod(info, true);
            if (methodAttributes.IsInCollection(ReflectionHelper.FromIdlNameAttributeType)) {
                FromIdlNameAttribute idlNameAttr =
                    (FromIdlNameAttribute)methodAttributes.GetAttributeForType(ReflectionHelper.FromIdlNameAttributeType);
                return idlNameAttr.IdlName;
            } else {
                return null;
            }
        }

        /// <summary>
        /// determines the operation name to use in a corba request for a method.
        /// </summary>
        internal static string GetPropertyRequestOperationName(PropertyInfo forProperty, bool forSetter) {
            string methodName;
            if (!forSetter) {
                methodName = GetRequestMethodNameFromAttr(forProperty.GetGetMethod());
            } else {
                methodName = GetRequestMethodNameFromAttr(forProperty.GetSetMethod());
            }
            if (methodName == null) {
                string mappedPropertyName =
                    IdlNaming.MapClsNameToIdlName(forProperty.Name);
                if (!forSetter) {
                    methodName = DetermineGetterTransmissionName(mappedPropertyName);
                } else {
                    methodName = DetermineSetterTransmissionName(mappedPropertyName);
                }
            }
            return methodName;
        }

        /// <summary>
        /// determines the operation name to use in a corba request for a method.
        /// </summary>
        internal static string GetMethodRequestOperationName(MethodInfo method, bool isOverloaded) {
            string methodName = GetRequestMethodNameFromAttr(method);
            if (methodName == null) {
                // determine name for a native .NET method (not mapped from idl)
                // do a CLS to IDL mapping, because .NET server expect this for every client, also for a
                // native .NET client, which uses not CLS -> IDL -> CLS mapping
                methodName = IdlNaming.MapClsMethodNameToIdlName(method,
                                                                 isOverloaded);
                methodName = DetermineOperationTransmissionName(methodName);
            }
            return methodName;
        }
                
        /// <summary>
        /// Determine the operation name to transmit for an idl operation name
        /// </summary>        
        public static string DetermineOperationTransmissionName(string idlName) {
            return DetermineTransmissionName(idlName);
        }        

        /// <summary>
        /// Determine the attribute name to transmit for an idl attribute name
        /// </summary>
        public static string DetermineAttributeTransmissionName(string idlName) {
            return DetermineTransmissionName(idlName);
        }                
        
        /// <summary>
        /// Determine the getter name to transmit for an idl attribute name
        /// </summary>
        public static string DetermineGetterTransmissionName(string idlName) {
            string attrNameToTransmit =
                DetermineTransmissionName(idlName);
            return "_get_" + attrNameToTransmit;
        }        
        
        /// <summary>
        /// Determine the setter name to transmit for an idl attribute name
        /// </summary>
        public static string DetermineSetterTransmissionName(string idlName) {
            string attrNameToTransmit =
                DetermineTransmissionName(idlName);
            return "_set_" + attrNameToTransmit;
        }                
        
        /// <summary>
        /// Determines the idl name to transmit over the wire according to 
        /// section 3.2.3.1 Escaped Identifiers, i.e. removes leading underscore.
        /// The underscore is added, because the identifier would clash with an idl keyword;
        /// The undersore is not transmitted, therefore remove it from transmissionName.
        /// </summary>
        private static string DetermineTransmissionName(string idlName) {
            string result = idlName;
            if (idlName.StartsWith("_")) {
                result = result.Substring(1);
            }
            return result;
        }        
        
        #endregion method name mapping

        /// <summary>
        /// maps the type part of an IDL repository id to a Cls type-name
        /// </summary>
        /// <remarks>
        /// uses the inverse Cls to Idl mapping to reproduce the CLS type name if assumeMappedFromIdl is false;
        /// otherwise use MapIdlNameToCls name for mapping names. 
        /// </remarks>
        /// <param name="idlName"></param>
        /// <param name="assumeMappedFromIdl">should assume that cls type represented by idlName is mapped from IDL to CLS</param>
        /// <returns></returns>
        internal static string MapIdlRepIdTypePartToClsName(string idlName, bool assumeMappedFromIdl) {
            string[] parts = idlName.Split('/');
            StringBuilder result = new StringBuilder();
            foreach (string part in parts) {
                if (result.Length != 0) {
                    result.Append('.');
                }
                if (!assumeMappedFromIdl) {
                    // standard case: type from rep-id represents a CLS type, which was mapped from CLS to IDL.
                    result.Append(ReverseClsToIdlNameMapping(part));
                } else {
                    // a type mapped from IDL to CLS -> map type name to CLS too
                    result.Append(MapIdlNameToClsName(part));
                }
            }
            return result.ToString();
        }


        /// <summary>
        /// maps an RMI fully/partially/not qualified type name or a namespace name into a Cls name
        /// </summary>
        /// <param name="rmiName"></param>
        /// <returns>the cls name for rmi-Name</returns>
        internal static string MapRmiNameToClsName(string rmiName) {
            string[] parts = rmiName.Split('.');
            StringBuilder result = new StringBuilder();
            foreach (string part in parts) {
                if (result.Length != 0) {
                    result.Append('.');
                }
                // a type mapped from IDL to CLS -> map type name to CLS too                                
                result.Append(MapIdlNameToClsName(ApplyRmiIdlClashEscape(part)));
            }
            return result.ToString();
        }
        
        internal static string ApplyRmiIdlClashEscape(string rmiIdPart) {
            if (!NameClashesWithIdlKeyWord(rmiIdPart)) {
                return rmiIdPart;
            } else {
                return "_" + rmiIdPart; // escaping rule of rmi-id's
            }
        }

        /// <summary>
        /// creates a repository id for a CLS type
        /// </summary>
        public static string MapFullTypeNameToIdlRepId(Type forType) {
            return MapTypeNameToIdlRepId(forType.Name, forType.Namespace);
        }
        
        /// <summary>
        /// creates a repository id from a fully qualified name for a CLS type
        /// </summary>
        internal static string MapFullTypeNameToIdlRepId(string fullTypeName) {
            string namespaceName = String.Empty;
            string shortName = fullTypeName;
            int lastSeparatorIndex = fullTypeName.LastIndexOf(".");
            if (lastSeparatorIndex >= 0) {
                namespaceName = fullTypeName.Substring(0, lastSeparatorIndex);
                shortName = fullTypeName.Substring(lastSeparatorIndex + 1);
            }
            return MapTypeNameToIdlRepId(shortName, namespaceName);
        }
        
        /// <summary>
        /// creates a repository id from a simple type name and the namespace name for a CLS type
        /// </summary>
        private static string MapTypeNameToIdlRepId(string shortTypeName, string namepsaceName) {
            string nameSpaceInIdl = MapNamespaceNameToIdl(namepsaceName, "/", false);
            if (nameSpaceInIdl.Length != 0) {
                nameSpaceInIdl += "/";
            }
            return "IDL:" + nameSpaceInIdl + MapClsNameToIdlName(shortTypeName) + ":1.0";            
        }

        /// <summary>
        /// map a name for a CLS type to a scoped IDL-name, e.g. a.b.c.Test --> ::a::b::c::Test
        /// </summary>
        /// <param name="forType"></param>
        /// <returns></returns>
        // generator needs scoped form
        public static string MapFullTypeNameToIdlScoped(Type forType) {
            bool isTypeMappedFromIdl = ReflectionHelper.IIdlEntityType.IsAssignableFrom(forType);
            string result = MapNamespaceNameToIdl(forType.Namespace, "::", isTypeMappedFromIdl);
            if (result.Length > 0) { 
                result += "::"; 
            }
            result = "::" + result;
            result += ((!isTypeMappedFromIdl) ? MapShortTypeNameToIdl(forType) :
                                                ReverseIdlToClsNameMapping(forType.Name));
            return result;
        }        

        /// <summary>
        /// maps the CLS typename (without namespace name) to an IDL name
        /// </summary>
        /// <param name="forType"></param>
        /// <returns></returns>
        public static string MapShortTypeNameToIdl(Type forType) {
            string idl;
            if (s_clsMapSpecial.TryGetValue(forType, out idl)) {
                // special
                return idl;
            }
            return MapClsNameToIdlName(forType.Name);
        }
        
        /// <summary>
        /// maps a namespace name to an IDL name
        /// </summary>
        /// <param name="separator">separator between module parts, e.g. / or ::</param>        
        private static string MapNamespaceNameToIdl(string namespaceName, string separator, 
                                                    bool isMappedFromIdlToCls) {            
            if (namespaceName == null) { 
                return string.Empty;
            }

            StringBuilder result = new StringBuilder(); ;
            string[] parts = namespaceName.Split('.');
            foreach (string part in parts) {
                if (result.Length != 0) {
                    result.Append(separator);
                }
                result.Append(!isMappedFromIdlToCls ? MapClsNameToIdlName(part) :
                                                      ReverseIdlToClsNameMapping(part));
            }
            return result.ToString();
        }

        /// <summary>
        /// maps a CLS namespace to a module hirarchy
        /// </summary>
        // used for generator
        public static string[] MapNamespaceToIdlModules(Type forType) {
            return MapNamespaceNameToIdlModules(forType.Namespace);
        }
        
        /// <summary>
        /// maps a CLS namespace to a module hirarchy
        /// </summary>
        // used for generator
        public static string[] MapNamespaceNameToIdlModules(string clsNamespace) {            
            string[] modules;
            if ((clsNamespace != null) && !StringConversions.IsBlank(clsNamespace)) {
                modules = clsNamespace.Split('.');
            } else {
                modules = new string[0];
            }
            for (int i = 0; i < modules.Length; i++) {
                modules[i] = MapClsNameToIdlName(modules[i]);
            }
            return modules;
        }        
        
        /// <summary>
        /// return the fully qualified name of typedef for the idl sequence
        /// </summary>
        public static string GetFullyQualifiedIdlTypeDefAliasForSequenceType(Type seqType, int bound, AttributeExtCollection elemTypeAttributes) {
            string namespaceName;
            string elemTypeFullQualName;
            string typedefName = GetTypeDefAliasForSequenceType(seqType, bound, elemTypeAttributes, out namespaceName, out elemTypeFullQualName);
                        
            string result = MapNamespaceNameToIdl(namespaceName, "::", false);
            if (result.Length > 0) { 
                result += "::"; 
            }
            result = "::" + result;
            result += typedefName;
            return result;
        }

        /// <summary>
        /// return the fully qualified name of typedef for the idl sequence
        /// </summary>
        public static string GetFullyQualifiedIdlTypeDefAliasForArrayType(Type arrayType, int[] dimensions, AttributeExtCollection elemTypeAttributes) {
            string namespaceName;
            string elemTypeFullQualName;
            string typedefName = GetTypeDefAliasForArrayType(arrayType, dimensions, elemTypeAttributes, out namespaceName, out elemTypeFullQualName);
                        
            string result = MapNamespaceNameToIdl(namespaceName, "::", false);
            if (result.Length > 0) { 
                result += "::"; 
            }
            result = "::" + result;
            result += typedefName;
            return result;
        }
        
        /// <summary>
        /// return the short type name of typedef for the idl sequence
        /// </summary>
        public static string GetTypeDefAliasForSequenceType(Type seqType, int bound, AttributeExtCollection elemTypeAttributes, out string namespaceName, out string elemTypeFullQualName) {
            elemTypeFullQualName = (string)ClsToIdlMapper.GetSingleton().MapClsType(seqType.GetElementType(), elemTypeAttributes,
                                                                                    s_genIdlNameforClsTypeNoAnonSeq);
            
            string elemTypeNameId = elemTypeFullQualName.Replace(":", "_");
            elemTypeNameId = elemTypeNameId.Replace(" ", "_");
            string typedefName = "seqTd" + bound + "_" + elemTypeNameId;
            
            namespaceName = "org.omg.seqTypeDef";
            return typedefName;
        }

        /// <summary>
        /// return the short type name of typedef for the idl array
        /// </summary>
        public static string GetTypeDefAliasForArrayType(Type arrayType, int[] dimensions, AttributeExtCollection elemTypeAttributes, out string namespaceName, out string elemTypeFullQualName) {
            elemTypeFullQualName = (string)ClsToIdlMapper.GetSingleton().MapClsType(arrayType.GetElementType(), elemTypeAttributes,
                                                                                    s_genIdlNameforClsTypeNoAnonSeq);
            
            string elemTypeNameId = elemTypeFullQualName.Replace(":", "_");
            elemTypeNameId = elemTypeNameId.Replace(" ", "_");
            string dimensionRep = "";
            for (int i = 0; i < dimensions.Length; i++) {
                dimensionRep = dimensionRep + "_" + dimensions[i];
            }
            string typedefName = "arrayTd" +  dimensionRep + "_" + elemTypeNameId;
            
            namespaceName = "org.omg.arrayTypeDef";
            return typedefName;
        }
        
        private static void InitMapClsToIdlSpecial() {
            s_clsMapSpecial.Add(ReflectionHelper.Int16Type, "short");
            s_clsMapSpecial.Add(ReflectionHelper.Int32Type, "long");
            s_clsMapSpecial.Add(ReflectionHelper.Int64Type, "long long");
            s_clsMapSpecial.Add(ReflectionHelper.UInt16Type, "unsigned short");
            s_clsMapSpecial.Add(ReflectionHelper.UInt32Type, "unsigned long");
            s_clsMapSpecial.Add(ReflectionHelper.UInt64Type, "unsigned long long");            
            s_clsMapSpecial.Add(ReflectionHelper.ByteType, "octet");
            s_clsMapSpecial.Add(ReflectionHelper.SByteType, "octet");
            s_clsMapSpecial.Add(ReflectionHelper.BooleanType, "boolean");
            s_clsMapSpecial.Add(ReflectionHelper.VoidType, "void");

            s_clsMapSpecial.Add(ReflectionHelper.SingleType, "float");
            s_clsMapSpecial.Add(ReflectionHelper.DoubleType, "double");
            
            s_clsMapSpecial.Add(ReflectionHelper.CharType, "wchar");
            s_clsMapSpecial.Add(ReflectionHelper.StringType, "wstring");
        }

        
        #region Keyword handling
        
        /// <summary>checks, if the given name clashes with a cls keyword</summary>
        internal static bool NameClashesWithClsKeyWord(string name) {
            if (s_clsKeywordList.BinarySearch(name, s_keyWordComparer) >= 0) {
                return true;
            } else {
                return false;
            }
        }
        
        /// <summary>checks, if the given name clashes with an idl keyword</summary>
        internal static bool NameClashesWithIdlKeyWord(string name) {
            if (s_idlKeywordList.BinarySearch(name, s_keyWordComparer) >= 0) {
                return true;
            } else {
                return false;
            }
        }
        
        /// <summary>initalize the CLS keyword-list</summary>
        private static void InitClsKeyWordList() {
            s_clsKeywordList.Add("AddHandler");
            s_clsKeywordList.Add("AddressOf");
            s_clsKeywordList.Add("Alias");
            s_clsKeywordList.Add("And");
            s_clsKeywordList.Add("Ansi");
            s_clsKeywordList.Add("As");
            s_clsKeywordList.Add("Assembly");
            s_clsKeywordList.Add("Auto");
            s_clsKeywordList.Add("Base");
            s_clsKeywordList.Add("Boolean");
            s_clsKeywordList.Add("bool");
            s_clsKeywordList.Add("ByRef");
            s_clsKeywordList.Add("Byte");
            s_clsKeywordList.Add("ByVal");
            s_clsKeywordList.Add("Call");
            s_clsKeywordList.Add("Case");
            s_clsKeywordList.Add("Catch");
            s_clsKeywordList.Add("CBool");
            s_clsKeywordList.Add("CByte");
            s_clsKeywordList.Add("CChar");
            s_clsKeywordList.Add("CDate");
            s_clsKeywordList.Add("CDec");
            s_clsKeywordList.Add("CDbl");
            s_clsKeywordList.Add("Char");
            s_clsKeywordList.Add("CInt");
            s_clsKeywordList.Add("Class");
            s_clsKeywordList.Add("CLng");
            s_clsKeywordList.Add("CObj");
            s_clsKeywordList.Add("Const");
            s_clsKeywordList.Add("CShort");
            s_clsKeywordList.Add("CSng");
            s_clsKeywordList.Add("CStr");
            s_clsKeywordList.Add("CType");
            s_clsKeywordList.Add("Date");
            s_clsKeywordList.Add("Decimal");
            s_clsKeywordList.Add("Declare");
            s_clsKeywordList.Add("Default");
            s_clsKeywordList.Add("Delegate");
            s_clsKeywordList.Add("Dim");
            s_clsKeywordList.Add("Do");
            s_clsKeywordList.Add("Double");
            s_clsKeywordList.Add("Each");
            s_clsKeywordList.Add("Else");
            s_clsKeywordList.Add("ElseIf");
            s_clsKeywordList.Add("End");
            s_clsKeywordList.Add("Enum");
            s_clsKeywordList.Add("Erase");
            s_clsKeywordList.Add("Error");
            s_clsKeywordList.Add("Event");
            s_clsKeywordList.Add("Exit");
            s_clsKeywordList.Add("ExternalSource");
            s_clsKeywordList.Add("False");
            s_clsKeywordList.Add("Finalize");
            s_clsKeywordList.Add("Finally");
            s_clsKeywordList.Add("Float");
            s_clsKeywordList.Add("For");
            s_clsKeywordList.Add("foreach");
            s_clsKeywordList.Add("Friend");
            s_clsKeywordList.Add("Function");
            s_clsKeywordList.Add("Get");
            s_clsKeywordList.Add("GetType");
            s_clsKeywordList.Add("Goto");
            s_clsKeywordList.Add("Handles");
            s_clsKeywordList.Add("If");
            s_clsKeywordList.Add("Implements");
            s_clsKeywordList.Add("Imports");
            s_clsKeywordList.Add("In");
            s_clsKeywordList.Add("Inherits");
            s_clsKeywordList.Add("Integer");
            s_clsKeywordList.Add("int");
            s_clsKeywordList.Add("Interface");
            s_clsKeywordList.Add("Is");
            s_clsKeywordList.Add("Let");
            s_clsKeywordList.Add("Lib");
            s_clsKeywordList.Add("Like");
            s_clsKeywordList.Add("lock");
            s_clsKeywordList.Add("Long");
            s_clsKeywordList.Add("Loop");
            s_clsKeywordList.Add("Me");
            s_clsKeywordList.Add("Mod");
            s_clsKeywordList.Add("Module");
            s_clsKeywordList.Add("MustInherit");
            s_clsKeywordList.Add("MustOverride");
            s_clsKeywordList.Add("MyBase"); 
            s_clsKeywordList.Add("MyClass");
            s_clsKeywordList.Add("Namespace");
            s_clsKeywordList.Add("New");
            s_clsKeywordList.Add("Next");
            s_clsKeywordList.Add("Not");
            s_clsKeywordList.Add("Nothing");
            s_clsKeywordList.Add("NotInheritable");
            s_clsKeywordList.Add("NotOverridable");
            s_clsKeywordList.Add("Object");
            s_clsKeywordList.Add("On");
            s_clsKeywordList.Add("Option");
            s_clsKeywordList.Add("Optional");
            s_clsKeywordList.Add("Or");
            s_clsKeywordList.Add("Overloads");
            s_clsKeywordList.Add("Overridable"); 
            s_clsKeywordList.Add("override");
            s_clsKeywordList.Add("Overrides");
            s_clsKeywordList.Add("ParamArray");
            s_clsKeywordList.Add("Preserve");
            s_clsKeywordList.Add("Private");
            s_clsKeywordList.Add("Property"); 
            s_clsKeywordList.Add("Protected");
            s_clsKeywordList.Add("Public");
            s_clsKeywordList.Add("RaiseEvent");
            s_clsKeywordList.Add("ReadOnly");
            s_clsKeywordList.Add("ReDim");
            s_clsKeywordList.Add("Region");
            s_clsKeywordList.Add("REM"); 
            s_clsKeywordList.Add("RemoveHandler");
            s_clsKeywordList.Add("Resume");
            s_clsKeywordList.Add("Return");
            s_clsKeywordList.Add("Select");
            s_clsKeywordList.Add("Set");
            s_clsKeywordList.Add("Shadows");
            s_clsKeywordList.Add("Shared");
            s_clsKeywordList.Add("Short"); 
            s_clsKeywordList.Add("Single");
            s_clsKeywordList.Add("Static");
            s_clsKeywordList.Add("Step");
            s_clsKeywordList.Add("Stop");
            s_clsKeywordList.Add("String");
            s_clsKeywordList.Add("Structure");
            s_clsKeywordList.Add("struct");
            s_clsKeywordList.Add("Sub");
            s_clsKeywordList.Add("SyncLock");
            s_clsKeywordList.Add("Then");
            s_clsKeywordList.Add("Throw");
            s_clsKeywordList.Add("To");
            s_clsKeywordList.Add("True");
            s_clsKeywordList.Add("Try");
            s_clsKeywordList.Add("TypeOf");
            s_clsKeywordList.Add("Unicode");
            s_clsKeywordList.Add("Until");
            s_clsKeywordList.Add("volatile");
            s_clsKeywordList.Add("When");
            s_clsKeywordList.Add("While");
            s_clsKeywordList.Add("With");
            s_clsKeywordList.Add("WithEvents");
            s_clsKeywordList.Add("WriteOnly");
            s_clsKeywordList.Add("Xor");
            s_clsKeywordList.Add("eval");
            s_clsKeywordList.Add("extends");
            s_clsKeywordList.Add("instanceof");
            s_clsKeywordList.Add("package");
            s_clsKeywordList.Add("var");            
            // sort the list for binary search
            s_clsKeywordList.Sort(s_keyWordComparer);
        }
        /// <summary>initalize the IDL keyword-list</summary>        
        /// <remarks>see section 3.2.4, Corba 2.3.1</remarks>
        private static void InitIdlKeyWordList() {
            s_idlKeywordList.Add("abstract");
            s_idlKeywordList.Add("any");
            s_idlKeywordList.Add("attribute");
            s_idlKeywordList.Add("boolean");
            s_idlKeywordList.Add("case");
            s_idlKeywordList.Add("char");
            s_idlKeywordList.Add("const");
            s_idlKeywordList.Add("context");
            s_idlKeywordList.Add("custom");
            s_idlKeywordList.Add("default");
            s_idlKeywordList.Add("double");
            s_idlKeywordList.Add("enum");
            s_idlKeywordList.Add("exception");
            s_idlKeywordList.Add("factory");
            s_idlKeywordList.Add("FALSE");
            s_idlKeywordList.Add("fixed");
            s_idlKeywordList.Add("float");
            s_idlKeywordList.Add("in");
            s_idlKeywordList.Add("inout");
            s_idlKeywordList.Add("interface");
            s_idlKeywordList.Add("long");
            s_idlKeywordList.Add("module");
            s_idlKeywordList.Add("native");
            s_idlKeywordList.Add("Object");
            s_idlKeywordList.Add("octet");
            s_idlKeywordList.Add("oneway");
            s_idlKeywordList.Add("out");
            s_idlKeywordList.Add("private");
            s_idlKeywordList.Add("public");
            s_idlKeywordList.Add("raises");
            s_idlKeywordList.Add("readonly");
            s_idlKeywordList.Add("sequence");
            s_idlKeywordList.Add("short");
            s_idlKeywordList.Add("string");
            s_idlKeywordList.Add("struct");
            s_idlKeywordList.Add("supports");
            s_idlKeywordList.Add("switch");
            s_idlKeywordList.Add("TRUE");
            s_idlKeywordList.Add("truncatable");
            s_idlKeywordList.Add("typedef");
            s_idlKeywordList.Add("unsigned");
            s_idlKeywordList.Add("union");
            s_idlKeywordList.Add("ValueBase");
            s_idlKeywordList.Add("valuetype");
            s_idlKeywordList.Add("void");
            s_idlKeywordList.Add("wchar");
            s_idlKeywordList.Add("wstring");

            // to fix a problem with the idlj, when using a class/interface in System namespace:
            s_idlKeywordList.Add("System");

            // sort the list for binary search
            s_idlKeywordList.Sort(s_keyWordComparer);
        }

        #endregion Keyword handling

        #endregion SMethods

    }
    
    
    /// <summary>returns the IDL for a type referenced from another type. This class doesn't write type declarations/definition, this is left to GenerationActionDefineTypes
    /// All methods returns the IDL-string which shoulb be inserted, if the specified type is referenced</summary>
    public class GenerationActionReference : MappingAction {
    
        #region IFields

        private ClsToIdlMapper m_mapper = ClsToIdlMapper.GetSingleton();
        private bool m_useAnonymousSequences;

        #endregion IFields
        #region IConstructors
        
        public GenerationActionReference(bool useAnonymousSequences) {
            m_useAnonymousSequences = useAnonymousSequences;    
        }
        
        #endregion IConstructors
        #region IMethods

        #region Implementation of MappingAction
        
        public object MapToIdlLong(System.Type dotNetType) {
            return "long";
        }
        public object MapToIdlULong(System.Type dotNetType) {
            return "unsigned long";
        }
        public object MapToIdlLongLong(System.Type dotNetType) {
            return "long long";
        }
        public object MapToIdlULongLong(System.Type dotNetType) {
            return "unsigned long long";
        }
        public object MapToIdlUShort(System.Type dotNetType) {
            return "unsigned short";
        }
        public object MapToIdlShort(System.Type dotNetType) {
            return "short";
        }
        public object MapToIdlOctet(System.Type dotNetType) {
            return "octet";
        }
        public object MapToIdlSByteEquivalent(Type clsType) {
            return MapToIdlOctet(clsType);
        }
        public object MapToIdlVoid(System.Type dotNetType) {
            return "void";
        }        
        public object MapToIdlFloat(System.Type dotNetType) {
            return "float";
        }
        public object MapToIdlDouble(System.Type dotNetType) {
            return "double";
        }
        public object MapToIdlChar(System.Type dotNetType) {
            return "char";
        }
        public object MapToIdlWChar(System.Type dotNetType) {
            return "wchar";
        }
        public object MapToIdlBoolean(System.Type dotNetType) {
            return "boolean";
        }
        public object MapToIdlString(System.Type dotNetType) {
            return "string";
        }
        public object MapToIdlWString(System.Type dotNetType) {
            return "wstring";
        }
        public object MapToIdlAny(System.Type dotNetType) {
            return "any";
        }
        public object MapToStringValue(System.Type dotNetType) {
            return "::CORBA::StringValue";
        }
        public object MapToWStringValue(System.Type dotNetType) {
            return "::CORBA::WStringValue";
        }
        public object MapToIdlEnum(System.Type dotNetType) {
            return IdlNaming.MapFullTypeNameToIdlScoped(dotNetType);
        }
        public object MapToIdlFlagsEquivalent(Type clsType) {            
            Type underlyingType = Enum.GetUnderlyingType(clsType);
            // map to the base type of the flags (i.e. underlyingType).
            string refUnderlyingType = (string)m_mapper.MapClsType(underlyingType, AttributeExtCollection.EmptyCollection,
                                                                   this);            
            return refUnderlyingType;
        }
        public object MapToIdlConcreteInterface(System.Type dotNetType) {
            if (!dotNetType.Equals(ReflectionHelper.MarshalByRefObjectType)) {
                return IdlNaming.MapFullTypeNameToIdlScoped(dotNetType);
            } else {
                return "Object";
            }
        }
        public object MapToIdlLocalInterface(System.Type dotNetType) {
            return IdlNaming.MapFullTypeNameToIdlScoped(dotNetType);
        }
        public object MapToIdlConcreateValueType(System.Type dotNetType) {
            return IdlNaming.MapFullTypeNameToIdlScoped(dotNetType);
        }
        public object MapToIdlAbstractInterface(System.Type dotNetType) {
            return IdlNaming.MapFullTypeNameToIdlScoped(dotNetType);
        }
        public object MapToIdlStruct(System.Type dotNetType) {
            return IdlNaming.MapFullTypeNameToIdlScoped(dotNetType);
        }
        public object MapToIdlUnion(System.Type dotNetType) {
            return IdlNaming.MapFullTypeNameToIdlScoped(dotNetType);
        }
        public object MapToIdlAbstractValueType(System.Type dotNetType) {
            return IdlNaming.MapFullTypeNameToIdlScoped(dotNetType);
        }
        public object MapToIdlSequence(System.Type dotNetType, int bound, AttributeExtCollection allAttributes, AttributeExtCollection elemTypeAttributes) {
            if (m_useAnonymousSequences) {
                string refToElemType = (string)m_mapper.MapClsType(dotNetType.GetElementType(), elemTypeAttributes, this);
                return "sequence<" + refToElemType + ">";
            } else {
                // use a typedef for non-anonymous sequence
                return IdlNaming.GetFullyQualifiedIdlTypeDefAliasForSequenceType(dotNetType, bound, elemTypeAttributes);
            }
        }
        public object MapToIdlArray(System.Type dotNetType, int[] dimensions, AttributeExtCollection allAttributes, AttributeExtCollection elemTypeAttributes) {
            // use a typedef for arrays, because dimension is part of the name of the thing, the type is an array
            return IdlNaming.GetFullyQualifiedIdlTypeDefAliasForArrayType(dotNetType, dimensions, elemTypeAttributes);
        }
        public object MapToIdlBoxedValueType(System.Type dotNetType, System.Type needsBoxingFrom) {
            // the dotNetType is a subclass of BoxedValueBase representing the boxed value type
            return IdlNaming.MapFullTypeNameToIdlScoped(dotNetType);
        }
        public object MapToValueBase(System.Type dotNetType) {
            return "::CORBA::ValueBase";
        }
        public object MapToAbstractBase(System.Type dotNetType) {
            return "::CORBA::AbstractBase";
        }
        
        public object MapToTypeDesc(System.Type dotNetType) {
            return "::CORBA::TypeCode";
        }        
        public object MapToTypeCode(System.Type dotNetType) {
            return "::CORBA::TypeCode";
        }
        public object MapException(System.Type dotNetType) {
            return IdlNaming.MapFullTypeNameToIdlScoped(dotNetType);
        }
        #endregion

        #endregion IMethods

    }
    
}
