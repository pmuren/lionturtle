using System;
using System.Text.RegularExpressions;

namespace lionturtle
{
    public static class GridUtilities
    {
        public static bool VertexPointsUp(AxialPosition vertexPosition)
        {
            if (vertexPosition.Q.Mod(3) == 0)
                throw new InvalidOperationException($"{vertexPosition.Q}.Mod(3) should never be 0.");

            else if (vertexPosition.Q.Mod(3) == 1) return true;
            else if (vertexPosition.Q.Mod(3) == 2) return false;

            return false;
        }

        public static AxialPosition[] GetVertexPositionsFromHexPosiiton(AxialPosition hexPosition)
        {
            AxialPosition[] directions = Constants.axial_directions;

            AxialPosition[] vPositions = new AxialPosition[6];
            for (int i = 0; i < vPositions.Length; i++)
            {
                AxialPosition n0Position = hexPosition + directions[(0 + i) % 6];
                AxialPosition n1Position = hexPosition + directions[(1 + i) % 6];
                vPositions[i] = hexPosition + n0Position + n1Position;
            }
            return vPositions;
        }

        public static AxialPosition GetVertexPositionForHexV(AxialPosition hexPosition, int vIndex)
        {
            AxialPosition[] directions = Constants.axial_directions;

            AxialPosition n0Position = hexPosition + directions[(vIndex + 0) % 6];
            AxialPosition n1Position = hexPosition + directions[(vIndex + 1) % 6];

            return hexPosition + n0Position + n1Position;
        }

        public static VertexGroup[] GetVertexGroups(AxialPosition primaryVertexPosition)
        {
            if (VertexPointsUp(primaryVertexPosition))
            {
                return new VertexGroup[]
                {
                    //First Neighbor Groups
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(1, -2),
                        primaryPushedVertices: new AxialPosition[]{
                            new AxialPosition(-2, 1),
                            new AxialPosition(1, 1),
                            new AxialPosition(-3, 3),
                            new AxialPosition(0, 3),
                            new AxialPosition(-2, 4),
                        },
                        secondaryPushedVertices: new AxialPosition[] {
                            new AxialPosition(3, -3),
                            new AxialPosition(0, -3),
                            new AxialPosition(4, -5),
                            new AxialPosition(1, -5),
                            new AxialPosition(3, -6),
                        }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(-2, 1),
                        primaryPushedVertices: new AxialPosition[]{
                            new AxialPosition(1, 1),
                            new AxialPosition(1, -2),
                            new AxialPosition(3, 0),
                            new AxialPosition(3, -3),
                            new AxialPosition(4, -2),
                        },
                        secondaryPushedVertices: new AxialPosition[]{
                            new AxialPosition(-3, 0),
                            new AxialPosition(-3, 3),
                            new AxialPosition(-5, 1),
                            new AxialPosition(-5, 4),
                            new AxialPosition(-6, 3),
                        }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(1, 1),
                        primaryPushedVertices: new AxialPosition[]
                        {
                            new AxialPosition(1, -2),
                            new AxialPosition(-2, 1),
                            new AxialPosition(0, -3),
                            new AxialPosition(-3, 0),
                            new AxialPosition(-2, -2),
                        },
                        secondaryPushedVertices: new AxialPosition[]
                        {
                            new AxialPosition(0, 3),
                            new AxialPosition(3, 0),
                            new AxialPosition(1, 4),
                            new AxialPosition(4, 1),
                            new AxialPosition(3, 3),
                        }
                    ),

                    //Second Neighbor Groups
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(3, 0),
                        pinchedVertices: new AxialPosition[]{new AxialPosition(1, 1) },
                        primaryPushedVertices: new AxialPosition[]{new AxialPosition(-2, 1)},
                        secondaryPushedVertices: new AxialPosition[]{new AxialPosition(4, 1)}
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(3, -3),
                        pinchedVertices: new AxialPosition[]{new AxialPosition(1, -2)},
                        primaryPushedVertices: new AxialPosition[]{new AxialPosition(-2, 1)},
                        secondaryPushedVertices: new AxialPosition[]{new AxialPosition(4, -5)}
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(0, -3),
                        pinchedVertices: new AxialPosition[]{new AxialPosition(1, -2)},
                        primaryPushedVertices: new AxialPosition[]{new AxialPosition(1, 1)},
                        secondaryPushedVertices: new AxialPosition[]{new AxialPosition(1, -5)}
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(-3, 0),
                        pinchedVertices: new AxialPosition[]{new AxialPosition(-2, 1)},
                        primaryPushedVertices: new AxialPosition[]{new AxialPosition(1, 1)},
                        secondaryPushedVertices: new AxialPosition[]{new AxialPosition(-5, 1)}
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(-3, 3),
                        pinchedVertices: new AxialPosition[]{new AxialPosition(-2, 1)},
                        primaryPushedVertices: new AxialPosition[]{new AxialPosition(1, -2)},
                        secondaryPushedVertices: new AxialPosition[]{new AxialPosition(-5, 4)}
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(0, 3),
                        pinchedVertices: new AxialPosition[]{new AxialPosition(1, 1)},
                        primaryPushedVertices: new AxialPosition[]{new AxialPosition(1, -2)},
                        secondaryPushedVertices: new AxialPosition[]{new AxialPosition(1, 4)}
                    ),

                    //Third Neighbor Groups
                    //ZigZag type 3rd neighbors
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(4, -5),
                        pinchedVertices: new AxialPosition[]{
                            new AxialPosition(1, -2),
                            new AxialPosition(3, -3)
                        }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(1, -5),
                        pinchedVertices: new AxialPosition[]{
                            new AxialPosition(1, -2),
                            new AxialPosition(0, -3)
                        }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(-5, 1),
                        pinchedVertices: new AxialPosition[]{
                            new AxialPosition(-2, 1),
                            new AxialPosition(-3, 0)
                        }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(-5, 4),
                        pinchedVertices: new AxialPosition[]{
                            new AxialPosition(-2, 1),
                            new AxialPosition(-3, 3)
                        }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(1, 4),
                        pinchedVertices: new AxialPosition[]{
                            new AxialPosition(1, 1),
                            new AxialPosition(0, 3)
                        }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(4, 1),
                        pinchedVertices: new AxialPosition[]{
                            new AxialPosition(1, 1),
                            new AxialPosition(3, 0)
                        }
                    ),

