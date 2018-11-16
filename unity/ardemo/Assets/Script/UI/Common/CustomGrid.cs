using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// @author : YangDan
/// @date : 2014-6-20
/// 
/// CustomGrid,这个类主要做了一件事,就是优化了,NGUI UIGrid 在数据量很多都时候,创建过多都GameObject对象,造成资源浪费.
/// 该类需要和 CustomScrollView.cs 以及 CustomDragScrollView.cs一起使用;
/// CustomScrollView 类只上把bounds字段给暴露出来，因为bounds都大小需要在外面设置了.
/// CustomDragScrollView 类 没有修改,因为默认都UIDragScrollView 默认里面调用都上UIScrollView 不能与我们都CustomScrollView兼容.
/// 所以只是将里面都UIScrollView 改为 CustomScrollView.
/// Item 是一个渲染操作类.可以自己定义,或者不要,并没有影响.
/// </summary>
/// 
public class CustomGrid : UIWidgetContainer
{
    public delegate void DelegateInitItem(ItemObjectEx item, ItemDataEx data, params object[] args);

    public delegate ItemObjectEx DelegateCreateItemObject(GameObject obj);
    //响应
    public DelegateInitItem OnItemInit;

    public DelegateCreateItemObject OnCreateItem;

    public GameObject Item;

    //item的起始位置
    Vector3 itemStartPos = Vector3.zero;

    //菜单项间隙
    public float gapDis = 0f;

    private float m_AllCellHeight;
    private float m_AllCellWidth;

    public int m_maxLine = 10;

    public int showNum;

    public int checkOffset = 0;

    public ItemObjectEx[] m_cellList;

    private UIScrollViewEx mDrag;

    private float lastPos = float.MinValue;
    private Vector3 startPos = Vector3.zero;
   
    private int lastStartIndex = -1;
    private int lastEndIndex = -1;
    private int lastItemStartIndex = -1;

    private List<ItemDataEx> m_listData;
    private Dictionary<string, ItemDataEx> mDataDict;

    Transform mTrans;

    public enum ArrangeDirection
    {
        Left_to_Right,
        Right_to_Left,
        Up_to_Down,
        Down_to_Up,
    }

    public ArrangeDirection ArrangeDir = ArrangeDirection.Up_to_Down;               //items的排列方式
    public List<ItemDataEx> DataList
    {
        get { return m_listData; }
        set { m_listData = value; }
    }
    private Vector3 defaultVec = Vector3.zero;


    //item初始化
    public void Init(List<ItemDataEx> datas, DelegateInitItem onItemInitCallback, DelegateCreateItemObject onCreateItemBack)
    {
        m_listData = datas;
        if (null != datas)
        {
            mDataDict = new Dictionary<string, ItemDataEx>();
            for (int i = 0, imax = datas.Count; i < imax; ++i)
            {
                mDataDict[datas[i].name] = datas[i];
            }
        }
        this.OnItemInit = onItemInitCallback;
        this.OnCreateItem = onCreateItemBack;
        CalculatePosition();
        /*if (datas.Count > 0)
        {
            m_maxLine = m_maxLine < datas.Count ? m_maxLine : datas.Count;
        }*/
        CreateItem();
        Validate();
        UpdateBounds();
    }
    public Vector3 GetItemPosition(ItemDataEx item)
    {
        Vector2 scale = new Vector2(mTrans.lossyScale.x / mDrag.transform.lossyScale.x, mTrans.lossyScale.y / mDrag.transform.lossyScale.y);
        Vector3 pos = new Vector3(item.localPosition.x * scale.x * mDrag.transform.lossyScale.x, item.localPosition.y * scale.y * mDrag.transform.lossyScale.y, item.localPosition.z * mDrag.transform.lossyScale.z);
        return pos + mDrag.transform.position;
    }

    public bool ItemActiveInHierarchy(ItemDataEx item)
    {
        if (null == m_cellList || m_cellList.Length <= 0)
        {
            return false;
        }

        for (int i = 0, icount = m_cellList.Length; i < icount; ++i)
        {
            if (m_cellList[i].data == null)
            {
                return false;
            }
            if (m_cellList[i].itemObj.activeInHierarchy && m_cellList[i].data == item)
            {
                return true;
            }
        }
        return false;
    }
    public float GetAllWidth()
    {
        return m_AllCellWidth;
    }
    public float GetShowWidth()
    {
        if (m_AllCellWidth < mDrag.panel.finalClipRegion.z)
        {
            return m_AllCellWidth;
        }
        return mDrag.panel.finalClipRegion.z;
    }
    public float GetAllHeight()
    {
        return m_AllCellHeight;
    }
    public float GetShowHeight()
    {
        if (m_AllCellHeight < mDrag.panel.finalClipRegion.w)
        {
            return m_AllCellHeight;
        }
        return mDrag.panel.finalClipRegion.w;
    }
    public bool IsEnd(ItemDataEx data)
    {
        if (null == m_listData || m_listData.Count <= 0)
        {
            return false;
        }
        if (m_listData[m_listData.Count - 1] == data)
        {
            return true;
        }
        return false;
    }

