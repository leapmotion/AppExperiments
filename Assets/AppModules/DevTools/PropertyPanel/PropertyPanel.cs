using System;
using System.Collections.Generic;
using UnityEngine;

public class PropertyPanel : MonoBehaviour {

  public List<Element> elements;
  public Mode mode = Mode.Edit;

  [ContextMenu("Edit Panel")]
  public void EnterEditMode() {
    mode = Mode.Edit;
  }

  [Serializable]
  public class Element {
    public ElementType type;

    public UnityEngine.Object[] objs;
    public string name;
    public string path;
  }

  public enum ElementType {
    Header,
    Link
  }

  public enum Mode {
    View,
    Edit,
    Object
  }

}
