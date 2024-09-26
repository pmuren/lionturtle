using System;
using System.Diagnostics;

namespace lionturtle;

public class SlotGrid
{
	public Dictionary<AxialPosition, Slot> Slots = new Dictionary<AxialPosition, Slot>();
    public Dictionary<AxialPosition, float> TileHeights = new Dictionary<AxialPosition, float>();

	Queue<(Module, AxialPosition)> ModuleRemovalQueue = new Queue<(Module, AxialPosition)>();

	int numRings = 160;

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

}
