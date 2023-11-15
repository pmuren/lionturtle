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
            int resolution = vv.Resolve(0);
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

            Assert.True(grid.VirtualVertices[new AxialPosition(8, -7)].min == 5);
        }

        [Fact]
        public void Propagate_Constraints_Second_Neighbor()
        {
            HexGrid grid = new();
            AxialPosition primaryVertexPosition = new(2, -1);
            AxialPosition secondaryVertexPosition = new(5, -1);
            grid.VirtualVertices[primaryVertexPosition] = new(primaryVertexPosition, 7, 7);
            grid.VirtualVertices[secondaryVertexPosition] = new(secondaryVertexPosition, 2, 3);
            grid.PropagateConstraints(primaryVertexPosition);

            Assert.True(grid.VirtualVertices[new AxialPosition(-1, -1)].min == 7);
            Assert.True(grid.VirtualVertices[new AxialPosition(8, -1)].max == 3);
        }

        [Fact]
        public void Propagate_Constraints_Chain_Reaction()
        {
            HexGrid grid = new();
            AxialPosition primaryVertexPosition = new(2, -1);
            AxialPosition secondaryVertexPosition = new(5, -1);
            AxialPosition tertiaryVertexPosition = new(11, -1);
            grid.VirtualVertices[primaryVertexPosition] = new(primaryVertexPosition, 7, 7);
            grid.VirtualVertices[secondaryVertexPosition] = new(secondaryVertexPosition, 2, 3);
            grid.VirtualVertices[tertiaryVertexPosition] = new(tertiaryVertexPosition, 9, null);
            grid.ResolveVertexAtPosition(primaryVertexPosition, 7);

            Assert.True(grid.VirtualVertices[new AxialPosition(14, -1)].min == 9);
        }

        [Fact]
        public void Make_Some_Hexes()
        {
            HexGrid grid = new();

            //grid.ManifestHexAtPosition(new AxialPosition(0, 0), 0);

            //int numPlacements = 0;
            //int maxPlacements = 25000;
            //while (numPlacements < maxPlacements)
            //{
            //    Random rand = new Random();
            //    List<AxialPosition> hexPositions = grid.Hexes.Keys.ToList();
            //    AxialPosition randomHexPosition = hexPositions[rand.Next(hexPositions.Count)];
            //    int previousHeuristic = grid.Hexes[randomHexPosition].heuristic;

            //    AxialPosition[] directions = Constants.axial_directions;
            //    AxialPosition randomDirection = directions[rand.Next(directions.Length)];

            //    AxialPosition candidateHexPosition = randomHexPosition + randomDirection;
            //    if (!grid.Hexes.ContainsKey(candidateHexPosition))
            //    {
            //        int[] possibleHeuristics = new int[] {
            //                previousHeuristic - 1,
            //                previousHeuristic,
            //                previousHeuristic,
            //                previousHeuristic,
            //                previousHeuristic,
            //                previousHeuristic,
            //                previousHeuristic + 1
            //            };
            //        int newHeuristic = possibleHeuristics[rand.Next(possibleHeuristics.Length)];
            //        grid.ManifestHexAtPosition(candidateHexPosition, newHeuristic);
            //    }

            //    numPlacements++;
            //}

            //int numRings = 20;
            //int lowIndex = -1 * numRings + 1;
            //int highIndex = numRings;

            //for (int q = lowIndex; q < highIndex; q++)
            //{
            //    for (int r = lowIndex; r < highIndex; r++)
            //    {
            //        for (int s = lowIndex; s < highIndex; s++)
            //        {
            //            if (q + r + s == 0)
            //            {
            //                if (!grid.Hexes.ContainsKey(new AxialPosition(q, r)))
            //                    grid.ManifestHexAtPosition(new AxialPosition(q, r), new Random().Next(0, 4));
            //            }
            //        }
            //    }
            //}


            //Spiral from center outward
            AxialPosition[] directions = Constants.axial_directions;

            int numRings = 50;
            AxialPosition walkPosition = new(0, 0);
            int walkHeight = 0;

            grid.ManifestHexAtPosition(walkPosition, walkHeight);

            for (int i = 1; i < numRings; i++)
            {
                walkPosition += directions[4];

                for (int j = 0; j < 6; j++)
                {
                    for (int k = 0; k < i; k++)
                    {
                        walkPosition += directions[j];

                        Dictionary<AxialPosition, Hex> nearbyHexes = new();
                        for(int n = 0; n < 6; n++)
                        {
                            if(grid.Hexes.ContainsKey(walkPosition + directions[n]))
                            {
                                nearbyHexes[walkPosition + directions[n]] = grid.Hexes[walkPosition + directions[n]];
                            }

                            for(int nn = 0; nn < 6; nn++)
                            {
                                if(grid.Hexes.ContainsKey(walkPosition + directions[n] + directions[nn]))
                                {
                                    nearbyHexes[walkPosition + directions[n] + directions[nn]] = grid.Hexes[walkPosition + directions[n] + directions[nn]];
                                }
                            }
                        }

                        HashSet<Vertex> nearbyVertices = new();
                        foreach(AxialPosition key in nearbyHexes.Keys)
                        {
                            for(int m = 0; m < 6; m++)
                            {
                                nearbyVertices.Add(nearbyHexes[key].Verts[m]);
                            }
                        }

                        int averageHeight = walkHeight;
                        if(nearbyVertices.Count != 0)
                        {
                            int sum = 0;
                            foreach(Vertex v in nearbyVertices)
                            {
                                sum += v.height;
                            }
                            averageHeight = sum / nearbyVertices.Count;
                        }

                        walkHeight = averageHeight;
                        if(new Random().Next(0, 5) > 3)
                        {
                            walkHeight += new Random().Next(0, 3) - 1;
                        }

                        if (new Random().Next(0, 5) > 3)
                        {
                            walkHeight += new Random().Next(0, 3) - 1;
                        }


                        if (!grid.Hexes.ContainsKey(walkPosition))
                            grid.ManifestHexAtPosition(walkPosition, walkHeight);
                    }
                }
            }

            string stringHexes = grid.GetStringHexes();
            Assert.True(1 + 1 == 2);
        }

        //[Fact]
        //public void Manifest_New_Vertex_At_Position()
        //{
        //    HexGrid grid = new();
        //    AxialPosition vertexPosition = new AxialPosition(2, -1);
        //    VirtualVertex blurryVertex = grid.DeterminePossibleValuesForVertex(vertexPosition);
        //    int vertexHeight = blurryVertex.Resolve(5);
        //    Vertex newVertex = new Vertex(vertexPosition, vertexHeight);
        //}

        //[Fact]
        //public void The_Algorithm()
        //{
        //    HexGrid grid = new();
        //    Func<float, float, int> heuristicSampler = (float x, float y) => new Random().Next(0, 6);

        //    //Manifest a new hexagon
        //        //Manifest a new Vertex
        //        //Use existing vertices to discover constraints (virtual vertices)
        //        //Use rules that respect constraints to determine range of allowed values for vertex
        //        //Manifest according to allowed range and heuristic
        //}
    }
}