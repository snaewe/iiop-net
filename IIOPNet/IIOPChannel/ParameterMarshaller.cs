/* ParameterMarshaller.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 14.01.03  Dominic Ullmann (DUL), dul@elca.ch
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
using System.Collections;
using Ch.Elca.Iiop.Cdr;
using Ch.Elca.Iiop.Util;

namespace Ch.Elca.Iiop.Marshalling {

    /// <summary>
    /// Marshalles and Unmarshalles method parameters
    /// </summary>
    public class ParameterMarshaller {

        #region SFields

        private static ParameterMarshaller s_singletonMarshaller = new ParameterMarshaller();

        #endregion SFields
        #region IConstructors
        
        private ParameterMarshaller() {
        }

        #endregion IConstructors
        #region SMethods

        public static ParameterMarshaller GetSingleton() {
            return s_singletonMarshaller;
        }

        #endregion SMethods
        #region IMethods

        /// <summary>
        /// marshals a parameter
        /// </summary>
        /// <param name="paramInfo">The parameter reflection data</param>
        /// <param name="actual">the value, which should be marshalled for the parameter</param>
        private void Marshal(ParameterInfo paramInfo, object actual, CdrOutputStream targetStream) {
            Marshal(paramInfo.ParameterType, 
                    AttributeExtCollection.ConvertToAttributeCollection(
                                                paramInfo.GetCustomAttributes(true)),
                    actual, targetStream);
        }

        /// <summary>
        /// marshals an intem
        /// </summary>
        /// <param name="type"></param>
        /// <param name="attributes"></param>
        /// <param name="actual">the, which should be marshalled</param>
        /// <param name="targetStream"></param>
        private void Marshal(Type type, AttributeExtCollection attributes, object actual,
                             CdrOutputStream targetStream) {
            Marshaller marshal = Marshaller.GetSingleton();
            marshal.Marshal(type, attributes, actual, targetStream);
        }

        /// <summary>
        /// unmarshals a parameter
        /// </summary>
        /// <param name="paramInfo"></param>
        /// <param name="sourceStream"></param>
        /// <returns></returns>
        private object Unmarshal(ParameterInfo paramInfo, CdrInputStream sourceStream) {
            return Unmarshal(paramInfo.ParameterType, 
                             AttributeExtCollection.ConvertToAttributeCollection(
                                    paramInfo.GetCustomAttributes(true)),
                             sourceStream);
        }

        /// <summary>
        /// unmarshals an item
        /// </summary>
        /// <param name="type"></param>
        /// <param name="attributes"></param>
        /// <param name="sourceStream"></param>
        /// <returns></returns>
        private object Unmarshal(Type type, AttributeExtCollection attributes, 
                                 CdrInputStream sourceStream) {
            Marshaller marshal = Marshaller.GetSingleton();   
            object unmarshalled = marshal.Unmarshal(type, attributes, sourceStream);
            return unmarshalled;
        }


        public static bool IsOutParam(ParameterInfo paramInfo) {
            return paramInfo.IsOut;
        }

        public static bool IsRefParam(ParameterInfo paramInfo) {
            if (!paramInfo.ParameterType.IsByRef) { return false; }
            return (!paramInfo.IsOut) || (paramInfo.IsOut && paramInfo.IsIn);
        }

        public static bool IsInParam(ParameterInfo paramInfo) {
            if (paramInfo.ParameterType.IsByRef) { return false; } // only out/ref params have a byRef type
            return ((paramInfo.IsIn) || 
                    ((!(paramInfo.IsOut)) && (!(paramInfo.IsRetval))));
        }

        /// <summary>
        /// serialises the parameters while sending a request.
        /// </summary>
        /// <param name="method">the information about the method the request is targeted for</param>
        /// <param name="actual">the values, which should be marshalled for the parameters</param>
        public void SerialiseRequestArgs(MethodInfo method, object[] actual, CdrOutputStream targetStream) {
            ParameterInfo[] parameters = method.GetParameters();
            int actualParamNr = 0;
            foreach (ParameterInfo paramInfo in parameters) {
                // iterate through the parameters, nonOut and nonRetval params are serialised for a request
                if (IsInParam(paramInfo) || IsRefParam(paramInfo)) {
                    Marshal(paramInfo, actual[actualParamNr], targetStream);                    
                    actualParamNr++;
                }
            }
        }

        /// <summary>
        /// deserialises the parameters while receiving a request.
        /// </summary>
        /// <param name="method">the information about the method the request is targeted for</param>
        /// <returns>
        /// an array of all deserialised arguments
        /// </returns>
        public object[] DeserialiseRequestArgs(MethodInfo method, CdrInputStream sourceStream) {
            ParameterInfo[] parameters = method.GetParameters();
            ArrayList demarshalled = new ArrayList();
            
            foreach (ParameterInfo paramInfo in parameters) {
                if (IsInParam(paramInfo) || IsRefParam(paramInfo)) {
                    object unmarshalledParam = Unmarshal(paramInfo, sourceStream);
                    demarshalled.Add(unmarshalledParam);
                } else if (IsOutParam(paramInfo)) {
                    // add null for an out parameter
                    demarshalled.Add(null);
                }
            }
            // prepare the result
            object[] result = demarshalled.ToArray();
            if (result == null) { result = new object[0]; }
            return result;
        }

        /// <summary>
        /// serialises the paramters while sending a response.
        /// </summary>
        /// <param name="method">the information about the the method this response is from</param>
        /// <param name="retValue">the return value of the method call</param>
        public void SerialiseResponseArgs(MethodInfo method, object retValue, object[] outArgs,
                                          CdrOutputStream targetStream) {
            ParameterInfo[] parameters = method.GetParameters();
            // first serialise the return value, 
            if (method.ReturnType != typeof(void)) {
                AttributeExtCollection returnAttr = AttributeExtCollection.ConvertToAttributeCollection(
                                                        method.ReturnTypeCustomAttributes.GetCustomAttributes(true));
                Marshal(method.ReturnType, returnAttr, retValue, targetStream);
            }
            // ... then the out/ref args
            int outParamNr = 0;
            foreach (ParameterInfo paramInfo in parameters) {
                // iterate through the parameters, out/ref parameters are serialised
                if (IsOutParam(paramInfo) || IsRefParam(paramInfo)) {
                    Marshal(paramInfo, outArgs[outParamNr], targetStream);
                    outParamNr++;
                }
            }
        }

        /// <summary>
        /// deserialises the parameters while receiving a response.
        /// </summary>
        /// <param name="method">the information about the the method this response is from</param>
        /// <param name="outArgs">the out-arguments deserialiesed here</param>
        /// <returns>the return value of the method-call</returns>
        public object DeserialiseResponseArgs(MethodInfo method, CdrInputStream sourceStream,
                                              out object[] outArgs) {
            ParameterInfo[] parameters = method.GetParameters();
            // demarshal first the return value, 
            object retValue = null;
            if (method.ReturnType != typeof(void)) {
                AttributeExtCollection returnAttr = AttributeExtCollection.ConvertToAttributeCollection(
                                                        method.ReturnTypeCustomAttributes.GetCustomAttributes(true));
                retValue = Unmarshal(method.ReturnType, returnAttr, sourceStream);
            }
            
            // ... then the outargs
            ArrayList demarshalledOutArgs = new ArrayList();
            foreach (ParameterInfo paramInfo in parameters) 
            {
                if (IsOutParam(paramInfo) || IsRefParam(paramInfo)) {
                    object unmarshalledParam = Unmarshal(paramInfo, sourceStream);
                    demarshalledOutArgs.Add(unmarshalledParam);
                }
            }

            // prepare the result
            outArgs = demarshalledOutArgs.ToArray();
            if (outArgs == null) { 
                outArgs = new object[0]; 
            }
            return retValue;
        }

        #endregion IMethods

    }
}
