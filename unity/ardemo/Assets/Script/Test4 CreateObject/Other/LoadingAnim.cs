using UnityEngine;
using System.Collections;

public class LoadingAnim : MonoBehaviour {
    UISprite loadSprite;
    int i = 0;
	// Use this for initialization
	void Awake () {
        loadSprite = this.transform.GetComponent<UISprite>();

        //InvokeRepeating("Input",0.01f,0.001f);
	}
	
    public void Input()
    {
        if (i < 48)
        {
            if (i % 3 == 0)
            {

                int j = i / 3;
                loadSprite.spriteName = "loading_" + j;
            }

            i++;
            if (i == 47)
            {
                i = 0;
            }
        }
    }

	// Update is called once per frame
	void Update () {
        Input();

    }
}
