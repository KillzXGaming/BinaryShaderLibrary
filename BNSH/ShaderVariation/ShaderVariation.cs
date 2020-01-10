using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BfshaLibrary.Core;
using System.Diagnostics;

namespace BfshaLibrary
{
    [DebuggerDisplay(nameof(ShaderVariation))]
    public class ShaderVariation : IResData
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------


        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        // ---- METHODS ------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        public ShaderProgram BinaryProgram { get; set; }
        public ShaderProgram SourceProgram { get; set; }

        public List<ShaderProgram> GetPrograms()
        {
            List<ShaderProgram> shaders = new List<ShaderProgram>();
            if (BinaryProgram != null) shaders.Add(BinaryProgram);
            if (SourceProgram != null) shaders.Add(SourceProgram);
            return shaders;
        }

        void IResData.Load(BfshaFileLoader loader)
        {
            long sourceProgramOffset = loader.ReadInt64();
            long padding = loader.ReadInt64();
            long binaryProgramOffset = loader.ReadInt64();
            long parentOffset = loader.ReadInt64();
            loader.Seek(32);

            if (sourceProgramOffset != 0) {
                SourceProgram = loader.Load<ShaderProgram>(sourceProgramOffset);
                SourceProgram.ParentShaderVariation = this;
            }
            if (binaryProgramOffset != 0) {
                BinaryProgram = loader.Load<ShaderProgram>(binaryProgramOffset);
                BinaryProgram.ParentShaderVariation = this;
            }
        }

        internal long _position;

        void IResData.Save(BfshaFileSaver saver)
        {
            _position = saver.Position;

            saver.Write(0L); //source pointer
            saver.Write(0L);
            saver.Write(0L); //binary pointer
            saver.Write(saver.BnshFile.SizeOfGRSC);
            saver.Write(new byte[32]); //reserved
        }
    }
}
