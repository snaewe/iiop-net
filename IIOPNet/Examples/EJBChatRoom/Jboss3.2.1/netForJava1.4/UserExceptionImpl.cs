/* UserExceptionImpl.cs
 *
 * Project: IIOP.NET
 * Examples
 *
 * WHEN      RESPONSIBLE
 * 23.07.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
    public class NotRegisteredExceptionImpl : NotRegisteredException {

        #region IFields

	private ThrowableData m_data = new ThrowableData();

        #endregion IFields

        public NotRegisteredExceptionImpl() : base() {
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
