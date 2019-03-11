using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Xml;

public class AssetBundleTool : EditorWindow
{
    string mSelectPath;
    Dictionary<string, AssetBundleData> mAssetBundelDict;
    Vector2 mAssetBundlePosition = Vector2.zero;

    string mSelectAnimPath;
    Dictionary<string, AnimFileData> mAnimDict;
    Vector2 mAnimPosition = Vector2.zero;
    string mAssetBundleOutPath;

    [MenuItem("MyTool/AssetBundle")]
    public static void OpenCopyFilesEditor()
    {
        AssetBundleTool windows = EditorWindow.GetWindow<AssetBundleTool>(true, "AssetBundleTool");
        windows.position = new Rect(400, 100, 700, 900);
    }

    [MenuItem("MyTool/OptimizeAnim")]
    public static void OptimizeAnim()
    {
        var tObjArr = Selection.gameObjects;
        foreach (var obj in tObjArr)
        {
            RemoveAnimationCurve(obj);
        }
    }

    //移除scale
    public static void RemoveAnimationCurve(GameObject _obj)
    {
        List<AnimationClip> tAnimationClipList = new List<AnimationClip>(AnimationUtility.GetAnimationClips(_obj));
        if (tAnimationClipList.Count == 0)
        {
            AnimationClip[] tObjectList = UnityEngine.Object.FindObjectsOfType(typeof(AnimationClip)) as AnimationClip[];
            tAnimationClipList.AddRange(tObjectList);
        }

        foreach (AnimationClip animClip in tAnimationClipList)
        {
            foreach (EditorCurveBinding curveBinding in AnimationUtility.GetCurveBindings(animClip))
            {
                string tName = curveBinding.propertyName.ToLower();
                if (tName.Contains("scale"))
                {
                    AnimationUtility.SetEditorCurve(animClip, curveBinding, null);
                }
            }
            CompressAnimationClip(animClip);
        }
    }

    //压缩精度
    public static void CompressAnimationClip(AnimationClip _clip)
    {
        AnimationClipCurveData[] tCurveArr = AnimationUtility.GetAllCurves(_clip);
        Keyframe tKey;
        Keyframe[] tKeyFrameArr;
        for (int i = 0; i < tCurveArr.Length; ++i)
        {
            AnimationClipCurveData tCurveData = tCurveArr[i];
            if (tCurveData.curve == null || tCurveData.curve.keys == null)
            {
                continue;
            }
            tKeyFrameArr = tCurveData.curve.keys;
            for (int j = 0; j < tKeyFrameArr.Length; j++)
            {
                tKey = tKeyFrameArr[j];
                tKey.value = float.Parse(tKey.value.ToString("f3"));    //#.###
                tKey.inTangent = float.Parse(tKey.inTangent.ToString("f3"));
                tKey.outTangent = float.Parse(tKey.outTangent.ToString("f3"));
                tKeyFrameArr[j] = tKey;
            }
            tCurveData.curve.keys = tKeyFrameArr;
            _clip.SetCurve(tCurveData.path, tCurveData.type, tCurveData.propertyName, tCurveData.curve);
        }
    }
    void OnGUI()
    {
        GUILayout.BeginVertical();
        AssetBundleConvertToAsset();
        BuildAssetBundle();
        GUILayout.EndVertical();
    }

