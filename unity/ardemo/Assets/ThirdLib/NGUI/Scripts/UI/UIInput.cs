//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY || UNITY_WINRT)
#define MOBILE
#endif

using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Input field makes it possible to enter custom information within the UI.
/// </summary>

[AddComponentMenu("NGUI/UI/Input Field")]
public class UIInput : MonoBehaviour
{
	public enum InputType
	{
		Standard,
		AutoCorrect,
		Password,
	}

	public enum Validation
	{
		None,
		Integer,
		Float,
		Alphanumeric,
		Username,
		Name,
	}

	public enum KeyboardType
	{
		Default = 0,
		ASCIICapable = 1,
		NumbersAndPunctuation = 2,
		URL = 3,
		NumberPad = 4,
		PhonePad = 5,
		NamePhonePad = 6,
		EmailAddress = 7,
	}

	public enum OnReturnKey
	{
		Default,
		Submit,
		NewLine,
	}

    public delegate void OnSelectDelegate(bool isSelect, GameObject obj);

	public delegate char OnValidate (string text, int charIndex, char addedChar);

	/// <summary>
	/// Currently active input field. Only valid during callbacks.
	/// </summary>

	static public UIInput current;

	/// <summary>
	/// Currently selected input field, if any.
	/// </summary>

	static public UIInput selection;

	/// <summary>
	/// Text label used to display the input's value.
	/// </summary>

	public UILabel label;

	/// <summary>
	/// Type of data expected by the input field.
	/// </summary>

	public InputType inputType = InputType.Standard;

	/// <summary>
	/// What to do when the Return key is pressed on the keyboard.
	/// </summary>

	public OnReturnKey onReturnKey = OnReturnKey.Default;

	/// <summary>
	/// Keyboard type applies to mobile keyboards that get shown.
	/// </summary>

	public KeyboardType keyboardType = KeyboardType.Default;

	/// <summary>
	/// Whether the input will be hidden on mobile platforms.
	/// </summary>

	public bool hideInput = false;

	/// <summary>
	/// What kind of validation to use with the input field's data.
	/// </summary>

	public Validation validation = Validation.None;

	/// <summary>
	/// Maximum number of characters allowed before input no longer works.
	/// </summary>

	public int characterLimit = 0;

	/// <summary>
	/// Field in player prefs used to automatically save the value.
	/// </summary>

	public string savedAs;

	/// <summary>
	/// Object to select when Tab key gets pressed.
	/// </summary>

	public GameObject selectOnTab;

	/// <summary>
	/// Color of the label when the input field has focus.
	/// </summary>

	public Color activeTextColor = Color.white;

	/// <summary>
	/// Color used by the caret symbol.
	/// </summary>

	public Color caretColor = new Color(1f, 1f, 1f, 0.8f);

	/// <summary>
	/// Color used by the selection rectangle.
	/// </summary>

	public Color selectionColor = new Color(1f, 223f / 255f, 141f / 255f, 0.5f);

	/// <summary>
	/// Event delegates triggered when the input field submits its data.
	/// </summary>

	public List<EventDelegate> onSubmit = new List<EventDelegate>();

	/// <summary>
	/// Event delegates triggered when the input field's text changes for any reason.
	/// </summary>

	public List<EventDelegate> onChange = new List<EventDelegate>();


    public OnSelectDelegate onSelect;
	/// <summary>
	/// Custom validation callback.
	/// </summary>

	public OnValidate onValidate;

	/// <summary>
	/// Input field's value.
	/// </summary>

	[SerializeField][HideInInspector] protected string mValue;

	[System.NonSerialized] protected string mDefaultText = "";
	[System.NonSerialized] protected Color mDefaultColor = Color.white;
	[System.NonSerialized] protected float mPosition = 0f;
	[System.NonSerialized] protected bool mDoInit = true;
	[System.NonSerialized] protected UIWidget.Pivot mPivot = UIWidget.Pivot.TopLeft;
	[System.NonSerialized] protected bool mLoadSavedValue = true;

	static protected int mDrawStart = 0;
	static protected string mLastIME = "";

#if MOBILE
	// Unity fails to compile if the touch screen keyboard is used on a non-mobile device
	static protected TouchScreenKeyboard mKeyboard;
	static bool mWaitForKeyboard = false;
#endif
	[System.NonSerialized] protected int mSelectionStart = 0;
	[System.NonSerialized] protected int mSelectionEnd = 0;
	[System.NonSerialized] protected UITexture mHighlight = null;
	[System.NonSerialized] protected UITexture mCaret = null;
	[System.NonSerialized] protected Texture2D mBlankTex = null;
	[System.NonSerialized] protected float mNextBlink = 0f;
	[System.NonSerialized] protected float mLastAlpha = 0f;
	[System.NonSerialized] protected string mCached = "";
	[System.NonSerialized] protected int mSelectMe = -1;
    [System.NonSerialized] protected UILabel mFloatingLabel = null;
    protected TweenColor mFloatingTweenColor;
    protected TweenPosition mFloatingTweenPosition;
    protected TweenScale mFloatingScale;
    public Color mFloatingColor = Color.white;
    public Color mDisFloatingColor = Color.white;
    public bool openFloatLabel = false;
    public Vector2 mFloatOffset = Vector2.zero;

    /// <summary>
    /// Default text used by the input's label.
    /// </summary>

    public string defaultText
	{
		get
		{
			return mDefaultText;
		}
		set
		{
			if (mDoInit) Init();
			mDefaultText = value;
            if (null != mFloatingLabel)
            {
                mFloatingLabel.text = value;
            }
			UpdateLabel();
		}
	}

	/// <summary>
	/// Should the input be hidden?
	/// </summary>

	public bool inputShouldBeHidden
	{
		get
		{
#if UNITY_METRO
			return true;
#else
			return hideInput && label != null && !label.multiLine && inputType != InputType.Password;
#endif
		}
	}

	[System.Obsolete("Use UIInput.value instead")]
	public string text { get { return this.value; } set { this.value = value; } }

	/// <summary>
	/// Input field's current text value.
	/// </summary>

