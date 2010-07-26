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
        
        public static readonly Type ClassType = typeof(AttributeExtCollection);
        
        #endregion
        #region IFields
        
        private object[] m_attributes;
        private int hashCode = 0;
        
        #endregion IFields
        #region IConstructors

        public AttributeExtCollection() {
            m_attributes = new object[0];
        }
        
        public AttributeExtCollection(Attribute[] attrs) {
            m_attributes = new object[attrs.Length];
            attrs.CopyTo(m_attributes, 0);
        }

        public AttributeExtCollection(AttributeExtCollection coll) {
            hashCode = coll.hashCode;
            m_attributes = (object[])coll.m_attributes.Clone();
        }
        
        private AttributeExtCollection(object[] content) {
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
                return m_attributes.Length;
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
                return new AttributeExtCollection(attrs);
            } else {
                return EmptyCollection;
            }
        }

        #endregion
        #region IMethods
    
        private bool Contains(object attr) {
            for (int i = 0; i < m_attributes.Length; i++) {
                if (m_attributes[i].Equals(attr)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// check, if an attribute of the given type is in the collection
        /// </summary>
        public bool IsInCollection(Type attrType) {
            for (int i = 0; i < m_attributes.Length; i++) {
                Attribute attr = (Attribute)m_attributes[i];
                if (attr.GetType() == attrType) { 
                    return true; 
                }
            }
            return false;
        }
        
        private Attribute GetAttributeForTypeInternal(Type attrType, out int position) {
            Attribute result = null;
            position = -1;
            bool isOrdered = false;
            if (ReflectionHelper.IOrderedAttributeType.IsAssignableFrom(attrType)) {
                isOrdered = true;
            }
            for (int i = 0; i < m_attributes.Length; i++) {
                Attribute attr = (Attribute)m_attributes[i];
                if (attr.GetType() == attrType) { 
                    if (!isOrdered) {
                        result = attr;
                        position = i;
                        break;
                    } else {
                        if ((result == null) ||
                           (((IOrderedAttribute)result).OrderNr <
                            ((IOrderedAttribute)attr).OrderNr)) {
                            result = attr;
                            position = i;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// returns the first attribute in the collection, which is of the specified type
        /// </summary>
        /// <remarks>
        /// for attributes implementing IOrderedAttribute, this returns the attribute
        /// with the highest order number
        /// </remarks>
        public Attribute GetAttributeForType(Type attrType) {
            int position;
            return GetAttributeForTypeInternal(attrType, out position);
        }

        /// <summary>
        /// Get highest order ordered attribute
        /// </summary>
        /// <remarks>
        /// this returns the attribute with the highest order number or null if not available
        /// </remarks>
        public Attribute GetHighestOrderAttribute() {
            Attribute result = null;
            for (int i = 0; i < m_attributes.Length; i++) {
                Attribute attr = (Attribute)m_attributes[i];
                if ((attr is IOrderedAttribute) && 
                    ((result == null) ||
                    (((IOrderedAttribute)result).OrderNr <
                     ((IOrderedAttribute)attr).OrderNr))) {
                    result = attr;
                }
            }
            return result;
        }


        private AttributeExtCollection RemoveAttributeAtPosition(int position) {
            object[] newCollection = new object[m_attributes.Length - 1]; // m_attributes.Length must be >= 0, because attr found
            // copy elements before position to newCollection; don't use Array.Copy, because only few elements
            for (int i = 0; i < position; i++) {
                newCollection[i] = m_attributes[i];
            }
            // copy elements after position to newCollection; don't use Array.Copy, because only few elements
            for (int i = position + 1; i < m_attributes.Length; i++) {
                newCollection[i-1] = m_attributes[i];
            }
            AttributeExtCollection result = new AttributeExtCollection(newCollection);
            if(hashCode != 0)
                result.hashCode = hashCode ^ m_attributes[position].GetHashCode();
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
            int position;
            foundAttr = GetAttributeForTypeInternal(attrType, out position);
            if (foundAttr != null) {
                return RemoveAttributeAtPosition(position);
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
            for (int i = 0; i < m_attributes.Length; i++) {
                if ((m_attributes[i] is IAssociatedAttribute) &&
                    (((IAssociatedAttribute)m_attributes[i]).AssociatedToAttributeWithKey == associatedTo)) {
                    removedAttributes.Add(m_attributes[i]); 
                }
            }
            object[] newAttributes = new object[m_attributes.Length - removedAttributes.Count];
            int newAttributesIndex = 0;
            for (int i = 0; i < m_attributes.Length; i++) {
                if (!((m_attributes[i] is IAssociatedAttribute) &&
                    (((IAssociatedAttribute)m_attributes[i]).AssociatedToAttributeWithKey == associatedTo))) {
                    newAttributes[newAttributesIndex] = m_attributes[i];
                    newAttributesIndex++;
                }
            }
            return new AttributeExtCollection(newAttributes);
        }

        /// <summary>returns a new collection without the Attribute attribute</summary
        public AttributeExtCollection RemoveAttribute(Attribute attribute) {
            int position = 0;
            bool found = false;
            for (int i = 0; i < m_attributes.Length; i++) {
                if (m_attributes[i] == attribute) {
                    position = i;
                    found = true;
                    break;
                }
            }
            if (found) {
                return RemoveAttributeAtPosition(position);
            } else {
                return this;
            }
        }

        /// <summary>
        /// insert the attribute in the collection at the first position
        /// </summary>
        public AttributeExtCollection MergeAttribute(Attribute attr) {
            object[] newAttributes = new object[m_attributes.Length + 1];
            newAttributes[0] = attr;
            m_attributes.CopyTo(newAttributes, 1);
            return new AttributeExtCollection(newAttributes);
        }
        
        /// <summary>
        /// returns an attribute collection produced by merging this collection and the argument collection.
        /// </summary>
        public AttributeExtCollection MergeAttributeCollections(AttributeExtCollection coll) {
            object[] resultList = new object[coll.m_attributes.Length + m_attributes.Length];
            // first the new ones
            coll.m_attributes.CopyTo(resultList, 0);
            // append content of this collection
            m_attributes.CopyTo(resultList, coll.m_attributes.Length);
            return new AttributeExtCollection(resultList);
        }

        public AttributeExtCollection MergeMissingAttributes(object[] toAdd) {
            ArrayList resultList = new ArrayList(m_attributes);
            foreach (Attribute attr in toAdd) {
                if (!resultList.Contains(attr)) {
                    resultList.Insert(0, attr);
                }
            }
            return new AttributeExtCollection(resultList.ToArray());
        }

        public override bool Equals(object obj) {
            AttributeExtCollection other = obj as AttributeExtCollection;
            if (other == null) {
                return false;
            }
            
            for (int i = 0; i < this.m_attributes.Length; i++) {
                if (!other.Contains(this.m_attributes[i])) {
                    return false; 
                }
            }
            for (int i = 0; i < other.m_attributes.Length; i++) {
                if (!this.Contains(other.m_attributes[i])) {
                    return false;
                }
            }
            
            return true;
        }

        public override int GetHashCode() {
            if(hashCode != 0)
                return hashCode;
            
            for (int i = 0; i < m_attributes.Length; i++) {
                hashCode ^= m_attributes[i].GetHashCode();
            }
            return hashCode;
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

        /// <summary>
        /// get attribute at position index
        /// </summary>
        public Attribute GetAttributeAt(int index) {
            return (Attribute)m_attributes[index];
        }

        #endregion IMethods
        
    }
}


#if UnitTest

namespace Ch.Elca.Iiop.Tests {
    
    using System.Reflection;
    using System.Collections;
    using System.IO;
    using NUnit.Framework;
    using Ch.Elca.Iiop;
    using Ch.Elca.Iiop.Util;
    
    internal class TestAttributeForColl : Attribute {
        
        private int m_val;

        internal TestAttributeForColl(int val) {
            m_val = val;
        }

        internal int Val {
            get {
                return m_val;
            }
        }
        
    }
    
    internal class TestAttributeForCollT1 : TestAttributeForColl {
    
        internal TestAttributeForCollT1(int val) : base(val) {
        }
        
    }
    
    internal class TestAttributeForCollT2 : TestAttributeForColl {
          
        internal TestAttributeForCollT2(int val) : base(val) {
        }
                
    }
    
    internal class TestAttributeForCollT3 : TestAttributeForColl {
          
        internal TestAttributeForCollT3(int val) : base(val) {
        }
                
    }

    internal class TestAttributeForCollT4 : TestAttributeForColl {
          
        internal TestAttributeForCollT4(int val) : base(val) {
        }
                
    }

    internal class TestAttributeForCollT5 : TestAttributeForColl {
          
        internal TestAttributeForCollT5(int val) : base(val) {
        }
                
    }
    


    /// <summary>
    /// Unit-tests for testing AttributeExtCollection
    /// </summary>
    [TestFixture]
    public class AttributeExtCollectionTest {
        
        [Test]
        public void TestRemoveElement() {
            TestAttributeForCollT1 a1 = new TestAttributeForCollT1(1);
            TestAttributeForCollT2 a2 = new TestAttributeForCollT2(2);
            TestAttributeForCollT3 a3 = new TestAttributeForCollT3(3);
            TestAttributeForCollT4 a4 = new TestAttributeForCollT4(4);
            TestAttributeForCollT5 a5 = new TestAttributeForCollT5(5);
            
            Attribute removed;
            
            AttributeExtCollection testColl1 = 
                AttributeExtCollection.ConvertToAttributeCollection(new object[] { a1, a2, a3, a4, a5 });
            
            AttributeExtCollection result1 = testColl1.RemoveAttributeOfType(typeof(TestAttributeForCollT1), 
                                                                             out removed);
            Assert.AreEqual(a1, removed, "wrong removed");
            Assert.AreEqual(a2, result1.GetAttributeAt(0), "wrong removed");
            Assert.AreEqual(a3, result1.GetAttributeAt(1), "wrong removed");
            Assert.AreEqual(a4, result1.GetAttributeAt(2), "wrong removed");
            Assert.AreEqual(a5, result1.GetAttributeAt(3), "wrong removed");
            Assert.AreEqual(4, result1.Count, "result length");

            result1 = testColl1.RemoveAttributeOfType(typeof(TestAttributeForCollT2), 
                                                      out removed);

            Assert.AreEqual(a2, removed, "wrong removed");
            Assert.AreEqual(a1, result1.GetAttributeAt(0), "wrong removed");
            Assert.AreEqual(a3, result1.GetAttributeAt(1), "wrong removed");
            Assert.AreEqual(a4, result1.GetAttributeAt(2), "wrong removed");
            Assert.AreEqual(a5, result1.GetAttributeAt(3), "wrong removed");
            Assert.AreEqual(4, result1.Count, "result length");

            result1 = testColl1.RemoveAttributeOfType(typeof(TestAttributeForCollT3), 
                                                      out removed);

            Assert.AreEqual(a3, removed, "wrong removed");
            Assert.AreEqual(a1, result1.GetAttributeAt(0), "wrong removed");
            Assert.AreEqual(a2, result1.GetAttributeAt(1), "wrong removed");
            Assert.AreEqual(a4, result1.GetAttributeAt(2), "wrong removed");
            Assert.AreEqual(a5, result1.GetAttributeAt(3), "wrong removed");
            Assert.AreEqual(4, result1.Count, "result length");
            
            
            result1 = testColl1.RemoveAttributeOfType(typeof(TestAttributeForCollT4), 
                                                      out removed);

            Assert.AreEqual(a4, removed, "wrong removed");
            Assert.AreEqual(a1, result1.GetAttributeAt(0), "wrong removed");
            Assert.AreEqual(a2, result1.GetAttributeAt(1), "wrong removed");
            Assert.AreEqual(a3, result1.GetAttributeAt(2), "wrong removed");
            Assert.AreEqual(a5, result1.GetAttributeAt(3), "wrong removed");
            Assert.AreEqual(4, result1.Count, "result length");
            
            
            result1 = testColl1.RemoveAttributeOfType(typeof(TestAttributeForCollT5), 
                                                      out removed);

            Assert.AreEqual(a5, removed, "wrong removed");
            Assert.AreEqual(a1, result1.GetAttributeAt(0), "wrong removed");
            Assert.AreEqual(a2, result1.GetAttributeAt(1), "wrong removed");
            Assert.AreEqual(a3, result1.GetAttributeAt(2), "wrong removed");
            Assert.AreEqual(a4, result1.GetAttributeAt(3), "wrong removed");
            Assert.AreEqual(4, result1.Count, "result length");
            
            // start with one elem coll
            
            AttributeExtCollection testColl2 = 
                AttributeExtCollection.ConvertToAttributeCollection(new object[] { a1 });
            AttributeExtCollection result2 = testColl2.RemoveAttributeOfType(typeof(TestAttributeForCollT1), 
                                                                             out removed);
            Assert.AreEqual(a1, removed, "wrong removed");
            Assert.AreEqual(0, result2.Count, "result length");
            
            
            // start with two elem coll
            
            AttributeExtCollection testColl3 =
                AttributeExtCollection.ConvertToAttributeCollection(new object[] { a1, a2 });
            
            AttributeExtCollection result3 = testColl3.RemoveAttributeOfType(typeof(TestAttributeForCollT1), 
                                                                             out removed);
            Assert.AreEqual(a1, removed, "wrong removed");
            Assert.AreEqual(a2, result3.GetAttributeAt(0), "wrong removed");
            Assert.AreEqual(1, result3.Count, "result length");
            
            
            result3 = testColl3.RemoveAttributeOfType(typeof(TestAttributeForCollT2), 
                                                      out removed);

            Assert.AreEqual(a2, removed, "wrong removed");
            Assert.AreEqual(a1, result3.GetAttributeAt(0), "wrong removed");
            Assert.AreEqual(1, result3.Count, "result length");

            
        }
        
        [Test]
        public void TestMergeCollections() {
            
            TestAttributeForCollT1 a1 = new TestAttributeForCollT1(1);
            TestAttributeForCollT2 a2 = new TestAttributeForCollT2(2);
            TestAttributeForCollT3 a3 = new TestAttributeForCollT3(3);
            TestAttributeForCollT4 a4 = new TestAttributeForCollT4(4);
            TestAttributeForCollT5 a5 = new TestAttributeForCollT5(5);
            
            AttributeExtCollection testColl1 = 
                AttributeExtCollection.ConvertToAttributeCollection(new object[] { a1, a2 });
            
            AttributeExtCollection testColl2 = 
                AttributeExtCollection.ConvertToAttributeCollection(new object[] { a3, a4, a5 });
            
            AttributeExtCollection merged1 = testColl1.MergeAttributeCollections(testColl2);
            Assert.AreEqual(a3, merged1.GetAttributeAt(0), "wrong merged");
            Assert.AreEqual(a4, merged1.GetAttributeAt(1), "wrong merged");
            Assert.AreEqual(a5, merged1.GetAttributeAt(2), "wrong merged");
            Assert.AreEqual(a1, merged1.GetAttributeAt(3), "wrong merged");
            Assert.AreEqual(a2, merged1.GetAttributeAt(4), "wrong merged");
            Assert.AreEqual(5, merged1.Count, "result length");
            
            AttributeExtCollection merged2 = testColl2.MergeAttributeCollections(testColl1);
            Assert.AreEqual(a1, merged2.GetAttributeAt(0), "wrong merged");
            Assert.AreEqual(a2, merged2.GetAttributeAt(1), "wrong merged");
            Assert.AreEqual(a3, merged2.GetAttributeAt(2), "wrong merged");
            Assert.AreEqual(a4, merged2.GetAttributeAt(3), "wrong merged");
            Assert.AreEqual(a5, merged2.GetAttributeAt(4), "wrong merged");
            Assert.AreEqual(5, merged2.Count, "result length");
            
        }
        
        [Test]
        public void TestMergeAttribute() {
            
            TestAttributeForCollT1 a1 = new TestAttributeForCollT1(1);
            TestAttributeForCollT2 a2 = new TestAttributeForCollT2(2);
            TestAttributeForCollT3 a3 = new TestAttributeForCollT3(3);
            TestAttributeForCollT4 a4 = new TestAttributeForCollT4(4);
            TestAttributeForCollT5 a5 = new TestAttributeForCollT5(5);
            
            AttributeExtCollection testColl1 = 
                AttributeExtCollection.ConvertToAttributeCollection(new object[] { a1, a2, a3, a4 });
                      
            AttributeExtCollection merged1 = testColl1.MergeAttribute(a5);
            Assert.AreEqual(a5, merged1.GetAttributeAt(0), "wrong merged");
            Assert.AreEqual(a1, merged1.GetAttributeAt(1), "wrong merged");
            Assert.AreEqual(a2, merged1.GetAttributeAt(2), "wrong merged");
            Assert.AreEqual(a3, merged1.GetAttributeAt(3), "wrong merged");
            Assert.AreEqual(a4, merged1.GetAttributeAt(4), "wrong merged");
            Assert.AreEqual(5, merged1.Count, "result length");
        }
        
        [Test]
        public void TestCollectionEqualsIsEqual() {
            TestAttributeForCollT1 a1 = new TestAttributeForCollT1(1);
            TestAttributeForCollT2 a2 = new TestAttributeForCollT2(2);
            TestAttributeForCollT3 a3 = new TestAttributeForCollT3(3);
            
            AttributeExtCollection c1 = new AttributeExtCollection(new Attribute[] { a1, a2, a3 });
            AttributeExtCollection c2 = new AttributeExtCollection(new Attribute[] { a3, a2, a1 });

            Assert.IsTrue(c1.Equals(c2), "collection equality");
        }
        
        [Test]
        public void TestCollectionEqualsNotEqual() {
            TestAttributeForCollT1 a1 = new TestAttributeForCollT1(1);
            TestAttributeForCollT2 a2 = new TestAttributeForCollT2(2);
            TestAttributeForCollT3 a3 = new TestAttributeForCollT3(3);
            
            AttributeExtCollection c1 = new AttributeExtCollection(new Attribute[] { a1, a2, a3 });
            AttributeExtCollection c2 = new AttributeExtCollection(new Attribute[] { a3, a2 });

            Assert.IsTrue(!c1.Equals(c2), "collection equality");
        }

        
    }

}
    
#endif
