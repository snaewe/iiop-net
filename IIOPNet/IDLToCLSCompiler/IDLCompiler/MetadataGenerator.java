/* MetadataGenerator.java
 * 
 * Project: IIOP.NET
 * IDLToCLSCompiler
 * 
 * WHEN      RESPONSIBLE
 * 14.02.03  Dominic Ullmann (DUL), dul@elca.ch
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

package action;

import java.io.FileOutputStream;
import java.io.PrintWriter;

import parser.*;
import parser.ASTadd_expr;
import parser.ASTand_expr;
import parser.ASTany_type;
import parser.ASTarray_declarator;
import parser.ASTattr_dcl;
import parser.ASTbase_type_spec;
import parser.ASTboolean_type;
import parser.ASTcase_label;
import parser.ASTcasex;
import parser.ASTchar_type;
import parser.ASTcomplex_declarator;
import parser.ASTconst_dcl;
import parser.ASTconst_exp;
import parser.ASTconst_type;
import parser.ASTconstr_type_spec;
import parser.ASTcontext_expr;
import parser.ASTdeclarator;
import parser.ASTdeclarators;
import parser.ASTdefinition;
import parser.ASTelement_spec;
import parser.ASTenum_type;
import parser.ASTenumerator;
import parser.ASTexcept_dcl;
import parser.ASTexport;
import parser.ASTfixed_array_size;
import parser.ASTfloating_pt_type;
import parser.ASTforward_dcl;
import parser.ASTinteger_type;
import parser.ASTinterface_body;
import parser.ASTinterface_dcl;
import parser.ASTinterface_header;
import parser.ASTinterface_inheritance_spec;
import parser.ASTinterface_name;
import parser.ASTinterfacex;
import parser.ASTliteral;
import parser.ASTmember;
import parser.ASTmember_list;
import parser.ASTmodule;
import parser.ASTmult_expr;
import parser.ASToctet_type;
import parser.ASTop_dcl;
import parser.ASTop_type_spec;
import parser.ASTor_expr;
import parser.ASTparam_attribute;
import parser.ASTparam_dcl;
import parser.ASTparam_type_spec;
import parser.ASTparameter_dcls;
import parser.ASTpositive_int_const;
import parser.ASTprimary_expr;
import parser.ASTraises_expr;
import parser.ASTscoped_name;
import parser.ASTsequence_type;
import parser.ASTshift_expr;
import parser.ASTsigned_int;
import parser.ASTsigned_long_int;
import parser.ASTsigned_longlong_int;
import parser.ASTsigned_short_int;
import parser.ASTsimple_declarator;
import parser.ASTsimple_type_spec;
import parser.ASTspecification;
import parser.ASTstring_type;
import parser.ASTstruct_type;
import parser.ASTswitch_body;
import parser.ASTswitch_type_spec;
import parser.ASTtemplate_type_spec;
import parser.ASTtype_dcl;
import parser.ASTtype_declarator;
import parser.ASTtype_spec;
import parser.ASTunary_expr;
import parser.ASTunion_type;
import parser.ASTunsigned_int;
import parser.ASTunsigned_long_int;
import parser.ASTunsigned_short_int;
import parser.ASTvalue;
import parser.ASTvalue_box_decl;
import parser.ASTwide_char_type;
import parser.ASTxor_expr;
import parser.IDLParserVisitor;
import parser.SimpleNode;
import symboltable.Scope;
import symboltable.SymbolTable;
import symboltable.Symbol;
import java.util.LinkedList;

import System.Reflection.*;
import System.Reflection.Emit.*;
import System.Type;
import System.Diagnostics.*;

import Ch.Elca.Iiop.Idl.*;
import Ch.Elca.Iiop.Marshalling.ICustomMarshalled;
import Ch.Elca.Iiop.Util.AttributeExtCollection;
import Ch.Elca.Iiop.AbstractUserException;

/**
 * 
 * @version 
 * @author dul
 * 
 *
 */

public class MetaDataGenerator implements IDLParserVisitor {

    private SymbolTable m_symbolTable;

    private AssemblyBuilder m_asmBuilder;

    private ModuleBuilderManager m_modBuilderManager;

    private TypeManager m_typeManager;

    private String m_targetAsmName;

    /** helper class, to pass information */
    class BuildInfo {

        #region Types
        
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
    class ParameterSpec {
        
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

        public String getPramName() {
            return m_paramName;
        }
        
        public TypeContainer getParamType() {
            return m_paramType;
        }

        public int getParamDirection() {
            return m_direction;
        }

        public boolean isInOut() {
            return (m_direction == ASTparam_attribute.ParamDir_INOUT);
        }

        public boolean isIn() {
            return (m_direction == ASTparam_attribute.ParamDir_IN);
        }

        public boolean isOut() {
            return (m_direction == ASTparam_attribute.ParamDir_OUT);
        }

        #endregion IMethods

    }

    #endregion Types
    #region IFields

    /** reference to one of the internal constructor of class ParameterInfo. Used for assigning custom attributes to the return parameter */
    private ConstructorInfo m_paramBuildConstr;
    /** is the generator initalized for parsing a file */
    private boolean m_initalized = false;

    #endregion IFields
    #region IConstructors

    public MetaDataGenerator(String targetAssemblyName) {
        m_targetAsmName = targetAssemblyName;
        AssemblyName asmname = new AssemblyName();
        asmname.set_Name(targetAssemblyName);
        // define a persistent assembly
        m_asmBuilder = System.Threading.Thread.GetDomain().
            DefineDynamicAssembly(asmname, AssemblyBuilderAccess.RunAndSave);
        // manager for persistent modules
        m_modBuilderManager = new ModuleBuilderManager(m_asmBuilder, targetAssemblyName);
        Type paramBuildType = ParameterBuilder.class.ToType();
        m_paramBuildConstr = paramBuildType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                                                           new Type[] { MethodBuilder.class.ToType(), System.Int32.class.ToType(), ParameterAttributes.class.ToType(), String.class.ToType() }, 
                                                           null);
    }

    #endregion IConstructors
    #region IMethods

    /** ends the build process, after this is called, the Generator is not able to process more files */
    public void SaveAssembly() {
        // save the assembly to disk
        m_asmBuilder.Save(m_targetAsmName + ".dll");
    }

    /** initalize the generator for next source, with using the same target assembly / target modules */
    public void InitalizeForSource(SymbolTable symbolTable) {
        m_symbolTable = symbolTable;
        // helps to find already declared types
        m_typeManager = new TypeManager(m_modBuilderManager);
        // ready for code generation
        m_initalized = true;
    }
    
    /** checks if a type generation can be skipped, because type is already defined in a previous run over a parse tree 
     * this method is used to support runs over more than one parse tree */
    private boolean checkSkip(Symbol forSymbol) {
        // do skip, if type is not known (or only fwd declared) in current run over a parse tree, but is known from a previous run
        if (m_typeManager.IsTypeFullyDeclarded(forSymbol)) { 
            return false; 
        } // already known in this run, do checks if this is a redefinition error

        if (m_typeManager.checkInBuildModulesForType(forSymbol)) { // safe to skip, because type is already fully declared in a previous run
            return true;
        }
        return false;
    }

    /** register a skipped type 
     *  this method is used to support runs over more than one parse tree */
    private void registerSkipped(Symbol forSymbol, boolean fwdDecl) {
        m_typeManager.registerTypeFromBuildModule(forSymbol, fwdDecl);
        // register nested types too, if present
        if (!fwdDecl) { 
            Scope typeScope = forSymbol.getDeclaredIn().getChildScope(forSymbol.getSymbolName());
            if (typeScope != null) {  // e.g boxed value types doesn't open a scope for it's member
                registerSkippedNestedTypes(typeScope, forSymbol); 
            }
        }
    }

