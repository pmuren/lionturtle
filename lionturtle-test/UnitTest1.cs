using lionturtle;
using System.Diagnostics;
using System.Numerics;

namespace lionturtle_test
{
    public class AxialPositionTests
    {
        [Fact]
        public void Data_Playground()
        {
            //var allModules = DataGenerator.allModules;

            //SlotGrid grid = new SlotGrid();
            //grid.CollapseEverything();

            //var hexes = new Dictionary<AxialPosition, int[]>();
            //foreach(AxialPosition position in grid.Slots.Keys)
            //{
            //    var tileHeight = grid.TileHeights[position];
            //    var relativeVertexHeights = grid.Slots[position].modules.First().GetRelativeVertexHeights();
            //    int[] hex = new int[6];
            //    for(int vertexIndex = 0; vertexIndex < 6; vertexIndex++)
            //    {
            //        hex[vertexIndex] = relativeVertexHeights[vertexIndex] + (int)Math.Floor(tileHeight);
            //    }
            //    hexes[position] = hex;
            //}

            //var hexGrid = new HexGrid();
            //hexGrid.Populate();
            //var hexesString = hexGrid.GetStringHexes();
            //Debug.Write(hexesString);

            //bool allGood = grid.ValidateAllSlots();
            //Debug.Write(allGood);

            //List<int[]> relativeVertexGroups = new List<int[]>();
            //foreach (AxialPosition position in grid.Slots.Keys)
            //{
            //    relativeVertexGroups.Add(grid.Slots[position].modules.First().GetRelativeVertexHeights());
            //}
            //Debug.Write(relativeVertexGroups);


            //the trick here is to get the hexType and rotation from the slotGrid data . . .
            //I'm half-way to doing this in LandMesh.cs (Godot)
            //var vertices24 = DataGenerator.GenerateV24Vertices(2, 1);
            //vertices24 = DataGenerator.PopulateVertexTypes(vertices24, 2, new VertexType[]
            //{
            //    VertexType.Slope, VertexType.Crest, VertexType.FootCrest,
            //    VertexType.Slope, VertexType.Slope, VertexType.Slope
            //}, new VertexType[]
            //{
            //    VertexType.Slope, VertexType.Crest, VertexType.Slope,
            //    VertexType.Slope, VertexType.Slope, VertexType.Slope
            //});

            //Debug.Write(vertices24);


            Dictionary<AxialPosition, Vertex> Vertices = new();
            Dictionary<AxialPosition, Hex> Hexes = new();

            SlotGrid slotGrid = new SlotGrid();
            slotGrid.CollapseEverything();

            var hexPositionToVertHeights = new Dictionary<AxialPosition, int[]>();
            foreach (AxialPosition position in slotGrid.Slots.Keys)
            {
                var tileHeight = slotGrid.TileHeights[position];
                var relativeVertexHeights = slotGrid.Slots[position].modules.First().GetRelativeVertexHeights();
                int[] hex = new int[6];
                for (int vertexIndex = 0; vertexIndex < 6; vertexIndex++)
                {
                    hex[vertexIndex] = relativeVertexHeights[vertexIndex] + (int)Math.Floor(tileHeight);
                }
                hexPositionToVertHeights[position] = hex;
            }

            foreach (AxialPosition position in hexPositionToVertHeights.Keys)
            {
                Hexes[position] = new Hex(new Vertex[6], new VertexType[6]);
                for (int vertexIndex = 0; vertexIndex < 6; vertexIndex++)
                {
                    AxialPosition vPosition =
                        GridUtilities.GetVertexPositionForHexV(position, vertexIndex);
                    int vHeight = hexPositionToVertHeights[position][vertexIndex];
                    Vertex currentVertex;
                    if (!Vertices.ContainsKey(vPosition))
                    {
                        currentVertex = new Vertex(vPosition, vHeight);
                    }
                    else
                    {
                        currentVertex = Vertices[vPosition];
                    }
                    Hexes[position].Verts[vertexIndex] = currentVertex;
                    Vertices[vPosition] = currentVertex;
                }
            }

            //foreach( AxialPosition position in Vertices.Keys)
            //{
            //Vertices[position].type = slotGrid.GetVertexType(position);
            //}

            foreach (AxialPosition position in slotGrid.Slots.Keys)
            {
                var module = slotGrid.Slots[position].modules.First();
                var relativeVerts = module.GetRelativeVertexHeights();

                int hexType = 0;
                int rotation = 0;
                for (int direction = 0; direction < 6; direction++)
                {
                    if (relativeVerts[direction] == 1)
                    {
                        if (rotation == 0) rotation = direction;
                        hexType++;
                    }
                }

                var cornerVertexTypes = slotGrid.GetCornerVertexTypesForHex(position);
                var edgeVertexTypes = slotGrid.GetEdgeVertexTypesForHex(position);

                var vertices24 = DataGenerator.GenerateV24Vertices(hexType, rotation);
                vertices24 = DataGenerator.PopulateVertexTypes(
                    vertices24,
                    hexType,
                    cornerVertexTypes,
                    edgeVertexTypes
                );

                //Now IN THEORY we just need to bundle these vertices into
                //triangles and create geometry out of them!
                //oh right, and I gotta add absolute heights & positions to the rels

                var triangleGroups = DataGenerator.triangleCoordinateGroups;
                foreach (AxialPosition[] triangleGroup in triangleGroups)
                {
                    var v0 = vertices24[triangleGroup[0]];
                    var v1 = vertices24[triangleGroup[1]];
                    var v2 = vertices24[triangleGroup[2]];

                    var v0AbsHeight = v0.height + slotGrid.TileHeights[position];
                    var v1AbsHeight = v1.height + slotGrid.TileHeights[position];
                    var v2AbsHeight = v2.height + slotGrid.TileHeights[position];

                    AxialPosition v0AbsPos6 = triangleGroup[0] + position * 6;
                    AxialPosition v1AbsPos6 = triangleGroup[1] + position * 6;
                    AxialPosition v2AbsPos6 = triangleGroup[2] + position * 6;

                    var v0AbsPosCartesian6 = GridUtilities.AxialPositionToVec2(v0AbsPos6);
                    var v0AbsPosXYZ = new Vector3((float)v0AbsPosCartesian6.X/6, (float)v0AbsPosCartesian6.Y/6, (float)v0AbsHeight);

                    var v1AbsPosCartesian6 = GridUtilities.AxialPositionToVec2(v1AbsPos6);
                    var v1AbsPosXYZ = new Vector3((float)v1AbsPosCartesian6.X/6, (float)v1AbsPosCartesian6.Y/6, (float)v1AbsHeight);

                    var v2AbsPosCartesian6 = GridUtilities.AxialPositionToVec2(v2AbsPos6);
                    var v2AbsPosXYZ = new Vector3((float)v2AbsPosCartesian6.X/6, (float)v2AbsPosCartesian6.Y/6, (float)v2AbsHeight);
                }
            }
            Debug.Write(Vertices);
        }

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
            int numRings = 20;
            AxialPosition axialPosition = new(0, 0);


