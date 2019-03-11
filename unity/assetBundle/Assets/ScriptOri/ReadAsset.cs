using UnityEngine;
using System.Collections;

public class ReadAsset : MonoBehaviour {
    //不同平台下StreamingAssets的路径是不同的，这里需要注意一下。
    public static readonly string m_PathURL =
#if UNITY_ANDROID
		"jar:file://" + Application.dataPath + "!/assets/";
#elif UNITY_IPHONE
		Application.dataPath + "/Raw/";
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
 "file:///" + Application.persistentDataPath + "/AssetBundleLearn/StreamingAssets/";
#else
		string.Empty;
#endif

    GameObject obj;
    void OnGUI()
    {
        if (GUILayout.Button("加载分开打包的Assetbundle"))
        {
            //StartCoroutine(LoadGameObjectPackedByThemselves(m_PathURL + "One.assetbundle"));
            StartCoroutine(LoadGameObjectPackedByThemselves(m_PathURL + "Two.assetbundle"));
            StartCoroutine(LoadGameObjectPackedByThemselves(m_PathURL + "Three.assetbundle"));


            string path1 = "file:///" + Application.persistentDataPath + "/AssetBundleLearn/StreamingAssets/qlg.assetbundle";
            WWW bundle1 = new WWW(path1);
            Object t= bundle1.assetBundle.mainAsset;
          
            AnimationClip anim = t as AnimationClip;


            string path2 = "file:///" + Application.persistentDataPath + "/AssetBundleLearn/StreamingAssets/One.assetbundle";

          Debug.Log("path2:" + path2);
          WWW bundle2 = new WWW(path2);
          GameObject t2 = Instantiate(bundle2.assetBundle.mainAsset) as GameObject;

         
          Debug.Log("obj:"+t2.name);
          Animation animT= t2.AddComponent<Animation>();
          animT.AddClip(anim,anim.name,0,300);
          
           //Debug.Log(type1);

        }

        if (GUILayout.Button("加载打包在一起的Assetbundle"))
        {
            StartCoroutine(LoadGameObjectPackedTogether(m_PathURL + "Together.assetbundle"));
        }

    }
    //单独读取资源
    private IEnumerator LoadGameObjectPackedByThemselves(string path)
    {
        Debug.Log("path:"+path);
        WWW bundle = new WWW(path);
        yield return bundle;

        //加载
        obj = Instantiate(bundle.assetBundle.mainAsset) as GameObject;
        Debug.Log("objtf:" + obj.name);
        yield return obj;
        bundle.assetBundle.Unload(false);
    }

    IEnumerator LoadGameObjectPackedTogether(string path)
    {
        WWW bundle = new WWW(path);
        yield return bundle;

       // Object one = bundle.assetBundle.Load("One");
        Object two = bundle.assetBundle.LoadAsset("Two");
        Object three = bundle.assetBundle.LoadAsset("Three");

        Object t = bundle.assetBundle.LoadAsset("qlg");

        AnimationClip anim = t as AnimationClip;
        Debug.Log(anim.length);

        //加载
        //yield return Instantiate(one);
        yield return Instantiate(two);
        yield return Instantiate(three);
        bundle.assetBundle.Unload(false);
    }
}

