/* Scope.cs
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
using System.Collections;
using Ch.Elca.Iiop.Idl;

namespace symboltable {

/// <summary>
/// represents a name scope
/// </summary>
public class Scope {

    #region IFields
    
    /** the unqualified name of this scope */
    private String m_scopeName;

    /** the parent scope of this scope */
    private Scope m_parentScope;

    /** the child scopes of this scope */
    private Hashtable m_childScopes = new Hashtable();

    // fields and methods for handling symbols in scope
    private Hashtable m_symbols = new Hashtable();

    private Hashtable m_pragmas = new Hashtable();

    #endregion IFields
    #region IConstructors

    public Scope(String name, Scope parentScope) {
        m_scopeName = name;
        m_parentScope = parentScope;
        if (m_parentScope != null) {
            m_parentScope.addChildScope(this);
        }
    }

    #endregion IConstructors
    #region IMethods
    
    public String getScopeName() {
        return m_scopeName;
    }    
    
    public Scope getParentScope() {
        return m_parentScope;
    }
    
    public void addChildScope(Scope scope) {
        m_childScopes[scope.getScopeName()] = scope;
    }
    
    public bool containsChildScope(String scopeName) {
        return m_childScopes.ContainsKey(scopeName);
    }
    
    /** retrieve a childscope in the scope childs and in all pragma scopes childs of this scope */
    public Scope getChildScope(String scopeName) {
        Scope result = (Scope)m_childScopes[scopeName];
        if (result == null) {
            result = getChildScopeInPragmaScope(scopeName); // check to see if scope is child of a pragma scope
        }
        return result;
    }

    private Scope getChildScopeInPragmaScope(String scopeName) {
        Scope result = null;
        IEnumerator enumerator = getChildScopeEnumeration();
        while ((enumerator.MoveNext()) && (result == null)) {
            Scope thisScope = (Scope) enumerator.Current;
            if (thisScope is PragmaScope) {
                result = thisScope.getChildScope(scopeName);
            }
        }
        return result;
    }
    
    public IEnumerator getChildScopeEnumeration() {
        return m_childScopes.Values.GetEnumerator();
    }    
    
    /// <sumamry>
    /// add a full defined symbol. This method is not intended for forwardDeclarations
    /// </summary>
    /// <exception cref="ScopeException">thrown, when trying to redefine symbol</exception>
    public void addSymbol(String symbolName)  {
        if (m_symbols.ContainsKey(symbolName)) {
            Symbol sym = (Symbol)m_symbols[symbolName];
            if (!(sym is SymbolFwdDecl)) {
                throw new ScopeException("symbol redifined: " + symbolName);
            }
        }
        Symbol newSymbol = new SymbolDefinition(symbolName, this);
        m_symbols[symbolName] = newSymbol;
    }
    
    /// <summary>
    /// add an encountered fwd declaration
    /// </summary>
    public void addFwdDecl(String symbolName) {
        if (m_symbols.ContainsKey(symbolName)) {
            return; // more than one fwd-decl is allowed
        }
        Symbol newSymbol = new SymbolFwdDecl(symbolName, this);
        m_symbols[symbolName] = newSymbol;
    }

    /// <exception cref="ScopeException">thrown, when trying to redefine symbol</exception>
    public void addTypeDef(String definedType) {
        if (m_symbols.ContainsKey(definedType)) {
            throw new ScopeException("typedef not possible, this type already exists");
        }
        Symbol newSymbol = new SymbolTypedef(definedType, this);
        m_symbols[definedType] = newSymbol;
    }
    
    /** gets the symbol in this scope */
    public Symbol getSymbol(String symbolName) {
        return (Symbol)m_symbols[symbolName];
    }

    public IEnumerator getSymbolEnum() {
        return m_symbols.Values.GetEnumerator();
    }
        
    /** adds a pragma id to this scope */
    public void addPragmaID(String id, String value) {
        if (m_pragmas.ContainsKey(id)) { 
            throw new Exception("pragma id error, id already defined: " + id); 
        }
        m_pragmas[id] = value;
    }

    public String getRepositoryIdFor(String unqualTypeName) {
        return (String)m_pragmas[unqualTypeName];
    }

    /** gets the fully qualified name for the symbol with the symbolName
     * This method checks if the symbol is present in the Scope and throws an error if not
     */
    public String getFullyQualifiedNameForSymbol(String symbolName) {
        if (getSymbol(symbolName) == null) { 
            throw new Exception("error in scope " + this + ", symbol with name: " + symbolName + " not found"); 
        }
        String namespaceName = getFullyQualifiedScopeName();
        String fullyQualName = namespaceName;
        if (fullyQualName.Length > 0) { 
            fullyQualName += "."; 
        }
        fullyQualName += IdlNaming.MapIdlNameToClsName(symbolName);
        return fullyQualName;
    }

    /** returns the fully qualified name of this scope */
    /** @return the fully qualified scope name in CLS */
    public String getFullyQualifiedScopeName() {
        if (getParentScope() == null) { 
            return ""; 
        }
        Stack scopeStack = new Stack();
        scopeStack.Push(m_scopeName);
        Scope parent = getParentScope();
        while (parent.getParentScope() != null) {
            scopeStack.Push(parent.m_scopeName);
            parent = parent.getParentScope();
        }
        
        String result = "";
        int stackSize = scopeStack.Count;
        for (int i = 0; i < stackSize; i++) {
            if (i > 0) { 
                result += "."; 
            }
            result += IdlNaming.MapIdlNameToClsName((String)scopeStack.Pop());
        }
        return result;
    }

    /// <summary>
    /// Assures, that for all forward declarations, a full declaration is present.
    /// Otherwise: throw exception
    /// </summary>
    public void CheckAllFwdCompleted() {
        foreach (Symbol sym in m_symbols.Values) {            
            // in scope, all fwd symbols must be replaces by def symbols --> otherwise no def present.
            if (sym is SymbolFwdDecl) {
                throw new Exception("type only fwd declared: " + sym);
            }                                              
        }
    }

    /// <summary>
    /// create or retrieve a Scope for nested IDL-types, which may not be nested inside the mapped CLS type of the container scope.
    /// </summary>
    /// <param name="cratedFor">the Symbol for which the nested scope should be created / retrieved</param>
    public Scope GetScopeForNested(Symbol createdFor) {
        Scope parentOfContainer = getParentScope();
        String nestedScopeName = getScopeName() + "_package";
        if (!(parentOfContainer.containsChildScope(nestedScopeName))) {
            parentOfContainer.addChildScope(new Scope(nestedScopeName, parentOfContainer));
        }
        Scope nestedScope = parentOfContainer.getChildScope(nestedScopeName);
        nestedScope.addSymbol(createdFor.getSymbolName());
        return nestedScope;
    }

    public override String ToString() {
        String result = "scope begins: " + m_scopeName + "\n";
        // symbols
        foreach (Symbol sym in m_symbols.Values) {
            result += (sym + "\n");
        }
        // scopes
        IEnumerator scopeEnum = getChildScopeEnumeration();
        while (scopeEnum.MoveNext()) {
            result += (scopeEnum.Current + "\n");
        }
        result += ("scope ends");
        return result;
    }

    #endregion IMethods

}

}