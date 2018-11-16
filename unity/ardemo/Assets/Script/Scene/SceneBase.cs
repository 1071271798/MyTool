// ------------------------------------------------------------------
// Description : 场景基类
// Author      : oyy
// Date        : 2015-04-24
// Histories   : 
// ------------------------------------------------------------------

using UnityEngine;
using System.Collections;

namespace Game.Scene
{
    abstract class SceneBase
    {
        protected int m_id;
        public int Id { get { return this.m_id; } }

        //销毁
        public virtual void Dispose()
        {

        }
    }
}
