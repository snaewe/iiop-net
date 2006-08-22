/* 
 * ChatroomServer.java
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

import java.util.Hashtable;

/**
 * A singleton, managing the chatroom clients.
 * Remark: This is not a clean solution, but it choosen for simplicity.
 * It doesn't scale as normally required by EJB solutions.
 */
public class ChatroomServer {


    private static ChatroomServer s_chatroomServer = new ChatroomServer();

    private Hashtable m_clients = new Hashtable();

    private ChatroomServer() {
        super();
    }
    
    public static ChatroomServer getSingleton() {
    	return s_chatroomServer;
    }
    
    /**
     * Add a new messagelistener. 
     * @throws AlreadyRegisteredException if a message listener for the given user name is
     * already registered.
     **/
    public synchronized void addClient(MessageListener ml, String forUser) 
                                    throws AlreadyRegisteredException {				
    	if (!m_clients.containsKey(forUser)) {
    	    m_clients.put(forUser, ml);
    	} else {
    		throw new AlreadyRegisteredException("a message listener is already registered for user: " + 
    		                                      forUser);
    	}        
    }
    
    public synchronized void removeClient(String forUser) throws NotRegisteredException {
    	if (m_clients.containsKey(forUser)) {
    		m_clients.remove(forUser);
    	} else {
    		throw new NotRegisteredException("no message listener registered for the user: " + 
    		                                  forUser);
    	}
    }
    
    /**
     * removes the specified listener 
     **/
    public synchronized void removeListener(MessageListener listener) {
    	m_clients.values().remove(listener);
    }
    
    /** 
     * returns an array of message listeners registered at the moment. 
     **/
    public synchronized MessageListener[] getClients() {
        MessageListener[] result = new MessageListener[m_clients.size()];
        result = (MessageListener[])(m_clients.values().toArray(result));
        return result;
    }
           

}
