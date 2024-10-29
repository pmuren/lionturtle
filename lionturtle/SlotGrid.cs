using System;
using System.Diagnostics;

namespace lionturtle;

public class SlotGrid
{
	public Dictionary<AxialPosition, Slot> Slots = new Dictionary<AxialPosition, Slot>();
    public Dictionary<AxialPosition, float> TileHeights = new Dictionary<AxialPosition, float>();

	Queue<(Module, AxialPosition)> ModuleRemovalQueue = new Queue<(Module, AxialPosition)>();

	int numRings = 80;

    public SlotGrid()
	{
		Slots = new Dictionary<AxialPosition, Slot>();

        AxialPosition position = new(0, 0);
        Slots[position] = new Slot(DataGenerator.allModules);
        for (int i = 0; i < numRings; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                for (int k = 0; k < i; k++)
                {
                    position += Constants.axial_directions[j];
                    Slots[position] = new Slot(DataGenerator.allModules);
                }
            }
            position += Constants.axial_directions[4];
        }
    }

	public void CollapseEverything()
	{
        AxialPosition position = new(0, 0);
        TryCollapseAndHandleQueue(position);

        if (Slots[position].modules.First() == DataGenerator.allModules.First())
            TileHeights[position] = 0.0f;
        else
            TileHeights[position] = 0.5f;

        for (int i = 0; i < numRings; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                for (int k = 0; k < i; k++)
                {
                    position += Constants.axial_directions[j];

                    //var inwardNeighborPosition = position + Constants.axial_directions[(j + 2) % 6];

                    TryCollapseAndHandleQueue(position);
                    TileHeights[position] = InferTileHeight(position);
                }
            }
            position += Constants.axial_directions[4];
        }
	}

    public void TryCollapseAndHandleQueue(AxialPosition position)
    {
        if (Slots[position].modules.Count > 0)
        {
            CollapseSlotAtPosition(position);
            while (ModuleRemovalQueue.Count > 0)
            {
                var removalData = ModuleRemovalQueue.Dequeue();
                if (Slots[removalData.Item2].modules.Contains(removalData.Item1)) // found the bug here
                {
                    RemoveModuleAtPosition(removalData.Item1, removalData.Item2);
                }
            }
        }
    }

	public void CollapseSlotAtPosition(AxialPosition position)
	{
        Random rng = new Random();
        var randomIndex = rng.Next(0, Slots[position].modules.Count);


        Module selectedModule = Slots[position].modules.ToArray<Module>()[randomIndex]; //TODO: decide on selection criteria
		foreach (Module module in Slots[position].modules)
		{
            if (module != selectedModule) ModuleRemovalQueue.Enqueue((module, position));
		}
	}

    public float InferTileHeight(AxialPosition position)
    {
        int direction = 0;
        AxialPosition neighborPosition = new AxialPosition(0,0);
        while (direction < 6){
            neighborPosition = position + Constants.axial_directions[direction];
            if (TileHeights.ContainsKey(neighborPosition))
            {
                break;
            }

            direction++;
        }
        if (direction > 5) throw new Exception("Need a collapsed neighbor to infer my height");

        Module homeModule = Slots[position].modules.First();
        Module neighborModule = Slots[neighborPosition].modules.First();

        Connector[] homeConnectors = homeModule.GetConnectorArray();
        Connector[] neighborConnectors = neighborModule.GetConnectorArray();

        float stepHeight =  GetStepHeight(neighborConnectors[(direction+3)%6], homeConnectors[direction]);
        return TileHeights[neighborPosition] + stepHeight;
    }

    public void RemoveModuleAtPosition(Module module, AxialPosition position)
	{
        //if (!Slots.ContainsKey(position)) return; //TODO: handle bad positions

		var newlyUnsupportedModules = Slots[position].RemoveModule(module);
		for(int direction = 0; direction < 6; direction++)
		{
			AxialPosition neighboringPosition = position + Constants.axial_directions[direction];

			foreach(Module unsupportedModule in newlyUnsupportedModules[direction])
			{
                if (Slots.ContainsKey(neighboringPosition))
                {
                    ModuleRemovalQueue.Enqueue((unsupportedModule, neighboringPosition));
                }
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
                AxialPosition neighboringPosition = position + Constants.axial_directions[direction];
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

    public Dictionary<AxialPosition, int[]> ToHexGrid()
    {
        var hexes = new Dictionary<AxialPosition, int[]>();
        float tileHeight = 0.0f;

        AxialPosition axialPosition = new(0, 0);
        if (Slots[axialPosition].modules.First() != DataGenerator.allModules.First())
        {
            tileHeight += 0.5f;
        }

        int[] relativeHeights = Slots[axialPosition].modules.First().GetRelativeVertexHeights();
        hexes.Add(axialPosition, relativeHeights.Select(
            i => (int)Math.Floor(i + tileHeight)).ToArray());

        for (int i = 0; i < numRings; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                for (int k = 0; k < i; k++)
                {
                    axialPosition += Constants.axial_directions[j];

                    relativeHeights = Slots[axialPosition].modules.First().GetRelativeVertexHeights();
                    hexes.Add(axialPosition, relativeHeights.Select(
                        i => (int)Math.Floor(i + tileHeight)).ToArray());

                    //var inwardNeighborPosition = axialPosition + Constants.axial_directions[(j + 2) % 6];
                }
            }
            axialPosition += Constants.axial_directions[4];
        }

        return hexes;
    }

    public float GetStepHeight(Connector fromConnector, Connector toConnector)
    {
        float stepHeight = 0.0f;
        if (fromConnector.Charge == 1) stepHeight += 0.5f;
        else if (fromConnector.Charge == -1) stepHeight -= 0.5f;
        if (toConnector.Charge == 1) stepHeight -= 0.5f;
        else if (toConnector.Charge == -1) stepHeight += 0.5f;
        return stepHeight;
    }

    public VertexType[] GetEdgeVertexTypesForHex(AxialPosition hPosition)
    {
        var vertexTypes = new VertexType[6];
        for (int i = 0; i < 6; i++)
        {
            var neighborDirection = Constants.axial_directions[i];
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

        // the rest of the owl, some flats -> ?

        //for each of 3 modules
        // if I'm flat
        // if I step up in to any neighbors
        // it's a foot
        // if I step down into any neighbors
        // it's a crest!
        // and if it's both
        // it's a crestfoot!

        for(int i = 0; i < 3; i++)
        {
            int directionToRight = Array.IndexOf(Constants.axial_directions, hexPositions[(i + 1) % 3] - hexPositions[i] );
            //Connector homeRightConnector = modules[i].GetConnectorArray()[directionToRight];
            Connector rightLeftConnector = modules[(i + 1) % 3].GetConnectorArray()[(directionToRight + 3) % 6];

            int directionToLeft = Array.IndexOf(Constants.axial_directions, hexPositions[(i + 2) % 3] - hexPositions[i]);
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
}
