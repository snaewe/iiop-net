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
using System.Diagnostics;
using Ch.Elca.Iiop.Idl;

namespace Ch.Elca.Iiop.Util {

    /// <summary>
    /// A more powerful Attribute collection than AttributeCollection.
    /// </summary>
    public class AttributeExtCollection : ICollection {
        
        #region SFields
        
        private static AttributeExtCollection s_emptyCollection = new AttributeExtCollection();
        
        #endregion
        #region IFields
        
        private ArrayList m_attributes;
        
        #endregion IFields
        #region IConstructors

        public AttributeExtCollection() {
            m_attributes = new ArrayList();    
        }
        
        public AttributeExtCollection(Attribute[] attrs) {
            m_attributes = new ArrayList();
            m_attributes.AddRange(attrs);
        }

        public AttributeExtCollection(AttributeExtCollection coll) {
            m_attributes = (ArrayList)coll.m_attributes.Clone();
        }
        
        private AttributeExtCollection(ArrayList content) {
            m_attributes = content;
        }

        #endregion IConstructors
        #region SProperties
        
        public static AttributeExtCollection EmptyCollection {
            get {
                return s_emptyCollection;
            }
        }
        
        #endregion SProperties
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

        public object this[int index] {
            get {
                return m_attributes[index];
            }
        }        

        #endregion IProperties
        #region SMethods
        
        /// <summary>
        /// creates an AttibuteExtCollection containing all Attributes in attrs
        /// </summary>
        public static AttributeExtCollection ConvertToAttributeCollection(object[] attrs) {
            if ((attrs != null) && (attrs.Length > 0)) {
                ArrayList resultList = new ArrayList();
                resultList.AddRange(attrs);
                return new AttributeExtCollection(resultList);
            } else {
                return EmptyCollection;
            }
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
                if (attr.GetType() == attrType) { 
                    return true; 
                }
            }
            return false;
        }

        /// <summary>
        /// returns the first attribute in the collection, which is of the specified type
        /// </summary>
        /// <remarks>
        /// for attributes implementing IOrderedAttribute, this returns the attribute
        /// with the highest order number
        /// </remarks>
        public Attribute GetAttributeForType(Type attrType) {
            Attribute result = null;
            bool isOrdered = false;
            if (ReflectionHelper.IOrderedAttributeType.IsAssignableFrom(attrType)) {
                isOrdered = true;
            }
            IEnumerator enumerator = GetEnumerator();
            while (enumerator.MoveNext()) {
                Attribute attr = (Attribute)enumerator.Current;
                if (attr.GetType() == attrType) { 
                    if (!isOrdered) {
                        result = attr;
                        break;
                    } else {
                        if ((result == null) ||
                           (((IOrderedAttribute)result).OrderNr <
                            ((IOrderedAttribute)attr).OrderNr)) {
                            result = attr;        
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Get highest order ordered attribute
        /// </summary>
        /// <remarks>
        /// this returns the attribute with the highest order number or null if not available
        /// </remarks>
        public Attribute GetHighestOrderAttribute() {
            Attribute result = null;
            IEnumerator enumerator = GetEnumerator();
            while (enumerator.MoveNext()) {
                Attribute attr = (Attribute)enumerator.Current;
                if ((attr is IOrderedAttribute) && 
                    ((result == null) ||
                    (((IOrderedAttribute)result).OrderNr <
                     ((IOrderedAttribute)attr).OrderNr))) {
                    result = attr;        
                }
            }
            return result;
        }


        /// <summary>
        /// removes the first attribute of the given type
        /// </summary>
        /// <remarks>
        /// for attributes implementing IOrderedAttribute, this removes the attribute
        /// with the highest order number
        /// </remarks>
        /// <returns>The removed attribute, or null if not found</returns>
        public AttributeExtCollection RemoveAttributeOfType(Type attrType, out Attribute foundAttr) {
            foundAttr = GetAttributeForType(attrType);
            if (foundAttr != null) {
                return RemoveAttribute(foundAttr);
            } else {
                return this;
            }
        }

        /// <summary>
        /// removes the first attribute of the given type
        /// </summary>
        /// <remarks>
        /// for attributes implementing IOrderedAttribute, this removes the attribute
        /// with the highest order number
        /// </remarks>
        /// <returns>The removed attribute, or null if not found</returns>
        public AttributeExtCollection RemoveAssociatedAttributes(long associatedTo, out IList removedAttributes) {
            removedAttributes = new ArrayList();
            ArrayList newCollection = (ArrayList)m_attributes.Clone();
            for (int i = 0; i < m_attributes.Count; i++) {
                if ((m_attributes[i] is IAssociatedAttribute) &&
                    (((IAssociatedAttribute)m_attributes[i]).AssociatedToAttributeWithKey == associatedTo)) {
                    removedAttributes.Add(m_attributes[i]); 
                    newCollection.Remove(m_attributes[i]);
                }
            }
            return new AttributeExtCollection(newCollection);
        }


        /// <summary>returns a new collection without the Attribute attribute</summary
        public AttributeExtCollection RemoveAttribute(Attribute attribute) {
            ArrayList newCollection = (ArrayList)m_attributes.Clone();
            newCollection.Remove(attribute);
            return new AttributeExtCollection(newCollection);
        }

        /// <summary>
        /// insert the attribute in the collection at the first position
        /// </summary>
        public AttributeExtCollection MergeAttribute(Attribute attr) {
            ArrayList newAttributes = (ArrayList)m_attributes.Clone();            
            newAttributes.Insert(0, attr);
            return new AttributeExtCollection(newAttributes);
        }
        
        /// <summary>
        /// returns an attribute collection produced by merging this collection and the argument collection.
        /// </summary>
        public AttributeExtCollection MergeAttributeCollections(AttributeExtCollection coll) {
            // first the new ones
            ArrayList resultList = (ArrayList)coll.m_attributes.Clone();
            // append content of this collection
            resultList.AddRange(m_attributes);
            return new AttributeExtCollection(resultList);
        }

        public AttributeExtCollection MergeMissingAttributes(object[] toAdd) {
            ArrayList resultList = (ArrayList)m_attributes.Clone();                        
            foreach (Attribute attr in toAdd) {
                if (!resultList.Contains(attr)) {
                    resultList.Insert(0, attr);
                }
            }
            return new AttributeExtCollection(resultList);
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
            int result = 0;
            for (int i = 0; i < m_attributes.Count; i++) {
                result = result ^ m_attributes[i].GetHashCode();
            }
            return result;
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
        
        #endregion IMethods
        
    }
}
