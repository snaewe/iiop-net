/* CORBAExceptions.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 24.01.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using Ch.Elca.Iiop.Idl;


namespace Ch.Elca.Iiop {

    /// <summary>
    /// this class is a class used, if the system-exception-type received is unknown
    /// </summary>
    [Serializable]
    public class UnknownSystemException : omg.org.CORBA.AbstractCORBASystemException {
        
        #region IConstructors

        public UnknownSystemException(int minor, omg.org.CORBA.CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors
    }

    /// <summary>
    /// this class is used, if the user-exception-type received is unknown
    /// </summary>
    [Serializable]
    public class UnknownUserException : AbstractUserException {

        #region IConstructors

        public UnknownUserException(string reason) : base(reason) {
        }

        #endregion IConstructors

    }

    /// <summary>
    /// this class is the base class for all user exceptions mapped from IDL
    /// </summary>
    [Serializable]
    public abstract class AbstractUserException : Exception, IIdlEntity {
        
        #region IConstructors
        
        /// <summary>subclasses need all a no argument constructor, to be deserialisable</summary>
        public AbstractUserException() { }
        protected AbstractUserException(string reason) : base(reason) {    }

        #endregion IConstructors

    }

    /// <summary>
    /// all non-system-exceptions are mapped to a generic user exception, 
    /// because in .NET throws in method signature is not present
    /// </summary>
    [Serializable]
    public class GenericUserException : AbstractUserException {
        
        #region IFields

        private string m_exceptionString;
        private string m_innerExceptionMsg = null;
        private string m_throwingMethod;

        #endregion IFields
        #region IConstructors

        // needed for Deserialization
        public GenericUserException() : base() {
        }

        /// <param name="reason">the .NET exception encountered</param>
        public GenericUserException(Exception reason) : base(reason.Message) {
            m_exceptionString = reason.Message;
            if (reason.InnerException != null) 
            {
                m_innerExceptionMsg = reason.InnerException.Message;
            }
            m_throwingMethod = reason.TargetSite.Name;
        }

        #endregion IConstructors

    }

}

namespace omg.org.CORBA {

    // Code convention: identifier names are not according the convention, because the following is mapped from
    // IDL by hand
    
    [IdlEnumAttribute]
    public enum CompletionStatus { Completed_Yes, Completed_No, Completed_MayBe }

    /// <summary>
    /// this class is the base class for all corba system exceptions mapped from IDL
    /// </summary>
    [Serializable]
    public abstract class AbstractCORBASystemException : Exception, IIdlEntity {

        #region IFields
        
        private int m_minor;
        private CompletionStatus m_status;

        #endregion IFields
        #region IConstructors

        public AbstractCORBASystemException() : base("CORBA system exception") {
        }
        
        public AbstractCORBASystemException(int minor, CompletionStatus status) : base("CORBA system exception, completed: " + status + " minor: " + minor) {
            m_minor = minor;
            m_status = status;
        }

        #endregion IConstructors
        #region IProperties

        public int Minor {
            get {
            	return m_minor; 
            }
        }
        
        public CompletionStatus Status {
            get { 
            	return m_status; 
            }
        }

        #endregion IProperties

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/UNKNOWN:1.0")]
    [Serializable]
    public class UNKNOWN : AbstractCORBASystemException { 
        
        #region IConstructors

        public UNKNOWN(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }
    
    [RepositoryIDAttribute("IDL:omg.org/CORBA/BAD_PARAM:1.0")]
    [Serializable]
    public class BAD_PARAM : AbstractCORBASystemException { 
        
        #region IConstructors
        
        public BAD_PARAM(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }
    
    [RepositoryIDAttribute("IDL:omg.org/CORBA/NO_MEMORY:1.0")]
    [Serializable]
    public class NO_MEMORY : AbstractCORBASystemException { 

        #region IConstructors

        public NO_MEMORY(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }
    
    [RepositoryIDAttribute("IDL:omg.org/CORBA/IMP_LIMIT:1.0")]
    [Serializable]
    public class IMP_LIMIT : AbstractCORBASystemException { 

        #region IConstructors

        public IMP_LIMIT(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }
    
    [RepositoryIDAttribute("IDL:omg.org/CORBA/COMM_FAILURE:1.0")]
    [Serializable]
    public class COMM_FAILURE : AbstractCORBASystemException {
 
        #region IConstructors

        public COMM_FAILURE(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }
    
    [RepositoryIDAttribute("IDL:omg.org/CORBA/INV_OBJREF:1.0")]
    [Serializable]
    public class INV_OBJREF : AbstractCORBASystemException {

        #region IConstructors

        public INV_OBJREF(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }
    
    [RepositoryIDAttribute("IDL:omg.org/CORBA/NO_PERMISSION:1.0")]
    [Serializable]
    public class NO_PERMISSION : AbstractCORBASystemException {
 
        #region IConstructors

        public NO_PERMISSION(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }
    
    [RepositoryIDAttribute("IDL:omg.org/CORBA/INTERNAL:1.0")]
    [Serializable]
    public class INTERNAL : AbstractCORBASystemException {

        #region IConstructors

        public INTERNAL(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }
    
    [RepositoryIDAttribute("IDL:omg.org/CORBA/MARSHAL:1.0")]
    [Serializable]
    public class MARSHAL : AbstractCORBASystemException { 

        #region IConstructors

        public MARSHAL(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors
    }
    
    [RepositoryIDAttribute("IDL:omg.org/CORBA/INITALIZE:1.0")]
    [Serializable]
    public class INITALIZE : AbstractCORBASystemException { 

        #region IConstructors

        public INITALIZE(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors
        
    }
    
    [RepositoryIDAttribute("IDL:omg.org/CORBA/NO_IMPLEMENT:1.0")]
    [Serializable]
    public class NO_IMPLEMENT : AbstractCORBASystemException {
 
        #region IConstructors

        public NO_IMPLEMENT(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }
    
    [RepositoryIDAttribute("IDL:omg.org/CORBA/BAD_TYPECODE:1.0")]
    [Serializable]
    public class BAD_TYPECODE : AbstractCORBASystemException { 

        #region IConstructors

        public BAD_TYPECODE(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }
    
    [RepositoryIDAttribute("IDL:omg.org/CORBA/BAD_OPERATION:1.0")]
    [Serializable]
    public class BAD_OPERATION : AbstractCORBASystemException { 

        #region IConstructors

        public BAD_OPERATION(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }
    
    [RepositoryIDAttribute("IDL:omg.org/CORBA/NO_RESOURCES:1.0")]
    [Serializable]
    public class NO_RESOURCES : AbstractCORBASystemException { 

        #region IConstructors

        public NO_RESOURCES(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }
    
    [RepositoryIDAttribute("IDL:omg.org/CORBA/NO_RESPONSE:1.0")]
    [Serializable]
    public class NO_RESPONSE : AbstractCORBASystemException { 

        #region IConstructors

        public NO_RESPONSE(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/PERSIST_STORE:1.0")]
    [Serializable]
    public class PERSIST_STORE : AbstractCORBASystemException {
 
        #region IConstructors

        public PERSIST_STORE(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/BAD_INV_ORDER:1.0")]
    [Serializable]
    public class BAD_INV_ORDER : AbstractCORBASystemException { 

        #region IConstructors

        public BAD_INV_ORDER(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/TRANSIENT:1.0")]
    [Serializable]
    public class TRANSIENT : AbstractCORBASystemException { 

        #region IConstructors

        public TRANSIENT(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/FREE_MEM:1.0")]
    [Serializable]
    public class FREE_MEM : AbstractCORBASystemException { 

        #region IConstructors

        public FREE_MEM(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/INV_IDENT:1.0")]
    [Serializable]
    public class INV_IDENT : AbstractCORBASystemException { 

        #region IConstructors

        public INV_IDENT(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/INV_FLAG:1.0")]
    [Serializable]
    public class INV_FLAG : AbstractCORBASystemException { 

        #region IConstructors

        public INV_FLAG(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/INTF_REPOS:1.0")]
    [Serializable]
    public class INTF_REPOS : AbstractCORBASystemException { 

        #region IConstructors

        public INTF_REPOS(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/BAD_CONTEXT:1.0")]
    [Serializable]
    public class BAD_CONTEXT : AbstractCORBASystemException { 

        #region IConstructors

        public BAD_CONTEXT(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/OBJ_ADAPTER:1.0")]
    [Serializable]
    public class OBJ_ADAPTER : AbstractCORBASystemException {
 
        #region IConstructors

        public OBJ_ADAPTER(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/DATA_CONVERSION:1.0")]
    [Serializable]
    public class DATA_CONVERSION : AbstractCORBASystemException {
 
        #region IConstructors

        public DATA_CONVERSION(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/OBJECT_NOT_EXIST:1.0")]
    [Serializable]
    public class OBJECT_NOT_EXIST : AbstractCORBASystemException { 

        #region IConstructors

        public OBJECT_NOT_EXIST(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/TRANSACTION_REQUIRED:1.0")]
    [Serializable]
    public class TRANSACTION_REQUIRED : AbstractCORBASystemException {
 
        #region IConstructors

        public TRANSACTION_REQUIRED(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/INV_POLICY:1.0")]
    [Serializable]
    public class INV_POLICY : AbstractCORBASystemException { 

        #region IConstructors

        public INV_POLICY(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/CODESET_INCOMPATIBLE:1.0")]
    [Serializable]
    public class CODESET_INCOMPATIBLE : AbstractCORBASystemException { 

        #region IConstructors

        public CODESET_INCOMPATIBLE(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/TRANSACTION_MODE:1.0")]
    [Serializable]
    public class TRANSACTION_MODE : AbstractCORBASystemException { 

        #region IConstructors

        public TRANSACTION_MODE(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/TRANSACTION_UNAVAILABLE:1.0")]
    [Serializable]
    public class TRANSACTION_UNAVAILABLE : AbstractCORBASystemException { 

        #region IConstructors

        public TRANSACTION_UNAVAILABLE(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/REBIND:1.0")]
    [Serializable]
    public class REBIND : AbstractCORBASystemException { 

        #region IConstructors

        public REBIND(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/TIMEOUT:1.0")]
    [Serializable]
    public class TIMEOUT : AbstractCORBASystemException { 

        #region IConstructors

        public TIMEOUT(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/BAD_QOS:1.0")]
    [Serializable]
    public class BAD_QOS : AbstractCORBASystemException { 

        #region IConstructors

        public BAD_QOS(int minor, CompletionStatus status) : base(minor, status) { }

        #endregion IConstructors

    }


}
