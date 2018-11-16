using UnityEngine;
using System.Collections;

public class TextureSize : MonoBehaviour {

	// Use this for initialization
	void Start () {
        UITexture iosTex = this.transform.GetComponent<UITexture>();
        if (null != iosTex)
        {
            int screenWidth = PublicFunction.GetWidth();
            int screenHeight = PublicFunction.GetHeight();

            float oldWidth = 1334.0f;
            float oldHeight = 750.0f;
            int height = (int)(screenHeight * oldWidth / screenWidth);
            if (height >= oldHeight)
            {
                iosTex.height = screenHeight;
                iosTex.width = Mathf.CeilToInt(screenHeight * oldWidth / oldHeight);
            }
            else
            {
                iosTex.height = Mathf.CeilToInt(screenWidth * oldHeight / oldWidth);
                iosTex.width = screenWidth;
            }
        }

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
