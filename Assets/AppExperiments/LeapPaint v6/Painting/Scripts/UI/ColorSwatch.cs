using Leap.Unity.Apps.Paint6.Drawing;
using Leap.Unity.Query;
using Leap.Unity.UserContext;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Apps.Paint6 {

  public class ColorSwatch : MonoBehaviour {

    [Header("Ucon Output Channel")]
    public ColorChannel colorChannel = new ColorChannel("brush/color");

    public Color swatchColor;

    [Header("Visuals")]

    public Renderer swatchRendererToSet;
    public string colorPropertyName = "_Color";
    private int _colorPropId = -1;
    private Material _materialInstance;

    void OnValidate() {
      if (swatchRendererToSet != null) {
        swatchRendererToSet.sharedMaterial.SetColor(colorPropertyName, swatchColor);
      }
    }

    void OnEnable() {
      if (swatchRendererToSet == null) {
        swatchRendererToSet = GetComponentInChildren<Renderer>();
      }
      if (_materialInstance == null) {
        _materialInstance = swatchRendererToSet.material;
      }

      _colorPropId = Shader.PropertyToID(colorPropertyName);

      sendColorToSwatchRenderer();
    }

    public void SendColorToBrush() {
      colorChannel.Set(swatchColor);

      sendColorToSwatchRenderer();
    }

    private void sendColorToSwatchRenderer() {
      if (_materialInstance != null) {
        _materialInstance.SetColor(_colorPropId, swatchColor);
      }
    }

    // TODO: Deleteme -- thinking about user models
    private class UserModelThoughts {

      private class UserModel {
        public List<Paintbrush> Get<Paintbrush>() {
          return new List<Paintbrush>();
        }
      }

      // Somewhere in a Paintbrush class:
      //
      // private Color _color;
      // public Color color {
      //   get { return _color; }
      //   set { _color = value; }
      // }
      // private Action<Color> _setColorFunc = null;
      // public Action<Color> setColorFunc {
      //   get { return _setColorFunc = _setColorFunc ?? SetColor; }
      // }
      // public void SetColor(Color color) { this.color = color; }
      // private Func<Color> _getColorFunc = null;
      // public Func<Color> getColorFunc {
      //   get { return _getColorFunc = _getColorFunc ?? GetColor; }
      // }
      // public Color GetColor() { return this.color; }
      //
      // Paintbrush picks a user to associate with.. this could be some default for
      // "human player 1":
      // var user = UserModel.Users().Where(u => u.id == 0);
      // user.Register<Paintbrush>(this);

      // Somewhere in a ColorSwatch class:
      //
      // ColorSwatch picks a user to associate with. Again, we can get a default to capture
      // our main "the player" case, but feasibly give e.g. NPCs UserModels with arbitrary
      // storage (via Quickboards)
      // 
      // var color = this.currentColor;
      // user.Get<Paintbrush>().EachInvoke(b => b.setColorFunc, color);

      // We can already do some of the parts of this:
      //
      private void sendColorToUserBrushes() {
        UserModel user = new UserModel(); // dummy model, this would be something else

        var color = Color.white;

        // This would work, but allocates because the closure has to capture "color".
        // something like an EachInvoke method would take an Action<T> or a Func<T>
        // and a T to pass it.
        user.Get<Paintbrush>().Query().Select(b => b.setColorFunc)
                                      .Fold((acc, f) => { f(color); return null; });

        // Leaning in to the Query system, seemingly normal calls like "user.Get()"
        // or "UserModel.Users()" could return QueryWrappers, so there would be no
        // ".Query()" call above.

      }

    }

  }

}