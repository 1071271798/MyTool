// ------------------------------------------------------------------
// Description :  模型控制器
// Author       :  oyy
// Date           :  2015-04-24
// Histories     : 
// ------------------------------------------------------------------


using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Game.Model
{
    class ModelControl
    {
        readonly Vector3 m_modelPos = new Vector3(-100, 0, 0);                                          //模型位置

        readonly Dictionary<string, Type> model2Type = new Dictionary<string, Type>() 
        {
            {"duoji",Type.GetType("Duoji")},
        };

        GameObject m_modelParent;                                                                                   //模型挂载的父物体

        GameObject m_modelObj;                                                                                        //模型物体

        Dictionary<ModelType, List<ModelBase>> m_childModel = new Dictionary<ModelType, List<ModelBase>>();               //所有的模型

        public ModelControl()
        {
            Init();
        }

        void Init()
        {
            CreateParent();
            LoadModel();
            CheckModel();
        }

        void CreateParent()
        {
            m_modelParent = new GameObject("Model");
            m_modelParent.transform.localPosition = m_modelPos;
        }

        void LoadModel()
        {
            GameObject obj = Resources.Load<GameObject>("Prefab/Normal/Model");
            if (obj != null)
            {
                m_modelObj = GameObject.Instantiate(obj) as GameObject;
                m_modelObj.transform.parent = m_modelParent.transform;
                m_modelObj.transform.localPosition = Vector3.zero;
                m_modelObj.transform.localRotation = Quaternion.Euler(Vector3.zero);
                m_modelObj.transform.localScale = Vector3.one;
            }
        }

        //检测模型的组成，比如模型A由模型B,C,D,组成
        void CheckModel()
        {
            for (int i = 0; i < m_modelObj.transform.childCount; i++)
            {
                GameObject obj = m_modelObj.transform.GetChild(i).gameObject;
                ModelBase model = (System.Reflection.Assembly.GetExecutingAssembly().CreateInstance(obj.name)) as ModelBase;
                if (model != null)
                {
                    if (m_childModel.ContainsKey(model.Type))
                    {
                        m_childModel[model.Type].Add(model);
                    }
                    else
                    {
                        m_childModel.Add(model.Type, new List<ModelBase>() { model });
                    }
                }
            }
        }

    }
}

    
