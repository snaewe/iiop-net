/* TypesInAssemblyManager.java
 * 
 * Project: IIOP.NET
 * IDLToCLSCompiler
 * 
 * WHEN      RESPONSIBLE
 * 23.07.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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



package Ch.Elca.Iiop.IdlCompiler.Action;

import Ch.Elca.Iiop.Idl.RepositoryIDAttribute;

import java.util.Hashtable;
import java.util.LinkedList;

import System.Reflection.*;
import System.Reflection.Emit.*;
import System.Type;
import symboltable.Symbol;
import symboltable.Scope;


/** manages types defined in referenced assemblies */
public class TypesInAssemblyManager {

    #region IFiels

    private LinkedList m_refAssemblies;

    #endregion IFields
    #region IConstructors

    public TypesInAssemblyManager(LinkedList refAssemblies) {
        m_refAssemblies = refAssemblies;
    }

    #endregion IConstructors
    #region IMethods

    /** 
     * checkes, if a type for the symbol forSymbol is defined in
     * a referenced assembly
     * @return the Type if found, otherwise null
     */
    public Type GetTypeFromRefAssemblies(Symbol forSymbol) {
        Type result = null;
        for (int i = 0; i < m_refAssemblies.size(); i++) {
            Assembly asm = (Assembly)m_refAssemblies.get(i);
            Scope symScope = forSymbol.getDeclaredIn();
            String fullName = symScope.getFullyQualifiedNameForSymbol(forSymbol.getSymbolName());
            result = asm.GetType(fullName);
            if (result != null) {
                String repId = symScope.getRepositoryIdFor(forSymbol.getSymbolName());
                if (TypeRepIdEquals(result, repId)) {
                    break;    
                } else {
                    System.out.println("warning: found a type in a referenced assembly, with the same fully qualified name, but a different rep-id.\n" +
                                       " -> do not use this type, instead of regeneration. (repId: " + repId + ")");
                }                
            }
        }
        return result;
    }

    /** checks, if the type toCheck has a repository Id with id repId associated.
     * @return true if present and equal, false otherwise
     */
    private boolean TypeRepIdEquals(Type toCheck, String repId) {
        if (repId != null) {
            // check that repId is present on type from asm and that it is equal
            Object[] attrs = toCheck.GetCustomAttributes(RepositoryIDAttribute.class.ToType(), true);
            if (attrs.length > 0) {
                RepositoryIDAttribute repIdAttr = (RepositoryIDAttribute)attrs[0];
                if (repIdAttr.get_Id().equals(repId)) {
                    return true;
                } else {
                    // not equal
                    return false;
                }
            } else {
                // no rep-id attr found
                return false;
            }
        } else {
            // no rep-id to check -> ok
            return true;
        }    
    }

    #endregion IMethods


}