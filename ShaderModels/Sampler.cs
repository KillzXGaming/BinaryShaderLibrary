using System.Collections.Generic;
using System.Diagnostics;
using BfshaLibrary.Core;

namespace BfshaLibrary
{
    /// <summary>
    /// Represents an FMDL subfile in a <see cref="ShaderModel"/>, storing model vertex data, skeletons and used materials.
    /// </summary>
    [DebuggerDisplay(nameof(Sampler))]
    public class Sampler : IResData
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------


        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        //Empty string
        public string Extra { get; set; }

        public byte Index { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(BfshaFileLoader loader)
        {
            Extra = loader.LoadString();
            Index = loader.ReadByte();
            loader.Seek(7);
        }

        void IResData.Save(BfshaFileSaver saver)
        {
            saver.SaveString(Extra);
            saver.Write((byte)Index);
            saver.Seek(7);
        }
    }
}