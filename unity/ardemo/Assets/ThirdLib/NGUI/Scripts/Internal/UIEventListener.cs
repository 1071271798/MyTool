//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Event Hook class lets you easily add remote event listener functions to an object.
/// Example usage: UIEventListener.Get(gameObject).onClick += MyClickFunction;
/// </summary>

[AddComponentMenu("NGUI/Internal/Event Listener")]
public class UIEventListener : MonoBehaviour
{
    public delegate void VoidDelegate(GameObject go);
    public delegate void BoolDelegate(GameObject go, bool state);
    public delegate void FloatDelegate(GameObject go, float delta);
    public delegate void VectorDelegate(GameObject go, Vector2 delta);
    public delegate void ObjectDelegate(GameObject go, GameObject draggedObject);
    public delegate void KeyCodeDelegate(GameObject go, KeyCode key);

    public bool IsMultiTouch = false;  //多点触控

    public object parameter;

    public VoidDelegate onSubmit;
    public VoidDelegate onClick;
    public VoidDelegate onDoubleClick;
    public BoolDelegate onHover;
    public BoolDelegate onPress;
    public BoolDelegate onSelect;
    public FloatDelegate onScroll;
    public VectorDelegate onDrag;
    public VoidDelegate onDragOver;
    public VoidDelegate onDragOut;
    public ObjectDelegate onDrop;
    public KeyCodeDelegate onKey;

    public FloatDelegate OnMultiTouch;

    void OnSubmit() { if (onSubmit != null) onSubmit(gameObject); }
    void OnClick() { if (onClick != null) onClick(gameObject); }
    void OnDoubleClick() { if (onDoubleClick != null) onDoubleClick(gameObject); }
    void OnHover(bool isOver) { if (onHover != null) onHover(gameObject, isOver); }
    void OnPress(bool isPressed) { if (onPress != null) onPress(gameObject, isPressed);}
    void OnSelect(bool selected) { if (onSelect != null) onSelect(gameObject, selected); }
    void OnScroll(float delta) { if (onScroll != null) onScroll(gameObject, delta); }
    void OnDrag(Vector2 delta) { if (onDrag != null) onDrag(gameObject, delta); }
    void OnDragOver() { if (onDragOver != null) onDragOver(gameObject); }
    void OnDragOut() { if (onDragOut != null) onDragOut(gameObject); }
    void OnDrop(GameObject go) { if (onDrop != null) onDrop(gameObject, go); }
    void OnKey(KeyCode key) { if (onKey != null) onKey(gameObject, key); }

    /// <summary>
    /// Get or add an event listener to the specified game object.
    /// </summary>

    static public UIEventListener Get(GameObject go)
    {
        UIEventListener listener = go.GetComponent<UIEventListener>();
        if (listener == null) listener = go.AddComponent<UIEventListener>();
        return listener;
    }

    float Distance;
    Vector3 mPos;
    float ZoomSpeed = 2f;
    float MinDistance = 2f;
    float MaxDistance = 5f;
    void Update()
    {
        if (IsMultiTouch && Input.touches.Length > 1)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved)  //两指滑动
            {
                //计算移动的向量
                Vector2 mDir = Input.touches[1].position - Input.touches[0].position;
                //根据向量的大小判断当前手势是放大还是缩小  
                if (mDir.sqrMagnitude > mPos.sqrMagnitude)
                {
                    Distance -= ZoomSpeed;
                }
                else
                {
                    Distance += ZoomSpeed;
                }
                //限制距离  
                Distance = Mathf.Clamp(Distance, MinDistance, MaxDistance);

                OnMultiTouch(gameObject, Distance);    //双指滑动事件
                //更新当前手势  
                mPos = mDir;
            }
        }
    }
}

