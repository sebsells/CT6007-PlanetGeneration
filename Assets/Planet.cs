using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Planet : MonoBehaviour
{
    [Range(2, 256)] public int resolution = 10; // The resolution of the meshes on each side of the quad sphere
    [Range(0.1f, 10.0f)] public float radius = 1; // The radius of the planet

    MeshFilter[] meshFilters; // The meshes for each side of the planet
    PlanetSide[] planetSides; // The PlanetSides for each side of the planet

    // Calls whenever scripts are reloaded in editor
    private void OnValidate()
    {
        //GeneratePlanet();
    }

    // Initialises the planet and sets up the mesh for each side of the planet's quad sphere
    private void Init()
    {
        if (meshFilters == null) meshFilters = new MeshFilter[6]; // 6 mesh filters for each side of the planet's quad sphere
        planetSides = new PlanetSide[6]; // Array that will contain each of the planets sides
        Vector3[] allDirections = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back }; // Array containing all 6 directions

        for (int i = 0; i < 6; ++i)
        {
            GameObject meshObject;

            // There is currently no mesh gameobject for this side in the scene, create a new one
            if (transform.Find("Mesh " + i) == null)
            {
                // Create GameObject for the mesh and parent it to the planet
                meshObject = new GameObject("Mesh " + i);
                meshObject.transform.parent = this.transform;

                // Give the GameObject a mesh renderer and apply default material
                meshObject.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));

                // Add mesh filter and give it a new mesh
                meshFilters[i] = meshObject.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }

            // Gameobjects are in scene but filters are empty (happens after unity restart)
            else if (meshFilters[i] == null)
            {
                // Get the mesh filter from the gameobject
                meshObject = transform.Find("Mesh " + i).gameObject;
                MeshFilter meshFilter = meshObject.GetComponent<MeshFilter>();

                // If the gameobject is missing the mesh filter component for whatever reason then generate a new one
                if (meshFilter == null)
                {
                    meshFilter = meshObject.AddComponent<MeshFilter>();
                    meshFilter.sharedMesh = new Mesh();
                }

                // Set mesh filter in the array
                meshFilters[i] = meshFilter;
            }

            // Set up PlanetSide constructors so mesh can be generated on each face of the quad sphere
            planetSides[i] = new PlanetSide(meshFilters[i].sharedMesh, resolution, allDirections[i], radius);
        }
    }

    // Generates the meshes for each side of the planet's quad sphere
    private void GenerateMesh()
    {
        for (int i = 0; i < 6; ++i)
        {
            // Create mesh for each side of the quad sphere
            if (planetSides[i] != null) planetSides[i].CreateMesh();
        }
    }

    // Completely generates a new planet
    public void GeneratePlanet()
    {
        Init();
        GenerateMesh();
    }
}

// Button in editor that allows the planet to be regenerated
[CustomEditor(typeof(Planet))]
public class PlanetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Regenerate")) ((Planet)target).GeneratePlanet();
    }
}