    public bool IsFront(ItemDataEx data)
    {
        if (null == m_listData || m_listData.Count <= 0)
        {
            return false;
        }
        if (m_listData[0] == data)
        {
            return true;
        }
        return false;
    }

    public Vector3 StartPos
    {
        get { return startPos; }
        set { startPos = value; }
    }
    /// <summary>
    /// 移动到最后
    /// </summary>
    /// <param name="instant">true表示立即生效</param>
    public void MoveToEnd(bool instant)
    {
        if (null != mDrag)
        {
            Vector3 offset = GetToEndPos();
            mDrag.MoveTo(offset, instant);
        }
        
    }

    public Vector3 GetToEndPos()
    {
        Vector3 offset = startPos;
        Vector2 scale = new Vector2(mTrans.lossyScale.x / mDrag.transform.lossyScale.x, mTrans.lossyScale.y / mDrag.transform.lossyScale.y);
        if (ArrangeDir == ArrangeDirection.Down_to_Up)
        {
            offset.y -= Mathf.Max(m_AllCellHeight * scale.y - mDrag.panel.finalClipRegion.w + mDrag.panel.clipSoftness.y, 0);
        }
        else if (ArrangeDir == ArrangeDirection.Up_to_Down)
        {
            offset.y += Mathf.Max(m_AllCellHeight * scale.y - mDrag.panel.finalClipRegion.w - mDrag.panel.clipSoftness.y, 0);
        }
        else if (ArrangeDir == ArrangeDirection.Left_to_Right)
        {
            offset.x -= Mathf.Max(m_AllCellWidth * scale.x - mDrag.panel.finalClipRegion.z + mDrag.panel.clipSoftness.x, 0);
        }
        else if (ArrangeDir == ArrangeDirection.Right_to_Left)
        {
            offset.x += Mathf.Max(m_AllCellWidth * scale.x - mDrag.panel.finalClipRegion.z - mDrag.panel.clipSoftness.x, 0);
        }
        return offset;
    }
    /// <summary>
    /// 移动到开始位置
    /// </summary>
    /// <param name="instant"></param>
    public void MoveToFront(bool instant)
    {
        if (null != mDrag)
        {
            mDrag.MoveTo(startPos, instant);
        }
    }
    /// <summary>
    /// 把某一帧移到前面
    /// </summary>
    /// <param name="index"></param>
    /// <param name="instant"></param>
    public void FrameMoveToFront(int index, bool instant)
    {
        if (null == mDrag)
        {
            return;
        }
        float offset = 0;
        Vector2 scale = new Vector2(mTrans.lossyScale.x / mDrag.transform.lossyScale.x, mTrans.lossyScale.y / mDrag.transform.lossyScale.y);
        Vector3 to = startPos;
        if (UIScrollViewEx.Movement.Vertical == mDrag.movement)
        {
            for (int i = 0; i < index; ++i)
            {
                offset += m_listData[i].height;
            }
            offset *= scale.y;
        }
        else
        {
            for (int i = 0; i < index; ++i)
            {
                offset += m_listData[i].width;
            }
            offset *= scale.x;
        }
        if (ArrangeDir == ArrangeDirection.Down_to_Up)
        {
            to.y -= offset;
        }
        else if (ArrangeDir == ArrangeDirection.Up_to_Down)
        {
            to.y += offset;
        }
        else if (ArrangeDir == ArrangeDirection.Left_to_Right)
        {
            to.x -= offset;
        }
        else if (ArrangeDir == ArrangeDirection.Right_to_Left)
        {
            to.x += offset;
        }
        MoveWithinBounds(to - mDrag.transform.localPosition, instant);
    }
    /// <summary>
    /// 把某一帧移到面板内
    /// </summary>
    /// <param name="index"></param>
    /// <param name="instant"></param>
    public void FrameMoveToWithinBounds(int index, bool instant)
    {
        if (null == mDrag || null == m_listData || index < 0 || index >= m_listData.Count)
        {
            return;
        }
        ItemDataEx data = m_listData[index];
        Vector3 dragPos = mDrag.transform.localPosition;
        Vector4 clipRect = mDrag.panel.finalClipRegion;
        Vector3 dataPos = startPos + mTrans.localPosition + data.localPosition;
        Vector2 minRect = new Vector2(dataPos.x - data.width / 2, dataPos.y - data.height / 2);
        Vector2 maxRect = new Vector2(dataPos.x + data.width / 2, dataPos.y + data.height / 2);
        Vector2 minArea = new Vector2(clipRect.x - clipRect.z / 2, clipRect.y - clipRect.w / 2);
        Vector2 maxArea = new Vector2(clipRect.x + clipRect.z / 2, clipRect.y + clipRect.w / 2);
        Vector2 constrain = NGUIMath.ConstrainRect(minRect, maxRect, minArea, maxArea);
        if (UIScrollViewEx.Movement.Vertical == mDrag.movement)
        {
            constrain.x = 0;
        }
        else
        {
            constrain.y = 0;
        }
        MoveWithinBounds(constrain, instant);
    }
    /// <summary>
    /// 移动面板，不会超出panel区域
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="instant"></param>
    public void MoveWithinBounds(Vector3 offset, bool instant, int moveMs = 0)
    {
        if (null != mDrag)
        {
            Vector3 pos = mDrag.transform.localPosition;
            Vector3 endPos = GetToEndPos();
            Vector3 to = pos + offset;
            if (ArrangeDir == ArrangeDirection.Down_to_Up)
            {
                to.x = pos.x;
                to.z = pos.z;
                if (to.y < endPos.y)
                {
                    to.y = endPos.y;
                }
                else if (to.y > startPos.y) 
                {
                    to.y = startPos.y;
                }
            }
            else if (ArrangeDir == ArrangeDirection.Up_to_Down)
            {
                to.x = pos.x;
                to.z = pos.z;
                if (to.y > endPos.y)
                {
                    to.y = endPos.y;
                }
                else if (to.y < startPos.y)
                {
                    to.y = startPos.y;
                }
            }
            else if (ArrangeDir == ArrangeDirection.Left_to_Right)
            {
                to.y = pos.y;
                to.z = pos.z;
                if (to.x < endPos.x)
                {
                    to.x = endPos.x;
                }
                else if (to.x > startPos.x)
                {
                    to.x = startPos.x;
                }
            }
            else if (ArrangeDir == ArrangeDirection.Right_to_Left)
            {
                to.y = pos.y;
                to.z = pos.z;
                if (to.x > endPos.x)
                {
                    to.x = endPos.x;
                }
                else if (to.x < startPos.x)
                {
                    to.x = startPos.x;
                }
            }
            mDrag.MoveTo(to, instant, moveMs);
        }
    }


