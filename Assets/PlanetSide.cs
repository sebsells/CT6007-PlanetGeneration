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

    // Constructor
    public PlanetSide(Mesh a_mesh, int a_resolution, Vector3 a_localY)
    {
        // Setting variables
        mesh = a_mesh;
        resolution = a_resolution;
        localY = a_localY;

        // Calculating other 2 directions
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

        // For loop that iterates through every single vertex
        int iterations = 0; // How many vertices has been iterated through in the for loops
        int triIndex = 0; // How many entries have been added to the triangles array
        for (int y = 0; y < resolution; y++)
        {
            float progressY = (float)y / (resolution - 1); // How far through iterating the y axis we are

            for (int x = 0; x < resolution; x++)
            {
                float progressX = (float)x / (resolution - 1); // How far through iterating the x axis we are

                // Finding the point where this vertex lies on this side of the planet's quad sphere
                // The far left corner (first vertex) would have the coordinates X:-1 Y:1 Z:-1 and the opposite corner would be X:1 Y:1 Z:1 (if localY was an upwards vector)
                Vector3 pointOnCubeMesh = localY + (((progressX * 2.0f) - 1.0f) * localX) + (((progressY * 2.0f) - 1.0f) * localZ);

                // Finding where that same point would be on the quad sphere. This is done by making the distance from the centre point to each vertex equal
                Vector3 pointOnSphereMesh = pointOnCubeMesh.normalized;

                // Add this vertex to the vertex array
                vertices[iterations] = pointOnSphereMesh;

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
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = vertices;
    }
}