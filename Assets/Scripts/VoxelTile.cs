using System;
using UnityEngine;

public class VoxelTile : MonoBehaviour
{
    public float voxelSize = 0.1f;
    public int tileSideVoxels = 8;
    [Range(0, 100)] public int weight = 50;
    public RotationType Rotation;

    public enum RotationType
    {
        SingleRotation,
        TwoRotations,
        FourRotations
    }

    [HideInInspector] public byte[] ColorsRight;
    [HideInInspector] public byte[] ColorsForward;
    [HideInInspector] public byte[] ColorsLeft;
    [HideInInspector] public byte[] ColorsBack;


    public void CalculateSideColors()
    {
        ColorsRight = new byte[tileSideVoxels * tileSideVoxels];
        ColorsForward = new byte[tileSideVoxels * tileSideVoxels];
        ColorsLeft = new byte[tileSideVoxels * tileSideVoxels];
        ColorsBack = new byte[tileSideVoxels * tileSideVoxels];
        
        for (int y = 0; y < tileSideVoxels; y++)
        {
            for (int x = 0; x < tileSideVoxels; x++)
            {
                ColorsRight[y * tileSideVoxels + x] = GetVoxelColor(y, x, Direction.Right);
                ColorsForward[y * tileSideVoxels + x] = GetVoxelColor(y, x, Direction.Forward);
                ColorsLeft[y * tileSideVoxels + x] = GetVoxelColor(y, x, Direction.Left);
                ColorsBack[y * tileSideVoxels + x] = GetVoxelColor(y, x, Direction.Back);
            }
        }
    }

    public void Rotate90()
    {
        transform.Rotate(0,90,0);

        byte[] colorsRightNew = new byte[tileSideVoxels * tileSideVoxels];
        byte[] colorsForwardNew = new byte[tileSideVoxels * tileSideVoxels];
        byte[] colorsLeftNew = new byte[tileSideVoxels * tileSideVoxels];
        byte[] colorsBackNew = new byte[tileSideVoxels * tileSideVoxels];

        for (int layer = 0; layer < tileSideVoxels; layer++)
        {
            for (int offset = 0; offset < tileSideVoxels; offset++)
            {
                colorsRightNew[layer * tileSideVoxels + offset] = ColorsForward[layer * tileSideVoxels + tileSideVoxels - offset - 1];
                colorsForwardNew[layer * tileSideVoxels + offset] = ColorsLeft[layer * tileSideVoxels + offset];
                colorsLeftNew[layer * tileSideVoxels + offset] = ColorsBack[layer * tileSideVoxels + tileSideVoxels - offset - 1];
                colorsBackNew[layer * tileSideVoxels + offset] = ColorsRight[layer * tileSideVoxels + offset];
            }
        }

        ColorsRight = colorsRightNew;
        ColorsForward = colorsForwardNew;
        ColorsLeft = colorsLeftNew;
        ColorsBack = colorsBackNew;
    }
    
    private byte GetVoxelColor(int verticalLayer, int horizontalOffset, Direction direction)
    {
        var meshCollider = GetComponentInChildren<MeshCollider>();

        float vox = voxelSize;
        float halfVox = voxelSize / 2;

        Vector3 rayStart;
        Vector3 rayDir;
        if (direction == Direction.Right)
        {
            rayStart = meshCollider.bounds.min +
                       new Vector3(-halfVox, 0, halfVox + horizontalOffset * vox);
            rayDir = Vector3.right;
        }
        else if (direction == Direction.Forward)
        {
            rayStart = meshCollider.bounds.min +
                       new Vector3(halfVox + horizontalOffset * vox, 0, -halfVox);
            rayDir = Vector3.forward;
        }
        else if (direction == Direction.Left)
        {
            rayStart = meshCollider.bounds.max +
                       new Vector3(halfVox, 0, -halfVox - (tileSideVoxels - horizontalOffset - 1) * vox);
            rayDir = Vector3.left;
        }
        else if (direction == Direction.Back)
        {
            rayStart = meshCollider.bounds.max +
                       new Vector3(-halfVox - (tileSideVoxels - horizontalOffset - 1) * vox, 0, halfVox);
            rayDir = Vector3.back;
        }
        else
        {
            throw new ArgumentException("Wrong direction value, should be Vector3.forward/right/left/back",
                nameof(direction));
        }

        rayStart.y = meshCollider.bounds.min.y + halfVox + verticalLayer * vox;

        if (Physics.Raycast(new Ray(rayStart, rayDir), out RaycastHit hit, vox))
        {
            byte colorIndex = (byte) (hit.textureCoord.x * 256);
            if (colorIndex == 0) Debug.LogWarning("Found color 0 in mesh palette, this can cause conflicts");
            return colorIndex;
        }
        return 0;
    }

    
}