using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;

public class VoxelTilePlacerSequential : MonoBehaviour
{
    public List<VoxelTile> TilePrefabs;
    public Vector2Int MapSize = new Vector2Int(10, 10);
    

    private VoxelTile[,] _spawnedTiles;
    private VoxelTileRotator _voxelTileRotator;

    private void Start()
    {
        _spawnedTiles = new VoxelTile[MapSize.x, MapSize.y];

        foreach (VoxelTile tilePrefab in TilePrefabs)
        {
            tilePrefab.CalculateSideColors();
        }

        _voxelTileRotator = GetComponent<VoxelTileRotator>();
        TilePrefabs = _voxelTileRotator.MakeRotatedTiles(TilePrefabs);
        
        StartCoroutine(Generate());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            StopAllCoroutines();

            foreach (VoxelTile spawnedTile in _spawnedTiles)
            {
                if (spawnedTile != null) Destroy(spawnedTile.gameObject);
            }

            StartCoroutine(Generate());
        }
    }

    private IEnumerator Generate()
    {
        for (int x = 1; x < MapSize.x-1; x++)
        {
            for (int y = 1; y < MapSize.y-1; y++)
            {
                yield return new WaitForSeconds(0.02f);
                PlaceTile(x,y);
            }
        }
    }

    private void PlaceTile(int x, int y)
    {
        List<VoxelTile> availableTiles = new List<VoxelTile>();

        foreach (VoxelTile tilePrefab in TilePrefabs)
        {
            if (VoxelTileMatcher.Instance.CanAppendTile(_spawnedTiles[x-1,y], tilePrefab, Direction.Left) &&
                VoxelTileMatcher.Instance.CanAppendTile(_spawnedTiles[x+1,y], tilePrefab, Direction.Right) &&
                VoxelTileMatcher.Instance.CanAppendTile(_spawnedTiles[x,y-1], tilePrefab, Direction.Back) &&
                VoxelTileMatcher.Instance.CanAppendTile(_spawnedTiles[x,y+1], tilePrefab, Direction.Forward))
            {
                availableTiles.Add(tilePrefab);
            }
        }

        if (availableTiles.Count == 0) return;

        VoxelTile selectedTile = VoxelTileRandomizer.Instance.GetRandomTile(availableTiles);
        Vector3 position = new Vector3(x, 0, y) * selectedTile.voxelSize * selectedTile.tileSideVoxels;
        _spawnedTiles[x, y] = Instantiate(selectedTile, position, selectedTile.transform.rotation);
    }
}
