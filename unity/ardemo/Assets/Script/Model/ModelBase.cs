// ------------------------------------------------------------------
// Description : 模型基类
// Author      : oyy
// Date        : 2015-04-24
// Histories   : 
// ------------------------------------------------------------------


using UnityEngine;
using System.Collections;

namespace Game.Model
{
    class ModelBase
    {
        protected ModelType m_type;
        public ModelType Type { get { return this.m_type; } }

        protected GameObject m_gameObj;

        public ModelBase(GameObject obj)
        {
            m_gameObj = obj;
        }
    }
}
