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

import System.Reflection.Assembly;
import System.Xml.XmlDocument;
import System.Xml.XmlNode;
import System.Xml.XmlElement;
import System.Xml.XmlNodeList;
import System.Xml.XmlTextReader;
import System.Xml.XmlValidatingReader;
import System.Xml.Schema.XmlSchema;
import System.Xml.Schema.ValidationEventHandler;
import System.Xml.Schema.ValidationEventArgs;
import System.Xml.Schema.XmlSchemaException;

import System.IO.FileInfo;
import System.IO.FileStream;
import System.IO.FileMode;
import System.IO.Stream;

public class CompilerMappingPlugin {
	
    #region Constants

    private final static String XSD_RESOURCE_NAME = "MappingPluginSchema.xsd";

    #endregion Constants
    #region SIFields
	
	private static CompilerMappingPlugin s_mappingPlugin = new CompilerMappingPlugin();
	
	#endregion SFields
	#region IFields
	
	private System.Collections.Hashtable m_mappingTable = new System.Collections.Hashtable();

    private XmlSchema m_mappingPluginSchema;
	
	#endregion IFields
	#region IConstructors
	
	private CompilerMappingPlugin() {
        // load xsd schema from compiler assembly
        Assembly asm = GetType().get_Assembly();
        Stream xsdStream = asm.GetManifestResourceStream(XSD_RESOURCE_NAME);
        if (xsdStream != null) {
            m_mappingPluginSchema = XmlSchema.Read(xsdStream, new ValidationEventHandler(this.OnValidationEvent));
            xsdStream.Close();
        }
	}
	
	#endregion IConstructors
	#region SMethods
	
	public static CompilerMappingPlugin GetSingleton() {
		return s_mappingPlugin;
	}
	
	#endregion SMethods
	#region IMethods

	/** 
     * is called on errors in schema.
     * remark: should not be called, because schema is ok
     */ 
    private void OnValidationEvent(System.Object sender, ValidationEventArgs e) {
    }

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
     
    public void AddMappingsFromFile(FileInfo configFile) {
        // check schema ok
        if (m_mappingPluginSchema == null) {
            throw new RuntimeException("schema loading problem");
        }
        // load the xml-file
        XmlDocument doc = new XmlDocument();
        FileStream stream = new FileStream(configFile.get_FullName(), FileMode.Open);
        XmlTextReader textReader = new XmlTextReader(stream);            
        XmlValidatingReader validatingReader = new XmlValidatingReader(textReader);
        try {
            validatingReader.get_Schemas().Add(m_mappingPluginSchema);
            try {
                doc.Load(validatingReader);
            } catch (XmlSchemaException e) {
                throw new RuntimeException("validation failed: " + e.ToString());
            }
            // process the file
            XmlNodeList elemList = doc.GetElementsByTagName("mapping");
            for (int i = 0; i < elemList.get_Count(); i++) {
                XmlNode elem = elemList.get_ItemOf(i);
                XmlElement idlTypeName = elem.get_Item("idlTypeName");
                XmlElement clsTypeAsqName = elem.get_Item("clsType");
                // idl-type name:
                System.String idlType = idlTypeName.get_InnerText();
                // clsType:
                System.Type clsType = System.Type.GetType(clsTypeAsqName.get_InnerText(), true);
                AddMapping(idlType, clsType);
            }
        } finally {
            validatingReader.Close();            
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
