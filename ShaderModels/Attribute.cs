using System.Collections.Generic;
using System.Diagnostics;
using BfshaLibrary.Core;

namespace BfshaLibrary
{
    /// <summary>
    /// Represents an FMDL subfile in a <see cref="ShaderModel"/>, storing model vertex data, skeletons and used materials.
    /// </summary>
    [DebuggerDisplay(nameof(Attribute))]
    public class Attribute : IResData
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------


        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        public byte Index { get; set; }

        public byte Location { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(BfshaFileLoader loader)
        {
            Index = loader.ReadByte();
            Location = loader.ReadByte();
        }

        void IResData.Save(BfshaFileSaver saver)
        {
            saver.Write((byte)Index);
            saver.Write((byte)Location);
        }
    }
}