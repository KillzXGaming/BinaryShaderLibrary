using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BfshaLibrary.Core;
using System.Diagnostics;

namespace BfshaLibrary
{
    [DebuggerDisplay(nameof(ShaderProgram))]
    public class ShaderCodeDataCompressed : ShaderCodeData, IResData
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------


        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        public byte[] CompressedData { get; set; }

        public uint DecompressedSize { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(BfshaFileLoader loader)
        {
            uint size = loader.ReadUInt32();
            DecompressedSize = loader.ReadUInt32();
            ulong codePointer = loader.ReadUInt64();

            loader.Seek((int)codePointer, System.IO.SeekOrigin.Begin);
            CompressedData = loader.ReadBytes((int)size);
        }

        internal long PtrCodeSizesPos;
        internal long PtrCodeOffsets;
        internal long PtrCodeOffsetsPos;

        void IResData.Save(BfshaFileSaver saver)
        {
            saver.Write(CompressedData.Length);
            saver.Write(DecompressedSize);
            PtrCodeOffsetsPos = saver.SaveOffset();
        }
    }
}
