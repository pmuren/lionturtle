namespace lionturtle
{
	public class Vertex
	{
		public AxialPosition position { get; set; }
		public int height;

        public Vertex(AxialPosition inPosition, int inHeight)
		{
			position = inPosition;
			height = inHeight;
        }
	}
}