/* TypeManager.cs
 * 
 * Project: IIOP.NET
 * IDLToCLSCompiler
 * 
 * WHEN      RESPONSIBLE
 * 19.02.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Reflection.Emit;
using System.Collections;
using System.Diagnostics;
using symboltable;
using Ch.Elca.Iiop.Idl;
using Ch.Elca.Iiop.Util;
using omg.org.CORBA;


namespace Ch.Elca.Iiop.IdlCompiler.Action {

    /// <summary>
    /// manages fully created and only partly created types
    /// </summary>
    public class TypeManager {
        
        #region Types
        
        private class TypeSymbolPair {
            
            public TypeSymbolPair(Symbol symbolPart, TypeContainer typePart) {
                SymbolPart = symbolPart;
                TypePart = typePart;
            }
            
            public Symbol SymbolPart;
            public TypeContainer TypePart;
        }
        
        #endregion Types
        #region IFields

        private Hashtable m_typesInCreation = new Hashtable();
        private Hashtable m_typeTable = new Hashtable();
        private Hashtable m_typedefTable = new Hashtable();
        /// <summary>
        /// for structs, union: only recursion allowed is through an idl sequence -> handle special
        /// </summary>
        private TypeSymbolPair m_sequenceRecursionAllowedType = null;        
        private ModuleBuilder m_modBuilder;

        private TypesInAssemblyManager m_refAsmTypes;        

        #endregion IFields
        #region IConstructors

        public TypeManager(ModuleBuilder modBuilder, TypesInAssemblyManager refAsmTypes) {
            m_modBuilder = modBuilder;
            m_refAsmTypes = refAsmTypes;
        }

        #endregion IConstructors
        #region IMethods
        
        /// <summary>
        /// creates a typename for a nested type, which is not nestable
        /// directly in containing type.
        /// These types are defined in a special namespace.
        /// </summary>
        public string GetFullTypeNameForNestedNotInOuterType(Symbol forSymbol) {
            Scope declIn = forSymbol.getDeclaredIn();
            return declIn.getFullyQualifiedNameForNested(forSymbol.getSymbolName());
        }
        
        public string GetFullTypeNameForSymbol(Symbol forSymbol) {
            Scope declIn = forSymbol.getDeclaredIn();
            if (!(IsNestedType(forSymbol) && !IsNestableDirectlyInParent(forSymbol))) {
                String fullName = declIn.getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());
                return fullName;                
            } else {
                // forSymbol represents a nested type, which is not directly nestable into
                // the outer type -> type is defined in a special namespace
                return GetFullTypeNameForNestedNotInOuterType(forSymbol);
            }
        }

        /// <summary>
        /// register a not fully created type
        /// </summary>
        public void RegisterTypeFwdDecl(TypeBuilder type, Symbol forSymbol) {
            if (forSymbol == null) { 
                throw new Exception("register error for type: " + 
                                    type.FullName + ", symbol may not be null"); 
            }
            if (IsTypeDeclarded(forSymbol)) {
                throw new Exception("internal error; a type with the name " + 
                                    GetKnownType(forSymbol).GetCompactClsType().FullName +
                                    " is already declared for symbol: " + forSymbol);
            }
            TypeContainer container = new CompileTimeTypeContainer(this, type);
            m_typesInCreation[forSymbol] = container;
        }

        /// <summary>
        /// is at least a forward declaration for the type represented by the symbol present
        /// </summary>
        public bool IsTypeDeclarded(Symbol forSymbol) {
            TypeContainer type = GetKnownType(forSymbol);
            if (type == null) { 
                return false; 
            } else {
                return true;
            }
        }
        
        /// <summary>
        /// is a full definitaion present for the type represented by the symbol forSymbol
        /// </summary>
        /// <param name="forSymbol"></param>
        /// <returns></returns>
        public bool IsTypeFullyDeclarded(Symbol forSymbol) {
            if ((!IsFwdDeclared(forSymbol)) && (IsTypeDeclarded(forSymbol))) { 
                return true; 
            } else {
                return false;    
            }
        }

        public bool IsFwdDeclared(Symbol forSymbol) {
            TypeContainer result = (TypeContainer)m_typesInCreation[forSymbol];
            if (result == null) {
                return false;
            } else {
                return true;
            }
        }

        /// <summary>
        /// checks, if a type is already declared in a referenced assembly
        /// </summary>
        public bool IsTypeDeclaredInRefAssemblies(Symbol forSymbol) {           
           return GetTypeFromRefAssemblies(forSymbol) != null;
        }
        
        private Type GetTypeFromRefAssemblies(Symbol forSymbol) {
            string fullName = GetFullTypeNameForSymbol(forSymbol);          
            String repId = 
                forSymbol.getDeclaredIn().getRepositoryIdFor(forSymbol.getSymbolName());
            return m_refAsmTypes.GetTypeFromRefAssemblies(fullName, repId);
        }

        #region methods for supporting generation for more than one parse result    
        
        /// <summary>checks if a type generation can be skipped, 
        /// because type is already defined in a previous run over a parse tree 
        /// or in a reference assembly
        /// </summary>
        /// <remarks>this method is used to support runs over more than one parse tree
        /// </remarks>
        public bool CheckSkip(Symbol forSymbol) {
        
            if (IsTypeDeclaredInRefAssemblies(forSymbol)) {
                // skip, because already defined in a referenced assembly 
                // -> this overrides the def from idl
                return true;
            }
        
            if (CheckInBuildModulesForType(forSymbol)) { 
                // safe to skip, because type is already fully declared in a previous run
                return true;
            }
                
            // do not skip
            return false;
        }
        
        /// <summary>
        /// checks if a type is fully declared in a module of the resulting assembly.
        /// Does only return fully defined types, others are not interesting here.
        /// </summary>  
        private Type GetTypeFromBuildModule(Symbol forSymbol) {
            string fullName = GetFullTypeNameForSymbol(forSymbol);
            Debug.WriteLine("check type already defined in buildmodule: " + fullName + "; symbol: " + forSymbol.getSymbolName());
            Type result = GetTypeFromBuildModule(fullName);
            Debug.WriteLine("type found: " + (result != null));
            if (!(result is TypeBuilder)) {  // type is fully defined (do not return not fully defined types here)
                Debug.WriteLine("type is already complete");
                return result;
            }
            return null;
        }
        
        /// <summary>
        /// checks, if type is known by ModuleBuilder and if yes, returns it;
        /// otherwise, returns null.
        ///</summary>
        internal Type GetTypeFromBuildModule(string fullName) {
            Type result = m_modBuilder.GetType(fullName);
            return result;
        }
        
        /// <summary>
        ///  checks, if a type is already defined in a previous run
        /// </summary>
        public bool CheckInBuildModulesForType(Symbol forSymbol) {
            if (GetTypeFromBuildModule(forSymbol) != null) {
                return true;
            } else {
                return false;
            }
        }

        #endregion methods for supporting generation for more than one parse result

        /// <summary> 
        /// check, that no types not defined left; if so this is an internal error, 
        /// because already checked during symbol table build.
        /// </summary>
        public void AssertAllTypesDefined() {
            if (m_typesInCreation.Count > 0) {
                foreach (TypeContainer container in m_typesInCreation.Values) {
                    Console.WriteLine("internal error, only forward declared: " + 
                                      container.GetCompactClsType());
                    // this should not occur, because checked by symbol table
                }
                throw new Exception("internal error occured, not all types fully defined");
            }
        }

        /// <summary> 
        /// get the full defined Type or the fwd decl
        /// </summary>
        public TypeContainer GetKnownType(Symbol forSymbol) {
            TypeContainer result = (TypeContainer)m_typeTable[forSymbol];
            if (result == null) {
                result = (TypeContainer)m_typesInCreation[forSymbol];
            }
            if (result == null) {
                Type fromBuildMod = GetTypeFromBuildModule(forSymbol);
                if (fromBuildMod != null) {
                    result = new CompileTimeTypeContainer(this, fromBuildMod);
                }        
            }
            if (result == null) { 
                // check in types, which are defined in referenced assemblies

                Type fromAsm = GetTypeFromRefAssemblies(forSymbol);
                if (fromAsm != null) {
                    // remark: all types in ref assemblies are fully completed -> no need for compileTimeTypeContainer
                    result = new TypeContainer(fromAsm);
                }
            }
            if ((result == null) && (m_sequenceRecursionAllowedType != null) &&
                m_sequenceRecursionAllowedType.SymbolPart.Equals(forSymbol)) {
                result = m_sequenceRecursionAllowedType.TypePart;                
            }
            return result;
        }
        
        /// <summary>add a fully defined type to the known types</summary>
        private void AddTypeDefinition(TypeContainer fullDecl, Symbol forSymbol) {
            if (m_typesInCreation.ContainsKey(forSymbol)) { 
                throw new Exception("type can't be registered, a fwd declaration exists");  // should not occur, check by sym table
            }
            if (m_typeTable.ContainsKey(forSymbol)) { 
                throw new Exception("type already defined"); // should not occur, checked by sym table
            }
            m_typeTable[forSymbol] = fullDecl;
        }
        
        /// <summary>register a full type definition (CreateType() already called)</summary>
        public void RegisterTypeDefinition(Type fullDecl, Symbol forSymbol) {
            TypeContainer container = new CompileTimeTypeContainer(this, fullDecl);
            AddTypeDefinition(container, forSymbol);
        }

        /// <summary>
        /// use this to tell the type manager, that a type is now fully created.
        /// The typemanager checks at the end, if not fully declared types exists and throw an error, if so
        /// </summary>
        public void ReplaceFwdDeclWithFullDecl(Type fullDecl, Symbol forSymbol) {
            m_typesInCreation.Remove(forSymbol);
            // add to the fully created types
            TypeContainer container = new CompileTimeTypeContainer(this, fullDecl);
            m_typeTable[forSymbol] = container;
        }

        public void RegisterTypeDef(TypeContainer fullDecl, Symbol forSymbol) {
            AddTypeDefinition(fullDecl, forSymbol);
        }
        
        /// <summary>
        /// allows sequence recursion for unions/struct. Don't forget to call
        /// UnpublishTypeForSequenceRecursion after sequence elem type is identified
        /// </summary>
        public void PublishTypeForSequenceRecursion(Symbol typeSym, TypeBuilder type) {
            if (!IsTypeDeclarded(typeSym)) {
                TypeContainer container =
                    new CompileTimeTypeContainer(this, type);
                m_sequenceRecursionAllowedType = new TypeSymbolPair(typeSym, container);
            }            
        }
        
        /// <summary>
        /// called after sequence element type has been identified
        /// </summary>
        public void UnpublishTypeForSequenceRecursion() {
            m_sequenceRecursionAllowedType = null;
        }
               
        /// <summary>
        /// is the type represented by forSymbol nested in another type?
        /// </summary>
        private bool IsNestedType(Symbol forSymbol) {
            Scope parentScope = forSymbol.getDeclaredIn();
            return parentScope.IsTypeScope();
        }
        
        /// <summary>Checks, if a nested type is </summary>
        private bool IsNestableDirectlyInParent(Symbol forSymbol) {
            Scope potentialTypeScope = forSymbol.getDeclaredIn();
            string potentialTypeName = potentialTypeScope.getScopeName();
            if ((potentialTypeScope.getParentScope() != null) &&
                (potentialTypeScope.IsTypeScope())) {
                Symbol typeSymbol = 
                    potentialTypeScope.getParentScope().getSymbol(potentialTypeName);
                TypeContainer type = GetKnownType(typeSymbol);
                if (type.GetCompactClsType().IsClass) {
                    return true;
                }
            }
            return false;
        }                

        #endregion IMethods                     

    }

}
