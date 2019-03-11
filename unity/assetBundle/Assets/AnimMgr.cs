using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class AnimMgr : MonoBehaviour {
  
    private Animation anim;
    public bool isPlay = true;

  //  public bool[] plays;

    public bool play0;
    public bool play1;
    public bool play2;
    public bool play3;
    public bool play4;

    List<bool> boollist = new List<bool>();

    List<string> animNames=new List<string>();

    List<bool> boollistOld = new List<bool>();

	// Use this for initialization
	void Start () {
        

        anim = this.GetComponent<Animation>();

        foreach(AnimationState _state in anim)
        {
            Debug.Log("CLIP:" + _state.name);
            animNames.Add(_state.name);
        }
        int animCount = animNames.Count;

        

            play0=false;
            play1 = true;
            play2 = false;
            play3 = false;
            play4 = false;
            boollist.Add(play0);
            boollist.Add(play1);
            boollist.Add(play2);
            boollist.Add(play3);
            boollist.Add(play4);

	}


    string namePlayTemp;
	// Update is called once per frame
	void Update () {
	   if(isPlay && !anim.enabled)
       {
           anim.enabled = true;
           anim.Play();
       }
       else if(!isPlay &&anim.enabled)
       {
           anim.Stop();
           anim.enabled = false;
          
       }

       JustOne();
	}

    int numTemp = 0;
    public void JustOne()
    {
        for(int i=0;i<boollist.Count;i++)
        {

            if (boollist[i]&&numTemp!=i&&i<animNames.Count)
            {
                Debug.Log("nameTemp play:" + i);
                anim.Play(animNames[i]);

                numTemp = i;    
            }
        }

    }

}
