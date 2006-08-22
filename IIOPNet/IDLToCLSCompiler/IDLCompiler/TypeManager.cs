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
using Ch.Elca.Iiop.IdlCompiler.Exceptions;


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
        /// <summary>
        /// a subset of types in creation, lists all the fwd declarations not completed
        /// </summary>
        private Hashtable m_fwdDeclaredTypes = new Hashtable();
        private Hashtable m_completeTypeTable = new Hashtable();

        /// <summary>
        /// for structs, union: only recursion allowed is through an idl sequence -> handle special
        /// </summary>
        private TypeSymbolPair m_sequenceRecursionAllowedType = null;        
        
        private ModuleBuilder m_modBuilder;
        private TypesInAssemblyManager m_refAsmTypes;        
        
        private BoxedValueTypeGenerator m_boxedValueTypeGenerator = new BoxedValueTypeGenerator();

        #endregion IFields
        #region IConstructors

        public TypeManager(ModuleBuilder modBuilder, TypesInAssemblyManager refAsmTypes,
                           SymbolTable symTable) {
            m_modBuilder = modBuilder;
            m_refAsmTypes = refAsmTypes;
            InitalizePredefinedSymbolMappings(symTable);
        }

        #endregion IConstructors
        #region IMethods
        
        private void InitalizePredefinedSymbolMappings(SymbolTable symTable) {
            Symbol abstrBase = symTable.ResolveScopedNameToSymbol(symTable.getTopScope(),
                                                                  new ArrayList(new string[] { "omg.org", "CORBA", "AbstractBase" }));
            m_completeTypeTable[abstrBase] = 
                new TypeContainer(ReflectionHelper.ObjectType,
                                  new AttributeExtCollection(new Attribute[] { new ObjectIdlTypeAttribute(IdlTypeObject.AbstractBase) }));
        }
        
        #region TypeCreation
        
        /// <summary>finds the repository id attribute for the symbol starting the search in startInScope</summary>
        private string FindRepositoryId(Symbol forSymbol) {
            Scope startInScope = forSymbol.getDeclaredIn();
            string forName = forSymbol.getSymbolName();
            string result = startInScope.getRepositoryIdFor(forName);
            if (result == null) {
                // try inside scope to find it
                Scope innerScope = startInScope.getChildScope(forName);
                if (innerScope != null) {
                    result = innerScope.getRepositoryIdFor(forName);
                }
            }
            return result;
        }
               
        /// <summary>
        /// start a type definition for a type
        /// </summary>        
        /// <param name="isForwardDeclaration">specifies, if this type is created from an idl forward declaration</param>
        private TypeBuilder StartTypeDefinition(Symbol typeSymbol, string fullyQualName,
                                               TypeAttributes typeAttributes, Type parent, Type[] interfaces,
                                               bool isForwardDeclaration) {                                   
                                              
             if (IsTypeDeclarded(typeSymbol)) {
                 // was not skipped, tried to redeclare -> internal error
                 throw new InternalCompilerException("A type with the name " + 
                                                     GetKnownType(typeSymbol).GetCompactClsType().FullName +
                                                     " is already declared for symbol: " + typeSymbol);
             }            
            
            TypeBuilder result = m_modBuilder.DefineType(fullyQualName, typeAttributes, parent, interfaces);
            AddRepositoryIdAttribute(result, typeSymbol); // every constructed type should have a rep-id.
            // book-keeping
            TypeContainer container = new CompileTimeTypeContainer(this, result);
            m_typesInCreation[typeSymbol] = container;
                                                                                         
            if (isForwardDeclaration) {
                // store type-fwd declaration in a special place, to allow searching only those when trying to complete a fwd declaration.
                m_fwdDeclaredTypes[typeSymbol] = container;
            }
                                                   
            return result;
        }

        
        /// <summary>
        /// start a type definition for a type
        /// </summary>        
        /// <param name="isForwardDeclaration">specifies, if this type is created from an idl forward declaration</param>
        public TypeBuilder StartTypeDefinition(Symbol typeSymbol,
                                               TypeAttributes typeAttributes, Type parent, Type[] interfaces,
                                               bool isForwardDeclaration) {
            Scope enclosingScope = typeSymbol.getDeclaredIn();
            String fullyQualName = enclosingScope.GetFullyQualifiedNameForSymbol(typeSymbol.getSymbolName());
            return StartTypeDefinition(typeSymbol, fullyQualName, 
                                       typeAttributes, parent, interfaces, isForwardDeclaration);
        }
        
        /// <summary>
        /// start a type definition for a type with System.Object as parent
        /// </summary>        
        /// <param name="isForwardDeclaration">specifies, if this type is created from an idl forward declaration</param>
        public TypeBuilder StartTypeDefinition(Symbol typeSymbol,
                                               TypeAttributes typeAttributes, Type[] interfaces,
                                               bool isForwardDeclaration) {
            return StartTypeDefinition(typeSymbol, typeAttributes, typeof(System.Object), interfaces,
                                       isForwardDeclaration);        
        }
        
        /// <summary>
        /// Start a type definition for a boxed value type
        /// Adds also repository id and IIdlEntity inheritance.
        /// </summary>
        public TypeBuilder StartBoxedValueTypeDefinition(Symbol typeSymbol, Type boxedType, 
                                                         CustomAttributeBuilder[] boxedTypeAttributes) {            

            if (IsTypeDeclarded(typeSymbol)) {
                // was not skipped, tried to redeclare -> internal error
                throw new InternalCompilerException("A type with the name " + 
                                                    GetKnownType(typeSymbol).GetCompactClsType().FullName +
                                                    " is already declared for symbol: " + typeSymbol);
            }

            Scope enclosingScope = typeSymbol.getDeclaredIn();
            String fullyQualName = enclosingScope.GetFullyQualifiedNameForSymbol(typeSymbol.getSymbolName());
                                                    
            Trace.WriteLine("generating code for boxed value type: " + fullyQualName);
            
            TypeBuilder result =
                m_boxedValueTypeGenerator.CreateBoxedType(boxedType, m_modBuilder,
                                                          fullyQualName, boxedTypeAttributes);
                                                            
            // book-keeping
            TypeContainer container = new CompileTimeTypeContainer(this, result);
            m_typesInCreation[typeSymbol] = container;
            
            // repository and iidlentiry inheritance
            result.AddInterfaceImplementation(typeof(IIdlEntity));
            AddRepositoryIdAttribute(result, typeSymbol);            
            
            return result;            
        }    
        
        public UnionGenerationHelper StartUnionTypeDefinition(Symbol typeSymbol, string fullyQualName) {            
            
            if (IsTypeDeclarded(typeSymbol)) {
                // was not skipped, tried to redeclare -> internal error
                throw new InternalCompilerException("A type with the name " + 
                                                    GetKnownType(typeSymbol).GetCompactClsType().FullName +
                                                    " is already declared for symbol: " + typeSymbol);
            }            
            
            UnionGenerationHelper genHelper = new UnionGenerationHelper(m_modBuilder, fullyQualName,
                                                                        TypeAttributes.Public);
            AddRepositoryIdAttribute(genHelper.Builder, typeSymbol);
            // book-keeping
            TypeContainer container = new CompileTimeTypeContainer(this, genHelper.Builder);
            m_typesInCreation[typeSymbol] = container;

            return genHelper;
        }
                               
        public Type EndUnionTypeDefinition(Symbol typeSymbol, UnionGenerationHelper helper) {
            Type resultType = helper.FinalizeType();
            
            // book-keeping
            m_typesInCreation.Remove(typeSymbol);
            AddCompleteTypeDefinition(typeSymbol, new CompileTimeTypeContainer(this, resultType));
            
            return resultType;            
        }
        
        
        /// <summary>
        /// completed the type definiton by creation toComplete.CreateType and
        /// updates internal state used to tell about status of types
        /// </summary>
        /// <param name="toComplete"></param>
        /// <returns></returns>
        public Type EndTypeDefinition(Symbol typeSymbol) {                                                
            TypeBuilder toComplete = GetTypeFromTypesInCreation(typeSymbol);                                                
            Type resultType = toComplete.CreateType();
            
            // update the book-keeping
            m_typesInCreation.Remove(typeSymbol);
            m_fwdDeclaredTypes.Remove(typeSymbol);
            AddCompleteTypeDefinition(typeSymbol,new CompileTimeTypeContainer(this, resultType));
            return resultType;                        
        }                
        
        #endregion TypeCreation
        
        public TypeBuilder GetIncompleteForwardDeclaration(Symbol typeSymbol) {
            TypeBuilder result = GetIncompleteForwardDeclartionUnchecked(typeSymbol);
            if (result != null) {
                return result;
            } else {
                // must be found, otherwise an implementation error ...
                throw new InternalCompilerException("forward decl not retrieved again for " + typeSymbol);
            }
        }
        
        /// <summary>
        /// the same as GetIncompleteForwardDeclaration, but throws no exception if not found
        /// </summary>
        private TypeBuilder GetIncompleteForwardDeclartionUnchecked(Symbol typeSymbol) {
            TypeContainer container = (TypeContainer)m_fwdDeclaredTypes[typeSymbol];
            TypeBuilder result = (container != null ? container.GetCompactClsType() as TypeBuilder : null);
            return result;
        }
        
        /// <summary>
        /// returns the typebuilder if found among types in creation, otherwise throws exception
        /// </summary>                
        private TypeBuilder GetTypeFromTypesInCreation(Symbol typeSymbol) {
            TypeContainer container = (TypeContainer)m_typesInCreation[typeSymbol];
            TypeBuilder toComplete = (container != null ? container.GetCompactClsType() as TypeBuilder : null);
            if (toComplete != null) {
                return toComplete;            
            } else {
                throw new InternalCompilerException("type in creation not found: " + typeSymbol);
            }
        }
        
        /// <summary>add a fully defined type to the known types</summary>
        private void AddCompleteTypeDefinition(Symbol typeSymbol, TypeContainer fullDecl) {
            if (m_typesInCreation.ContainsKey(typeSymbol)) { 
                throw new InternalCompilerException("type can't be registered, an incomplete declaration exists; symbol: " + typeSymbol);  // should not occur, check by sym table
            }
            if (m_completeTypeTable.ContainsKey(typeSymbol)) { 
                throw new InternalCompilerException("type already defined; symbol: " + typeSymbol); // should not occur, checked by sym table
            }
            
            m_completeTypeTable[typeSymbol] = fullDecl;
        }
        
        private bool IsIncompleteDeclarationPresent(Symbol typeSymbol) {
            return m_typesInCreation[typeSymbol] != null;
        }                        
        
        /// <summary>
        /// is at least a forward declaration for the type represented by the symbol present
        /// </summary>
        public bool IsTypeDeclarded(Symbol forSymbol) {
            TypeContainer type = GetKnownType(forSymbol);
            return (type != null);
        }
        
        public bool IsFwdDeclared(Symbol forSymbol) {
            return GetIncompleteForwardDeclartionUnchecked(forSymbol) != null;
        }

        #region respositoryIds
        
        private void AddRepositoryIdAttribute(Symbol typeSymbol) {                        
            TypeBuilder inCreation = GetTypeFromTypesInCreation(typeSymbol);
            AddRepositoryIdAttribute(inCreation, typeSymbol);            
        }
        
        private void AddRepositoryIdAttribute(TypeBuilder typeBuild, Symbol typeSymbol) {
            string repositoryId = FindRepositoryId(typeSymbol);
            if (repositoryId == null) {
                // no repository id specified, create one from the idl, because of special name mappings
                // creating a rep-id in Channel code can lead to the wrong one ...                
                repositoryId = typeSymbol.ConstructRepositoryId();
            }
            AddRepositoryIdAttribute(typeBuild, repositoryId);
        }
                
        private void AddRepositoryIdAttribute(TypeBuilder typebuild, string repId) {
            if (repId != null) {
                IlEmitHelper.GetSingleton().AddRepositoryIDAttribute(typebuild, repId);
            }
        }                
        
        #endregion respositoryIds
                
        private string GetFullTypeNameForSymbol(Symbol forSymbol) {
            Scope declIn = forSymbol.getDeclaredIn();
            return declIn.GetFullyQualifiedNameForSymbol(forSymbol.getSymbolName());
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
        
            // check here directly on module, to find out about runs of the compiler for
            // previous files (for every run over a new file on the IDL To CLS argument line, 
            // a new TypeManager is used)
            Type inBuildModule = GetTypeFromBuildModule(forSymbol);
            if (inBuildModule != null && !IsIncompleteDeclarationPresent(forSymbol)) { 
                // safe to skip, because type is already fully declared in this run
                return true;
            }
                
            // do not skip
            return false;
        }
        
        /// <summary>
        /// checks if a type is known by ModuleBuilder and if yes, returns it; otherwise; returns null        
        /// </summary>  
        private Type GetTypeFromBuildModule(Symbol forSymbol) {
            string fullName = GetFullTypeNameForSymbol(forSymbol);
            Type result = GetTypeFromBuildModule(fullName);
            return result;
        }
        
        /// <summary>
        /// checks, if type is known by ModuleBuilder and if yes, returns it;
        /// otherwise, returns null.
        ///</summary>
        internal Type GetTypeFromBuildModule(string fullName) {
            Type result = m_modBuilder.GetType(fullName);
            return result;
        }
        
        #endregion methods for supporting generation for more than one parse result

        /// <summary> 
        /// get the full defined Type or the fwd decl
        /// </summary>
        public TypeContainer GetKnownType(Symbol forSymbol) {
            // search in all complete types from this run, also typesdefs, ...
            TypeContainer result = (TypeContainer)m_completeTypeTable[forSymbol];
            if (result == null) {
                result = (TypeContainer)m_typesInCreation[forSymbol];
            }
            if (result == null) {
                // search in build-module to get also types from a run over a previous root idl-file (one specified on the command line)
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
                
        /// <summary>
        /// register a resolved typedef
        /// </summary>
        public void RegisterTypeDef(TypeContainer fullDecl, Symbol forSymbol) {
            AddCompleteTypeDefinition(forSymbol, fullDecl);
        }
        
        /// <summary> 
        /// check, that no types not defined left; if so this is an internal error, 
        /// because already checked during symbol table build.
        /// </summary>
        public void AssertAllTypesDefined() {
            if (m_typesInCreation.Count > 0) {
                foreach (TypeContainer container in m_typesInCreation.Values) {
                    Console.WriteLine("internal error, not fully declared: " + 
                                      container.GetCompactClsType());
                    // this should not occur, because checked by symbol table
                }
                throw new Exception("internal error occured, not all types fully defined");
            }
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
                
        #endregion IMethods                     

    }

}
