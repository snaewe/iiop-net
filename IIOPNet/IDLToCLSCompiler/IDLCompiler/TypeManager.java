/* TypeManager.java
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



package Ch.Elca.Iiop.IdlCompiler.Action;

import java.util.Hashtable;

import System.Reflection.*;
import System.Reflection.Emit.*;
import System.Type;
import symboltable.Symbol;
import symboltable.Scope;


/** manages fully created and only partly created types */
public class TypeManager {

    #region IFields

    private Hashtable m_typesInCreation = new Hashtable();
    private Hashtable m_typeTable = new Hashtable();
    private Hashtable m_typedefTable = new Hashtable();
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

    /** register a not fully created type */
    public void RegisterTypeFwdDecl(TypeBuilder type, Symbol forSymbol) {
        if (forSymbol == null) { 
            throw new RuntimeException("register error for type: " + 
                                       type.get_FullName() + ", symbol may not be null"); 
        }
        if (IsTypeDeclarded(forSymbol)) {
            throw new RuntimeException("internal error; a type with the name " + 
                                       GetKnownType(forSymbol).GetClsType().get_FullName() +
                                       " is already declared for symbol: " + forSymbol);
        }
        TypeContainer container = new TypeContainer(type, new CustomAttributeBuilder[0]);
        m_typesInCreation.put(forSymbol, container);
    }

    /** is at least a forward declaration for the type represented by the symbol present */
    public boolean IsTypeDeclarded(Symbol forSymbol) {
        TypeContainer type = GetKnownType(forSymbol);
        if (type == null) { 
            return false; 
        } else {
            return true;
        }
    }
    
    /** is a full definitaion present for the type represented by the symbol forSymbol */
    public boolean IsTypeFullyDeclarded(Symbol forSymbol) {
        if ((!IsFwdDeclared(forSymbol)) && (IsTypeDeclarded(forSymbol))) { 
            return true; 
        } else {
            return false;    
        }
    }

    public boolean IsFwdDeclared(Symbol forSymbol) {
        TypeContainer result = (TypeContainer)m_typesInCreation.get(forSymbol);
        if (result == null) {
            return false;
        } else {
            return true;
        }
    }

    /** checks, if a type is already declared in a referenced assembly */
    public boolean IsTypeDeclaredInRefAssemblies(Symbol forSymbol) {
        return m_refAsmTypes.GetTypeFromRefAssemblies(forSymbol) != null;
    }

    #region methods for supporting generation for more than one parse result    
    
    /** checks if a type is fully declared in a module of the resulting assembly.
     *  Does only return fully defined types, others are not interesting here. 
     **/
    private Type GetTypeFromBuildModule(Symbol forSymbol) {
        Scope declIn = declIn = forSymbol.getDeclaredIn();
        String fullName = declIn.getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());
        Type result = m_modBuilder.GetType(fullName);
        if (!(result instanceof TypeBuilder)) {  // type is fully defined (do not return not fully defined types here)
            return result;
        }
        return null;
    }
    
    /** checks, if a type is already defined in a previous run */
    public boolean CheckInBuildModulesForType(Symbol forSymbol) {
        if (GetTypeFromBuildModule(forSymbol) != null) {
            return true;
        } else {
            return false;
        }
    }

    #endregion methods for supporting generation for more than one parse result

    /** check, that no types not defined left; if so this is an internal error, 
     * because already checked during symbol table build. */
    public void AssertAllTypesDefined() {
        if (m_typesInCreation.size() > 0) {
            java.util.Enumeration enum = m_typesInCreation.elements();
            while (enum.hasMoreElements()) {
                System.err.println("internal error, only forward declared: " + 
                                   ((TypeContainer)enum.nextElement()).GetClsType());
                // this should not occur, because checked by symbol table
            }
            throw new RuntimeException("internal error occured, not all types fully defined");
        }
    }

    /** get the full defined Type or the fwd decl
     */
    public TypeContainer GetKnownType(Symbol forSymbol) {
        TypeContainer result = (TypeContainer)m_typeTable.get(forSymbol);
        if (result == null) {
            result = (TypeContainer)m_typesInCreation.get(forSymbol);
        }
        if (result == null) {
            Type fromBuildMod = GetTypeFromBuildModule(forSymbol);
            if (fromBuildMod != null) {
                result = new TypeContainer(fromBuildMod, new CustomAttributeBuilder[0]);
            }        
        }
        if (result == null) { 
            // check in types, which are defined in referenced assemblies
            Type fromAsm = m_refAsmTypes.GetTypeFromRefAssemblies(forSymbol);
            if (fromAsm != null) {
                result = new TypeContainer(fromAsm, new CustomAttributeBuilder[0]);
            }
        }
        return result;
    }
    
    /** add a fully defined type to the known types
     */
    private void AddTypeDefinition(TypeContainer fullDecl, Symbol forSymbol) {
        if (m_typesInCreation.containsKey(forSymbol)) { 
            throw new RuntimeException("type can't be registered, a fwd declaration exists");  // should not occur, check by sym table
        }
        if (m_typeTable.containsKey(forSymbol)) { 
            throw new RuntimeException("type already defined"); // should not occur, checked by sym table
        }
        m_typeTable.put(forSymbol, fullDecl);
    }
    
    /** register a full type definition (CreateType() already called)
     */
    public void RegisterTypeDefinition(Type fullDecl, Symbol forSymbol) {
        TypeContainer container = new TypeContainer(fullDecl, new CustomAttributeBuilder[0]);
        AddTypeDefinition(container, forSymbol);
    }

    /** use this to tell the type manager, that a type is now fully created.
     *  The typemanager checks at the end, if not fully declared types exists and throw an error, if so
     */
    public void ReplaceFwdDeclWithFullDecl(Type fullDecl, Symbol forSymbol) {
        m_typesInCreation.remove(forSymbol);
        // add to the fully created types
        TypeContainer container = new TypeContainer(fullDecl, new CustomAttributeBuilder[0]);
        m_typeTable.put(forSymbol, container);
    }

    public void RegisterTypeDef(TypeContainer fullDecl, Symbol forSymbol) {
        AddTypeDefinition(fullDecl, forSymbol);
    }

    #endregion IMethods

}

/** helper class to contain a .NET Type and the attributes on the param, field, ... */
class TypeContainer {
    
    #region IFields

    private Type m_clsType;
    private CustomAttributeBuilder[] m_attrs;

    #endregion IFields
    #region IConstructors
    
    public TypeContainer(Type clsType, CustomAttributeBuilder[] attrs) {
        m_clsType = clsType;
        m_attrs = attrs;
    }

    public TypeContainer(Type clsType) {
        this(clsType, new CustomAttributeBuilder[0]);
    }

    #endregion IConstructors
    #region IMethods

    public Type GetClsType() {
        return m_clsType;
    }

    public CustomAttributeBuilder[] GetAttrs() {
        return m_attrs;
    }

    #endregion IMethods

}

