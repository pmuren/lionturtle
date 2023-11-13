using System;
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

        public void ManifestHexAtPosition(AxialPosition position, int heuristic)
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

            Hex newHex = new(verts, heuristic);
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

		public Vertex ResolveVertexAtPosition(AxialPosition position, int heuristic)
		{
			VirtualVertex blurryVertex = GetOrCreateVirtualVertex(position);
			int height = blurryVertex.Resolve(heuristic);
			Vertex newVert = new(position, height);
            Vertices[position] = newVert;
			PropagationQueue.Enqueue(position);
			while(PropagationQueue.Count > 0)
			{
				PropagateConstraints(PropagationQueue.Dequeue());
            }
            return newVert;
        }

		public void PropagateConstraints(AxialPosition position)
		{
			VertexGroup[] vGroups = GridUtilities.GetVertexGroups(position);

			VirtualVertex primaryV = VirtualVertices[position];

			AxialPosition[] longSpokes = new AxialPosition[]
			{
				new AxialPosition(6, 0),
				new AxialPosition(6, -6),
				new AxialPosition(0, -6),
				new AxialPosition(-6, 0),
				new AxialPosition(-6, 6),
				new AxialPosition(0, 6),
			};

			AxialPosition[] shortSpokes = new AxialPosition[]
			{
				new AxialPosition(3, 0),
				new AxialPosition(3, -3),
				new AxialPosition(0, -3),
				new AxialPosition(-3, 0),
				new AxialPosition(-3, 3),
				new AxialPosition(0, 3),
			};

            for (int i = 0; i < 6; i++)
			{
				if (VirtualVertices.ContainsKey(position + longSpokes[i]))
				{
					VirtualVertex secondaryV = VirtualVertices[position + longSpokes[i]];
					AxialPosition squeezedVPosition = shortSpokes[i];
					VirtualVertex squeezedV = GetOrCreateVirtualVertex(position + squeezedVPosition);

					int? lowerMin = primaryV.min;
					if (secondaryV.min == null) lowerMin = null;
					else if (secondaryV.min < primaryV.min) lowerMin = secondaryV.min;

					int? higherMax = primaryV.max;
					if (secondaryV.max == null) higherMax = null;
					else if (secondaryV.max > primaryV.max) higherMax = secondaryV.max;

					int? oldMin = squeezedV.min;
					int? oldMax = squeezedV.max;

					squeezedV.Constrain(lowerMin, higherMax);

					if (squeezedV.min != oldMin || squeezedV.max != oldMax) PropagationQueue.Enqueue(squeezedV.position);
				}
            }


            for (int i = 0; i < vGroups.Length; i++)
			{
				AxialPosition secondaryVPosition = position + vGroups[i].SecondaryVertex;
				if (VirtualVertices.ContainsKey(secondaryVPosition))
				{
					VirtualVertex secondaryV = VirtualVertices[secondaryVPosition];

					//squeezed vertex if this is a long group
					if (vGroups[i].SqueezedVertex != null)
					{
						AxialPosition squeezedVPosition = vGroups[i].SqueezedVertex?? new AxialPosition(0, 0); //Hack, TODO: Handle this possibly null value instead of worthless default
						VirtualVertex squeezedV = GetOrCreateVirtualVertex(position + squeezedVPosition);

						int? lowerMin = primaryV.min;
						if (secondaryV.min == null) lowerMin = null;
						else if (secondaryV.min < primaryV.min) lowerMin = secondaryV.min;

                        int? higherMax = primaryV.max;
                        if (secondaryV.max == null) higherMax = null;
                        else if (secondaryV.max > primaryV.max) higherMax = secondaryV.max;

						int? oldMin = squeezedV.min;
						int? oldMax = squeezedV.max;

						squeezedV.Constrain(lowerMin, higherMax);

						if (squeezedV.min != oldMin || squeezedV.max != oldMax) PropagationQueue.Enqueue(squeezedV.position);
					}

					//primary is lower than secondary
					if(primaryV.max < secondaryV.min)
					{
                        AxialPosition[] primaryAffectedPositions = vGroups[i].PrimaryAffected;
                        AxialPosition[] secondaryAffectedPositions = vGroups[i].SecondaryAffected;

                        for (int j = 0; j < primaryAffectedPositions.Length; j++)
                        {
                            VirtualVertex vv = GetOrCreateVirtualVertex(position + primaryAffectedPositions[j]);

                            int? oldMin = vv.min;
                            int? oldMax = vv.max;

                            vv.Constrain(null, primaryV.max);

                            if (vv.min != oldMin || vv.max != oldMax) PropagationQueue.Enqueue(vv.position);
                        }

                        for (int j = 0; j < secondaryAffectedPositions.Length; j++)
						{
							VirtualVertex vv = GetOrCreateVirtualVertex(position + secondaryAffectedPositions[j]);

                            int? oldMin = vv.min;
                            int? oldMax = vv.max;

                            vv.Constrain(secondaryV.min, null);

                            if (vv.min != oldMin || vv.max != oldMax) PropagationQueue.Enqueue(vv.position);
                        }
                    }

					//primary is higher than secondary
                    if (primaryV.min > secondaryV.max)
                    {
                        AxialPosition[] primaryAffectedPositions = vGroups[i].PrimaryAffected;
                        AxialPosition[] secondaryAffectedPositions = vGroups[i].SecondaryAffected;

                        for (int j = 0; j < primaryAffectedPositions.Length; j++)
                        {
                            VirtualVertex vv = GetOrCreateVirtualVertex(position + primaryAffectedPositions[j]);

                            int? oldMin = vv.min;
                            int? oldMax = vv.max;

                            vv.Constrain(primaryV.min, null);

                            if (vv.min != oldMin || vv.max != oldMax) PropagationQueue.Enqueue(vv.position);
                        }

                        for (int j = 0; j < secondaryAffectedPositions.Length; j++)
                        {
                            VirtualVertex vv = GetOrCreateVirtualVertex(position + secondaryAffectedPositions[j]);

                            int? oldMin = vv.min;
                            int? oldMax = vv.max;

                            vv.Constrain(null, secondaryV.max);

                            if (vv.min != oldMin || vv.max != oldMax) PropagationQueue.Enqueue(vv.position);
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

                stringHexes += stringHex;
                stringHexes += ", ";
            }
            stringHexes += "}";

			return stringHexes;
        }
    }
}