using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
class Chunk
{
    public Vector2 center;
    public float height; public float width;
    public int[,] map;

    public Chunk(Vector2 _center, float _height, float _width, int[,] _map)
    {
        center = _center;
        height = _height;
        width = _width;
        map = _map;
    }
}

class Square
{
    //This class is used to store the location and color of each square
    public SpriteRenderer sprite;
    public int x;
    public int y;

    public Square(SpriteRenderer _sprite, int _x, int _y)
    {
        sprite = _sprite;
        x = _x;
        y = _y;
    }
}
public class MapGenerator : MonoBehaviour
{


    //Size of a Chunk
    [Tooltip("Don't set this >32. may crash")]
    public Vector2Int chunkDimension;
    //Chunk Grid Size
    [Tooltip("Dont set this to a number > 10. Will definitly Crash")]
    public Vector2Int chunksGridSize;
    //Randomizer
    public string seed;
    public bool useRandom;

    public int maxWaterFillPercentOffset;
    public int minWaterFillPercentOffset;

    [Range(0, 100)]
    public int waterFillPercent;

    //Colors for Island and Water 
    //TODO To be replaced with textures
    public Color waterColor;
    public Color islandColor;

    //All loaded Square Prefabs in the Scene with their position in the grid
    List<Square> squares = new List<Square>();

    //Square Prefab to load
    public GameObject squarePrefab;

    //Smoothing
    public int iterations;

    

    public float totalGridWidth = 10f;  // Total width of the grid in Unity units
    public float totalGridHeight = 10f; // Total height of the grid in Unity units

    Chunk[,] chunks;
    private void Start()
    {
        GenerateChunkGrid();
    }
    void GenerateChunkGrid()
    {
        chunks = new Chunk[chunksGridSize.x, chunksGridSize.y];
        for (int x = 0; x < chunksGridSize.x; x++)
        {
            for (int y = 0; y < chunksGridSize.y; y++)
            {
                Vector2 pos = new Vector2Int(x, y);
                chunks[x, y] = new Chunk(pos, chunkDimension.x, chunkDimension.y, FillMapRandom(chunkDimension.x, chunkDimension.y));
            }
        }
        LoadInSquares();
        for (int x = 0; x < chunksGridSize.x; x++)
        {
            for (int y = 0; y < chunksGridSize.y; y++)
            {
                for (int i = 0; i < iterations; i++)
                {
                    chunks[x, y].map = SmoothMap(chunkDimension.x, chunkDimension.y, chunks[x, y].map);
                }
            }
        }
        UpdateSquares(chunks);

    }
    int[,] FillMapRandom(int width, int height)
    {
        int[,] newMap = new int[width, height];
        if (useRandom)
        {
            seed = Time.time.ToString() + Random.Range(0, 2000);
        }
        System.Random rand = new System.Random(seed.GetHashCode());

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int currentFill = waterFillPercent + rand.Next(minWaterFillPercentOffset, maxWaterFillPercentOffset);
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    newMap[x, y] = 1;
                }
                else
                {
                    if (rand.Next(0, 100) < currentFill)
                    {
                        newMap[x, y] = 1;
                    }
                    else
                    {
                        newMap[x, y] = 0;
                    }
                }
            }
        }
        return newMap;
    }
    int[,] SmoothMap(int width, int height, int[,] map)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbourCount = GetNeighbourCount(x, y, map);

                if (neighbourCount > 4)
                {
                    map[x, y] = 1;
                }
                else if (neighbourCount < 4)
                {
                    map[x, y] = 0;
                }
            }
        }
        return map;
    }
    int GetNeighbourCount(int gridX, int gridY, int[,] map)
    {
        int count = 0;

        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < chunkDimension.x & neighbourY >= 0 && neighbourY < chunkDimension.y)
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        count += map[neighbourX, neighbourY];
                    }
                }
                else
                {
                    count++;
                }
            }
        }
        return count;
    }

    void LoadInSquares()
    {
        float squareWidth = totalGridWidth / (chunksGridSize.x * chunkDimension.x);
        float squareHeight = totalGridHeight / (chunksGridSize.y * chunkDimension.y);

        for (int chunkX = 0; chunkX < chunksGridSize.x; chunkX++)
        {
            for (int chunkY = 0; chunkY < chunksGridSize.y; chunkY++)
            {
                GameObject chunkHolder = new GameObject();
                chunkHolder.transform.position = new Vector2(chunkX * chunkDimension.x * squareWidth, chunkY * chunkDimension.y * squareHeight);

                for (int x = 0; x < chunkDimension.x; x++)
                {
                    for (int y = 0; y < chunkDimension.y; y++)
                    {
                        GameObject squareObj = Instantiate(squarePrefab, new Vector2(chunkHolder.transform.position.x + x * squareWidth, chunkHolder.transform.position.y + y * squareHeight), Quaternion.identity, chunkHolder.transform);
                        squareObj.transform.localScale = new Vector2(squareWidth, squareHeight); // Scale the square

                        SpriteRenderer render = squareObj.GetComponent<SpriteRenderer>();
                        Square square = new Square(render, chunkX * chunkDimension.x + x, chunkY * chunkDimension.y + y);
                        squares.Add(square);
                    }
                }
            }
        }
    }

    void UpdateSquares(Chunk[,] chunks)
    {
        for (int chunkX = 0; chunkX < chunks.GetLength(0); chunkX += 1)
        {
            for (int chunkY = 0; chunkY < chunks.GetLength(1); chunkY += 1)
            {
                for (int x = 0; x < chunks[chunkX, chunkY].map.GetLength(0); x++)
                {
                    for (int y = 0; y < chunks[chunkX, chunkY].map.GetLength(1); y++)
                    {
                        Square square = squares.FirstOrDefault(e => e.x == x + chunkX * chunkDimension.x && e.y == y + chunkY * chunkDimension.y);
                        if (square != null)
                        {
                            if (chunks[chunkX, chunkY].map[x, y] == 1)
                            {
                                square.sprite.color = waterColor;
                            }
                            else
                            {
                                square.sprite.color = islandColor;
                            }
                        }
                        else
                        {
                            Debug.LogError("No matching square found");
                        }
                        // Yield control back to Unity to allow for screen updates.
                    }
                }
            }
        }
    }
}
