/* ModuleBuilderManager.cs
 * 
 * Project: IIOP.NET
 * IDLToCLSCompiler
 * 
 * WHEN      RESPONSIBLE
 * 24.02.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
using symboltable;

namespace Ch.Elca.Iiop.IdlCompiler.Action {


/// <summary>
/// manages the modules in creation.
/// </summary>
public class ModuleBuilderManager {
    
    #region IFields

    private AssemblyBuilder m_asmBuilder;
    private String m_targetAsmName;

    private Hashtable m_moduleBuilders = new Hashtable();

    #endregion IFields
    #region IConstructors

    public ModuleBuilderManager(AssemblyBuilder assemblyBuilder, 
                                String targetAsmName) {
        m_asmBuilder = assemblyBuilder;
        m_targetAsmName = targetAsmName;
    }

    #endregion IConstructors
    #region IMethods

    /// <summary>
    /// get a modulebuilder which is responsible for the specified scope
    /// </summary>
    public ModuleBuilder GetOrCreateModuleBuilderFor(Scope scope) {
        String modName = null;
        modName = GetModuleName(scope);
        ModuleBuilder result = GetModuleBuilderFor(scope);
        if (result == null) {    
            // create a new module builder for the scope
            result = m_asmBuilder.DefineDynamicModule(modName, modName);
            m_moduleBuilders.Add(scope, result);
        }
        return result;
    }

    public ModuleBuilder GetModuleBuilderFor(Scope scope) {
        String modName = null;
        modName = GetModuleName(scope);
        if (m_moduleBuilders.ContainsKey(scope)) {
            return (ModuleBuilder)m_moduleBuilders[scope];    
        } else if (m_asmBuilder.GetDynamicModule(modName) != null) { // needed if independant idl-files are specified at compiler command line arguments
            return (ModuleBuilder) m_asmBuilder.GetDynamicModule(modName);
        } else {
            return null;
        }
    }

    /** construct the name of the target module */
    private String GetModuleName(Scope scope) {
        String modName = scope.getFullyQualifiedScopeName();
        modName = modName.Replace(':', '_');
        modName = modName.Replace('.', '_');
        modName = modName.Trim();
        if (modName.Equals("")) { 
            modName = "default"; 
        } else {
            modName = "_" + modName; 
        }
        modName = "_" + m_targetAsmName + modName + ".netmodule";
        return modName;        
    }

    #endregion IMethods

    
}

}