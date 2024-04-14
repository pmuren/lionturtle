using System;
namespace lionturtle
{
	public class VirtualVertex
	{
		public AxialPosition position;
		public double? min;
		public double? max;

		public VirtualVertex(AxialPosition inPosition, double? inMin, double? inMax)
		{
			position = inPosition;
			min = inMin;
			max = inMax;

            if (min != null && max != null && min > max)
            {
                throw new InvalidOperationException("Conflicting min/max constraints");
            }
        }

		public bool Constrain(double? newMin, double? newMax)
		{
			bool changed;
			double? initMin = min;
			double? initMax = max;

			if ( (newMin != null && newMax != null && newMin > newMax)
			||   (newMin != null && max != null && newMin > max)
			||   (newMax != null && min != null && newMax < min)
			)
			{
				Console.WriteLine($"conflict for vertex at {position}, old min/max: {min}/{max}, new min/max: {newMin}/{newMax}");
				throw new InvalidOperationException("Conflicting min/max constraints");
			}

			if (min == null) min = newMin;
			else if (newMin != null && newMin > min) min = newMin;

			if (max == null) max = newMax;
            else if (newMax != null && newMax < max) max = newMax;

			changed = (min != initMin || max != initMax);
			return changed;
		}

        public double Resolve(double heuristic)
        {
			if(min == null || min < heuristic)
			{
				if(max != null)
				{
					if(max > heuristic)
					{
                        min = heuristic;
                        max = heuristic;
                    }
					else
					{
						min = max;
					}
				}
				else
				{
                    min = heuristic;
                    max = heuristic;
                }
			}

            if (max == null || max > heuristic)
            {
				if(min != null)
				{
					if(min < heuristic)
					{
						min = heuristic;
						max = heuristic;
					}
					else
					{
						max = min;
					}
				}
				else
				{
                    min = heuristic;
                    max = heuristic;
                }
            }

			if (min.HasValue && max.HasValue)
			{
				double resolvedValue;
				if(min != max)
				{
                    throw new InvalidOperationException("Failed to resolve, min and max are not equal");
				}
				else
				{
					resolvedValue = (double)max;
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

