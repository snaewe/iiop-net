/* TestServiceCommon.cs
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
using Ch.Elca.Iiop.Idl;

namespace Ch.Elca.Iiop.Benchmarks {

    public enum TestEnum {
        A, B, C, D
    }

    [Serializable]
    public struct TestStructA {
        public System.Int32 X;
        public System.Int32 Y;
    }


    public class RefType: MarshalByRefObject {
    }


    [Serializable]
    public class ValType1 {
        public int v1;
        public int v2;
        public int v3;

        public ValType1 () {
        }

        public ValType1(int v1, int v2, int v3) {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }
    }

    [Serializable]
    public class ValType2 {
        
        ValType1[]  v1;

        public ValType2 () {
        }

        public ValType2(bool repeat, int count, int v1, int v2, int v3) {
            this.v1 = new ValType1[count];

            if (repeat) {
                ValType1 vt = new ValType1(v1, v2, v3);
                for (int i = 0; i < count; i++) {
                    this.v1[i] = vt;
                }
            } else {
                for (int i = 0; i < count; i++) {
                    this.v1[i] = new ValType1(v1, v2, v3);
                }
            }
        }
    }

    public interface TestService {
        void Void();
        void VI(int i);
        void VII(int i, int j);
        void VIIIII(int i, int j, int k, int l, int m);

        int II(int i);

        int IIIIII(int i, int j, int k, int l, int m);

        void VRef(RefType rt);
        RefType RefLocal();
        RefType RefRef(RefType rt); // return remote
        RefType RefRefLocal(RefType rt); // return local

        ValType1 Val1();
        ValType1 Val1Val1(ValType1 vt);
        void VVal1(ValType1 vt);

        ValType2 Val2(bool repeat);
        ValType2 Val2Val2(ValType2 vt);
        void VVal2(ValType2 vt);

        double[] DoulbeArrCreate(int nrOfElems);
        double[] DoubleArrEcho(double[] arg);
        int DoubleArrCountElems(double[] arg);
                
    }

}

