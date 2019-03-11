/// The modified version of this software is Copyright (C) 2013 ZHing.
/// The original version's copyright as below.

using UnityEngine;

namespace HTMLEngine.NGUI {
  /// <summary>
  /// Provides font for use with HTMLEngine. Implements abstract class.
  /// </summary>
  public class NGUIFont : HtFont {
    /// <summary>
    /// style to draw
    /// </summary>
    public UIFont uiFont;
    /// <summary>
    /// Width of whitespace
    /// </summary>
    private readonly int whiteSize;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="face">Font name</param>
    /// <param name="size">Font size</param>
    /// <param name="bold">Bold flag</param>
    /// <param name="italic">Italic flag</param>
    public NGUIFont(string face, int size, bool bold, bool italic)
      : base(face, size, bold, italic)
    {
      // load from resources
      string key = string.Format("{0}{1}{2}{3}", face, size, bold ? "b" : "", italic ? "i" : "");
      var uiFontTemplate = Resources.Load("fonts/" + key, typeof(UIFont)) as UIFont;

      // showing error if font not found
      if (uiFontTemplate == null) {
        Debug.LogError("Could not load font: " + key);
        return;
      }

      uiFont = UnityEngine.GameObject.Instantiate(uiFontTemplate) as UIFont;
      Object.DontDestroyOnLoad(uiFont);
      var cachedHtmlFontsGo = GameObject.Find("/cachedHtmlFonts");
      if (cachedHtmlFontsGo == null)
        cachedHtmlFontsGo = new GameObject("cachedHtmlFonts");
      uiFont.transform.parent = cachedHtmlFontsGo.transform;
      uiFont.name = key;

      //// calculating whitesize
      NGUIText.regionWidth = 10;
      Vector2 PrintedSize = NGUIText.CalculatePrintedSize(".", uiFont);


      whiteSize = (int)(PrintedSize.y);
      this.whiteSize = whiteSize/5;
    }

    /// <summary>
    /// Space between text lines in pixels
    /// </summary>
    public override int LineSpacing { get { return (int)uiFont.defaultSize; } }

    /// <summary>
    /// Space between words
    /// </summary>
    public override int WhiteSize { 
        get 
        {
            return this.whiteSize; 
        } 
    }

    /// <summary>
    /// Measuring text width and height
    /// </summary>
    /// <param name="text">text to measure</param>
    /// <returns>width and height of measured text</returns>
    public override HtSize Measure(string text) {
        NGUIText.regionWidth = 1000000;
        Vector2 PrintedSize = NGUIText.CalculatePrintedSize(text, uiFont);
        var r = PrintedSize; //* uiFont.size;

        float scale = 1.0f; //uiFont.defaultSize / 16.0f;
        return new HtSize((int)(r.x * scale), (int)r.y);
    }

    /// <summary>
    /// Draw method.
    /// </summary>
    /// <param name="rect">Where to draw</param>
    /// <param name="color">Text color</param>
    /// <param name="text">Text</param>
    /// <param name="isEffect">Is effect</param>
		/// <param name="effect">Effect</param>
    /// <param name="effectColor">Effect color</param>
		/// <param name="effectAmount">Effect amount</param>
		/// <param name="linkText">Link text</param>
    /// <param name="userData">User data</param>
    public override void Draw(string id, HtRect rect, HtColor color, string text, bool isEffect, Core.DrawTextEffect effect, HtColor effectColor, int effectAmount, string linkText, object userData) {
			// NGUI do not need to draw effect.
      if (isEffect) return;

      var root = userData as Transform;
      if (root != null) {
        var go = new GameObject(string.IsNullOrEmpty(id) ? "label" : id, typeof(UILabel));
        go.layer = root.gameObject.layer;
        go.transform.parent = root;
        go.transform.localPosition = new Vector3(rect.X + rect.Width / 2, -rect.Y - rect.Height / 2, 0f);
        go.transform.localScale = new Vector3(uiFont.size, uiFont.size, 1f);
        var lab = go.GetComponent<UILabel>();
        lab.pivot = UIWidget.Pivot.Center;
        lab.supportEncoding = false;
        lab.font = uiFont;
        lab.text = text;
        lab.fontSize = uiFont.defaultSize;
        lab.color = new Color32(color.R, color.G, color.B, color.A);
        switch (effect) {
        case Core.DrawTextEffect.Outline:
          lab.effectStyle = UILabel.Effect.Outline;
          break;
        case Core.DrawTextEffect.Shadow:
          lab.effectStyle = UILabel.Effect.Shadow;
          break;
        }
        lab.effectColor = new Color32(effectColor.R, effectColor.G, effectColor.B, effectColor.A);
        lab.effectDistance = new Vector2(effectAmount, effectAmount);
        lab.MakePixelPerfect();

				// build link.
        if (!string.IsNullOrEmpty(linkText)) {
          var collider = go.AddComponent<BoxCollider>();
          collider.isTrigger = true;
          collider.center = new Vector3(0f, 0f, -0.25f);
          collider.size = new Vector3(lab.relativeSize.x, 1f, 1f);

          var nguiLinkText = go.AddComponent<NGUILinkText>();
          nguiLinkText.linkText = linkText;

          var uiButtonColor = go.AddComponent<UIButtonColor>();
          uiButtonColor.tweenTarget = go;
          uiButtonColor.hover = new Color32(
            HtEngine.LinkHoverColor.R,
            HtEngine.LinkHoverColor.G,
            HtEngine.LinkHoverColor.B,
            HtEngine.LinkHoverColor.A);
          uiButtonColor.pressed = new Color(
            lab.color.r * HtEngine.LinkPressedFactor,
            lab.color.g * HtEngine.LinkPressedFactor,
            lab.color.b * HtEngine.LinkPressedFactor, lab.color.a);
          uiButtonColor.duration = 0f;

          var uiButtonMessage = go.AddComponent<UIButtonMessage>();
          uiButtonMessage.target = root.gameObject;
          uiButtonMessage.functionName = HtEngine.LinkFunctionName;
        }
      } else {
        HtEngine.Log(HtLogLevel.Error, "Can't draw without root.");
      }
		}

		/// <summary>
		/// on the font be released.
		/// </summary>
    public void OnRelease() {
      if (uiFont != null && (bool)uiFont) {
        UnityEngine.GameObject.Destroy(uiFont.gameObject);
        uiFont = null;
      }
    }
  }
}