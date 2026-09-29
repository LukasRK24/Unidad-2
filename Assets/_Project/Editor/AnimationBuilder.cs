#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Crea los clips del Ninja Frog y el Animator Controller (máquina de estados) del jugador.
public static class AnimationBuilder
{
    private const string Art = BuilderUtil.Root + "/Art/Player/";
    private const string Out = BuilderUtil.Root + "/Animations/";

    public static AnimatorController Build()
    {
        BuilderUtil.EnsureFolder(Out);

        AnimationClip idle = MakeClip("Player_Idle", Art + "NinjaFrog_Idle (32x32).png", 14f, true);
        AnimationClip run = MakeClip("Player_Run", Art + "NinjaFrog_Run (32x32).png", 18f, true);
        AnimationClip jump = MakeClip("Player_Jump", Art + "NinjaFrog_Jump (32x32).png", 12f, false);
        AnimationClip fall = MakeClip("Player_Fall", Art + "NinjaFrog_Fall (32x32).png", 12f, false);
        AnimationClip dash = MakeClip("Player_Dash", Art + "NinjaFrog_Double Jump (32x32).png", 24f, false);
        AnimationClip hit = MakeClip("Player_Hit", Art + "NinjaFrog_Hit (32x32).png", 20f, false);

        string controllerPath = Out + "Player.controller";
        AssetDatabase.DeleteAsset(controllerPath);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("VelocityY", AnimatorControllerParameterType.Float);
        controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Dashing", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Hurt", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Dead", AnimatorControllerParameterType.Bool);

        AnimatorStateMachine sm = controller.layers[0].stateMachine;
        AnimatorState sIdle = sm.AddState("Idle", new Vector3(250, 0));
        AnimatorState sRun = sm.AddState("Run", new Vector3(250, 80));
        AnimatorState sJump = sm.AddState("Jump", new Vector3(520, 0));
        AnimatorState sFall = sm.AddState("Fall", new Vector3(520, 80));
        AnimatorState sDash = sm.AddState("Dash", new Vector3(250, 200));
        AnimatorState sHit = sm.AddState("Hit", new Vector3(520, 200));
        sIdle.motion = idle; sRun.motion = run; sJump.motion = jump;
        sFall.motion = fall; sDash.motion = dash; sHit.motion = hit;
        sm.defaultState = sIdle;

        Link(sIdle, sRun, Gt("Speed", 0.1f));
        Link(sRun, sIdle, Lt("Speed", 0.1f));

        foreach (AnimatorState ground in new[] { sIdle, sRun })
        {
            Link(ground, sJump, Flag("Grounded", false), Gt("VelocityY", 0.1f));
            Link(ground, sFall, Flag("Grounded", false), Lt("VelocityY", -0.1f));
        }

        Link(sJump, sFall, Lt("VelocityY", 0f));
        Link(sJump, sIdle, Flag("Grounded", true));
        Link(sFall, sIdle, Flag("Grounded", true));

        AnimatorStateTransition anyDash = sm.AddAnyStateTransition(sDash);
        Configure(anyDash, Flag("Dashing", true));
        anyDash.canTransitionToSelf = false;
        Link(sDash, sIdle, Flag("Dashing", false), Flag("Grounded", true));
        Link(sDash, sFall, Flag("Dashing", false), Flag("Grounded", false));

        AnimatorStateTransition anyHit = sm.AddAnyStateTransition(sHit);
        Configure(anyHit, new AnimatorCondition { mode = AnimatorConditionMode.If, parameter = "Hurt" });
        anyHit.canTransitionToSelf = false;
        AnimatorStateTransition hitExit = sHit.AddTransition(sIdle);
        Configure(hitExit, Flag("Dead", false));
        hitExit.hasExitTime = true;
        hitExit.exitTime = 1f;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        return controller;
    }

    private static AnimationClip MakeClip(string clipName, string spritePath, float fps, bool loop)
    {
        Sprite[] frames = BuilderUtil.LoadSprites(spritePath);
        AnimationClip clip = new AnimationClip { frameRate = fps, name = clipName };

        EditorCurveBinding binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[frames.Length];
        for (int i = 0; i < frames.Length; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = frames[i] };
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        string path = Out + clipName + ".anim";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    private static void Link(AnimatorState from, AnimatorState to, params AnimatorCondition[] conditions)
    {
        Configure(from.AddTransition(to), conditions);
    }

    private static void Configure(AnimatorStateTransition transition, params AnimatorCondition[] conditions)
    {
        transition.hasExitTime = false;
        transition.duration = 0f;
        transition.hasFixedDuration = true;
        foreach (AnimatorCondition c in conditions) transition.AddCondition(c.mode, c.threshold, c.parameter);
    }

    private static AnimatorCondition Gt(string p, float v) => new AnimatorCondition { mode = AnimatorConditionMode.Greater, parameter = p, threshold = v };
    private static AnimatorCondition Lt(string p, float v) => new AnimatorCondition { mode = AnimatorConditionMode.Less, parameter = p, threshold = v };
    private static AnimatorCondition Flag(string p, bool v) => new AnimatorCondition { mode = v ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, parameter = p };
}
#endif
