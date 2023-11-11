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

        [Fact]
        public void Find_Existing_Vertex_By_Hex_Position()
        {
            HexGrid grid = new();
            AxialPosition hexPosition = new AxialPosition(0, 0);
            AxialPosition newHexPosition = new AxialPosition(1, 0);

            grid.ManifestHexAtPosition(hexPosition, 0);

            Vertex? existingVertex = grid.FindExistingVertexForHexV(newHexPosition, 2);
            Assert.True(existingVertex != null);
        }

        [Fact]
        public void New_Hexes_Use_Existing_Vertices()
        {
            HexGrid grid = new();

            grid.ManifestHexAtPosition(new AxialPosition(0, 0), 0);
            grid.ManifestHexAtPosition(new AxialPosition(1, 0), 0);

            Assert.True(grid.Vertices.Count == 10);
            Assert.True(grid.Hexes.Count == 2);
            Assert.True(grid.Hexes[new AxialPosition(0, 0)].Verts[0] ==
                grid.Hexes[new AxialPosition(1, 0)].Verts[2]);
        }

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

            vv.Constrain(null, 1);
            Assert.True(vv.min == -4);
            Assert.True(vv.max == 1);

            //Assert.Throws<InvalidOperationException>(() => vv.Constrain(2, null));

            VirtualVertex vv2 = new(new AxialPosition(0, 0), null, null);
            vv2.Constrain(1, null);
            Assert.True(vv2.min == 1);
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
        public void Get_Constraints_From_Resolved_Vertices()
        {
            AxialPosition positionA = new(2, -1);
            Vertex vA = new Vertex(positionA, 4);
            AxialPosition positionB = new(1, -2);
            Vertex vB = new Vertex(positionB, 7);

            VirtualVertex[] constraints = GridUtilities.GetConstraintsCausedByVertexPair(vA, vB);
            bool certainConstraintExists = constraints.Any(c =>
                c.position == new AxialPosition(-1, -4) &&
                c.min == 7 && c.max == null);
            Assert.True(certainConstraintExists);


            positionA = new(1, -2);
            positionB = new(2, -4);

            vA = new Vertex(positionA, 1);
            vB = new Vertex(positionB, 0);

            constraints = GridUtilities.GetConstraintsCausedByVertexPair(vA, vB);
            certainConstraintExists = constraints.Any(c =>
                c.position == new AxialPosition(2, -1) &&
                c.min == 1 && c.max == null);
            Assert.True(certainConstraintExists);


            positionA = new(2, -1);
            positionB = new(1, -2);

            vA = new Vertex(positionA, 7);
            vB = new Vertex(positionB, 7);

            constraints = GridUtilities.GetConstraintsCausedByVertexPair(vA, vB);
            bool noConstraintsReturned = constraints.Length == 0;
            Assert.True(noConstraintsReturned);
        }

        [Fact]
        public void Determine_Local_Constraints()
        {
            HexGrid grid = new();
            AxialPosition[] h0VPositions = GridUtilities.GetVertexPositionsFromHexPosiiton(new AxialPosition(0, -1));
            int[] h0Heights = new int[] { 0, 0, 0, 0, 1, 1 };
            for (int i = 0; i < h0VPositions.Length; i++)
            {
                if (!grid.Vertices.ContainsKey(h0VPositions[i]))
                {
                    grid.Vertices[h0VPositions[i]] = new Vertex(h0VPositions[i], h0Heights[i]);
                }
            }

            AxialPosition currentVertexPosition = new AxialPosition(2, -1);
            Dictionary<AxialPosition, VirtualVertex> localConstraints = grid.DetermineLocalConstraints(currentVertexPosition);
            Assert.True(localConstraints[currentVertexPosition].min == 1);
        }

        [Fact]
        public void Determine_Min_And_Max_From_Local_Constraints()
        {
            HexGrid grid = new();
            AxialPosition[] h0VPositions = GridUtilities.GetVertexPositionsFromHexPosiiton(new AxialPosition(0, -1));
            int[] h0Heights = new int[] { 0, 0, 0, 0, 1, 1 };
            for (int i = 0; i < h0VPositions.Length; i++)
            {
                if (!grid.Vertices.ContainsKey(h0VPositions[i]))
                {
                    grid.Vertices[h0VPositions[i]] = new Vertex(h0VPositions[i], h0Heights[i]);
                }
            }

            AxialPosition currentVertexPosition = new AxialPosition(2, -1);
            Dictionary<AxialPosition, VirtualVertex> localConstraints = grid.DetermineLocalConstraints(currentVertexPosition);
            VirtualVertex blurryVertex = GridUtilities.GetBlurryVertex(currentVertexPosition, localConstraints);
            //This GetRange method will need to subtract current position from localConstraints positions
            //to convert to coordinates relative to the current vertex's position
            //then do diagonal hex calculations. :)

            Assert.True(blurryVertex.min == 1);
        }

        [Fact]
        public void Determine_Min_And_Max_From_Local_Constraints_2()
        {
            HexGrid grid = new();

            grid.Vertices[new AxialPosition(-2, -5)] = new Vertex(new AxialPosition(-2, -5), 7);
            grid.Vertices[new AxialPosition(-1, -4)] = new Vertex(new AxialPosition(-1, -4), 6);

            grid.Vertices[new AxialPosition(-7, 5)] = new Vertex(new AxialPosition(-7, 5), 3);
            grid.Vertices[new AxialPosition(-5, 4)] = new Vertex(new AxialPosition(-5, 4), 4);

            grid.Vertices[new AxialPosition(8, -7)] = new Vertex(new AxialPosition(8, -7), 1);
            grid.Vertices[new AxialPosition(7, -5)] = new Vertex(new AxialPosition(7, -5), 2);

            AxialPosition xVertexPosition = new AxialPosition(1, 1);
            Dictionary<AxialPosition, VirtualVertex> xLocalConstraints = grid.DetermineLocalConstraints(xVertexPosition);
            VirtualVertex xBlurryVertex = GridUtilities.GetBlurryVertex(xVertexPosition, xLocalConstraints);

            AxialPosition yVertexPosition = new AxialPosition(2, -1);
            Dictionary<AxialPosition, VirtualVertex> yLocalConstraints = grid.DetermineLocalConstraints(yVertexPosition);
            VirtualVertex yBlurryVertex = GridUtilities.GetBlurryVertex(yVertexPosition, yLocalConstraints);

            Assert.True(xBlurryVertex.min == 2);
            Assert.True(xBlurryVertex.max == null);
            Assert.True(yBlurryVertex.min == 2);
            Assert.True(yBlurryVertex.max == null);
        }

        [Fact]
        public void Determine_Min_And_Max_From_Local_Constraints_3()
        {
            HexGrid grid = new();

            grid.Vertices[new AxialPosition(2, -4)] = new Vertex(new AxialPosition(2, -4), 7);
            grid.Vertices[new AxialPosition(1, -2)] = new Vertex(new AxialPosition(1, -2), 6);

            grid.Vertices[new AxialPosition(2, 2)] = new Vertex(new AxialPosition(2, 2), 3);
            grid.Vertices[new AxialPosition(1, 1)] = new Vertex(new AxialPosition(1, 1), 4);

            AxialPosition xVertexPosition = new AxialPosition(-1, -1);
            Dictionary<AxialPosition, VirtualVertex> xLocalConstraints = grid.DetermineLocalConstraints(xVertexPosition);
            VirtualVertex xBlurryVertex = GridUtilities.GetBlurryVertex(xVertexPosition, xLocalConstraints);

            Assert.True(xBlurryVertex.min == 4);
            Assert.True(xBlurryVertex.max == 6);
        }

        [Fact]
        public void Make_Some_Hexes()
        {
            HexGrid grid = new();

            grid.ManifestHexAtPosition(new AxialPosition(0, 0), 0);

            int numPlacements = 0;
            int maxPlacements = 2000;
            while(numPlacements < maxPlacements)
            {
                Random rand = new Random();
                List<AxialPosition> hexPositions = grid.Hexes.Keys.ToList();
                AxialPosition randomHexPosition = hexPositions[rand.Next(hexPositions.Count)];
                int previousHeuristic = grid.Hexes[randomHexPosition].heuristic;

                AxialPosition[] directions = Constants.axial_directions;
                AxialPosition randomDirection = directions[rand.Next(directions.Length)];

                AxialPosition candidateHexPosition = randomHexPosition + randomDirection;
                if (!grid.Hexes.ContainsKey(candidateHexPosition))
                {
                    int[] possibleHeuristics = new int[] {
                        previousHeuristic - 1,
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
                    int newHeuristic = possibleHeuristics[rand.Next(possibleHeuristics.Length)];
                    grid.ManifestHexAtPosition(candidateHexPosition, newHeuristic);
                }

                numPlacements++;
            }

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
            //                if(!grid.Hexes.ContainsKey(new AxialPosition(q, r)))
            //                    grid.ManifestHexAtPosition(new AxialPosition(q, r), new Random().Next(0, 4));
            //            }
            //        }
            //    }
            //}

            string stringHexes = "hexes = {";
            foreach (KeyValuePair<AxialPosition, Hex> pair in grid.Hexes)
            {
                AxialPosition position = pair.Key;
                Hex hex = pair.Value;

                string stringHex = $"HexPosition({position.Q}, {position.R}): Hex([";

                for (int j = 0; j < hex.Verts.Length; j++)
                {
                    if (j < hex.Verts.Length - 1)
                        stringHex += $"{hex.Verts[j].height}, ";
                    else
                        stringHex += $"{hex.Verts[j].height}])";
                }

                stringHexes += stringHex;
                stringHexes += ", ";
            }
            stringHexes += "}";
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