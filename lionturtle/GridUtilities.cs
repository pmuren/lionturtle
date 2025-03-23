using System;
using System.Diagnostics;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;

namespace lionturtle
{
    public static class GridUtilities
    {
        public static List<AxialPosition> GetSpiralVertexPositions(int numRings)
        {
            if(numRings < 0) throw new Exception("numRings must be non-negative");
            if(numRings == 0) return new List<AxialPosition>();

            int index = 0;
            List<AxialPosition> positions = new();

            AxialPosition turtle = new AxialPosition(0, 0); //starting on hex center

            for (int i = 1; i < numRings; i++)
            {
                turtle += Constants.dualDirections[0];
                for (int j = 0; j < 6; j++)
                {
                    for (int k = 0; k < i; k++)
                    {
                        if (turtle.Q.Mod(3) != 0){ //ignore hex centers
                            positions.Add(turtle);
                            index++;
                        }

                        turtle += Constants.dualDirections[(j+2)%6];
                    }
                }
            }

            return positions;
        }

        public static AxialPosition[] GetSpiralHexPositions(int numRings)
        {
            if(numRings < 0) throw new Exception("numRings must be non-negative");
            if(numRings == 0) return new AxialPosition[0];

            int index = 0;
            AxialPosition[] positions = new AxialPosition[6*GetTriangleNumber(numRings-1)+1];

            AxialPosition turtle = new AxialPosition(0, 0);
            positions[index] = turtle;
            index++;

            for (int i = 1; i < numRings; i++)
            {
                turtle += Constants.axialDirections[0];
                for (int j = 0; j < 6; j++)
                {
                    for (int k = 0; k < i; k++)
                    {
                        positions[index] = turtle;
                        index++;
                        turtle += Constants.axialDirections[(j+2)%6];
                    }
                }
            }

            return positions;
        }

        public static int GetTriangleNumber(int n){
            return n*(n+1)/2;
        }

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
            AxialPosition[] directions = Constants.axialDirections;

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
            AxialPosition[] directions = Constants.axialDirections;

            AxialPosition n0Position = hexPosition + directions[(vIndex + 0) % 6];
            AxialPosition n1Position = hexPosition + directions[(vIndex + 1) % 6];

            return hexPosition + n0Position + n1Position;
        }

        public static Vector2 AxialPositionToVec2(AxialPosition axial)
        {
            double x = (Math.Sqrt(3) * axial.Q + Math.Sqrt(3) / 2 * axial.R);
            double y = (3.0 / 2 * axial.R * -1);
            return new Vector2((float)x, (float)y);
        }

        public static Vector2 AxialRound(Vector2 fractionalAxial)
        {
            float fractionalQ = fractionalAxial.X;
            float fractionalR = fractionalAxial.Y;
            float fractionalS = 0.0f - fractionalQ - fractionalR;

            float whole_q = (float)Math.Round(fractionalQ);
            float whole_r = (float)Math.Round(fractionalR);
            float whole_s = (float)Math.Round(fractionalS);

            float q_diff = Math.Abs(fractionalQ - whole_q);
            float r_diff = Math.Abs(fractionalR - whole_r);
            float s_diff = Math.Abs(fractionalS - whole_s);

            float q = whole_q;
            float r = whole_r;
            if (q_diff > r_diff && q_diff > s_diff)
                q = -whole_r - whole_s;
            else if (r_diff > q_diff && r_diff > s_diff)
                r = -whole_q - whole_s;

            return new Vector2(q, r);
        }

        public static Vector2 AxialToCartesian(AxialPosition axial, float grid_scale)
        {
            float x = (float)(Math.Sqrt(3.0) * axial.Q + Math.Sqrt(3.0) / 2.0 * axial.R) / grid_scale;
            float y = (float)(3.0 / 2.0 * axial.R) / grid_scale;

            return new Vector2(x, y);
        }

        public static Vector2 CartesianToFractionalAxial(Vector2 cartesian, float grid_scale)
        {
            float q = (float)((cartesian.X / Math.Sqrt(3.0)) - (cartesian.Y / 3.0)) * grid_scale;
            float r = (float)(2.0 * cartesian.Y / 3.0) * grid_scale;

            return new Vector2(q, r);
        }

        public static Vector2 CartesianToWholeAxial(Vector2 cartesian, float grid_scale)
        {
            return AxialRound(CartesianToFractionalAxial(cartesian, grid_scale));
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

    public readonly struct CreaseVertexGroup
    {
        public CreaseVertexGroup(
            AxialPosition a,
            AxialPosition mid,
            AxialPosition handle,
            AxialPosition b
            )
        {
            A = a;
            Mid = mid;
            Handle = handle;
            B = b;
        }

        public AxialPosition A { get; }
        public AxialPosition Mid { get; }
        public AxialPosition Handle { get; }
        public AxialPosition B { get; }
    }
}

