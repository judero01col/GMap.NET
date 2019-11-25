// ZipStorer, by Jaime Olivares
// Website: zipstorer.codeplex.com
// Version: 2.35 (March 14, 2010)

using System.Collections.Generic;
using System.Text;

namespace System.IO.Compression
{
    /// <summary>
    ///     Unique class for compression/decompression file. Represents a Zip file.
    /// </summary>
    public class ZipStorer : IDisposable
    {
        /// <summary>
        ///     Compression method enumeration
        /// </summary>
        public enum Compression : ushort
        {
            /// <summary>Uncompressed storage</summary>
            Store = 0,

            /// <summary>Deflate compression method</summary>
            Deflate = 8
        }

        /// <summary>
        ///     Represents an entry in Zip file directory
        /// </summary>
        public struct ZipFileEntry
        {
            /// <summary>Compression method</summary>
            public Compression Method;

            /// <summary>Full path and filename as stored in Zip</summary>
            public string FilenameInZip;

            /// <summary>Original file size</summary>
            public uint FileSize;

            /// <summary>Compressed file size</summary>
            public uint CompressedSize;

            /// <summary>Offset of header information inside Zip storage</summary>
            public uint HeaderOffset;

            /// <summary>Offset of file inside Zip storage</summary>
            public uint FileOffset;

            /// <summary>Size of header information</summary>
            public uint HeaderSize;

            /// <summary>32-bit checksum of entire file</summary>
            public uint Crc32;

            /// <summary>Last modification time of file</summary>
            public DateTime ModifyTime;

            /// <summary>User comment for file</summary>
            public string Comment;

            /// <summary>True if UTF8 encoding for filename and comments, false if default (CP 437)</summary>
            public bool EncodeUTF8;

            /// <summary>Overriden method</summary>
            /// <returns>Filename in Zip</returns>
            public override string ToString()
            {
                return FilenameInZip;
            }
        }

        #region Public fields

        /// <summary>True if UTF8 encoding for filename and comments, false if default (CP 437)</summary>
        public bool EncodeUTF8 = false;

        /// <summary>Force deflate algotithm even if it inflates the stored file. Off by default.</summary>
        public bool ForceDeflating = false;

        #endregion

        #region Private fields

        // List of files to store
        private List<ZipFileEntry> Files = new List<ZipFileEntry>();

        // Filename of storage file
        private string _fileName;

        // Stream object of storage file
        private Stream _zipFileStream;

        // General comment
        private string _comment = "";

        // Central dir image
        private byte[] _centralDirImage;

        // Existing files in zip
        private ushort _existingFiles;

        // File access for Open method
        private FileAccess _access;

        // Static CRC32 Table
        private static UInt32[] _crcTable;

        // Default filename encoder
        private static readonly Encoding DefaultEncoding = Encoding.GetEncoding(437);

        #endregion

        #region Public methods

        // Static constructor. Just invoked once in order to create the CRC32 lookup table.
        static ZipStorer()
        {
            // Generate CRC32 table
            _crcTable = new UInt32[256];
            for (int i = 0; i < _crcTable.Length; i++)
            {
                UInt32 c = (UInt32)i;
                for (int j = 0; j < 8; j++)
                {
                    if ((c & 1) != 0)
                        c = 3988292384 ^ (c >> 1);
                    else
                        c >>= 1;
                }

                _crcTable[i] = c;
            }
        }

        /// <summary>
        ///     Method to create a new storage file
        /// </summary>
        /// <param name="filename">Full path of Zip file to create</param>
        /// <param name="comment">General comment for Zip file</param>
        /// <returns>A valid ZipStorer object</returns>
        public static ZipStorer Create(string filename, string comment)
        {
            Stream stream = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite);

            var zip = Create(stream, comment);
            zip._comment = comment;
            zip._fileName = filename;

            return zip;
        }

        /// <summary>
        ///     Method to create a new zip storage in a stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="comment"></param>
        /// <returns>A valid ZipStorer object</returns>
        public static ZipStorer Create(Stream stream, string comment)
        {
            var zip = new ZipStorer();
            zip._comment = comment;
            zip._zipFileStream = stream;
            zip._access = FileAccess.Write;

            return zip;
        }

