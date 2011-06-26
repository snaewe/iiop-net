/* Repository.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 14.01.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using Ch.Elca.Iiop.Util;
using Corba;
using omg.org.CORBA;
using System.Collections.Generic;

namespace Ch.Elca.Iiop.Idl {

    /// <summary>
    /// The repository contains all types known to IIOP.NET. It handles the repository-id resolution / retrieval as
    /// well as typecode creation / resolution.
    /// </summary>
    public class Repository {

        /// <summary>
        /// Keeps track of loaded assemblies. It handles the load of new assemblies by registering types
        /// in repository.
        /// </summary>
        private class AssemblyLoader {

            private Repository m_forRepository;
            private Hashtable m_loadedAssemblies = new Hashtable();

            #region IConstructors

            internal AssemblyLoader(Repository forRepository) {
                m_forRepository = forRepository;
                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(this.AssemblyNotResolvable);
                AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(this.AssemblyLoaded);
                Assembly[] alreadyLoaded = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < alreadyLoaded.Length; i++) {
                    RegisterTypes(alreadyLoaded[i]);
                }
            }
 
            #endregion IConstructors
            #region IMethods

            private void AssemblyLoaded(object sender, AssemblyLoadEventArgs args) {
                // !!! BUG on .Net 2 SP1: going further breaks DefineDynamicAssembly in very strange manner, so don't process dynamic assemblies
                // simple test:
                // AppDomain.CurrentDomain.DefineDynamicAssembly(new System.Reflection.AssemblyName("dynBoxed" + Guid.NewGuid().ToString()), System.Reflection.Emit.AssemblyBuilderAccess.Run);
                // previous workaround using RegisterAssemblyForNonAutoRegistration/ShouldSkipAssemblyTypeAutoRegistration
                // is deficient, there may be dynamic assemblies irrelevant to IIOPNet.
                // So this patch is simple and universal:
                if (args.LoadedAssembly is System.Reflection.Emit.AssemblyBuilder)
                    return;

                RegisterTypes(args.LoadedAssembly);
                AssemblyName[] refAssemblies =
                    args.LoadedAssembly.GetReferencedAssemblies();
                if (refAssemblies != null) {
                    for (int i = 0; i <refAssemblies.Length; i++) {
                        try {
                            if (refAssemblies[i] != null) {
                                Assembly.Load(refAssemblies[i]); // this will call AssemblyLoaded for this assembly
                            }
                        } catch (BadImageFormatException) {
                            Trace.WriteLine("bad format -> ignoring assembly " + refAssemblies[i].FullName);
                            // ignore assembly
                        } catch (FileNotFoundException) {
                            Trace.WriteLine("missing -> ignoring assembly " + refAssemblies[i].FullName);
                            // ignore assembly
                        } catch (System.Security.SecurityException) {
                            Trace.WriteLine("security problem -> ignoring assembly " + refAssemblies[i].FullName);
                            // ignore assembly
                        }
                    }
                }
            }


            private string ParseAssemblyNamePart(string part) {
                int valueIndex = part.IndexOf("=");
                if (valueIndex > 0) {
                    return part.Substring(valueIndex + 1).Trim();
                } else {
                    return null;
                }
            }
            private void ParseAssemblyName(string asmName, out string simpleName, out string version,
                                           out string publicKeyToken, out string culture) {
                version = null;
                publicKeyToken = null;
                culture = null;
                string[] nameParts = asmName.Split(',');
                simpleName = nameParts[0].Trim();
                for (int i = 1; i < nameParts.Length; i++) {
                    string trimmedPart = nameParts[i].Trim();
                    if (trimmedPart.StartsWith("Version")) {
                        version = ParseAssemblyNamePart(nameParts[i]);
                    } else if (trimmedPart.StartsWith("PublicKeyToken")) {
                        publicKeyToken = ParseAssemblyNamePart(nameParts[i]);
                    } else if (trimmedPart.StartsWith("Culture")) {
                        culture = ParseAssemblyNamePart(nameParts[i]);
                    }
                }
            }

            private Assembly AssemblyNotResolvable(object sender, ResolveEventArgs args) {
                Debug.WriteLine("assembly was not resolvable: " + args.Name);
                string toResolveName;
                string toResolveVersion;
                string toResolvePkToken;
                string toResolveCulture;
                ParseAssemblyName(args.Name, out toResolveName, out toResolveVersion,
                                  out toResolvePkToken, out toResolveCulture);
                if ((toResolvePkToken != null) && (toResolveVersion != null) && (toResolveCulture == null)) {
                    // il emit doesn't generate full assembly names for referenced types if Culture is Invariant.
                    // Therefore, a type can't be resolved, if the assembly is only installed in the gac ->
                    // fix this by adding invariant culture here
                    Assembly[] assemblies;
                    lock(m_loadedAssemblies.SyncRoot) {
                        assemblies = new Assembly[m_loadedAssemblies.Count];
                        m_loadedAssemblies.Values.CopyTo(assemblies, 0);
                    }
                    for (int i = 0; i < assemblies.Length; i++) {
                        if (((Assembly)assemblies[i]).GetName().Name == toResolveName) {
                            // candidate
                            string candidateName;
                            string candidateVersion;
                            string candidatePkToken;
                            string candidateCulture;
                            ParseAssemblyName(((Assembly)assemblies[i]).FullName,
                                              out candidateName, out candidateVersion,
                                              out candidatePkToken, out candidateCulture);
                            if ((candidateVersion == toResolveVersion) &&
                                (candidatePkToken == toResolvePkToken)) {
                                return assemblies[i];
                            }
                        }
                    }
                }
                return null;
            }

            private void RegisterTypes(Assembly forAssembly) {
                lock(m_loadedAssemblies.SyncRoot) {
                    if (!m_loadedAssemblies.ContainsKey(forAssembly.FullName)) {
                        m_loadedAssemblies[forAssembly.FullName] = forAssembly;
                    } else {
                        return; // already loaded
                    }
                }
                // locking is done by register type
                Type[] types;
                try {
                    types = forAssembly.GetTypes();
                } catch (ReflectionTypeLoadException rtle) { // can happen for dynamic assemblies
                    Debug.WriteLine(String.Format("Couldn't load all types from assembly {0}; exception: {1}",
                                                  forAssembly.FullName, rtle));
                    types = rtle.Types; // the types loaded without exception
                }
                if (types != null) { // check needed for mono, because mono returns null for GetTypes in case of dynamically created assemblies
                    for (int i = 0; i < types.Length; i++) {
                        if (types[i] != null) { // null, if type coudln't be loaded
                            m_forRepository.RegisterType(types[i]);
                        }
                    }
                }
            }
 
            #endregion IMethods

        }

        #region IFields
 
        private Repository.AssemblyLoader m_assemblyLoader;
        private IDictionary<string, Type> m_loadedTypesByRepId = new Dictionary<string, Type>();
        private IDictionary<Type, string> m_repIdsByLoadedTypes = new Dictionary<Type, string>(); // for types not dynamically generated by IIOP.NET
        private IDictionary<string, Type> m_valueTypeImpls = new Dictionary<string, Type>();
 
        #endregion IFields

        #region IConstructors

        private Repository() {
            m_assemblyLoader = new Repository.AssemblyLoader(this);
        }

        #endregion IConstructors
        #region SFields
 

        private static Repository s_instance = new Repository();

        #endregion SFields
        #region SMethods

        #region rep-id parsing

        /// <summary>
        /// gets a CLS Type for the repository-id
        /// </summary>
        public static Type GetTypeForId(string repId) {
            if ((repId == String.Empty) ||
                (repId == "IDL:omg.org/CORBA/Object:1.0")) {
                return ReflectionHelper.MarshalByRefObjectType;
            }
            Type result;
            if (!s_instance.m_loadedTypesByRepId.TryGetValue(repId, out result)) {
                // check for java bug in idl to java mapping;
                // the idlj compiler changes the repository id while mapping from idl to java
                const string id = "IDL:org/omg/BoxedArray/System";
                if (!repId.StartsWith(id) ||
                    !s_instance.m_loadedTypesByRepId.TryGetValue("IDL:org/omg/BoxedArray/_System" + repId.Substring(id.Length),
                                                                 out result))
                {
                    Debug.WriteLine("type not found for id: " + repId);
                }
            }
            return result;
        }

        /// <summary>
        /// create a fully qualified type name for the repository-id; if no type is registered for
        /// a repostiroy id, this name can be used to create a type for the repository id.
        /// </summary>
        internal static string CreateTypeNameForId(string repId) {
            if (repId.StartsWith("RMI")) {
                return CreateTypeNameForRMIId(repId);
            } else if (repId.StartsWith("IDL")) {
                return CreateTypeNameForIDLId(repId, false);
            } else {
                return null; // unknown
            }
        }

        /// <summary>
        /// creates a CLS type name for a type represented by the IDL-id
        /// </summary>
        /// <param name="idlID"></param>
        /// <param name="assumeMappedFromIdl">should assume, that type represented by idlid is mapped from IDL to CLS</param>
        /// <returns>
        /// The typename for the idlID
        /// </returns>
        private static string CreateTypeNameForIDLId(string idlID, bool assumeMappedFromIdl) {
            int colonIndex = idlID.IndexOf(':', 4);
            if (colonIndex < 0) {
                // invalid repository id: idlID
                throw new INV_IDENT(9901, CompletionStatus.Completed_MayBe);
            }
            string typeName = idlID.Substring(4, colonIndex - 4);
            typeName = IdlNaming.MapIdlRepIdTypePartToClsName(typeName, assumeMappedFromIdl);
            return typeName;
        }
        /// <summary>
        /// create a CLS type name for a type represented by the RMI-id
        /// </summary>
        /// <param name="rmiID"></param>
        /// <returns></returns>
        private static string CreateTypeNameForRMIId(string rmiID) {
            string typeName;
            int colonIndex = rmiID.IndexOf(':', 4);
            if (colonIndex >= 0) {
                typeName = rmiID.Substring(4, colonIndex - 4);
            } else {
                typeName = rmiID.Substring(4);
            }
            // check for array type
            if (typeName.StartsWith("[")) {
                if (typeName.Length == 1) {
                    // invalid rmi-repository-id: typeName
                    throw new INV_IDENT(10002, CompletionStatus.Completed_MayBe);
                }
                string elemType = typeName.TrimStart('[');
                int arrayRank = typeName.Length - elemType.Length; // array rank = number of [ - characters
                // parse the elem-type, which is in RMI-ID format
                string elemNameSpace;
                string unqualElemType = ParseRMIArrayElemType(elemType, out elemNameSpace);
                if (elemNameSpace.Length > 0) {
                    elemNameSpace = "." + elemNameSpace;
                } 
                unqualElemType = ResolveRmiInnerTypeMapping(unqualElemType);
                // determine name of boxed value type
                typeName = "org.omg.boxedRMI" + elemNameSpace + ".seq" + arrayRank + "_" + unqualElemType;
                Debug.WriteLine("mapped rmi id to boxed value type name:" + typeName);
            } else {
                int lastPIndex = typeName.LastIndexOf(".");
                if (lastPIndex >= 0)  {
                    string elemNamespace = typeName.Substring(0, lastPIndex);
                    string unqualName = typeName.Substring(lastPIndex + 1);
                    if (unqualName.StartsWith("_")) {
                        // rmi mapping adds a J before a class name starting with _
                        typeName = elemNamespace + ".J" + unqualName;
                    }
                } else {
                    if (typeName.StartsWith("_")) {
                        // rmi mapping adds a J before a class name starting with _
                        typeName = "J" + typeName;
                    }
                }
                typeName = ResolveRmiInnerTypeMapping(typeName);
 
                // do name mapping (e.g. resolve clashes with CLS keywords)
                typeName = IdlNaming.MapRmiNameToClsName(typeName);
            }
 
            return typeName;
        }
 
        private static string ResolveRmiInnerTypeMapping(string typeName) {
            return typeName.Replace(@"\U0024", "__");
        }
 
        /// <param name="elemNamespace">the namespace of the elementType</param>
        /// <returns> the unqualified elem-type name </returns>
        private static string ParseRMIArrayElemType(string rmiElemType, out string elemNamespace) {
            elemNamespace = string.Empty; // for primitve types, this is the correct namespace
            // first character in elemType determines what kind of type
            switch (rmiElemType[0]) {
                case 'I':
                    return "long";
                case 'Z':
                    return "boolean";
                case 'B':
                    return "octet";
                case 'C':
                    return "wchar";
                case 'D':
                    return "double";
                case 'F':
                    return "float";
                case 'J':
                    return "long_long";
                case 'S':
                    return "short";
                case 'L':
                    if (rmiElemType.Length <= 1) {
                        // invalid element type in RMI array repository id"
                        throw new INV_IDENT(10004, CompletionStatus.Completed_MayBe);
                    }
                    string elemTypeName = rmiElemType.Substring(1, rmiElemType.Length - 1 - (rmiElemType[rmiElemType.Length - 1] == ';' ? 1 : 0));
                    string unqualName = string.Empty;
                    int lastPIndex = elemTypeName.LastIndexOf('.');
                    if (lastPIndex < 0)  {
                        elemNamespace = string.Empty;
                        unqualName = elemTypeName;
                    } else {
                        if (string.Compare(elemTypeName, 0, "java.lang", 0, lastPIndex) == 0 &&
                            string.Compare(elemTypeName, lastPIndex + 1, "String", 0, elemTypeName.Length - lastPIndex - 1) == 0) {
                            // special case: map to CORBA.WStringValue
                            elemNamespace = "CORBA";
                            unqualName = "WStringValue";
                        } else {
                            elemNamespace = elemTypeName.Substring(0, lastPIndex);
                            unqualName = elemTypeName.Substring(lastPIndex + 1);
                            // map rmi name to cls name, handle e.g. clashes with cls keywords
                            if (unqualName.StartsWith("_")) {
                                unqualName = "J" + unqualName; // rmi mapping adds a J before a class name starting with _
                            }
                            unqualName = IdlNaming.MapRmiNameToClsName(unqualName);
                            elemNamespace = IdlNaming.MapRmiNameToClsName(elemNamespace);
                            if (unqualName.StartsWith("_")) {
                                // remove _ added by IDL to CLS mapping, because class name follows a seq<n>_, therefore _ not added
                                // in IDL to CLS mapping
                                unqualName = unqualName.Substring(1);
                            }
                        }
                    }
                    return unqualName;
                default:
                    // invalid element type identifier in RMI array repository id: firstChar
                    throw new INV_IDENT(10003, CompletionStatus.Completed_MayBe);
            }
        }

        #endregion rep-id parsing
        #region rep-id creation
        /// <summary>
        /// gets the repository id for a CLS type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetRepositoryID(Type type, bool checkIsPublic) {
            // internal types are not supported, because there are a lot of 
            // internal types with the same name in ms.net assemblies.
            if (checkIsPublic && !type.IsPublic) {
                throw new BAD_PARAM(568, CompletionStatus.Completed_MayBe);
            }

            // can need registration for dynamically created types otherwise RegisterType
            // will simply get it for us
            return s_instance.RegisterType(type);
        }

        public static string GetRepositoryID(Type type) {
            return GetRepositoryID(type, true);
        }
        #endregion rep-id creation
        #region loading types

        /// <summary>
        /// gets the impl class for a value type specified by the ImplClassAttribute.
        /// </summary>
        /// <remarks>returns null, if impl class not found.</remarks>
        internal static Type GetValueTypeImplClass(string clsImplClassName) {
            lock(s_instance) {
                Type result;
                if (s_instance.m_valueTypeImpls.TryGetValue(clsImplClassName, out result)) {
                    return result;
                }
                Debug.WriteLine("value type impl not found in repository: " + clsImplClassName);
                return null;
            }
        }

        #endregion loading types
        #region dynamically created types

        /// <summary>
        /// informs the repository about a dynamically created type
        /// </summary>
        internal static void RegisterDynamicallyCreatedType(Type type) {
            s_instance.RegisterType(type);
        }

        #endregion dynamically created types
        #region verifying types
 
        /// <summary>
        /// returns true, if the Type objectType is compatible with requiredType,
        /// i.e. objectType is_a requiredType.
        /// </summary>
        /// <remarks>objectType could be e.g. the interface type implemented by a remote object,
        /// or the type of a local object implementation class, ...</remarks>
        public static bool IsCompatible(Type requiredType, Type objectType) {
            return requiredType.IsAssignableFrom(objectType);
        }
 
        /// <summary>
        /// returns true, if interface type iorType is assignable to requiredType.
        /// If not verifable with static inheritance information, returns false.
        /// </summary>
        /// <param name="useTypeForId">returns the best local type to use for iorTypeId</param>
        public static bool IsInterfaceCompatible(Type requiredType, string iorTypeId, out Type useTypeForId) {
            Type interfaceType;
            if (iorTypeId.Length != 0) { // empty string stands for CORBA::Object
                interfaceType = Repository.GetTypeForId(iorTypeId);
            } else {
                interfaceType = ReflectionHelper.MarshalByRefObjectType;
            }
            // for requiredType MarshalByRefObject and omg.org.CORBA.IObject
            // everything is possible (i.e. every remote object type can be assigned to it),
            // the other requiredType types must be checked remote if not locally verifable
            if ((!requiredType.Equals(ReflectionHelper.MarshalByRefObjectType)) &&
                (!requiredType.Equals(ReflectionHelper.IObjectType)) &&
                ((interfaceType == null) || (!IsCompatible(requiredType, interfaceType)))) {
                // remote type not known or locally assignability not verifable
                useTypeForId = requiredType;
                return false;
            }
            // interface is assignable (or required type is compatible with all -> do a null check for this case)
            useTypeForId = (interfaceType != null ? interfaceType : requiredType);
            return true;
        }
 
        #endregion verifying types
        #region for typecodes

        /// <summary>
        /// creates a CORBA type code for a CLS type
        /// </summary>
        /// <returns>the typecode for the CLS type</returns>
        internal static TypeCodeImpl CreateTypeCodeForType(Type forType,
                                                          AttributeExtCollection attributes) {
            return CreateTypeCodeForTypeInternal(forType, attributes, new TypeCodeCreater());
        }
 
        /// <summary>used by type code creating methods</summary>
        internal static TypeCodeImpl CreateTypeCodeForTypeInternal(Type forType, AttributeExtCollection attributes,
                                                                   TypeCodeCreater typeCodeCreator) {
            if (forType != null) {
                ClsToIdlMapper mapper = ClsToIdlMapper.GetSingleton();
                return (TypeCodeImpl)mapper.MapClsType(forType, attributes, typeCodeCreator);
            } else {
                // if no type info present, map to null typecode; the case can't be handled by cls to idl mapper
                return new NullTC();
            }
        }

        /// <summary>gets the CLS type for the Typecode</summary>
        public static Type GetTypeForTypeCode(omg.org.CORBA.TypeCode typeCode) {
            if (!(typeCode is omg.org.CORBA.TypeCodeImpl)) {
                throw new INTERNAL(567, CompletionStatus.Completed_MayBe);
            } else {
                return (typeCode as TypeCodeImpl).GetClsForTypeCode();
            }
        }
 
        public static AttributeExtCollection GetAttrsForTypeCode(omg.org.CORBA.TypeCode typeCode) {
            if (!(typeCode is omg.org.CORBA.TypeCodeImpl)) {
                return AttributeExtCollection.EmptyCollection;
            } else {
                return (typeCode as TypeCodeImpl).GetClsAttributesForTypeCode();
            }
        }
        #endregion for typecodes
        #endregion SMethods
        #region IMethods

        /// <summary>
        /// returns the repository id to use for type. In the repIdForType the string identifying the
        /// type is returned. The result and the repIdForType can be different (e.g. SupportedInterface)
        /// </summary>
        private string GetRepositoryIDFromType(Type type, out string repIdForType) {
            string result;
            object[] attr = type.GetCustomAttributes(ReflectionHelper.RepositoryIDAttributeType, false);
            if (attr != null && attr.Length > 0) {
                RepositoryIDAttribute repIDAttr = (RepositoryIDAttribute) attr[0];
                result = repIDAttr.Id;
            } else {
                result = IdlNaming.MapFullTypeNameToIdlRepId(type);
            }
            repIdForType = result;
            if (type.IsMarshalByRef) {
                attr = type.GetCustomAttributes(ReflectionHelper.SupportedInterfaceAttributeType, true);
                if (attr != null && attr.Length > 0) {
                    SupportedInterfaceAttribute repIDFrom = (SupportedInterfaceAttribute) attr[0];
                    Type fromType = repIDFrom.FromType;
                    if (fromType.Equals(type)) {
                        throw new INTERNAL(1701, CompletionStatus.Completed_MayBe);
                    }
                    string repIdForInterface;
                    result = GetRepositoryIDFromType(fromType, out repIdForInterface); // repository ID to use when serialising such a type
                }
            }
            if (IsValueTypeImplClass(type)) {
                string repIdForValType;
                result = GetRepositoryIDFromType(type.BaseType, out repIdForValType); // repository ID to use when serialising such a type
            }
            return result;
        }
 
        private bool IsValueTypeImplClass(Type type) {
            Type baseType = type.BaseType;
            if (baseType != null) {
                object[] attr = baseType.GetCustomAttributes(ReflectionHelper.ImplClassAttributeType, false);
                if (attr != null && attr.Length > 0) {
                    string implClassName = ((ImplClassAttribute)attr[0]).ImplClass;
                    return (implClassName == type.FullName); // implClassName must be the name of type
                }
            }
            return false;
        }
 
 
        private string RegisterType(Type type) {
            if (!type.IsPublic) {
                return null;
            }

            lock(this) {
                string repIdToUse;
                if (!m_repIdsByLoadedTypes.TryGetValue(type, out repIdToUse)) {
                    string repIdForType; // the rep-id, which should be resolved to the Type type
                    // repIdToUse is the rep-id, which should be told everyone, which wants to refernce Type type.
                    // This needs not to be the same as repIdForType (e.g. for SupportedInterface attribute)
                    repIdToUse = GetRepositoryIDFromType(type, out repIdForType);
                    m_repIdsByLoadedTypes[type] = repIdToUse;
                    Type alreadyMappedType;
                    if (!m_loadedTypesByRepId.TryGetValue(repIdForType, out alreadyMappedType)) {
                        m_loadedTypesByRepId.Add(repIdForType, type);
                    } else {
                        // rep-id should by unique
                        Trace.WriteLine(String.Format("For the repId {0} type {1} is already present, tried to " +
                                                      "assign another type {2} to same id!",
                                                      repIdForType, alreadyMappedType.AssemblyQualifiedName,
                                                      type.AssemblyQualifiedName));
                        // throw new INTERNAL(905, CompletionStatus.Completed_MayBe);
                    }
                    // check if it's a value type impl class
                    if (IsValueTypeImplClass(type)) {
                        m_valueTypeImpls[type.FullName] = type;
                    }
 
                }
                return repIdToUse;
            }
        }
 
        #endregion IMethods

    }

}


#if UnitTest

namespace Ch.Elca.Iiop.Tests {

    using System.IO;
    using NUnit.Framework;
    using omg.org.CORBA;
    using Ch.Elca.Iiop.Services;
    using Ch.Elca.Iiop;
    using Ch.Elca.Iiop.Idl;

    [RepositoryID("IDL:Ch/Elca/Iiop/Tests/IRepositoryTestIf1:1.0")]
    public interface IRepositoryTestIf1 {

    }

    [RepositoryID("IDL:Ch/Elca/Iiop/Tests/IRepositoryTestIf2:1.0")]
    public interface IRepositoryTestIf2 : IRepositoryTestIf1 {

    }

    [RepositoryID("IDL:Ch/Elca/Iiop/Tests/IRepositoryTestIf3:1.0")]
    public interface IRepositoryTestIf3 {

    }

    [RepositoryID("IDL:Ch/Elca/Iiop/Tests/RepositoryTestClassImpl:1.0")]
    public class RepositoryTestClassImpl : IRepositoryTestIf2 {

    }
 
    /// <summary>
    /// Unit tests for repository type check operations.
    /// </summary>
    [TestFixture]
    public class RepositoryTypeChecksTest {
 
 
        [Test]
        public void TestInterfaceCompatible() {
            string repIdIf = "IDL:Ch/Elca/Iiop/Tests/IRepositoryTestIf2:1.0";
            string repIdCl = "IDL:Ch/Elca/Iiop/Tests/RepositoryTestClassImpl:1.0";
 
            Type required = typeof(IRepositoryTestIf1);
 
            Type typeForId;
            Assert.IsTrue(Repository.IsInterfaceCompatible(required,
                                                              repIdIf,
                                                              out typeForId),"type compatibility for TestIf2" );
            Assert.AreEqual(typeof(IRepositoryTestIf2),
                                   typeForId, "type for if id");
 
            Assert.IsTrue(
                             Repository.IsInterfaceCompatible(required,
                                                              repIdCl,
                                                              out typeForId), "type compatibility for TestClassImpl");
            Assert.AreEqual(typeof(RepositoryTestClassImpl),
                                   typeForId,"type for cl id");
        }
 
        [Test]
        public void TestInterfaceNotCompatible() {
            string repIdCl = "IDL:Ch/Elca/Iiop/Tests/RepositoryTestClassImpl:1.0";
 
            Type required = typeof(IRepositoryTestIf3);
 
            Type typeForId;
            bool isCompatible =
                Repository.IsInterfaceCompatible(required, repIdCl,
                                                 out typeForId);
            // for non-verifiable type compatibility, return required Type for the id
            Assert.AreEqual( required,
                                   typeForId, "type for incompatible id");
            Assert.IsTrue(!isCompatible, "type compatibility for TestIf2");
        }


    }

}

#endif
