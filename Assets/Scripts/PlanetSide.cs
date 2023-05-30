using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Each planet has 6 sides like a cube
// Each side has it's own mesh, and direction and resolution of the mesh
public class PlanetSide
{
    Mesh mesh; // The mesh of this face of the planet
    int resolution; // How detailed the mesh will be. The mesh will have a resolution x resolution sized grid of vertices

    Vector3 localY; // The local Y direction of this mesh. Would be up if you were to stand on the mesh's surface
    Vector3 localX; // ^ local X axis
    Vector3 localZ; // ^ local Z axis

    float radius; // Radius of the planet

    Vector2 elevationMixMax = new Vector2(float.MaxValue, float.MinValue); // Maximum and minimum elevation values of this planet face

    List<NoiseSettings> noiseLayers;

    // Constructor
    public PlanetSide(Mesh a_mesh, int a_resolution, Vector3 a_localY, float a_radius, List<NoiseSettings> a_noise)
    {
        // Setting variables
        mesh = a_mesh;
        resolution = a_resolution;
        localY = a_localY;
        radius = a_radius;
        noiseLayers = a_noise;

        // Calculating other 2 local axes
        localX = new Vector3(localY.y, localY.z, localY.x);
        localZ = Vector3.Cross(localY, localX); // Cross product will return direction perpendicular to both Z and Y, making the X axis
    }

    // Creates the mesh of this planet face
    public void CreateMesh()
    {
        // Array that will hold the positon of every vertex on the mesh
        Vector3[] vertices = new Vector3[resolution * resolution];

        // Array that holds the index of every triangle on the mesh
        // The size of the array is equivalent to the total number of points on every triangle
        // The amount of squares in the array is (resolution - 1)^2. The amount of triangles is just double that. Then times 3 for every point on the triangles.
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];

        // Array that holds the normals for each vertex
        Vector3[] normals = new Vector3[resolution * resolution];

        // For loop that iterates through every single vertex
        int iterations = 0; // How many vertices has been iterated through in the for loops
        int triIndex = 0; // How many entries have been added to the triangles array
        for (int y = 0; y < resolution; y++)
        {
            float progressY = y / (resolution - 1.0f); // How far through iterating the y axis we are

            for (int x = 0; x < resolution; x++)
            {
                float progressX = (float)x / (resolution - 1); // How far through iterating the x axis we are

                // Finding the point where this vertex lies on this side of the planet's quad sphere
                // The far left corner (first vertex) would have the coordinates X:-1 Y:1 Z:-1 and the opposite corner would be X:1 Y:1 Z:1 (if localY was an upwards vector)
                // It is then normalised to transform the cube shape into a sphere (making a quad sphere)
                Vector3 pointOnMesh = (localY + (((progressX * 2.0f) - 1.0f) * localX) + (((progressY * 2.0f) - 1.0f) * localZ)).normalized;

                // Add this vertex to the vertex array, also multiply by radius
                vertices[iterations] = pointOnMesh * (radius);

                // Apply noise layers
                float totalNoiseValue = 0.0f;
                for (int i = 0; i < noiseLayers.Count; ++i)
                {
                    totalNoiseValue += noiseLayers[i].GetNoiseFromPoint(pointOnMesh) + 0.5f;
                }
                if (totalNoiseValue != 0.0f) vertices[iterations] *= totalNoiseValue;

                // Check for minimum and maximum value
                elevationMixMax.x = Mathf.Min(elevationMixMax.x, vertices[iterations].magnitude);
                elevationMixMax.y = Mathf.Max(elevationMixMax.y, vertices[iterations].magnitude);

                // Calculate normals
                normals[iterations] = pointOnMesh;

                // Adding the triangles to their array
                // Do not generate triangles on vertices that are on the edge of the mesh
                if (x != resolution - 1 && y != resolution - 1)
                {
                    // Each vertex has two triangles
                    triangles[triIndex] = iterations;
                    triangles[triIndex + 1] = iterations + resolution + 1;
                    triangles[triIndex + 2] = iterations + resolution;

                    triangles[triIndex + 3] = iterations;
                    triangles[triIndex + 4] = iterations + 1;
                    triangles[triIndex + 5] = iterations + resolution + 1;

                    // Triangle index increases by six for each iteration
                    triIndex += 6;
                }

                // One iteration through the vertices
                iterations++;
            }
        }

        // Clear any pre-existing mesh data
        mesh.Clear();

        // Set mesh vertices and triangles to newly calculated ones
        // Also update the normals so the lighting on the planet looks normal
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    // Returns the highest and lowest elevation values on this planet face
    public Vector2 GetElevationMinMax() { return elevationMixMax; }
}
