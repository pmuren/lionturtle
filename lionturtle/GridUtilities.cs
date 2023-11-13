using System;
using System.Text.RegularExpressions;

namespace lionturtle
{
    public static class GridUtilities
    {
        public static bool VertexPointsUp(AxialPosition vertexPosition)
        {
            if (vertexPosition.Q.Mod(3) == 0)
                throw new InvalidOperationException($"{vertexPosition.Q}.Mod(3) should not be 0.");

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

        public static VirtualVertex[] GetConstraintsCausedByVertexPair(Vertex vertexA, Vertex vertexB)
        {
            if (vertexA.height == vertexB.height) return Array.Empty<VirtualVertex>();

            VirtualVertex[] constraints = new VirtualVertex[12];

            AxialPosition directionBA = vertexA.position - vertexB.position;
            AxialPosition hexAPosition3 = vertexA.position + directionBA;
            AxialPosition hexAPosition = new(hexAPosition3.Q / 3, hexAPosition3.R / 3);
            AxialPosition[] hexAVertexPositions = GetVertexPositionsFromHexPosiiton(hexAPosition);

            AxialPosition directionAB = vertexB.position - vertexA.position;
            AxialPosition hexBPosition3 = vertexB.position + directionAB;
            AxialPosition hexBPosition = new(hexBPosition3.Q / 3, hexBPosition3.R / 3);
            AxialPosition[] hexBVertexPositions = GetVertexPositionsFromHexPosiiton(hexBPosition);

            if (vertexA.height < vertexB.height)
            {
                for (int i = 0; i < hexAVertexPositions.Length; i++)
                {
                    AxialPosition vPosition = hexAVertexPositions[i];
                    int maxHeight = vertexA.height;
                    constraints[i] = new VirtualVertex(vPosition, null, maxHeight);
                }

                for (int i = 0; i < hexBVertexPositions.Length; i++)
                {
                    AxialPosition vPosition = hexBVertexPositions[i];
                    int minHeight = vertexB.height;
                    constraints[i + 6] = new VirtualVertex(vPosition, minHeight, null);
                }
            }
            else if (vertexA.height > vertexB.height)
            {
                for (int i = 0; i < hexAVertexPositions.Length; i++)
                {
                    AxialPosition vPosition = hexAVertexPositions[i];
                    int minHeight = vertexA.height;
                    constraints[i] = new VirtualVertex(vPosition, minHeight, null);
                }

                for (int i = 0; i < hexBVertexPositions.Length; i++)
                {
                    AxialPosition vPosition = hexBVertexPositions[i];
                    int maxHeight = vertexB.height;
                    constraints[i + 6] = new VirtualVertex(vPosition, null, maxHeight);
                }
            }

            return constraints;
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
                        primaryAffected: new AxialPosition[]{
                            new AxialPosition(-2, 1),
                            new AxialPosition(1, 1),
                            new AxialPosition(-3, 3),
                            new AxialPosition(0, 3),
                            new AxialPosition(-5, 4),
                            new AxialPosition(-2, 4),
                            new AxialPosition(1, 4)
                        },
                        secondaryAffected: new AxialPosition[] {
                            new AxialPosition(3, -3),
                            new AxialPosition(0, -3),
                            new AxialPosition(4, -5),
                            new AxialPosition(1, -5),
                            new AxialPosition(6, -6),
                            new AxialPosition(3, -6),
                            new AxialPosition(0, -6)
                        }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(-2, 1),
                        primaryAffected: new AxialPosition[]{
                            new AxialPosition(1, 1),
                            new AxialPosition(1, -2),
                            new AxialPosition(3, 0),
                            new AxialPosition(3, -3),
                            new AxialPosition(4, 1),
                            new AxialPosition(4, -2),
                            new AxialPosition(4, -5),
                        },
                        secondaryAffected: new AxialPosition[]{
                            new AxialPosition(-3, 0),
                            new AxialPosition(-3, 3),
                            new AxialPosition(-5, 1),
                            new AxialPosition(-5, 4),
                            new AxialPosition(-6, 0),
                            new AxialPosition(-6, 3),
                            new AxialPosition(-6, 6)
                        }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(1, 1),
                        primaryAffected: new AxialPosition[]
                        {
                            new AxialPosition(1, -2),
                            new AxialPosition(-2, 1),
                            new AxialPosition(0, -3),
                            new AxialPosition(-3, 0),
                            new AxialPosition(1, -5),
                            new AxialPosition(-2, -2),
                            new AxialPosition(-5, 1)
                        },
                        secondaryAffected: new AxialPosition[]
                        {
                            new AxialPosition(0, 3),
                            new AxialPosition(3, 0),
                            new AxialPosition(1, 4),
                            new AxialPosition(4, 1),
                            new AxialPosition(0, 6),
                            new AxialPosition(3, 3),
                            new AxialPosition(6, 0)
                        }
                    ),

                    //Second Neighbor Groups
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(3, 0),
                        squeezedVertex: new AxialPosition(1, 1),
                        primaryAffected: new AxialPosition[]{new AxialPosition(-2, 1), new AxialPosition(-3, 0) },
                        secondaryAffected: new AxialPosition[]{new AxialPosition(4, 1), new AxialPosition(6, 0) }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(3, -3),
                        squeezedVertex: new AxialPosition(1, -2),
                        primaryAffected: new AxialPosition[]{new AxialPosition(-2, 1), new AxialPosition(-3, 3) },
                        secondaryAffected: new AxialPosition[]{new AxialPosition(4, -5), new AxialPosition(6, -6) }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(0, -3),
                        squeezedVertex: new AxialPosition(1, -2),
                        primaryAffected: new AxialPosition[]{new AxialPosition(1, 1), new AxialPosition(0, 3) },
                        secondaryAffected: new AxialPosition[]{new AxialPosition(1, -5), new AxialPosition(0, -6) }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(-3, 0),
                        squeezedVertex: new AxialPosition(-2, 1),
                        primaryAffected: new AxialPosition[]{new AxialPosition(1, 1), new AxialPosition(3, 0) },
                        secondaryAffected: new AxialPosition[]{new AxialPosition(-5, 1), new AxialPosition(-6, 0) }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(-3, 3),
                        squeezedVertex: new AxialPosition(-2, 1),
                        primaryAffected: new AxialPosition[]{new AxialPosition(1, -2), new AxialPosition(3, -3) },
                        secondaryAffected: new AxialPosition[]{new AxialPosition(-5, 4), new AxialPosition(-6, 6) }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(0, 3),
                        squeezedVertex: new AxialPosition(1, 1),
                        primaryAffected: new AxialPosition[]{new AxialPosition(1, -2), new AxialPosition(0, -3) },
                        secondaryAffected: new AxialPosition[]{new AxialPosition(1, 4), new AxialPosition(0, 6) }
                    )
                };
            }
            else
            {
                return new VertexGroup[]
                {
                    //First Neighbor Groups
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(2, -1),
                        primaryAffected: new AxialPosition[]
                        {
                            new AxialPosition(-1, -1),
                            new AxialPosition(-1, 2),
                            new AxialPosition(-3, 0),
                            new AxialPosition(-3, 3),
                            new AxialPosition(-4, -1),
                            new AxialPosition(-4, 2),
                            new AxialPosition(-4, 5)
                        },
                        secondaryAffected: new AxialPosition[]
                        {
                            new AxialPosition(3, 0),
                            new AxialPosition(3, -3),
                            new AxialPosition(5, -1),
                            new AxialPosition(5, -4),
                            new AxialPosition(6, 0),
                            new AxialPosition(6, -3),
                            new AxialPosition(6, -6)
                        }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(-1, -1),
                        primaryAffected: new AxialPosition[]
                        {
                            new AxialPosition(-1, 2),
                            new AxialPosition(2, -1),
                            new AxialPosition(0, 3),
                            new AxialPosition(3, 0),
                            new AxialPosition(-1, 5),
                            new AxialPosition(2, 2),
                            new AxialPosition(5, -1)
                        },
                        secondaryAffected: new AxialPosition[]
                        {
                            new AxialPosition(0, -3),
                            new AxialPosition(-3, 0),
                            new AxialPosition(-1, -4),
                            new AxialPosition(-4, -1),
                            new AxialPosition(0, -6),
                            new AxialPosition(-3, -3),
                            new AxialPosition(-6, 0)
                        }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(-1, 2),
                        primaryAffected: new AxialPosition[]
                        {
                            new AxialPosition(2, -1),
                            new AxialPosition(-1, -1),
                            new AxialPosition(3, -3),
                            new AxialPosition(0, -3),
                            new AxialPosition(5, -4),
                            new AxialPosition(2, -4),
                            new AxialPosition(-1, -4)
                        },
                        secondaryAffected: new AxialPosition[]
                        {
                            new AxialPosition(-3, 3),
                            new AxialPosition(0, 3),
                            new AxialPosition(-4, 5),
                            new AxialPosition(-1, 5),
                            new AxialPosition(-6, 6),
                            new AxialPosition(-3, 6),
                            new AxialPosition(0, 6)
                        }
                    ),

                    //Second Neighbor Groups
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(3, 0),
                        squeezedVertex: new AxialPosition(2, -1),
                        primaryAffected: new AxialPosition[]{new AxialPosition(-1, -1), new AxialPosition(-3, 0) },
                        secondaryAffected: new AxialPosition[]{new AxialPosition(5, -1), new AxialPosition(6, 0) }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(3, -3),
                        squeezedVertex: new AxialPosition(2, -1),
                        primaryAffected: new AxialPosition[]{new AxialPosition(-1, 2), new AxialPosition(-3, 3) },
                        secondaryAffected: new AxialPosition[]{new AxialPosition(5, -4), new AxialPosition(6, -6) }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(0, -3),
                        squeezedVertex: new AxialPosition(-1, -1),
                        primaryAffected: new AxialPosition[]{new AxialPosition(-1, 2), new AxialPosition(0, 3) },
                        secondaryAffected: new AxialPosition[]{new AxialPosition(-1, -4), new AxialPosition(0, -6) }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(-3, 0),
                        squeezedVertex: new AxialPosition(-1, -1),
                        primaryAffected: new AxialPosition[]{new AxialPosition(2, -1), new AxialPosition(3, 0) },
                        secondaryAffected: new AxialPosition[]{new AxialPosition(-4, -1), new AxialPosition(-6, 0) }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(-3, 3),
                        squeezedVertex: new AxialPosition(-1, 2),
                        primaryAffected: new AxialPosition[]{new AxialPosition(2, -1), new AxialPosition(3, -3) },
                        secondaryAffected: new AxialPosition[]{new AxialPosition(-4, 5), new AxialPosition(-6, 6) }
                    ),
                    new VertexGroup(
                        primaryVertex: new AxialPosition(0, 0),
                        secondaryVertex: new AxialPosition(0, 3),
                        squeezedVertex: new AxialPosition(-1, 2),
                        primaryAffected: new AxialPosition[]{new AxialPosition(-1, -1), new AxialPosition(0, -3) },
                        secondaryAffected: new AxialPosition[]{new AxialPosition(-1, 5), new AxialPosition(0, 6) }
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
            AxialPosition[] primaryAffected,
            AxialPosition[] secondaryAffected,
            AxialPosition? squeezedVertex = null
            )
        {
            PrimaryVertex = primaryVertex;
            SecondaryVertex = secondaryVertex;
            SqueezedVertex = squeezedVertex;
            PrimaryAffected = primaryAffected;
            SecondaryAffected = secondaryAffected;
        }

        public AxialPosition PrimaryVertex { get; }
        public AxialPosition SecondaryVertex { get; }
        public AxialPosition? SqueezedVertex { get; }
        public AxialPosition[] PrimaryAffected { get; }
        public AxialPosition[] SecondaryAffected { get; }
    }
}

