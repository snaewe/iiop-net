using System;
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Idl;
using Ch.Elca.Iiop.Services;

namespace tutorial {
    
    using java.lang;

    public class CustomExceptionImpl: CustomException {

        #region IFields

        private ExceptionCommon m_data = new ExceptionCommon();

        #endregion IFields

        public CustomExceptionImpl() : base() {
        }
            
        public override void Deserialise(Corba.DataInputStream stream) {
            m_data.Deserialise(stream);
            reason = stream.read_WStringValue();
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
/// used to Deserialise the java.lang.Exception data
/// </summary>
public class ExceptionCommon {

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
        stream.write_octet(1);
        stream.write_boolean(true);

        stream.write_ValueOfActualType(m_cause);
        stream.write_WStringValue(m_msg);

        stream.write_boxed(m_trace, new BoxedValueAttribute("RMI:[Ljava.lang.StackTraceElement;:CD38F9930EA8AAEC:6109C59A2636DD85"));
    }

}

    
public class StackTraceElementImpl : StackTraceElement {

    public override int hashCode() {
        return GetHashCode();
    }            
        
    public override string toString() {
        return "StackTraceElement:\n" + "in class: " + m_declaringClass + 
            " (file: " + m_fileName + "); method: " + m_methodName;
    }

    public override bool equals([ObjectIdlTypeAttribute(IdlTypeObject.Any)] object arg) {
        return this.Equals(arg);
    }

    public override string className {
        get { return m_declaringClass; }
    }

    public override string fileName {
        get { return m_fileName; }
    }

    public override int lineNumber {
        get { return m_lineNumber; }
    }

    public override string methodName {
        get { return m_methodName; }
    }

    public override bool nativeMethod {
        get { return false; }
    }
        
}

[Serializable]
public class ThrowableImpl : Throwable {

    #region IFields

    private ExceptionCommon m_data = new ExceptionCommon();

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


[Serializable]
public class _ExceptionImpl : _Exception {

    #region IFields

    private ExceptionCommon m_data = new ExceptionCommon();

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


}
