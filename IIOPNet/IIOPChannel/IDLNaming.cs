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
using System.Reflection;
using System.Collections;
using Ch.Elca.Iiop.Util;

namespace Ch.Elca.Iiop.Idl {

    /// <summary>
    /// This class is responsible for realising the identifier (name) mapping
    ///  in the IDL to .NET and .NET to IDL mapping.
    /// </summary>
    public class IdlNaming {

        #region SFields
        
        private static ArrayList s_clsKeywordList = new ArrayList();
        private static Hashtable s_clsMapSpecial = new Hashtable();
        
        private static ArrayList s_idlKeywordList = new ArrayList();

        private static IComparer s_keyWordComparer = new CaseInsensitiveComparer();
    	
    	/// <summary>used as helper to map type names to idl for Cls types</summary>
    	private static GenerationActionReference s_genIdlNameforClsType = new GenerationActionReference();

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
        #region IConstructors

        private IdlNaming() {
        }

        #endregion IConstructors
        #region SMethods
        
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
                result = result.Substring(1); // because of property accessor methods: .NET accessor have no _ before get/set
            } else if (result.StartsWith("N_")) {
                result = result.Substring(1);
            }
            return result;
        }                
        
        /// <summary>
        /// Maps the method name for the CLS method to IDL
        /// </summary>
        public static string MapClsMethodNameToIdlName(MethodInfo method, bool isOverloaded) {            
            // TODO: handle exceptions in method name mapping -> use standard name mapping
            string methodName = method.Name;
            if (isOverloaded) {
                // do the mangling                
                ParameterInfo[] parameters = method.GetParameters();
            	if (parameters.Length == 0) {
            	    methodName += "__";
            	}
                ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
                foreach (ParameterInfo parameter in parameters) {
                    string mappedTypeName = (string)mapper.MapClsType(parameter.ParameterType,
                                                                      AttributeExtCollection.ConvertToAttributeCollection(parameter.GetCustomAttributes(true)),
                                                                      s_genIdlNameforClsType);
                	mappedTypeName.Replace(" ", "_");
                	mappedTypeName.Replace("::", "__");
                	methodName = methodName + "__" + mappedTypeName;
                }                
            }
            return methodName;
        }
        
        /// <summary>
        /// find the CLS method for the idl name of an overloaded CLS method, defined in type serverType
        /// </summary>
        internal static MethodInfo FindClsMethodForOverloadedMethodIdlName(string idlName, Type serverType) {
            // TODO: implement this
            throw new omg.org.CORBA.BAD_OPERATION(247, omg.org.CORBA.CompletionStatus.Completed_No);
        }
        
        /// <summary>
        /// maps a type name in IDL to a Cls name
        /// </summary>
        /// <param name="idlName"></param>
        /// <returns></returns>
        public static string MapIdltoClsName(string idlName) {
            // replace / with .
            string result = idlName.Replace("/", ".");
            // TODO: exceptions
            return result;
        }

        /// <summary>
        /// map a fully qualified name for a CLS type to an IDL-name
        /// </summary>
        /// <param name="forType"></param>
        /// <returns></returns>
        public static string MapFullTypeNameToIdl(Type forType) {
            // TODO: exceptions in naming
            string nameSpaceInIdl = MapNamespaceToIdl(forType);
            if (!nameSpaceInIdl.Equals("")) {
                nameSpaceInIdl += "/";
            }
            return nameSpaceInIdl + MapShortTypeNameToIdl(forType);
        }

        /// <summary>
        /// map a name for a CLS type to a scoped IDL-name, e.g. a.b.c.Test --> ::a::b::c::Test
        /// </summary>
        /// <param name="forType"></param>
        /// <returns></returns>
        // generator needs scoped form
        public static string MapFullTypeNameToIdlScoped(Type forType) {
            string result = forType.Namespace;
            if (result == null) { 
                result = ""; 
            }
            result = result.Replace(".", "::");
            if (result.Length > 0) { 
                result += "::"; 
            }
            result = "::" + result;
            result += MapShortTypeNameToIdl(forType);
            return result;
        }

        /// <summary>
        /// map an IDL scoped name to a corresponding CLS name
        /// </summary>
        /// <param name="IDLScoped"></param>
        /// <returns></returns>
        /// <remarks>not used</remarks>
        public static string MapIdlScopedToFullTypeName(string IdlScoped) {
            string result = IdlScoped;    
            result = result.Replace("::", ".");
            if (result.StartsWith(".")) { result = result.Substring(1); }
            // TODO: handle exception in direct mapping rule ...
            return result;
        }

        /// <summary>
        /// maps the CLS typename (without namespace name) to an IDL name
        /// </summary>
        /// <param name="forType"></param>
        /// <returns></returns>
        public static string MapShortTypeNameToIdl(Type forType) {
            if (s_clsMapSpecial.ContainsKey(forType)) {
                // special
                return (string)s_clsMapSpecial[forType];
            } else {
                return forType.Name;
            }
            // TODO: exceptions

        }

        /// <summary>
        /// maps a namespace name to an IDL name
        /// </summary>
        private static string MapNamespaceToIdl(Type forType) {
            string result = forType.Namespace;
            if (result == null) { 
                result = "";
            }
            result = result.Replace(".", "/");
            return result;
        }

        /// <summary>
        /// maps a CLS namespace to a module hirarchy
        /// </summary>
        // used for generator
        public static string[] MapNamespaceToIdlModules(Type forType) {
            // TODO: exceptions
            string clsNamespace = forType.Namespace;
            string[] modules;
            if ((clsNamespace != null) && (!clsNamespace.Trim().Equals(""))) {
                modules = clsNamespace.Split(new char[] { Char.Parse(".") } );
            } else {
                modules = new string[0];
            }
            return modules;
        }
        
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

        private static void InitMapClsToIdlSpecial() {
            s_clsMapSpecial.Add(typeof(System.Int16), "short");
            s_clsMapSpecial.Add(typeof(System.Int32), "long");
            s_clsMapSpecial.Add(typeof(System.Int64), "long long");
            s_clsMapSpecial.Add(typeof(System.Byte), "octet");
            s_clsMapSpecial.Add(typeof(System.Boolean), "boolean");
            s_clsMapSpecial.Add(typeof(void), "void");

            s_clsMapSpecial.Add(typeof(System.Single), "float");
            s_clsMapSpecial.Add(typeof(System.Double), "double");
            
            s_clsMapSpecial.Add(typeof(System.Char), "wchar");
            s_clsMapSpecial.Add(typeof(System.String), "wstring");
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
        }

        #endregion SMethods

    }
    
    
    /// <summary>returns the IDL for a type referenced from another type. This class doesn't write type declarations/definition, this is left to GenerationActionDefineTypes
    /// All methods returns the IDL-string which shoulb be inserted, if the specified type is referenced</summary>
    public class GenerationActionReference : MappingAction {
    
        #region IFields

        private ClsToIdlMapper m_mapper = ClsToIdlMapper.GetSingleton();

        #endregion IFields
        #region IMethods

        #region Implementation of MappingAction
        
        public object MapToIdlLong(System.Type dotNetType) {
            return "long";
        }
        public object MapToIdlULong(System.Type dotNetType) {
            return "ulong";
        }
        public object MapToIdlLongLong(System.Type dotNetType) {
            return "long long";
        }
        public object MapToIdlULongLong(System.Type dotNetType) {
            return "ulong ulong";
        }
        public object MapToIdlUShort(System.Type dotNetType) {
            return "ushort";
        }
        public object MapToIdlShort(System.Type dotNetType) {
            return "short";
        }
        public object MapToIdlOctet(System.Type dotNetType) {
            return "octet";
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
        public object MapToIdlConcreteInterface(System.Type dotNetType) {
            if (!dotNetType.Equals(typeof(MarshalByRefObject))) {
                return IdlNaming.MapFullTypeNameToIdlScoped(dotNetType);
            } else {
                return "Object";
            }
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
        public object MapToIdlAbstractValueType(System.Type dotNetType) {
            return IdlNaming.MapFullTypeNameToIdlScoped(dotNetType);
        }
        public object MapToIdlSequence(System.Type dotNetType) {
            string refToElemType = (string)m_mapper.MapClsType(dotNetType.GetElementType(), new Util.AttributeExtCollection(), this);
            return "sequence<" + refToElemType + ">";
        }
        public object MapToIdlBoxedValueType(System.Type dotNetType, AttributeExtCollection attributes, bool isAlreadyBoxed) {
            // the dotNetType is a subclass of BoxedValueBase representing the boxed value type
            return IdlNaming.MapFullTypeNameToIdlScoped(dotNetType);
        }
        public object MapToValueBase(System.Type dotNetType) {
            return "::CORBA::ValueBase";
        }
        public object MapToAbstractBase(System.Type dotNetType) {
            return "::CORBA:AbstractBase";
        }
        
        public object MapToTypeDesc(System.Type dotNetType) {
            return null;
        }        
        public object MapToTypeCode(System.Type dotNetType) {
            return "::CORBA::TypeCode";
        }
        public object MapException(System.Type dotNetType) {
            return IdlNaming.MapShortTypeNameToIdl(dotNetType);
        }
        #endregion

        #endregion IMethods

    }
    
}
