namespace lionturtle
{
	public class Vertex
	{
		public AxialPosition position { get; set; }
		public double height;

        public Vertex(AxialPosition inPosition, double inHeight)
		{
			position = inPosition;
			height = inHeight;
        }
	}
}