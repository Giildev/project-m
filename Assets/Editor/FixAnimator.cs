using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;

public class FixAnimator
{
    [MenuItem("Tools/Fix Warrior Animator")]
    public static void DoFix()
    {
        string path = "Assets/Art/Characters/SciFiWarriorPBRHPPolyart/Animators/SciFiWarrior.controller";
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        
        if (controller == null)
        {
            Debug.LogError("Animator Controller not found!");
            return;
        }

        AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;

        var idleState = rootStateMachine.states.FirstOrDefault(s => s.state.name.Contains("Idle_gunMiddle")).state;
        var runState = rootStateMachine.states.FirstOrDefault(s => s.state.name.Contains("Run_gunMiddle") || s.state.name.Contains("Walk")).state;
        var jumpState = rootStateMachine.states.FirstOrDefault(s => s.state.name.Contains("Jump")).state;

        if (idleState == null || runState == null || jumpState == null)
        {
            Debug.LogError("Could not find required states.");
            return;
        }

        // 1. Fix Idle Speed
        idleState.speed = 0.5f; // Slow down idle animation

        // 2. Clear all existing transitions
        ClearTransitions(idleState);
        ClearTransitions(runState);
        ClearTransitions(jumpState);
        
        foreach(var t in rootStateMachine.anyStateTransitions.ToArray())
        {
            rootStateMachine.RemoveAnyStateTransition(t);
        }

        // 3. Setup Transitions
        // Idle -> Run
        var idleToRun = idleState.AddTransition(runState);
        idleToRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        idleToRun.hasExitTime = false;
        idleToRun.duration = 0.15f;

        // Run -> Idle
        var runToIdle = runState.AddTransition(idleState);
        runToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        runToIdle.hasExitTime = false;
        runToIdle.duration = 0.1f;

        // Any -> Jump
        var anyToJump = rootStateMachine.AddAnyStateTransition(jumpState);
        anyToJump.AddCondition(AnimatorConditionMode.If, 0, "IsJumping");
        anyToJump.hasExitTime = false;
        anyToJump.duration = 0.1f;
        anyToJump.canTransitionToSelf = false; // CRITICAL: stops repeating in the air

        // Jump -> Idle
        var jumpToIdle = jumpState.AddTransition(idleState);
        jumpToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsJumping");
        jumpToIdle.hasExitTime = false; // Immediately stand when hitting the ground
        jumpToIdle.duration = 0.1f;

        // Jump -> Run (so it seamlessly runs if touching the ground while pressing keys)
        var jumpToRun = jumpState.AddTransition(runState);
        jumpToRun.AddCondition(AnimatorConditionMode.IfNot, 0, "IsJumping");
        jumpToRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        jumpToRun.hasExitTime = false;
        jumpToRun.duration = 0.1f;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Debug.Log("Animator completely fixed! Physics bug with jumping also resolved via script updates.");
    }

    private static void ClearTransitions(AnimatorState state)
    {
        var transitions = state.transitions;
        foreach (var t in transitions)
        {
            state.RemoveTransition(t);
        }
    }
}
