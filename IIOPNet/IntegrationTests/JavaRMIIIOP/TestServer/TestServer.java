/* TestServer.java
 *
 * Project: IIOP.NET
 * IntegrationTests
 *
 * WHEN      RESPONSIBLE
 * 14.07.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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


import Ch.Elca.Iiop.IntegrationTests.TestService;
import Ch.Elca.Iiop.IntegrationTests.TestServiceImpl;

import javax.naming.InitialContext;
import javax.naming.Context;


public class TestServer {

    public static void main(String[] args) {
        try {

            // Instantiate the service
            TestService test = new TestServiceImpl();

            // publish the reference with the naming service:
            Context initialNamingContext = new InitialContext();
            initialNamingContext.rebind("test", test);

            System.out.println("Server Ready...");

        } catch (Exception e) {
            System.out.println("Trouble: " + e); e.printStackTrace();
        }

    }

}