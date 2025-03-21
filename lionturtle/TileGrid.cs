using System;
using lionturtle;
using System.Numerics;

namespace lionturtle;

public class TileGrid
{
	public Dictionary<AxialPosition, Tile> Tiles;

	public SlotGrid SGrid;

    private PerlinNoise perlin;

	public TileGrid(SlotGrid sGrid)
	{
		Tiles = new();
        SGrid = sGrid;

        AxialPosition initialTilePosition = new AxialPosition(0, 0);
        //Module initialModule = SGrid.CollapseSlotToModule(initialTilePosition);
        Module selectedModule = DataGenerator.allModules.First();
        sGrid.Slots.Add(initialTilePosition, new Slot(DataGenerator.allModules.ToHashSet()));
        SGrid.CollapseSlotToModule(initialTilePosition, selectedModule);
        Tiles[initialTilePosition] = new Tile(
            selectedModule.GetRelativeVertexHeights(),
            0
        );

        Random rng = new Random();
        int seed = rng.Next(0, 10000);
        //perlin = new PerlinNoise(1337);
        perlin = new PerlinNoise(seed);
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
        //if (!SGrid.Slots.ContainsKey(tPosition))
        //{
        //    SGrid.Slots.Add(tPosition, new Slot(DataGenerator.allModules.ToHashSet()));
        //}

        //List<Module> candidates = SGrid.Slots[tPosition].modules.ToList();
        List<Module> candidates = SGrid.GetCandidates(tPosition);
        Module selectedModule = candidates[0];
        float minError = GetCandidateError(tPosition, candidates[0]);
        for (int i = 1; i < candidates.Count; i++)
        {
            float candidateError = GetCandidateError(tPosition, candidates[i]);
            if (candidateError < minError)
            {
                selectedModule = candidates[i];
                minError = candidateError;
            }
        }

        SGrid.CollapseSlotToModule(tPosition, selectedModule);

        Tiles[tPosition] = new Tile(
            selectedModule.GetRelativeVertexHeights(),
            InferTileHeight(tPosition, selectedModule)
        );

        return Tiles[tPosition];
    }

    public float heuristic(AxialPosition position)
    {
        Vector2 cartesian = GridUtilities.AxialPositionToVec2(position);
        double sumOfNoise = 0;
        sumOfNoise += perlin.Noise(cartesian.X * 0.002f, cartesian.Y * 0.002f) * 24.0f;

        sumOfNoise += perlin.Noise(cartesian.X * 0.004f, cartesian.Y * 0.004f) * 12.0f;
        //sumOfNoise += perlin.Noise(cartesian.X * 0.1f, cartesian.Y * 0.1f) * 16.0f;
        sumOfNoise += perlin.Noise(cartesian.X * 0.01f, cartesian.Y * 0.01) * 8.0f;
        //sumOfNoise += perlin.Noise(cartesian.X * 0.0125f, cartesian.Y * 0.0125f) * 2.0f;
        sumOfNoise += perlin.Noise(cartesian.X * 0.05f, cartesian.Y * 0.05f) * 4.0f;
        sumOfNoise += perlin.Noise(cartesian.X * 2.0f, cartesian.Y * 2.0f) * 2.0f;

        return (float)sumOfNoise - (24 + 12 + 8 + 4 + 2f)/2.0f;
    }

    public float GetCandidateError(AxialPosition position, Module module)
    {
        var candidateBaseHeight = InferTileHeight(position, module);
        var candidateRelativeVHeights = module.GetRelativeVertexHeights();

        float totalError = 0.0f;
        for(int direction = 0; direction < 6; direction++)
        {
            float totalVertexHeight = candidateRelativeVHeights[direction] + candidateBaseHeight;
            totalError += Math.Abs(heuristic(position) - totalVertexHeight);
        }

        return totalError;
    }

    public int InferTileHeight(AxialPosition position, Module homeModule)
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

        //Module homeModule = SGrid.Slots[position].modules.First();
        Module neighborModule = SGrid.Slots[neighborPosition].modules.First();

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