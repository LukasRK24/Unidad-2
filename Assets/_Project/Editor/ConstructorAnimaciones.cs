#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Crea los clips del Ninja Frog y el Animator Controller (máquina de estados) del jugador.
public static class ConstructorAnimaciones
{
    private const string Art = UtilConstructor.Raiz + "/Art/Player/";
    private const string Out = UtilConstructor.Raiz + "/Animations/";

    public static AnimatorController Build()
    {
        UtilConstructor.AsegurarCarpeta(Out);

        AnimationClip idle = CrearClip("Jugador_Idle", Art + "NinjaFrog_Idle (32x32).png", 14f, true);
        AnimationClip run = CrearClip("Jugador_Run", Art + "NinjaFrog_Run (32x32).png", 18f, true);
        AnimationClip jump = CrearClip("Jugador_Jump", Art + "NinjaFrog_Jump (32x32).png", 12f, false);
        AnimationClip fall = CrearClip("Jugador_Fall", Art + "NinjaFrog_Fall (32x32).png", 12f, false);
        AnimationClip dash = CrearClip("Jugador_Dash", Art + "NinjaFrog_Double Jump (32x32).png", 24f, false);
        AnimationClip hit = CrearClip("Jugador_Hit", Art + "NinjaFrog_Hit (32x32).png", 20f, false);

        string controllerPath = Out + "Jugador.controller";
        AssetDatabase.DeleteAsset(controllerPath);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        controller.AddParameter("Velocidad", AnimatorControllerParameterType.Float);
        controller.AddParameter("VelocidadY", AnimatorControllerParameterType.Float);
        controller.AddParameter("EnSuelo", AnimatorControllerParameterType.Bool);
        controller.AddParameter("EnDash", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Herido", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Muerto", AnimatorControllerParameterType.Bool);

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

        Enlazar(sIdle, sRun, Mayor("Velocidad", 0.1f));
        Enlazar(sRun, sIdle, Menor("Velocidad", 0.1f));

        foreach (AnimatorState ground in new[] { sIdle, sRun })
        {
            Enlazar(ground, sJump, Booleano("EnSuelo", false), Mayor("VelocidadY", 0.1f));
            Enlazar(ground, sFall, Booleano("EnSuelo", false), Menor("VelocidadY", -0.1f));
        }

        Enlazar(sJump, sFall, Menor("VelocidadY", 0f));
        Enlazar(sJump, sIdle, Booleano("EnSuelo", true));
        Enlazar(sFall, sIdle, Booleano("EnSuelo", true));

        AnimatorStateTransition anyDash = sm.AddAnyStateTransition(sDash);
        Configurar(anyDash, Booleano("EnDash", true));
        anyDash.canTransitionToSelf = false;
        Enlazar(sDash, sIdle, Booleano("EnDash", false), Booleano("EnSuelo", true));
        Enlazar(sDash, sFall, Booleano("EnDash", false), Booleano("EnSuelo", false));

        AnimatorStateTransition anyHit = sm.AddAnyStateTransition(sHit);
        Configurar(anyHit, new AnimatorCondition { mode = AnimatorConditionMode.If, parameter = "Herido" });
        anyHit.canTransitionToSelf = false;
        AnimatorStateTransition hitExit = sHit.AddTransition(sIdle);
        Configurar(hitExit, Booleano("Muerto", false));
        hitExit.hasExitTime = true;
        hitExit.exitTime = 1f;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        return controller;
    }

    private static AnimationClip CrearClip(string clipName, string spritePath, float fps, bool loop)
    {
        Sprite[] frames = UtilConstructor.CargarSprites(spritePath);
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

    private static void Enlazar(AnimatorState from, AnimatorState to, params AnimatorCondition[] conditions)
    {
        Configurar(from.AddTransition(to), conditions);
    }

    private static void Configurar(AnimatorStateTransition transition, params AnimatorCondition[] conditions)
    {
        transition.hasExitTime = false;
        transition.duration = 0f;
        transition.hasFixedDuration = true;
        foreach (AnimatorCondition c in conditions) transition.AddCondition(c.mode, c.threshold, c.parameter);
    }

    private static AnimatorCondition Mayor(string p, float v) => new AnimatorCondition { mode = AnimatorConditionMode.Greater, parameter = p, threshold = v };
    private static AnimatorCondition Menor(string p, float v) => new AnimatorCondition { mode = AnimatorConditionMode.Less, parameter = p, threshold = v };
    private static AnimatorCondition Booleano(string p, bool v) => new AnimatorCondition { mode = v ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, parameter = p };
}
#endif
