using System.Collections.Generic;
using Syroot.Maths;
using BfshaLibrary.Core;

namespace BfshaLibrary
{
    /// <summary>
    /// Represents an memory info section in a <see cref="BfshaFile"/> subfile. References vertex and index buffers
    /// </summary>
    public class BufferMemoryPoolInfo : IResData
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        byte[] Reserved;

        // ---- PROPERTIES (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the buffer instance that stores face data first, then vertex buffer after.
        /// </summary>
        public static ulong BufferOffset { get; set; }

        /// <summary>
        /// Gets or sets the memory pool property
        /// </summary>
        public uint Property { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(BfshaFileLoader loader)
        {
            Property        = loader.ReadUInt32();
            uint Size       = loader.ReadUInt32();
            BufferOffset = loader.ReadUInt64();
            Reserved        = loader.ReadBytes(16);
        }

        void IResData.Save(BfshaFileSaver saver)
        {
            saver.Write(Property);
            saver.Write(0);
            saver.Write(0);
            saver.Write(Reserved);
        }
    }
}
