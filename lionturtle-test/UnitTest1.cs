using lionturtle;
using System;
using System.Diagnostics;
using System.Numerics;

namespace lionturtle_test
{
       public class GridTests
    {
        [Fact]
        public void One_Pair()
        {
            BlurryGrid grid = new();
            AxialPosition position0 = new AxialPosition(2, -1);
            grid.FindOrCreateBlurryValue(position0);
            AxialPosition position1 = new AxialPosition(1, -2);
            grid.FindOrCreateBlurryValue(position1);

            grid.ResolveValueAtPosition(position0, 4);
            grid.ResolveValueAtPosition(position1, 3);
        }

        [Fact]
        public void Small_Grid_With_Perlin()
        {
            BlurryGrid grid = new();
            PerlinNoise perlin = new PerlinNoise(1337);
            int size = 5;
            List<AxialPosition> positions = GridUtilities.GetSpiralVertexPositions(size*2);
            foreach(AxialPosition position in positions){
                Vector2 cartesian = GridUtilities.AxialToCartesian(position, 1);
                double heuristic = Math.Round(perlin.Noise(cartesian.X, cartesian.Y)*5.0);
                grid.ResolveValueAtPosition(position, heuristic);
            }

            Debug.Write(grid.GetStringHexes(size));
        }
    }

    public class BlurryValueTests{
        [Fact]
        public void Create_And_Constrain_A_BlurryValue()
        {
            BlurryValue blur = new BlurryValue();
            blur.Constrain(null, null);
            Assert.Null(blur.Min);
            Assert.Null(blur.Max);

            blur.Constrain(-5, null);
            Assert.Equal(-5, blur.Min);
            Assert.Null(blur.Max);

            blur.Constrain(null, 8);
            Assert.Equal(-5, blur.Min);
            Assert.Equal(8, blur.Max);
        }
    }

    public class AxialPositionTests
    {
        [Fact]
        public void Add_Axial_Positions()
        {
            AxialPosition positionA = new(1, 5);
            AxialPosition positionB = new(2, 3);

            AxialPosition result = positionA + positionB;

            Assert.Equal(new AxialPosition(3, 8), result);
        }

        [Fact]
        public void Subtract_Axial_Positions()
        {
            AxialPosition positionA = new(2, 3);
            AxialPosition positionB = new(1, 5);

            AxialPosition result = positionA - positionB;

            Assert.Equal(new AxialPosition(1, -2), result);
        }

        [Fact]
        public void Add_Axial_Direction_To_Axial_Position()
        {
            AxialPosition position = new(5, 4);
            AxialPosition direction = Constants.axialDirections[1];

            AxialPosition result = position + direction;

            Assert.Equal(new AxialPosition(6, 3), result);
        }
    }

    public class ModuloTests
    {
        [Fact]
        public void Perform_Modulo()
        {
            // turns out the % operator in c# is actually 'remainder'
            Assert.Equal(1, 21.Mod(4));
            Assert.Equal(2, 1337.Mod(3));
            Assert.Equal(2, (-7).Mod(3));
            Assert.Equal(4, (-24).Mod(7));
        }
    }

    public class GridUtilitiesTests
    {
        [Fact]
        public void Get_Vertex_Position_By_Hex()
        {
            AxialPosition hexPosition = new AxialPosition(0, 0);
            int vIndex = 0;
            AxialPosition vertexPosition = GridUtilities.GetVertexPositionForHexV(hexPosition, vIndex);
            Assert.True(vertexPosition == new AxialPosition(2, -1));
        }

        [Fact]
        public void Determine_If_Vertex_Points_Up()
        {
            AxialPosition firstUpPosition = new(4, 1);
            AxialPosition secondUpPosition = new(-8, 7);

            AxialPosition firstDownPosition = new(5, -7);
            AxialPosition secondDownPosition = new(-1, -1);

            bool firstUpResult = GridUtilities.VertexPointsUp(firstUpPosition);
            bool secondUpResult = GridUtilities.VertexPointsUp(secondUpPosition);
            bool firstDownResult = GridUtilities.VertexPointsUp(firstDownPosition);
            bool secondDownResult = GridUtilities.VertexPointsUp(secondDownPosition);

            Assert.True(firstUpResult);
            Assert.True(secondUpResult);
            Assert.False(firstDownResult);
            Assert.False(secondDownResult);
        }

        [Fact]
        public void Get_Vertex_Positions_From_Hex_Position()
        {
            AxialPosition hexPosition = new(2, -2);
            AxialPosition[] vertexPositions = GridUtilities.GetVertexPositionsFromHexPosiiton(hexPosition);
            Assert.True(vertexPositions[2] == new AxialPosition(5, -7));
            Assert.True(vertexPositions[5] == new AxialPosition(7, -5));
        }

        [Fact]
        public void Get_Spiral_Hex_Positions()
        {
            AxialPosition[] sizeZeroPositions = GridUtilities.GetSpiralHexPositions(0);
            Assert.Empty(sizeZeroPositions);

            AxialPosition[] sizeOnePositions = GridUtilities.GetSpiralHexPositions(1);
            Assert.Single(sizeOnePositions);

            AxialPosition[] sizeTwoPositions = GridUtilities.GetSpiralHexPositions(2);
            Assert.Equal(7, sizeTwoPositions.Length);

            AxialPosition[] sizeThreePositions = GridUtilities.GetSpiralHexPositions(3);
            Assert.Equal(19, sizeThreePositions.Length);
        }

        [Fact]
        public void Get_Spiral_Vertex_Positions()
        {
            List<AxialPosition> sizeZeroPositions = GridUtilities.GetSpiralVertexPositions(0);
            Assert.Empty(sizeZeroPositions);

            //confusingly, the first 'ring' is a single vertex that is
            //a hex center, so it is ignored
            List<AxialPosition> sizeOnePositions = GridUtilities.GetSpiralVertexPositions(1);
            Assert.Empty(sizeOnePositions);

            //I think it would feel weird if 1 ring got you a complete
            //hex, but you had to add 2 more rings to get the next hex
            //So instead, complete hexes land on even numbers
            List<AxialPosition> sizeTwoPositions = GridUtilities.GetSpiralVertexPositions(2);
            Assert.Equal(6, sizeTwoPositions.Count);

            List<AxialPosition> sizeThreePositions = GridUtilities.GetSpiralVertexPositions(3);
            Assert.Equal(12, sizeThreePositions.Count);

            List<AxialPosition> sizeFourPositions = GridUtilities.GetSpiralVertexPositions(4);
            Assert.Equal(24, sizeFourPositions.Count);

            List<AxialPosition> sizeFivePositions = GridUtilities.GetSpiralVertexPositions(5);
            Assert.Equal(42, sizeFivePositions.Count);
        }
    }
}