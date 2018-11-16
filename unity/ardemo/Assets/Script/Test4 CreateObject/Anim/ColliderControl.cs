using UnityEngine;
using System.Collections;

public class ColliderControl{

	// Update is called once per frame
    public static void AddClass(GameObject go,string type)
    {
        Debug.Log("AD");
        switch (type){
            case "ysjlsCollider":
                ysjlsCollider ys = go.GetComponent<ysjlsCollider>();
                if (ys==null)
                {
                    go.AddComponent<ysjlsCollider>();
                }
                
                break;
 
        }

	}
}
