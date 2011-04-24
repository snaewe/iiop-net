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
using Ch.Elca.Iiop.Util;
using omg.org.CORBA;


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
            // check that idlType implements IIdlEntity:
            if (!(ReflectionHelper.IIdlEntityType.IsAssignableFrom(idlType))) {
                throw new Exception("illegal type for custom mapping encountered: " + idlType.FullName);
            }
            // be aware: mapping is not bijektive, because of impl classes; however for an idl type only one
            // cls type is allowed
            if (m_mappingsIdl.ContainsKey(idlType) && (!((CustomMappingDesc)m_mappingsIdl[idlType]).ClsType.Equals(clsType))) {
                throw new Exception("mapping constraint violated, tried to insert another cls type " + clsType +
                                     "mapped to the idl type " + idlType);
            }

            CustomMappingDesc desc = new CustomMappingDesc(clsType, idlType, mapper);
            m_mappingsCls[clsType] = desc;
            m_mappingsIdl[idlType] = desc;
            // check for impl class attribute, if present: add impl class here too
            object[] implAttr = idlType.GetCustomAttributes(ReflectionHelper.ImplClassAttributeType, false);
            if ((implAttr != null) && (implAttr.Length > 0)) {
                ImplClassAttribute implCl = (ImplClassAttribute) implAttr[0];
                // get the type
                Type implIdlType = Repository.GetValueTypeImplClass(implCl.ImplClass);
                if (implIdlType != null) { // if impl type not found, (test needed e.g. when called from CLSToIDLGen)
                    CustomMappingDesc descImpl = new CustomMappingDesc(clsType, implIdlType, mapper);
                    m_mappingsIdl[implIdlType] = descImpl;
                }
            }
        }
 
        /// <summary>
        /// adds special mappings from a config stream
        /// </summary>
        public void AddMappingFromStream(Stream configStream) {
            // check schema ok
            if (m_mappingPluginSchema == null) {
                configStream.Close();
                throw new Exception("schema loading problem");
            }
            XmlDocument doc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.Schemas.Add(m_mappingPluginSchema);
            XmlReader validatingReader = XmlReader.Create(configStream, settings);
            try {
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
        /// adds special mappings from a config file
        /// </summary>
        public void AddMappingsFromFile(FileInfo configFile) {
            // load the xml-file
            FileStream stream = new FileStream(configFile.FullName, FileMode.Open);
            AddMappingFromStream(stream);
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
 
        /// <summary>
        /// takes an instance deserialised from a cdr stream and maps it to the instance
        /// used in .NET. The mapped instance must be assignable to formalSig.
        /// </summary>
        /// <remarks>
        /// Custom mapping must be present; otherwise an exception is thrown.
        /// </remarks>
        public object CreateClsForIdlInstance(object idlInstance, Type formalSig) {
            // for subtype of idl formal support, get acutal mapping
            CustomMappingDesc actualMapping = GetMappingForIdl(idlInstance.GetType());
            if (actualMapping == null) {
                throw new BAD_PARAM(12309, CompletionStatus.Completed_MayBe);
            }
            ICustomMapper mapper = actualMapping.Mapper;
            object result = mapper.CreateClsForIdlInstance(idlInstance);
            // check, if mapped instance is assignable to formal in CLS signature -> otherwise will not work.
            if (!formalSig.IsAssignableFrom(result.GetType())) {
                throw new BAD_PARAM(12311, CompletionStatus.Completed_MayBe);
            }
            return result;
        }
 
        /// <summary>
        /// takes a .NET instance and maps it to the instance, which should be serialised into
        /// the CDR stream. The mapped instance must be assignable to the formal type specified in idl.
        /// </summary>
        /// <remarks>
        /// Custom mapping must be present; otherwise an exception is thrown.
        /// </remarks>
        public object CreateIdlForClsInstance(object clsInstance, Type formal) {
            // for subtypes support (subtypes of formal before cls to idl mapping; new formal is idl formal)
            CustomMappingDesc actualMapping =
                GetMappingForCls(clsInstance.GetType());
            if (actualMapping == null) {
                throw new BAD_PARAM(12308, CompletionStatus.Completed_MayBe);
            }
            ICustomMapper mapper = actualMapping.Mapper;
            object actual = mapper.CreateIdlForClsInstance(clsInstance);
            // check, if mapped is instance is assignable to formal -> otherwise will not work on other side ...
            if (!formal.IsAssignableFrom(actual.GetType())) {
                throw new BAD_PARAM(12310, CompletionStatus.Completed_MayBe);
            }
            return actual;
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



    /// <summary>
    /// configuration singleton, which allows to configure certain aspects of mapping
    /// </summary>
    public class MappingConfiguration {
 
        #region IFields
 
        private bool m_useBoxedInAny;
        private bool m_useWideCharByDefault;
 
        #endregion IFields
        #region SFields
 
        private static MappingConfiguration s_instance = new MappingConfiguration();
 
        #endregion SFields
        #region IConstructors
 
        private MappingConfiguration() {
            m_useBoxedInAny = true; // default is optimal for java rmi/iiop
            m_useWideCharByDefault = true; // default is optimal for java rmi/iiop
        }
 
        #endregion IConstructors
        #region IProperties
 
        /// <summary>
        /// use the boxed form of .NET string, .NET arrays
        /// when passing them in any.
        /// Default is true.
        /// </summary>
        /// <remarks>for JacORB, OmniORB, ... disable this property to simplify any usage:
        /// No need to create any wrapper objects; e.g. with string typecode to prevent passing
        /// a boxed string.</remarks>
        public bool UseBoxedInAny {
            get { return m_useBoxedInAny; }
            set { m_useBoxedInAny = value; }
        }
 
        /// <summary>
        /// gets or sets value indicating wether wide char should be used by default
        /// when no WideCharAttribute is specified
        /// Default is true.
        /// </summary>
        /// <remarks>for ACE+TAO, ... disable this property to simplify any usage:
        /// No need to create any wrapper objects; e.g. with string typecode to prevent passing
        /// a wstring.</remarks>
        public bool UseWideCharByDefault {
            get { return m_useWideCharByDefault; }
            set { m_useWideCharByDefault = value; }
        }

        #endregion IProperties
        #region SProperties
 
        /// <summary>
        /// the singleton instance.
        /// </summary>
        public static MappingConfiguration Instance {
            get {
                return s_instance;
            }
        }
 
        #endregion SProperties
 
    }

}
