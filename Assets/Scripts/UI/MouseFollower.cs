using Inventory.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MouseFollower : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;


    [SerializeField]
    private InventoryItem item;
    void Awake()
    {
        canvas = transform.root.GetComponent<Canvas>();
  
        item = GetComponentInChildren<InventoryItem>();
    }

    // Update is called once per frame
    public void SetData(Sprite sprite, int quantity)
    {
        item.SetData(sprite, quantity);
    }
    private void Update()
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            Input.mousePosition,
            canvas.worldCamera,
            out pos);

        transform.position = canvas.transform.TransformPoint(pos);
    }
    public void Toggle(bool val)
    {
        Debug.Log($"Item toggled {val}");
        gameObject.SetActive( val );
    }

}
