using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;

public class FixAnimatorAggressive
{
    [MenuItem("Tools/Fix Warrior Animator Complete")]
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

        // 1. Fix Speeds
        idleState.speed = 0.5f; // Slow down idle
        jumpState.speed = 1.0f; // Ensure jump isn't playing too fast

        // 2. Clear ALL existing transitions from these states
        ClearTransitions(idleState);
        ClearTransitions(runState);
        ClearTransitions(jumpState);
        
        // Clear ALL AnyState transitions
        var anyTrans = rootStateMachine.anyStateTransitions;
        foreach(var t in anyTrans)
        {
            rootStateMachine.RemoveAnyStateTransition(t);
        }

        // 3. Setup Transitions
        
        // Idle -> Run
        var idleToRun = idleState.AddTransition(runState);
        idleToRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        idleToRun.hasExitTime = false;
        idleToRun.duration = 0.1f;

        // Run -> Idle
        var runToIdle = runState.AddTransition(idleState);
        runToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        runToIdle.hasExitTime = false;
        runToIdle.duration = 0.1f;

        // Idle/Run -> Jump (Direct transitions instead of Any state)
        var idleToJump = idleState.AddTransition(jumpState);
        idleToJump.AddCondition(AnimatorConditionMode.If, 0, "IsJumping");
        idleToJump.hasExitTime = false;
        idleToJump.duration = 0.1f;
        idleToJump.interruptionSource = TransitionInterruptionSource.None;

        var runToJump = runState.AddTransition(jumpState);
        runToJump.AddCondition(AnimatorConditionMode.If, 0, "IsJumping");
        runToJump.hasExitTime = false;
        runToJump.duration = 0.1f;
        runToJump.interruptionSource = TransitionInterruptionSource.None;

        // Jump -> Idle
        var jumpToIdle = jumpState.AddTransition(idleState);
        jumpToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsJumping");
        jumpToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        jumpToIdle.hasExitTime = false; 
        jumpToIdle.duration = 0.1f;
        jumpToIdle.interruptionSource = TransitionInterruptionSource.None;

        // Jump -> Run
        var jumpToRun = jumpState.AddTransition(runState);
        jumpToRun.AddCondition(AnimatorConditionMode.IfNot, 0, "IsJumping");
        jumpToRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        jumpToRun.hasExitTime = false;
        jumpToRun.duration = 0.1f;
        jumpToRun.interruptionSource = TransitionInterruptionSource.None;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Debug.Log("Animator completely fixed WITHOUT using AnyState!");
    }

    private static void ClearTransitions(AnimatorState state)
    {
        var transitions = state.transitions.ToArray();
        foreach (var t in transitions)
        {
            state.RemoveTransition(t);
        }
    }
}
