using System.Collections.Generic;
using System.Diagnostics;
using BfshaLibrary.Core;

namespace BfshaLibrary
{
    /// <summary>
    /// </summary>
    [DebuggerDisplay(nameof(UniformVar))]
    public class UniformVar : IResData
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        public string Name { get; set; }

        public int Index { get; set; }

        public ushort Offset { get; set; }

        public byte BlockIndex { get; set; }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(BfshaFileLoader loader)
        {
            Name = loader.LoadString();
            Index = loader.ReadInt32();
            Offset = loader.ReadUInt16();
            BlockIndex = loader.ReadByte();
            byte padding = loader.ReadByte();
        }

        void IResData.Save(BfshaFileSaver saver)
        {
            saver.SaveString(Name);
            saver.Write(Index);
            saver.Write((ushort)Offset);
            saver.Write((byte)BlockIndex);
            saver.Seek(1);
        }
    }
}