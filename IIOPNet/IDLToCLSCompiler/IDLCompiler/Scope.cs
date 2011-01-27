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
using Ch.Elca.Iiop.IdlCompiler.Exceptions;

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
    
    private ArrayList m_inheritedScopes = new ArrayList();

    private bool m_isTypeScope = false;

    #endregion IFields
    #region IConstructors

    public Scope(String name, Scope parentScope, bool isTypeScope) {
        m_scopeName = name;
        m_parentScope = parentScope;
        if (m_parentScope != null) {
            m_parentScope.addChildScope(this);
        }
        m_isTypeScope = isTypeScope;
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
    
    public bool IsTypeScope() {
        return m_isTypeScope;
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
   
    /// <sumamry>
    /// add a symbol for a value, like a const or a enumerator of an enumeration.
    /// </summary>
    /// <exception cref="ScopeException">thrown, when trying to redefine symbol</exception>
    public void addSymbolValue(String symbolName) {
        if (m_symbols.ContainsKey(symbolName)) {
            throw new ScopeException("symbol redifined: " + symbolName);
        }
        Symbol newSymbol = new SymbolValue(symbolName, this);
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
        if (m_pragmas.ContainsKey(id) && (!m_pragmas[id].Equals(value))) { 
            // according to 10.6.5.1 in CORBA 2.6, it's only illegal to reassign a different id
            throw new ScopeException(String.Format("pragma id error, id redefined: {0}; last value : {1}, redef: {2}",
                                                   id, m_pragmas[id], value));
        }
        m_pragmas[id] = value;
    }

    public String getRepositoryIdFor(String unqualTypeName) {
        return (String)m_pragmas[unqualTypeName];
    }

    /** gets the fully qualified name for the symbol with the symbolName
     * This method checks if the symbol is present in the Scope and throws an error if not
     */
    public String GetFullyQualifiedNameForSymbol(String symbolName) {
        if (getSymbol(symbolName) == null) { 
            throw new InternalCompilerException("error in scope " + this + ", symbol with name: " + symbolName + " not found"); 
        }
        String namespaceName = GetFullyQualifiedScopeName();
        String fullyQualName = namespaceName;
        if (fullyQualName.Length > 0) { 
            fullyQualName += "."; 
        }
        fullyQualName += IdlNaming.MapIdlNameToClsName(symbolName);
        return fullyQualName;
    }
    
    /// <summary>returns the associated nested scope name for this Scope</summary>
    private String getNestedScopeNameForScope() {
        return getScopeName() + "_package";     
    }
    
    private String getTypeScopeName() {
        // for a type scope, the nested name must be used, because types are always nested in a special namespace    
        if (!IsTypeScope()) {
            return m_scopeName;
        } else {
            return getNestedScopeNameForScope();
        }
    }

    /// <summary>returns the fully qualified CLS name of this scope (usable for type generation)</summary>
    /// <remarks>for type-scopes, returns the nested scope name, because this one is needed for type generation</remarks>
    private String GetFullyQualifiedScopeName() {
        string result = "";

        for (Scope scope = this; scope.getParentScope() != null; scope = scope.getParentScope()) {
            string scopeName = IdlNaming.MapIdlNameToClsName(scope.getTypeScopeName());
            if(result == "")
                result = scopeName;
            else
                result = scopeName + '.' + result;
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
                throw new InvalidIdlException("type only fwd declared: " + sym);
            }                                              
        }
    }
    
    /// <summary>
    /// adds an inherited scope (e.g. interface A : B leads to A inheriting scope B)
    /// </summary>
    public void AddInheritedScope(Scope scope) {
        m_inheritedScopes.Add(scope);
    }
    
    public IList GetInheritedScopes() {
        return m_inheritedScopes;
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

    /// <summary>
    /// constructs a repository id part for this scope and the parent scopes.
    /// </summary>  
    public virtual string ConstructRepositoryIDPart() {
        string result = (!m_scopeName.StartsWith("_") ? m_scopeName : m_scopeName.Substring(1));
        if (getParentScope() != null) { 
             string parentIdPart = getParentScope().ConstructRepositoryIDPart();             
             result = (!parentIdPart.Equals("") ? parentIdPart + "/" + result : result); 
        }
        return result;
    }

    #endregion IMethods

}

}
