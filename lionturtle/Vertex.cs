using System.Numerics;

namespace lionturtle
{
	public class Vertex
	{
		public double Height;
		public VertexType Type { get; set; }

        public Vertex(double height, VertexType type = VertexType.Unknown)
		{
			Height = height;
			Type = type;
        }
	}
}

public enum VertexType
{
    Crest,
    Foot,
    Slope,
    Flat,
    Unknown,
    FootCrest
}