	public string value
	{
		get
		{
#if UNITY_EDITOR
			if (!Application.isPlaying) return "";
#endif
			if (mDoInit) Init();
			return mValue;
		}
		set
		{
#if UNITY_EDITOR
			if (!Application.isPlaying) return;
#endif
			if (mDoInit) Init();
			mDrawStart = 0;

			// BB10's implementation has a bug in Unity
 #if UNITY_4_3
			if (Application.platform == RuntimePlatform.BB10Player)
 #else
			if (Application.platform == RuntimePlatform.BlackBerryPlayer)
 #endif
				value = value.Replace("\\b", "\b");

			// Validate all input
			value = Validate(value);
#if MOBILE
			if (isSelected && mKeyboard != null && mCached != value)
			{
				mKeyboard.text = value;
				mCached = value;
			}
#endif
			if (mValue != value)
			{
				mValue = value;
				mLoadSavedValue = false;

				if (isSelected)
				{
					if (string.IsNullOrEmpty(value))
					{
						mSelectionStart = 0;
						mSelectionEnd = 0;
					}
					else
					{
						mSelectionStart = value.Length;
						mSelectionEnd = mSelectionStart;
					}
				}
				else SaveToPlayerPrefs(value);

				UpdateLabel();
				ExecuteOnChange();
			}
		}
	}

	[System.Obsolete("Use UIInput.isSelected instead")]
	public bool selected { get { return isSelected; } set { isSelected = value; } }

	/// <summary>
	/// Whether the input is currently selected.
	/// </summary>

	public bool isSelected
	{
		get
		{
			return selection == this;
		}
		set
		{
			if (!value) { if (isSelected) UICamera.selectedObject = null; }
			else UICamera.selectedObject = gameObject;
		}
	}

	/// <summary>
	/// Current position of the cursor.
	/// </summary>

	public int cursorPosition
	{
		get
		{
#if MOBILE
			if (mKeyboard != null && !inputShouldBeHidden) return value.Length;
#endif
			return isSelected ? mSelectionEnd : value.Length;
		}
		set
		{
			if (isSelected)
			{
#if MOBILE
				if (mKeyboard != null && !inputShouldBeHidden) return;
#endif
				mSelectionEnd = value;
				UpdateLabel();
			}
		}
	}

	/// <summary>
	/// Index of the character where selection begins.
	/// </summary>

	public int selectionStart
	{
		get
		{
#if MOBILE
			if (mKeyboard != null && !inputShouldBeHidden) return 0;
#endif
			return isSelected ? mSelectionStart : value.Length;
		}
		set
		{
			if (isSelected)
			{
#if MOBILE
				if (mKeyboard != null && !inputShouldBeHidden) return;
#endif
				mSelectionStart = value;
				UpdateLabel();
			}
		}
	}

	/// <summary>
	/// Index of the character where selection ends.
	/// </summary>

	public int selectionEnd
	{
		get
		{
#if MOBILE
			if (mKeyboard != null && !inputShouldBeHidden) return value.Length;
#endif
			return isSelected ? mSelectionEnd : value.Length;
		}
		set
		{
			if (isSelected)
			{
#if MOBILE
				if (mKeyboard != null && !inputShouldBeHidden) return;
#endif
				mSelectionEnd = value;
				UpdateLabel();
			}
		}
	}

	/// <summary>
	/// Caret, in case it's needed.
	/// </summary>

	public UITexture caret { get { return mCaret; } }

	/// <summary>
	/// Validate the specified text, returning the validated version.
	/// </summary>

	public string Validate (string val)
	{
		if (string.IsNullOrEmpty(val)) return "";

		StringBuilder sb = new StringBuilder(val.Length);
		for (int i = 0; i < val.Length; ++i)
		{
			char c = val[i];
            /*bool isEmoji = false;
            if (0xd800 <= c && c <= 0xdbff)
            {
                if (i < val.Length - 1)
                {
                    char ls = val[i +  1];
                    int uc = ((c - 0xd800) * 0x400) + (ls - 0xdc00) + 0x10000;
                    if (0x1d000 <= uc && uc <= 0x1f77f)
                    {
                        isEmoji = true;
                    }
                }
            }
            else
            {
                if (0x2100 <= c && c <= 0x27ff && c != 0x263b)
                {
                    isEmoji = true;
                }
                else if (0x2B05 <= c && c <= 0x2b07)
                {
                    isEmoji = true;
                }
                else if (0x2934 <= c && c <= 0x2935)
                {
                    isEmoji = true;
                }
                else if (0x3297 <= c && c <= 0x3299)
                {
                    isEmoji = true;
                }
                else if (c == 0xa9 || c == 0xae || c == 0x303d || c == 0x3030 || c == 0x2b55 || c == 0x2b1c || c == 0x2b1b || c == 0x2b50 || c == 0x231a)
                {
                    isEmoji = true;
                }
                if (!isEmoji && i < val.Length - 1)
                {
                    char ls = val[i + 1];
                    if (ls == 0x20e3)
                    {
                        isEmoji = true;
                    }
                }
            }
            if (isEmoji) c = (char)0;
            else */if (onValidate != null) c = onValidate(sb.ToString(), sb.Length, c);
			else if (validation != Validation.None) c = Validate(sb.ToString(), sb.Length, c);
            
			if (c != 0) sb.Append(c);
		}
        string str = RemoveEmoji(sb.ToString());
		if (characterLimit > 0 && str.Length > characterLimit)
			return str.Substring(0, characterLimit);
		return str;
	}
    /// <summary>
    /// 移除掉emoji表情
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    string RemoveEmoji(string str)
    {
        Regex regex = new Regex(@"[^\u0020-\u007E\u0400-\u052F\u0600-\u06FF\u0750-\u077F\u00A0-\u00BE\u2E80-\uA4CF\uF900-\uFAFF\uFE30-\uFE4F\uFF00-\uFFEF\u0080-\u009F\u2000-\u201f\r\n]");
        //Regex regex = new Regex(@"\p{Cs}");
        if (regex.IsMatch(str))
        {
            return regex.Replace(str, string.Empty);
        }
        return str;
    }
    bool HaveArab(string str)
    {
        for (int i = 0; i < str.Length; ++i)
        {
            char c = str[i];
            if (IsArab(c))
            {
                return true;
            }
        }
        return false;
    }

