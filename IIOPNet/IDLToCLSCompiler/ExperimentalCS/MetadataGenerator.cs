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
using parser;
using symboltable;
using Ch.Elca.Iiop.Idl;
using Ch.Elca.Iiop.Marshalling;
using Ch.Elca.Iiop.Util;
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.IdlCompiler.Exceptions;


namespace Ch.Elca.Iiop.IdlCompiler.Action {


/**
 * generates CLS metadeta for OMG IDL
 */
public class MetaDataGenerator : IDLParserVisitor {

    
    #region Types

    /** helper class, to pass information */
    public class BuildInfo {
        
        #region IFields

        private TypeBuilder m_containerType;
        private Scope m_buildScope;
        
        #endregion IFields
        #region IConstructors
        
        public BuildInfo(Scope buildScope, TypeBuilder containerType) {
            m_buildScope = buildScope;
            m_containerType = containerType;
        }

        #endregion IConstructors
        #region IMethods

        public TypeBuilder GetContainterType() {
            return m_containerType;
        }
        public Scope GetBuildScope() {
            return m_buildScope;
        }

        #endregion IMethods
    }

    /** helper class to collect data about a parameter of an operation */
    internal class ParameterSpec {
        
        #region IFields

        private TypeContainer m_paramType;
        private String m_paramName;
        private int m_direction;

        #endregion IFields
        #region IConstructors
        
        public ParameterSpec(String paramName, TypeContainer paramType, 
                             int direction) {
            m_paramName = paramName;
            m_paramType = paramType;
            m_direction = direction;
        }

        #endregion IConstructors
        #region IMethods

        public String GetPramName() {
            return m_paramName;
        }
        
        public TypeContainer GetParamType() {
            return m_paramType;
        }

        public int GetParamDirection() {
            return m_direction;
        }

        public bool IsInOut() {
            return (m_direction == ASTparam_attribute.ParamDir_INOUT);
        }

        public bool IsIn() {
            return (m_direction == ASTparam_attribute.ParamDir_IN);
        }

        public bool IsOut() {
            return (m_direction == ASTparam_attribute.ParamDir_OUT);
        }

        #endregion IMethods

    }

    #endregion Types
    #region IFields

    private SymbolTable m_symbolTable;

    private AssemblyBuilder m_asmBuilder;

    private ModuleBuilderManager m_modBuilderManager;

    private TypeManager m_typeManager;

    private String m_targetAsmName;

    private TypesInAssemblyManager m_typesInRefAsms;

    /** reference to one of the internal constructor of class ParameterInfo. Used for assigning custom attributes to the return parameter */
    private ConstructorInfo m_paramBuildConstr;
    /** is the generator initalized for parsing a file */
    private bool m_initalized = false;

    /** used to store value types generated: for this types an implementation class must be provided */
    private ArrayList m_valueTypesDefined = new ArrayList();

    #endregion IFields
    #region IConstructors

    public MetaDataGenerator(String targetAssemblyName, String targetDir, ArrayList refAssemblies) {
        m_targetAsmName = targetAssemblyName;
        AssemblyName asmname = new AssemblyName();
        asmname.Name = targetAssemblyName;
        // define a persistent assembly
        m_asmBuilder = System.Threading.Thread.GetDomain().
            DefineDynamicAssembly(asmname, AssemblyBuilderAccess.RunAndSave, targetDir);
        // manager for persistent modules
        m_modBuilderManager = new ModuleBuilderManager(m_asmBuilder, targetAssemblyName);
        Type paramBuildType = typeof(ParameterBuilder);
        m_paramBuildConstr = paramBuildType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                                                           new Type[] { typeof(MethodBuilder), typeof(System.Int32), typeof(ParameterAttributes),
                                                                        typeof(String) }, 
                                                           null);
        // channel assembly contains predef types, which shouldn't be regenerated.
        FileInfo compilerPath = new FileInfo(this.GetType().Assembly.Location);
        refAssemblies.Add(Assembly.LoadFrom(compilerPath.DirectoryName + 
                                            Path.DirectorySeparatorChar + "IIOPChannel.dll")); 
        m_typesInRefAsms = new TypesInAssemblyManager(refAssemblies);
        
    }

    #endregion IConstructors
    #region IMethods

    /** ends the build process, after this is called, the Generator is not able to process more files */
    public void SaveAssembly() {
        // save the assembly to disk
        m_asmBuilder.Save(m_targetAsmName + ".dll");
        // print a remark to remember implementing the valuetypes:
        PrintNeededValueImplList();
    }

    /** prints a list of value types for which an implementation must be provided. */
    private void PrintNeededValueImplList() {
        if (m_valueTypesDefined.Count > 0) {
            Console.WriteLine("\nDon't forget to provide an implementation for the following value types: \n");
            for (int i = 0; i < m_valueTypesDefined.Count; i++) {
                Console.WriteLine(((Type)m_valueTypesDefined[i]).FullName);
            }
            Console.WriteLine("");
        }        
    }

    /** initalize the generator for next source, with using the same target assembly / target modules */
    public void InitalizeForSource(SymbolTable symbolTable) {
        m_symbolTable = symbolTable;
        m_symbolTable.CheckAllFwdDeclsComplete(); // assure symbol table is valid: all fwd decls are defined by a full definition
        // helps to find already declared types
        m_typeManager = new TypeManager(m_modBuilderManager, m_typesInRefAsms);
        // ready for code generation
        m_initalized = true;
    }
    
    /** checks if a type generation can be skipped, because type is already defined in a previous run over a parse tree or in a reference assembly
     * this method is used to support runs over more than one parse tree */
    private bool CheckSkip(Symbol forSymbol) {
        
        if (m_typeManager.IsTypeDeclaredInRefAssemblies(forSymbol)) {
            return true; // skip, because already defined in a referenced assembly -> this overrides the def from idl
        }
        
        if (m_typeManager.CheckInBuildModulesForType(forSymbol)) { 
            return true; // safe to skip, because type is already fully declared in a previous run
        }
                
        // do not skip
        return false;
    }

    /** create or retrieve a Scope for nested IDL-types, which may not be nested inside the mapped CLS type of the container. */
    private Scope GetScopeForNested(Scope containerScope, Symbol createdFor) {
        Scope parentOfContainer = containerScope.getParentScope();
        String nestedScopeName = containerScope.getScopeName() + "_package";
        if (!(parentOfContainer.containsChildScope(nestedScopeName))) {
            parentOfContainer.addChildScope(new Scope(nestedScopeName, parentOfContainer));
        }
        Scope nestedScope = parentOfContainer.getChildScope(nestedScopeName);
        nestedScope.addSymbol(createdFor.getSymbolName());
        return nestedScope;
    }
    
