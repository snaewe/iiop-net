/* AttributeExtCollection.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 14.01.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.ComponentModel;
using System.Collections;
using Ch.Elca.Iiop.Idl;

namespace Ch.Elca.Iiop.Util {

    /// <summary>
    /// A more powerful Attribute collection than AttributeCollection.
    /// </summary>
    public class AttributeExtCollection : ICollection, ICloneable {
        
        #region SFields
        
        private static Attribute[] s_emptyAttrArray = new Attribute[0];
        
        #endregion
        #region IFields
        
        private ArrayList m_attributes = new ArrayList();
        
        #endregion IFields
        #region IConstructors

        public AttributeExtCollection() : this(s_emptyAttrArray) {
        }
        
        public AttributeExtCollection(Attribute[] attrs) {
            m_attributes = new ArrayList(attrs);
        }

        #endregion IConstructors
        #region IProperties
        
        public bool IsSynchronized {
            get { 
                return false; 
            }
        }

        public int Count {
            get {
                return m_attributes.Count;
            }
        }

        public object SyncRoot {
            get { 
                return m_attributes.SyncRoot; 
            }
        }
        #endregion IProperties
        #region SMethods
        
        /// <summary>
        /// creates an AttibuteExtCollection containing all Attributes in attrs
        /// </summary>
        public static AttributeExtCollection ConvertToAttributeCollection(object[] attrs) {
            Attribute[] result = s_emptyAttrArray;
            if (attrs != null) {
                result = new Attribute[attrs.Length];
                for (int i = 0; i < attrs.Length; i++) {
                    result[i] = (Attribute) attrs[i];
                }
            }
            return new AttributeExtCollection(result);
        }

        #endregion
        #region IMethods
    
        public bool Contains(Attribute attr) {
            return m_attributes.Contains(attr);
        }

        /// <summary>
        /// check, if an attribute of the given type is in the collection
        /// </summary>
        public bool IsInCollection(Type attrType) {
            IEnumerator enumerator = GetEnumerator();
            while (enumerator.MoveNext()) {
                Attribute attr = (Attribute)enumerator.Current;
                if (attr.GetType() == attrType) { return true; }
            }
            return false;
        }

        /// <summary>
        /// returns the first attribute in the collection, which is of the specified type
        /// </summary>
        public Attribute GetAttributeForType(Type attrType) {
            IEnumerator enumerator = GetEnumerator();
            while (enumerator.MoveNext()) {
                Attribute attr = (Attribute)enumerator.Current;
                if (attr.GetType() == attrType) { 
                    return attr; 
                }
            }
            return null;
        }

        /// <summary>
        /// removes the first attribute of the given type
        /// </summary>
        public void RemoveAttributeOfType(Type attrType) {
            IEnumerator enumerator = GetEnumerator();
            Attribute foundAttr = null;
            while (enumerator.MoveNext()) {
                Attribute attr = (Attribute)enumerator.Current;
                if (attr.GetType() == attrType) { 
                    foundAttr = attr;
                    break;
                }
            }
            if (foundAttr != null) {
                m_attributes.Remove(foundAttr);
            }
        }

        /// <summary>
        /// insert the attribute in the collection at the first position
        /// </summary>
        public void InsertAttribute(Attribute attr) {
            m_attributes.Insert(0, attr);
        }
        
        /// <summary>
        /// inserts all attributes from coll into the collection
        public void InsertAttributes(AttributeExtCollection coll) {
            IEnumerator enumerator = coll.GetEnumerator();
            while (enumerator.MoveNext()) {
                Attribute attr = (Attribute) enumerator.Current;
                InsertAttribute(attr);
            }
        }

        public void AddMissingAttributes(object[] toAdd) {
            foreach (Attribute attr in toAdd) {
                if (!Contains(attr)) {
                    InsertAttribute(attr);
                }
            }
        }

        public override bool Equals(object obj) {
            if (obj == null) { return false; }
            if (!(obj.GetType().Equals(typeof(AttributeExtCollection)))) { return false; }

            IEnumerator enumerator = GetEnumerator();
            while (enumerator.MoveNext()) {
                Attribute attr = (Attribute) enumerator.Current;
                if (!((AttributeExtCollection)obj).Contains(attr)) { return false; }
            }

            enumerator = ((AttributeExtCollection)obj).GetEnumerator();
            while (enumerator.MoveNext()) {
                Attribute attr = (Attribute) enumerator.Current;
                if (!(m_attributes.Contains(attr))) { return false; }
            }
            return true;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        #region Implementation of ICollection

        public void CopyTo(System.Array array, int index) {
            m_attributes.CopyTo(array, index);
        }

        #endregion Implementation of ICollection
        #region Implementation of IEnumerable

        public System.Collections.IEnumerator GetEnumerator() {
            return m_attributes.GetEnumerator();
        }

        #endregion Implementation of IEnumerable
        
        /// <summary>creates a copy of AttributeExtCollection; 
        /// does not create a copy of the Attributes</summary>
        public virtual object Clone() {
            AttributeExtCollection copy = new AttributeExtCollection();
            copy.m_attributes = (ArrayList)m_attributes.Clone();
            return copy;
        }

        #endregion IMethods
        
    }
}
