/* CORBANameService.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 23.01.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using Ch.Elca.Iiop.Idl;
using Ch.Elca.Iiop;
using System.Reflection;
using System.Diagnostics;
using omg.org.CORBA;

namespace omg.org.CosNaming {


    // violation of the coding conventions, because of mapping IDL -> CLS.

    [Serializable]
    [IdlStructAttribute]
    [RepositoryIDAttribute("IDL:omg.org/CosNaming/NameComponent:1.0")]
    public struct NameComponent : IIdlEntity {
             
        #region IFields

        [WideChar(false)]
        [StringValueAttribute]
        private string m_id;
        [WideChar(false)]
        [StringValueAttribute]
        private string m_kind;

        #endregion IFields
        #region IConstructors

        public NameComponent(string id, string kind) {
            m_id = id;
            m_kind = kind;
        }

        #endregion IConstructors
        #region IProperties

        public string id {
            get { 
                return m_id; 
            }
        }
        public string kind {
            get { 
                return m_kind; 
            }
        }

        #endregion IProperties

    }
 
    /// <summary>
    /// the interface of the namingcontext
    /// </summary>
    [RepositoryIDAttribute("IDL:omg.org/CosNaming/NamingContext:1.0")]
    [InterfaceTypeAttribute(IdlTypeInterface.ConcreteInterface)]
    public interface NamingContext : IIdlEntity {
        
        #region IMethods

        /// <summary>resolve a bound object</summary>
        MarshalByRefObject resolve([IdlSequenceAttribute] NameComponent[] nameComponents);
        /// <summary>bind an object for the name</summary>
        void bind([IdlSequenceAttribute] NameComponent[] nameComponents, MarshalByRefObject obj);

        void rebind([IdlSequenceAttribute] NameComponent[] nameComponents, MarshalByRefObject obj);
    
        void  bind_context([IdlSequenceAttribute] NameComponent[] nameComponents, NamingContext toBind);
    
        void  rebind_context([IdlSequenceAttribute] NameComponent[] nameComponents, NamingContext toRebind);
    
        void  unbind([IdlSequenceAttribute] NameComponent[] nameComponents);
    
        NamingContext new_context();
    
        NamingContext bind_new_context([IdlSequenceAttribute] NameComponent[] nameComponents);
        
        void destroy();

        #endregion IMethods

    }


    /// <summary>
    /// the interface of the namingcontextExt
    /// </summary>
    [RepositoryIDAttribute("IDL:omg.org/CosNaming/NamingContextExt:1.0")]
    [InterfaceTypeAttribute(IdlTypeInterface.ConcreteInterface)]
    public interface NamingContextExt : NamingContext {
    }
    
    
    /// <summary>
    /// This is an implementation of the COSNamingContext. 
    /// </summary>
    /// <remarks>
    /// here, it's required to use the Repository-id: "IDL:omg.org/CosNaming/NamingContext:1.0"
    /// </remarks>
    [RepositoryIDAttribute("IDL:omg.org/CosNaming/NamingContext:1.0")]
    public class COSNamingContextImpl : MarshalByRefObject, NamingContext {

        #region IFields

        private Hashtable m_nameTable = new Hashtable();

        #endregion IFields
        #region IConstructors

        public COSNamingContextImpl() {            
        }

        #endregion IConstructors
        #region IMethods
        
        public MarshalByRefObject resolve([IdlSequenceAttribute] NameComponent[] nameComponents) {
            // create uri for name-components:
            string name = CreateNameForNameComponents(nameComponents);        
            
            lock(m_nameTable.SyncRoot) {
                if (m_nameTable.ContainsKey(name)) {
                    // is in the own name-table
                    return (MarshalByRefObject)m_nameTable[name];
                }
            }
            
            // Type serverType = RemotingServices.GetServerTypeForUri(uri);
            // get it from the .NET name service
            return GetObjectRegisteredAtUri(name, nameComponents);
        }

        private MarshalByRefObject GetObjectRegisteredAtUri(string uri, NameComponent[] nameComponents) {
            // this is not nice, because it does circument internal on IdentityHolder-class
            Debug.WriteLine("get registeredObject: " + uri);
            Assembly remotingAssembly = Assembly.LoadWithPartialName("mscorlib");
            if (remotingAssembly == null) { 
                throw new INTERNAL(16001, CompletionStatus.Completed_MayBe); 
            }
            Type identityHolderType = remotingAssembly.GetType("System.Runtime.Remoting.IdentityHolder");
            if (identityHolderType == null) { 
                throw new INTERNAL(16002, CompletionStatus.Completed_MayBe); 
            }
            // identityHolder class, manages the published remote objects
            // get the resolveUri-method to get the Identity for the URI
            MethodInfo resolveIdMethod = identityHolderType.GetMethod("ResolveIdentity", 
                                                                      BindingFlags.Static | BindingFlags.NonPublic);
            if (resolveIdMethod == null) { 
                throw new INTERNAL(16003, CompletionStatus.Completed_MayBe); 
            }
            
            // now call resolve-method:
            object identity = resolveIdMethod.Invoke(null, new object[] { uri } );
            if (identity == null) { 
                throw new NamingContext_package.NotFound(NamingContext_package.NotFoundReason.missing_node, nameComponents); 
            }

            // now get the object from the identity
            Type identityType = remotingAssembly.GetType("System.Runtime.Remoting.Identity");
            if (identityType == null) { 
                throw new INTERNAL(16004, CompletionStatus.Completed_MayBe); 
            }
            
            // property TPOrObject holds the object, therefor access this property
            PropertyInfo tpOrObjProp = identityType.GetProperty("TPOrObject", 
                                                                BindingFlags.Instance | BindingFlags.NonPublic);
            if (tpOrObjProp == null) { 
                throw new INTERNAL(16005, CompletionStatus.Completed_MayBe); 
            }
            MarshalByRefObject result = (MarshalByRefObject)tpOrObjProp.GetValue(identity, null);
            if (result == null) { 
                throw new INTERNAL(16006, CompletionStatus.Completed_MayBe); 
            }
            return result;
        }

        /// <summary>create a name for the name-components</summary>
        private string CreateNameForNameComponents(NameComponent[] nameComponents) {
            string name = "";
            foreach (NameComponent component in nameComponents) {
                if (!name.Equals("")) { name = name + "/"; }
                if (component.kind != null && (!component.kind.Equals(""))) {
                    name = name + component.id + "." + component.kind;
                } else {
                    name = name + component.id;
                }
            }
            return name;
        }
        
        public void bind([IdlSequenceAttribute] NameComponent[] nameComponents, MarshalByRefObject obj) {
            string name = CreateNameForNameComponents(nameComponents);
            // bind the object to the name
            
            // register it in the nametable
            lock(m_nameTable.SyncRoot) {
                if (m_nameTable.ContainsKey(name)) { 
                    throw new NamingContext_package.AlreadyBound(); 
                }
                m_nameTable.Add(name, obj);
            }
        }

        public void unbind([IdlSequenceAttribute] NameComponent[] nameComponents) {
            string name = CreateNameForNameComponents(nameComponents);
            lock(m_nameTable.SyncRoot) {
                if (!(m_nameTable.ContainsKey(name))) { 
                    throw new NamingContext_package.NotFound(NamingContext_package.NotFoundReason.missing_node, nameComponents); 
                }
                m_nameTable.Remove(name);
            }
        }

        public void rebind([IdlSequenceAttribute] NameComponent[] nameComponents, MarshalByRefObject obj) {
            unbind(nameComponents);
            bind(nameComponents, obj);
        }

        public void destroy() {
            throw new NO_IMPLEMENT(0, CompletionStatus.Completed_MayBe);
        }

        public NamingContext new_context() {
            throw new NO_IMPLEMENT(0, CompletionStatus.Completed_MayBe);
        }

        public void rebind_context([IdlSequenceAttribute] NameComponent[] nameComponents, NamingContext nameCtx) {
            throw new NO_IMPLEMENT(0, CompletionStatus.Completed_MayBe);
        }

        public void bind_context([IdlSequenceAttribute] NameComponent[] nameComponents, NamingContext nameCtx) {
            throw new NO_IMPLEMENT(0, omg.org.CORBA.CompletionStatus.Completed_MayBe);
        }

        public NamingContext bind_new_context([IdlSequenceAttribute] NameComponent[] nameComponents) {
            throw new NO_IMPLEMENT(0, CompletionStatus.Completed_MayBe);
        }

        #endregion IMethods
        
    }


    /// <summary>
    /// This is an implementation of the COSNamingContext, living forever
    /// </summary>
    /// <remarks>
    /// here, it's required to use the Repository-id: "IDL:omg.org/CosNaming/NamingContext:1.0"
    /// </remarks>
    [RepositoryIDAttribute("IDL:omg.org/CosNaming/NamingContext:1.0")]
    public class InitialCOSNamingContextImpl : COSNamingContextImpl {

        #region SFields
        
        internal static byte[] s_initalnamingObjKey = 
            new byte[] { 0x4E, 0x61, 0x6D, 0x65, 0x53, 0x65, 0x72, 0x76, 0x69, 0x63, 0x65 };

        #endregion SFields
        #region IMethods
        
        public override Object InitializeLifetimeService() {
            // this should live forever
            return null;
        }

        #endregion IMethods

    }


}

namespace omg.org.CosNaming.NamingContext_package {

    [IdlEnumAttribute()]
    public enum NotFoundReason {
        missing_node, not_context, not_object
    }

    [RepositoryIDAttribute("IDL:omg.org/CosNaming/NamingContext/NotFound:1.0")]
    [Serializable]
    public class NotFound : AbstractUserException {
        
        #region IFields
        
        public NotFoundReason why;
        [IdlSequenceAttribute()]
        public NameComponent[] rest_of_name;

        #endregion IFields
        #region IConstructors
        
        /// <summary>constructor needed for deserialisation</summary>
        public NotFound() {
        }

        public NotFound(NotFoundReason why, NameComponent[] restOfName) {
            this.why = why;
            this.rest_of_name = restOfName;
        }

        #endregion IConstructors 

    } 

    [RepositoryIDAttribute("IDL:omg.org/CosNaming/NamingContext/CannotProceed:1.0")]
    [Serializable]
    public class CannotProceed : AbstractUserException {
        
        #region IFields
        
        public NamingContext cxt;
        [IdlSequenceAttribute()]
        public NameComponent[] rest_of_name;

        #endregion IFields
        #region IConstructors

        /// <summary>constructor needed for deserialisation</summary>
        public CannotProceed() { }

        #endregion IConstructors

    } 

    [RepositoryIDAttribute("IDL:omg.org/CosNaming/NamingContext/InvalidName:1.0")]
    [Serializable]
    public class InvalidName : AbstractUserException {
        
        #region IConstructors

        /// <summary>constructor needed for deserialisation</summary>
        public InvalidName() { }

        #endregion IConstructors

    } 

    [RepositoryIDAttribute("IDL:omg.org/CosNaming/NamingContext/AlreadyBound:1.0")]
    [Serializable]
    public class AlreadyBound : AbstractUserException {
        
        #region IConstructors

        /// <summary>constructor needed for deserialisation</summary>
        public AlreadyBound() { }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CosNaming/NamingContext/NotEmpty:1.0")]
    [Serializable]
    public class NotEmpty : AbstractUserException {
        
        #region IConstructors
        
        /// <summary>constructor needed for deserialisation</summary>
        public NotEmpty() { }

        #endregion IConstructors
    }

}
