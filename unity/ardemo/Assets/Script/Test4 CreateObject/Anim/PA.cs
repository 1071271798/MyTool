using UnityEngine;
using System.Collections;

public class PA : MonoBehaviour {

    //public GameObject jt=null;
    public GameObject c6=null;
    public GameObject dp = null;
    //public AnimationClip paAnim;  //模型动画
	// Use this for initialization
	void Start () {
       // paAnim = Resources.Load("Prefab/Test4/Anims/PA_Animation") as AnimationClip;
       //this.transform.GetComponent<Animation>().AddClip(paAnim,"test",0,(int)(paAnim.length*60));
       // this.transform.GetComponent<Animation>().Play("test");
       // this.transform.GetComponent<Animation>().wrapMode=;
	}

    //float timeT = 0;
	// Update is called once per frame
	void Update () {

       if( this.transform.GetComponent<Animation>().isPlaying==false)
       {
           StartCoroutine(DelayPlay());
       }
	}

    IEnumerator DelayPlay()
    {
        yield return new WaitForEndOfFrame();
        this.transform.GetComponent<Animation>().Play();
        //jt.GetComponent<UISpriteAnimation>().Reset();
        if(c6!=null)
        {
           c6.GetComponent<UISpriteAnimation>().Reset();
        }

        if (dp != null)
        {
            dp.GetComponent<UISpriteAnimation>().Reset();
        }
    }
}
