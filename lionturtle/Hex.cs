namespace lionturtle
{
    public enum VertexType
    {
        Crest,
        Foot,
        Slope,
        Flat,
        Unknown,
        FootCrest
    }

    public record Hex(Vertex[] Verts, VertexType[] VertexTypes);
}