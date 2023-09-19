using UnityEngine;
using System.Collections.Generic;

public class RegionData : MonoBehaviour
{
    public List<Vector2Int> RegionTiles;

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}