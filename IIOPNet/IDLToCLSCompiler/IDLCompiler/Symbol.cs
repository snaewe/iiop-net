/* Symbol.cs
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

namespace symboltable {

/// <summary>
/// represents a Symbol in the symbol table
/// </summary>
public abstract class Symbol {

    #region IFields

    private Scope m_declaredIn;

    protected String m_symbolName;

    #endregion IFields
    #region IConstructors

    public Symbol(String symbolName, Scope declaredIn) {
        m_symbolName = symbolName;
        m_declaredIn = declaredIn;
    }
    
    #endregion IConstructors
    #region IMethods        

    public Scope getDeclaredIn() {
        return m_declaredIn;
    }

    public String getSymbolName() {
        return m_symbolName;
    }

    public string ConstructRepositoryId() {
        string scopeIdParent = getDeclaredIn().ConstructRepositoryIDPart();
        if (!scopeIdParent.Equals("")) {
            scopeIdParent = scopeIdParent + "/";    
        }
        return "IDL:" + scopeIdParent + (!m_symbolName.StartsWith("_") ? m_symbolName : m_symbolName.Substring(1)) +
               ":1.0";
    }    

    #endregion IMethods

}

}