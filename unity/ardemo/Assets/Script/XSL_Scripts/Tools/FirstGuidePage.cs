using UnityEngine;
using System.Collections;

public class FirstGuidePage : MonoBehaviour {

    public GameObject CloseBtn;
    public GameObject ConfrimBtn;
    public Transform labelTrans;
    public UILabel overButton;
    public Transform LinesTrans;

    private static FirstGuidePage ins;
    public static FirstGuidePage GetIns()
    {
        return ins;
    }
    void Awake()
    {
        ins = this;
    }
    void Start()
    {
      //  Show(0.5f);
    }

    public static void LoadGuidePage(string path, Transform parent)
    {
        GameObject obj = Resources.Load(path) as GameObject;
        if (obj != null)
        {
            obj = Instantiate(obj) as GameObject;
            obj.transform.SetParent(parent);
            obj.transform.localScale = Vector3.one;
            if (path.Contains("pad"))
                obj.transform.localPosition = new Vector3(-5, 0, 0);
            else
                obj.transform.localPosition = new Vector3(-25, 0, 0);

        }
    }

    //public static void LoadGuidePage(GameObject go, Transform parent)
    //{
    //    GameObject obj = go;
    //    string nameT = obj.name;
    //    if (obj != null)
    //    {
    //        //obj = Instantiate(obj) as GameObject;
    //        obj.transform.SetParent(parent);
    //        obj.transform.localScale = Vector3.one;
    //        if (nameT.Contains("pad"))
    //            obj.transform.localPosition = new Vector3(-5, 0, 0);
    //        else
    //            obj.transform.localPosition = new Vector3(-25, 0, 0);
            
    //        if(obj.GetComponent<FirstGuidePage>()!=null)
    //        {
    //            obj.GetComponent<FirstGuidePage>().enabled = true;
    //        }
    //    }
    //}

    public void Show(float time)
    {
        if (CloseBtn != null)
            UIEventListener.Get(CloseBtn).onClick = OnCloseBtn;
        if (ConfrimBtn != null)
            UIEventListener.Get(ConfrimBtn).onClick = OnConfrimBtn;

        StartCoroutine(WaitAtime(time));
       // EventDelegate a = new EventDelegate(OnClosePage);
    }

    IEnumerator WaitAtime(float time)
    {
        yield return new WaitForSeconds(time);
        TweenColor tcolor = gameObject.GetComponent<TweenColor>();
        if (tcolor == null)
            tcolor = gameObject.AddComponent<TweenColor>();
        tcolor.from = new Color(1, 1, 1, 0);
        tcolor.to = new Color(1, 1, 1, 0.9f);
        tcolor.duration = 0.5f;
        tcolor.PlayForward();

        if (labelTrans != null)
        {
            for (int i = 0; i < labelTrans.childCount; i++)
            {
                labelTrans.GetChild(i).GetChild(0).GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("Guidepage_"+(i+1).ToString() +"_1");
                labelTrans.GetChild(i).GetChild(1).GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("Guidepage_" + (i + 1).ToString() + "_2");
            }
        }
        if (overButton != null)
        {
            overButton.text = LauguageTool.GetIns().GetText("Guidepage_over");
        }
    }

    void OnConfrimBtn(GameObject go)
    {
        TweenColor tcolor = gameObject.GetComponent<TweenColor>();
        if(tcolor == null)
            tcolor = gameObject.AddComponent<TweenColor>();
        //tcolor.from = new Color(1,1,1,1);
        //tcolor.to = new Color(1, 1, 1, 0);
        tcolor.duration = 0.5f;
        
        EventDelegate a = new EventDelegate(OnClosePage);
        tcolor.PlayReverse();
        tcolor.onFinished.Add(a);
    }

    public void OnClosePage()
    {
        Destroy(gameObject);
    }

    void OnCloseBtn(GameObject go)
    {
        Destroy(gameObject);
    }
    
}
