using UnityEngine;
using System.Collections;
using UnityEditor;

public class test : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Debug.Log("AAS");
	}
	
	// Update is called once per frame
	void Update () {
	if(Input.GetKeyDown(KeyCode.A))
    {
        Debug.Log("AA");
        AnimationUtility am = new AnimationUtility();
        this.gameObject.transform.Translate(new Vector3(0.5f,0,0f),Space.Self);
    }
	}
}
