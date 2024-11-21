using System;
using lionturtle;

namespace lionturtle;

public class TileGrid
{
	public Dictionary<AxialPosition, Tile> Tiles;

	public SlotGrid sGrid;

	public TileGrid()
	{
		Tiles = new();
		sGrid = new SlotGrid();

        AxialPosition initialTilePosition = new AxialPosition(0, 0);
        Module initialModule = sGrid.TryCollapseAndHandleQueue(initialTilePosition);
        Tiles[initialTilePosition] = new Tile(
            initialModule.GetRelativeVertexHeights(),
            0
        );
    }

    public void GrowDendritic()
    {
        AxialPosition newTilePosition;
        while(true)
        {
            //get a random Tile from Tiles
            Random rng = new Random();
            List<AxialPosition> existingTilePositions = Tiles.Keys.ToList();
            AxialPosition randomExisting = existingTilePositions[rng.Next(0, existingTilePositions.Count)];
            //get a random neighbor
            AxialPosition randomDirection = Constants.axialDirections[rng.Next(0, 6)];
            AxialPosition candidatePosition = randomExisting + randomDirection;

            if(!Tiles.ContainsKey(candidatePosition))
            {
                newTilePosition = candidatePosition;
                break;
            }
        }

        AddTile(newTilePosition);
    }

    public Tile AddTile(AxialPosition tPosition)
    {
        if (!sGrid.Slots.ContainsKey(tPosition))
        {
            sGrid.ExpandToInclude(tPosition);
            sGrid.ValidateAllSlots();
        }

        Module selectedModule = sGrid.TryCollapseAndHandleQueue(tPosition);

        Tiles[tPosition] = new Tile(
            selectedModule.GetRelativeVertexHeights(),
            InferTileHeight(tPosition)
        );

        return Tiles[tPosition];
    }

    public int InferTileHeight(AxialPosition position)
    {
        int direction = 0;
        AxialPosition neighborPosition = new AxialPosition(0, 0);
        while (direction < 6)
        {
            neighborPosition = position + Constants.axialDirections[direction];
            if (Tiles.ContainsKey(neighborPosition))
            {
                break;
            }

            direction++;
        }
        if (direction > 5) throw new Exception("Need a collapsed neighbor to infer my height");

        Module homeModule = sGrid.Slots[position].modules.First();
        Module neighborModule = sGrid.Slots[neighborPosition].modules.First();

        Connector[] homeConnectors = homeModule.GetConnectorArray();
        Connector[] neighborConnectors = neighborModule.GetConnectorArray();

        float neighborBonusHeight = neighborModule == DataGenerator.allModules[0] ? 0.0f : 0.5f;

        float stepHeight = GetStepHeight(neighborConnectors[(direction + 3) % 6], homeConnectors[direction]);
        return (int)Math.Floor(
            (Tiles[neighborPosition].BaseHeight + neighborBonusHeight + stepHeight)
        );
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