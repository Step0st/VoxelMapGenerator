using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoxelTileRandomizer
{
    private VoxelTileRandomizer()
    {
    }

    public static VoxelTileRandomizer Instance { get; } = new VoxelTileRandomizer();

    public VoxelTile GetRandomTile(List<VoxelTile> availableTiles)
    {
        List<float> chances = new List<float>();
        for (int i = 0; i < availableTiles.Count; i++)
        {
            chances.Add(availableTiles[i].weight);
        }

        float value = Random.Range(0, chances.Sum());
        float sum = 0;

        for (int i = 0; i < chances.Count; i++)
        {
            sum += chances[i];
            if (value<sum)
            {
                return availableTiles[i];
            }
        }
        return availableTiles[availableTiles.Count - 1];
    }
}
