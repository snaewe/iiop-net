/* Client.java
 * 
 * Project: IIOP.NET
 * Examples
 * 
 * WHEN      RESPONSIBLE
 * 25.05.03  Patrik Reali (PRR), patrik.reali -at- elca.ch
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


import java.io.IOException;
import java.io.InputStreamReader;
import java.io.LineNumberReader;
import java.util.Vector;

import javax.naming.InitialContext;
import javax.rmi.PortableRemoteObject;

import Ch.Elca.Iiop.GenericUserException;
import Ch.Elca.Iiop.Demo.StorageSystem.*;

public class Client {
	
	LineNumberReader lnr = new LineNumberReader(new InputStreamReader(System.in));
	Manager m;

	Client(Manager m) {
		this.m = m;
	}
	
	int readInt() {
		try {
			return Integer.parseInt(lnr.readLine());
		} catch (Exception e) {
			return -1;
		}
	}
	
	char getChar(char from, char to) throws java.io.IOException {
		char ch;

		do {
			ch = (char)System.in.read();
		} while ((ch < from) || (ch > to));
		
		return ch;
	}
	
	void setEntry(Container c) throws GenericUserException {

		System.out.println("Enter an key / value pair:");
		System.out.print("  key: ");
		try {
			String key   = lnr.readLine();
			System.out.print("value: ");
			String value = lnr.readLine();
			c.SetValue(key, value);
		} catch (IOException e) {
			System.out.println("Error while reading input");
		}
		
	}
	
	void dumpContainer(Container c) throws GenericUserException {
		System.out.println("List Entries");						
		Entry en[];
			en = c.Enumerate();
			for (int i = 0; i < en.length; i++) {
				System.out.println("  Entry[" + en[i].key + "] = " + en[i].value);
			}
	}
	
	void manageContainer(Container c) {
		boolean quit = false;
		try {
			do {
				System.out.println();
				System.out.println("Container Menu:");
				System.out.println("0. Return to previous menu");
				System.out.println("1. Set Entry");
				System.out.println("2. Show Entries");
// bug: server-side ObjRef deserialization
// not supported yet
//				System.out.println("3. Delete Container");
				switch (readInt()) {
					case 0:
						quit = true;
						break;
					case 1:
						setEntry(c);
						break;
					case 2:
						dumpContainer(c);
						break;
// bug: server-side ObjRef deserialization
// not supported yet
//					case 3:
//						m.DeleteContainer(c);
//						quit = true;
//						break;
				}
			} while (!quit);
		} catch (GenericUserException e) {
                    System.out.println("Exception " + e.name);
                    System.out.println("  message: " + e.message);
		}
		
	}

	void selectContainer(Manager m) {
		System.out.println();
		System.out.println("Select Containers: enter a list of key / values pairs; terminate with an empty key");
		Vector keys = new Vector();
		Vector values = new Vector();
		
		try {
			do {
				System.out.print("  key: ");
				String key   = lnr.readLine();
				if (key.equals("")) break;
				System.out.print("value: ");
				String value = lnr.readLine();
				
				keys.add(key);
				values.add(value);
			} while (true);
			Entry list[] = new Entry[keys.size()];
			for (int i=0; i < keys.size(); i++) {
				list[i] = new EntryImpl();
				list[i].key = (String)keys.elementAt(i);
				list[i].value = (String)values.elementAt(i);
			}
			
			System.out.println();
			System.out.println("Matches:");	
			Container clist[] = m.FilterContainers(list);
			for (int i=0; i < clist.length; i++) {
				System.out.println();
				System.out.println("Container " + (i+1));
				dumpContainer(clist[i]);
			}
			int i = 0;
			do {
				System.out.println("Select container number or 0 to return to previous menu");
				i = readInt();
				if ((i > 0) && (i <= clist.length)) {
					manageContainer(clist[i-1]);
				}
			} while (i != 0);
			
		} catch (Exception e) {
			System.out.println("Exception: " + e.getMessage());
		}
	}
	
	void topMenu() {
		boolean quit = false;
		try {
			do {
				System.out.println();
				System.out.println("Main Menu:");
				System.out.println("0. Terminate");
				System.out.println("1. Create Container");
				System.out.println("2. Select Container");
				switch (readInt()) {
					case 0:
						quit = true;
						break;
					case 1:
						manageContainer(m.CreateContainer());
						break;
					case 2:
						selectContainer(m);
						break;
					default:
						System.out.println("Invalid entry");
						break;
				}
			} while (!quit);
		} catch (Exception e) {
			System.out.println("Exception: " + e.getMessage());
		}
	}

	public static void main(String args[]) {
		Manager m = null;
		try {
			InitialContext ic = new InitialContext();
			Object obj = ic.lookup("storagemanager");
			m = (Manager) PortableRemoteObject.narrow(obj, Manager.class);
			Client c = new Client(m);
			
			c.topMenu();
		} catch (Exception e) {
			System.out.println("Exception: " + e.getMessage());
		}
	}
}
