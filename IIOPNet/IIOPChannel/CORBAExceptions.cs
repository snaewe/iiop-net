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

        public UnknownSystemException(int minor, omg.org.CORBA.CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }
        
        protected UnknownSystemException(System.Runtime.Serialization.SerializationInfo info,
                                         System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }
        
        public UnknownSystemException() {
        }

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
        
        protected UnknownUserException(System.Runtime.Serialization.SerializationInfo info,
                                        System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }
        
        public UnknownUserException() {
        }

        #endregion IConstructors

    }

    /// <summary>
    /// this class is the base class for all user exceptions
    /// </summary>
    [Serializable]
    public abstract class AbstractUserException : Exception {
        
        #region IConstructors
        
        /// <summary>subclasses need all a no argument constructor, to be deserialisable</summary>
        protected AbstractUserException() { }
        protected AbstractUserException(string reason) : base(reason) {    }
        
        protected AbstractUserException(System.Runtime.Serialization.SerializationInfo info,
                                        System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }
        
        #endregion IConstructors

    }

    /// <summary>
    /// all non-system-exceptions are mapped to a generic user exception,
    /// because in .NET throws in method signature is not present
    /// </summary>
    [RepositoryIDAttribute("IDL:Ch/Elca/Iiop/GenericUserException:1.0")]
    [Serializable]
    [ExplicitSerializationOrdered()]
    public class GenericUserException : AbstractUserException {
        
        #region IFields
        
        /// <summary>
        /// recursively lists the name of the exceptions (i.e. includes inner exception names)
        /// </summary>
        /// <remarks>mapped to the name field in the CORBA exception for this class;
        /// is mapped as normal string and not as WStringValue to support Orbs, which don't implement value types</remarks>
        [StringValue()]
        [ExplicitSerializationOrderNr(0)]
        private string name = "";
        /// <summary>
        /// recursively lists the exception messages (i.e. includes inner exception messages)
        /// </summary>
        /// <remarks>mapped to the message field in the CORBA exception for this class;
        /// is mapped as normal string and not as WStringValue to support Orbs, which don't implement value types</remarks>
        [StringValue()]
        [ExplicitSerializationOrderNr(1)]
        private string message = "";
        /// <summary>
        /// recursively lists the methods throwed the exceptions
        /// <remarks>mapped to the throwingMethod field in the CORBA exception for this class;
        /// is mapped as normal string and not as WStringValue to support Orbs, which don't implement value types</remarks>
        [StringValue()]
        [ExplicitSerializationOrderNr(2)]
        private string throwingMethod = "";

        #endregion IFields
        #region IConstructors

        // needed for Deserialization
        public GenericUserException() : base() {
        }

        /// <param name="reason">the .NET exception encountered</param>
        public GenericUserException(Exception reason) : base(reason.Message) {
            AddExceptionDetails(reason);
        }
        
        protected GenericUserException(System.Runtime.Serialization.SerializationInfo info,
                                       System.Runtime.Serialization.StreamingContext context) : base(info, context) {
            this.name = info.GetString("name");
            this.message = info.GetString("message");
            this.throwingMethod = info.GetString("throwingMethod");
        }
        
        #endregion IConstructors
        #region IProperties
        
        public string ExceptionMessage {
            get {
                return message;
            }
        }
        
        public string ExceptionName {
            get {
                return name;
            }
        }
        
        public string ThrowingMethod {
            get {
                return throwingMethod;
            }
        }
        
        #endregion IProperties
        #region IMethods

        /// <summary>
        /// recursively adds the exception details to name, message, throwingMethod fields
        /// </summary>
        private void AddExceptionDetails(Exception exception) {
            if (exception == null) {
                return;
            }
            name += exception.GetType().Name;
            message += exception.Message;
            if (exception.TargetSite != null) {
                throwingMethod += exception.TargetSite.Name;
            }
            if (exception.InnerException != null) {
                name += "\n";
                message += "\n";
                throwingMethod += "\n";
                AddExceptionDetails(exception.InnerException);
            }
        }
        
        public override string ToString() {
            return "Name: " + name + "\r\nMessage: " + message + "\r\n----------------------\r\n\r\n" + base.ToString ();
        }
        
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info,
                                           System.Runtime.Serialization.StreamingContext context) {
            base.GetObjectData(info, context);
            info.AddValue("name", name);
            info.AddValue("message", message);
            info.AddValue("throwingMethod", throwingMethod);
        }

        #endregion IMethods

    }
    
    /// <summary>
    /// Contains constant values related to corba system exceptions.
    /// </summary>
    internal sealed class CorbaSystemExceptionCodes {
        
        #region Constants
        
        /// <summary>
        /// IIOP.NET is not able to connect to the target.
        /// </summary>
        internal const int TRANSIENT_CANTCONNECT = 4000;
        
        /// <summary>
        /// IIOP.NET detected, that an allocated connection has been dropped,
        /// before the request was sent.
        /// </summary>
        internal const int TRANSIENT_CONNECTION_DROPPED = 4001;
        
        /// <summary>
        /// IIOP.NET detected, that reading/writing to a connection is
        /// no longer possible for a request in progress.
        /// </summary>
        internal const int COMM_FAILURE_CONNECTION_DROPPED = 209;
        
        #endregion Constants
        #region IConstructors
        
        private CorbaSystemExceptionCodes() {
            
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

        protected AbstractCORBASystemException() : base("CORBA system exception") {
        }
        
        protected AbstractCORBASystemException(string exceptionDesc, int minor, CompletionStatus status) :
            base("CORBA system exception : " + exceptionDesc +
                 ", completed: " + status + " minor: " + minor) {
            m_minor = minor;
            m_status = status;
        }
        
        protected AbstractCORBASystemException(System.Runtime.Serialization.SerializationInfo info,
                                               System.Runtime.Serialization.StreamingContext context) : base(info, context) {
            m_minor = info.GetInt32("minor");
            m_status = (CompletionStatus)
                       info.GetValue("status", typeof(CompletionStatus));
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
        #region IMethods
        
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info,
                                           System.Runtime.Serialization.StreamingContext context) {
            info.AddValue("minor", m_minor);
            info.AddValue("status", m_status);
            base.GetObjectData(info, context);
        }
        
        #endregion IMethods

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/UNKNOWN:1.0")]
    [Serializable]
    public class UNKNOWN : AbstractCORBASystemException {
        
        #region IConstructors

        public UNKNOWN(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }
        
        protected UNKNOWN(System.Runtime.Serialization.SerializationInfo info,
                          System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }
        
        public UNKNOWN() {
        }

        #endregion IConstructors

    }
    
    [RepositoryIDAttribute("IDL:omg.org/CORBA/BAD_PARAM:1.0")]
    [Serializable]
    public class BAD_PARAM : AbstractCORBASystemException {
        
        #region IConstructors
        
        public BAD_PARAM(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }
        
        public BAD_PARAM(int minor, CompletionStatus status, string additionalDetail) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName +
                " ["+additionalDetail+"] ", minor, status) { }
        
        protected BAD_PARAM(System.Runtime.Serialization.SerializationInfo info,
                            System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public BAD_PARAM() {
        }

        #endregion IConstructors

    }
    
    [RepositoryIDAttribute("IDL:omg.org/CORBA/NO_MEMORY:1.0")]
    [Serializable]
    public class NO_MEMORY : AbstractCORBASystemException {

        #region IConstructors

        public NO_MEMORY(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }

        protected NO_MEMORY(System.Runtime.Serialization.SerializationInfo info,
                            System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public NO_MEMORY() {
        }
        
        #endregion IConstructors

    }
    
    [RepositoryIDAttribute("IDL:omg.org/CORBA/IMP_LIMIT:1.0")]
    [Serializable]
    public class IMP_LIMIT : AbstractCORBASystemException {

        #region IConstructors

        public IMP_LIMIT(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }

        protected IMP_LIMIT(System.Runtime.Serialization.SerializationInfo info,
                            System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public IMP_LIMIT() {
        }
        
        #endregion IConstructors

    }
    
    [RepositoryIDAttribute("IDL:omg.org/CORBA/COMM_FAILURE:1.0")]
    [Serializable]
    public class COMM_FAILURE : AbstractCORBASystemException {
 
        #region IConstructors

        public COMM_FAILURE(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }
        
        protected COMM_FAILURE(System.Runtime.Serialization.SerializationInfo info,
                               System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public COMM_FAILURE() {
        }

        #endregion IConstructors

    }
    
    [RepositoryIDAttribute("IDL:omg.org/CORBA/INV_OBJREF:1.0")]
    [Serializable]
    public class INV_OBJREF : AbstractCORBASystemException {

        #region IConstructors

        public INV_OBJREF(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }
        
        protected INV_OBJREF(System.Runtime.Serialization.SerializationInfo info,
                             System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public INV_OBJREF() {
        }

        #endregion IConstructors

    }
    
    [RepositoryIDAttribute("IDL:omg.org/CORBA/NO_PERMISSION:1.0")]
    [Serializable]
    public class NO_PERMISSION : AbstractCORBASystemException {
 
        #region IConstructors

        public NO_PERMISSION(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }
        
        protected NO_PERMISSION(System.Runtime.Serialization.SerializationInfo info,
                                System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public NO_PERMISSION() {
        }

        #endregion IConstructors

    }
    
    [RepositoryIDAttribute("IDL:omg.org/CORBA/INTERNAL:1.0")]
    [Serializable]
    public class INTERNAL : AbstractCORBASystemException {

        #region IConstructors

        public INTERNAL(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }

        protected INTERNAL(System.Runtime.Serialization.SerializationInfo info,
                           System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public INTERNAL() {
        }
        
        #endregion IConstructors

    }
    
    [RepositoryIDAttribute("IDL:omg.org/CORBA/MARSHAL:1.0")]
    [Serializable]
    public class MARSHAL : AbstractCORBASystemException {

        #region IConstructors

        public MARSHAL(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }
        
        protected MARSHAL(System.Runtime.Serialization.SerializationInfo info,
                          System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public MARSHAL() {
        }

        #endregion IConstructors
    }
    
    [RepositoryIDAttribute("IDL:omg.org/CORBA/INITALIZE:1.0")]
    [Serializable]
    public class INITALIZE : AbstractCORBASystemException {

        #region IConstructors

        public INITALIZE(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }
        
        protected INITALIZE(System.Runtime.Serialization.SerializationInfo info,
                            System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public INITALIZE() {
        }

        #endregion IConstructors
        
    }
    
    [RepositoryIDAttribute("IDL:omg.org/CORBA/NO_IMPLEMENT:1.0")]
    [Serializable]
    public class NO_IMPLEMENT : AbstractCORBASystemException {
 
        #region IConstructors

        public NO_IMPLEMENT(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }

        public NO_IMPLEMENT(int minor, CompletionStatus status, string missingtype) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName +
                " ["+missingtype+"] ", minor, status) { }
        
        protected NO_IMPLEMENT(System.Runtime.Serialization.SerializationInfo info,
                               System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public NO_IMPLEMENT() {
        }

        #endregion IConstructors

    }
    
    [RepositoryIDAttribute("IDL:omg.org/CORBA/BAD_TYPECODE:1.0")]
    [Serializable]
    public class BAD_TYPECODE : AbstractCORBASystemException {

        #region IConstructors

        public BAD_TYPECODE(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }

        protected BAD_TYPECODE(System.Runtime.Serialization.SerializationInfo info,
                               System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public BAD_TYPECODE() {
        }
        
        #endregion IConstructors

    }
    
    [RepositoryIDAttribute("IDL:omg.org/CORBA/BAD_OPERATION:1.0")]
    [Serializable]
    public class BAD_OPERATION : AbstractCORBASystemException {

        #region IConstructors

        public BAD_OPERATION(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }
        
        protected BAD_OPERATION(System.Runtime.Serialization.SerializationInfo info,
                                System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public BAD_OPERATION() {
        }

        #endregion IConstructors

    }
    
    [RepositoryIDAttribute("IDL:omg.org/CORBA/NO_RESOURCES:1.0")]
    [Serializable]
    public class NO_RESOURCES : AbstractCORBASystemException {

        #region IConstructors

        public NO_RESOURCES(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }
        
        protected NO_RESOURCES(System.Runtime.Serialization.SerializationInfo info,
                               System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public NO_RESOURCES() {
        }

        #endregion IConstructors

    }
    
    [RepositoryIDAttribute("IDL:omg.org/CORBA/NO_RESPONSE:1.0")]
    [Serializable]
    public class NO_RESPONSE : AbstractCORBASystemException {

        #region IConstructors

        public NO_RESPONSE(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }
        
        protected NO_RESPONSE(System.Runtime.Serialization.SerializationInfo info,
                              System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public NO_RESPONSE() {
        }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/PERSIST_STORE:1.0")]
    [Serializable]
    public class PERSIST_STORE : AbstractCORBASystemException {
 
        #region IConstructors

        public PERSIST_STORE(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }

        protected PERSIST_STORE(System.Runtime.Serialization.SerializationInfo info,
                                System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public PERSIST_STORE() {
        }
        
        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/BAD_INV_ORDER:1.0")]
    [Serializable]
    public class BAD_INV_ORDER : AbstractCORBASystemException {

        #region IConstructors

        public BAD_INV_ORDER(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }
        
        protected BAD_INV_ORDER(System.Runtime.Serialization.SerializationInfo info,
                                System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public BAD_INV_ORDER() {
        }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/TRANSIENT:1.0")]
    [Serializable]
    public class TRANSIENT : AbstractCORBASystemException {

        #region IConstructors

        public TRANSIENT(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }
        
        public TRANSIENT(int minor, CompletionStatus status, string additionalDetail) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName +
                " ["+additionalDetail+"] ", minor, status) { }
        
        
        protected TRANSIENT(System.Runtime.Serialization.SerializationInfo info,
                            System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public TRANSIENT() {
        }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/FREE_MEM:1.0")]
    [Serializable]
    public class FREE_MEM : AbstractCORBASystemException {

        #region IConstructors

        public FREE_MEM(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }
        
        protected FREE_MEM(System.Runtime.Serialization.SerializationInfo info,
                           System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public FREE_MEM() {
        }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/INV_IDENT:1.0")]
    [Serializable]
    public class INV_IDENT : AbstractCORBASystemException {

        #region IConstructors

        public INV_IDENT(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }
        
        protected INV_IDENT(System.Runtime.Serialization.SerializationInfo info,
                            System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public INV_IDENT() {
        }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/INV_FLAG:1.0")]
    [Serializable]
    public class INV_FLAG : AbstractCORBASystemException {

        #region IConstructors

        public INV_FLAG(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }
        
        protected INV_FLAG(System.Runtime.Serialization.SerializationInfo info,
                           System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public INV_FLAG() {
        }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/INTF_REPOS:1.0")]
    [Serializable]
    public class INTF_REPOS : AbstractCORBASystemException {

        #region IConstructors

        public INTF_REPOS(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }
        
        protected INTF_REPOS(System.Runtime.Serialization.SerializationInfo info,
                             System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public INTF_REPOS() {
        }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/BAD_CONTEXT:1.0")]
    [Serializable]
    public class BAD_CONTEXT : AbstractCORBASystemException {

        #region IConstructors

        public BAD_CONTEXT(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }
        
        protected BAD_CONTEXT(System.Runtime.Serialization.SerializationInfo info,
                              System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public BAD_CONTEXT() {
        }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/OBJ_ADAPTER:1.0")]
    [Serializable]
    public class OBJ_ADAPTER : AbstractCORBASystemException {
 
        #region IConstructors

        public OBJ_ADAPTER(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }
        
        protected OBJ_ADAPTER(System.Runtime.Serialization.SerializationInfo info,
                              System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public OBJ_ADAPTER() {
        }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/DATA_CONVERSION:1.0")]
    [Serializable]
    public class DATA_CONVERSION : AbstractCORBASystemException {
 
        #region IConstructors

        public DATA_CONVERSION(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }
        
        protected DATA_CONVERSION(System.Runtime.Serialization.SerializationInfo info,
                                  System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public DATA_CONVERSION() {
        }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/OBJECT_NOT_EXIST:1.0")]
    [Serializable]
    public class OBJECT_NOT_EXIST : AbstractCORBASystemException {

        #region IConstructors

        public OBJECT_NOT_EXIST(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }
        
        protected OBJECT_NOT_EXIST(System.Runtime.Serialization.SerializationInfo info,
                                   System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public OBJECT_NOT_EXIST() {
        }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/TRANSACTION_REQUIRED:1.0")]
    [Serializable]
    public class TRANSACTION_REQUIRED : AbstractCORBASystemException {
 
        #region IConstructors

        public TRANSACTION_REQUIRED(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }
        
        protected TRANSACTION_REQUIRED(System.Runtime.Serialization.SerializationInfo info,
                                       System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public TRANSACTION_REQUIRED() {
        }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/INV_POLICY:1.0")]
    [Serializable]
    public class INV_POLICY : AbstractCORBASystemException {

        #region IConstructors

        public INV_POLICY(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }
        
        protected INV_POLICY(System.Runtime.Serialization.SerializationInfo info,
                             System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public INV_POLICY() {
        }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/CODESET_INCOMPATIBLE:1.0")]
    [Serializable]
    public class CODESET_INCOMPATIBLE : AbstractCORBASystemException {

        #region IConstructors

        public CODESET_INCOMPATIBLE(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }

        protected CODESET_INCOMPATIBLE(System.Runtime.Serialization.SerializationInfo info,
                                       System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public CODESET_INCOMPATIBLE() {
        }
        
        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/TRANSACTION_MODE:1.0")]
    [Serializable]
    public class TRANSACTION_MODE : AbstractCORBASystemException {

        #region IConstructors

        public TRANSACTION_MODE(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }
        
        protected TRANSACTION_MODE(System.Runtime.Serialization.SerializationInfo info,
                                   System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public TRANSACTION_MODE() {
        }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/TRANSACTION_UNAVAILABLE:1.0")]
    [Serializable]
    public class TRANSACTION_UNAVAILABLE : AbstractCORBASystemException {

        #region IConstructors

        public TRANSACTION_UNAVAILABLE(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }
        
        protected TRANSACTION_UNAVAILABLE(System.Runtime.Serialization.SerializationInfo info,
                                          System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public TRANSACTION_UNAVAILABLE() {
        }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/REBIND:1.0")]
    [Serializable]
    public class REBIND : AbstractCORBASystemException {

        #region IConstructors

        public REBIND(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }

        protected REBIND(System.Runtime.Serialization.SerializationInfo info,
                         System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public REBIND() {
        }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/TIMEOUT:1.0")]
    [Serializable]
    public class TIMEOUT : AbstractCORBASystemException {

        #region IConstructors

        public TIMEOUT(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }
        
        protected TIMEOUT(System.Runtime.Serialization.SerializationInfo info,
                          System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public TIMEOUT() {
        }

        #endregion IConstructors

    }

    [RepositoryIDAttribute("IDL:omg.org/CORBA/BAD_QOS:1.0")]
    [Serializable]
    public class BAD_QOS : AbstractCORBASystemException {

        #region IConstructors

        public BAD_QOS(int minor, CompletionStatus status) :
            base(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.FullName, minor, status) { }
        
        protected BAD_QOS(System.Runtime.Serialization.SerializationInfo info,
                          System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public BAD_QOS() {
        }

        #endregion IConstructors

    }


}
