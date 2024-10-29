namespace lionturtle
{
	public class Vertex
	{
		public AxialPosition position { get; set; }
		public double height;
		public VertexType type { get; set; }

        public Vertex(AxialPosition inPosition, double inHeight, VertexType inType = VertexType.Unknown)
		{
			position = inPosition;
			height = inHeight;
			type = inType;
        }
	}
}