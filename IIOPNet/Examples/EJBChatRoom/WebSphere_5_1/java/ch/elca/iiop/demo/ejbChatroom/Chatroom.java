/* Chatroom.java
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

import javax.ejb.*;
import java.rmi.RemoteException;

/**
 * The methods in this interface are the public face of the Adder.
 */
public interface Chatroom extends EJBObject {

    /**
     * post message in chat-room
     **/
    public void broadCast(Message msg) throws RemoteException;
    
    /**
     * register a client, interested in chatroom messages 
     **/
    public void registerMe(MessageListener listener, String forUser) 
    			            throws RemoteException, AlreadyRegisteredException;
                                         
    /**
     * unregisters the clien with the name userName.
     **/
    public void unregisterMe(String userName) 
                              throws RemoteException, NotRegisteredException;

}
