using UnityEngine;
using System.Collections;

public class UV : MonoBehaviour {
    public Vector2 tile = Vector2.one;
    public Vector2 offset = Vector2.zero;
	public Color color = Color.white;
	void Update () {
        Material mat = GetComponent<Renderer>().sharedMaterial;
        if (mat)
        {
            mat.mainTextureOffset = offset;
            mat.mainTextureScale = tile;
			mat.SetColor("_Color", color);
        }

	}


}
