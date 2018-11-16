using UnityEngine;
using System.Collections;

public class JTAnim : MonoBehaviour {

    public bool isPlay = false;
	// Use this for initialization
	void Start () {
        
	}
	
	void Update () {
        if(isPlay)
        {
            this.GetComponent<UISpriteAnimation>().Reset();
        }
	}
}
