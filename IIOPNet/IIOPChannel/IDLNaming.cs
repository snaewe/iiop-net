/* IDLNaming.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 14.01.03  Dominic Ullmann (DUL), dul@elca.ch
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

namespace Ch.Elca.Iiop.Idl {

    /// <summary>
    /// This class is responsible for realising the identifier (name) mapping
    ///  in the IDL to .NET and .NET to IDL mapping.
    /// </summary>
    public class IdlNaming {

        #region SFields
        
        private static ArrayList s_keywordList = new ArrayList();
        private static Hashtable s_mapSpecial = new Hashtable();

        private static IComparer s_keyWordComparer = new CaseInsensitiveComparer();

        #endregion SFields
        #region SConstructor

        static IdlNaming() {
            // creates the list of reserved CLS words
            InitClsKeyWordList();
            // create mapping table for primitive types and special types
            InitMapSpecial();
        }

        #endregion SConstructor
        #region IConstructors

        private IdlNaming() {
        }

        #endregion IConstructors
        #region SMethods

        /// <summary>
        /// converts an IDL name to a .NET name
        /// </summary>
        /// <param name="idlName"></param>
        /// <param name="serverType">needed to handle overloaded method-names correctly</param>
        /// <returns></returns>
        public static string MapIdlMethodNameToClsName(string idlName, Type serverType) {
            // TODO: exception in 1 to 1 mapping rule, e.g. handling of overloaded method names
            if (idlName.StartsWith("_")) { 
                // remove underscore: see CORBA2.3, 3.2.3.1
                idlName = idlName.Substring(1);
            }
            string result = idlName;
            if (s_keywordList.BinarySearch(idlName, s_keyWordComparer) >= 0) {
                // check for key-words, .NET keyworks are escaped
                result = "_" + result;
            }
            return result;
        }

        /// <summary>
        /// returns the IDLName for a CLS name
        /// </summary>
        /// <param name="dotNetName"></param>
        /// <param name="serverType">needed to handle overloaded method-names correctly</param>
        /// <returns></returns>
        public static string MapClsMethodNameToIdlName(string dotNetName, Type serverType) {
            // TODO: handle exceptions
            if (typeof(IIdlEntity).IsAssignableFrom(serverType)) {
                // method name collided with .NET identifier must be mapped to the original identifier
                if (dotNetName.StartsWith("_")) { 
                    dotNetName = dotNetName.Substring(1); 
                }
            } else {
            
            }
            
            return dotNetName;
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
            return MapNamespaceToIdl(forType) + "/" + MapShortTypeNameToIdl(forType);
        }

        /// <summary>
        /// map a name for a CLS type to a scoped IDL-name, e.g. a.b.c.Test --> ::a::b::c::Test
        /// </summary>
        /// <param name="forType"></param>
        /// <returns></returns>
        // generator needs scoped form
        public static string MapFullTypeNameToIdlScoped(Type forType) {
            string result = forType.Namespace;
            result = result.Replace(".", "::");
            if (result.Length > 0) { result += "::"; }
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
            if (s_mapSpecial.ContainsKey(forType)) {
                // special
                return (string)s_mapSpecial[forType];
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
            string[] modules = clsNamespace.Split(new char[] { Char.Parse(".") } );

            return modules;
        }

        private static void InitMapSpecial() {
            s_mapSpecial.Add(typeof(System.Int16), "short");
            s_mapSpecial.Add(typeof(System.Int32), "long");
            s_mapSpecial.Add(typeof(System.Int64), "long long");
            s_mapSpecial.Add(typeof(System.Byte), "octet");
            s_mapSpecial.Add(typeof(System.Boolean), "boolean");
            s_mapSpecial.Add(typeof(void), "void");

            s_mapSpecial.Add(typeof(System.Single), "float");
            s_mapSpecial.Add(typeof(System.Double), "double");
            
            s_mapSpecial.Add(typeof(System.Char), "wchar");
            s_mapSpecial.Add(typeof(System.String), "wstring");
        }
        
        /// <summary>initalize the CLS keyword-list</summary>
        private static void InitClsKeyWordList() {
            s_keywordList.Add("AddHandler");
            s_keywordList.Add("AddressOf");
            s_keywordList.Add("Alias");
            s_keywordList.Add("And");
            s_keywordList.Add("Ansi");
            s_keywordList.Add("As");
            s_keywordList.Add("Assembly");
            s_keywordList.Add("Auto");
            s_keywordList.Add("Base");
            s_keywordList.Add("Boolean");
            s_keywordList.Add("bool");
            s_keywordList.Add("ByRef");
            s_keywordList.Add("Byte");
            s_keywordList.Add("ByVal");
            s_keywordList.Add("Call");
            s_keywordList.Add("Case");
            s_keywordList.Add("Catch");
            s_keywordList.Add("CBool");
            s_keywordList.Add("CByte");
            s_keywordList.Add("CChar");
            s_keywordList.Add("CDate");
            s_keywordList.Add("CDec");
            s_keywordList.Add("CDbl");
            s_keywordList.Add("Char");
            s_keywordList.Add("CInt");
            s_keywordList.Add("Class");
            s_keywordList.Add("CLng");
            s_keywordList.Add("CObj");
            s_keywordList.Add("Const");
            s_keywordList.Add("CShort");
            s_keywordList.Add("CSng");
            s_keywordList.Add("CStr");
            s_keywordList.Add("CType");
            s_keywordList.Add("Date");
            s_keywordList.Add("Decimal");
            s_keywordList.Add("Declare");
            s_keywordList.Add("Default");
            s_keywordList.Add("Delegate");
            s_keywordList.Add("Dim");
            s_keywordList.Add("Do");
            s_keywordList.Add("Double");
            s_keywordList.Add("Each");
            s_keywordList.Add("Else");
            s_keywordList.Add("ElseIf");
            s_keywordList.Add("End");
            s_keywordList.Add("Enum");
            s_keywordList.Add("Erase");
            s_keywordList.Add("Error");
            s_keywordList.Add("Event");
            s_keywordList.Add("Exit");
            s_keywordList.Add("ExternalSource");
            s_keywordList.Add("False");
            s_keywordList.Add("Finalize");
            s_keywordList.Add("Finally");
            s_keywordList.Add("Float");
            s_keywordList.Add("For");
            s_keywordList.Add("foreach");
            s_keywordList.Add("Friend");
            s_keywordList.Add("Function");
            s_keywordList.Add("Get");
            s_keywordList.Add("GetType");
            s_keywordList.Add("Goto");
            s_keywordList.Add("Handles");
            s_keywordList.Add("If");
            s_keywordList.Add("Implements");
            s_keywordList.Add("Imports");
            s_keywordList.Add("In");
            s_keywordList.Add("Inherits");
            s_keywordList.Add("Integer");
            s_keywordList.Add("int");
            s_keywordList.Add("Interface");
            s_keywordList.Add("Is");
            s_keywordList.Add("Let");
            s_keywordList.Add("Lib");
            s_keywordList.Add("Like");
            s_keywordList.Add("lock");
            s_keywordList.Add("Long");
            s_keywordList.Add("Loop");
            s_keywordList.Add("Me");
            s_keywordList.Add("Mod");
            s_keywordList.Add("Module");
            s_keywordList.Add("MustInherit");
            s_keywordList.Add("MustOverride");
            s_keywordList.Add("MyBase"); 
            s_keywordList.Add("MyClass");
            s_keywordList.Add("Namespace");
            s_keywordList.Add("New");
            s_keywordList.Add("Next");
            s_keywordList.Add("Not");
            s_keywordList.Add("Nothing");
            s_keywordList.Add("NotInheritable");
            s_keywordList.Add("NotOverridable");
            s_keywordList.Add("Object");
            s_keywordList.Add("On");
            s_keywordList.Add("Option");
            s_keywordList.Add("Optional");
            s_keywordList.Add("Or");
            s_keywordList.Add("Overloads");
            s_keywordList.Add("Overridable"); 
            s_keywordList.Add("override");
            s_keywordList.Add("Overrides");
            s_keywordList.Add("ParamArray");
            s_keywordList.Add("Preserve");
            s_keywordList.Add("Private");
            s_keywordList.Add("Property"); 
            s_keywordList.Add("Protected");
            s_keywordList.Add("Public");
            s_keywordList.Add("RaiseEvent");
            s_keywordList.Add("ReadOnly");
            s_keywordList.Add("ReDim");
            s_keywordList.Add("Region");
            s_keywordList.Add("REM"); 
            s_keywordList.Add("RemoveHandler");
            s_keywordList.Add("Resume");
            s_keywordList.Add("Return");
            s_keywordList.Add("Select");
            s_keywordList.Add("Set");
            s_keywordList.Add("Shadows");
            s_keywordList.Add("Shared");
            s_keywordList.Add("Short"); 
            s_keywordList.Add("Single");
            s_keywordList.Add("Static");
            s_keywordList.Add("Step");
            s_keywordList.Add("Stop");
            s_keywordList.Add("String");
            s_keywordList.Add("Structure");
            s_keywordList.Add("struct");
            s_keywordList.Add("Sub");
            s_keywordList.Add("SyncLock");
            s_keywordList.Add("Then");
            s_keywordList.Add("Throw");
            s_keywordList.Add("To");
            s_keywordList.Add("True");
            s_keywordList.Add("Try");
            s_keywordList.Add("TypeOf");
            s_keywordList.Add("Unicode");
            s_keywordList.Add("Until");
            s_keywordList.Add("volatile");
            s_keywordList.Add("When");
            s_keywordList.Add("While");
            s_keywordList.Add("With");
            s_keywordList.Add("WithEvents");
            s_keywordList.Add("WriteOnly");
            s_keywordList.Add("Xor");
            s_keywordList.Add("eval");
            s_keywordList.Add("extends");
            s_keywordList.Add("instanceof");
            s_keywordList.Add("package");
            s_keywordList.Add("var");            
        }

        #endregion SMethods

    }
}