        /// <summary>
        ///     Method to open an existing storage file
        /// </summary>
        /// <param name="filename">Full path of Zip file to open</param>
        /// <param name="access">File access mode as used in FileStream constructor</param>
        /// <returns>A valid ZipStorer object</returns>
        public static ZipStorer Open(string filename, FileAccess access)
        {
            var stream = (Stream)new FileStream(filename,
                FileMode.Open,
                access == FileAccess.Read ? FileAccess.Read : FileAccess.ReadWrite);

            var zip = Open(stream, access);
            zip._fileName = filename;

            return zip;
        }

        /// <summary>
        ///     Method to open an existing storage from stream
        /// </summary>
        /// <param name="stream">Already opened stream with zip contents</param>
        /// <param name="access">File access mode for stream operations</param>
        /// <returns>A valid ZipStorer object</returns>
        public static ZipStorer Open(Stream stream, FileAccess access)
        {
            if (!stream.CanSeek && access != FileAccess.Read)
                throw new InvalidOperationException("Stream cannot seek");

            var zip = new ZipStorer();
            //zip.FileName = _filename;
            zip._zipFileStream = stream;
            zip._access = access;

            if (zip.ReadFileInfo())
                return zip;

            throw new InvalidDataException();
        }

        /// <summary>
        ///     Add full contents of a file into the Zip storage
        /// </summary>
        /// <param name="method">Compression method</param>
        /// <param name="pathname">Full path of file to add to Zip storage</param>
        /// <param name="filenameInZip">Filename and path as desired in Zip directory</param>
        /// <param name="comment">Comment for stored file</param>
        public void AddFile(Compression method, string pathname, string filenameInZip, string comment)
        {
            if (_access == FileAccess.Read)
                throw new InvalidOperationException("Writing is not alowed");

            var stream = new FileStream(pathname, FileMode.Open, FileAccess.Read);
            AddStream(method, filenameInZip, stream, File.GetLastWriteTime(pathname), comment);
            stream.Close();
        }

        /// <summary>
        ///     Add full contents of a stream into the Zip storage
        /// </summary>
        /// <param name="method">Compression method</param>
        /// <param name="filenameInZip">Filename and path as desired in Zip directory</param>
        /// <param name="source">Stream object containing the data to store in Zip</param>
        /// <param name="modTime">Modification time of the data to store</param>
        /// <param name="comment">Comment for stored file</param>
        public void AddStream(Compression method, string filenameInZip, Stream source, DateTime modTime,
            string comment)
        {
            if (_access == FileAccess.Read)
                throw new InvalidOperationException("Writing is not alowed");

            long offset;
            if (Files.Count == 0)
                offset = 0;
            else
            {
                var last = Files[Files.Count - 1];
                offset = last.HeaderOffset + last.HeaderSize;
            }

            // Prepare the fileinfo
            var zfe = new ZipFileEntry();
            zfe.Method = method;
            zfe.EncodeUTF8 = EncodeUTF8;
            zfe.FilenameInZip = NormalizedFilename(filenameInZip);
            zfe.Comment = comment == null ? "" : comment;

            // Even though we write the header now, it will have to be rewritten, since we don't know compressed size or crc.
            zfe.Crc32 = 0; // to be updated later
            zfe.HeaderOffset =
                (uint)_zipFileStream.Position; // offset within file of the start of this local record
            zfe.ModifyTime = modTime;

            // Write local header
            WriteLocalHeader(ref zfe);
            zfe.FileOffset = (uint)_zipFileStream.Position;

            // Write file to zip (store)
            Store(ref zfe, source);
            source.Close();

            UpdateCrcAndSizes(ref zfe);

            Files.Add(zfe);
        }

