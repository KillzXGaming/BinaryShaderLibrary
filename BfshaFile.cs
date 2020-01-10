using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using Syroot.BinaryData;
using BfshaLibrary.Core;

namespace BfshaLibrary
{
    /// <summary>
    /// Represents a NintendoWare for Cafe (NW4F) graphics data archive file.
    /// </summary>
    [DebuggerDisplay(nameof(BfshaFile) + " {" + nameof(Name) + "}")]
    public class BfshaFile : IResData
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const string _signature = "FSHA";

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="BfshaFile"/> class.
        /// </summary>
        public BfshaFile()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BfshaFile"/> class from the given <paramref name="stream"/> which
        /// is optionally left open.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to load the data from.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the stream open after reading, otherwise <c>false</c>.</param>
        public BfshaFile(Stream stream, bool leaveOpen = false)
        {
            using (BfshaFileLoader loader = new BfshaFileLoader(this, stream, leaveOpen))
            {
                loader.Execute();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BfshaFile"/> class from the file with the given
        /// <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName">The name of the file to load the data from.</param>
        public BfshaFile(string fileName)
        {
            using (BfshaFileLoader loader = new BfshaFileLoader(this, fileName))
            {
                loader.Execute();
            }
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the revision of the BFRES structure formats.
        /// </summary>
        public short VersionMajor { get; set; }
        public uint VersionMinor { get; set; }
        public uint VersionMicro { get; set; }

        /// <summary>
        /// Gets the byte order in which data is stored. Must be the endianness of the target platform.
        /// </summary>
        public ByteOrder ByteOrder { get; private set; }


        /// <summary>
        /// Gets or sets the alignment to use for raw data blocks in the file.
        /// </summary>
        public uint Alignment { get; set; }

        /// <summary>
        /// Gets or sets the data alignment to use for raw data blocks in the file.
        /// </summary>
        public uint DataAlignment
        {
            get
            {
                return (uint)(1 << (int)Alignment);
            }
        }


        /// <summary>
        /// Gets or sets the target adress size to use for raw data blocks in the file.
        /// </summary>
        public uint TargetAddressSize { get; set; }

        /// <summary>
        /// Gets or sets the flag. Unknown purpose.
        /// </summary>
        public uint Flag { get; set; }

        /// <summary>
        /// Gets or sets the BlockOffset. 
        /// </summary>
        public uint BlockOffset { get; set; }

        /// <summary>
        /// Gets or sets a name describing the contents.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the path of the file which originally supplied the data of this instance.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the stored <see cref="RelocationTable"/> (_RLT) instance.
        /// </summary>
        public RelocationTable RelocationTable { get; set; }

        /// <summary>
        /// Gets or sets the stored <see cref="Model"/> (FMDL) instances.
        /// </summary>
        public IList<ShaderModel> ShaderModels { get; set; }

        /// <summary>
        /// Gets or sets attached <see cref="ShaderModel"/> names
        /// </summary>
        public ResDict ShaderModelDict { get; set; }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Saves the contents in the given <paramref name="stream"/> and optionally leaves it open
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to save the contents into.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the stream open after writing, otherwise <c>false</c>.</param>
        public void Save(Stream stream, bool leaveOpen = false)
        {
            using (BfshaFileSaver saver = new BfshaFileSaver(this, stream, leaveOpen))
            {
                saver.Execute();
            }
        }

        /// <summary>
        /// Saves the contents in the file with the given <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName">The name of the file to save the contents into.</param>
        public void Save(string fileName)
        {
            using (BfshaFileSaver saver = new BfshaFileSaver(this, fileName))
            {
                saver.Execute();
            }
        }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(BfshaFileLoader loader)
        {
            loader.CheckSignature(_signature); //Same binary header as BNSH, BFRES, ETC
            uint padding                       = loader.ReadUInt32();
            VersionMajor                       = loader.ReadInt16();
            VersionMinor                       = loader.ReadByte();
            VersionMicro                       = loader.ReadByte();
            ByteOrder                          = loader.ReadEnum<ByteOrder>(false);
            Alignment                          = loader.ReadByte();
            TargetAddressSize                  = loader.ReadByte(); //Thanks MasterF0X for pointing out the layout of the these
            uint OffsetToFileName              = loader.ReadUInt32();
            uint OffsetPath = loader.ReadUInt32();
            uint RelocationTableOffset         = loader.ReadUInt32();
            uint sizFile                       = loader.ReadUInt32();
            long shaderArchiveOffset           = loader.ReadOffset();
            long StringPoolOffset              = loader.ReadOffset();
            long ShaderingModelOffset          = loader.ReadOffset();
            Name                               = loader.LoadString();
            Path = loader.LoadString();
            long ShaderModelArrayOffset        = loader.ReadOffset();
            ShaderModelDict                    = loader.LoadDict();
            var padding2 = loader.ReadUInt64();
            var unk1 = loader.ReadUInt64();
            var unk2 = loader.ReadUInt64();

            ushort ModelCount = loader.ReadUInt16();
            ushort flag = loader.ReadUInt16();
            loader.ReadUInt16();
            if (VersionMinor >= 7) //padding
                loader.ReadUInt16();

            ShaderModels = loader.LoadList<ShaderModel>(ModelCount, ShaderModelArrayOffset);
        }

        void IResData.Save(BfshaFileSaver saver)
        {
            //     PreSave(); 

            saver.WriteSignature(_signature);
            saver.Write(0x20202020);
            saver.Write(VersionMajor);
            saver.Write(VersionMinor);
            saver.Write(VersionMicro);
            saver.Write(ByteOrder, true);
            saver.Write((byte)Alignment);
            saver.Write((byte)TargetAddressSize);
            saver.Write(0);
            saver.Write((ushort)Flag);
            saver.Write((ushort)BlockOffset);
          //  saver.Save(RelocationTable, true);
            saver.SaveFieldFileSize();
            saver.SaveString(Name);
 
        }

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

        private void PreSave()
        {
            // Update Shape instances.
       
        }
    }
}