    public void Move(Vector3 offset, bool instant, int moveMs = 0)
    {
        if (null != mDrag)
        {
            Vector3 pos = mDrag.transform.localPosition;
            pos += offset;
            mDrag.MoveTo(pos, instant, moveMs);
        }
    }

    public Bounds CalculatedBounds()
    {
        Vector3 size = new Vector2(m_AllCellWidth, m_AllCellHeight);
        if (size.x == 0)
        {
            size.x = 100;
        }
        else if (size.y == 0)
        {
            size.y = 100;
        }
        Vector3 center = new Vector2(mTrans.localPosition.x + m_AllCellWidth / 2, mTrans.localPosition.y + m_AllCellHeight / 2);
        Bounds bounds = new Bounds(center, size);
        return bounds;
    }

    /// <summary>
    /// 计算所有item的位置
    /// </summary>
    public void CalculatePosition()
    {
        if (null == m_listData || m_listData.Count <= 0 || null == mDrag)
        {
            return;
        }
        m_listData[0].localPosition = itemStartPos;
        ItemDataEx lastDataEx = m_listData[0];
        int flag = 1;
        if (mDrag.movement == UIScrollViewEx.Movement.Vertical)
        {
            if (ArrangeDir == ArrangeDirection.Down_to_Up) flag = -1;
            m_AllCellHeight = m_listData[0].height;
            float offsetY = m_listData[0].height * 0.5f * flag;
            m_listData[0].localPosition = itemStartPos - new Vector3(0f, offsetY, 0f);
            for (int i = 1, icount = m_listData.Count; i < icount; ++i)
            {
                m_AllCellHeight += gapDis + m_listData[i].height;
                offsetY = (lastDataEx.height * 0.5f + gapDis + m_listData[i].height * 0.5f) * flag;
                m_listData[i].localPosition = lastDataEx.localPosition - new Vector3(0f, offsetY, 0f);
                lastDataEx = m_listData[i];
            }
        }
        else
        {
            if (ArrangeDir == ArrangeDirection.Right_to_Left) flag = -1;
            m_AllCellWidth = m_listData[0].width;
            float offsetX = m_listData[0].width * 0.5f;
            if (ArrangeDir == ArrangeDirection.Right_to_Left) offsetX *= -1f;
            m_listData[0].localPosition = itemStartPos + new Vector3(offsetX, 0f, 0f);
            for (int i = 1, icount = m_listData.Count; i < icount; ++i)
            {
                m_AllCellWidth += gapDis + m_listData[i].width;
                offsetX = (lastDataEx.width * 0.5f + gapDis + m_listData[i].width * 0.5f) * flag;
                m_listData[i].localPosition = lastDataEx.localPosition + new Vector3(offsetX, 0f, 0f);
                lastDataEx = m_listData[i];
            }
        }
        /*m_AllCellHeight = m_listData[0].height;
        m_AllCellWidth = m_listData[0].width;
        for (int i = 1, icount = m_listData.Count; i < icount; ++i)
        {
            if (mDrag.movement == UIScrollViewEx.Movement.Vertical)
            {
                m_AllCellHeight += gapDis + m_listData[i].height;
                float offsetY = lastDataEx.height * 0.5f + gapDis + m_listData[i].height * 0.5f;
                if (ArrangeDir == ArrangeDirection.Down_to_Up) offsetY *= -1f;
                m_listData[i].localPosition = lastDataEx.localPosition - new Vector3(0f, offsetY, 0f);
            }
            else
            {
                m_AllCellWidth += gapDis + m_listData[i].width;
                float offsetX = lastDataEx.width * 0.5f + gapDis + m_listData[i].width * 0.5f;
                if (ArrangeDir == ArrangeDirection.Right_to_Left) offsetX *= -1f;
                m_listData[i].localPosition = lastDataEx.localPosition + new Vector3(offsetX, 0f, 0f);
            }
            lastDataEx = m_listData[i];
        }*/
    }
    /// <summary>
    /// 从传入参数开始重新计算位置
    /// </summary>
    /// <param name="data"></param>
    public void CalculatePosition(ItemDataEx data, Vector2 oldSize)
    {
        if (null == m_listData || m_listData.Count <= 0 || null == mDrag)
        {
            return;
        }
        int index = m_listData.IndexOf(data);
        float offset = 0;
        if (-1 != index)
        {
            if (mDrag.movement == UIScrollViewEx.Movement.Vertical)
            {
                offset = data.height - oldSize.y;
                m_AllCellHeight += offset;
                data.localPosition += new Vector3(0, offset / 2, 0);
                for (int i = index + 1, imax = m_listData.Count; i < imax; ++i)
                {
                    m_listData[i].localPosition += new Vector3(0, offset, 0);
                }
            }
            else
            {
                offset = data.width - oldSize.x;
                m_AllCellWidth += offset;
                data.localPosition += new Vector3(offset / 2, 0, 0);
                for (int i = index + 1, imax = m_listData.Count; i < imax; ++i)
                {
                    m_listData[i].localPosition += new Vector3(offset, 0, 0);
                }
            }
        }
        else
        {
            Debug.LogError("CalculatePosition data error!");
        }
        
    }
    /// <summary>
    /// 重新排列位置
    /// </summary>
    /// <param name="data">从传入参数开始</param>
    public ItemObjectEx Reposition(ItemDataEx data, Vector2 oldSize, params object[] args)
    {
        if (null == m_listData || m_listData.Count <= 0 || null == mDrag)
        {
            return null;
        }
        ItemObjectEx objectEx = null;
        CalculatePosition(data, oldSize);
        int index = m_listData.IndexOf(data);
        if (-1 != index)
        {
            ItemObjectEx cell;
            int itemIndex = lastItemStartIndex + index - lastStartIndex;
            for (int i = index, imax = lastEndIndex; i <= imax; ++i)
            {
                if (itemIndex >= m_maxLine)
                {
                    itemIndex -= m_maxLine;
                }
                else if (itemIndex < 0)
                {
                    itemIndex += m_maxLine;
                }
                cell = m_cellList[itemIndex];
                if (i == index && null != OnItemInit)
                {
                    objectEx = cell;
                    OnItemInit(cell, m_listData[i], args);
                }
                cell.itemObj.transform.localPosition = m_listData[i].localPosition;
                ++itemIndex;
            }
        }
        /*if (index == m_listData.Count - 1)
        {
            MoveToEnd(true);
        }
        else*/
        {
            Vector3 offset = new Vector3(oldSize.x - data.width, oldSize.y - data.height) / 2;
            MoveWithinBounds(offset, true);
        }
        
        return objectEx;
    }
    /// <summary>
    /// 重新排列位置
    /// </summary>
    public void Reposition()
    {
        if (null == m_listData || m_listData.Count <= 0 || null == mDrag)
        {
            return;
        }
        CalculatePosition();
        ItemObjectEx cell;
        int itemIndex = lastItemStartIndex;
        for (int i = lastStartIndex, imax = lastEndIndex; i <= imax; ++i)
        {
            if (itemIndex >= m_maxLine)
            {
                itemIndex -= m_maxLine;
            }
            else if (itemIndex < 0)
            {
                itemIndex += m_maxLine;
            }
            cell = m_cellList[itemIndex];
            if (null != OnItemInit)
            {
                OnItemInit(cell, m_listData[i], true, true);
            }
            cell.itemObj.transform.localPosition = m_listData[i].localPosition;
            ++itemIndex;
        }
        MoveWithinBounds(Vector3.zero, true);
    }

