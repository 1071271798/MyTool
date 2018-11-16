using UnityEngine;
using System.Collections;

public class CreateUnityGO : MonoBehaviour {

	public static GameObject CreateEmptyGO()
    {
        GameObject emptygo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(emptygo.GetComponent<MeshFilter>());
        Destroy(emptygo.GetComponent<BoxCollider>());
        Destroy(emptygo.GetComponent<MeshRenderer>());

        return emptygo;
    }
}
