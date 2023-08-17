using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerShipController : MonoBehaviour
{
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
    }
    void Update()
    {
        //Draw Wind Dir
        Vector3 windStartingPoint = new Vector3(0, 0, 0);  // For example, the origin. Adjust as needed.
        float windRayLength = 10f;  // Arbitrary length. Adjust as needed.
        Debug.DrawRay(windStartingPoint, windDirection * windRayLength, Color.red);

        //Wer das anfässt ist ein Hurensohn
        //Draw Player Travel Dir
        Debug.DrawRay(transform.position, transform.up * 5, Color.blue);
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

        // Bewege das Schiff basierend auf der aktuellen Geschwindigkeit.
        transform.position += transform.up * currentSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(-Vector3.forward * rotationSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            sailObject.transform.Rotate(Vector3.forward * sailRotationSpeed * Time.deltaTime);
        }

        // Wenn die E-Taste gedr�ckt wird, dreht das Segel nach rechts.
        if (Input.GetKey(KeyCode.E))
        {
            sailObject.transform.Rotate(-Vector3.forward * sailRotationSpeed * Time.deltaTime, Space.World);
        }

        ApplySpeedBoost();
    }
    //Total Time wasted here: 2h
    //Increment the counter for each time you think you can redacted
    void ApplySpeedBoost()
    {
        float speedBoost = CalculateSpeedBoost(CheckWindAngleToSail());
        float effectiveSpeed = baseSpeed + baseSpeed * speedBoost;

        transform.position += transform.up * effectiveSpeed * Time.deltaTime;

    }
    float CalculateSpeedBoost(float relativeAngle)
    {
        float speedBoost;
        // Use the sine function to get the speed boost, with a phase shift for negative angles
        if (relativeAngle > 180)
        {
            relativeAngle -= 360;
        }
        if (relativeAngle >= 0)
        {
            speedBoost = 3 * Mathf.Sin(relativeAngle * Mathf.Deg2Rad);
        }
        else
        {
            speedBoost = 3 * Mathf.Sin((relativeAngle + 180f) * Mathf.Deg2Rad);
        }
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
            //Nur Gott und ich Wissen was diese Funktion macht und ich glaube selbst Gott nicht mehr
            float windAngleDegrees = Random.Range(0f, 360f);
            windDirection = new Vector2(Mathf.Cos(windAngleDegrees * Mathf.Deg2Rad), Mathf.Sin(windAngleDegrees * Mathf.Deg2Rad));
            yield return new WaitForSeconds(windTurnTime);
        }
    }
    Vector2 GetSailDirection()
    {
        return sailObject.transform.up;
    }

}