    public ItemDataEx GetItemData(string name)
    {
        /*for (int i = 0, icount = m_listData.Count; i < icount; ++i)
        {
            if (m_listData[i].name.Equals(name))
            {
                return m_listData[i];
            }
        }*/
        if (null != mDataDict && mDataDict.ContainsKey(name))
        {
            return mDataDict[name];
        }
        return null;
    }

    public ItemObjectEx GetItemObject(ItemDataEx data)
    {
        ItemObjectEx cell = null;
        if (null != m_cellList)
        {
            for (int i = 0, imax = m_cellList.Length; i < imax; ++i)
            {
                if (m_cellList[i].data == data)
                {
                    cell = m_cellList[i];
                    break;
                }
            }
        }
        return cell;
    }

    public ItemObjectEx GetItemObject(int index)
    {
        if (null != m_cellList && index < m_cellList.Length)
        {
            return m_cellList[index];
        }
        return null;
    }
    /// <summary>
    /// 获得data在cell中的下标
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public int GetItemIndex(ItemDataEx data)
    {
        if (null != m_cellList)
        {
            for (int i = 0, imax = m_cellList.Length; i < imax; ++i)
            {
                if (m_cellList[i].data == data)
                {
                    return i;
                }
            }
        }
        return -1;
    }

    /// <summary>
    /// 更新显示，用于突然改变显示区域没更新过来的现象
    /// </summary>
    public void UpdateShow()
    {
        Validate();
    }


