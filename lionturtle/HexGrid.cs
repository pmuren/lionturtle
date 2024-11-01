﻿using System;
using lionturtle;

namespace lionturtle
{
	public class HexGrid
	{
		public Dictionary<AxialPosition, VirtualVertex> VirtualVertices;
		public Dictionary<AxialPosition, Vertex> Vertices;
		public Dictionary<AxialPosition, Hex> Hexes;
		public Queue<AxialPosition> PropagationQueue;

		public HexGrid()
		{
			VirtualVertices = new();
			Vertices = new();
			Hexes = new();
			PropagationQueue = new();
		}

        //TODO: Put this somewhere else!
        public void Populate()
        {
            SlotGrid slotGrid = new SlotGrid();
            slotGrid.CollapseEverything();

            var hexes = new Dictionary<AxialPosition, int[]>();
            foreach (AxialPosition position in slotGrid.Slots.Keys)
            {
                var tileHeight = slotGrid.TileHeights[position];
                var relativeVertexHeights = slotGrid.Slots[position].modules.First().GetRelativeVertexHeights();
                int[] hex = new int[6];
                for (int vertexIndex = 0; vertexIndex < 6; vertexIndex++)
                {
                    hex[vertexIndex] = relativeVertexHeights[vertexIndex] + (int)Math.Floor(tileHeight);
                }
                hexes[position] = hex;
            }

            foreach (AxialPosition position in hexes.Keys)
            {
                Hexes[position] = new Hex(new Vertex[6], new VertexType[6]);
                for (int vertexIndex = 0; vertexIndex < 6; vertexIndex++)
                {
                    AxialPosition vPosition =
                        GridUtilities.GetVertexPositionForHexV(position, vertexIndex);
                    int vHeight = hexes[position][vertexIndex];
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

            foreach( AxialPosition position in Vertices.Keys)
            {
                Vertices[position].type = slotGrid.GetCornerVertexType(position);
            }
        }

		public Vertex? FindExistingVertexForHexV(AxialPosition hexPosition, int vIndex)
		{
			AxialPosition vertexPosition = GridUtilities.GetVertexPositionForHexV(hexPosition, vIndex);

			Vertex? vertex = FindExistingVertex(vertexPosition);
			return vertex;
		}

		public Hex? FindExistingHex(AxialPosition position)
		{
			return Hexes.ContainsKey(position)
				? Hexes[position]
				: null;
		}

		public Vertex? FindExistingVertex(AxialPosition position)
		{
			return Vertices.ContainsKey(position)
				? Vertices[position]
				: null;
        }

        public void ManifestHexAtPosition(AxialPosition position, double heuristic)
		{
			Vertex[] verts = new Vertex[6];

			AxialPosition[] vertexPositions = GridUtilities.GetVertexPositionsFromHexPosiiton(position);

			for(int i = 0; i < vertexPositions.Length; i++)
			{
				Vertex? existingVertex = FindExistingVertexForHexV(position, i);
                if (existingVertex != null)
				{
					verts[i] = existingVertex;
				}
				else
				{
					Vertex newVert = ResolveVertexAtPosition(vertexPositions[i], heuristic);
                    verts[i] = newVert;
                }
			}

            Hex newHex = new(verts, new VertexType[] {
                VertexType.Unknown,
                VertexType.Unknown,
                VertexType.Unknown,
                VertexType.Unknown,
                VertexType.Unknown,
                VertexType.Unknown
            });
            Hexes[position] = newHex;
        }

        public void ManifestVerticesAtPosition(AxialPosition position, double[] heuristics)
        {
            Vertex[] verts = new Vertex[6];

            AxialPosition[] vertexPositions = GridUtilities.GetVertexPositionsFromHexPosiiton(position);

            for (int i = 0; i < vertexPositions.Length; i++)
            {
                Vertex? existingVertex = FindExistingVertexForHexV(position, i);
                if (existingVertex != null)
                {
                    verts[i] = existingVertex;
                }
                else
                {
                    Vertex newVert = ResolveVertexAtPosition(vertexPositions[i], heuristics[i]);
                    verts[i] = newVert;
                }
            }

            Hex newHex = new(verts, new VertexType[] {
                VertexType.Unknown,
                VertexType.Unknown,
                VertexType.Unknown,
                VertexType.Unknown,
                VertexType.Unknown,
                VertexType.Unknown
            });
            Hexes[position] = newHex;
        }

        public VirtualVertex GetOrCreateVirtualVertex(AxialPosition position)
		{
			if (VirtualVertices.ContainsKey(position))
			{
				return VirtualVertices[position];
			}
			else
			{
				VirtualVertex vv = new(position, null, null);
				VirtualVertices[position] = vv;
				return vv;
			}
				
        }

		public Vertex ResolveVertexAtPosition(AxialPosition position, double heuristic)
		{
            bool freeform = false;
            if (freeform)
            {
                double height = heuristic;
                Vertex newVert = new(position, height);
                Vertices[position] = newVert;
                return newVert;
            }
            else
            {
                VirtualVertex blurryVertex = GetOrCreateVirtualVertex(position);
                double height = blurryVertex.Resolve(heuristic);
                Vertex newVert = new(position, height);
                Vertices[position] = newVert;
                PropagationQueue.Enqueue(position);
                while (PropagationQueue.Count > 0)
                {
                    PropagateConstraints(PropagationQueue.Dequeue());
                }
                return newVert;
            }
            
        }

		public void ConstrainAndPropagate(VirtualVertex vv, double? newMin, double? newMax)
		{
            double? oldMin = vv.min;
            double? oldMax = vv.max;

            try
            {
                vv.Constrain(newMin, newMax);
            }
            catch (Exception e)
            {
                Console.WriteLine(this.GetStringVVs());
                Console.WriteLine(this.GetStringHexes());
                throw e;
            }

            if (vv.min != oldMin || vv.max != oldMax)
			{
				PropagationQueue.Enqueue(vv.position);
			}
        }

        public void PinchAndPropagate(VirtualVertex vv, VirtualVertex vva, VirtualVertex vvb)
        {
            double? oldMin = vv.min;
            double? oldMax = vv.max;

            double? highMax = NullOrMax(vva.max, vvb.max);
            double? lowMin = NullOrMin(vva.min, vvb.min);

            try
            {
                vv.Constrain(lowMin, highMax);
            }
            catch (Exception e)
            {
                Console.WriteLine(this.GetStringVVs());
                Console.WriteLine(this.GetStringHexes());
                throw e;
            }

            if (vv.min != oldMin || vv.max != oldMax)
            {
                PropagationQueue.Enqueue(vv.position);
            }
        }

        public static double? NullOrMax(double? a, double? b)
        {
            if (a == null || b == null) return null;
            return Math.Max(a.Value, b.Value);
        }

        public static double? NullOrMin(double? a, double? b)
        {
            if (a == null || b == null) return null;
            return Math.Min(a.Value, b.Value);
        }

        public void PropagateConstraints(AxialPosition position)
        {
            VertexGroup[] vGroups = GridUtilities.GetVertexGroups(position);

            VirtualVertex primaryV = VirtualVertices[position];

            for (int i = 0; i < vGroups.Length; i++)
            {
                AxialPosition secondaryVPosition = position + vGroups[i].SecondaryVertex;
                if (VirtualVertices.ContainsKey(secondaryVPosition))
                {
                    VirtualVertex secondaryV = VirtualVertices[secondaryVPosition];

                    //handle pinches
                    AxialPosition[] pinchedPositions = vGroups[i].PinchedVertices;
                    for (int j = 0; j < pinchedPositions.Length; j++)
                    {
                        VirtualVertex vv = GetOrCreateVirtualVertex(position + pinchedPositions[j]);
                        PinchAndPropagate(vv, primaryV, secondaryV);
                    }

                    //handle pushes
                    //primary is lower than secondary
                    if (primaryV.max < secondaryV.min)
                    {
                        AxialPosition[] primaryAffectedPositions = vGroups[i].PrimaryPushedVertices;
                        AxialPosition[] secondaryAffectedPositions = vGroups[i].SecondaryPushedVertices;

                        for (int j = 0; j < primaryAffectedPositions.Length; j++)
                        {
                            VirtualVertex vv = GetOrCreateVirtualVertex(position + primaryAffectedPositions[j]);
                            ConstrainAndPropagate(vv, null, primaryV.max);
                        }

                        for (int j = 0; j < secondaryAffectedPositions.Length; j++)
                        {
                            VirtualVertex vv = GetOrCreateVirtualVertex(position + secondaryAffectedPositions[j]);
                            ConstrainAndPropagate(vv, secondaryV.min, null);
                        }
                    }

                    //primary is higher than secondary
                    if (primaryV.min > secondaryV.max)
                    {
                        AxialPosition[] primaryAffectedPositions = vGroups[i].PrimaryPushedVertices;
                        AxialPosition[] secondaryAffectedPositions = vGroups[i].SecondaryPushedVertices;

                        for (int j = 0; j < primaryAffectedPositions.Length; j++)
                        {
                            VirtualVertex vv = GetOrCreateVirtualVertex(position + primaryAffectedPositions[j]);

                            ConstrainAndPropagate(vv, primaryV.min, null);
                        }

                        for (int j = 0; j < secondaryAffectedPositions.Length; j++)
                        {
                            VirtualVertex vv = GetOrCreateVirtualVertex(position + secondaryAffectedPositions[j]);
                            ConstrainAndPropagate(vv, null, secondaryV.max);
                        }
                    }
                }
            }
        }

        public string GetStringHexes()
		{
            string stringHexes = "hexes = {";
            foreach (KeyValuePair<AxialPosition, Hex> pair in Hexes)
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

                stringHex += $"], [";
                for (int j = 0; j < hex.Verts.Length; j++)
                {
                    if (j < hex.Verts.Length - 1)
                        stringHex += $"{hex.VertexTypes[j]}, ";
                    else
                        stringHex += $"{hex.VertexTypes[j]}])";
                }

                stringHexes += stringHex;
                stringHexes += ", ";
            }
            stringHexes += "}";

			return stringHexes;
        }

        public string GetStringVVs()
        {
            string stringVVs = "vvs = new List<VirtualVertex>{";
            foreach (KeyValuePair<AxialPosition, VirtualVertex> pair in VirtualVertices)
            {
                AxialPosition position = pair.Key;
                VirtualVertex vv = pair.Value;

				string minString = vv.min == null ? "null" : vv.min + "";
				string maxString = vv.max == null ? "null" : vv.max + "";

                string stringVV = $"new VirtualVertex {{" +
					$"hexPosition = new HexCoordinate ({position.Q}, {position.R})," +
					$"min = {minString}," +
					$"max = {maxString}" +
				$"}}";

                stringVVs += stringVV;
                stringVVs += ", ";
            }
            stringVVs += "};";

            return stringVVs;
        }
    }
}