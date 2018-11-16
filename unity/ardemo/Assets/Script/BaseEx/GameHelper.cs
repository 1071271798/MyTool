using UnityEngine;
using System.Collections;
using Game.Platform;

public class GameHelper
{
    public static T FindChildComponent<T>(Transform trans, string path) where T : Component
    {
        if (trans != null)
        {

            Transform tmp = trans.Find(path);
            if (tmp != null)
            {
                T obj = tmp.GetComponent<T>();
                return obj;
            }
        }
        return null;
    }

    public static bool SetLabelText(Transform label, string text)
    {
        if (null != label)
        {
            UILabel lb = label.GetComponent<UILabel>();
            if (null != lb)
            {
                lb.text = text;
                return true;
            }
        }
        return false;
    }

    public static bool SetSprite(Transform sprite, string spriteName)
    {
        if (null != sprite)
        {
            UISprite sp = sprite.GetComponent<UISprite>();
            if (null != sp)
            {
                sp.spriteName = spriteName;
                sp.MakePixelPerfect();
                return true;
            }
        }
        return false;
    }

    public static int GetTextWidth(UILabel lb)
    {
        if (null != lb)
        {
            Font font = lb.bitmapFont.dynamicFont;
            int fontsize = lb.fontSize;
            string text = lb.text;
            font.RequestCharactersInTexture(text, fontsize, lb.fontStyle);
            CharacterInfo characterInfo;
            float width = 0f;
            for (int i = 0; i < text.Length; i++)
            {

                font.GetCharacterInfo(text[i], out characterInfo, fontsize);
                width += characterInfo.width;
            }
            return (int)width;
        }
        return 0;
    }

