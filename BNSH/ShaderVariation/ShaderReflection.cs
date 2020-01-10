using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BfshaLibrary.Core;
using System.Diagnostics;

namespace BfshaLibrary
{
    [DebuggerDisplay(nameof(ShaderInfoData))]
    public class ShaderReflection : IResData
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------


        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        public ShaderReflectionData VertexShaderCode { get; set; }
        public ShaderReflectionData Tess0ShaderCode { get; set; }
        public ShaderReflectionData Tess1ShaderCode { get; set; }
        public ShaderReflectionData GeometryShaderCode { get; set; }
        public ShaderReflectionData PixelShaderCode { get; set; }
        public ShaderReflectionData ComputeShaderCode { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(BfshaFileLoader loader)
        {
            VertexShaderCode = loader.Load<ShaderReflectionData>();
            Tess0ShaderCode = loader.Load<ShaderReflectionData>();
            Tess1ShaderCode = loader.Load<ShaderReflectionData>();
            GeometryShaderCode = loader.Load<ShaderReflectionData>();
            PixelShaderCode = loader.Load<ShaderReflectionData>();
            ComputeShaderCode = loader.Load<ShaderReflectionData>();

            if (VertexShaderCode != null)
                VertexShaderCode.ShaderType = ShaderType.VERTEX;
            if (Tess0ShaderCode != null)
                Tess0ShaderCode.ShaderType = ShaderType.TESS0;
            if (Tess1ShaderCode != null)
                Tess1ShaderCode.ShaderType = ShaderType.TESS1;
            if (GeometryShaderCode != null)
                GeometryShaderCode.ShaderType = ShaderType.GEOMETRY;
            if (PixelShaderCode != null)
                PixelShaderCode.ShaderType = ShaderType.PIXEL;
            if (ComputeShaderCode != null)
                ComputeShaderCode.ShaderType = ShaderType.COMPUTE;
        }

        internal long PtrVertexShaderCodePos;
        internal long PtrTess0ShaderCodePos;
        internal long PtrTess1ShaderCodePos;
        internal long PtrGeometryShaderCodePos;
        internal long PtrPixelShaderCodePos;
        internal long PtrComputeShaderCodePos;

        void IResData.Save(BfshaFileSaver saver)
        {
            PtrVertexShaderCodePos = saver.SaveOffset();
            PtrTess0ShaderCodePos = saver.SaveOffset();
            PtrTess1ShaderCodePos = saver.SaveOffset();
            PtrGeometryShaderCodePos = saver.SaveOffset();
            PtrPixelShaderCodePos = saver.SaveOffset();
            PtrComputeShaderCodePos = saver.SaveOffset();
        }

        public enum ShaderFormat : byte
        {
            Binary = 0x00,
            Source = 0x03,
        }
    }
}
