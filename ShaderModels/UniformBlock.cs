using System.Collections.Generic;
using System.Diagnostics;
using BfshaLibrary.Core;

namespace BfshaLibrary
{
    /// <summary>
    /// Represents an FMDL subfile in a <see cref="ShaderModel"/>, storing model vertex data, skeletons and used materials.
    /// </summary>
    [DebuggerDisplay(nameof(UniformBlock))]
    public class UniformBlock : IResData
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------


        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the stored <see cref="UniformBlock"/> (FMDL) instances.
        /// </summary>
        public List<UniformVar> Uniforms { get; set; }

        /// <summary>
        /// Gets or sets attached <see cref="UniformBlock"/> names
        /// </summary>
        public ResDict UniformDict { get; set; }

        public byte Index { get; set; }

        public BlockType Type { get; set; }

        public ushort Size { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(BfshaFileLoader loader)
        {
            long uniformArrayOffset = loader.ReadInt64();
            UniformDict = loader.LoadDict();
            long defaultOffset = loader.ReadInt64();
            Index = loader.ReadByte();
            Type = loader.ReadEnum<BlockType>(true);
            Size = loader.ReadUInt16();
            ushort uniformCount = loader.ReadUInt16();
            ushort padding = loader.ReadUInt16();

            Uniforms = loader.LoadList<UniformVar>(uniformCount, uniformArrayOffset);
        }

        void IResData.Save(BfshaFileSaver saver)
        {
            saver.SaveList(Uniforms);
            saver.SaveDict(UniformDict);

        }


        public enum BlockType : byte
        {
            None,
            Material,
            Shape,
            Option,
            Num,
        }

    }
}