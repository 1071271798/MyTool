using UnityEngine;
using System.Collections;

public class connectEffect : MonoBehaviour {
    UISprite sprite;

    
    void OnEnable()
    {
        sprite = gameObject.GetComponent<UISprite>();
        sprite.fillAmount = 0;
        StartCoroutine(LoadTex());
    }
    IEnumerator LoadTex()
    {


        while(sprite.fillAmount < 0.9f)
        {
            float waitTime = Random.Range(0.7f,1f);
            yield return new WaitForSeconds(waitTime);
            float randomStatus = Random.Range(0.003f,0.03f);
            for (int i = 0; i < 20; i++)
            {
                sprite.fillAmount += randomStatus;
                yield return new WaitForSeconds(0.02f);
                if (sprite.fillAmount > 0.9f)
                {
                    break;
                }
            }

        }
    }
}
