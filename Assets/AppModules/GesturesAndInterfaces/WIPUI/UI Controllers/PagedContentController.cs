using Leap.Unity.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PagedContentController : MonoBehaviour {

  public GameObject[] pages;

  [SerializeField, OnEditorChange("pageIdx")]
  private int _pageIdx = 0;
  public int pageIdx {
    get {
      return _pageIdx;
    }
    set {
      _pageIdx = Mathf.Max(0, Mathf.Min(pages.Length - 1, value));

      disableOtherPages(_pageIdx);
      enablePage(_pageIdx);
    }
  }

  [Header("Page Index Controller")]
  public RadioToggleGroup pageIndexController;

  void OnValidate() {
    _pageIdx = Mathf.Max(0, Mathf.Min(pages.Length, pageIdx));
  }

  void Awake() {
    pageIndexController.OnIndexToggled += (i) => {
      pageIdx = i;
    };
  }

  #region Hacky Page Initialization

  // Sucks that this is necessary but, there's lots of important initialization logic
  // in UI MonoBehaviours that really _needs_ to run so that sliders and buttons are in
  // a valid state and value change events are hooked up. So we activate all pages at 
  // once to make sure they are initialized properly and THEN on the NEXT FRAME we hide
  // all but the active page.
  private bool _firstFrame = true;
  private bool _secondFrame = false;
  void Update() {
    if (_secondFrame && !_firstFrame) {
      pageIdx = pageIdx;
      _secondFrame = false;
    }

    if (_firstFrame) {
      for (int i = 0; i < pages.Length; i++) {
        if (i == pageIdx) continue;
        pages[i].SetActive(true);
      }
      _firstFrame = false;
      _secondFrame = true;
    }
  }

  #endregion

  private void disableOtherPages(int pageIdx) {
    for (int i = 0; i < pages.Length; i++) {
      if (i == pageIdx) continue;
      pages[i].SetActive(false);
    }
  }

  private void enablePage(int pageIdx) {
    pages[pageIdx].SetActive(true);
  }

}
