using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Leap.Unity;

[CustomEditor(typeof(PropertyPanel))]
public class PropertyPanelEditor : CustomEditorBase<PropertyPanel> {

  private ReorderableList list = null;

  private List<GameObject> editObjects = new List<GameObject>();
  private HashSet<PropertyKey> selectedProperties = new HashSet<PropertyKey>();
  private List<PropertyKey> selectedPropertyList = new List<PropertyKey>();

  protected override void OnEnable() {
    base.OnEnable();

    list = new ReorderableList(serializedObject,
                               serializedObject.FindProperty("elements"),
                               draggable: true,
                               displayHeader: true,
                               displayAddButton: false,
                               displayRemoveButton: true);

    list.drawElementCallback = drawElementEditMode;
    list.drawHeaderCallback = drawHeader;

    hideField("mode");
    specifyCustomDrawer("elements", drawElements);
  }

  public override void OnInspectorGUI() {
    _showScriptField = serializedObject.FindProperty("mode").intValue != (int)PropertyPanel.Mode.View;

    base.OnInspectorGUI();
  }

  private void drawElements(SerializedProperty property) {
    SerializedProperty modeProp = serializedObject.FindProperty("mode");

    switch ((PropertyPanel.Mode)modeProp.intValue) {
      case PropertyPanel.Mode.View:
        drawViewMode(property);
        break;
      case PropertyPanel.Mode.Edit:
        drawEditMode(property);
        break;
      case PropertyPanel.Mode.Object:
        drawObjectMode(property);
        break;
    }
  }

  private void enterMode(PropertyPanel.Mode mode) {
    serializedObject.FindProperty("mode").intValue = (int)mode;
  }

  #region VIEW MODE
  private void drawViewMode(SerializedProperty property) {
    for (int i = 0; i < property.arraySize; i++) {
      SerializedProperty elementProp = property.GetArrayElementAtIndex(i);
      SerializedProperty typeProp = elementProp.FindPropertyRelative("type");
      SerializedProperty nameProp = elementProp.FindPropertyRelative("name");

      switch ((PropertyPanel.ElementType)typeProp.intValue) {
        case PropertyPanel.ElementType.Header:
          EditorGUILayout.Space();
          EditorGUILayout.LabelField(nameProp.stringValue, EditorStyles.boldLabel);
          break;
        case PropertyPanel.ElementType.Link:
          SerializedProperty objsProp = elementProp.FindPropertyRelative("objs");

          List<Object> objs = new List<Object>();
          for (int j = 0; j < objsProp.arraySize; j++) {
            var obj = objsProp.GetArrayElementAtIndex(j).objectReferenceValue;
            if (obj != null) {
              objs.Add(obj);
            }
          }

          if (objs.Count == 0) {
            continue;
          }

          SerializedProperty pathProp = elementProp.FindPropertyRelative("path");

          SerializedObject linkedObj = new SerializedObject(objs.ToArray());
          SerializedProperty linkedProp = linkedObj.FindProperty(pathProp.stringValue);
          if (linkedProp == null) continue;

          EditorGUI.BeginChangeCheck();
          EditorGUILayout.PropertyField(linkedProp, new GUIContent(nameProp.stringValue), includeChildren: true);
          if (EditorGUI.EndChangeCheck()) {
            linkedObj.ApplyModifiedProperties();
          }
          break;
      }
    }
  }
  #endregion

  #region EDIT MODE
  private void drawEditMode(SerializedProperty property) {
    list.DoLayoutList();
  }

  private void drawHeader(Rect rect) {
    Rect left, middle, right;
    rect.SplitHorizontally(out left, out middle);
    middle.SplitHorizontally(out middle, out right);

    if (GUI.Button(left, "Finish", EditorStyles.miniButtonLeft)) {
      enterMode(PropertyPanel.Mode.View);
    }

    if (GUI.Button(middle, "Header", EditorStyles.miniButtonMid)) {
      addNewHeader();
    }

    if (GUI.Button(right, "Properties", EditorStyles.miniButtonRight)) {
      enterMode(PropertyPanel.Mode.Object);
      editObjects.Clear();
      selectedProperties.Clear();
      selectedPropertyList.Clear();
    }
  }

  private void addNewHeader() {
    int index = list.serializedProperty.arraySize;
    list.serializedProperty.InsertArrayElementAtIndex(index);
    var newElement = list.serializedProperty.GetArrayElementAtIndex(index);

    newElement.FindPropertyRelative("type").intValue = (int)PropertyPanel.ElementType.Header;
    newElement.FindPropertyRelative("name").stringValue = "New Header";

    list.serializedProperty.serializedObject.ApplyModifiedProperties();
  }

  private void drawElementEditMode(Rect rect, int index, bool isActive, bool isFocused) {
    rect.height = EditorGUIUtility.singleLineHeight;
    var elementProp = list.serializedProperty.GetArrayElementAtIndex(index);

    SerializedProperty typeProp = elementProp.FindPropertyRelative("type");
    SerializedProperty nameProp = elementProp.FindPropertyRelative("name");

    Color prevColor = GUI.color;
    if (typeProp.intValue == (int)PropertyPanel.ElementType.Header) {
      GUI.color = new Color(0.8f, 0.8f, 0.8f);
    }

    EditorGUI.PropertyField(rect, nameProp, GUIContent.none);

    GUI.color = prevColor;
  }
  #endregion

