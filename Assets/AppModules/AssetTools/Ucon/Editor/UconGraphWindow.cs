
using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Reflection;
using Leap.Unity.Query;
using System.Collections.Generic;

namespace Leap.Unity.UserContext {
  
  class UconGraphWindow : EditorWindow {

    [MenuItem("Window/Leap Motion AppModules/Ucon Graph")]
    public static void ShowWindow() {
      var uconWindow = GetWindow(typeof(UconGraphWindow));
      uconWindow.titleContent = new GUIContent("Ucon Graph");
    }

    void OnGUI() {
      drawUconTypes();
    }

    private Vector2 _scrollPos = Vector2.zero;

    private void drawUconTypes() {

      _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos,
                                                   GUILayout.ExpandWidth(false));

      foreach (var analyzedUconType in UconAnalysis.uconChannelTypes) {
        GUILayout.Box(new GUIContent(analyzedUconType.type.Name), EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical();
        foreach (var uconChannelField in analyzedUconType.channelFields) {
          EditorGUILayout.LabelField(new GUIContent(uconChannelField.Name));
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
      }

      EditorGUILayout.EndScrollView();

    }

  }

}