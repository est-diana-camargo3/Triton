using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwimmingDebugUI : MonoBehaviour
{
    public StrokeDetector leftHand;
    public StrokeDetector rightHand;
    public SwimmingController swimmer;

    void OnGUI()
    {
        GUI.color = Color.black;
        GUILayout.BeginArea(new Rect(10, 10, 350, 200));

        GUILayout.Label("=== SWIMMING DEBUG ===");
        GUILayout.Label($"Velocidad jugador: {swimmer.currentSpeed:F2} m/s");
        GUILayout.Label($"Total brazadas:    {swimmer.totalStrokes}");
        GUILayout.Space(10);
        GUILayout.Label($"Mano IZQ | vel: {leftHand.currentVelocity:F2} | {leftHand.currentPhase}");
        GUILayout.Label($"Mano DER | vel: {rightHand.currentVelocity:F2} | {rightHand.currentPhase}");

        GUILayout.EndArea();
    }
}