/* Service.cs
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


using System;
using System.Collections;
using System.Runtime.Remoting;

namespace Ch.Elca.Iiop.Demo.StorageSystem {

	[Serializable]
	public struct Entry {
		public string key;
		public string value;
	}

	public class Container: MarshalByRefObject {
		private Hashtable  m_entries;


		public Container() {
			m_entries = new Hashtable();
		}
		
		public Entry[] Enumerate() {
			Entry[] result = new Entry[m_entries.Count];
			IEnumerator e = m_entries.GetEnumerator();
			int i = 0;

			while (e.MoveNext()) {
				DictionaryEntry entry = (DictionaryEntry)e.Current;
				result[i].key = (String)entry.Key;
				result[i++].value = (String)entry.Value;
			}
			return result;
		}

		public void    SetValue(string key, string value) {
			m_entries[key] = value;
		}
		
		public void    SetEntry(Entry e) {
			SetValue(e.key, e.value);
		}
		
		public String   GetValue(string key) {
			return m_entries[key] as String;
		}
	}

	public class Manager: MarshalByRefObject {

		ArrayList m_containers;

		public Manager() {
			m_containers = new ArrayList();
		}

		public Container   CreateContainer() {
			Container c = new Container();
			m_containers.Add(c);
			return c;
		}
		
		public Container[] FilterContainers(Entry[] filter) {
			ArrayList matches;
			if ((filter == null) || (filter.Length == 0)) {
				matches = m_containers;
			} else {
				matches = new ArrayList();
				foreach (Container c in m_containers) {
					int i = 0;
					bool match = true;
					do {
						String value = c.GetValue(filter[i].key);
						match = (value != null) && (value == filter[i].value);
					} while ((++i < filter.Length) && match);
					if (match) {
						matches.Add(c);
					}
				}
			}
			Container[] res = new Container[matches.Count];
			matches.CopyTo(res);
			return res;
		}
		
// bug: server-side ObjRef deserialization
// not supported yet
//		public void DeleteContainer(Container c) {
//			m_containers.Remove(c);
//		}

                public override object InitializeLifetimeService() {
                    return null; // live forever
                }
	}


}
