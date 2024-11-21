using lionturtle;
using System.Diagnostics;
using System.Numerics;

namespace lionturtle_test
{
    public class GeoGridTests
    {
        [Fact]
        public void Place_One()
        {
            GeoGrid gGrid = new GeoGrid();
            gGrid.AddGeo(new AxialPosition(0, 0) + Constants.axialDirections[0]);
        }
    }

    public class TileGridTests
    {
        [Fact]
        public void Dendritic_Growth_One()
        {
            TileGrid tGrid = new TileGrid();
            tGrid.GrowDendritic();
            Assert.Equal(2, tGrid.Tiles.Count);
        }

        [Fact]
        public void Dendritic_Growth_Hundred()
        {
            TileGrid tGrid = new TileGrid();
            for(int i = 0; i < 100; i++)
            {
                tGrid.GrowDendritic();
            }
            Assert.Equal(101, tGrid.Tiles.Count);
        }

        [Fact]
        public void Dendritic_Growth_TenThousand()
        {
            TileGrid tGrid = new TileGrid();
            for (int i = 0; i < 10000; i++)
            {
                tGrid.GrowDendritic();
            }
            Assert.Equal(10001, tGrid.Tiles.Count);
        }
    }

    public class SlotGridTests
    {
        [Fact]
        public void Gathering_Support()
        {
            SlotGrid sGrid = new SlotGrid();
            AxialPosition slotAPosition = new AxialPosition(0, 0);
            AxialPosition slotBPosition = new AxialPosition(0, 0) + Constants.axialDirections[0];
            AxialPosition slotCPosition = new AxialPosition(0, 0) + Constants.axialDirections[1];
            sGrid.Slots[slotAPosition] = new Slot(DataGenerator.allModules);
            sGrid.Slots[slotBPosition] = new Slot(DataGenerator.allModules);
            List<Module> supportedInSlotC = sGrid.GatherSupportedModules(slotCPosition);
            Assert.Equal(19, supportedInSlotC.Count);
        }

        [Fact]
        public void Gathering_Support_From_Resolved_Flat_Neighbors()
        {
            SlotGrid sGrid = new SlotGrid();
            AxialPosition slotAPosition = new AxialPosition(0, 0);
            AxialPosition slotBPosition = new AxialPosition(0, 0) + Constants.axialDirections[0];
            AxialPosition slotCPosition = new AxialPosition(0, 0) + Constants.axialDirections[1];
            sGrid.Slots[slotAPosition] = new Slot(new List<Module>{DataGenerator.allModules.First()});
            sGrid.Slots[slotBPosition] = new Slot(new List<Module>{DataGenerator.allModules.First()});
            List<Module> supportedInSlotC = sGrid.GatherSupportedModules(slotCPosition);
            Assert.Equal(7, supportedInSlotC.Count);
        }

        [Fact]
        public void Expand_To_Include_One()
        {
            SlotGrid sGrid = new SlotGrid();
            AxialPosition slotAPosition = new AxialPosition(0, 0);
            sGrid.Slots[slotAPosition] = new Slot(new List<Module> { DataGenerator.allModules.First() });
            sGrid.ExpandToInclude(new AxialPosition(0, 0) + Constants.axialDirections[0]);

            Assert.Equal(new List<AxialPosition> {
                new AxialPosition(0, 0),
                new AxialPosition(1, 0)
            }, sGrid.Slots.Keys.ToList());
        }

        [Fact]
        public void Expand_To_Include_Multiple()
        {
            SlotGrid sGrid = new SlotGrid();
            AxialPosition slotAPosition = new AxialPosition(0, 0);
            sGrid.Slots[slotAPosition] = new Slot(new List<Module> { DataGenerator.allModules.First() });
            sGrid.ExpandToInclude(new AxialPosition(0, 0) + Constants.axialDirections[0]);
            sGrid.ExpandToInclude(new AxialPosition(0, 0) + Constants.axialDirections[2]);
            sGrid.ExpandToInclude(new AxialPosition(0, 0) + Constants.axialDirections[1] + Constants.axialDirections[1]);

            Assert.Equal(new List<AxialPosition> {
                new AxialPosition(0, 0),
                new AxialPosition(1, 0),
                new AxialPosition(1, -1),
                new AxialPosition(0, -1),
                new AxialPosition(2, -1),
                new AxialPosition(2, -2),
                new AxialPosition(1, -2)
            }, sGrid.Slots.Keys.ToList());
        }

