/* ValueStream.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 21.01.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.Diagnostics;
using Ch.Elca.Iiop.Marshalling;
using Ch.Elca.Iiop.Idl;
using Ch.Elca.Iiop.Util;
using Corba;
using omg.org.CORBA;

namespace Ch.Elca.Iiop.Cdr {
    

    /// <summary>contains information about a chunck</summary>
    internal class ChunkInfo {

        #region IFields

        /// <summary>the starting position in the stream</summary>
        private ulong m_streamStartPos;
        /// <summary>the stream, this chunk is in</summary>
        private CdrInputStreamImpl m_stream;

        private ulong m_chunkLength;
        private bool m_continuationExpected;
        private bool m_finished = false;

        #endregion IFields
        #region IConstructors
        
        /// <param name="length">the length of the chunk</param>
        /// <param name="inStream">the stream this chunk is in</param>
        public ChunkInfo(int length, CdrInputStreamImpl inStream) {
            m_chunkLength = (ulong)length;
            m_stream = inStream;
            m_streamStartPos = inStream.GetPosition();
        }

        #endregion IConstructors
        #region IProperties
        
        /// <summary>
        /// the length of this chunk
        /// </summary>
        public int ChunkLength {
            get { 
                return (int)m_chunkLength; 
            }
        }

        /// <summary>
        /// is the chunk finished?
        /// </summary>
        public bool IsFinished {
            get { 
                return m_finished; 
            }
            set {
                m_finished = value; 
            }
        }

        /// <summary>
        /// if this chunk is intercepted by an inner value, a continuation of this chunk is expected
        /// </summary>
        public bool IsContinuationExpected {
            get {
                return m_continuationExpected; 
            }
            set { 
                m_continuationExpected = value; 
            }
        }

        #endregion IProperties
        #region IMethods

        /// <summary>after reading a continuation chunk begin, this method is used to set the chunk length</summary>
        public void SetContinuationLength(int length) {
            m_chunkLength = (ulong)length;
            m_streamStartPos = m_stream.GetPosition();
        }

        public bool IsDataAvailable() {
            if (m_streamStartPos + m_chunkLength > m_stream.GetPosition()) { 
                return true; 
            } else {
                return false; 
            }
        }

        public bool IsBorderCrossed() {
            return (GetBytesAvailable() < 0);
        }
        
        public bool IsBorderReached() {
            return (GetBytesAvailable() == 0);
        }

        public int GetBytesAvailable() {
            return (int) (m_streamStartPos + m_chunkLength - m_stream.GetPosition());
        }
        
        #endregion IMethods

    }

    /// <summary>
    /// Base class for ValueInputStream and ValueOutputStream. 
    /// Provides some common functionality
    /// </summary>
    public abstract class ValueBaseStream {

        #region Constants
                
        protected const uint MIN_VALUE_TAG = 0x7fffff00;
        protected const uint MAX_VALUE_TAG = 0x7fffffff;

        #endregion Constants
        #region IFields

        /// <summary>this stack holds the chunking information</summary>
        protected Stack m_chunkStack = new Stack();

        #endregion IFields
        #region IMethods

        /// <summary>
        /// creates a Stack with the inheritance information for the Type forType.
        /// </summary>
        protected Stack CreateTypeHirarchyStack(Type forType) {
            Stack typeHierarchy = new Stack();
            Type currentType = forType;
            while (currentType != null) {
                if (!IsImplClass(currentType)) { // ignore impl-classes in serialization code
                    typeHierarchy.Push(currentType);
                    if (CheckForCustomMarshalled(currentType)) { 
                        break; 
                    }
                }

                currentType = currentType.BaseType;
                if (currentType == typeof(System.Object) || currentType == typeof(System.ValueType) ||
                   (ClsToIdlMapper.IsMappedToAbstractValueType(currentType, 
                                                               new AttributeExtCollection()))) { // abstract value types are not serialized
                    break;                
                }
            }
            return typeHierarchy;
        }

        /// <summary>checks, if the type is an implementation of a value-type</summary>
        /// <remarks>fields of implementation classes are not serialized/deserialized</remarks>
        protected bool IsImplClass(Type forType) {
            Type baseType = forType.BaseType;
            if (baseType != null) {
                AttributeExtCollection attrs = AttributeExtCollection.ConvertToAttributeCollection(
                                                    baseType.GetCustomAttributes(false));
                if (attrs.IsInCollection(typeof(ImplClassAttribute))) {
                    ImplClassAttribute implAttr = (ImplClassAttribute)
                                                  attrs.GetAttributeForType(typeof(ImplClassAttribute));
                    Type implClass = Repository.LoadType(implAttr.ImplClass);
                    if (implClass == null) {                        
                        Trace.WriteLine("implementation class : " + implAttr.ImplClass + 
                                    " of value-type: " + baseType + " couldn't be found");
                        throw new NO_IMPLEMENT(1, CompletionStatus.Completed_MayBe);
                    }
                    if (implClass.Equals(forType)) {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>checks, if custom marshalling must be used</summary>
        protected bool CheckForCustomMarshalled(Type forType) {
            if (typeof(ICustomMarshalled).IsAssignableFrom(forType)) { // subclasses of a custom marshalled type are automatically also custom marshalled: CORBA-spec-99-10-07: page 3-27
                return true; 
            } else {
                return false;
            }
        }                

        #endregion IMethods
    
    }


    /// <summary>
    /// the valueStream is a CDR-stream for reading seriaized value-types
    /// </summary>
    public class ValueInputStream :  ValueBaseStream, CdrInputStream {

        #region IFields

        private CdrInputStreamImpl m_baseStream;

        #endregion IFields
        #region IProperties

        public uint CharSet {
            get { 
                throw new NotSupportedException("not supported for value-stream");
            }
            set { 
                throw new NotSupportedException("not supported for value-stream");
            }
        }

        public uint WCharSet {
            get {
                throw new NotSupportedException("not supported for value-stream");
            }
            set {
                throw new NotSupportedException("not supported for value-stream");
            }
        }

        #endregion IProperties
        #region IConstructors
        
        public ValueInputStream(CdrInputStreamImpl baseStream) {
            m_baseStream = baseStream;
            Debug.WriteLine("postion when value-inp-stream was constructed: " + m_baseStream.GetPosition());
        }

        #endregion IConstructors
        #region IMethods

        // delegate the allowed standard operation to the base-stream
        #region Implementation of CDRInputStream
        public byte ReadOctet() {
            byte result = m_baseStream.ReadOctet();
            CheckPosition();
            return result;
        }

        public bool ReadBool() {
            bool result = m_baseStream.ReadBool();
            CheckPosition();
            return result;
        }

        public char ReadWChar() {
            char result = m_baseStream.ReadWChar();
            CheckPosition();
            return result;
        }

        public char ReadChar(){
            char result = m_baseStream.ReadChar();
            CheckPosition();
            return result;
        }

        public string ReadString() {
            string result = m_baseStream.ReadString();
            CheckPosition();
            return result;
        }

        public string ReadWString() {
            string result = m_baseStream.ReadWString();
            CheckPosition();
            return result;
        }

        public byte[] ReadOpaque(int nrOfBytes) {
            byte[] result = m_baseStream.ReadOpaque(nrOfBytes);
            CheckPosition();
            return result;
        }

        public void ReadBytes(byte[] buf, int offset, int count) {
            m_baseStream.ReadBytes(buf, offset, count);
            CheckPosition();
        }

        public void ReadPadding(ulong nrOfBytes) {
            m_baseStream.ReadPadding(nrOfBytes);
            CheckPosition();
        }

        public void ForceReadAlign(Aligns align) {
            // the alignement is done for the basic reading operations on the underlying stream
            throw new NotSupportedException("this operation is not supported for value-input streams");
        }

        public CdrEncapsulationInputStream ReadEncapsulation() {
            CdrEncapsulationInputStream result = m_baseStream.ReadEncapsulation();
            CheckPosition();
            return result;
        }

        public void SkipRest() {
            // not useful for this stream
            throw new NotSupportedException("this operation is not supported for value-input streams");
        }
        #endregion Implementation of CDRInputStream

        #region Implementation of CDREndianDepInputStreamOp
        public short ReadShort() {
            short result = m_baseStream.ReadShort();
            CheckPosition();
            return result;
        }

        public ushort ReadUShort() {
            ushort result = m_baseStream.ReadUShort();
            CheckPosition();
            return result;
        }

        public int ReadLong() {
            int result = m_baseStream.ReadLong();
            CheckPosition();
            return result;
        }

        public uint ReadULong() {
            uint result = m_baseStream.ReadULong();
            CheckPosition();
            return result;
        }

        public long ReadLongLong() {
            long result = m_baseStream.ReadLongLong();
            CheckPosition();
            return result;
        }

        public ulong ReadULongLong() {
            ulong result = m_baseStream.ReadULongLong();
            CheckPosition();
            return result;
        }

        public float ReadFloat() {
            float result = m_baseStream.ReadFloat();
            CheckPosition();            
            return result;
        }

        public double ReadDouble() {
            double result = m_baseStream.ReadDouble();
            CheckPosition();
            return result;
        }
        #endregion Implementation of CDREndianDepInputStreamOp

        #region value-type specific operation

        /// <summary>check if a chunk is active</summary>
        /// <returns></returns>
        private bool IsChunkActive() {
            if (m_chunkStack.Count == 0) { return false; }
            ChunkInfo info = (ChunkInfo)m_chunkStack.Peek(); // chunks are not nested -> check only topmost
            if (info.IsContinuationExpected || info.IsFinished) { // not active
                // is continuationExpected means, that a chunk of a nested value type follows;
                // the current chunk is inactive up to end of inner value type chunk.
                return false;
            } else {
                return true;
            }
        }
        
        /// <summary>
        /// check, if the current position violates chunk constraints.
        /// See if new chunk starts, if last ended at current position.
        /// </summary>
        private void CheckPosition() {
            if (IsChunkActive()) { // when a chunk is active, use chunk aware increment
                CheckChunkPosition();
            }
        }
        
        /// <summary>
        /// check chunk border, see if new chunk starts if on chunk border
        /// </summary>
        private void CheckChunkPosition() {
            if (m_chunkStack.Count == 0) { 
                return; // no chunking used -> return
            }
            ChunkInfo chunkInfo = (ChunkInfo)m_chunkStack.Peek();
            if (chunkInfo.IsBorderCrossed()) {
                // invlaid serialized value-type, try to read over the chunk border
                throw new MARSHAL(901, CompletionStatus.Completed_MayBe);
            }
            if (chunkInfo.IsBorderReached()) {
                // check if a new chunk starts here
                m_baseStream.StartPeeking(); // switch to peek mode
                int tag = m_baseStream.ReadLong();
                m_baseStream.StopPeeking(); // end peek mode
                if ((tag > 0) && (tag < ValueBaseStream.MIN_VALUE_TAG)) {
                    // a chunk starts here
                    m_baseStream.ReadLong(); // read start chunk
                    chunkInfo.IsContinuationExpected = false;
                    // set chunk to start after tag and contains tag bytes
                    chunkInfo.SetContinuationLength(tag);
                } else if ((tag >= ValueBaseStream.MIN_VALUE_TAG) && (tag <= ValueBaseStream.MAX_VALUE_TAG)) {
                    // a value type starting here -> current chunk is deactived while nested val type is read
                    chunkInfo.IsContinuationExpected = true;                    
                }                                
            }
        }
            
        /// <summary>reads in a field in a value-type instance</summary>
        /// <param name="containingInstance">the instance, in which the field should be set</param>
        public void ReadAndSetField(FieldInfo field, object containingInstance) {
            Marshaller marshaller = Marshaller.GetSingleton();
            AttributeExtCollection attrColl = AttributeExtCollection.ConvertToAttributeCollection(
                                                    field.GetCustomAttributes(true));
            
            // if an indirection tag is used, load the indirection
            long indirectionOffset;
            object result = null;
            // don't check indirection for non-value-types, else misunderstandings possible: int with same value as indirection tag would result in error
            if ((ClsToIdlMapper.IsDefaultMarshalByVal(field.FieldType)) &&
                (CheckForIndirection(out indirectionOffset))) { 
                // load the value for the indirection
                result = GetObjectForIndir(indirectionOffset, false,
                                           IndirectionType.IndirValue,
                                           IndirectionUsage.ValueType);
                // here a problem is possible: in the indirection table, the boxed from is present for boxed values, 
                // but for setting the field, the unboxed version may be needed
                if ((result is BoxedValueBase) && (!field.FieldType.IsSubclassOf(typeof(BoxedValueBase)))) {
                    result = ((BoxedValueBase)result).Unbox();
                }
                
            } else {
                result = marshaller.Unmarshal(field.FieldType, attrColl, this);
                // indirection table update not needed here: if a marshalled value type was read, it's already in the indirection table, primitive types / object references are not inserted into the indirection table (15.3.4.3 in CORBA 2.3.1 99-10-07)
            }
            field.SetValue(containingInstance, result);
        }

        /// <summary>reads in a whole value-type instance</summary>
        public object ReadValue(Type formal) {
            // a reference to the whole value is also possible for an indirection, prepare the position to add to indir table
            StreamPosition indirPos = new StreamPosition(m_baseStream, 
                                                         false); // indirPos will contain the next aligned position in the base stream, after next read-op is performed
            
            long indirOffset = 0;
            if (CheckForIndirection(out indirOffset)) { // check if an indirection will follow instead of a value
                return GetObjectForIndir(indirOffset, false,
                                         IndirectionType.IndirValue,
                                         IndirectionUsage.ValueType);
            }
            
            int valueTag = ReadLong();
            if (valueTag == 0) { 
                return null; 
            } // a null-value

            // handle codebase-url
            HandleCodeBaseURL(valueTag);

            // get the type of the value to deserialise
            Type actualType = GetActualType(formal, valueTag);

            // now the state data follows ...
            // create the instance of this value-type
            object instance = CreateInstance(actualType);
            if (!(formal.IsInstanceOfType(instance))) { 
                // invalid implementation class of value type: 
                // instance.GetType() is incompatible with: formal
                throw new BAD_PARAM(903, CompletionStatus.Completed_MayBe);
            }
            
            // add the instance, which is created at the moment to the indirection table
            StoreIndirection(new IndirectionInfo(indirPos.Position, 
                                                 IndirectionType.IndirValue,
                                                 IndirectionUsage.ValueType),
                             instance);

            bool chunked = false;
            int chunkLevel = 0;
            if ((valueTag & 0x00000008) > 0) { // chunking is used, a new chunk begins here
                chunked = true;
                
                m_baseStream.StartPeeking(); // switch to peek mode
                int tag = m_baseStream.ReadLong();
                m_baseStream.StopPeeking(); // end peek mode
                ChunkInfo info;
                if ((tag > 0) && (tag < ValueBaseStream.MIN_VALUE_TAG)) {
                    // a chunk starts here
                    m_baseStream.ReadLong(); // read start chunk
                    info = new ChunkInfo(tag, m_baseStream);
                    info.IsContinuationExpected = false;
                } else {
                    // deactivate chunk, inner value must follow
                    info = new ChunkInfo(0, m_baseStream);
                    info.IsContinuationExpected = true;
                }               
                // store chunk-info
                m_chunkStack.Push(info);
                chunkLevel = m_chunkStack.Count;
            }
            
            // set the fields: from the most basic type to the most derived type ...
            Stack typeHierarchy = CreateTypeHirarchyStack(actualType);
            while (typeHierarchy.Count > 0) {
                Type demarshalType = (Type) typeHierarchy.Pop();
                if (!CheckForCustomMarshalled(demarshalType)) {
                    ReadFieldsForType(instance, demarshalType, chunked);
                } else { // custom marshalled
                    if (!(instance is ICustomMarshalled)) { 
                        // can't deserialise custom value type, because ICustomMarshalled not implented
                        throw new INTERNAL(909, CompletionStatus.Completed_MayBe);
                    }
                    ((ICustomMarshalled) instance).Deserialise(new DataInputStreamImpl(this));
                }
            }
            
            // state data finished
            if (chunked) { 
                EndChunk(); 
                if (chunkLevel == 1) {
                    // outermost value: no chunks must be on the stack
                    if (m_chunkStack.Count > 0) { 
                        // not all chunks closed at the ending of the value-type
                        throw new MARSHAL(911, CompletionStatus.Completed_MayBe);
                    }
                }
            }
            
            return instance;
            
        }

        /// <summary>reads and sets the field declared in the type ofType.</summary>
        private void ReadFieldsForType(object instance, Type ofType, bool chunkedRep) {
            // reads all fields declared in the Type: no inherited fields
            FieldInfo[] fields = ofType.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo fieldInfo in fields) {
                if (!fieldInfo.IsNotSerialized) { // do not serialize transient fields
                    ReadAndSetField(fieldInfo, instance);
                }
            }
        }

        /// <summary>ends chunk(s) here</summary>
        private void EndChunk() {
            // end chunk(s) here
            ChunkInfo top = (ChunkInfo) m_chunkStack.Pop();
            if (top.IsFinished) {
                return; // more than one level was ended for an inner value-type --> this chunk is already finished
            }
            FinishChunk(top);
            CheckChunkInfoAtEnd(top); // check if chunk is completely read here
            top.IsFinished = true; // chunk is finished
            // read-endTag for this chunk and possibly for outer chunks
            int endTag = ReadLong(); // not part of chunk -> do not check if over border; IsFinished = true deactivated chunk border checking

            if (endTag >= 0) { 
                // end-tag for a chunk must be < 0
                throw new MARSHAL(914, CompletionStatus.Completed_MayBe);
            }
            int levelsToEnd = m_chunkStack.Count + 2 + endTag; // already removed topmost element --> add 2 here
            if (levelsToEnd <= 0) { 
                // invalid end-chunk tag
                throw new MARSHAL(915, CompletionStatus.Completed_MayBe);
            }
            // set for the chunks, that are not removed here the IsFinished property to true!
            IEnumerator enumerator = m_chunkStack.GetEnumerator();
            for (int i = 1; i < levelsToEnd; i++) {
                enumerator.MoveNext();
                CheckChunkInfoAtEnd((ChunkInfo) enumerator.Current); // check if chunk can end here!
                ((ChunkInfo) enumerator.Current).IsFinished = true;
            }
            if (enumerator.MoveNext()) {
                // was a nested value, continue chunk if no val type follows ...
                ChunkInfo continuation = (ChunkInfo) enumerator.Current;
                if (continuation.IsContinuationExpected) {
                    // check for non-val chunk start ...
                    m_baseStream.StartPeeking(); // switch to peek mode
                    int tag = m_baseStream.ReadLong();
                    m_baseStream.StopPeeking(); // end peek mode
                    if ((tag > 0) && (tag < ValueBaseStream.MIN_VALUE_TAG)) {
                        // a chunk starts here
                        m_baseStream.ReadLong(); // read start chunk                        
                        continuation.SetContinuationLength(tag); // set chunk length
                        continuation.IsContinuationExpected = false;
                    }
                }
            }
        }

        /// <summary>checks, if a chunk can end at the specified position</summary>
        private void CheckChunkInfoAtEnd(ChunkInfo chunkInfo) {
            if (chunkInfo.IsDataAvailable()) {
                // a chunk containing unread data couldn't be eneded here
                throw new MARSHAL(917, CompletionStatus.Completed_MayBe);
            }
        }

        /// <summary>read the unread data bytes of the chunk here</summary>
        /// <param name="chunk"></param>
        private void FinishChunk(ChunkInfo chunk) {
            int toRead =  chunk.GetBytesAvailable();
            if (toRead > 0) {
                ReadPadding((ulong)toRead);
            }
        }

        /// <summary>creates an instance of the given type via reflection</summary>
        private object CreateInstance(Type actualType) {
            object[] implAttr = actualType.GetCustomAttributes(typeof(ImplClassAttribute), false);
            if ((implAttr != null) && (implAttr.Length > 0)) {
                if (implAttr.Length > 1) { 
                    // invalid type: actualType, only one ImplClassAttribute allowed
                    throw new INTERNAL(923, CompletionStatus.Completed_MayBe);
                }
                ImplClassAttribute implCl = (ImplClassAttribute) implAttr[0];
                // get the type
                actualType = Repository.LoadType(implCl.ImplClass);
                if (actualType == null) { 
                    Trace.WriteLine("implementation class : " + implCl.ImplClass + 
                                    " of value-type: " + actualType + " couldn't be found");
                    throw new NO_IMPLEMENT(1, CompletionStatus.Completed_MayBe);
                }
            }
            // type must not be abstract for beeing instantiable
            if (actualType.IsAbstract) { 
                // value-type couln't be instantiated: actualType
                throw new NO_IMPLEMENT(931, CompletionStatus.Completed_MayBe);
            }
            // instantiate            
            object instance = Activator.CreateInstance(actualType);
            return instance;
        }

        /// <summary>
        /// gets the type of which the actual parameter is / should be ...
        /// </summary>
        private Type GetActualType(Type formal, int valueTag) {
            Type actualType = null;
            switch (valueTag & 0x00000006) {
                case 0: 
                    // actual = formal-type
                    actualType = formal;
                    break;
                case 2:
                    // single repository-id follows
                    long indirectionOffset;
                    string repId = "";
                    if (CheckForIndirection(out indirectionOffset)) {
                        repId = (string)GetObjectForIndir(indirectionOffset, false,
                                                          IndirectionType.IndirRepId,
                                                          IndirectionUsage.ValueType);
                    } else {
                        StreamPosition indirPos = new StreamPosition(m_baseStream, 
                                                                     false); // indirPos will contain the next aligned position in the base stream after next read-Op
                        repId = ReadString();
                        // add repository id to indirection table
                        StoreIndirection(new IndirectionInfo(indirPos.Position,
                                                             IndirectionType.IndirRepId,
                                                             IndirectionUsage.ValueType),
                                         repId);
                    }
                    actualType = Repository.GetTypeForId(repId);
                    if (actualType == null) { 
                        // repository id used is unknown: repId
                        throw new NO_IMPLEMENT(941, CompletionStatus.Completed_MayBe);
                    }
                    break;
                case 6:
                    // a list of repository-id's
                    int nrOfIds = ReadLong();
                    if (nrOfIds == 0) { 
                        // a list of repository-id's for type-information must contain at least one element
                        throw new MARSHAL(935, CompletionStatus.Completed_MayBe);
                    }
                    string mostDerived = ReadString(); // use only the most derived type, no truncation allowed
                    for (int i = 1; i < nrOfIds; i++) { 
                        ReadString(); 
                    }
                    actualType = Repository.GetTypeForId(mostDerived);
                    break;
                default:
                    // invalid value-tag found: " + valueTag
                    throw new MARSHAL(937, CompletionStatus.Completed_MayBe);
            }
            if (ClsToIdlMapper.IsInterface(actualType)) { 
                // can't instantiate value-type of type: actualType
                throw new NO_IMPLEMENT(945, CompletionStatus.Completed_MayBe);
            }
            return actualType;
        }

        /// <summary>reads codebase-URL</summary>
        private void HandleCodeBaseURL(long valueTag) {
            // codebase is not supported, but codebase-url must be read anyway
            if ((valueTag & 0x00000001) > 0) {
                long indirectionOffset;
                if (CheckForIndirection(out indirectionOffset)) {
                    CheckIndirectionResolvable(indirectionOffset, false, 
                                               IndirectionType.CodeBaseUrl,
                                               IndirectionUsage.ValueType);
                } else {
                    StreamPosition indirPos = new StreamPosition(m_baseStream,
                                                                 false); // indirPos will contain the next aligned position in the base stream, after next read-op is performed
                    string codeBaseURL = ReadString();    
                    // add codebase url to indirection table
                    StoreIndirection(new IndirectionInfo(indirPos.Position, 
                                                         IndirectionType.CodeBaseUrl,
                                                         IndirectionUsage.ValueType),
                                     codeBaseURL);
                }
            }
        }                

        #endregion
        
        public bool CheckForIndirection(out long indirectionOffset) {
            return m_baseStream.CheckForIndirection(out indirectionOffset);
        }
        
        public long ReadIndirection() {
            ReadPadding(4); // read indir tag
            return ReadLong();
        }
        
        public void StoreIndirection(IndirectionInfo info, object value) {
            m_baseStream.StoreIndirection(info, value);
        }
        
        /// <param name="resolveGlobal">must be false for ValueStream, because indirections are not resolved across encapsulation bounderies</param>
        public bool IsIndirectionResolvable(long indirOffset, bool resolveGlobal,
                                            IndirectionType indirType,
                                            IndirectionUsage indirUsage) {
            return m_baseStream.IsIndirectionResolvable(indirOffset, false,
                                                        indirType, indirUsage);
        }
        
        /// <param name="resolveGlobal">must be false for ValueStream, because indirections are not resolved across encapsulation bounderies</param>
        public void CheckIndirectionResolvable(long indirOffset, bool resolveGlobal,
                                               IndirectionType indirType,
                                               IndirectionUsage indirUsage) {
            m_baseStream.CheckIndirectionResolvable(indirOffset, false, indirType, indirUsage);                                                   
        }
        
        /// <param name="resolveGlobal">must be false for ValueStream, because indirections are not resolved across encapsulation bounderies</param>
        public object GetObjectForIndir(long indirOffset, bool resolveGlobal,
                                        IndirectionType indirType,
                                        IndirectionUsage indirUsage) {
            return m_baseStream.GetObjectForIndir(indirOffset, false, indirType, indirUsage);
        }
        
        public ulong GetPosition() {
            return m_baseStream.GetPosition();
        }
        
        public ulong GetGlobalOffset() {
            return m_baseStream.GetGlobalOffset();
        }
        
        public ulong GetNextAlignedPosition(Aligns align) {
            return m_baseStream.GetNextAlignedPosition(align);
        }
        
        public void MarkNextAlignedPosition(StreamPosition streamPosition) {
            m_baseStream.MarkNextAlignedPosition(streamPosition);
        }

        #endregion IMethods

    }


    /// <summary>
    /// the ValueOutputStream is a CDR-stream for writing seriaized value-types
    /// </summary>
    /// <remarks>this stream doesn't use chunking</remarks>
    public class ValueOutputStream :  ValueBaseStream, CdrOutputStream {

        #region IFields

        private CdrOutputStreamImpl m_baseStream;
        
        #endregion IFields
        #region IProperties

        public uint CharSet {
            get { 
                throw new NotSupportedException("not supported for value-stream");
            }
            set { 
                throw new NotSupportedException("not supported for value-stream");
            }
        }

        public uint WCharSet {
            get {
                throw new NotSupportedException("not supported for value-stream");
            }
            set {
                throw new NotSupportedException("not supported for value-stream");
            }
        }

        #endregion IProperties
        #region IConstructors

        public ValueOutputStream(CdrOutputStreamImpl baseStream) {
            m_baseStream = baseStream;
        }

        #endregion IConstructors
        #region IMethods

    
        #region Implementation of CDROutputStream
        
        public void WriteOctet(byte data) {
            m_baseStream.WriteOctet(data);
        }
        
        public void WriteBool(bool data) {
            m_baseStream.WriteBool(data);
        }
        
        public void WriteWChar(char data) {
            m_baseStream.WriteWChar(data);
        }
        
        public void WriteChar(char data) {
            m_baseStream.WriteChar(data);
        }
        
        public void WriteString(string data) {
            m_baseStream.WriteString(data);
        }
        
        public void WriteWString(string data) {
            m_baseStream.WriteWString(data);
        }
        
        public void WriteOpaque(byte[] data) {
            m_baseStream.WriteOpaque(data);
        }
        
        public void WriteBytes(byte[] data, int offset, int count) {
            m_baseStream.WriteBytes(data, offset, count);
        }
        
        public void ForceWriteAlign(Aligns align) {
            throw new NotSupportedException("this operation is not supported for ValueOutput-streams");
        }
        
        public void WritePadding(ulong nrOfBytes) {
            m_baseStream.WritePadding(nrOfBytes);
        }
        
        public void WriteEncapsulation(CdrEncapsulationOutputStream encap) {
            m_baseStream.WriteEncapsulation(encap);
        }
    
        #endregion

        #region Implementation of CDREndianDepOutputStreamOp
        public void WriteShort(short data) {
            m_baseStream.WriteShort(data);
        }

        public void WriteUShort(ushort data) {
            m_baseStream.WriteUShort(data);
        }

        public void WriteLong(int data) {
            m_baseStream.WriteLong(data);
        }

        public void WriteULong(uint data) {
            m_baseStream.WriteULong(data);
        }

        public void WriteLongLong(long data) {
            m_baseStream.WriteLongLong(data);
        }

        public void WriteULongLong(ulong data) {
            m_baseStream.WriteULongLong(data);
        }

        public void WriteFloat(float data) {
            m_baseStream.WriteFloat(data);
        }

        public void WriteDouble(double data) {
            m_baseStream.WriteDouble(data);
        }
        #endregion

        /// <summary>
        /// write a value to the stream
        /// </summary>
        /// <param name="instance">the instance to write</param>
        /// <param name="formal">the formal parameter (field or parameter type)</param>
        public void WriteValue(object instance, Type formal) {    
            if (instance == null) { 
                WriteULong(0); // write a null-value
                return;
            }
            
            // if value is already in indirection table, write indirection
            if (IsPreviouslyMarshalled(instance, 
                                       IndirectionType.IndirValue, IndirectionUsage.ValueType)) {
                // write indirection
                WriteIndirection(instance);
                return; // write completed
            }

            // prepare to add instance to write to indirection table
            StreamPosition indirPos = new StreamPosition(m_baseStream); // indirPos will contain the next aligned position in the base stream, after next write-op is performed
            
            uint valueTag = ValueBaseStream.MIN_VALUE_TAG; // value-tag with no option set
            WriteTagAndTypeInformation(formal, instance, valueTag);

            // add instance to indirection table
            StoreIndirection(instance, 
                             new IndirectionInfo(indirPos.Position, IndirectionType.IndirValue,
                                                 IndirectionUsage.ValueType));

            Stack typeHierarchy = CreateTypeHirarchyStack(instance.GetType());
            while (typeHierarchy.Count > 0) {
                Type marshalType = (Type) typeHierarchy.Pop();
                if (!CheckForCustomMarshalled(marshalType)) {
                    WriteFieldsForType(instance, marshalType);    
                } else { // custom marshalled
                    if (!(instance is ICustomMarshalled)) {
                        // can't serialise custom value type, because ICustomMarshalled not implemented
                        throw new INTERNAL(909, CompletionStatus.Completed_MayBe);
                    }
                    ((ICustomMarshalled) instance).Serialize(new DataOutputStreamImpl(this));
                }
            }
        }

        /// <summary>writes the fields delcared in the type ofType of the instance instance</summary>
        /// <param name="chunkedRep">use chunked representation</param>
        private void WriteFieldsForType(object instance, Type ofType) {
            FieldInfo[] fields = ofType.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo fieldInfo in fields) {
                if (!fieldInfo.IsNotSerialized) { // do not serialize transient fields
                    WriteField(fieldInfo, instance);
                }
            }
        }

        /// <summary>write the value of the field to the underlying stream</summary>
        private void WriteField(FieldInfo field, object instance) {
            Marshaller marshaller = Marshaller.GetSingleton();
            AttributeExtCollection attrColl = AttributeExtCollection.ConvertToAttributeCollection(
                                                    field.GetCustomAttributes(true));
            
            object fieldVal = field.GetValue(instance);
            // check if indirection must be used
            if (fieldVal != null && IsPreviouslyMarshalled(fieldVal, IndirectionType.IndirValue,
                                                           IndirectionUsage.ValueType)) {
                // write indirection
                WriteIndirection(fieldVal);
            } else {
                // write value                
                marshaller.Marshal(field.FieldType, attrColl, fieldVal, this);
                // indirection table update not needed here: if a marshalled value type was read, it's already in the indirection table, primitive types / object references are not inserted into the indirection table (15.3.4.3 in CORBA 2.3.1 99-10-07)
            }    
        }

        /// <summary>writes the type information to the stream</summary>
        /// <param name="valueTag">the valuetag without type information bits set</param>
        private void WriteTagAndTypeInformation(Type formal, object instance, uint valueTag) {
            // attentition here: if formal type represents an IDL abstract interface, writing no type information is not ok.
            // do not use no typing information option, because java orb can't handle it
            valueTag = valueTag | 0x00000002;
            WriteULong(valueTag);
            string repId = "";
            if (!IsImplClass(instance.GetType())) {
                repId = Repository.GetRepositoryID(instance.GetType());
            } else { // an impl-class is not serialized, because it's not known at the receiving ORB
                repId = Repository.GetRepositoryID(instance.GetType().BaseType);
            }
            if (IsPreviouslyMarshalled(repId, 
                                       IndirectionType.IndirRepId, IndirectionUsage.ValueType)) {        
                // write indirection
                WriteIndirection(repId);
            } else {            
                // prepare to add repId to indirection table
                StreamPosition indirPos = new StreamPosition(m_baseStream); // indirPos will contain the next aligned position in the base stream, after next write-op is performed
                WriteString(repId);
                
                IndirectionInfo indirInfo = new IndirectionInfo(indirPos.Position,
                                                                IndirectionType.IndirRepId,
                                                                IndirectionUsage.ValueType);
                StoreIndirection(repId, indirInfo);
            }
        }
        
        public void WriteIndirection(object forVal) {
            object indirInfo = m_baseStream.GetIndirectionInfoFor(forVal);
            if (indirInfo != null) {                
                WriteULong(CdrStreamHelper.INDIRECTION_TAG);         
                // remark: indirection offset must be calculated after indir tag has been written!
                int indirOffset = (int)CalculateIndirectionOffset((IndirectionInfo)indirInfo);
                WriteLong(indirOffset); // write the nr of bytes the value is before the current position                
            } else {
                throw m_baseStream.CreateWriteInexistentIndirectionException();
            }
        }
                
        public bool IsPreviouslyMarshalled(object val, IndirectionType indirType, 
                                           IndirectionUsage indirUsage) {
            return m_baseStream.IsPreviouslyMarshalled(val, indirType, indirUsage);                                       
        }
        
        
        public void StoreIndirection(object val, IndirectionInfo indirInfo) {
            m_baseStream.StoreIndirection(val, indirInfo);
        }                
        
        public long CalculateIndirectionOffset(IndirectionInfo indirInfo) {
            return m_baseStream.CalculateIndirectionOffset(indirInfo);
        }
        
        public ulong GetPosition() {
            return m_baseStream.GetPosition();
        }
        
        public ulong GetNextAlignedPosition(Aligns align) {
            return m_baseStream.GetNextAlignedPosition(align);
        }
        
        public void MarkNextAlignedPosition(StreamPosition streamPosition) {
            m_baseStream.MarkNextAlignedPosition(streamPosition);
        }
        
        #endregion IMethods
    }

}
