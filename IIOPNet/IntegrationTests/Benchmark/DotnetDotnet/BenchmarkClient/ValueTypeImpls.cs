/* 
 * ValueTypeImpls.cs
 * 
 * Project: IIOP.NET
 * Benchmarks
 *
 * WHEN      RESPONSIBLE
 * 20.05.04  Patrik Reali (PRR), patrik.reali -at- elca.ch
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


namespace Ch.Elca.Iiop.Benchmarks {    
    
    
    [Serializable()]
    public class ValType1Impl : ValType1 {
        
        public ValType1Impl() {
        }
        
        public ValType1Impl(int v1, int v2, int v3) : this() {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }

    }       
    
    [Serializable()]
    public class ValType2Impl : ValType2 {
        
        public ValType2Impl() {
        }
        
        public ValType2Impl(bool repeat, int count, int v1, int v2, int v3) : this() {
            this.m_v1 = new ValType1[count];

            if (repeat) {
                ValType1 vt = new ValType1Impl(v1, v2, v3);
                for (int i = 0; i < count; i++) {
                    this.m_v1[i] = vt;
                }
            } else {
                for (int i = 0; i < count; i++) {
                    this.m_v1[i] = new ValType1Impl(v1, v2, v3);
                }
            }
        }        
        
    }
    
}