        /** if a type, which contains nested types is skipped, the nested types must be also added to the type-table*/
    private void registerSkippedNestedTypes(Scope nesterScope, Symbol nesterSymbol) {
                
        // symbolDefinition : fetch type and add
        java.util.Enumeration symEnum = nesterScope.getSymbolEnum();
        while (symEnum.hasMoreElements()) {
            Symbol current = (Symbol) symEnum.nextElement();
            if (current instanceof symboltable.SymbolDefinition) {
                // the already defined type in a previous run can be found in the correct build-module
                Scope nested = getScopeForNested(nesterScope, current);
                Symbol newSymbol = nested.getSymbol(current.getSymbolName());
                Type defined = m_typeManager.getTypeFromBuildModule(newSymbol);
                // the Type resolution will work with the normal symbol --> therefore add type for this
                if (defined == null) { 
                    throw new RuntimeException("internal exception, while adding nested type for a type from a previous run: " + current.getSymbolName()); 
                }
                m_typeManager.registerTypeDefinition(defined, current);            
            }
        }

        // typedefs: do again
        // TBD
    }

    /** create or retrieve a Scope for nested IDL-types, which may not be nested inside the mapped CLS type of the container. */
    private Scope getScopeForNested(Scope containerScope, Symbol createdFor) {
        Scope parentOfContainer = containerScope.getParentScope();
        String nestedScopeName = containerScope.getScopeName() + "_package";
        if (!(parentOfContainer.containsChildScope(nestedScopeName))) {
            parentOfContainer.addChildScope(new Scope(nestedScopeName, parentOfContainer));
        }
        Scope nestedScope = parentOfContainer.getChildScope(nestedScopeName);
        nestedScope.addSymbol(createdFor.getSymbolName());
        return nestedScope;
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
            throw new RuntimeException("initalize not called"); 
        }
        Scope topScope = m_symbolTable.getTopScope();
        BuildInfo info = new BuildInfo(topScope, null);
        node.childrenAccept(this, info);
        m_initalized = false; // this file is finished
        if (!m_typeManager.AllTypesDefined()) {
            throw new RuntimeException("not all types fully defined");
        }
        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTdefinition, Object)
     * @param data an instance of buildinfo is expected
     */
    public Object visit(ASTdefinition node, Object data) {
        checkParameterForBuildInfo(data, node);
        node.childrenAccept(this, data);
        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTmodule, Object)
     * @param data an instance of buildInfo is expected
     */
    public Object visit(ASTmodule node, Object data) {
        checkParameterForBuildInfo(data, node);
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

    private void addRepIdAttribute(TypeBuilder typebuild, String repId) {
        if (repId != null) {
            CustomAttributeBuilder repIdAttrBuilder = new RepositoryIDAttribute(repId).CreateAttributeBuilder();
            typebuild.SetCustomAttribute(repIdAttrBuilder);
        }
    }

    /** handles the declaration for the interface definition / fwd declaration
     * @return the TypeBuilder for this interface
     */
    private TypeBuilder createOrGetInterfaceDcl(String fullyQualName, System.Type[] interfaces, boolean isAbstract,
                                                Symbol forSymbol, String repId, ModuleBuilder modBuilder) {
        TypeBuilder interfaceToBuild;
        if (!m_typeManager.IsFwdDeclared(forSymbol)) {
            Trace.WriteLine("generating code for interface: " + fullyQualName);
            interfaceToBuild = modBuilder.DefineType(fullyQualName, TypeAttributes.Interface | TypeAttributes.Public | TypeAttributes.Abstract,
                                                     null, interfaces);
            // add InterfaceTypeAttribute
            IdlTypeInterface ifType = IdlTypeInterface.ConcreteInterface;
            if (isAbstract) { ifType = IdlTypeInterface.AbstractInterface; }
            // add interface type
            CustomAttributeBuilder interfaceTypeAttrBuilder = new InterfaceTypeAttribute(ifType).CreateAttributeBuilder();
            interfaceToBuild.SetCustomAttribute(interfaceTypeAttrBuilder);
            // add repository ID
            addRepIdAttribute(interfaceToBuild, repId);
            interfaceToBuild.AddInterfaceImplementation(IIdlEntity.class.ToType());
            // register type with type manager as not fully declared
            m_typeManager.RegisterTypeFwdDecl(interfaceToBuild, forSymbol);    
        } else {
            // get incomplete type
            Trace.WriteLine("complete interface: " + fullyQualName);
            interfaceToBuild = (TypeBuilder)(m_typeManager.GetKnownType(forSymbol).getCLSType());
            // add inheritance relationship:
            for (int i = 0; i < interfaces.length; i++) {
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
        checkParameterForBuildInfo(data, node);
        // data contains the scope, this interface is declared in
        Scope enclosingScope = ((BuildInfo) data).GetBuildScope();
        
        
        // an IDL concrete interface
        // get the header
        ASTinterface_header header = (ASTinterface_header)node.jjtGetChild(0);
        Symbol forSymbol = enclosingScope.getSymbol(header.getIdent());
        // check if a type declaration exists from a previous run
        if (checkSkip(forSymbol)) { 
            registerSkipped(forSymbol, false); 
            return null; 
        }

        // retrieve first types for the inherited
        System.Type[] interfaces = (System.Type[])header.jjtAccept(this, data);
        String fullyQualName = enclosingScope.getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());

        ModuleBuilder curModBuilder = m_modBuilderManager.GetOrCreateModuleBuilderFor(enclosingScope);
        TypeBuilder interfaceToBuild = createOrGetInterfaceDcl(fullyQualName, interfaces, header.isAbstract(), 
                                                               forSymbol, enclosingScope.getRepositoryIdFor(header.getIdent()),
                                                               curModBuilder);

        // generate body
        ASTinterface_body body = (ASTinterface_body)node.jjtGetChild(1);
        BuildInfo buildInfo = new BuildInfo(enclosingScope.getChildScope(forSymbol.getSymbolName()), interfaceToBuild);
        body.jjtAccept(this, buildInfo);
    
        // create the type
        Type resultType = interfaceToBuild.CreateType();
        m_typeManager.replaceFwdDeclWithFullDecl(resultType, forSymbol);
        return null;
    }
    
    /**
     * @see parser.IDLParserVisitor#visit(ASTforward_dcl, Object)
     * @param data the buildinfo of the scope, this type should be declared in
     */
    public Object visit(ASTforward_dcl node, Object data) {
        checkParameterForBuildInfo(data, node);
        Scope enclosingScope = ((BuildInfo) data).GetBuildScope();
        // create only the type-builder, but don't call createType()
        Symbol forSymbol = enclosingScope.getSymbol(node.getIdent());
        // check if type is known from a previous run over a parse tree --> if so: skip
        if (checkSkip(forSymbol)) { 
            registerSkipped(forSymbol, true); 
            return null; 
        }
        
        String fullyQualName = enclosingScope.getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());
        if (!(m_typeManager.IsTypeDeclarded(forSymbol))) { // ignore fwd-decl if type is already declared, if not generate type for fwd decl
            ModuleBuilder curModBuilder = m_modBuilderManager.GetOrCreateModuleBuilderFor(enclosingScope);
            // it's no problem to add later on interfaces this type should implement with AddInterfaceImplementation,
            // here: specify no interface inheritance, because not known at this point
            createOrGetInterfaceDcl(fullyQualName, Type.EmptyTypes, node.isAbstract(), 
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

    
    /** get the types for the scoped names specified in an inheritance relationship
     * @param data the buildinfo of the container of the type having this inheritance relationship
     */
    private Type[] parseInheritanceRelation(SimpleNode node, BuildInfo data) {
        Type[] result = new Type[node.jjtGetNumChildren()];
        for (int i = 0; i < node.jjtGetNumChildren(); i++) {
            // get symbol
            Symbol sym = (Symbol)(node.jjtGetChild(i).jjtAccept(this, data)); // accept interface_name
            // get Type
            TypeContainer resultType = m_typeManager.GetKnownType(sym);
            if (resultType == null) {
                // this is an error: type must be created before it is inherited from
                throw new RuntimeException("type not seen before in inheritance spec");
            } else if (m_typeManager.IsFwdDeclared(sym)) {
                // this is an error: can't inherit from a fwd declared type
                throw new RuntimeException("type only fwd declared, but for inheritance full definition is needed");
            }
            result[i] = resultType.getCLSType();
        }
        return result;        
    }
    
    
    /**
     * @see parser.IDLParserVisitor#visit(ASTinterface_inheritance_spec, Object)
     * @param data the buildinfo of the container for this interface (e.g. a module)
     * @return an Array of the types the interface inherits from
     */
    public Object visit(ASTinterface_inheritance_spec node, Object data) {
        checkParameterForBuildInfo(data, node);
        Type[] result = parseInheritanceRelation(node, (BuildInfo)data);
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
        checkParameterForBuildInfo(data, node);
        LinkedList parts = node.getNameParts();
        Scope currentScope = ((BuildInfo) data).GetBuildScope();
        if (node.hasFileScope()) { currentScope = m_symbolTable.getTopScope(); }
        for (int i = 0; i < parts.size() - 1; i++) {
            // resolve scopes
            currentScope = currentScope.getChildScope((String)parts.get(i));
            if (currentScope == null) { 
                throw new RuntimeException("scope resolving error, subscope " + parts.get(i) + " not found in scope "); 
            }
        }
        // resolve symbol
        Symbol sym = currentScope.getSymbol((String)parts.getLast());
        // if name is unqualified search in outer scopes
        // TBD: in inherited scopes as described in CORBA2.3 3.15.2
        if ((sym == null) && (parts.size() == 1)) {
            sym = searchSymbolInEnclosingScopes(currentScope, (String)parts.getLast());    
        }

        if (sym == null) { 
            throw new RuntimeException("scoped name not resolvable: " + node.getScopedName()); 
        }
        return sym;
    }

    private Symbol searchSymbolInEnclosingScopes(Scope startScope, String symbolName) {
        Symbol result = null;
        Scope currentScope = startScope;
        while ((result == null) && (currentScope != null)) {
            result = currentScope.getSymbol(symbolName);
            currentScope = currentScope.getParentScope();
        }
        return result;
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
    private TypeBuilder createOrGetValueDcl(String fullyQualName, System.Type[] interfaces, 
                                            System.Type parent, boolean isAbstract, Symbol forSymbol, 
                                            String repId, ModuleBuilder modBuilder) {
        TypeBuilder valueToBuild;
        if (!m_typeManager.IsFwdDeclared(forSymbol)) {
            Trace.WriteLine("generating code for value type: " + fullyQualName);
            TypeAttributes attrs = TypeAttributes.Public | TypeAttributes.Abstract;
            if (isAbstract) {
                attrs |= TypeAttributes.Interface;
                if (parent != null) { throw new RuntimeException("not possible for an abstract value type to inherit from a concrete one"); }
            } else {
                attrs |= TypeAttributes.Class;
            }
            valueToBuild = modBuilder.DefineType(fullyQualName, attrs, parent, interfaces);
            // add repository ID
            addRepIdAttribute(valueToBuild, repId);
            if (isAbstract) {
                // add InterfaceTypeAttribute
                IdlTypeInterface ifType = IdlTypeInterface.AbstractValueType;
                CustomAttributeBuilder interfaceTypeAttrBuilder = new InterfaceTypeAttribute(ifType).CreateAttributeBuilder();
                valueToBuild.SetCustomAttribute(interfaceTypeAttrBuilder);
            }
            valueToBuild.AddInterfaceImplementation(IIdlEntity.class.ToType()); // implement IDLEntity
            // register type with type manager as not fully declared
            m_typeManager.RegisterTypeFwdDecl(valueToBuild, forSymbol);    
        } else {
            // get incomplete type
            Trace.WriteLine("complete valuetype: " + fullyQualName);
            valueToBuild = (TypeBuilder)m_typeManager.GetKnownType(forSymbol).getCLSType();
            // add inheritance relationship:
            for (int i = 0; i < interfaces.length; i++) {
                valueToBuild.AddInterfaceImplementation(interfaces[i]);
            }
            if (parent != null) { valueToBuild.SetParent(parent); }
        }
        // add abstract methods for all interface methods, a class inherit from (only if valueToBuild is a class an not an interface)
        addMethodAbstractDeclToClassForIf(valueToBuild, interfaces);
        return valueToBuild;
    }

    /** add abstract methods for all implemented interfaces to the abstract class */
    private void addMethodAbstractDeclToClassForIf(TypeBuilder classBuilder, System.Type[] interfaces) {
        if (!(classBuilder.get_IsClass())) { return; } // only needed for classes
        for (int i = 0; i < interfaces.length; i++) {
            Type ifType = interfaces[i];    
            MethodInfo[] methods = ifType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            for (int j = 0; j < methods.length; j++) {
                // normal parameters
                ParameterInfo[] params = methods[j].GetParameters();
                System.Type[] paramTypes = new System.Type[params.length];
                for (int k = 0; k < params.length; k++) {
                    paramTypes[k] = params[k].get_ParameterType();
                }
                MethodBuilder method = classBuilder.DefineMethod(methods[j].get_Name(), 
                                                                 MethodAttributes.Abstract | MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig, 
                                                                 methods[j].get_ReturnType(), paramTypes);
                for (int k = 0; k < params.length; k++) {
                    copyParamAttrs(method, params[k]);
                }
                // return parameter
                Object[] retAttrs = methods[j].get_ReturnTypeCustomAttributes().GetCustomAttributes(false);
                // add custom attributes for the return type
                ParameterBuilder paramBuild = createParamBuilderForRetParam(method);
                for (int k = 0; k < retAttrs.length; k++) {
                    if (retAttrs[i] instanceof IIdlAttribute) {
                        CustomAttributeBuilder attrBuilder = ((IIdlAttribute) retAttrs[i]).CreateAttributeBuilder();
                        paramBuild.SetCustomAttribute(attrBuilder);    
                    }
                }
            }
        }
    }

    private void copyParamAttrs(MethodBuilder methodBuild, ParameterInfo info) {
        ParameterAttributes paramAttr = ParameterAttributes.None;
        if (info.get_IsOut()) { paramAttr = paramAttr | ParameterAttributes.Out; }
        ParameterBuilder paramBuild = methodBuild.DefineParameter(info.get_Position() + 1, 
                                                                  paramAttr, info.get_Name());
        // custom attributes
        System.Object[] attrs = info.GetCustomAttributes(false);
        for (int i = 0; i < attrs.length; i++) {
            if (attrs[i] instanceof IIdlAttribute) {
                CustomAttributeBuilder attrBuilder = ((IIdlAttribute) attrs[i]).CreateAttributeBuilder();
                paramBuild.SetCustomAttribute(attrBuilder);    
            }
        }
    }

    private void addSerializableAttribute(TypeBuilder typebuild) {
        Type attrType = System.SerializableAttribute.class.ToType();
        ConstructorInfo attrConstr = attrType.GetConstructor(Type.EmptyTypes);
        CustomAttributeBuilder serAttr = new CustomAttributeBuilder(attrConstr, new Object[0]);    
        typebuild.SetCustomAttribute(serAttr);
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTvalue_decl, Object)
     * @param data an instance of the type buildinfo specifing the scope, this value is declared in
     */
    public Object visit(ASTvalue_decl node, Object data) {
        checkParameterForBuildInfo(data, node);
        // data contains the scope, this value type is declared in
        Scope enclosingScope = ((BuildInfo) data).GetBuildScope();
        // an IDL concrete value type
        // get the header
        ASTvalue_header header = (ASTvalue_header)node.jjtGetChild(0);
        
        Symbol forSymbol = enclosingScope.getSymbol(header.getIdent());
        // check if type is known from a previous run over a parse tree --> if so: skip
        if (checkSkip(forSymbol)) { registerSkipped(forSymbol, false); return null; }
        
        // retrieve first types for the inherited
        System.Type[] inheritFrom = (System.Type[])header.jjtAccept(this, data);

        // check is custom:
        if (header.isCustom()) {    
            System.Type[] newInherit = new System.Type[inheritFrom.length + 1];
            System.arraycopy(inheritFrom, 0, newInherit, 0, inheritFrom.length);
            newInherit[inheritFrom.length] = ICustomMarshalled.class.ToType();
            inheritFrom = newInherit;
        }

        Type baseClass = null;
        if ((inheritFrom.length > 0) && (inheritFrom[0].get_IsClass())) {
            // only the first entry may be a class for a concrete value type: multiple inheritance is not allowed for concrete value types, the value type from which is inherited from must be first in inheritance list, 3.8.5 in CORBA 2.3.1 spec
            baseClass = inheritFrom[0];
            Type[] tmp = new Type[inheritFrom.length-1];
            System.arraycopy(inheritFrom, 1, tmp, 0, tmp.length);
            inheritFrom = tmp;
        }

        String fullyQualName = enclosingScope.getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());
        ModuleBuilder curModBuilder = m_modBuilderManager.GetOrCreateModuleBuilderFor(enclosingScope);
        TypeBuilder valueToBuild = createOrGetValueDcl(fullyQualName, inheritFrom, baseClass, 
                                                       false, forSymbol, 
                                                       enclosingScope.getRepositoryIdFor(header.getIdent()), 
                                                       curModBuilder);
        
        // add implementation class attribute
        valueToBuild.SetCustomAttribute(new ImplClassAttribute(fullyQualName + "Impl").CreateAttributeBuilder());
        // add serializable attribute
        addSerializableAttribute(valueToBuild);

        // generate elements
        BuildInfo buildInfo = new BuildInfo(enclosingScope, valueToBuild);
        for (int i = 1; i < node.jjtGetNumChildren(); i++) { // for all value_element children
            ASTvalue_element elem = (ASTvalue_element)node.jjtGetChild(i);
            elem.jjtAccept(this, buildInfo);    
        }

        // finally create the type
        Type resultType = valueToBuild.CreateType();
        m_typeManager.replaceFwdDeclWithFullDecl(resultType, forSymbol);
        return null;
    }
    
    /**
     * @see parser.IDLParserVisitor#visit(ASTvalue_abs_decl, Object)
     */
    public Object visit(ASTvalue_abs_decl node, Object data) {
        checkParameterForBuildInfo(data, node);
        // data contains the scope, this value type is declared in
        Scope enclosingScope = ((BuildInfo) data).GetBuildScope();
        // an IDL abstract value type
        
        Symbol forSymbol = enclosingScope.getSymbol(node.getIdent());
        // check if type is known from a previous run over a parse tree --> if so: skip
        if (checkSkip(forSymbol)) { registerSkipped(forSymbol, false); return null; }

        Type[] interfaces = parseValueInheritSpec(node, (BuildInfo) data);
        if ((interfaces.length > 0) && (interfaces[0].get_IsClass())) { 
            throw new RuntimeException("invalid abstract value type, can only inherit from abstract value types, but not from: " + interfaces[0]); 
        }
        int bodyNodeIndex = 0;
        for (int i = 0; i < node.jjtGetNumChildren(); i++) {
            if (!((node.jjtGetChild(i) instanceof ASTvalue_base_inheritance_spec) || (node.jjtGetChild(i) instanceof ASTvalue_support_inheritance_spec))) {
                bodyNodeIndex = i;
                break;
            }
        }

        String fullyQualName = enclosingScope.getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());
        ModuleBuilder curModBuilder = m_modBuilderManager.GetOrCreateModuleBuilderFor(enclosingScope);
        TypeBuilder valueToBuild = createOrGetValueDcl(fullyQualName, interfaces, null,
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
        m_typeManager.replaceFwdDeclWithFullDecl(resultType, forSymbol);
        return null;
    }
    
    /**
     * @see parser.idlparservisitor#visit(ASTvalue_box_decl, Object)
     * @param data the current buildinfo
     */
    public Object visit(ASTvalue_box_decl node, Object data) {
        checkParameterForBuildInfo(data, node);
        Scope enclosingScope = ((BuildInfo) data).GetBuildScope();
        Symbol forSymbol = enclosingScope.getSymbol(node.getIdent());
        // check if type is known from a previous run over a parse tree --> if so: skip
        if (checkSkip(forSymbol)) { 
            registerSkipped(forSymbol, false); 
            return null;
        }
        
        Debug.WriteLine("begin boxed value type: " + node.getIdent());
        String fullyQualName = enclosingScope.getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());
        ModuleBuilder curModBuilder = m_modBuilderManager.GetOrCreateModuleBuilderFor(enclosingScope);
        // get the boxed type
        TypeContainer boxedType = (TypeContainer)node.jjtGetChild(0).jjtAccept(this, data);
        Trace.WriteLine("generating code for boxed value type: " + fullyQualName);
        BoxedValueTypeGenerator boxedValueGen = new BoxedValueTypeGenerator();
        TypeBuilder resultType = boxedValueGen.CreateBoxedType(boxedType.getCLSType(), curModBuilder,
                                                               fullyQualName, boxedType.getAttrs());
        addRepIdAttribute(resultType, enclosingScope.getRepositoryIdFor(node.getIdent()));
        resultType.AddInterfaceImplementation(IIdlEntity.class.ToType());
        Type result = resultType.CreateType();
        m_typeManager.registerTypeDefinition(result, forSymbol);        
        return null;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTvalue_forward_decl, Object)
     * @param data the buildinfo of the container
     */
    public Object visit(ASTvalue_forward_decl node, Object data) {
        checkParameterForBuildInfo(data, node);
        // is possible to do with reflection emit, because interface and class inheritance can be specified later on with setParent() and AddInterfaceImplementation()
        Scope enclosingScope = ((BuildInfo) data).GetBuildScope();
        Symbol forSymbol = enclosingScope.getSymbol(node.getIdent());
        // check if type is known from a previous run over a parse tree --> if so: skip
        if (checkSkip(forSymbol)) { registerSkipped(forSymbol, true); return null; }
        
        // create only the type-builder, but don't call createType()
        String fullyQualName = enclosingScope.getFullyQualifiedNameForSymbol(node.getIdent());
        if (!(m_typeManager.IsTypeDeclarded(forSymbol))) { // if the full type declaration already exists, ignore fwd decl
            ModuleBuilder curModBuilder = m_modBuilderManager.GetOrCreateModuleBuilderFor(enclosingScope);
            // it's no problem to add later on interfaces this type should implement and the base class this type should inherit from with AddInterfaceImplementation / set parent
            // here: specify no inheritance, because not known at this point
            createOrGetValueDcl(fullyQualName, Type.EmptyTypes, null, node.isAbstract(),
                                forSymbol, enclosingScope.getRepositoryIdFor(node.getIdent()),
                                curModBuilder);        
        }
        return null;
    }

    
    /** search in a value_header_node / abs_value_node for inheritance information and parse it
     * @param parentOfPossibleInhNode the node possibly containing value inheritance nodes
     */
    public Type[] parseValueInheritSpec(Node parentOfPossibleInhNode, BuildInfo data) {
        Type[] result = new Type[0];
        if (parentOfPossibleInhNode.jjtGetNumChildren() > 0) {
            if (parentOfPossibleInhNode.jjtGetChild(0) instanceof ASTvalue_base_inheritance_spec) {
                ASTvalue_base_inheritance_spec inheritSpec = (ASTvalue_base_inheritance_spec) parentOfPossibleInhNode.jjtGetChild(0);
                result = (Type[])inheritSpec.jjtAccept(this, data);
            } else if (parentOfPossibleInhNode.jjtGetChild(0) instanceof ASTvalue_support_inheritance_spec){
                ASTvalue_support_inheritance_spec inheritSpec = (ASTvalue_support_inheritance_spec) parentOfPossibleInhNode.jjtGetChild(0);
                result = (Type[])inheritSpec.jjtAccept(this, data);    
            }
        }
        if ((parentOfPossibleInhNode.jjtGetNumChildren() > 1) && (parentOfPossibleInhNode.jjtGetChild(1) instanceof ASTvalue_support_inheritance_spec)) {
            // append the support inheritance spec to the rest
            ASTvalue_support_inheritance_spec inheritSpec = (ASTvalue_support_inheritance_spec) parentOfPossibleInhNode.jjtGetChild(1);
            Type[] supportTypes = (Type[])inheritSpec.jjtAccept(this, data);
            Type[] resultCrt = new Type[result.length + supportTypes.length];
            System.arraycopy(result, 0, resultCrt, 0, result.length);
            System.arraycopy(supportTypes, 0, resultCrt, result.length, supportTypes.length);
            result = resultCrt;
        }
        return result;
    }
    
    
    /**
     * @see parser.IDLParserVisitor#visit(ASTvalue_header, Object)
     * @param data the buildinfo of the container for this valuetype
     */
    public Object visit(ASTvalue_header node, Object data) {
        checkParameterForBuildInfo(data, node);
        return parseValueInheritSpec(node, (BuildInfo) data);
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
        checkParameterForBuildInfo(data, node);
        BuildInfo info = (BuildInfo) data;
        TypeBuilder builder = info.GetContainterType();
        ASTtype_spec typeSpecNode = (ASTtype_spec)node.jjtGetChild(0);
        TypeContainer fieldType = (TypeContainer)typeSpecNode.jjtAccept(this, info);
        // special handling for BoxedValue types --> unbox it
        if (fieldType.getCLSType().IsSubclassOf(BoxedValueBase.class.ToType())) {
            fieldType = mapBoxedValueTypeToUnboxed(fieldType.getCLSType());
        }
        String[] decl = (String[])node.jjtGetChild(1).jjtAccept(this, data);
        FieldBuilder fieldBuild;
        for (int i = 0; i < decl.length; i++) {
            if (node.isPrivate()) { // map to protected field
                String privateName = decl[i];
                // compensate a problem in the java rmi compiler, which can produce illegal idl:
                // it produces idl-files with name clashes if a method getx() and a field x exists
                if (!privateName.startsWith("m_")) { privateName = "m_" + privateName; }
                fieldBuild = builder.DefineField(privateName, fieldType.getCLSType(), FieldAttributes.Family);
            } else { // map to public field
                fieldBuild = builder.DefineField(decl[i], fieldType.getCLSType(), FieldAttributes.Public);
            }
            // add custom attributes
            for (int j = 0; j < fieldType.getAttrs().length; j++) {
                fieldBuild.SetCustomAttribute(fieldType.getAttrs()[j]);    
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
        checkParameterForBuildInfo(data, node);
        Type[] result = parseInheritanceRelation(node, (BuildInfo)data);
        for (int i = 0; i < result.length; i++) {
            if ((i > 0) && (result[i].get_IsClass())) {
                throw new RuntimeException("invalid supertype: for value types, only one concrete value type parent is possible at the first position in the inheritance spec");
            }
            AttributeExtCollection attrs = AttributeExtCollection.ConvertToAttributeCollection(result[i].GetCustomAttributes(InterfaceTypeAttribute.class.ToType(), true));
            if (attrs.IsInCollection(InterfaceTypeAttribute.class.ToType())) {
                InterfaceTypeAttribute ifAttr = (InterfaceTypeAttribute)attrs.GetAttributeForType(InterfaceTypeAttribute.class.ToType());
                if (!(ifAttr.get_IdlType().equals(IdlTypeInterface.AbstractValueType))) {
                    throw new RuntimeException("invalid supertype: only abstract value types are allowed in value inheritance clause and no interfaces");
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
        checkParameterForBuildInfo(data, node);
        Type[] result = parseInheritanceRelation(node, (BuildInfo)data);
        for (int i = 0; i < result.length; i++) {
            if (result[i].get_IsClass()) {
                throw new RuntimeException("invalid supertype: only abstract/concrete interfaces are allowed in support clause");            
            }
            AttributeExtCollection attrs = AttributeExtCollection.ConvertToAttributeCollection(result[i].GetCustomAttributes(InterfaceTypeAttribute.class.ToType(), true));
            if (attrs.IsInCollection(InterfaceTypeAttribute.class.ToType())) {
                InterfaceTypeAttribute ifAttr = (InterfaceTypeAttribute)attrs.GetAttributeForType(InterfaceTypeAttribute.class.ToType());
                if (ifAttr.get_IdlType().equals(IdlTypeInterface.AbstractValueType)) {
                    throw new RuntimeException("invalid supertype: only abstract/concrete interfaces are allowed in support clause and no abstract value type");
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
     * if ((BuildInfo)data).getContainerType() is null, than an independant const-decl is created, else
     * the const delcaration is added to the Type in creation
     */
    public Object visit(ASTconst_dcl node, Object data) {
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
        checkParameterForBuildInfo(data, node);
        Scope currentScope = ((BuildInfo) data).GetBuildScope();
        TypeContainer typeUsedInDefine = (TypeContainer) node.jjtGetChild(0).jjtAccept(this, data);
        Node declarators = node.jjtGetChild(1);    
        for (int i = 0; i < declarators.jjtGetNumChildren(); i++) {
            ASTdeclarator decl = (ASTdeclarator) declarators.jjtGetChild(i);
            if (decl.jjtGetChild(0) instanceof ASTsimple_declarator) {
                String ident = ((SimpleNodeWithIdent) decl.jjtGetChild(0)).getIdent();
                Symbol typedefSymbol = currentScope.getSymbol(ident);
                // inform the type-manager of this typedef
                Debug.WriteLine("typedef defined here, type: " + typeUsedInDefine.getCLSType() +
                                ", symbol: " + typedefSymbol);
                m_typeManager.registerTypeDef(typeUsedInDefine, typedefSymbol);
            }
        }    
        return null;
    }

    
    /** resovle a param_type_spec or a simple_type_spec or other type specs which may return a symbol or a typecontainer
     *  @param node the child node of the type_spec node containing the spec data
     *  @param currentInfo the buildinfo for the scope, this type is specified in
     *  @return a TypeContainer for the represented type
     */
    private TypeContainer resovleTypeSpec(SimpleNode node, BuildInfo currentInfo) {    
        Object result = node.jjtAccept(this, currentInfo);
        TypeContainer resultingType = null;
        if (result instanceof Symbol) { // case <scoped_name>
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
        checkParameterForBuildInfo(data, node);
        SimpleNode child = (SimpleNode)node.jjtGetChild(0);
        return resovleTypeSpec(child, (BuildInfo) data);
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
            if (child instanceof ASTcomplex_declarator) {
                throw new RuntimeException("complex_declarator is unsupported by this compiler");
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
        return new TypeContainer(System.Single.class.ToType());
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTfloating_pt_type_double, Object)
     * @param data unused
     * @return a TypeContainer for the double type
     */
    public Object visit(ASTfloating_pt_type_double node, Object data) {
        return new TypeContainer(System.Double.class.ToType());
    }

    /**
     * unsupported
     * @see parser.IDLParserVisitor#visit(ASTfloating_pt_type_longdouble, Object)
     */
    public Object visit(ASTfloating_pt_type_longdouble node, Object data) {
        throw new System.NotSupportedException("long double not supported by this compiler");
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
        return new TypeContainer(System.Int16.class.ToType());
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTsigned_long_int, Object)
     * @param data unused
     * @return a TypeContainer for the long type
     */
    public Object visit(ASTsigned_long_int node, Object data) {
        return new TypeContainer(System.Int32.class.ToType());
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTsigned_longlong_int, Object)
     * @param data unused
     * @return a TypeContainer for the long long type
     */
    public Object visit(ASTsigned_longlong_int node, Object data) {
        return new TypeContainer(System.Int64.class.ToType());
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
        return new TypeContainer(System.Int16.class.ToType());
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTunsigned_long_int, Object)
     * @param data unused
     * @return a TypeContainer for the long type
     */
    public Object visit(ASTunsigned_long_int node, Object data) {
        return new TypeContainer(System.Int32.class.ToType());
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTunsigned_longlong_int, Object)
     * @param data unused
     * @return a TypeContainer for the long long type
     */
    public Object visit(ASTunsigned_longlong_int node, Object data) {
        return new TypeContainer(System.Int64.class.ToType());
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTchar_type, Object)
     * @param data unused
     * @return a TypeContainer for the char type
     */
    public Object visit(ASTchar_type node, Object data) {
        CustomAttributeBuilder[] attrs = new CustomAttributeBuilder[] { new WideCharAttribute(false).CreateAttributeBuilder()  };
        TypeContainer containter = new TypeContainer(System.Char.class.ToType(), attrs);
        return containter;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTwide_char_type, Object)
     * @param data unused
     * @return a type type container for the wchar type
     */
    public Object visit(ASTwide_char_type node, Object data) {
        CustomAttributeBuilder[] attrs = new CustomAttributeBuilder[] { new WideCharAttribute(false).CreateAttributeBuilder() };
        TypeContainer containter = new TypeContainer(System.Char.class.ToType(), attrs);
        return containter;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTboolean_type, Object)
     * @param data unused
     * @return a TypeContainer for the boolean type
     */
    public Object visit(ASTboolean_type node, Object data) {
        TypeContainer container = new TypeContainer(System.Boolean.class.ToType());
        return container;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASToctet_type, Object)
     * @param data unused
     * @return a TypeContainer for the octet type
     */
    public Object visit(ASToctet_type node, Object data) {
        TypeContainer container = new TypeContainer(System.Byte.class.ToType());
        return container;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTany_type, Object)
     * @param data unused
     * @return a TypeContainer for the any type
     */
    public Object visit(ASTany_type node, Object data) {
        CustomAttributeBuilder[] attrs = new CustomAttributeBuilder[] { new ObjectIdlTypeAttribute(IdlTypeObject.Any).CreateAttributeBuilder() };
        TypeContainer container = new TypeContainer(System.Object.class.ToType(), attrs);
        return container;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTobject_type, Object)
     * @param data unused
     * @return a TypeContainer for the Object type
     */
    public Object visit(ASTobject_type node, Object data) {
        TypeContainer container = new TypeContainer(System.MarshalByRefObject.class.ToType());
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
        checkParameterForBuildInfo(data, node);
        BuildInfo buildInfo = (BuildInfo) data;
        Symbol forSymbol = buildInfo.GetBuildScope().getSymbol(node.getIdent());
        // check if type is known from a previous run over a parse tree --> if so: skip
        if (checkSkip(forSymbol)) { registerSkipped(forSymbol, false); return null; }
        
        TypeBuilder structToCreate = null;
        BuildInfo thisTypeInfo = null;
        TypeAttributes typeAttrs = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Serializable | TypeAttributes.BeforeFieldInit | TypeAttributes.SequentialLayout | TypeAttributes.Sealed;
        if (buildInfo.GetContainterType() == null) {
            // independent dcl
            ModuleBuilder curModBuilder = m_modBuilderManager.GetOrCreateModuleBuilderFor(buildInfo.GetBuildScope());
            String fullyQualName = buildInfo.GetBuildScope().getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());
            structToCreate = curModBuilder.DefineType(fullyQualName, typeAttrs, System.ValueType.class.ToType(),
                                                      new System.Type[] { IIdlEntity.class.ToType() });
            thisTypeInfo = new BuildInfo(buildInfo.GetBuildScope(), structToCreate);
        } else {
            // nested dcl
            if (buildInfo.GetContainterType().get_IsClass()) {
                String fullyQualName = buildInfo.GetBuildScope().getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());
                structToCreate = buildInfo.GetContainterType().DefineNestedType(fullyQualName, typeAttrs,
                                                                                System.ValueType.class.ToType(),
                                                                                new System.Type[] { IIdlEntity.class.ToType() });
            } else {
                // only a class can contain nested types --> therefore use another solution than a nested type for container types which are not classes
                Scope nestedScope = getScopeForNested(buildInfo.GetBuildScope(), forSymbol);
                String fullyQualName = nestedScope.getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());
                ModuleBuilder curModBuilder = m_modBuilderManager.GetOrCreateModuleBuilderFor(nestedScope);
                structToCreate = curModBuilder.DefineType(fullyQualName, typeAttrs, System.ValueType.class.ToType(),
                                                          new System.Type[] { IIdlEntity.class.ToType() });
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
        m_typeManager.registerTypeDefinition(resultType, forSymbol);
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
        checkParameterForBuildInfo(data, node);
        BuildInfo info = (BuildInfo) data;
        TypeBuilder builder = info.GetContainterType();
        ASTtype_spec typeSpecNode = (ASTtype_spec)node.jjtGetChild(0);
        TypeContainer fieldType = (TypeContainer)typeSpecNode.jjtAccept(this, info);
        // special handling for BoxedValue types --> unbox it
        if (fieldType.getCLSType().IsSubclassOf(BoxedValueBase.class.ToType())) {
            fieldType = mapBoxedValueTypeToUnboxed(fieldType.getCLSType());
        }
        String[] decl = (String[])node.jjtGetChild(1).jjtAccept(this, info);
        FieldBuilder fieldBuild;
        for (int i = 0; i < decl.length; i++) {
            fieldBuild = builder.DefineField(decl[i], fieldType.getCLSType(), FieldAttributes.Public);
            // add custom attributes
            for (int j = 0; j < fieldType.getAttrs().length; j++) {
                fieldBuild.SetCustomAttribute(fieldType.getAttrs()[j]);    
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
        throw new System.NotSupportedException("union type is not supported by this compiler");
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTswitch_type_spec, Object)
     */
    public Object visit(ASTswitch_type_spec node, Object data) {
        throw new System.NotSupportedException("union type is not supported by this compiler");
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTswitch_body, Object)
     */
    public Object visit(ASTswitch_body node, Object data) {
        throw new System.NotSupportedException("union type is not supported by this compiler");
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTcasex, Object)
     */
    public Object visit(ASTcasex node, Object data) {
        throw new System.NotSupportedException("union type is not supported by this compiler");
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTcase_label, Object)
     */
    public Object visit(ASTcase_label node, Object data) {
        throw new System.NotSupportedException("union type is not supported by this compiler");
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTelement_spec, Object)
     */
    public Object visit(ASTelement_spec node, Object data) {
        throw new System.NotSupportedException("union type is not supported by this compiler");
    }
    #endregion

    /**
     * @see parser.IDLParserVisitor#visit(ASTenum_type, Object)
     * @param data the current buildinfo instance
     */
    public Object visit(ASTenum_type node, Object data) {
        checkParameterForBuildInfo(data, node);
        BuildInfo buildInfo = (BuildInfo) data;
        Symbol forSymbol = buildInfo.GetBuildScope().getSymbol(node.getIdent());
        // check if type is known from a previous run over a parse tree --> if so: skip
        if (checkSkip(forSymbol)) { registerSkipped(forSymbol, false); return null; }

        TypeBuilder enumToCreate = null;
        TypeAttributes typeAttrs = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed |
                                   TypeAttributes.Serializable;
        if (buildInfo.GetContainterType() == null) {
            // independent dcl
            String fullyQualName = buildInfo.GetBuildScope().getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());
            ModuleBuilder curModBuilder = m_modBuilderManager.GetOrCreateModuleBuilderFor(buildInfo.GetBuildScope());
            enumToCreate = curModBuilder.DefineType(fullyQualName, typeAttrs, System.Enum.class.ToType());
        } else {
            // nested dcl
            if (buildInfo.GetContainterType().get_IsClass()) {
                String fullyQualName = buildInfo.GetBuildScope().getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());
                enumToCreate = buildInfo.GetContainterType().DefineNestedType(fullyQualName, typeAttrs,
                                                                              System.Enum.class.ToType());
            } else {
                // only a class can contain nested types --> therefore use another solution than a nested type for container types which are not classes
                Scope nestedScope = getScopeForNested(buildInfo.GetBuildScope(), forSymbol);
                String fullyQualName = nestedScope.getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());
                ModuleBuilder curModBuilder = m_modBuilderManager.GetOrCreateModuleBuilderFor(nestedScope);
                enumToCreate = curModBuilder.DefineType(fullyQualName, typeAttrs, System.Enum.class.ToType());    
            }
        }
        // add value__ field, see DefineEnum method of ModuleBuilder
        enumToCreate.DefineField("value__", System.Int32.class.ToType(), 
                                 FieldAttributes.Public | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName);
        
        // add enum entries
        for (int i = 0; i < node.jjtGetNumChildren(); i++) {
            String enumeratorId = ((SimpleNodeWithIdent)node.jjtGetChild(i)).getIdent();
            enumToCreate.DefineField(enumeratorId, enumToCreate, 
                                     FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal);
        }

        // add IDLEnum attribute
        enumToCreate.SetCustomAttribute(new IdlEnumAttribute().CreateAttributeBuilder());
        
        // create the type
        Type resultType = enumToCreate.CreateType();
        // type must be registered with the type-manager
        m_typeManager.registerTypeDefinition(resultType, forSymbol);
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
        checkParameterForBuildInfo(data, node);
        if (node.jjtGetNumChildren() > 1) { 
            throw new RuntimeException("sequence with a bound not supported by this compiler"); 
        }
        Node elemTypeNode = node.jjtGetChild(0);
        Debug.WriteLine("determine element type of IDLSequence");
        TypeContainer elemType = (TypeContainer)elemTypeNode.jjtAccept(this, data);
        Debug.WriteLine("seq type determined: " + elemType.getCLSType());
        // create CLS array type with the help of GetType(), otherwise not possible
        Type arrayType;
        if (elemType.getCLSType() instanceof TypeBuilder) {
            Module declModule = ((TypeBuilder)elemType.getCLSType()).get_Module();
            Debug.WriteLine("get-elem-Type: " + declModule.GetType(elemType.getCLSType().get_FullName()));
            arrayType = declModule.GetType(elemType.getCLSType().get_FullName() + "[]"); // not nice, better solution ?
        } else {
            Assembly declAssembly = elemType.getCLSType().get_Assembly();
            Debug.WriteLine("decl-Assembly: " + declAssembly);
            arrayType = declAssembly.GetType(elemType.getCLSType().get_FullName() + "[]"); // not nice, better solution ?
        }
        
        Debug.WriteLine("created array type: " + arrayType);        
        TypeContainer result = new TypeContainer(arrayType, new CustomAttributeBuilder[] { new IdlSequenceAttribute().CreateAttributeBuilder() } );
        return result;
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTstring_type, Object)
     * @param data unsed
     */
    public Object visit(ASTstring_type node, Object data) {
        CustomAttributeBuilder[] attrs = new CustomAttributeBuilder[] { new StringValueAttribute().CreateAttributeBuilder(), new WideCharAttribute(false).CreateAttributeBuilder() };
        TypeContainer containter = new TypeContainer(System.String.class.ToType(), attrs);
        return containter;
    }
    
    /**
     * @see parser.IDLParserVisitor#visit(ASTwide_string_type, Object)
     * @param data unsed
     * @return a TypeContainer for the wideString-Type
     */
    public Object visit(ASTwide_string_type node, Object data) {
        CustomAttributeBuilder[] attrs = new CustomAttributeBuilder[] { new StringValueAttribute().CreateAttributeBuilder(), new WideCharAttribute(true).CreateAttributeBuilder() };
        TypeContainer containter = new TypeContainer(System.String.class.ToType(), attrs);
        return containter;
    }

    #region array unsupported at the moment
    /**
     * @see parser.IDLParserVisitor#visit(ASTarray_declarator, Object)
     */
    public Object visit(ASTarray_declarator node, Object data) {
        throw new System.NotSupportedException("array type is not supported by this compiler");
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTfixed_array_size, Object)
     */
    public Object visit(ASTfixed_array_size node, Object data) {
        throw new System.NotSupportedException("union type is not supported by this compiler");
    }
    #endregion

    /** need this, because define-parameter prevent creating a parameterbuilder for param-0, the ret param.
     *  For defining custom attributes on the ret-param, a parambuilder is however needed
     *  TBD: search nicer solution for this */
    private ParameterBuilder createParamBuilderForRetParam(MethodBuilder forMethod) {
        return (ParameterBuilder) m_paramBuildConstr.Invoke(new Object[] { forMethod, (System.Int32) 0, ParameterAttributes.Retval, "" } );
    }

    /**
     * @see parser.IDLParserVisitor#visit(ASTattr_dcl, Object)
     * @param data the buildinfo of the type, which declares this attribute
     */
    public Object visit(ASTattr_dcl node, Object data) {
        checkParameterForBuildInfo(data, node);
        BuildInfo info = (BuildInfo) data;
        TypeBuilder builder = info.GetContainterType();
        ASTparam_type_spec typeSpecNode = (ASTparam_type_spec)node.jjtGetChild(0);
        TypeContainer propType = (TypeContainer)typeSpecNode.jjtAccept(this, info);
        // special handling for BoxedValue types --> unbox it
        if (propType.getCLSType().IsSubclassOf(BoxedValueBase.class.ToType())) {
            propType = mapBoxedValueTypeToUnboxed(propType.getCLSType());
        }
        PropertyBuilder propBuild;
        for (int i = 1; i < node.jjtGetNumChildren(); i++) {
            ASTsimple_declarator simpleDecl = (ASTsimple_declarator) node.jjtGetChild(i);
            propBuild = builder.DefineProperty(simpleDecl.getIdent(), PropertyAttributes.None, 
                                               propType.getCLSType(), System.Type.EmptyTypes);
            // set the methods for the property
            MethodBuilder getAccessor = builder.DefineMethod("get_" + simpleDecl.getIdent(), 
                                                             MethodAttributes.Virtual | MethodAttributes.Abstract | MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, 
                                                             propType.getCLSType(), System.Type.EmptyTypes);
            propBuild.SetGetMethod(getAccessor);
            MethodBuilder setAccessor = null;
            if (!(node.isReadOnly())) {
                setAccessor = builder.DefineMethod("set_" + simpleDecl.getIdent(), 
                                                   MethodAttributes.Virtual | MethodAttributes.Abstract | MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, 
                                                   null, new System.Type[] { propType.getCLSType() });
                propBuild.SetSetMethod(setAccessor);
            }
            
            ParameterBuilder retParamGet = createParamBuilderForRetParam(getAccessor);
            ParameterBuilder valParam = null;
            if (setAccessor != null) { 
                valParam = setAccessor.DefineParameter(1, ParameterAttributes.None, "value"); 
            }
            // add custom attributes
            for (int j = 0; j < propType.getAttrs().length; j++) {
                propBuild.SetCustomAttribute(propType.getAttrs()[j]);    
                
                retParamGet.SetCustomAttribute(propType.getAttrs()[j]);
                if (setAccessor != null) {
                    valParam.SetCustomAttribute(propType.getAttrs()[j]);
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
        checkParameterForBuildInfo(data, node);
        BuildInfo buildInfo = (BuildInfo) data;
        Symbol forSymbol = buildInfo.GetBuildScope().getSymbol(node.getIdent());
        // check if type is known from a previous run over a parse tree --> if so: skip
        if (checkSkip(forSymbol)) { registerSkipped(forSymbol, false); return null; }

        TypeBuilder exceptToCreate = null;
        BuildInfo thisTypeInfo = null;
        
        if (buildInfo.GetContainterType() == null) {
            // independent dcl
            String fullyQualName = buildInfo.GetBuildScope().getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());
            ModuleBuilder curModBuilder = m_modBuilderManager.GetOrCreateModuleBuilderFor(buildInfo.GetBuildScope());
            exceptToCreate = curModBuilder.DefineType(fullyQualName, 
                                                      TypeAttributes.Class | TypeAttributes.Public, 
                                                      AbstractUserException.class.ToType());
            thisTypeInfo = new BuildInfo(buildInfo.GetBuildScope(), exceptToCreate);
        } else {
            // nested dcl
            if (buildInfo.GetContainterType().get_IsClass()) {
                String fullyQualName = buildInfo.GetBuildScope().getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());                
                exceptToCreate = buildInfo.GetContainterType().DefineNestedType(fullyQualName, 
                                                                                TypeAttributes.Class | TypeAttributes.NestedPublic, 
                                                                                AbstractUserException.class.ToType());
            } else {
                // only a class can contain nested types --> therefore use another solution than a nested type for container types which are not classes
                Scope nestedScope = getScopeForNested(buildInfo.GetBuildScope(), forSymbol);
                String fullyQualName = nestedScope.getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());
                ModuleBuilder curModBuilder = m_modBuilderManager.GetOrCreateModuleBuilderFor(nestedScope);
                exceptToCreate = curModBuilder.DefineType(fullyQualName, 
                                                          TypeAttributes.Class | TypeAttributes.Public,
                                                          AbstractUserException.class.ToType());                
            }
            thisTypeInfo = new BuildInfo(buildInfo.GetBuildScope(), exceptToCreate);
        }
        String repId = getRepIdForException(forSymbol);
        addRepIdAttribute(exceptToCreate, repId);

        // add fileds ...
        node.childrenAccept(this, thisTypeInfo); // let the members add themself to the typeBuilder
        
        // create the type
        Type resultType = exceptToCreate.CreateType();
        // type must be registered with the type-manager
        m_typeManager.registerTypeDefinition(resultType, forSymbol);
        return null;
    }

    /** generates a rep-id for a CLS exception class
     *  @param forSymbol the symbol of the exception */
    private String getRepIdForException(Symbol forSymbol) {
        java.util.Stack scopeStack = new java.util.Stack();
        Scope currentScope = forSymbol.getDeclaredIn();
        while (currentScope != null) {
            if (!currentScope.getScopeName().equals("")) {
                scopeStack.push(currentScope.getScopeName());
            }
            currentScope = currentScope.getParentScope();
        }
        String repId = "IDL:";
        while (!scopeStack.isEmpty()) {
            String currentScopeName = (String) scopeStack.pop();
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
        checkParameterForBuildInfo(data, node);
        BuildInfo buildInfo = (BuildInfo) data;
        
        // return type
        TypeContainer returnType = (TypeContainer)node.jjtGetChild(0).jjtAccept(this, buildInfo);
        // special handling for BoxedValue types --> unbox it
        if (returnType.getCLSType().IsSubclassOf(BoxedValueBase.class.ToType())) {
            returnType = mapBoxedValueTypeToUnboxed(returnType.getCLSType());
        }
        // parameters
        ParameterSpec[] params = (ParameterSpec[])node.jjtGetChild(1).jjtAccept(this, buildInfo);
        Type[] paramTypes = new Type[params.length];
        for (int i = 0; i < params.length; i++) { paramTypes[i] = getParamType(params[i]); }
        // name
        String methodName = node.getIdent();
        // ready to create method
        TypeBuilder typeAtBuild = buildInfo.GetContainterType();
        MethodBuilder methodBuild = typeAtBuild.DefineMethod(methodName,  
                                                             MethodAttributes.Virtual | MethodAttributes.Abstract | MethodAttributes.Public | MethodAttributes.HideBySig,
                                                             returnType.getCLSType(), paramTypes);
        // define the paramter-names / attributes
        for (int i = 0; i < params.length; i++) {
            defineParamter(methodBuild, params[i], i+1);
        }
        // add custom attributes for the return type
        ParameterBuilder paramBuild = createParamBuilderForRetParam(methodBuild);
        for (int i = 0; i < returnType.getAttrs().length; i++) {
            paramBuild.SetCustomAttribute(returnType.getAttrs()[i]);
        }
        return null;
    }

    /** retrieve the correct type out of ParameterSpec. Consider parameter-direction too */
    private Type getParamType(ParameterSpec spec) {
        TypeContainer specType = spec.getParamType();
        // special handling for BoxedValue types --> unbox it
        if (specType.getCLSType().IsSubclassOf(BoxedValueBase.class.ToType())) {
            specType = mapBoxedValueTypeToUnboxed(specType.getCLSType());
        }
        Type resultType;
        if (spec.isIn()) {
            resultType = specType.getCLSType();
        } else { // out or inout parameter
            // need a type which represents a reference to the parametertype
            Assembly declAssembly = specType.getCLSType().get_Assembly();
            resultType = declAssembly.GetType(specType.getCLSType().get_FullName() + "&"); // not nice, better solution ?
        }
        return resultType;
    }

    private void defineParamter(MethodBuilder methodBuild, ParameterSpec spec, int paramNr) {
        ParameterAttributes paramAttr = ParameterAttributes.None;
        if (spec.isOut()) { paramAttr = paramAttr | ParameterAttributes.Out; }
        ParameterBuilder paramBuild = methodBuild.DefineParameter(paramNr, paramAttr, spec.getPramName());
        // custom attribute spec
        TypeContainer specType = spec.getParamType();
        // special handling for BoxedValue types --> unbox it
        if (specType.getCLSType().IsSubclassOf(BoxedValueBase.class.ToType())) {
            specType = mapBoxedValueTypeToUnboxed(specType.getCLSType());
        }
        for (int i = 0; i < specType.getAttrs().length; i++) {
            paramBuild.SetCustomAttribute(specType.getAttrs()[i]);    
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
            returnType = new TypeContainer(System.Void.class.ToType());
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
        ParameterSpec[] params = new ParameterSpec[node.jjtGetNumChildren()];
        for (int i = 0; i < node.jjtGetNumChildren(); i++) {
            params[i] = (ParameterSpec) node.jjtGetChild(i).jjtAccept(this, data);
        }
        return params;
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
        String paramName = ((ASTsimple_declarator)node.jjtGetChild(2)).getIdent();
        
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
        checkParameterForBuildInfo(data, node);
        SimpleNode child = (SimpleNode)node.jjtGetChild(0); // get the node representing <base_type_spec> or <string_type> or <widestring_type> or <scoped_name>
        return resovleTypeSpec(child, (BuildInfo)data);
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
        TypeContainer container = new TypeContainer(System.Object.class.ToType(), attrs);
        return container;
    }

    /** for method parameters, fields, attributes, the unboxed type for the boxed value type is used. This 
     * method returns the unboxed type and created the correct boxed value attribute
     */
    private TypeContainer mapBoxedValueTypeToUnboxed(Type boxedValueType) {
        Debug.WriteLine("unbox boxed value type");
        if (!(boxedValueType.IsSubclassOf(BoxedValueBase.class.ToType()))) {
            throw new RuntimeException("a boxed value type is expected");
        }
        AttributeExtCollection attrColl = AttributeExtCollection.ConvertToAttributeCollection(boxedValueType.GetCustomAttributes(true));
        if (!(attrColl.IsInCollection(RepositoryIDAttribute.class.ToType()))) {
            throw new RuntimeException("invalid boxed value type created, rep-id missing: " + boxedValueType);
        }
        String boxedValueRepId = ((RepositoryIDAttribute)attrColl.GetAttributeForType(RepositoryIDAttribute.class.ToType())).get_Id();
        try {
            Type fullUnboxed = (Type)boxedValueType.InvokeMember(BoxedValueBase.GET_FIRST_NONBOXED_TYPE_METHODNAME,
                                                                 BindingFlags.InvokeMethod | BindingFlags.Public |
                                                                     BindingFlags.NonPublic | BindingFlags.Static |
                                                                     BindingFlags.DeclaredOnly,
                                                                 null, null, new System.Object[0]);
            CustomAttributeBuilder[] attrs = new CustomAttributeBuilder[] { new BoxedValueAttribute(boxedValueRepId).CreateAttributeBuilder() };
            return new TypeContainer(fullUnboxed, attrs);
        } catch (Exception e) {
            throw new RuntimeException("invalid type found: " + boxedValueType + 
                                       ", static method missing or not callable: " + BoxedValueBase.GET_FIRST_NONBOXED_TYPE_METHODNAME);
        }
    }


    /** check if data is an instance of buildinfo, if not throws an exception */
    private void checkParameterForBuildInfo(Object data, Node visitedNode) {
        if (!(data instanceof BuildInfo)) { 
            throw new RuntimeException("precondition violation in visitor for node" + visitedNode.GetType() + ", " + data.GetType() + " but expected BuildInfo"); 
        }
    }

    #endregion IMethods

}
