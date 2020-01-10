using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Syroot.BinaryData;
using Syroot.Maths;

namespace BfshaLibrary.Core
{
    /// <summary>
    /// Saves the hierachy and data of a <see cref="Bfsha.BfshaFile"/>.
    /// </summary>
    public class BfshaFileSaver : BinaryDataWriter
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        //For RLT
        internal const int Section1 = 1;
        internal const int Section2 = 2;
        internal const int Section3 = 3;
        internal const int Section4 = 4;
        internal const int Section5 = 5;

        /// <summary>
        /// Gets or sets a data block alignment typically seen with <see cref="Buffer.Data"/>.
        /// </summary>
        internal const uint AlignmentSmall = 0x40;

        // ---- FIELDS -------------------------------------------------------------------------------------------------

        //These to save pointer info back to
        private uint _ofsFileName;
        private uint _ofsFileNameString;

        private uint _ofsFileSize;
        private uint _ofsStringPool;
        private uint _ofsEndOfBlock;
        private uint _ofsRelocationTable;

        private List<ItemEntry> _savedItems;
        private IDictionary<string, StringEntry> _savedStrings;
        private IDictionary<object, BlockEntry> _savedBlocks;

        private List<long> _savedHeaderBlockPositions;

        private string _fileName;

        private List<RelocationEntry> _savedSection1Entries;
        private List<RelocationEntry> _savedSection2Entries;
        private List<RelocationEntry> _savedSection3Entries;
        private List<RelocationEntry> _savedSection4Entries;
        private List<RelocationEntry> _savedSection5Entries;