    public void CleanUp()
    {
        if (null != m_listData)
        {
            m_listData.Clear();
        }
        UpdateItem(0, -1, 0);
        UpdateBounds();
        mDrag.transform.localPosition = startPos;
        if (mDrag.movement == UIScrollViewEx.Movement.Vertical)
        {
            lastPos = mDrag.transform.localPosition.y;
        }
        else
        {
            lastPos = mDrag.transform.localPosition.x;
        }
        lastStartIndex = -1;
        lastEndIndex = -1;
        lastItemStartIndex = -1;
    }


    void Awake()
    {
        m_listData = new List<ItemDataEx>();
        mDataDict = new Dictionary<string, ItemDataEx>();

        defaultVec = new Vector3(0, 300, 0);
        mTrans = transform;

        mDrag = NGUITools.FindInParents<UIScrollViewEx>(gameObject);
        startPos = mDrag.transform.localPosition;

        // 设置scrollview的movement
        if (ArrangeDir == ArrangeDirection.Up_to_Down ||
           ArrangeDir == ArrangeDirection.Down_to_Up)
        {
            mDrag.movement = UIScrollViewEx.Movement.Vertical;
        }
        else
        {
            mDrag.movement = UIScrollViewEx.Movement.Horizontal;
        }
        if (null != Item)
        {
            Item.SetActive(false);
        }
        //m_maxLine = Mathf.CeilToInt(m_height / m_cellHeight) + 1;
    }

    void Update()
    {
        if (mDrag.movement == UIScrollViewEx.Movement.Vertical)
        {
            if (Mathf.Abs(mDrag.transform.localPosition.y - lastPos) > checkOffset * (mTrans.lossyScale.y / mDrag.transform.localScale.y))
            {
                Validate();
                lastPos = mDrag.transform.localPosition.y;
            }
        }
        else
        {
            if (Mathf.Abs(mDrag.transform.localPosition.x - lastPos) > checkOffset * (mTrans.lossyScale.x / mDrag.transform.localScale.x))
            {
                Validate();
                lastPos = mDrag.transform.localPosition.x;
            }
        }
        
    }

    private void UpdateBounds(bool toEnd = false, bool instant = true)
    {
        if (null == mDrag)
        {
            return;
        }
        Vector3 vMin = new Vector3();
        Vector2 scale = new Vector2(mTrans.lossyScale.x / mDrag.transform.lossyScale.x, mTrans.lossyScale.y / mDrag.transform.lossyScale.y);
        if (mDrag.movement == UIScrollViewEx.Movement.Vertical)
        {
            vMin.x = -mTrans.localPosition.x;
            vMin.y = mTrans.localPosition.y - m_AllCellHeight * scale.y;
        }
        else
        {
            vMin.x = mTrans.localPosition.x - m_AllCellWidth * scale.x;
            vMin.y = -mTrans.localPosition.y;
        }

        vMin.z = mTrans.localPosition.z;
        Bounds b = new Bounds(vMin, Vector3.one);

        b.Encapsulate(mTrans.localPosition);

        mDrag.bounds = b;
        //mDrag.UpdateScrollbars(true);
        if (toEnd)
        {
            MoveToEnd(instant);
        }
        else
        {
            mDrag.RestrictWithinBounds(instant);
        }
    }
    public void AddItem(ItemDataEx data, bool toEnd = true)
    {
        if (null != m_listData)
        {
            Vector3 lastPos = itemStartPos;
            Vector2 lastSize = Vector2.zero;
            float dis = 0;
            if (m_listData.Count > 0)
            {
                ItemDataEx lastData = m_listData[m_listData.Count - 1];
                lastPos = lastData.localPosition;
                lastSize = new Vector2(lastData.width, lastData.height);
                dis = gapDis;
            }
            if (mDrag.movement == UIScrollViewEx.Movement.Vertical)
            {
                m_AllCellHeight += dis +data.height;
                float offsetY = lastSize.y * 0.5f + dis + data.height * 0.5f;
                if (ArrangeDir == ArrangeDirection.Down_to_Up) offsetY *= -1f;
                data.localPosition = lastPos - new Vector3(0f, offsetY, 0f);
            }
            else
            {
                m_AllCellWidth += dis + data.width;
                float offsetX = lastSize.x * 0.5f + dis + data.width * 0.5f;
                if (ArrangeDir == ArrangeDirection.Right_to_Left) offsetX *= -1f;
                data.localPosition = lastPos + new Vector3(offsetX, 0f, 0f);
            }
            m_listData.Add(data);
            mDataDict[data.name] = data;
            if (m_listData.Count <= m_maxLine)
            {
                ++lastEndIndex;
                UpdateItem(lastStartIndex, lastEndIndex, lastItemStartIndex);
            }
            //int tmpStart = lastEndIndex + 1;
            //lastEndIndex = m_listData.Count - 1;
            //UpdateItem(tmpStart, lastEndIndex, tmpStart);
            UpdateBounds(toEnd, false);
        }
    }

