/* BoxedArrayHelper.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 09.02.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using omg.org.CORBA;
using Ch.Elca.Iiop.Util;

namespace Ch.Elca.Iiop.Idl {
    
    public sealed class BoxedArrayHelper {
        
        #region Constants
        
        public const string BOX_ONEDIM_ARRAY_METHODNAME = "BoxOneDimArray";
        
        #endregion Constants
        #region IConstructors

        private BoxedArrayHelper() {
        }

        #endregion IConstructors
        #region SMethods

        /// <summary>
        /// returns the boxed content for a boxed value type, if a one dimensional array of a non-array
        /// or of an array type should be boxed
        /// </summary>
        /// <remarks>
        /// This method is called via reflection
        /// </remarks>
        public static object BoxOneDimArray(Type valueBoxType, object arrayToBox) {
            if (!(arrayToBox is Array)) { 
                // invalid argument type: arrayToBox.GetType(), expected: System.Array
                throw new INTERNAL(30009, CompletionStatus.Completed_MayBe);
            }
            Array array = (Array) arrayToBox;

            object boxContent = null; // the boxed content for the valueBox
            if (array == null) {
                // array: can't box null
                throw new BAD_PARAM(30010, CompletionStatus.Completed_MayBe);
            } else if (array.Rank > 1) {
                // array: only one dim-arrays are supported (array of array of array is also possible, but not true moredim)
                throw new INTERNAL(30010, CompletionStatus.Completed_MayBe);
            }
            // array is ok
            Type boxed = null;
            try {
                boxed = (Type)valueBoxType.InvokeMember(BoxedValueBase.GET_BOXED_TYPE_METHOD_NAME,
                                                        BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly,
                                                        null, null, new object[0]);
            } catch (Exception) {
                // invalid type: valueBoxType static method missing or not callable: BoxedValueBase.GET_BOXED_TYPE_METHOD_NAME
                throw new INTERNAL(30011, CompletionStatus.Completed_MayBe);
            }
            if (array.GetType().GetElementType().IsArray) {
                boxContent = Array.CreateInstance(boxed.GetElementType(), array.Length);
                for (int i = 0; i < array.Length; i++) {
                    // box array elements of the array of array
                    object boxedElement = null;
                    if ((Array)array.GetValue(i) != null) {
                        object boxedElementContent = BoxOneDimArray(boxed.GetElementType(), (Array)array.GetValue(i));
                        boxedElement = Activator.CreateInstance(boxed.GetElementType(), new object[] { boxedElementContent });
                    }                    
                    ((Array)boxContent).SetValue(boxedElement, i);
                }
            } else if (!boxed.GetElementType().IsSubclassOf(ReflectionHelper.BoxedValueBaseType)) {
                // array elements are non boxed / no arrays
                boxContent = array;
            } else {
                // array elements are boxed values --> box the values here
                boxContent = Array.CreateInstance(boxed.GetElementType(), array.Length);
                for (int i = 0; i < array.Length; i++) {
                    object boxedElement = Activator.CreateInstance(boxed.GetElementType(), new object[] { array.GetValue(i) });
                    ((Array)boxContent).SetValue(boxedElement, i);
                }
            }
            return boxContent;
        }
        
        /// <summary>converts a true more dimensional array to an array of array of array of ...</summary>
        public static Array ConvertMoreDimToNestedOneDimChecked(object toConv) {
            if ((!toConv.GetType().IsArray) && (!(toConv.GetType().GetArrayRank() > 1))) {
                throw new BAD_PARAM(9004, CompletionStatus.Completed_MayBe);
            }
            return ConvertMoreDimToNestedOneDim((Array)toConv);
        }
        
        /// <summary>converts a true more dimensional array to an array of array of array of ...</summary>
        private static Array ConvertMoreDimToNestedOneDim(Array toConv) {
            if (toConv == null) { 
                return null; 
            } else if (toConv.Rank <= 1) { 
                return toConv;
            }
            
            // need to convert
            int[] newLength = new int[toConv.Rank - 1];
            for (int i = 1; i < toConv.Rank; i++) {
                newLength[i - 1] = toConv.GetLength(i);
            }
            // create the type of the new array elements
            Type resultType = CreateNestedOneDimType(toConv);
            Array convertedArray = Array.CreateInstance(resultType.GetElementType(), toConv.GetLength(0));
            // create the components, then recursive convert the components
            for (int i = 0; i < toConv.GetLength(0); i++) {
                Array partialArray = Array.CreateInstance(toConv.GetType().GetElementType(), newLength);
                FillPartialArray(partialArray, toConv, i);
                Array convPartial = ConvertMoreDimToNestedOneDim(partialArray);
                convertedArray.SetValue(convPartial, i);
            }
            return convertedArray;
        }
        
        /// <summary>converts an array of array of ... to a true more dimensional array</summary>
        public static Array ConvertNestedOneDimToMoreDimChecked(object toConv) {
            if (!toConv.GetType().IsArray) {
                throw new BAD_PARAM(9004, CompletionStatus.Completed_MayBe);
            }
            return ConvertNestedOneDimToMoreDim((Array)toConv);
        }
        
        /// <summary>converts an array of array of ... to a true more dimensional array</summary>
        private static Array ConvertNestedOneDimToMoreDim(Array toConv) {
            if (toConv == null) { 
                return null; 
            } else if (toConv.Rank > 1) {
                 // more-dimensional array not allowed
                 throw new INTERNAL(10039, CompletionStatus.Completed_MayBe);
            }
            // need to convert
            int nestLevel = DetermineNestLevel(toConv);
            if (nestLevel == 1) { 
                return toConv; 
            } // nothing to do
            
            // create an array of the length of the dimensions smaller than the biggest one
            int[] newLength = new int[nestLevel - 1];
            // create the type of the new array elements
            Type innermostElemType = DetermineInnermostArrayElemType(toConv.GetType());
            Type partialArrayType = Array.CreateInstance(innermostElemType, newLength).GetType();
            // create array of arrays of one smaller dimension, this must be finally converted to a true multi-dim
            Array arrayOfArray = Array.CreateInstance(partialArrayType, toConv.Length);
            // convert the arrays to more dim arrays
            for (int i = 0; i < toConv.Length; i++) {
                arrayOfArray.SetValue(ConvertNestedOneDimToMoreDim((Array)toConv.GetValue(i)), i);
            }
            // create resulting array
            return ConvertArrayOfArrayToMDArray(arrayOfArray);
        }

        /// <summary>determines the innermost array type</summary>
        private static Type DetermineInnermostArrayElemType(Type arrayType) {
            Type elemType = arrayType.GetElementType();
            while (elemType.IsArray) {
                elemType = elemType.GetElementType();
            }
            return elemType;
        }
        
        /// <summary>create the Type for the nested one dim array which is created out of the moredim array</summary>
        private static Type CreateNestedOneDimType(Array moreDimArray) {
            return CreateNestedOneDimType(moreDimArray.GetType());
        }

        /// <summary>create the Type for the nested one dim array which is created out of the moredim array</summary>        
        internal static Type CreateNestedOneDimType(Type arrayType) {
            Type elemType = arrayType.GetElementType();
            for (int i = 0; i < arrayType.GetArrayRank(); i++) {
                Array tmpArr = Array.CreateInstance(elemType, 0);
                elemType = tmpArr.GetType();
            }
            return elemType;            
        }
        
        /// <summary>determine the number of nests of a nested one-dim array, e.g. int[] -> 1, int[][] -> 2 </summary>
        private static int DetermineNestLevel(Array nestedArray) {
            Type elemType = nestedArray.GetType().GetElementType();
            int level = 1;
            while (elemType.IsArray) {
                elemType = elemType.GetElementType();
                level++;
            }
            return level;
        }
        
        /// <summary>fills in the values from wholeArray into partialArray for removedDimension index forIndexRemovedDimension</summary>
        /// <param name="partialArray">the target</param>
        /// <param name="wholeArray">the source</param>
        /// <param name="forindexRemovedDimension">the index in the first dimension</param>
        private static void FillPartialArray(Array partialArray, Array wholeArray, int forindexRemovedDimension) {
            FillElements(partialArray, wholeArray, new int[0], forindexRemovedDimension);
        }

        /// <summary>fill in elements recursively</summary>
        /// <param name="partialArray"></param>
        /// <param name="wholeArray"></param>
        /// <param name="ind"></param>
        private static void FillElements(Array partialArray, Array wholeArray, int[] ind, int removedDimIndex) {
            if (ind.Length == partialArray.GetType().GetArrayRank()) {    
                int[] fullInd = new int[ind.Length + 1];
                Array.Copy((Array)ind, 0, (Array)fullInd, 1, ind.Length);
                fullInd[0] = removedDimIndex;
                object srcVal = wholeArray.GetValue(fullInd);
                partialArray.SetValue(srcVal, ind);
            } else {
                // create new array of fixed indizes for copying
                int[] newInd = new int[ind.Length + 1];
                Array.Copy((Array)ind, 0, (Array)newInd, 0, ind.Length);
                // fix next dimensionsion for copying and copy
                int elemsInDim = partialArray.GetLength(ind.Length);
                for (int i = 0; i < elemsInDim; i++) {
                    newInd[ind.Length] = i;
                    FillElements(partialArray, wholeArray, newInd, removedDimIndex);
                }
            }
        }
        
        /// <summary>convert an arrayOfArray to a true more-dimensional array</summary>
        /// <param name="arrOfArr">the array of arrays of one smaller dimension</param>
        private static Array ConvertArrayOfArrayToMDArray(Array arrOfArr) {
            Type arrOfArrType = arrOfArr.GetType();
            int firstDimLength = arrOfArr.Length; // one dim array
            // create the length of the dimension of the resulting array
            Type elementType = arrOfArrType.GetElementType();
            // length of the dimension of the result array
            int resultDim = 1 + elementType.GetArrayRank();
            int[] resultLength = new int[resultDim];
            resultLength[0] = firstDimLength;
            int[] elemLength = DetermineDimensionLengthOfArrayElems(arrOfArr);
            Array.Copy((Array)elemLength, 0, (Array)resultLength, 1, elemLength.Length);
            // create the resulting array
            Array result = Array.CreateInstance(elementType.GetElementType(), resultLength);
            // fill in values
            for (int i = 0; i < arrOfArr.Length; i++) {
                CopyToArray((Array)arrOfArr.GetValue(i), result, new int[0], i);
            }
            return result;
        }

        
        private static int[] DetermineDimensionLengthOfArrayElems(Array arrOfArr) {
            if (arrOfArr.Length == 0) { 
                return new int[arrOfArr.GetType().GetElementType().GetArrayRank()]; 
            }
            int[] result = FillLengthArray(((Array)arrOfArr.GetValue(0)));
            for (int i = 1; i < arrOfArr.Length; i++) {
                // check if other elements are of the same dimensions
                int[] thisElemDim = FillLengthArray((Array)arrOfArr.GetValue(i));
                for (int j = 0; j < thisElemDim.Length; j++) {
                    if (thisElemDim[j] != result[j]) { 
                        throw new MARSHAL(10037, CompletionStatus.Completed_MayBe); 
                    }
                }
            }
            return result;
        }

        /// <summary>creates an array of the length of the dimensions aof the Array arrayElem</summary>
        private static int[] FillLengthArray(Array arrayElem) {
            // a null element is not allowed here, because in a matrix, no part should be missing
            if (arrayElem == null) { 
                throw new MARSHAL(10038, CompletionStatus.Completed_MayBe); 
            }
            int rank = arrayElem.GetType().GetArrayRank();
            int[] result = new int[rank];
            for (int i = 0; i < rank; i++) {
                result[i] = arrayElem.GetLength(i);
            }
            return result;
        }

        private static void CopyToArray(Array arrOfArrElem, Array result, int[] ind, int firstDim) {
            if (ind.Length == result.GetType().GetArrayRank() - 1) { // index determined in the higher dims fully determined, together with firstDim index: copy possible
                object val = arrOfArrElem.GetValue(ind);
                int[] fullIndex = new int[result.GetType().GetArrayRank()];
                Array.Copy((Array)ind, 0, (Array)fullIndex, 1, ind.Length);
                fullIndex[0] = firstDim;
                result.SetValue(val, fullIndex);
            } else {
                // create new array of fixed indizes for copying
                int[] newInd = new int[ind.Length + 1];
                Array.Copy((Array)ind, 0, (Array)newInd, 0, ind.Length);
                // fix next dimensionsion for copying and copy
                int elemsInDim = arrOfArrElem.GetLength(ind.Length);
                for (int i = 0; i < elemsInDim; i++) {
                    newInd[ind.Length] = i;
                    CopyToArray(arrOfArrElem, result, newInd, firstDim);
                }
            }
        }

        #endregion SMethods
        
    }
    
}
