//----------------------------------------------
//            积木2: xiongsonglin
// Copyright © 2015 for Open
//----------------------------------------------
using UnityEngine;
using System.Collections;

public class LoadScenesOver : MonoBehaviour {

	// Use this for initialization
	void Start () {

        StartCoroutine(LoadOver());
        StartCoroutine(LoadOver1());
	}

    IEnumerator LoadOver()
    {
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator LoadOver1()
    {
        yield return null;
        yield return null;
        yield return new WaitForEndOfFrame();
    }

}
