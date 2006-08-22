/* ExceptionImpl.cs
 *
 * Project: IIOP.NET
 * Examples
 *
 * WHEN      RESPONSIBLE
 * 24.06.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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


namespace ch.elca.iiop.demo.ejbChatroom {

    using java.lang;

    [Serializable]
    public class AlreadyRegisteredExceptionImpl : AlreadyRegisteredException {
        
        #region IFields

        private ThrowableData m_data = new ThrowableData();

        #endregion IFields

        public AlreadyRegisteredExceptionImpl() : base() {
        }

        public override Throwable initCause(Throwable arg) {
            return null;
        }
            
        public override string toString() {
            return ToString();
        }

        public override Throwable fillInStackTrace() {
            return null;
        }

        public override Throwable cause {
            get { return m_data.Cause; }
        }

        public override string localizedMessage {
            get { return m_data.Msg; }
        }

        public override string message {
            get { return m_data.Msg; }
        }

        public override void printStackTrace__() {          
        }

        public override void printStackTrace__java_io_PrintStream(java.io.PrintStream arg) {
        }

        public override void printStackTrace__java_io_PrintWriter(java.io.PrintWriter arg) {
        }

        public override StackTraceElement[] stackTrace {
            get { return m_data.Trace; }
            set { }
        }

        public override string ToString() {
            return base.ToString() + "; msg: " + m_data.Msg;
        }

        public override void Deserialise(Corba.DataInputStream stream) {
            m_data.Deserialise(stream);
        }
            
        public override void Serialize(Corba.DataOutputStream stream) {
            m_data.Serialise(stream);
        }

    }

    [Serializable]
    public class NotRegisteredExceptionImpl : NotRegisteredException {

        #region IFields

        private ThrowableData m_data = new ThrowableData();

        #endregion IFields
        
        public NotRegisteredExceptionImpl() : base() {
        }

        public override Throwable initCause(Throwable arg) {
            return null;
        }
            
        public override string toString() {
            return ToString();
        }

        public override Throwable fillInStackTrace() {
            return null;
        }

        public override Throwable cause {
            get { return m_data.Cause; }
        }

        public override string localizedMessage {
            get { return m_data.Msg; }
        }

        public override string message {
            get { return m_data.Msg; }
        }

        public override void printStackTrace__() {          
        }

        public override void printStackTrace__java_io_PrintStream(java.io.PrintStream arg) {
        }

        public override void printStackTrace__java_io_PrintWriter(java.io.PrintWriter arg) {
        }

        public override StackTraceElement[] stackTrace {
            get { return m_data.Trace; }
            set { }
        }

        public override string ToString() {
            return base.ToString() + "; msg: " + m_data.Msg;
        }

        public override void Deserialise(Corba.DataInputStream stream) {
            m_data.Deserialise(stream);
        }
            
        public override void Serialize(Corba.DataOutputStream stream) {
            m_data.Serialise(stream);
        }

    }
}

namespace javax.ejb {

    using java.lang;


    [Serializable]
    public class CreateExceptionImpl : CreateException {
        
        #region IFields

        private ThrowableData m_data = new ThrowableData();

        #endregion IFields

        public CreateExceptionImpl() : base() {
        }
        
        public override void Deserialise(Corba.DataInputStream stream) {
            m_data.Deserialise(stream);
        }
            
        public override void Serialize(Corba.DataOutputStream stream) {
            m_data.Serialise(stream);
        }

        public override Throwable initCause(Throwable arg) {
            return null;
        }
            
        public override string toString() {
            return ToString();
        }

        public override Throwable fillInStackTrace() {
            return null;
        }

        public override Throwable cause {
            get { return m_data.Cause; }
        }

        public override string localizedMessage {
            get { return m_data.Msg; }
        }

        public override string message {
            get { return m_data.Msg; }
        }

        public override void printStackTrace__() {          
        }

        public override void printStackTrace__java_io_PrintStream(java.io.PrintStream arg) {
        }

        public override void printStackTrace__java_io_PrintWriter(java.io.PrintWriter arg) {
        }

        public override StackTraceElement[] stackTrace {
            get { return m_data.Trace; }
            set { }
        }

        public override string ToString() {
            return base.ToString() + "; msg: " + m_data.Msg;
        }

    }

    [Serializable]
    public class RemoveExceptionImpl : RemoveException {

        #region IFields

        private ThrowableData m_data = new ThrowableData();

        #endregion IFields

        public RemoveExceptionImpl() : base() {
        }
        
        public override void Deserialise(Corba.DataInputStream stream) {
            m_data.Deserialise(stream);
        }
            
        public override void Serialize(Corba.DataOutputStream stream) {
            m_data.Serialise(stream);
        }

        public override Throwable initCause(Throwable arg) {
            return null;
        }
            
        public override string toString() {
            return ToString();
        }

        public override Throwable fillInStackTrace() {
            return null;
        }

        public override Throwable cause {
            get { return m_data.Cause; }
        }

        public override string localizedMessage {
            get { return m_data.Msg; }
        }

        public override string message {
            get { return m_data.Msg; }
        }

        public override void printStackTrace__() {          
        }

        public override void printStackTrace__java_io_PrintStream(java.io.PrintStream arg) {
        }

        public override void printStackTrace__java_io_PrintWriter(java.io.PrintWriter arg) {
        }

        public override StackTraceElement[] stackTrace {
            get { return m_data.Trace; }
            set { }
        }

        public override string ToString() {
            return base.ToString() + "; msg: " + m_data.Msg;
        }

    }



}


namespace java.lang {


    /// <summary>
    /// used to Deserialise the java exception data
    /// </summary>
    public class ThrowableData {

        private java.lang.Throwable m_cause;
        private string m_msg;
        private java.lang.StackTraceElement[] m_trace;

        public java.lang.Throwable Cause {
            get { 
                return m_cause; 
            }
        }

        public string Msg {
            get { 
                return m_msg; 
            }
        }

        public java.lang.StackTraceElement[] Trace {
            get { 
                return m_trace; 
            }
        }

        public void Deserialise(Corba.DataInputStream stream) {
            stream.read_octet(); // ignore format version: java RMI specific
            stream.read_boolean(); // ignore default read object: java RMI specific
            
            m_cause = (java.lang.Throwable)stream.read_ValueOfType(typeof(java.lang.Throwable));
            m_msg = stream.read_WStringValue();

            object boxedTrace = stream.read_Value();
            if (boxedTrace != null) {
                m_trace = (StackTraceElement[])((BoxedValueBase) boxedTrace).Unbox();
            }
       }

       public void Serialise(Corba.DataOutputStream stream) {
           throw new omg.org.CORBA.NO_IMPLEMENT(2876, omg.org.CORBA.CompletionStatus.Completed_MayBe);
       }

    }

    
    public class StackTraceElementImpl : StackTraceElement {

        public override int hashCode() {
            return GetHashCode();
        }           
        
        public override string toString() {
            return "StackTraceElement:\n" + "in class: " + m_declaringClass + 
                   " (file: " + m_fileName_ + "); method: " + m_methodName_;
        }

        public override bool equals([ObjectIdlTypeAttribute(IdlTypeObject.Any)]  object arg) {
            return this.Equals(arg);
        }

        public override string className {
            get { return m_declaringClass; }
        }

        public override string fileName {
            get { return m_fileName_; }
        }

        public override int lineNumber {
            get { return m_lineNumber_; }
        }

        public override string methodName {
            get { return m_methodName_; }
        }

        public override bool nativeMethod {
            get { return false; }
        }
        
    }

    /// <summary>
    /// implementation of the java exception value type (for java 1.4)
    /// </summary>
    [Serializable]
    public class _ExceptionImpl : _Exception {

        #region IFields

        private ThrowableData m_data = new ThrowableData();

        #endregion IFields

        public _ExceptionImpl() : base() {
        }
        
        public override void Deserialise(Corba.DataInputStream stream) {
            m_data.Deserialise(stream);
        }
            
        public override void Serialize(Corba.DataOutputStream stream) {
            m_data.Serialise(stream);
        }

        public override Throwable initCause(Throwable arg) {
            return null;
        }
            
        public override string toString() {
            return ToString();
        }

        public override Throwable fillInStackTrace() {
            return null;
        }

        public override Throwable cause {
            get { return m_data.Cause; }
        }

        public override string localizedMessage {
            get { return m_data.Msg; }
        }

        public override string message {
            get { return m_data.Msg; }
        }

        public override void printStackTrace__() {          
        }

        public override void printStackTrace__java_io_PrintStream(java.io.PrintStream arg) {
        }

        public override void printStackTrace__java_io_PrintWriter(java.io.PrintWriter arg) {
        }

        public override StackTraceElement[] stackTrace {
            get { return m_data.Trace; }
            set { }
        }

        public override string ToString() {
            return base.ToString() + "; msg: " + m_data.Msg;
        }

    }


    [Serializable]
    public class ThrowableImpl : Throwable {

        #region IFields

        private ThrowableData m_data = new ThrowableData();

        #endregion IFields

        public ThrowableImpl() : base() {
        }
        
        public override void Deserialise(Corba.DataInputStream stream) {
            m_data.Deserialise(stream);
        }
            
        public override void Serialize(Corba.DataOutputStream stream) {
            m_data.Serialise(stream);
        }

        public override Throwable initCause(Throwable arg) {
            return null;
        }
            
        public override string toString() {
            return ToString();
        }

        public override Throwable fillInStackTrace() {
            return null;
        }

        public override Throwable cause {
            get { return m_data.Cause; }
        }

        public override string localizedMessage {
            get { return m_data.Msg; }
        }

        public override string message {
            get { return m_data.Msg; }
        }

        public override void printStackTrace__() {          
        }

        public override void printStackTrace__java_io_PrintStream(java.io.PrintStream arg) {
        }

        public override void printStackTrace__java_io_PrintWriter(java.io.PrintWriter arg) {
        }

        public override StackTraceElement[] stackTrace {
            get { return m_data.Trace; }
            set { }
        }

        public override string ToString() {
            return base.ToString() + "; msg: " + m_data.Msg;
        }

    }


}


