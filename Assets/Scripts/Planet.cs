using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Planet : MonoBehaviour
{
    [SerializeField] bool autoUpdate = true; // Planet will auto re-generate if values are changed in the inspector
    [SerializeField] [Range(2, 256)] public int resolution = 10; // The resolution of the meshes on each side of the quad sphere

    [SerializeField] [Range(0.1f, 10.0f)] public float radius = 1; // The radius of the planet
    [SerializeField] Material material; // The material that the planet uses
    [SerializeField] Gradient gradient; // The colour gradient that the material will use
    Gradient lastGradient; // Gradient of the planet on it's last generation. Used to prevent constantly creating a new texture2D every time the planet is updated

    [SerializeField] List<NoiseSettings> noiseSettings;

    MeshFilter[] meshFilters; // The meshes for each side of the planet
    PlanetSide[] planetSides; // The PlanetSides for each side of the planet

    // Calls whenever scripts are reloaded in editor
    private void OnValidate()
    {
        if (autoUpdate) GeneratePlanet();
    }

    // Initialises the planet and sets up the mesh for each side of the planet's quad sphere
    private void Init()
    {
        if (meshFilters == null) meshFilters = new MeshFilter[6]; // 6 mesh filters for each side of the planet's quad sphere
        planetSides = new PlanetSide[6]; // Array that will contain each of the planets sides
        Vector3[] allDirections = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back }; // Array containing all 6 directions

        // If a planet material is missing use default unity shader
        if (material == null) material = new Material(Shader.Find("Standard"));

        for (int i = 0; i < 6; ++i)
        {
            // The gameobject that the mesh will live on
            GameObject meshObject;

            // There is currently no exisiting gameobject for this mesh & side in the scene, create a new one
            if (transform.Find("Mesh " + i) == null)
            {
                // Create GameObject for the mesh and parent it to the planet
                meshObject = new GameObject("Mesh " + i);
                meshObject.transform.parent = this.transform;

                // Give the GameObject a mesh renderer and apply default material
                meshObject.AddComponent<MeshRenderer>().sharedMaterial = material;

                // Add mesh filter and give it a new mesh
                meshFilters[i] = meshObject.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }

            // The gameobject is in the scene but the mesh filter for this side is empty (happens after a unity restart)
            else if (meshFilters[i] == null)
            {
                // Get the mesh filter from the gameobject
                meshObject = transform.Find("Mesh " + i).gameObject;

                // If the gameobject is missing a mesh renderer component for whatever reason then generate a new one
                if (meshObject.GetComponent<MeshRenderer>() == null)
                {
                    meshObject.AddComponent<MeshRenderer>().sharedMaterial = material;
                }
                // Same as above but for the mesh filter component
                if (meshObject.GetComponent<MeshFilter>() == null)
                {
                    meshObject.AddComponent<MeshFilter>().sharedMesh = new Mesh();
                }

                // Set mesh filter in the array
                meshFilters[i] = meshObject.GetComponent<MeshFilter>();
            }

            // Set up PlanetSide constructors so mesh can be generated on each face of the quad sphere
            planetSides[i] = new PlanetSide(meshFilters[i].sharedMesh, resolution, allDirections[i], radius, noiseSettings);
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

    // Updates the planets material
    private void UpdateMaterial()
    {
        // If the gradient has changed update the shader's texture2D sample
        if (lastGradient == null || gradient.Equals(lastGradient))
        {
            // Create a new texture based off the gradient that the shader can sample from
            // Wouldn't need to do this if shadergraph could expose gradients to the editor :)

            Debug.Log(gradient.colorKeys);
            Debug.Log(lastGradient.colorKeys);

            int textureWidth = 256; // Width of the texture
            Texture2D texture = new Texture2D(textureWidth, 1); // Create a new texture

            for (int j = 0; j < textureWidth; ++j)
            {
                texture.SetPixel(j, 0, gradient.Evaluate(j / (textureWidth - 1.0f))); // Set each pixel in the texture to the corresponding colour in the gradient
            }
            texture.Apply();

            material.SetTexture("_GradientTex", texture); // Set new texture on the shader
        }
        // Update last gradient value
        gradient = lastGradient;

        // Update elevation values on the material
        Vector2 elevationValues = GetElevationMinMax(planetSides);
        material.SetFloat("_MinHeight", elevationValues.x + 1.0f);
        material.SetFloat("_MaxHeight", elevationValues.y + 1.0f);

        material.SetFloat("_RangeSize", elevationValues.y - elevationValues.x);

        // Updating the material for each planet side
        for (int i = 0; i < 6; ++i)
        {
            MeshRenderer renderer = transform.Find("Mesh " + i).GetComponent<MeshRenderer>();
            if (renderer != null) renderer.sharedMaterial = material;
        }
    }

    // Completely generates a new planet
    public void GeneratePlanet()
    {
        Init();
        GenerateMesh();
        UpdateMaterial();
    }

    // Gets maximum and minimum elevation values for the whole planet
    private Vector2 GetElevationMinMax(PlanetSide[] a_planetSides)
    {
        Vector2 elevationMinMax = new Vector2(float.MaxValue, float.MinValue);
        for (int i = 0; i < a_planetSides.Length; ++i)
        {
            if (a_planetSides[i] != null)
            {
                Vector2 sideElevations = a_planetSides[i].GetElevationMinMax();
                elevationMinMax.x = Mathf.Min(elevationMinMax.x, sideElevations.x);
                elevationMinMax.y = Mathf.Max(elevationMinMax.y, sideElevations.y);
            }
        }
        return elevationMinMax;
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
