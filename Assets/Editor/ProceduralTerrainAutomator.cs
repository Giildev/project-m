using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class ProceduralTerrainAutomator
{
    static ProceduralTerrainAutomator()
    {
        EditorApplication.delayCall += InstantiateTerrain;
    }

    static void InstantiateTerrain()
    {
        if (SessionState.GetBool("ProceduralTerrainDone", false)) return;
        SessionState.SetBool("ProceduralTerrainDone", true);

        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        bool changed = false;

        // 1. Delete old flat grounds 
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
            // Create fallback if missing
            dirtMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (dirtMat.shader == null || !dirtMat.shader.isSupported) dirtMat = new Material(Shader.Find("Standard"));
            if (!AssetDatabase.IsValidFolder("Assets/Materials")) AssetDatabase.CreateFolder("Assets", "Materials");
            AssetDatabase.CreateAsset(dirtMat, matPath);
        }

        // 3. Create the Procedural Terrain GameObject
        GameObject terrainGO = new GameObject("WormsTerrain");
        terrainGO.transform.position = new Vector3(0, -2f, 0); // Put it slightly below center

        // 4. Add the generator component
        Procedural2DTerrain generator = terrainGO.AddComponent<Procedural2DTerrain>();
        
        // 5. Configure values
        generator.width = 100f; // very wide
        generator.heightMultiplier = 3f; // nice medium hills
        generator.noiseScale = 0.05f; // broad gradual hills
        generator.terrainMaterial = dirtMat;
        generator.groundDepth = 15f; // thick ground

        // Explicitly call generate so it builds the mesh immediately in editor
        generator.GenerateTerrain();
        
        // Change layer to Default (which is usually what raycast hits unless specified otherwise)
        terrainGO.layer = LayerMask.NameToLayer("Default");

        Debug.Log("[Worms Terrain] Successfully generated irregular wavy terrain!");
        changed = true;

        if (changed)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
        
        // Clean up self
        string selfPath = "Assets/Editor/ProceduralTerrainAutomator.cs";
        if (System.IO.File.Exists(selfPath))
        {
            EditorApplication.delayCall += () => {
                AssetDatabase.DeleteAsset(selfPath);
            };
        }
    }
}
