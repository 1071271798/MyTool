
using UnityEngine;

public class GameBg : SingletonObject<GameBg>
{
    private UITexture mGamebgTexture;



    public void ShowBg()
    {
        FindBgGameObject();
        if (null != mGamebgTexture)
        {
            mGamebgTexture.enabled = true;
        }
    }


    public void HideBg()
    {
        if (null != mGamebgTexture)
        {
            mGamebgTexture.enabled = false;
        }
    }


    private void FindBgGameObject()
    {
        if (null == mGamebgTexture)
        {
            GameObject tex = GameObject.Find("MainClient/UI Root/Camera/gamebg/tex");
            if (null != tex)
            {
                mGamebgTexture = tex.GetComponent<UITexture>();
            }
        }
    }


}