        /// <summary>
        ///     Updates central directory (if pertinent) and close the Zip storage
        /// </summary>
        /// <remarks>This is a required step, unless automatic dispose is used</remarks>
        public void Close()
        {
            if (_zipFileStream == null)
                return;

            if (_access != FileAccess.Read)
            {
                uint centralOffset = (uint)_zipFileStream.Position;
                uint centralSize = 0;

                if (_centralDirImage != null)
                    _zipFileStream.Write(_centralDirImage, 0, _centralDirImage.Length);

                for (int i = 0; i < Files.Count; i++)
                {
                    long pos = _zipFileStream.Position;
                    WriteCentralDirRecord(Files[i]);
                    centralSize += (uint)(_zipFileStream.Position - pos);
                }

                if (_centralDirImage != null)
                    WriteEndRecord(centralSize + (uint)_centralDirImage.Length, centralOffset);
                else
                    WriteEndRecord(centralSize, centralOffset);
            }

            if (_zipFileStream != null)
            {
                _zipFileStream.Flush();
                _zipFileStream.Dispose();
                _zipFileStream = null;
            }
        }

        /// <summary>
        ///     Read all the file records in the central directory
        /// </summary>
        /// <returns>List of all entries in directory</returns>
        public List<ZipFileEntry> ReadCentralDir()
        {
            if (_centralDirImage == null)
                throw new InvalidOperationException("Central directory currently does not exist");

            var result = new List<ZipFileEntry>();

            for (int pointer = 0; pointer < _centralDirImage.Length;)
            {
                uint signature = BitConverter.ToUInt32(_centralDirImage, pointer);
                if (signature != 0x02014b50)
                    break;

                bool encodeUTF8 = (BitConverter.ToUInt16(_centralDirImage, pointer + 8) & 0x0800) != 0;
                ushort method = BitConverter.ToUInt16(_centralDirImage, pointer + 10);
                uint modifyTime = BitConverter.ToUInt32(_centralDirImage, pointer + 12);
                uint crc32 = BitConverter.ToUInt32(_centralDirImage, pointer + 16);
                uint comprSize = BitConverter.ToUInt32(_centralDirImage, pointer + 20);
                uint fileSize = BitConverter.ToUInt32(_centralDirImage, pointer + 24);
                ushort filenameSize = BitConverter.ToUInt16(_centralDirImage, pointer + 28);
                ushort extraSize = BitConverter.ToUInt16(_centralDirImage, pointer + 30);
                ushort commentSize = BitConverter.ToUInt16(_centralDirImage, pointer + 32);
                uint headerOffset = BitConverter.ToUInt32(_centralDirImage, pointer + 42);
                uint headerSize = (uint)(46 + filenameSize + extraSize + commentSize);

                var encoder = encodeUTF8 ? Encoding.UTF8 : DefaultEncoding;

                var zfe = new ZipFileEntry();
                zfe.Method = (Compression)method;
                zfe.FilenameInZip = encoder.GetString(_centralDirImage, pointer + 46, filenameSize);
                zfe.FileOffset = GetFileOffset(headerOffset);
                zfe.FileSize = fileSize;
                zfe.CompressedSize = comprSize;
                zfe.HeaderOffset = headerOffset;
                zfe.HeaderSize = headerSize;
                zfe.Crc32 = crc32;
                zfe.ModifyTime = DosTimeToDateTime(modifyTime);
                if (commentSize > 0)
                    zfe.Comment = encoder.GetString(_centralDirImage,
                        pointer + 46 + filenameSize + extraSize,
                        commentSize);

                result.Add(zfe);
                pointer += 46 + filenameSize + extraSize + commentSize;
            }

            return result;
        }

        /// <summary>
        ///     Copy the contents of a stored file into a physical file
        /// </summary>
        /// <param name="zfe">Entry information of file to extract</param>
        /// <param name="filename">Name of file to store uncompressed data</param>
        /// <returns>True if success, false if not.</returns>
        /// <remarks>Unique compression methods are Store and Deflate</remarks>
        public bool ExtractFile(ZipFileEntry zfe, string filename)
        {
            // Make sure the parent directory exist
            string path = Path.GetDirectoryName(filename);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            // Check it is directory. If so, do nothing
            if (Directory.Exists(filename))
                return true;

            Stream output = new FileStream(filename, FileMode.Create, FileAccess.Write);
            bool result = ExtractFile(zfe, output);
            if (result)
                output.Close();

            File.SetCreationTime(filename, zfe.ModifyTime);
            File.SetLastWriteTime(filename, zfe.ModifyTime);

            return result;
        }

