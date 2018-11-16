using UnityEngine;
using System.Collections;

public class AdaptPower : MonoBehaviour
{

    private UISprite powerSprite;
    public float durTime;
    float curTime;
    int curTex;
    float ttt;

    //private float 
    void FixedUpdate()
    {
        if (null != powerSprite && powerSprite.enabled && enabled)
        {
            if (curTime + durTime > Time.time)
            {

            }
            else
            {
                curTex++;
                curTex = curTex % 20;
                powerSprite.fillAmount = curTex * 0.05f;
                //showErea.mainTexture = tex[curTex];
                curTime = Time.time;
                ttt += curTime;
            }
        }
    }

    void OnEnable()
    {
        if(powerSprite == null)
            powerSprite = GetComponent<UISprite>();
    }
}
