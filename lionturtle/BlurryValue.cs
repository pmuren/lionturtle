using System;
namespace lionturtle
{
	public class BlurryValue : ICloneable
	{
		public double? Min;
		public double? Max;

		public BlurryValue(double? min = null, double? max = null)
		{
			Min = min;
			Max = max;

            if (min != null && max != null && min > max)
            {
                throw new InvalidOperationException("Conflicting min/max constraints");
            }
        }

		public object Clone(){
			return new BlurryValue(Min, Max);
		}
		
		public bool SameAs(BlurryValue other){
			return Min == other.Min && Max == other.Max;
		}

		public bool DifferentFrom(BlurryValue other){
			return !SameAs(other);
		}

		public void Constrain(double? newMin, double? newMax)
		{
			if ( (newMin != null && newMax != null && newMin > newMax)
			||   (newMin != null && Max != null && newMin > Max)
			||   (newMax != null && Min != null && newMax < Min)
			)
			{
				Console.WriteLine($"conflict for blurry value, old Min/Max: {Min}/{Max}, new Min/Max: {newMin}/{newMax}");
				throw new InvalidOperationException("Conflicting Min/Max constraints");
			}

			if (Min == null) Min = newMin;
			else if (newMin != null && newMin > Min) Min = newMin;

			if (Max == null) Max = newMax;
            else if (newMax != null && newMax < Max) Max = newMax;
		}

		public void Pinch(BlurryValue blurryValueA, BlurryValue blurryValueB)
        {
            double? highMax = NullOrMax(blurryValueA.Max, blurryValueB.Max);
            double? lowMin = NullOrMin(blurryValueA.Min, blurryValueB.Min);

			Constrain(lowMin, highMax);
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

        public double Resolve(double heuristic)
        {
			if(Min == null || Min < heuristic)
			{
				if(Max != null)
				{
					if(Max > heuristic)
					{
                        Min = heuristic;
                        Max = heuristic;
                    }
					else
					{
						Min = Max;
					}
				}
				else
				{
                    Min = heuristic;
                    Max = heuristic;
                }
			}

            if (Max == null || Max > heuristic)
            {
				if(Min != null)
				{
					if(Min < heuristic)
					{
						Min = heuristic;
						Max = heuristic;
					}
					else
					{
						Max = Min;
					}
				}
				else
				{
                    Min = heuristic;
                    Max = heuristic;
                }
            }

			if (Min.HasValue && Max.HasValue)
			{
				double resolvedValue;
				if(Min != Max)
				{
                    throw new InvalidOperationException("Failed to resolve, Min and Max are not equal");
				}
				else
				{
					resolvedValue = (double)Max;
					return resolvedValue;
				}
			}
			else
			{
				throw new InvalidOperationException("Either min or max remained null");
			}
        }
    }
}
