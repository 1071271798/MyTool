/// The modified version of this software is Copyright (C) 2013 ZHing.
/// The original version's copyright as below.

using UnityEngine;
using System.Collections;

using HTMLEngine;
using HTMLEngine.NGUI;

[AddComponentMenu("HTMLEngine/NGUIHTML")]
public class NGUIHTML : MonoBehaviour {

  public enum AutoScrollType {
		MANUAL,
		AUTO_TOP,
		AUTO_BOTTOM,
  }

  public string _html = "";
  public int maxLineWidth = 0;
  public AutoScrollType autoScroll = AutoScrollType.MANUAL;

  /// <summary>
  /// is the html content changed?
  /// </summary>
  private bool changed = false;

  /// <summary>
  /// our html compiler
  /// </summary>
  private HtCompiler compiler;

  /// <summary>
  /// setting text here will raise changed flag
  /// </summary>
  public string html {
    get { return this._html; }
    set {
      this._html = value;
      this.changed = true;
    }
  }

  void Start() {
    compiler = HtEngine.GetCompiler();
  }

	void Update() {
    if (changed && compiler != null) {
      // we have new html text, so compile it
      compiler.Compile(html, maxLineWidth > 0 ? maxLineWidth : Screen.width);

			// destroy old widgets.
			foreach (Transform childTr in transform)
        Destroy(childTr.gameObject);

      // generate the widgets.
      compiler.Draw(Time.deltaTime, transform);

      // release changed flag
      changed = false;

      if (autoScroll != AutoScrollType.MANUAL) {
        StartCoroutine(updateAutoScroll());
      }
    }
  }

  void OnDestroy() {
    // we need to dispose compiler to prevent GC
    if (compiler != null) {
      compiler.Dispose();
      compiler = null;
    }
  }

  private IEnumerator updateAutoScroll() {
    yield return new WaitForEndOfFrame();

    var uiDraggablePanel = NGUITools.FindInParents<UIDraggablePanel>(gameObject);
    if (uiDraggablePanel != null) {
      switch (autoScroll) {
      case AutoScrollType.AUTO_TOP:
        uiDraggablePanel.relativePositionOnReset = Vector2.zero;
        break;
      case AutoScrollType.AUTO_BOTTOM:
        uiDraggablePanel.relativePositionOnReset = new Vector2(0f, 1f);
        break;
      }
      uiDraggablePanel.ResetPosition();
    }
  }
}
