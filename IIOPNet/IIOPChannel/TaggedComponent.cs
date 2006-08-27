/* TaggedComponent.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 16.04.05  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using Ch.Elca.Iiop.Cdr;
using Ch.Elca.Iiop.Idl;
using Ch.Elca.Iiop.Marshalling;
using Ch.Elca.Iiop.Util;
using omg.org.CORBA;

namespace omg.org.IOP {       
       
    /// <summary>the id of the tagged component for code-set</summary>
    public sealed class TAG_CODE_SETS {
        
        #region Constants
        
        public const int ConstVal = 1;
        
        #endregion Constants
        #region IConstructors
        
        private TAG_CODE_SETS() {
        }
        
        #endregion IConstructors
        
    }
    
    /// <summary>the id of the tagged component for code-set</summary>
    public sealed class ALTERNATE_IIOP_ADDRESS {
        
        #region Constants
        
        public const int ConstVal = 3;
        
        #endregion Constants
        #region IConstructors
        
        private ALTERNATE_IIOP_ADDRESS() {
        }
        
        #endregion IConstructors
        
    }

    /// <summary>the id of the tagged component for code-set</summary>
    public sealed class TAG_SSL_SEC_TRANS {
        
        #region Constants
        
        public const int ConstVal = 20;
        
        #endregion Constants
        #region IConstructors
        
        private TAG_SSL_SEC_TRANS() {
        }
        
        #endregion IConstructors
        
    }
            
    [RepositoryID("IDL:omg.org/IOP/TaggedComponent:1.0")]
    [IdlStruct]
    [Serializable]
    [ExplicitSerializationOrdered()]
    public struct TaggedComponent {
    
        #region SFields
        
        public static readonly Type ClassType = typeof(TaggedComponent);
        
        #endregion SFields
        #region IFields
        
        [ExplicitSerializationOrderNr(0)]
        public int tag;
        [ExplicitSerializationOrderNr(1)]
        [IdlSequence(0L)]
        public byte[] component_data;
        
        #endregion IFields
        #region IConstructors
        
        public TaggedComponent(int tag, byte[] component_data) {
            this.tag = tag;
            this.component_data = component_data;
        }
        
        /// <summary>
        /// deserialise from input stream
        /// </summary>
        internal TaggedComponent(CdrInputStream inputStream) {
            this.tag = (int)inputStream.ReadULong();
            int componentDataLength = (int)inputStream.ReadULong();
            this.component_data = inputStream.ReadOpaque(componentDataLength);
        }
        
        #endregion IConstructors
        #region IMethods
        
        /// <summary>
        /// serialise the service context
        /// </summary>
        internal void Write(CdrOutputStream outputStream) {
            outputStream.WriteULong((uint)tag);
            outputStream.WriteULong((uint)component_data.Length);
            outputStream.WriteOpaque(component_data);
        }
        
        #endregion IMethods
        
    }
    
    
    /// <summary>
    /// This class represents the collection of tagged components in an IOR
    /// </summary>    
    public class TaggedComponentList {
        
        #region SFields
        
        private static TaggedComponent[] s_emptyList = new TaggedComponent[0];
        
        #endregion SFields
        #region IFields
        
        private TaggedComponent[] m_components;
        
        #endregion IFields
        #region IConstructors
        
        internal TaggedComponentList(params TaggedComponent[] components) {
            m_components = new TaggedComponent[components.Length];
            Array.Copy(components, m_components, components.Length);
        }
        
        internal TaggedComponentList() {
            m_components = s_emptyList;
        }
        
        /// <summary>
        /// deserialise a service context from 
        /// </summary>
        /// <param name="inputStream"></param>
        internal TaggedComponentList(CdrInputStream inputStream) {
            ReadTaggedComponenttList(inputStream);
        }
        
        #endregion IConstructors
        #region IProperties
        
        public int Count {
            get {
                return m_components.Length;
            }
        }
        
        #endregion IProperties
        #region IMethods
        
        private void ReadTaggedComponenttList(CdrInputStream inputStream) {
            int nrOfComponents = (int)inputStream.ReadULong();
            m_components = new TaggedComponent[nrOfComponents];
            for (int i = 0; i < nrOfComponents; i++) {
                TaggedComponent component = new TaggedComponent(inputStream);
                m_components[i] = component;
            }
        }
        
        internal void WriteTaggedComponentList(CdrOutputStream outputStream) {
            outputStream.WriteULong((uint)m_components.Length);
            for (int i = 0; i < m_components.Length; i++) {                
                m_components[i].Write(outputStream);
            }
        }
        
        /// <summary>
        /// gets the component or null, if not found
        /// </summary>
        private object GetComponentInternal(int tag) {
            for (int i = 0; i < m_components.Length; i++) {
                if (m_components[i].tag == tag) {
                    return m_components[i];
                }
            }            
            return null;                        
        }
        
        /// <summary>
        /// is a tagged component present for the given id.
        /// </summary>
        public bool ContainsTaggedComponent(int tag) {
            return GetComponentInternal(tag) != null;
        }
               
        /// <summary>
        /// serialise the given component data and adds it to the list of components.
        /// The data is encoded in a cdr encapsulation.
        /// </summary>
        public TaggedComponent AddComponentWithData(int tag, object data, Codec codec) {
            if ((data == null) || (codec == null)) {
                throw new BAD_PARAM(80, CompletionStatus.Completed_MayBe);
            }                        
            TaggedComponent result = new TaggedComponent(tag, codec.encode_value(data));
            AddComponent(result);
            return result;
        }
        
        public void AddComponent(TaggedComponent component) {
            TaggedComponent[] resultEntries = new TaggedComponent[m_components.Length + 1];
            Array.Copy(m_components, resultEntries, m_components.Length);
            resultEntries[m_components.Length] = component;                
            m_components = resultEntries;            
        }
        
        public void AddComponents(TaggedComponent[] components) {
            if (components == null) {
                throw new BAD_PARAM(80, CompletionStatus.Completed_MayBe);
            }
            if (components.Length == 0) {
                return; // nothing to do.
            }
            TaggedComponent[] resultEntries = new TaggedComponent[m_components.Length + components.Length];
            Array.Copy(m_components, resultEntries, m_components.Length);
            Array.Copy(components, 0, resultEntries, m_components.Length, components.Length);
            m_components = resultEntries;            
        }
                                
        /// <summary>
        /// returns the deserialised data of the first component with the given tag or null, if not found.
        /// Assumes, that the componentData is encapsulated in a cdr encapsulation. The secound argument
        /// specifies, how the data inside the encapsulation looks like.
        /// </summary>
        public object GetComponentData(int tag, Codec codec, 
                                       omg.org.CORBA.TypeCode componentDataType) {
            object result = null;
            object resultComp = GetComponentInternal(tag);
            if (resultComp != null) {
                return codec.decode_value(((TaggedComponent)resultComp).component_data,
                                          componentDataType);
            }
            return result;
        }        
        
        /// <summary>
        /// returns the component with the given id. Throws a BAD_PARAM exception, if not found.
        /// </summary>        
        public TaggedComponent GetComponent(int tag) {
            object resultComp = GetComponentInternal(tag);
            if (resultComp != null) {
                return (TaggedComponent)resultComp;
            } else {
                throw new BAD_PARAM(75, CompletionStatus.Completed_MayBe);
            }
        }
        
        public TaggedComponent[] GetComponents(int tag) {
            ArrayList result = new ArrayList();
            for (int i = 0; i < m_components.Length; i++) {
                if (m_components[i].tag == tag) {
                    result.Add(m_components[i]);
                }
            }            
            return (TaggedComponent[])result.ToArray(TaggedComponent.ClassType);
        }
                
        #endregion IMethods
        
    }    
    
    
}
