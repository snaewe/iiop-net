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

        private DependencyAnalyzer m_analyzer;

        private Hashtable m_filesForMappedTypes = Hashtable.Synchronized(new Hashtable());

        /// <summary>contains the types, which are already mapped during generation</summary>
        private ArrayList m_alreadyMappedTypes = ArrayList.Synchronized(new ArrayList());

        /// <summary>contains all the default mapped types</summary>
        private ArrayList m_defaultMappedTypes = new ArrayList();

        /// <summary>stores the next types to map</summary>
        private Queue m_toMap = Queue.Synchronized(new Queue());

        #endregion IFields
        #region IConstructors

        public DependencyManager() {
            m_analyzer = new DependencyAnalyzer(this);
            SetUpKnownMappings();
        }

        #endregion IConstructors
        #region IMethods

        private void SetUpKnownMappings() {
            m_defaultMappedTypes.Add(typeof(System.Int16));
            m_defaultMappedTypes.Add(typeof(System.Int32));
            m_defaultMappedTypes.Add(typeof(System.Int64));
            m_defaultMappedTypes.Add(typeof(void));
            m_defaultMappedTypes.Add(typeof(System.Boolean));
            m_defaultMappedTypes.Add(typeof(System.Byte));
            m_defaultMappedTypes.Add(typeof(System.Single));
            m_defaultMappedTypes.Add(typeof(System.Double));
            m_defaultMappedTypes.Add(typeof(System.String));
            m_defaultMappedTypes.Add(typeof(System.Char));
            // some default types
            m_defaultMappedTypes.Add(typeof(System.Object));
            m_defaultMappedTypes.Add(typeof(System.Type));
            m_defaultMappedTypes.Add(typeof(omg.org.CORBA.TypeCode));
            m_defaultMappedTypes.Add(typeof(GenericUserException));
            m_defaultMappedTypes.Add(typeof(MarshalByRefObject));
            m_defaultMappedTypes.Add(typeof(omg.org.CORBA.WStringValue));
            m_defaultMappedTypes.Add(typeof(omg.org.CORBA.StringValue));
        }
        
        /// <summary>
        /// is this type already mapped by default (standard-types)
        /// </summary>
        internal bool IsDefaultMapped(Type toMap) {
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
        public void RegisterMappedType(Type mapped, string idlFile) {
            if (m_alreadyMappedTypes.Contains(mapped) || IsDefaultMapped(mapped)) {
                throw new Exception("reregister of mapped type not possible, already mapped: " + mapped); 
            }
            m_alreadyMappedTypes.Add(mapped);
            m_filesForMappedTypes.Add(mapped, idlFile);
        }

        /// <summary>get the name of the idl-file for the mapped type</summary>
        public string GetIdlFileForMappedType(Type mapped) {
            if (!IsCustomMappedToIdlType(mapped)) { 
                object res = m_filesForMappedTypes[mapped];
                return (string) res;
            } else {
                GeneratorMappingPlugin mappingPlugin = GeneratorMappingPlugin.GetSingleton();
                GeneratorCustomMappingDesc mappingDesc = mappingPlugin.GetMappingForIdlTarget(mapped);
                return mappingDesc.IdlFileName;
            }
        }

        /// <summary>checks if a type is already mapped</summary>
        /// <returns>true, if mapped, else false</returns>
        public bool CheckMapped(Type toMap) {
            if (m_alreadyMappedTypes.Contains(toMap) || 
                IsMappedBeforeGeneration(toMap)){
                return true; 
            } else {
                return false;
            }
        }
        
        private bool IsMappedBeforeGeneration(Type toMap) {
            return (IsDefaultMapped(toMap) ||
                    ReflectionHelper.IIdlEntityType.IsAssignableFrom(toMap) ||
                    IsCustomMappedFromClsType(toMap));
        }

        /// <summary>returns the dependency information for Type forType</summary>
        /// <returns>all the non-default types references in forType</returns>
        public DependencyInformation GetDependencyInformation(Type forType) {
            ArrayList depsInh = m_analyzer.DetermineInheritanceDependencies(forType);
            ArrayList depsCont = m_analyzer.DetermineContentDependencies(forType);
            return new DependencyInformation(forType, depsCont, depsInh);
        }

        
        /// <summary>check the dependencies for not mapped types. Store the found deps</summary>
        /// <param name="depInfo"></param>
        public void RegisterNotMapped(DependencyInformation depInfo) {
            StoreToMapNext(depInfo.DependenciesContent);
            StoreToMapNext(depInfo.DependenciesInheritance);
        }

        
        /// <summary>stores the dependant types, which are not already mapped</summary>
        /// <param name="deps"></param>
        private void StoreToMapNext(ArrayList deps) {
            IEnumerator enumerator = deps.GetEnumerator();
            while (enumerator.MoveNext()) {
                MapTypeInfo info = (MapTypeInfo) enumerator.Current;        


                // check if already mapped
                // CheckMapped for custom mapped types is ture, because in info target type is stored.
                if (!(CheckMapped(info.Type) || m_toMap.Contains(info))) {
                    m_toMap.Enqueue(info);
                }
            }
        }
        
        /// <summary>
        /// gets the types which must be mapped before type (the already mapped and the not already mapped ones).
        /// For those types, an include is needed before the type definition starts
        /// </summary>
        /// <param name="depInfo"></param>
        public ArrayList GetTypesToIncludeBeforeType(DependencyInformation depInfo) {
            ArrayList result = new ArrayList();
            // inheritance deps must be mapped before type
            InsertInheritanceTypesBefore(depInfo, false, result); // do not check for already mapped here
            
            // insert all already mapped types, this type depends on
            IEnumerator enumerator = depInfo.DependenciesContent.GetEnumerator();
            while (enumerator.MoveNext()) {
                MapTypeInfo info = (MapTypeInfo) enumerator.Current;
                Debug.WriteLine("getTypesToIncludeBefore-content: " + depInfo.ForType + 
                                ", pot include: " + info.Type + ", already mapped: " + CheckMapped(info.Type) +
                                ", is fwd ref possible: " + IsForwardDeclPossible(info));
                if (!IsForwardDeclPossible(info) && (!IsDefaultMapped(info.Type))) {
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
        private void InsertInheritanceTypesBefore(DependencyInformation depInfo, bool check, ArrayList target) {
            IEnumerator enumerator = depInfo.DependenciesInheritance.GetEnumerator();
            while (enumerator.MoveNext()) {
                MapTypeInfo info = (MapTypeInfo) enumerator.Current;
                Debug.WriteLine("getTypesBefore-inh: " + depInfo.ForType + ", " + check + 
                                ", pot include: " + info.Type + ", already mapped: " + CheckMapped(info.Type));
                if ((!check) || (!CheckMapped(info.Type))) {
                    target.Add(info);
                    Debug.WriteLine("add it");
                }
            }
        }
        
        /// <summary>
        /// returns true, if a forward declaration for the given type info is possible;
        /// it's not possible e.g. for Boxed value types.
        /// </summary>
        private bool IsForwardDeclPossible(MapTypeInfo forInfo) {
            // boxed value types can't have forward declaration --> be sure to not include boxed value types here
            return ((ClsToIdlMapper.IsDefaultMarshalByVal(forInfo.Type) || 
                     ClsToIdlMapper.IsMappedToAbstractValueType(forInfo.Type, forInfo.Attributes) ||
                     ClsToIdlMapper.IsMarshalByRef(forInfo.Type)) && 
                    (!(forInfo.Type.IsSubclassOf(ReflectionHelper.BoxedValueBaseType))) &&
                    (!IsMappedBeforeGeneration(forInfo.Type)));
            // don't create fwd references for entities, which are mapped before generation run
        }
               
        /// <summary>gets the types for which forward references must be created</summary>
        /// <returns>an arraylist containing MapTypeInfo for all types which needs a forward reference</returns>
        public ArrayList GetNeededForwardRefs(DependencyInformation depInfo) {
            ArrayList result = new ArrayList();
            // only content dependencies can lead to forward references, for inheritance deps, forward refs is not allowed
            IEnumerator enumerator = depInfo.DependenciesContent.GetEnumerator();    
            while (enumerator.MoveNext()) {
                MapTypeInfo info = (MapTypeInfo) enumerator.Current;
                if (IsForwardDeclPossible(info) && (!result.Contains(info))) {
                    // do use fwd declarations if possible to prevent cyclic includes
                    // do not generate forward declarations for custom mapped, because full idl already present
                    // for custom mapped: CheckMapped results in true -> ok.                    
                    result.Add(info);
                }
            }
            return result;
        }

        /// <summary>gets the types, which must be mapped before it's possible to map the Type toMap</summary>
        /// <returns>an arraylist containing MapTypeInfo for all types which needs to be mapped before</returns>
        public ArrayList GetTypesToMapBeforeType(DependencyInformation depInfo) {
            ArrayList result = new ArrayList();
            // inheritance deps must be mapped before type
            InsertInheritanceTypesBefore(depInfo, true, result); // do check for already mapped here
            
            // all non-interface and non-value types must be mapped before the type (also boxed value types)
            IEnumerator enumerator = depInfo.DependenciesContent.GetEnumerator();
            while (enumerator.MoveNext()) {
                MapTypeInfo info = (MapTypeInfo) enumerator.Current;
                Debug.WriteLine("getTypesBefore-content: " + depInfo.ForType + ", pot include: " +
                                info.Type + ", already mapped: " + CheckMapped(info.Type));
                // every type, which can't be fwd declared must be mapped before (e.g. boxed value types)
                if ((!IsForwardDeclPossible(info)) && (!CheckMapped(info.Type))) { 
                    // needs to be mapped before current type can be mapped
                    Debug.WriteLine("add it");
                    result.Add(info);
                }
            }
            return result;
        }

        /// <summary>
        /// gets the type, which should be mapped next
        /// </summary>
        public MapTypeInfo GetNextTypeToMap() {
            MapTypeInfo result = null;
            
            while (m_toMap.Count > 0) {
                MapTypeInfo candidate = (MapTypeInfo) m_toMap.Dequeue();
                if (!CheckMapped(candidate.Type)) {
                    result = candidate;
                    break;
                }
            }
            return result;
        }

        #endregion IMethods
    
    }

    /// <summary>contains the dependency information for the Type forType</summary>
    internal class DependencyInformation {

        #region IFields
    
        private Type m_forType;
        
        private ArrayList m_dependenciesInheritance;
        private ArrayList m_dependenciesContent;

        #endregion IFields
        #region IConstructors

        internal DependencyInformation(Type forType, ArrayList dependenciesContent, 
                                       ArrayList dependenciesInheritance) {
            m_forType = forType;
            m_dependenciesContent = dependenciesContent;
            m_dependenciesInheritance = dependenciesInheritance;
        }

        #endregion IConstructors
        #region IProperties

        public Type ForType {
            get { 
                return m_forType; 
            }
        }

        public ArrayList DependenciesInheritance {
            get { 
                return m_dependenciesInheritance; 
            }
        }

        public ArrayList DependenciesContent {
            get { 
                return m_dependenciesContent; 
            }
        }

        #endregion IProperties

    }

    /// <summary>analyzes the dependencies of a type to map</summary>
    internal class DependencyAnalyzer {
        
        #region IFields

        private DependencyManager m_manager;

        private ClsToIdlMapper m_mapper = ClsToIdlMapper.GetSingleton();

        #endregion IFields
        #region IConstructors
        
        public DependencyAnalyzer(DependencyManager manager) {
            m_manager = manager;
        }

        #endregion IConstructors
        #region IMethods

        private void AddToDepList(ArrayList depList, MapTypeInfo info) {            
            if (info.Type.IsByRef) {
                info = new MapTypeInfo(info.Type.GetElementType(), 
                                       info.Attributes);
            }
            
            if (info.Attributes.IsInCollection(typeof(IdlSequenceAttribute))) {
                // for an IDL-sequence: add dep for sequence element and not for sequence itself,
                // because the sequence type itself will never be mapped to a type on it's own.
                // remark: there are no sequences of sequences because of the CLS to IDL mapping,
                // only sequences of boxed sequences!
                info.RemoveAttributeOfType(ReflectionHelper.IdlSequenceAttributeType);
                Type elemType = info.Type.GetElementType();
                info = new MapTypeInfo(elemType, info.Attributes);
            }
            
            if (!depList.Contains(info)) {
                depList.Add(info);
            }
        }

        /// <summary>determines the non-default types, this type depends on</summary>
        /// <param name="forType"></param>
        /// <returns></returns>
        public ArrayList DetermineInheritanceDependencies(Type forType) {
            ArrayList result = new ArrayList();

            // for the following types the base classes must be mapped
            if ((ClsToIdlMapper.IsDefaultMarshalByVal(forType) || 
                 ClsToIdlMapper.IsMarshalByRef(forType)) &&
                (!(forType.IsSubclassOf(typeof(BoxedValueBase))))) {
                // boxed value types are excluded here, because they do not have inheritance dependencies
                Type baseType = forType.BaseType;
                if (!((baseType.Equals(typeof(System.Object))) || (baseType.Equals(typeof(System.ValueType))) ||
                     (baseType.Equals(typeof(System.ComponentModel.MarshalByValueComponent))) ||
                     (baseType.Equals(typeof(System.MarshalByRefObject))))) {
                    AddToDepList(result, new MapTypeInfo(baseType, new AttributeExtCollection()));
                }
                foreach (Type ifType in forType.GetInterfaces()) {
                    AddToDepList(result, new MapTypeInfo(ifType, new AttributeExtCollection()));
                }
            }

            // for the following types, implemented interfaces must be considered
            if ((ClsToIdlMapper.IsInterface(forType)) ||
                (ClsToIdlMapper.IsDefaultMarshalByVal(forType) || ClsToIdlMapper.IsMarshalByRef(forType)) &&
                (!(forType.IsSubclassOf(typeof(BoxedValueBase))))) {
                Type[] implementedIF = forType.GetInterfaces();
                for (int i = 0; i < implementedIF.Length; i++) {
                    AddToDepList(result, new MapTypeInfo(implementedIF[i], new AttributeExtCollection()));
                }
            }

            return result;

        }

        public ArrayList DetermineContentDependencies(Type forType) {
            ArrayList result = new ArrayList();

            // for the following types methods and properties must be considered
            if (ClsToIdlMapper.IsDefaultMarshalByVal(forType) ||
                ClsToIdlMapper.IsMarshalByRef(forType) ||
                ClsToIdlMapper.IsInterface(forType)) {
                // check the methods
                AddTypesFromMethods(forType, result, BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public);
                // check the properties
                AddTypesFromProperties(forType, result, BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public);
            }

            // fields must be considered only for value-types
            if (ClsToIdlMapper.IsDefaultMarshalByVal(forType)) {
                AddTypesFromFields(forType, result, BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic);
            }

            return result;
        }

        private void AddTypesFromMethods(Type forType, ArrayList dependencies, BindingFlags flags) {
            MethodInfo[] methods = forType.GetMethods(flags);
            foreach (MethodInfo info in methods) {

                // return type
                Type returnType = info.ReturnType;
                if (!m_manager.IsDefaultMapped(returnType) && !returnType.Equals(forType)) {                 	
                    AddToDepList(dependencies, new MapTypeInfo(returnType, 
                                                               ReflectionHelper.CollectReturnParameterAttributes(info))); 
                }
                
                ParameterInfo[] methodParams = info.GetParameters();
                foreach (ParameterInfo paramInfo in methodParams) {
                    if (!m_manager.IsDefaultMapped(paramInfo.ParameterType) && !paramInfo.ParameterType.Equals(forType)) {
                        // only add to dependencies, if not default mapped and not the same as type which is checked
                        AddToDepList(dependencies, new MapTypeInfo(paramInfo.ParameterType, 
                                                                   ReflectionHelper.CollectParameterAttributes(paramInfo, info)));
                    }
                }
            }
        }

        private void AddTypesFromProperties(Type forType, ArrayList dependencies, BindingFlags flags) {
            PropertyInfo[] properties = forType.GetProperties(flags);
            foreach (PropertyInfo info in properties) {
                if (!m_manager.IsDefaultMapped(info.PropertyType) && !info.PropertyType.Equals(forType))     {
                    // only add to dependencies, if not default mapped
                    AddToDepList(dependencies, new MapTypeInfo(info.PropertyType, AttributeExtCollection.ConvertToAttributeCollection(info.GetCustomAttributes(true))));
                }
            }
        }

        private void AddTypesFromFields(Type forType, ArrayList dependencies, BindingFlags flags) {
            FieldInfo[] fields = forType.GetFields(flags);
            foreach (FieldInfo info in fields) {
                if (!m_manager.IsDefaultMapped(info.FieldType) && !info.FieldType.Equals(forType)) {
                    // only add to dependencies, if not default mapped
                    AddToDepList(dependencies, new MapTypeInfo(info.FieldType, AttributeExtCollection.ConvertToAttributeCollection(info.GetCustomAttributes(true))));
                }
            }
        }

        #endregion IMethods

    }
    
}
