using UnityEngine;
using System.Collections;

namespace Game.UI
{
    class UIHelper
    {
        //添加侦听器,trans要添加的物件,size碰撞体大小
        public static UIEventListener AddUIListener(Transform trans, Vector3 size)
        {
            if (trans != null)
            {
                if (trans.GetComponent<Collider>() == null)
                {
                    BoxCollider box = trans.gameObject.AddComponent<BoxCollider>();
                    box.center = Vector3.zero;
                    box.size = size;
                }

                UIEventListener lis = trans.gameObject.GetComponent<UIEventListener>();
                if (lis == null)
                {
                    lis = trans.gameObject.AddComponent<UIEventListener>();
                }
                return lis;
            }
            Debuger.LogWarning("Function AddUIListener,Transform is null");
            return null;
        }

    }
}
