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
    public class ShaderCodeDataSource : ShaderCodeData, IResData
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------


        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        public List<byte[]> SourceData { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(BfshaFileLoader loader)
        {
            ushort codeLength = loader.ReadUInt16();
            loader.Seek(6);
            uint[] codeSizes = loader.LoadCustom(() => loader.ReadUInt32s(codeLength));
            ulong[] codeOffsets = loader.LoadCustom(() => loader.ReadUInt64s(codeLength));
            loader.Seek(8); //reserved

            SourceData = new List<byte[]>();
            for (int i = 0; i < codeLength; i++)
            {
                loader.Seek((long)codeOffsets[i], System.IO.SeekOrigin.Begin);
                SourceData.Add(loader.ReadBytes((int)codeSizes[i]));
            }
        }

        internal long PtrCodeSizesPos;
        internal long PtrCodeOffsets;
        internal long PtrCodeOffsetsPos;

        void IResData.Save(BfshaFileSaver saver)
        {
            saver.Write((ushort)SourceData.Count);
            saver.Write(new byte[6]); //reserved
            PtrCodeSizesPos = saver.SaveOffset();
            PtrCodeOffsetsPos = saver.SaveOffset();
            saver.Write(new byte[8]); //reserved
        }
    }
}
