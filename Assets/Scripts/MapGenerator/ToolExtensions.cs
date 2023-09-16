using Unity;
using System;
using UnityEngine;

public static class ToolExtensions
{
    /// <summary>
    /// Generates a random number from a Gaussian distribution.
    /// </summary>
    /// <param name="mean">Mean of the distribution.</param>
    /// <param name="standardDeviation">Standard deviation of the distribution.</param>
    /// <returns>Random number from the Gaussian distribution.</returns>
    public static float NextGaussianFloat(float mean, float standardDeviation)
    {
        System.Random r = new System.Random();
        float u1 = 1.0f - (float)r.NextDouble();
        float u2 = 1.0f - (float)r.NextDouble();
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
        float randNormal = mean + standardDeviation * (float)randStdNormal;
        return Mathf.Clamp(randNormal, 0f, 1f);  // Ensuring the result stays between 0 and 1
    }
   
}

/// <summary>
/// Enum to define various tile types.
/// </summary>