        /// <summary>
        ///     Copy the contents of a stored file into an opened stream
        /// </summary>
        /// <param name="zfe">Entry information of file to extract</param>
        /// <param name="stream">Stream to store the uncompressed data</param>
        /// <returns>True if success, false if not.</returns>
        /// <remarks>Unique compression methods are Store and Deflate</remarks>
        public bool ExtractFile(ZipFileEntry zfe, Stream stream)
        {
            if (!stream.CanWrite)
                throw new InvalidOperationException("Stream cannot be written");

            // check signature
            var signature = new byte[4];
            _zipFileStream.Seek(zfe.HeaderOffset, SeekOrigin.Begin);
            _zipFileStream.Read(signature, 0, 4);
            if (BitConverter.ToUInt32(signature, 0) != 0x04034b50)
                return false;

            // Select input stream for inflating or just reading
            Stream inStream;
            if (zfe.Method == Compression.Store)
                inStream = _zipFileStream;
            else if (zfe.Method == Compression.Deflate)
                inStream = new DeflateStream(_zipFileStream, CompressionMode.Decompress, true);
            else
                return false;

            // Buffered copy
            var buffer = new byte[16384];
            _zipFileStream.Seek(zfe.FileOffset, SeekOrigin.Begin);
            uint bytesPending = zfe.FileSize;
            while (bytesPending > 0)
            {
                int bytesRead = inStream.Read(buffer, 0, (int)Math.Min(bytesPending, buffer.Length));
                stream.Write(buffer, 0, bytesRead);
                bytesPending -= (uint)bytesRead;
            }

            stream.Flush();

            if (zfe.Method == Compression.Deflate)
                inStream.Dispose();
            return true;
        }

        /// <summary>
        ///     Removes one of many files in storage. It creates a new Zip file.
        /// </summary>
        /// <param name="zip">Reference to the current Zip object</param>
        /// <param name="zfes">List of Entries to remove from storage</param>
        /// <returns>True if success, false if not</returns>
        /// <remarks>This method only works for storage of type FileStream</remarks>
        public static bool RemoveEntries(ref ZipStorer zip, List<ZipFileEntry> zfes)
        {
            if (!(zip._zipFileStream is FileStream))
                throw new InvalidOperationException("RemoveEntries is allowed just over streams of type FileStream");


            //Get full list of entries
            var fullList = zip.ReadCentralDir();

            //In order to delete we need to create a copy of the zip file excluding the selected items
            string tempZipName = Path.GetTempFileName();
            string tempEntryName = Path.GetTempFileName();

            try
            {
                var tempZip = Create(tempZipName, string.Empty);

                foreach (var zfe in fullList)
                {
                    if (!zfes.Contains(zfe))
                    {
                        if (zip.ExtractFile(zfe, tempEntryName))
                        {
                            tempZip.AddFile(zfe.Method, tempEntryName, zfe.FilenameInZip, zfe.Comment);
                        }
                    }
                }

                zip.Close();
                tempZip.Close();

                File.Delete(zip._fileName);
                File.Move(tempZipName, zip._fileName);

                zip = Open(zip._fileName, zip._access);
            }
            catch
            {
                return false;
            }
            finally
            {
                if (File.Exists(tempZipName))
                    File.Delete(tempZipName);
                if (File.Exists(tempEntryName))
                    File.Delete(tempEntryName);
            }

            return true;
        }

        #endregion

        #region Private methods

        // Calculate the file offset by reading the corresponding local header
        private uint GetFileOffset(uint headerOffset)
        {
            var buffer = new byte[2];

            _zipFileStream.Seek(headerOffset + 26, SeekOrigin.Begin);
            _zipFileStream.Read(buffer, 0, 2);
            ushort filenameSize = BitConverter.ToUInt16(buffer, 0);
            _zipFileStream.Read(buffer, 0, 2);
            ushort extraSize = BitConverter.ToUInt16(buffer, 0);

            return (uint)(30 + filenameSize + extraSize + headerOffset);
        }

