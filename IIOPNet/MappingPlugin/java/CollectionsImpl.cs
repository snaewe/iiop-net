/* CollectionsImpl.cs
 * 
 * Project: IIOP.NET
 * Mapping-Plugin
 * 
 * WHEN      RESPONSIBLE
 * 17.08.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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

using System;
using Ch.Elca.Iiop.Idl;

namespace java.util {

    [Serializable]    
    public class ArrayListImpl : ArrayList {

        /// <summary>the elements in the list</summary>
        /// <remarks>m_elements must never be null</remarks>
        private object[] m_elements;

        private int m_backArraySize;
    
        public ArrayListImpl() {
        	m_elements = new object[0];
        	m_backArraySize = 0;
        }    

        #region methods needed by mapper
        
        public override void Deserialise(Corba.DataInputStream arg) {
            // skip rmi data
            arg.read_octet();
            arg.read_octet();
            // size
            int size = arg.read_long();
            // size of backing array
            m_backArraySize = arg.read_long();
            // elements
            m_elements = new object[size];
            for (int i = 0; i < size; i++) {
                // something like a bool or octet:
                bool isByRef = arg.read_boolean();
                if (!isByRef) {
                    m_elements[i] = arg.read_Value();
                    if (m_elements[i] is BoxedValueBase) {
                        m_elements[i] = ((BoxedValueBase) m_elements[i]).Unbox();
                    }
                } else {
                    m_elements[i] = arg.read_Object();
                }
            }
            
        }
        
        public override void Serialize(Corba.DataOutputStream arg) {
            // rmi data
            arg.write_octet(1);
            arg.write_octet(1);
            // size: is equal to size of m_elements
            int size = m_elements.Length;
            arg.write_long(size);
            if (m_backArraySize < size) {
                m_backArraySize = size;
            }
            arg.write_long(m_backArraySize);

            for (int i = 0; i < size; i++) {
                bool isByRef = false;
                if (m_elements[i] != null) {
                    isByRef = ClsToIdlMapper.IsMarshalByRef(m_elements[i].GetType());
                }
                arg.write_boolean(isByRef);
                if (!isByRef) {
                    arg.write_ValueOfActualType(m_elements[i]);
                } else {
                    arg.write_Object((MarshalByRefObject)m_elements[i]);
                }
            }
            
        }

        public int Size {
        	get {
        	    return m_elements.Length;
            }
        }

        public int Capacity {
            get {
                return m_backArraySize;
            }
            set {
                m_backArraySize = value;
                if (m_backArraySize < m_elements.Length) {
                    m_backArraySize = m_elements.Length;
                }
            }
        }
        
        public object[] GetElements() {
            return m_elements;
        }
        
        /// <summary>sets the contents of the list</summary>
        public void SetElements(object[] arg) {
            if (arg != null) {
                m_elements = arg;
            } else {
                m_elements = new object[0];
            }
        }
        
        #endregion methods needed by mapper
        #region not needed methods (for mapper)
        // the following methods are not used -> no useful implementation

/*        public override void removeRange(int arg0, int arg1) {
        }

        public override List subList(int arg0, int arg1) {
            return null;
        }

        public override ListIterator listIterator__long(int arg0) {
            return null;
        }

        public override ListIterator listIterator__() {
            return null;
        }

        public override int lastIndexOf(object arg) {
            return 0;
        }

        public override int indexOf(object arg) {
            return 0;
        }

        public override object remove__long(int arg) {
            return null;
        }

        public override void add__long__java_lang_Object(int arg0, object arg1) {
        }

        public override object _set(int arg0, object arg1) {
            return null;
        }

        public override object _get(int arg0) {
            return null;
        }

        public override bool addAll__long__java_util_Collection(int arg0, java.util.Collection arg1) {
            return false;
        }

        public override void trimToSize() {
        }

        public override void ensureCapacity(int arg) {
        }
        
        public override object clone() {
            return null;
        }

        public override string toString() {
            return "";
        }

        public override int hashCode() {
            return 0;
        }

        public override bool equals(object arg) {
            return (this == arg);
        }
        
        public override void clear() {
        }

        public override bool retainAll(java.util.Collection arg) {
            return false;
        }
        
        public override bool removeAll(java.util.Collection arg) {
            return false;
        }

        public override bool addAll(java.util.Collection arg) {
            return false;
        }

        public override bool containsAll(java.util.Collection arg) {
            return false;
        }

        public override bool remove(object arg) {
            return false;
        }

        public override bool add(object arg) {
            return false;
        }

        public override object[] toArray__org_omg_boxedRMI_java_lang_seq1_Object(object[] arg) {
            return arg;
        }
        
        public override object[] toArray__() {
            return m_elements;
        }

        public override Iterator iterator() {
            return null;
        }

        public override bool contains(object arg) {
            return false;
        }
        
        public override bool isEmpty() {
            return true;
        }
        
        public override int size() {
            return m_elements.Length;
        }
*/
        
        #endregion not needed methods (for Mapper)

    }

}