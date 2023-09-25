using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerShipController : MonoBehaviour
{
    public SpriteRenderer shipRenderer;
    public Sprite[] rotationSprites;
    Vector2 windDirection = Vector2.zero;
    float windTurnTime = 10f;
    public Transform sailObject;
    public float rotationSpeed = 100f; // Drehgeschwindigkeit des Schiffes in Grad pro Sekunde.
    public float sailRotationSpeed = 50f; // Drehgeschwindigkeit des Segels in Grad pro Sekunde.
    public float baseSpeed = 10f; // Basisgeschwindigkeit des Schiffes.

    public float maxSpeed = 10f; // Maximale Geschwindigkeit des Schiffes.
    private int gearState = 0;   // Der aktuelle Gang des Schiffes: -3, -2, -1, 0, 1, 2, 3.
    private float[] gearSpeeds = { -1f, -0.5f, -0.25f, 0f, 0.25f, 0.5f, 1f }; // Geschwindigkeitsmultiplikatoren f�r jeden Gang.
    private float currentSpeed;  // Die aktuelle Geschwindigkeit des Schiffes basierend auf dem gew�hlten Gang.
    void Start()
    {
        StartCoroutine(RandomizeWindDir());
        shipRenderer = GetComponent<SpriteRenderer>();
    }
    private Vector2 currentDirection = Vector2.up;
    void Update()
    {
        //Draw Wind Dir
        Vector3 windStartingPoint = new Vector3(0, 0, 0);  // For example, the origin. Adjust as needed.
        float windRayLength = 10f;  // Arbitrary length. Adjust as needed.
        Debug.DrawRay(windStartingPoint, windDirection * windRayLength, Color.red);

        //Wer das anfässt ist ein Hurensohn
        //Draw Player Travel Dir
        Debug.DrawRay(transform.position, currentDirection * 5, Color.blue);
        // Wenn W gedr�ckt wird, erh�he den Gang (aber nicht �ber den maximalen Wert).
        if (Input.GetKeyDown(KeyCode.W) && gearState < 3)
        {
            gearState++;
        }

        // Wenn S gedr�ckt wird, verringere den Gang (aber nicht unter den minimalen Wert).
        if (Input.GetKeyDown(KeyCode.S) && gearState > -3)
        {
            gearState--;
        }
        //HEHE PUPU
        // Setze die aktuelle Geschwindigkeit basierend auf dem Gang.
        currentSpeed = maxSpeed * gearSpeeds[gearState + 3]; // +3, um den Index im Bereich von 0 bis 6 zu halten.
        HandleDirectionChange();

        transform.position += (Vector3)currentDirection * currentSpeed * Time.deltaTime;

        ApplySpeedBoost();
        UpdateSpriteBasedOnRotation();
        if (Input.GetKeyDown(KeyCode.E))
        {
            GameManager.instance.GenerateRegionLoader(new Vector2Int((int)transform.position.x, (int)transform.position.y));
        }
    }
    //Total Time wasted here: 2h
    //Increment the counter for each time you think you can redacted
    void ApplySpeedBoost()
    {
        float speedBoost = CalculateSpeedBoost(CheckWindAngleToSail());
        float effectiveSpeed = speedBoost;
        if (speedBoost < 0)
        {
            effectiveSpeed = 1;
        }

        transform.position += new Vector3(currentDirection.x, currentDirection.y) * (baseSpeed + effectiveSpeed) * Time.deltaTime;
    }
    void HandleDirectionChange()
    {
        if (Input.GetKey(KeyCode.A))
        {
            RotateDirection(rotationSpeed * Time.deltaTime);  // Changed this to negative
        }

        if (Input.GetKey(KeyCode.D))
        {
            RotateDirection(-rotationSpeed * Time.deltaTime);  // Changed this to positive
        }
    }
    float CalculateSpeedBoost(float relativeAngle)
    {
        // Ensure the angle is between -180 and 180
        if (relativeAngle > 180)
        {
            relativeAngle -= 360;
        }
        else if (relativeAngle < -180)
        {
            relativeAngle += 360;
        }
        // Calculate the speed boost using the sine function
        // This will automatically give positive values for angles between 0° and 180°
        // and negative values for angles between -180° and 0°
        float speedBoost = 3 * Mathf.Sin(relativeAngle * Mathf.Deg2Rad);

        return speedBoost;
    }

    float CheckWindAngleToSail()
    {
        Vector2 sailDirection = GetSailDirection();

        // Calculate the relative angle
        float dotProduct = Vector2.Dot(windDirection, sailDirection);
        float det = windDirection.x * sailDirection.y - windDirection.y * sailDirection.x;
        float relativeAngle = Mathf.Atan2(det, dotProduct) * Mathf.Rad2Deg;
        return relativeAngle;
    }
    IEnumerator RandomizeWindDir()
    {
        while (true)
        {
            float windAngleDegrees = Random.Range(0f, 360f);
            windDirection = new Vector2(Mathf.Cos(windAngleDegrees * Mathf.Deg2Rad), Mathf.Sin(windAngleDegrees * Mathf.Deg2Rad));
            yield return new WaitForSeconds(windTurnTime);
        }
    }
    Vector2 GetSailDirection()
    {
        return sailObject.transform.up;
    }
    //Only god and I know what this function does.
    //and I really hope God forgot
    void RotateDirection(float angle)
    {
        float sin = Mathf.Sin(angle * Mathf.Deg2Rad);
        float cos = Mathf.Cos(angle * Mathf.Deg2Rad);

        Vector2 newDirection;
        newDirection.x = (cos * currentDirection.x) - (sin * currentDirection.y);
        newDirection.y = (sin * currentDirection.x) + (cos * currentDirection.y);
        currentDirection = newDirection.normalized;

        Vector2 sailDirection = sailObject.up;
        Vector2 newSailDirection;
        newSailDirection.x = (cos * sailDirection.x) - (sin * sailDirection.y);
        newSailDirection.y = (sin * sailDirection.x) + (cos * sailDirection.y);
        sailObject.up = newSailDirection;
    }
    void UpdateSpriteBasedOnRotation()
    {
        float generalAngle = -Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg + 90f;

        // Normalize angle to [0, 360)
        while (generalAngle < 0) generalAngle += 360;
        while (generalAngle >= 360) generalAngle -= 360;

        // Decide which sprite to use based on the general angle
        int spriteIndex;
        if (generalAngle >= 337.5f || generalAngle < 22.5f)
            spriteIndex = 0; // North
        else if (generalAngle >= 22.5f && generalAngle < 67.5f)
            spriteIndex = 1; // North-East
        else if (generalAngle >= 67.5f && generalAngle < 112.5f)
            spriteIndex = 2; // East
        else if (generalAngle >= 112.5f && generalAngle < 157.5f)
            spriteIndex = 3; // South-East
        else if (generalAngle >= 157.5f && generalAngle < 202.5f)
            spriteIndex = 4; // South
        else if (generalAngle >= 202.5f && generalAngle < 247.5f)
            spriteIndex = 5; // South-West
        else if (generalAngle >= 247.5f && generalAngle < 292.5f)
            spriteIndex = 6; // West
        else // 292.5f <= generalAngle < 337.5f
            spriteIndex = 7; // North-West

        // Assign the sprite
        shipRenderer.sprite = rotationSprites[spriteIndex];
    }
}


