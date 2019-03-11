using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;

public class ExportAssets : MonoBehaviour
{

    //Texture2D test;
    //string url;
    //// Use this for initialization
    //void Start()
    //{
    //    url = "file:" + Application.persistentDataPath + "/test.unity";
    //   // url = Path.Combine(url, "Armor Chariot.jpg");
    //    Debug.Log("url:" + url);
    //    StartCoroutine(LoadWWW());
    //    //WWW www = new WWW(url);
    //    //test = www.texture;
        
    //}
    //private IEnumerator LoadWWW()
    //{
    //    yield return 0;
    //    WWW www = WWW.LoadFromCacheOrDownload(url,1); //new WWW(url);
    //    yield return www;
    //     if(www.isDone)
    //     {
    //         Instantiate(www.assetBundle.mainAsset);

    //         //test = www.;

    //        // Debug.Log("test:" + test);
    //     }

    //} 

    /// <summary>
    /// 将选中的预制分别打包
    /// </summary>
	[MenuItem("AssetBundleDemo/Create AssetBundles By themselves")]
    static void CreateAssetBundleThemelves()
    {
        //获取要打包的对象（在Project视图中）
        Object[] selects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        //遍历选中的对象
        foreach (Object obj in selects)
        {
            Debug.Log("ffdfdf");
            //这里建立一个本地测试
            //注意本地测试中可以是任意的文件，但是到了移动平台只能读取路径StreamingAssets里面的
            //StreamingAssets是只读路径，不能写入
            string targetPath = Application.dataPath + "/AssetBundleLearn/StreamingAssets/editor/" + obj.name + ".assetbundle";//文件的后缀名是assetbundle和unity都可以
			if (BuildPipeline.BuildAssetBundle(obj, null, targetPath, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, BuildTarget.Android))   //|BuildAssetBundleOptions.DeterministicAssetBundle|BuildAssetBundleOptions.CompleteAssets, BuildTarget.StandaloneWindows
            {

                Debug.Log(obj.name + "is packed successfully!");
            }
            else
            {
                Debug.Log(obj.name + "is packed failly!");
            }

            string targetPathandroid = Application.dataPath + "/AssetBundleLearn/StreamingAssets/android/" + obj.name + ".assetbundle";//文件的后缀名是assetbundle和unity都可以
            if (BuildPipeline.BuildAssetBundle(obj, null, targetPathandroid, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, BuildTarget.Android))
            {

                Debug.Log(obj.name + "android is packed successfully!");
            }
            else
            {
                Debug.Log(obj.name + "android is packed failly!");
            }

            string targetPathios = Application.dataPath + "/AssetBundleLearn/StreamingAssets/ios/" + obj.name + ".assetbundle";
            if (BuildPipeline.BuildAssetBundle(obj, null, targetPathios, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, BuildTarget.iOS))
            {

                Debug.Log(obj.name + "ios is packed successfully!");
            }
            else
            {
                Debug.Log(obj.name + "ios is packed failly!");
            }
        }
        //刷新编辑器（不写的话要手动刷新,否则打包的资源不能及时在Project视图内显示）
        AssetDatabase.Refresh();
    }

    [MenuItem("AssetBundleDemo/Create AssetBundles Together")]
    static void CreateAssetBundleTogether()
    {
		
    //要打包的对象
		
    Object[] selects = Selection.GetFiltered (typeof(Object),SelectionMode.DeepAssets);
		
    //要打包到的路径
		
    string targetPath = Application.dataPath + "/AssetBundleLearn/StreamingAssets/editor/Together.assetbundle";
    
		if(BuildPipeline.BuildAssetBundle(null,selects,targetPath,BuildAssetBundleOptions.CollectDependencies, BuildTarget.Android))
    {
			
    Debug.Log("Packed successfully!");

		
    }
    else
    {
			
    Debug.Log("Packed failly!");
		
    }

    string targetPathandroid = Application.dataPath + "/AssetBundleLearn/StreamingAssets/android/Together.assetbundle";//文件的后缀名是assetbundle和unity都可以
    if (BuildPipeline.BuildAssetBundle(null, selects, targetPathandroid, BuildAssetBundleOptions.CollectDependencies, BuildTarget.Android))
    {

        Debug.Log( "android is packed successfully!");
    }
    else
    {
        Debug.Log( "android is packed failly!");
    }

    string targetPathios = Application.dataPath + "/AssetBundleLearn/StreamingAssets/ios/Together.assetbundle";//文件的后缀名是assetbundle和unity都可以
    if (BuildPipeline.BuildAssetBundle(null,selects, targetPathios, BuildAssetBundleOptions.CollectDependencies, BuildTarget.iOS))
    {

        Debug.Log( "ios is packed successfully!");
    }
    else
    {
        Debug.Log("ios is packed failly!");
    }
		
    //刷新编辑器（不写的话要手动刷新）
		
    AssetDatabase.Refresh ();
	
    }

    [MenuItem("AssetBundleDemo/批量打包")]
    static void BuildeSelectFloder()
    {

    }
}
