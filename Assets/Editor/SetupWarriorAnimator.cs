using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.Linq;

public class SetupWarriorAnimator
{
    [MenuItem("Tools/Setup Warrior Animator")]
    public static void SetupAnimator()
    {
        string path = "Assets/Art/Characters/SciFiWarriorPBRHPPolyart/Animators/SciFiWarrior.controller";
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        
        if (controller == null)
        {
            Debug.LogError("Animator Controller not found at " + path);
            return;
        }

        // Add Parameters
        if (!controller.parameters.Any(p => p.name == "Speed"))
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            
        if (!controller.parameters.Any(p => p.name == "IsJumping"))
            controller.AddParameter("IsJumping", AnimatorControllerParameterType.Bool);

        AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;

        // Find States
        var idleState = rootStateMachine.states.FirstOrDefault(s => s.state.name == "Idle_gunMiddle_AR").state;
        var walkState = rootStateMachine.states.FirstOrDefault(s => s.state.name == "WalkFront_Shoot_AR").state;
        var jumpState = rootStateMachine.states.FirstOrDefault(s => s.state.name == "Jump").state;

        if (idleState == null || walkState == null || jumpState == null)
        {
            Debug.LogError("Could not find required states in Animator Controller.");
            return;
        }

        // Set Default State
        rootStateMachine.defaultState = idleState;
        
        // Remove existing transitions to avoid duplicates if run multiple times
        idleState.transitions = new AnimatorStateTransition[0];
        walkState.transitions = new AnimatorStateTransition[0];
        jumpState.transitions = new AnimatorStateTransition[0];
        rootStateMachine.anyStateTransitions = new AnimatorStateTransition[0];

        // Setup Transitions
        
        // Idle -> Walk
        var idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        idleToWalk.hasExitTime = false;
        idleToWalk.duration = 0.1f;

        // Walk -> Idle
        var walkToIdle = walkState.AddTransition(idleState);
        walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        walkToIdle.hasExitTime = false;
        walkToIdle.duration = 0.1f;

        // Any -> Jump
        var anyToJump = rootStateMachine.AddAnyStateTransition(jumpState);
        anyToJump.AddCondition(AnimatorConditionMode.If, 0, "IsJumping");
        anyToJump.hasExitTime = false;
        anyToJump.duration = 0.1f;

        // Jump -> Idle
        var jumpToIdle = jumpState.AddTransition(idleState);
        jumpToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsJumping");
        jumpToIdle.hasExitTime = true;
        jumpToIdle.exitTime = 0.8f;   // Let jump finish mostly
        jumpToIdle.duration = 0.2f;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Debug.Log("Animator Controller successfully configured!");
    }
}