        /* Local file header:
            local file header signature     4 bytes  (0x04034b50)
            version needed to extract       2 bytes
            general purpose bit flag        2 bytes
            compression method              2 bytes
            last mod file time              2 bytes
            last mod file date              2 bytes
            crc-32                          4 bytes
            compressed size                 4 bytes
            uncompressed size               4 bytes
            filename length                 2 bytes
            extra field length              2 bytes

            filename (variable size)
            extra field (variable size)
        */
        private void WriteLocalHeader(ref ZipFileEntry zfe)
        {
            long pos = _zipFileStream.Position;
            var encoder = zfe.EncodeUTF8 ? Encoding.UTF8 : DefaultEncoding;
            var encodedFilename = encoder.GetBytes(zfe.FilenameInZip);

            _zipFileStream.Write(new byte[] {80, 75, 3, 4, 20, 0}, 0, 6); // No extra header
            _zipFileStream.Write(BitConverter.GetBytes((ushort)(zfe.EncodeUTF8 ? 0x0800 : 0)),
                0,
                2); // filename and comment encoding 
            _zipFileStream.Write(BitConverter.GetBytes((ushort)zfe.Method), 0, 2); // zipping method
            _zipFileStream.Write(BitConverter.GetBytes(DateTimeToDosTime(zfe.ModifyTime)),
                0,
                4); // zipping date and time
            _zipFileStream.Write(new byte[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                0,
                12); // unused CRC, un/compressed size, updated later
            _zipFileStream.Write(BitConverter.GetBytes((ushort)encodedFilename.Length), 0, 2); // filename length
            _zipFileStream.Write(BitConverter.GetBytes((ushort)0), 0, 2); // extra length

            _zipFileStream.Write(encodedFilename, 0, encodedFilename.Length);
            zfe.HeaderSize = (uint)(_zipFileStream.Position - pos);
        }

        /* Central directory's File header:
            central file header signature   4 bytes  (0x02014b50)
            version made by                 2 bytes
            version needed to extract       2 bytes
            general purpose bit flag        2 bytes
            compression method              2 bytes
            last mod file time              2 bytes
            last mod file date              2 bytes
            crc-32                          4 bytes
            compressed size                 4 bytes
            uncompressed size               4 bytes
            filename length                 2 bytes
            extra field length              2 bytes
            file comment length             2 bytes
            disk number start               2 bytes
            internal file attributes        2 bytes
            external file attributes        4 bytes
            relative offset of local header 4 bytes

            filename (variable size)
            extra field (variable size)
            file comment (variable size)
        */
        private void WriteCentralDirRecord(ZipFileEntry zfe)
        {
            var encoder = zfe.EncodeUTF8 ? Encoding.UTF8 : DefaultEncoding;
            var encodedFilename = encoder.GetBytes(zfe.FilenameInZip);
            var encodedComment = encoder.GetBytes(zfe.Comment);

            _zipFileStream.Write(new byte[] {80, 75, 1, 2, 23, 0xB, 20, 0}, 0, 8);
            _zipFileStream.Write(BitConverter.GetBytes((ushort)(zfe.EncodeUTF8 ? 0x0800 : 0)),
                0,
                2); // filename and comment encoding 
            _zipFileStream.Write(BitConverter.GetBytes((ushort)zfe.Method), 0, 2); // zipping method
            _zipFileStream.Write(BitConverter.GetBytes(DateTimeToDosTime(zfe.ModifyTime)),
                0,
                4); // zipping date and time
            _zipFileStream.Write(BitConverter.GetBytes(zfe.Crc32), 0, 4); // file CRC
            _zipFileStream.Write(BitConverter.GetBytes(zfe.CompressedSize), 0, 4); // compressed file size
            _zipFileStream.Write(BitConverter.GetBytes(zfe.FileSize), 0, 4); // uncompressed file size
            _zipFileStream.Write(BitConverter.GetBytes((ushort)encodedFilename.Length), 0, 2); // Filename in zip
            _zipFileStream.Write(BitConverter.GetBytes((ushort)0), 0, 2); // extra length
            _zipFileStream.Write(BitConverter.GetBytes((ushort)encodedComment.Length), 0, 2);

            _zipFileStream.Write(BitConverter.GetBytes((ushort)0), 0, 2); // disk=0
            _zipFileStream.Write(BitConverter.GetBytes((ushort)0), 0, 2); // file type: binary
            _zipFileStream.Write(BitConverter.GetBytes((ushort)0), 0, 2); // Internal file attributes
            _zipFileStream.Write(BitConverter.GetBytes((ushort)0x8100),
                0,
                2); // External file attributes (normal/readable)
            _zipFileStream.Write(BitConverter.GetBytes(zfe.HeaderOffset), 0, 4); // Offset of header

            _zipFileStream.Write(encodedFilename, 0, encodedFilename.Length);
            _zipFileStream.Write(encodedComment, 0, encodedComment.Length);
        }

        /* End of central dir record:
            end of central dir signature    4 bytes  (0x06054b50)
            number of this disk             2 bytes
            number of the disk with the
            start of the central directory  2 bytes
            total number of entries in
            the central dir on this disk    2 bytes
            total number of entries in
            the central dir                 2 bytes
            size of the central directory   4 bytes
            offset of start of central
            directory with respect to
            the starting disk number        4 bytes
            zipfile comment length          2 bytes
            zipfile comment (variable size)
        */
        private void WriteEndRecord(uint size, uint offset)
        {
            var encoder = EncodeUTF8 ? Encoding.UTF8 : DefaultEncoding;
            var encodedComment = encoder.GetBytes(_comment);

            _zipFileStream.Write(new byte[] {80, 75, 5, 6, 0, 0, 0, 0}, 0, 8);
            _zipFileStream.Write(BitConverter.GetBytes((ushort)Files.Count + _existingFiles), 0, 2);
            _zipFileStream.Write(BitConverter.GetBytes((ushort)Files.Count + _existingFiles), 0, 2);
            _zipFileStream.Write(BitConverter.GetBytes(size), 0, 4);
            _zipFileStream.Write(BitConverter.GetBytes(offset), 0, 4);
            _zipFileStream.Write(BitConverter.GetBytes((ushort)encodedComment.Length), 0, 2);
            _zipFileStream.Write(encodedComment, 0, encodedComment.Length);
        }

        // Copies all source file into storage file
        private void Store(ref ZipFileEntry zfe, Stream source)
        {
            var buffer = new byte[16384];
            int bytesRead;
            uint totalRead = 0;
            Stream outStream;

            long posStart = _zipFileStream.Position;
            long sourceStart = source.Position;

            if (zfe.Method == Compression.Store)
                outStream = _zipFileStream;
            else
                outStream = new DeflateStream(_zipFileStream, CompressionMode.Compress, true);

            zfe.Crc32 = 0 ^ 0xffffffff;

            do
            {
                bytesRead = source.Read(buffer, 0, buffer.Length);
                totalRead += (uint)bytesRead;
                if (bytesRead > 0)
                {
                    outStream.Write(buffer, 0, bytesRead);

                    for (uint i = 0; i < bytesRead; i++)
                    {
                        zfe.Crc32 = _crcTable[(zfe.Crc32 ^ buffer[i]) & 0xFF] ^ (zfe.Crc32 >> 8);
                    }
                }
            } while (bytesRead == buffer.Length);

            outStream.Flush();

            if (zfe.Method == Compression.Deflate)
                outStream.Dispose();

            zfe.Crc32 ^= 0xffffffff;
            zfe.FileSize = totalRead;
            zfe.CompressedSize = (uint)(_zipFileStream.Position - posStart);

            // Verify for real compression
            if (zfe.Method == Compression.Deflate && !ForceDeflating && source.CanSeek &&
                zfe.CompressedSize > zfe.FileSize)
            {
                // Start operation again with Store algorithm
                zfe.Method = Compression.Store;
                _zipFileStream.Position = posStart;
                _zipFileStream.SetLength(posStart);
                source.Position = sourceStart;
                Store(ref zfe, source);
            }
        }

        /* DOS Date and time:
            MS-DOS date. The date is a packed value with the following format. Bits Description 
                0-4 Day of the month (1–31) 
                5-8 Month (1 = January, 2 = February, and so on) 
                9-15 Year offset from 1980 (add 1980 to get actual year) 
            MS-DOS time. The time is a packed value with the following format. Bits Description 
                0-4 Second divided by 2 
                5-10 Minute (0–59) 
                11-15 Hour (0–23 on a 24-hour clock) 
        */
        private uint DateTimeToDosTime(DateTime dt)
        {
            return (uint)(
                (dt.Second / 2) | (dt.Minute << 5) | (dt.Hour << 11) |
                (dt.Day << 16) | (dt.Month << 21) | ((dt.Year - 1980) << 25));
        }

        private DateTime DosTimeToDateTime(uint dt)
        {
            return new DateTime(
                (int)(dt >> 25) + 1980,
                (int)(dt >> 21) & 15,
                (int)(dt >> 16) & 31,
                (int)(dt >> 11) & 31,
                (int)(dt >> 5) & 63,
                (int)(dt & 31) * 2);
        }

        /* CRC32 algorithm
          The 'magic number' for the CRC is 0xdebb20e3.  
          The proper CRC pre and post conditioning
          is used, meaning that the CRC register is
          pre-conditioned with all ones (a starting value
          of 0xffffffff) and the value is post-conditioned by
          taking the one's complement of the CRC residual.
          If bit 3 of the general purpose flag is set, this
          field is set to zero in the local header and the correct
          value is put in the data descriptor and in the central
          directory.
        */
        private void UpdateCrcAndSizes(ref ZipFileEntry zfe)
        {
            long lastPos = _zipFileStream.Position; // remember position

            _zipFileStream.Position = zfe.HeaderOffset + 8;
            _zipFileStream.Write(BitConverter.GetBytes((ushort)zfe.Method), 0, 2); // zipping method

            _zipFileStream.Position = zfe.HeaderOffset + 14;
            _zipFileStream.Write(BitConverter.GetBytes(zfe.Crc32), 0, 4); // Update CRC
            _zipFileStream.Write(BitConverter.GetBytes(zfe.CompressedSize), 0, 4); // Compressed size
            _zipFileStream.Write(BitConverter.GetBytes(zfe.FileSize), 0, 4); // Uncompressed size

            _zipFileStream.Position = lastPos; // restore position
        }

        // Replaces backslashes with slashes to store in zip header
        private string NormalizedFilename(string filename)
        {
            filename = filename.Replace('\\', '/');

            int pos = filename.IndexOf(':');
            if (pos >= 0)
                filename = filename.Remove(0, pos + 1);

            return filename.Trim('/');
        }

        // Reads the end-of-central-directory record
        private bool ReadFileInfo()
        {
            if (_zipFileStream.Length < 22)
                return false;

            try
            {
                _zipFileStream.Seek(-17, SeekOrigin.End);
                var br = new BinaryReader(_zipFileStream);
                do
                {
                    _zipFileStream.Seek(-5, SeekOrigin.Current);
                    UInt32 sig = br.ReadUInt32();
                    if (sig == 0x06054b50)
                    {
                        _zipFileStream.Seek(6, SeekOrigin.Current);

                        UInt16 entries = br.ReadUInt16();
                        Int32 centralSize = br.ReadInt32();
                        UInt32 centralDirOffset = br.ReadUInt32();
                        UInt16 commentSize = br.ReadUInt16();

                        // check if comment field is the very last data in file
                        if (_zipFileStream.Position + commentSize != _zipFileStream.Length)
                            return false;

                        // Copy entire central directory to a memory buffer
                        _existingFiles = entries;
                        _centralDirImage = new byte[centralSize];
                        _zipFileStream.Seek(centralDirOffset, SeekOrigin.Begin);
                        _zipFileStream.Read(_centralDirImage, 0, centralSize);

                        // Leave the pointer at the begining of central dir, to append new files
                        _zipFileStream.Seek(centralDirOffset, SeekOrigin.Begin);
                        return true;
                    }
                } while (_zipFileStream.Position > 0);
            }
            catch { }

            return false;
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        ///     Closes the Zip file stream
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        #endregion
    }
}