    public void InsertItem(int index, ItemDataEx data)
    {
        if (null != m_listData)
        {
            if (index >= m_listData.Count)
            {
                AddItem(data);
                return;
            }
            Vector2 lastSize = Vector2.zero;
            Vector3 lastPos = itemStartPos;
            float dis = 0;
            if (index - 1 >= 0)
            {
                ItemDataEx last = m_listData[index - 1];
                lastSize = new Vector2(last.width, last.height);
                lastPos = new Vector2(last.localPosition.x, last.localPosition.y);
                dis = gapDis;
            }
            //重新计算宽高，和从插入位置开始的数据的坐标
            if (mDrag.movement == UIScrollViewEx.Movement.Vertical)
            {
                float offset = gapDis + data.height;
                m_AllCellHeight += offset;
                float offsetY = lastSize.y * 0.5f + dis + data.height * 0.5f;
                if (ArrangeDir == ArrangeDirection.Down_to_Up)
                {
                    offsetY *= -1f;
                    offset *= -1f;
                }
                data.localPosition = lastPos - new Vector3(0f, offsetY, 0f);
                for (int i = index, imax = m_listData.Count; i < imax; ++i)
                {
                    m_listData[i].localPosition -= new Vector3(0, offset, 0);
                }
            }
            else
            {
                float offset = gapDis + data.width;
                m_AllCellWidth += offset;
                float offsetX = lastSize.x * 0.5f + dis + data.width * 0.5f;
                if (ArrangeDir == ArrangeDirection.Right_to_Left) 
                {
                    offsetX *= -1f;
                    offset *= -1f;
                } 
                data.localPosition = lastPos + new Vector3(offsetX, 0f, 0f);
                for (int i = index, imax = m_listData.Count; i < imax; ++i)
                {
                    m_listData[i].localPosition += new Vector3(offset, 0, 0);
                }
            }
            m_listData.Insert(index, data);
            mDataDict[data.name] = data;
            //当前显示的元素
            int tmpNum = lastEndIndex - lastStartIndex + 1;
            int needNum = Mathf.Min(m_maxLine, m_listData.Count);
            if (tmpNum < needNum)
            {
                lastEndIndex += needNum - tmpNum;
            }
            UpdateItem(lastStartIndex, lastEndIndex, lastItemStartIndex);
            UpdateBounds(false);
            FrameMoveToWithinBounds(index, false);
        }
    }

    public void RemoveItem(ItemDataEx data)
    {
        if (null != m_listData && m_listData.Count > 0)
        {
            bool isLast = false;
            int index = m_listData.IndexOf(data);
            if (-1 == index)
            {
                return;
            }
            if (m_listData.Count - 1 == index)
            {
                isLast = true;
            }
            m_listData.Remove(data);
            mDataDict.Remove(data.name);
            float dis = 0;
            if (m_listData.Count > 0)
            {
                dis = gapDis;
            }
            if (mDrag.movement == UIScrollViewEx.Movement.Vertical)
            {
                m_AllCellHeight -= dis + data.height;
            }
            else
            {
                m_AllCellWidth -= dis + data.width;
            }
            if (lastEndIndex >= m_listData.Count)
            {//最后的元素超出了
                --lastEndIndex;
            }

            //if (isLast)
            {
                //当前显示的元素
                int tmpNum = lastEndIndex - lastStartIndex + 1;
                if (tmpNum < m_listData.Count && tmpNum < m_maxLine)
                {//当前显示的元素小于data的数量且小于cell的数量，则需增加显示元素，因为删除的是lastdata所以只能减少startData
                    int changeNum = m_maxLine - tmpNum;
                    if (lastStartIndex >= changeNum)
                    {
                        lastStartIndex -= changeNum;
                        lastItemStartIndex -= changeNum;
                    }
                    else
                    {
                        lastItemStartIndex -= lastStartIndex;
                        lastStartIndex = 0;
                    }
                    if (lastItemStartIndex < 0)
                    {
                        lastItemStartIndex += m_maxLine;
                    }
                }
                
            }
            if (!isLast)
            {
                if (mDrag.movement == UIScrollViewEx.Movement.Vertical)
                {
                    float offset = dis + data.height;
                    if (ArrangeDir == ArrangeDirection.Down_to_Up) offset *= -1;
                    for (int i = index, imax = m_listData.Count; i < imax; ++i)
                    {
                        m_listData[i].localPosition += new Vector3(0, offset, 0);
                    }
                }
                else
                {
                    float offset = dis + data.width;
                    if (ArrangeDir == ArrangeDirection.Right_to_Left) offset *= -1f;
                    for (int i = index, imax = m_listData.Count; i < imax; ++i)
                    {
                        m_listData[i].localPosition -= new Vector3(offset, 0, 0);
                    }
                }
            }
            UpdateItem(lastStartIndex, lastEndIndex, lastItemStartIndex);
            UpdateBounds(false, false);

        }
    }



