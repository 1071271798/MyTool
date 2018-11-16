using System.Collections.Generic;
using UnityEngine;


public class LoadPrefab : MonoBehaviour
{
    public List<string> resPathList;

    void Awake()
    {
        if (null != resPathList && resPathList.Count > 0)
        {
            List<GameObject> list = new List<GameObject>();
            for (int i = 0, imax = resPathList.Count; i < imax; ++i)
            {
                if (!string.IsNullOrEmpty(resPathList[i]))
                {
                    GameObject obj = Resources.Load(resPathList[i], typeof(GameObject)) as GameObject;
                    if (null != obj)
                    {
                        list.Add(obj);
                    }
                }
            }
            for (int i = 0, imax = list.Count; i < imax; ++i)
            {
                GameObject o = UnityEngine.Object.Instantiate(list[i]) as GameObject;
                o.name = list[i].name;
            }
            list.Clear();
        }
        
    }
}