                    //Tip type 3rd neighbors
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(4, -2),
                        primaryPushedVertices: new AxialPosition[]
                        {
                            new AxialPosition(-2, 1)
                        },
                        secondaryPushedVertices: new AxialPosition[]
                        {
                            new AxialPosition(6, -3)
                        }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(-2, -2),
                        primaryPushedVertices: new AxialPosition[]
                        {
                            new AxialPosition(1, 1)
                        },
                        secondaryPushedVertices: new AxialPosition[]
                        {
                            new AxialPosition(-3, -3)
                        }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(-2, 4),
                        primaryPushedVertices: new AxialPosition[]
                        {
                            new AxialPosition(1, -2)
                        },
                        secondaryPushedVertices: new AxialPosition[]
                        {
                            new AxialPosition(-3, 6)
                        }
                    ),

                    //Fourth Neighbor Groups
                    //Balloon type 4th neighbors
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(6, -3),
                        pinchedVertices: new AxialPosition[]{
                            new AxialPosition(4, -2),
                        }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(-3, -3),
                        pinchedVertices: new AxialPosition[]{
                            new AxialPosition(-2, -2),
                        }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(-3, 6),
                        pinchedVertices: new AxialPosition[]{
                            new AxialPosition(-2, 4),
                        }
                    ),

                    //Stem type 4th neighbors
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(3, -6),
                        pinchedVertices: new AxialPosition[]{
                            new AxialPosition(1, -2),
                        }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(-6, 3),
                        pinchedVertices: new AxialPosition[]{
                            new AxialPosition(-2, 1),
                        }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(3, 3),
                        pinchedVertices: new AxialPosition[]{
                            new AxialPosition(1, 1),
                        }
                    ),
                };
            }
            else //vertex points down
            {
                return new VertexGroup[]
                {
                    //First Neighbor Groups
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(2, -1),
                        primaryPushedVertices: new AxialPosition[]
                        {
                            new AxialPosition(-1, -1),
                            new AxialPosition(-1, 2),
                            new AxialPosition(-3, 0),
                            new AxialPosition(-3, 3),
                            new AxialPosition(-4, 2),
                        },
                        secondaryPushedVertices: new AxialPosition[]
                        {
                            new AxialPosition(3, 0),
                            new AxialPosition(3, -3),
                            new AxialPosition(5, -1),
                            new AxialPosition(5, -4),
                            new AxialPosition(6, -3),
                        }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(-1, -1),
                        primaryPushedVertices: new AxialPosition[]
                        {
                            new AxialPosition(-1, 2),
                            new AxialPosition(2, -1),
                            new AxialPosition(0, 3),
                            new AxialPosition(3, 0),
                            new AxialPosition(2, 2),
                        },
                        secondaryPushedVertices: new AxialPosition[]
                        {
                            new AxialPosition(0, -3),
                            new AxialPosition(-3, 0),
                            new AxialPosition(-1, -4),
                            new AxialPosition(-4, -1),
                            new AxialPosition(-3, -3),
                        }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(-1, 2),
                        primaryPushedVertices: new AxialPosition[]
                        {
                            new AxialPosition(2, -1),
                            new AxialPosition(-1, -1),
                            new AxialPosition(3, -3),
                            new AxialPosition(0, -3),
                            new AxialPosition(2, -4),
                        },
                        secondaryPushedVertices: new AxialPosition[]
                        {
                            new AxialPosition(-3, 3),
                            new AxialPosition(0, 3),
                            new AxialPosition(-4, 5),
                            new AxialPosition(-1, 5),
                            new AxialPosition(-3, 6),
                        }
                    ),

                    //Second Neighbor Groups
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(3, 0),
                        pinchedVertices: new AxialPosition[]{new AxialPosition(2, -1) },
                        primaryPushedVertices: new AxialPosition[]{new AxialPosition(-1, -1)},
                        secondaryPushedVertices: new AxialPosition[]{new AxialPosition(5, -1)}
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(3, -3),
                        pinchedVertices: new AxialPosition[]{new AxialPosition(2, -1)},
                        primaryPushedVertices: new AxialPosition[]{new AxialPosition(-1, 2)},
                        secondaryPushedVertices: new AxialPosition[]{new AxialPosition(5, -4)}
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(0, -3),
                        pinchedVertices: new AxialPosition[]{new AxialPosition(-1, -1)},
                        primaryPushedVertices: new AxialPosition[]{new AxialPosition(-1, 2)},
                        secondaryPushedVertices: new AxialPosition[]{new AxialPosition(-1, -4)}
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(-3, 0),
                        pinchedVertices: new AxialPosition[]{new AxialPosition(-1, -1)},
                        primaryPushedVertices: new AxialPosition[]{new AxialPosition(2, -1)},
                        secondaryPushedVertices: new AxialPosition[]{new AxialPosition(-4, -1)}
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(-3, 3),
                        pinchedVertices: new AxialPosition[]{new AxialPosition(-1, 2)},
                        primaryPushedVertices: new AxialPosition[]{new AxialPosition(2, -1)},
                        secondaryPushedVertices: new AxialPosition[]{new AxialPosition(-4, 5)}
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(0, 3),
                        pinchedVertices: new AxialPosition[]{new AxialPosition(-1, 2)},
                        primaryPushedVertices: new AxialPosition[]{new AxialPosition(-1, -1)},
                        secondaryPushedVertices: new AxialPosition[]{new AxialPosition(-1, 5)}
                    ),

                    //Third Neighbor Groups
                    //ZigZag type 3rd neighbors
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(5, -1),
                        pinchedVertices: new AxialPosition[]{
                            new AxialPosition(2, -1),
                            new AxialPosition(3, 0)
                        }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(5, -4),
                        pinchedVertices: new AxialPosition[]{
                            new AxialPosition(2, -1),
                            new AxialPosition(3, -3)
                        }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(-1, -4),
                        pinchedVertices: new AxialPosition[]{
                            new AxialPosition(-1, -1),
                            new AxialPosition(0, -3)
                        }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(-4, -1),
                        pinchedVertices: new AxialPosition[]{
                            new AxialPosition(-1, -1),
                            new AxialPosition(-3, 0)
                        }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(-4, 5),
                        pinchedVertices: new AxialPosition[]{
                            new AxialPosition(-1, 2),
                            new AxialPosition(-3, 3)
                        }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(-1, 5),
                        pinchedVertices: new AxialPosition[]{
                            new AxialPosition(-1, 2),
                            new AxialPosition(0, 3)
                        }
                    ),

                    //Tip type 3rd neighbors
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(2, -4),
                        primaryPushedVertices: new AxialPosition[]
                        {
                            new AxialPosition(-1, 2)
                        },
                        secondaryPushedVertices: new AxialPosition[]
                        {
                            new AxialPosition(3, -6)
                        }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(-4, 2),
                        primaryPushedVertices: new AxialPosition[]
                        {
                            new AxialPosition(2, -1)
                        },
                        secondaryPushedVertices: new AxialPosition[]
                        {
                            new AxialPosition(-6, 3)
                        }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(2, 2),
                        primaryPushedVertices: new AxialPosition[]
                        {
                            new AxialPosition(-1, -1)
                        },
                        secondaryPushedVertices: new AxialPosition[]
                        {
                            new AxialPosition(3, 3)
                        }
                    ),

                    //Fourth Neighbor Groups
                    //Balloon type 4th neighbors
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(3, -6),
                        pinchedVertices: new AxialPosition[]{
                            new AxialPosition(2, -4),
                        }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(-6, 3),
                        pinchedVertices: new AxialPosition[]{
                            new AxialPosition(-4, 2),
                        }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(3, 3),
                        pinchedVertices: new AxialPosition[]{
                            new AxialPosition(2, 2),
                        }
                    ),

                    //Stem type 4th neighbors
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(6, -3),
                        pinchedVertices: new AxialPosition[]{
                            new AxialPosition(2, -1),
                        }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(-3, -3),
                        pinchedVertices: new AxialPosition[]{
                            new AxialPosition(-1, -1),
                        }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(-3, 6),
                        pinchedVertices: new AxialPosition[]{
                            new AxialPosition(-1, 2),
                        }
                    )
                };
            }
        }
    }

    public readonly struct VertexGroup
    {
        public VertexGroup(
            AxialPosition primaryVertex,
            AxialPosition secondaryVertex,
            AxialPosition[]? primaryPushedVertices = null,
            AxialPosition[]? secondaryPushedVertices = null,
            AxialPosition[]? pinchedVertices = null
            )
        {
            PrimaryVertex = primaryVertex;
            SecondaryVertex = secondaryVertex;
            if (pinchedVertices == null) PinchedVertices = Array.Empty<AxialPosition>();
            else PinchedVertices = pinchedVertices;

            if (primaryPushedVertices == null) PrimaryPushedVertices = Array.Empty<AxialPosition>();
            else PrimaryPushedVertices = primaryPushedVertices;

            if (secondaryPushedVertices == null) SecondaryPushedVertices = Array.Empty<AxialPosition>();
            else SecondaryPushedVertices = secondaryPushedVertices;
        }

        public AxialPosition PrimaryVertex { get; }
        public AxialPosition SecondaryVertex { get; }
        public AxialPosition[] PinchedVertices { get; }
        public AxialPosition[] PrimaryPushedVertices { get; }
        public AxialPosition[] SecondaryPushedVertices { get; }
    }
}

