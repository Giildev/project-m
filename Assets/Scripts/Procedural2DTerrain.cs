using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Procedural2DTerrain : MonoBehaviour
{
    [Header("Terrain Settings")]
    public float width = 50f;
    public float heightMultiplier = 5f;
    public float noiseScale = 0.1f;
    public int resolution = 100;
    public float groundDepth = 10f;
    public float zDepth = 2f; // Thickness of the 2.5D layer
    
    [Header("Material")]
    public Material terrainMaterial;
    
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private Mesh mesh;

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        
        if (terrainMaterial != null)
        {
            GetComponent<MeshRenderer>().sharedMaterial = terrainMaterial;
        }

        GenerateTerrain();
    }

    [ContextMenu("Regenerate Terrain")]
    public void GenerateTerrain()
    {
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
        if (meshCollider == null) meshCollider = GetComponent<MeshCollider>();

        if (terrainMaterial != null)
        {
            GetComponent<MeshRenderer>().sharedMaterial = terrainMaterial;
        }

        mesh = new Mesh();
        mesh.name = "Procedural Terrain 3D";

        int vertsPerEdge = resolution + 1;
        Vector3[] vertices = new Vector3[vertsPerEdge * 4];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[resolution * 12]; // 6 for front, 6 for top

        float stepX = width / resolution;
        float startX = -width / 2f;
        float zFront = -zDepth / 2f;
        float zBack = zDepth / 2f;

        for (int i = 0; i <= resolution; i++)
        {
            float x = startX + (i * stepX);
            float worldX = transform.position.x + x;
            float y = Mathf.PerlinNoise(worldX * noiseScale, 0f) * heightMultiplier;

            // --- Front Face ---
            vertices[i] = new Vector3(x, y, zFront);
            uv[i] = new Vector2(x, y); 
            
            vertices[i + vertsPerEdge] = new Vector3(x, -groundDepth, zFront);
            uv[i + vertsPerEdge] = new Vector2(x, -groundDepth);

            // --- Top Face ---
            vertices[i + vertsPerEdge * 2] = new Vector3(x, y, zFront);
            uv[i + vertsPerEdge * 2] = new Vector2(x, 0); 
            
            vertices[i + vertsPerEdge * 3] = new Vector3(x, y, zBack);
            uv[i + vertsPerEdge * 3] = new Vector2(x, zDepth);
        }

        int tris = 0;
        // Generate Triangles
        for (int i = 0; i < resolution; i++)
        {
            // Front Face Triangles (Clockwise for Front)
            triangles[tris + 0] = i;              // Top-Left
            triangles[tris + 1] = i + 1;          // Top-Right
            triangles[tris + 2] = i + vertsPerEdge;// Bottom-Left

            triangles[tris + 3] = i + 1;
            triangles[tris + 4] = i + vertsPerEdge + 1; // Bottom-Right
            triangles[tris + 5] = i + vertsPerEdge;

            // Top Face Triangles (Clockwise looking down from +Y)
            int tTop = i + vertsPerEdge * 2;
            triangles[tris + 6] = tTop;                   // Front-Left
            triangles[tris + 7] = tTop + vertsPerEdge;    // Back-Left
            triangles[tris + 8] = tTop + 1;               // Front-Right

            triangles[tris + 9] = tTop + 1;
            triangles[tris + 10] = tTop + vertsPerEdge;
            triangles[tris + 11] = tTop + vertsPerEdge + 1; // Back-Right

            tris += 12;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }
}
