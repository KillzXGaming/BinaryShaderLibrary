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
    public class ShaderProgram : IResData
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------


        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        public bool IsSource => ParentShaderVariation.SourceProgram == this;

        public bool IsBinary => ParentShaderVariation.BinaryProgram == this;

        public ShaderVariation ParentShaderVariation { get; set; }

        public ShaderInfoData ShaderInfoData { get; set; }
        public ShaderReflection ShaderReflection { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        public byte[] MemoryData { get; set; }

        public ShaderProgram()
        {

        }

        public ShaderProgram(ShaderVariation variation)
        {
            ShaderInfoData = new ShaderInfoData();
            MemoryData = new byte[64];
            ParentShaderVariation = variation;

            if (variation.BinaryProgram == this)
                ShaderReflection = new ShaderReflection();
        }

        void IResData.Load(BfshaFileLoader loader)
        {
            ShaderInfoData = loader.LoadSection<ShaderInfoData>();
            uint memorySize = loader.ReadUInt32();
            loader.ReadUInt32(); //padding
            //Stores a bunch of 0s
            MemoryData = loader.LoadCustom(() => loader.ReadBytes((int)memorySize));
            loader.ReadUInt64(); //offset to parent shader variation
            ShaderReflection = loader.Load<ShaderReflection>();
            loader.Seek(32); //reserved
        }

        internal long PtrMemPos;
        internal long PtrRefelctionPos;

        void IResData.Save(BfshaFileSaver saver)
        {
            saver.WriteSection(ShaderInfoData);
            saver.Write(MemoryData.Length);
            saver.Write(0); //padding
            PtrMemPos = saver.SaveOffset();
            saver.Write(ParentShaderVariation._position);
            PtrRefelctionPos = saver.SaveOffset();
            saver.Write(new byte[32]); //padding
        }
    }
}
