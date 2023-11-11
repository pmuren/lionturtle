using System;
using lionturtle;

namespace lionturtle
{
	public class HexGrid
	{
		public Dictionary<AxialPosition, Vertex> Vertices;
		public Dictionary<AxialPosition, Hex> Hexes;

		public HexGrid()
		{
			Vertices = new();
			Hexes = new();
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

		public Vertex ResolveVertexAtPosition(AxialPosition position, int heuristic)
		{
            Dictionary<AxialPosition, VirtualVertex> localConstraints = DetermineLocalConstraints(position);
			VirtualVertex blurryVertex = GridUtilities.GetBlurryVertex(position, localConstraints);
			int height = blurryVertex.Resolve(heuristic);
			Vertex newVert = new(position, height);
            Vertices[position] = newVert;
			return newVert;
        }

        public Dictionary<AxialPosition, VirtualVertex> DetermineLocalConstraints(AxialPosition vertexPosition)
		{
			Dictionary<AxialPosition, VirtualVertex> localConstraints = new();

			(AxialPosition, AxialPosition)[] localVertexPositionPairs = GridUtilities.GetLocalVertexPositionPairs(vertexPosition);
			//For each pair of local vertex positions
			for(int i = 0; i < localVertexPositionPairs.Length; i++)
			{
				AxialPosition vertexAPosition = vertexPosition + localVertexPositionPairs[i].Item1;
				AxialPosition vertexBPosition = vertexPosition + localVertexPositionPairs[i].Item2;

				Vertex? vertexA = FindExistingVertex(vertexAPosition);
				Vertex? vertexB = FindExistingVertex(vertexBPosition);

				//If both vertices exist
				if(vertexA != null && vertexB != null)
				{
					//Get the constraints they generate
					VirtualVertex[] newConstraints = GridUtilities.GetConstraintsCausedByVertexPair(vertexA, vertexB);

					//And Apply them to Virtual Vertices so far
					for(int j = 0; j < newConstraints.Length; j++)
					{
						if (!localConstraints.ContainsKey(newConstraints[j].position))
						{
							localConstraints[newConstraints[j].position] = newConstraints[j];
						}
						else
						{
							VirtualVertex existingConstraint = localConstraints[newConstraints[j].position];
							VirtualVertex newConstraint = newConstraints[j];
							existingConstraint.Constrain(newConstraint.min, newConstraint.max);
						}
					}
				}

				//If a vertex is already resolved, the min and max of its virtual should be its height
				//Room for optimization, since I may be doing this for every pair
				if(vertexA != null)
					localConstraints[vertexAPosition] = new VirtualVertex(vertexAPosition, vertexA.height, vertexA.height);
				if(vertexB != null)
					localConstraints[vertexBPosition] = new VirtualVertex(vertexBPosition, vertexB.height, vertexB.height);

			}

			return localConstraints;
		}
    }
}