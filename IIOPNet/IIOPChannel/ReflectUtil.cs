/* ReflectUtil.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 10.09.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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

namespace Ch.Elca.Iiop.Util {
    
    /// <summary>
    /// Adds some missing reflection functionalty.
    /// </summary>
    public class ReflectionHelper {

        /// <summary>
        /// collects the custom attributes on the current parameter and from
        /// the paramters from inherited methods.
        /// </summary>
        /// <param name="paramInfo">the parameter to check</param>
        /// <returns>a collection of attributes</returns>
        public static AttributeExtCollection CollectParameterAttributes(ParameterInfo paramInfo, MethodInfo paramInMethod) {
            AttributeExtCollection result = AttributeExtCollection.ConvertToAttributeCollection(
                paramInfo.GetCustomAttributes(true));
            if (!paramInMethod.IsVirtual) {
                return result;
            }

            MethodInfo baseDecl = paramInMethod.GetBaseDefinition();
            if (!baseDecl.Equals(paramInMethod)) {
                // add param attributes from base definition if not already present
                ParameterInfo[] baseParams = baseDecl.GetParameters();
                ParameterInfo baseParamToConsider = baseParams[paramInfo.Position];
                result.AddMissingAttributes(baseParamToConsider.GetCustomAttributes(true));
            }
            
            Type declaringType = paramInMethod.DeclaringType;
            // search interfaces for method definition
            Type[] interfaces = declaringType.GetInterfaces();
            foreach (Type interf in interfaces) {
                MethodInfo found = IsMethodDefinedInInterface(paramInMethod, interf);
                if (found != null) {
                    // add param attributes from interface definition if not already present
                    ParameterInfo[] ifParams = found.GetParameters();
                    ParameterInfo ifParamToConsider = ifParams[paramInfo.Position];
                    result.AddMissingAttributes(ifParamToConsider.GetCustomAttributes(true));
                }
            }

            return result;
        }


        /// <summary>
        /// collects the custom attributes on the return parameter and from
        /// the return paramters from inherited methods.
        /// </summary>
        /// <returns>a collection of attributes</returns>
        public static AttributeExtCollection CollectReturnParameterAttributes(MethodInfo method) {
            AttributeExtCollection result = AttributeExtCollection.ConvertToAttributeCollection(
                method.ReturnTypeCustomAttributes.GetCustomAttributes(true));

            if (!method.IsVirtual) {
                return result;
            }
            
            MethodInfo baseDecl = method.GetBaseDefinition();
            if (!baseDecl.Equals(method)) {
                // add return param attributes from base definition if not already present               
                result.AddMissingAttributes(baseDecl.ReturnTypeCustomAttributes.GetCustomAttributes(true));
            }
            
            Type declaringType = method.DeclaringType;
            // search interfaces for method definition
            Type[] interfaces = declaringType.GetInterfaces();
            foreach (Type interf in interfaces) {
                MethodInfo found = IsMethodDefinedInInterface(method, interf);
                if (found != null) {
                    // add return param attributes from interface definition if not already present
                    result.AddMissingAttributes(found.ReturnTypeCustomAttributes.GetCustomAttributes(true));
                }
            }

            return result;
                               
        }

        /// <summary>
        /// checks, if a similar method is defined in the interface specified;
        /// returns its MethodInfo if true, else returns null;
        /// </summary>
        /// <param name="method"></param>
        /// <param name="ifType"></param>
        /// <returns></returns>
        private static MethodInfo IsMethodDefinedInInterface(MethodInfo method, Type ifType) {            
            try {
                MethodInfo found = ifType.GetMethod(method.Name, ExtractMethodTypes(method));
                return found;
            } catch (Exception) {
                return null;
            }
        }

        private static Type[] ExtractMethodTypes(MethodInfo method) {
            ParameterInfo[] parameters = method.GetParameters();
            Type[] result = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++) {
                result[i] = parameters[i].ParameterType;
            }
            return result;
        }
        
        /// <summary>checks, if a method matching the MethodInfo method is defined on the type</summary>
        public static bool IsMethodDefinedOnType(MethodInfo method, Type type,
                                                 BindingFlags flags) {        	
        	MethodInfo foundMethod = type.GetMethod(method.Name, flags, null, ExtractMethodTypes(method),
        	                                        null);
            return (foundMethod != null);            
        }
        
        public static bool IsPropertyDefinedOnType(PropertyInfo property, Type type,
                                                   BindingFlags flags) {
            PropertyInfo foundProperty = type.GetProperty(property.Name, flags,
                                                          null, property.PropertyType,
                                                          Type.EmptyTypes, null);
            return (foundProperty != null);
        }

    }


}
