/* 
 * ChatroomServer.cs
 *
 * Project: IIOP.NET
 * Examples
 *
 * WHEN      RESPONSIBLE
 * 09.09.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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

namespace Ch.Elca.Iiop.Demo.Chatroom {



    /// <summary>
    /// A singleton, managing the chatroom clients.
    /// </summary>
    public class ChatroomServer {


        private static ChatroomServer s_chatroomServer = new ChatroomServer();

        private Hashtable m_clients = new Hashtable();

        private ChatroomServer() {
        }
    
        public static ChatroomServer GetSingleton() {
        	return s_chatroomServer;
        }
    
        /// <summary>
        /// Add a new messagelistener. 
        /// </summary>
        public void AddClient(MessageListener ml, String forUser) {
            lock(this) {
    	        if (!m_clients.ContainsKey(forUser)) {
    	            m_clients.Add(forUser, ml);
        	    } else {
        		    throw new AlreadyRegisteredException("a message listener is already registered for user: " + 
            	                                          forUser);
        	    }
        	}
        }
    
        public void RemoveClient(String forUser) {
        	lock(this) {
    	        if (m_clients.ContainsKey(forUser)) {
    		        m_clients.Remove(forUser);
            	} else {
            		throw new NotRegisteredException("no message listener registered for the user: " + 
    	        	                                  forUser);
        	    }
            }
        }
    
    
        /// <summary>
        /// returns an array of message listeners registered at the moment. 
        /// </summary>
        public MessageListener[] GetClients() {
        	lock(this) {
                MessageListener[] result = new MessageListener[m_clients.Count];
                m_clients.Values.CopyTo(result, 0);
                return result;
            }
        }
          

    }

}
