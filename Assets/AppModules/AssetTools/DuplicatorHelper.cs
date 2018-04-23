using Leap.Unity;
using Leap.Unity.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuplicatorHelper : MonoBehaviour {

  [QuickButton("Duplicate!", "Duplicate")]
  [MinValue(1)]
  public int numWidthCopies = 1;
  [MinValue(1)]
  public int numHeightCopies = 1;

  public float horizontalSpacing = 0.10f;
  public float verticalSpacing = 0.10f;

  [QuickButton("Clear Children!", "ClearDuplicationParentChildren")]
  public Transform duplicationParent;

  public GameObject toDuplicate;
  public bool autoEnableDuplicates = false;

  [Header("Names and Text In Duplicates")]
  [Tooltip("Newline-or-comma delimited strings in column-major order for names / text meshes")]
  [TextArea(6, 256)]
  public string sourceString = "";
  public bool setDuplicateNames = false;
  public bool searchAndSetTextMeshes = false;
  private string[] _childTokens = new string[256];

  public void Duplicate() {
    if (duplicationParent == null) {
      Debug.LogError("Can't duplicate without a duplication parent. Warning: Pre-existing objects in the "
        + "duplication parent will be destroyed.", this);
      return;
    }
    if (toDuplicate == null) {
      Debug.LogError("Can't duplicate without target GameObject toDuplicate.", this);
      return;
    }

    ClearDuplicationParentChildren();

    if (!string.IsNullOrEmpty(sourceString)) {
      var tokens = sourceString.Split(new char[] {',', '\n'}, 256, System.StringSplitOptions.None);
      for (int i = 0; i < tokens.Length; i++) {
        tokens[i] = tokens[i].Trim();
      }
      tokens.CopyTo(_childTokens, 0);
    }

    for (int i = 0; i < numHeightCopies; i++) {
      for (int j = 0; j < numWidthCopies; j++) {
        var position = toDuplicate.transform.position
                       + j * horizontalSpacing * duplicationParent.transform.right * this.transform.lossyScale.x
                       + i * verticalSpacing * -duplicationParent.transform.up * this.transform.lossyScale.y;


        GameObject duplicate = GameObject.Instantiate(toDuplicate);
        duplicate.transform.parent = duplicationParent;
        duplicate.transform.position = position;
        duplicate.transform.rotation = toDuplicate.transform.rotation;
        duplicate.transform.localScale = toDuplicate.transform.localScale;

        if (autoEnableDuplicates) {
          duplicate.gameObject.SetActive(true);
        }

        if (setDuplicateNames || searchAndSetTextMeshes) {
          int k = i * numWidthCopies + j;

          if (setDuplicateNames) {
            duplicate.name = _childTokens[k];
          }
          if (searchAndSetTextMeshes) {
            var textMeshes = duplicate.GetComponentsInChildren<TextMesh>();
            foreach (var textMesh in textMeshes) {
              textMesh.text = _childTokens[k];
            }
          }
        }
      }
    }
  }

  public void ClearDuplicationParentChildren() {
    var numChildren = duplicationParent.childCount;
    for (int i = numChildren - 1; i >= 0; i--) {
      DestroyImmediate(duplicationParent.GetChild(i).gameObject);
    }
  }

}
