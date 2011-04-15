/* CDRStream.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 14.01.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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
using System.IO;
using System.Text;
using System.Collections;
using System.Diagnostics;
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Util;
using Ch.Elca.Iiop.Services;
using Ch.Elca.Iiop.CodeSet;
using omg.org.CORBA;

namespace Ch.Elca.Iiop.Cdr {
    
    /// <summary>
    /// alignements possible in CDRStreams
    /// </summary>
    public enum Aligns : byte {
        Align2 = 2,
        Align4 = 4,
        Align8 = 8
    }
    

    /// <summary>the type of the entity pointed to by the indirection</summary>
    public enum IndirectionType {
        IndirRepId,
        IndirValue,
        CodeBaseUrl,
        TypeCode
    }
    
    
    /// <summary>specifies, by what facility using indirections, the
    /// indirection was created; needed because only the same usage is allowed (see 15.3.4.3)
    /// </summary>
    public enum IndirectionUsage {
        ValueType,
        TypeCode
    }

    /// <summary>
    /// stores information about indirections; indirections are used in value type and typecode
    /// encodings.
    /// </summary>
    [CLSCompliant(false)]
    public class IndirectionInfo {
        
        #region IFields
        
        private uint m_streamPos;
        private IndirectionType m_indirType;
        private IndirectionUsage m_indirUsage;

        #endregion IFields
        #region IConstructors
        
        internal IndirectionInfo(uint streamPos, IndirectionType indirType,
                                 IndirectionUsage indirUsage) {
            m_streamPos = streamPos;
            m_indirType = indirType;
            m_indirUsage = indirUsage;
        }

        #endregion IConstructors
        #region IProperties

        public uint StreamPos {
            get {
                return m_streamPos;
            }
        }

        public IndirectionType IndirType {
            get {
                return m_indirType;
            }
        }
        
        public IndirectionUsage IndirUsage {
            get {
                return m_indirUsage;
            }
        }

        #endregion IProperties
        #region IMethods
        
        public override bool Equals(object other) {
            if (!(other is IndirectionInfo)) {
                return false;
            }
            IndirectionInfo otherInfo = (IndirectionInfo)other;
            return ((StreamPos == otherInfo.StreamPos) &&
                    (IndirType == otherInfo.IndirType) &&
                    (IndirUsage == otherInfo.IndirUsage));
        }
        
        public override int GetHashCode() {
            return (StreamPos.GetHashCode() ^
                    IndirType.GetHashCode() ^
                    IndirUsage.GetHashCode());
        }
        
        #endregion IMethods

    }
    
    public class IndirectionStoreBase {
        
        #region IFields
        
        private IDictionary m_indirections = new Hashtable();
        
        private uint m_lastEncapsulationBoundry = 0;
        
        #endregion IFields
        #region IConstructors
        
        internal IndirectionStoreBase() {
        }
        
        #endregion IConsturctors
        #region IProperties
        
        protected IDictionary Store {
            get {
                return m_indirections;
            }
        }
        
        #endregion IProperties
        #region IMethods
        
        /// <summary>is the specified position before the last encapsulation boundry</summary>
        internal bool IsEncapBoundryCrossed(uint position) {
            return position < m_lastEncapsulationBoundry;
        }
        
        /// <summary>sets the last encapsulation boundry position</summary>
        internal void SetLastEncapBoundry(uint position) {
            m_lastEncapsulationBoundry = position;
        }
        
        #endregion IMethods
        
    }
    
    /// <summary>an indirection store, storing indirections for values</summary>
    public class IndirectionStoreValueKey : IndirectionStoreBase {
    
        #region IMethods
        
        /// <summary>returns true, if the value has been previously marshalled to the stream</summary>
        internal bool IsPreviouslyMarshalled(object val, IndirectionType indirType,
                                             IndirectionUsage indirUsage) {
            object indirInfo = GetIndirectionInfoFor(val);
            return ((indirInfo != null) && (((IndirectionInfo)indirInfo).IndirType == indirType) &&
                    (((IndirectionInfo)indirInfo).IndirUsage == indirUsage));
        }
        
        /// <summary>stores an indirection for the val at the next aligned position</summary>
        internal void StoreIndirection(object val, IndirectionInfo indirDesc) {
             Store[val] = indirDesc;
        }
        
        internal IndirectionInfo GetIndirectionInfoFor(object forVal) {
            return (IndirectionInfo)Store[forVal];
        }
                
        #endregion IMethods
    
    }
    
    /// <summary>an indirection store, storing values for indirections</summary>
    public class IndirectionStoreIndirKey : IndirectionStoreBase {
    
        #region IMethods
        
        internal void StoreIndirection(IndirectionInfo indirDesc,
                                       object valueAtIndirPos) {
            Store[indirDesc] = valueAtIndirPos;
        }
        
        /// <summary>resolves indirection, if not possible throws marshal excpetion</summary>
        internal object GetObjectForIndir(IndirectionInfo indirDesc, bool allowEncapBoundryCross) {
            if (!IsIndirectionResolvable(indirDesc, allowEncapBoundryCross)) {
                // indirection not resolvable!
                throw CreateIndirectionNotResolvableException();
            }
            return Store[indirDesc];
        }
   
        internal bool IsIndirectionResolvable(IndirectionInfo indirInfo, bool allowEncapBoundryCross) {
            if (!Store.Contains(indirInfo)) {
                Debug.WriteLine("indirection not found, streamPos: " + indirInfo.StreamPos +
                                ", type: " + indirInfo.IndirType +
                                ", usage: " + indirInfo.IndirUsage);
                IEnumerator enumerator = Store.Keys.GetEnumerator();
                while (enumerator.MoveNext()) {
                    IndirectionInfo infoEntry = (IndirectionInfo) enumerator.Current;
                    Debug.WriteLine(infoEntry + ", pos: " + infoEntry.StreamPos + ", type: " +
                                    infoEntry.IndirType);
                    Debug.WriteLine("value for key: " + Store[infoEntry]);
                }
                return false;
            } else {
                Debug.WriteLine("indirection resolved, streamPos: " + indirInfo.StreamPos +
                                ", type: " + indirInfo.IndirType +
                                ", usage: " + indirInfo.IndirUsage);
                if ((!allowEncapBoundryCross) && (IsEncapBoundryCrossed(indirInfo.StreamPos))) {
                    throw CreateIndirectionBoundryCrossException();
                }
                return true;
            }
        }
        
        internal void CheckIndirectionResolvable(IndirectionInfo indirInfo, bool allowEncapBoundryCross) {
            if (!IsIndirectionResolvable(indirInfo, allowEncapBoundryCross)) {
                throw CreateIndirectionNotResolvableException();
            }
        }
        
        private Exception CreateIndirectionNotResolvableException() {
            return new MARSHAL(951, CompletionStatus.Completed_MayBe);
        }
        
        private Exception CreateIndirectionBoundryCrossException() {
            return new MARSHAL(961, CompletionStatus.Completed_MayBe);
        }
        
        #endregion IMethods
    
    }
    
    /// <summary>
    /// this class is a helper class, which holds stream positions
    /// </summary>
    [CLSCompliant(false)]
    public class StreamPosition {
     
        #region IFields
 
        private uint m_position = 0;
        private uint m_globalOffset = 0;
 
        #endregion IFields
        #region IConstructors
         
        public StreamPosition(CdrStreamBase stream) : this (stream, 0, false) {
        }

        /// <summary>
        /// constructs the stream position from the position in the currentstream + adding offsetFromCurrent.
        /// </summary>
        /// <param name="stream">the stream to get positon from</param>
        /// <param name="offsetFromCurrent">the offset to add to the current position</param>
        /// <param name="isNegativeOffset">specifies, if the offset should be in positive or negative direction</param>
        public StreamPosition(CdrStreamBase stream, uint offsetFromCurrent, bool isNegativeOffset) {
            m_globalOffset = stream.GetGlobalOffset();
            m_position = stream.GetPosition();
            if (isNegativeOffset) {
                m_position -= offsetFromCurrent;
            } else {
                m_position += offsetFromCurrent;
            }
        }
 
        #endregion IConstructors
        #region IProperties
       
        /// <summary>
        /// returns the position relative to the beginning of the stream used to create the postion.
        /// </summary>
        /// <value></value>
        public uint LocalPosition {
            get {
                return m_position;
            }
        }

        /// <summary>
        /// returns the global position (i.e. relative to the outermost stream)
        /// </summary>
        public uint GlobalPosition {
            get {
                return m_globalOffset + m_position;
            }
        }
 
        #endregion IProperties
 
    }

    /// <summary>base interface for cdr input and output streams</summary>
    [CLSCompliant(false)]
    public interface CdrStreamBase {
        
        #region IProperties

        /// <summary>the charset to use</summary>
        /// <remarks>
        /// The CORBA standards uses LATIN1 charset, if no charset is specified
        /// in IOR.
        /// </remarks>
        int CharSet {
            get;
            set;
        }
        
        /// <summary>the wcharset to use</summary>
        /// <remarks>
        /// The CORBA standard defines, that there is no default.
        /// If this is not set, serializing/deserializing a wchar/wstring
        /// is not allowed.
        /// </remarks>
        int WCharSet {
            get;
            set;
        }
        
        #endregion IProperties
        #region IMethods
        
        /// <summary>
        /// the current position in the stream relative to the beginning of the stream;
        /// </summary>
        uint GetPosition();
        
        /// <summary>for the outermost (the marshal stream) this returns 0.
        /// For streams embedded in othter streams, this returns the position of the current stream
        /// relative to the outermost stream beginning; e.g. used together with encapsulations</summary>
        uint GetGlobalOffset();

        /// <summary>gets the next aligned position in the stream</summary>
        uint GetNextAlignedPosition(Aligns align);
        
        #endregion IMethods

    }
    

    /// <summary>
    /// this interfaces describes the methods for reading from streams containing CDR-data, which are different for big-endian / little-endian
    /// </summary>
    /// <remarks>
    /// this interface is not intended for CDR-stream users
    /// </remarks>
    [CLSCompliant(false)]
    public interface CdrEndianDepInputStreamOp {
    
        #region IMethods
        
        /// <summary>reads a short from the stream</summary>
        short ReadShort();
        /// <summary>reads an unsigned short from the stream</summary>
        ushort ReadUShort();
        /// <summary>reads a long from the stream</summary>
        int ReadLong();
        /// <summary>reads an unsigned long from the stream</summary>
        uint ReadULong();
        /// <summary>reads a long long from the stream</summary>
        long ReadLongLong();
        /// <summary>reads an unsigned long long from the stream</summary>
        ulong ReadULongLong();
        /// <summary>reads a float from the stream</summary>
        float ReadFloat();
        /// <summary>reads a double from the stream</summary>
        double ReadDouble();
        /// <summary>reads a wide character from the stream</summary>
        /// <remarks>this is endian dependant, in contrast to char: char is an array of byte, wchar is one fixed size mulitbyte value</remarks>
        char ReadWChar();
        /// <summary>reads a wide string from the stream</summary>
        /// <remarks>this is endian depandant, in contrast to string</remarks>
        string ReadWString();
        /// <summary>reads multiple elements of primitive type to array</summary>
        /// <param name="arrayType">Type of array</param>
        /// <param name="elemCount">Number of elements to read</param>
        T[] ReadPrimitiveTypeArray<T>(int elemCount);
        
        #endregion IMethods

    }
    
    
    /// <summary>
    /// this interface describes the mehtods, which are available on streams containing CDR-data for reading
    /// </summary>
    [CLSCompliant(false)]
    public interface CdrInputStream : CdrEndianDepInputStreamOp, CdrStreamBase {
        
        #region IMethods
        
        /// <summary>reads an octet from the stream</summary>
        byte ReadOctet();
        /// <summary>reads a boolean from the stream</summary>
        bool ReadBool();
        /// <summary>reads a character from the stream</summary>
        char ReadChar();
        /// <summary>reads a string from the stream</summary>
        string ReadString();
        /// <summary>reads nrOfBytes from the stream</summary>
        byte[] ReadOpaque(int nrOfBytes);

        void ReadBytes(byte[] buf, int offset, int count);

        void ReadPadding(uint nrOfBytes);

        /// <summary>
        /// forces the alignement on a boundary. Is for example useful in IIOP 1.2, where a request/response body
        /// must be 8-aligned.
        /// </summary>
        /// <param name="align">the requested Alignement</param>
        void ForceReadAlign(Aligns align);

        /// <summary>
        /// forces the alignement on a boundary. Is for example useful in IIOP 1.2, where a request/response body
        /// must be 8-aligned. This method doesn't throw an exception in conrast to ForceReadAlign,
        /// if not enough data in the stream.
        /// </summary>
        /// <param name="align">the requested Alignement</param>
        bool TryForceReadAlign(Aligns align);

        /// <summary>reads an encapsulation from this stream</summary>
        CdrEncapsulationInputStream ReadEncapsulation();

        /// <summary>skip the remaining bytes in the message</summary>
        /// <remarks>throws an exception if length not known</remarks>
        void SkipRest();
                
        /// <summary>
        /// reads the indirection info from the stream.
        /// </summary>
        /// <returns>the position, the indirection is pointing to</returns>
        StreamPosition ReadIndirectionOffset();
        
        ///<summary>stores an indirection described by indirDesc</summary>
        void StoreIndirection(IndirectionInfo indirDesc, object valueAtIndirPos);
        
        /// <summary>get the indirection matching the given desc;
        /// throws MarshalException, if not present</summary>
        /// <param name="reolveGlobal">specify, if indirection should be reolved
        /// relative to the local stream or to the outermost (global) stream</param>
        object GetObjectForIndir(IndirectionInfo indirInfo, bool resolveGlobal);
        
        /// <summary>
        /// reads an indirection tag or instance start tag and returns:
        /// - the tag read (either indirection or instance tag)
        /// - the stream position just before the tag (out-param instanceStartPosition)
        /// - a bool specifying, if a indirection tag has been read
        /// </summary>
        /// <returns></returns>
        uint ReadInstanceOrIndirectionTag(out StreamPosition instanceStartPosition,
                                          out bool isIndirection);

        /// <summary>
        /// read a string, which may be indirected; the arguments indirType, indirUsage, resolveGlobal are
        /// used for resolving an indirection.
        /// </summary>
        string ReadIndirectableString(IndirectionType indirType, IndirectionUsage indirUsage,
                                      bool resolveGlobal);

        /// <summary>
        /// tells the stream, that a new valuetype is starting now
        /// </summary>
        /// <remarks>needed to handle chunking correctly</remarks>
        void BeginReadNewValue();

        /// <summary>
        /// starts the body of a value type, which started with tag valueTag.
        /// </summary>
        /// <param name="valueTag">the tag, which started the value type</param>
        void BeginReadValueBody(uint valueTag);

        /// <summary>
        /// ends reading the value type
        /// </summary>
        void EndReadValue(uint valueTag);

        #endregion IMethods

    }

    /// <summary>
    /// this interfaces describes the methods for writing to streams containing CDR-data, which are different for big-endian / little-endian
    /// </summary>
    /// <remarks>
    /// this interface is not intended for CDR-stream users
    /// </remarks>
    [CLSCompliant(false)]
    public interface CdrEndianDepOutputStreamOp {
        
        #region IMethods
        
        /// <summary>writes a short to the stream</summary>
        void WriteShort(short data);
        /// <summary>writes an unsigned short to the stream</summary>
        void WriteUShort(ushort data);
        /// <summary>writes a long to the stream</summary>
        void WriteLong(int data);
        /// <summary>writes an unsigned long to the stream</summary>
        void WriteULong(uint data);
        /// <summary>writes a long long to the stream</summary>
        void WriteLongLong(long data);
        /// <summary>writes an unsigned long long to the stream</summary>
        void WriteULongLong(ulong data);
        /// <summary>writes a float to the stream</summary>
        void WriteFloat(float data);
        /// <summary>writes a double to the stream</summary>
        void WriteDouble(double data);
        
        /// <summary>writes multiple elements of primitive type</summary>
        /// <param name="data">array of elements to write to the stream</param>
        void WritePrimitiveTypeArray<T>(T[] data);
        
        /// <summary>writes a wide character to the stream</summary>
        /// <remarks>
        /// this is endian depdendant in contrast to char:
        /// char is an array of byte, wchar is one fixed size mulitbyte value
        /// </remarks>
        void WriteWChar(char data);
        /// <summary>writes a wstring to the stream</summary>
        /// <remarks>this is endian dependant in contrast to string</remarks>
        void WriteWString(string data);

        #endregion IMethods

    }

    /// <summary>
    /// this interface describes the methods, which are available on streams containing CDR-data for writeing
    /// </summary>
    [CLSCompliant(false)]
    public interface CdrOutputStream : CdrEndianDepOutputStreamOp, CdrStreamBase {
        
        #region IMethods

        /// <summary>writes an octet to the stream</summary>
        void WriteOctet(byte data);
        /// <summary>writes a boolean to the stream</summary>
        void WriteBool(bool data);
        /// <summary>writes a character to the stream</summary>
        void WriteChar(char data);
        /// <summary>writes a string to the stream</summary>
        void WriteString(string data);

        /// <summary>writes opaque data to the stream</summary>
        void WriteOpaque(byte[] data);

        void WriteBytes(byte[] data, int offset, int count);

        /// <summary>
        /// forces the alignement on a boundary. Is for example useful in IIOP 1.2, where a request/response body
        /// must be 8-aligned.
        /// </summary>
        /// <param name="align">the requested Alignement</param>
        void ForceWriteAlign(Aligns align);

        /// <summary>writes a nr of padding bytes</summary>
        void WritePadding(uint nrOfBytes);

        /// <summary>writes an encapsulation to this stream</summary>
        void WriteEncapsulation(CdrEncapsulationOutputStream encap);

        /// <summary>
        /// writes a tag for an instance, which is indirectable;
        /// returns the StreamPosition to store for the instance for later indirections.
        /// </summary>
        /// <remarks>if an direction should be written, use WriteIndirection</remarks>
        /// <param name="tag">the tag to write; must be != INDIRECTION_TAG</param>
        StreamPosition WriteIndirectableInstanceTag(uint tag);

        /// <summary>
        /// writes either an indirection to a string or the string value itself, if not previously marshalled.
        /// </summary>
        /// <param name="val">the string to write;
        /// if already marshalled, write a indirection to the already marshalled string</param>
        /// <param name="indirType">a value describing how the string is used</param>
        /// <param name="indirUsage">a value describing by whom the string is used</param>
        void WriteIndirectableString(string val, IndirectionType indirType, IndirectionUsage indirUsage);

        /// <summary>wirtes the indirection to the stream</summary>
        void WriteIndirection(object forVal);
        
        /// <summary>returns true, if the value has been previously marshalled to the stream</summary>
        bool IsPreviouslyMarshalled(object val, IndirectionType indirType,
                                    IndirectionUsage indirUsage);
        
        /// <summary>stores an indirection for the val at the next aligned position</summary>
        void StoreIndirection(object val, IndirectionInfo indirDesc);
        
        /// <summary>calculates the indirection offset for the given indirInfo;
        /// this method assumes, that the current stream position is directly after the
        /// indirection tag!
        /// !!! before indirection tag is difficult, because of alignement: don't really know,
        /// how much alignement bytes are before indirection tag</summary>
        long CalculateIndirectionOffset(IndirectionInfo indirInfo);
        
        /// <summary>gets the indirection info stored for the value or null if not present</summary>
        IndirectionInfo GetIndirectionInfoFor(object forVal);

        /// <summary>
        /// gets stream flags
        /// </summary>
        byte Flags { get; }

        #endregion IMethods
    }
       
    /// <summary>
    /// this class represents a stream for writing a message to an underlaying stream
    /// </summary>
    internal class CdrMessageOutputStream {

        #region IFields
        
        private CdrOutputStreamImpl m_stream;
        private MemoryStream m_buffer;
        private CdrOutputStream m_contentStream;
        private GiopHeader m_header;

        #endregion IFields
        #region IConstructors
        
        internal CdrMessageOutputStream(Stream stream, GiopHeader header) {
            m_stream = new CdrOutputStreamImpl(stream, header.GiopFlags, header.Version);
            m_buffer = new MemoryStream();
            m_contentStream = new CdrOutputStreamImpl(m_buffer, header.GiopFlags, header.Version, GiopHeader.HEADER_LENGTH);
            m_header = header;
        }

        #endregion IConstructors
        #region IProperties
        
        /// <summary>
        /// the stream, this message output stream writes to, i.e. the stream
        /// passed in as argument.
        /// </summary>
        internal Stream BackingStream {
            get {
                return m_stream.BackingStream;
            }
        }
        
        /// <summary>
        /// The giop header used for this stream.
        /// </summary>
        internal GiopHeader Header {
            get {
                return m_header;
            }
        }
        
        #endregion IProperties
        #region IMethods

        /// <summary>get a CDROutputStream for writing the content of the message</summary>
        internal CdrOutputStream GetMessageContentWritingStream() {
            return m_contentStream;
        }

        /// <summary>
        /// this operation closes the output stream for the message content and writes the whole message
        /// to the underlaying stream.
        /// After this operation, the stream is not further usable.
        /// </summary>
        internal void CloseStream() {
            m_buffer.Seek(0, SeekOrigin.Begin);
            // write header
            m_header.WriteToStream(m_stream, (uint)m_buffer.Length);
            // write content
            m_stream.WriteBytes(m_buffer.GetBuffer(), 0, (int)m_buffer.Length);
            m_buffer.Close();
            m_contentStream = null;
        }

        #endregion IMethods

    }
    
    /// <summary>
    /// this class represents a stream for reading a message from an underlaying stream
    /// </summary>
    internal class CdrMessageInputStream {

        #region IFields

        private CdrInputStreamImpl m_inputStream;
        private GiopHeader m_header;

        #endregion IFields
        #region IConstructors
        
        internal CdrMessageInputStream(Stream stream) {
            m_inputStream = new CdrInputStreamImpl(stream);
            // read the header, this sets the big/little endian implementation and bytesToFollow
            m_header = new GiopHeader(m_inputStream);
        }

        #endregion IConstructors
        #region IProperties

        /// <summary>
        /// the stream, this message output stream writes to, i.e. the stream
        /// passed in as argument.
        /// </summary>
        internal Stream BackingStream {
            get {
                return m_inputStream.BackingStream;
            }
        }
        
        internal GiopHeader Header {
            get {
                return m_header;
            }
        }
        
        #endregion IProperties
        #region IMethods
        
        /// <summary>
        /// get a CDRInputStream for reading the content of the message
        /// </summary>
        /// <returns></returns>
        internal CdrInputStream GetMessageContentReadingStream() {
            return m_inputStream;
        }

        #endregion IMethods
    }

       
    /// <summary>
    /// this class provide some helper methods to CDRStream implementation
    /// </summary>
    [CLSCompliant(false)]
    public abstract class CdrStreamHelper : CdrStreamBase {
        
        #region Constants
        
        internal const uint INDIRECTION_TAG = 0xffffffff;
        internal const uint MIN_VALUE_TAG = 0x7fffff00;
        internal const uint MAX_VALUE_TAG = 0x7fffffff;
        
        private const int WCHARSET_NOT_SET = -1;

        #endregion Constants
        #region IFields

        /// <summary>the underlying stream</summary>
        private Stream m_stream;

        private uint m_index = 0;

        // default for this is latin1
        private int m_charSet = (int)Ch.Elca.Iiop.Services.CharSet.LATIN1;
        // no default for this available
        private int m_wcharSet = WCHARSET_NOT_SET;
        
        #endregion IFields
        #region IConstructors
        
        protected CdrStreamHelper(Stream stream) {
            m_stream = stream;
        }

        /// <summary>for inheritors only</summary>
        protected CdrStreamHelper() {
        }

        #endregion IConstructors
        #region IProperties

        protected Stream BaseStream {
            get {
                return m_stream;
            }
        }

        public int CharSet {
            get {
                return m_charSet;
            }
            set {
                m_charSet = value;
            }
        }

        public int WCharSet {
            get {
                return m_wcharSet;
            }
            set {
                m_wcharSet = value;
            }
        }

        #endregion IProperties
        #region IMethods
        
        /// <summary>for inheritors not using the public constructor</summary>
        protected virtual void SetStream(Stream stream) {
            m_stream = stream;
        }

        /// <summary>gets the nr of bytes which are missing to next aligned position</summary>
        protected uint GetAlignBytes(byte requiredAlignement) {
            uint alignBytes = GetAlignedBytesInternal(requiredAlignement);
            return alignBytes;
        }
        
        private uint GetAlignedBytesInternal(byte requiredAlignement) {
            // nr of bytes the index is after the last aligned index
            uint afterLastAlign = m_index % requiredAlignement;
            uint alignBytes = 0;
            if (afterLastAlign != 0) {
                alignBytes = (requiredAlignement - afterLastAlign);
            }
            return alignBytes;
        }

        /// <summary>update the bookkeeping</summary>
        /// <param name="bytes"></param>
        protected void IncrementPosition(uint bytes) {
            m_index += bytes;
        }
        public uint GetPosition() {
            return m_index;
        }
        
        public virtual uint GetGlobalOffset() {
            return 0;
        }

        public uint GetNextAlignedPosition(Aligns align) {
            return GetPosition() + GetAlignedBytesInternal((byte)align);
        }

        protected void SetPosition(uint position) {
            m_index = position;
        }

        /// <summary>determines, if big/little endian should be used</summary>
        /// <returns>true for little endian, false for big endian</returns>
        protected bool ParseEndianFlag(byte flag) {
            return ((flag & 0x01) > 0);
        }
        
        #endregion IMethods
    }

    /// <summary>contains information about a chunck</summary>
    internal class ChunkInfo {

        #region IFields

        /// <summary>the starting position in the stream</summary>
        private uint m_streamStartPos;
        /// <summary>the stream, this chunk is in</summary>
        private CdrInputStreamImpl m_stream;

        private uint m_chunkLength;
        private bool m_continuationExpected;
        private bool m_finished = false;

        #endregion IFields
        #region IConstructors

        /// <param name="length">the length of the chunk</param>
        /// <param name="inStream">the stream this chunk is in</param>
        internal ChunkInfo(uint length, CdrInputStreamImpl inStream) {
            m_chunkLength = length;
            m_stream = inStream;
            m_streamStartPos = inStream.GetPosition();
        }

        #endregion IConstructors
        #region IProperties

        /// <summary>
        /// the length of this chunk
        /// </summary>
        internal uint ChunkLength {
            get {
                return m_chunkLength;
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
        internal void SetContinuationLength(uint length) {
            m_chunkLength = length;
            m_streamStartPos = m_stream.GetPosition();
        }

        internal bool IsDataAvailable() {
            if (m_streamStartPos + m_chunkLength > m_stream.GetPosition()) {
                return true;
            } else {
                return false;
            }
        }

        internal bool IsBorderCrossed() {
            return (GetBytesAvailable() < 0);
        }

        internal bool WillBorderBeCrossed(int nrOfBytesToRead) {
            return ((GetBytesAvailable() - nrOfBytesToRead) < 0);
        }

        internal bool IsBorderReached() {
            return (GetBytesAvailable() == 0);
        }

        internal uint GetBytesAvailable() {
            return (m_streamStartPos + m_chunkLength - m_stream.GetPosition());
        }

        #endregion IMethods

    }

    /// <summary>the base class for streams, reading CDR data</summary>
    [CLSCompliant(false)]
    public class CdrInputStreamImpl : CdrStreamHelper, CdrInputStream {
        
        #region SFields

        private static CdrEndianOpNotSpecified s_endianNotSpec = new CdrEndianOpNotSpecified();

        #endregion SFields
        #region IFields
        
        private object m_version = null;
        private CdrEndianDepInputStreamOp m_endianOp = s_endianNotSpec;

        private bool m_bytesToFollowSet = false;
        private uint m_bytesToFollow = 0;
        /// <summary>this is the position, when bytes to follow was set</summary>
        private uint m_indexForBytesToF = 0;

        private uint m_startPeekPosition = 0;
        
        private Stream m_backingStream = null;
        
        /// <summary>used to store indirections encountered in this stream</summary>
        private IndirectionStoreIndirKey m_indirections = new IndirectionStoreIndirKey();

        /// <summary>this stack holds the chunking information</summary>
        private Stack m_chunkStack = new Stack();
        private bool m_skipChunkCheck = false;
        private int m_chunkLevel = 0;

        
        #endregion IFields
        #region IConstructors

        public CdrInputStreamImpl(Stream stream) : base() {
            SetStream(stream);
        }

        /// <summary>for inheritors only</summary>
        protected CdrInputStreamImpl() : base() {
        }
        
        /// <summary>for inheritors only</summary>
        protected CdrInputStreamImpl(IndirectionStoreIndirKey indirStore) : this() {
            m_indirections = indirStore;
        }

        #endregion IConstructors
        #region IProperties
        
        /// <summary>gets the stream, which was used to construct the stream</summary>
        internal Stream BackingStream {
            get {
                return m_backingStream;
            }
        }

        #endregion IProperties
        #region IMethods

        protected override void SetStream(Stream stream) {
            // use a peeksupporting stream, because peek-support is needed for value-type deserialization
            base.SetStream(new PeekSupportingStream(stream));
            m_backingStream = stream;
        }
        
        /// <summary>
        /// sets GIOP-Version of this stream.
        /// </summary>
        /// <remarks>
        /// before GIOP-Version is not set, no version dependant operation are possible (e.g. reading wstring)
        /// </remarks>
        private void SetGiopVersion(GiopVersion version) {
            if (m_version != null) {
                // giop version already set before
                throw new INTERNAL(1201, CompletionStatus.Completed_MayBe);
            }
            m_version = version;
        }
        
        /// <summary>
        /// set big/little endian for this stream.
        /// </summary>
        /// <remarks>
        /// before this is set, the endian dependent operation are not usable.
        /// </remarks>
        /// <param name="endianFlag"></param>
        private void SetEndian(byte endianFlag) {
            if (m_endianOp != s_endianNotSpec) {
                // endian flag was already set before
                throw new INTERNAL(1202, CompletionStatus.Completed_MayBe);
            }
            bool isLittleEndian = ParseEndianFlag(endianFlag);
            if (isLittleEndian != BitConverter.IsLittleEndian) {
                m_endianOp = new CdrStreamNonNativeEndianReadOP(this, (GiopVersion)m_version);
            } else {
                m_endianOp = new CdrStreamNativeEndianReadOP(this, (GiopVersion)m_version);
            }
        }

        /// <summary>
        /// configure the input stream. Before this operation is called, no GIOP-Version dependant and no endian flag
        /// dependant operation is callable. E.g WString, Double, Long, ... are not possible before stream is configured
        /// </summary>
        /// <param name="endianFlag"></param>
        /// <param name="version"></param>
        public void ConfigStream(byte endianFlag, GiopVersion version) {
            SetGiopVersion(version);
            SetEndian(endianFlag);
        }

        
        /// <summary>with this method, the nr of bytes following in the stream can be set.
        /// After this method is called, it's not possible to read more then bytesToFollow
        /// </summary>
        /// <param name="streamLength"></param>
        public void SetMaxLength(uint bytesToFollow) {
            m_indexForBytesToF = GetPosition();
            m_bytesToFollow = bytesToFollow;
            m_bytesToFollowSet = true;
        }

        #region helper methods

        /// <summary>switch to peeking mode</summary>
        private void StartPeeking() {
            m_startPeekPosition = GetPosition(); // store postion to be able to go back to this position
            ((PeekSupportingStream)BaseStream).StartPeeking();
        }

        /// <summary>stops peeking, switch back to normal mode</summary>
        private void StopPeeking() {
            SetPosition(m_startPeekPosition);
            ((PeekSupportingStream)BaseStream).EndPeeking();
        }

        /// <summary>
        /// returns true, if in peeking mode
        /// </summary>
        private bool IsPeeking() {
            return ((PeekSupportingStream)BaseStream).IsPeeking();
        }

        /// <summary>
        /// gets the bytes to follow in the stream. If this is not set, an exception is thrown.
        /// </summary>
        protected uint GetBytesToFollow() {
            if (m_bytesToFollowSet) {
                return m_indexForBytesToF + m_bytesToFollow - GetPosition();
            } else {
                // bytes to follow not set
                throw new INTERNAL(1203, CompletionStatus.Completed_MayBe);
            }
        }
                
        /// <summary>
        /// check reading past end of stream; in case of chunked valuetypes,
        /// check also reading over chunk border
        /// </summary>
        /// <param name="bytesToRead"></param>
        private void CheckStreamPosition(uint bytesToRead) {
            if (m_bytesToFollowSet) {
                if (GetPosition() + bytesToRead > m_indexForBytesToF + m_bytesToFollow) {
                    // no more bytes readable in this message
                    // eof reached, read not possible
                    throw new MARSHAL(1207, CompletionStatus.Completed_MayBe);
                }
            }
            UpdateAndCheckChunking(bytesToRead);
        }
        
        /// <summary>read padding for an aligned read with the requiredAlignement</summary>
        /// <param name="requiredAlignment">align to which size</param>
        protected void AlignRead(byte requiredAlignment) {
            // do a chunk-start check here, because it's possible, that
            // the value, for which we align, is in a new chunk
            // -> therefore a chunk length tag could be in between ->
            // the alignement must be check after the chunk-length tag.
            UpdateAndCheckChunking(0);

            // nr of bytes the index is after the last aligned index
            uint align = GetAlignBytes(requiredAlignment);
            if (align != 0) {
                // go to the next aligned position
                ReadPadding(align);
            }
        }

        /// <summary>read padding for an aligned read with the requiredAlignement;
        /// if not enough bytes in the stream read as much as possible and return false</summary>
        /// <param name="requiredAlignment">align to which size</param>
        protected bool TryAlignRead(byte requiredAlignment) {
            bool hadEnoughToRead = false;
            // do a chunk-start check here, because it's possible, that
            // the value, for which we align, is in a new chunk
            // -> therefore a chunk length tag could be in between ->
            // the alignement must be check after the chunk-length tag.
            UpdateAndCheckChunking(0);

            // nr of bytes the index is after the last aligned index
            uint align = GetAlignBytes(requiredAlignment);
            if (align != 0) {
                if (align <= GetBytesToFollow()) {
                    // go to the next aligned position
                    ReadPadding(align);
                    hadEnoughToRead = true;
                } else {
                    SkipRest(); // read the remaining
                }
            }
            return hadEnoughToRead;
        }

        /// <summary>reads a nr of padding bytes</summary>
        public void ReadPadding(uint nrOfBytes) {
            for (uint i = 0; i < nrOfBytes; i++) {
                ReadOctet();
            }
        }
        #endregion helper methods
        #region Implementation of CDRInputStream

        public byte ReadOctet() {
            CheckStreamPosition(1);
            byte read = (byte)BaseStream.ReadByte();
            IncrementPosition(1);
            return read;
        }

        public bool ReadBool() {
            byte read = ReadOctet();
            if (read == 0) {
                return false;
            }
            else if (read == 1) {
                return true;
            }
            else {
                // invalid data for boolean: read
                throw new BAD_PARAM(10030, CompletionStatus.Completed_MayBe);
            }
        }
        
        public char ReadChar() {
            // char is a multibyte format with not fixed length characters, but in IDL one char is one byte
            Encoding encoding = CodeSetService.GetCharEncoding(CharSet, false);
            byte[] data = new byte[] { ReadOctet() };
            char[] result = encoding.GetChars(data);
            return result[0];
        }

        #region the following read methods are subject to byte ordering

        public short ReadShort() {
            return m_endianOp.ReadShort();
        }
        public ushort ReadUShort() {
            return m_endianOp.ReadUShort();
        }
        public int ReadLong() {
            return m_endianOp.ReadLong();
        }
        public uint ReadULong() {
            return m_endianOp.ReadULong();
        }
        public long ReadLongLong() {
            return m_endianOp.ReadLongLong();
        }
        public ulong ReadULongLong() {
            return m_endianOp.ReadULongLong();
        }
        public float ReadFloat() {
            return m_endianOp.ReadFloat();
        }
        public double ReadDouble() {
            return m_endianOp.ReadDouble();
        }
        public char ReadWChar() {
            return m_endianOp.ReadWChar();
        }
        public string ReadWString() {
            return m_endianOp.ReadWString();
        }
        public T[] ReadPrimitiveTypeArray<T>(int elemCount) {
            return m_endianOp.ReadPrimitiveTypeArray<T>(elemCount);
        }
        
        #endregion the following read methods are subject to byte ordering

        public string ReadString() {
            uint length = ReadULong(); // nr of bytes used including the terminating 0
            return ReadStringData(length);
        }

        private string ReadStringData(uint length) {
            if (length == 0) {
                // not valid accoring to CORBA 2.6 standard 15.3.2.7, but used by some orbs.
                // -> therefore, return the zero length string too, instead of an exception
                return String.Empty;
            }
            byte[] charData = ReadOpaque((int)length - 1); // read string data
            ReadOctet(); // read terminating 0
            Encoding encoding = CodeSetService.GetCharEncoding(CharSet, false);
            char[] data = encoding.GetChars(charData);
            string result = new string(data);
            return result;
        }

        public byte[] ReadOpaque(int nrOfBytes) {
            CheckStreamPosition((uint)nrOfBytes);
            byte[] data = new byte[nrOfBytes];
            BaseStream.Read(data, 0, nrOfBytes);
            IncrementPosition((uint)nrOfBytes);
            return data;
        }

        public void ReadBytes(byte[] buf, int offset, int count) {
            CheckStreamPosition((uint)count);
            BaseStream.Read(buf, offset, count);
            IncrementPosition((uint)count);
        }

        public void ForceReadAlign(Aligns align) {
            AlignRead((byte)align);
        }
        
        public bool TryForceReadAlign(Aligns align) {
            return TryAlignRead((byte)align);
        }
        
        public CdrEncapsulationInputStream ReadEncapsulation() {
            CdrEncapsulationInputStream encap = new CdrEncapsulationInputStream(this, m_indirections);
            return encap;
        }

        public void SkipRest() {
            if (!m_bytesToFollowSet) {
                // only possible to call skipRest, if nrOfBytes set
                throw new INTERNAL(976, CompletionStatus.Completed_MayBe);
            }
            try {
                // disable chunk checking, because skip-rest should ignore the rest of the stream -> don't want chunk cross exceptions
                m_skipChunkCheck = true;
                ReadPadding(GetBytesToFollow());
            } finally {
                m_skipChunkCheck = false;
            }
        }

        #region ValueTypeHandling

        private void UpdateAndCheckChunking(uint nrOfBytesToRead) {
            // only do something, if chunking is active
            // ignore chunking, if in peeking mode or if chunking check deactivated while in this method
            if ((IsChunkActive()) && !IsPeeking() && !m_skipChunkCheck) {
                try {
                    m_skipChunkCheck = true; // ignore chunking check during this method call
                    ChunkInfo chunkInfo = (ChunkInfo)m_chunkStack.Peek();
                    if (chunkInfo.IsBorderReached()) {
                        StartPeeking();
                        int tagOrChunkLength = ReadLong();
                        StopPeeking();
                        if ((tagOrChunkLength > 0) && (tagOrChunkLength < MIN_VALUE_TAG)) {
                            // a chunk starts here
                            ReadLong();
                            chunkInfo.IsContinuationExpected = false;
                            // set chunk to start after tag and contains tag bytes
                            chunkInfo.SetContinuationLength((uint)tagOrChunkLength);
                        } else if ((tagOrChunkLength >= MIN_VALUE_TAG) &&
                                   (tagOrChunkLength <= MAX_VALUE_TAG)) {
                            // a value type starting here -> current chunk is deactived while nested val type is read
                            chunkInfo.IsContinuationExpected = true;
                        }
                    }
                    // for non-valuetypes following, we need to check, if the chunk border is not crossed
                    // embedded valuetypes deactivate the current chunk, therefore no checking
                    if ((!chunkInfo.IsContinuationExpected) &&
                        (chunkInfo.WillBorderBeCrossed((int)nrOfBytesToRead))) {
                        // invlaid serialized value-type, try to read over the chunk border
                        throw new MARSHAL(901, CompletionStatus.Completed_MayBe);
                    }
                } finally {
                    m_skipChunkCheck = false;
                }
            }
        }

        /// <summary>check if a chunk is active</summary>
        private bool IsChunkActive() {
            if (m_chunkStack.Count == 0) {
                return false;
            }
            ChunkInfo info = (ChunkInfo)m_chunkStack.Peek(); // chunks are not nested -> check only topmost
            if (info.IsContinuationExpected || info.IsFinished) { // not active
                // is continuationExpected means, that a chunk of a nested value type follows;
                // the current chunk is inactive up to end of inner value type chunk.
                return false;
            } else {
                return true;
            }
        }

        private bool IsChunked(uint valueTag) {
            return ((valueTag & 0x00000008) > 0);
        }

        public void BeginReadNewValue() {
            // nothing to do yet
        }

        public void BeginReadValueBody(uint valueTag) {
            if (IsChunked(valueTag)) {
                m_chunkLevel++;
                // add a chunkinfo for this value type
                ChunkInfo info = new ChunkInfo(0, this);
                info.IsContinuationExpected = false;
                // store chunk-info
                m_chunkStack.Push(info);

            }
        }

        public void EndReadValue(uint valueTag) {
            if (IsChunked(valueTag)) {
                EndChunk();
                if (m_chunkLevel == 1) {
                    // outermost value: no chunks must be on the stack
                    if (m_chunkStack.Count > 0) {
                        // not all chunks closed at the ending of the value-type
                        throw new MARSHAL(911, CompletionStatus.Completed_MayBe);
                    }
                }
                m_chunkLevel--;
            }
        }

        /// <summary>ends chunk(s) here</summary>
        private void EndChunk() {
            // end chunk(s) here
            ChunkInfo top = (ChunkInfo)m_chunkStack.Pop();
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
                CheckChunkInfoAtEnd((ChunkInfo)enumerator.Current); // check if chunk can end here!
                ((ChunkInfo)enumerator.Current).IsFinished = true;
            }
            if (enumerator.MoveNext()) {
                // was a nested value, continue chunk if no val type follows ...
                ChunkInfo continuation = (ChunkInfo)enumerator.Current;
                continuation.IsContinuationExpected = false; // inner value has ended here, reactivate outer chunk
                // set chunk start position to current position, length 0
                // -> either a chunk length or a embedded valuetype must follow now!
                continuation.SetContinuationLength(0);
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
            uint toRead = chunk.GetBytesAvailable();
            if (toRead > 0) {
                ReadPadding(toRead);
            }
        }

        #endregion ValueTypeHandling
                
        #region Indirection Handling
        
        public uint ReadInstanceOrIndirectionTag(out StreamPosition instanceStartPosition,
                                                 out bool isIndirection) {

            uint tag = ReadULong();
            instanceStartPosition = new StreamPosition(this, 4, true);
            isIndirection = (tag == INDIRECTION_TAG);
            return tag;
        }

        public string ReadIndirectableString(IndirectionType indirType, IndirectionUsage indirUsage,
                                             bool resolveGlobal) {
            uint lengthOrTag = ReadULong();
            if (lengthOrTag == INDIRECTION_TAG) {
                StreamPosition indirPos = ReadIndirectionOffset();
                return (string)GetObjectForIndir(new IndirectionInfo(indirPos.GlobalPosition,
                                                                     indirType, indirUsage),
                                         resolveGlobal);
            } else {
                StreamPosition beforeStringPos = new StreamPosition(this, 4, true);
                string result = ReadStringData(lengthOrTag);
                StoreIndirection(new IndirectionInfo(beforeStringPos.GlobalPosition,
                                                     indirType, indirUsage),
                                 result);
                return result;
            }
        }

        public StreamPosition ReadIndirectionOffset() {
            int indirectionOffset = ReadLong();
            if (indirectionOffset >= -4) {
                // indirection-offset is not ok: indirectionOffset
               throw new MARSHAL(949, CompletionStatus.Completed_MayBe);
            }
            // indirection-offset is negative --> therefore add to stream-position;
            // -4, because indirectionoffset itself doesn't count --> stream-pos too high
            StreamPosition result = new StreamPosition(this, (uint)Math.Abs(indirectionOffset) + 4,
                                                       true);
            return result;
        }

        public object GetObjectForIndir(IndirectionInfo indirInfo, bool resolveGlobal) {
            return m_indirections.GetObjectForIndir(indirInfo, resolveGlobal);
        }

        public void StoreIndirection(IndirectionInfo indirDesc, object valueAtIndirPos) {
            m_indirections.StoreIndirection(indirDesc, valueAtIndirPos);
        }
        
        #endregion IndirectionHandling
        #endregion Implementation of CDRInputStream

        #endregion IMethods

    }


    /// <summary>the base class for streams, writing CDR data</summary>
    [CLSCompliant(false)]
    public class CdrOutputStreamImpl : CdrStreamHelper, CdrOutputStream {
        
        #region SFields

        private static CdrEndianOpNotSpecified s_endianNotSpec = new CdrEndianOpNotSpecified();

        #endregion SFields
        #region IFields
        
        /// <summary>responsible for implementing the endian dependant operation</summary>
        private CdrEndianDepOutputStreamOp m_endianOp = s_endianNotSpec;

        /// <summary>stores stream flags for use for derived encapsulation streams</summary>
        private byte flags;

        /// <summary>used to store indirections encountered in this stream</summary>
        private IndirectionStoreValueKey m_indirections = new IndirectionStoreValueKey();


        #endregion IFields
        #region IConstructors
        
        public CdrOutputStreamImpl(Stream stream, byte flags) : this(stream, flags, new GiopVersion(1, 2)) {
        }
        
        public CdrOutputStreamImpl(Stream stream, byte flags, GiopVersion giopVersion) : base(stream) {
            this.flags = flags;
            bool isLittleEndian = ParseEndianFlag(flags);
            if (isLittleEndian != BitConverter.IsLittleEndian) {
                m_endianOp = new CdrStreamNonNativeEndianWriteOP(this, giopVersion);
            } else {
                m_endianOp = new CdrStreamNativeEndianWriteOP(this, giopVersion);
            }
        }

        /// <summary>
        /// this constructor is used, to construct a stream, which doesn't start at offset 0.
        /// It is used to construct a stream for a message body: because the header is not in this stream, the
        /// starting index is not 0.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="flags"></param>
        /// <param name="initialOffset"></param>
        internal CdrOutputStreamImpl(Stream stream, byte flags, GiopVersion version, uint initialOffset) : this(stream, flags, version) {
            IncrementPosition(initialOffset);
        }

        #endregion IConstructors
        #region IProperties

        
        internal Stream BackingStream {
            get {
                return base.BaseStream;
            }
        }

        #endregion IProperties
        #region IMethods

        #region helper methods

        /// <summary>write padding for an aligned write with the requiredAlignement</summary>
        /// <param name="requiredAlignment">align to which size</param>
        protected void AlignWrite(byte requiredAlignment) {
            uint align = GetAlignBytes(requiredAlignment);
            if (align != 0) {
                // go to the next aligned position
                WritePadding(align);
            }
        }

        /// <summary>wirtes a nr of padding bytes</summary>
        public void WritePadding(uint nrOfBytes) {
            for (uint i = 0; i < nrOfBytes; i++) {
                WriteOctet(0);
            }
        }

        #endregion helper methods

        #region Implementation of CDROutputStream
        public void WriteOctet(byte data) {
            BaseStream.WriteByte(data);
            IncrementPosition(1);
        }

        public void WriteBool(bool data) {
            if (data) {
                WriteOctet(1);
            } else {
                WriteOctet(0);
            }
        }

        public void WriteChar(char data) {
            Encoding encoding = CodeSetService.GetCharEncoding(CharSet, false);
            byte[] toSend = encoding.GetBytes(new char[] { data });
            if (toSend.Length > 1) {
                // character can't be sent: only one byte representation allowed
                throw new DATA_CONVERSION(10001, CompletionStatus.Completed_MayBe);
            } // is this correct ?
            WriteOpaque(toSend);
        }

        #region the following write methods are subject to byte ordering
        public void WriteShort(short data) {
            m_endianOp.WriteShort(data);
        }
        public void WriteUShort(ushort data) {
            m_endianOp.WriteUShort(data);
        }
        public void WriteLong(int data) {
            m_endianOp.WriteLong(data);
        }
        public void WriteULong(uint data) {
            m_endianOp.WriteULong(data);
        }
        public void WriteLongLong(long data) {
            m_endianOp.WriteLongLong(data);
        }
        public void WriteULongLong(ulong data) {
            m_endianOp.WriteULongLong(data);
        }
        public void WriteFloat(float data) {
            m_endianOp.WriteFloat(data);
        }
        public void WriteDouble(double data) {
            m_endianOp.WriteDouble(data);
        }
        public void WritePrimitiveTypeArray<T>(T[] data) {
            m_endianOp.WritePrimitiveTypeArray(data);
        }
        public void WriteWChar(char data) {
            m_endianOp.WriteWChar(data);
        }
        public void WriteWString(string data) {
            m_endianOp.WriteWString(data);
        }
        #endregion the following write methods are subject to byte ordering
        
        public void WriteString(string data) {
            Encoding encoding = CodeSetService.GetCharEncoding(CharSet, false);
            byte[] toSend = encoding.GetBytes(data.ToCharArray()); // encode the string
            WriteULong((uint)(toSend.Length + 1));
            WriteOpaque(toSend);
            WriteOctet(0);
        }
        
        public void WriteOpaque(byte[] data) {
            WriteBytes(data, 0, data.Length);
        }

        public void WriteBytes(byte[] data, int offset, int count) {
            if (data == null) {
                return;
            }
            BaseStream.Write(data, offset, count);
            IncrementPosition((uint)count);
        }

        public void ForceWriteAlign(Aligns align) {
            AlignWrite((byte)align);
        }

        public void WriteEncapsulation(CdrEncapsulationOutputStream encap) {
            encap.WriteToStream(this);
        }

        public StreamPosition WriteIndirectableInstanceTag(uint tag) {
            WriteULong(tag);
            return new StreamPosition(this, 4, true); // position just before tag
        }

        public void WriteIndirectableString(string val,
                                            IndirectionType indirType,
                                            IndirectionUsage indirUsage) {

            if (IsPreviouslyMarshalled(val,
                                       indirType, indirUsage)) {
                // write indirection
                WriteIndirection(val);
            } else {
                // prepare to add repId to indirection table
                ForceWriteAlign(Aligns.Align4);
                StreamPosition indirPos = new StreamPosition(this);
                WriteString(val);

                IndirectionInfo indirInfo = new IndirectionInfo(indirPos.GlobalPosition,
                                                                indirType,
                                                                indirUsage);
                StoreIndirection(val, indirInfo);
            }
        }

        public void WriteIndirection(object forVal) {
            object indirInfo = GetIndirectionInfoFor(forVal);
            if (indirInfo != null) {
                WriteULong(INDIRECTION_TAG);
                // remark: indirection offset must be calculated after indir tag has been written!
                int indirOffset = (int)CalculateIndirectionOffset((IndirectionInfo)indirInfo);
                WriteLong(indirOffset); // write the nr of bytes the value is before the current position
            } else {
                 throw CreateWriteInexistentIndirectionException();
            }
        }
                
        public virtual bool IsPreviouslyMarshalled(object val, IndirectionType indirType,
                                           IndirectionUsage indirUsage) {
            return m_indirections.IsPreviouslyMarshalled(val, indirType, indirUsage);
        }
        

        public void StoreIndirection(object val, IndirectionInfo indirInfo) {
            if (IsAllowedToStore(val, indirInfo)) {
                m_indirections.StoreIndirection(val, indirInfo);
            } else {
                throw CreateReplaceIndirectionException();
            }
        }

        public byte Flags {
            get { return flags; }
        }
        
        protected virtual bool IsAllowedToStore(object val, IndirectionInfo indirInfo) {
            return GetIndirectionInfoFor(val) == null;
        }
        
        public virtual IndirectionInfo GetIndirectionInfoFor(object forVal) {
            return m_indirections.GetIndirectionInfoFor(forVal);
        }
        
        public virtual long CalculateIndirectionOffset(IndirectionInfo indirInfo) {
            return -1 * (long)(GetPosition() - indirInfo.StreamPos);
        }
        
        internal Exception CreateWriteInexistentIndirectionException() {
            return new MARSHAL(958, CompletionStatus.Completed_MayBe);
        }
        
        private Exception CreateReplaceIndirectionException() {
            return new MARSHAL(959, CompletionStatus.Completed_MayBe);
        }

        #endregion Implementation of CDROutputStream

        #endregion IMethods
        
    }

    [CLSCompliant(false)]
    public class CdrEncapsulationInputStream : CdrInputStreamImpl {
        
        #region IFields
        
        private uint m_globalOffset;
        
        #endregion IFields
        #region IConstructor
        
        /// <summary>
        /// constructs an encapsulation from a sequence of byte array following in input stream.
        /// </summary>
        /// <remarks>should only be called from inside CdrInputStreamImpl</remarks>
        internal CdrEncapsulationInputStream(CdrInputStream stream,
                                             IndirectionStoreIndirKey indirStore) : base(indirStore) {
            uint encapsLength = stream.ReadULong();
            // read the encapsulation from the input stream
            StreamPosition streamPos = new StreamPosition(stream); // also include encaps length in global offset, because not in GetPosition() considered
            uint globalOffset = streamPos.GlobalPosition;  // global position of this stream beginning (GetPosition() on this stream doesn't consider length field read -> global offset is just after lenght)
            byte[] data = stream.ReadOpaque((int)encapsLength);
            Initalize(data, globalOffset, new GiopVersion(1,2)); // for encapsulation, the GIOP-dependant operation must be compatible with GIOP-1.2, if not specified otherwise
        }
        
        /// <summary>
        /// constructs an encapsulation input stream from the encapsulation data.
        /// </summary>
        /// <param name="encapsulationData"></param>
        public CdrEncapsulationInputStream(byte[] encapsulationData) : this(encapsulationData, new GiopVersion(1,2)) {
             // for encapsulation, the GIOP-dependant operation must be compatible with GIOP-1.2, if not specified otherwise
        }
        
        public CdrEncapsulationInputStream(byte[] encapsulationData, GiopVersion version) : base(new IndirectionStoreIndirKey()) {
            Initalize(encapsulationData, 0, version);
        }

        #endregion IConstructor
        #region IMethods
        
        private void Initalize(byte[] data, uint globalOffset, GiopVersion version) {
            m_globalOffset = globalOffset;
            Stream baseStream = new MemoryStream();
            // copy the data into the underlying stream
            baseStream.Write(data, 0, data.Length);
            baseStream.Seek(0, SeekOrigin.Begin);
            // now set the stream
            SetStream(baseStream);
            byte flags = ReadOctet(); // read the flags out of the encapsulation
            ConfigStream(flags, version);
            SetMaxLength(((uint)data.Length)-1); // flags are already read --> minus 1
        }
        
        public byte[] ReadRestOpaque() {
            return ReadOpaque((int)GetBytesToFollow());
        }
        
        public override uint GetGlobalOffset() {
            return m_globalOffset;
        }

        #endregion IMethods
    }


    /// <summary>
    /// this class represents a CDR encapsulation.
    /// </summary>
    /// <remarks>
    /// when writing to the CDR encapsulation, it's not needed to write anything else than the
    /// encapsulation content
    /// </remarks>
    [CLSCompliant(false)]
    public class CdrEncapsulationOutputStream : CdrOutputStreamImpl {
        
        #region IFields
        
        private CdrOutputStream m_targetStream;
        private uint m_targetPosition = 0;
        private uint m_indirectionOffsetPosition = 0;
        
        #endregion IFields
        #region IConstructors

        public CdrEncapsulationOutputStream(byte flags) :
            this(flags, new GiopVersion(1, 2), null) { // for Encapsulation, GIOP-Version dep operation must be compatible with GIOP-1.2, if not specified otherwise
        }
        
        public CdrEncapsulationOutputStream(GiopVersion version) :
            this(GiopHeader.GetDefaultHeaderFlagsForPlatform(), version, null) {
        }
        
        public CdrEncapsulationOutputStream(CdrOutputStream targetStream) :
            this(targetStream.Flags, new GiopVersion(1, 2), targetStream) {
        }

        private CdrEncapsulationOutputStream(byte flags, GiopVersion version,
                                             CdrOutputStream targetStream) :
            base(new MemoryStream(), flags, version) {
            m_targetStream = targetStream;
            if (targetStream != null) {
                // store target position:
                m_targetPosition = targetStream.GetPosition();
                // encapsulation is stored 4 aligned,
                // 4 bytes length following before encap content start
                m_indirectionOffsetPosition =
                    targetStream.GetGlobalOffset() +
                    targetStream.GetNextAlignedPosition(Aligns.Align4) + 4;
            }
            WriteOctet(flags); // the flag is the first byte in the encapsulation --> has influence on alignement
        }

        #endregion IConstructors
        #region IMethods

        /// <summary>
        /// writes the encapsulation to the specified stream.
        /// Use this, if encapsulation is not targeted for a specific output stream.
        /// </summary>
        /// <param name="outputStream"></param>
        internal void WriteToStream(CdrOutputStream stream) {
            if (m_targetStream == null) {
                WriteToStreamInternal(stream);
            } else {
                throw CreateInvalidTargetStreamException();
            }
        }
        
        /// <summary>this method is used, if encapsulation is targeted for a specific stream</summary>
        public void WriteToTargetStream() {
            if ((m_targetStream != null) &&
                (m_targetStream.GetPosition() == m_targetPosition)) {
                WriteToStreamInternal(m_targetStream);
            } else {
                throw CreateInvalidTargetStreamException();
            }
        }
        
        private void WriteToStreamInternal(CdrOutputStream stream) {
            stream.WriteULong(((uint)BaseStream.Length)); // length of the encapsulation
            MemoryStream mem = (MemoryStream) BaseStream;
            stream.WriteOpaque(mem.ToArray());
        }
        
        private Exception CreateInvalidTargetStreamException() {
            return new INTERNAL(856, CompletionStatus.Completed_MayBe);
        }
        
        public override uint GetGlobalOffset() {
            return m_indirectionOffsetPosition;
        }
               
        public override bool IsPreviouslyMarshalled(object val, IndirectionType indirType,
                                                    IndirectionUsage indirUsage) {
            bool result = base.IsPreviouslyMarshalled(val, indirType, indirUsage);
            if (m_targetStream != null) {
                result = result ||
                         m_targetStream.IsPreviouslyMarshalled(val, indirType, indirUsage);
            }
            return result;
        }
                
        protected override bool IsAllowedToStore(object val, IndirectionInfo indirInfo) {
            // do not allow to overwrite indirections for a value in this stream
            return base.GetIndirectionInfoFor(val) == null;
        }
        
        public override IndirectionInfo GetIndirectionInfoFor(object forVal) {
            IndirectionInfo result = base.GetIndirectionInfoFor(forVal);
            if ((result == null) && (m_targetStream != null)) {
                result = m_targetStream.GetIndirectionInfoFor(forVal);
            }
            return result;
        }
        
        public override long CalculateIndirectionOffset(IndirectionInfo indirInfo) {
            return -1 * (long)(GetGlobalOffset() + GetPosition() - indirInfo.StreamPos);
        }
        
        /// <summary>
        /// returns the encapsulation sequence data of this encapsulation as a byte[].
        /// </summary>
        /// <remarks>not included is the length of the encapsulated data; it starts with the
        /// endian flag.</remarks>
        public byte[] GetEncapsulationData() {
            MemoryStream mem = (MemoryStream) BaseStream;
            return mem.ToArray();
        }

        #endregion IMethods
    }

}
