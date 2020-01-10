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
    public class ShaderReflectionData : ShaderCodeData, IResData
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------


        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        public ResDict ShaderInputDictionary { get; set; }

        public ResDict ShaderOutputDictionary { get; set; }

        public ResDict ShaderSamplerDictionary { get; set; }

        public ResDict ShaderConstantsDictionary { get; set; }

        public ResDict ShaderUnknownDictionary { get; set; }

        public int Unknown1 { get; set; }
        public int Unknown2 { get; set; }
        public int Unknown3 { get; set; }
        public int Unknown4 { get; set; }
        public int Unknown5 { get; set; }
        public int Unknown6 { get; set; }
        public int Unknown7 { get; set; }
        public int Unknown8 { get; set; }
        public int Unknown9 { get; set; }
        public int Unknown10 { get; set; }
        public int Unknown11 { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(BfshaFileLoader loader)
        {
            ShaderInputDictionary = loader.LoadDict();
            ShaderOutputDictionary = loader.LoadDict();
            ShaderSamplerDictionary = loader.LoadDict();
            ShaderConstantsDictionary = loader.LoadDict();
            ShaderUnknownDictionary = loader.LoadDict();
            Unknown1 = loader.ReadInt32();
            Unknown2 = loader.ReadInt32();
            Unknown3 = loader.ReadInt32();
            Unknown4 = loader.ReadInt32();
            Unknown5 = loader.ReadInt32();
            Unknown6 = loader.ReadInt32();
            Unknown7 = loader.ReadInt32();
            Unknown8 = loader.ReadInt32();
            Unknown9 = loader.ReadInt32();
            Unknown10 = loader.ReadInt32();
            Unknown11 = loader.ReadInt32();
            loader.ReadInt64(); //padding

            foreach (var key in ShaderInputDictionary)
                Console.WriteLine($"Input: {key}");
            foreach (var key in ShaderOutputDictionary)
                Console.WriteLine($"Output: {key}");
            foreach (var key in ShaderSamplerDictionary)
                Console.WriteLine($"Sampler: {key}");
            foreach (var key in ShaderConstantsDictionary)
                Console.WriteLine($"Constant: {key}");
            foreach (var key in ShaderUnknownDictionary)
                Console.WriteLine($"Unknown: {key}");
        }

        void IResData.Save(BfshaFileSaver saver)
        {

        }
    }
}
