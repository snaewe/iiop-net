/* MappingPlugin.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 12.08.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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

public class CompilerMappingPlugin {
	
	#region SIFields
	
	private static CompilerMappingPlugin s_mappingPlugin = new CompilerMappingPlugin();
	
	#endregion SFields
	#region IFields
	
	private System.Collections.Hashtable m_mappingTable = new System.Collections.Hashtable();
	
	#endregion IFields
	#region IConstructors
	
	private CompilerMappingPlugin() {
		
	}
	
	#endregion IConstructors
	#region SMethods
	
	public static CompilerMappingPlugin GetSingleton() {
		return s_mappingPlugin;
	}
	
	#endregion SMethods
	#region IMethods
	
	/**
     * checks, if a special mapping for the idl-type idlType exists.
     * @param idlFullName the fully qualified name of the type to check
     */
     public boolean IsCustomMappingPresentForIdl(System.String idlFullName) {
         return m_mappingTable.Contains(idlFullName);
     }
     
     /**
      * specifies a special mapping from idl to cls, e.g. java.util.ArrayList -> CLS System.Collections.ArrayList
      */
     public void AddMapping(System.String idlFullName, System.Type clsType) {
     	if (!m_mappingTable.Contains(idlFullName)) {
     		m_mappingTable.Add(idlFullName, clsType);
     	}
     }
     
     /**
      * returns the CLS type to which the given idl type should be mapped to
      */
     public System.Type GetMappingForIdl(System.String idlFullName) {
     	return (System.Type)m_mappingTable.get_Item(idlFullName);
     }

	
	#endregion IMethods
	
	
}
