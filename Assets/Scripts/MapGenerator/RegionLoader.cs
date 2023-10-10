using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;


public class RegionLoader : MonoBehaviour
{
    
    public Sprite[] trees;
    public Sprite[] grassTiles;

    public List<GameObject> allSpawnedIslandObjects;
    public Vector2Int regionSize; // This would be the size of your upscaled region.
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    public void Update()
    {
        //if(playerObj != null)
        //{
        //    playerObj.GetComponent<SpriteRenderer>().sortingOrder=regionSize.y-(int)playerObj.transform.position.y-1;
        //}
    }
    private int[,] InitializeGridWithWater(int width, int height)
    {
        int[,] grid = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = (int)TileType.Water;
            }
        }
        return grid;
    }
    // Determine the dominant TileType among the four tiles, using bilinear interpolation
    private TileType BilinearInterpolate(TileType bottomLeft, TileType topLeft, TileType bottomRight, TileType topRight, float xPercent, float yPercent)
    {
        Dictionary<TileType, float> tileWeights = new Dictionary<TileType, float>();
        foreach (TileType type in System.Enum.GetValues(typeof(TileType)))
        {
            tileWeights[type] = 0f;
        }

        tileWeights[bottomLeft] += (1 - xPercent) * (1 - yPercent);
        tileWeights[topLeft] += (1 - xPercent) * yPercent;
        tileWeights[bottomRight] += xPercent * (1 - yPercent);
        tileWeights[topRight] += xPercent * yPercent;

        // Return the TileType with the highest weight
        return tileWeights.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
    }
    public int[,] CreateGrid(Region region, int[,] sourceGrid, int desiredWidth, int desiredHeight)
    {
        int[,] grid = InitializeGridWithWater(desiredWidth, desiredHeight);

        // Determine the bounding box of the regionTiles
        int minX = region.regionTiles.Min(tile => tile.x);
        int maxX = region.regionTiles.Max(tile => tile.x);
        int minY = region.regionTiles.Min(tile => tile.y);
        int maxY = region.regionTiles.Max(tile => tile.y);

        int offsetX = (desiredWidth - (maxX - minX + 1)) / 2;
        int offsetY = (desiredHeight - (maxY - minY + 1)) / 2;

        // Place the region tiles into the grid, filtering by supported tile types
        foreach (Vector2Int tile in region.regionTiles)
        {
            TileType currentType = (TileType)sourceGrid[tile.x, tile.y];

                grid[tile.x - minX + offsetX, tile.y - minY + offsetY] = (int)currentType;
        }
        grid = UpscaleGrid(grid, 4);
        regionSize = new Vector2Int(grid.GetLength(0), grid.GetLength(1));
        grid = PlaceTrees(grid);
        return grid;
    }
    public int[,] PlaceTrees(int[,] fullmap)
    {
        List<SpriteRenderer> renderers = new List<SpriteRenderer>();
        for (int x = 0; x < regionSize.x; x++)
        {
            for (int y = 0; y < regionSize.y; y++)
            {
                if (fullmap[x, y] == (int)TileType.BigGrass)
                {
                    fullmap[x, y] = (int)TileType.Island;
                    if (Random.Range(0, 100) > 75f)
                    {
                        GameObject tree = Instantiate(new GameObject(), new Vector3(x, y, 0), Quaternion.identity);
                        allSpawnedIslandObjects.Add(tree);
                        var sr = tree.AddComponent<SpriteRenderer>();
                        sr.sprite = trees[Random.Range(0, trees.Length)];
                        sr.spriteSortPoint = SpriteSortPoint.Pivot;
                    }
                }
                if (fullmap[x, y] == (int)TileType.MediumGrass)
                {
                    fullmap[x, y] = (int)TileType.Island;
                    if (Random.Range(0, 100) > 50f)
                    {
                        GameObject tree = Instantiate(new GameObject(), new Vector3(x, y, 0), Quaternion.identity);
                        var sr = tree.AddComponent<SpriteRenderer>();
                        sr.sprite = grassTiles[Random.Range(0, grassTiles.Length)];
                        allSpawnedIslandObjects.Add(tree);
                    }
                }
            }
        }
        return fullmap;
    }
    public int[,] UpscaleGrid(int[,] grid, int upscaleFactor)
    {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);

        int upscaledWidth = width * upscaleFactor;
        int upscaledHeight = height * upscaleFactor;
        int[,] upscaledGrid = InitializeGridWithWater(upscaledWidth, upscaledHeight);

        for (int x = 0; x < upscaledWidth; x++)
        {
            for (int y = 0; y < upscaledHeight; y++)
            {
                float gx = x / (float)upscaleFactor;
                float gy = y / (float)upscaleFactor;

                int gxi = (int)gx;
                int gyi = (int)gy;

                // Create a dictionary to hold weights of each TileType
                Dictionary<TileType, float> tileWeights = new Dictionary<TileType, float>();
                foreach (TileType type in System.Enum.GetValues(typeof(TileType)))
                {
                    tileWeights[type] = 0f;
                }

                for (int i = 0; i <= 1; i++)
                {
                    for (int j = 0; j <= 1; j++)
                    {
                        int xi1 = Mathf.Clamp(gxi, 0, width - 1);
                        int xi2 = Mathf.Clamp(gxi + 1, 0, width - 1);
                        int yi1 = Mathf.Clamp(gyi, 0, height - 1);
                        int yi2 = Mathf.Clamp(gyi + 1, 0, height - 1);

                        TileType bottomLeft = (TileType)grid[xi1, yi1];
                        TileType topLeft = (TileType)grid[xi1, yi2];
                        TileType bottomRight = (TileType)grid[xi2, yi1];
                        TileType topRight = (TileType)grid[xi2, yi2];

                        TileType interpolatedTile = BilinearInterpolate(bottomLeft, topLeft, bottomRight, topRight, gx + i - gxi, gy + j - gyi);
                        tileWeights[interpolatedTile] += 1.0f; // Increment the count for the interpolated TileType
                    }
                }

                // Find the TileType with the maximum weight
                TileType dominantTile = tileWeights.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;

                upscaledGrid[x, y] = (int)dominantTile;
            }
        }
        return upscaledGrid;
    }
}


