using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed;
    float moveX;
    float moveY;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && Vector2Int.Distance(GameManager.instance.parkedShipPos, new Vector2Int((int)transform.position.x, (int)transform.position.y)) < 2f)
        {
            GameManager.instance.LoadMap();
        }
        moveX = Input.GetAxisRaw("Horizontal");
        moveY = Input.GetAxisRaw("Vertical");
        transform.Translate(new Vector3(moveX, moveY, 0) * speed * Time.deltaTime);
        GetComponent<SpriteRenderer>().sortingOrder = (int)transform.position.y;
    }
}
