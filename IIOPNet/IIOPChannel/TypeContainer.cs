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
using System.Collections;
using System.Diagnostics;
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
        private readonly CustomAttributeBuilder[] m_compactTypeAttrsBuilder;
        private readonly AttributeExtCollection m_compactTypeAttrInstances;
        private readonly Type m_assignableFromType;
        private Type m_separatedClsType;        
        private CustomAttributeBuilder[] m_separatedAttrsBuilder;
        private AttributeExtCollection m_separatedAttrInstances;

        #endregion IFields
        #region IConstructors
    
        public TypeContainer(Type clsType) : this(clsType, new AttributeExtCollection()){
        }
        
        /// <param name="clsType">the type in this container</param>
        /// <param name="assignableFromType">can be used to specify the type of values assignable to clsType;
        /// useful e.g for uints, which are casted to ints</param>
        public TypeContainer(Type clsType, Type assignableFromType) :
            this(clsType, AttributeExtCollection.EmptyCollection, assignableFromType, false){
        }        
        
        /// <summary>takes the type and the attributes in compact form</summary>
        public TypeContainer(Type clsType, AttributeExtCollection attrs) : this(clsType, attrs, clsType, false) {
        }

        /// <summary>
        /// takes the type and the attributes either in compact or separated form (use alreadySeparated to specify).
        /// </summary>
        public TypeContainer(Type clsType, AttributeExtCollection attrs,
                             bool alreadySeparated) : this(clsType, attrs, clsType, alreadySeparated) {
                                 
        }

        
        /// <summary>
        /// takes the type and the attributes either in compact or separated form (use alreadySeparated to specify).
        /// </summary>
        /// <param name="clsType">the type in this container</param>        
        /// <param name="attrs">the cls type attributes</param>
        /// <param name="assignableFromType">can be used to specify the type of values assignable to clsType;
        /// useful e.g for uints, which are casted to ints</param>        
        /// <param name="alreadySeparated">is clsType in separated form or not</param>
        public TypeContainer(Type clsType, AttributeExtCollection attrs, Type assignableFromType,
                             bool alreadySeparated) {
            if (attrs == null) {
                throw new ArgumentException("TypeContainer; attrs must be != null"); 
            }
            
            ArrayList custAttrBuilders = new ArrayList();
            foreach (Object attr in attrs) {
                if (attr is IIdlAttribute) {
                    CustomAttributeBuilder builder =
                        ((IIdlAttribute) attr).CreateAttributeBuilder();
                    custAttrBuilders.Add(builder);
                } else {
                    throw new INTERNAL(454, CompletionStatus.Completed_MayBe);
                }
            }
            CustomAttributeBuilder[] custAttrBuilderArray = 
                (CustomAttributeBuilder[])custAttrBuilders.ToArray(typeof(CustomAttributeBuilder));
            
            m_clsType = clsType;
            m_compactTypeAttrsBuilder = custAttrBuilderArray;
            m_compactTypeAttrInstances = attrs;                       
            m_assignableFromType = assignableFromType;
            if (alreadySeparated) {   
                m_separatedClsType = clsType;
                m_separatedAttrsBuilder = custAttrBuilderArray;
                m_separatedAttrInstances = attrs;
            } else {
                m_separatedClsType = null;
                m_separatedAttrsBuilder = null;
                m_separatedAttrInstances = null;
            }            
        }                
        
        #endregion IConstructors
        #region IMethods
        
        /// <summary>
        /// check if CLS type is a fusioned form;
        /// </summary>
        private void CheckForFusionedForm() {
            // initalize for non-splittable:
            m_separatedClsType = m_clsType;
            m_separatedAttrsBuilder = m_compactTypeAttrsBuilder;
            // check for splittable
            if (m_clsType.IsSubclassOf(ReflectionHelper.BoxedValueBaseType)) {
                AttributeExtCollection attrColl = AttributeExtCollection.
                    ConvertToAttributeCollection(m_clsType.GetCustomAttributes(true));
                if (!(attrColl.IsInCollection(typeof(RepositoryIDAttribute)))) {
                    // invalid boxed value type
                    throw new INTERNAL(890, CompletionStatus.Completed_MayBe);
                }
                String boxedValueRepId = ((RepositoryIDAttribute)attrColl.GetAttributeForType(typeof(RepositoryIDAttribute))).Id;
                try {
                    SplitBoxedForm(boxedValueRepId);
                } catch (Exception e) {
                    // invalid boxedValueType found: static method missing or not callable: BoxedValueBase.GET_FIRST_NONBOXED_TYPE_METHODNAME
                    Trace.WriteLine("problematic boxed val found: " + m_clsType.FullName + "; ex: " + e);
                    throw new INTERNAL(890, CompletionStatus.Completed_MayBe);
                }
            }
        }
        
        
        /// <summary>split the boxed representation of a boxedValueType into unboxed + attributes</summary>
        /// <remarks>
        /// attention: make sure, this is not called before all involved boxed types are completetly created; 
        /// otherwise type couldn't be loaded from assembly?
        /// </remakrs>
        protected virtual void SplitBoxedForm(string boxedValueRepId) {
            Type separatedClsType = (Type)m_clsType.InvokeMember(BoxedValueBase.GET_FIRST_NONBOXED_TYPE_METHODNAME,
                                                              BindingFlags.InvokeMethod | BindingFlags.Public |
                                                              BindingFlags.NonPublic | BindingFlags.Static |
                                                              BindingFlags.DeclaredOnly,
                                                              null, null, new System.Object[0]);
            
            BoxedValueAttribute boxedValueAttr = new BoxedValueAttribute(boxedValueRepId);
            CustomAttributeBuilder[] separatedAttrsBuilder = new CustomAttributeBuilder[] {
                                    boxedValueAttr.CreateAttributeBuilder() };
            SetSeparated(separatedClsType, separatedAttrsBuilder, 
                         new object[] { boxedValueAttr } );
            
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
            return m_compactTypeAttrsBuilder;            
        }
        
        public AttributeExtCollection GetCompactTypeAttrInstances() {
            return m_compactTypeAttrInstances;
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
                if (m_separatedAttrsBuilder == null) {
                    CheckForFusionedForm();
                }
                return m_separatedAttrsBuilder;
            }
        }
        
        public AttributeExtCollection GetSeparatedAttrInstances() {
            lock(this) {
                if (m_separatedAttrsBuilder == null) {
                    CheckForFusionedForm();
                }
                return m_separatedAttrInstances;
            }
        }
        
        protected void SetSeparated(Type separatedType, CustomAttributeBuilder[] separatedAttrs,
                                    object[] separatedAttrInstances) {
            lock(this) {
                m_separatedClsType = separatedType;
                m_separatedAttrsBuilder = separatedAttrs;
                m_separatedAttrInstances = 
                    AttributeExtCollection.ConvertToAttributeCollection(separatedAttrInstances);
            }
        }
        
        /// <summary>
        /// the the type of values assignable to the contained type; useable e.g in case of uint -> int conversion.
        /// </summary>        
        public Type GetAssignableFromType() {
            return m_assignableFromType;
        }

        #endregion IMethods

    }

}
