using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShipController : MonoBehaviour
{
    Vector2 windDir = Vector2.zero;
    float windTurnTime = 10f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator RandomizeWindDir()
    {
        while (true)
        {
            windDir = Random.insideUnitCircle.normalized;
        }
    }
}
