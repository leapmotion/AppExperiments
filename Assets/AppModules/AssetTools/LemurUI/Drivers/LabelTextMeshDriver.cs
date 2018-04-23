using UnityEngine;
using System.Collections;
using Leap.Unity.Animation;
using System;

namespace Leap.Unity.LemurUI {

  public class LabelTextMeshDriver : LabelDriver<TextMesh> {

    public Label label;
    public TextMesh textMesh;

    public LabelTextMeshDriver() {
      textMesh = driven.gameObject.GetComponent<TextMesh>();

      Updater.instance.OnUpdate += onUpdate;
    }

    public override void Bind(Label label) {
      this.label = label;
    }

    private void onUpdate() {
      if (label == null) return;
      if (textMesh == null) return;

      textMesh.transform.localScale = Vector3.one * 0.01f;

      // UnwrapDo: Applies the label color style to the text mesh if it has one.
      label.textStyle.color.UnwrapDo(
        addArg1: textMesh,
        doIfValue: (color, textMesh) => { textMesh.color = color; }
      );

      textMesh.text = label.text;
    }

  }
  
  public static class NullableExtensions {

    /// <summary>
    /// TODO: Move to TodoUMWard and add more argument counts
    /// </summary>
    public static void UnwrapDo<UnwrapType, Arg1Type>(
                         this UnwrapType? unwrappedArg0,
                         Arg1Type addArg1,
                         Action<UnwrapType, Arg1Type> doIfValue)
                         where UnwrapType : struct {
      if (unwrappedArg0.HasValue) {
        doIfValue(unwrappedArg0.Value, addArg1);
      }
    }
    public static void UnwrapDo<UnwrapType, Arg1Type,
                                            Arg2Type>(
                         this UnwrapType? unwrappedArg0,
                         Arg1Type addArg1,
                         Arg2Type addArg2,
                         Action<UnwrapType, Arg1Type, Arg2Type> doIfValue)
                         where UnwrapType : struct {
      if (unwrappedArg0.HasValue) {
        doIfValue(unwrappedArg0.Value, addArg1, addArg2);
      }
    }

  }

}
