/* TypeManager.cs
 * 
 * Project: IIOP.NET
 * IDLToCLSCompiler
 * 
 * WHEN      RESPONSIBLE
 * 19.02.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Reflection;
using System.Reflection.Emit;
using Ch.Elca.Iiop.Util;
using omg.org.CORBA;

namespace Ch.Elca.Iiop.Idl {    
    
    /// <summary>    
    /// helper class to contain a .NET Type and the attributes on the param, field, ...
    /// Convention: a boxed value type is in Boxed form in a type container
    /// </summary>
    public class TypeContainer {
    
        #region IFields

        private readonly Type m_clsType;
        private readonly CustomAttributeBuilder[] m_compactTypeAttrs;
        private Type m_separatedClsType;
        private CustomAttributeBuilder[] m_separatedAttrs;

        #endregion IFields
        #region IConstructors
    
        public TypeContainer(Type clsType, CustomAttributeBuilder[] attrs) {
            m_clsType = clsType;
            m_separatedClsType = null;
            m_compactTypeAttrs = attrs;
            m_separatedAttrs = null;
        }

        public TypeContainer(Type clsType) : this(clsType, new CustomAttributeBuilder[0]){
        }

        #endregion IConstructors
        #region IMethods

        /// <summary>
        /// check if CLS type is a fusioned form;
        /// </summary>
        /// <remarks>
        /// attention: make sure, this is not called before all involved boxed types are completetly created; 
        /// otherwise type couldn't be loaded from assembly?
        /// </remakrs>
        private void CheckForFusionedForm() {
        	// initalize for non-splittable:
        	m_separatedClsType = m_clsType;
        	m_separatedAttrs = m_compactTypeAttrs;
        	// check for splittable
            if (m_clsType.IsSubclassOf(typeof(BoxedValueBase))) {
                AttributeExtCollection attrColl = AttributeExtCollection.
                    ConvertToAttributeCollection(m_clsType.GetCustomAttributes(true));
                if (!(attrColl.IsInCollection(typeof(RepositoryIDAttribute)))) {
                    // invalid boxed value type
                    throw new INTERNAL(890, CompletionStatus.Completed_MayBe);
                }
                String boxedValueRepId = ((RepositoryIDAttribute)attrColl.GetAttributeForType(typeof(RepositoryIDAttribute))).Id;
                try {
                    m_separatedClsType = (Type)m_clsType.InvokeMember(BoxedValueBase.GET_FIRST_NONBOXED_TYPE_METHODNAME,
                                                                      BindingFlags.InvokeMethod | BindingFlags.Public |
                                                                      BindingFlags.NonPublic | BindingFlags.Static |
                                                                      BindingFlags.DeclaredOnly,
                                                                      null, null, new System.Object[0]);
                    m_separatedAttrs = new CustomAttributeBuilder[] { 
                                           new BoxedValueAttribute(boxedValueRepId).CreateAttributeBuilder() };
                    
                } catch (Exception e) {
                    // invalid boxedValueType found: static method missing or not callable: BoxedValueBase.GET_FIRST_NONBOXED_TYPE_METHODNAME
                    Console.WriteLine("problematic boxed val found: " + m_clsType.FullName + "; ex: " + e);
                    throw new INTERNAL(890, CompletionStatus.Completed_MayBe);
                }
            }
        }

        /// <summary>
        /// returns the type this TypeContainer was initalized from.        
        /// </summary>
        /// <remarks>The typeContainer is initalized with the fusioned form of a type when a form exists, where
        /// fusioned type is separated in a type and attributes.</remarks>
        public Type GetCompactClsType() {
            return m_clsType;
        }

        /// <summary>
        /// returns the custom attributes this TypeContainer was initalized from.        
        /// </summary>
        public CustomAttributeBuilder[] GetCompactTypeAttrs() {
            return m_compactTypeAttrs;            
        }

        
        /// <summary>
        /// returns the type, which is used for paramters, fields, ...        
        /// </summary>
        /// <remarks>
        /// If TypeContainer is initalized with a fusioned form of a type, 
        /// this method returns the extracted type from the fusioned form.
        /// </remarks>
        /// <returns></returns>
        public Type GetSeparatedClsType() {
            lock(this) {
                if (m_separatedClsType == null) {
                    CheckForFusionedForm();
                }
                return m_separatedClsType;
            }
        }

        /// <summary>
        /// Returns the attributes needed on parameters, fields, ...
        /// </summary>
        /// <remarks>
        /// If TypeContainer is initalized with a fusioned form of a type, 
        /// this method returns the extracted attributes from the fusioned form.
        /// </remarks>
        /// <returns></returns>
        public CustomAttributeBuilder[] GetSeparatedAttrs() {
            lock(this) {
                if (m_separatedAttrs == null) {
                    CheckForFusionedForm();
                }
                return m_separatedAttrs;
            }
        }

        #endregion IMethods

    }

}