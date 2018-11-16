// ------------------------------------------------------------------
// Description : UI点击体（UI按钮）
// Author      : oyy
// Date        : 
// Histories   : 
// ------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using Game;

namespace Game.UI
{
    class UIClickBody:ClickBody
    {
        private UIWidget.Pivot m_pivot = UIWidget.Pivot.Center;              //按钮中心点
        public UIWidget.Pivot Pivot
        {
            get { return this.m_pivot; }
            set 
            {
                m_pivot = value;
                SetCollider(value);
            }
        }

        private UISprite m_back;
        private UILabel m_label;
        public UILabel Label { get { return this.m_label; } }

        public UIClickBody(GameObject obj, int id = -1)
            : base(obj, id)
        {
            if (obj != null)
            {
                Init();
            }
        }

        private void Init()
        {
            m_back = GameHelper.FindChildComponent<UISprite>(m_gameObj.transform, "back");
            m_label = GameHelper.FindChildComponent<UILabel>(m_gameObj.transform, "Label");
            SetCollider(m_pivot);
        }

        public void Show()
        {
            m_gameObj.SetActive(true);
        }

        public void Hide()
        {
            m_gameObj.SetActive(false);
        }

        //根据按钮中心点来设置按钮Collider
        private void SetCollider(UIWidget.Pivot pivot)
        {
            Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(m_gameObj.transform);
            if (!bounds.size.Equals(Vector3.zero))
            {
                //m_box.size = bounds.size;
                Vector2 offset = NGUIMath.GetPivotOffset(pivot);
                offset = new Vector2(0.5f, 0.5f) - offset;
                offset.x *= m_box.size.x;
                offset.y *= m_box.size.y;
                m_box.center = offset;
            }
        }

        protected override void PlayAni(bool ispress)
        {
            
        }
    }
}
