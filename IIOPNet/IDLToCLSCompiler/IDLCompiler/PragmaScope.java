/* PragmaScope.java
 * 
 * Project: IIOP.NET
 * IDLToCLSCompiler
 * 
 * WHEN      RESPONSIBLE
 * 23.02.03  Dominic Ullmann (DUL), dul@elca.ch
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
 * represents a scope which is constructed because of a #pragma prefix directive
 */
public class PragmaScope extends Scope {

    #region IConstructors

    public PragmaScope(String name, Scope parentScope) {
        super(name, parentScope);
    }

    #endregion IConstructors
    #region SMethods

    /** returns a Stack of all scope names from a child scope of the pragma scope up to the pragmascope itself
     */
    public static Stack getPathToPragmaScope(Scope fromScope) {
        Scope current = fromScope;
        Stack result = new Stack();
        while ((current != null) && (!(current instanceof PragmaScope))) {
            result.push(current.getScopeName());
            current = current.getParentScope();
        }
        if (!(current instanceof PragmaScope)) {
            result = new Stack();
        }
        return result;
    }

    #endregion SMethods
}