        [Fact]
        public void ExpandToTwoAway()
        {
            SlotGrid sGrid = new SlotGrid();
            AxialPosition slotAPosition = new AxialPosition(0, 0);
            sGrid.Slots[slotAPosition] = new Slot(new List<Module> { DataGenerator.allModules.First() });
            sGrid.ExpandToInclude(new AxialPosition(0, 0) + Constants.axialDirections[1] + Constants.axialDirections[1]);

            Assert.Equal(new List<AxialPosition> {
                new AxialPosition(0, 0),
                new AxialPosition(1, -1),
                new AxialPosition(2, -2)
            }, sGrid.Slots.Keys.ToList());
        }
    }

    public class ArcTests
    {
        [Fact]
        public void Normalizing_Arc()
        {
            SlotGrid sGrid = new();
            List<int> unsorted = new List<int> { 1, 2, 5, 0 };
            var sorted = sGrid.NormalizeArc(unsorted);
            Assert.Equal(new List<int> { 5, 0, 1, 2 }, sorted);

            List<int> unsorted2 = new List<int> { 0, 1, 5 };
            var sorted2 = sGrid.NormalizeArc(unsorted2);
            Assert.Equal(new List<int> { 5, 0, 1 }, sorted2);

            List<int> unsorted3 = new List<int> { 0, 1, 2, 4, 5 };
            var sorted3 = sGrid.NormalizeArc(unsorted3);
            Assert.Equal(new List<int> { 4, 5, 0, 1, 2 }, sorted3);

            List<int> unsorted4 = new List<int> { 1, 2, 3 };
            var sorted4 = sGrid.NormalizeArc(unsorted4);
            Assert.Equal(new List<int> { 1, 2, 3 }, sorted4);

            List<int> unsorted5 = new List<int> { 3, 4, 5, 0, 1, 2 };
            var sorted5 = sGrid.NormalizeArc(unsorted5);
            Assert.Equal(new List<int> { 3, 4, 5, 0, 1, 2 }, sorted5);
        }
    }

    public class AxialPositionTests
    {
        //[Fact]
        //public void Data_Playground()
        //{
        //    //var allModules = DataGenerator.allModules;

        //    //SlotGrid grid = new SlotGrid();
        //    //grid.CollapseEverything();

        //    //var hexes = new Dictionary<AxialPosition, int[]>();
        //    //foreach(AxialPosition position in grid.Slots.Keys)
        //    //{
        //    //    var tileHeight = grid.TileHeights[position];
        //    //    var relativeVertexHeights = grid.Slots[position].modules.First().GetRelativeVertexHeights();
        //    //    int[] hex = new int[6];
        //    //    for(int vertexIndex = 0; vertexIndex < 6; vertexIndex++)
        //    //    {
        //    //        hex[vertexIndex] = relativeVertexHeights[vertexIndex] + (int)Math.Floor(tileHeight);
        //    //    }
        //    //    hexes[position] = hex;
        //    //}

        //    //var hexGrid = new HexGrid();
        //    //hexGrid.Populate();
        //    //var hexesString = hexGrid.GetStringHexes();
        //    //Debug.Write(hexesString);

        //    //bool allGood = grid.ValidateAllSlots();
        //    //Debug.Write(allGood);

        //    //List<int[]> relativeVertexGroups = new List<int[]>();
        //    //foreach (AxialPosition position in grid.Slots.Keys)
        //    //{
        //    //    relativeVertexGroups.Add(grid.Slots[position].modules.First().GetRelativeVertexHeights());
        //    //}
        //    //Debug.Write(relativeVertexGroups);


        //    //the trick here is to get the hexType and rotation from the slotGrid data . . .
        //    //I'm half-way to doing this in LandMesh.cs (Godot)
        //    //var vertices24 = DataGenerator.GenerateV24Vertices(2, 1);
        //    //vertices24 = DataGenerator.PopulateVertexTypes(vertices24, 2, new VertexType[]
        //    //{
        //    //    VertexType.Slope, VertexType.Crest, VertexType.FootCrest,
        //    //    VertexType.Slope, VertexType.Slope, VertexType.Slope
        //    //}, new VertexType[]
        //    //{
        //    //    VertexType.Slope, VertexType.Crest, VertexType.Slope,
        //    //    VertexType.Slope, VertexType.Slope, VertexType.Slope
        //    //});

        //    //Debug.Write(vertices24);


        //    Dictionary<AxialPosition, Vertex> Vertices = new();
        //    Dictionary<AxialPosition, Hex> Hexes = new();

        //    SlotGrid slotGrid = new SlotGrid();
        //    slotGrid.CollapseEverything();

