// ------------------------------------------------------------------
// Description : 游戏点击体
// Author      : oyy
// Date        : 
// Histories   : 
// ------------------------------------------------------------------

using UnityEngine;
using System.Collections;

namespace Game
{
    class ClickBody
    {
        protected int m_id = 0;                                             //按钮的ID
        public int Id
        {
            get { return this.m_id; }
            set
            {
                if (value > 0)
                {
                    this.m_id = value;
                }
            }
        }

        protected GameObject m_gameObj;                                     //按钮
        public GameObject GameObj
        {
            get { return this.m_gameObj; }
        }

        protected BoxCollider m_box;                                        //按钮的碰撞体
        public BoxCollider Box
        {
            get { return this.m_box; }
        }

        protected UIEventListener m_listener;                               //事件监听器
        public UIEventListener Listener
        {
            get { return this.m_listener; }
        }

        protected bool m_enable = true;                                     //是否可以点击
        public bool Enable
        {
            get { return this.m_enable; }
            set
            {
                this.m_enable = value;
                if (Box != null)
                {
                    this.Box.enabled = m_enable;
                }
            }
        }

        protected bool m_soundEnable = true;                                //按钮音效启用
        public bool SoundEnable
        {
            get { return m_soundEnable; }
            set { m_soundEnable = value; }
        }

        public enum AniType                                                 //按钮点击之后的动画表现
        {
            Color,
            Scale,
            None,
        }
        public AniType AniTyped = AniType.None;

        public ClickBody(GameObject obj, int id = -1)
        {
            if (obj != null)
            {
                this.Id = id;
                this.m_gameObj = obj;
                Init();
            }
        }

        private void Init()
        {
            m_box = m_gameObj.GetComponent<BoxCollider>();
            if (m_box == null)
            {
                m_box = m_gameObj.AddComponent<BoxCollider>();
                m_box.size = Vector3.one;
                m_box.center = Vector3.zero;
            }
            m_listener = m_gameObj.GetComponent<UIEventListener>();
            if (m_listener == null)
            {
                m_listener = m_gameObj.AddComponent<UIEventListener>();
            }
            m_listener.onPress += On_Click;
        }

        void On_Click(GameObject go,bool isPress)
        {
            PlayAni(isPress);
        }

        //按钮点击动画
        protected virtual void PlayAni(bool ispress)
        {
            if (AniTyped == AniType.Scale)
                this.m_gameObj.transform.localScale = ispress ? new Vector3(1.2f, 1.2f, 1) : Vector3.one;
        }

    }
}
