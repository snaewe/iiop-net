/*
 * PerformanceCounter.cs
 *
 * Project: IIOP.NET
 * Benchmarks
 *
 * WHEN      RESPONSIBLE
 * 20.05.04  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Runtime.InteropServices ;

namespace Ch.Elca.Iiop.Benchmarks {

    /// <summary>
    /// PerformanceCounter class usage : 
    /// 
    /// PerformanceCounter counter = new PerformanceCounter();
    /// ...
    /// counter.Stop();
    /// 
    /// System.Console.WriteLine("measure time in second = " + counter.Difference.TotalSeconds);
    /// 
    /// </summary>
    public class PerformanceCounter {
        
        #region IFields
        
        private long m_frequency;
        
        private long m_startPoint;
        private long m_stopPoint;
        private bool m_isStarted = false;
        private bool m_isStartPointSet = false;
        private bool m_isStopPointSet = false;
        
        #endregion IFields

        #region IConstructors

        /// <summary>
        /// Constructor by default
        /// </summary>
        public PerformanceCounter() : this(true) {            
        }
        
        public PerformanceCounter(bool start) {            
            m_frequency = GetFrequency(); 
            if (start) {
                Start();
            }            
        }

        #endregion IConstructors
        #region IProperties
        
        /// <summary>the startPoint for this Counter</summary>
        public long StartPoint {
            get {
                if (m_isStartPointSet) {
                    return m_startPoint;
                } else {
                    throw CreateNoStartPointException();
                }
            }
        }
        
        /// <summary>gets current counter value - reference counter value
        public TimeSpan Difference {
            get {
                if (m_isStopPointSet) {
                    return CovertToTimeSpan(m_stopPoint - m_startPoint);
                } else {
                    throw CreateNoStopPointException();
                }
            }
        }        
                
        #endregion IProperties
        #region IMethods
        
        /// <summary>starts the counter</summary>
        public void Start() {
            if (!m_isStarted) {
                m_startPoint = GetCurrentCounterValue();
                m_isStarted = true;
                m_isStartPointSet = true;
                m_isStopPointSet = false;
            } else {
                throw CreateAlreadyStartedException();
            }            
        }
        
        public void Stop() {
            if (m_isStarted) {
                m_stopPoint = GetCurrentCounterValue();                
                m_isStarted = false;
                m_isStopPointSet = true;
            } else {
                throw CreateNotStartedException();
            }
        }
        
        private TimeSpan CovertToTimeSpan(long counterDifference) {
            // m_frequency are the number of ticks in one seconds;                         
            long timespanTicks = (long)(TimeSpan.TicksPerSecond * counterDifference / m_frequency);
            return new TimeSpan(timespanTicks);
        }        
        
        /// <summary>
        /// Get counter value
        /// </summary>
        /// <returns>Counter ticks</returns>
        private long GetCurrentCounterValue() {
            long counterValue ;
            QueryPerformanceCounter(out counterValue) ;
            return counterValue ;
        }

        /// <summary>
        /// Get frequency value
        /// </summary>
        /// <returns>Number of ticks for 1 second</returns>
        private long GetFrequency() {
            long frequencyValue ;
            QueryPerformanceFrequency(out frequencyValue) ;
            return frequencyValue ;
        }
        
        #region Exceptions
        
        private Exception CreateAlreadyStartedException() {
            return new InvalidOperationException("counter has already been started");
        }               
        
        private Exception CreateNotStartedException() {
            return new InvalidOperationException("counter has not been started");        
        }
        
        private Exception CreateNoStartPointException() {
            return new InvalidOperationException("counter has no start point");
        }                
        
        private Exception CreateNoStopPointException() {
            return new InvalidOperationException("counter has no stop point");
        }
        
        #endregion Exceptions
        
        #endregion IMethods
        #region SMethod

        /// <summary>
        /// QueryPerformanceCounter into kernel32 access
        /// </summary>
        [DllImport("kernel32.dll")]
        private static extern int QueryPerformanceCounter(out long lpPerformanceCount);

        /// <summary>
        /// QueryPerformanceFrequency into kernel32 access
        /// </summary>
        [DllImport("kernel32.dll")]
        private static extern int QueryPerformanceFrequency(out long lpFrequency);
        
        #endregion SMethod        
    }
    
}
