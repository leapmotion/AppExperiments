using System.IO;
using UnityEngine;
using Leap.Unity.DevGui;

public class TextRenderingPresets : MonoBehaviour {

  [DevCategory("Presets")]
  [DevValue]
  public string presetName;

  public TextRenderingSettings settings;

  private string presetPath {
    get {
      return Path.Combine(Application.streamingAssetsPath, presetName + ".json");
    }
  }

  [DevCategory("Presets")]
  [DevButton]
  public void SavePreset() {
    Directory.CreateDirectory(Application.streamingAssetsPath);
    File.WriteAllText(presetPath, JsonUtility.ToJson(settings, prettyPrint: true));
  }

  [DevButton]
  public void LoadPreset() {
    JsonUtility.FromJsonOverwrite(File.ReadAllText(presetPath), settings);
  }
}
