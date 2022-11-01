using System;
using System.Collections.Generic;
using UnityEngine;

public class VoxelTileRotator : MonoBehaviour
{

    public List<VoxelTile> MakeRotatedTiles(List<VoxelTile> tilePrefabs)
    {
        int countBeforeAdding = tilePrefabs.Count;
        for (int i = 0; i < countBeforeAdding; i++)
        {
            VoxelTile clone;
            switch (tilePrefabs[i].Rotation)
            {
                case VoxelTile.RotationType.SingleRotation:
                    break;
                
                case VoxelTile.RotationType.TwoRotations:
                    tilePrefabs[i].weight /= 2;
                    if (tilePrefabs[i].weight <= 0) tilePrefabs[i].weight = 1;
                    
                    clone = Instantiate(tilePrefabs[i], tilePrefabs[i].transform.position + Vector3.forward, Quaternion.identity);
                    clone.Rotate90();
                    tilePrefabs.Add(clone);
                    break;
                
                case VoxelTile.RotationType.FourRotations:
                    tilePrefabs[i].weight /= 4;
                    if (tilePrefabs[i].weight <= 0) tilePrefabs[i].weight = 1;
                    
                    clone = Instantiate(tilePrefabs[i], tilePrefabs[i].transform.position + Vector3.forward, Quaternion.identity);
                    clone.Rotate90();
                    tilePrefabs.Add(clone);
                    
                    clone = Instantiate(tilePrefabs[i], tilePrefabs[i].transform.position + Vector3.forward*2, Quaternion.identity);
                    clone.Rotate90();
                    clone.Rotate90();
                    tilePrefabs.Add(clone);
                    
                    clone = Instantiate(tilePrefabs[i], tilePrefabs[i].transform.position + Vector3.forward*3, Quaternion.identity);
                    clone.Rotate90();
                    clone.Rotate90();
                    clone.Rotate90();
                    tilePrefabs.Add(clone);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return tilePrefabs;
    }
    
    
}