    /** get the types for the scoped names specified in an inheritance relationship
     * @param data the buildinfo of the container of the type having this inheritance relationship
     */
    private Type[] ParseInheritanceRelation(SimpleNode node, BuildInfo data) {
        Type[] result = new Type[node.jjtGetNumChildren()];
        for (int i = 0; i < node.jjtGetNumChildren(); i++) {
            // get symbol
            Symbol sym = (Symbol)(node.jjtGetChild(i).jjtAccept(this, data)); // accept interface_name
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
            result[i] = resultType.GetClsType();
        }
        return result;        
    }
    
    private void AddRepIdAttribute(TypeBuilder typebuild, String repId) {
        if (repId != null) {
            CustomAttributeBuilder repIdAttrBuilder = new RepositoryIDAttribute(repId).CreateAttributeBuilder();
            typebuild.SetCustomAttribute(repIdAttrBuilder);
        }
    }
    
    private void AddSerializableAttribute(TypeBuilder typebuild) {
        Type attrType = typeof(System.SerializableAttribute);
        ConstructorInfo attrConstr = attrType.GetConstructor(Type.EmptyTypes);
        CustomAttributeBuilder serAttr = new CustomAttributeBuilder(attrConstr, new Object[0]);    
        typebuild.SetCustomAttribute(serAttr);
    }
    
    /** check if data is an instance of buildinfo, if not throws an exception */
    private void CheckParameterForBuildInfo(Object data, Node visitedNode) {
        if (!(data is BuildInfo)) { 
            throw new InternalCompilerException("precondition violation in visitor for node" + visitedNode.GetType() +
                                                ", " + data.GetType() + " but expected BuildInfo"); 
        }
    }

    /** need this, because define-parameter prevent creating a parameterbuilder for param-0, the ret param.
     *  For defining custom attributes on the ret-param, a parambuilder is however needed
     *  TBD: search nicer solution for this */
    private ParameterBuilder CreateParamBuilderForRetParam(MethodBuilder forMethod) {
        return (ParameterBuilder) m_paramBuildConstr.Invoke(new Object[] { forMethod, (System.Int32) 0, ParameterAttributes.Retval, "" } );
    }

