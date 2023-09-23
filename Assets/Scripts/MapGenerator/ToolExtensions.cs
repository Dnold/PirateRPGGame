using Unity;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace ToolExtensions
{

    public static class ChunkTools
    {
        /// <summary>
        /// Generates a random number from a Gaussian distribution.
        /// </summary>
        /// <param name="mean">Mean of the distribution.</param>
        /// <param name="standardDeviation">Standard deviation of the distribution.</param>
        /// <returns>Random number from the Gaussian distribution.</returns>
        public static float NextGaussianFloat(float mean, float standardDeviation)
        {
            System.Random r = new System.Random();
            float u1 = 1.0f - (float)r.NextDouble();
            float u2 = 1.0f - (float)r.NextDouble();
            float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
            float randNormal = mean + standardDeviation * (float)randStdNormal;
            return Mathf.Clamp(randNormal, 0f, 1f);  // Ensuring the result stays between 0 and 1
        }
        public static Chunk[] To1DArray(Chunk[,] input)
        {
            //get total size of 2D array, and allocate 1D array.
            int size = input.Length;
            Chunk[] result = new Chunk[size];

            //copy 2D array elements into a 1D array.
            int write = 0;
            for (int i = 0; i <= input.GetUpperBound(0); i++)
            {
                for (int z = 0; z <= input.GetUpperBound(1); z++)
                {
                    result[write++] = input[i, z];
                }
            }

            return result;
        }
        public static bool IsBoundaryTile(Vector2Int tile, int[,] map, Vector2Int size, params TileType[] tileTypes)
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
        public static bool IsInMapRange(int x, int y, Vector2Int size)
        {
            return x >= 0 && x < size.x && y >= 0 && y < size.y;
        }
        public static List<Vector2Int> GetClosestRegion(Vector2Int playerTilePos, List<List<Vector2Int>> regions, TileType[] islandTiles)
        {
            List<Vector2Int> closestRegion = new List<Vector2Int>();
            float closestDistance = float.MaxValue;

            //Check for every tile in region if its the closest
            for (int x = 0; x < regions.Count(); x++)
            {
                //Compare Distance
                Vector2Int currentTile = GetRegionCenter(regions[x]);
                float currentDistance = (playerTilePos - currentTile).sqrMagnitude;

                if (currentDistance < closestDistance)
                {
                    closestDistance = currentDistance;
                    closestRegion = regions[x];
                }
            }



            return closestRegion;
        }
        public static Vector2Int GetClosestTileOfType(Vector2Int playerTilePos, int[,] upscaledRegion, TileType[] tileTypes)
        {
            Vector2Int closestTile = new Vector2Int(-1, -1); // (-1,-1) for not found
            float closestDistance = float.MaxValue;

            //Check for every tile in region if its the closest
            for (int x = 0; x < upscaledRegion.GetLength(0); x++)
            {
                for (int y = 0; y < upscaledRegion.GetLength(1); y++)
                {
                    if (tileTypes.Contains((TileType)upscaledRegion[x, y]))
                    {
                        //Compare Distance
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
        public static int GetDistanceToNearestIsland(Vector2Int position, List<Vector2Int> islandPositions)
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
        public static Vector2Int GetRegionCenter(List<Vector2Int> regionTiles)
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
        public static Vector2Int FindWaterRandomWaterTile(int[,] map)
        {
            List<Vector2Int> waterTiles = new List<Vector2Int>();
            for(int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    if (map[x,y] == (int)TileType.deepWater)
                    {
                        waterTiles.Add(new Vector2Int(x,y));
                    }
                }
            }
            return waterTiles[UnityEngine.Random.Range(0, waterTiles.Count())];
        }
    }
}



