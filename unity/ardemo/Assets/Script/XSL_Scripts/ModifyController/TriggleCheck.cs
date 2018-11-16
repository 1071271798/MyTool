using UnityEngine;
using System.Collections;

public class TriggleCheck : MonoBehaviour {
    public delegate void voidDelegate(Collider col);
    public voidDelegate onTriggleEnter;
    public voidDelegate onTriggleExit;
    void OnTriggerEnter(Collider collider)
    {
        if (onTriggleEnter != null)
            onTriggleEnter(collider);
    }

    void OnTriggerExit(Collider collider)
    {
        if (onTriggleExit != null)
            onTriggleExit(collider);
        
    }
}
