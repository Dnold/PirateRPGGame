using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class shipCamFollow : MonoBehaviour
{
    public Transform playerPos;
    void Start()
    {

    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (playerPos != null)
        {
            transform.position = new Vector3(playerPos.position.x, playerPos.position.y, -10);
        }

    }
}
