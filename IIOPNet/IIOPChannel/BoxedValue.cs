/* BoxedValue.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 08.02.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using Ch.Elca.Iiop.Idl;
using Ch.Elca.Iiop.Util;
using omg.org.CORBA;

namespace Ch.Elca.Iiop.Idl {
    
    public abstract class BoxedValueBase {
        
        #region Constants
        
        /// <summary>the name of the static method, returning the Type of the boxed type</summary>
        internal const string GET_BOXED_TYPE_METHOD_NAME = "GetBoxedType";

        /// <summary>the name of the static method, returning the attributes  for the Type of the boxed type</summary>
        internal const string GET_BOXED_TYPE_ATTRIBUTES_METHOD_NAME = "GetBoxedTypeAttributes";
        
        /// <summary>the name of the static method, which returns the first non-boxed type, when following the boxing chain (array of boxed is here not considered as non-boxed type)</summary>
        public const string GET_FIRST_NONBOXED_TYPE_METHODNAME = "GetFirstNonBoxedType";
        
        /// <summary>the name of the static method, which returns the name of the first non-boxed type, when following the boxing chain (array of boxed is here not considered as non-boxed type)</summary>
        public const string GET_FIRST_NONBOXED_TYPENAME_METHODNAME = "GetFirstNonBoxedTypeName";

        #endregion Constants
        #region IConstructors

        public BoxedValueBase() {
        }

        #endregion IConstructors
        #region IMethods
        
        /// <returns>the boxed value</returns>
        protected abstract object GetValue();
        
        /// <summary>unbox this boxed value</summary>
        public object Unbox() {
            object val = GetValue();
            if (!val.GetType().IsArray) {
                return val;
            } else {
                // unbox array
                Type elemType = val.GetType().GetElementType();
                // an array of boxed values --> unbox elems?
                if (elemType.IsSubclassOf(ReflectionHelper.BoxedValueBaseType)) {
                    int length = ((Array)val).Length;
                    // determine the Type, which is boxed in boxed array element
                    Type boxedType;
                    try {
                        boxedType = (Type)elemType.InvokeMember(GET_FIRST_NONBOXED_TYPE_METHODNAME,
                                                                BindingFlags.InvokeMethod | BindingFlags.Public |
                                                                    BindingFlags.NonPublic | BindingFlags.Static |
                                                                    BindingFlags.DeclaredOnly,
                                                                null, null, new object[0]);
                    } catch (Exception) {
                        // invalid type found: elemType
                        // static method missing or not callable:
                        // GET_FIRST_NONBOXED_TYPE_METHODNAME
                        throw new INTERNAL(10041, CompletionStatus.Completed_MayBe);
                    }
                    
                    Array unboxed = Array.CreateInstance(boxedType, length);
                    for (int i = 0; i < length; i++) {
                        object unboxedElem = null;
                        if (((Array)val).GetValue(i) != null) {
                            unboxedElem = ((BoxedValueBase)((Array)val).GetValue(i)).Unbox(); // recursive unbox up to the non-boxed type
                        }
                        unboxed.SetValue(unboxedElem, i);
                    }
                    return unboxed;
                } else {
                    return val;
                }
            }
        }

        #endregion IMethods
    }
    
}


namespace omg.org.CORBA {

    /// <summary>predefined CORBA::WStringValue boxed value type</summary>
    [RepositoryIDAttribute("IDL:omg.org/CORBA/WStringValue:1.0")]
    [Serializable]
    public class WStringValue : BoxedValueBase, IIdlEntity {
        
        #region IFields
        
        [StringValue][WideCharAttribute(true)]
        private string m_val;

        #endregion IFields
        #region IConstructors
        
        /// <remarks>needed for instantiation in Value-object deserialization</remarks>
        public WStringValue() : this("") {
        }
        
        public WStringValue(string val) {
            m_val = val;
        }

        #endregion IConstructors
        #region SMethods
        public static Type GetBoxedType() {
            return ReflectionHelper.StringType;
        }
        
        internal static Type GetFirstNonBoxedType() {
            return GetBoxedType();
        }

        public static object[] GetBoxedTypeAttributes() {
            return new object[] { new StringValueAttribute() };
        }

        #endregion SMethods
        #region IMethods
        
        protected override object GetValue() {
            return m_val;
        }

        #endregion IMethods

    }
    
    /// <summary>predefined CORBA::StringValue boxed value type</summary>
    [RepositoryIDAttribute("IDL:omg.org/CORBA/StringValue:1.0")]
    [Serializable]
    public class StringValue : BoxedValueBase, IIdlEntity {
        
        #region IFields
        
        [StringValue][WideCharAttribute(false)]
        private string m_val;

        #endregion IFields
        #region IConstructors
        
        /// <remarks>needed for instantiation in Value-object deserialization</remarks>
        public StringValue() : this("") {
        }

        public StringValue(string val) {
            m_val = val;
        }

        #endregion IConstructors
        #region SMethods
        
        public static Type GetBoxedType() {
            return ReflectionHelper.StringType;
        }

        public static object[] GetBoxedTypeAttributes() {
            return new object[] { new StringValueAttribute(), new WideCharAttribute(false) };
        }
        
        internal static Type GetFirstNonBoxedType() {
            return GetBoxedType();
        }

        #endregion SMethods
        #region IMethods
        
        protected override object GetValue() {
            return m_val;
        }
        #endregion IMethods

    }


}
