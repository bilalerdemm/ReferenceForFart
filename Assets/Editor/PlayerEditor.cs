using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(Player))]
public class PlayerEditor : Editor
{
    public AnimationCurve curve;
    public int angle;
    public float trueAngle;
    public override void OnInspectorGUI()
    {
        Player player = target as Player;
        base.OnInspectorGUI();
        GUILayout.Space(25);
        GUILayout.Label("Observe Values:");
        GUILayout.BeginHorizontal();
        GUILayout.Label("Converted Angle:");
        trueAngle = EditorGUILayout.FloatField(player.trueAngle);
        GUILayout.EndHorizontal();
    }
}
