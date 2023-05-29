// Ignore Spelling: lacunarity

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseSettings
{
    public float frequency; // The frequency of the perlin noise. As this value rises so will the frequency of elevation changes
    public float strength; // The strength of the perlin noise. As this value rises so will the peaks in elevation

    [Range(1,6)]public int octaves; // The amount of layers of noise
    public float lacunarity; // The rate of change of frequency between each layer of noise
    public float persistence; // The rate of change of strength between each layer of noise

    public Vector3 noiseOffset; // The origin of the noise. Unity's Perlin noise mirrors at 0,0 so providing a vec3 offset will prevent a symmetrical looking planet

    public float minimumHeight; // Minimum value possible for the noise. Essentially acts as a sea level for the planet

    // Returns a float between 0 and 1 based off the passed in vector 3 and other variables in the script
    public float GetNoiseFromPoint(Vector3 pointOnSphere)
    {
        pointOnSphere += noiseOffset; // Adding offset to the point for the noise to be generated
        float noiseValue = 0.0f; // Final noise value
        float freqAfterLacunarity = frequency; // Frequency after lacunarity has been applied
        float strengthAfterPersistence = strength;

        // Loop through every octave
        for (int i = 0; i < octaves; ++i)
        {
            // Modifying the position so that frequency is applied
            Vector3 octavePoint = pointOnSphere * freqAfterLacunarity;

            // Separating the vector 3 values into floats for simplicity
            float x = octavePoint.x;
            float y = octavePoint.y;
            float z = octavePoint.z;

            float xy = Mathf.PerlinNoise(x, y);
            float xz = Mathf.PerlinNoise(x, z);
            float yx = Mathf.PerlinNoise(y, x);
            float yz = Mathf.PerlinNoise(y, z);
            float zx = Mathf.PerlinNoise(z, x);
            float zy = Mathf.PerlinNoise(z, y);

            // Average of all 6 permutations leads to 3D noise
            // This has the downside of having a bell-curve at the value of 0.5, but for now is passable
            // This should be changed though
            noiseValue += (xy + xz + yx + yz + zx + zy) * strengthAfterPersistence / (6.0f);

            // Apply lacunarity and persistence for next octave
            freqAfterLacunarity *= lacunarity;
            strengthAfterPersistence *= persistence;
        }

        // Average out noise value between each octave and apply noise strength
        return Mathf.Max(minimumHeight, noiseValue * strength);
    }
}
