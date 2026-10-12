
using Unity.Collections;

public interface IMessage
{
    public FixedString512Bytes Message { get; set; }
    public ushort ChunkIndex { get; set; }
    public ushort ChunkCount { get; set; }
    public uint MessageIndex { get; set; }
}