    bool IsArab(char c)
    {
        if (c >= 0x0600 && c <= 0x06FF || c >= 0x0750 && c <= 0x077F)
        {//阿拉伯语
            return true;
        }
        return false;
    }

    string ConvertArab(string str)
    {
        char[] chars = str.ToCharArray();
        if (null != chars)
        {
            List<List<char>> list = new List<List<char>>();
            int newState = 0;
            int oldState = 0;
            List<char> addList = null;
            for (int i = 0, imax = chars.Length; i < imax; ++i)
            {
                if (chars[i] > 127)
                {
                    newState = -1;
                }
                else
                {
                    newState = 1;
                }
                if (newState != oldState)
                {
                    if (null != addList)
                    {
                        list.Add(addList);
                    }
                    addList = new List<char>();
                    oldState = newState;
                }
                if (newState == -1 || chars[i] == ' ' || chars[i] == '.' || chars[i] == ',' || chars[i] == ';')
                {//阿拉伯语，需逆序||空格需逆序
                    if (addList.Count < 1)
                    {
                        addList.Add(chars[i]);
                    }
                    else
                    {
                        addList.Insert(0, chars[i]);
                    }
                }
                else
                {
                    addList.Add(chars[i]);
                }
            }
            if (null != addList)
            {
                list.Add(addList);
            }
            StringBuilder sb = new StringBuilder();
            for (int i = list.Count - 1; i >= 0; --i)
            {//把list逆序
                sb.Append(list[i].ToArray());
            }
            return sb.ToString();
        }
        return string.Empty;
    }

    bool IsRussian(char c)
    {
        if (c >= 0x0400 && c <= 0x052F)
        {//俄语
            return true;
        }
        return false;
    }

    /// <summary>
    /// Automatically set the value by loading it from player prefs if possible.
    /// </summary>

    void Start ()
	{
		if (mLoadSavedValue && !string.IsNullOrEmpty(savedAs)) LoadValue();
		else value = mValue.Replace("\\n", "\n");
	}

	/// <summary>
	/// Labels used for input shouldn't support rich text.
	/// </summary>

	protected void Init ()
	{
		if (mDoInit && label != null)
		{
			mDoInit = false;
			mDefaultText = label.text;
			mDefaultColor = label.color;
			label.supportEncoding = false;

			if (label.alignment == NGUIText.Alignment.Justified)
			{
				label.alignment = NGUIText.Alignment.Left;
				Debug.LogWarning("Input fields using labels with justified alignment are not supported at this time", this);
			}

			mPivot = label.pivot;
			mPosition = label.cachedTransform.localPosition.x;

            if (openFloatLabel && null == mFloatingLabel)
            {
                mFloatingLabel = NGUITools.AddWidget<UILabel>(label.cachedGameObject);
                mFloatingLabel.name = "Floating Label";
                mFloatingLabel.pivot = label.pivot;
                mFloatingLabel.bitmapFont = this.label.bitmapFont;
                mFloatingLabel.fontSize = this.label.fontSize;
                mFloatingLabel.overflowMethod = this.label.overflowMethod;
                mFloatingLabel.text = mDefaultText;
                mFloatingLabel.color = mFloatingColor;
                mFloatingLabel.enabled = false;
                mFloatingLabel.width = this.label.width;
                mFloatingLabel.height = this.label.height;
            }
			UpdateLabel();
		}
	}

    void ShowFloatingLabel()
    {
        mFloatingLabel.enabled = true;
        Vector3 pos = new Vector3(mFloatOffset.x, mFloatOffset.y + label.fontSize / 2 + mFloatingLabel.fontSize / 2);
        mFloatingLabel.transform.localPosition = Vector3.zero;
        if (null == mFloatingTweenPosition)
        {
            mFloatingTweenPosition = TweenPosition.Begin(mFloatingLabel.gameObject, 0.3f, pos);
        }
        else if (!mFloatingTweenPosition.enabled)
        {
            mFloatingTweenPosition.enabled = true;
            mFloatingTweenPosition.ResetToBeginning();
            mFloatingTweenPosition.duration = 0.3f;
            mFloatingTweenPosition.from = Vector3.zero;
            mFloatingTweenPosition.to = pos;
            mFloatingTweenPosition.Play(true);
        }
        mFloatingLabel.color = mDefaultColor;
        if (null == mFloatingTweenColor)
        {
            mFloatingTweenColor = TweenColor.Begin(mFloatingLabel.gameObject, 0.3f, mFloatingColor);
        }
        else if (!mFloatingTweenColor.enabled)
        {
            mFloatingTweenColor.enabled = true;
            mFloatingTweenColor.ResetToBeginning();
            mFloatingTweenColor.duration = 0.3f;
            mFloatingTweenColor.from = mDefaultColor;
            mFloatingTweenColor.to = mFloatingColor;
            mFloatingTweenColor.Play(true);
        }
        mFloatingLabel.transform.localScale = Vector3.one;
        if (null == mFloatingScale)
        {
            mFloatingScale = TweenScale.Begin(mFloatingLabel.gameObject, 0.3f, Vector3.one * 0.7f);
        }
        else if (!mFloatingScale.enabled)
        {
            mFloatingScale.enabled = true;
            mFloatingScale.ResetToBeginning();
            mFloatingScale.duration = 0.3f;
            mFloatingScale.from = Vector3.one;
            mFloatingScale.to = Vector3.one * 0.7f;
            mFloatingScale.Play(true);
        }
        mFloatingTweenPosition.SetOnFinished(ShowFloatingLabelFinished);
    }

