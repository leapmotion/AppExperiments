using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace Leap.Unity.UserContext {

  using UnityObject = UnityEngine.Object;

  [CustomPropertyDrawer(typeof(UconChannel), useForChildren: true)]
  public class UconChannelDrawer : PropertyDrawer {

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
      return EditorGUIUtility.singleLineHeight * 2
             + EditorGUIUtility.standardVerticalSpacing * 1;
    }

    public override void OnGUI(Rect position,
                               SerializedProperty property,
                               GUIContent label) {

      //var value = fieldInfo.GetValue(property.serializedObject.targetObject);

      //EditorGUI.BeginChangeCheck();
      //EditorGUI.EndChangeCheck();

      var rect = EditorGUI.PrefixLabel(position,
                   new GUIContent(Utils.GenerateNiceName(property.name)));

      var rowHeight = EditorGUIUtility.singleLineHeight;
      var rowSpacer = EditorGUIUtility.standardVerticalSpacing;

      var descProp = property.FindPropertyRelative("_contextType");

      var descType = (ContextDescriptionType)descProp.enumValueIndex;
      var contextRect = rect.TakeHorizontal(rowHeight, out rect);
      var contextLabelRect = contextRect.TakeLeft(60f, out contextRect);
      EditorGUI.LabelField(contextLabelRect, "Context", EditorStyles.miniBoldLabel);
      var contextDescRect = contextRect.TakeRight(20f, out contextRect);
      switch (descType) {
        case ContextDescriptionType.UserModel:
          var userContextProp = property.FindPropertyRelative("_userContextType");
          var curContext = (UserContextType)userContextProp.enumValueIndex;

          var newUserContext = (UserContextType)
                                 EditorGUI.EnumPopup(contextRect, curContext);
          if (newUserContext != curContext) {
            userContextProp.enumValueIndex = (int)newUserContext;
          }
          break;
        case ContextDescriptionType.UnityObject:
          var objContextProp = property.FindPropertyRelative("_contextObj");
          var curObjContext = objContextProp.objectReferenceValue;

          var newObjContext = EditorGUI.ObjectField(contextRect, "",
                                                    curObjContext, typeof(UnityObject),
                                                    true);

          if (newObjContext != curObjContext) {
            objContextProp.objectReferenceValue = newObjContext;
          }

          break;
      }
      EditorGUI.PropertyField(contextDescRect, descProp, new GUIContent());

      rect.TakeHorizontal(rowSpacer, out rect);

      var pathProp = property.FindPropertyRelative("_channelPath");
      var channelPathRect = rect.TakeHorizontal(rowHeight, out rect);
      var pathLabelRect = channelPathRect.TakeLeft(60f, out channelPathRect);
      EditorGUI.LabelField(pathLabelRect, "Path", EditorStyles.miniBoldLabel);
      pathProp.stringValue = EditorGUI.TextField(channelPathRect, pathProp.stringValue);
    }

  }

}