            Random rand = new Random();
            int firstSeed = rand.Next(500);
            //int firstSeed = 95;

            //#################################################################
            int numOctaves = 16;
            double shortestWavelength = 2f;
            double amplitude = 1.5f;

            List<PerlinNoise> octaves = new List<PerlinNoise>();

            for (int i = 0; i < numOctaves; i++)
            {
                octaves.Add(new PerlinNoise(firstSeed + i));
            }

            double zero_heuristic = SampleOctaves(axialPosition, octaves, shortestWavelength, amplitude);
            grid.ManifestHexAtPosition(axialPosition, 0);

            for (int i = 0; i < numRings; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    for (int k = 0; k < i; k++)
                    {
                        axialPosition += directions[j];
                        double heuristic = SampleOctaves(axialPosition, octaves, shortestWavelength, amplitude);
                        grid.ManifestHexAtPosition(axialPosition, heuristic - zero_heuristic);
                    }
                }
                axialPosition += directions[4];
            }

            Console.WriteLine(grid.GetStringHexes());
        }

        public double SampleOctaves(AxialPosition axial, List<PerlinNoise> octaves, double shortestWavelength, double amplitude)
        {
            var v2 = GridUtilities.AxialPositionToVec2(axial);
            double composite = 0;

            for(int i = 0; i < octaves.Count; i++)
            {
                double twoToI = Math.Pow(2, i);
                double sample = octaves[i].Noise(v2.X / (shortestWavelength*twoToI), v2.Y / (shortestWavelength*twoToI)) * (amplitude*i);
                composite += sample;
            }

            return composite;
        }

        [Fact]
        public void Perlin_Height()
        {
            PerlinNoise noise = new PerlinNoise(0);

            double x = 10.5;
            double y = -4.2;
            double height = noise.Noise(x, y);

            Console.WriteLine($"Height at ({x}, {y}): {height}");
        }

        //[Fact]
        //public void Generate_Dendritic()
        //{
        //    AxialPosition[] directions = Constants.axial_directions;
        //    HexGrid grid = new();

        //    grid.ManifestHexAtPosition(new AxialPosition(0, 0), 0);

        //    int numPlacements = 0;
        //    int maxPlacements = 1000;
        //    while (numPlacements < maxPlacements)
        //    {
        //        Random rand = new Random();
        //        List<AxialPosition> hexPositions = grid.Hexes.Keys.ToList();
        //        AxialPosition randomHexPosition = hexPositions[rand.Next(hexPositions.Count)];
        //        double previousHeuristic = grid.Hexes[randomHexPosition].heuristic;

        //        AxialPosition randomDirection = directions[rand.Next(directions.Length)];

        //        AxialPosition candidateHexPosition = randomHexPosition + randomDirection;
        //        if (!grid.Hexes.ContainsKey(candidateHexPosition))
        //        {
        //            double[] possibleHeuristics = new double[] {
        //                    previousHeuristic - 1,
        //                    previousHeuristic,
        //                    previousHeuristic,
        //                    previousHeuristic,
        //                    previousHeuristic,
        //                    previousHeuristic,
        //                    previousHeuristic,
        //                    previousHeuristic,
        //                    previousHeuristic,
        //                    previousHeuristic,
        //                    previousHeuristic,
        //                    previousHeuristic,
        //                    previousHeuristic,
        //                    previousHeuristic + 1
        //                };
        //            double newHeuristic = possibleHeuristics[rand.Next(possibleHeuristics.Length)];
        //            grid.ManifestHexAtPosition(candidateHexPosition, newHeuristic);
        //        }

        //        numPlacements++;
        //    }
        //    Console.WriteLine(grid.GetStringHexes());
        //}
    }
}