/* CDRStream.cs
 * 
 * Project: IIOP.NET
 * IIOPChannel
 * 
 * WHEN      RESPONSIBLE
 * 14.01.03  Dominic Ullmann (DUL), dul@elca.ch
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
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Util;
using Ch.Elca.Iiop.Services;
using Ch.Elca.Iiop.CodeSet;
using omg.org.CORBA;

namespace Ch.Elca.Iiop.Cdr {
    
    /// <summary>
    /// alignements possible in CDRStreams
    /// </summary>
    public enum Aligns {
        Align2 = 2,
        Align4 = 4,
        Align8 = 8
    }


    /// <summary>
    /// this interfaces describes the methods for reading from streams containing CDR-data, which are different for big-endian / little-endian
    /// </summary>
    /// <remarks>
    /// this interface is not intended for CDR-stream users
    /// </remarks>
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
        
        #endregion IMethods

    }
    
    
    /// <summary>
    /// this interface describes the mehtods, which are available on streams containing CDR-data for reading
    /// </summary>
    public interface CdrInputStream : CdrEndianDepInputStreamOp {
        
        #region IProperties

        /// <summary>the charset to use</summary>
        uint CharSet {
            get;
            set;
        }
        
        /// <summary>the wcharset to use</summary>
        uint WCharSet {
            get; 
            set;
        }

        #endregion IProperties
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

        void ReadPadding(ulong nrOfBytes);

        /// <summary>
        /// forces the alignement on a boundary. Is for example useful in IIOP 1.2, where a request/response body
        /// must be 8-aligned.
        /// </summary>
        /// <param name="align">the requested Alignement</param>
        void ForceReadAlign(Aligns align);

        /// <summary>reads an encapsulation from this stream</summary>
        CdrEncapsulationInputStream ReadEncapsulation();

        /// <summary>skip the remaining bytes in the message</summary>
        /// <remarks>throws an exception if length not known</remarks>
        void SkipRest();

        #endregion IMethods

    }

    /// <summary>
    /// this interfaces describes the methods for writing to streams containing CDR-data, which are different for big-endian / little-endian
    /// </summary>
    /// <remarks>
    /// this interface is not intended for CDR-stream users
    /// </remarks>
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
    public interface CdrOutputStream : CdrEndianDepOutputStreamOp {
        
        #region IProperties

        /// <summary>the charset to use</summary>
        uint CharSet {
            get;
            set;
        }
        
        /// <summary>the wcharset to use</summary>
        uint WCharSet {
            get; 
            set;
        }

        #endregion IProperties
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
        /// <summary>
        /// forces the alignement on a boundary. Is for example useful in IIOP 1.2, where a request/response body
        /// must be 8-aligned.
        /// </summary>
        /// <param name="align">the requested Alignement</param>
        void ForceWriteAlign(Aligns align);

        /// <summary>writes a nr of padding bytes</summary>
        void WritePadding(ulong nrOfBytes);

        /// <summary>writes an encapsulation to this stream</summary>
        void WriteEncapsulation(CdrEncapsulationOutputStream encap);

        #endregion IMethods
    }
       
    /// <summary>
    /// this class represents a stream for writing a message to an underlaying stream
    /// </summary>
    public class CdrMessageOutputStream {

        #region IFields
        
        private CdrOutputStreamImpl m_stream;
        private MemoryStream m_buffer;
        private CdrOutputStream m_contentStream;
        private GiopHeader m_header;

        #endregion IFields
        #region IConstructors
        
        public CdrMessageOutputStream(Stream stream, GiopHeader header) {
            m_stream = new CdrOutputStreamImpl(stream, header.GiopFlags, header.Version);
            m_buffer = new MemoryStream();
            m_contentStream = new CdrOutputStreamImpl(m_buffer, header.GiopFlags, header.Version, GiopHeader.HEADER_LENGTH);
            m_header = header;
        }

        #endregion IConstructors
        #region IMethods

        /// <summary>get a CDROutputStream for writing the content of the message</summary>
        public CdrOutputStream GetMessageContentWritingStream() {
            return m_contentStream;
        }

        /// <summary>
        /// this operation closes the output stream for the message content and writes the whole message
        /// to the underlaying stream.
        /// After this operation, the stream is not further usable.
        /// </summary>
        public void CloseStream() {
            m_buffer.Seek(0, SeekOrigin.Begin);
            // write header
            m_header.WriteToStream(m_stream, (uint)m_buffer.Length);
            // write content
            for (int i = 0; i < m_buffer.Length; i++) {
                m_stream.WriteOctet((byte)m_buffer.ReadByte());
            }
            m_buffer.Close();
            m_contentStream = null;
        }

        #endregion IMethods

    }
    
    /// <summary>
    /// this class represents a stream for reading a message from an underlaying stream
    /// </summary>    
    public class CdrMessageInputStream {

        #region IFields

        private CdrInputStreamImpl m_inputStream;
        private GiopHeader m_header;

        #endregion IFields
        #region IConstructors
        
        public CdrMessageInputStream(Stream stream) {
            m_inputStream = new CdrInputStreamImpl(stream);
            // read the header, this sets the big/little endian implementation and bytesToFollow
            m_header = new GiopHeader(m_inputStream);
        }

        #endregion IConstructors
        #region IProperties
        
        public GiopHeader Header {
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
        public CdrInputStream GetMessageContentReadingStream() {
            return m_inputStream;
        }

        #endregion IMethods
    }    

    
    /// <summary>
    /// this class provide some helper methods to CDRStream implementation
    /// </summary>
    public abstract class CdrStreamHelper {
        
        #region IFields

        /// <summary>the underlying stream</summary>
        private Stream m_stream;
        
        /// <summary>the flags used for this stream</summary>
        protected byte m_flags;

        private ulong m_index = 0;

        #region for informing about next aligned pos
        private StreamPosition m_storeNextAlignedPos;
        private bool m_memNextAlign = false;
        #endregion for informing about next aligned pos

        protected uint m_charSet = CodeSetService.DEFAULT_CHAR_SET;
        protected uint m_wcharSet = CodeSetService.DEFAULT_WCHAR_SET;

        #endregion IFields
        #region IConstructors
        
        public CdrStreamHelper(Stream stream) {
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

        #endregion IProperties
        #region IMethods
        
        /// <summary>for inheritors not using the public constructor</summary>
        protected virtual void SetStream(Stream stream) {
            m_stream = stream;
        }

        /// <summary>gets the nr of bytes which are missing to next aligned position</summary>
        protected ulong GetAlignBytes(byte requiredAlignement) {
            // nr of bytes the index is after the last aligned index
            ulong afterLastAlign = m_index % requiredAlignement;
            ulong alignBytes = 0;
            if (afterLastAlign != 0) {
                alignBytes = (requiredAlignement - afterLastAlign);
            }

            // helper code for informing about next aligned position
            if (m_memNextAlign) {
                m_memNextAlign = false;
                m_storeNextAlignedPos.Position = GetPosition() + alignBytes;
            }

            return alignBytes;
        }

        /// <summary>update the bookkeeping</summary>
        /// <param name="bytes"></param>
        protected void IncrementPosition(ulong bytes) {
            m_memNextAlign = false; // after incrementing position, do not further update m_storeNextAlignPos
            m_index += bytes;
        }
        internal ulong GetPosition() {
            return m_index;
        }

        protected void SetPosition(ulong position) {
            m_index = position;
        }

        #region helper methods for informing about next aligned position

        
        /// <summary>set streamPos to the next aligned position, after reaching it</summary>
        /// <param name="streamPos"></param>
        internal void MarkNextAlignedPosition(StreamPosition streamPos) {
            m_memNextAlign = true;
            m_storeNextAlignedPos = streamPos;
            m_storeNextAlignedPos.Position = GetPosition();
        }

        #endregion helper methods for informing about next aligned position

        /// <summary>determines, if big/little endian should be used</summary>
        /// <returns>true for big endian, false for little endian</returns>
        protected bool ParseEndianFlag(byte flag) {
            if ((flag & 0x01) > 0) {
                // little endian
                return false;
            } else {
                // big endian
                return true;
            }        
        }

        /// <summary>
        /// get the char encoding to use for this stream
        /// </summary>
        internal static Encoding GetCharEncoding(uint charSet, CodeSetConversionRegistry regToUse) {
            return regToUse.GetEncoding(charSet); // get Encoding for charSet
        }
        
        /// <summary>
        /// get the wchar encoding to use for this stream
        /// </summary>
        internal static Encoding GetWCharEncoding(uint wcharSet, CodeSetConversionRegistry regToUse) {
            return regToUse.GetEncoding(wcharSet); // get Encoding for wcharSet
        }

        #endregion IMethods
    }
    
    
    /// <summary>the base class for streams, reading CDR data</summary>
    public class CdrInputStreamImpl : CdrStreamHelper, CdrInputStream {
        
        #region SFields

        private static CdrEndianOpNotSpecified s_endianNotSpec = new CdrEndianOpNotSpecified();

        #endregion SFields
        #region IFields
        
        private object m_version = null;
        private CdrEndianDepInputStreamOp m_endianOp = s_endianNotSpec;

        private bool m_bytesToFollowSet = false;
        private ulong m_bytesToFollow = 0;
        /// <summary>this is the position, when bytes to follow was set</summary>
        private ulong m_indexForBytesToF = 0;

        private ulong m_startPeekPosition = 0;
        
        #endregion IFields
        #region IConstructors

        public CdrInputStreamImpl(Stream stream) : base() {
            SetStream(stream);
        }

        /// <summary>for inheritors only</summary>
        protected CdrInputStreamImpl() : base() {
        }

        #endregion IConstructors
        #region IProperties

        public uint CharSet {
            get { 
                return m_charSet;
            }
            set { 
                m_charSet = value;
            }
        }

        public uint WCharSet {
            get {
                return m_wcharSet;
            }
            set {
                m_wcharSet = value;
            }
        }

        #endregion IProperties
        #region IMethods

        protected override void SetStream(Stream stream) {
            // use a peeksupporting stream, because peek-support is needed for value-type deserialization
            base.SetStream(new PeekSupportingStream(stream));
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
            m_flags = endianFlag;
            if (ParseEndianFlag(endianFlag)) {
                m_endianOp = new CdrStreamBigEndianReadOP(this, (GiopVersion)m_version);
            } else {
                throw new NotImplementedException("little endian support not yet impelmented"); // TODO
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
        public void SetMaxLength(ulong bytesToFollow) {
            m_indexForBytesToF = GetPosition();
            m_bytesToFollow = bytesToFollow;
            m_bytesToFollowSet = true;
        }

        #region helper methods

        /// <summary>switch to peeking mode</summary>
        public void StartPeeking() {
            m_startPeekPosition = GetPosition(); // store postion to be able to go back to this position
            ((PeekSupportingStream)BaseStream).StartPeeking();
        }

        /// <summary>stops peeking, switch back to normal mode</summary>
        public void StopPeeking() {
            SetPosition(m_startPeekPosition);
            ((PeekSupportingStream)BaseStream).EndPeeking();
        }

        /// <summary>
        /// gets the bytes to follow in the stream. If this is not set, an exception is thrown.
        /// </summary>
        protected ulong GetBytesToFollow() {
            if (m_bytesToFollowSet) {
                return m_indexForBytesToF + m_bytesToFollow - GetPosition();
            } else {
                // bytes to follow not set
                throw new INTERNAL(1203, CompletionStatus.Completed_MayBe);
            }
        }
                
        private void CheckEndOfStream(ulong bytesToRead) {
            if (m_bytesToFollowSet) {
                if (GetPosition() + bytesToRead > m_indexForBytesToF + m_bytesToFollow) {
                    // no more bytes readable in this message
                    // eof reached, read not possible
                    throw new MARSHAL(1207, CompletionStatus.Completed_MayBe);
                }
            }
        }                
        
        /// <summary>read padding for an aligned read with the requiredAlignement</summary>
        /// <param name="requiredAlignment">align to which size</param>
        protected void AlignRead(byte requiredAlignment) {
            // nr of bytes the index is after the last aligned index
            ulong align = GetAlignBytes(requiredAlignment);
            if (align != 0) {
                // go to the next aligned position
                ReadPadding(align);
            }
        }

        /// <summary>reads a nr of padding bytes</summary>
        public void ReadPadding(ulong nrOfBytes) {
            for (ulong i = 0; i < nrOfBytes; i++) { 
                ReadOctet();
            }
        }
        #endregion helper methods
        #region Implementation of CDRInputStream

        public byte ReadOctet() {
            CheckEndOfStream(1);
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
            Encoding encoding = CdrStreamHelper.GetCharEncoding(CharSet, CodeSetConversionRegistry.GetRegistry());
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

        #endregion the following read methods are subject to byte ordering

        public string ReadString() {
            uint length = ReadULong(); // nr of bytes used including the terminating 0
            byte[] charData = ReadOpaque((int)length-1); // read string data
            ReadOctet(); // read terminating 0
            Encoding encoding = CdrStreamHelper.GetCharEncoding(CharSet, CodeSetConversionRegistry.GetRegistry());
            char[] data = encoding.GetChars(charData);
            string result = new string(data);
            return result;
        }

        public byte[] ReadOpaque(int nrOfBytes)    {
            CheckEndOfStream((ulong)nrOfBytes);
            byte[] data = new byte[nrOfBytes];
            BaseStream.Read(data, 0, nrOfBytes);
            IncrementPosition((ulong)nrOfBytes);
            return data;
        }

        public void ForceReadAlign(Aligns align) {
            AlignRead((byte)align);
        }
        
        public CdrEncapsulationInputStream ReadEncapsulation() {
            CdrEncapsulationInputStream encap = new CdrEncapsulationInputStream(this);
            return encap;
        }

        public void SkipRest() {
            if (!m_bytesToFollowSet) { 
                // only possible to call skipRest, if nrOfBytes set
                throw new INTERNAL(976, CompletionStatus.Completed_MayBe);
            }
            ReadPadding(GetBytesToFollow());
        }

        #endregion Implementation of CDRInputStream

        #endregion IMethods

    }


    /// <summary>the base class for streams, writing CDR data</summary>    
    public class CdrOutputStreamImpl : CdrStreamHelper, CdrOutputStream {
        
        #region SFields

        private static CdrEndianOpNotSpecified s_endianNotSpec = new CdrEndianOpNotSpecified();

        #endregion SFields
        #region IFields
        
        /// <summary>responsible for implementing the endian dependant operation</summary>
        private CdrEndianDepOutputStreamOp m_endianOp = s_endianNotSpec;
        private GiopVersion m_giopVersion;

        #endregion IFields
        #region IConstructors
        
        public CdrOutputStreamImpl(Stream stream, byte flags) : this(stream, flags, new GiopVersion(1, 2)) {
        }
        
        public CdrOutputStreamImpl(Stream stream, byte flags, GiopVersion giopVersion) : base(stream) {
            m_flags = flags;
            m_giopVersion = giopVersion;
            if (ParseEndianFlag(flags)) {
                m_endianOp = new CdrStreamBigEndianWriteOP(this, giopVersion);
            } else {
                throw new NotImplementedException("little endian support not yet impelmented"); // TODO
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
        internal CdrOutputStreamImpl(Stream stream, byte flags, GiopVersion version, ulong initialOffset) : this(stream, flags, version) {
            IncrementPosition(initialOffset);
        }

        #endregion IConstructors
        #region IProperties

        public uint CharSet {
            get { 
                return m_charSet;
            }
            set { 
                m_charSet = value;
            }
        }

        public uint WCharSet {
            get {
                return m_wcharSet;
            }
            set {
                m_wcharSet = value;
            }
        }

        #endregion IProperties
        #region IMethods

        #region helper methods

        /// <summary>write padding for an aligned write with the requiredAlignement</summary>
        /// <param name="requiredAlignment">align to which size</param>
        protected void AlignWrite(byte requiredAlignment) {
            ulong align = GetAlignBytes(requiredAlignment);
            if (align != 0) {
                // go to the next aligned position
                WritePadding(align);
            }
        }

        /// <summary>wirtes a nr of padding bytes</summary>
        public void WritePadding(ulong nrOfBytes) {
            for (ulong i = 0; i < nrOfBytes; i++) {
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
            if (data == false) {
                WriteOctet(0);
            } else {
                WriteOctet(1);
            }
        }

        public void WriteChar(char data) {
            Encoding encoding = CdrStreamHelper.GetCharEncoding(CharSet, CodeSetConversionRegistry.GetRegistry());
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
        public void WriteWChar(char data) {
            m_endianOp.WriteWChar(data);
        }
        public void WriteWString(string data) {
            m_endianOp.WriteWString(data);
        }
        #endregion the following write methods are subject to byte ordering
        
        public void WriteString(string data) {
            Encoding encoding = CdrStreamHelper.GetCharEncoding(CharSet, CodeSetConversionRegistry.GetRegistry());
            byte[] toSend = encoding.GetBytes(data.ToCharArray()); // encode the string
            WriteULong((uint)(toSend.Length + 1));
            WriteOpaque(toSend);
            WriteOctet(0);
        }
        
        public void WriteOpaque(byte[] data) {
            if (data == null) { 
                return; 
            }
            BaseStream.Write(data, 0, data.Length);
            IncrementPosition((ulong)data.Length);
        }

        public void ForceWriteAlign(Aligns align) {
            AlignWrite((byte)align);
        }

        public void WriteEncapsulation(CdrEncapsulationOutputStream encap) {
            encap.WriteToMessageContentStream(this);
        }

        #endregion Implementation of CDROutputStream

        #endregion IMethods
    
    }


    public class CdrEncapsulationInputStream : CdrInputStreamImpl {
        
        #region IConstructor
        
        internal CdrEncapsulationInputStream(CdrInputStream stream) : base() {
            Stream baseStream = new MemoryStream();
            // read the encapsulation from the input stream
            ulong encapsLength = stream.ReadULong();
            byte[] data = stream.ReadOpaque((int)encapsLength);
            // copy the data into the underlying stream
            baseStream.Write(data, 0, data.Length);
            baseStream.Seek(0, SeekOrigin.Begin);
            // now set the stream
            SetStream(baseStream);
            byte flags = ReadOctet(); // read the flags out of the encapsulation
            ConfigStream(flags, new GiopVersion(1,2)); // for encapsulation, the GIOP-dependant operation must be compatible with GIOP-1.2
            SetMaxLength(encapsLength-1); // flags are already read --> minus 1
        }

        #endregion IConstructor
        #region IMethods
        
        public byte[] ReadRestOpaque() {
            return ReadOpaque((int)GetBytesToFollow());
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
    public class CdrEncapsulationOutputStream : CdrOutputStreamImpl {
        
        #region IConstructors
        
        public CdrEncapsulationOutputStream(byte flags) :
            this(flags, new GiopVersion(1,2)) { // for Encapsulation, GIOP-Version dep operation must be compatible with GIOP-1.2
        }

        private CdrEncapsulationOutputStream(byte flags, GiopVersion version) : 
            base(new MemoryStream(), flags, version) {
            WriteOctet(flags); // the flag is the first byte in the encapsulation --> has influence on alignement
        }

        #endregion IConstructors
        #region IMethods

        /// <summary>
        /// writes the encapsulation to the MessageContentStream
        /// </summary>
        /// <param name="outputStream"></param>
        internal void WriteToMessageContentStream(CdrOutputStream stream) {
            stream.WriteULong(((uint)BaseStream.Length)); // length of the encapsulation
            MemoryStream mem = (MemoryStream) BaseStream;
            stream.WriteOpaque(mem.ToArray());
        }

        #endregion IMethods
    }
    
    
    /// <summary>
    /// this is a big-endian implementation for the endian dependent operation for CDRInput-streams
    /// </summary>
    /// <remarks>
    /// this class is not intended for use by CDRStream users
    /// </remarks>
    internal class CdrStreamBigEndianReadOP : CdrEndianDepInputStreamOp {
        
        #region IFields

        private CdrInputStream m_stream;
        private GiopVersion m_version;

        #endregion IFields
        #region IConstructors

        public CdrStreamBigEndianReadOP(CdrInputStream stream, GiopVersion version) {
            m_stream = stream;
            m_version = version;
        }

        #endregion IConstructors
        #region IMethods
        
        #region read methods depenant on byte ordering
        public short ReadShort() {
            // in stream: two-complement representation
            ushort numberTwoCRepr = ReadUShort();

            byte msbit = (byte)((numberTwoCRepr & 0x8000) >> 15);

            short result;
            if (msbit == 1) {
                // a negative number
                ushort invtwoCNumber = (ushort)(numberTwoCRepr ^ 0xFFFF);
                result = (short)((invtwoCNumber + 1) * -1);
            } else {
                // a positive number
                result = (short)(numberTwoCRepr);
            }
            return result;
        }

        public ushort ReadUShort() {
            m_stream.ForceReadAlign(Aligns.Align2);
            return (ushort) ((m_stream.ReadOctet() << 8) | m_stream.ReadOctet());
        }

        public int ReadLong() {
            // in stream: two-complement representation
            uint numberTwoCRepr = ReadULong();

            byte msbit = (byte)((numberTwoCRepr & 0x80000000) >> 31);

            int result;
            if (msbit == 1) {
                // a negative number
                uint invtwoCNumber = (uint)(numberTwoCRepr ^ 0xFFFFFFFF);
                result = (int)((invtwoCNumber + 1) * -1);
            } else {
                // a positive number
                result = (int)(numberTwoCRepr);
            }
            return result;
        }

        public uint ReadULong() {
            m_stream.ForceReadAlign(Aligns.Align4);
            return (
                (((uint)m_stream.ReadOctet()) << 24) | (((uint)m_stream.ReadOctet()) << 16) | 
                (((uint)m_stream.ReadOctet()) << 8) | ((uint)m_stream.ReadOctet())
            );
        }

        public long ReadLongLong() {
            // in stream: two-complement representation
            ulong numberTwoCRepr = ReadULongLong();

            byte msbit = (byte)((numberTwoCRepr & 0x8000000000000000) >> 63);
            long result;
            if (msbit == 1) {
                // a negative number
                ulong invtwoCNumber = (ulong)(numberTwoCRepr ^ 0xFFFFFFFFFFFFFFFF);
                result = (long)(0 - (invtwoCNumber + 1));
            } else {
                // a positive number
                result = (long)(numberTwoCRepr);
            }
            return result;
        }

        public ulong ReadULongLong() {
            m_stream.ForceReadAlign(Aligns.Align8);
            return (ulong)(
                (((ulong)m_stream.ReadOctet()) << 56) | (((ulong)m_stream.ReadOctet()) << 48)  | 
                (((ulong)m_stream.ReadOctet()) << 40) | (((ulong)m_stream.ReadOctet()) << 32) |                 
                (((ulong)m_stream.ReadOctet()) << 24) | (((ulong)m_stream.ReadOctet()) << 16) | 
                (((ulong)m_stream.ReadOctet()) << 8) | ((ulong)m_stream.ReadOctet()) 
            );

        }

        public float ReadFloat() {
            m_stream.ForceReadAlign(Aligns.Align4);
            byte[] data = m_stream.ReadOpaque(4);
            Array.Reverse((Array)data); // BitConverter wants little endian
            float result = BitConverter.ToSingle(data, 0);
            return result;
        }

        public double ReadDouble() {
            m_stream.ForceReadAlign(Aligns.Align8);
            byte[] data = m_stream.ReadOpaque(8);
            // BitConverter takes an 8 byte array, containing the litte endian representation of the double
            Array.Reverse((Array)data);
            double result = BitConverter.ToDouble(data, 0);
            return result;
        }
        
        private byte[] AppendChar(byte[] oldData) {
            byte[] newData = new byte[oldData.Length+1];
            Array.Copy(oldData, 0, newData, 0, oldData.Length);
            newData[oldData.Length] = m_stream.ReadOctet();
            return newData;
        }
        
        public char ReadWChar() {
            Encoding encoding = CdrStreamHelper.GetWCharEncoding(m_stream.WCharSet, 
                                    CodeSetConversionRegistryBigEndian.GetRegistry());
            byte[] data = new byte[] { m_stream.ReadOctet() };
            while (encoding.GetCharCount(data) < 1) {
                data = AppendChar(data);
            }
            char[] result = encoding.GetChars(data);            
            return result[0];
        }

        public string ReadWString()    {
            Encoding encoding = CdrStreamHelper.GetWCharEncoding(m_stream.WCharSet,
                                    CodeSetConversionRegistryBigEndian.GetRegistry());
            uint length = ReadULong(); 
            byte[] data;
            if ((m_version.Major == 1) && (m_version.Minor <= 1)) { // GIOP 1.1 / 1.0
                length = (length * 2); // only 2 bytes fixed size characters supported
                data = m_stream.ReadOpaque((int)length - 2); // exclude trailing zero
                m_stream.ReadOctet(); // read trailing zero: a wide character
                m_stream.ReadOctet(); // read trailing zero: a wide character
            } else {
                data = m_stream.ReadOpaque((int)length);
            }
            char[] result = encoding.GetChars(data);
            
            return new string(result);
        }

        #endregion

        #endregion IMethods

    }

    /// <summary>
    /// this is a big-endian implementation for the endian dependent operation for CDROutput-streams
    /// </summary>
    /// <remarks>
    /// this class is not intended for use by CDRStream users
    /// </remarks>
    internal class CdrStreamBigEndianWriteOP : CdrEndianDepOutputStreamOp {

        #region IFields

        private CdrOutputStream m_stream;
        private GiopVersion m_version;

        #endregion IFields
        #region IConstructors
        
        public CdrStreamBigEndianWriteOP(CdrOutputStream stream, GiopVersion version) {
            m_stream = stream;
            m_version = version;
        }

        #endregion IConstructors
        #region IMethods
        
        #region write methods dependant on byte ordering
        
        public void WriteShort(short data) {
            m_stream.ForceWriteAlign(Aligns.Align2);
            if (data < 0) {
                // calculate two complement
                ushort positiveNumber = (ushort)(data * -1);
                ushort invNumber = (ushort)(positiveNumber ^ 0xFFFF);
                WriteUShort((ushort)(invNumber + 1));
            } else {
                WriteUShort((ushort)data);
            }
        }

        public void WriteUShort(ushort data) {
            m_stream.ForceWriteAlign(Aligns.Align2);
            m_stream.WriteOctet((byte) ((data & 0xFF00) >> 8));
            m_stream.WriteOctet((byte) ( data & 0x00FF));
        }

        public void WriteLong(int data) {
            m_stream.ForceWriteAlign(Aligns.Align4);
            if (data < 0) {
                // calculate two complement
                uint positiveNumber = (uint)(data * -1);
                uint invNumber = (uint)(positiveNumber ^ 0xFFFFFFFF);
                WriteULong((uint)(invNumber + 1));
            } else {
                WriteULong((uint)data);
            }
        }

        public void WriteULong(uint data) {
            m_stream.ForceWriteAlign(Aligns.Align4);
            m_stream.WriteOctet((byte) ((data & 0xFF000000) >> 24));
            m_stream.WriteOctet((byte) ((data & 0x00FF0000) >> 16));            
            m_stream.WriteOctet((byte) ((data & 0x0000FF00) >> 8));
            m_stream.WriteOctet((byte) ( data & 0x000000FF));
        }

        public void WriteLongLong(long data) {
            m_stream.ForceWriteAlign(Aligns.Align8);
            if (data < 0) {
                // calculate two complement
                ulong positiveNumber = (ulong)(data * -1);
                ulong invNumber = (ulong)(positiveNumber ^ 0xFFFFFFFFFFFFFFFF);
                WriteULongLong((ulong)(invNumber + 1));
            } else {
                WriteULongLong((ulong)data);
            }
        }

        public void WriteULongLong(ulong data) {
            m_stream.ForceWriteAlign(Aligns.Align8);
            m_stream.WriteOctet((byte) ((data & 0xFF00000000000000) >> 56));
            m_stream.WriteOctet((byte) ((data & 0x00FF000000000000) >> 48));
            m_stream.WriteOctet((byte) ((data & 0x0000FF0000000000) >> 40));
            m_stream.WriteOctet((byte) ((data & 0x000000FF00000000) >> 32));
            m_stream.WriteOctet((byte) ((data & 0x00000000FF000000) >> 24));
            m_stream.WriteOctet((byte) ((data & 0x0000000000FF0000) >> 16));            
            m_stream.WriteOctet((byte) ((data & 0x000000000000FF00) >> 8));
            m_stream.WriteOctet((byte) ( data & 0x00000000000000FF));
        }

        public void WriteFloat(float data) {
            m_stream.ForceWriteAlign(Aligns.Align4);
            byte[] byteRep = BitConverter.GetBytes(data);
            Array.Reverse((Array)byteRep);
            m_stream.WriteOpaque(byteRep);
        }

        public void WriteDouble(double data) {
            m_stream.ForceWriteAlign(Aligns.Align8);
            byte[] byteRep = BitConverter.GetBytes(data); // create the little endian representation of the double
            Array.Reverse((Array)byteRep);
            m_stream.WriteOpaque(byteRep);
        }

        public void WriteWChar(char data) {
            Encoding encoding = CdrStreamHelper.GetWCharEncoding(m_stream.WCharSet,
                                                                 CodeSetConversionRegistryBigEndian.GetRegistry());
            byte[] toSend = encoding.GetBytes(new char[] { data } );
            m_stream.WriteOpaque(toSend);
        }

        public void WriteWString(string data) {
            Encoding encoding = CdrStreamHelper.GetWCharEncoding(m_stream.WCharSet,
                                                                 CodeSetConversionRegistryBigEndian.GetRegistry());
            byte[] toSend = encoding.GetBytes(data);
            if ((m_version.Major == 1) && (m_version.Minor <= 1)) { // GIOP 1.0, 1.1
                byte[] sendNew = new byte[toSend.Length + 2];
                Array.Copy((Array)toSend, 0, (Array)sendNew, 0, toSend.Length);
                sendNew[toSend.Length] = 0; // trailing zero: a wide char
                sendNew[toSend.Length + 1] = 0; // trailing zero: a wide char
                m_stream.WriteULong(((uint)toSend.Length / 2) + 1); // number of chars instead of number of bytes, only 2 bytes character supported
                m_stream.WriteOpaque(sendNew);
            } else {
                m_stream.WriteULong((uint)toSend.Length);
                m_stream.WriteOpaque(toSend);
            }
        }

        #endregion write methods dependant on byte ordering

        #endregion IMethods

    }


    /// <summary>
    /// An Instance of this class is used, if the endian flag is not yet specified in a CdrStream.
    /// </summary>
    internal class CdrEndianOpNotSpecified : CdrEndianDepInputStreamOp, CdrEndianDepOutputStreamOp {
        
        #region IMethods
        
        public short ReadShort() {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }
        
        public ushort ReadUShort() {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        public int ReadLong() {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        public uint ReadULong() {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);        
        }

        public long ReadLongLong() {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        public ulong ReadULongLong() {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        public float ReadFloat() {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        public double ReadDouble() {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        public char ReadWChar() {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        public string ReadWString() {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }
   

        public void WriteShort(short data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        public void WriteUShort(ushort data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        public void WriteLong(int data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        public void WriteULong(uint data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        public void WriteLongLong(long data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        public void WriteULongLong(ulong data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        public void WriteFloat(float data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        public void WriteDouble(double data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        public void WriteWChar(char data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        public void WriteWString(string data) {
            // endian flag was not set, operation not available
            throw new INTERNAL(919, CompletionStatus.Completed_MayBe);
        }

        #endregion IMethods
            
    }


}