    private void Validate()
    {
        if (null == m_listData || null == m_cellList || m_cellList.Length == 0)
        {
            return;
        }
        if (m_listData.Count <= m_maxLine)
        {//只需要更新加入的元素或者删除了元素
            if (-1 == lastStartIndex)
            {
                lastStartIndex = 0;
                lastEndIndex = m_listData.Count - 1;
                lastItemStartIndex = 0;
                UpdateItem(lastStartIndex, lastEndIndex, 0);
            }
            return;
        }
        //data多于cell,需重复利用cell
        Vector3 position = mDrag.panel.transform.localPosition;
        int _ver = 0;
        float allVer = 0;
        Vector3 offset = position - startPos;
        Vector2 scale = new Vector2(mTrans.lossyScale.x / mDrag.transform.lossyScale.x, mTrans.lossyScale.y / mDrag.transform.lossyScale.y);
        int startIndex = 0;
        int endIndex = 0;
        if (ArrangeDir == ArrangeDirection.Down_to_Up)
        {
            if (offset.y < 0)
            {
                _ver = -(int)offset.y;
            }//else拉到了开始位置，从0开始
            allVer = m_AllCellHeight * scale.y - mDrag.panel.finalClipRegion.w;
        }
        else if (ArrangeDir == ArrangeDirection.Up_to_Down)
        {
            if (offset.y > 0)
            {
                _ver = (int)offset.y;
            }//else拉到了开始位置，从0开始
            allVer = m_AllCellHeight * scale.y - mDrag.panel.finalClipRegion.w;
        }
        else if (ArrangeDir == ArrangeDirection.Left_to_Right)
        {
            if (offset.x < 0)
            {
                _ver = (int)-offset.x;
            }//else拉到了开始位置，从0开始
            allVer = m_AllCellWidth * scale.x - mDrag.panel.finalClipRegion.z;
        }
        else if (ArrangeDir == ArrangeDirection.Right_to_Left)
        {
            if (offset.x > 0)
            {
                _ver = (int)offset.x;
            }//else拉到了开始位置，从0开始
            allVer = m_AllCellWidth * scale.x - mDrag.panel.finalClipRegion.z;
        }
        //data数大于了cell数，需保证allVer大于0
        if (_ver == 0)
        {//拉到了开始位置，从0开始
            startIndex = 0;
            endIndex = m_maxLine - 1;
            if (endIndex >= m_listData.Count)
            {
                endIndex = m_listData.Count - 1;
            }
        }
        else if (_ver < allVer)
        {
            float size = 0;
            for (int i = 0, icount = m_listData.Count; i < icount; ++i)
            {
                if (mDrag.movement == UIScrollViewEx.Movement.Vertical)
                {
                    size += m_listData[i].height * scale.y;
                }
                else
                {
                    size += m_listData[i].width * scale.x;
                }
                if (size >= _ver)
                {
                    startIndex = i - (m_maxLine - (showNum + 1)) / 2;
                    if (startIndex < 0)
                    {
                        startIndex = 0;
                    }
                    endIndex = startIndex + m_maxLine - 1;
                    if (endIndex >= m_listData.Count)
                    {
                        endIndex = m_listData.Count - 1;
                    }
                    startIndex = endIndex - m_maxLine + 1;
                    if (startIndex < 0)
                    {
                        startIndex = 0;
                    }
                    break;
                }
            }
        }
        else
        {//到了最后
            endIndex = m_listData.Count - 1;
            startIndex = endIndex - m_maxLine + 1;
            if (startIndex < 0)
            {
                startIndex = 0;
            }
        }
        if (-1 == lastStartIndex)
        {
            lastStartIndex = startIndex;
            lastEndIndex = endIndex;
            lastItemStartIndex = 0;
            UpdateItem(lastStartIndex, lastEndIndex, 0);
        }
        else
        {
            GridItem(startIndex, endIndex);
        }
    }

