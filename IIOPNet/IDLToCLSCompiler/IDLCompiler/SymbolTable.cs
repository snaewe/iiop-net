/* SymbolTable.cs
 * 
 * Project: IIOP.NET
 * IDLToCLSCompiler
 * 
 * WHEN      RESPONSIBLE
 * 18.02.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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

namespace symboltable {

/// <summary>
/// This class stores the scope information encountered during parsing
/// of an IDL file
/// </summaray>
public class SymbolTable {

    #region IFields

    /** the currently open scope */
    private Scope m_currentScope;
    /** the top scope for this symboltable */
    private Scope m_topScope;

    #endregion IFields
    #region IConstructors

    public SymbolTable() {
        m_currentScope = new Scope("", null, false);
        m_topScope = m_currentScope;
        AddPredefinedSymbols();
    }

    #endregion IConstructors
    #region IMethods
    
    private void AddPredefinedSymbols() {
        // add CORBA::TypeCode
        openPragmaScope("omg.org");
        openScope("CORBA");
        getCurrentScope().addSymbol("TypeCode");
        getCurrentScope().addSymbol("AbstractBase");
        closeScope();
        closePragmaScope();
    }

    /** opens a scope in the current scope */
    public Scope openScope(String scopeName, bool isTypeScope) {
        if (m_currentScope.containsChildScope(scopeName)) {
            m_currentScope = m_currentScope.getChildScope(scopeName);
        } else {
            Scope newScope = new Scope(scopeName, m_currentScope, isTypeScope);
            m_currentScope = newScope;
        }
        return m_currentScope;
    }

    /** opens a scope in the current scope */
    private Scope openScope(String scopeName) {
        return openScope(scopeName, false);
    }

    private void openScopes(Stack scopesToOpen) {
        if (scopesToOpen == null) { 
            return; 
        }
        while (!(scopesToOpen.Count == 0)) {
            String toOpen = (String)scopesToOpen.Pop();
            openScope(toOpen);
        }
    }
    
    /// <summary>
    /// closes an open scope
    /// </summary>
    public void closeScope() {
        if (m_currentScope == m_topScope) {
            throw new ScopeException("top scope can't be closed");
        }
        m_currentScope = m_currentScope.getParentScope();
    }
    
    /** gets the current scope */
    public Scope getCurrentScope() {
        return m_currentScope;
    }
    
    /** gets the top scope */
    public Scope getTopScope() {
        return m_topScope;
    }

    /// <summary>
    /// resolve a scoped name to a symbol starting in searchScope 
    /// </summary>
    private Symbol ResolveScopedNameToSymbolFromScope(Scope searchScope, IList parts) {
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
    
    /// <summary>search starting from serachScope for a scope with name scopeNameParts
    private Scope ResolveScopedNameToScopeFromScope(Scope searchScope, IList parts) {
        if ((parts == null) || (parts.Count == 0)) {
            return null;
        }
        Scope currentScope = searchScope;
        for (int i = 0; i < parts.Count; i++) {
            // resolve scopes
            currentScope = currentScope.getChildScope((String)parts[i]);
            if (currentScope == null) { 
                return null; // not found within this searchScope
            }
        }
        return currentScope;
    }
    
    /// <summary>serach for a scoped name representing a symbol with name parts in searchStartScope and all visible scopes</summary>
    public Symbol ResolveScopedNameToSymbol(Scope searchStartScope, IList parts) {
        Queue scopesToSearch = new Queue();
        IList alreadySearchedScopes = new ArrayList(); // more efficient, don't search two times the same scope.
        scopesToSearch.Enqueue(searchStartScope);
        Symbol found = null;
        // search in this scope and all parent scopes
        while ((found == null) && (scopesToSearch.Count > 0)) {
            Scope searchScope = (Scope)scopesToSearch.Dequeue();
            alreadySearchedScopes.Add(searchScope);
            found = ResolveScopedNameToSymbolFromScope(searchScope, parts);
            // if not found: next scope to search in is parent scope
            if ((searchScope.getParentScope() != null) &&
                (!alreadySearchedScopes.Contains(searchScope.getParentScope()))) {
                // if parent scope not null, search in parent
                scopesToSearch.Enqueue(searchScope.getParentScope());
            }
            // for interfaces, search in inherited scopes as described in CORBA 2.3, section 3.15.2
            // "Inheritance causes all identifiers defined in base interfaces, both direct and indirect, to
            // be visible in derived interfaces"
            foreach (Scope inheritedScope in searchScope.GetInheritedScopes()) {
                if (!alreadySearchedScopes.Contains(inheritedScope)) {
                    scopesToSearch.Enqueue(inheritedScope);
                }
            }
        }    
        return found;
    }
    
    
    /// <summary>serach for a scoped name with name parts in searchStartScope and all visible scopes</summary>
    public Scope ResolveScopedNameToScope(Scope searchStartScope, IList parts) {
        
        IList alreadySearchedScopes = new ArrayList(); // more efficient, don't search two times the same scope.
        Queue scopesToSearch = new Queue();
        scopesToSearch.Enqueue(searchStartScope);
        Scope found = null;
        // search in this scope and all parent scopes
        while ((found == null) && (scopesToSearch.Count > 0)) {
            Scope searchScope = (Scope)scopesToSearch.Dequeue();
            alreadySearchedScopes.Add(searchScope);
            found = ResolveScopedNameToScopeFromScope(searchScope, parts);
            // if not found: next scope to search in is parent scope
            if ((searchScope.getParentScope() != null) && 
                (!alreadySearchedScopes.Contains(searchScope.getParentScope()))) {
                // if parent scope not null, search in parent
                scopesToSearch.Enqueue(searchScope.getParentScope());
            }
            // search also in inherited Scopes            
            foreach (Scope inheritedScope in searchScope.GetInheritedScopes()) {
                if (!alreadySearchedScopes.Contains(inheritedScope)) {
                    scopesToSearch.Enqueue(inheritedScope);
                }
            }
            
        }        
               
        return found;
    }

    #region pragma prefix helpers

    public bool isPragmaScopeOpen() {
        Scope scope = getOpenPragmaScope();
        if (scope == null) {
            return false;
        } else {
            return true;
        }
    }

    /** get the opened pragma scope, if one is open */
    public PragmaScope getOpenPragmaScope() {
        Scope current = getCurrentScope();
        while ((current != null) && (!(current is PragmaScope))) {
            current = current.getParentScope();
        }
        return (PragmaScope)current;
    }

    /** open a pragma-scope */
    public Scope openPragmaScope(String scopeName) {
        if (!(scopeName.Equals(""))) {
            if (m_currentScope.containsChildScope(scopeName)) {
                m_currentScope = m_currentScope.getChildScope(scopeName);
            } else {
                Scope newScope = new PragmaScope(scopeName, m_currentScope);
                m_currentScope = newScope;
            }
        }
        return m_currentScope;
    }

    /** close the opened pragma scope */
    public void closePragmaScope() {
        PragmaScope scopeToClose = getOpenPragmaScope();
        if (scopeToClose != null) {
            m_currentScope = scopeToClose.getParentScope();
        }
    }

    #endregion

    /** assures, that for all fwd decls, a full definition is present. */
    public void CheckAllFwdDeclsComplete() {
        getTopScope().CheckAllFwdCompleted();
    }


    /**
     * @see java.lang.Object#toString()
     */
    public override String ToString() {
        String result = "symbol table contents\n";
        result += m_topScope.ToString();
        return result;
    }

    #endregion IMethods

}

}
