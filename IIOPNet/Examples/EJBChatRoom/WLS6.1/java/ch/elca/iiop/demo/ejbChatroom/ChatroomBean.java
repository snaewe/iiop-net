/* ChatroomBean.java
 *
 * Project: IIOP.NET
 * Examples
 *
 * WHEN      RESPONSIBLE
 * 21.07.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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


package ch.elca.iiop.demo.ejbChatroom;

import javax.ejb.CreateException;
import javax.ejb.EJBException;
import javax.ejb.SessionBean;
import javax.ejb.SessionContext;
import javax.naming.InitialContext;


/**
 * AdderBean is a stateless Session Bean. It implements the 
 * chatroom functionality.
 */
public class ChatroomBean implements SessionBean {

    private SessionContext m_ctx;

    /**
     * This method is required by the EJB Specification,
     * but is not used here.
     *
     */
    public void ejbActivate() {
    }

    /**
     * This method is required by the EJB Specification,
     * but is not used here.
     *
     */
    public void ejbRemove() {
    }

    /**
     * This method is required by the EJB Specification,
     * but is not used by this example.
     *
     */
    public void ejbPassivate() {
    }

    /**
     * Sets the session context.
     *
     * @param ctx               SessionContext Context for session
     */
    public void setSessionContext(SessionContext ctx) {
        m_ctx = ctx;
    }

    /**
     * This method corresponds to the create method in the home interface
     * "AdderHome.java".
     *
     */
    public void ejbCreate () throws CreateException {
    }


    public void broadCast(Message msg) {
        ChatroomServer server = ChatroomServer.getSingleton();
        MessageListener[] listeners = server.getClients();
        for (int i = 0; i < listeners.length; i++) {
			try {
	            listeners[i].notifyMessage(msg);
			} catch (Exception e) {
				System.err.println("error sending msg: " + e);
				System.err.println("--> removing listener");
				server.removeListener(listeners[i]);
			}
        }

    }
    
    public void registerMe(MessageListener listener, String forUser) 
                     throws AlreadyRegisteredException {
		ChatroomServer server = ChatroomServer.getSingleton();
		server.addClient(listener, forUser);
    }    
    
    public void unregisterMe(String userName) throws NotRegisteredException {
    	ChatroomServer server = ChatroomServer.getSingleton();
    	server.removeClient(userName);    
    }
    

}

