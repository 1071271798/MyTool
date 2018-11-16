//----------------------------------------------
//            积木2: xiongsonglin
// Copyright © 2015 for Open
//----------------------------------------------
using UnityEngine;
using System.Collections;

public class ManageObject : MonoBehaviour {

    public GameObject ManageOBJ;

    public static ManageObject Ins;

    void Awake()
    {
        if (Ins == null)
            Ins = this;
    }
	// Use this for initialization
	void Start () {

	}

}
