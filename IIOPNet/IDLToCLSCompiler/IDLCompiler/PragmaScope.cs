/* PragmaScope.cs
 * 
 * Project: IIOP.NET
 * IDLToCLSCompiler
 * 
 * WHEN      RESPONSIBLE
 * 23.02.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
/// represents a scope which is constructed because of a #pragma prefix directive
/// </summary>
public class PragmaScope : Scope {

    #region IConstructors

    public PragmaScope(String name, Scope parentScope) : base(name, parentScope, false) {
    }

    #endregion IConstructors
    #region SMethods

    /** returns a Stack of all scope names from a child scope of the pragma scope up to the pragmascope itself
     */
    public static Stack getPathToPragmaScope(Scope fromScope) {
        Scope current = fromScope;
        Stack result = new Stack();
        while ((current != null) && (!(current is PragmaScope))) {
            result.Push(current.getScopeName());
            current = current.getParentScope();
        }
        if (!(current is PragmaScope)) {
            result = new Stack();
        }
        return result;
    }

    /// <summary>
    /// constructs a repository id part for this scope and the parent scopes.
    /// </summary>  
    public override string ConstructRepositoryIDPart() {
        return getScopeName(); // special for pragma prefix; according to CORBA 2.6 standard, chapter 10.6.5.4        
    }

    #endregion SMethods
}

}
