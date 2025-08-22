using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using UndertaleModLib.Models;
using UndertaleModLib.ModelsDebug;

namespace UndertaleModLib
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods| DynamicallyAccessedMemberTypes.PublicProperties| DynamicallyAccessedMemberTypes.PublicEvents| DynamicallyAccessedMemberTypes.PublicConstructors)]
    public class UndertaleDebugFORM : UndertaleChunk
    {
        public override string Name => "FORM";

        public Dictionary<string, UndertaleChunk> Chunks = new Dictionary<string, UndertaleChunk>();

        public UndertaleDebugChunkSCPT SCPT => Chunks["SCPT"] as UndertaleDebugChunkSCPT;
        public UndertaleDebugChunkDBGI DBGI => Chunks["DBGI"] as UndertaleDebugChunkDBGI;
        public UndertaleDebugChunkINST INST => Chunks["INST"] as UndertaleDebugChunkINST;
        public UndertaleDebugChunkLOCL LOCL => Chunks["LOCL"] as UndertaleDebugChunkLOCL;
        public UndertaleDebugChunkSTRG STRG => Chunks["STRG"] as UndertaleDebugChunkSTRG;

        internal override void SerializeChunk(UndertaleWriter writer)
        {
            foreach (var chunk in Chunks)
            {
                writer.Write(chunk.Value);
            }
        }

        internal override void UnserializeChunk(UndertaleReader reader)
        {
            Chunks.Clear();
            long startPos = reader.Position;
            while (reader.Position < startPos + Length)
            {
                UndertaleChunk chunk = reader.ReadUndertaleChunk();
                if (chunk != null)
                {
                    if (Chunks.ContainsKey(chunk.Name))
                        throw new IOException("Duplicate chunk " + chunk.Name);
                    Chunks.Add(chunk.Name, chunk);
                }
            }
        }

        internal override uint UnserializeObjectCount(UndertaleReader reader)
        {
            throw new NotImplementedException();
        }
    }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods| DynamicallyAccessedMemberTypes.PublicProperties| DynamicallyAccessedMemberTypes.PublicEvents| DynamicallyAccessedMemberTypes.PublicConstructors)]
    public class UndertaleDebugChunkSCPT : UndertaleListChunk<UndertaleScriptSource>
    {
        public override string Name => "SCPT";
    }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods| DynamicallyAccessedMemberTypes.PublicProperties| DynamicallyAccessedMemberTypes.PublicEvents| DynamicallyAccessedMemberTypes.PublicConstructors)]
    public class UndertaleDebugChunkDBGI : UndertaleListChunk<UndertaleDebugInfo>
    {
        public override string Name => "DBGI";
    }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods| DynamicallyAccessedMemberTypes.PublicProperties| DynamicallyAccessedMemberTypes.PublicEvents| DynamicallyAccessedMemberTypes.PublicConstructors)]
    public class UndertaleDebugChunkINST : UndertaleListChunk<UndertaleInstanceVars>
    {
        public override string Name => "INST";
    }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods| DynamicallyAccessedMemberTypes.PublicProperties| DynamicallyAccessedMemberTypes.PublicEvents| DynamicallyAccessedMemberTypes.PublicConstructors)]
    public class UndertaleDebugChunkLOCL : UndertaleListChunk<UndertaleCodeLocals>
    {
        public override string Name => "LOCL";
    }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods| DynamicallyAccessedMemberTypes.PublicProperties| DynamicallyAccessedMemberTypes.PublicEvents| DynamicallyAccessedMemberTypes.PublicConstructors)]
    public class UndertaleDebugChunkSTRG : UndertaleListChunk<UndertaleString>
    {
        public override string Name => "STRG";
    }
}
