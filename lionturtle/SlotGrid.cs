using System;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace lionturtle;

public class SlotGrid
{
	public Dictionary<AxialPosition, Slot> Slots = new Dictionary<AxialPosition, Slot>();
    BoundingHex bounds;

    Queue<(Module, AxialPosition)> ModuleRemovalQueue = new Queue<(Module, AxialPosition)>();

    public SlotGrid()
	{
		Slots = new Dictionary<AxialPosition, Slot>();
        bounds = new BoundingHex();

        AxialPosition initialSlotPosition = new AxialPosition(0, 0);
        Slots[initialSlotPosition] = new Slot( DataGenerator.allModules );
    }

    public List<Module> GatherSupportedModules(AxialPosition homePosition){
        List<Module> supportedModules = new();
        Dictionary<Module, int> supportCount = new();
        int numNeighborSlots = 0;

        for(int direction = 0; direction < 6; direction++)
        {
            AxialPosition neighborDirection = Constants.axialDirections[direction];
            AxialPosition neighborPosition = homePosition + neighborDirection;
            if (Slots.ContainsKey(neighborPosition))
            {
                Slot neighbor = Slots[neighborPosition];
                numNeighborSlots++;
                int reverseDirection = (direction + 3) % 6;
                foreach(Module module in neighbor.supportedModules[reverseDirection].Keys)
                {
                    if (!supportCount.ContainsKey(module))
                    {
                        supportCount.Add(module, 0);
                    }
                    supportCount[module] += 1;
                }
            }
        }

        foreach (Module module in supportCount.Keys)
        {
            if (supportCount[module] == numNeighborSlots)
            {
                supportedModules.Add(module);
            }
        }

        return supportedModules;
    }

    public void HandleQueue(){
        while (ModuleRemovalQueue.Count > 0)
        {
            var (moduleToRemove, slotPosition) = ModuleRemovalQueue.Dequeue();
            if(!Slots.ContainsKey(slotPosition)) continue;
            if(!Slots[slotPosition].modules.Contains(moduleToRemove)) continue;
            RemoveModuleAndPropagate(moduleToRemove, slotPosition);
        }
    }

    public Module TryCollapseAndHandleQueue(AxialPosition position)
    {
        Module resultantModule = CollapseSlotAtPosition(position);
        HandleQueue();
        return resultantModule;
    }

	private Module CollapseSlotAtPosition(AxialPosition position)
	{
        Random rng = new Random();
        var randomIndex = rng.Next(0, Slots[position].modules.Count);

        Module selectedModule = Slots[position].modules.ToArray()[randomIndex]; //TODO: decide on selection criteria
		foreach (Module module in Slots[position].modules)
		{
            if (module != selectedModule) ModuleRemovalQueue.Enqueue((module, position));
		}

        return selectedModule;
	}

    private void RemoveModuleAndPropagate(Module module, AxialPosition position)
	{
        if (!Slots.ContainsKey(position)) throw new ArgumentException("No Slot at position " + position, nameof(position));

		var newlyUnsupportedModules = Slots[position].RemoveModule(module);

        if (Slots[position].modules.Count == 0)
            throw new Exception("Oof. I removed its last module");

		for(int direction = 0; direction < 6; direction++)
		{
			AxialPosition neighboringPosition = position + Constants.axialDirections[direction];
			foreach(Module unsupportedModule in newlyUnsupportedModules[direction])
			{
                ModuleRemovalQueue.Enqueue((unsupportedModule, neighboringPosition));
            }
		}
	}

