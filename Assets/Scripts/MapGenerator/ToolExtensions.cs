using Unity;
using System;
using UnityEngine;
namespace ToolExtensions
{

    public static class ChunkTools
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
        public static Chunk[] To1DArray(Chunk[,] input)
        {
            // Step 1: get total size of 2D array, and allocate 1D array.
            int size = input.Length;
            Chunk[] result = new Chunk[size];

            // Step 2: copy 2D array elements into a 1D array.
            int write = 0;
            for (int i = 0; i <= input.GetUpperBound(0); i++)
            {
                for (int z = 0; z <= input.GetUpperBound(1); z++)
                {
                    result[write++] = input[i, z];
                }
            }
            // Step 3: return the new array.
            return result;
        }




        /// <summary>
        /// Enum to define various tile types.
        /// </summary>
    }
}