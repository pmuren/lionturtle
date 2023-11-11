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
                    constraints[i+6] = new VirtualVertex(vPosition, minHeight, null);
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
                    constraints[i+6] = new VirtualVertex(vPosition, null, maxHeight);
                }
            }

            return constraints;
        }

        public static VirtualVertex GetBlurryVertex(
            AxialPosition vertexPosition,
            Dictionary<AxialPosition, VirtualVertex> localConstraints
            )
        {
            VirtualVertex blurryVertex = new(vertexPosition, null, null);

            //Apply existing constraints (not sure this is actually necessary)
            if (localConstraints.ContainsKey(vertexPosition))
            {
                VirtualVertex existingConstraints = localConstraints[vertexPosition];
                blurryVertex.Constrain(existingConstraints.min, existingConstraints.max);
            }

            //For each vertex neighbor direction, apply rules for avoiding conflicting constraints
            //All of the positions in CPG are relative to vertexPosition
            ConstraintPositionsGroup[] constraintPositionsGroups = GetConstraintPositionGroups(vertexPosition);

            foreach (ConstraintPositionsGroup CPG in constraintPositionsGroups)
            {
                //Get the absolute position of the neighboring vertex
                AxialPosition secondaryVertexPosition = CPG.VSecondary + vertexPosition;

                if (localConstraints.ContainsKey(secondaryVertexPosition))
                {
                    VirtualVertex nv = localConstraints[secondaryVertexPosition];

                    for (int i = 0; i < CPG.HPrimary.Length; i++)
                    {
                        AxialPosition primaryHexVertexPosition = CPG.HPrimary[i] + vertexPosition;
                        if (localConstraints.ContainsKey(primaryHexVertexPosition))
                        {
                            VirtualVertex phv = localConstraints[primaryHexVertexPosition];
                            if (nv.min >= phv.max)
                                blurryVertex.Constrain(null, nv.max);
                            if (nv.max <= phv.min)
                                blurryVertex.Constrain(nv.min, null);

                            if (nv.max <= phv.max)
                                blurryVertex.Constrain(null, phv.max);
                            if (nv.min >= phv.min)
                                blurryVertex.Constrain(phv.min, null);

                            if (nv.max >= phv.max)
                                blurryVertex.Constrain(null, nv.max);
                            if (nv.min <= phv.min)
                                blurryVertex.Constrain(nv.min, null);
                        }
                    }

                    for (int i = 0; i < CPG.HSecondary.Length; i++)
                    {
                        AxialPosition secondaryHexVertexPosition = CPG.HSecondary[i] + vertexPosition;
                        if (localConstraints.ContainsKey(secondaryHexVertexPosition))
                        {
                            VirtualVertex shv = localConstraints[secondaryHexVertexPosition];
                            if (nv.min > shv.max)
                                blurryVertex.Constrain(nv.min, null);
                            if (nv.max < shv.min)
                                blurryVertex.Constrain(null, nv.max);
                        }
                    }
                }
            }

            ////Hacking in some more constraints to handle an edge case
            //(AxialPosition, AxialPosition, AxialPosition)[] upDistantInfluencers = new (AxialPosition, AxialPosition, AxialPosition)[] {
            //    (new AxialPosition(7, -5), new AxialPosition(3, -3), new AxialPosition(4, -2)),
            //    (new AxialPosition(-2, -5), new AxialPosition(0, -3), new AxialPosition(-2, -2)),
            //    (new AxialPosition(-5, -2), new AxialPosition(-3, 0), new AxialPosition(-2, -2)),
            //    (new AxialPosition(-5, -7), new AxialPosition(-3, 3), new AxialPosition(-2, 4)),
            //    (new AxialPosition(-2, 7), new AxialPosition(0, 3), new AxialPosition(-2, 4)),
            //    (new AxialPosition(7, -2), new AxialPosition(3, 0), new AxialPosition(4, -2))
            //};

            //(AxialPosition, AxialPosition, AxialPosition)[] downDistantInfluencers = new (AxialPosition, AxialPosition, AxialPosition)[] {
            //    (new AxialPosition(5, -7), new AxialPosition(3, -3), new AxialPosition(2, -4)),
            //    (new AxialPosition(2, -7), new AxialPosition(0, -3), new AxialPosition(2, -4)),
            //    (new AxialPosition(-7, 2), new AxialPosition(-3, 0), new AxialPosition(-4, 2)),
            //    (new AxialPosition(-7, 5), new AxialPosition(-3, 3), new AxialPosition(-4, 2)),
            //    (new AxialPosition(2, 5), new AxialPosition(0, 3), new AxialPosition(2, 2)),
            //    (new AxialPosition(5, 2), new AxialPosition(3, 0), new AxialPosition(2, 2))
            //};

            //(AxialPosition, AxialPosition, AxialPosition)[] distantInfluencers;
            //if (VertexPointsUp(vertexPosition))
            //    distantInfluencers = upDistantInfluencers;
            //else
            //    distantInfluencers = downDistantInfluencers;

            //for(int i = 0; i < distantInfluencers.Length; i++)
            //{
            //    VirtualVertex? a = localConstraints.ContainsKey(distantInfluencers[i].Item1 + vertexPosition)
            //        ? localConstraints[distantInfluencers[i].Item1 + vertexPosition]
            //        : null;

            //    VirtualVertex? b = localConstraints.ContainsKey(distantInfluencers[i].Item2 + vertexPosition)
            //        ? localConstraints[distantInfluencers[i].Item2 + vertexPosition]
            //        : null;

            //    VirtualVertex? c = localConstraints.ContainsKey(distantInfluencers[i].Item3 + vertexPosition)
            //        ? localConstraints[distantInfluencers[i].Item3 + vertexPosition]
            //        : null;

            //    if(a != null)
            //    {
            //        //Attempt 2 . . . better? . . . but still bad
            //        //if (b != null && c != null)
            //        //{
            //        //    if (a.max != null && b.min != null && c.min != null && a.max < b.min && b.min == c.min)
            //        //        blurryVertex.Constrain(b.min, null);

            //        //    if (a.min != null && b.max != null && c.max != null && a.min > b.max && b.max == c.max)
            //        //        blurryVertex.Constrain(null, b.max);
            //        //}

            //        //Attempt 1
            //        //if( b != null)
            //        //{
            //        //    if (a.max != null && b.min != null && a.max < b.min)
            //        //        blurryVertex.Constrain(b.min, null);

            //        //    if (a.min != null && b.max != null && a.min > b.max)
            //        //        blurryVertex.Constrain(null, b.max);
            //        //}

            //        //if (c != null)
            //        //{
            //        //    if (a.max != null && c.min != null && a.max < c.min)
            //        //        blurryVertex.Constrain(c.min, null);

            //        //    if (a.min != null && c.max != null && a.min > c.max)
            //        //        blurryVertex.Constrain(null, c.max);
            //        //}
            //    }
            //}

            return blurryVertex;
        }

        public static (AxialPosition, AxialPosition)[] GetLocalVertexPositionPairs(AxialPosition vertexPosition)
		{
            if (VertexPointsUp(vertexPosition))
            {
                return new (AxialPosition, AxialPosition)[]
                {
                    (new AxialPosition(0, 0), new AxialPosition(1, -2)),
                    (new AxialPosition(0, 0), new AxialPosition(-2, 1)),
                    (new AxialPosition(0, 0), new AxialPosition(1, 1)),
                    (new AxialPosition(1, -2), new AxialPosition(3, -3)),
                    (new AxialPosition(1, -2), new AxialPosition(0, -3)),
                    (new AxialPosition(-2, 1), new AxialPosition(-3, 0)),
                    (new AxialPosition(-2, 1), new AxialPosition(-3, 3)),
                    (new AxialPosition(1, 1), new AxialPosition(0, 3)),
                    (new AxialPosition(1, 1), new AxialPosition(3, 0)),
                    (new AxialPosition(3, 0), new AxialPosition(4, -2)),
                    (new AxialPosition(4, -2), new AxialPosition(3, -3)),
                    (new AxialPosition(3, -3), new AxialPosition(4, -5)),
                    (new AxialPosition(4, -5), new AxialPosition(3, -6)),
                    (new AxialPosition(3, -6), new AxialPosition(1, -5)),
                    (new AxialPosition(1, -5), new AxialPosition(0, -3)),
                    (new AxialPosition(0, -3), new AxialPosition(-2, -2)),
                    (new AxialPosition(-2, -2), new AxialPosition(-3, 0)),
                    (new AxialPosition(-3, 0), new AxialPosition(-5, 1)),
                    (new AxialPosition(-5, 1), new AxialPosition(-6, 3)),
                    (new AxialPosition(-6, 3), new AxialPosition(-5, 4)),
                    (new AxialPosition(-5, 4), new AxialPosition(-3, 3)),
                    (new AxialPosition(-3, 3), new AxialPosition(-2, 4)),
                    (new AxialPosition(-2, 4), new AxialPosition(0, 3)),
                    (new AxialPosition(0, 3), new AxialPosition(1, 4)),
                    (new AxialPosition(1, 4), new AxialPosition(3, 3)),
                    (new AxialPosition(3, 3), new AxialPosition(4, 1)),
                    (new AxialPosition(4, 1), new AxialPosition(3, 0)),
                    (new AxialPosition(4, -2), new AxialPosition(6, -3)),
                    (new AxialPosition(4, -5), new AxialPosition(6, -6)),
                    (new AxialPosition(3, -6), new AxialPosition(4, -8)),
                    (new AxialPosition(1, -5), new AxialPosition(0, -6)),
                    (new AxialPosition(-2, -2), new AxialPosition(-3, -3)),
                    (new AxialPosition(-5, 1), new AxialPosition(-6, 0)),
                    (new AxialPosition(-6, 3), new AxialPosition(-8, 4)),
                    (new AxialPosition(-5, 4), new AxialPosition(-6, 6)),
                    (new AxialPosition(-2, 4), new AxialPosition(-3, 6)),
                    (new AxialPosition(1, 4), new AxialPosition(0, 6)),
                    (new AxialPosition(3, 3), new AxialPosition(4, 4)),
                    (new AxialPosition(4, 1), new AxialPosition(6, 0)),
                    (new AxialPosition(6, 0), new AxialPosition(7, -2)),
                    (new AxialPosition(7, -2), new AxialPosition(6, -3)),
                    (new AxialPosition(6, -3), new AxialPosition(7, -5)),
                    (new AxialPosition(7, -5), new AxialPosition(6, -6)),
                    (new AxialPosition(6, -6), new AxialPosition(7, -8)),
                    (new AxialPosition(7, -8), new AxialPosition(6, -9)),
                    (new AxialPosition(6, -9), new AxialPosition(4, -8)),
                    (new AxialPosition(4, -8), new AxialPosition(3, -9)),
                    (new AxialPosition(3, -9), new AxialPosition(1, -8)),
                    (new AxialPosition(1, -8), new AxialPosition(0, -6)),
                    (new AxialPosition(0, -6), new AxialPosition(-2, -5)),
                    (new AxialPosition(-2, -5), new AxialPosition(-3, -3)),
                    (new AxialPosition(-3, -3), new AxialPosition(-5, -2)),
                    (new AxialPosition(-5, -2), new AxialPosition(-6, 0)),
                    (new AxialPosition(-6, 0), new AxialPosition(-8, 1)),
                    (new AxialPosition(-8, 1), new AxialPosition(-9, 3)),
                    (new AxialPosition(-9, 3), new AxialPosition(-8, 4)),
                    (new AxialPosition(-8, 4), new AxialPosition(-9, 6)),
                    (new AxialPosition(-9, 6), new AxialPosition(-8, 7)),
                    (new AxialPosition(-8, 7), new AxialPosition(-6, 6)),
                    (new AxialPosition(-6, 6), new AxialPosition(-5, 7)),
                    (new AxialPosition(-5, 7), new AxialPosition(-3, 6)),
                    (new AxialPosition(-3, 6), new AxialPosition(-2, 7)),
                    (new AxialPosition(-2, 7), new AxialPosition(0, 6)),
                    (new AxialPosition(0, 6), new AxialPosition(1, 7)),
                    (new AxialPosition(1, 7), new AxialPosition(3, 6)),
                    (new AxialPosition(3, 6), new AxialPosition(4, 4)),
                    (new AxialPosition(4, 4), new AxialPosition(6, 3)),
                    (new AxialPosition(6, 3), new AxialPosition(7, 1)),
                    (new AxialPosition(7, 1), new AxialPosition(6, 0)),
                    (new AxialPosition(7, -2), new AxialPosition(9, -3)),
                    (new AxialPosition(7, -5), new AxialPosition(9, -6)),
                    (new AxialPosition(7, -8), new AxialPosition(9, -9)),
                    (new AxialPosition(6, -9), new AxialPosition(7, -11)),
                    (new AxialPosition(3, -9), new AxialPosition(4, -11)),
                    (new AxialPosition(1, -8), new AxialPosition(0, -9)),
                    (new AxialPosition(-2, -5), new AxialPosition(-3, -6)),
                    (new AxialPosition(-5, -2), new AxialPosition(-6, -3)),
                    (new AxialPosition(-8, 1), new AxialPosition(-9, 0)),
                    (new AxialPosition(-9, 3), new AxialPosition(-11, 4)),
                    (new AxialPosition(-9, 6), new AxialPosition(-11, 7)),
                    (new AxialPosition(-8, 7), new AxialPosition(-9, 9)),
                    (new AxialPosition(-5, 7), new AxialPosition(-6, 9)),
                    (new AxialPosition(-2, 7), new AxialPosition(-3, 9)),
                    (new AxialPosition(1, 7), new AxialPosition(0, 9)),
                    (new AxialPosition(3, 6), new AxialPosition(4, 7)),
                    (new AxialPosition(6, 3), new AxialPosition(7, 4)),
                    (new AxialPosition(7, 1), new AxialPosition(9, 0))
                };
            }
            else
            {
                return new (AxialPosition, AxialPosition)[]
                {
                    (new AxialPosition(0, 0), new AxialPosition(2, -1)),
                    (new AxialPosition(0, 0), new AxialPosition(-1, -1)),
                    (new AxialPosition(0, 0), new AxialPosition(-1, 2)),
                    (new AxialPosition(2, -1), new AxialPosition(3, 0)),
                    (new AxialPosition(2, -1), new AxialPosition(3, -3)),
                    (new AxialPosition(-1, -1), new AxialPosition(0, -3)),
                    (new AxialPosition(-1, -1), new AxialPosition(-3, 0)),
                    (new AxialPosition(-1, 2), new AxialPosition(-3, 3)),
                    (new AxialPosition(-1, 2), new AxialPosition(0, 3)),
                    (new AxialPosition(3, 0), new AxialPosition(5, -1)),
                    (new AxialPosition(5, -1), new AxialPosition(6, -3)),
                    (new AxialPosition(6, -3), new AxialPosition(5, -4)),
                    (new AxialPosition(5, -4), new AxialPosition(3, -3)),
                    (new AxialPosition(3, -3), new AxialPosition(2, -4)),
                    (new AxialPosition(2, -4), new AxialPosition(0, -3)),
                    (new AxialPosition(0, -3), new AxialPosition(-1, -4)),
                    (new AxialPosition(-1, -4), new AxialPosition(-3, -3)),
                    (new AxialPosition(-3, -3), new AxialPosition(-4, -1)),
                    (new AxialPosition(-4, -1), new AxialPosition(-3, 0)),
                    (new AxialPosition(-3, 0), new AxialPosition(-4, 2)),
                    (new AxialPosition(-4, 2), new AxialPosition(-3, 3)),
                    (new AxialPosition(-3, 3), new AxialPosition(-4, 5)),
                    (new AxialPosition(-4, 5), new AxialPosition(-3, 6)),
                    (new AxialPosition(-3, 6), new AxialPosition(-1, 5)),
                    (new AxialPosition(-1, 5), new AxialPosition(0, 3)),
                    (new AxialPosition(0, 3), new AxialPosition(2, 2)),
                    (new AxialPosition(2, 2), new AxialPosition(3, 0)),
                    (new AxialPosition(5, -1), new AxialPosition(6, 0)),
                    (new AxialPosition(6, -3), new AxialPosition(8, -4)),
                    (new AxialPosition(5, -4), new AxialPosition(6, -6)),
                    (new AxialPosition(2, -4), new AxialPosition(3, -6)),
                    (new AxialPosition(-1, -4), new AxialPosition(0, -6)),
                    (new AxialPosition(-3, -3), new AxialPosition(-4, -4)),
                    (new AxialPosition(-4, -1), new AxialPosition(-6, 0)),
                    (new AxialPosition(-4, 2), new AxialPosition(-6, 3)),
                    (new AxialPosition(-4, 5), new AxialPosition(-6, 6)),
                    (new AxialPosition(-3, 6), new AxialPosition(-4, 8)),
                    (new AxialPosition(-1, 5), new AxialPosition(0, 6)),
                    (new AxialPosition(2, 2), new AxialPosition(3, 3)),
                    (new AxialPosition(6, 0), new AxialPosition(8, -1)),
                    (new AxialPosition(8, -1), new AxialPosition(9, -3)),
                    (new AxialPosition(9, -3), new AxialPosition(8, -4)),
                    (new AxialPosition(8, -4), new AxialPosition(9, -6)),
                    (new AxialPosition(9, -6), new AxialPosition(8, -7)),
                    (new AxialPosition(8, -7), new AxialPosition(6, -6)),
                    (new AxialPosition(6, -6), new AxialPosition(5, -7)),
                    (new AxialPosition(5, -7), new AxialPosition(3, -6)),
                    (new AxialPosition(3, -6), new AxialPosition(2, -7)),
                    (new AxialPosition(2, -7), new AxialPosition(0, -6)),
                    (new AxialPosition(0, -6), new AxialPosition(-1, -7)),
                    (new AxialPosition(-1, -7), new AxialPosition(-3, -6)),
                    (new AxialPosition(-3, -6), new AxialPosition(-4, -4)),
                    (new AxialPosition(-4, -4), new AxialPosition(-6, -3)),
                    (new AxialPosition(-6, -3), new AxialPosition(-7, -1)),
                    (new AxialPosition(-7, -1), new AxialPosition(-6, 0)),
                    (new AxialPosition(-6, 0), new AxialPosition(-7, 2)),
                    (new AxialPosition(-7, 2), new AxialPosition(-6, 3)),
                    (new AxialPosition(-6, 3), new AxialPosition(-7, 5)),
                    (new AxialPosition(-7, 5), new AxialPosition(-6, 6)),
                    (new AxialPosition(-6, 6), new AxialPosition(-7, 8)),
                    (new AxialPosition(-7, 8), new AxialPosition(-6, 9)),
                    (new AxialPosition(-6, 9), new AxialPosition(-4, 8)),
                    (new AxialPosition(-4, 8), new AxialPosition(-3, 9)),
                    (new AxialPosition(-3, 9), new AxialPosition(-1, 8)),
                    (new AxialPosition(-1, 8), new AxialPosition(0, 6)),
                    (new AxialPosition(0, 6), new AxialPosition(2, 5)),
                    (new AxialPosition(2, 5), new AxialPosition(3, 3)),
                    (new AxialPosition(3, 3), new AxialPosition(5, 2)),
                    (new AxialPosition(5, 2), new AxialPosition(6, 0)),
                    (new AxialPosition(8, -1), new AxialPosition(9, 0)),
                    (new AxialPosition(9, -3), new AxialPosition(11, -4)),
                    (new AxialPosition(9, -6), new AxialPosition(11, -7)),
                    (new AxialPosition(8, -7), new AxialPosition(9, -9)),
                    (new AxialPosition(5, -7), new AxialPosition(6, -9)),
                    (new AxialPosition(2, -7), new AxialPosition(3, -9)),
                    (new AxialPosition(-1, -7), new AxialPosition(0, -9)),
                    (new AxialPosition(-3, -6), new AxialPosition(-4, -7)),
                    (new AxialPosition(-6, -3), new AxialPosition(-7, -4)),
                    (new AxialPosition(-7, -1), new AxialPosition(-9, 0)),
                    (new AxialPosition(-7, 2), new AxialPosition(-9, 3)),
                    (new AxialPosition(-7, 5), new AxialPosition(-9, 6)),
                    (new AxialPosition(-7, 8), new AxialPosition(-9, 9)),
                    (new AxialPosition(-6, 9), new AxialPosition(-7, 11)),
                    (new AxialPosition(-3, 9), new AxialPosition(-4, 11)),
                    (new AxialPosition(-1, 8), new AxialPosition(0, 9)),
                    (new AxialPosition(2, 5), new AxialPosition(3, 6)),
                    (new AxialPosition(5, 2), new AxialPosition(6, 3))
                };
            }
		}

        public static ConstraintPositionsGroup[] GetConstraintPositionGroups(AxialPosition primaryVertexPosition)
        {
            if (VertexPointsUp(primaryVertexPosition))
            {
                ConstraintPositionsGroup constraintGroup1 = new(
                    new AxialPosition(0, 0),
                    new AxialPosition(1, -2),
                    new AxialPosition[]
                    {
                        new AxialPosition(0, 0),
                        new AxialPosition(-2, 1),
                        new AxialPosition(-3, 3),
                        new AxialPosition(-2, 4),
                        new AxialPosition(0, 3),
                        new AxialPosition(1, 1)
                    },
                    new AxialPosition[]
                    {
                        new AxialPosition(3, -3),
                        new AxialPosition(4, -5),
                        new AxialPosition(3, -6),
                        new AxialPosition(1, -5),
                        new AxialPosition(0, -3)
                    }
                );

                ConstraintPositionsGroup constraintGroup2 = new(
                    new AxialPosition(0, 0),
                    new AxialPosition(-2, 1),
                    new AxialPosition[]
                    {
                        new AxialPosition(0, 0),
                        new AxialPosition(1, 1),
                        new AxialPosition(3, 0),
                        new AxialPosition(4, -2),
                        new AxialPosition(3, -3),
                        new AxialPosition(1, -2)
                    },
                    new AxialPosition[]
                    {
                        new AxialPosition(-3, 0),
                        new AxialPosition(-5, 1),
                        new AxialPosition(-6, 3),
                        new AxialPosition(-5, 4),
                        new AxialPosition(-3, 3)
                    }
                );

                ConstraintPositionsGroup constraintGroup3 = new(
                    new AxialPosition(0, 0),
                    new AxialPosition(1, 1),
                    new AxialPosition[]
                    {
                        new AxialPosition(0, 0),
                        new AxialPosition(1, -2),
                        new AxialPosition(0, -3),
                        new AxialPosition(-2, -2),
                        new AxialPosition(-3, 0),
                        new AxialPosition(-2, 1)
                    },
                    new AxialPosition[]
                    {
                        new AxialPosition(0, 3),
                        new AxialPosition(1, 4),
                        new AxialPosition(3, 3),
                        new AxialPosition(4, 1),
                        new AxialPosition(3, 0)
                    }
                );

                return new ConstraintPositionsGroup[]
                {
                    constraintGroup1,
                    constraintGroup2,
                    constraintGroup3
                };
            }
            else
            {
                ConstraintPositionsGroup constraintGroup1 = new(
                    new AxialPosition(0, 0),
                    new AxialPosition(2, -1),
                    new AxialPosition[]
                    {
                        new AxialPosition(0, 0),
                        new AxialPosition(-1, -1),
                        new AxialPosition(-3, 0),
                        new AxialPosition(-4, 2),
                        new AxialPosition(-3, 3),
                        new AxialPosition(-1, 2)
                    },
                    new AxialPosition[]
                    {
                        new AxialPosition(3, 0),
                        new AxialPosition(5, -1),
                        new AxialPosition(6, -3),
                        new AxialPosition(5, -4),
                        new AxialPosition(3, -3),
                    }
                );

                ConstraintPositionsGroup constraintGroup2 = new(
                    new AxialPosition(0, 0),
                    new AxialPosition(-1, -1),
                    new AxialPosition[]
                    {
                        new AxialPosition(0, 0),
                        new AxialPosition(-1, 2),
                        new AxialPosition(0, 3),
                        new AxialPosition(2, 2),
                        new AxialPosition(3, 0),
                        new AxialPosition(2, -1)
                    },
                    new AxialPosition[]
                    {
                        new AxialPosition(0, -3),
                        new AxialPosition(-1, -4),
                        new AxialPosition(-3, -3),
                        new AxialPosition(-4, -1),
                        new AxialPosition(-3, 0)
                    }
                );

                ConstraintPositionsGroup constraintGroup3 = new(
                    new AxialPosition(0, 0),
                    new AxialPosition(-1, 2),
                    new AxialPosition[]
                    {
                        new AxialPosition(0, 0),
                        new AxialPosition(2, -1),
                        new AxialPosition(3, -3),
                        new AxialPosition(2, -4),
                        new AxialPosition(0, -3),
                        new AxialPosition(-1, -1)
                    },
                    new AxialPosition[]
                    {
                        new AxialPosition(-3, 3),
                        new AxialPosition(-4, 5),
                        new AxialPosition(-3, 6),
                        new AxialPosition(-1, 5),
                        new AxialPosition(0, 3)
                    }
                );

                return new ConstraintPositionsGroup[]
                {
                    constraintGroup1,
                    constraintGroup2,
                    constraintGroup3
                };
            }
        }

        public struct ConstraintPositionsGroup
        {
            public ConstraintPositionsGroup(
                AxialPosition vPrimary,
                AxialPosition vSecondary,
                AxialPosition[] hPrimary,
                AxialPosition[] hSecondary
                )
            {
                VPrimary = vPrimary;
                VSecondary = vSecondary;
                HPrimary = hPrimary;
                HSecondary = hSecondary;
            }

            public AxialPosition VPrimary { get; }
            public AxialPosition VSecondary { get; }
            public AxialPosition[] HPrimary { get; }
            public AxialPosition[] HSecondary { get; }
        }
    }
}

