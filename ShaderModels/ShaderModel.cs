using System.Collections.Generic;
using System.Diagnostics;
using BfshaLibrary.Core;
using System.ComponentModel;
using System;

namespace BfshaLibrary
{
    /// <summary>
    /// Represents an FMDL subfile in a <see cref="ResFile"/>, storing model vertex data, skeletons and used materials.
    /// </summary>
    [DebuggerDisplay(nameof(ShaderModel) + " {" + nameof(Name) + "}")]
    public class ShaderModel : IResData
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------


        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in <see cref="ResDict{Model}"/>
        /// instances.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the path of the file which originally supplied the data of this instance.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The binary shader file used to store programs, variations and binary data
        /// </summary>
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public BnshFile BnshFile { get; set; }

        /// <summary>
        /// Gets or sets the stored <see cref="ShaderOption"/> (FMDL) instances.
        /// </summary>
        public List<ShaderOption> StaticOptions { get; set; }

        /// <summary>
        /// Gets or sets attached <see cref="ShaderOption"/> names
        /// </summary>
        public ResDict StaticOptionDict { get; set; }

        /// <summary>
        /// Gets or sets the stored <see cref="DynamiOption"/> (FMDL) instances.
        /// </summary>
        public List<ShaderOption> DynamiOptions { get; set; }

        /// <summary>
        /// Gets or sets attached <see cref="DynamiOption"/> names
        /// </summary>
        public ResDict DynamiOptionDict { get; set; }

        /// <summary>
        /// Gets or sets the stored <see cref="Attribute"/> (FMDL) instances.
        /// </summary>
        public List<Attribute> Attributes { get; set; }

        /// <summary>
        /// Gets or sets attached <see cref="Attribute"/> names
        /// </summary>
        public ResDict AttributeDict { get; set; }

        /// <summary>
        /// Gets or sets the stored <see cref="Sampler"/> (FMDL) instances.
        /// </summary>
        public List<Sampler> Samplers { get; set; }

        /// <summary>
        /// Gets or sets attached <see cref="Sampler"/> names
        /// </summary>
        public ResDict SamplersDict { get; set; }

        /// <summary>
        /// Gets or sets the stored <see cref="UniformBlock"/> (FMDL) instances.
        /// </summary>
        public List<UniformBlock> UniformBlocks { get; set; }

        /// <summary>
        /// Gets or sets the stored <see cref="UniformVar"/> (FMDL) instances.
        /// </summary>
        public List<UniformVar> UniformVars { get; set; }

        /// <summary>
        /// Gets or sets attached <see cref="UniformBlock"/> names
        /// </summary>
        public ResDict UniformBlockDict { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        public int GetStaticKey(int StaticOptionIndex, int ChoiceIndex)
        {
            return -1;
        }

        void IResData.Load(BfshaFileLoader loader)
        {
            Name = loader.LoadString();
            long staticOptionArrayOffset = loader.ReadOffset();
            StaticOptionDict = loader.LoadDict(); 
            long dynamicOptionArrayOffset = loader.ReadOffset();
            DynamiOptionDict = loader.LoadDict(); 
            long attribArrayOffset = loader.ReadOffset();
            AttributeDict = loader.LoadDict(); 
            long samplerArrayOffset = loader.ReadOffset();
            SamplersDict = loader.LoadDict();

            if (loader.BfshaFile.VersionMinor >= 8)
            {
                loader.ReadInt64();
                loader.ReadInt64();
            }

            long uniformBlockArrayOffset = loader.ReadOffset();
            UniformBlockDict = loader.LoadDict();
            long uniformArrayOffset = loader.ReadOffset();

            if (loader.BfshaFile.VersionMinor >= 7)
            {
                loader.ReadInt64();
                loader.ReadInt64();
                loader.ReadInt64();
            }

            long shaderProgramArrayOffset = loader.ReadOffset();
            long tableOffset = loader.ReadOffset();
            long shaderArchiveOffset = loader.ReadOffset();
            long shaderInfoOffset = loader.ReadOffset();
            long shaderFileOffset = loader.ReadOffset();
            loader.ReadUInt64();

            if (loader.BfshaFile.VersionMinor >= 7)
            {
                //padding
                loader.ReadInt64();
                loader.ReadInt64();
            }

            loader.ReadUInt64();
            loader.ReadUInt64();

            uint uniformCount = loader.ReadUInt32();
            if (loader.BfshaFile.VersionMinor <= 7)
            {
                loader.ReadUInt32();
            }
            int defaultProgramIndex = loader.ReadInt32();
            ushort staticOptionCount = loader.ReadUInt16();
            ushort dynamicOptionCount = loader.ReadUInt16();
            ushort shaderProgramCount = loader.ReadUInt16();
            byte staticKeyLength = loader.ReadByte();
            byte dynamicKeyLength = loader.ReadByte();
            byte attribCount = loader.ReadByte();
            byte samplerCount = loader.ReadByte();

            if (loader.BfshaFile.VersionMinor >= 8)
            {
                byte imageCount = loader.ReadByte();
            }

            byte uniformBlockCount = loader.ReadByte();
            loader.ReadBytes(5);

            if (loader.BfshaFile.VersionMinor >= 7)
            {
                loader.ReadBytes(6);
            }

            System.Console.WriteLine($"shaderFileOffset " + shaderFileOffset);
            System.Console.WriteLine($"Sampler " + samplerCount);
            System.Console.WriteLine($"attribCount " + attribCount);
            System.Console.WriteLine($"dynamicOptionCount " + dynamicOptionCount);
            System.Console.WriteLine($"staticOptionArrayOffset " + staticOptionArrayOffset);
            System.Console.WriteLine($"staticOptionCount " + staticOptionCount);

            if (loader.BfshaFile.VersionMinor >= 8)
                loader.ReadBytes(7);
            if (loader.BfshaFile.VersionMinor < 7)
                loader.ReadBytes(4);

            System.Console.WriteLine($"end pos " + loader.Position);

            int BnshSize = 0;

            if (shaderFileOffset != 0)
            {
                //Go into the bnsh file and get the file size
                using (loader.TemporarySeek(shaderFileOffset + 0x1C, System.IO.SeekOrigin.Begin))
                {
                    BnshSize = (int)loader.ReadUInt32();
                }
            }

            byte[] BinaryShaderData = loader.LoadCustom(() => loader.ReadBytes(BnshSize), shaderFileOffset);
            BnshFile = new BnshFile(new System.IO.MemoryStream(BinaryShaderData));

            StaticOptions = loader.LoadList<ShaderOption>(staticOptionCount, staticOptionArrayOffset);
            DynamiOptions = loader.LoadList<ShaderOption>(dynamicOptionCount, dynamicOptionArrayOffset);
            Attributes = loader.LoadList<Attribute>(attribCount, attribArrayOffset);
            Samplers = loader.LoadList<Sampler>(samplerCount, samplerArrayOffset);
            UniformBlocks = loader.LoadList<UniformBlock>(uniformBlockCount, uniformBlockArrayOffset);
            UniformVars = loader.LoadList<UniformVar>((int)uniformCount, uniformArrayOffset);
        }

        void IResData.Save(BfshaFileSaver saver)
        {
  
        }
    }
}