/* AddClient.java
 * 
 * Project: IIOP.NET
 * IIOPChannel Tutorial
 * 
 * WHEN      RESPONSIBLE
 * 23.04.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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

import javax.naming.NamingException;
import javax.naming.InitialContext;
import javax.naming.Context;
import javax.rmi.PortableRemoteObject;
import java.io.BufferedReader;
import java.io.InputStreamReader;
import Ch.Elca.Iiop.Tutorial.GettingStarted.AdderImpl;

public class AddClient {

    public static void main(String[] args) {

        try {
            BufferedReader reader = new BufferedReader(new InputStreamReader(System.in));
            System.out.println("input the two summands");
            System.out.println("summand 1: ");
            double sum1 = Double.parseDouble(reader.readLine());
            System.out.println("summand 2: ");
            double sum2 = Double.parseDouble(reader.readLine());

            System.out.println("get inital naming context");
            Context ic = new InitialContext();
            System.out.println("ic received, retrieve add");
            Object objRef = ic.lookup("adder");
            AdderImpl adder = (AdderImpl) PortableRemoteObject.narrow(objRef, AdderImpl.class);
            System.out.println("call add method");
            double result = adder.add(sum1, sum2);
            System.out.println("result: " + result);
        } catch (Exception e) {
            System.out.println("exception encountered: " + e);
        }
    }

}
