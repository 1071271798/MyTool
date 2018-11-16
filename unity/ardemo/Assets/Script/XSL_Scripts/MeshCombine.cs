//----------------------------------------------
//            积木2: xiongsonglin
// Copyright © 2015 for Open
//----------------------------------------------
using UnityEngine;
using System.Collections;

public class MeshCombine : MonoBehaviour {

void Start ()
    {
        MeshFilter [] meshFilters = GetComponentsInChildren<MeshFilter> ();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++) {
            combine [i].mesh = meshFilters [i].sharedMesh;
            combine [i].transform = meshFilters [i].transform.localToWorldMatrix;
            meshFilters [i].gameObject.SetActive(false); 
        }

        if (transform.GetComponent<MeshFilter>().mesh == null)
        {
            transform.gameObject.AddComponent<MeshFilter>();
        }
        transform.GetComponent<MeshFilter> ().mesh = new Mesh ();
        transform.GetComponent<MeshFilter> ().mesh.CombineMeshes (combine);
        transform.gameObject.SetActive(true);
    }
}

