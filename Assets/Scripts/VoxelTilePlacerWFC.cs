using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;
using UnityEngine.WSA;

public class VoxelTilePlacerWFC : MonoBehaviour
{
    
    public List<VoxelTile> TilePrefabs;
    public Vector2Int MapSize = new Vector2Int(10, 10);
    
    private VoxelTile[,] _spawnedTiles;
    private Queue<Vector2Int> _recalcPossibleTilesQueue = new Queue<Vector2Int>();
    private List<VoxelTile>[,] _possibleTiles;
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
        Generate();
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
            
            Generate();
        }
    }
    
    private void Generate()
    {
        _possibleTiles = new List<VoxelTile>[MapSize.x, MapSize.y];
        
        int maxAttempts = 10;
        int attempts= 0;
        while (attempts++ < maxAttempts)
        {
            for (int x = 0; x < MapSize.x; x++)
            for (int y = 0; y < MapSize.y; y++)
            {
                _possibleTiles[x, y] = new List<VoxelTile>(TilePrefabs);
            }

            VoxelTile tileInCenter = VoxelTileRandomizer.Instance.GetRandomTile(TilePrefabs);
            _possibleTiles[MapSize.x/2, MapSize.y/2] = new List<VoxelTile> {tileInCenter};
            _recalcPossibleTilesQueue.Clear();
            EnqueueNeighboursToRecalc(new Vector2Int(MapSize.x/2, MapSize.y/2));
        
            bool success = GenerateAllPossibleTiles();
            if (success) break;
        }
        PlaceAllTiles();
    }

    private bool GenerateAllPossibleTiles()
    {
        int maxIterations = MapSize.x * MapSize.y;
        int iterations = 0;
        while (iterations++ < maxIterations)
        {
            int maxInnerIterations = 500;
            int innerIterations = 0;
            while (_recalcPossibleTilesQueue.Count>0 && innerIterations++ < maxInnerIterations)
            {
                Vector2Int position = _recalcPossibleTilesQueue.Dequeue();
                if (position.x == 0 || position.y == 0 || position.x == MapSize.x - 1 || position.y == MapSize.y - 1)
                {
                    continue;
                }

                List<VoxelTile> possibleTilesHere = _possibleTiles[position.x, position.y];
                int countRemoved = possibleTilesHere.RemoveAll(t=> !IsTilePossible(t,position));

                if (countRemoved > 0) EnqueueNeighboursToRecalc(position);
                if (possibleTilesHere.Count == 0)
                {
                    // code to resolve possible stuck in placement. Repeat arrangement of this and neigh cells.
                    possibleTilesHere.AddRange(TilePrefabs);
                    _possibleTiles[position.x + 1, position.y] = new List<VoxelTile>(TilePrefabs);
                    _possibleTiles[position.x - 1, position.y] = new List<VoxelTile>(TilePrefabs);
                    _possibleTiles[position.x, position.y + 1] = new List<VoxelTile>(TilePrefabs);
                    _possibleTiles[position.x, position.y - 1] = new List<VoxelTile>(TilePrefabs);
                    
                    EnqueueNeighboursToRecalc(position);
                }
            }

            if (innerIterations == maxInnerIterations) break;

            List<VoxelTile> maxCountTile = _possibleTiles[1, 1];
            Vector2Int maxCountTilePosition = new Vector2Int(1, 1);

            for (int x = 1; x < MapSize.x-1; x++)
            for (int y = 1; y < MapSize.y-1; y++)
            {
                if (_possibleTiles[x,y].Count > maxCountTile.Count)
                {
                    maxCountTile = _possibleTiles[x, y];
                    maxCountTilePosition = new Vector2Int(x, y);
                }
            }

            if (maxCountTile.Count == 1)
            {
                return true;
            }
            
            VoxelTile tileToCollapse = VoxelTileRandomizer.Instance.GetRandomTile(maxCountTile);
            _possibleTiles[maxCountTilePosition.x, maxCountTilePosition.y] = new List<VoxelTile> {tileToCollapse};
            EnqueueNeighboursToRecalc(maxCountTilePosition);
        }
        Debug.Log("Out of iterations!");
        return false;
    }

    private bool IsTilePossible(VoxelTile tile, Vector2Int position)
    {
        bool isAllRightImpossible = _possibleTiles[position.x - 1, position.y]
            .All(rightTile => !VoxelTileMatcher.Instance.CanAppendTile(tile, rightTile, Direction.Right));
        if (isAllRightImpossible) return false;
        
        bool isAllLeftImpossible = _possibleTiles[position.x + 1, position.y]
            .All(leftTile => !VoxelTileMatcher.Instance.CanAppendTile(tile, leftTile, Direction.Left));
        if (isAllLeftImpossible) return false;
        
        bool isAllForwardImpossible = _possibleTiles[position.x, position.y - 1]
            .All(forwardTile => !VoxelTileMatcher.Instance.CanAppendTile(tile, forwardTile, Direction.Forward));
        if (isAllForwardImpossible) return false;
        
        bool isAllBackImpossible = _possibleTiles[position.x, position.y + 1]
            .All(backTile => !VoxelTileMatcher.Instance.CanAppendTile(tile, backTile, Direction.Back));
        if (isAllBackImpossible) return false;

        return true;
    }

    private void PlaceAllTiles()
    {
        for (int x = 1; x < MapSize.x - 1; x++)
        for (int y = 1; y < MapSize.y - 1; y++)
        {
            PlaceTile(x,y);
        }
    }

    private void EnqueueNeighboursToRecalc(Vector2Int position)
    {
        _recalcPossibleTilesQueue.Enqueue(new Vector2Int(position.x+1,position.y));
        _recalcPossibleTilesQueue.Enqueue(new Vector2Int(position.x-1,position.y));
        _recalcPossibleTilesQueue.Enqueue(new Vector2Int(position.x,position.y+1));
        _recalcPossibleTilesQueue.Enqueue(new Vector2Int(position.x,position.y-1));
    }

    private void PlaceTile(int x, int y)
    {
        if (_possibleTiles[x, y].Count == 0) return;

        VoxelTile selectedTile = VoxelTileRandomizer.Instance.GetRandomTile(_possibleTiles[x, y]);
        Vector3 position = new Vector3(x, 0, y) * selectedTile.voxelSize * selectedTile.tileSideVoxels;
        _spawnedTiles[x, y] = Instantiate(selectedTile, position, selectedTile.transform.rotation);
    }
}
