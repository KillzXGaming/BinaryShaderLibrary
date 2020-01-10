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
    public class ShaderCodeDataBinary : ShaderCodeData, IResData
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------


        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        public List<byte[]> BinaryData { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(BfshaFileLoader loader)  
        {
            loader.Seek(8); //always empty
            ulong[] codeOffsets = loader.ReadUInt64s(2);
            uint[] codeSizes = loader.ReadUInt32s(2);
            loader.Seek(32); //padding

            BinaryData = new List<byte[]>();
            for (int i = 0; i < 2; i++) //Fixed with 2 binaries
            {
                loader.Seek((long)codeOffsets[i], System.IO.SeekOrigin.Begin);
                if (i == 0)
                    BinaryData.Add(loader.ReadBytes((int)codeSizes[1]));
                else
                    BinaryData.Add(loader.ReadBytes((int)codeSizes[0]));
            }
        }

        internal long PtrCodeOffset1Pos;
        internal long PtrCodeOffset2Pos;

        void IResData.Save(BfshaFileSaver saver)
        {
            saver.Write(new byte[8]); //padding
            PtrCodeOffset1Pos = saver.SaveOffset();
            PtrCodeOffset2Pos = saver.SaveOffset();
            saver.Write(BinaryData[1].Length);
            saver.Write(BinaryData[0].Length);
            saver.Write(new byte[32]); //padding
        }
    }
}
