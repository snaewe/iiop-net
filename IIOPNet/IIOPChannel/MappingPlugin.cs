/* MappingPlugin.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 06.08.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
    /// this interface must be implemented to plugin a custom serialisation mapping.
    /// </summary>
    public interface ICustomMapper {

        /// <summary>
        /// Creates the instance to marshal for the CLS instance.    
        /// Example: For a CLS ArrayList, which is mapped to a java ArrayList, this method must return an ArrayListImpl
        /// for the CLS ArrayList. This ArrayListImpl will then be marshalled.
        /// </summary>    
        /// <param name="clsObject">the cls instance to convert</param>
        /// <returns>the converted instance used for serialisation</returns>
        object CreateIdlForClsInstance(object clsObject);

        /// <summary>
        /// Creates the CLS instance for the deserialised IDL instance. This instance will be returned to the user.
        /// Example: For a serialised java ArrayList received an ArrayListImpl instance is created. This method must convert
        /// this instance to a CLS ArrayList instance.
        /// </summary>
        /// <param name="idlObject">the deserialised IDL instance</param>
        /// <returns>the converted instance</returns>
        object CreateClsForIdlInstance(object idlObject);

    }

    
    /// <summary>
    /// mangages the plugged mappings. The registry is used to find out, if a custom mapping is 
    /// defined and it is used to retrieve the information needed to perform this mapping.
    /// </summary>
    public class CustomMapperRegistry {

        #region Constants

        private const string XSD_RESOURCE_NAME = "MappingPluginSchema.xsd";

        #endregion Constants
        #region SFields

        private static CustomMapperRegistry s_registry;

        #endregion SFields
        #region IFields
        
        private Hashtable m_mappingsCls = new Hashtable();

        private Hashtable m_mappingsIdl = new Hashtable();

        private XmlSchema m_mappingPluginSchema;

        #endregion IFields
        #region SConstructor
 
        static CustomMapperRegistry() {
            s_registry = new CustomMapperRegistry();
        }

        #endregion SConstructor
        #region IConstructors

        private CustomMapperRegistry() {
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

        public static CustomMapperRegistry GetSingleton() {
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
            return m_mappingsCls.Contains(clsType);
        }

        /// <summary>
        /// checks, if a special mapping for the idl-type idlType exists.
        /// </summary>
        /// <param name="clsType">the idl-type to check for.</param>
        /// <returns></returns>
        public bool IsCustomMappingPresentForIdl(Type idlType) {
            return m_mappingsIdl.Contains(idlType);
        }

        /// <summary>
        /// specify a special mapping, e.g. CLS ArrayList <=> java.util.ArrayList.
        /// </summary>
        /// <param name="clsType">the native cls type, e.g. ArrayList</param>
        /// <param name="idlType">the idl type (mapped from idl to CLS) used to describe serialisation / deserialisation, e.g. java.util.ArrayListImpl</param>
        /// <param name="mapper">the mapper, knowing how to map instances of CLS ArrayList to java.util.ArrayListImpl and in the other direction</param>
        public void AddMapping(Type clsType, Type idlType, ICustomMapper mapper) {
            CustomMappingDesc desc = new CustomMappingDesc(clsType, idlType, mapper);
            m_mappingsCls[clsType] = desc;
            m_mappingsIdl[idlType] = desc;
            // check for impl class attribute, if present: add impl class here too
            object[] implAttr = idlType.GetCustomAttributes(typeof(ImplClassAttribute), false);
            if ((implAttr != null) && (implAttr.Length > 0)) {
                ImplClassAttribute implCl = (ImplClassAttribute) implAttr[0];
                // get the type
                Type implIdlType = Repository.LoadType(implCl.ImplClass);
                CustomMappingDesc descImpl = new CustomMappingDesc(clsType, implIdlType, mapper);
                m_mappingsIdl[implIdlType] = descImpl;
            }
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
            XmlTextReader textReader = new XmlTextReader(stream);            
            XmlValidatingReader validatingReader = new XmlValidatingReader(textReader);
            try {
                validatingReader.Schemas.Add(m_mappingPluginSchema);
                doc.Load(validatingReader);
                // process the file
                XmlNodeList elemList = doc.GetElementsByTagName("mapping");
                foreach (XmlNode elem in elemList) {                 
                    XmlElement idlTypeName = elem["idlTypeName"];
                    XmlElement idlTypeAsm = elem["idlTypeAssembly"];
                    XmlElement clsTypeAsqName = elem["clsType"];
                    XmlElement customMapperElem = elem["customMapper"];
                    // idlType:
                    String asmQualIdlName = idlTypeName.InnerText + "," + idlTypeAsm.InnerText;
                    Type idlType = Type.GetType(asmQualIdlName, true);
                    // clsType:
                    Type clsType = Type.GetType(clsTypeAsqName.InnerText, true);
                    // custom Mapper:
                    ICustomMapper customMapper = null;
                    if (customMapperElem != null) {
                        Type customMapperType = Type.GetType(customMapperElem.InnerText, true);
                        customMapper = (ICustomMapper)Activator.CreateInstance(customMapperType);                     
                    }
                    AddMapping(clsType, idlType, customMapper);
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
        public CustomMappingDesc GetMappingForCls(Type clsType) {
            return (CustomMappingDesc)m_mappingsCls[clsType];
        }

        /// <summary>
        /// returns all information required to perform the custom mapping given the clsType.
        /// </summary>
        /// <param name="idlType">the idl type the mapping should be retrieved for</param>
        /// <returns></returns>
        public CustomMappingDesc GetMappingForIdl(Type idlType) {
            return (CustomMappingDesc)m_mappingsIdl[idlType];
        }

        #endregion IMethods

    }

    /// <summary>
    /// stores information on how the custom mapping should be done.
    /// </summary>
    public class CustomMappingDesc {
        
        #region IFields

        private Type m_clsType;
        private Type m_idlType;
        private ICustomMapper m_mapper;
        
        #endregion IFields
        #region IConstructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clsType"></param>
        /// <param name="idlType"></param>
        /// <param name="mapper"></param>
        /// <remarks>precondition: clsType != null, idlType != null, mapper != null</remarks>
        public CustomMappingDesc(Type clsType, Type idlType, ICustomMapper mapper) {
            m_clsType = clsType;
            m_idlType = idlType;
            m_mapper = mapper;
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
        /// the mapper maps instances of the idl type to instances of the cls type and
        /// instances of the cls type to the idl type.
        /// </summary>
        public ICustomMapper Mapper {
            get {
                return m_mapper;
            }
        }

        #endregion IProperties

    }


}