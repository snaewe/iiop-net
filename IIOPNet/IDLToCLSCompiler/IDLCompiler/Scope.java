/* Scope.java
 * 
 * Project: IIOP.NET
 * IDLToCLSCompiler
 * 
 * WHEN      RESPONSIBLE
 * 19.02.03  Dominic Ullmann (DUL), dul@elca.ch
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

package symboltable;

import java.util.ArrayList;
import java.util.Enumeration;
import java.util.Hashtable;
import java.util.LinkedList;
import java.util.Stack;

/**
 * 
 * @version 
 * @author dul
 * 
 *
 */
public class Scope {

    public Scope(String name, Scope parentScope) {
        m_scopeName = name;
        m_parentScope = parentScope;
        if (m_parentScope != null) {
            m_parentScope.addChildScope(this);
        }
    }
    
    /** the unqualified name of this scope */
    private String m_scopeName;
    public String getScopeName() {
        return m_scopeName;
    }
    
    /** the parent scope of this scope */
    private Scope m_parentScope;
    
    /** the child scopes of this scope */
    private Hashtable m_childScopes = new Hashtable();
    
    
    public Scope getParentScope() {
        return m_parentScope;
    }
    
    public void addChildScope(Scope scope) {
        m_childScopes.put(scope.getScopeName(), scope);
    }
    
    public boolean containsChildScope(String scopeName) {
        return m_childScopes.containsKey(scopeName);
    }
    
    /** retrieve a childscope in the scope childs and in all pragma scopes childs of this scope */
    public Scope getChildScope(String scopeName) {
        Scope result = (Scope)m_childScopes.get(scopeName);
        if (result == null) {
            result = getChildScopeInPragmaScope(scopeName); // check to see if scope is child of a pragma scope
        }
        return result;
    }

    private Scope getChildScopeInPragmaScope(String scopeName) {
        Scope result = null;
        Enumeration enum = getChildScopeEnumeration();
        while ((enum.hasMoreElements()) && (result == null)) {
            Scope thisScope = (Scope) enum.nextElement();
            if (thisScope instanceof PragmaScope) {
                result = thisScope.getChildScope(scopeName);
            }
        }
        return result;
    }
    
    public Enumeration getChildScopeEnumeration() {
        return m_childScopes.elements();
    }
    
    // fields and methods for handling symbols in scope
    private Hashtable m_symbols = new Hashtable();
    
    /** add a full defined symbol. This method is not intended for forwardDeclarations */
    public void addSymbol(String symbolName) throws ScopeException {
        if (m_symbols.containsKey(symbolName)) {
            Symbol sym = (Symbol)m_symbols.get(symbolName);
            if (!(sym instanceof SymbolFwdDecl)) {
                throw new ScopeException("symbol redifined: " + symbolName);
            }
        }
        Symbol newSymbol = new SymbolDefinition(symbolName, this);
        m_symbols.put(symbolName, newSymbol);
    }
    
    /** add an encountered fwd declaration */
    public void addFwdDecl(String symbolName) throws ScopeException {
        if (m_symbols.containsKey(symbolName)) {
            return; // more than one fwd-decl is allowed
        }
        Symbol newSymbol = new SymbolFwdDecl(symbolName, this);
        m_symbols.put(symbolName, newSymbol);
    }

    public void addTypeDef(String definedType) throws ScopeException {
        if (m_symbols.containsKey(definedType)) {
            throw new ScopeException("typedef not possible, this type already exists");
        }
        Symbol newSymbol = new SymbolTypedef(definedType, this);
        m_symbols.put(definedType, newSymbol);
    }
    
    /** gets the symbol in this scope */
    public Symbol getSymbol(String symbolName) {
        return (Symbol)m_symbols.get(symbolName);
    }

    public Enumeration getSymbolEnum() {
        return m_symbols.elements();
    }

    
    private Hashtable m_pragmas = new Hashtable();
    /** adds a pragma id to this scope */
    public void addPragmaID(String id, String value) {
        if (m_pragmas.containsKey(id)) { throw new RuntimeException("pragma id error, id already defined: " + id); }
        m_pragmas.put(id, value);
    }

    public String getRepositoryIdFor(String unqualTypeName) {
        return (String)m_pragmas.get(unqualTypeName);
    }



    /** gets the fully qualified name for the symbol with the symbolName
     * This method checks if the symbol is present in the Scope and throws an error if not
     */
    public String getFullyQualifiedNameForSymbol(String symbolName) {
        if (getSymbol(symbolName) == null) { throw new RuntimeException("error in scope " + this + ", symbol with name: " + symbolName + " not found"); }
        String namespaceName = getFullyQualifiedScopeName();
        String fullyQualName = namespaceName;
        if (fullyQualName.length() > 0) { fullyQualName += "."; }
        fullyQualName += symbolName;
        return fullyQualName;
    }


    /** returns the fully qualified name of this scope */
    /** @return an array of all scope names. At index 0 the outermost scope name is present */
    public String getFullyQualifiedScopeName() {
        if (getParentScope() == null) { return ""; }
        Stack scopeStack = new Stack();
        scopeStack.push(m_scopeName);
        Scope parent = getParentScope();
        while (parent.getParentScope() != null) {
            scopeStack.push(parent.m_scopeName);
            parent = parent.getParentScope();
        }
        
        String result = "";
        int stackSize = scopeStack.size();
        for (int i = 0; i < stackSize; i++) {
            if (i > 0) { result += "."; }
            result += scopeStack.pop();
        }
        return result;
    }


    /**
     * @see java.lang.Object#toString()
     */
    public String toString() {
        String result = "scope begins: " + m_scopeName + "\n";
        // symbols
        Enumeration symEnum = m_symbols.elements();
        while (symEnum.hasMoreElements()) {
            result += (symEnum.nextElement() + "\n");
        }
        // scopes
        Enumeration scopeEnum = getChildScopeEnumeration();
        while (scopeEnum.hasMoreElements()) {
            result += (scopeEnum.nextElement() + "\n");
        }
        result += ("scope ends");
        return result;
    }

}
