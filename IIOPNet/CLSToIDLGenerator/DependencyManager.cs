/* DependencyManager.cs
 * 
 * Project: IIOP.NET
 * CLSToIDLGenerator
 * 
 * WHEN      RESPONSIBLE
 * 06.02.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Diagnostics;
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Util;

namespace Ch.Elca.Iiop.Idl {

    /// <summary>analyzes types to map for non-mapped dependencies</summary>
    internal class DependencyManager {

        #region IFields

        private Hashtable m_filesForMappedTypes = new Hashtable();

        /// <summary>contains the types, which are already mapped during generation</summary>
        private ArrayList /* MapTypeInfo */ m_alreadyMappedTypes = new ArrayList();

        /// <summary>contains all the default mapped types</summary>
        private ArrayList m_defaultMappedTypes = new ArrayList();

        /// <summary>stores the next types to map</summary>
        private Queue m_toMap = new Queue();

        #endregion IFields
        #region IConstructors

        public DependencyManager() {
            SetUpKnownMappings();
        }

        #endregion IConstructors
        #region IMethods

        private void SetUpKnownMappings() {
            m_defaultMappedTypes.Add(typeof(System.Int16));
            m_defaultMappedTypes.Add(typeof(System.Int32));
            m_defaultMappedTypes.Add(typeof(System.Int64));
            m_defaultMappedTypes.Add(typeof(System.UInt16));
            m_defaultMappedTypes.Add(typeof(System.UInt32));
            m_defaultMappedTypes.Add(typeof(System.UInt64));
            m_defaultMappedTypes.Add(typeof(void));
            m_defaultMappedTypes.Add(typeof(System.Boolean));
            m_defaultMappedTypes.Add(typeof(System.Byte));
            m_defaultMappedTypes.Add(typeof(System.SByte));
            m_defaultMappedTypes.Add(typeof(System.Single));
            m_defaultMappedTypes.Add(typeof(System.Double));
            m_defaultMappedTypes.Add(typeof(System.String));
            m_defaultMappedTypes.Add(typeof(System.Char));
            // some default types
            m_defaultMappedTypes.Add(typeof(System.Object));
            m_defaultMappedTypes.Add(typeof(System.Type));
            m_defaultMappedTypes.Add(typeof(omg.org.CORBA.TypeCode));
            m_defaultMappedTypes.Add(typeof(MarshalByRefObject));
            m_defaultMappedTypes.Add(typeof(omg.org.CORBA.WStringValue));
            m_defaultMappedTypes.Add(typeof(omg.org.CORBA.StringValue));
            m_defaultMappedTypes.Add(typeof(omg.org.CORBA.Any));
        }
 
        /// <summary>
        /// is this type already mapped by default (standard-types)
        /// </summary>
        public bool IsDefaultMapped(Type toMap) {
            return m_defaultMappedTypes.Contains(toMap);
        }
 
        /// <summary>
        /// checks if a custom mapping is specified for the target idl type.
        /// </summary>
        private bool IsCustomMappedToIdlType(Type idlType) {
            if (!(typeof(IIdlEntity).IsAssignableFrom(idlType))) {
                return false;
            }
            GeneratorMappingPlugin mappingPlugin = GeneratorMappingPlugin.GetSingleton();
            return mappingPlugin.IsCustomMappingTarget(idlType);
        }
 
        /// <summary>
        /// checks if a custom mapping is specified from the specified clsType.
        /// </summary>
        private bool IsCustomMappedFromClsType(Type clsType) {
            GeneratorMappingPlugin mappingPlugin = GeneratorMappingPlugin.GetSingleton();
            return mappingPlugin.IsCustomMappingPresentForCls(clsType);
        }
 
        /// <summary>
        /// register a type that is mapped. The idl-definition of this type is stored in the file idlFile
        /// </summary>
        /// <param name="mapped"></param>
        /// <param name="idlFile">the filename of the idlFile containing the mapped type. The filename is relative to the output-directory, does not include the full path</param>
        public void RegisterMappedType(Type mapped, AttributeExtCollection attributes, string idlFile) {
            RegisterMappedType(CreateMapTypeInfo(mapped, attributes), idlFile);
        }
 
        /// <summary>
        /// register a type that is mapped. The idl-definition of this type is stored in the file idlFile
        /// </summary>
        /// <param name="mapped"></param>
        /// <param name="idlFile">the filename of the idlFile containing the mapped type. The filename is relative to the output-directory, does not include the full path</param>
        public void RegisterMappedType(Type mapped, string idlFile) {
            RegisterMappedType(mapped, AttributeExtCollection.EmptyCollection, idlFile);
        }

 
        /// <summary>
        /// register a type that is mapped. The idl-definition of this type is stored in the file idlFile
        /// </summary>
        /// <param name="mapped"></param>
        /// <param name="idlFile">the filename of the idlFile containing the mapped type. The filename is relative to the output-directory, does not include the full path</param>
        private void RegisterMappedType(MapTypeInfo mapped, string idlFile) {
            if (m_alreadyMappedTypes.Contains(mapped) || IsDefaultMapped(mapped.Type)) {
                throw new Exception("reregister of mapped type not possible, already mapped: " + mapped);
            }
            m_alreadyMappedTypes.Add(mapped);
            m_filesForMappedTypes.Add(mapped, idlFile);
        }

        /// <summary>get the name of the idl-file for the mapped type</summary>
        public string GetIdlFileForMappedType(Type mapped) {
            return GetIdlFileForMappedType(mapped, AttributeExtCollection.EmptyCollection);
        }
 
        /// <summary>get the name of the idl-file for the mapped type</summary>
        public string GetIdlFileForMappedType(Type mapped, AttributeExtCollection attributes) {
            return GetIdlFileForMappedType(CreateMapTypeInfo(mapped, attributes));
        }
 
        /// <summary>get the name of the idl-file for the mapped type</summary>
        public string GetIdlFileForMappedType(MapTypeInfo mapped) {
            if (!IsCustomMappedToIdlType(mapped.Type)) {
                object res = m_filesForMappedTypes[mapped];
                return (string) res;
            } else {
                GeneratorMappingPlugin mappingPlugin = GeneratorMappingPlugin.GetSingleton();
                GeneratorCustomMappingDesc mappingDesc = mappingPlugin.GetMappingForIdlTarget(mapped.Type);
                return mappingDesc.IdlFileName;
            }
        }

        /// <summary>checks if a type is already mapped</summary>
        /// <returns>true, if mapped, else false</returns>
        public bool CheckMapped(MapTypeInfo toMap) {
            return (m_alreadyMappedTypes.Contains(toMap) ||
                    IsMappedBeforeGeneration(toMap.Type));
        }
 
        /// <summary>checks if a type is already mapped</summary>
        /// <returns>true, if mapped, else false</returns>
        public bool CheckMappedType(Type toMap) {
            return CheckMappedType(toMap, AttributeExtCollection.EmptyCollection);
        }

        // <summary>checks if a type is already mapped</summary>
        /// <returns>true, if mapped, else false</returns>
        public bool CheckMappedType(Type toMap, AttributeExtCollection attributes) {
            return CheckMapped(CreateMapTypeInfo(toMap, attributes));
        }
 
        public bool IsMappedBeforeGeneration(Type toMap) {
            return (IsDefaultMapped(toMap) ||
                    ReflectionHelper.IIdlEntityType.IsAssignableFrom(toMap) ||
                    IsCustomMappedFromClsType(toMap));
        }

        /// <summary>returns the dependency information for Type forType</summary>
        /// <param name="forType">the type to determine the dependencies of</param>
        /// <param name="attributesAfterMap">the attributes staying after removeing the ones used to map to forType</param>
        /// <returns>all the non-default types references in forType</returns>
        public DependencyInformation GetDependencyInformation(Type forType, AttributeExtCollection attributesAfterMap) {
            return new DependencyInformation(forType, attributesAfterMap, this);
        }

 
        /// <summary>check the dependencies for not mapped types. Store the found deps</summary>
        /// <param name="depInfo"></param>
        public void RegisterNotMapped(DependencyInformation depInfo) {
            StoreToMapNext(depInfo.DependenciesContent);
            StoreToMapNext(depInfo.DependenciesInheritance);
        }

 
        /// <summary>stores the dependant types, which are not already mapped</summary>
        /// <param name="deps"></param>
        private void StoreToMapNext(IList /* MapTypeInfo */ deps) {
            IEnumerator enumerator = deps.GetEnumerator();
            while (enumerator.MoveNext()) {
                MapTypeInfo info = (MapTypeInfo) enumerator.Current;


                // check if already mapped
                // CheckMapped for custom mapped types is ture, because in info target type is stored.
                if (!(CheckMapped(info) || m_toMap.Contains(info))) {
                    m_toMap.Enqueue(info);
                }
            }
        }
 
        /// <summary>
        /// returns true, if a forward declaration for the given type info is possible;
        /// it's not possible e.g. for Boxed value types.
        /// </summary>
        public bool IsForwardDeclPossible(Type forType, AttributeExtCollection attributes) {
            // boxed value types can't have forward declaration --> be sure to not include boxed value types here
            // CORBA 2.3: only concrete/abstract value types and concrete/abstract/local interface can be fwd declared.
            return ((ClsToIdlMapper.IsMappedToConcreteValueType(forType) ||
                     ClsToIdlMapper.IsMappedToAbstractValueType(forType, attributes) ||
                     ClsToIdlMapper.IsMappedToConcreteInterface(forType) ||
                     ClsToIdlMapper.IsMappedToAbstractInterface(forType) ||
                     ClsToIdlMapper.IsMappedToLocalInterface(forType)) &&
                    (!IsMappedBeforeGeneration(forType)));
            // don't create fwd references for entities, which are mapped before generation run
        }
 
        /// <summary>
        /// gets the type, which should be mapped next
        /// </summary>
        public MapTypeInfo GetNextTypeToMap() {
            MapTypeInfo result = null;
 
            while (m_toMap.Count > 0) {
                MapTypeInfo candidate = (MapTypeInfo) m_toMap.Dequeue();
                if (!CheckMapped(candidate)) {
                    result = candidate;
                    break;
                }
            }
            return result;
        }

        public MapTypeInfo CreateMapTypeInfo(Type type, AttributeExtCollection attributes) {
            return new MapTypeInfo(type, attributes, IsForwardDeclPossible(type, attributes));
        }
 
        #endregion IMethods
 
    }

    /// <summary>contains the dependency information for the Type forType</summary>
    internal class DependencyInformation {

        #region IFields
 
        private Type m_forType;
 
        private ArrayList m_dependenciesInheritance;
        private ArrayList m_dependenciesContent;
 
        private DependencyManager m_manager;

        #endregion IFields
        #region IConstructors

        internal DependencyInformation(Type forType, AttributeExtCollection attributesAfterMap, DependencyManager manager) {
            m_forType = forType;
            m_manager = manager;
            DetermineInheritanceDependencies();
            DetermineContentDependencies(attributesAfterMap);
        }

        #endregion IConstructors
        #region IProperties

        public Type ForType {
            get {
                return m_forType;
            }
        }

        public IList DependenciesInheritance {
            get {
                return m_dependenciesInheritance;
            }
        }

        public IList DependenciesContent {
            get {
                return m_dependenciesContent;
            }
        }

        #endregion IProperties
        #region IMethods
 
        private MapTypeInfo CreateMapTypeInfo(Type type, AttributeExtCollection attributes) {
            return m_manager.CreateMapTypeInfo(type, attributes);
        }
 
        private void AddToDepList(ArrayList depList, MapTypeInfo info, bool useAnanymousSequences) {
            if (info.Type.IsByRef) {
                info = CreateMapTypeInfo(info.Type.GetElementType(),
                                         info.Attributes);
            }
 
            if (useAnanymousSequences && info.Attributes.IsInCollection(typeof(IdlSequenceAttribute))) {
                // for anonymous IDL-sequence: add dep for sequence element and not for sequence itself,
                // because the sequence type itself will never be mapped to a type on it's own.
                // remark: non-anonymous sequences are mapped to a type-def
                info.RemoveAttributeOfType(ReflectionHelper.IdlSequenceAttributeType);
                Type elemType = info.Type.GetElementType();
                info = CreateMapTypeInfo(elemType, info.Attributes);
            }
 
            if (!depList.Contains(info)) {
                depList.Add(info);
            }
        }
 

        /// <summary>determines the non-default types, this type depends on</summary>
        private void DetermineInheritanceDependencies() {
            m_dependenciesInheritance = new ArrayList();

            // for the following types the base classes must be mapped
            if (m_forType.IsClass &&
                (ClsToIdlMapper.IsMappedToConcreteValueType(m_forType) ||
                 ClsToIdlMapper.IsMappedToAbstractValueType(m_forType) ||
                 ClsToIdlMapper.IsMappedToConcreteInterface(m_forType) ||
                 ClsToIdlMapper.IsMappedToAbstractInterface(m_forType) ||
                 ClsToIdlMapper.IsMappedToLocalInterface(m_forType))) {
                // boxed value types are excluded here, because they do not have inheritance dependencies
                Type baseType = m_forType.BaseType;
                if (!((baseType.Equals(typeof(System.Object))) || (baseType.Equals(typeof(System.ValueType))) ||
                     (baseType.Equals(typeof(System.ComponentModel.MarshalByValueComponent))) ||
                     (baseType.Equals(typeof(System.MarshalByRefObject))))) {
                    AddToDepList(m_dependenciesInheritance,
                                 CreateMapTypeInfo(baseType, AttributeExtCollection.EmptyCollection),
                                 true);
                }
            }

            // for the following types, implemented interfaces must be considered
            if (ClsToIdlMapper.IsMappedToAbstractValueType(m_forType) ||
                ClsToIdlMapper.IsMappedToConcreteValueType(m_forType) ||
                ClsToIdlMapper.IsMappedToConcreteInterface(m_forType) ||
                ClsToIdlMapper.IsMappedToAbstractInterface(m_forType) ||
                ClsToIdlMapper.IsMappedToLocalInterface(m_forType)) {
                Type[] implementedIF = m_forType.GetInterfaces();
                for (int i = 0; i < implementedIF.Length; i++) {
                    // don't map to IDL, if not legal to inherit from
                    if (ClsToIdlMapper.MapInheritanceFromInterfaceToIdl(implementedIF[i], m_forType)) {
                        AddToDepList(m_dependenciesInheritance,
                                 CreateMapTypeInfo(implementedIF[i], AttributeExtCollection.EmptyCollection),
                                 true);
                    }
                }
            }
        }

        private void DetermineContentDependencies(AttributeExtCollection attributesAfterMap) {
            m_dependenciesContent = new ArrayList();

            // for the following types methods and properties must be considered
            if (ClsToIdlMapper.IsMappedToConcreteValueType(m_forType) ||
                ClsToIdlMapper.IsMappedToAbstractValueType(m_forType) ||
                ClsToIdlMapper.IsMappedToConcreteInterface(m_forType) ||
                ClsToIdlMapper.IsMappedToAbstractInterface(m_forType) ||
                ClsToIdlMapper.IsMappedToLocalInterface(m_forType)) {
                // check the methods
                DetermineContentDependenciesFromMethods();
                // check the properties
                DetermineContentDependenciesFromProperties();
            }

            // fields must be considered only for value-types
            if (ClsToIdlMapper.IsMappedToConcreteValueType(m_forType) ||
                ClsToIdlMapper.IsException(m_forType) ||
                ClsToIdlMapper.IsMarshalledAsStruct(m_forType) ||
                ClsToIdlMapper.IsMappedToBoxedValueType(m_forType)) {
                DetermineContentDependenciesFromFields();
            }
            if (ClsToIdlMapper.IsArray(m_forType)) {
                DetermineContentDependenciesFromElementType(attributesAfterMap);
            }
        }

        private void DetermineContentDependenciesFromMethods() {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public;
 
            MethodInfo[] methods = m_forType.GetMethods(flags);
            foreach (MethodInfo info in methods) {

                // return type
                Type returnType = info.ReturnType;
                if (!m_manager.IsDefaultMapped(returnType) && !returnType.Equals(m_forType)) {
                    AddToDepList(m_dependenciesContent, CreateMapTypeInfo(returnType,
                                                               ReflectionHelper.CollectReturnParameterAttributes(info)),
                                 false);
                }
 
                ParameterInfo[] methodParams = info.GetParameters();
                foreach (ParameterInfo paramInfo in methodParams) {
                    if (!m_manager.IsDefaultMapped(paramInfo.ParameterType) && !paramInfo.ParameterType.Equals(m_forType)) {
                        // only add to dependencies, if not default mapped and not the same as type which is checked
                        AddToDepList(m_dependenciesContent, CreateMapTypeInfo(paramInfo.ParameterType,
                                                                   ReflectionHelper.CollectParameterAttributes(paramInfo, info)),
                                     false);
                    }
                }
 
                if ((ClsToIdlMapper.IsMappedToAbstractInterface(m_forType) ||
                      ClsToIdlMapper.IsMappedToConcreteInterface(m_forType))) {
                    DetermineContentDependenciesFromExceptions(info);
                }
 
            }
        }

        private void DetermineContentDependenciesFromProperties() {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public;
 
            PropertyInfo[] properties = m_forType.GetProperties(flags);
            foreach (PropertyInfo info in properties) {
                if (!m_manager.IsDefaultMapped(info.PropertyType) && !info.PropertyType.Equals(m_forType)) {
                    // only add to dependencies, if not default mapped
                    AddToDepList(m_dependenciesContent,
                                 CreateMapTypeInfo(info.PropertyType, AttributeExtCollection.ConvertToAttributeCollection(info.GetCustomAttributes(true))),
                                 false);
                }
            }
        }

        private void DetermineContentDependenciesFromFields() {
            FieldInfo[] fields = ReflectionHelper.GetAllDeclaredInstanceFieldsOrdered(m_forType);
            foreach (FieldInfo info in fields) {
                if (!m_manager.IsDefaultMapped(info.FieldType) && !info.FieldType.Equals(m_forType)) {
                    // only add to dependencies, if not default mapped
                    AddToDepList(m_dependenciesContent,
                                 CreateMapTypeInfo(info.FieldType, AttributeExtCollection.ConvertToAttributeCollection(info.GetCustomAttributes(true))),
                                 true);
                }
            }
        }
 
        private void DetermineContentDependenciesFromElementType(AttributeExtCollection attributesAfterMap) {
            Type elemType = m_forType.GetElementType();
            if (elemType != null) {
                if (!m_manager.IsDefaultMapped(elemType) && !elemType.Equals(m_forType)) {
                    AddToDepList(m_dependenciesContent,
                                 CreateMapTypeInfo(elemType, attributesAfterMap),
                                 false); // for sequence of sequence, don't use anonymous seq for element type
                }
            } // else: should not happen
        }
 
        private void DetermineContentDependenciesFromExceptions(MethodInfo thrower) {
            AttributeExtCollection methodAttributes =
                ReflectionHelper.GetCustomAttriutesForMethod(thrower, true);
            foreach (Attribute attr in methodAttributes) {
                if (ReflectionHelper.ThrowsIdlExceptionAttributeType.
                    IsAssignableFrom(attr.GetType())) {
                    Type exceptionType = ((ThrowsIdlExceptionAttribute)attr).ExceptionType;
                    AddToDepList(m_dependenciesContent,
                                 CreateMapTypeInfo(exceptionType, AttributeExtCollection.EmptyCollection),
                                 false);
                }
            }
            // default exception, which can be thrown
            AddToDepList(m_dependenciesContent,
                         CreateMapTypeInfo(typeof(GenericUserException), AttributeExtCollection.EmptyCollection),
                         false);
        }
 
        /// <summary>
        /// gets the types which must be mapped before type (the already mapped and the not already mapped ones).
        /// For those types, an include is needed before the type definition starts
        /// </summary>
        /// <param name="depInfo"></param>
        public IList GetTypesToIncludeBeforeType() {
            ArrayList result = new ArrayList();
            // inheritance deps must be mapped before type
            InsertInheritanceTypesBefore(false, result); // do not check for already mapped here
 
            // insert all already mapped types, this type depends on
            foreach (MapTypeInfo info in m_dependenciesContent) {
                Debug.WriteLine("getTypesToIncludeBefore-content: " + ForType +
                                ", pot include: " + info.Type + ", already mapped: " + m_manager.CheckMapped(info) +
                                ", is fwd ref possible: " + info.IsForwardDeclPossible);
                if (!info.IsForwardDeclPossible && (!m_manager.IsDefaultMapped(info.Type))) {
                    // need to add an include, if no forward declaration is possible
                    // do always use forward declarations if possible to prevent cyclic includes!
                    // for default idl types, don't add an include!
                    Debug.WriteLine("add it");
                    result.Add(info);
                }
            }

            return result;
        }

        /// <summary>
        /// inserts the inheritance dependencies into the target List.
        /// </summary>
        /// <param name="check">if true, do not include already mapped types</param>
        private void InsertInheritanceTypesBefore(bool check, ArrayList target) {
            foreach (MapTypeInfo info in m_dependenciesInheritance) {
                Debug.WriteLine("getTypesBefore-inh: " + ForType + ", " + check +
                                ", pot include: " + info.Type + ", already mapped: " + m_manager.CheckMapped(info));
                if ((!check) || (!m_manager.CheckMapped(info))) {
                    target.Add(info);
                    Debug.WriteLine("add it");
                }
            }
        }
 
        /// <summary>gets the types, which must be mapped before it's possible to map the Type toMap</summary>
        /// <returns>an arraylist containing MapTypeInfo for all types which needs to be mapped before</returns>
        public IList GetTypesToMapBeforeType() {
            ArrayList result = new ArrayList();
            // inheritance deps must be mapped before type
            InsertInheritanceTypesBefore(true, result); // do check for already mapped here
 
            // all non-interface and non-value types must be mapped before the type (also boxed value types)
            foreach (MapTypeInfo info in m_dependenciesContent) {
                Debug.WriteLine("getTypesBefore-content: " + ForType + ", pot include: " +
                                info.Type + ", already mapped: " + m_manager.CheckMapped(info));
                // every type, which can't be fwd declared must be mapped before (e.g. boxed value types)
                if ((!info.IsForwardDeclPossible) && (!m_manager.CheckMapped(info))) {
                    // needs to be mapped before current type can be mapped
                    Debug.WriteLine("add it");
                    result.Add(info);
                }
            }
            return result;
        }
 
        /// <summary>gets the types for which forward references must be created</summary>
        /// <returns>an arraylist containing MapTypeInfo for all types which needs a forward reference</returns>
        public IList GetNeededForwardRefs() {
            ArrayList result = new ArrayList();
            // only content dependencies can lead to forward references, for inheritance deps, forward refs is not allowed
            foreach (MapTypeInfo info in m_dependenciesContent) {
                if (info.IsForwardDeclPossible && (!result.Contains(info))) {
                    // do use fwd declarations if possible to prevent cyclic includes
                    // do not generate forward declarations for custom mapped, because full idl already present
                    // for custom mapped: CheckMapped results in true -> ok.
                    result.Add(info);
                }
            }
            return result;
        }
 
        #endregion IMethods

    }
 
}
