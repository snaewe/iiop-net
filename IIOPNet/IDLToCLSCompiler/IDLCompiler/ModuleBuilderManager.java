/* ModuleBuilderManager.java
 * 
 * Project: IIOP.NET
 * IDLToCLSCompiler
 * 
 * WHEN      RESPONSIBLE
 * 24.02.03  Dominic Ullmann (DUL), dul@elca.ch
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


package action;

import System.Reflection.*;
import System.Reflection.Emit.*;

import java.util.Hashtable;
import symboltable.Scope;

/**
 * manages the modules in creation.
 */
public class ModuleBuilderManager {
    
    private AssemblyBuilder m_asmBuilder;
    private String m_targetAsmName;

    private Hashtable m_moduleBuilders = new Hashtable();

    public ModuleBuilderManager(AssemblyBuilder assemblyBuilder, String targetAsmName) {
        m_asmBuilder = assemblyBuilder;
        m_targetAsmName = targetAsmName;
    }

    /** get a modulebuilder which is responsible for the specified scope */
    public ModuleBuilder getOrCreateModuleBuilderFor(Scope scope) {
        String modName = null;
        modName = getModuleName(scope);
        ModuleBuilder result = getModuleBuilderFor(scope);
        if (result == null) {    
            // create a new module builder for the scope
            result = m_asmBuilder.DefineDynamicModule(modName, modName);
            m_moduleBuilders.put(scope, result);
        }
        return result;
    }

    public ModuleBuilder getModuleBuilderFor(Scope scope) {
        String modName = null;
        modName = getModuleName(scope);
        if (m_moduleBuilders.containsKey(scope)) {
            return (ModuleBuilder)m_moduleBuilders.get(scope);    
        } else if (m_asmBuilder.GetDynamicModule(modName) != null) { // needed if independant idl-files are specified at compiler command line arguments
            return (ModuleBuilder) m_asmBuilder.GetDynamicModule(modName);
        } else {
            return null;
        }
    }

    /** construct the name of the target module */
    private String getModuleName(Scope scope) {
        String modName = scope.getFullyQualifiedScopeName();
        modName = modName.replace(':', '_');
        modName = modName.replace('.', '_');
        modName = modName.trim();
        if (modName.equals("")) { modName = "default"; } 
        else { modName = "_" + modName; }
        modName = "_" + m_targetAsmName + modName + ".netmodule";
        return modName;        
    }

    
}
