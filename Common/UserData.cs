using System;
using System.Diagnostics;
using System.Text;
using BfshaLibrary.Core;

namespace BfshaLibrary
{
    /// <summary>
    /// Represents custom user variables which can be attached to many sections and subfiles of a <see cref="BfshaFile"/>.
    /// </summary>
    [DebuggerDisplay(nameof(UserData) + " {" + nameof(Name) + "}")]
    public class UserData : IResData
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------
        
        private object _value;
        
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in <see cref="ResDict{UserData}"/>
        /// instances.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The data type of the stored values.
        /// </summary>
        public UserDataType Type { get; private set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(BfshaFileLoader loader)
        {
            Name = loader.LoadString();
            long DataOffset = loader.ReadOffset();
            ushort count = loader.ReadUInt16();
            Type = loader.ReadEnum<UserDataType>(true);

      //      UserDataData = loader.LoadCustom(() => loader.Load<UserDataData>(Type, count), DataOffset);

        }

        void IResData.Save(BfshaFileSaver saver)
        {
            saver.SaveString(Name);
            saver.Write((ushort)((Array)_value).Length); // Unsafe cast, but _value should always be Array.
            saver.Write(Type, true);
  
        }
    }

    /// <summary>
    /// Represents the possible data types of values stored in <see cref="UserData"/> instances.
    /// </summary>
    public enum UserDataType : byte
    {
        /// <summary>
        /// The values is an <see cref="Int32"/> array.
        /// </summary>
        Int32,

        /// <summary>
        /// The values is a <see cref="Single"/> array.
        /// </summary>
        Single,

        /// <summary>
        /// The values is a <see cref="String"/> array encoded in ASCII.
        /// </summary>
        String,

        /// <summary>
        /// The values is a <see cref="String"/> array encoded in UTF-16.
        /// </summary>
        WString,

        /// <summary>
        /// The values is a <see cref="Byte"/> array.
        /// </summary>
        Byte
    }
}