using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class placeCam : MonoBehaviour
{

    public Vector2 pos = Vector2.zero;
    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(pos.x,pos.y,-10);
    }
}
