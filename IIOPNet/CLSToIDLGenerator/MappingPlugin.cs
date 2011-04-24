/* MappingPlugin.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 21.08.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Xml;
using System.Xml.Schema;
using System.IO;


namespace Ch.Elca.Iiop.Idl {

    
    /// <summary>
    /// mangages the plugged mappings. The registry is used to find out, if a custom mapping is 
    /// defined and it is used to retrieve the information needed to perform this mapping.
    /// </summary>
    public class GeneratorMappingPlugin {

        #region Constants

        private const string XSD_RESOURCE_NAME = "GeneratorMappingPluginSchema.xsd";

        #endregion Constants
        #region SFields

        private static GeneratorMappingPlugin s_registry;

        #endregion SFields
        #region IFields
        
        private Hashtable m_mappingTable = new Hashtable();

        private Hashtable m_inverseMappingTable = new Hashtable();

        private XmlSchema m_mappingPluginSchema;

        #endregion IFields
        #region SConstructor
 
        static GeneratorMappingPlugin() {
            s_registry = new GeneratorMappingPlugin();
        }

        #endregion SConstructor
        #region IConstructors

        private GeneratorMappingPlugin() {
            // load xsd schema from channel assembly
            Assembly asm = GetType().Assembly;
            Stream xsdStream = asm.GetManifestResourceStream(XSD_RESOURCE_NAME);
            if (xsdStream != null) {
                m_mappingPluginSchema = XmlSchema.Read(xsdStream, new ValidationEventHandler(this.OnValidationEvent));
                xsdStream.Close();
            }
        }

        #endregion IConstructors
        #region SMethods

        public static GeneratorMappingPlugin GetSingleton() {
            return s_registry;
        }

        #endregion SMethods
        #region IMethods

        /// <summary>
        /// is called on errors in schema
        /// </summary>
        /// <remarks>should not be called, because schema is ok</remarks>
        private void OnValidationEvent(object sender, ValidationEventArgs e) {
        }

        /// <summary>
        /// checks, if a special mapping for the native clsType clsType exists.
        /// </summary>
        /// <param name="clsType">the native cls type to check for.</param>
        /// <returns></returns>
        public bool IsCustomMappingPresentForCls(Type clsType) {
            return m_mappingTable.Contains(clsType);
        }

        /// <summary>
        /// checks, if a special mapping for the target idl-type idlType exists.
        /// </summary>
        /// <param name="idlType">the idl-type to check for.</param>
        /// <returns></returns>
        public bool IsCustomMappingTarget(Type idlType) {
            return m_inverseMappingTable.Contains(idlType);
        }

        /// <summary>
        /// specify a special mapping, e.g. CLS ArrayList <=> java.util.ArrayList.
        /// </summary>
        /// <param name="clsType">the native cls type, e.g. ArrayList</param>
        /// <param name="idlType">the target idl type (mapped from idl to CLS)</param>
        /// <param name="idlFileName">the file containing the idl for the target idl-type</param>
        private void AddMapping(Type clsType, Type idlType, string idlFileName) {            
            // check that idlType implements IIdlEntity:
            Type idlEntityType = typeof(IIdlEntity);
            if (!(idlEntityType.IsAssignableFrom(idlType))) {
                throw new Exception("illegal target type for custom mapping encountered: " + idlType.FullName);
            }
            // mapping must be bijective, i.e. for an idl type only one cls type is allowed and vice versa
            if (m_inverseMappingTable.ContainsKey(idlType) && (!((CustomMappingDesc)m_inverseMappingTable[idlType]).ClsType.Equals(clsType))) {
                throw new Exception("mapping constraint violated, tried to insert another cls type " + clsType + 
                                     "mapped to the idl type " + idlType);
            }

            GeneratorCustomMappingDesc desc = new GeneratorCustomMappingDesc(clsType, idlType, idlFileName);
            m_mappingTable[clsType] = desc;           
            m_inverseMappingTable[idlType] = desc;
            // add also to the channel custom mapper reg for CLS to IDL mapper
            CustomMapperRegistry reg = CustomMapperRegistry.GetSingleton();
            reg.AddMapping(clsType, idlType, null);
        }
        
        /// <summary>
        /// adds special mappings from a config file
        /// </summary>
        public void AddMappingsFromFile(FileInfo configFile) {
            // check schema ok
            if (m_mappingPluginSchema == null) {
                throw new Exception("schema loading problem");
            }
            // load the xml-file
            XmlDocument doc = new XmlDocument();
            FileStream stream = new FileStream(configFile.FullName, FileMode.Open);
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.Schemas.Add(m_mappingPluginSchema);
            XmlReader validatingReader = XmlReader.Create(stream, settings);

            try {
                doc.Load(validatingReader);
                // process the file
                XmlNodeList elemList = doc.GetElementsByTagName("mapping");
                foreach (XmlNode elem in elemList) {
                    XmlElement clsTypeAsqName = elem["clsType"];
                    XmlElement idlTypeName = elem["idlTypeName"];
                    XmlElement idlTypeAsm = elem["idlTypeAssembly"];
                    XmlElement idlFileName = elem["idlFile"];
                    // idlType:
                    String asmQualIdlName = idlTypeName.InnerText + "," + idlTypeAsm.InnerText;
                    Type idlType = Type.GetType(asmQualIdlName, true);
                    // clsType:
                    Type clsType = Type.GetType(clsTypeAsqName.InnerText, true);
                    AddMapping(clsType, idlType, idlFileName.InnerText);
                }
            } finally {
                validatingReader.Close();
            }
        }

        /// <summary>
        /// returns all information required to perform the custom mapping given the clsType.
        /// </summary>
        /// <param name="clsType">the native cls type the mapping should be retrieved for</param>
        /// <returns></returns>
        public GeneratorCustomMappingDesc GetMappingForCls(Type clsType) {
            return (GeneratorCustomMappingDesc)m_mappingTable[clsType];
        }

        /// <summary>
        /// returns all information required to perform the custom mapping given the target
        /// idlType.
        /// </summary>
        /// <param name="idlType">the idl type the mapping should be retrieved for</param>
        /// <returns></returns>
        public GeneratorCustomMappingDesc GetMappingForIdlTarget(Type idlType) {
            return (GeneratorCustomMappingDesc)m_inverseMappingTable[idlType];
        }

        #endregion IMethods

    }

    /// <summary>
    /// stores information on how the custom mapping should be done.
    /// </summary>
    public class GeneratorCustomMappingDesc {
        
        #region IFields

        private Type m_clsType;
        private Type m_idlType;
        private String m_idlFileName;
        
        #endregion IFields
        #region IConstructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clsType"></param>
        /// <param name="idlType"></param>
        /// <param name="mapper"></param>
        /// <remarks>precondition: clsType != null, idlType != null, idlFileName != null</remarks>
        public GeneratorCustomMappingDesc(Type clsType, Type idlType, string idlFileName) {
            m_clsType = clsType;
            m_idlType = idlType;
            m_idlFileName = idlFileName;
        }

        #endregion IConstructors
        #region IProperties

        /// <summary>
        /// the idl type the mapper maps from/to.
        /// </summary>
        public Type IdlType {
            get { 
                return m_idlType;
            }
        }

        /// <summary>
        /// the cls type the mapper maps from/to.
        /// </summary>
        public Type ClsType {
            get { 
                return m_clsType;
            }
        }

        /// <summary>
        /// the name of the idl file containing the idl for the custom mapped cls type
        /// </summary>
        public string IdlFileName {
            get {
                return m_idlFileName;
            }
        }

        #endregion IProperties

    }


}
