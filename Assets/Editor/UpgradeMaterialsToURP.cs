using UnityEngine;
using UnityEditor;

public class UpgradeMaterialsToURP
{
    [MenuItem("Tools/Upgrade Warrior Materials to URP")]
    public static void UpgradeMaterials()
    {
        string[] materialPaths = new string[]
        {
            "Assets/Art/Characters/SciFiWarriorPBRHPPolyart/Materials/PBR.mat",
            "Assets/Art/Characters/SciFiWarriorPBRHPPolyart/Materials/HP.mat",
            "Assets/Art/Characters/SciFiWarriorPBRHPPolyart/Materials/Polyart.mat"
        };

        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null)
        {
            Debug.LogError("Could not find URP Lit shader! Make sure the Universal Render Pipeline package is installed.");
            return;
        }

        foreach (string path in materialPaths)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null)
            {
                // Standard shader often maps _MainTex to _BaseMap in URP
                Texture mainTex = mat.GetTexture("_MainTex");
                Color color = mat.HasProperty("_Color") ? mat.GetColor("_Color") : Color.white;
                
                mat.shader = urpLit;
                
                if (mainTex != null)
                {
                    mat.SetTexture("_BaseMap", mainTex);
                }
                mat.SetColor("_BaseColor", color);

                EditorUtility.SetDirty(mat);
                Debug.Log($"Upgraded material {mat.name} to URP.");
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Material upgrade complete!");
    }
}
