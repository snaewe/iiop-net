/* CompileTimeTypeContainer.cs
 * 
 * Project: IIOP.NET
 * IDLToCLSCompiler
 * 
 * WHEN      RESPONSIBLE
 * 09.11.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using Ch.Elca.Iiop.Idl;
using Ch.Elca.Iiop.Util;

namespace Ch.Elca.Iiop.IdlCompiler.Action {
    
    
    public class CompileTimeTypeContainer : TypeContainer {
        
        #region IFields
        
        private TypeManager m_typeManager;
        
        #endregion IFIelds
        #region IConstructors
    
        public CompileTimeTypeContainer(TypeManager typeManager, Type clsType, AttributeExtCollection attrs) : 
            base(clsType, attrs) {
            m_typeManager = typeManager;
        }

        public CompileTimeTypeContainer(TypeManager typeManager, Type clsType) : 
            this(typeManager, clsType, new AttributeExtCollection()){
        }

        #endregion IConstructors
        #region IMethods

        /// <summary>split the boxed representation of a boxedValueType into unboxed + attributes</summary>
        /// <remarks>
        /// attention: make sure, this is not called before all involved boxed types are completetly created; 
        /// otherwise type couldn't be loaded from assembly?
        /// </remakrs>
        protected override void SplitBoxedForm(string boxedValueRepId) {
            string separatedClsTypeName = (string)GetCompactClsType().InvokeMember(BoxedValueBase.GET_FIRST_NONBOXED_TYPENAME_METHODNAME,
                                                                          BindingFlags.InvokeMethod | BindingFlags.Public |
                                                                          BindingFlags.NonPublic | BindingFlags.Static |
                                                                          BindingFlags.DeclaredOnly,
                                                                          null, null, new System.Object[0]);


            // check, if separatedClsTypeName is in Consturction
            Type separatedClsType = m_typeManager.GetTypeFromBuildModule(separatedClsTypeName);
            if (separatedClsType != null) {
                // this prevents possible TypeLoadException, when separated is not fully defined,
                // thrown by base implementation
                BoxedValueAttribute boxAttr = new BoxedValueAttribute(boxedValueRepId);
                CustomAttributeBuilder[] separatedAttrs = new CustomAttributeBuilder[] { 
                    boxAttr.CreateAttributeBuilder() };
                
                SetSeparated(separatedClsType, separatedAttrs, new object[] { boxAttr });
            } else {
                // not know by ModuleBuilder -> possibly in an extern assembly -> load type directly instead of byName
                base.SplitBoxedForm(boxedValueRepId);
            }
        }
        
        #endregion IMethods

    }
    
}
