/// The modified version of this software is Copyright (C) 2013 ZHing.
/// The original version's copyright as below.

using System;
using UnityEngine;

namespace HTMLEngine.NGUI {
  /// <summary>
  /// Provides image for use with HTMLEngine. Implements abstract class.
  /// </summary>
  public class NGUIImage : HtImage {
    /// <summary>
    /// Is special kind of image? (shows time)
    /// </summary>
    private readonly bool isTime;
    /// <summary>
    /// HtFont for time
    /// </summary>
    private readonly HtFont timeFont;
    /// <summary>
    /// Loaded ui atlas
    /// </summary>
    public readonly UIAtlas uiAtlas;
		/// <summary>
		/// Sprite name.
		/// </summary>
    public readonly string spriteName;
		/// <summary>
		/// Is animation.
		/// </summary>
    public readonly bool isAnim;
		/// <summary>
		/// Animation FPS.
		/// </summary>
    public readonly int FPS;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="source">src attribute from img tag</param>
    /// <param name="fps">fps attribute from img tag</param>
    public NGUIImage(string source, int fps) {
      if ("#time".Equals(source, StringComparison.InvariantCultureIgnoreCase)) {
        isTime = true;
        timeFont = HtEngine.Device.LoadFont("code", 16, false, false);
      } else {
        string atlasName = source.Substring(0, source.LastIndexOf('/'));
        spriteName = source.Substring(source.LastIndexOf('/') + 1);
        isAnim = fps >= 0;
        FPS = fps;
        uiAtlas = Resources.Load("atlases/" + atlasName, typeof(UIAtlas)) as UIAtlas;
        if (uiAtlas == null) {
          Debug.LogError("Could not load html image atlas from " + "atlases/" + atlasName);
        }
      }
    }

    /// <summary>
    /// Returns width of image
    /// </summary>
    public override int Width {
      get {
        if (isTime) return 120;
        if (uiAtlas == null) return 1;
        UIAtlas.Sprite sprite = null;
        if (isAnim) {
          for (int i = 0, imax = uiAtlas.spriteList.Count; i < imax; ++i) {
            UIAtlas.Sprite tmp = uiAtlas.spriteList[i];

            if (string.IsNullOrEmpty(spriteName) || tmp.name.StartsWith(spriteName)) {
              sprite = tmp;
              break;
            }
          }
        } else {
          sprite = uiAtlas.GetSprite(spriteName);
        }
        return sprite != null ? (int)sprite.outer.width : 1;
      }
    }

    /// <summary>
    /// Returns height of image
    /// </summary>
    public override int Height {
      get {
        if (isTime) return 20;
        if (uiAtlas == null) return 1;
        UIAtlas.Sprite sprite = null;
        if (isAnim) {
          for (int i = 0, imax = uiAtlas.spriteList.Count; i < imax; ++i) {
            UIAtlas.Sprite tmp = uiAtlas.spriteList[i];

            if (string.IsNullOrEmpty(spriteName) || tmp.name.StartsWith(spriteName)) {
              sprite = tmp;
              break;
            }
          }
        } else {
          sprite = uiAtlas.GetSprite(spriteName);
        }
        return sprite != null ? (int)sprite.outer.height : 1;
      }
    }

    /// <summary>
    /// Draw method
    /// </summary>
    /// <param name="rect">Where to draw</param>
    /// <param name="color">Color to use (ignored for now)</param>
		/// <param name="linkText">Link text</param>
    /// <param name="userData">User data</param>
    public override void Draw(string id, HtRect rect, HtColor color, string linkText, object userData) {
      if (isTime) {
        var now = DateTime.Now;
        timeFont.Draw(
					"time",
          rect,
          color,
          string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D3}", now.Hour, now.Minute, now.Second, now.Millisecond),
					false,
					Core.DrawTextEffect.None,
					HtColor.white,
					0,
					linkText,
          userData);
      } else if (uiAtlas != null) {
        var root = userData as Transform;
        if (root != null) {
          var go = new GameObject(string.IsNullOrEmpty(id) ? "image" : id, typeof(UISprite));
          go.layer = root.gameObject.layer;
          go.transform.parent = root;
          go.transform.localPosition = new Vector3(rect.X + rect.Width / 2, -rect.Y - rect.Height / 2, -1f);
          go.transform.localScale = new Vector3(rect.Width, rect.Height, 1f);
          var spr = go.GetComponent<UISprite>();
          spr.pivot = UIWidget.Pivot.Center;
          spr.atlas = uiAtlas;
          spr.color = new Color32(color.R, color.G, color.B, color.A);

          if (isAnim) {
            var sprAnim = go.AddComponent<UISpriteAnimation>();
            sprAnim.framesPerSecond = FPS;
            sprAnim.namePrefix = spriteName;
          } else {
            spr.spriteName = spriteName;
            spr.MakePixelPerfect();
            if (go.transform.localScale.y == 0f) go.transform.localScale = new Vector3(go.transform.localScale.x, 1f, 1f);
          }

          if (!string.IsNullOrEmpty(linkText)) {
            var collider = go.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.center = new Vector3(0f, 0f, -0.25f);
            collider.size = new Vector3(1f, 1f, 1f);

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
              spr.color.r * HtEngine.LinkPressedFactor,
              spr.color.g * HtEngine.LinkPressedFactor,
              spr.color.b * HtEngine.LinkPressedFactor, spr.color.a);
            uiButtonColor.duration = 0f;

            var uiButtonMessage = go.AddComponent<UIButtonMessage>();
            uiButtonMessage.target = root.gameObject;
            uiButtonMessage.functionName = HtEngine.LinkFunctionName;
          }
        } else {
          HtEngine.Log(HtLogLevel.Error, "Can't draw without root.");
        }
      }
    }
  }

}