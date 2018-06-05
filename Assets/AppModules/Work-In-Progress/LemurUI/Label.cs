using System;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.LemurUI {

  #region TextStyle

  public struct TextStyle {
    public Maybe<Font> font;
    public int?        fontSize;
    public FontStyle?  fontStyle;
    public bool?       richText;
    public Color?      color;

    public TextStyle Copy() {
      return new TextStyle() {
        font = font,
        fontSize = fontSize,
        fontStyle = fontStyle,
        richText = richText,
        color = color
      };
    }

    /// <summary>
    /// Returns a new style with the properties of the argument style overlayed onto this
    /// style. Any non-null properties of the argument style will overwrite the
    /// corresponding property on this style.
    /// </summary>
    public TextStyle OverlayedWith(TextStyle other) {
      var copy = Copy();
      copy.Overlay(other);
      return copy;
    }

    /// <summary>
    /// Overwrites the style properties on this style with non-null properties in the
    /// argument style.
    /// </summary>
    /// <param name="other"></param>
    public void Overlay(TextStyle other) {
      font = other.font.ValueOr(this.font);
      fontSize = other.fontSize.ValueOr(this.fontSize);
      fontStyle = other.fontStyle.ValueOr(this.fontStyle);
      richText = other.richText.ValueOr(this.richText);
      color = other.color.ValueOr(this.color);
    }
  }

  #endregion

  #region TextFlow

  public struct TextFlow {
    public TextAlignment? alignment;
    public TextAnchor?    anchor;
    public float?         lineSpacing;
    public bool?          allowOverflowWidth;
    public bool?          allowOverflowHeight;

    /// <summary>
    /// Returns a new style with the properties of the argument style overlayed onto this
    /// style. Any non-null properties of the argument style will overwrite the
    /// corresponding property on this style.
    /// </summary>
    public TextFlow OverlayedWith(TextFlow other) {
      return new TextFlow() {
        alignment = other.alignment.ValueOr(this.alignment),
        anchor = other.anchor.ValueOr(this.anchor),
        lineSpacing = other.lineSpacing.ValueOr(this.lineSpacing),
        allowOverflowWidth = other.allowOverflowWidth.ValueOr(this.allowOverflowWidth),
        allowOverflowHeight = other.allowOverflowHeight.ValueOr(this.allowOverflowHeight),
      };
    }
  }

  #endregion

  #region Label

  public abstract class Label : LemurUIElement, IDefaultableLemurType {

    #region Static

    /// <summary>
    /// The default label Rect is 10 cm by 4 cm.
    /// </summary>
    public static Rect DefaultRect {
      get { return new Rect(0f, 0f, 0.04f, 0.10f); }
    }

    private const string DEFAULT_TEXT = "New Label";
    /// <summary>
    /// "New Label".
    /// </summary>
    public static string DefaultText {
      get { return DEFAULT_TEXT; }
    }

    #endregion

    /// <summary>
    /// The local 2D bounding box for this label. Label rects are always in local space
    /// with the origin in the lower-left corner of the rect and with dimensions
    /// specified in meters. The default label Rect is 10 cm by 4 cm.
    /// </summary>
    public Rect      rect = DefaultRect;

    /// <summary>
    /// The text contained in this label.
    /// </summary>
    public string    text = DefaultText;

    /// <summary>
    /// Visual style data for how each character should be rendered, e.g. font and color.
    /// </summary>
    public TextStyle textStyle = default(TextStyle);

    /// <summary>
    /// Visual flow data for where the text should be rendered in its bounding rect,
    /// e.g. centering and line spacing.
    /// </summary>
    public TextFlow  textFlowConfig = default(TextFlow);

    /// <summary>
    /// The Type of the renderer being used to render the Label in the scene. May be
    /// null if the Label is not in active use by Unity.
    /// 
    /// For example, if this Label implementation is driving a TextMesh text renderer
    /// component, this type will be TextMesh.
    /// </summary>
    public abstract Type textRenderingType { get; }
  }

  public class Label<TextRenderingComponent, Driver>
                 : Label
                 where TextRenderingComponent : Component
                 where Driver : LabelDriver<TextRenderingComponent>, new() {

    public override Type textRenderingType {
      get { return typeof(TextRenderingComponent); }
    }

    private IGameObjectDriver _backingGameObjectDriver = null;
    protected override IGameObjectDriver gameObjectDriver {
      get { return _backingGameObjectDriver; }
    }

    public Label() {
      _backingGameObjectDriver = new Driver();
      ((Driver)_backingGameObjectDriver).Bind(this);
    }

  }

  public abstract class LabelDriver<TextRenderingComponent>
                          : GameObjectDriver<Label>
                          where TextRenderingComponent : Component {

    /// <summary>
    /// A LabelDriver for a generic TextRenderingComponent requires the
    /// TextRenderingComponent itself to exist on any pooled GameObject it will drive.
    /// </summary>
    public override GameObjectComponentDescription requiredComponents {
      get {
        return new GameObjectComponentDescription(
          typeof(TextRenderingComponent));
      }
    }

    protected TextRenderingComponent componentInstance;

  }

  #endregion

}