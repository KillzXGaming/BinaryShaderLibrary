using System.Collections.Generic;
using System.Diagnostics;
using BfshaLibrary.Core;

namespace BfshaLibrary
{
    /// <summary>
    /// Represents an FMDL subfile in a <see cref="ShaderModel"/>, storing model vertex data, skeletons and used materials.
    /// </summary>
    [DebuggerDisplay(nameof(Attribute))]
    public class ShaderOption : IResData
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------


        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in <see cref="ResDict{StaticOption}"/>
        /// instances.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the stored <see cref="UniformBlock"/> (FMDL) instances.
        /// </summary>
        public uint[] Choices { get; set; }

        /// <summary>
        /// Gets or sets attached <see cref="UniformBlock"/> names
        /// </summary>
        public ResDict ChoiceDict { get; set; }

        /// <summary>
        /// Gets or sets attached <see cref="UniformBlock"/> names
        /// </summary>
        public string[] choices
        {
            get
            {
                return ChoiceDict.GetKeys();
            }
        }

        /// <summary>
        /// Gets or sets attached <see cref="UniformBlock"/> names
        /// </summary>
        public string defaultChoice
        {
            get
            {
                return ChoiceDict.GetKey(DefaultIndex);
            }
        }

        public byte DefaultIndex { get; set; }
        public ushort BlockOffset { get; set; }
        public byte flag { get; set; }
        public byte keyOffset { get; set; }
        public byte flags0 { get; set; }
        public byte flags1 { get; set; }
        public uint flags2 { get; set; }


        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(BfshaFileLoader loader)
        {
            Name = loader.LoadString();
            ChoiceDict = loader.LoadDict();
            long ChoiceValuesOffset = loader.ReadOffset();
            byte choiceCount = loader.ReadByte();
            DefaultIndex = loader.ReadByte(); //Index to default choice value
            BlockOffset = loader.ReadUInt16(); // Uniform block offset.
            flag = loader.ReadByte();
            keyOffset = loader.ReadByte(); //Offset for key table
            flags0 = loader.ReadByte(); //Flags for choices
            flags1 = loader.ReadByte();
            flags2 = loader.ReadUInt32();
            uint padding = loader.ReadUInt32();

            Choices = loader.LoadCustom(() => loader.ReadUInt32s(choiceCount), ChoiceValuesOffset);
        }

        void IResData.Save(BfshaFileSaver saver)
        {
            saver.SaveString(Name);
            saver.SaveDict(ChoiceDict);

        }
    }
}