    /**
     * @see parser.IDLParserVisitor#visit(SimpleNode, Object)
     */
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
        BuildInfo info = new BuildInfo(topScope, null);
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
        BuildInfo modInfo = new BuildInfo(moduleScope, info.GetContainterType());
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
    private TypeBuilder CreateOrGetInterfaceDcl(String fullyQualName, System.Type[] interfaces, bool isAbstract, bool isLocal,
                                                Symbol forSymbol, String repId, ModuleBuilder modBuilder) {
        TypeBuilder interfaceToBuild = null;
        if (!m_typeManager.IsFwdDeclared(forSymbol)) {
            Trace.WriteLine("generating code for interface: " + fullyQualName);
            interfaceToBuild = modBuilder.DefineType(fullyQualName, TypeAttributes.Interface | TypeAttributes.Public | TypeAttributes.Abstract,
                                                     null, interfaces);
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
            // add repository ID
            AddRepIdAttribute(interfaceToBuild, repId);
            interfaceToBuild.AddInterfaceImplementation(typeof(IIdlEntity));
            // register type with type manager as not fully declared
            m_typeManager.RegisterTypeFwdDecl(interfaceToBuild, forSymbol);    
        } else {
            // get incomplete type
            Trace.WriteLine("complete interface: " + fullyQualName);
            interfaceToBuild = (TypeBuilder)(m_typeManager.GetKnownType(forSymbol).GetClsType());
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
        if (CheckSkip(forSymbol)) { 
            return null; 
        }

        // retrieve first types for the inherited
        System.Type[] interfaces = (System.Type[])header.jjtAccept(this, data);
        String fullyQualName = enclosingScope.getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());

        ModuleBuilder curModBuilder = m_modBuilderManager.GetOrCreateModuleBuilderFor(enclosingScope);
        TypeBuilder interfaceToBuild = CreateOrGetInterfaceDcl(fullyQualName, interfaces, header.isAbstract(), header.isLocal(), 
                                                               forSymbol, enclosingScope.getRepositoryIdFor(header.getIdent()),
                                                               curModBuilder);

        // generate body
        ASTinterface_body body = (ASTinterface_body)node.jjtGetChild(1);
        BuildInfo buildInfo = new BuildInfo(enclosingScope.getChildScope(forSymbol.getSymbolName()), interfaceToBuild);
        body.jjtAccept(this, buildInfo);
    
        // create the type
        Type resultType = interfaceToBuild.CreateType();
        m_typeManager.ReplaceFwdDeclWithFullDecl(resultType, forSymbol);
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
        if (CheckSkip(forSymbol)) { 
            return null; 
        }
        
        String fullyQualName = enclosingScope.getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());
        if (!(m_typeManager.IsTypeDeclarded(forSymbol))) { // ignore fwd-decl if type is already declared, if not generate type for fwd decl
            ModuleBuilder curModBuilder = m_modBuilderManager.GetOrCreateModuleBuilderFor(enclosingScope);
            // it's no problem to add later on interfaces this type should implement with AddInterfaceImplementation,
            // here: specify no interface inheritance, because not known at this point
            CreateOrGetInterfaceDcl(fullyQualName, Type.EmptyTypes, node.isAbstract(), node.isLocal(),
                                    forSymbol, enclosingScope.getRepositoryIdFor(node.getIdent()), 
                                    curModBuilder);
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
        if (node.jjtGetNumChildren() > 0) {
            ASTinterface_inheritance_spec inheritSpec = (ASTinterface_inheritance_spec) node.jjtGetChild(0);
            result = (Type[])inheritSpec.jjtAccept(this, data);
        }
        return result;
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
     * resolve a scoped name starting in searchScope
     */
    private Symbol ResolveScopedName(Scope searchScope, ArrayList parts) {
        if ((parts == null) || (parts.Count == 0)) {
        	return null;
        }
        Scope currentScope = searchScope;
        for (int i = 0; i < parts.Count - 1; i++) {
            // resolve scopes
            currentScope = currentScope.getChildScope((String)parts[i]);
            if (currentScope == null) { 
            	return null; // not found within this searchScope
            }
        }
        // resolve symbol
        Symbol sym = currentScope.getSymbol((String)parts[parts.Count - 1]);
        return sym;
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
        
        Symbol found = null;
        // search in this scope and all parent scopes
        while ((found == null) && (currentScope != null)) {
            found = ResolveScopedName(currentScope, parts);
            // if not found: next scope to search in is parent scope
            currentScope = currentScope.getParentScope();
        }
        
        // TODO: search in inherited scopes as described in CORBA 2.3, section 3.15.2
        
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
    private TypeBuilder CreateOrGetValueDcl(String fullyQualName, System.Type[] interfaces, 
                                            System.Type parent, bool isAbstract, Symbol forSymbol, 
                                            String repId, ModuleBuilder modBuilder) {
        TypeBuilder valueToBuild;
        if (!m_typeManager.IsFwdDeclared(forSymbol)) {
            Trace.WriteLine("generating code for value type: " + fullyQualName);
            TypeAttributes attrs = TypeAttributes.Public | TypeAttributes.Abstract;
            if (isAbstract) {
                attrs |= TypeAttributes.Interface;
                if (parent != null) { 
                    throw new InvalidIdlException("not possible for an abstract value type " +
                                                  fullyQualName + " to inherit from a concrete one " +
                                                  parent.FullName);
                }
            } else {
                attrs |= TypeAttributes.Class;
            }
            valueToBuild = modBuilder.DefineType(fullyQualName, attrs, parent, interfaces);
            // add repository ID
            AddRepIdAttribute(valueToBuild, repId);
            if (isAbstract) {
                // add InterfaceTypeAttribute
                IdlTypeInterface ifType = IdlTypeInterface.AbstractValueType;
                CustomAttributeBuilder interfaceTypeAttrBuilder = new InterfaceTypeAttribute(ifType).CreateAttributeBuilder();
                valueToBuild.SetCustomAttribute(interfaceTypeAttrBuilder);
            }
            valueToBuild.AddInterfaceImplementation(typeof(IIdlEntity)); // implement IDLEntity
            // register type with type manager as not fully declared
            m_typeManager.RegisterTypeFwdDecl(valueToBuild, forSymbol);    
        } else {
            // get incomplete type
            Trace.WriteLine("complete valuetype: " + fullyQualName);
            valueToBuild = (TypeBuilder)m_typeManager.GetKnownType(forSymbol).GetClsType();
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

    /** add abstract methods for all implemented interfaces to the abstract class,
     *  add properties for all implemented interfaces to the abstrct class */
    private void AddInheritedMembersAbstractDeclToClassForIf(TypeBuilder classBuilder, System.Type[] interfaces) {
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
                // normal parameters
                ParameterInfo[] parameters = methods[j].GetParameters();
                System.Type[] paramTypes = new System.Type[parameters.Length];
                for (int k = 0; k < parameters.Length; k++) {
                    paramTypes[k] = parameters[k].ParameterType;
                }
                MethodBuilder method = classBuilder.DefineMethod(methods[j].Name, 
                                                                 MethodAttributes.Abstract | MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig, 
                                                                 methods[j].ReturnType, paramTypes);
                for (int k = 0; k < parameters.Length; k++) {
                    SetParamAttrs(method, parameters[k]);
                }
                // return parameter
                Object[] retAttrs = methods[j].ReturnTypeCustomAttributes.GetCustomAttributes(false);
                // add custom attributes for the return type
                ParameterBuilder paramBuild = CreateParamBuilderForRetParam(method);
                for (int k = 0; k < retAttrs.Length; k++) {
                    if (retAttrs[k] is IIdlAttribute) {
                        CustomAttributeBuilder attrBuilder = ((IIdlAttribute) retAttrs[k]).CreateAttributeBuilder();
                        paramBuild.SetCustomAttribute(attrBuilder);    
                    }
                }
            }
            // properties
            PropertyInfo[] properties = ifType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            for (int j = 0; j < properties.Length; j++) {
                PropertyBuilder propBuild = classBuilder.DefineProperty(properties[j].Name, PropertyAttributes.None,
                                                                        properties[j].PropertyType, System.Type.EmptyTypes);


                // set the methods for the property
                MethodBuilder getAccessor = classBuilder.DefineMethod("__get_" + properties[j].Name, 
                                                                      MethodAttributes.Virtual | MethodAttributes.Abstract | MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, 
                                                                      properties[j].PropertyType, System.Type.EmptyTypes);
                propBuild.SetGetMethod(getAccessor);
                MethodBuilder setAccessor = null;
                if (properties[j].CanWrite) {
                    setAccessor = classBuilder.DefineMethod("__set_" + properties[j].Name, 
                                                            MethodAttributes.Virtual | MethodAttributes.Abstract | MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, 
                                                            null, new System.Type[] { properties[j].PropertyType });
                    propBuild.SetSetMethod(setAccessor);
                }
            
                ParameterBuilder retParamGet = CreateParamBuilderForRetParam(getAccessor);
                ParameterBuilder valParam = null;
                if (setAccessor != null) { 
                    valParam = setAccessor.DefineParameter(1, ParameterAttributes.None, "value"); 
                }
                // add custom attributes
                Object[] attrs = properties[j].GetCustomAttributes(true);
                for (int k = 0; k < attrs.Length; k++) {
                    if (attrs[k] is IIdlAttribute) {
                        CustomAttributeBuilder attrBuilder = ((IIdlAttribute) attrs[k]).CreateAttributeBuilder();
                        propBuild.SetCustomAttribute(attrBuilder);
                
                        retParamGet.SetCustomAttribute(attrBuilder);
                        if (setAccessor != null) {
                            valParam.SetCustomAttribute(attrBuilder);
                        }
                    }
                } 

            }

        }
    }
    
    /** Defines the parameter-attributes for a method parameter (inclusive custom attributes) */
    private void SetParamAttrs(MethodBuilder methodBuild, ParameterInfo info) {
        ParameterAttributes paramAttr = ParameterAttributes.None;
        if (info.IsOut) { 
            paramAttr = paramAttr | ParameterAttributes.Out; 
        }
        ParameterBuilder paramBuild = methodBuild.DefineParameter(info.Position + 1, 
                                                                  paramAttr, info.Name);
        // custom attributes
        System.Object[] attrs = info.GetCustomAttributes(false);
        for (int i = 0; i < attrs.Length; i++) {
            if (attrs[i] is IIdlAttribute) {
                CustomAttributeBuilder attrBuilder = ((IIdlAttribute) attrs[i]).CreateAttributeBuilder();
                paramBuild.SetCustomAttribute(attrBuilder);    
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
        if (CheckSkip(forSymbol)) { 
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

        Type baseClass = null;
        if ((inheritFrom.Length > 0) && (inheritFrom[0].IsClass)) {
            // only the first entry may be a class for a concrete value type: multiple inheritance is not allowed for concrete value types, the value type from which is inherited from must be first in inheritance list, 3.8.5 in CORBA 2.3.1 spec
            baseClass = inheritFrom[0];
            Type[] tmp = new Type[inheritFrom.Length-1];
            Array.Copy(inheritFrom, 1, tmp, 0, tmp.Length);
            inheritFrom = tmp;
        }

        String fullyQualName = enclosingScope.getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());
        ModuleBuilder curModBuilder = m_modBuilderManager.GetOrCreateModuleBuilderFor(enclosingScope);
        TypeBuilder valueToBuild = CreateOrGetValueDcl(fullyQualName, inheritFrom, baseClass, 
                                                       false, forSymbol, 
                                                       enclosingScope.getRepositoryIdFor(header.getIdent()), 
                                                       curModBuilder);
        
        // add implementation class attribute
        valueToBuild.SetCustomAttribute(new ImplClassAttribute(fullyQualName + "Impl").CreateAttributeBuilder());
        // add serializable attribute
        AddSerializableAttribute(valueToBuild);

        // generate elements
        BuildInfo buildInfo = new BuildInfo(enclosingScope, valueToBuild);
        for (int i = 1; i < node.jjtGetNumChildren(); i++) { // for all value_element children
            ASTvalue_element elem = (ASTvalue_element)node.jjtGetChild(i);
            elem.jjtAccept(this, buildInfo);    
        }

        // finally create the type
        Type resultType = valueToBuild.CreateType();
        m_typeManager.ReplaceFwdDeclWithFullDecl(resultType, forSymbol);
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
        if (CheckSkip(forSymbol)) { 
            return null; 
        }

        Type[] interfaces = ParseValueInheritSpec(node, (BuildInfo) data);
        if ((interfaces.Length > 0) && (interfaces[0].IsClass)) { 
            throw new InvalidIdlException("invalid abstract value type, " + forSymbol.getSymbolName() + 
                                          " can only inherit from abstract value types, but not from: " + interfaces[0]);
        }
        int bodyNodeIndex = 0;
        for (int i = 0; i < node.jjtGetNumChildren(); i++) {
            if (!((node.jjtGetChild(i) is ASTvalue_base_inheritance_spec) || (node.jjtGetChild(i) is ASTvalue_support_inheritance_spec))) {
                bodyNodeIndex = i;
                break;
            }
        }

        String fullyQualName = enclosingScope.getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());
        ModuleBuilder curModBuilder = m_modBuilderManager.GetOrCreateModuleBuilderFor(enclosingScope);
        TypeBuilder valueToBuild = CreateOrGetValueDcl(fullyQualName, interfaces, null,
                                                       true, forSymbol, 
                                                       enclosingScope.getRepositoryIdFor(node.getIdent()),
                                                       curModBuilder);

        // generate elements
        BuildInfo buildInfo = new BuildInfo(enclosingScope, valueToBuild);
        for (int i = bodyNodeIndex; i < node.jjtGetNumChildren(); i++) { // for all export children
            Node child = node.jjtGetChild(i);
            child.jjtAccept(this, buildInfo);    
        }

        // finally create the type
        Type resultType = valueToBuild.CreateType();
        m_typeManager.ReplaceFwdDeclWithFullDecl(resultType, forSymbol);
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
        if (CheckSkip(forSymbol)) { 
            return null;
        }
        
        Debug.WriteLine("begin boxed value type: " + node.getIdent());
        String fullyQualName = enclosingScope.getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());
        ModuleBuilder curModBuilder = m_modBuilderManager.GetOrCreateModuleBuilderFor(enclosingScope);
        // get the boxed type
        TypeContainer boxedType = (TypeContainer)node.jjtGetChild(0).jjtAccept(this, data);
        Trace.WriteLine("generating code for boxed value type: " + fullyQualName);
        BoxedValueTypeGenerator boxedValueGen = new BoxedValueTypeGenerator();
        TypeBuilder resultType = boxedValueGen.CreateBoxedType(boxedType.GetClsType(), curModBuilder,
                                                               fullyQualName, boxedType.GetAttrs());
        AddRepIdAttribute(resultType, enclosingScope.getRepositoryIdFor(node.getIdent()));
        resultType.AddInterfaceImplementation(typeof(IIdlEntity));
        Type result = resultType.CreateType();
        m_typeManager.RegisterTypeDefinition(result, forSymbol);        
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
        if (CheckSkip(forSymbol)) { 
            return null; 
        }
        
        // create only the type-builder, but don't call createType()
        String fullyQualName = enclosingScope.getFullyQualifiedNameForSymbol(node.getIdent());
        if (!(m_typeManager.IsTypeDeclarded(forSymbol))) { // if the full type declaration already exists, ignore fwd decl
            ModuleBuilder curModBuilder = m_modBuilderManager.GetOrCreateModuleBuilderFor(enclosingScope);
            // it's no problem to add later on interfaces this type should implement and the base class this type should inherit from with AddInterfaceImplementation / set parent
            // here: specify no inheritance, because not known at this point
            CreateOrGetValueDcl(fullyQualName, Type.EmptyTypes, null, node.isAbstract(),
                                forSymbol, enclosingScope.getRepositoryIdFor(node.getIdent()),
                                curModBuilder);        
        }
        return null;
    }

    
    /** search in a value_header_node / abs_value_node for inheritance information and parse it
     * @param parentOfPossibleInhNode the node possibly containing value inheritance nodes
     */
    public Type[] ParseValueInheritSpec(Node parentOfPossibleInhNode, BuildInfo data) {
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
        node.jjtGetChild(0).jjtAccept(this, data); // generate for an export, state or init_dcl member
        return null;
    }

    #region constructor definition, at the moment not supported
    /**
     * @see parser.IDLParserVisitor#visit(ASTinit_decl, Object)
     */
    public Object visit(ASTinit_decl node, Object data) {
        // at the moment do nothing
        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTinit_param_attribute, Object)
     */
    public Object visit(ASTinit_param_attribute node, Object data) {
        // at the moment do nothing
        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTinit_param_decl, Object)
     */
    public Object visit(ASTinit_param_decl node, Object data) {
        // at the moment do nothing
        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTinit_param_delcs, Object)
     */
    public Object visit(ASTinit_param_delcs node, Object data) {
        // at the moment do nothing
        return null;
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
        // special handling for BoxedValue types --> unbox it
        if (fieldType.GetClsType().IsSubclassOf(typeof(BoxedValueBase))) {
            fieldType = mapBoxedValueTypeToUnboxed(fieldType.GetClsType());
        }
        String[] decl = (String[])node.jjtGetChild(1).jjtAccept(this, data);
        FieldBuilder fieldBuild;
    	Type clsType = GetTypeFromTypeContainer(fieldType);
        for (int i = 0; i < decl.Length; i++) {
            if (node.isPrivate()) { // map to protected field
                String privateName = decl[i];
                // compensate a problem in the java rmi compiler, which can produce illegal idl:
                // it produces idl-files with name clashes if a method getx() and a field x exists
                if (!privateName.StartsWith("m_")) { 
                    privateName = "m_" + privateName; 
                }
                privateName = IdlNaming.MapIdlNameToClsName(privateName);
                fieldBuild = builder.DefineField(privateName, clsType, FieldAttributes.Family);
            } else { // map to public field
                String fieldName = IdlNaming.MapIdlNameToClsName(decl[i]);
                fieldBuild = builder.DefineField(fieldName, clsType, FieldAttributes.Public);
            }
            // add custom attributes
            for (int j = 0; j < fieldType.GetAttrs().Length; j++) {
                fieldBuild.SetCustomAttribute(fieldType.GetAttrs()[j]);    
            }
        }
        return null;
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
                                              ((BuildInfo)data).GetContainterType().FullName +
                                              " for value types, only one concrete value type parent is possible at the first position in the inheritance spec");
            }
            AttributeExtCollection attrs = AttributeExtCollection.ConvertToAttributeCollection(result[i].GetCustomAttributes(typeof(InterfaceTypeAttribute), true));
            if (attrs.IsInCollection(typeof(InterfaceTypeAttribute))) {
                InterfaceTypeAttribute ifAttr = (InterfaceTypeAttribute)attrs.GetAttributeForType(typeof(InterfaceTypeAttribute));
                if (!(ifAttr.IdlType.Equals(IdlTypeInterface.AbstractValueType))) {
                    throw new Exception("invalid supertype: " + result[i].FullName + " for type: " + 
                                               ((BuildInfo)data).GetContainterType().FullName +
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
        // throw new NotSupportedException("constants are not yet supported");
        Console.WriteLine("warning: constants are not yet supported; " + node.getIdent());
        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTconst_type, Object)
     */
    public Object visit(ASTconst_type node, Object data) {
        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTconst_exp, Object)
     */
    public Object visit(ASTconst_exp node, Object data) {
        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTor_expr, Object)
     */
    public Object visit(ASTor_expr node, Object data) {
        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTxor_expr, Object)
     */
    public Object visit(ASTxor_expr node, Object data) {
        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTand_expr, Object)
     */
    public Object visit(ASTand_expr node, Object data) {
        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTshift_expr, Object)
     */
    public Object visit(ASTshift_expr node, Object data) {
        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTadd_expr, Object)
     */
    public Object visit(ASTadd_expr node, Object data) {
        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTmult_expr, Object)
     */
    public Object visit(ASTmult_expr node, Object data) {
        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTunary_expr, Object)
     */
    public Object visit(ASTunary_expr node, Object data) {
        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTprimary_expr, Object)
     */
    public Object visit(ASTprimary_expr node, Object data) {
        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTliteral, Object)
     */
    public Object visit(ASTliteral node, Object data) {
        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTpositive_int_const, Object)
     */
    public Object visit(ASTpositive_int_const node, Object data) {
        return null;
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
            if (decl.jjtGetChild(0) is ASTsimple_declarator) {
                String ident = ((SimpleNodeWithIdent) decl.jjtGetChild(0)).getIdent();
                Symbol typedefSymbol = currentScope.getSymbol(ident);
                // inform the type-manager of this typedef
                Debug.WriteLine("typedef defined here, type: " + typeUsedInDefine.GetClsType() +
                                ", symbol: " + typedefSymbol);
                m_typeManager.RegisterTypeDef(typeUsedInDefine, typedefSymbol);
            }
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

    /**
     * @see parser.IDLParserVisitor#visit(ASTdeclarators, Object)
     * @param data unused
     * @return an array of all declared elements here
     */
    public Object visit(ASTdeclarators node, Object data) {
        String[] result = new String[node.jjtGetNumChildren()];
        for (int i = 0; i < node.jjtGetNumChildren(); i++) {
            Node child = node.jjtGetChild(i).jjtGetChild(0); // child of i-th declarator
            if (child is ASTcomplex_declarator) {
                throw new NotSupportedException("complex_declarator is unsupported by this compiler");
            }
            // a simple delcarator
            ASTsimple_declarator simpleDecl = (ASTsimple_declarator) child;
            result[i] = simpleDecl.getIdent();
        }
        return result;
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
        return new TypeContainer(typeof(System.Int16));
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTunsigned_long_int, Object)
     * @param data unused
     * @return a TypeContainer for the long type
     */
    public Object visit(ASTunsigned_long_int node, Object data) {
        return new TypeContainer(typeof(System.Int32));
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTunsigned_longlong_int, Object)
     * @param data unused
     * @return a TypeContainer for the long long type
     */
    public Object visit(ASTunsigned_longlong_int node, Object data) {
        return new TypeContainer(typeof(System.Int64));
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTchar_type, Object)
     * @param data unused
     * @return a TypeContainer for the char type
     */
    public Object visit(ASTchar_type node, Object data) {
        CustomAttributeBuilder[] attrs = new CustomAttributeBuilder[] { new WideCharAttribute(false).CreateAttributeBuilder()  };
        TypeContainer containter = new TypeContainer(typeof(System.Char), attrs);
        return containter;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTwide_char_type, Object)
     * @param data unused
     * @return a type type container for the wchar type
     */
    public Object visit(ASTwide_char_type node, Object data) {
        CustomAttributeBuilder[] attrs = new CustomAttributeBuilder[] { new WideCharAttribute(true).CreateAttributeBuilder() };
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
        CustomAttributeBuilder[] attrs = new CustomAttributeBuilder[] { new ObjectIdlTypeAttribute(IdlTypeObject.Any).CreateAttributeBuilder() };
        TypeContainer container = new TypeContainer(typeof(System.Object), attrs);
        return container;
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
        // not needed to check if struct is a nested types, because parent type should already be skipped --> code generation for all nested types skipped to
        if (CheckSkip(forSymbol)) { 
            return null; 
        }
        
        TypeBuilder structToCreate = null;
        BuildInfo thisTypeInfo = null;
        TypeAttributes typeAttrs = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Serializable | TypeAttributes.BeforeFieldInit | TypeAttributes.SequentialLayout | TypeAttributes.Sealed;
        if (buildInfo.GetContainterType() == null) {
            // independent dcl
            ModuleBuilder curModBuilder = m_modBuilderManager.GetOrCreateModuleBuilderFor(buildInfo.GetBuildScope());
            String fullyQualName = buildInfo.GetBuildScope().getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());
            structToCreate = curModBuilder.DefineType(fullyQualName, typeAttrs, typeof(System.ValueType),
                                                      new System.Type[] { typeof(IIdlEntity) });
            thisTypeInfo = new BuildInfo(buildInfo.GetBuildScope(), structToCreate);
        } else {
            // nested dcl
            if (buildInfo.GetContainterType().IsClass) {
                String fullyQualName = buildInfo.GetBuildScope().getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());
                structToCreate = buildInfo.GetContainterType().DefineNestedType(fullyQualName, typeAttrs,
                                                                                typeof(System.ValueType),
                                                                                new System.Type[] { typeof(IIdlEntity) });
            } else {
                // only a class can contain nested types --> therefore use another solution than a nested type for container types which are not classes
                Scope nestedScope = GetScopeForNested(buildInfo.GetBuildScope(), forSymbol);
                String fullyQualName = nestedScope.getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());
                ModuleBuilder curModBuilder = m_modBuilderManager.GetOrCreateModuleBuilderFor(nestedScope);
                structToCreate = curModBuilder.DefineType(fullyQualName, typeAttrs, typeof(System.ValueType),
                                                          new System.Type[] { typeof(IIdlEntity) });
            }
            thisTypeInfo = new BuildInfo(buildInfo.GetBuildScope(), structToCreate);
        }

        // add fileds
        node.childrenAccept(this, thisTypeInfo); // let the members add themself to the typeBuilder

        // add IDLStruct attribute
        structToCreate.SetCustomAttribute(new IdlStructAttribute().CreateAttributeBuilder());
        
        // create the type
        Type resultType = structToCreate.CreateType();
        // type must be registered with the type-manager
        m_typeManager.RegisterTypeDefinition(resultType, forSymbol);
        return new TypeContainer(resultType, new CustomAttributeBuilder[0]);
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTmember_list, Object)
     * @param data an instance of buildinfo for the type, which should contain this members
     */
    public Object visit(ASTmember_list node, Object data) {
        node.childrenAccept(this, data); // let the member add itself to the typebuilder
        return null;
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
        // special handling for BoxedValue types --> unbox it
        if (fieldType.GetClsType().IsSubclassOf(typeof(BoxedValueBase))) {
            fieldType = mapBoxedValueTypeToUnboxed(fieldType.GetClsType());
        }
        String[] decl = (String[])node.jjtGetChild(1).jjtAccept(this, info);
        FieldBuilder fieldBuild;
        for (int i = 0; i < decl.Length; i++) {
            fieldBuild = builder.DefineField(decl[i], fieldType.GetClsType(), FieldAttributes.Public);
            // add custom attributes
            for (int j = 0; j < fieldType.GetAttrs().Length; j++) {
                fieldBuild.SetCustomAttribute(fieldType.GetAttrs()[j]);    
            }
        }
        return null;
    }

    #region union unsupported at the moment
    /**
     * @see parser.IDLParserVisitor#visit(ASTunion_type, Object)
     */
    public Object visit(ASTunion_type node, Object data) 
    {
        throw new NotSupportedException("union type is not supported by this compiler");
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTswitch_type_spec, Object)
     */
    public Object visit(ASTswitch_type_spec node, Object data) {
        throw new NotSupportedException("union type is not supported by this compiler");
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTswitch_body, Object)
     */
    public Object visit(ASTswitch_body node, Object data) {
        throw new NotSupportedException("union type is not supported by this compiler");
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTcasex, Object)
     */
    public Object visit(ASTcasex node, Object data) {
        throw new NotSupportedException("union type is not supported by this compiler");
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTcase_label, Object)
     */
    public Object visit(ASTcase_label node, Object data) {
        throw new NotSupportedException("union type is not supported by this compiler");
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTelement_spec, Object)
     */
    public Object visit(ASTelement_spec node, Object data) {
        throw new NotSupportedException("union type is not supported by this compiler");
    }
    #endregion

    /**
     * @see parser.IDLParserVisitor#visit(ASTenum_type, Object)
     * @param data the current buildinfo instance
     */
    public Object visit(ASTenum_type node, Object data) {
        CheckParameterForBuildInfo(data, node);
        BuildInfo buildInfo = (BuildInfo) data;
        Symbol forSymbol = buildInfo.GetBuildScope().getSymbol(node.getIdent());
        // check if type is known from a previous run over a parse tree --> if so: skip
        if (CheckSkip(forSymbol)) { 
            return null; 
        }

        TypeBuilder enumToCreate = null;
        TypeAttributes typeAttrs = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed;
        if (buildInfo.GetContainterType() == null) {
            // independent dcl
            String fullyQualName = buildInfo.GetBuildScope().getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());
            ModuleBuilder curModBuilder = m_modBuilderManager.GetOrCreateModuleBuilderFor(buildInfo.GetBuildScope());
            enumToCreate = curModBuilder.DefineType(fullyQualName, typeAttrs, typeof(System.Enum));
        } else {
            // nested dcl
            if (buildInfo.GetContainterType().IsClass) {
                String fullyQualName = buildInfo.GetBuildScope().getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());
                enumToCreate = buildInfo.GetContainterType().DefineNestedType(fullyQualName, typeAttrs,
                                                                              typeof(System.Enum));
            } else {
                // only a class can contain nested types --> therefore use another solution than a nested type for container types which are not classes
                Scope nestedScope = GetScopeForNested(buildInfo.GetBuildScope(), forSymbol);
                String fullyQualName = nestedScope.getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());
                ModuleBuilder curModBuilder = m_modBuilderManager.GetOrCreateModuleBuilderFor(nestedScope);
                enumToCreate = curModBuilder.DefineType(fullyQualName, typeAttrs, typeof(System.Enum));    
            }
        }
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

        // add IDLEnum attribute
        enumToCreate.SetCustomAttribute(new IdlEnumAttribute().CreateAttributeBuilder());
        
        // create the type
        Type resultType = enumToCreate.CreateType();
        // type must be registered with the type-manager
        m_typeManager.RegisterTypeDefinition(resultType, forSymbol);
        return new TypeContainer(resultType, new CustomAttributeBuilder[0]);
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
        if (node.jjtGetNumChildren() > 1) { 
            throw new NotSupportedException("sequence with a bound not supported by this compiler"); 
        }
        Node elemTypeNode = node.jjtGetChild(0);
        Debug.WriteLine("determine element type of IDLSequence");
        TypeContainer elemType = (TypeContainer)elemTypeNode.jjtAccept(this, data);
        Debug.WriteLine("seq type determined: " + elemType.GetClsType());
        // create CLS array type with the help of GetType(), otherwise not possible
        Type arrayType;
        if (elemType.GetClsType() is TypeBuilder) {
            Module declModule = ((TypeBuilder)elemType.GetClsType()).Module;
            Debug.WriteLine("get-elem-Type: " + declModule.GetType(elemType.GetClsType().FullName));
            arrayType = declModule.GetType(elemType.GetClsType().FullName + "[]"); // not nice, better solution ?
        } else {
            Assembly declAssembly = elemType.GetClsType().Assembly;
            Debug.WriteLine("decl-Assembly: " + declAssembly);
            arrayType = declAssembly.GetType(elemType.GetClsType().FullName + "[]"); // not nice, better solution ?
        }
        
        Debug.WriteLine("created array type: " + arrayType);        
        TypeContainer result = new TypeContainer(arrayType, 
                                                 new CustomAttributeBuilder[] { new IdlSequenceAttribute().CreateAttributeBuilder() } );
        return result;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTstring_type, Object)
     * @param data unsed
     */
    public Object visit(ASTstring_type node, Object data) {
        CustomAttributeBuilder[] attrs = new CustomAttributeBuilder[] { new StringValueAttribute().CreateAttributeBuilder(), new WideCharAttribute(false).CreateAttributeBuilder() };
        TypeContainer containter = new TypeContainer(typeof(System.String), attrs);
        return containter;
    }
    
    /**
     * @see parser.IDLParserVisitor#visit(ASTwide_string_type, Object)
     * @param data unsed
     * @return a TypeContainer for the wideString-Type
     */
    public Object visit(ASTwide_string_type node, Object data) {
        CustomAttributeBuilder[] attrs = new CustomAttributeBuilder[] { new StringValueAttribute().CreateAttributeBuilder(), new WideCharAttribute(true).CreateAttributeBuilder() };
        TypeContainer containter = new TypeContainer(typeof(System.String), attrs);
        return containter;
    }

    #region array unsupported at the moment
    /**
     * @see parser.IDLParserVisitor#visit(ASTarray_declarator, Object)
     */
    public Object visit(ASTarray_declarator node, Object data) {
        throw new NotSupportedException("array type is not supported by this compiler");
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTfixed_array_size, Object)
     */
    public Object visit(ASTfixed_array_size node, Object data) {
        throw new NotSupportedException("union type is not supported by this compiler");
    }
    #endregion

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
        // special handling for BoxedValue types --> unbox it
        if (propType.GetClsType().IsSubclassOf(typeof(BoxedValueBase))) {
            propType = mapBoxedValueTypeToUnboxed(propType.GetClsType());
        }
        Type propTypeCls = GetTypeFromTypeContainer(propType);
        PropertyBuilder propBuild;
        for (int i = 1; i < node.jjtGetNumChildren(); i++) {
            ASTsimple_declarator simpleDecl = (ASTsimple_declarator) node.jjtGetChild(i);
            String propName = IdlNaming.MapIdlNameToClsName(simpleDecl.getIdent());
            propBuild = builder.DefineProperty(propName, PropertyAttributes.None, 
                                               propTypeCls, System.Type.EmptyTypes);
            // set the methods for the property
            MethodBuilder getAccessor = builder.DefineMethod("__get_" + propName, 
                                                             MethodAttributes.Virtual | MethodAttributes.Abstract | MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, 
                                                             propTypeCls, System.Type.EmptyTypes);
            propBuild.SetGetMethod(getAccessor);
            MethodBuilder setAccessor = null;
            if (!(node.isReadOnly())) {
                setAccessor = builder.DefineMethod("__set_" + propName, 
                                                   MethodAttributes.Virtual | MethodAttributes.Abstract | MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, 
                                                   null, new System.Type[] { propTypeCls });
                propBuild.SetSetMethod(setAccessor);
            }
            
            ParameterBuilder retParamGet = CreateParamBuilderForRetParam(getAccessor);
            ParameterBuilder valParam = null;
            if (setAccessor != null) { 
                valParam = setAccessor.DefineParameter(1, ParameterAttributes.None, "value"); 
            }
            // add custom attributes
            for (int j = 0; j < propType.GetAttrs().Length; j++) {
                propBuild.SetCustomAttribute(propType.GetAttrs()[j]);    
                
                retParamGet.SetCustomAttribute(propType.GetAttrs()[j]);
                if (setAccessor != null) {
                    valParam.SetCustomAttribute(propType.GetAttrs()[j]);
                }
            }
            
        }
        
        return null;
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
        if (CheckSkip(forSymbol)) { 
            return null; 
        }

        TypeBuilder exceptToCreate = null;
        BuildInfo thisTypeInfo = null;
        
        if (buildInfo.GetContainterType() == null) {
            // independent dcl
            String fullyQualName = buildInfo.GetBuildScope().getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());
            ModuleBuilder curModBuilder = m_modBuilderManager.GetOrCreateModuleBuilderFor(buildInfo.GetBuildScope());
            exceptToCreate = curModBuilder.DefineType(fullyQualName, 
                                                      TypeAttributes.Class | TypeAttributes.Public, 
                                                      typeof(AbstractUserException));
            thisTypeInfo = new BuildInfo(buildInfo.GetBuildScope(), exceptToCreate);
        } else {
            // nested dcl
            if (buildInfo.GetContainterType().IsClass) {
                String fullyQualName = buildInfo.GetBuildScope().getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());                
                exceptToCreate = buildInfo.GetContainterType().DefineNestedType(fullyQualName, 
                                                                                TypeAttributes.Class | TypeAttributes.NestedPublic, 
                                                                                typeof(AbstractUserException));
            } else {
                // only a class can contain nested types --> therefore use another solution than a nested type for container types which are not classes
                Scope nestedScope = GetScopeForNested(buildInfo.GetBuildScope(), forSymbol);
                String fullyQualName = nestedScope.getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());
                ModuleBuilder curModBuilder = m_modBuilderManager.GetOrCreateModuleBuilderFor(nestedScope);
                exceptToCreate = curModBuilder.DefineType(fullyQualName, 
                                                          TypeAttributes.Class | TypeAttributes.Public,
                                                          typeof(AbstractUserException));                
            }
            thisTypeInfo = new BuildInfo(buildInfo.GetBuildScope(), exceptToCreate);
        }
        String repId = GetRepIdForException(forSymbol);
        AddRepIdAttribute(exceptToCreate, repId);

        // add fileds ...
        node.childrenAccept(this, thisTypeInfo); // let the members add themself to the typeBuilder
        
        // create the type
        Type resultType = exceptToCreate.CreateType();
        // type must be registered with the type-manager
        m_typeManager.RegisterTypeDefinition(resultType, forSymbol);
        return null;
    }

    /** generates a rep-id for a CLS exception class
     *  @param forSymbol the symbol of the exception */
    private String GetRepIdForException(Symbol forSymbol) {
        System.Collections.Stack scopeStack = new System.Collections.Stack();
        Scope currentScope = forSymbol.getDeclaredIn();
        while (currentScope != null) {
            if (!currentScope.getScopeName().Equals("")) {
                scopeStack.Push(currentScope.getScopeName());
            }
            currentScope = currentScope.getParentScope();
        }
        String repId = "IDL:";
        while (!(scopeStack.Count == 0)) {
            String currentScopeName = (String) scopeStack.Pop();
            repId += currentScopeName + "/";
        }
        repId += forSymbol.getSymbolName();
        repId += ":1.0";
        return repId;
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
        // special handling for BoxedValue types --> unbox it
        if (returnType.GetClsType().IsSubclassOf(typeof(BoxedValueBase))) {
            returnType = mapBoxedValueTypeToUnboxed(returnType.GetClsType());
        }
        // parameters
        ParameterSpec[] parameters = (ParameterSpec[])node.jjtGetChild(1).jjtAccept(this, buildInfo);
        Type[] paramTypes = new Type[parameters.Length];
        for (int i = 0; i < parameters.Length; i++) { 
            paramTypes[i] = GetParamType(parameters[i]); 
        }
        // name
        String methodName = IdlNaming.MapIdlNameToClsName(node.getIdent());
        // ready to create method
        TypeBuilder typeAtBuild = buildInfo.GetContainterType();
        MethodBuilder methodBuild = typeAtBuild.DefineMethod(methodName,  
                                                             MethodAttributes.Virtual | MethodAttributes.Abstract | MethodAttributes.Public | MethodAttributes.HideBySig,
                                                             GetTypeFromTypeContainer(returnType), 
                                                             paramTypes);
        // define the paramter-names / attributes
        for (int i = 0; i < parameters.Length; i++) {
            DefineParamter(methodBuild, parameters[i], i+1);
        }
        // add custom attributes for the return type
        ParameterBuilder paramBuild = CreateParamBuilderForRetParam(methodBuild);
        for (int i = 0; i < returnType.GetAttrs().Length; i++) {
            paramBuild.SetCustomAttribute(returnType.GetAttrs()[i]);
        }
        return null;
    }

    /** retrieve the correct type out of ParameterSpec. Consider parameter-direction too */
    private Type GetParamType(ParameterSpec spec) {
        TypeContainer specType = spec.GetParamType();
    	// special handling for BoxedValue types --> unbox it
        if (specType.GetClsType().IsSubclassOf(typeof(BoxedValueBase))) {
            specType = mapBoxedValueTypeToUnboxed(specType.GetClsType());
        }

    	Type clsType = GetTypeFromTypeContainer(specType);
    	Type resultType;
    	// get correct type for param direction:
        if (spec.IsIn()) {
            resultType = clsType;
        } else { // out or inout parameter
            // need a type which represents a reference to the parametertype
            Assembly declAssembly = clsType.Assembly;
            resultType = declAssembly.GetType(clsType.FullName + "&"); // not nice, better solution ?
        }
        return resultType;
    }
    
    /** 
     * retrieves the cls type out of the type container: considers custom mapping.
     **/
    private Type GetTypeFromTypeContainer(TypeContainer specType) {        
    	Type clsType = specType.GetClsType();
    	// check for custom Mapping here:
    	CompilerMappingPlugin plugin = CompilerMappingPlugin.GetSingleton();
    	if (plugin.IsCustomMappingPresentForIdl(clsType.FullName)) {
    	    clsType = plugin.GetMappingForIdl(clsType.FullName);
    	}    
    	return clsType;
    }

    private void DefineParamter(MethodBuilder methodBuild, ParameterSpec spec, int paramNr) {
        ParameterAttributes paramAttr = ParameterAttributes.None;
        if (spec.IsOut()) { 
            paramAttr = paramAttr | ParameterAttributes.Out; 
        }
        ParameterBuilder paramBuild = methodBuild.DefineParameter(paramNr, paramAttr, spec.GetPramName());
        // custom attribute spec
        TypeContainer specType = spec.GetParamType();
        // special handling for BoxedValue types --> unbox it
        if (specType.GetClsType().IsSubclassOf(typeof(BoxedValueBase))) {
            specType = mapBoxedValueTypeToUnboxed(specType.GetClsType());
        }
        for (int i = 0; i < specType.GetAttrs().Length; i++) {
            paramBuild.SetCustomAttribute(specType.GetAttrs()[i]);    
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
        int direction = ((ASTparam_attribute) node.jjtGetChild(0)).getParamDir();
        // determine name and type
        TypeContainer paramType = (TypeContainer)node.jjtGetChild(1).jjtAccept(this, data);
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
        return null; // TBD: check if exceptions in raise clause are declared
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTcontext_expr, Object)
     */
    public Object visit(ASTcontext_expr node, Object data) {
        return null; // TBD: ???
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
        CustomAttributeBuilder[] attrs = new CustomAttributeBuilder[] { new ObjectIdlTypeAttribute(IdlTypeObject.ValueBase).CreateAttributeBuilder() };
        TypeContainer container = new TypeContainer(typeof(System.Object), attrs);
        return container;
    }

    /** for method parameters, fields, attributes, the unboxed type for the boxed value type is used. This 
     * method returns the unboxed type and created the correct boxed value attribute
     */
    private TypeContainer mapBoxedValueTypeToUnboxed(Type boxedValueType) {
        Debug.WriteLine("unbox boxed value type");
        if (!(boxedValueType.IsSubclassOf(typeof(BoxedValueBase)))) {
            throw new InternalCompilerException("a boxed value type is expected");
        }
        AttributeExtCollection attrColl = AttributeExtCollection.ConvertToAttributeCollection(boxedValueType.GetCustomAttributes(true));
        if (!(attrColl.IsInCollection(typeof(RepositoryIDAttribute)))) {
            throw new InternalCompilerException("invalid boxed value type created, rep-id missing: " + boxedValueType);
        }
        String boxedValueRepId = ((RepositoryIDAttribute)attrColl.GetAttributeForType(typeof(RepositoryIDAttribute))).Id;
        try {
            Type fullUnboxed = (Type)boxedValueType.InvokeMember(BoxedValueBase.GET_FIRST_NONBOXED_TYPE_METHODNAME,
                                                                 BindingFlags.InvokeMethod | BindingFlags.Public |
                                                                     BindingFlags.NonPublic | BindingFlags.Static |
                                                                     BindingFlags.DeclaredOnly,
                                                                 null, null, new System.Object[0]);
            CustomAttributeBuilder[] attrs = new CustomAttributeBuilder[] { new BoxedValueAttribute(boxedValueRepId).CreateAttributeBuilder() };
            return new TypeContainer(fullUnboxed, attrs);
        } catch (Exception) {
            throw new InternalCompilerException("invalid type found: " + boxedValueType + 
                                                ", static method missing or not callable: " + 
                                                BoxedValueBase.GET_FIRST_NONBOXED_TYPE_METHODNAME);
        }
    }

    #endregion IMethods

}

}