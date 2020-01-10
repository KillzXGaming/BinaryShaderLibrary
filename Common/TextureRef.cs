using System;
using System.Diagnostics;
using BfshaLibrary.Core;

namespace BfshaLibrary
{
    /// <summary>
    /// Represents a reference to a <see cref="Texture"/> instance by name.
    /// </summary>
    [DebuggerDisplay(nameof(TextureRef) + " {" + nameof(Name) + "}")]
    public class TextureRef : IResData
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------
        
        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in
        /// <see cref="ResDict{TextureRef}"/> instances. Typically the same as the <see cref="Texture.Name"/>.
        /// </summary>
        public string Name { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(BfshaFileLoader loader)
        {
            Name = loader.LoadString();
        }
        
        void IResData.Save(BfshaFileSaver saver)
        {
            saver.SaveString(Name);
        }
    }
}