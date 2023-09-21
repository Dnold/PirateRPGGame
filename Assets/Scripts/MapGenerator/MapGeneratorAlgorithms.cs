using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Diagnostics;
using System;

public class MapGeneratorAlgorithms : MapGeneratorHelper
{


    [Header("Randomnes")]
    public float standardDeviation = 0.15f; 
    public bool useRandomSeed;
    public string seed;
    public float flowerFillPercent = 1;
    public float noiseThreshold = 0.4f;
    public float noiseScale = 1f;
    public float grassChance = 70;


    public int[,] FillMapRandom(Vector2Int size)
    {
        int[,] newMap = new int[size.x, size.y];

        if (useRandomSeed)
        {
            seed = Time.time.ToString() + UnityEngine.Random.Range(0, 2000);
        }

        System.Random rand = new System.Random(seed.GetHashCode());

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (IsBorder(x, y, size))
                {
                    newMap[x, y] = 1; // Border always filled
                }
                else
                {
                    newMap[x, y] = rand.Next(0, 100) < waterFillPercent ? 1 : 0;
                }
            }
        }
        return newMap;
    }
    #region Cellular Automata Smoothing
    public int[,] SmoothMap(Vector2Int size, int[,] map)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                int neighbourCount = GetNeighbourCount(x, y, map, size);

                if (neighbourCount > 4)
                {
                    map[x, y] = 1;  // Convert to the target tile type
                }
                else if (neighbourCount < 4)
                {
                 
                    map[x, y] = (int)TileType.Island;
                }
                // If neighbourCount is exactly 4, the cell remains unchanged.
            }
        }
        return map;
    }
    public int[,] SmoothMapInRegion(List<Vector2Int> region, int[,] map, TileType targetTile)
    {
        // Iterate only over the tiles in the specified region
        foreach (Vector2Int tile in region)
        {
            int x = tile.x;
            int y = tile.y;

            int neighbourCount = GetNeighbourCountInRegion(x, y, map, targetTile, region);

            if (neighbourCount > 3)
            {
                map[x, y] = (int)targetTile;  // Convert to the target tile type
            }
            else if (neighbourCount < 3)
            {
                map[x, y] = 0; // Set to Empty for simplicity
            }
            // If neighbourCount is exactly 4, the cell remains unchanged.
        }
        return map;
    }
    #endregion

    #region FloodFill Region Detection
    public List<List<Vector2Int>> GetRegions(int[,] map, Vector2Int size, params TileType[] tileTypes)
    {
        List<List<Vector2Int>> regions = new List<List<Vector2Int>>();
        int[,] mapFlags = new int[size.x, size.y];
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (mapFlags[x, y] == 0 && tileTypes.Contains((TileType)map[x, y]))
                {
                    List<Vector2Int> newRegion = GetRegionTiles(x, y, map, tileTypes);
                    regions.Add(newRegion);
                    foreach (Vector2Int tile in newRegion)
                    {
                        mapFlags[tile.x, tile.y] = 1;
                    }
                }
            }
        }
        return regions;
    }

    public List<Vector2Int> GetRegionTiles(int startX, int startY, int[,] map, params TileType[] treatedAsSameTypes)
    {
        Vector2Int size = new Vector2Int(map.GetLength(0), map.GetLength(1));
        List<Vector2Int> tiles = new List<Vector2Int>();
        int[,] mapFlags = new int[size.x, size.y];
        TileType currentTileType = (TileType)map[startX, startY];

        // If no specific types are provided, only the current tile type.
        if (treatedAsSameTypes.Length == 0)
        {
            treatedAsSameTypes = new TileType[] { currentTileType };
        }

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startX, startY));
        mapFlags[startX, startY] = 1;

        int[] dirX = { 0, 0, 1, -1 };
        int[] dirY = { 1, -1, 0, 0 };

        while (queue.Count > 0)
        {
            Vector2Int tile = queue.Dequeue();
            tiles.Add(tile);

            for (int i = 0; i < 4; i++)
            {
                int neighbourX = tile.x + dirX[i];
                int neighbourY = tile.y + dirY[i];

                if (ToolExtensions.ChunkTools.IsInMapRange(neighbourX, neighbourY,size) && mapFlags[neighbourX, neighbourY] == 0)
                {
                    // If flood-filling a water region encounter Sand or Grass,treat  as barriers/boundaries
                    if (!treatedAsSameTypes.Contains(currentTileType) && currentTileType == TileType.Water &&
                       (map[neighbourX, neighbourY] == (int)TileType.Sand))
                    {
                        continue;
                    }

                    // If the neighboring tile is one of the types it will treated as the same
                    if (treatedAsSameTypes.Contains((TileType)map[neighbourX, neighbourY]))
                    {
                        mapFlags[neighbourX, neighbourY] = 1;
                        queue.Enqueue(new Vector2Int(neighbourX, neighbourY));
                    }
                }
            }
        }
        return tiles;
    }

    public List<List<Vector2Int>> FindAllSubRegionsInLargerRegion(List<Vector2Int> largerRegion, int[,] map, TileType targetTileType)
    {
        // To keep track of tiles that have been processed
        HashSet<Vector2Int> processedTiles = new HashSet<Vector2Int>();

        List<List<Vector2Int>> subRegions = new List<List<Vector2Int>>();

        foreach (Vector2Int tile in largerRegion)
        {
            if (!processedTiles.Contains(tile) && map[tile.x, tile.y] == (int)targetTileType)
            {
                List<Vector2Int> subRegion = GetRegionTiles(tile.x, tile.y, map, targetTileType);

                subRegions.Add(subRegion);

                foreach (Vector2Int subTile in subRegion)
                {
                    processedTiles.Add(subTile);
                }
            }
        }
        return subRegions;
    }

    public int[,] ProcessMap(int[,] map, TileType tileType, int threshold)
    {
        List<List<Vector2Int>> regions = GetRegions(map, chunkSize, tileType);

        foreach (var region in regions)
        {
            if (region.Count < threshold)
            {
                foreach (Vector2Int tile in region)
                {
                    map[tile.x, tile.y] = (int)TileType.Water; //convert the tiles to Water if they're below the threshold
                }
            }
        }
        return map;
    }
    #endregion
    #region Populate Map

    public int[,] Sandbanks(List<List<Vector2Int>> regions, int[,] map)
    {
        for (int i = 0; i < regions.Count; i++)
        {
            foreach (var region in regions)
            {
                foreach (var tile in region)
                {
                    int waterCount = WaterCount(tile.x, tile.y, map, chunkSize);
                    if (waterCount > 0)
                    {
                        map[tile.x, tile.y] = 2;
                    }
                }
            }
        }
        return map;
    }
    public int[,] SetGrassInRegions(List<List<Vector2Int>> regions, int[,] map, TileType[] flowerTypes, TileType[] grassTypes)
    {
        foreach (var region in regions)
        {

            foreach (Vector2Int tile in region)
            {
                // Check if the current tile is an island tile
                if (map[tile.x, tile.y] == 0) // 0 = IslandTile
                {
                    // For every island tile, check with randomness
                    if (UnityEngine.Random.Range(0f, 100f) >= grassChance)
                    {
                        map[tile.x, tile.y] = 3; // 3 for grass
                    }
                }
            }
            map = SmoothMapInRegion(region, map, TileType.SmallGrass);
            List<List<Vector2Int>> subRegions = FindAllSubRegionsInLargerRegion(region, map, TileType.SmallGrass);
            foreach (var subRegion in subRegions)
            {
                map = RandomizeGrassTypeInRegion(subRegion, map, grassTypes);
               
            }
        }
        return map;
    }
    public int[,] RandomizeGrassTypeInRegion(List<Vector2Int> region, int[,] map, TileType[] grassTypes)
    {
        if (grassTypes.Contains((TileType)map[region[0].x, region[0].y]))
        {
            int rand = UnityEngine.Random.Range(0, grassTypes.Length);
            foreach (var tile in region)
            {
                map[tile.x, tile.y] = (int)grassTypes[rand];
            }
        }
        return map;
    }
    public int[,] SetOceanDepth(int[,] fullMap)
    {
        int width = fullMap.GetLength(0);
        int height = fullMap.GetLength(1);

        //get a list of all island tiles
        List<Vector2Int> islandPositions = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (fullMap[x, y] == (int)TileType.Island)
                {
                    islandPositions.Add(new Vector2Int(x, y));
                }
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (fullMap[x, y] == (int)TileType.Water)
                {
                    // For every water tile, compute the distance to the nearest island
                    int minDistance = ToolExtensions.ChunkTools.GetDistanceToNearestIsland(new Vector2Int(x, y), islandPositions);

                    // Based on minDistance, set the water depth
                    if (minDistance < 2)
                    {
                        fullMap[x, y] = (int)TileType.shallowWater;
                    }
                    else if (minDistance < 5)
                    {
                        fullMap[x, y] = (int)TileType.lowWater;
                    }
                    else if (minDistance < 9)
                    {
                        fullMap[x, y] = (int)TileType.mediumWater;
                    }
                    else
                    {
                        fullMap[x, y] = (int)TileType.deepWater;
                    }
                }
            }
        }
        return fullMap;
    }
    public int[,] BlurOceanDepthWithNoise(int[,] map, float noiseScale, float noiseThreshold)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);
        int[,] blurredMap = new int[width, height];

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                // Check if the tile is a water type tile
                if (map[x - 1, y - 1] == (int)TileType.shallowWater ||
                    map[x - 1, y - 1] == (int)TileType.lowWater ||
                    map[x - 1, y - 1] == (int)TileType.mediumWater ||
                    map[x - 1, y - 1] == (int)TileType.deepWater)
                {
                    float noiseValue = Mathf.PerlinNoise(x * noiseScale, y * noiseScale);

                    if (noiseValue > noiseThreshold)
                    {
                        // If noise value is above the threshold, compute average blur for this tile.
                        int sum = 0;
                        int count = 0; // Number of water tiles considered in the blur.

                        for (int i = -1; i <= 1; i++)
                        {
                            for (int j = -1; j <= 1; j++)
                            {
                                if (map[x + i, y + j] == (int)TileType.shallowWater ||
                                    map[x + i, y + j] == (int)TileType.lowWater ||
                                    map[x + i, y + j] == (int)TileType.mediumWater ||
                                    map[x + i, y + j] == (int)TileType.deepWater)
                                {
                                    sum += map[x + i, y + j];
                                    count++;
                                }
                            }
                        }

                        blurredMap[x, y] = sum / count; // Average over the water cells only.
                    }
                    else
                    {
                        // If noise value is below the threshold, keep the original tile value.
                        blurredMap[x, y] = map[x, y];
                    }
                }
                else
                {
                    // If the tile is not a water type, copy it to the blurredMap without changes.
                    blurredMap[x, y] = map[x, y];
                }
            }
        }

        return blurredMap;
    }
    #endregion
}

