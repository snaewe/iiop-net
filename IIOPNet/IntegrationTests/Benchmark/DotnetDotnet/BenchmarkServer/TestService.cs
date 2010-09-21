/* TestService.cs
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

    [SupportedInterfaceAttribute(typeof(TestService))]
    public class TestServiceImpl : MarshalByRefObject, TestService {

        RefType m_rt = new RefType();

        public void Void() {
            // do nothing
        }

        public void VI(int i) {
            // do nothing
        }

        public void VII(int i, int j) {
            // do nothing
        }

        public void VIIIII(int i, int j, int k, int l, int m) {
            // do nothing
        }

        public int II(int i) {
            return i;
        }

        public int IIIIII(int i, int j, int k, int l, int m) {
            return i+j+k+l+m;
        }

        [return: StringValue]
        public string StSt([StringValue] string a) {
            return a;
        }

        [return: StringValue]
        public string StStStSt([StringValue] string a, [StringValue] string b, 
                               [StringValue] string c) {
            return a + b + c;
        }

        public void VD(double i) {
            // do nothing
        }

        public double DDDDDD(double i, double j, double k, double l, double m) {
            return i + j + k + l + m;
        }        

        public void VRef(RefType rt) {
            // do nothing
        }

        public RefType RefLocal() {
            return m_rt;
        }

        public RefType RefRef(RefType rt) {
            return rt;
        }

        public RefType RefRefLocal(RefType rt) {
            return m_rt;
        }

        public ValType1 Val1() {
            return new ValType1(1, 3, 4);
        }

        public ValType1 Val1Val1(ValType1 vt) {
            return vt;
        }

        public void VVal1(ValType1 vt) {
            // do nothing
        }

        public ValType2 Val2(bool repeat) {
            return new ValType2(repeat, 100, 1, 3, 4);
        }

        public ValType2 Val2Val2(ValType2 vt) {
            return vt;
        }

        public void VVal2(ValType2 vt) {
            // do nothing
        }

        public double[] DoulbeArrCreate(int nrOfElems) {
            return new double[nrOfElems];
        }

        public double[] DoubleArrEcho(double[] arg) {
            return arg;
        }
        
        public int DoubleArrCountElems(double[] arg) {
            return arg.Length;
        }

        [return: IdlSequence(0L)]
        public double[] DoubleIdlSeqEcho([IdlSequence(0L)] double[] arg) {
            return arg;
        }

        [return: IdlSequence(0L)]
        public int[] IntIdlSeqEcho([IdlSequence(0L)] int[] arg) {
            return arg;
        }

        public IdlStructA EchoStruct(IdlStructA arg) {
            return arg;
        }

        [return: IdlSequence(0L)]
        public IdlStructA[] EchoStructSeq(IdlStructA[] arg) {
            return arg;
        }

        [return: IdlSequence(0L)]
        public object[] EchoAnySeq(object[] arg) {
            return arg;
        }

        public EnumA EchoEnum(EnumA arg) {
            return arg;
        }

        [return: IdlSequence(0L)]
        public EnumA[] EnumIdlSeqEcho([IdlSequence(0L)] EnumA[] arg) {
            return arg;
        }
        
        [return: IdlArray(0L, 500)]
        [return: IdlArrayDimension(0L, 1, 3)]
        public System.Int32[,] IdlLongArray5times3Echo([IdlArray(0L, 500)][IdlArrayDimension(0L, 1, 3)] System.Int32[,] arg) {
            return arg;
        }
        
        [return: IdlArray(0L, 40)]
        [return: IdlArrayDimension(0L, 1, 400000)]
        public System.Single[,] IdlFloatArray40times400000Echo([IdlArray(0L, 40)][IdlArrayDimension(0L, 1, 400000)] System.Single[,] arg) {
            return arg;
        }

        [return: IdlSequence(0L)]
        public System.Single[] SingleIdlSeqEcho([IdlSequence(0L)] System.Single[] arg) {
            return arg;
        }

        [return: IdlSequence(0L)]
        public System.Byte[] ByteIdlSeqEcho([IdlSequence(0L)] System.Byte[] arg) {
            return arg;
        }


        public override object InitializeLifetimeService() {
            // live forever
            return null;
        }

    }

}
