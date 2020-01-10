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
    public class ShaderInfoData : IResData
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------


        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        public ShaderCodeData VertexShaderCode { get; set; }
        public ShaderCodeData TessellationControlShaderCode { get; set; }
        public ShaderCodeData TessellationEvaluationShaderCode { get; set; }
        public ShaderCodeData GeometryShaderCode { get; set; }
        public ShaderCodeData PixelShaderCode { get; set; }
        public ShaderCodeData ComputeShaderCode { get; set; }

        public ShaderCodeData[] GetAllShaders()
        {
            List<ShaderCodeData> shaders = new List<ShaderCodeData>();
            if (VertexShaderCode != null)
                shaders.Add(VertexShaderCode);
            if (TessellationControlShaderCode != null)
                shaders.Add(TessellationControlShaderCode);
            if (TessellationEvaluationShaderCode != null)
                shaders.Add(TessellationEvaluationShaderCode);
            if (GeometryShaderCode != null)
                shaders.Add(GeometryShaderCode);
            if (PixelShaderCode != null)
                shaders.Add(PixelShaderCode);
            if (ComputeShaderCode != null)
                shaders.Add(ComputeShaderCode);
            return shaders.ToArray();
        }

        public byte Type { get; set; }
        public ShaderFormat Format { get; set; }

        // 0 = NONE, 1 = ZLIB
        public uint CompressionType { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(BfshaFileLoader loader)
        {
            Type = loader.ReadByte();
            Format = loader.ReadEnum<ShaderFormat>(false);
            loader.Seek(2); //padding
            CompressionType = loader.ReadUInt32();
            VertexShaderCode = ReadShaderCode(loader, Format);
            TessellationControlShaderCode = ReadShaderCode(loader, Format);
            TessellationEvaluationShaderCode = ReadShaderCode(loader, Format);
            GeometryShaderCode = ReadShaderCode(loader, Format);
            PixelShaderCode = ReadShaderCode(loader,  Format);
            ComputeShaderCode = ReadShaderCode(loader, Format);

            if (VertexShaderCode != null)
                VertexShaderCode.ShaderType = ShaderType.VERTEX;
            if (TessellationControlShaderCode != null)
                TessellationControlShaderCode.ShaderType = ShaderType.TESS0;
            if (TessellationEvaluationShaderCode != null)
                TessellationEvaluationShaderCode.ShaderType = ShaderType.TESS1;
            if (GeometryShaderCode != null)
                GeometryShaderCode.ShaderType = ShaderType.GEOMETRY;
            if (PixelShaderCode != null)
                PixelShaderCode.ShaderType = ShaderType.PIXEL;
            if (ComputeShaderCode != null)
                ComputeShaderCode.ShaderType = ShaderType.COMPUTE;

            loader.Seek(40); //padding
        }

        private ShaderCodeData ReadShaderCode(BfshaFileLoader loader, ShaderFormat format)
        {
            ShaderCodeData codeData = null;
            if (CompressionType == 1)
                codeData = loader.Load<ShaderCodeDataCompressed>();
            else if (format == ShaderFormat.Source)
                codeData = loader.Load<ShaderCodeDataSource>();
            else
                codeData = loader.Load<ShaderCodeDataBinary>();
            return codeData;
        }

        internal long PtrVertexShaderCodePos;
        internal long PtrTessellatioControlShaderCodePos;
        internal long PtrTessellationEvaluationShaderCodePos;
        internal long PtrGeometryShaderCodePos;
        internal long PtrPixelShaderCodePos;
        internal long PtrComputeShaderCodePos;

        void IResData.Save(BfshaFileSaver saver)
        {
            saver.Write(Type);
            saver.Write(Format, false);
            saver.Write(new byte[2]); //padding
            saver.Write(CompressionType);
            PtrVertexShaderCodePos = saver.SaveOffset();
            PtrTessellatioControlShaderCodePos = saver.SaveOffset();
            PtrTessellationEvaluationShaderCodePos = saver.SaveOffset();
            PtrGeometryShaderCodePos = saver.SaveOffset();
            PtrPixelShaderCodePos = saver.SaveOffset();
            PtrComputeShaderCodePos = saver.SaveOffset();
            saver.Write(new byte[40]); //padding
        }

        public enum ShaderFormat : byte
        {
            Binary = 0x00,
            Source = 0x03,
        }
    }
}
