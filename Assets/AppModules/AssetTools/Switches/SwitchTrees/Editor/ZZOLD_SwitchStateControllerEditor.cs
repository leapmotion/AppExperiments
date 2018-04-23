using UnityEngine;
using UnityEditor;
using Leap.Unity.Query;

namespace Leap.Unity.Animation {
  
  [CustomEditor(typeof(ZZOLD_SwitchStateController), editorForChildClasses: true)]
  public class StateSwitchControllerEditor : CustomEditorBase<ZZOLD_SwitchStateController> {

    protected override void OnEnable() {
      base.OnEnable();

      specifyCustomDecorator("_curState", drawPreCurStateDecorator);
      specifyCustomPostDecorator("_curState", drawPostCurStateDecorator);
    }

    public override void OnInspectorGUI() {
      base.OnInspectorGUI();
    }

    private void drawPreCurStateDecorator(SerializedProperty property) {
      drawStateTransitionButtons();
    }

    private void drawPostCurStateDecorator(SerializedProperty property) {
      bool invalidState = targets.Query()
                                 .Any(t => !t.states.ContainsKey(property.stringValue));

      if (invalidState) {
        EditorGUILayout.HelpBox("There is no state '" + property.stringValue + "' in this "
                              + "state controller.", MessageType.Error);
      }
    }

    private void drawStateTransitionButtons() {
      EditorGUILayout.LabelField("Set State To...", EditorStyles.boldLabel);

      bool inHorizontalLayout = false;
      int numCols = 4;
      int curCol  = 0;

      Color defaultTextColor = GUI.skin.button.normal.textColor;
      try {
        foreach (var nameSwitchPair in target.states) {
          var stateName   = nameSwitchPair.Key;
          var stateSwitch = nameSwitchPair.Value;

          if (curCol == 0) {
            EditorGUILayout.BeginHorizontal();
            inHorizontalLayout = true;
          }

          string tooltip = (Application.isPlaying ? "Calls Off() on the current state's "
                                                + "switch and On() on this state switch."
                                                : "Calls OffNow() on the current state's "
                                                + "switch and OnNow() on this state "
                                                + "switch. To view switch animations, "
                                                + "enter play-mode.");

          bool buttonHasSwitch = stateSwitch.propertySwitch != null;
          if (!buttonHasSwitch) {
            GUI.skin.button.normal.textColor = Color.red;

            tooltip += " Warning: This state's switch is null, so there will be no "
                     + "On() or Off() calls sent for this state.";
          }

          if (GUILayout.Button(new GUIContent(stateName, tooltip))) {
            if (!Application.isPlaying) {
              Undo.RecordObject(serializedObject.targetObject, "Set State To " + stateName);
            }

            target.curState = stateName;
          }

          if (curCol == numCols - 1) {
            EditorGUILayout.EndHorizontal();
            inHorizontalLayout = false;
          }

          curCol++;

          if (curCol == numCols) {
            curCol = 0;
          }
        }

        if (curCol != 0) {
          while (curCol < numCols - 1) {
            EditorGUILayout.GetControlRect(hasLabel: false);
            curCol++;
          }
        }

        if (inHorizontalLayout) {
          EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();
      }
      finally {
        GUI.skin.button.normal.textColor = defaultTextColor;
      }
    }

  }

}