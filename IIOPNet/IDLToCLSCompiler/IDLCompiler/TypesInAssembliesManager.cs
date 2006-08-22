/* TypesInAssemblyManager.cs
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

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using symboltable;
using Ch.Elca.Iiop.Idl;

namespace Ch.Elca.Iiop.IdlCompiler.Action {


    /// <summary>
    /// manages types defined in referenced assemblies
    /// </summary> 
    public class TypesInAssemblyManager {

        #region IFiels

        private IList m_refAssemblies;

        #endregion IFields
        #region IConstructors

        public TypesInAssemblyManager(IList refAssemblies) {
            m_refAssemblies = refAssemblies;
        }

        #endregion IConstructors
        #region IMethods

        /// <summary>
        /// checkes, if a type with name typename and repositoryID repId is defined
        /// a referenced assembly         
        /// </summary>
        /// <return>
        /// return the Type if found, otherwise null
        /// </return>
        public Type GetTypeFromRefAssemblies(string fullName, string expectedRepId) {
            Type result = null;
            foreach (Assembly asm in m_refAssemblies){
                result = asm.GetType(fullName);
                if (result != null) {                    
                    if (TypeRepIdEquals(result, expectedRepId)) {
                        break;    
                    } else {
                        Console.WriteLine("warning: found a type in a referenced assembly, " + 
                                          "with the same fully qualified name, " +
                                          "but a different rep-id.\n" +
                                          " -> do not use this type, instead of regeneration. (repId: {0})",
                                          expectedRepId);
                    }                
                }
            }
            return result;
        }

        /// <summary>
        /// checks, if the type toCheck has a repository Id with id repId associated.
        /// </summary> 
        /// <return>
        /// return true if present and equal, false otherwise
        /// </return> 
        private bool TypeRepIdEquals(Type toCheck, String repId) {
            if (repId != null) {
                // check that repId is present on type from asm and that it is equal
                Object[] attrs = toCheck.GetCustomAttributes(typeof(RepositoryIDAttribute), 
                                                             true);
                if (attrs.Length > 0) {
                    RepositoryIDAttribute repIdAttr = (RepositoryIDAttribute)attrs[0];
                    if (repIdAttr.Id.Equals(repId)) {
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


}