    public bool ValidateAllSlots()
    {
        foreach (AxialPosition position in Slots.Keys)
        {
            Slot slot = Slots[position];
            for (int direction = 0; direction < 6; direction++)
            {
                AxialPosition neighboringPosition = position + Constants.axialDirections[direction];
                if (Slots.ContainsKey(neighboringPosition))
                {
                    foreach (Module neighborModule in Slots[neighboringPosition].modules)
                    {
                        if (!slot.supportedModules[direction].ContainsKey(neighborModule))
                        {
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }

    public void SettleNeighborsAndPropagate(AxialPosition position)
    {
        Slot slot = Slots[position];
        for (int direction = 0; direction < 6; direction++)
        {
            AxialPosition neighboringPosition = position + Constants.axialDirections[direction];
            if (Slots.ContainsKey(neighboringPosition))
            {
                foreach (Module neighborModule in Slots[neighboringPosition].modules)
                {
                    if (!slot.supportedModules[direction].ContainsKey(neighborModule))
                    {
                        RemoveModuleAndPropagate(neighborModule, neighboringPosition);
                    }
                }
            }
        }
    }

    public List<Module>[] GatherUnsupportedNeighborModules(AxialPosition position)
    {
        List<Module>[] unsupportedNeighborModules = new List<Module>[6];

        Slot slot = Slots[position];
        for (int direction = 0; direction < 6; direction++)
        {
            AxialPosition neighboringPosition = position + Constants.axialDirections[direction];
            if (Slots.ContainsKey(neighboringPosition))
            {
                foreach (Module neighborModule in Slots[neighboringPosition].modules)
                {
                    if (!slot.supportedModules[direction].ContainsKey(neighborModule))
                    {
                        unsupportedNeighborModules[direction].Add(neighborModule);
                    }
                }
            }
        }
        return unsupportedNeighborModules;
    }

    public VertexType[] GetEdgeVertexTypesForHex(AxialPosition hPosition)
    {
        var vertexTypes = new VertexType[6];
        for (int i = 0; i < 6; i++)
        {
            var neighborDirection = Constants.axialDirections[i];
            var neighborPosition = hPosition + neighborDirection;

            if (!Slots.ContainsKey(neighborPosition))
            {
                vertexTypes[i] = VertexType.Unknown;
                continue;
            }

            var homeModule = Slots[hPosition].modules.First();
            var remoteModule = Slots[neighborPosition].modules.First();

            vertexTypes[i] = VertexType.Slope;

            if (homeModule == DataGenerator.allModules[0] && remoteModule == DataGenerator.allModules[0])
            {
                vertexTypes[i] = VertexType.Flat;
                continue;
            }
            if (homeModule == DataGenerator.allModules[0]) // home is flat
            {
                if (remoteModule.GetConnectorArray()[(i+3)%6].Charge == 1)
                    vertexTypes[i] = VertexType.Crest;
                else if (remoteModule.GetConnectorArray()[(i + 3) % 6].Charge == -1)
                    vertexTypes[i] = VertexType.Foot;
                continue;
            }
            if(remoteModule == DataGenerator.allModules[0]) // remote is flat
            {
                if (homeModule.GetConnectorArray()[i].Charge == 1)
                    vertexTypes[i] = VertexType.Crest;
                else if (homeModule.GetConnectorArray()[i].Charge == -1)
                    vertexTypes[i] = VertexType.Foot;
                continue;
            }
        }
        return vertexTypes;
    }

    public VertexType[] GetCornerVertexTypesForHex(AxialPosition hPosition)
    {
        var vertexTypes = new VertexType[6];
        for(int i = 0; i < 6; i++)
        {
            var vPosition = GridUtilities.GetVertexPositionForHexV(hPosition, i);
            vertexTypes[i] = GetCornerVertexType(vPosition);
        }
        return vertexTypes;
    }

    public VertexType GetCornerVertexType(AxialPosition position)
    {
        AxialPosition[] hexPositions = new AxialPosition[3];
        if (GridUtilities.VertexPointsUp(position))
        {
            //(+2, -1), (-1, -1), (-1, +2)
            hexPositions[0] = (position + new AxialPosition(+2, -1)) / 3;
            hexPositions[1] = (position + new AxialPosition(-1, -1)) / 3;
            hexPositions[2] = (position + new AxialPosition(-1, +2)) / 3;
        }
        else
        {
            //(+1, -2), (-2, +1), (+1, +1)
            hexPositions[0] = (position + new AxialPosition(+1, -2)) / 3;
            hexPositions[1] = (position + new AxialPosition(-2, +1)) / 3;
            hexPositions[2] = (position + new AxialPosition(+1, +1)) / 3;
        }

        foreach(AxialPosition hexPosition in hexPositions)
        {
            if (!Slots.ContainsKey(hexPosition)) return VertexType.Unknown;
        }

        Module[] modules = new Module[]
        {
            Slots[hexPositions[0]].modules.First(),
            Slots[hexPositions[1]].modules.First(),
            Slots[hexPositions[2]].modules.First(),
        };

        if (modules[0] == DataGenerator.allModules[0]
            && modules[1] == DataGenerator.allModules[0]
            && modules[2] == DataGenerator.allModules[0])
        {
            return VertexType.Flat; //all flats -> Flat
        }

        if (modules[0] != DataGenerator.allModules[0]
            && modules[1] != DataGenerator.allModules[0]
            && modules[2] != DataGenerator.allModules[0])
        {
            return VertexType.Slope; //no flats -> Slope
        }

        VertexType vertexType = VertexType.Unknown;

        for(int i = 0; i < 3; i++)
        {
            int directionToRight = Array.IndexOf(Constants.axialDirections, hexPositions[(i + 1) % 3] - hexPositions[i] );
            //Connector homeRightConnector = modules[i].GetConnectorArray()[directionToRight];
            Connector rightLeftConnector = modules[(i + 1) % 3].GetConnectorArray()[(directionToRight + 3) % 6];

            int directionToLeft = Array.IndexOf(Constants.axialDirections, hexPositions[(i + 2) % 3] - hexPositions[i]);
            //Connector homeLeftConnector = modules[i].GetConnectorArray()[directionToLeft];
            Connector leftRightConnector = modules[(i + 2) % 3].GetConnectorArray()[(directionToLeft + 3) % 6];

            bool crest = false;
            bool foot = false;

            if(rightLeftConnector.Charge == 1 || leftRightConnector.Charge == 1)
            {
                crest = true;
                vertexType = VertexType.Crest;
            }

            if(rightLeftConnector.Charge == -1 || leftRightConnector.Charge == -1)
            {
                foot = true;
                vertexType = VertexType.Foot;
            }

            if(crest && foot)
            {
                return VertexType.FootCrest;
            }
        }

        return vertexType;
    }

    /* Ensures the arc starts and ends at the tips */
    public List<int> NormalizeArc(List<int> unsorted)
    {
        for (int i = 1; i < unsorted.Count; i++)
        {
            if ((unsorted[i] - unsorted[i - 1] + 6) % 6 != 1)
                return unsorted.GetRange(i, unsorted.Count - i).Concat(
                    unsorted.GetRange(0, i)).ToList();
        }
        return unsorted;
    }

    public void ExpandToInclude(AxialPosition hPosition)
    {
        var hPositionS = 0 - hPosition.Q - hPosition.R;
        BoundingHex newBounds = new BoundingHex(new int[]{
            Math.Max(bounds.Extents[(int)Extent.QMax], hPosition.Q),
            Math.Min(bounds.Extents[(int)Extent.RMin], hPosition.R),
            Math.Max(bounds.Extents[(int)Extent.SMax], hPositionS),
            Math.Min(bounds.Extents[(int)Extent.QMin], hPosition.Q),
            Math.Max(bounds.Extents[(int)Extent.RMax], hPosition.R),
            Math.Min(bounds.Extents[(int)Extent.SMin], hPositionS)
        });
        ExpandToDesiredBounds(newBounds);
    }

    private void ExpandToDesiredBounds(BoundingHex boundsDesired)
    {
        while(!bounds.Contains(boundsDesired)) //consider this condition
        {
            List<int> sidesToExpand = FigureSidesToExpand(boundsDesired);
            PushBoundariesByOne(sidesToExpand);
        }
    }

    private void PushBoundariesByOne(List<int> sidesToNudge)
    {
        sidesToNudge = NormalizeArc(sidesToNudge);

        int initialSide = sidesToNudge.First();
        int headDirection = (initialSide + 2) % 6;

        bounds.Nudge(sidesToNudge); //expand boundaries before fill
        AxialPosition headPosition = bounds.GetCorner(initialSide);

        while (true)
        {
            List<Module> supportedModules = GatherSupportedModules(headPosition);
            Slots.Add(headPosition, new Slot(supportedModules));
            SettleNeighborsAndPropagate(headPosition);

            //maybe adjust heading
            while (!bounds.InBounds(headPosition + Constants.axialDirections[headDirection]))
            {
                headDirection = (headDirection + 1) % 6;
            }

            //maybe stop
            if (Slots.ContainsKey(headPosition + Constants.axialDirections[headDirection]))
            {
                break;
            }

            //advance
            headPosition += Constants.axialDirections[headDirection];
        }
    }

    private List<int> FigureSidesToExpand(BoundingHex desired)
    {
        List<int> affectedDirections = new();
        if (desired.Extents[(int)Extent.QMax] > bounds.Extents[(int)Extent.QMax]) affectedDirections.Add(0);
        if (desired.Extents[(int)Extent.RMin] < bounds.Extents[(int)Extent.RMin]) affectedDirections.Add(1);
        if (desired.Extents[(int)Extent.SMax] > bounds.Extents[(int)Extent.SMax]) affectedDirections.Add(2);
        if (desired.Extents[(int)Extent.QMin] < bounds.Extents[(int)Extent.QMin]) affectedDirections.Add(3);
        if (desired.Extents[(int)Extent.RMax] > bounds.Extents[(int)Extent.RMax]) affectedDirections.Add(4);
        if (desired.Extents[(int)Extent.SMin] < bounds.Extents[(int)Extent.SMin]) affectedDirections.Add(5);
        return affectedDirections;
    }
}

public enum Extent
{
    QMax,
    RMin,
    SMax,
    QMin,
    RMax,
    SMin
}

class BoundingHex //this class is kind of mess. need to figure out a data structure for these sides
{

    public int[] Extents;
    public BoundingHex()
    {
        Extents = new int[] {0, 0, 0, 0, 0, 0};
    }

    public BoundingHex(int[] extents)
    {
        Extents = extents;
    }

    public void ExpandToInclude(AxialPosition hexPosition)
    {
        int hexS = 0 - hexPosition.Q - hexPosition.R;
        if (hexPosition.Q > Extents[(int)Extent.QMax])
            Extents[(int)Extent.QMax] = hexPosition.Q;
        if (hexPosition.R < Extents[(int)Extent.RMin])
            Extents[(int)Extent.RMin] = hexPosition.R;
        if (hexS > Extents[(int)Extent.SMax])
            Extents[(int)Extent.SMax] = hexS;
        if (hexPosition.Q < Extents[(int)Extent.QMin])
            Extents[(int)Extent.QMin] = hexPosition.Q;
        if (hexPosition.R > Extents[(int)Extent.RMax])
            Extents[(int)Extent.RMax] = hexPosition.R;
        if (hexS < Extents[(int)Extent.SMin])
            Extents[(int)Extent.SMin] = hexS;
    }

    public bool InBounds(AxialPosition position)
    {
        int S = 0 - position.Q - position.R;
        if (position.Q > Extents[(int)Extent.QMax])
            return false;
        if (position.R < Extents[(int)Extent.RMin])
            return false;
        if (S > Extents[(int)Extent.SMax])
            return false;
        if (position.Q < Extents[(int)Extent.QMin])
            return false;
        if (position.R > Extents[(int)Extent.RMax])
            return false;
        if (S < Extents[(int)Extent.SMin])
            return false;
        return true;
    }

    public bool Contains(BoundingHex other)
    {
        if (Extents[(int)Extent.QMax] < other.Extents[(int)Extent.QMax])
            return false;
        if (Extents[(int)Extent.RMin] > other.Extents[(int)Extent.RMin])
            return false;
        if (Extents[(int)Extent.SMax] < other.Extents[(int)Extent.SMax])
            return false;
        if (Extents[(int)Extent.QMin] > other.Extents[(int)Extent.QMin])
            return false;
        if (Extents[(int)Extent.RMax] < other.Extents[(int)Extent.RMax])
            return false;
        if (Extents[(int)Extent.SMin] > other.Extents[(int)Extent.SMin])
            return false;
        return true;
    }

    public void Nudge(List<int> sides)
    {
        if (sides.Contains(0)) Extents[(int)Extent.QMax] += 1;
        if (sides.Contains(1)) Extents[(int)Extent.RMin] -= 1;
        if (sides.Contains(2)) Extents[(int)Extent.SMax] += 1;
        if (sides.Contains(3)) Extents[(int)Extent.QMin] -= 1;
        if (sides.Contains(4)) Extents[(int)Extent.RMax] += 1;
        if (sides.Contains(5)) Extents[(int)Extent.SMin] -= 1;
    }

    public AxialPosition GetCorner(int index)
    {
        switch (index)
        {
            case 0:
                return new AxialPosition((Extents[(int)Extent.QMax]), 0 - (Extents[(int)Extent.QMax]) - Extents[(int)Extent.SMin]);
            case 1:
                return new AxialPosition(Extents[(int)Extent.QMax], (Extents[(int)Extent.RMin]));
            case 2:
                return new AxialPosition(0 - Extents[(int)Extent.RMin] - (Extents[(int)Extent.SMax]), Extents[(int)Extent.RMin]);
            case 3:
                return new AxialPosition((Extents[(int)Extent.QMin]), 0 - (Extents[(int)Extent.QMin]) - Extents[(int)Extent.SMax]);
            case 4:
                return new AxialPosition(Extents[(int)Extent.QMin], (Extents[(int)Extent.RMax]));
            case 5:
                return new AxialPosition(0 - Extents[(int)Extent.RMax] - (Extents[(int)Extent.SMin]), Extents[(int)Extent.RMax]);
        }
        throw new ArgumentException("argument must be 0, 1, 2, 3, 4, or 5");
    }
}
