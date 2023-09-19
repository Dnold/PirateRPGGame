using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapGeneratorHelper : MonoBehaviour
{

    [SerializeField]
    [Header("Dimensions")]
    public Vector2Int gridSize;
    public Vector2Int chunkSize;
    [Header("Settings")]
    public float waterFillPercent = 50;
    public int proccessThreshhold = 25;
   
    [Header("Benchmark")]
    public TMP_Text elapsedMsText;
    public TMP_Text statsText;
    public placeCam placeCam;
    public bool IsBorder(int x, int y, Vector2Int size)
    {
        return x == 0 || y == 0 || x == size.x - 1 || y == size.y - 1;
    }
    public int GetNeighbourCount(int gridX, int gridY, int[,] map, Vector2Int size)
    {
        int count = 0;
        // Directions representing the 8 neighboring cells
        int[] dirX = { -1, 0, 1, -1, 1, -1, 0, 1 };
        int[] dirY = { -1, -1, -1, 0, 0, 1, 1, 1 };

        for (int i = 0; i < 8; i++)
        {
            int neighbourX = gridX + dirX[i];
            int neighbourY = gridY + dirY[i];

            // If the neighbor is within map boundaries, check its status
            if (IsInMapRange(neighbourX, neighbourY, size))
            {
                count += map[neighbourX, neighbourY];
            }
            else
            {
                // Out-of-boundary neighbors are considered alive to create a solid boundary.
                count++;
            }
        }
        return count;
    }
    public int GetNeighbourCountInRegion(int x, int y, int[,] map, TileType targetType, List<Vector2Int> region)
    {
        int count = 0;

        int[] dirX = { -1, 0, 1, 0 };
        int[] dirY = { 0, -1, 0, 1 };

        for (int i = 0; i < 4; i++)
        {
            int neighbourX = x + dirX[i];
            int neighbourY = y + dirY[i];
            Vector2Int neighbourTile = new Vector2Int(neighbourX, neighbourY);

            if (region.Contains(neighbourTile) && map[neighbourX, neighbourY] == (int)targetType)
            {
                count++;
            }
        }
        return count;
    }
    public bool IsInMapRange(int x, int y, Vector2Int size)
    {
        return x >= 0 && x < size.x && y >= 0 && y < size.y;
    }
    public int WaterCount(int xTile, int yTile, int[,] map, Vector2Int size)
    {
        int count = 0;
        for (int x = xTile - 1; x < xTile + 1; x++)
        {
            for (int y = yTile - 1; y < yTile + 1; y++)
            {
                if (IsInMapRange(x, y, size))
                {
                    if (map[x, y] == 1)
                    {
                        count++;
                    }
                }
            }
        }
        return count;
    }
    public float minimalDistance = 50f; // Define your minimal distance threshold here.
                                        // Convert LoadedMapRegionTilePosition to LoadedRegionTilePosition
    public Vector2Int ToScaledPosition(Vector2Int mapRegionTilePosition)
    {
        return new Vector2Int((int)(mapRegionTilePosition.x * 4),
                              (int)(mapRegionTilePosition.y * 4));
    }

    // Convert LoadedRegionTilePosition to LoadedMapRegionTilePosition
    public Vector2Int ToMapRegionTilePosition(Vector2Int scaledRegionTilePosition)
    {
        return new Vector2Int((int)(scaledRegionTilePosition.x / 4),
                              (int)(scaledRegionTilePosition.y / 4));
    }
    public Vector2Int GetClosestIslandTile(Vector2Int playerTilePos, int[,] upscaledRegion, TileType[] islandTiles)
    {
        Vector2Int closestTile = new Vector2Int(-1, -1);
        float closestDistance = float.MaxValue;

        for (int x = 0; x < upscaledRegion.GetLength(0); x++)
        {
            for (int y = 0; y < upscaledRegion.GetLength(1); y++)
            {
                // Assuming a value of 1 in upscaledRegion indicates it's part of the island.
                // Adjust this condition based on your actual representation.
                if (islandTiles.Contains((TileType)upscaledRegion[x, y]))
                {
                    Vector2Int currentTile = new Vector2Int(x, y);
                    float currentDistance = (playerTilePos - currentTile).sqrMagnitude;

                    if (currentDistance < closestDistance)
                    {
                        closestDistance = currentDistance;
                        closestTile = currentTile;
                    }
                }
            }
        }

        return closestTile;
    }

    // Call this method to get the center of the closest region under minimal distance to the player.
    public Vector2Int? GetClosestRegionToPlayer(List<Vector2Int> regionCenters, Vector2 playerPosition)
    {
        Vector2Int? closestRegionCenter = null;
        float closestDistanceSqr = minimalDistance * minimalDistance; // We will compare squared distances for performance.

        foreach (Vector2Int regionCenter in regionCenters)
        {
            float currentDistanceSqr = (playerPosition - (Vector2)regionCenter).sqrMagnitude;
            if (currentDistanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = currentDistanceSqr;
                closestRegionCenter = regionCenter;
            }
        }

        return closestRegionCenter; // This will be null if no region center is found within the minimal distance.
    }
    public static int[,] ConcatenateChunks(Chunk[,] chunkse)
    {
        int chunkSizeX = chunkse[0, 0].size.x;
        int chunkSizeY = chunkse[0, 0].size.y;
        int numChunksX = chunkse.GetLength(0);
        int numChunksY = chunkse.GetLength(1);
        int fullMapSizeX = chunkSizeX * numChunksX;
        int fullMapSizeY = chunkSizeY * numChunksY;
        int[,] fullMap = new int[fullMapSizeX, fullMapSizeY];

        for (int cx = 0; cx < numChunksX; cx++)
        {
            for (int cy = 0; cy < numChunksY; cy++)
            {
                for (int x = 0; x < chunkSizeX; x++)
                {
                    for (int y = 0; y < chunkSizeY; y++)
                    {
                        fullMap[cx * chunkSizeX + x, cy * chunkSizeY + y] = chunkse[cx, cy].map[x, y];
                    }
                }
            }
        }

        return fullMap;
    }
    public Chunk[,] DivideIntoChunks(int[,] fullMap, Vector2Int chunkSize, Chunk[,] chunks)
    {
        int width = fullMap.GetLength(0);
        int height = fullMap.GetLength(1);
        int numChunksX = width / chunkSize.x;
        int numChunksY = height / chunkSize.y;
        Chunk[,] chunkse = new Chunk[numChunksX, numChunksY];

        for (int cx = 0; cx < numChunksX; cx++)
        {
            for (int cy = 0; cy < numChunksY; cy++)
            {
                Vector2Int chunkCenter = new Vector2Int(cx * chunkSize.x + chunkSize.x / 2, cy * chunkSize.y + chunkSize.y / 2);
                int[,] chunkMap = new int[chunkSize.x, chunkSize.y];
                for (int x = 0; x < chunkSize.x; x++)
                {
                    for (int y = 0; y < chunkSize.y; y++)
                    {
                        chunkMap[x, y] = fullMap[cx * chunkSize.x + x, cy * chunkSize.y + y];
                    }
                }
                chunkse[cx, cy] = new Chunk(chunkCenter, chunkSize, chunkMap, chunks[cx,cy].regions);
                
            }
        }

        return chunkse;
    }

    public int GetDistanceToNearestIsland(Vector2Int position, List<Vector2Int> islandPositions)
    {
        int minDistance = Int32.MaxValue;
        foreach (var islandPos in islandPositions)
        {
            int distance = Math.Abs(islandPos.x - position.x) + Math.Abs(islandPos.y - position.y);
            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }
        return minDistance;
    }
    public bool IsBoundaryTile(Vector2Int tile, int[,] map, Vector2Int size, params TileType[] tileTypes)
    {
        int[] dirX = { 0, 0, 1, -1 };
        int[] dirY = { 1, -1, 0, 0 };

        for (int i = 0; i < 4; i++)
        {
            int neighbourX = tile.x + dirX[i];
            int neighbourY = tile.y + dirY[i];

            // If out of range, continue to next iteration
            if (!IsInMapRange(neighbourX, neighbourY, size))
                continue;

            if (!tileTypes.Contains((TileType)map[neighbourX, neighbourY]))
            {
                return true;  // This tile has a neighbor of a different type, so it's a boundary tile.
            }
        }
        return false;
    }

    public Vector2Int GetRegionCenter(List<Vector2Int> regionTiles)
    {
        int totalX = 0;
        int totalY = 0;

        foreach (Vector2Int tile in regionTiles)
        {
            totalX += tile.x;
            totalY += tile.y;
        }

        return new Vector2Int(totalX / regionTiles.Count, totalY / regionTiles.Count);
    }
}
