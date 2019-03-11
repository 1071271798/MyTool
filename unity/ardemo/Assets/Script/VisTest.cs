using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    // Update is called once per frame
    void OnBecameVisible()
    {

        Debug.Log("摄像机视野内");

    }

    void OnBecameInvisible()
    {

        Debug.Log("在摄像机视野外");

    }
}
