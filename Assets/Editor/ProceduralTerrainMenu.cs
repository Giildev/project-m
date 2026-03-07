using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProceduralTerrainMenu
{
    [MenuItem("Tools/Project M/Generate Worms Terrain")]
    public static void GenerateWormsTerrain()
    {
        bool changed = false;

        // 1. Delete old grounds 
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var obj in rootObjects)
        {
            if (obj == null) continue;
            
            if (obj.name.ToLower().Contains("ground") || (obj.name.ToLower().Contains("cube") && obj.transform.localScale.x > 3))
            {
                Object.DestroyImmediate(obj);
                Debug.Log($"[Worms Terrain] Destroyed old flat ground.");
                changed = true;
            }
        }

        // 2. Ensure Material exists
        string matPath = "Assets/Materials/GrassGroundMat.mat";
        Material dirtMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (dirtMat == null)
        {
            dirtMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (dirtMat.shader == null || !dirtMat.shader.isSupported) dirtMat = new Material(Shader.Find("Standard"));
            if (!AssetDatabase.IsValidFolder("Assets/Materials")) AssetDatabase.CreateFolder("Assets", "Materials");
            AssetDatabase.CreateAsset(dirtMat, matPath);
        }

        Texture2D baseTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/TerrainSampleAssets/Textures/Terrain/Grass_A_BaseColor.tif");
        if (baseTex != null)
        {
            dirtMat.mainTexture = baseTex;
            if (dirtMat.HasProperty("_BaseMap")) dirtMat.SetTexture("_BaseMap", baseTex);
            // Texture scale so it repeats nicely on world coordinates
            dirtMat.mainTextureScale = new Vector2(0.2f, 0.2f);
        }

        // 3. Delete existing WormsTerrain if the user is regenerating
        GameObject existing = GameObject.Find("WormsTerrain");
        if (existing) Object.DestroyImmediate(existing);

        // 4. Create the Procedural Terrain GameObject
        GameObject terrainGO = new GameObject("WormsTerrain");
        // Center it below the origin
        terrainGO.transform.position = new Vector3(0, -2f, 0);

        // 5. Add the generator component
        Procedural2DTerrain generator = terrainGO.AddComponent<Procedural2DTerrain>();
        
        // 6. Configure values
        generator.width = 120f; 
        generator.heightMultiplier = 5f; // Pronounced hills
        generator.noiseScale = 0.08f; // Frequency of hills
        generator.groundDepth = 20f; // Deep ground
        generator.terrainMaterial = dirtMat;

        // Force generate
        generator.GenerateTerrain();
        
        terrainGO.layer = LayerMask.NameToLayer("Default");

        Debug.Log("[Worms Terrain] Successfully generated irregular wavy terrain from Menu!");
        
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }
}