    void AssetBundleConvertToAsset()
    {
        GUILayout.Label("======================AssetBundle资源提取======================");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("选择文件夹", GUILayout.Width(100)))
        {
            
            string folderPath = EditorUtility.OpenFolderPanel("选择文件夹", mSelectPath, string.Empty);
            if (!string.IsNullOrEmpty(folderPath))
            {
                if (!folderPath.Equals(mSelectPath))
                {
                    mSelectPath = folderPath;
                    List<string> files = new List<string>();
                    CommonUtils.GetFiles(mSelectPath, files);
                    string tmpPath;
                    for (int i = 0, imax = files.Count; i < imax; ++i)
                    {
                        tmpPath = files[i].Replace("\\", "/");
                        if (tmpPath.Contains("clip/editor") && tmpPath.EndsWith(".assetbundle"))
                        {
                            if (null == mAssetBundelDict)
                            {
                                mAssetBundelDict = new Dictionary<string, AssetBundleData>();
                            }
                            AssetBundleData data = new AssetBundleData();
                            string modelFolder = tmpPath.Substring(0, tmpPath.IndexOf("/clip/"));
                            modelFolder = modelFolder.Substring(modelFolder.LastIndexOf("/") + 1);
                            data.modelName = modelFolder;
                            data.loadPath = tmpPath;
                            mAssetBundelDict.Add(tmpPath, data);
                        }
                    }
                }
            }            
        }
        if (!string.IsNullOrEmpty(mSelectPath))
        {
            EditorGUILayout.LabelField(mSelectPath);
        }
        GUILayout.EndHorizontal();
        if (null != mAssetBundelDict && mAssetBundelDict.Count > 0)
        {
            EditorGUILayout.LabelField("发现动画数量：" + mAssetBundelDict.Count);
            mAssetBundlePosition = EditorGUILayout.BeginScrollView(mAssetBundlePosition, GUILayout.Width(680), GUILayout.Height(400));
            foreach (var item in mAssetBundelDict)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(item.Value.modelName, GUILayout.Width(100));
                switch (item.Value.mLoadState)
                {
                    case LoadState.None:
                        {
                            if (GUILayout.Button("加载", GUILayout.Width(60)))
                            {
                                item.Value.mLoadState = LoadState.Loading;
                                item.Value.LoadAssetBundle();
                            }
                        }
                        break;
                    case LoadState.Loading:
                        {
                            if (GUILayout.Button("加载中", GUILayout.Width(60)))
                            {
                            }
                        }
                        break;
                    case LoadState.Completed:
                        {
                            if (GUILayout.Button("卸载", GUILayout.Width(60)))
                            {
                                item.Value.mLoadState = LoadState.None;
                                if (null != item.Value.clipList)
                                {
                                    for (int i = 0, imax = item.Value.clipList.Count; i < imax; ++i)
                                    {
                                        GameObject.DestroyImmediate(item.Value.clipList[i]);
                                    }
                                    item.Value.clipList.Clear();
                                }
                            }
                        }
                        break;
                }
                if (GUILayout.Button("保存动画", GUILayout.Width(60)))
                {
                    if (item.Value.mLoadState == LoadState.Completed && null != item.Value.clipList)
                    {
                        item.Value.SaveClip();
                    }
                }
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("导出全部动画", GUILayout.Width(120)))
            {
                foreach (var item in mAssetBundelDict)
                {
                    item.Value.LoadAssetBundle();
                    item.Value.SaveClip();
                }
            }
        }
    }

    void BuildAssetBundle()
    {
        GUILayout.Label("======================动画资源打包成AssetBundle======================");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("选择工程中的动画文件夹", GUILayout.Width(200)))
        {
            string folderPath = EditorUtility.OpenFolderPanel("选择文件夹", string.Empty, string.Empty);
            if (!string.IsNullOrEmpty(folderPath))
            {
                if (folderPath.StartsWith(Application.dataPath))
                {
                    mSelectAnimPath = folderPath;
                    mAnimDict = new Dictionary<string, AnimFileData>();
                    AddAnimFileData(folderPath);
                    string[] dirs = Directory.GetDirectories(folderPath);
                    if (null != dirs)
                    {
                        foreach (var item in dirs)
                        {
                            AddAnimFileData(item);
                        }
                    }
                } else
                {
                    EditorUtility.DisplayDialog("错误", "请选择当前工程中的目录", "确定");
                }

            }
        }
        if (!string.IsNullOrEmpty(mSelectAnimPath))
        {
            EditorGUILayout.LabelField(mSelectAnimPath);
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("选择AssetBundle输出路径（选择模型文件夹）", GUILayout.Width(300)))
        {
            string folderPath = EditorUtility.OpenFolderPanel("选择文件夹", string.Empty, string.Empty);
            if (!string.IsNullOrEmpty(folderPath) && ContainsModelFloder(folderPath))
            {
                mAssetBundleOutPath = folderPath;
            } else
            {
                EditorUtility.DisplayDialog("错误", "请选择模型文件夹", "确定");
            }
        }
        if (!string.IsNullOrEmpty(mAssetBundleOutPath))
        {
            EditorGUILayout.LabelField(mAssetBundleOutPath);
        }
        GUILayout.EndHorizontal();
        if (null != mAnimDict && mAnimDict.Count > 0)
        {
            EditorGUILayout.LabelField("发现动画数量：" + mAnimDict.Count);
            mAnimPosition = EditorGUILayout.BeginScrollView(mAnimPosition, GUILayout.Width(680), GUILayout.Height(400));
            foreach (var item in mAnimDict)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(item.Value.modelName, GUILayout.Width(100));
                switch (item.Value.loadState)
                {
                    case LoadState.None:
                        {
                            if (GUILayout.Button("加载", GUILayout.Width(60)))
                            {
                                item.Value.loadState = LoadState.Loading;
                                item.Value.LoadClip();
                            }
                        }
                        break;
                    case LoadState.Loading:
                        {
                            if (GUILayout.Button("加载中", GUILayout.Width(60)))
                            {
                            }
                        }
                        break;
                    case LoadState.Completed:
                        {
                            if (GUILayout.Button("卸载", GUILayout.Width(60)))
                            {
                                item.Value.loadState = LoadState.None;
                                if (null != item.Value.animationClip)
                                {
                                    for (int i = 0, imax = item.Value.animationClip.Length; i < imax; ++i)
                                    {
                                        GameObject.DestroyImmediate(item.Value.animationClip[i]);
                                    }
                                    item.Value.animationClip = null;
                                }
                            }
                        }
                        break;
                }
                if (GUILayout.Button("打包-Editor", GUILayout.Width(100)))
                {
                    if (item.Value.loadState == LoadState.Completed && null != item.Value.animationClip)
                    {
                        item.Value.BuildAsset(mAssetBundleOutPath, BuildTarget.StandaloneWindows64);
                    }
                }
                if (GUILayout.Button("打包-Android", GUILayout.Width(100)))
                {
                    if (item.Value.loadState == LoadState.Completed && null != item.Value.animationClip)
                    {
                        item.Value.BuildAsset(mAssetBundleOutPath, BuildTarget.Android);
                    }
                }
                if (GUILayout.Button("打包-IOS", GUILayout.Width(100)))
                {
                    if (item.Value.loadState == LoadState.Completed && null != item.Value.animationClip)
                    {
                        item.Value.BuildAsset(mAssetBundleOutPath, BuildTarget.iOS);
                    }
                }
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("打包全部动画-Editor", GUILayout.Width(200)))
            {
                foreach (var item in mAnimDict)
                {
                    item.Value.LoadClip();
                    item.Value.BuildAsset(mAssetBundleOutPath, BuildTarget.StandaloneWindows64);
                }
            }
            if (GUILayout.Button("打包全部动画-Android", GUILayout.Width(200)))
            {
                foreach (var item in mAnimDict)
                {
                    item.Value.LoadClip();
                    item.Value.BuildAsset(mAssetBundleOutPath, BuildTarget.Android);
                }
            }
            if (GUILayout.Button("打包全部动画-IOS", GUILayout.Width(200)))
            {
                foreach (var item in mAnimDict)
                {
                    item.Value.LoadClip();
                    item.Value.BuildAsset(mAssetBundleOutPath, BuildTarget.iOS);
                }
            }
            if (GUILayout.Button("打包所有平台动画", GUILayout.Width(200)))
            {
                foreach (var item in mAnimDict)
                {
                    item.Value.LoadClip();
                    item.Value.BuildAsset(mAssetBundleOutPath, BuildTarget.StandaloneWindows64);
                }
                foreach (var item in mAnimDict)
                {
                    item.Value.LoadClip();
                    item.Value.BuildAsset(mAssetBundleOutPath, BuildTarget.Android);
                }
                foreach (var item in mAnimDict)
                {
                    item.Value.LoadClip();
                    item.Value.BuildAsset(mAssetBundleOutPath, BuildTarget.iOS);
                }
            }
        }
    }

    void AddAnimFileData(string path)
    {
        string[] files = Directory.GetFiles(path);
        if (null != files && files.Length > 0)
        {
            path = CommonUtils.ConvertSlashPath(path);
            AnimFileData data = new AnimFileData();
            data.modelName = CommonUtils.GetFolderName(path);
            data.loadState = LoadState.None;
            for (int i = 0, imax = files.Length; i < imax; ++i)
            {
                if (files[i].EndsWith(".anim"))
                {
                    if (data.loadPath == null)
                    {
                        data.loadPath = new List<string>();
                    }
                    data.loadPath.Add("Assets/" + CommonUtils.ConvertSlashPath(files[i]).Substring(CommonUtils.ConvertSlashPath(Application.dataPath).Length + 1));
                }
            }
            if (null != data.loadPath)
            {
                mAnimDict.Add(path, data);
            }
        }
    }

    bool ContainsModelFloder(string path)
    {
        if (IsModelFloder(path))
        {
            return true;
        }
        string[] dirs = Directory.GetDirectories(path);
        if (null != dirs)
        {
            foreach (var item in dirs)
            {
                if (IsModelFloder(item))
                {
                    return true;
                }
            }
        }
        return false;
    }

    bool IsModelFloder(string path)
    {
        string modelName = CommonUtils.GetFolderName(path);
        if (File.Exists(CommonUtils.GetFolderPath(path) + "/" + modelName + ".xml"))
        {
            return true;
        }
        return false;
    }

    private class AssetBundleData
    {
        public string modelName;
        public LoadState mLoadState;
        public string loadPath;
        public List<AnimationClip> clipList;

        public AssetBundleData()
        {

        }

        public void LoadAssetBundle()
        {
            AssetBundle bundle = AssetBundle.LoadFromFile(loadPath);
            if (null != bundle)
            {
                Debug.Log(string.Join(",", bundle.GetAllAssetNames()));
                clipList = new List<AnimationClip>();
                for (int i = 0, imax = bundle.GetAllAssetNames().Length; i < imax; ++i)
                {
                    UnityEngine.Object t = bundle.LoadAsset(bundle.GetAllAssetNames()[i]);
                    AnimationClip clipTemp = GameObject.Instantiate(t) as AnimationClip;
                    clipTemp.name = bundle.GetAllAssetNames()[i];
                    clipList.Add(clipTemp);
                }
                mLoadState = LoadState.Completed;
                bundle.Unload(true);
            }
        }

        public void SaveClip()
        {
            string path = "modelClip/" + modelName;
            if (!Directory.Exists(Application.dataPath + "/" + path))
            {
                Directory.CreateDirectory(Application.dataPath + "/" + path);
            }
            if (clipList != null && clipList.Count > 0)
            {
                for (int i = 0, imax = clipList.Count; i < imax; ++i)
                {
                    string savePath = "Assets/" + path + "/" + clipList[i].name + ".anim";
                    Debug.Log("保存动画:" + savePath);
                    //CompressAnimationClip(clipList[i]);
                    AssetDatabase.CreateAsset(clipList[i], savePath);
                    AssetDatabase.SaveAssets();
                    Resources.UnloadAsset(clipList[i]);
                }
                clipList.Clear();
            }
            mLoadState = LoadState.None;
        }
    }

    private enum LoadState : byte
    {
        None = 0,
        Loading,
        Completed
    }

    private class AnimFileData
    {
        public string modelName;
        public LoadState loadState;
        public AnimationClip[] animationClip;
        public List<string> loadPath;
        public string robotId;

        public AnimFileData()
        {

        }

        public void LoadClip()
        {
            if (null != loadPath)
            {
                animationClip = new AnimationClip[loadPath.Count];
                for (int i = 0, imax = loadPath.Count; i < imax; ++i)
                {
                    Debug.Log(loadPath[i]);
                    animationClip[i] = AssetDatabase.LoadAssetAtPath(loadPath[i], typeof(AnimationClip)) as AnimationClip;
                }
            }
            loadState = LoadState.Completed;
        }

        public void BuildAsset(string outPath, BuildTarget target)
        {
            if (!modelName.Equals(CommonUtils.GetFolderName(outPath)))
            {//输出路径不是单个模型的文件夹需加上模型名称
                outPath += "/" + modelName;
            }
            if (string.IsNullOrEmpty(robotId))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(outPath + "/" + modelName + ".xml");
                XmlElement xmlRoot = xmlDoc.DocumentElement;
                XmlNode node = xmlRoot.FirstChild;
                while (null != node)
                {
                    if (!(node is XmlElement))
                    {
                        continue;
                    }
                    XmlElement xe = (XmlElement)node;
                    if ("RobotID".Equals(xe.Name))
                    {
                        robotId = xe.InnerText;
                        break;
                    }
                }
            }
            if (string.IsNullOrEmpty(robotId))
            {
                Debug.LogError(modelName + "未找到输出目录");
            } else
            {
                outPath = outPath.Replace(modelName, "AssetBundle/" + modelName);
                string path = outPath + "/clip2018";
                if (target == BuildTarget.StandaloneWindows64)
                {
                    path += "/editor";
                }
                else if (target == BuildTarget.Android)
                {
                    path += "/android";
                }
                else if (target == BuildTarget.iOS)
                {
                    path += "/ios";
                }
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                BuildPipeline.BuildAssetBundle(null, animationClip, path + "/" + robotId + ".assetbundle", BuildAssetBundleOptions.CollectDependencies, target);
                foreach (var item in animationClip)
                {
                    Resources.UnloadAsset(item);
                }
                animationClip = null;
                loadState = LoadState.None;
            }
            
        }
    }
}