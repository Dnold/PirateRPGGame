using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.Tilemaps;


public class RegionLoader : MonoBehaviour
{
    public GameObject player;
    public Sprite[] trees;
    public Sprite[] grassTiles;
    GameObject playerObj;
    public bool mapLoaded = false;
    public Vector2Int playerSpawnTile;
    public int[,] fullmap;
    public Tilemap? tilemap;
    public TileValue[] tiles; // Assuming you have an array or list of 'TileValue' which contains information about each tile type.

    public Vector2Int regionSize; // This would be the size of your upscaled region.

    public float minimalDistance = 50f; // Define your minimal distance threshold here.

    // Call this method to get the center of the closest region under minimal distance to the player.

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    public void LoadRegionTiles(int[,] regionGrid)
    {
        // Clear tilemaps
        tilemap.ClearAllTiles();


        // Loop through every tile in the region grid
        for (int x = 0; x < regionSize.x; x++)
        {
            for (int y = 0; y < regionSize.y; y++)
            {
                // Get the global position of the tile.
                Vector3Int position = new Vector3Int(x, y, 0);

                // Get the tile type from the region grid
                int value = regionGrid[x, y];


                // Set the main tile (ground, island, etc.)
                tilemap.SetTile(position, tiles.FirstOrDefault(e => (int)e.Type == value).tile);

                //// If this is a region tile, set the top tile.
                //if (regionTiles.Contains(new Vector2Int(x, y)))
                //{
                //    tilemapTop.SetTile(position, tiles[4].tile);
                //}
            }
        }


    }

    public void Update()
    {

        if (GameObject.FindGameObjectWithTag("regionMap") != null && mapLoaded)
        {
            tilemap = (GameObject.FindGameObjectWithTag("regionMap").GetComponent<Tilemap>());
            PlaceTrees();
            LoadRegionTiles(fullmap);
            playerObj = Instantiate(player, new Vector3(playerSpawnTile.x, playerSpawnTile.y), Quaternion.identity);
            Camera.main.transform.position = new Vector3(playerObj.transform.position.x, playerObj.transform.position.y, -10);
            Camera.main.transform.parent = playerObj.transform;

            mapLoaded = false;


        }
        if(playerObj != null)
        {
            playerObj.GetComponent<SpriteRenderer>().sortingOrder=regionSize.y-(int)playerObj.transform.position.y-1;
        }
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

    public int[,] CreateGrid(List<Vector2Int> regionTiles, TileType[] supportedTileTypes, int[,] sourceGrid, int desiredWidth, int desiredHeight)
    {
        int[,] grid = InitializeGridWithWater(desiredWidth, desiredHeight);

        // Convert the supported types to a HashSet for efficient lookups
        HashSet<TileType> supportedTypes = new HashSet<TileType>(supportedTileTypes);

        // Determine the bounding box of the regionTiles
        int minX = regionTiles.Min(tile => tile.x);
        int maxX = regionTiles.Max(tile => tile.x);
        int minY = regionTiles.Min(tile => tile.y);
        int maxY = regionTiles.Max(tile => tile.y);

        int offsetX = (desiredWidth - (maxX - minX + 1)) / 2;
        int offsetY = (desiredHeight - (maxY - minY + 1)) / 2;

        // Place the region tiles into the grid, filtering by supported tile types
        foreach (Vector2Int tile in regionTiles)
        {
            TileType currentType = (TileType)sourceGrid[tile.x, tile.y];

            if (supportedTypes.Contains(currentType))
            {
                grid[tile.x - minX + offsetX, tile.y - minY + offsetY] = (int)currentType;
            }
        }

        return grid;
    }
    public void PlaceTrees()
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
                        var sr = tree.AddComponent<SpriteRenderer>();
                        sr.sprite = trees[Random.Range(0, trees.Length)];
                        sr.sortingOrder = regionSize.y-(int)tree.transform.position.y;

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
                        

                    }
                }
            }
        }
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


