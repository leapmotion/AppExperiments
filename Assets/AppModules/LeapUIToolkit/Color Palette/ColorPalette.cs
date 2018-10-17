using System.Collections;
using System.Collections.Generic;
using Leap.Unity;
using UnityEngine;

[CreateAssetMenu(fileName = "NewColorPalette", menuName = "Color Palette", order = 311)]
public class ColorPalette : ScriptableObject {
  
  [System.Serializable]
  public class ColorMap : SerializableDictionary<string, Color> { }

  public ColorMap colors;

}