  #region OBJECT MODE
  private void drawObjectMode(SerializedProperty property) {
    EditorGUILayout.BeginHorizontal();

    bool noProperties = selectedPropertyList.Count == 0;
    EditorGUI.BeginDisabledGroup(noProperties);
    bool addProperty = GUILayout.Button(noProperties ? 
                                        "No Properties Selected" : 
                                        "Add Properties");
    EditorGUI.EndDisabledGroup();
    if (addProperty) {
      foreach (var key in selectedPropertyList) {
        int index = property.arraySize;
        property.InsertArrayElementAtIndex(index);
        var newElement = property.GetArrayElementAtIndex(index);

        SerializedObject sobj = new SerializedObject(key.objs[0]);
        var name = sobj.FindProperty(key.path).displayName;

        newElement.FindPropertyRelative("type").intValue = (int)PropertyPanel.ElementType.Link;
        newElement.FindPropertyRelative("path").stringValue = key.path;
        newElement.FindPropertyRelative("name").stringValue = name;

        var arrayProp = newElement.FindPropertyRelative("objs");
        arrayProp.arraySize = key.objs.Count;
        for (int i = 0; i < key.objs.Count; i++) {
          arrayProp.GetArrayElementAtIndex(i).objectReferenceValue = key.objs[i];
        }
      }

      enterMode(PropertyPanel.Mode.Edit);
    }

    if (GUILayout.Button("Cancel")) {
      enterMode(PropertyPanel.Mode.Edit);
    }

    EditorGUILayout.EndHorizontal();

    EditorGUILayout.HelpBox("Drag Game Objects here to inspect and add their properties.", MessageType.Info);
    Rect dragRect = GUILayoutUtility.GetLastRect();
    if (dragRect.Contains(Event.current.mousePosition) &&
        DragAndDrop.objectReferences.Length > 0 &&
        DragAndDrop.objectReferences.All(o => o is GameObject)) {

      switch (Event.current.type) {
        case EventType.DragUpdated:
          DragAndDrop.AcceptDrag();
          DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
          Event.current.Use();
          break;
        case EventType.DragPerform:
          editObjects.AddRange(DragAndDrop.objectReferences.Cast<GameObject>());
          Event.current.Use();
          break;
      }
    }

    for (int i = editObjects.Count; i-- != 0;) {
      EditorGUILayout.BeginHorizontal();

      editObjects[i] = (GameObject)EditorGUILayout.ObjectField(editObjects[i], typeof(GameObject), allowSceneObjects: true);

      if (editObjects[i] == null) {
        editObjects.RemoveAt(i);
        continue;
      }

      switch (PrefabUtility.GetPrefabType(editObjects[i])) {
        case PrefabType.ModelPrefab:
        case PrefabType.Prefab:
          editObjects.RemoveAt(i);
          continue;
      }

      if (GUILayout.Button("x")) {
        editObjects.RemoveAt(i);
      }

      EditorGUILayout.EndHorizontal();
    }

    EditorGUILayout.Space();

    selectedPropertyList.Clear();

    var groups = createGroups();
    foreach (var group in groups) {
      SerializedObject sobj = new SerializedObject(group.ToArray());
      SerializedProperty it = sobj.GetIterator();
      bool hasDisplayed = false;
      while (it.NextVisible(true)) {
        if (!hasDisplayed) {
          EditorGUILayout.LabelField(group[0].GetType().Name, EditorStyles.boldLabel);
          hasDisplayed = true;
          EditorGUI.indentLevel++;
        }

        var key = new PropertyKey() {
          objs = group,
          path = it.propertyPath
        };

        bool enabled = selectedProperties.Contains(key);
        enabled = EditorGUILayout.ToggleLeft(it.propertyPath, enabled);
        if (enabled) {
          selectedProperties.Add(key);
        } else {
          selectedProperties.Remove(key);
        }

        if (enabled) {
          selectedPropertyList.Add(key);
        }
      }

      if (hasDisplayed) {
        EditorGUI.indentLevel--;
      }
    }
  }

  private List<List<Object>> createGroups() {
    var groups = new List<List<Object>>();

    if (editObjects.Count == 0) {
      return groups;
    }

    groups.Add(new List<Object>(editObjects.Cast<Object>()));

    var map = new Dictionary<GameObject, List<Component>>();
    foreach (var obj in editObjects) {
      map[obj] = new List<Component>(obj.GetComponents<Component>());
    }

    while (true) {
      var anyComponent = map.SelectMany(p => p.Value).FirstOrDefault();
      if (anyComponent == null) {
        break;
      }
      var type = anyComponent.GetType();

      List<Object> group = new List<Object>();
      foreach (var pair in map) {
        Component item = pair.Value.FirstOrDefault(t => t.GetType() == type);
        if (item != null) {
          pair.Value.Remove(item);
          group.Add(item);
        }
      }

      groups.Add(group);
    }

    return groups;
  }

  private struct PropertyKey {
    public List<Object> objs;
    public string path;

    public override bool Equals(object obj) {
      PropertyKey other = (PropertyKey)obj;
      if (objs.Count != other.objs.Count) {
        return false;
      }

      if (path != other.path) {
        return false;
      }

      for (int i = 0; i < objs.Count; i++) {
        if (objs[i] != other.objs[i]) {
          return false;
        }
      }

      return true;
    }

    public override int GetHashCode() {
      Hash hash = new Hash() {
        path
      };

      hash.AddRange(objs);
      return hash;
    }
  }

  #endregion
}
