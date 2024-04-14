using lionturtle;
using System.Diagnostics;

namespace lionturtle_test
{
    public class AxialPositionTests
    {
        [Fact]
        public void Add_Hex_Positions()
        {
            AxialPosition positionA = new(1, 5);
            AxialPosition positionB = new(2, 3);

            AxialPosition result = positionA + positionB;

            Assert.Equal(new AxialPosition(3, 8), result);
        }

        [Fact]
        public void Subtract_Hex_Positions()
        {
            AxialPosition positionA = new(2, 3);
            AxialPosition positionB = new(1, 5);

            AxialPosition result = positionA - positionB;

            Assert.Equal(new AxialPosition(1, -2), result);
        }

        [Fact]
        public void Add_Axial_Direction_To_Hex_Position()
        {
            AxialPosition position = new(5, 4);
            AxialPosition direction = Constants.axial_directions[1];

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

    public class GridTests
    {
        [Fact]
        public void Get_Vertex_Position_By_Hex()
        {
            AxialPosition hexPosition = new AxialPosition(0, 0);
            int vIndex = 0;
            AxialPosition vertexPosition = GridUtilities.GetVertexPositionForHexV(hexPosition, vIndex);
            Assert.True(vertexPosition == new AxialPosition(2, -1));
        }

        //[Fact]
        //public void Find_Existing_Vertex_By_Hex_Position()
        //{
        //    HexGrid grid = new();
        //    AxialPosition hexPosition = new AxialPosition(0, 0);
        //    AxialPosition newHexPosition = new AxialPosition(1, 0);

        //    grid.ManifestHexAtPosition(hexPosition, 0);

        //    Vertex? existingVertex = grid.FindExistingVertexForHexV(newHexPosition, 2);
        //    Assert.True(existingVertex != null);
        //}

        //[Fact]
        //public void New_Hexes_Use_Existing_Vertices()
        //{
        //    HexGrid grid = new();

        //    grid.ManifestHexAtPosition(new AxialPosition(0, 0), 0);
        //    grid.ManifestHexAtPosition(new AxialPosition(1, 0), 0);

        //    Assert.True(grid.Vertices.Count == 10);
        //    Assert.True(grid.Hexes.Count == 2);
        //    Assert.True(grid.Hexes[new AxialPosition(0, 0)].Verts[0] ==
        //        grid.Hexes[new AxialPosition(1, 0)].Verts[2]);
        //}

        [Fact]
        public void Determine_If_Vertex_Points_Up()
        {
            HexGrid grid = new();

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
        public void Apply_Constraints_To_Virtual_Vertex()
        {
            VirtualVertex vv = new(new AxialPosition(0, 0), -5, 5);

            vv.Constrain(-4, 4);
            Assert.True(vv.min == -4);
            Assert.True(vv.max == 4);

            vv.Constrain(null, null);
            Assert.True(vv.min == -4);
            Assert.True(vv.max == 4);

            vv.Constrain(null, 1);
            Assert.True(vv.min == -4);
            Assert.True(vv.max == 1);

            vv.Constrain(-1, null);
            Assert.True(vv.min == -1);
            Assert.True(vv.max == 1);

            VirtualVertex vv2 = new(new AxialPosition(0, 0), null, null);
            vv2.Constrain(1, null);
            Assert.True(vv2.min == 1);
            Assert.True(vv2.max == null);

            VirtualVertex vv3 = new(new AxialPosition(0, 0), null, null);
            vv3.Constrain(null, 7);
            Assert.True(vv3.min == null);
            Assert.True(vv3.max == 7);
        }

        [Fact]
        public void Resolve_Virtual_Vertex_With_Heuristic()
        {
            VirtualVertex vv = new(new AxialPosition(0, 0), 4, 10);
            double resolution = vv.Resolve(0);
            Assert.True(resolution == 4);

            vv = new(new AxialPosition(0, 0), 4, 10);
            resolution = vv.Resolve(11);
            Assert.True(resolution == 10);

            vv = new(new AxialPosition(0, 0), 4, 10);
            resolution = vv.Resolve(7);
            Assert.True(resolution == 7);

            vv = new(new AxialPosition(0, 0), null, 10);
            resolution = vv.Resolve(3);
            Assert.True(resolution == 3);

            vv = new(new AxialPosition(0, 0), -40, null);
            resolution = vv.Resolve(50);
            Assert.True(resolution == 50);

            vv = new(new AxialPosition(0, 0), null, null);
            resolution = vv.Resolve(84);
            Assert.True(resolution == 84);

            vv = new(new AxialPosition(0, 0), 4, null);
            resolution = vv.Resolve(4);
            Assert.True(resolution == 4);

            vv = new(new AxialPosition(0, 0), null, 10);
            resolution = vv.Resolve(10);
            Assert.True(resolution == 10);

            vv = new(new AxialPosition(0, 0), 4, 10);
            resolution = vv.Resolve(4);
            Assert.True(resolution == 4);

            vv = new(new AxialPosition(0, 0), 4, 10);
            resolution = vv.Resolve(10);
            Assert.True(resolution == 10);
        }

        [Fact]
        public void Propagate_Constraints_Direct_Neighbor()
        {
            HexGrid grid = new();
            AxialPosition primaryVertexPosition = new(2, -1);
            AxialPosition secondaryVertexPosition = new(4, -2);
            grid.VirtualVertices[primaryVertexPosition] = new(primaryVertexPosition, 4, 4);
            grid.VirtualVertices[secondaryVertexPosition] = new(secondaryVertexPosition, 5, 10);
            grid.PropagateConstraints(primaryVertexPosition);

            Assert.True(grid.VirtualVertices[new AxialPosition(8, -4)].min == 5);
        }

        [Fact]
        public void Generate_Spiral_Perlin()
        {
            AxialPosition[] directions = Constants.axial_directions;

            HexGrid grid = new();

            //Spiral from center outward
            int numRings = 8;
            AxialPosition walkPosition = new(0, 0);

            double zero_heuristic = GetPerlinHeuristic(walkPosition);
            grid.ManifestHexAtPosition(walkPosition, zero_heuristic);

            for (int i = 0; i < numRings; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    for (int k = 0; k < i; k++)
                    {
                        walkPosition += directions[j];

                        double heuristic = GetPerlinHeuristic(walkPosition);

                        grid.ManifestHexAtPosition(walkPosition, heuristic);
                    }
                }
                walkPosition += directions[4];
            }

            Console.WriteLine(grid.GetStringHexes());
        }

        static Random rand = new Random();
        //static int firstSeed = rand.Next(200);
        static int firstSeed = 99;
        PerlinNoise lowNoise = new PerlinNoise(firstSeed + 0);
        PerlinNoise midNoise = new PerlinNoise(firstSeed + 1);
        PerlinNoise highNoise = new PerlinNoise(firstSeed + 2);
        PerlinNoise superHighNoise = new PerlinNoise(firstSeed + 3);

        public double GetPerlinHeuristic(AxialPosition axial)
        {
            var v2 = GridUtilities.AxialPositionToVector2(axial);
            double perlinDeepOctave = lowNoise.Noise(v2.X / 128, v2.Y / 128) * 2f;
            //double perlinBassOctave = lowNoise.Noise(v2.X / 64, v2.Y / 64) * 2f;
            //double perlinSubOctave = lowNoise.Noise(v2.X / 32, v2.Y / 32) * 2f;
            //double perlinLowOctave = lowNoise.Noise(v2.X / 16, v2.Y / 16) * 2f;
            //double perlinMidOctave = midNoise.Noise(v2.X / 8, v2.Y / 8) * 3.5f;
            //double perlinHighOctave = highNoise.Noise(v2.X / 4, v2.Y / 4) * 0.5f;
            //double perlinSuperHighOctave = superHighNoise.Noise(v2.X / 2, v2.Y / 2) * 0.25f;
            double perlinBassOctave = lowNoise.Noise(v2.X / 64, v2.Y / 64) * 4f;
            double perlinSubOctave = lowNoise.Noise(v2.X / 32, v2.Y / 32) * 3f;
            double perlinLowOctave = lowNoise.Noise(v2.X / 16, v2.Y / 16) * 1f;
            double perlinMidOctave = midNoise.Noise(v2.X / 8, v2.Y / 8) * 5f;
            double perlinHighOctave = highNoise.Noise(v2.X / 4, v2.Y / 4) * 1f;
            double perlinSuperHighOctave = superHighNoise.Noise(v2.X / 2, v2.Y / 2) * 0f;
            //int perlinHeuristic = (int)Math.Floor((perlinLowOctave + perlinMidOctave + perlinHighOctave + perlinSuperHighOctave) / 4.0f);
            //int perlinHeuristic = (int)Math.Floor((perlinLowOctave + perlinMidOctave + perlinHighOctave) / 3.0f);

            double boost = 1f;

            double perlinHeuristic = (perlinDeepOctave + perlinLowOctave + perlinMidOctave + perlinHighOctave + perlinSuperHighOctave + perlinSubOctave + perlinBassOctave)*boost;
            return perlinHeuristic;
        }

        //var v2 = GridUtilities.AxialPositionToVector2(axial);
        //double perlinBassOctave = lowNoise.Noise(v2.X / 64, v2.Y / 64) * 4f;
        //double perlinSubOctave = lowNoise.Noise(v2.X / 32, v2.Y / 32) * 3f;
        //double perlinLowOctave = lowNoise.Noise(v2.X / 16, v2.Y / 16) * 1f;
        //double perlinMidOctave = midNoise.Noise(v2.X / 8, v2.Y / 8) * 5f;
        //double perlinHighOctave = highNoise.Noise(v2.X / 4, v2.Y / 4) * 1f;
        //double perlinSuperHighOctave = superHighNoise.Noise(v2.X / 2, v2.Y / 2) * 0f;

        [Fact]
        public void Perlin_Height()
        {
            PerlinNoise noise = new PerlinNoise(0);

            double x = 10.5;
            double y = -4.2;
            double height = noise.Noise(x, y);

            Console.WriteLine($"Height at ({x}, {y}): {height}");
        }

        [Fact]
        public void Generate_Dendritic()
        {
            AxialPosition[] directions = Constants.axial_directions;
            HexGrid grid = new();

            grid.ManifestHexAtPosition(new AxialPosition(0, 0), 0);

            int numPlacements = 0;
            int maxPlacements = 1000;
            while (numPlacements < maxPlacements)
            {
                Random rand = new Random();
                List<AxialPosition> hexPositions = grid.Hexes.Keys.ToList();
                AxialPosition randomHexPosition = hexPositions[rand.Next(hexPositions.Count)];
                double previousHeuristic = grid.Hexes[randomHexPosition].heuristic;

                AxialPosition randomDirection = directions[rand.Next(directions.Length)];

                AxialPosition candidateHexPosition = randomHexPosition + randomDirection;
                if (!grid.Hexes.ContainsKey(candidateHexPosition))
                {
                    double[] possibleHeuristics = new double[] {
                            previousHeuristic - 1,
                            previousHeuristic,
                            previousHeuristic,
                            previousHeuristic,
                            previousHeuristic,
                            previousHeuristic,
                            previousHeuristic,
                            previousHeuristic,
                            previousHeuristic,
                            previousHeuristic,
                            previousHeuristic,
                            previousHeuristic,
                            previousHeuristic,
                            previousHeuristic + 1
                        };
                    double newHeuristic = possibleHeuristics[rand.Next(possibleHeuristics.Length)];
                    grid.ManifestHexAtPosition(candidateHexPosition, newHeuristic);
                }

                numPlacements++;
            }
            Console.WriteLine(grid.GetStringHexes());
        }
    }
}