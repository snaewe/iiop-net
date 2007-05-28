/* MetadataGenerator.cs
 * 
 * Project: IIOP.NET
 * IDLToCLSCompiler
 * 
 * WHEN      RESPONSIBLE
 * 14.02.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using System.CodeDom;
using System.CodeDom.Compiler;
using parser;
using symboltable;
using Ch.Elca.Iiop.Idl;
using Ch.Elca.Iiop.Marshalling;
using Ch.Elca.Iiop.Util;
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.IdlCompiler.Exceptions;


namespace Ch.Elca.Iiop.IdlCompiler.Action {

/// <summary>
/// generates CLS metadeta for OMG IDL
/// </summary>
public class MetaDataGenerator : IDLParserVisitor {

    
    #region Types

    /// <summary>helper class, to pass information</summary>
    private class BuildInfo {
        
        #region IFields

        private TypeBuilder m_containerType;
        private Symbol m_containerSymbol;
        private Scope m_buildScope;        
        
        #endregion IFields
        #region IConstructors
        
        public BuildInfo(Scope buildScope, TypeBuilder containerType, 
                         Symbol containerSymbol) {
            m_buildScope = buildScope;
            m_containerType = containerType;
            m_containerSymbol = containerSymbol;
        }

        #endregion IConstructors
        #region IMethods

        public TypeBuilder GetContainterType() {
            return m_containerType;
        }
        public Symbol GetContainerSymbol() {
            return m_containerSymbol;
        }
        public Scope GetBuildScope() {
            return m_buildScope;
        }        

        #endregion IMethods
    }

    /// <summary>
    /// helper class to pass information for union-visitor methods
    /// </summary>
    private class UnionBuildInfo : BuildInfo {
        #region IFields

        private UnionGenerationHelper m_helper;

        #endregion IFields
        #region IConstructors

        public UnionBuildInfo(Scope buildScope, UnionGenerationHelper helper,
                              Symbol containerSymbol) : 
                              base(buildScope, helper.Builder, containerSymbol) {
            m_helper = helper;
        }

        #endregion IConstructors
        #region IMethods

        public UnionGenerationHelper GetGenerationHelper() {
            return m_helper;
        }

        #endregion IMethods

    }
    
    /// <summary>
    /// helper class to pass information for valuetype-visitor methods
    /// </summary>    
    private class ConcreteValTypeBuildInfo : BuildInfo {
        
        #region IFields
        
        private ConstructorBuilder m_defaultConstr;
        
        #endregion IFields
        #region IConstructors
                
        public ConcreteValTypeBuildInfo(Scope buildScope,
                                        TypeBuilder containerType,
                                        Symbol containerSymbol,
                                        ConstructorBuilder defConstr) :
                                            base(buildScope, containerType,
                                                 containerSymbol) {
            m_defaultConstr = defConstr;
        }
        
        #endregion IConstructors        
        #region IProperties
        
        /// <summary>
        /// the default constructor for the value type class
        /// </summary>        
        public ConstructorBuilder DefaultConstructor {
            get {
                return m_defaultConstr;
            }
        }
        
        #endregion IProperties
        
    }
    
    #endregion Types
    #region IFields

    private SymbolTable m_symbolTable;

    private AssemblyBuilder m_asmBuilder;

    private ModuleBuilder m_modBuilder;

    private TypeManager m_typeManager;

    private AssemblyName m_targetAsmName;

    private TypesInAssemblyManager m_typesInRefAsms;

    private Type m_interfaceToInheritFrom = null;

    /** is the generator initalized for parsing a file */
    private bool m_initalized = false;

    private IlEmitHelper m_ilEmitHelper = IlEmitHelper.GetSingleton();

    /** used to store value types generated: for this types an implementation class must be provided */
    private ArrayList m_valueTypesDefined = new ArrayList();
    
    private DirectoryInfo m_valTypeImplSkelTargetDir = new DirectoryInfo(".");
    private bool m_valTypeImplSkelOverwriteWhenExist = false;
    private CodeDomProvider m_valTypeImplSkelProvider = null;
    
    private bool m_mapAnyToAnyContainer = false; // default is map any to object

    #endregion IFields
    #region IConstructors

    /// <param name="refAssemblies">
    /// contains a list of assemblies, which contains
    /// allready generated types for idl types
    /// PRE: must be != null
    /// </param>
    /// <param name="signKey">the key to use to sign the generated assembly; pass null to not sign the assembly</param>
    public MetaDataGenerator(AssemblyName targetAssemblyName, String targetDir,
                             IList refAssemblies) {
        m_targetAsmName = targetAssemblyName;        
        // define a persistent assembly
        CreateResultAssembly(targetDir);
        // channel assembly contains predef types, which shouldn't be regenerated.
        InitalizeRefAssemblies(refAssemblies);        
    }

    #endregion IConstructors
    #region IProperties     
    
    /// <summary>
    /// should idl any be mapped to object (false) or to the container omg.org.CORBA.Any (true)
    /// </summary>
    public bool MapAnyToAnyContainer {
        get {
            return m_mapAnyToAnyContainer;
        }
        set {
            m_mapAnyToAnyContainer = value;
        }
    }

    /// <summary>
    /// optionally defines an base interface for the generated interface.
    /// </summary>
    public Type InheritedInterface {
        get {
            return m_interfaceToInheritFrom;
        }
        set {
            m_interfaceToInheritFrom = value;
        }
    }

    #if UnitTest
    
    public Assembly ResultAssembly {
        get {
            return m_asmBuilder;
        }
    }
    
    #endif    
    
    #endregion IProperties
    #region IMethods
    
    private string GetDllName() {
        return m_targetAsmName.Name + ".dll";
    }
    
    /// <summary>
    /// creates the persistent assembly and the module, which will hold the
    /// resulting CLS
    /// </summary>
    private void CreateResultAssembly(string targetDir) {

        m_asmBuilder = System.Threading.Thread.GetDomain().
            DefineDynamicAssembly(m_targetAsmName, AssemblyBuilderAccess.RunAndSave,
                                  targetDir);
        // define one module containing the resulting CLS
        string modName = "_" + m_targetAsmName.Name + ".netmodule";
        m_modBuilder = m_asmBuilder.DefineDynamicModule(modName, GetDllName());
    }
    
    /// <summary>initalizes the assemblies, which contains type to use
    /// instead of generating them</summary>
    private void InitalizeRefAssemblies(IList refAssemblies) {
        // add the IIOPChannel dll; IIdlAttribute is in channel assembly
        Type typeInChannel = typeof(IIdlAttribute);
        ArrayList refAssembliesWithChannelAsm = new ArrayList(refAssemblies);
        refAssembliesWithChannelAsm.Add(typeInChannel.Assembly);
        m_typesInRefAsms = new TypesInAssemblyManager(refAssembliesWithChannelAsm);
    }    

    ///<summary>
    /// ends the build process, after this is called, the Generator is not able to process more files
    ///</summary>
    public void SaveAssembly() {
        // save the assembly to disk
        m_asmBuilder.Save(GetDllName());
        
        if (m_valTypeImplSkelProvider != null) {
            CreateValueTypeImplSkeletons();
        } else {
            // print a remark to remember implementing the valuetypes:
            PrintNeededValueImplList();
        }
    }
    
    /// <summary>
    /// prints a list of value types for which an implementation must be provided.
    /// </summary>
    private void PrintNeededValueImplList() {
        if (m_valueTypesDefined.Count > 0) {
            Console.WriteLine("\nDon't forget to provide an implementation for the following value types: \n");
            for (int i = 0; i < m_valueTypesDefined.Count; i++) {
                Console.WriteLine(((Type)m_valueTypesDefined[i]).FullName);
            }
            Console.WriteLine("");
        }        
    }
    
    /// <summary>
    /// Enable the generation of skeletons for corba value types
    /// </summary>
    /// <param name="provider">
    /// The provider to use for generation; must be != null.
    /// </param>
    /// <param name="targetDir">
    /// The targetDirectory for the generated files
    /// </param>
    /// <param name="overwriteWhenExist">
    /// Specify, if an already existing file should be overwritten.
    /// </param>
    public void EnableValueTypeSkeletonGeneration(CodeDomProvider provider,
                                                  DirectoryInfo targetDir,
                                                  bool overwriteWhenExist) {
        if (provider == null) {
            throw new ArgumentException("provider must be != null");
        }
        m_valTypeImplSkelProvider = provider;
        m_valTypeImplSkelTargetDir = targetDir;
        m_valTypeImplSkelOverwriteWhenExist = overwriteWhenExist;    
    }    
    
    /// <sumamry>
    /// Creates skeleton implementations for the value type implementations.
    /// </sumamry>
    private void CreateValueTypeImplSkeletons() {
        ValueTypeImplGenerator gen = 
            new ValueTypeImplGenerator(m_valTypeImplSkelProvider,
                                       m_valTypeImplSkelTargetDir,
                                       m_valTypeImplSkelOverwriteWhenExist);
        if (m_valueTypesDefined.Count > 0) {
            Console.WriteLine("\nDon't forget to complete implementation for the following value types: \n");
        }
        
        for (int i = 0; i < m_valueTypesDefined.Count; i++) {
            bool generated = gen.GenerateValueTypeImpl((Type)m_valueTypesDefined[i]);
            if (generated) {
                Console.WriteLine(((Type)m_valueTypesDefined[i]).FullName);
            }
        }        
    }

    /// <summary>
    /// initalize the generator for next source, with using the same target assembly / target modules
    /// </summary>
    public void InitalizeForSource(SymbolTable symbolTable) {
        m_symbolTable = symbolTable;
        m_symbolTable.CheckAllFwdDeclsComplete(); // assure symbol table is valid: all fwd decls are defined by a full definition
        // helps to find already declared types
        m_typeManager = new TypeManager(m_modBuilder, m_typesInRefAsms, symbolTable);
        // ready for code generation
        m_initalized = true;
    }    
    
    /// <summary> 
    /// get the types for the scoped names specified in an inheritance relationship
    /// </summary>
    /// <param name="data">the buildinfo of the container of the type having this inheritance relationship</param>    
    private Type[] ParseInheritanceRelation(SimpleNode node, BuildInfo data) {        
        ArrayList result = new ArrayList();
        for (int i = 0; i < node.jjtGetNumChildren(); i++) {
            // get symbol
            Symbol sym = (Symbol)(node.jjtGetChild(i).jjtAccept(this, data)); // accept interface_name
            if (sym.getDeclaredIn().GetFullyQualifiedNameForSymbol(sym.getSymbolName()).Equals("java.io.Serializable")) {
                Console.WriteLine("ignoring inheritance from java.io.Serializable, because not allowed");                
                continue;
            }
            // get Type
            TypeContainer resultType = m_typeManager.GetKnownType(sym);
            if (resultType == null) {
                // this is an error: type must be created before it is inherited from
                throw new InvalidIdlException("type " + sym.getSymbolName() +
                                              " not seen before in inheritance spec");
            } else if (m_typeManager.IsFwdDeclared(sym)) {
                // this is an error: can't inherit from a fwd declared type
                throw new InvalidIdlException("type " + sym.getSymbolName() + 
                                              " only fwd declared, but for inheritance full definition is needed");
            }
            result.Add(resultType.GetCompactClsType());
        }
        return (System.Type[])result.ToArray(typeof(Type));
    }

    ///<summary>check if data is an instance of buildinfo, if not throws an exception</summary>
    private void CheckParameterForBuildInfo(Object data, Node visitedNode) {
        if (!(data is BuildInfo)) { 
            throw new InternalCompilerException("precondition violation in visitor for node" + visitedNode.GetType() +
                                                ", " + data.GetType() + " but expected BuildInfo"); 
        }
    }

    /// <summary>
    /// <see cref="parser.IDLParserVisitor.visit(SimpleNode, Object)"/>
    /// </summary>
    public Object visit(SimpleNode node, Object data) {
        return null; // not needed
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTspecification, Object)
     * @param data unused
     */
    public Object visit(ASTspecification node, Object data) {
        if (!m_initalized) { 
            throw new InternalCompilerException("initalize not called"); 
        }
        Scope topScope = m_symbolTable.getTopScope();
        BuildInfo info = new BuildInfo(topScope, null, null);
        node.childrenAccept(this, info);
        m_initalized = false; // this file is finished
        m_typeManager.AssertAllTypesDefined(); // check if all types are completely defined. if not ok, assembly can't be saved to file.
        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTdefinition, Object)
     * @param data an instance of buildinfo is expected
     */
    public Object visit(ASTdefinition node, Object data) {
        CheckParameterForBuildInfo(data, node);
        node.childrenAccept(this, data);
        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTmodule, Object)
     * @param data an instance of buildInfo is expected
     */
    public Object visit(ASTmodule node, Object data) {
        CheckParameterForBuildInfo(data, node);
        Trace.WriteLine("accepting module with ident: " + node.getIdent());
        BuildInfo info = (BuildInfo) data;
        // info contains the scope this module is defined in
        Scope enclosingScope = info.GetBuildScope();
        Scope moduleScope = enclosingScope.getChildScope(node.getIdent());
        BuildInfo modInfo = new BuildInfo(moduleScope, info.GetContainterType(),
                                          info.GetContainerSymbol());
        node.childrenAccept(this, modInfo);
        Trace.WriteLine("module with ident sucessfully accepted: " + node.getIdent());
        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTinterfacex, Object)
     */
    public Object visit(ASTinterfacex node, Object data) {
        node.childrenAccept(this, data);
        return null;
    }

    /** handles the declaration for the interface definition / fwd declaration
     * @return the TypeBuilder for this interface
     */
    private TypeBuilder CreateOrGetInterfaceDcl(Symbol forSymbol, System.Type[] interfaces, 
                                                bool isAbstract, bool isLocal, bool isForward) {
        TypeBuilder interfaceToBuild = null;
        if (!m_typeManager.IsFwdDeclared(forSymbol)) {
            Trace.WriteLine("generating code for interface: " + forSymbol.getSymbolName());
            interfaceToBuild = m_typeManager.StartTypeDefinition(forSymbol, 
                                                                 TypeAttributes.Interface | TypeAttributes.Public | TypeAttributes.Abstract,
                                                                 null, interfaces, isForward);
            // add InterfaceTypeAttribute
            IdlTypeInterface ifType = IdlTypeInterface.ConcreteInterface;
            if (isAbstract) { 
                ifType = IdlTypeInterface.AbstractInterface; 
            }
            if (isLocal) {
                ifType = IdlTypeInterface.LocalInterface;
            }
            if ((isLocal) && (isAbstract)) {
                throw new InternalCompilerException("internal error: iftype precondition");
            }
            // add interface type
            CustomAttributeBuilder interfaceTypeAttrBuilder = new InterfaceTypeAttribute(ifType).CreateAttributeBuilder();
            interfaceToBuild.SetCustomAttribute(interfaceTypeAttrBuilder);
            interfaceToBuild.AddInterfaceImplementation(typeof(IIdlEntity));
        } else {
            // get incomplete type
            Trace.WriteLine("complete interface: " + forSymbol.getSymbolName());
            interfaceToBuild = m_typeManager.GetIncompleteForwardDeclaration(forSymbol);
            // add inheritance relationship:
            for (int i = 0; i < interfaces.Length; i++) {
                interfaceToBuild.AddInterfaceImplementation(interfaces[i]);
            }
        }
        return interfaceToBuild;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTinterface_dcl, Object)
     * @param data expected is the buildinfo of the scope, this interface is declared in
     * @return the created type
     */
    public Object visit(ASTinterface_dcl node, Object data) {
        CheckParameterForBuildInfo(data, node);
        // data contains the scope, this interface is declared in
        Scope enclosingScope = ((BuildInfo) data).GetBuildScope();
        
        
        // an IDL concrete interface
        // get the header
        ASTinterface_header header = (ASTinterface_header)node.jjtGetChild(0);
        Symbol forSymbol = enclosingScope.getSymbol(header.getIdent());
        // check if a type declaration exists from a previous run / in ref assemblies
        if (m_typeManager.CheckSkip(forSymbol)) { 
            return null; 
        }

        // retrieve first types for the inherited
        System.Type[] interfaces = (System.Type[])header.jjtAccept(this, data);
        TypeBuilder interfaceToBuild = CreateOrGetInterfaceDcl(forSymbol, interfaces, 
                                                               header.isAbstract(), header.isLocal(),
                                                               false);

        // generate body
        ASTinterface_body body = (ASTinterface_body)node.jjtGetChild(1);
        BuildInfo buildInfo = new BuildInfo(enclosingScope.getChildScope(forSymbol.getSymbolName()),
                                            interfaceToBuild, forSymbol);
        body.jjtAccept(this, buildInfo);
    
        // create the type
        m_typeManager.EndTypeDefinition(forSymbol);
        return null;
    }
    
    /**
     * @see parser.IDLParserVisitor#visit(ASTforward_dcl, Object)
     * @param data the buildinfo of the scope, this type should be declared in
     */
    public Object visit(ASTforward_dcl node, Object data) {
        CheckParameterForBuildInfo(data, node);
        Scope enclosingScope = ((BuildInfo) data).GetBuildScope();
        // create only the type-builder, but don't call createType()
        Symbol forSymbol = enclosingScope.getSymbol(node.getIdent());
        // check if type is known from a previous run over a parse tree --> if so: skip
        if (m_typeManager.CheckSkip(forSymbol)) { 
            return null; 
        }        
        if (!(m_typeManager.IsTypeDeclarded(forSymbol))) { // ignore fwd-decl if type is already declared, if not generate type for fwd decl
            // it's no problem to add later on interfaces this type should implement with AddInterfaceImplementation,
            // here: specify no interface inheritance, because not known at this point
            CreateOrGetInterfaceDcl(forSymbol, Type.EmptyTypes, 
                                    node.isAbstract(), node.isLocal(), true);
        }
        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTinterface_header, Object)
     * @param data the buildinfo of the container, containing this interface (e.g of a module)
     * @return an array of System.Type containing all the interfaces the intefaced defined with this header extend
     */
    public Object visit(ASTinterface_header node, Object data) {
        Type[] result = new Type[0];
        ArrayList resList = new ArrayList();
        if (node.jjtGetNumChildren() > 0) {
            ASTinterface_inheritance_spec inheritSpec = (ASTinterface_inheritance_spec) node.jjtGetChild(0);
            result = (Type[])inheritSpec.jjtAccept(this, data);
            resList.AddRange(result);
        }
        if (m_interfaceToInheritFrom != null && !node.isLocal()) {
            resList.Add(m_interfaceToInheritFrom);
        }
        return resList.ToArray(typeof(Type));
    }

    /**
     * Adds all exports to the type which is defined at the moment
     * @see parser.IDLParserVisitor#visit(ASTinterface_body, Object)
     * @param data a BuildInfo instance.
     */
    public Object visit(ASTinterface_body node, Object data) {
        node.childrenAccept(this, data); // generate for all exports
        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTexport, Object)
     * @param data expected is: an Instance of BuildInfo
     */
    public Object visit(ASTexport node, Object data) {
        // <export> ::= <type_dcl> | <const_dcl> | <except_dcl> | <attr_dcl> | <op_dcl>
        // let the children add themself to the type in creation
        node.childrenAccept(this, data);
        return null;
    }
    
    /**
     * @see parser.IDLParserVisitor#visit(ASTinterface_inheritance_spec, Object)
     * @param data the buildinfo of the container for this interface (e.g. a module)
     * @return an Array of the types the interface inherits from
     */
    public Object visit(ASTinterface_inheritance_spec node, Object data) {
        CheckParameterForBuildInfo(data, node);
        Type[] result = ParseInheritanceRelation(node, (BuildInfo)data);
        return result;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTinterface_name, Object)
     */
    public Object visit(ASTinterface_name node, Object data) {
        Symbol result = (Symbol)node.jjtGetChild(0).jjtAccept(this, data);
        return result;
    }
            
    /**
     * @see parser.IDLParserVisitor#visit(ASTscoped_name, Object)
     * @param data a buildinfo instance
     * @return the symbol represented by this scoped name or null
     */
    public Object visit(ASTscoped_name node, Object data) {
        CheckParameterForBuildInfo(data, node);
        ArrayList parts = node.getNameParts();
        Scope currentScope = ((BuildInfo) data).GetBuildScope();
        if (node.hasFileScope()) { 
            currentScope = m_symbolTable.getTopScope(); 
        }
        
        Symbol found = m_symbolTable.ResolveScopedNameToSymbol(currentScope, parts);        
        if (found == null) {
            throw new InvalidIdlException("scoped name not resolvable: " + node.getScopedName() + 
                                       "; currentscope: " + ((BuildInfo) data).GetBuildScope().getScopeName()); 
        }        
        return found;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTvalue, Object)
     * @param data the buildino of the container for this valuetype
     */
    public Object visit(ASTvalue node, Object data) {
        // <value> ::= <value_decl> | <value_abs_decl> | <value_box_dcl> | <value_forward_dcl>
        node.childrenAccept(this, data);
        return null;
    }

    /** handles the declaration for the value definition / fwd declaration
     * @return the TypeBuilder for this interface
     */
    private TypeBuilder CreateOrGetValueDcl(Symbol forSymbol,
                                            System.Type parent, System.Type[] interfaces,
                                            bool isAbstract, bool isForwardDecl) {
        TypeBuilder valueToBuild;
        if (!m_typeManager.IsFwdDeclared(forSymbol)) {
            Trace.WriteLine("generating code for value type: " + forSymbol.getSymbolName());
            TypeAttributes attrs = TypeAttributes.Public | TypeAttributes.Abstract;
            if (isAbstract) {
                attrs |= TypeAttributes.Interface;
                if (parent != null) { 
                    // classes are considered as concrete value types
                    throw new InvalidIdlException("not possible for an abstract value type " +
                                                  forSymbol.getDeclaredIn().GetFullyQualifiedNameForSymbol(forSymbol.getSymbolName()) + 
                                                  " to inherit from a concrete one " +
                                                  parent.FullName);
                }
            } else {
                attrs |= TypeAttributes.Class;
            }
            valueToBuild = m_typeManager.StartTypeDefinition(forSymbol, attrs, parent, interfaces, 
                                                             isForwardDecl);            
            if (isAbstract) {
                // add InterfaceTypeAttribute
                IdlTypeInterface ifType = IdlTypeInterface.AbstractValueType;
                CustomAttributeBuilder interfaceTypeAttrBuilder = new InterfaceTypeAttribute(ifType).CreateAttributeBuilder();
                valueToBuild.SetCustomAttribute(interfaceTypeAttrBuilder);
            }
            valueToBuild.AddInterfaceImplementation(typeof(IIdlEntity)); // implement IDLEntity
        } else {
            // get incomplete type
            Trace.WriteLine("complete valuetype: " + forSymbol.getSymbolName());
            valueToBuild = m_typeManager.GetIncompleteForwardDeclaration(forSymbol);
            // add inheritance relationship:
            for (int i = 0; i < interfaces.Length; i++) {
                valueToBuild.AddInterfaceImplementation(interfaces[i]);
            }
            if (parent != null) { 
                valueToBuild.SetParent(parent);
            }
        }
        // add abstract methods for all interface methods, a class inherit from (only if valueToBuild is a class an not an interface)
        // add property to abstract class for all properties defined in an interface (only if valueToBuild is a class an not an interface)
        AddInheritedMembersAbstractDeclToClassForIf(valueToBuild, interfaces);
        return valueToBuild;
    }


    private System.Type[] FlattenInterfaceHierarchy(System.Type[] interfaces) {
        System.Collections.ArrayList result = new System.Collections.ArrayList(interfaces);
        for (int i = 0; i < interfaces.Length; i++) {
            // all inherited interfaces, also inherited by inherited:
            System.Type[] inherited = interfaces[i].GetInterfaces();
            for (int j = 0; j < inherited.Length; j++) {
                if (!result.Contains(inherited[j])) {
                    result.Add(inherited[j]);
                }
            }
        }
        return (System.Type[])result.ToArray(typeof(System.Type));
    }

    ///<summary>add abstract methods for all implemented interfaces to the abstract class,
    ///add properties for all implemented interfaces to the abstrct class</summary>
    private void AddInheritedMembersAbstractDeclToClassForIf(TypeBuilder classBuilder, 
                                                             System.Type[] interfaces) {
        if (!(classBuilder.IsClass)) { 
            return; 
        } // only needed for classes
        
        // make sure to include interfaces inherited by the direct implemented interfaces are also considered here
        interfaces = FlattenInterfaceHierarchy(interfaces);
        for (int i = 0; i < interfaces.Length; i++) {
            Type ifType = interfaces[i];    
            // methods
            MethodInfo[] methods = ifType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            for (int j = 0; j < methods.Length; j++) {
                if (methods[j].IsSpecialName) {
                    continue; // do not add methods with special name, e.g. property accessor methods
                }
                if (ReflectionHelper.IsMethodDefinedOnType(methods[j], classBuilder.BaseType,
                                                           BindingFlags.Instance | BindingFlags.Public)) {
                    continue; // method is already defined on a baseclass -> do not re-add
                }
                // normal parameters
                ParameterInfo[] parameters = methods[j].GetParameters();
                ParameterSpec[] paramSpecs = new ParameterSpec[parameters.Length];
                for (int k = 0; k < parameters.Length; k++) {
                    paramSpecs[k] = new ParameterSpec(parameters[k]);
                }
                m_ilEmitHelper.AddMethod(classBuilder, methods[j].Name, paramSpecs,
                                         new TypeContainer(methods[j].ReturnType, 
                                                           AttributeExtCollection.ConvertToAttributeCollection(
                                                               methods[j].ReturnTypeCustomAttributes.GetCustomAttributes(false)),
                                                           true),
                                         MethodAttributes.Abstract | MethodAttributes.Public |
                                         MethodAttributes.Virtual | MethodAttributes.NewSlot |
                                         MethodAttributes.HideBySig);
                
            }
            // properties
            PropertyInfo[] properties = ifType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            for (int j = 0; j < properties.Length; j++) {
                
                if (ReflectionHelper.IsPropertyDefinedOnType(properties[j], classBuilder.BaseType,
                                                             BindingFlags.Instance | BindingFlags.Public)) {
                    continue; // property is already defined on a baseclass -> do not re-add
                }
                
                TypeContainer propType = new TypeContainer(properties[j].PropertyType,
                                                           AttributeExtCollection.ConvertToAttributeCollection(
                                                           properties[j].GetCustomAttributes(true)), true);
                MethodBuilder getAccessor = 
                    m_ilEmitHelper.AddPropertyGetter(classBuilder, properties[j].Name,
                                                     propType, MethodAttributes.Virtual | MethodAttributes.Abstract |
                                                               MethodAttributes.Public | MethodAttributes.HideBySig | 
                                                               MethodAttributes.SpecialName | MethodAttributes.NewSlot);
                MethodBuilder setAccessor = null;
                if (properties[j].CanWrite) {
                    setAccessor = 
                        m_ilEmitHelper.AddPropertySetter(classBuilder, properties[j].Name,
                                                         propType, MethodAttributes.Virtual | MethodAttributes.Abstract |
                                                                   MethodAttributes.Public | MethodAttributes.HideBySig |
                                                                   MethodAttributes.SpecialName | MethodAttributes.NewSlot);
                }
                
                m_ilEmitHelper.AddProperty(classBuilder, properties[j].Name,
                                           propType,
                                           getAccessor, setAccessor);                                                                                           
            }

        }
    }
    
    /**
     * @see parser.IDLParserVisitor#visit(ASTvalue_decl, Object)
     * @param data an instance of the type buildinfo specifing the scope, this value is declared in
     */
    public Object visit(ASTvalue_decl node, Object data) {
        CheckParameterForBuildInfo(data, node);
        // data contains the scope, this value type is declared in
        Scope enclosingScope = ((BuildInfo) data).GetBuildScope();
        // an IDL concrete value type
        // get the header
        ASTvalue_header header = (ASTvalue_header)node.jjtGetChild(0);
        
        Symbol forSymbol = enclosingScope.getSymbol(header.getIdent());
        // check if type is known from a previous run over a parse tree --> if so: skip
        if (m_typeManager.CheckSkip(forSymbol)) { 
            return null; 
        }
        
        // retrieve first types for the inherited
        System.Type[] inheritFrom = (System.Type[])header.jjtAccept(this, data);

        // check is custom:
        if (header.isCustom()) {    
            System.Type[] newInherit = new System.Type[inheritFrom.Length + 1];
            Array.Copy(inheritFrom, 0, newInherit, 0, inheritFrom.Length);
            newInherit[inheritFrom.Length] = typeof(ICustomMarshalled);
            inheritFrom = newInherit;
        }

        Type baseClass = ReflectionHelper.ObjectType;
        if ((inheritFrom.Length > 0) && (inheritFrom[0].IsClass)) {
            // only the first entry may be a class for a concrete value type:
            // multiple inheritance is not allowed for concrete value types, 
            // the value type from which is inherited from must be first in inheritance list,
            // 3.8.5 in CORBA 2.3.1 spec
            baseClass = inheritFrom[0];
            Type[] tmp = new Type[inheritFrom.Length-1];
            Array.Copy(inheritFrom, 1, tmp, 0, tmp.Length);
            inheritFrom = tmp;
        }
        
        TypeBuilder valueToBuild = CreateOrGetValueDcl(forSymbol, 
                                                       baseClass, inheritFrom,
                                                       false, false);
        
        // add implementation class attribute
        valueToBuild.SetCustomAttribute(new ImplClassAttribute(valueToBuild.FullName + "Impl").CreateAttributeBuilder());
        m_ilEmitHelper.AddSerializableAttribute(valueToBuild);
        
        // make sure, every value type has a default constructor
        ConstructorBuilder defConstr = 
            valueToBuild.DefineDefaultConstructor(MethodAttributes.Family);

        // generate elements
        BuildInfo buildInfo = 
            new ConcreteValTypeBuildInfo(enclosingScope.getChildScope(forSymbol.getSymbolName()),
                                         valueToBuild, forSymbol, defConstr);
        ArrayList /* FieldBuilder */ fields = new ArrayList();
        for (int i = 1; i < node.jjtGetNumChildren(); i++) { // for all value_element children
            ASTvalue_element elem = (ASTvalue_element)node.jjtGetChild(i);
            IList stateFields = (IList)
                elem.jjtAccept(this, buildInfo);
            if (stateFields != null) {
                // only state value elements return field builders
                fields.AddRange(stateFields);
            }
        }
        AddExplicitSerializationOrder(valueToBuild, fields);        

        // finally create the type
        Type resultType = m_typeManager.EndTypeDefinition(forSymbol);        
        
        // add to list of value types generated for informing the user of need for implementation class
        m_valueTypesDefined.Add(resultType);
        return null;
    }
    
    /**
     * @see parser.IDLParserVisitor#visit(ASTvalue_abs_decl, Object)
     */
    public Object visit(ASTvalue_abs_decl node, Object data) {
        CheckParameterForBuildInfo(data, node);
        // data contains the scope, this value type is declared in
        Scope enclosingScope = ((BuildInfo) data).GetBuildScope();
        // an IDL abstract value type
        
        Symbol forSymbol = enclosingScope.getSymbol(node.getIdent());
        // check if type is known from a previous run over a parse tree --> if so: skip
        if (m_typeManager.CheckSkip(forSymbol)) { 
            return null; 
        }

        Type[] interfaces = ParseValueInheritSpec(node, (BuildInfo) data);
        if ((interfaces.Length > 0) && (interfaces[0].IsClass)) { 
            throw new InvalidIdlException("invalid " + node.GetIdentification() + 
                                          ", can only inherit from abstract value types, but not from: " + 
                                          interfaces[0].FullName);
        }
        int bodyNodeIndex = 0;
        for (int i = 0; i < node.jjtGetNumChildren(); i++) {
            if (!((node.jjtGetChild(i) is ASTvalue_base_inheritance_spec) || (node.jjtGetChild(i) is ASTvalue_support_inheritance_spec))) {
                bodyNodeIndex = i;
                break;
            }
        }

        TypeBuilder valueToBuild = CreateOrGetValueDcl(forSymbol, null, interfaces,
                                                       true, false); 

        // generate elements
        BuildInfo buildInfo = new BuildInfo(enclosingScope.getChildScope(forSymbol.getSymbolName()), 
                                            valueToBuild, forSymbol);
        for (int i = bodyNodeIndex; i < node.jjtGetNumChildren(); i++) { // for all export children
            Node child = node.jjtGetChild(i);
            child.jjtAccept(this, buildInfo);    
        }

        // finally create the type
        m_typeManager.EndTypeDefinition(forSymbol);
        return null;
    }
    
    /**
     * @see parser.idlparservisitor#visit(ASTvalue_box_decl, Object)
     * @param data the current buildinfo
     */
    public Object visit(ASTvalue_box_decl node, Object data) {
        CheckParameterForBuildInfo(data, node);
        Scope enclosingScope = ((BuildInfo) data).GetBuildScope();
        Symbol forSymbol = enclosingScope.getSymbol(node.getIdent());
        // check if type is known from a previous run over a parse tree --> if so: skip
        if (m_typeManager.CheckSkip(forSymbol)) { 
            return null;
        }
        
        Debug.WriteLine("begin boxed value type: " + node.getIdent());
        // get the boxed type
        TypeContainer boxedType = (TypeContainer)node.jjtGetChild(0).jjtAccept(this, data);
        if (boxedType == null) {
            throw new InvalidIdlException(
                String.Format("boxed type " + 
                              ((SimpleNode)node.jjtGetChild(0)).GetIdentification() +
                              " not (yet) defined for boxed value type " +
                              node.GetIdentification()));
        }
        boxedType = ReplaceByCustomMappedIfNeeded(boxedType);        

        // do use fusioned type + attributes on fusioned type for boxed value;
        m_typeManager.StartBoxedValueTypeDefinition(forSymbol, boxedType.GetCompactClsType(),
                                                    boxedType.GetCompactTypeAttrs());

        m_typeManager.EndTypeDefinition(forSymbol);
        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTvalue_forward_decl, Object)
     * @param data the buildinfo of the container
     */
    public Object visit(ASTvalue_forward_decl node, Object data) {
        CheckParameterForBuildInfo(data, node);
        // is possible to do with reflection emit, because interface and class inheritance can be specified later on with setParent() and AddInterfaceImplementation()
        Scope enclosingScope = ((BuildInfo) data).GetBuildScope();
        Symbol forSymbol = enclosingScope.getSymbol(node.getIdent());
        // check if type is known from a previous run over a parse tree --> if so: skip
        if (m_typeManager.CheckSkip(forSymbol)) { 
            return null; 
        }
        
        // create only the type-builder, but don't call createType()
        if (!(m_typeManager.IsTypeDeclarded(forSymbol))) { // if the full type declaration already exists, ignore fwd decl
            // it's no problem to add later on interfaces this type should implement and the base class this type should inherit from with AddInterfaceImplementation / set parent
            // here: specify no inheritance, because not known at this point
            CreateOrGetValueDcl(forSymbol, null, Type.EmptyTypes, 
                                node.isAbstract(), true);
        }
        return null;
    }

    
    /** search in a value_header_node / abs_value_node for inheritance information and parse it
     * @param parentOfPossibleInhNode the node possibly containing value inheritance nodes
     */
    private Type[] ParseValueInheritSpec(Node parentOfPossibleInhNode, BuildInfo data) {
        Type[] result = new Type[0];
        if (parentOfPossibleInhNode.jjtGetNumChildren() > 0) {
            if (parentOfPossibleInhNode.jjtGetChild(0) is ASTvalue_base_inheritance_spec) {
                ASTvalue_base_inheritance_spec inheritSpec = (ASTvalue_base_inheritance_spec) parentOfPossibleInhNode.jjtGetChild(0);
                result = (Type[])inheritSpec.jjtAccept(this, data);
            } else if (parentOfPossibleInhNode.jjtGetChild(0) is ASTvalue_support_inheritance_spec){
                ASTvalue_support_inheritance_spec inheritSpec = (ASTvalue_support_inheritance_spec) parentOfPossibleInhNode.jjtGetChild(0);
                result = (Type[])inheritSpec.jjtAccept(this, data);    
            }
        }
        if ((parentOfPossibleInhNode.jjtGetNumChildren() > 1) && (parentOfPossibleInhNode.jjtGetChild(1) is ASTvalue_support_inheritance_spec)) {
            // append the support inheritance spec to the rest
            ASTvalue_support_inheritance_spec inheritSpec = (ASTvalue_support_inheritance_spec) parentOfPossibleInhNode.jjtGetChild(1);
            Type[] supportTypes = (Type[])inheritSpec.jjtAccept(this, data);
            Type[] resultCrt = new Type[result.Length + supportTypes.Length];
            Array.Copy(result, 0, resultCrt, 0, result.Length);
            Array.Copy(supportTypes, 0, resultCrt, result.Length, supportTypes.Length);
            result = resultCrt;
        }
        return result;
    }
    
    /**
     * @see parser.IDLParserVisitor#visit(ASTvalue_header, Object)
     * @param data the buildinfo of the container for this valuetype
     */
    public Object visit(ASTvalue_header node, Object data) {
        CheckParameterForBuildInfo(data, node);
        return ParseValueInheritSpec(node, (BuildInfo) data);
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTvalue_element, Object)
     * @param data a Buildinfo instance for the value-type containing this content
     */
    public Object visit(ASTvalue_element node, Object data) {
        return node.jjtGetChild(0).jjtAccept(this, data); // generate for an export, state or init_dcl member
    }

    #region constructor definition, at the moment not supported
    /**
     * @see parser.IDLParserVisitor#visit(ASTinit_decl, Object)
     */
    public Object visit(ASTinit_decl node, Object data) {
        CheckParameterForBuildInfo(data, node);
        ConcreteValTypeBuildInfo info = (ConcreteValTypeBuildInfo) data;
        TypeBuilder builder = info.GetContainterType();                
        
        // ConstructorInfo defConstr = builder.GetConstructor(Type.EmptyTypes);
                
        if (node.jjtGetNumChildren() > 0) {
            // non-default constructor
            ParameterSpec[] parameters =
                (ParameterSpec[])node.jjtGetChild(0).jjtAccept(this, info);
            
            ConstructorBuilder constr = 
                m_ilEmitHelper.AddConstructor(builder, parameters,
                                              MethodAttributes.Family);
            ILGenerator gen = constr.GetILGenerator();
            // call default constructor of class
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, info.DefaultConstructor);
            gen.Emit(OpCodes.Ret);
            
        } 
        
        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTinit_param_attribute, Object)
     */
    public Object visit(ASTinit_param_attribute node, Object data) {
        // always in
        return ParameterSpec.ParameterDirection.s_in;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTinit_param_decl, Object)
     */
    public Object visit(ASTinit_param_decl node, Object data) {
        // direction always in
        ParameterSpec.ParameterDirection direction = (ParameterSpec.ParameterDirection)
            node.jjtGetChild(0).jjtAccept(this, data);
        // determine name and type
        TypeContainer paramType = (TypeContainer)node.jjtGetChild(1).jjtAccept(this, data);
        if (paramType == null) {
            throw new InvalidIdlException(String.Format("init parameter type {0} not (yet) defined for {1}", 
                                                        ((SimpleNode)node.jjtGetChild(1)).GetIdentification(),
                                                        node.GetIdentification()));
        }
        paramType = ReplaceByCustomMappedIfNeeded(paramType);
        String paramName = IdlNaming.MapIdlNameToClsName(((ASTsimple_declarator)node.jjtGetChild(2)).getIdent());
        
        ParameterSpec result = new ParameterSpec(paramName, paramType, direction);
        return result;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTinit_param_delcs, Object)
     */
    public Object visit(ASTinit_param_delcs node, Object data) {
        // visit all init parameters
        ParameterSpec[] parameters = new ParameterSpec[node.jjtGetNumChildren()];
        for (int i = 0; i < node.jjtGetNumChildren(); i++) {
            parameters[i] = (ParameterSpec) node.jjtGetChild(i).jjtAccept(this, data);
        }
        return parameters;
    }
    #endregion

    /**
     * @see parser.IDLParserVisitor#visit(ASTstate_member, Object)
     * @param data the buildInfo for this value-type
     */
    public Object visit(ASTstate_member node, Object data) {        
        CheckParameterForBuildInfo(data, node);
        BuildInfo info = (BuildInfo) data;
        TypeBuilder builder = info.GetContainterType();
        ASTtype_spec typeSpecNode = (ASTtype_spec)node.jjtGetChild(0);
        TypeContainer fieldType = (TypeContainer)typeSpecNode.jjtAccept(this, info);
        fieldType = ReplaceByCustomMappedIfNeeded(fieldType);
        if (fieldType == null) {
            throw new InvalidIdlException(
                String.Format("field type {0} not (yet) defined for {1} in value type",
                              typeSpecNode.GetIdentification(), 
                              node.GetIdentification()));
        }
        IList /* FieldBuilder */ fields = new ArrayList();
        ASTdeclarators decl = (ASTdeclarators)node.jjtGetChild(1);
        for (int i = 0; i < decl.jjtGetNumChildren(); i++) {
            string declFieldName = 
                DetermineTypeAndNameForDeclarator((ASTdeclarator)decl.jjtGetChild(i), data,
                                                  ref fieldType);
            string idlFieldName;
            FieldAttributes fieldAttributes;
            if (node.isPrivate()) { // map to protected field
                String privateName = declFieldName;
                // compensate a problem in the java rmi compiler, which can produce illegal idl:
                // it produces idl-files with name clashes if a method getx() and a field x exists
                if (!privateName.StartsWith("m_")) { 
                    privateName = "m_" + privateName; 
                }
                idlFieldName = privateName;     
                fieldAttributes = FieldAttributes.Family;
            } else { // map to public field
                idlFieldName = declFieldName;                
                fieldAttributes = FieldAttributes.Public;
            }
            idlFieldName = IdlNaming.MapIdlNameToClsName(idlFieldName);
            FieldBuilder field = 
                m_ilEmitHelper.AddFieldWithCustomAttrs(builder, idlFieldName,
                                                       fieldType, fieldAttributes);
            fields.Add(field);
        }
        return fields;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTvalue_base_inheritance_spec, Object)
     * @param data the buildinfo of the container of the type, having this inheritance relationship
     * @return an array of System.Type containing all direct supertypes
     */
    public Object visit(ASTvalue_base_inheritance_spec node, Object data) {
        CheckParameterForBuildInfo(data, node);
        Type[] result = ParseInheritanceRelation(node, (BuildInfo)data);
        for (int i = 0; i < result.Length; i++) {
            if ((i > 0) && (result[i].IsClass)) {
                throw new InvalidIdlException("invalid supertype: " + result[i].FullName + " for type: " + 
                                              node.GetEmbedderDesc() +
                                              " for value types, only one concrete value type parent is possible at the first position in the inheritance spec");
            }
            AttributeExtCollection attrs = AttributeExtCollection.ConvertToAttributeCollection(result[i].GetCustomAttributes(typeof(InterfaceTypeAttribute), true));
            if (attrs.IsInCollection(typeof(InterfaceTypeAttribute))) {
                InterfaceTypeAttribute ifAttr = (InterfaceTypeAttribute)attrs.GetAttributeForType(typeof(InterfaceTypeAttribute));
                if (!(ifAttr.IdlType.Equals(IdlTypeInterface.AbstractValueType))) {
                    throw new InvalidIdlException("invalid supertype: " + result[i].FullName + " for type: " + 
                                               node.GetEmbedderDesc() +
                                               " only abstract value types are allowed in value inheritance clause and no interfaces");
                }
            }
        }        
        return result;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTvalue_support_inheritance_spec, Object)
     * @param data the buildinfo of the container of the type, having this inheritance relationship
     * @return an array of System.Type containing all interfaces, this type supports directly
     */
    public Object visit(ASTvalue_support_inheritance_spec node, Object data) {
        CheckParameterForBuildInfo(data, node);
        Type[] result = ParseInheritanceRelation(node, (BuildInfo)data);
        for (int i = 0; i < result.Length; i++) {
            if (result[i].IsClass) {
                throw new InvalidIdlException("invalid supertype: " + result[i].FullName + " for type: " +
                                              ((BuildInfo)data).GetContainterType().FullName +
                                              " only abstract/concrete interfaces are allowed in support clause");
            }
            AttributeExtCollection attrs = AttributeExtCollection.ConvertToAttributeCollection(result[i].GetCustomAttributes(typeof(InterfaceTypeAttribute), true));
            if (attrs.IsInCollection(typeof(InterfaceTypeAttribute))) {
                InterfaceTypeAttribute ifAttr = (InterfaceTypeAttribute)attrs.GetAttributeForType(typeof(InterfaceTypeAttribute));
                if (ifAttr.IdlType.Equals(IdlTypeInterface.AbstractValueType)) {
                    throw new InvalidIdlException("invalid supertype: " + result[i].FullName + " for type: " +
                                                  ((BuildInfo)data).GetContainterType().FullName +
                                                  " only abstract/concrete interfaces are allowed in support clause and no abstract value type");
                }
            }
        }
        return result;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTvalue_name, Object)
     * @param data the buildinfo of container of the valuetype using this value_name in inheritance spec
     * @return the symbol for the scoped name represented by value_name node
     */
    public Object visit(ASTvalue_name node, Object data) {
        Symbol result = (Symbol)node.jjtGetChild(0).jjtAccept(this, data);
        return result;
    }

    #region const-dcl

    /// <summary>
    /// returns true, if the Type type can be used as type for a constant field; otherwise false.
    /// </summary>
    private bool CanSetConstantValue(Type type) {
        // ok for the following types:
        // Boolean, SByte, Int16, Int32, Int64, Byte, UInt16, UInt32, UInt64,
        // Single, Double, DateTime, Char, String und Enum        
        return (type.Equals(ReflectionHelper.BooleanType) ||
                type.Equals(ReflectionHelper.SByteType) ||
                type.Equals(ReflectionHelper.Int16Type) ||
                type.Equals(ReflectionHelper.Int32Type) ||
                type.Equals(ReflectionHelper.Int64Type) ||
                type.Equals(ReflectionHelper.ByteType) ||
                type.Equals(ReflectionHelper.UInt16Type) ||
                type.Equals(ReflectionHelper.UInt32Type) ||
                type.Equals(ReflectionHelper.UInt64Type) ||
                type.Equals(ReflectionHelper.SingleType) ||
                type.Equals(ReflectionHelper.DoubleType) ||
                type.Equals(ReflectionHelper.DateTimeType) ||
                type.Equals(ReflectionHelper.CharType) ||
                type.Equals(ReflectionHelper.StringType) ||
                type.IsEnum);
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTconst_dcl, Object)
     * @param data expects a BuildInfo instance
     * The following two cases are possible here:
     * constant directly declared in module: ((BuildInfo)data).GetContainerType() is null 
     * constant declared in an interface or value type: ((BuildInfo)data).GetContainerType() is not null
     * 
     * remark: fields in interfaces are not CLS-compliant!
     */
    public Object visit(ASTconst_dcl node, Object data) {
        CheckParameterForBuildInfo(data, node);
        BuildInfo buildInfo = (BuildInfo)data;
        Scope enclosingScope = buildInfo.GetBuildScope();
        
        SymbolValue constSymbol = (SymbolValue)enclosingScope.getSymbol(node.getIdent());
        // check if type is known from a previous run over a parse tree --> if so: skip
        // not needed to check if const is nested inside a type, because parent type should already be skipped 
        // --> code generation for all nested types skipped too
        if (m_typeManager.CheckSkip(constSymbol)) {
            return null; 
        }
        
        TypeContainer constType = (TypeContainer)node.jjtGetChild(0).jjtAccept(this, data);
        Literal val = (Literal)node.jjtGetChild(1).jjtAccept(this, data);
        if (val == null) {
            throw new InvalidIdlException("constant can't be evaluated: " + constSymbol.getSymbolName());
        }
        // set the value of the constant:
        constSymbol.SetValueAsLiteral(val);
                
        TypeBuilder constContainer = m_typeManager.StartTypeDefinition(constSymbol,
                                                                       TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.Public, 
                                                                       typeof(System.Object), new System.Type[] { typeof(IIdlEntity) }, false);
        string constFieldName = "ConstVal";
        FieldBuilder constField;
        if (CanSetConstantValue(constType.GetSeparatedClsType())) {
            // possible as constant field
            constField = m_ilEmitHelper.AddFieldWithCustomAttrs(constContainer, constFieldName, constType,
                                                                FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.Public);
            constField.SetConstant(val.GetValueToAssign(constType.GetSeparatedClsType(),
                                                        constType.GetAssignableFromType()));
        } else {
            // not possible as constant -> use a readonly static field instead of a constant
            constField = m_ilEmitHelper.AddFieldWithCustomAttrs(constContainer, constFieldName, constType,
                                                                FieldAttributes.Static | FieldAttributes.InitOnly | FieldAttributes.Public);
            // add static initalizer to assign value
            ConstructorBuilder staticInit = constContainer.DefineConstructor(MethodAttributes.Private | MethodAttributes.Static,
                                                                            CallingConventions.Standard, Type.EmptyTypes);
            ILGenerator constrIl = staticInit.GetILGenerator();
            val.EmitLoadValue(constrIl, constType.GetSeparatedClsType(), constType.GetAssignableFromType());
            constrIl.Emit(OpCodes.Stsfld, constField);
            constrIl.Emit(OpCodes.Ret);
        }

        // add private default constructor
        constContainer.DefineDefaultConstructor(MethodAttributes.Private);

        // create the type
        m_typeManager.EndTypeDefinition(constSymbol);
        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTconst_type, Object)
     */
    public Object visit(ASTconst_type node, Object data) {
        CheckParameterForBuildInfo(data, node);
        BuildInfo buildInfo = (BuildInfo)data;
        SimpleNode child = (SimpleNode)node.jjtGetChild(0);
        return ResovleTypeSpec(child, buildInfo);
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTconst_exp, Object)
     */
    public Object visit(ASTconst_exp node, Object data) {
        // evaluate or_expr
        Object result = node.jjtGetChild(0).jjtAccept(this, data);
        return result;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTor_expr, Object)
     */
    public Object visit(ASTor_expr node, Object data) {
        Literal result = 
            (Literal)node.jjtGetChild(0).jjtAccept(this, data);
        for(int i=1; i < node.jjtGetNumChildren(); i++) {
            // evaluate the xor-expr and or it to the current result
            result = result.Or((Literal)node.jjtGetChild(i).jjtAccept(this, data));
        }
        return result;                
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTxor_expr, Object)
     */
    public Object visit(ASTxor_expr node, Object data) {
        Literal result = 
            (Literal)node.jjtGetChild(0).jjtAccept(this, data);
        for(int i=1; i < node.jjtGetNumChildren(); i++) {
            // evaluate the and-expr and xor it to the current result
            result = result.Xor((Literal)node.jjtGetChild(i).jjtAccept(this, data));
        }
        return result;        
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTand_expr, Object)
     */
    public Object visit(ASTand_expr node, Object data) {
        Literal result = 
            (Literal)node.jjtGetChild(0).jjtAccept(this, data);
        for(int i=1; i < node.jjtGetNumChildren(); i++) {
            // evaluate the shift-expr and and it to the current result
            result = result.And((Literal)node.jjtGetChild(i).jjtAccept(this, data));
        }
        return result;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTshift_expr, Object)
     */
    public Object visit(ASTshift_expr node, Object data) {        
        Literal result = 
            (Literal)node.jjtGetChild(0).jjtAccept(this, data);
        for(int i=1; i < node.jjtGetNumChildren(); i++) {
            // evaluate the add-expr and lshift/rshift the current result with it
            switch (node.GetShiftOperation(i-1)) {
                case ShiftOps.Right:
                    result = result.ShiftRightBy((Literal)node.jjtGetChild(i).jjtAccept(this, data));
                    break;
                case ShiftOps.Left:
                    result = result.ShiftLeftBy((Literal)node.jjtGetChild(i).jjtAccept(this, data));
                    break;
            }
        }
        return result;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTadd_expr, Object)
     */
    public Object visit(ASTadd_expr node, Object data) {                
        Literal result = 
            (Literal)node.jjtGetChild(0).jjtAccept(this, data);
        for(int i=1; i < node.jjtGetNumChildren(); i++) {
            // evaluate the mult-expr and add/sub it to the current result
            switch (node.GetAddOperation(i-1)) {
                case AddOps.Plus:
                    result = result.Add((Literal)node.jjtGetChild(i).jjtAccept(this, data));
                    break;
                case AddOps.Minus:
                    result = result.Sub((Literal)node.jjtGetChild(i).jjtAccept(this, data));
                    break;
            }
        }
        return result;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTmult_expr, Object)
     */
    public Object visit(ASTmult_expr node, Object data) {
        Literal result = 
            (Literal)node.jjtGetChild(0).jjtAccept(this, data);
        for(int i=1; i < node.jjtGetNumChildren(); i++) {
            // evaluate the unary-expr and mult/div/mod it to the current result
            switch (node.GetMultOperation(i-1)) {
                case MultOps.Mult:
                    result = result.MultBy((Literal)node.jjtGetChild(i).jjtAccept(this, data));
                    break;
                case MultOps.Div:
                    result = result.DivBy((Literal)node.jjtGetChild(i).jjtAccept(this, data));
                    break;
                case MultOps.Mod:
                    result = result.ModBy((Literal)node.jjtGetChild(i).jjtAccept(this, data));
                    break;                    
            }
        }
        return result;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTunary_expr, Object)
     */
    public Object visit(ASTunary_expr node, Object data) {          
        // evaluate the primary-expr
        Literal result = (Literal)node.jjtGetChild(0).jjtAccept(this, data);
        switch (node.GetUnaryOperation()) {
            case UnaryOps.UnaryNegate:
                result.Negate();
                break;
            case UnaryOps.UnaryMinus:
                result.InvertSign();
                break;
            default:
                // for UnaryOps.Plus and UnaryOps.None: nothing to do
                break;
        }        
        return result;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTprimary_expr, Object)
     */
    public Object visit(ASTprimary_expr node, Object data) {
        // possible cases (one child):
        // scoped_name
        // literal
        // const_exp
        Object result = node.jjtGetChild(0).jjtAccept(this, data);
        if (result is SymbolValue) {
            // a scoped name, which points to a symbol containing a value
            return ((SymbolValue)result).GetValueAsLiteral();
        } else if (result is Symbol) {
            // A Symbol, but no value symbol, TODO: check if this is correct behaviour
            throw new InvalidIdlException("no valid primary expression: " + result);
        } else {
            // a literal: a Literal Value
            return result;
        }
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTliteral, Object)
     */
    public Object visit(ASTliteral node, Object data) {
        return node.getLitVal();
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTpositive_int_const, Object)
     */
    public Object visit(ASTpositive_int_const node, Object data) {
        // used for array, bounded seq, ...;
        // evaluate the const-exp:
        Literal result = (Literal)node.jjtGetChild(0).jjtAccept(this, data);
        if (!(result is IntegerLiteral)) {
            throw new InvalidIdlException("invalid positive int const found: " + result.GetValue());
        }
        long intConst = result.GetIntValue();
        if (intConst < 0) {
            throw new InvalidIdlException("negative int found where positive int constant was required: " + 
                result.GetValue());
        }

        return intConst;
    }
    #endregion

    /**
     * @see parser.IDLParserVisitor#visit(ASTtype_dcl, Object)
     * @param data the current buildinfo
     */
    public Object visit(ASTtype_dcl node, Object data) {
        Node childNode = node.jjtGetChild(0); // let the childnode declare the type
        childNode.jjtAccept(this, data);
        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTtype_declarator, Object)
     * @param data expected is an instance of BuildInfo
     */
    public Object visit(ASTtype_declarator node, Object data) {
        CheckParameterForBuildInfo(data, node);
        Scope currentScope = ((BuildInfo) data).GetBuildScope();
        TypeContainer typeUsedInDefine = (TypeContainer) node.jjtGetChild(0).jjtAccept(this, data);
        Node declarators = node.jjtGetChild(1);    
        for (int i = 0; i < declarators.jjtGetNumChildren(); i++) {
            ASTdeclarator decl = (ASTdeclarator) declarators.jjtGetChild(i);
            if (decl.jjtGetChild(0) is ASTcomplex_declarator) {
                // check for custom mapping, because will become element type of array
                typeUsedInDefine = ReplaceByCustomMappedIfNeeded(typeUsedInDefine);
            }
            string ident = 
                DetermineTypeAndNameForDeclarator(decl, data, 
                                                  ref typeUsedInDefine);
            Symbol typedefSymbol = currentScope.getSymbol(ident);
            // inform the type-manager of this typedef
            Debug.WriteLine("typedef defined here, type: " + typeUsedInDefine.GetCompactClsType() +
                            ", symbol: " + typedefSymbol);
            m_typeManager.RegisterTypeDef(typeUsedInDefine, typedefSymbol);
        }    
        return null;
    }

    
    /** resovle a param_type_spec or a simple_type_spec or other type specs which may return a symbol or a typecontainer
     *  @param node the child node of the type_spec node containing the spec data
     *  @param currentInfo the buildinfo for the scope, this type is specified in
     *  @return a TypeContainer for the represented type
     */
    private TypeContainer ResovleTypeSpec(SimpleNode node, BuildInfo currentInfo) {    
        Object result = node.jjtAccept(this, currentInfo);
        TypeContainer resultingType = null;
        if (result is Symbol) { // case <scoped_name>
            // get type for symbol
            resultingType = m_typeManager.GetKnownType((Symbol)result);
        } else { // other cases
            resultingType = (TypeContainer) result;
        }
        return resultingType;
    }
    
    /**
     * @see parser.IDLParserVisitor#visit(ASTtype_spec, Object)
     * @param data expected is an instance of BuildInfo
     * if ((BuildInfo)data).getContainerType() is null, than an independant type-decl is created, else
     * the type delcaration is added to the Type in creation
     */
    public Object visit(ASTtype_spec node, Object data) {
        Node child = node.jjtGetChild(0);
        return child.jjtAccept(this, data); // handle <simple_type_spec> or <constr_type_spec>
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTsimple_type_spec, Object)
     * @param data the buildinfo instance
     * @return a TypeContainer containing the type represented by this node
     */
    public Object visit(ASTsimple_type_spec node, Object data) {
        CheckParameterForBuildInfo(data, node);
        SimpleNode child = (SimpleNode)node.jjtGetChild(0);
        return ResovleTypeSpec(child, (BuildInfo) data);
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTbase_type_spec, Object)
     * @param data the buildinfo for the scope this spec is used in
     * @return a TypeContainer for the base type
     */
    public Object visit(ASTbase_type_spec node, Object data) {
        // the child-node does the work
        return node.jjtGetChild(0).jjtAccept(this, data);
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTtemplate_type_spec, Object)
     * @param data the buildinfo for the current scope
     * @return a typecontainer for the type respresented by this node
     */
    public Object visit(ASTtemplate_type_spec node, Object data) {
        Node child = node.jjtGetChild(0);
        return child.jjtAccept(this, data);
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTconstr_type_spec, Object)
     * @param data a buildinfo instance
     * @return the TypeContainer for the type represented by this node
     */
    public Object visit(ASTconstr_type_spec node, Object data) {
        Node child = node.jjtGetChild(0); // <struct_type>, <union_type>, <enum_type>
        return child.jjtAccept(this, data);
    }

    private TypeContainer GetTypeContainerForMultiDimArray(TypeContainer elemType, int[] dimensions) {
        string clsArrayRep = "[";
        for (int i = 1; i < dimensions.Length; i++) {
            clsArrayRep = clsArrayRep + ",";
        }
        clsArrayRep = clsArrayRep + "]";
        // create CLS array type with the help of GetType(), otherwise not possible
        Type arrayType;
        // because not fully defined types are possible, use module and not assembly to get type from
        Module declModule = elemType.GetCompactClsType().Module;
        arrayType = declModule.GetType(elemType.GetCompactClsType().FullName + clsArrayRep); // not nice, better solution ?        
        Debug.WriteLine("created array type: " + arrayType);

        // determine the needed attributes: IdlArray is required by the array itself (+optional dimension attrs); 
        // combine with the attribute from the element type
        // possible are: IdlArray (for array of array), IdlSequence (for array of sequence), ObjectIdlType,
        // WideChar, StringValue
        // invariant: boxed value attribute is not among them, because elem type 
        // is in the compact form        
        AttributeExtCollection elemAttributes = elemType.GetCompactTypeAttrInstances();
        long arrayAttrOrderNr = IdlArrayAttribute.DetermineArrayAttributeOrderNr(elemAttributes);
        
        IdlArrayAttribute arrayAttr = new IdlArrayAttribute(arrayAttrOrderNr, dimensions[0]); // at least one dimension available, because of grammar
        AttributeExtCollection arrayAttributes = 
            new AttributeExtCollection(elemAttributes);
        arrayAttributes = arrayAttributes.MergeAttribute(arrayAttr);        
        for (int i = 1; i < dimensions.Length; i++) {
            IdlArrayDimensionAttribute dimAttr = new IdlArrayDimensionAttribute(arrayAttrOrderNr, i, dimensions[i]);
            arrayAttributes = arrayAttributes.MergeAttribute(dimAttr);
        }
        
        return new TypeContainer(arrayType,
                                 arrayAttributes);        
    }

    /// <summary>
    /// returns the name of the declared identity. For array declarators, creates the array type out of the typeSpecType
    /// </summary>
    private string DetermineTypeAndNameForDeclarator(ASTdeclarator node, object visitorData, ref TypeContainer typeSpecType) {
        Node child = node.jjtGetChild(0); // child of declarator
        if (child is ASTsimple_declarator) {
            // a simple delcarator
            ASTsimple_declarator simpleDecl = (ASTsimple_declarator) child;
            return simpleDecl.getIdent();
        } else if (child is ASTcomplex_declarator) {
            ASTarray_declarator arrayDecl = (ASTarray_declarator) child.jjtGetChild(0);
            int[] dimensions = (int[])arrayDecl.jjtAccept(this, visitorData);
            typeSpecType = GetTypeContainerForMultiDimArray(typeSpecType, dimensions);
            return arrayDecl.getIdent();
        } else {
            throw new NotSupportedException("unexpected delcarator: " + child.GetType()); // should never occur
        }
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTdeclarators, Object)
     * @param data unused
     * @return an array of all declared elements here
     */
    public Object visit(ASTdeclarators node, Object data) {
        return null; // nothing to do, used by parent node
    }

    /**
     * does nothing, node is used by parent
     * @see parser.IDLParserVisitor#visit(ASTdeclarator, Object)
     */
    public Object visit(ASTdeclarator node, Object data) {
        return null; //nothing to do, used by parent node
    }

    /**
     * does nothing, node is used by parent
     * @see parser.IDLParserVisitor#visit(ASTsimple_declarator, Object)
     */
    public Object visit(ASTsimple_declarator node, Object data) {
        return null; // nothing to do, used by parent node
    }

    /**
     * does nothing, node is used by parent
     * @see parser.IDLParserVisitor#visit(ASTcomplex_declarator, Object)
     */
    public Object visit(ASTcomplex_declarator node, Object data) {
        return null; // nothing to do, used by parent node
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTfloating_pt_type, Object)
     * @param data unused
     * @return a TypeContainer for the floating pt type represented through this node
     */
    public Object visit(ASTfloating_pt_type node, Object data) {
        return node.jjtGetChild(0).jjtAccept(this, data);   
    }
    
    /**
     * @see parser.IDLParserVisitor#visit(ASTfloating_pt_type_float, Object)
     * @param data unused
     * @return a TypeContainer for the float type
     */
    public Object visit(ASTfloating_pt_type_float node, Object data) {
        return new TypeContainer(typeof(System.Single));
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTfloating_pt_type_double, Object)
     * @param data unused
     * @return a TypeContainer for the double type
     */
    public Object visit(ASTfloating_pt_type_double node, Object data) {
        return new TypeContainer(typeof(System.Double));
    }

    /**
     * unsupported
     * @see parser.IDLParserVisitor#visit(ASTfloating_pt_type_longdouble, Object)
     */
    public Object visit(ASTfloating_pt_type_longdouble node, Object data) {
        throw new NotSupportedException("long double not supported by this compiler");
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTinteger_type, Object)
     * @param data unused
     * @return a TypeContainer for the type represented by the node
     */
    public Object visit(ASTinteger_type node, Object data) {
        // integer_type ::= <signed_int> | <unsigned_int>
        return node.jjtGetChild(0).jjtAccept(this, data);
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTsigned_int, Object)
     * @param data unused
     * @return a TypeContainer for the type represented by the node
     */
    public Object visit(ASTsigned_int node, Object data) {
        // <signed_int> ::= <singed_short_int> || <signed_long_int> || <signed_longlong_int>
        return node.jjtGetChild(0).jjtAccept(this, data);
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTsigned_short_int, Object)
     * @param data unused
     * @return a TypeContainer for the short type
     */
    public Object visit(ASTsigned_short_int node, Object data) {
        return new TypeContainer(typeof(System.Int16));
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTsigned_long_int, Object)
     * @param data unused
     * @return a TypeContainer for the long type
     */
    public Object visit(ASTsigned_long_int node, Object data) {
        return new TypeContainer(typeof(System.Int32));
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTsigned_longlong_int, Object)
     * @param data unused
     * @return a TypeContainer for the long long type
     */
    public Object visit(ASTsigned_longlong_int node, Object data) {
        return new TypeContainer(typeof(System.Int64));
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTunsigned_int, Object)
     * @param data unused
     * @return a TypeContainer for the type represented by this node
     */
    public Object visit(ASTunsigned_int node, Object data) {
        return node.jjtGetChild(0).jjtAccept(this, data);
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTunsigned_short_int, Object)
     * @param data unused
     * @return a TypeContainer for the short type
     */
    public Object visit(ASTunsigned_short_int node, Object data) {
        return new TypeContainer(typeof(System.Int16), typeof(System.UInt16));
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTunsigned_long_int, Object)
     * @param data unused
     * @return a TypeContainer for the long type
     */
    public Object visit(ASTunsigned_long_int node, Object data) {
        return new TypeContainer(typeof(System.Int32), typeof(System.UInt32));
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTunsigned_longlong_int, Object)
     * @param data unused
     * @return a TypeContainer for the long long type
     */
    public Object visit(ASTunsigned_longlong_int node, Object data) {
        return new TypeContainer(typeof(System.Int64), typeof(System.UInt64));
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTchar_type, Object)
     * @param data unused
     * @return a TypeContainer for the char type
     */
    public Object visit(ASTchar_type node, Object data) {
        AttributeExtCollection attrs = new AttributeExtCollection(
            new Attribute[] { new WideCharAttribute(false) });
        TypeContainer containter = new TypeContainer(typeof(System.Char), attrs);
        return containter;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTwide_char_type, Object)
     * @param data unused
     * @return a type type container for the wchar type
     */
    public Object visit(ASTwide_char_type node, Object data) {
        AttributeExtCollection attrs = new AttributeExtCollection(
            new Attribute[] { new WideCharAttribute(true) });
        TypeContainer containter = new TypeContainer(typeof(System.Char), attrs);
        return containter;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTboolean_type, Object)
     * @param data unused
     * @return a TypeContainer for the boolean type
     */
    public Object visit(ASTboolean_type node, Object data) {
        TypeContainer container = new TypeContainer(typeof(System.Boolean));
        return container;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASToctet_type, Object)
     * @param data unused
     * @return a TypeContainer for the octet type
     */
    public Object visit(ASToctet_type node, Object data) {
        TypeContainer container = new TypeContainer(typeof(System.Byte));
        return container;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTany_type, Object)
     * @param data unused
     * @return a TypeContainer for the any type
     */
    public Object visit(ASTany_type node, Object data) {
        if (!MapAnyToAnyContainer) {
            AttributeExtCollection attrs = new AttributeExtCollection(
                new Attribute[] { new ObjectIdlTypeAttribute(IdlTypeObject.Any) });
            TypeContainer container = new TypeContainer(typeof(System.Object), attrs);
            return container;
        } else {
            TypeContainer container = new TypeContainer(typeof(omg.org.CORBA.Any));
            return container;
        }
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTobject_type, Object)
     * @param data unused
     * @return a TypeContainer for the Object type
     */
    public Object visit(ASTobject_type node, Object data) {
        TypeContainer container = new TypeContainer(typeof(System.MarshalByRefObject));
        return container;
    }
    
    private void AddExplicitSerializationOrder(TypeBuilder forType,
                                               IList /* FieldBuilder */ members) {
        forType.SetCustomAttribute(
            new ExplicitSerializationOrdered().CreateAttributeBuilder());
        for (int i = 0; i < members.Count; i++) {
            ((FieldBuilder)members[i]).SetCustomAttribute(
                new ExplicitSerializationOrderNr(i).CreateAttributeBuilder());
        }
    }
     
    private void AddStructConstructor(TypeBuilder structToCreate, IList /* members */ members) {
        ParameterSpec[] constrParams = new ParameterSpec[members.Count];
        for (int i = 0; i < members.Count; i++) {
            FieldBuilder member = (FieldBuilder)members[i];            
            constrParams[i] = new ParameterSpec(member.Name, member.FieldType);
        }
        ConstructorBuilder constrBuilder =
            m_ilEmitHelper.AddConstructor(structToCreate, constrParams,
                                          MethodAttributes.Public);
        ILGenerator body = constrBuilder.GetILGenerator();
        // don't need to call parent constructor for a struct; only assign the fields
        for (int i = 0; i < members.Count; i++) {
            FieldBuilder member = (FieldBuilder)members[i];
            body.Emit(OpCodes.Ldarg_0); // this
            body.Emit(OpCodes.Ldarg, i+1); // param value
            body.Emit(OpCodes.Stfld, member);
        }
        body.Emit(OpCodes.Ret);        
    }
    
    /**
     * @see parser.IDLParserVisitor#visit(ASTstruct_type, Object)
      * @param data expected is an instance of BuildInfo
     * if ((BuildInfo)data).getContainerType() is null, than an independant type-decl is created, else
     * the type delcaration is added to the Type in creation
     * @return the TypeContainer for the constructed type
     */
    public Object visit(ASTstruct_type node, Object data) {
        CheckParameterForBuildInfo(data, node);
        BuildInfo buildInfo = (BuildInfo) data;
        Symbol forSymbol = buildInfo.GetBuildScope().getSymbol(node.getIdent());
        // check if type is known from a previous run over a parse tree --> if so: skip
        // not needed to check if struct is a nested types, because parent type should already be skipped 
        // --> code generation for all nested types skipped too
        if (m_typeManager.CheckSkip(forSymbol)) { 
            return m_typeManager.GetKnownType(forSymbol);
        }
        
        // layout-sequential causes problem, if member of array type is not fully defined (TypeLoadException) -> use autolayout instead
        TypeAttributes typeAttrs = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Serializable | TypeAttributes.BeforeFieldInit | 
                                   /* TypeAttributes.SequentialLayout | */ TypeAttributes.Sealed;
        
        TypeBuilder structToCreate = m_typeManager.StartTypeDefinition(forSymbol,
                                                                       typeAttrs,
                                                                       typeof(System.ValueType), new System.Type[] { typeof(IIdlEntity) }, false);
        BuildInfo thisTypeInfo = new BuildInfo(buildInfo.GetBuildScope().getChildScope(forSymbol.getSymbolName()), 
                                               structToCreate,
                                               forSymbol);

        // add fileds and a constructor, which assigns the fields
        IList members = (IList)node.jjtGetChild(0).jjtAccept(this, thisTypeInfo); // accept the member list
        AddStructConstructor(structToCreate, members);        
        AddExplicitSerializationOrder(structToCreate, members);

        // add type specific attributes
        structToCreate.SetCustomAttribute(new IdlStructAttribute().CreateAttributeBuilder());
        m_ilEmitHelper.AddSerializableAttribute(structToCreate);
        
        // create the type
        Type resultType = m_typeManager.EndTypeDefinition(forSymbol);
        return new TypeContainer(resultType);
    }

    
    /// <summary>
    /// Adds ASTmember to the type in construction. MemberParent is the node containing ASTmember nodes.
    /// </summary>
    private IList /* FieldBuilder */ GenerateMemberList(SimpleNode memberParent, Object data) {
        ArrayList result = new ArrayList();
        for (int i = 0; i < memberParent.jjtGetNumChildren(); i++) {
            IList infos = 
                (IList)memberParent.jjtGetChild(i).jjtAccept(this, data);
            result.AddRange(infos);
        }        
        return result;
    }
     
    /**
     * @see parser.IDLParserVisitor#visit(ASTmember_list, Object)
     * @param data an instance of buildinfo for the type, which should contain this members
     */
    public Object visit(ASTmember_list node, Object data) {
        IList /* FieldBuilder */ members = GenerateMemberList(node, data);
        return members;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTmember, Object)
     */
    public Object visit(ASTmember node, Object data) {
        CheckParameterForBuildInfo(data, node);
        BuildInfo info = (BuildInfo) data;
        TypeBuilder builder = info.GetContainterType();
        ASTtype_spec typeSpecNode = (ASTtype_spec)node.jjtGetChild(0);
        TypeContainer fieldType = (TypeContainer)typeSpecNode.jjtAccept(this, info);
        if (fieldType == null) {
            throw new InvalidIdlException(
                String.Format("field type {0} not (yet) defined for struct-{1}",
                              typeSpecNode.GetIdentification(), 
                              node.GetIdentification()));
        }
        fieldType = ReplaceByCustomMappedIfNeeded(fieldType);
        ASTdeclarators decl = (ASTdeclarators)node.jjtGetChild(1);
        ArrayList generatedMembers = new ArrayList();
        for (int i = 0; i < decl.jjtGetNumChildren(); i++) {
            string fieldName = IdlNaming.MapIdlNameToClsName(
                                   DetermineTypeAndNameForDeclarator((ASTdeclarator)decl.jjtGetChild(i), data,
                                                                     ref fieldType));
            FieldBuilder generatedMember =
                m_ilEmitHelper.AddFieldWithCustomAttrs(builder, fieldName, fieldType,
                                                       FieldAttributes.Public);
            generatedMembers.Add(generatedMember);            
        }
        return generatedMembers;
    }

    private void CheckDiscrValAssignableToDiscrType(Literal discrVal, TypeContainer discrType) {
        Type clsDiscrType = discrType.GetCompactClsType();
        if (!(clsDiscrType.IsEnum ||
              clsDiscrType.Equals(typeof(System.Int16)) ||
              clsDiscrType.Equals(typeof(System.Int32)) ||
              clsDiscrType.Equals(typeof(System.Int64)) ||
              clsDiscrType.Equals(typeof(System.Char)) ||
              clsDiscrType.Equals(typeof(System.Boolean)))) {
            throw new InternalCompilerException("precond violation: discr type");
        }
        if (!discrVal.IsAssignableTo(discrType)) {
            throw new InvalidIdlException(
                    String.Format("discr val {0} not assignable to type: {1}",
                                  discrVal, clsDiscrType));    
        }
    }

    /// <summary>helper methods to collect discriminator values for casex node; checks if const-type is ok</summary>
    private object[] CollectDiscriminatorValuesForCase(ASTcasex node, TypeContainer discrType,
                                                       BuildInfo unionInfo) {
        object[] result = new object[node.jjtGetNumChildren() - 1];
        for (int i = 0; i < node.jjtGetNumChildren() - 1; i++) {
            if (!((ASTcase_label)node.jjtGetChild(i)).isDefault()) {
                Literal litVal = ((Literal)node.jjtGetChild(i).jjtAccept(this, unionInfo));
                if (litVal == null) {
                    throw new InvalidIdlException(
                        String.Format("invalid {0}, discrimitator value for case not retrievable",
                                      node.GetIdentification()));
                }                
                // check if val ok ...
                CheckDiscrValAssignableToDiscrType(litVal, discrType);
                result[i] = litVal.GetValue();
            } else {
                // default case
                result[i] = UnionGenerationHelper.DefaultCaseDiscriminator;
            }
        }
        return result;
    }

    /// <summary>
    /// collects all explicitely used discriminator values in switch cases.
    /// </summary>
    private ArrayList ExtractCoveredDiscriminatorRange(ASTswitch_body node, TypeContainer discrType,
                                                       BuildInfo unionInfo) {
        ArrayList result = new ArrayList();
        for (int i = 0; i < node.jjtGetNumChildren(); i++) {
            ASTcasex caseNode = (ASTcasex)node.jjtGetChild(i);
            object[] discrValsForCase = CollectDiscriminatorValuesForCase(caseNode, discrType, unionInfo);
            foreach (object discrVal in discrValsForCase) {
                if (discrVal.Equals(UnionGenerationHelper.DefaultCaseDiscriminator)) {
                    continue; // do not add default case here
                }
                if (result.Contains(discrVal)) {
                    throw new InvalidIdlException(
                        String.Format("discriminator value {0} used more than once in {1}",
                                      discrVal, node.GetIdentification()));
                }
                result.Add(discrVal);
            }
        }
        return result;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTunion_type, Object)
     */
    public Object visit(ASTunion_type node, Object data) {
        // generate the struct for this union
        CheckParameterForBuildInfo(data, node);
        BuildInfo buildInfo = (BuildInfo) data;
        Symbol forSymbol = buildInfo.GetBuildScope().getSymbol(node.getIdent());
        // check if type is known from a previous run over a parse tree --> if so: skip
        // not needed to check if struct is a nested types, because parent type should already be skipped 
        // --> code generation for all nested types skipped too
        if (m_typeManager.CheckSkip(forSymbol)) { 
            return m_typeManager.GetKnownType(forSymbol);
        }
   
        // create Helper for union generation
        String fullyQualName = buildInfo.GetBuildScope().GetFullyQualifiedNameForSymbol(forSymbol.getSymbolName());
        UnionGenerationHelper genHelper = 
            m_typeManager.StartUnionTypeDefinition(forSymbol, fullyQualName);
        
        UnionBuildInfo thisInfo = new UnionBuildInfo(buildInfo.GetBuildScope().getChildScope(forSymbol.getSymbolName()), genHelper,
                                                               forSymbol);        

        Node switchBody = node.jjtGetChild(1);
        TypeContainer discrType = (TypeContainer)node.jjtGetChild(0).jjtAccept(this, thisInfo);
        if (discrType == null) {
            throw new InvalidIdlException(
                String.Format("dicriminator type {0} not (yet) defined for union {1}",
                              ((SimpleNode)node.jjtGetChild(0)).GetIdentification(),
                              node.GetIdentification()));
        }
        discrType = ReplaceByCustomMappedIfNeeded(discrType);
        ArrayList coveredDiscriminatorRange = ExtractCoveredDiscriminatorRange((ASTswitch_body)switchBody, 
                                                                               discrType, thisInfo);
        
        genHelper.AddDiscriminatorFieldAndProperty(discrType, coveredDiscriminatorRange);
        switchBody.jjtAccept(this, thisInfo);        
        
        // create the resulting type
        Type resultType = m_typeManager.EndUnionTypeDefinition(forSymbol, genHelper);
        return new TypeContainer(resultType);
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTswitch_type_spec, Object)
     */
    public Object visit(ASTswitch_type_spec node, Object data) {
        if (!(data is UnionBuildInfo)) {
            throw new InternalCompilerException("invalid parameter in visis ASTswitch_type_spec");
        }
        UnionBuildInfo buildInfo = (UnionBuildInfo)data;
        SimpleNode child = (SimpleNode)node.jjtGetChild(0);
        return ResovleTypeSpec(child, buildInfo);
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTswitch_body, Object)
     */
    public Object visit(ASTswitch_body node, Object data) {        
        if (!(data is UnionBuildInfo)) {
            throw new InternalCompilerException("invalid parameter in visit ASTswitch_body");
        }
        UnionBuildInfo buildInfo = (UnionBuildInfo)data;
       
        // visit the different switch cases:
        node.childrenAccept(this, buildInfo);               
        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTcasex, Object)
     */
    public Object visit(ASTcasex node, Object data) {
        if (!(data is UnionBuildInfo)) {
            throw new InternalCompilerException("invalid parameter in visit ASTswitch_body");
        }
        UnionBuildInfo buildInfo = (UnionBuildInfo)data;
        // REFACTORING possiblity: replace direct use of values by using the Literals
        // case node consists of one or more case-labels followed by an element spec
        // collect the data for this switch-case
        object[] discriminatorValues = CollectDiscriminatorValuesForCase(node, 
                                                                         buildInfo.GetGenerationHelper().DiscriminatorType, 
                                                                         buildInfo);
        
        ASTelement_spec elemSpec = (ASTelement_spec)node.jjtGetChild(node.jjtGetNumChildren() - 1);
        ASTtype_spec typeSpecNode = (ASTtype_spec)elemSpec.jjtGetChild(0);
        TypeContainer elemType = (TypeContainer)typeSpecNode.jjtAccept(this, buildInfo);
        if (elemType == null) {
            throw new InvalidIdlException(
                String.Format("union elem type not defined for {0}",
                              node.GetIdentification()));
        }
        elemType = ReplaceByCustomMappedIfNeeded(elemType);
        Node elemDecl = elemSpec.jjtGetChild(1);
        string elemDeclIdent = 
                DetermineTypeAndNameForDeclarator((ASTdeclarator)elemDecl, data,
                                                  ref elemType);
        // generate the methods/field for this switch-case
        buildInfo.GetGenerationHelper().GenerateSwitchCase(elemType, elemDeclIdent, discriminatorValues);

        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTcase_label, Object)
     */
    public Object visit(ASTcase_label node, Object data) {
        // child constains a const_exp
        return node.jjtGetChild(0).jjtAccept(this, data);
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTelement_spec, Object)
     */
    public Object visit(ASTelement_spec node, Object data) {
        // nothing to do, nodes are handled by a parent node
        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTenum_type, Object)
     * @param data the current buildinfo instance
     */
    public Object visit(ASTenum_type node, Object data) {
        CheckParameterForBuildInfo(data, node);
        BuildInfo buildInfo = (BuildInfo) data;
        Symbol forSymbol = buildInfo.GetBuildScope().getSymbol(node.getIdent());
        // check if type is known from a previous run over a parse tree --> if so: skip
        TypeContainer resultTypeContainer = null;
        if (m_typeManager.CheckSkip(forSymbol)) {
            resultTypeContainer = m_typeManager.GetKnownType(forSymbol);            
        } else {                
            TypeAttributes typeAttrs = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed;        
            TypeBuilder enumToCreate = 
                m_typeManager.StartTypeDefinition(forSymbol, typeAttrs,
                                                  typeof(System.Enum), Type.EmptyTypes, false);                                                                                     
        
        	// add value__ field, see DefineEnum method of ModuleBuilder
            enumToCreate.DefineField("value__", typeof(System.Int32), 
                                     FieldAttributes.Public | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName);
        
        	// add enum entries
            for (int i = 0; i < node.jjtGetNumChildren(); i++) {
                String enumeratorId = ((SimpleNodeWithIdent)node.jjtGetChild(i)).getIdent();
                FieldBuilder enumVal = enumToCreate.DefineField(enumeratorId, enumToCreate, 
                	                                            FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal);
                enumVal.SetConstant((System.Int32) i);
            }

            // add type specific attributes
            enumToCreate.SetCustomAttribute(new IdlEnumAttribute().CreateAttributeBuilder());
            m_ilEmitHelper.AddSerializableAttribute(enumToCreate);
        
            // create the type
            Type resultType = m_typeManager.EndTypeDefinition(forSymbol);
            resultTypeContainer = new TypeContainer(resultType);
        }

        // update the symbol values: (do this also, if type is from another assembly)
        for (int i = 0; i < node.jjtGetNumChildren(); i++) {
            String enumeratorId = ((SimpleNodeWithIdent)node.jjtGetChild(i)).getIdent();
            // update symbol with value
            SymbolValue symbol = (SymbolValue)buildInfo.GetBuildScope().getSymbol(enumeratorId);
            object enumVal = Enum.ToObject(resultTypeContainer.GetCompactClsType(), (System.Int32) i);
            symbol.SetValueAsLiteral(new EnumValLiteral(enumVal));
        }

        return resultTypeContainer;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTenumerator, Object)
     */
    public Object visit(ASTenumerator node, Object data) {
        return null; // nothing to to, used by parent
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTsequence_type, Object)
     * @param data the buildinfo in use for the current scope
     * @return the type container for the IDLSequence type
     */
    public Object visit(ASTsequence_type node, Object data) {
        CheckParameterForBuildInfo(data, node);        
        BuildInfo containerInfo = (BuildInfo) data;
        SimpleNode elemTypeNode = (SimpleNode)node.jjtGetChild(0);
        if (containerInfo.GetContainterType() != null) {
            // inform the type-manager of structs/unions in creation, because
            // recursion using seuqneces is the only allowed recursion for structs/unions
            m_typeManager.PublishTypeForSequenceRecursion(containerInfo.GetContainerSymbol(),
                                                          containerInfo.GetContainterType());
        }
        Debug.WriteLine("determine element type of IDLSequence");
        TypeContainer elemType = (TypeContainer)elemTypeNode.jjtAccept(this, data);
        // disallow further recursive use of union/struct (before next seq recursion)
        m_typeManager.UnpublishTypeForSequenceRecursion();
        if (elemType == null) {
            throw new InvalidIdlException(
                String.Format("sequence element type not defined for {0}",
                              node.GetIdentification()));
        }
        elemType = ReplaceByCustomMappedIfNeeded(elemType);
        // use here the fusioned type as element type; potential unboxing of element type 
        // should be done by users of TypeContainer (if needed)!
        Debug.WriteLine("seq type determined: " + elemType.GetCompactClsType());
        // create CLS array type with the help of GetType(), otherwise not possible
        Type arrayType;
        // because not fully defined types are possible, use module and not assembly to get type from
        Module declModule = elemType.GetCompactClsType().Module;
        arrayType = declModule.GetType(elemType.GetCompactClsType().FullName + "[]"); // not nice, better solution ?        
        Debug.WriteLine("created array type: " + arrayType);
        
        // determin if sequence is bounded or unbounded
        long bound = 0;
        if (node.jjtGetNumChildren() > 1) { 
            // bounded sequnece
            bound = (long) node.jjtGetChild(1).jjtAccept(this, data);
        }

        // determine the needed attributes: IdlSequence is required by the sequence itself; 
        // combine with the attribute from the element type
        // possible are: IdlSequence (for sequence of sequence), ObjectIdlType,
        // WideChar, StringValue
        // invariant: boxed value attribute is not among them, because elem type 
        // is in the compact form        
        AttributeExtCollection elemAttributes = elemType.GetCompactTypeAttrInstances();
        long seqAttrOrderNr = IdlSequenceAttribute.DetermineSequenceAttributeOrderNr(elemAttributes);
        IdlSequenceAttribute seqAttr = new IdlSequenceAttribute(seqAttrOrderNr, bound);
        AttributeExtCollection sequenceAttributes = 
            new AttributeExtCollection(elemAttributes);
        sequenceAttributes = sequenceAttributes.MergeAttribute(seqAttr);
        TypeContainer result = new TypeContainer(arrayType,
                                                 sequenceAttributes );
        return result;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTstring_type, Object)
     * @param data unsed
     */
    public Object visit(ASTstring_type node, Object data) {
        AttributeExtCollection attrs = new AttributeExtCollection(
            new Attribute[] { new StringValueAttribute(), new WideCharAttribute(false) });
        TypeContainer containter = new TypeContainer(typeof(System.String), attrs);
        return containter;
    }
    
    /**
     * @see parser.IDLParserVisitor#visit(ASTwide_string_type, Object)
     * @param data unsed
     * @return a TypeContainer for the wideString-Type
     */
    public Object visit(ASTwide_string_type node, Object data) {
        AttributeExtCollection attrs = new AttributeExtCollection(
            new Attribute[] { new StringValueAttribute(), new WideCharAttribute(true) });
        TypeContainer containter = new TypeContainer(typeof(System.String), attrs);
        return containter;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTarray_declarator, Object)
     */
    public Object visit(ASTarray_declarator node, Object data) {
        int[] dimensions = new int[node.jjtGetNumChildren()];
        for (int i = 0; i < node.jjtGetNumChildren(); i++) {            
            long dimension = (long)node.jjtGetChild(i).jjtAccept(this, data);
            dimensions[i] = (int)dimension;
        }
        return dimensions;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTfixed_array_size, Object)
     */
    public Object visit(ASTfixed_array_size node, Object data) {
        long dimensionSize = (long)node.jjtGetChild(0).jjtAccept(this, data);                                  
        return dimensionSize;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTattr_dcl, Object)
     * @param data the buildinfo of the type, which declares this attribute
     */
    public Object visit(ASTattr_dcl node, Object data) {
        CheckParameterForBuildInfo(data, node);
        BuildInfo info = (BuildInfo) data;
        TypeBuilder builder = info.GetContainterType();
        ASTparam_type_spec typeSpecNode = (ASTparam_type_spec)node.jjtGetChild(0);
        TypeContainer propType = (TypeContainer)typeSpecNode.jjtAccept(this, info);
        if (propType == null) {
            throw new InvalidIdlException(
                String.Format("attribute type {0} not defined for attribute(s) {1}",
                              typeSpecNode.GetIdentification(), 
                              node.GetIdentification()));
        }
        propType = ReplaceByCustomMappedIfNeeded(propType);
        for (int i = 1; i < node.jjtGetNumChildren(); i++) {
            ASTsimple_declarator simpleDecl = (ASTsimple_declarator) node.jjtGetChild(i);
            String propName = IdlNaming.MapIdlNameToClsName(simpleDecl.getIdent());
            String transmittedNameGetter = 
                IdlNaming.DetermineGetterTransmissionName(simpleDecl.getIdent());
            // set the methods for the property
            MethodBuilder getAccessor = m_ilEmitHelper.AddPropertyGetter(builder, 
                                                                         propName, transmittedNameGetter,
                                                                         propType,
                                                                         MethodAttributes.Virtual | MethodAttributes.Abstract | MethodAttributes.Public |
                                                                         MethodAttributes.NewSlot);
            MethodBuilder setAccessor = null;
            if (!(node.isReadOnly())) {
                String transmittedNameSetter = 
                    IdlNaming.DetermineSetterTransmissionName(simpleDecl.getIdent());

                setAccessor = m_ilEmitHelper.AddPropertySetter(builder, 
                                                               propName, transmittedNameSetter,
                                                               propType,
                                                               MethodAttributes.Virtual | MethodAttributes.Abstract | MethodAttributes.Public |
                                                               MethodAttributes.NewSlot);
            }            
            string fromIdlPropertyName =
                IdlNaming.DetermineAttributeTransmissionName(simpleDecl.getIdent());
            m_ilEmitHelper.AddProperty(builder, propName, fromIdlPropertyName,
                                       propType, getAccessor, setAccessor);
        }
        
        return null;
    }

    /// <summary>
    /// adds a GetObjectData override to the exception to create.
    /// </summary>    
    private void AddExceptionGetObjectDataOverride(TypeBuilder exceptToCreate, IList members) {
        ParameterSpec[] getObjDataParams = new ParameterSpec[] { 
            new ParameterSpec("info", typeof(System.Runtime.Serialization.SerializationInfo)), 
            new ParameterSpec("context", typeof(System.Runtime.Serialization.StreamingContext)) };
        MethodBuilder getObjectDataMethod =
            m_ilEmitHelper.AddMethod(exceptToCreate, "GetObjectData", getObjDataParams,
                                 new TypeContainer(typeof(void)),
                                 MethodAttributes.Virtual | MethodAttributes.Public |
                                 MethodAttributes.HideBySig);        
        ILGenerator body = 
            getObjectDataMethod.GetILGenerator();
        body.Emit(OpCodes.Ldarg_0);
        body.Emit(OpCodes.Ldarg_1);
        body.Emit(OpCodes.Ldarg_2);
        body.Emit(OpCodes.Call, typeof(Exception).GetMethod("GetObjectData", BindingFlags.Public | 
                                                                             BindingFlags.Instance));        
        
        MethodInfo addValueMethod = 
            typeof(System.Runtime.Serialization.SerializationInfo).GetMethod("AddValue",  BindingFlags.Public | BindingFlags.Instance,
                                                                             null,
                                                                             new Type[] { ReflectionHelper.StringType,
                                                                                          ReflectionHelper.ObjectType,
                                                                                          ReflectionHelper.TypeType },
                                                                             new ParameterModifier[0]);
        MethodInfo getTypeFromH = ReflectionHelper.TypeType.GetMethod("GetTypeFromHandle", BindingFlags.Public | BindingFlags.Static);
        for (int i = 0; i < members.Count; i++) {
            FieldInfo member = (FieldInfo)members[i];
            body.Emit(OpCodes.Ldarg_1); // info
            body.Emit(OpCodes.Ldstr, member.Name); // memberName
            body.Emit(OpCodes.Ldarg_0); // this
            body.Emit(OpCodes.Ldfld, member); // load the member
            if (member.FieldType.IsValueType) {
                // need to box a valuetype, because formal parameter is object
                body.Emit(OpCodes.Box, member.FieldType);
            }           
            body.Emit(OpCodes.Ldtoken, member.FieldType);
            body.Emit(OpCodes.Call, getTypeFromH); // load the type, the third argument of AddValue
            body.Emit(OpCodes.Callvirt, addValueMethod);
        }
        body.Emit(OpCodes.Ret);
    }

    /// <summary>
    /// adds the constructors for deserialization.
    /// </summary>        
    private void AddExceptionDeserializationConstructors(TypeBuilder exceptToCreate,
                                                         IList members) {
        // for ISerializable
        ParameterSpec[] constrParams = new ParameterSpec[] {
            new ParameterSpec("info", typeof(System.Runtime.Serialization.SerializationInfo)), 
            new ParameterSpec("context", typeof(System.Runtime.Serialization.StreamingContext)) };
         ConstructorBuilder constrBuilder =
            m_ilEmitHelper.AddConstructor(exceptToCreate, constrParams,
                                          MethodAttributes.Family | MethodAttributes.HideBySig);
         ILGenerator body = constrBuilder.GetILGenerator();
         body.Emit(OpCodes.Ldarg_0);
         body.Emit(OpCodes.Ldarg_1);
         body.Emit(OpCodes.Ldarg_2);
         body.Emit(OpCodes.Call, typeof(AbstractUserException).GetConstructor(BindingFlags.Public | BindingFlags.NonPublic |
                                                                  BindingFlags.Instance,
                                                                  null,
                                                                  new Type[] { typeof(System.Runtime.Serialization.SerializationInfo),
                                                                               typeof(System.Runtime.Serialization.StreamingContext) },
                                                                  new ParameterModifier[0]));         
        
        MethodInfo getValueMethod = 
            typeof(System.Runtime.Serialization.SerializationInfo).GetMethod("GetValue", BindingFlags.Instance | BindingFlags.Public,
                                                                             null,
                                                                             new Type[] { typeof(string), typeof(Type) },
                                                                             new ParameterModifier[0]);
        MethodInfo getTypeFromH = ReflectionHelper.TypeType.GetMethod("GetTypeFromHandle", BindingFlags.Public | BindingFlags.Static);
        for (int i = 0; i < members.Count; i++) {
            FieldBuilder member = (FieldBuilder)members[i];
            body.Emit(OpCodes.Ldarg_0); // this for calling store field after GetValue
            body.Emit(OpCodes.Ldarg_1); // info
            body.Emit(OpCodes.Ldstr, member.Name); // ld the first arg of GetValue
            body.Emit(OpCodes.Ldtoken, member.FieldType);
            body.Emit(OpCodes.Call, getTypeFromH); // ld the 2nd arg of GetValue
            body.Emit(OpCodes.Callvirt, getValueMethod); // call info.GetValue
            // now store result in the corresponding field
            m_ilEmitHelper.GenerateCastObjectToType(body, member.FieldType);
            body.Emit(OpCodes.Stfld, member);
        }
         
        body.Emit(OpCodes.Ret);
        // default constructor
        m_ilEmitHelper.AddDefaultConstructor(exceptToCreate, MethodAttributes.Public);
    }
     
    private void AddExceptionRequiredSerializationCode(TypeBuilder exceptToCreate, IList /* FieldBuilder */ members) {
        // add type specific attributes
        m_ilEmitHelper.AddSerializableAttribute(exceptToCreate);
        // GetObjectDataMethod        
        AddExceptionGetObjectDataOverride(exceptToCreate, members);
        // add deserialization constructor
        AddExceptionDeserializationConstructors(exceptToCreate, members);
    }
     
    /**
     * @see parser.IDLParserVisitor#visit(ASTexcept_dcl, Object)
     * @param data expected is an instance of BuildInfo
     * if ((BuildInfo)data).getContainerType() is null, than an independant type-decl is created, else
     * the type delcaration is added to the Type in creation
     */
    public Object visit(ASTexcept_dcl node, Object data) {
        CheckParameterForBuildInfo(data, node);
        BuildInfo buildInfo = (BuildInfo) data;
        Symbol forSymbol = buildInfo.GetBuildScope().getSymbol(node.getIdent());
        // check if type is known from a previous run over a parse tree --> if so: skip
        if (m_typeManager.CheckSkip(forSymbol)) { 
            return null;
        }
       
        TypeBuilder exceptToCreate = 
            m_typeManager.StartTypeDefinition(forSymbol,
                                              TypeAttributes.Class | TypeAttributes.Public,
                                              typeof(AbstractUserException), Type.EmptyTypes, 
                                              false);
                                                                               
        BuildInfo thisTypeInfo = new BuildInfo(buildInfo.GetBuildScope().getChildScope(forSymbol.getSymbolName()),
                                               exceptToCreate,
                                               forSymbol);

        // add fileds ...
        IList members = (IList)GenerateMemberList(node, thisTypeInfo);
        // add inheritance from IIdlEntity        
        exceptToCreate.AddInterfaceImplementation(typeof(IIdlEntity));
        AddExceptionRequiredSerializationCode(exceptToCreate, members);
        AddExplicitSerializationOrder(exceptToCreate, members);
        
        // create the type
        m_typeManager.EndTypeDefinition(forSymbol);
        return null;
    }
    
    private void AddOneWayAttribute(MethodBuilder builder) {
        ConstructorInfo info = 
            typeof(System.Runtime.Remoting.Messaging.OneWayAttribute).GetConstructor(Type.EmptyTypes);
        CustomAttributeBuilder attributeBuilder = new CustomAttributeBuilder(info, new object[0]);
        builder.SetCustomAttribute(attributeBuilder);
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTop_dcl, Object)
     * @param data expected is an instance of BuildInfo, the operation is added to the type ((BuildInfo)data).getContainerType().
     */
    public Object visit(ASTop_dcl node, Object data) {
        CheckParameterForBuildInfo(data, node);
        BuildInfo buildInfo = (BuildInfo) data;
        
        // return type
        TypeContainer returnType = (TypeContainer)node.jjtGetChild(0).jjtAccept(this, buildInfo);        
        // parameters
        ParameterSpec[] parameters = (ParameterSpec[])node.jjtGetChild(1).jjtAccept(this, buildInfo);
        // name
        String methodName = IdlNaming.MapIdlNameToClsName(node.getIdent());
        String transmittedName = 
            IdlNaming.DetermineOperationTransmissionName(node.getIdent());
        // ready to create method
        TypeBuilder typeAtBuild = buildInfo.GetContainterType();
        MethodBuilder methodBuilder = 
            m_ilEmitHelper.AddMethod(typeAtBuild, methodName, transmittedName,
                                     parameters, returnType,
                                     MethodAttributes.Virtual | MethodAttributes.Abstract | MethodAttributes.Public |
                                     MethodAttributes.HideBySig | MethodAttributes.NewSlot);
        if (node.IsOneWay()) {
            AddOneWayAttribute(methodBuilder);
        }
        int currentChild = 2;
        if ((node.jjtGetNumChildren() > currentChild) && (node.jjtGetChild(currentChild) is ASTraises_expr)) {
            // has a raises expression, add attributes for allowed exceptions
            Type[] exceptionTypes = (Type[])node.jjtGetChild(2).jjtAccept(this, buildInfo);
            foreach (Type exceptionType in exceptionTypes) {
                methodBuilder.SetCustomAttribute(
                    new ThrowsIdlExceptionAttribute(exceptionType).CreateAttributeBuilder());
            }
            currentChild++;
        }        
        if ((node.jjtGetNumChildren() > currentChild) && (node.jjtGetChild(currentChild) is ASTcontext_expr)) {
            string[] contextElementAttrs = (string[])node.jjtGetChild(currentChild).jjtAccept(this, buildInfo);
            foreach (string contextElem in contextElementAttrs) {
                methodBuilder.SetCustomAttribute(
                    new ContextElementAttribute(contextElem).CreateAttributeBuilder());
            }
        }
        return null;
    }        
        
    /** 
     * replaces a TypeContainer with the one for the custom mapped type, if a custom mapped type is
     * present. Else Returns the unmodified one.
     **/
    internal static TypeContainer ReplaceByCustomMappedIfNeeded(TypeContainer specType) {
        Type clsType = specType.GetCompactClsType(); // do the mapping on the fusioned type!
        // check for custom Mapping here:
        CompilerMappingPlugin plugin = CompilerMappingPlugin.GetSingleton();
        if (plugin.IsCustomMappingPresentForIdl(clsType.FullName)) {
            Type mappedType = plugin.GetMappingForIdl(clsType.FullName);
            return new TypeContainer(mappedType);
        } else {
            return specType;
        }       
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTop_type_spec, Object)
     * @param data the active buildinfo for the current scope
     * @return the TypeContainer for this op_type_spec
     */
    public Object visit(ASTop_type_spec node, Object data) {
        TypeContainer returnType;
        if (node.jjtGetNumChildren() == 0) {
            // void
            returnType = new TypeContainer(typeof(void));
        } else {
            // <parameter type spec>
            returnType = (TypeContainer) node.jjtGetChild(0).jjtAccept(this, data);
            if (returnType == null) {
                throw new InvalidIdlException(
                    String.Format("type {0} not (yet) defined for {1}",
                                  ((SimpleNode)node.jjtGetChild(0)).GetIdentification(),
                                  node.GetIdentification()));
            }
            returnType = ReplaceByCustomMappedIfNeeded(returnType);
        }
        return returnType;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTparameter_dcls, Object)
     * @param data the active buildinfo for the current scope
     * @return an array of ParameterSpec instances, describing the paramters
     */
    public Object visit(ASTparameter_dcls node, Object data) {
        ParameterSpec[] parameters = new ParameterSpec[node.jjtGetNumChildren()];
        for (int i = 0; i < node.jjtGetNumChildren(); i++) {
            parameters[i] = (ParameterSpec) node.jjtGetChild(i).jjtAccept(this, data);
        }
        return parameters;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTparam_dcl, Object)
     * @param data the active buildinfo for the current scope
     * @return an instance of ParameterSpec, containing the relevant information
     */
    public Object visit(ASTparam_dcl node, Object data) {
        // determine direction ...
        ParameterSpec.ParameterDirection direction = ((ASTparam_attribute) node.jjtGetChild(0)).getParamDir();
        // determine name and type
        TypeContainer paramType = (TypeContainer)node.jjtGetChild(1).jjtAccept(this, data);
        if (paramType == null) {
            throw new InvalidIdlException(String.Format("parameter type {0} not (yet) defined for {1}", 
                                                        ((SimpleNode)node.jjtGetChild(1)).GetIdentification(),
                                                        node.GetIdentification()));
        }
        paramType = ReplaceByCustomMappedIfNeeded(paramType);
        String paramName = IdlNaming.MapIdlNameToClsName(((ASTsimple_declarator)node.jjtGetChild(2)).getIdent());
        
        ParameterSpec result = new ParameterSpec(paramName, paramType, direction);
        return result;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTparam_attribute, Object)
     */
    public Object visit(ASTparam_attribute node, Object data) {
        return null; // nothing to do
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTraises_expr, Object)
     */
    public Object visit(ASTraises_expr node, Object data) {        
        Type[] result = new Type[node.jjtGetNumChildren()];
        for (int i = 0; i < node.jjtGetNumChildren(); i++) {
            Symbol exceptionSymbol = (Symbol)node.jjtGetChild(i).jjtAccept(this, data);
            result[i] = 
                m_typeManager.GetKnownType(exceptionSymbol).GetCompactClsType();
            
        }
        return result;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTcontext_expr, Object)
     */
    public Object visit(ASTcontext_expr node, Object data) {
        ArrayList result = new ArrayList();
        foreach (string element in node.GetContextElements()) {
            if (!element.EndsWith("*")) {
                result.Add(element);
            } else {
                Console.WriteLine("warning: context element with * at the end not supported by IIOP.NET; ignoring");
            }
        }
        return (string[])result.ToArray(ReflectionHelper.StringType);
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTparam_type_spec, Object)
     * @param data the active buildinfo for the current scope
     * @return a TypeContainter for the Type this node represents
     */
    public Object visit(ASTparam_type_spec node, Object data) {
        CheckParameterForBuildInfo(data, node);
        SimpleNode child = (SimpleNode)node.jjtGetChild(0); // get the node representing <base_type_spec> or <string_type> or <widestring_type> or <scoped_name>
        return ResovleTypeSpec(child, (BuildInfo)data);
    }
    
    #region fixed pt not supported by this compiler
    /**
     * @see parser.IDLParserVisitor#visit(ASTfixed_pt_const_type, Object)
     */
    public Object visit(ASTfixed_pt_const_type node, Object data) {
        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTfixed_pt_type, Object)
     */
    public Object visit(ASTfixed_pt_type node, Object data) {
        return null;
    }
    #endregion

    /**
     * @see parser.IDLParserVisitor#visit(ASTvalue_base_type, Object)
     * @return a Type Container for the Corba type ValueBase
     */
    public Object visit(ASTvalue_base_type node, Object data) {
        AttributeExtCollection attrs = new AttributeExtCollection(
            new Attribute[] { new ObjectIdlTypeAttribute(IdlTypeObject.ValueBase) });
        TypeContainer container = new TypeContainer(typeof(System.Object), attrs);
        return container;
    }
    
    #endregion IMethods

}

}