        //    var hexPositionToVertHeights = new Dictionary<AxialPosition, int[]>();
        //    foreach (AxialPosition position in slotGrid.Slots.Keys)
        //    {
        //        var tileHeight = slotGrid.TileHeights[position];
        //        var relativeVertexHeights = slotGrid.Slots[position].modules.First().GetRelativeVertexHeights();
        //        int[] hex = new int[6];
        //        for (int vertexIndex = 0; vertexIndex < 6; vertexIndex++)
        //        {
        //            hex[vertexIndex] = relativeVertexHeights[vertexIndex] + (int)Math.Floor(tileHeight);
        //        }
        //        hexPositionToVertHeights[position] = hex;
        //    }

        //    //creating or finding Vertex objects and adding them to the Hexes and Vertices dictionaries
        //    foreach (AxialPosition position in hexPositionToVertHeights.Keys)
        //    {
        //        Hexes[position] = new Hex(new Vertex[6], new VertexType[6]);
        //        for (int vertexIndex = 0; vertexIndex < 6; vertexIndex++)
        //        {
        //            AxialPosition vPosition =
        //                GridUtilities.GetVertexPositionForHexV(position, vertexIndex);
        //            int vHeight = hexPositionToVertHeights[position][vertexIndex];
        //            Vertex currentVertex;
        //            if (!Vertices.ContainsKey(vPosition))
        //            {
        //                currentVertex = new Vertex(vPosition, vHeight);
        //            }
        //            else
        //            {
        //                currentVertex = Vertices[vPosition];
        //            }
        //            Hexes[position].Verts[vertexIndex] = currentVertex;
        //            Vertices[vPosition] = currentVertex;
        //        }
        //    }

        //    //foreach( AxialPosition position in Vertices.Keys)
        //    //{
        //    //Vertices[position].type = slotGrid.GetVertexType(position);
        //    //}

        //    foreach (AxialPosition position in slotGrid.Slots.Keys)
        //    {
        //        var module = slotGrid.Slots[position].modules.First();
        //        var relativeVerts = module.GetRelativeVertexHeights();

        //        int hexType = 0;
        //        int rotation = 0;
        //        for (int direction = 0; direction < 6; direction++)
        //        {
        //            if (relativeVerts[direction] == 1)
        //            {
        //                if (rotation == 0) rotation = direction;
        //                hexType++;
        //            }
        //        }

        //        var cornerVertexTypes = slotGrid.GetCornerVertexTypesForHex(position);
        //        var edgeVertexTypes = slotGrid.GetEdgeVertexTypesForHex(position);

        //        var vertices24 = DataGenerator.GenerateV24Vertices(hexType, rotation);
        //        vertices24 = DataGenerator.PopulateVertexTypes(
        //            vertices24,
        //            hexType,
        //            cornerVertexTypes,
        //            edgeVertexTypes
        //        );

        //        //Now IN THEORY we just need to bundle these vertices into
        //        //triangles and create geometry out of them!
        //        //oh right, and I gotta add absolute heights & positions to the rels

        //        var triangleGroups = DataGenerator.triangleCoordinateGroups;
        //        foreach (AxialPosition[] triangleGroup in triangleGroups)
        //        {
        //            var v0 = vertices24[triangleGroup[0]];
        //            var v1 = vertices24[triangleGroup[1]];
        //            var v2 = vertices24[triangleGroup[2]];

        //            var v0AbsHeight = v0.height + slotGrid.TileHeights[position];
        //            var v1AbsHeight = v1.height + slotGrid.TileHeights[position];
        //            var v2AbsHeight = v2.height + slotGrid.TileHeights[position];

        //            AxialPosition v0AbsPos6 = triangleGroup[0] + position * 6;
        //            AxialPosition v1AbsPos6 = triangleGroup[1] + position * 6;
        //            AxialPosition v2AbsPos6 = triangleGroup[2] + position * 6;

        //            var v0AbsPosCartesian6 = GridUtilities.AxialPositionToVec2(v0AbsPos6);
        //            var v0AbsPosXYZ = new Vector3((float)v0AbsPosCartesian6.X/6, (float)v0AbsPosCartesian6.Y/6, (float)v0AbsHeight);

        //            var v1AbsPosCartesian6 = GridUtilities.AxialPositionToVec2(v1AbsPos6);
        //            var v1AbsPosXYZ = new Vector3((float)v1AbsPosCartesian6.X/6, (float)v1AbsPosCartesian6.Y/6, (float)v1AbsHeight);

        //            var v2AbsPosCartesian6 = GridUtilities.AxialPositionToVec2(v2AbsPos6);
        //            var v2AbsPosXYZ = new Vector3((float)v2AbsPosCartesian6.X/6, (float)v2AbsPosCartesian6.Y/6, (float)v2AbsHeight);
        //        }
        //    }
        //    Debug.Write(Vertices);
        //}

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
    }
}