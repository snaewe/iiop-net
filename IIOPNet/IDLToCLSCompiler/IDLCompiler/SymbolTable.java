/* SymbolTable.java
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

package symboltable;

import java.util.Stack;

/**
 * This class stores the scope information encountered during parsing
 * of an IDL file
 */
public class SymbolTable {

    #region IFields

    /** the currently open scope */
    private Scope m_currentScope;
    /** the top scope for this symboltable */
    private Scope m_topScope;

    #endregion IFields
    #region IConstructors

    public SymbolTable() {
        m_currentScope = new Scope("", null);
        m_topScope = m_currentScope;
    }

    #endregion IConstructors
    #region IMethods

    /** opens a scope in the current scope */
    public Scope openScope(String scopeName) {
        if (m_currentScope.containsChildScope(scopeName)) {
            m_currentScope = m_currentScope.getChildScope(scopeName);
        } else {
            Scope newScope = new Scope(scopeName, m_currentScope);
            m_currentScope = newScope;
        }
        return m_currentScope;
    }

    private void openScopes(Stack scopesToOpen) {
        if (scopesToOpen == null) { return; }
        while (!scopesToOpen.isEmpty()) {
            String toOpen = (String)scopesToOpen.pop();
            openScope(toOpen);
        }
    }
    
    /** closes an open scope */
    public void closeScope() throws ScopeException {
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

    #region pragma prefix helpers

    public boolean isPragmaScopeOpen() {
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
        while ((current != null) && (!(current instanceof PragmaScope))) {
            current = current.getParentScope();
        }
        return (PragmaScope)current;
    }

    /** open a pragma-scope */
    public Scope openPragmaScope(String scopeName) {
        if (!(scopeName.equals(""))) {
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
    public String toString() {
        String result = "symbol table contents\n";
        result += m_topScope.toString();
        return result;
    }

    #endregion IMethods

}
