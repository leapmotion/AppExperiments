using UnityEngine;
using Leap.Unity.DevGui;

public class TextSceneController : MonoBehaviour {

  public TextRenderingSettings settings;
  public Camera mainCamera;
  public Transform backgroundPanel;
  public GameObject handPanelAnchor;
  public GameObject backgroundPanelAnchor;

  [DevCategory("Scene Settings")]
  [DevValue]
  public bool showHandPanel = true;
  [DevValue]
  public bool showBackgroundPanel = true;

  void Update() {
    if (showHandPanel != handPanelAnchor.activeSelf) {
      handPanelAnchor.SetActive(showHandPanel);
    }

    if (showBackgroundPanel != backgroundPanelAnchor.activeSelf) {
      backgroundPanelAnchor.SetActive(showBackgroundPanel);
    }

    mainCamera.backgroundColor = getGrayscale(settings.skyColor);

    backgroundPanel.localPosition = new Vector3(0, 0, settings.panelDistance);
  }

  private Color getGrayscale(float f) {
    return new Color(f, f, f, 1);
  }
}
