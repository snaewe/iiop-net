/* SimpleNodeWithIdent.cs
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

namespace parser {

/**
 * 
 * This node is used as a base class for nodes, which are created out of a grammar rule,
 * containing an ident;
 * e.g. for node ASTenum_type for the enum type rule.
 * Remark: in the grammar file IDL.jjt, setIdent is called to set the ident for the node.
 * 
 * 
 * @version 
 * @author dul
 * 
 *
 */
public abstract class SimpleNodeWithIdent : SimpleNode {

    #region IFields
    
    private String m_ident;

    #endregion IFields
    #region IConstructors

    /**
     * Constructor for ASTNodeWithIdent.
     * @param i
     */
    public SimpleNodeWithIdent(int i) : base (i) {
    }

    /**
     * Constructor for ASTNodeWithIdent.
     * @param p
     * @param i
     */
    public SimpleNodeWithIdent(IDLParser p, int i) : base(p, i) {
    }

    #endregion IConstructors
    #region IMethods    
    
    public String getIdent() {
        return m_ident;
    }
    public void setIdent(String ident) {
        m_ident = ident;
    }

    #endregion IMethods

}

}