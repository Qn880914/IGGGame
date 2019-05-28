using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(EasyImageEffectInner))]
public class EasyGamePostEffectEditor : Editor {

    private SerializedObject serObj;

    private SerializedProperty threshold;
    private SerializedProperty intensity;
    private SerializedProperty blurSize;
    private SerializedProperty blurIterations;

    private SerializedProperty fastBloomShader;

    private SerializedProperty redChannel;
    private SerializedProperty greenChannel;
    private SerializedProperty blueChannel;
    private SerializedProperty saturation;

    private SerializedProperty vignetIntensity;

    private SerializedProperty vignetOpen;
    private SerializedProperty colorCorrectOpen;
    private SerializedProperty bloomOpen;

    private bool applyCurveChanges = false;

    private void OnEnable() {

        serObj = new SerializedObject(target);
        threshold = serObj.FindProperty("threshold");
        intensity = serObj.FindProperty("intensity");
        blurSize = serObj.FindProperty("blurSize");
        blurIterations = serObj.FindProperty("blurIterations");
        fastBloomShader = serObj.FindProperty("fastBloomShader");

        redChannel = serObj.FindProperty("redChannel");
        greenChannel = serObj.FindProperty("greenChannel");
        blueChannel = serObj.FindProperty("blueChannel");
        saturation = serObj.FindProperty("saturation");

        vignetIntensity = serObj.FindProperty("vignetIntensity");

        vignetOpen = serObj.FindProperty("vignetOpen");
        colorCorrectOpen = serObj.FindProperty("colorCorrectOpen");
        bloomOpen = serObj.FindProperty("bloomOpen");
    }

    void CurveGui(string name, SerializedProperty animationCurve, Color color) {
        // @NOTE: EditorGUILayout.CurveField is buggy and flickers, using PropertyField for now
        //animationCurve.animationCurveValue = EditorGUILayout.CurveField (GUIContent (name), animationCurve.animationCurveValue, color, Rect (0.0f,0.0f,1.0f,1.0f));
        EditorGUILayout.PropertyField(animationCurve, new GUIContent(name));
        if (GUI.changed)
            applyCurveChanges = true;
    }

    void BeginCurves() {
        applyCurveChanges = false;
    }

    void ApplyCurves() {
        if (applyCurveChanges) {
            serObj.ApplyModifiedProperties();
            (serObj.targetObject as EasyImageEffectInner).gameObject.SendMessage("UpdateTextures");
        }
    }

    public override void OnInspectorGUI() {
        serObj.Update();

        GUILayout.Label("Bloom PostEffect", EditorStyles.miniBoldLabel);

        EditorGUILayout.PropertyField(bloomOpen, new GUIContent("bloomOpen"));
        EditorGUILayout.PropertyField(threshold, new GUIContent("threshold"));
        EditorGUILayout.PropertyField(intensity, new GUIContent("intensity"));
        EditorGUILayout.PropertyField(blurSize, new GUIContent("blurSize"));
        EditorGUILayout.PropertyField(blurIterations, new GUIContent("blurIterations"));
        EditorGUILayout.PropertyField(fastBloomShader, new GUIContent("fastBloomShader"));

        BeginCurves();
    

        GUILayout.Label("Color Correction PostEffect", EditorStyles.miniBoldLabel);

        EditorGUILayout.PropertyField(colorCorrectOpen, new GUIContent("colorCorrectOpen"));
        CurveGui("redChannel", redChannel, Color.red);
        CurveGui("greenChannel", greenChannel, Color.green);
        CurveGui("blueChannel", blueChannel, Color.blue);
        //EditorGUILayout.PropertyField(redChannel, new GUIContent("redChannel"));
        //EditorGUILayout.PropertyField(greenChannel, new GUIContent("greenChannel"));
        //EditorGUILayout.PropertyField(blueChannel, new GUIContent("blueChannel"));
        EditorGUILayout.PropertyField(saturation, new GUIContent("saturation"));

        GUILayout.Label("vignet PostEffect", EditorStyles.miniBoldLabel);

        EditorGUILayout.PropertyField(vignetOpen, new GUIContent("vignetOpen"));
        EditorGUILayout.PropertyField(vignetIntensity, new GUIContent("vignetIntensity"));

        ApplyCurves();

        if (!applyCurveChanges)
            serObj.ApplyModifiedProperties();
    }

}