    void HideFloatingLabel()
    {
        if (null == mFloatingTweenColor)
        {
            mFloatingTweenColor = TweenColor.Begin(mFloatingLabel.gameObject, 0.3f, mDefaultColor);
        }
        else if (!mFloatingTweenColor.enabled)
        {
            mFloatingTweenColor.enabled = true;
            mFloatingTweenColor.ResetToBeginning();
            mFloatingTweenColor.duration = 0.3f;
            mFloatingTweenColor.from = mFloatingColor;
            mFloatingTweenColor.to = new Color (mDefaultColor.r, mDefaultColor.g, mDefaultColor.b, 0);
            mFloatingTweenColor.Play(true);
        }
        if (null == mFloatingTweenPosition)
        {
            this.label.enabled = false;
            mFloatingTweenPosition = TweenPosition.Begin(mFloatingLabel.gameObject, 0.3f, Vector3.zero);
        }
        else if (!mFloatingTweenPosition.enabled)
        {
            //this.label.enabled = false;
            mFloatingTweenPosition.enabled = true;
            mFloatingTweenPosition.ResetToBeginning();
            mFloatingTweenPosition.duration = 0.3f;
            Vector3 pos = new Vector3(0, mFloatingLabel.fontSize);
            mFloatingTweenPosition.from = pos;// mFloatingLabel.transform.localPosition;
            mFloatingTweenPosition.to = Vector3.zero;
            mFloatingTweenPosition.Play(true);
        }
        if (null == mFloatingScale)
        {
            mFloatingScale = TweenScale.Begin(mFloatingLabel.gameObject, 0.3f, Vector3.one);
        }
        else if (!mFloatingScale.enabled)
        {
            mFloatingScale.enabled = true;
            mFloatingScale.ResetToBeginning();
            mFloatingScale.duration = 0.3f;
            mFloatingScale.from = Vector3.one * 0.7f;
            mFloatingScale.to = Vector3.one;
            mFloatingScale.Play(true);
        }
        mFloatingTweenPosition.SetOnFinished(HideFloatingLabelFinished);
    }

    void ShowFloatingLabelFinished()
    {

    }
    void HideFloatingLabelFinished()
    {
        mFloatingLabel.enabled = false;
        //this.label.enabled = true;
    }

    /// <summary>
    /// Save the specified value to player prefs.
    /// </summary>

    protected void SaveToPlayerPrefs (string val)
	{
		if (!string.IsNullOrEmpty(savedAs))
		{
			if (string.IsNullOrEmpty(val)) PlayerPrefs.DeleteKey(savedAs);
			else PlayerPrefs.SetString(savedAs, val);
		}
	}

	/// <summary>
	/// Selection event, sent by the EventSystem.
	/// </summary>

	protected virtual void OnSelect (bool isSelected)
	{
		if (isSelected) OnSelectEvent();
		else OnDeselectEvent();
        if (null != onSelect && enabled)
        {
            onSelect(isSelected, gameObject);
        }
        
	}

	/// <summary>
	/// Notification of the input field gaining selection.
	/// </summary>

	protected void OnSelectEvent ()
	{
		selection = this;
		if (mDoInit) Init();

		// Unity has issues bringing up the keyboard properly if it's in "hideInput" mode and you happen
		// to select one input in the same Update as de-selecting another.
		if (label != null && NGUITools.GetActive(this))
        {
            mSelectMe = Time.frameCount;
            if (openFloatLabel)
            {
                if (!mFloatingLabel.enabled && string.IsNullOrEmpty(mValue))
                {
                    //ShowFloatingLabel();
                }
                else
                {
                    mFloatingLabel.color = mFloatingColor;
                }
                
            }
        }
            

        
    }

	/// <summary>
	/// Notification of the input field losing selection.
	/// </summary>

	protected void OnDeselectEvent ()
	{
		if (mDoInit) Init();

		if (label != null && NGUITools.GetActive(this))
		{
			mValue = value;
#if MOBILE
			if (mKeyboard != null)
			{
				mWaitForKeyboard = false;
				mKeyboard.active = false;
				mKeyboard = null;
			}
#endif
            if (string.IsNullOrEmpty(mValue))
            {
                label.text = mDefaultText;
                label.color = mDefaultColor;
            }
            else
            {
                if (HaveArab(mValue))
                {
                    label.text = ConvertArab(mValue);
                }
                else
                    label.text = mValue;
            }

			Input.imeCompositionMode = IMECompositionMode.Auto;
			RestoreLabelPivot();

            if (openFloatLabel)
            {
                if (mFloatingLabel.enabled && string.IsNullOrEmpty(mValue))
                {
                    //HideFloatingLabel();
                }
                else
                {
                    mFloatingLabel.color = mDisFloatingColor;
                }
                
            }
        }

		selection = null;
		UpdateLabel();
	}

	/// <summary>
	/// Update the text based on input.
	/// </summary>
	
	void Update ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying) return;
