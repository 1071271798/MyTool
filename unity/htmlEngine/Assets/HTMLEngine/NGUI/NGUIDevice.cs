/// The modified version of this software is Copyright (C) 2013 ZHing.
/// The original version's copyright as below.

using System.Collections.Generic;
using UnityEngine;

namespace HTMLEngine.NGUI {
  /// <summary>
  /// Provides gate between HTMLEngine and Unity3D. Implements abstract class.
  /// </summary>
  public class NGUIDevice : HtDevice {
    /// <summary>
    /// Fonts cache (to do not load every time from resouces)
    /// </summary>
    private readonly Dictionary<string, NGUIFont> fonts = new Dictionary<string, NGUIFont>();
    /// <summary>
    /// Image cache (same thing)
    /// </summary>
    private readonly Dictionary<string, NGUIImage> images = new Dictionary<string, NGUIImage>();

    /// <summary>
    /// White texture (for FillRect method)
    /// </summary>
    private static Texture2D whiteTex;

    /// <summary>
    /// Load font
    /// </summary>
    /// <param name="face">Font name</param>
    /// <param name="size">Font size</param>
    /// <param name="bold">Bold flag</param>
    /// <param name="italic">Italic flag</param>
    /// <returns>Loaded font</returns>
    public override HtFont LoadFont(string face, int size, bool bold, bool italic) {
      // try get from cache
      string key = string.Format("{0}{1}{2}{3}", face, size, bold ? "b" : "", italic ? "i" : "");
      NGUIFont ret;
      if (fonts.TryGetValue(key, out ret)) return ret;
      // fail with cache, so create new and store into cache
      ret = new NGUIFont(face, size, bold, italic);
      fonts[key] = ret;
      return ret;
    }

    /// <summary>
    /// Load image
    /// </summary>
    /// <param name="src">src attribute from img tag</param>
    /// <param name="fps">fps attribute from img tag</param>
    /// <returns>Loaded image</returns>
    public override HtImage LoadImage(string src, int fps) {
      // try get from cache
      NGUIImage ret;
      if (images.TryGetValue(src, out ret)) return ret;
      // fail with cache, so create new and store into cache
      ret = new NGUIImage(src, fps);
      images[src] = ret;
      return ret;
    }

    /// <summary>
    /// FillRect implementation
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="color"></param>
		/// <param name="userData"></param>
    public override void FillRect(HtRect rect, HtColor color, object userData) {
      var root = userData as Transform;
      if (root != null) {
        var go = new GameObject("fill", typeof(UISlicedSprite));
        go.layer = root.gameObject.layer;
        go.transform.parent = root;
        go.transform.localPosition = new Vector3(rect.X + rect.Width / 2, -rect.Y - rect.Height / 2 - 2, -1f);
        go.transform.localScale = new Vector3(rect.Width, rect.Height, 1f);
        var spr = go.GetComponent<UISlicedSprite>();
        spr.pivot = UIWidget.Pivot.Center;
        spr.atlas = Resources.Load("atlases/white", typeof(UIAtlas)) as UIAtlas;
        spr.spriteName = "white";
        spr.color = new Color32(color.R, color.G, color.B, color.A);
        spr.MakePixelPerfect();
        if (go.transform.localScale.y == 0f) go.transform.localScale = new Vector3(go.transform.localScale.x, 1f, 1f);
      } else {
        HtEngine.Log(HtLogLevel.Error, "Can't draw without root.");
     }
    }

    /// <summary>
    /// On device is released.
    /// </summary>
    public override void OnRelease() {
      foreach (var pair in fonts) {
        pair.Value.OnRelease();
      }
    }
  }
}
