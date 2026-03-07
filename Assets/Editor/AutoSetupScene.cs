using UnityEngine;
using UnityEditor;

public class AutoSetupScene
{
    [MenuItem("Tools/Run Auto Setup Now")]
    [InitializeOnLoadMethod]
    private static void RunSetup()
    {
        // Use SessionState so it only runs once per Unity session
        // Removed SessionState check so the user can run it manually via the menu repeatedly if needed.
        // if (SessionState.GetBool("AutoSetupDone", false))
        //     return;
        //     
        // SessionState.SetBool("AutoSetupDone", true);
        
        // Ensure Animator is setup
        SetupWarriorAnimator.SetupAnimator();

        // Instantiate Prefab
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Characters/SciFiWarriorPBRHPPolyart/Prefabs/PBRCharacter.prefab");
        if (prefab == null) 
        {
            Debug.LogError("Could not find PBRCharacter prefab!");
            return;
        }

        var existing = GameObject.Find("PBRCharacter");
        if (existing != null) return;

        GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        if (player != null)
        {
            player.name = "PBRCharacter";
            
            if (player.GetComponent<CharacterMovement>() == null)
                player.AddComponent<CharacterMovement>();
            
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb == null) rb = player.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
            
            CapsuleCollider capsule = player.GetComponent<CapsuleCollider>();
            if (capsule == null) capsule = player.AddComponent<CapsuleCollider>();
            capsule.height = 1.8f;
            capsule.center = new Vector3(0, 0.9f, 0);

            // Hide old player and get its position
            CharacterMovement[] allMovements = Object.FindObjectsByType<CharacterMovement>(FindObjectsSortMode.None);
            Vector3 spawnPos = new Vector3(0, 2, 0); // Default spawn slightly above ground
            
            foreach(var m in allMovements)
            {
                if (m.gameObject != player)
                {
                    spawnPos = m.transform.position;
                    m.gameObject.SetActive(false);
                    Debug.Log("Deactivated old player: " + m.gameObject.name + " and took its position.");
                }
            }
            
            player.transform.position = spawnPos;

            // Assign to Camera
            CameraController camController = Object.FindAnyObjectByType<CameraController>();
            if (camController != null)
            {
                SerializedObject so = new SerializedObject(camController);
                SerializedProperty targetProp = so.FindProperty("target");
                if (targetProp != null)
                {
                    targetProp.objectReferenceValue = player.transform;
                    so.ApplyModifiedProperties();
                    Debug.Log("Assigned PBRCharacter to Camera target.");
                }
            }
            
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            Debug.Log("Auto Setup Complete: Character instantiated and configured in scene.");
        }
    }
}
