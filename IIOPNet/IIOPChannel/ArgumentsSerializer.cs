/* ArgumentsSerilizer.cs
 *
 * Project: IIOP.NET
 * IIOPChannel
 *
 * WHEN      RESPONSIBLE
 * 30.12.05  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
 *
 * Copyright 2005 Dominic Ullmann
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
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using Ch.Elca.Iiop.Cdr;
using Ch.Elca.Iiop.Idl;
using Ch.Elca.Iiop.Util;

namespace Ch.Elca.Iiop.Marshalling {

    /// <summary>
    /// Serializes/Deserialises Request/Replys arguments for a remote interface.
    /// </summary>
    internal class ArgumentsSerializer {

        #region Types

        internal class ArgumentsMapping {

            private ArgumentMapping[] m_arguments;
            private ArgumentMapping m_returnValue;
            private string[] m_contextElementKeys;

            internal ArgumentsMapping(ArgumentMapping returnValue, ArgumentMapping[] arguments,
                                      string[] contextElementKeys) {
                m_arguments = arguments;
                m_returnValue = returnValue;
                m_contextElementKeys = contextElementKeys;
            }

            private void SerializeContextElements(CdrOutputStream targetStream,
                                                  LogicalCallContext callContext,
                                                  Serializer contextElementSer) {
                // if no context elements specified, don't serialise a context sequence.
                if (m_contextElementKeys.Length > 0) {
                    string[] contextSeq = new string[m_contextElementKeys.Length * 2];
                    for (int i = 0; i < m_contextElementKeys.Length; i++) {
                        string contextKey =
                            m_contextElementKeys[i];
                        contextSeq[i * 2] = contextKey;
                        if (callContext.GetData(contextKey) != null) {
                            contextSeq[i * 2 + 1] = callContext.GetData(contextKey).ToString();
                        } else {
                            contextSeq[i * 2 + 1] = "";
                        }
                    }
                    contextElementSer.Serialize(contextSeq, targetStream);
                }
            }

            private IDictionary DeserializeContextElements(CdrInputStream sourceStream,
                                                           Serializer contextElementSer) {
                IDictionary result = new HybridDictionary();
                // if no context elements specified, don't deserialise a context sequence.
                if (m_contextElementKeys.Length > 0) {
                    string[] contextElems = (string[])contextElementSer.Deserialize(sourceStream);
                    if (contextElems.Length % 2 != 0) {
                        throw new omg.org.CORBA.MARSHAL(67, omg.org.CORBA.CompletionStatus.Completed_No);
                    }
                    for (int i = 0; i < contextElems.Length; i += 2) {
                        string contextElemKey = contextElems[i];
                        // insert into call context, if part of signature
                        for (int j = 0; j < m_contextElementKeys.Length; j++) {
                            if (m_contextElementKeys[j] == contextElemKey) {
                                result[contextElemKey] = contextElems[i + 1];
                                break;
                            }
                        }
                    }
                }
                return result;
            }

            internal void SerializeRequestArgs(object[] arguments, CdrOutputStream targetStream,
                                               LogicalCallContext context, Serializer contextElementSer) {
                for (int actualParamNr = 0; actualParamNr < arguments.Length; actualParamNr++) {
                    ArgumentMapping paramInfo = m_arguments[actualParamNr];
                    // iterate through the parameters, nonOut and nonRetval params are serialised for a request
                    if (paramInfo.IsInArg() || paramInfo.IsRefArg()) {
                        paramInfo.Serialize(targetStream, arguments[actualParamNr]);
                    }
                    // move to next parameter
                    // out-args are also part of the arguments array -> move to next for those whithout doing something
                }
                SerializeContextElements(targetStream, context, contextElementSer);
            }

            internal void SerializeResponseArgs(object result, object[] outArgs,
                                                CdrOutputStream targetStream) {
                // first serialise the return value,
                if (m_returnValue.IsNonVoidReturn()) {
                    m_returnValue.Serialize(targetStream, result);
                }
                // ... then the out/ref args
                int outParamNr = 0;
                for (int actualParamNr = 0; actualParamNr < m_arguments.Length; actualParamNr++) {
                    ArgumentMapping paramInfo = m_arguments[actualParamNr];
                    // iterate through the parameters, out/ref parameters are serialised
                    if (paramInfo.IsOutArg() || paramInfo.IsRefArg()) {
                        paramInfo.Serialize(targetStream, outArgs[outParamNr]);
                        outParamNr++;
                    }
                }
            }

            internal object[] DeserialiseRequestArgs(CdrInputStream sourceStream,
                                                     out IDictionary contextElements,
                                                     Serializer contextElementSer) {
                object[] result = new object[m_arguments.Length];
                for (int actualParamNr = 0; actualParamNr < m_arguments.Length; actualParamNr++) {
                    ArgumentMapping paramInfo = m_arguments[actualParamNr];
                    if (paramInfo.IsInArg() || paramInfo.IsRefArg()) {
                        result[actualParamNr] = paramInfo.Deserialize(sourceStream);
                    } // else: null for an out parameter
                }
                contextElements = DeserializeContextElements(sourceStream, contextElementSer);
                return result;
            }

            internal object DeserialiseResponseArgs(out object[] outArgs,
                                                    CdrInputStream sourceStream) {
                // demarshal first the return value;
                object retValue = null;
                if (m_returnValue.IsNonVoidReturn()) {
                    retValue = m_returnValue.Deserialize(sourceStream);
                }

                // ... then the outargs
                outArgs = new object[m_arguments.Length];
                bool outArgFound = false;
                for (int actualParamNr = 0; actualParamNr < m_arguments.Length; actualParamNr++) {
                    ArgumentMapping paramInfo = m_arguments[actualParamNr];
                    if (paramInfo.IsOutArg() || paramInfo.IsRefArg()) {
                        outArgs[actualParamNr] = paramInfo.Deserialize(sourceStream);
                        outArgFound = true;
                    } // else: for an in param null must be added to out-args
                }

                // prepare the result
                // need to return empty array, if no out-arg is present, because otherwise async calls fail
                if (!outArgFound) {
                    outArgs = new object[0];
                }
                return retValue;
            }

        }

        internal enum ArgumentsKind {
            Unknown, InArg, OutArg, RefArg, Return
        }

        /// <summary>A mapping for a single parameter of a method.</summary>
        internal class ArgumentMapping {

            private Serializer m_ser;
            private ArgumentsKind m_kind;

            internal ArgumentMapping(Serializer ser, ArgumentsKind kind) {
                m_ser = ser;
                m_kind = kind;
            }

            /// <summary> use this for mapping a void return</summary>
            internal ArgumentMapping() : this(null, ArgumentsKind.Return) {
            }

            internal bool IsVoidReturn() {
                return m_ser == null && m_kind == ArgumentsKind.Return;
            }

            internal bool IsNonVoidReturn() {
                return m_ser != null && m_kind == ArgumentsKind.Return;
            }

            internal bool IsInArg() {
                return m_ser != null && m_kind == ArgumentsKind.InArg;
            }

            internal bool IsOutArg() {
                return m_ser != null && m_kind == ArgumentsKind.OutArg;
            }

            internal bool IsRefArg() {
                return m_ser != null && m_kind == ArgumentsKind.RefArg;
            }

            internal void Serialize(CdrOutputStream stream, object actual) {
                m_ser.Serialize(actual, stream);
            }

            internal object Deserialize(CdrInputStream stream) {
                return m_ser.Deserialize(stream);
            }

        }

        #endregion Types
        #region IFields

        private IDictionary /* <string, ArgumentsMapping> */ m_methodMappings = new Hashtable();
        private IDictionary /* <string, MethodInfo> */ m_methodInfoForName = new Hashtable();
        private IDictionary /* <MethodInfo, string> */ m_nameForMethodInfo = new Hashtable();

        private Serializer m_contextElementSer; // for performance reasons: expensive to create

        #endregion IFields
        #region IConstructors

        internal ArgumentsSerializer(Type forType, SerializerFactory serFactory) {
            m_contextElementSer =
                serFactory.Create(typeof(string[]),
                                  new AttributeExtCollection(new Attribute[] { new IdlSequenceAttribute(0L),
                                                                               new StringValueAttribute(),
                                                                               new WideCharAttribute(false) }));
            DetermineTypeMapping(forType, serFactory);
        }

        #endregion IConstructors
        #region IMethods

        #region MappingDetermination

        /// <summary>
        /// returns true, if no serialization code should be generated for a method;
        /// otherwise false.
        /// </summary>
        private bool IgnoreMethod(MethodInfo info) {
            if (info.DeclaringType.IsInterface) {
                return false;
            }
            // only ignore methods on classes and not on interfaces
            if ((info.Name == "InitializeLifetimeService") && (info.GetParameters().Length == 0) &&
                (info.ReturnType.Equals(ReflectionHelper.ObjectType))) {
                return true;
            } // from MarshalByRefObject
            // TODO
            return false;
        }

        private bool IgnoreProperty(PropertyInfo info) {
            // TODO
            return false;
        }

        private string DetermineOperationName(MethodInfo method) {
            bool isMethodOverloaded = false;
            try {
                method.DeclaringType.GetMethod(method.Name, BindingFlags.Public | BindingFlags.Instance);
            } catch (AmbiguousMatchException) {
                isMethodOverloaded = true;
            }
            string mappedName =
                IdlNaming.GetMethodRequestOperationName(method, isMethodOverloaded);
            return mappedName;
        }

        private void StoreMethodMappingFor(MethodInfo info, string idlOperationName,
                                           SerializerFactory serFactory) {
            m_methodMappings[idlOperationName] =
                DetermineMethodMapping(info, serFactory);
            m_methodInfoForName[idlOperationName] = info;
            m_nameForMethodInfo[info] = idlOperationName;
        }

        private void DetermineTypeMapping(Type forType, SerializerFactory serFactory) {
            DetermineMapping(forType, serFactory);

            // Lets also check implemented interfaces that might be explicitely implemented
            // making methods/properties invisible to previous research
            foreach (Type interfaceType in forType.GetInterfaces())
            {
                DetermineMapping(interfaceType, serFactory);
            }
        }

        private void DetermineMapping(Type forType, SerializerFactory serFactory) {
            MethodInfo[] methods =
                forType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < methods.Length; i++) {
                if (methods[i].IsSpecialName) {
                    continue;
                }
                if (IgnoreMethod(methods[i])) {
                    // don't support remote calls for method
                    continue;
                }
                string operationName = DetermineOperationName(methods[i]);
                StoreMethodMappingFor(methods[i], operationName, serFactory);
            }

            PropertyInfo[] properties =
                forType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < properties.Length; i++) {
                if (IgnoreProperty(properties[i])) {
                    // don't support remote calls for this property
                    continue;
                }
                MethodInfo getter = properties[i].GetGetMethod();
                MethodInfo setter = properties[i].GetSetMethod();
                if (getter != null) {
                    string operationName = IdlNaming.GetPropertyRequestOperationName(properties[i], false);
                    StoreMethodMappingFor(getter, operationName, serFactory);
                }
                if (setter != null) {
                    string operationName = IdlNaming.GetPropertyRequestOperationName(properties[i], true);
                    StoreMethodMappingFor(setter, operationName, serFactory);
                }
            }
        }

        private ArgumentsMapping DetermineMethodMapping(MethodInfo method, SerializerFactory serFactory) {
            ArgumentMapping returnMapping =
                DetermineReturnParamMapping(method, serFactory);
            ArgumentMapping[] paramMappings =
                DetermineParamMappings(method, serFactory);
            string[] contextElementNames = DetermineContextElements(method);
            return new ArgumentsMapping(returnMapping, paramMappings, contextElementNames);
        }

        private ArgumentMapping DetermineReturnParamMapping(MethodInfo method, SerializerFactory serFactory) {
            ArgumentMapping returnMapping;
            // return value
            if (!method.ReturnType.Equals(ReflectionHelper.VoidType)) {
                AttributeExtCollection returnAttrs =
                    ReflectionHelper.CollectReturnParameterAttributes(method);
                Serializer retMappingSer = serFactory.Create(method.ReturnType, returnAttrs);
                returnMapping = new ArgumentMapping(retMappingSer, ArgumentsKind.Return);
            } else {
                // no retrun value to serialise/deserialise
                returnMapping = new ArgumentMapping();
            }
            return returnMapping;
        }

        private ArgumentMapping[] DetermineParamMappings(MethodInfo method, SerializerFactory serFactory) {
            ParameterInfo[] parameters = method.GetParameters();
            ArgumentMapping[] paramMappings = new ArgumentMapping[parameters.Length];
            for (int actualParamNr = 0; actualParamNr < parameters.Length; actualParamNr++) {
                ParameterInfo paramInfo = parameters[actualParamNr];
                // iterate through the parameters to determine mapping for each
                ArgumentsKind argKind = ArgumentsKind.Unknown;
                if (ReflectionHelper.IsInParam(paramInfo)) {
                    argKind = ArgumentsKind.InArg;
                } else if (ReflectionHelper.IsOutParam(paramInfo)) {
                    argKind = ArgumentsKind.OutArg;
                } else if (ReflectionHelper.IsRefParam(paramInfo)) {
                    argKind = ArgumentsKind.RefArg;
                }
                AttributeExtCollection paramAttrs =
                    ReflectionHelper.CollectParameterAttributes(paramInfo,
                                                                method);
                Serializer paramSer = serFactory.Create(paramInfo.ParameterType, paramAttrs);
                paramMappings[actualParamNr] = new ArgumentMapping(paramSer, argKind);
            }
            return paramMappings;
        }

        private string[] DetermineContextElements(MethodInfo method) {
            AttributeExtCollection methodAttrs =
                ReflectionHelper.GetCustomAttriutesForMethod(method, true,
                                                             ReflectionHelper.ContextElementAttributeType);
            string[] result = new string[methodAttrs.Count];
            for (int i = 0; i < methodAttrs.Count; i++) {
                result[i] = ((ContextElementAttribute)methodAttrs[i]).ContextElementKey;
            }
            return result;
        }

        #endregion MappingDetermination

        private ArgumentsMapping GetArgumentsMapping(string idlMethodName) {
            ArgumentsMapping result = (ArgumentsMapping)m_methodMappings[idlMethodName];
            if (result == null) {
                throw new omg.org.CORBA.INTERNAL(2101, omg.org.CORBA.CompletionStatus.Completed_MayBe);
            }
            return result;
        }

        internal void SerializeRequestArgs(string idlMethodName, object[] arguments,
                                           CdrOutputStream targetStream, LogicalCallContext context) {
            ArgumentsMapping mapping = GetArgumentsMapping(idlMethodName);
            mapping.SerializeRequestArgs(arguments, targetStream, context, m_contextElementSer);
        }

        internal void SerializeResponseArgs(string idlMethodName, object result, object[] outArgs,
                                            CdrOutputStream targetStream) {
            ArgumentsMapping mapping = GetArgumentsMapping(idlMethodName);
            mapping.SerializeResponseArgs(result, outArgs, targetStream);
        }

        internal object[] DeserializeRequestArgs(string idlMethodName, CdrInputStream sourceStream,
                                                 out IDictionary contextElements) {
            ArgumentsMapping mapping = GetArgumentsMapping(idlMethodName);
            return mapping.DeserialiseRequestArgs(sourceStream, out contextElements,
                                                  m_contextElementSer);
        }

        internal object DeserializeResponseArgs(string idlMethodName, out object[] outArgs,
                                                CdrInputStream sourceStream) {
            ArgumentsMapping mapping = GetArgumentsMapping(idlMethodName);
            return mapping.DeserialiseResponseArgs(out outArgs, sourceStream);
        }

        internal string GetRequestNameFor(MethodInfo info) {
            string result = (string)m_nameForMethodInfo[info];
            if (result == null) {
                throw new omg.org.CORBA.INTERNAL(2101, omg.org.CORBA.CompletionStatus.Completed_MayBe);
            }
            return result;
        }

        internal MethodInfo GetMethodInfoFor(string idlMethodName) {
            MethodInfo result = (MethodInfo)m_methodInfoForName[idlMethodName];
            if (result == null) {
                throw new omg.org.CORBA.BAD_OPERATION(2101, omg.org.CORBA.CompletionStatus.Completed_MayBe);
            }
            return result;
        }

        #endregion IMethods

    }

}
