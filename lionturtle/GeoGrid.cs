using System;
using System.Drawing;
using System.Numerics;

namespace lionturtle;

public class GeoGrid
{
    TileGrid tGrid;
    private float heightScale;

    public HashSet<AxialPosition> Geos; //Hex Positions where geometry has been created. Maybe user should track this?
    List<GeoVert> verts;
    Dictionary<AxialPosition, GeoVert> axialToVert;
    List<int> indices;
    Dictionary<AxialPosition, int> axialToIndex;

    public GeoGrid()
	{
		tGrid = new TileGrid();
        heightScale = 0.44f;

        Geos = new HashSet<AxialPosition>();
        verts = new List<GeoVert>();
        axialToVert = new Dictionary<AxialPosition, GeoVert>();
        indices = new List<int>();
        axialToIndex = new Dictionary<AxialPosition, int>();

    }

    //Assumes that the position on the underlying tGrid has at least 1 neighbor Tile
    //TODO ensure adjacent tile by pathfinding from existing tiles to this one?
    public (List<GeoVert>, List<int>) AddGeo(AxialPosition position)
    {
        List<GeoVert> newVerts = new List<GeoVert>();
        List<int> newIndices = new List<int>();

        FillSurroundingTiles(position);

        var relativeHeights = tGrid.Tiles[position].RelativeVertexHeights;

        //determine hexType and rotation from relative vertex heights
        int hexType = 0;
        int rotation = 0;
        for (int direction = 0; direction < 6; direction++)
        {
            if (relativeHeights[direction] == 1)
                hexType++;
            if (relativeHeights[(direction+5)%6] == 0 && relativeHeights[direction] == 1)
                rotation = 6 - direction; //TODO remember why this isn't just equal to direction
        }


        var cornerVertexTypes = tGrid.sGrid.GetCornerVertexTypesForHex(position); //TODO consider these definitions into TileGrid, using base/rel heights
        var edgeVertexTypes = tGrid.sGrid.GetEdgeVertexTypesForHex(position);
        var vertexTypes = DataGenerator.GetVertexTypes24(hexType, cornerVertexTypes, edgeVertexTypes);

        Dictionary<AxialPosition, Vertex> axialToVertex = DataGenerator.GenerateV24Vertices(hexType, rotation);
        foreach (AxialPosition v24Position in axialToVertex.Keys.ToArray()) //And this could be a method in Geo to populate types
        {
            Vector3 cartesian;
            Vector3 normal;
            Vector2 uv;
            VertexType type;

            Vertex v24 = axialToVertex[v24Position];
            float absHeight = (float)v24.Height + tGrid.Tiles[position].BaseHeight;
            AxialPosition absolutePosition6 = v24Position + position * 6;
            var absPosCartesian6 = GridUtilities.AxialPositionToVec2(absolutePosition6);
            cartesian = new Vector3(absPosCartesian6.X / 6, (float)absHeight * heightScale, absPosCartesian6.Y / 6);

            //TODO calculate real normal
            normal = new Vector3(0, 1, 0);

            //TODO use real UVs!
            uv = new Vector2(0, 0);

            type = vertexTypes[v24Position];

            if(!axialToVert.ContainsKey(absolutePosition6)) //TODO try letting this happen so separate hexes get separate vertices
                axialToVert.Add(absolutePosition6, new GeoVert(cartesian, normal, uv, type));
        }

        //^ for each vertex
        /////////////////////
        //v for each triangle group

        var triangleGroups = DataGenerator.triangleCoordinateGroups;
        foreach (AxialPosition[] triangleGroup in triangleGroups)
        {
            foreach (AxialPosition v24Position in triangleGroup)
            {
                AxialPosition absolutePosition = v24Position + position * 6;
                if (!axialToIndex.ContainsKey(absolutePosition))
                {
                    newVerts.Add(axialToVert[absolutePosition]);
                    axialToIndex.Add(absolutePosition, axialToIndex.Count);
                }

                newIndices.Add(axialToIndex[absolutePosition]);
            }
        }

        Geos.Add(position);

        verts.AddRange(newVerts);
        indices.AddRange(newIndices);
        return (newVerts, newIndices);
    }

    public void FillSurroundingTiles(AxialPosition position)
    {
        //if(!tGrid.Tiles.ContainsKey(position)) //shouldn't be necessary, since I'm already a neighbor of an existing geo
        //    tGrid.AddTile(position);

        for (int direction = 0; direction < 6; direction++)
        {
            AxialPosition neighborPosition = position + Constants.axialDirections[direction];
            if (!tGrid.Tiles.ContainsKey(neighborPosition))
            {
                tGrid.AddTile(neighborPosition);
            }
        }

    }
}

public record GeoVert(
    Vector3 cartesian = default,
    Vector3 normal = default,
    Vector2 uv = default,
    VertexType type = VertexType.Unknown
);