#endif
		if (isSelected)
		{

			if (mDoInit) Init();
#if MOBILE
			// Wait for the keyboard to open. Apparently mKeyboard.active will return 'false' for a while in some cases.
			if (mWaitForKeyboard)
			{
				if (mKeyboard != null && !mKeyboard.active) return;
				mWaitForKeyboard = false;
			}
#endif
			// Unity has issues bringing up the keyboard properly if it's in "hideInput" mode and you happen
			// to select one input in the same Update as de-selecting another.
			if (mSelectMe != -1 && mSelectMe != Time.frameCount)
			{
				mSelectMe = -1;
				mSelectionStart = 0;
				mSelectionEnd = string.IsNullOrEmpty(mValue) ? 0 : mValue.Length;
				mDrawStart = 0;
				label.color = activeTextColor;
#if MOBILE
				if (Application.platform == RuntimePlatform.IPhonePlayer
					|| Application.platform == RuntimePlatform.Android
				    || Application.platform == RuntimePlatform.WP8Player
 #if UNITY_4_3
					|| Application.platform == RuntimePlatform.BB10Player
 #else
					|| Application.platform == RuntimePlatform.BlackBerryPlayer
					|| Application.platform == RuntimePlatform.MetroPlayerARM
					|| Application.platform == RuntimePlatform.MetroPlayerX64
					|| Application.platform == RuntimePlatform.MetroPlayerX86
 #endif
				)
				{
					string val;
					TouchScreenKeyboardType kt;

					if (inputShouldBeHidden)
					{
						TouchScreenKeyboard.hideInput = true;
						kt = (TouchScreenKeyboardType)((int)keyboardType);
 #if UNITY_METRO
						val = "";
 #else
						val = "|";
 #endif
					}
					else if (inputType == InputType.Password)
					{
						TouchScreenKeyboard.hideInput = false;
						kt = TouchScreenKeyboardType.Default;
						val = mValue;
						mSelectionStart = mSelectionEnd;
					}
					else
					{
						TouchScreenKeyboard.hideInput = false;
						kt = (TouchScreenKeyboardType)((int)keyboardType);
						val = mValue;
						mSelectionStart = mSelectionEnd;
					}

					mWaitForKeyboard = true;
					mKeyboard = (inputType == InputType.Password) ?
						TouchScreenKeyboard.Open(val, kt, false, false, true) :
						TouchScreenKeyboard.Open(val, kt, !inputShouldBeHidden && inputType == InputType.AutoCorrect,
							label.multiLine && !hideInput, false, false, defaultText);
 #if UNITY_METRO
					mKeyboard.active = true;
 #endif
				}
				else
#endif // MOBILE
				{
					Vector2 pos = (UICamera.current != null && UICamera.current.cachedCamera != null) ?
						UICamera.current.cachedCamera.WorldToScreenPoint(label.worldCorners[0]) :
						label.worldCorners[0];
					pos.y = Screen.height - pos.y;
					Input.imeCompositionMode = IMECompositionMode.On;
					Input.compositionCursorPos = pos;
				}

				UpdateLabel();
				return;
			}
#if MOBILE
			if (mKeyboard != null)
			{
 #if UNITY_METRO
				string text = Input.inputString;
				if (!string.IsNullOrEmpty(text)) Insert(text);
 #else
				string text = mKeyboard.text;
 
				if (inputShouldBeHidden)
				{
					if (text != "|")
					{
						if (!string.IsNullOrEmpty(text))
						{
							Insert(text.Substring(1));
						}
						else DoBackspace();

						mKeyboard.text = "|";
					}
				}
				else if (mCached != text)
				{
					mCached = text;
					value = text;
				}
 #endif // UNITY_METRO
				if (mKeyboard.done || !mKeyboard.active)
				{
					if (!mKeyboard.wasCanceled) Submit();
					mKeyboard = null;
					isSelected = false;
					mCached = "";
				}
			}
			else
#endif // MOBILE
			{
				if (selectOnTab != null && Input.GetKeyDown(KeyCode.Tab))
				{
					UICamera.selectedObject = selectOnTab;
					return;
				}

				string ime = Input.compositionString;

				// There seems to be an inconsistency between IME on Windows, and IME on OSX.
				// On Windows, Input.inputString is always empty while IME is active. On the OSX it is not.
				if (string.IsNullOrEmpty(ime) && !string.IsNullOrEmpty(Input.inputString))
				{
					// Process input ignoring non-printable characters as they are not consistent.
					// Windows has them, OSX may not. They get handled inside OnGUI() instead.
					string s = Input.inputString;

					for (int i = 0; i < s.Length; ++i)
					{
						char ch = s[i];
						if (ch < ' ') continue;

						// OSX inserts these characters for arrow keys
						if (ch == '\uF700') continue;
						if (ch == '\uF701') continue;
						if (ch == '\uF702') continue;
						if (ch == '\uF703') continue;

						Insert(ch.ToString());
					}
				}

				// Append IME composition
				if (mLastIME != ime)
				{
					mSelectionEnd = string.IsNullOrEmpty(ime) ? mSelectionStart : mValue.Length + ime.Length;
					mLastIME = ime;
					UpdateLabel();
					ExecuteOnChange();
				}
			}

			// Blink the caret
			if (mCaret != null && mNextBlink < RealTime.time)
			{
				mNextBlink = RealTime.time + 0.5f;
				mCaret.enabled = !mCaret.enabled;
			}

			// If the label's final alpha changes, we need to update the drawn geometry,
			// or the highlight widgets (which have their geometry set manually) won't update.
			if (isSelected && mLastAlpha != label.finalAlpha)
				UpdateLabel();
		}
	}

	/// <summary>
	/// Unfortunately Unity 4.3 and earlier doesn't offer a way to properly process events outside of OnGUI.
	/// </summary>

	void OnGUI ()
	{
		if (isSelected && Event.current.rawType == EventType.KeyDown)
			ProcessEvent(Event.current);
	}

	/// <summary>
	/// Perform a backspace operation.
	/// </summary>

	protected void DoBackspace ()
	{
		if (!string.IsNullOrEmpty(mValue))
		{
			if (mSelectionStart == mSelectionEnd)
			{
				if (mSelectionStart < 1) return;
				--mSelectionEnd;
			}
			Insert("");
		}
	}

	/// <summary>
	/// Handle the specified event.
	/// </summary>

	protected virtual bool ProcessEvent (Event ev)
	{
		if (label == null) return false;

		RuntimePlatform rp = Application.platform;

		bool isMac = (
			rp == RuntimePlatform.OSXEditor ||
			rp == RuntimePlatform.OSXPlayer ||
			rp == RuntimePlatform.WebGLPlayer);

		bool ctrl = isMac ?
			((ev.modifiers & EventModifiers.Command) != 0) :
			((ev.modifiers & EventModifiers.Control) != 0);

		bool shift = ((ev.modifiers & EventModifiers.Shift) != 0);

		switch (ev.keyCode)
		{
			case KeyCode.Backspace:
			{
				ev.Use();
				DoBackspace();
				return true;
			}

			case KeyCode.Delete:
			{
				ev.Use();

				if (!string.IsNullOrEmpty(mValue))
				{
					if (mSelectionStart == mSelectionEnd)
					{
						if (mSelectionStart >= mValue.Length) return true;
						++mSelectionEnd;
					}
					Insert("");
				}
				return true;
			}

			case KeyCode.LeftArrow:
			{
				ev.Use();

				if (!string.IsNullOrEmpty(mValue))
				{
					mSelectionEnd = Mathf.Max(mSelectionEnd - 1, 0);
					if (!shift) mSelectionStart = mSelectionEnd;
					UpdateLabel();
				}
				return true;
			}

			case KeyCode.RightArrow:
			{
				ev.Use();

				if (!string.IsNullOrEmpty(mValue))
				{
					mSelectionEnd = Mathf.Min(mSelectionEnd + 1, mValue.Length);
					if (!shift) mSelectionStart = mSelectionEnd;
					UpdateLabel();
				}
				return true;
			}

			case KeyCode.PageUp:
			{
				ev.Use();

				if (!string.IsNullOrEmpty(mValue))
				{
					mSelectionEnd = 0;
					if (!shift) mSelectionStart = mSelectionEnd;
					UpdateLabel();
				}
				return true;
			}

			case KeyCode.PageDown:
			{
				ev.Use();

				if (!string.IsNullOrEmpty(mValue))
				{
					mSelectionEnd = mValue.Length;
					if (!shift) mSelectionStart = mSelectionEnd;
					UpdateLabel();
				}
				return true;
			}

			case KeyCode.Home:
			{
				ev.Use();

				if (!string.IsNullOrEmpty(mValue))
				{
					if (label.multiLine)
					{
						mSelectionEnd = label.GetCharacterIndex(mSelectionEnd, KeyCode.Home);
					}
					else mSelectionEnd = 0;

					if (!shift) mSelectionStart = mSelectionEnd;
					UpdateLabel();
				}
				return true;
			}

			case KeyCode.End:
			{
				ev.Use();

				if (!string.IsNullOrEmpty(mValue))
				{
					if (label.multiLine)
					{
						mSelectionEnd = label.GetCharacterIndex(mSelectionEnd, KeyCode.End);
					}
					else mSelectionEnd = mValue.Length;

					if (!shift) mSelectionStart = mSelectionEnd;
					UpdateLabel();
				}
				return true;
			}

			case KeyCode.UpArrow:
			{
				ev.Use();

				if (!string.IsNullOrEmpty(mValue))
				{
					mSelectionEnd = label.GetCharacterIndex(mSelectionEnd, KeyCode.UpArrow);
					if (mSelectionEnd != 0) mSelectionEnd += mDrawStart;
					if (!shift) mSelectionStart = mSelectionEnd;
					UpdateLabel();
				}
				return true;
			}

			case KeyCode.DownArrow:
			{
				ev.Use();

				if (!string.IsNullOrEmpty(mValue))
				{
					mSelectionEnd = label.GetCharacterIndex(mSelectionEnd, KeyCode.DownArrow);
					if (mSelectionEnd != label.processedText.Length) mSelectionEnd += mDrawStart;
					else mSelectionEnd = mValue.Length;
					if (!shift) mSelectionStart = mSelectionEnd;
					UpdateLabel();
				}
				return true;
			}

			// Select all
			case KeyCode.A:
			{
				if (ctrl)
				{
					ev.Use();
					mSelectionStart = 0;
					mSelectionEnd = mValue.Length;
					UpdateLabel();
				}
				return true;
			}

			// Copy
			case KeyCode.C:
			{
				if (ctrl)
				{
					ev.Use();
					NGUITools.clipboard = GetSelection();
				}
				return true;
			}

			// Paste
			case KeyCode.V:
			{
				if (ctrl)
				{
					ev.Use();
					Insert(NGUITools.clipboard);
				}
				return true;
			}

			// Cut
			case KeyCode.X:
			{
				if (ctrl)
				{
					ev.Use();
					NGUITools.clipboard = GetSelection();
					Insert("");
				}
				return true;
			}

			// Submit
			case KeyCode.Return:
			case KeyCode.KeypadEnter:
			{
				ev.Use();

				bool newLine = (onReturnKey == OnReturnKey.NewLine) ||
					(onReturnKey == OnReturnKey.Default &&
					label.multiLine && !ctrl &&
					label.overflowMethod != UILabel.Overflow.ClampContent &&
					validation == Validation.None);

				if (newLine)
				{
					Insert("\n");
				}
				else
				{
					UICamera.currentScheme = UICamera.ControlScheme.Controller;
					UICamera.currentKey = ev.keyCode;
					Submit();
					UICamera.currentKey = KeyCode.None;
				}
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Insert the specified text string into the current input value, respecting selection and validation.
	/// </summary>

	protected virtual void Insert (string text)
	{
		string left = GetLeftText();
		string right = GetRightText();
		int rl = right.Length;

		StringBuilder sb = new StringBuilder(left.Length + right.Length + text.Length);
		sb.Append(left);

		// Append the new text
		for (int i = 0, imax = text.Length; i < imax; ++i)
		{
			// If we have an input validator, validate the input first
			char c = text[i];

			if (c == '\b')
			{
				DoBackspace();
				continue;
			}

			// Can't go past the character limit
			if (characterLimit > 0 && sb.Length + rl >= characterLimit) break;

			if (onValidate != null) c = onValidate(sb.ToString(), sb.Length, c);
			else if (validation != Validation.None) c = Validate(sb.ToString(), sb.Length, c);

			// Append the character if it hasn't been invalidated
			if (c != 0) sb.Append(c);
		}

		// Advance the selection
		mSelectionStart = sb.Length;
		mSelectionEnd = mSelectionStart;

		// Append the text that follows it, ensuring that it's also validated after the inserted value
		for (int i = 0, imax = right.Length; i < imax; ++i)
		{
			char c = right[i];
			if (onValidate != null) c = onValidate(sb.ToString(), sb.Length, c);
			else if (validation != Validation.None) c = Validate(sb.ToString(), sb.Length, c);
			if (c != 0) sb.Append(c);
		}

		mValue = sb.ToString();
		UpdateLabel();
		ExecuteOnChange();
	}

	/// <summary>
	/// Get the text to the left of the selection.
	/// </summary>

	protected string GetLeftText ()
	{
		int min = Mathf.Min(mSelectionStart, mSelectionEnd);
		return (string.IsNullOrEmpty(mValue) || min < 0) ? "" : mValue.Substring(0, min);
	}

	/// <summary>
	/// Get the text to the right of the selection.
	/// </summary>

	protected string GetRightText ()
	{
		int max = Mathf.Max(mSelectionStart, mSelectionEnd);
		return (string.IsNullOrEmpty(mValue) || max >= mValue.Length) ? "" : mValue.Substring(max);
	}

	/// <summary>
	/// Get currently selected text.
	/// </summary>

	protected string GetSelection ()
	{
		if (string.IsNullOrEmpty(mValue) || mSelectionStart == mSelectionEnd)
		{
			return "";
		}
		else
		{
			int min = Mathf.Min(mSelectionStart, mSelectionEnd);
			int max = Mathf.Max(mSelectionStart, mSelectionEnd);
			return mValue.Substring(min, max - min);
		}
	}

	/// <summary>
	/// Helper function that retrieves the index of the character under the mouse.
	/// </summary>

	protected int GetCharUnderMouse ()
	{
		Vector3[] corners = label.worldCorners;
		Ray ray = UICamera.currentRay;
		Plane p = new Plane(corners[0], corners[1], corners[2]);
		float dist;
		return p.Raycast(ray, out dist) ? mDrawStart + label.GetCharacterIndexAtPosition(ray.GetPoint(dist)) : 0;
	}

	/// <summary>
	/// Move the caret on press.
	/// </summary>

	protected virtual void OnPress (bool isPressed)
	{
		if (isPressed && isSelected && label != null &&
			(UICamera.currentScheme == UICamera.ControlScheme.Mouse ||
			 UICamera.currentScheme == UICamera.ControlScheme.Touch))
		{
			selectionEnd = GetCharUnderMouse();
			if (!Input.GetKey(KeyCode.LeftShift) &&
				!Input.GetKey(KeyCode.RightShift)) selectionStart = mSelectionEnd;
		}
	}

	/// <summary>
	/// Drag selection.
	/// </summary>

	protected virtual void OnDrag (Vector2 delta)
	{
		if (label != null &&
			(UICamera.currentScheme == UICamera.ControlScheme.Mouse ||
			 UICamera.currentScheme == UICamera.ControlScheme.Touch))
		{
			selectionEnd = GetCharUnderMouse();
		}
	}

	/// <summary>
	/// Ensure we've released the dynamically created resources.
	/// </summary>

	void OnDisable () { Cleanup(); }

	/// <summary>
	/// Cleanup.
	/// </summary>

	protected virtual void Cleanup ()
	{
		if (mHighlight) mHighlight.enabled = false;
		if (mCaret) mCaret.enabled = false;

		if (mBlankTex)
		{
			NGUITools.Destroy(mBlankTex);
			mBlankTex = null;
		}
	}

	/// <summary>
	/// Submit the input field's text.
	/// </summary>

	public void Submit ()
	{
     
		if (NGUITools.GetActive(this))
		{
			mValue = value;

			if (current == null)
			{
                
				current = this;
				EventDelegate.Execute(onSubmit);
				current = null;
			}
			SaveToPlayerPrefs(mValue);
		}
	}

	/// <summary>
	/// Update the visual text label.
	/// </summary>

	public void UpdateLabel ()
	{
		if (label != null)
		{
            
			if (mDoInit) Init();
			bool selected = isSelected;
			string fullText = value;
			bool isEmpty = string.IsNullOrEmpty(fullText) && string.IsNullOrEmpty(Input.compositionString);

            //label.color = (isEmpty && !selected) ? mDefaultColor : activeTextColor;
            label.color = (isEmpty) ? mDefaultColor : activeTextColor;
            string processed;

			if (isEmpty)
			{
				processed = /*selected ? "" : */mDefaultText;
				RestoreLabelPivot();
                if (openFloatLabel)
                {
                    if (mFloatingLabel.enabled)
                    {
                        HideFloatingLabel();
                    }
                }
            }
			else
			{
                if (openFloatLabel)
                {
                    if (!mFloatingLabel.enabled)
                    {
                        ShowFloatingLabel();
                    }
                }
                if (inputType == InputType.Password)
				{
					processed = "";

					string asterisk = "*";

					if (label.bitmapFont != null && label.bitmapFont.bmFont != null &&
						label.bitmapFont.bmFont.GetGlyph('*') == null) asterisk = "x";

					for (int i = 0, imax = fullText.Length; i < imax; ++i) processed += asterisk;
				}
				else processed = fullText;

				// Start with text leading up to the selection
				int selPos = selected ? Mathf.Min(processed.Length, cursorPosition) : 0;
				string left = processed.Substring(0, selPos);

				// Append the composition string and the cursor character
				if (selected) left += Input.compositionString;

				// Append the text from the selection onwards
				processed = left + processed.Substring(selPos, processed.Length - selPos);

				// Clamped content needs to be adjusted further
				if (selected && label.overflowMethod == UILabel.Overflow.ClampContent && label.maxLineCount == 1)
				{
					// Determine what will actually fit into the given line
					int offset = label.CalculateOffsetToFit(processed);

					if (offset == 0)
					{
						mDrawStart = 0;
						RestoreLabelPivot();
					}
					else if (selPos < mDrawStart)
					{
						mDrawStart = selPos;
						SetPivotToLeft();
					}
					else if (offset < mDrawStart)
					{
						mDrawStart = offset;
						SetPivotToLeft();
					}
					else
					{
						offset = label.CalculateOffsetToFit(processed.Substring(0, selPos));

						if (offset > mDrawStart)
						{
							mDrawStart = offset;
							SetPivotToRight();
						}
					}

					// If necessary, trim the front
					if (mDrawStart != 0)
						processed = processed.Substring(mDrawStart, processed.Length - mDrawStart);
				}
				else
				{
					mDrawStart = 0;
					RestoreLabelPivot();
				}
			}
            if (HaveArab(processed))
            {
                label.text = ConvertArab(processed);
            }
            else
                label.text = processed;
#if MOBILE
			if (selected && (mKeyboard == null || inputShouldBeHidden))
#else
			if (selected)
#endif
			{
				int start = mSelectionStart - mDrawStart;
				int end = mSelectionEnd - mDrawStart;

				// Blank texture used by selection and caret
				if (mBlankTex == null)
				{
					mBlankTex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
					for (int y = 0; y < 2; ++y)
						for (int x = 0; x < 2; ++x)
							mBlankTex.SetPixel(x, y, Color.white);
					mBlankTex.Apply();
				}

				// Create the selection highlight
				if (start != end)
				{
					if (mHighlight == null)
					{
						mHighlight = NGUITools.AddWidget<UITexture>(label.cachedGameObject);
						mHighlight.name = "Input Highlight";
						mHighlight.mainTexture = mBlankTex;
						mHighlight.fillGeometry = false;
						mHighlight.pivot = label.pivot;
						mHighlight.SetAnchor(label.cachedTransform);
					}
					else
					{
						mHighlight.pivot = label.pivot;
						mHighlight.mainTexture = mBlankTex;
						mHighlight.MarkAsChanged();
						mHighlight.enabled = true;
					}
				}

				// Create the carter
				if (mCaret == null)
				{
					mCaret = NGUITools.AddWidget<UITexture>(label.cachedGameObject);
					mCaret.name = "Input Caret";
					mCaret.mainTexture = mBlankTex;
					mCaret.fillGeometry = false;
					mCaret.pivot = label.pivot;
					mCaret.SetAnchor(label.cachedTransform);
				}
				else
				{
					mCaret.pivot = label.pivot;
					mCaret.mainTexture = mBlankTex;
					mCaret.MarkAsChanged();
					mCaret.enabled = true;
				}

				if (start != end)
				{
					label.PrintOverlay(start, end, mCaret.geometry, mHighlight.geometry, caretColor, selectionColor);
					mHighlight.enabled = mHighlight.geometry.hasVertices;
				}
				else
				{
					label.PrintOverlay(start, end, mCaret.geometry, null, caretColor, selectionColor);
					if (mHighlight != null) mHighlight.enabled = false;
				}

				// Reset the blinking time
				mNextBlink = RealTime.time + 0.5f;
				mLastAlpha = label.finalAlpha;
			}
			else Cleanup();
		}
	}

	/// <summary>
	/// Set the label's pivot to the left.
	/// </summary>

	protected void SetPivotToLeft ()
	{
		Vector2 po = NGUIMath.GetPivotOffset(mPivot);
		po.x = 0f;
		label.pivot = NGUIMath.GetPivot(po);
	}

	/// <summary>
	/// Set the label's pivot to the right.
	/// </summary>

	protected void SetPivotToRight ()
	{
		Vector2 po = NGUIMath.GetPivotOffset(mPivot);
		po.x = 1f;
		label.pivot = NGUIMath.GetPivot(po);
	}

	/// <summary>
	/// Restore the input label's pivot point.
	/// </summary>

	protected void RestoreLabelPivot ()
	{
		if (label != null && label.pivot != mPivot)
			label.pivot = mPivot;
	}

	/// <summary>
	/// Validate the specified input.
	/// </summary>

	protected char Validate (string text, int pos, char ch)
	{
		// Validation is disabled
		if (validation == Validation.None || !enabled) return ch;

		if (validation == Validation.Integer)
		{
			// Integer number validation
			if (ch >= '0' && ch <= '9') return ch;
			if (ch == '-' && pos == 0 && !text.Contains("-")) return ch;
		}
		else if (validation == Validation.Float)
		{
			// Floating-point number
			if (ch >= '0' && ch <= '9') return ch;
			if (ch == '-' && pos == 0 && !text.Contains("-")) return ch;
			if (ch == '.' && !text.Contains(".")) return ch;
		}
		else if (validation == Validation.Alphanumeric)
		{
			// All alphanumeric characters
			if (ch >= 'A' && ch <= 'Z') return ch;
			if (ch >= 'a' && ch <= 'z') return ch;
			if (ch >= '0' && ch <= '9') return ch;
		}
		else if (validation == Validation.Username)
		{
			// Lowercase and numbers
			if (ch >= 'A' && ch <= 'Z') return (char)(ch - 'A' + 'a');
			if (ch >= 'a' && ch <= 'z') return ch;
			if (ch >= '0' && ch <= '9') return ch;
		}
		else if (validation == Validation.Name)
		{
			char lastChar = (text.Length > 0) ? text[Mathf.Clamp(pos, 0, text.Length - 1)] : ' ';
			char nextChar = (text.Length > 0) ? text[Mathf.Clamp(pos + 1, 0, text.Length - 1)] : '\n';

			if (ch >= 'a' && ch <= 'z')
			{
				// Space followed by a letter -- make sure it's capitalized
				if (lastChar == ' ') return (char)(ch - 'a' + 'A');
				return ch;
			}
			else if (ch >= 'A' && ch <= 'Z')
			{
				// Uppercase letters are only allowed after spaces (and apostrophes)
				if (lastChar != ' ' && lastChar != '\'') return (char)(ch - 'A' + 'a');
				return ch;
			}
			else if (ch == '\'')
			{
				// Don't allow more than one apostrophe
				if (lastChar != ' ' && lastChar != '\'' && nextChar != '\'' && !text.Contains("'")) return ch;
			}
			else if (ch == ' ')
			{
				// Don't allow more than one space in a row
				if (lastChar != ' ' && lastChar != '\'' && nextChar != ' ' && nextChar != '\'') return ch;
			}
		}
		return (char)0;
	}

	/// <summary>
	/// Execute the OnChange callback.
	/// </summary>

	protected void ExecuteOnChange ()
	{
		if (current == null && EventDelegate.IsValid(onChange))
		{
			current = this;
			EventDelegate.Execute(onChange);
			current = null;
		}
	}

	/// <summary>
	/// Convenience function to be used as a callback that will clear the input field's focus.
	/// </summary>

	public void RemoveFocus () { isSelected = false; }

	/// <summary>
	/// Convenience function that can be used as a callback for On Change notification.
	/// </summary>

	public void SaveValue () { SaveToPlayerPrefs(mValue); }

	/// <summary>
	/// Convenience function that can forcefully reset the input field's value to what was saved earlier.
	/// </summary>

	public void LoadValue ()
	{
		if (!string.IsNullOrEmpty(savedAs))
		{
			string val = mValue.Replace("\\n", "\n");
			mValue = "";
			value = PlayerPrefs.HasKey(savedAs) ? PlayerPrefs.GetString(savedAs) : val;
		}
	}
}
