using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
/// <summary>
/// Enum to define various tile types.
/// </summary>
public enum TileType
{
    Island = 0,
    Water = 1,
    Sand = 2,
    SmallGrass = 3,
    Empty = 4,
    MediumGrass = 5,
    BigGrass = 6,
    SmallFlowerRed = 7,
    SmallFlowerYellow = 8,
    MediumFlowerRed = 9,
    MediumFlowerYellow = 10,
    shallowWater = 11,
    lowWater = 12,
    mediumWater = 13,
    deepWater = 14
    // ... add other types if needed
}

/// <summary>
/// Represents a base class for tiles.
/// </summary>
[System.Serializable]
public struct TileValue
{
    public TileType Type; // The type of tile this represents
    public Tile tile;
    // You can add other attributes or functionalities that you want to be common to all tiles here.
}

/// <summary>
/// Represents a chunk of tiles.
/// </summary>

public class Chunk
{
    public Vector2Int center;
    public int[,] map;
    public Vector2Int size;
    public List<List<Vector2Int>> regions = new List<List<Vector2Int>>();
    public Chunk(Vector2Int _center, Vector2Int _size, int[,] _map, List<List<Vector2Int>> _regions)
    {
        center = _center;
        map = _map;
        size = _size;
        regions = _regions;
    }
}
public class Map
{
    public int id;
    public Vector2Int center;
    public int[,] fullMap;
    public Chunk[,] chunks;
    public List<List<Vector2Int>> regions;
    public Vector2Int gridSize;
    public Vector2Int chunkSize;
    public Map(int ID, Vector2Int _center, int[,] map, Chunk[,] _chunks, List<List<Vector2Int>> _regions,Vector2Int _gridSize, Vector2Int _chunkSize)
    {
        id = ID;
        center = _center;
        fullMap = map;
        chunks = _chunks;
        regions = _regions;
        gridSize = _gridSize;
        chunkSize = _chunkSize;
    }
}
public class DataTile
{
    public Vector2Int Position { get; set; }
    public int Distance { get; set; }

    public DataTile(Vector2Int position, int distance)
    {
        Position = position;
        Distance = distance;
    }
}

/// <summary>
/// Represents data about a specific tile.
/// </summary>

