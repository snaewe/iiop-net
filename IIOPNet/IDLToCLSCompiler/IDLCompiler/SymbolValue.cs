/* SymbolValue.cs
 * 
 * Project: IIOP.NET
 * IDLToCLSCompiler
 * 
 * WHEN      RESPONSIBLE
 * 03.10.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using parser;

namespace symboltable {

/// <summary>
/// represents a symbol for a value, like an enumerator in an enumeration, in the symbol table
/// </summary>
public class SymbolValue : Symbol {

    #region IFields
    
    private Literal m_valueAsLiteral;
    
    #endregion IFields
    #region IConstructors

    public SymbolValue(String symbolName, Scope declaredIn) : base(symbolName, declaredIn) {
    }

    #endregion IConstructors
    #region IMethods
    
    public override String ToString() {
        return " declaration with name: " + m_symbolName;
    }
    
    public void SetValueAsLiteral(Literal value) {
        m_valueAsLiteral = value;
    }
    
    public object GetValue() {
        return m_valueAsLiteral.GetValue();
    }

    public Literal GetValueAsLiteral() {
        return m_valueAsLiteral;
    }
    
    #endregion IMethods

}

}