        private List<RelocationSection> _savedRelocatedSections;

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="BfshaFileSaver"/> class saving data from the given
        /// <paramref name="BfshaFile"/> into the specified <paramref name="stream"/> which is optionally left open.
        /// </summary>
        /// <param name="BfshaFile">The <see cref="Bfsha.BfshaFile"/> instance to save data from.</param>
        /// <param name="stream">The <see cref="Stream"/> to save data into.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the stream open after writing, otherwise <c>false</c>.</param>
        internal BfshaFileSaver(BfshaFile bfshaFile, Stream stream, bool leaveOpen)
            : base(stream, Encoding.ASCII, leaveOpen)
        {
            ByteOrder = ByteOrder.LittleEndian;
            BfshaFile = bfshaFile;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="BfshaFileSaver"/> class saving data from the given
        /// <paramref name="BfshaFile"/> into the specified <paramref name="stream"/> which is optionally left open.
        /// </summary>
        /// <param name="BfshaFile">The <see cref="Bfsha.BfshaFile"/> instance to save data from.</param>
        /// <param name="stream">The <see cref="Stream"/> to save data into.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the stream open after writing, otherwise <c>false</c>.</param>
        internal BfshaFileSaver(BnshFile bnshFile, Stream stream, bool leaveOpen)
            : base(stream, Encoding.ASCII, leaveOpen)
        {
            ByteOrder = ByteOrder.LittleEndian;
            BnshFile = bnshFile;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="BfshaFileSaver"/> class for the file with the given
        /// <paramref name="fileName"/>.
        /// </summary>
        /// <param name="BfshaFile">The <see cref="Bfsha.BfshaFile"/> instance to save.</param>
        /// <param name="fileName">The name of the file to save the data into.</param>
        internal BfshaFileSaver(BfshaFile BfshaFile, string fileName)
            : this(BfshaFile, new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read), false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BfshaFileSaver"/> class for the file with the given
        /// <paramref name="fileName"/>.
        /// </summary>
        /// <param name="BfshaFile">The <see cref="Bfsha.BnshFile"/> instance to save.</param>
        /// <param name="fileName">The name of the file to save the data into.</param>
        internal BfshaFileSaver(BnshFile BnshFile, string fileName)
            : this(BnshFile, new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read), false)
        {
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the saved <see cref="Bfsha.BfshaFile"/> instance.
        /// </summary>
        internal BfshaFile BfshaFile { get; }

        /// <summary>
        /// Gets the saved <see cref="Bfsha.BnshFile"/> instance.
        /// </summary>
        internal BnshFile BnshFile { get; }

        /// <summary>
        /// Gets the current index when writing lists or dicts.
        /// </summary>
        internal int CurrentIndex { get; private set; }

        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        private void SetupLists()
        {
            _savedItems = new List<ItemEntry>();
            _savedStrings = new SortedDictionary<string, StringEntry>(ResStringComparer.Instance);
            _savedBlocks = new Dictionary<object, BlockEntry>();
            _savedHeaderBlockPositions = new List<long>();
            _savedRelocatedSections = new List<RelocationSection>();
            _savedSection1Entries = new List<RelocationEntry>();
            _savedSection2Entries = new List<RelocationEntry>();
            _savedSection3Entries = new List<RelocationEntry>();
            _savedSection4Entries = new List<RelocationEntry>();
            _savedSection5Entries = new List<RelocationEntry>();
        }

        /// <summary>
        /// Starts serializing the data from the <see cref="BfshaFile"/> root.
        /// </summary>
        internal void Execute()
        {
            // Create queues fetching the names for the string pool and data blocks to store behind the headers.
            SetupLists();

            if (BnshFile != null)
            {
                ((IResData)BnshFile).Save(this);
                SaveRelocateEntryToSection(Position, 1, 1, 0, Section1, "Shader Variation");
                WriteOffset(BnshFile.shaderVariationArrayOffset);

                //Save variation headers
                for (int i = 0; i < BnshFile.ShaderVariations?.Count; i++)
                    ((IResData)BnshFile.ShaderVariations[i]).Save(this);

                //Save programs
                for (int i = 0; i < BnshFile.ShaderVariations?.Count; i++)
                {
                
                }

                //After all the variations are saved, apply the obj data
                for (int i = 0; i < BnshFile.ShaderVariations?.Count; i++)
                {
                    var variation = BnshFile.ShaderVariations[i];
                    if (variation.SourceProgram != null)
                    {
                        WriteOffset(variation._position);
                        WriteProgram(variation.SourceProgram);
                    }
                    if (variation.IntermediateProgram != null)
                    {
                        WriteOffset(variation._position + 8);
                        WriteProgram(variation.IntermediateProgram);
                    }
                    if (variation.BinaryProgram != null)
                    {
                        WriteOffset(variation._position + 16);
                        WriteProgram(variation.BinaryProgram);
                    }


                    if (variation.SourceProgram != null) {
                        WriteOffset(variation.SourceProgram.PtrMemPos);
                        Write(variation.SourceProgram.MemoryData);
                    }
                    if (variation.BinaryProgram != null) {
                        WriteOffset(variation.BinaryProgram.PtrMemPos);
                        Write(variation.BinaryProgram.MemoryData);
                    }
                }

                //Then save the actual data from the shaders
                //Create a lookup table for all the shader code to point to to reference
                Dictionary<byte[], long> VertexShaderCodeLookup = new Dictionary<byte[], long>(new ByteArrayComparer());
                Dictionary<byte[], long> PixelShaderCodeLookup = new Dictionary<byte[], long>(new ByteArrayComparer());

                for (int i = 0; i < BnshFile.ShaderVariations?.Count; i++)
                {
                    foreach (var program in BnshFile.ShaderVariations[i].GetPrograms())
                    {
                        var info = program.ShaderInfoData;
                        var shaders = info.GetAllShaders();
                        for (int j = 0; j < shaders.Length; j++)
                        {
                            if (shaders[j] is ShaderCodeDataCompressed)
                            {
                                var codeHeader = shaders[j] as ShaderCodeDataCompressed;

                                if (shaders[j] == info.VertexShaderCode)
                                    SaveShaderCodeData(VertexShaderCodeLookup, codeHeader);
                                else if (shaders[j] == info.PixelShaderCode)
                                    SaveShaderCodeData(PixelShaderCodeLookup, codeHeader);
                                else
                                    throw new Exception("Unsupported shader type! " + shaders[j]);
                            }
                            else if (shaders[j] is ShaderCodeDataBinary)
                            {
                                var codeHeader = shaders[j] as ShaderCodeDataBinary;

                                for (int f = 0; f < codeHeader.BinaryData.Count; f++)
                                {
                                    if (shaders[j] == info.VertexShaderCode)
                                        SaveShaderCodeData(VertexShaderCodeLookup, codeHeader, f);
                                    else if (shaders[j] == info.PixelShaderCode)
                                        SaveShaderCodeData(PixelShaderCodeLookup, codeHeader, f);
                                    else
                                        throw new Exception("Unsupported shader type! " + shaders[j]);
                                }
                            }
                            else if (shaders[j] is ShaderCodeDataSource)
                            {
                                var codeHeader = shaders[j] as ShaderCodeDataSource;
                                for (int f = 0; f < codeHeader.SourceData.Count; f++)
                                {
                                    if (shaders[j] == info.VertexShaderCode)
                                        SaveShaderCodeData(VertexShaderCodeLookup, codeHeader, f);
                                    else if (shaders[j] == info.PixelShaderCode)
                                        SaveShaderCodeData(PixelShaderCodeLookup, codeHeader, f);
                                    else
                                        throw new Exception("Unsupported shader type! " + shaders[j]);
                                }
                            }
                        }
                    }
                }
                Align(16);

                VertexShaderCodeLookup.Clear();
                PixelShaderCodeLookup.Clear();
            }
            else
            {
                // Store the headers recursively and satisfy offsets to them, then the string pool and data blocks.
                ((IResData)BfshaFile).Save(this);


            }

            // Satisfy offsets, strings, and data blocks.
        //    WriteOffsets();
            WriteStrings();
            WriteBlocks();

            //First setup the values for RLT
            SetupRelocationTable();
            //Now write
            WriteRelocationTable();

            //Now determine block sizes!!
            //A note regarding these. They don't use alignment
            for (int i = 0; i < _savedHeaderBlockPositions.Count; i++)
            {
                Position = _savedHeaderBlockPositions[i];

                if (i == _savedHeaderBlockPositions.Count - 1)
                {
                    Write(0);
                    Write(_ofsEndOfBlock - _savedHeaderBlockPositions[i]); //Size of string table to relocation table
                }
                else
                {
                    if (i < _savedHeaderBlockPositions.Count - 1)
                    {
                        uint blockSize = (uint)(_savedHeaderBlockPositions[i + 1] - _savedHeaderBlockPositions[i]);
                        WriteHeaderBlock(blockSize, blockSize);
                    }
                }
            }

            //Save the file name. Goes directly to name instead of size
            Position = _ofsFileName;
            Write(_ofsFileNameString + 2);

            // Save final file size into root header at the provided offset.
            Position = _ofsFileSize;
            Write((uint)BaseStream.Length);

            Flush();
        }

        private void WriteProgram(ShaderProgram program)
        {
            ((IResData)program).Save(this);

            var info = program.ShaderInfoData;

            //First save the headers
            if (info.VertexShaderCode != null)
            {
                WriteOffset(info.PtrVertexShaderCodePos);
                ((IResData)info.VertexShaderCode).Save(this);
            }
            if (info.PixelShaderCode != null)
            {
                WriteOffset(info.PtrPixelShaderCodePos);
                ((IResData)info.PixelShaderCode).Save(this);
            }
            if (info.TessellationControlShaderCode != null)
            {
                WriteOffset(info.PtrTessellatioControlShaderCodePos);
                ((IResData)info.TessellationControlShaderCode).Save(this);
            }
            if (info.GeometryShaderCode != null)
            {
                WriteOffset(info.PtrGeometryShaderCodePos);
                ((IResData)info.GeometryShaderCode).Save(this);
            }
            if (info.TessellationEvaluationShaderCode != null)
            {
                WriteOffset(info.PtrTessellationEvaluationShaderCodePos);
                ((IResData)info.TessellationEvaluationShaderCode).Save(this);
            }

            //After all of them are saved, save the pointer arrays and the size arrays
            var shaders = info.GetAllShaders();

            Console.WriteLine($"shaders {shaders.Length}");
            for (int j = 0; j < shaders.Length; j++)
            {
                if (shaders[j] is ShaderCodeDataBinary)
                {
                    var codeHeader = shaders[j] as ShaderCodeDataBinary;

                }
                else if (shaders[j] is ShaderCodeDataSource)
                {
                    var codeHeader = shaders[j] as ShaderCodeDataSource;
                    //Reserve spaces for pointers first to write later
                    WriteOffset(codeHeader.PtrCodeOffsetsPos);

                    codeHeader.PtrCodeOffsets = Position;
                    for (int f = 0; f < codeHeader.SourceData.Count; f++)
                        Write(0L);

                    //Save sizes
                    WriteOffset(codeHeader.PtrCodeSizesPos);
                    for (int f = 0; f < codeHeader.SourceData.Count; f++)
                    {
                        Console.WriteLine($"SourceData {codeHeader.SourceData[f].Length} {Position}");
                        Write(codeHeader.SourceData[f].Length);
                    }
                }
            }
        }

        private void SaveShaderCodeData(Dictionary<byte[], long> lookup, ShaderCodeDataCompressed codeHeader)
        {
            if (!lookup.ContainsKey(codeHeader.CompressedData))
            {
                lookup.Add(codeHeader.CompressedData, Position);

                WriteOffset(codeHeader.PtrCodeOffsets);
                Write(codeHeader.CompressedData);
                Align(16);
            }
            else
            {
                var pos = lookup[codeHeader.CompressedData];
                WriteOffset(codeHeader.PtrCodeOffsets, pos);
            }
        }

        private void SaveShaderCodeData(Dictionary<byte[], long> lookup, ShaderCodeDataBinary codeHeader, int index)
        {
            if (!lookup.ContainsKey(codeHeader.BinaryData[index]))
            {
                lookup.Add(codeHeader.BinaryData[index], Position);

                if (index == 0)
                    WriteOffset(codeHeader.PtrCodeOffset2Pos);
                else
                    WriteOffset(codeHeader.PtrCodeOffset1Pos);
                Write(codeHeader.BinaryData[index]);
                Align(16);
            }
            else
            {
                var pos = lookup[codeHeader.BinaryData[index]];
                if (index == 0)
                    WriteOffset(codeHeader.PtrCodeOffset2Pos, pos);
                else
                    WriteOffset(codeHeader.PtrCodeOffset1Pos, pos);
            }
        }

        private void SaveShaderCodeData(Dictionary<byte[], long> lookup, ShaderCodeDataSource codeHeader, int index)
        {
            if (!lookup.ContainsKey(codeHeader.SourceData[index]))
            {
                lookup.Add(codeHeader.SourceData[index], Position);

                WriteOffset(codeHeader.PtrCodeOffsets + (index * 8));
                Write(codeHeader.SourceData[index]);
                Align(16);
            }
            else
            {
                var pos = lookup[codeHeader.SourceData[index]];
                WriteOffset(codeHeader.PtrCodeOffsets + (index * 8), pos);
            }
        }

        class ByteArrayComparer : IEqualityComparer<byte[]>
        {
            public bool Equals(byte[] left, byte[] right)
            {
                if (left == null || right == null)
                {
                    return left == right;
                }
                return left.SequenceEqual(right);
            }
            public int GetHashCode(byte[] key)
            {
                if (key == null)
                    throw new ArgumentNullException("key");
                return key.Sum(b => b);
            }
        }

        internal void SetupRelocationTable()
        {
            if (BnshFile != null)
            {
                //
                //Section 2 objdata
                //Section 3 shadr data
            }
        }

        private void SaveShaderCodeHeader(ShaderCodeData codeData)
        {

        }

        internal long SaveOffset()
        {
            long pos = Position;
            Write(0L);
            return pos;
        }

        private void ApplyOffsets()
        {
            // Store all queued items. Iterate via index as subsequent calls append to the list.
            for (int i = 0; i < _savedItems.Count; i++)
            {
                ItemEntry entry = _savedItems[i];
                if (entry.Target != null) continue; // Ignore if it has already been written (list or dict elements).

                Align(4);
                switch (entry.Type)
                {
                    case ItemEntryType.List:
                        IEnumerable<IResData> list = (IEnumerable<IResData>)entry.Data;
                        // Check if the first item has already been written by a previous dict.
                        if (TryGetItemEntry(list.First(), ItemEntryType.ResData, out ItemEntry firstElement))
                        {
                            entry.Target = firstElement.Target;
                        }
                        else
                        {
                            entry.Target = (uint)Position;
                            CurrentIndex = 0;
                            foreach (IResData element in list)
                            {
                                _savedItems.Add(new ItemEntry(element, ItemEntryType.ResData, target: (uint)Position,
                                    index: CurrentIndex));
                                element.Save(this);
                                CurrentIndex++;
                            }
                        }
                        break;

                    case ItemEntryType.Dict:
                    case ItemEntryType.ResData:
                        entry.Target = (uint)Position;
                        CurrentIndex = entry.Index;
                        ((IResData)entry.Data).Save(this);
                        break;

                    case ItemEntryType.Custom:
                        entry.Target = (uint)Position;
                        entry.Callback.Invoke();
                        break;
                }
            }
        }

        internal void SaveFileNameString(string Name, bool Relocate = false)
        {
            _fileName = Name;
            _ofsFileName = (uint)Position;
            Write(0);
        }

        private void WriteRelocationTable()
        {
            if (BfshaFile != null)
                Align((int)BfshaFile.DataAlignment);
            if (BnshFile != null)
                Align((int)BnshFile.DataAlignment);
            uint relocationTableOffset = (uint)Position;
            WriteSignature("_RLT");
            _ofsEndOfBlock = (uint)Position;
            Write(relocationTableOffset);
            Write(_savedRelocatedSections.Count);
            Write(0); //padding

            foreach (RelocationSection section in _savedRelocatedSections)
            {
                Write(0L); //padding
                Write(section.Position);
                Write(section.Size);
                Write(section.EntryIndex);
                Write(section.Entries.Count);
            }

            foreach (RelocationSection section in _savedRelocatedSections)
            {
                foreach (RelocationEntry entry in section.Entries)
                {
                    Write(entry.Position);
                    Write((ushort)entry.StructCount);
                    Write((byte)entry.OffsetCount);
                    Write((byte)entry.PadingCount);
                }
            }

            using (TemporarySeek(_ofsRelocationTable, SeekOrigin.Begin))
            {
                Write(relocationTableOffset);
            }
        }

        /// <summary>
        /// Reserves space for an offset to the <paramref name="resData"/> written later.
        /// </summary>
        /// <param name="resData">The <see cref="IResData"/> to save.</param>
        /// <param name="index">The index of the element, used for instances referenced by a <see cref="ResDict"/>.
        /// </param>
        [DebuggerStepThrough]
        internal void Save(IResData resData, int index = -1)
        {
            if (resData == null)
            {
                Write(0);
                return;
            }
            if (TryGetItemEntry(resData, ItemEntryType.ResData, out ItemEntry entry))
            {
                entry.Offsets.Add((uint)Position);
                entry.Index = index;
            }
            else
            {
                _savedItems.Add(new ItemEntry(resData, ItemEntryType.ResData, (uint)Position, index: index));
            }
            Write(UInt32.MaxValue);
            Write(0);

        }

        /// <summary>
        /// Reserves space for the <see cref="Bfres.ResFile"/> memory pool field which is automatically filled later.
        /// </summary>
        [DebuggerStepThrough]
        internal void SaveRelocationTablePointerPointer()
        {
            _ofsRelocationTable = (uint)Position;
            Write(0);
        }

        /// <summary>
        /// Reserves space for the <see cref="Bfsha.BfshaFile"/> file size field which is automatically filled later.
        /// </summary>
        [DebuggerStepThrough]
        internal void SaveFieldFileSize()
        {
            _ofsFileSize = (uint)Position;
            Write(0);
        }

        /// <summary>
        /// Reserves space for the <see cref="Bfsha.BfshaFile"/> string pool size and offset fields which are automatically
        /// filled later.
        /// </summary>
        [DebuggerStepThrough]
        internal void SaveFieldStringPool()
        {
            _ofsStringPool = (uint)Position;
            Write(0L);
        }

        /// <summary>
        /// Reserves space for an offset to the <paramref name="list"/> written later.
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="IResData"/> elements.</typeparam>
        /// <param name="list">The <see cref="IList{T}"/> to save.</param>
        [DebuggerStepThrough]
        internal void SaveList<T>(IEnumerable<T> list, bool IsUint32 = false)
            where T : IResData, new()
        {
            if (list?.Count() == 0)
            {
                Write((long)0);
                return;
            }
            // The offset to the list is the offset to the first element.
            if (TryGetItemEntry(list.First(), ItemEntryType.ResData, out ItemEntry entry))
            {
                entry.Offsets.Add((uint)Position);
                entry.Index = 0;
            }
            else
            {
                // Queue all elements of the list.
                int index = 0;
                foreach (T element in list)
                {
                    if (index == 0)
                    {
                        // Add with offset to the first item for the list.
                        _savedItems.Add(new ItemEntry(element, ItemEntryType.ResData, (uint)Position, index: index));
                    }
                    else
                    {
                        // Add without offsets existing yet.
                        _savedItems.Add(new ItemEntry(element, ItemEntryType.ResData, index: index));
                    }
                    index++;
                }
            }
            Write(UInt32.MaxValue);

            if (IsUint32 == false) //Add 0s to read as Uint64 (Default). A few offsets like relocation table would set this to true
                Write(0);
        }


        /// <summary>
        /// Reserves space for an offset to the <paramref name="dict"/> written later.
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="IResData"/> element values.</typeparam>
        /// <param name="dict">The <see cref="ResDict{T}"/> to save.</param>
        [DebuggerStepThrough]
        internal void SaveDict(ResDict dict, long ShiftPos = 0)
        {
            long NewPos = Position;
            if (ShiftPos != 0)
            {
                NewPos = ShiftPos;
            }

            if (dict?.Count == 0 && ShiftPos == 0)
            {
                Write(0L);
                return;
            }
            if (TryGetItemEntry(dict, ItemEntryType.Dict, out ItemEntry entry))
            {
                entry.Offsets.Add((uint)NewPos);
            }
            else
            {
                _savedItems.Add(new ItemEntry(dict, ItemEntryType.Dict, (uint)NewPos));
            }

            if (ShiftPos == 0)
            {
                Write(UInt32.MaxValue);
                Write(0);
            }
        }

        /// <summary>
        /// Reserves space for an offset to the <paramref name="data"/> written later with the
        /// <paramref name="callback"/>.
        /// </summary>
        /// <param name="data">The data to save.</param>
        /// <param name="callback">The <see cref="Action"/> to invoke to write the data.</param>
        [DebuggerStepThrough]
        internal void SaveCustom(object data, Action callback)
        {
            if (data == null)
            {
                Write((long)0);
                return;
            }
            if (TryGetItemEntry(data, ItemEntryType.Custom, out ItemEntry entry))
            {
                entry.Offsets.Add((uint)Position);
            }
            else
            {
                _savedItems.Add(new ItemEntry(data, ItemEntryType.Custom, (uint)Position, callback: callback));
            }
            Write(UInt32.MaxValue);
            Write(0);
        }

        /// <summary>
        /// Reserves space for an offset to the <paramref name="str"/> written later in the string pool with the
        /// specified <paramref name="encoding"/>.
        /// </summary>
        /// <param name="str">The name to save.</param>
        /// <param name="encoding">The <see cref="Encoding"/> in which the name will be stored.</param>
        [DebuggerStepThrough]
        internal void SaveString(string str, Encoding encoding = null)
        {
            if (str == null)
            {
                Write((long)0);
                return;
            }
            if (_savedStrings.TryGetValue(str, out StringEntry entry))
            {
                entry.Offsets.Add((uint)Position);
            }
            else
            {
                _savedStrings.Add(str, new StringEntry((uint)Position, encoding));
            }

            Write(UInt32.MaxValue);
            Write(0);
        }

        /// <summary>
        /// Reserves space for offsets to the <paramref name="strings"/> written later in the string pool with the
        /// specified <paramref name="encoding"/>
        /// </summary>
        /// <param name="strings">The names to save.</param>
        /// <param name="encoding">The <see cref="Encoding"/> in which the names will be stored.</param>
        [DebuggerStepThrough]
        internal void SaveStrings(IEnumerable<string> strings, Encoding encoding = null)
        {
            foreach (string str in strings)
            {
                SaveString(str, encoding);
            }
        }

        internal void WriteOffset(long offset, long pos)
        {
            using (TemporarySeek(offset, SeekOrigin.Begin))
            {
                Write(pos);
            }
        }

        internal void WriteOffset(long offset)
        {
            long pos = Position;
            using (TemporarySeek(offset, SeekOrigin.Begin))
            {
                Write(pos);
            }
        }

        internal void WriteSection(IResData resData)
        {
            resData.Save(this);
        }

        /// <summary>
        /// Reserves space for an offset and size for header block.
        /// </summary>
        [DebuggerStepThrough]
        internal void SaveHeaderBlock(bool IsBinaryHeader = false)
        {
            _savedHeaderBlockPositions.Add(Position);
            if (IsBinaryHeader) //Binary header is just a uint with no long offset
                Write((ushort)0);
            else
                WriteHeaderBlock(0, 0L);
        }

        /// <summary>
        /// Reserves space for an offset to the <paramref name="data"/> written later in the data block pool.
        /// </summary>
        /// <param name="data">The data to save.</param>
        /// <param name="alignment">The alignment to seek to before invoking the callback.</param>
        /// <param name="callback">The <see cref="Action"/> to invoke to write the data.</param>
        [DebuggerStepThrough]
        internal void SaveBlock(object data, uint alignment, Action callback)
        {
            if (data == null)
            {
                Write(0);
                return;
            }
            if (_savedBlocks.TryGetValue(data, out BlockEntry entry))
            {
                entry.Offsets.Add((uint)Position);
            }
            else
            {
                _savedBlocks.Add(data, new BlockEntry((uint)Position, alignment, callback));
            }
            Write(UInt32.MaxValue);
            Write(0);
        }

        /// <summary>
        /// Save pointer array to be relocated in section 1
        /// </summary>
        [DebuggerStepThrough]
        internal void SaveRelocateEntryToSection(long pos, uint OffsetCount, uint StructCount, uint PaddingCount, int SectionNumber, string Hint)
        {
            if (StructCount <= 0)
                throw new Exception("Invalid struct count. Should be greater than 0! " + StructCount);

            if (SectionNumber == Section1)
                _savedSection1Entries.Add(new RelocationEntry((uint)pos, OffsetCount, StructCount, PaddingCount, Hint));
            if (SectionNumber == Section2)
                _savedSection2Entries.Add(new RelocationEntry((uint)pos, OffsetCount, StructCount, PaddingCount, Hint));
            if (SectionNumber == Section3)
                _savedSection3Entries.Add(new RelocationEntry((uint)pos, OffsetCount, StructCount, PaddingCount, Hint));
            if (SectionNumber == Section4)
                _savedSection4Entries.Add(new RelocationEntry((uint)pos, OffsetCount, StructCount, PaddingCount, Hint));
            if (SectionNumber == Section5)
                _savedSection5Entries.Add(new RelocationEntry((uint)pos, OffsetCount, StructCount, PaddingCount, Hint));
        }

        /// <summary>
        /// Writes a Bfsha signature consisting of 4 ASCII characters encoded as an <see cref="UInt32"/>.
        /// </summary>
        /// <param name="value">A valid signature.</param>
        internal void WriteSignature(string value)
        {
            Write(Encoding.ASCII.GetBytes(value));
        }

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

        private bool TryGetItemEntry(object data, ItemEntryType type, out ItemEntry entry)
        {
            foreach (ItemEntry savedItem in _savedItems)
            {
                if (savedItem.Data.Equals(data) && savedItem.Type == type)
                {
                    entry = savedItem;
                    return true;
                }
            }
            entry = null;
            return false;
        }

        private void WriteHeaderBlock(uint Size, long Offset)
        {
            Write(Size);
            Write(Offset);
        }

        private void WriteStrings()
        {
            // Sort the strings ordinally.
            SortedList<string, StringEntry> sorted = new SortedList<string, StringEntry>(ResStringComparer.Instance);
            foreach (KeyValuePair<string, StringEntry> entry in _savedStrings)
            {
                sorted.Add(entry.Key, entry.Value);
            }

            Align(4);
            uint stringPoolOffset = (uint)Position;

            WriteSignature("_STR");
            SaveHeaderBlock();
            Write(sorted.Count);

            //Save filename
            _ofsFileNameString = (uint)Position;
            Write((short)_fileName.Length);
            Write(_fileName, BinaryStringFormat.ZeroTerminated);
            Align(4);

            foreach (KeyValuePair<string, StringEntry> entry in sorted)
            {
                // Align and satisfy offsets.
                Write((short)entry.Key.Length);
                using (TemporarySeek())
                {
                    SatisfyOffsets(entry.Value.Offsets, (uint)Position);
                }

                // Write the name.
                Write(entry.Key, BinaryStringFormat.ZeroTerminated, entry.Value.Encoding ?? Encoding);
                Align(4);
            }
            BaseStream.SetLength(Position); // Workaround to make last alignment expand the file if nothing follows.

            // Save string pool offset and size in main file header.
            /*    uint stringPoolSize = (uint)(Position - stringPoolOffset);
                using (TemporarySeek(_ofsStringPool, SeekOrigin.Begin))
                {
                    Write((int)(stringPoolOffset));
                    Write(0);
                    Write(stringPoolSize);
                }*/
        }

        private void WriteBlocks()
        {
            foreach (KeyValuePair<object, BlockEntry> entry in _savedBlocks)
            {
                // Align and satisfy offsets.
                if (entry.Value.Alignment != 0) Align((int)entry.Value.Alignment);
                using (TemporarySeek())
                {
                    SatisfyOffsets(entry.Value.Offsets, (uint)Position);
                }

                // Write the data.
                entry.Value.Callback.Invoke();
            }
        }

        private void WriteOffsets()
        {
            using (TemporarySeek())
            {
                foreach (ItemEntry entry in _savedItems)
                {
                    SatisfyOffsets(entry.Offsets, entry.Target.Value);
                }
            }
        }

        private void SatisfyOffsets(IEnumerable<uint> offsets, uint target)
        {
            foreach (uint offset in offsets)
            {
                Position = offset;
                Write((int)(target));
            }
        }

        // ---- STRUCTURES ---------------------------------------------------------------------------------------------

        [DebuggerDisplay("{" + nameof(Type) + "} {" + nameof(Data) + "}")]
        private class ItemEntry
        {
            internal object Data;
            internal ItemEntryType Type;
            internal List<uint> Offsets;
            internal uint? Target;
            internal Action Callback;
            internal int Index;

            internal ItemEntry(object data, ItemEntryType type, uint? offset = null, uint? target = null,
                Action callback = null, int index = -1)
            {
                Data = data;
                Type = type;
                Offsets = new List<uint>();
                if (offset.HasValue) // Might be null for enumerable entries to resolve references to them later.
                {
                    Offsets.Add(offset.Value);
                }
                Callback = callback;
                Target = target;
                Index = index;
            }
        }

        private enum ItemEntryType
        {
            List, Dict, ResData, Custom
        }

        private class RelocationSection
        {
            internal List<RelocationEntry> Entries;
            internal int EntryIndex;
            internal uint Size;
            internal uint Position;

            internal RelocationSection(uint position, int entryIndex, uint size, List<RelocationEntry> entries)
            {
                Position = position;
                EntryIndex = entryIndex;
                Size = size;
                Entries = entries;
            }
        }

        private class RelocationEntry
        {
            internal uint Position;
            internal uint PadingCount;
            internal uint StructCount;
            internal uint OffsetCount;
            internal string Hint;

            internal RelocationEntry(uint position, uint offsetCount, uint structCount, uint padingCount, string hint)
            {
                Position = position;
                StructCount = structCount;
                OffsetCount = offsetCount;
                PadingCount = padingCount;
                Hint = hint;
            }
        }

        private class StringEntry
        {
            internal List<uint> Offsets;
            internal Encoding Encoding;

            internal StringEntry(uint offset, Encoding encoding = null)
            {
                Offsets = new List<uint>(new uint[] { offset });
                Encoding = encoding;
            }
        }

        private class BlockEntry
        {
            internal List<uint> Offsets;
            internal uint Alignment;
            internal Action Callback;

            internal BlockEntry(uint offset, uint alignment, Action callback)
            {
                Offsets = new List<uint> { offset };
                Alignment = alignment;
                Callback = callback;
            }
        }
    }
}