    public static void PlayTweenPosition(TweenPosition tweens, Vector3 to, float duration = 0.3f)
    {
        try
        {
            if (null != tweens)
            {
                tweens.enabled = true;
                Vector3 from = tweens.transform.localPosition;
                tweens.ResetToBeginning();
                tweens.duration = duration;
                tweens.from = from;
                tweens.to = to;
                tweens.Play(true);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    public static TweenPosition PlayTweenPosition(Transform trans, Vector3 to, float duration = 0.3f)
    {
        if (null != trans)
        {
            TweenPosition tweenPosition = trans.GetComponent<TweenPosition>();
            if (null != tweenPosition)
            {
                PlayTweenPosition(tweenPosition, to, duration);
            }
            else
            {
                trans.localPosition = to;
            }
            return tweenPosition;
        }
        return null;
    }

    public static TweenPosition PlayTweenPosition(Transform trans, UIWidget.Pivot pivot, Vector2 offset, float duration = 0.6f, bool needChangePos = true)
    {
        if (null != trans)
        {
            Vector2 targetPos = UIManager.GetWinPos(trans, pivot, offset.x, offset.y);
            if (needChangePos)
            {
                Vector2 size = NGUIMath.CalculateRelativeWidgetBounds(trans).size;
                switch (pivot)
                {
                    case UIWidget.Pivot.TopLeft:
                    case UIWidget.Pivot.Left:
                    case UIWidget.Pivot.BottomLeft:
                        trans.localPosition = targetPos - new Vector2(size.x + 220, 0);
                        break;
                    case UIWidget.Pivot.TopRight:
                    case UIWidget.Pivot.Right:
                    case UIWidget.Pivot.BottomRight:
                        trans.localPosition = targetPos + new Vector2(size.x + 220, 0);
                        break;
                    case UIWidget.Pivot.Top:
                        trans.localPosition = targetPos + new Vector2(0, size.y + 220);
                        break;
                    case UIWidget.Pivot.Bottom:
                        trans.localPosition = targetPos - new Vector2(0, size.y + 220);
                        break;
                }
            }
            return PlayTweenPosition(trans, targetPos, duration);
        }
        return null;
    }

    public static void SetPosition(Transform trans, UIWidget.Pivot pivot, Vector2 offset)
    {
        if (null != trans)
        {
            trans.localPosition = UIManager.GetWinPos(trans, pivot, offset.x, offset.y);
        }
    }

    public static void PlayTweenScale(TweenScale tweens, Vector3 to, float duration = 0.3f)
    {
        try
        {
            if (null != tweens)
            {
                tweens.enabled = true;
                Vector3 from = tweens.transform.localScale;
                tweens.ResetToBeginning();
                tweens.duration = duration;
                tweens.from = from;
                tweens.to = to;
                tweens.Play(true);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    public static TweenScale PlayTweenScale(Transform trans, Vector3 to, float duration = 0.3f)
    {
        if (null != trans)
        {
            TweenScale tweenScale = trans.GetComponent<TweenScale>();
            if (null != tweenScale)
            {
                PlayTweenScale(tweenScale, to, duration);
            }
            else
            {
                trans.localScale = to;
            }
            return tweenScale;
        }
        return null;
    }

    public static void PlayTweenRota(TweenRotation tweens, Vector3 to, float duration = 0.3f)
    {
        try
        {
            if (null != tweens)
            {
                tweens.enabled = true;
                Vector3 from = tweens.transform.localEulerAngles;
                tweens.ResetToBeginning();
                tweens.duration = duration;
                tweens.from = from;
                tweens.to = to;
                tweens.Play(true);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    public static TweenRotation PlayTweenRota(Transform trans, Vector3 to, float duration = 0.3f)
    {
        if (null != trans)
        {
            TweenRotation tweenRotation = trans.GetComponent<TweenRotation>();
            if (null != tweenRotation)
            {
                PlayTweenRota(tweenRotation, to, duration);
            }
            else
            {
                trans.localEulerAngles = to;
            }
            return tweenRotation;
        }
        return null;
    }

    public static void PlayTweenAlpha(TweenAlpha tweens, float to, float duration = 0.3f)
    {
        try
        {
            if (null != tweens)
            {
                tweens.enabled = true;
                float from = tweens.value;
                tweens.ResetToBeginning();
                tweens.duration = duration;
                tweens.from = from;
                tweens.to = to;
                tweens.Play(true);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    public static TweenAlpha PlayTweenAlpha(Transform trans, float to, float duration = 0.3f)
    {
        if (null != trans)
        {
            TweenAlpha tweenAlpha = trans.GetComponent<TweenAlpha>();
            if (null != tweenAlpha)
            {
                PlayTweenAlpha(tweenAlpha, to, duration);
            }
            else
            {
                SetTransformAlpha(trans, to);
            }
            return tweenAlpha;
        }
        return null;
    }

    public static void SetTransformAlpha(Transform trans, float alpha)
    {
        UIWidget widget = trans.GetComponentInChildren<UIWidget>();
        if (null != widget)
        {
            widget.alpha = alpha;
        }
        else
        {
            UIPanel uiPanel = trans.GetComponent<UIPanel>();
            if (null != uiPanel)
            {
                uiPanel.alpha = alpha;
            }
        }
    }

    public static void PlayMyTweenAlpha(MyTweenAlpha tweens, float to, float duration = 0.3f)
    {
        try
        {
            if (null != tweens)
            {
                tweens.enabled = true;
                float from = tweens.value;
                tweens.ResetToBeginning();
                tweens.duration = duration;
                tweens.from = from;
                tweens.to = to;
                tweens.Play(true);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    public static void SetButtonCentered(float btnWidth, Transform icon, Transform label, float space)
    {
        if (null != icon && null != label)
        {
            UISprite sp = icon.GetComponent<UISprite>();
            UILabel lb = label.GetComponent<UILabel>();
            if (null != sp && null != lb)
            {
                float lbWidth = lb.printedSize.x;
                float margin = (btnWidth - sp.width - lbWidth - space) / 2;
                icon.localPosition = new Vector3(-btnWidth / 2 + margin + sp.width / 2, 0);
                if (lb.pivot == UIWidget.Pivot.Left)
                {
                    label.localPosition = new Vector3(btnWidth / 2 - margin - lbWidth, 0);
                }
                else if (lb.pivot == UIWidget.Pivot.Right)
                {
                    label.localPosition = new Vector3(btnWidth / 2 - margin, 0);
                }
                else
                {

                    label.localPosition = new Vector3(btnWidth / 2 - margin - lbWidth / 2, 0);
                }
            }
        }
    }

    public static void SetButtonCentered(Transform btn, float space)
    {
        Transform icon = btn.Find("sprite");
        Transform label = btn.Find("Label");
        UISprite bg = GameHelper.FindChildComponent<UISprite>(btn, "Background");
        if (null != bg)
        {
            SetButtonCentered(bg.width, icon, label, space);
        }
    }
}
