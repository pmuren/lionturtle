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

        public void ConstrainAndMaybePropagate(AxialPosition position, double? newMin, double? newMax, bool noProp = false){
            BlurryValue subject = FindOrCreateBlurryValue(position);
            BlurryValue before = (BlurryValue)subject.Clone();
            subject.Constrain(newMin, newMax);
            if(!subject.SameAs(before) && !noProp){
                PropagationQueue.Enqueue(position);
            }
        }

        public void PinchAndMaybePropagate(AxialPosition position, BlurryValue a, BlurryValue b){
            BlurryValue subject = FindOrCreateBlurryValue(position);
            BlurryValue before = (BlurryValue)subject.Clone();
            subject.Pinch(a, b);
            if(!subject.SameAs(before)){
                PropagationQueue.Enqueue(position);
            }
        }

        public void PropagateConstraints(AxialPosition position)
        {
            if(Math.Abs(position.Q) + Math.Abs(position.R) > 100)
                return;

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

                if(vGroups[i].Ribbons != null){
                    //handle sides (for ridges aka saddles)
                    //kinda just hacking over here rn!

                    //For all but the primary vertex,
                    //creates a temporary (null, null) if not present in grid

                    AxialPositionPair[] ribbons = vGroups[i].Ribbons.Select(
                        relativePair => new AxialPositionPair(
                            relativePair.a + position, relativePair.b + position
                        )
                    ).ToArray();

                    BlurryValuePair centerPair = new BlurryValuePair(
                        primaryV,
                        BlurryValues.ContainsKey(secondaryVPosition)?
                            BlurryValues[secondaryVPosition]
                            : new BlurryValue()
                    );
                    BlurryValuePair LFar = new BlurryValuePair(
                        BlurryValues.ContainsKey(ribbons[0].a)?
                            BlurryValues[ribbons[0].a]
                            : new BlurryValue(),
                        BlurryValues.ContainsKey(ribbons[0].b)?
                            BlurryValues[ribbons[0].b]
                            : new BlurryValue()
                    );
                    BlurryValuePair LNear = new BlurryValuePair(
                        BlurryValues.ContainsKey(ribbons[1].a)?
                            BlurryValues[ribbons[1].a]
                            : new BlurryValue(),
                        BlurryValues.ContainsKey(ribbons[1].b)?
                            BlurryValues[ribbons[1].b]
                            : new BlurryValue()
                    );
                    BlurryValuePair RNear = new BlurryValuePair(
                        BlurryValues.ContainsKey(ribbons[2].a)?
                            BlurryValues[ribbons[2].a]
                            : new BlurryValue(),
                        BlurryValues.ContainsKey(ribbons[2].b)?
                            BlurryValues[ribbons[2].b]
                            : new BlurryValue()
                    );
                    BlurryValuePair RFar = new BlurryValuePair(
                        BlurryValues.ContainsKey(ribbons[3].a)?
                            BlurryValues[ribbons[3].a]
                            : new BlurryValue(),
                        BlurryValues.ContainsKey(ribbons[3].b)?
                            BlurryValues[ribbons[3].b]
                            : new BlurryValue()
                    );

                    double? LFarHighMin = LFar.HighMin();
                    double? LFarLowMax = LFar.LowMax();

                    double? LNearHighMax = LNear.HighMax();
                    double? LNearLowMin = LNear.LowMin();

                    double? CenterHighMin = centerPair.HighMin();
                    double? CenterLowMax = centerPair.LowMax();

                    double? RNearHighMax = RNear.HighMax();
                    double? RNearLowMin = RNear.LowMin();

                    double? RFarHighMin = RFar.HighMin();
                    double? RFarLowMax = RFar.LowMax();

                    if(CenterHighMin != null && LNearHighMax != null){
                        if(CenterHighMin > LNearHighMax){
                            ConstrainAndMaybePropagate(ribbons[0].a, null, LNearHighMax, true);
                            ConstrainAndMaybePropagate(ribbons[0].b, null, LNearHighMax, true);
                        }
                    }

                    if(CenterLowMax != null && LNearLowMin != null){
                        if(CenterLowMax < LNearLowMin){
                            ConstrainAndMaybePropagate(ribbons[0].a, LNearLowMin, null, true);
                            ConstrainAndMaybePropagate(ribbons[0].b, LNearLowMin, null, true);
                        }
                    }

                    if(CenterHighMin != null && RNearHighMax != null){
                        if(CenterHighMin > RNearHighMax){
                            ConstrainAndMaybePropagate(ribbons[3].a, null, RNearHighMax, true);
                            ConstrainAndMaybePropagate(ribbons[3].b, null, RNearHighMax, true);
                        }
                    }

                    if(CenterLowMax != null && RNearLowMin != null){
                        if(CenterLowMax < RNearLowMin){
                            ConstrainAndMaybePropagate(ribbons[3].a, RNearLowMin, null, true);
                            ConstrainAndMaybePropagate(ribbons[3].b, RNearLowMin, null, true);
                        }
                    }


                    //How should near change, given center and far?
                    //This part is PLAINLY WRONG for now. Too tight and too loose!
                    // double? CenterHighMax = centerPair.HighMax();
                    // double? CenterLowMin = centerPair.LowMin();

                    // double? LFarHighMax = LFar.HighMax();
                    // double? LFarLowMin = LFar.LowMin();

                    // double? RFarHighMax = RFar.HighMax();
                    // double? RFarLowMin = RFar.LowMin();

                    // ConstrainAndMaybePropagate(ribbons[1].a, null, BlurryValue.NullOrMax(CenterHighMax, LFarHighMax));
                    // ConstrainAndMaybePropagate(ribbons[1].b, null, BlurryValue.NullOrMax(CenterHighMax, LFarHighMax));

                    // ConstrainAndMaybePropagate(ribbons[1].a, BlurryValue.NullOrMin(CenterLowMin, LFarLowMin), null);
                    // ConstrainAndMaybePropagate(ribbons[1].b, BlurryValue.NullOrMin(CenterLowMin, LFarLowMin), null);

                    // ConstrainAndMaybePropagate(ribbons[2].a, null, BlurryValue.NullOrMax(CenterHighMax, RFarHighMax));
                    // ConstrainAndMaybePropagate(ribbons[2].b, null, BlurryValue.NullOrMax(CenterHighMax, RFarHighMax));

                    // ConstrainAndMaybePropagate(ribbons[2].a, BlurryValue.NullOrMin(CenterLowMin, RFarLowMin), null);
                    // ConstrainAndMaybePropagate(ribbons[2].b, BlurryValue.NullOrMin(CenterLowMin, RFarLowMin), null);
                }

                // if(vGroups[i].Sides != null){
                //     //handle sides (for ridges aka saddles)
                //     //kinda just hacking over here rn!

                //     //For all but the primary vertex,
                //     //creates a temporary (null, null) if not present in grid

                //     AxialPositionPair[] sides = vGroups[i].Sides.Select(
                //         relativePair => new AxialPositionPair(
                //             relativePair.a + position, relativePair.b + position
                //         )
                //     ).ToArray();

                //     BlurryValuePair centerPair = new BlurryValuePair(
                //         primaryV,
                //         BlurryValues.ContainsKey(secondaryVPosition)?
                //             BlurryValues[secondaryVPosition]
                //             : new BlurryValue()
                //     );
                //     BlurryValuePair leftPair = new BlurryValuePair(
                //         BlurryValues.ContainsKey(sides[0].a)?
                //             BlurryValues[sides[0].a]
                //             : new BlurryValue(),
                //         BlurryValues.ContainsKey(sides[0].b)?
                //             BlurryValues[sides[0].b]
                //             : new BlurryValue()
                //     );
                //     BlurryValuePair rightPair = new BlurryValuePair(
                //         BlurryValues.ContainsKey(sides[1].a)?
                //             BlurryValues[sides[1].a]
                //             : new BlurryValue(),
                //         BlurryValues.ContainsKey(sides[1].b)?
                //             BlurryValues[sides[1].b]
                //             : new BlurryValue()
                //     );

                //     double? centerHighMax = centerPair.HighMax();
                //     double? centerLowMin = centerPair.LowMin();
                //     double? leftHighMin = leftPair.HighMin();
                //     double? leftLowMax = leftPair.LowMax();
                //     double? rightHighMin = rightPair.HighMin();
                //     double? rightLowMax = rightPair.LowMax();

                //     if(leftHighMin != null && centerHighMax != null){
                //         if(leftHighMin > centerHighMax){
                //             ConstrainAndMaybePropagate(sides[1].a, null, centerHighMax);
                //             ConstrainAndMaybePropagate(sides[1].b, null, centerHighMax);
                //         }
                //     }

                //     if(leftLowMax != null && centerLowMin != null){
                //         if(leftLowMax < centerLowMin){
                //             ConstrainAndMaybePropagate(sides[1].a, centerLowMin, null);
                //             ConstrainAndMaybePropagate(sides[1].b, centerLowMin, null);
                //         }
                //     }

                //     if(rightHighMin != null && centerHighMax != null){
                //         if(rightHighMin > centerHighMax){
                //             ConstrainAndMaybePropagate(sides[0].a, null, centerHighMax);
                //             ConstrainAndMaybePropagate(sides[0].b, null, centerHighMax);
                //         }
                //     }

                //     if(rightLowMax != null && centerLowMin != null){
                //         if(rightLowMax < centerLowMin){
                //             ConstrainAndMaybePropagate(sides[0].a, centerLowMin, null);
                //             ConstrainAndMaybePropagate(sides[0].b, centerLowMin, null);
                //         }
                //     }
                // }
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

    public record BlurryValuePair (BlurryValue A, BlurryValue B){
        public double? LowMin(){
            return BlurryValue.NullOrMin(A.Min, B.Min);
        }

        public double? HighMax(){
            return BlurryValue.NullOrMax(A.Max, B.Max);
        }

        public double? HighMin(){
            if(A.Min == null) return B.Min;
            if(B.Min == null) return A.Min;
            return Math.Max(A.Min.Value, B.Min.Value);
        }

        public double? LowMax(){
            if(A.Max == null) return B.Max;
            if(B.Max == null) return A.Max;
            return Math.Min(A.Max.Value, B.Max.Value);
        }
    }
}
