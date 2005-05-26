/* PICurrentImpl.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 23.04.05  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
 * 
 * Copyright 2005 Dominic Ullmann
 *
 * Copyright 2005 ELCA Informatique SA
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
using System.Threading;
using omg.org.CORBA;
using omg.org.PortableInterceptor;

namespace Ch.Elca.Iiop.Interception {

    
    /// <summary>
    /// manages PICurrents (thread and request scoped).
    /// </summary>
    internal class PICurrentManager {
        
        #region Constants
        
        private const int START_ID = 0;
        
        #endregion Constants
        #region SFields
               
        [ThreadStatic]
        private static PICurrentImpl t_threadScopePICurrent;
        
        #endregion SFields
        #region IFields
        
        private int m_nextIdToAllocate = START_ID;
        
        #endregion IFields
        #region IConstructors
        
        internal PICurrentManager() {            
        }
        
        #endregion IConstructors
        #region IMethods
        
        private int GetNrOfSlotsAllocated() {
            return m_nextIdToAllocate;
        }
        
        /// <summary>
        /// gets the current thread-scope PICurrent.
        /// </summary>
        /// <remarks>
        /// thread scope is managed inside a ThreadStatic member.
        /// Sadly using the .NET CallContext is not feasible, because model differs on the server side too much and
        /// is also not really specified in detail.
        /// </remarks>
        internal PICurrentImpl GetThreadScopedCurrent() {
            if (t_threadScopePICurrent == null) {
                t_threadScopePICurrent = new PICurrentImpl(GetNrOfSlotsAllocated());
            }
            return t_threadScopePICurrent;
        }
        
        /// <summary>
        /// copys the request-scope current to the thread-scope current.
        /// </summary>
        internal void SetFromRequestScope(PICurrentImpl requestCurrent) {
            PICurrentImpl threadScoped = GetThreadScopedCurrent();
            threadScoped.SetSlotsFrom(requestCurrent);            
        }
        
        /// <summary>
        /// creates a request scope PICurrent from the ThreadScope PICurrent.
        /// </summary>
        internal PICurrentImpl CreateRequestScopeFromThreadScope() {
            PICurrentImpl result = new PICurrentImpl(GetNrOfSlotsAllocated());
            result.SetSlotsFrom(GetThreadScopedCurrent());
            return result;
        }
        
        /// <summary>creates an empty request scope PICurrent</summary>
        internal PICurrentImpl CreateEmptyRequestScope() {
            PICurrentImpl result = new PICurrentImpl(GetNrOfSlotsAllocated());
            return result;
        }
        
        /// <summary>clears the thread context of all slot data.</summary>
        /// <remarks>e.g. used on the server side to remove all context data when the request
        /// has been processed.</remarks>
        internal void ClearThreadScope() {
            t_threadScopePICurrent = null;
        }        
                
        /// <summary>
        /// allocate a slot id.
        /// </summary>        
        internal int AllocateSlotId() {
            if (m_nextIdToAllocate < Int32.MaxValue) {
                int result = m_nextIdToAllocate;
                m_nextIdToAllocate++;
                return result;
            } else {
                throw new INTERNAL(2002, CompletionStatus.Completed_MayBe);
            }
        }
        
        #endregion IMethods
        
    }
    
           
    /// <summary>
    /// implementation of <see cref="omg.org.PortableInterceptor.Current"/>
    /// </summary>    
    internal class PICurrentImpl : omg.org.PortableInterceptor.Current {
        
        
        #region IFields
        
        private object[] m_slots;
        
        #endregion IFields
        #region IConstructors
        
        public PICurrentImpl(int nrOfSlots) {
            m_slots = new object[nrOfSlots];
        }                
        
        #endregion IConstructors       
        #region IMethods
               
        private void CheckSlotAllocated(int id) {
            if ((id < 0) || (id >= m_slots.Length)) {
                throw new InvalidSlot();
            } // else: ok
        }
               
        /// <summary>
        /// <see cref="omg.org.PortableInterceptor.Current.get_slot"/>
        /// </summary>        
        public object get_slot(int id) {
            CheckSlotAllocated(id);
            return m_slots[id];
        }

        /// <summary>
        /// <see cref="omg.org.PortableInterceptor.Current.set_slot"/>
        /// </summary>        
        public void set_slot(int id, object data) {
            CheckSlotAllocated(id);            
            m_slots[id] = data;
        }
            
        /// <summary>sets this picurrent data from another instance.</summary>
        internal void SetSlotsFrom(PICurrentImpl other) {
            if (other.m_slots.Length != m_slots.Length) {
                m_slots = new object[other.m_slots.Length];
            }
            for (int i = 0; i < m_slots.Length; i++) {
                m_slots[i] = other.m_slots[i];
            }
        }
        
                               
        #endregion IMethods
        
        
    }
            
    
}
