using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using lionturtle;

namespace lionturtle
{
	public class BlurryGrid
	{
		public Dictionary<AxialPosition, BlurryValue> BlurryValues;
		public Dictionary<AxialPosition, double> ResolvedValues;
		public Queue<AxialPosition> PropagationQueue;

		public BlurryGrid()
		{
			BlurryValues = new();
			ResolvedValues = new();
			PropagationQueue = new();
		}

        public BlurryValue FindOrCreateBlurryValue(AxialPosition position)
		{
			if (BlurryValues.ContainsKey(position))
			{
				return BlurryValues[position];
			}
			else
			{
				BlurryValue blurryValue = new();
				BlurryValues[position] = blurryValue;
				return blurryValue;
			}

        }

		public double ResolveValueAtPosition(AxialPosition position, double heuristic)
		{
            BlurryValue blurryVertex = FindOrCreateBlurryValue(position);
            double resolvedValue = blurryVertex.Resolve(heuristic);
            ResolvedValues[position] = resolvedValue;
            PropagationQueue.Enqueue(position);
            while (PropagationQueue.Count > 0)
            {
                PropagateConstraints(PropagationQueue.Dequeue());
            }
            return resolvedValue;
        }

		public double? FindResolvedValueAt(AxialPosition position)
		{
			return ResolvedValues.ContainsKey(position)
				? ResolvedValues[position]
				: null;
        }

        public void ConstrainAndMaybePropagate(AxialPosition position, double? newMin, double? newMax){
            BlurryValue subject = FindOrCreateBlurryValue(position);
            BlurryValue before = (BlurryValue)subject.Clone();
            subject.Constrain(newMin, newMax);
            if(subject.DifferentFrom(before)){
                PropagationQueue.Enqueue(position);
            }
        }

        public void PinchAndMaybePropagate(AxialPosition position, BlurryValue a, BlurryValue b){
            BlurryValue subject = FindOrCreateBlurryValue(position);
            BlurryValue before = (BlurryValue)subject.Clone();
            subject.Pinch(a, b);
            if(subject.DifferentFrom(before)){
                PropagationQueue.Enqueue(position);
            }
        }

        public void PropagateConstraints(AxialPosition position)
        {
            VertexGroup[] vGroups = GridUtilities.GetVertexGroups(position);

            BlurryValue primaryV = BlurryValues[position];

            for (int i = 0; i < vGroups.Length; i++)
            {
                AxialPosition secondaryVPosition = position + vGroups[i].SecondaryVertex;
                if (BlurryValues.ContainsKey(secondaryVPosition))
                {
                    BlurryValue secondaryV = BlurryValues[secondaryVPosition];

                    //handle pinches
                    AxialPosition[] pinchedPositions = vGroups[i].PinchedVertices;
                    for (int j = 0; j < pinchedPositions.Length; j++)
                    {
                        PinchAndMaybePropagate(
                            position + pinchedPositions[j],
                            primaryV,
                            secondaryV
                        );
                    }

                    //handle pushes
                    //primary is lower than secondary
                    if (primaryV.Max < secondaryV.Min)
                    {
                        AxialPosition[] primaryAffectedPositions = vGroups[i].PrimaryPushedVertices;
                        AxialPosition[] secondaryAffectedPositions = vGroups[i].SecondaryPushedVertices;

                        for (int j = 0; j < primaryAffectedPositions.Length; j++)
                        {
                            ConstrainAndMaybePropagate(
                                position + primaryAffectedPositions[j],
                                null,
                                primaryV.Max
                            );
                        }

                        for (int j = 0; j < secondaryAffectedPositions.Length; j++)
                        {
                            ConstrainAndMaybePropagate(
                                position + secondaryAffectedPositions[j],
                                secondaryV.Min,
                                null
                            );
                        }
                    }

                    //primary is higher than secondary
                    if (primaryV.Min > secondaryV.Max)
                    {
                        AxialPosition[] primaryAffectedPositions = vGroups[i].PrimaryPushedVertices;
                        AxialPosition[] secondaryAffectedPositions = vGroups[i].SecondaryPushedVertices;

                        for (int j = 0; j < primaryAffectedPositions.Length; j++)
                        {
                            ConstrainAndMaybePropagate(
                                position + primaryAffectedPositions[j],
                                primaryV.Min,
                                null
                            );
                        }

                        for (int j = 0; j < secondaryAffectedPositions.Length; j++)
                        {
                            ConstrainAndMaybePropagate(
                                position + secondaryAffectedPositions[j],
                                null,
                                secondaryV.Max
                            );
                        }
                    }
                }
            }
        }

        public string GetStringHexes(int numRings)
		{
            string stringHexes = "hexes = {";

            AxialPosition[] hexPositions = GridUtilities.GetSpiralHexPositions(numRings);

            foreach (AxialPosition hexPosition in hexPositions)
            {
                string stringHex = $"HexPosition({hexPosition.Q}, {hexPosition.R}): Hex([";

                for (int direction = 0; direction < 6; direction++)
                {
                    AxialPosition vertexPosition = hexPosition*3+Constants.dualDirections[direction];
                    double resolvedValue = ResolvedValues[vertexPosition];
                    if (direction < 5)
                        stringHex += $"{resolvedValue}, ";
                    else
                        stringHex += $"{resolvedValue}])";
                }

                stringHexes += stringHex;
                stringHexes += ", ";
            }
            stringHexes += "}";

			return stringHexes;
        }

        public string GetStringBlurryValues()
        {
            string stringBlurryValues = "blurryValues = new List<BlurryValue>{";
            foreach (KeyValuePair<AxialPosition, BlurryValue> pair in BlurryValues)
            {
                AxialPosition position = pair.Key;
                BlurryValue blurryValue = pair.Value;

				string minString = blurryValue.Min == null ? "null" : blurryValue.Min + "";
				string maxString = blurryValue.Max == null ? "null" : blurryValue.Max + "";

                string stringBlurryValue = $"new BlurryValue {{" +
					$"hexPosition = new HexCoordinate ({position.Q}, {position.R})," +
					$"min = {minString}," +
					$"max = {maxString}" +
				$"}}";

                stringBlurryValues += stringBlurryValue;
                stringBlurryValues += ", ";
            }
            stringBlurryValues += "};";

            return stringBlurryValues;
        }
    }
}