    /*private void Validate()
    {
        if (null == m_listData || null == m_cellList)
        {
            return;
        }
        Vector3 position = mDrag.panel.transform.localPosition;

        float _ver = 0;
        float allVer = 0;
        if (mDrag.movement == UIScrollViewEx.Movement.Vertical)
        {
            _ver = Mathf.Max(Mathf.Abs(position.y - startPos.y), 0);
            allVer = m_AllCellHeight;
        }
        else
        {
            _ver = Mathf.Max(Mathf.Abs(position.x - startPos.x), 0);
            allVer = m_AllCellWidth;
        }
        int startIndex = 0;// = Mathf.FloorToInt(_ver / m_cellHeight);

        if (_ver >= allVer)
        {
            if (m_maxLine < 20)
            {
                startIndex = m_listData.Count - 5;
            }
            else
            {
                startIndex = m_listData.Count - m_maxLine / 4;
            }
            if (startIndex < 0)
            {
                startIndex = 0;
            }
        }
        else
        {
            float scale = 0;
            for (int i = 0, icount = m_listData.Count; i < icount; ++i)
            {
                if (mDrag.movement == UIScrollViewEx.Movement.Vertical)
                {
                    scale += m_listData[i].height;
                }
                else
                {
                    scale += m_listData[i].width;
                }
                
                if (scale >= _ver)
                {
                    if (m_maxLine < 20)
                    {
                        startIndex = i - 5;
                    }
                    else
                    {
                        startIndex = i - m_maxLine / 4;
                    }
                    if (startIndex < 0)
                    {
                        startIndex = 0;
                    }
                    break;
                }
            }
        }
        
        
        int endIndex = Mathf.Min(m_listData.Count, startIndex + m_maxLine);
        ItemObjectEx cell;
        int index = 0;
        for (int i = startIndex, icount = startIndex + m_maxLine; i < icount; i++)
        {
            cell = m_cellList[index];

            if (i < endIndex)
            {
                if (cell.dataIndex != i)
                {
                    cell.dataIndex = i;
                    cell.itemObj.name = m_listData[i].name;
                    if (null != OnItemInit)
                    {
                        OnItemInit(cell, m_listData[i]);
                    }
                    cell.itemObj.transform.localPosition = m_listData[i].localPosition;
                    cell.itemObj.SetActive(false);
                }
                cell.itemObj.SetActive(true);
            }
            else
            {
                cell.itemObj.transform.localPosition = defaultVec;
                cell.itemObj.name = (endIndex + index).ToString();
                cell.dataIndex = -1;
                cell.itemObj.SetActive(false);
            }

            index++;
        }
    }*/

    void UpdateItem(int startIndex, int endIndex, int itemIndex)
    {
        ItemObjectEx cell;
        int index = itemIndex;
        for (int i = startIndex, imax = startIndex + m_maxLine; i < imax; ++i)
        {
            if (index >= m_maxLine)
            {
                index -= m_maxLine;
            }
            cell = m_cellList[index];
            if (i <= endIndex)
            {
                if (cell.data != m_listData[i])
                {
                    cell.data = m_listData[i];
                    cell.itemObj.name = m_listData[i].name;
                    if (null != OnItemInit)
                    {
                        OnItemInit(cell, m_listData[i], true, true);
                    }
                    cell.itemObj.transform.localPosition = m_listData[i].localPosition;
                    cell.itemObj.SetActive(false);
                }
                cell.itemObj.SetActive(true);
            }
            else
            {
                //cell.itemObj.transform.localPosition = defaultVec;
                cell.itemObj.name = (endIndex + index).ToString();
                cell.data = null;
                cell.itemObj.SetActive(false);
            }
            index++;
        }
    }

    /*void UpdateItem(int dataStartIndex, int dataEndIndex, int itemIndex)
    {
        ItemObjectEx cell;
        int index = itemIndex;
        for (int i = dataStartIndex; i <= dataEndIndex; i++)
        {
            if (index >= m_cellList.Length)
            {
                index -= m_cellList.Length;
            }
            cell = m_cellList[index];
            if (cell.dataIndex != i)
            {
                cell.dataIndex = i;
                cell.itemObj.name = m_listData[i].name;
                if (null != OnItemInit)
                {
                    OnItemInit(cell, m_listData[i]);
                }
                cell.itemObj.transform.localPosition = m_listData[i].localPosition;
                cell.itemObj.SetActive(false);
            }
            cell.itemObj.SetActive(true);
            index++;
        }
    }*/
    void GridItem(int startIndex, int endIndex)
    {
        int index = 0;
        if (startIndex > lastEndIndex || endIndex < lastStartIndex)
        {//与上次无重合元素,从0开始重新排列
            index = 0;
        }
        else if (startIndex <= lastStartIndex)
        {
            index = lastItemStartIndex - (lastStartIndex - startIndex);
        }
        else
        {
            index = lastItemStartIndex + startIndex - lastStartIndex;
        }
        if (index >= m_maxLine)
        {
            index -= m_maxLine;
        }
        else if (index < 0)
        {
            index += m_maxLine;
        }
        lastStartIndex = startIndex;
        lastEndIndex = endIndex;
        lastItemStartIndex = index;
        //Debuger.Log(string.Format("lastStartIndex={0},lastEndIndex={1},lastItemStartIndex={2}", lastStartIndex, lastEndIndex, lastItemStartIndex));
        UpdateItem(lastStartIndex, lastEndIndex, lastItemStartIndex);
    }

    private void CreateItem()
    {
        if (null != m_cellList)
        {
            return;
        }
        m_cellList = new ItemObjectEx[m_maxLine];
        for (int i = 0; i < m_maxLine; i++)
        {
            GameObject go;
            go = Instantiate(Item) as GameObject;
            go.transform.parent = transform;
            go.transform.localScale = Vector3.one;
            //go.SetActive(false);
            go.name = "Item" + i;
            ItemObjectEx item = this.OnCreateItem(go);
            m_cellList[i] = item;
        }
    }
}