using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class MeshTool : EditorWindow
{
    string mFolderPath = Application.dataPath + "/Parts";
    List<GameObject> mPrefabsList = null;
    List<GameObject> mMeshList = null;
    bool mNeedShadowsFlag = false;
    bool mNeedAnimFlag = false;
    ModelImporterMeshCompression mMeshCompression = ModelImporterMeshCompression.Off;
    bool mMeshReadOrWriteFlag = true;
    bool mImportBlendShapes = true;
    /*ModelImporterTangentSpaceMode mNormalsMode = ModelImporterTangentSpaceMode.Import;
    ModelImporterTangentSpaceMode mTangentsMode = ModelImporterTangentSpaceMode.Calculate;
    bool mImportAnimation = true;
    ModelImporterAnimationType mImportAnimationType = ModelImporterAnimationType.Legacy;*/


    [MenuItem("MyTool/Mesh工具", false, 100)]
    public static void OpenMeshEditor()
    {
        MeshTool windows = EditorWindow.GetWindow<MeshTool>(true, "MeshTool");
        windows.position = new Rect(400, 300, 500, 550);
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("选择文件夹", GUILayout.Width(100), GUILayout.Height(20)))
        {
            mFolderPath = EditorUtility.OpenFolderPanel("选择文件夹", mFolderPath, string.Empty);
            mPrefabsList = null;
            mMeshList = null;
            if (!string.IsNullOrEmpty(mFolderPath))
            {
                string path = mFolderPath.Replace('\\', '/');
                if (path.StartsWith(Application.dataPath))
                {
                    path = path.Substring(Application.dataPath.Length);
                    if (path.StartsWith("/"))
                    {
                        path = path.Substring(1);
                    }
                }
            }
        }
        EditorGUILayout.LabelField(mFolderPath);
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("读取资源", GUILayout.Width(100), GUILayout.Height(20)))
        {
            if (string.IsNullOrEmpty(mFolderPath))
            {
                EditorUtility.DisplayDialog("提示", "请选择资源路径", "确定");
                return;
            }
            ReadAssets();
        }
        if (null != mPrefabsList && mPrefabsList.Count > 0)
        {
            EditorGUILayout.LabelField("========================预设处理=======================");
            EditorGUILayout.LabelField("预设数量：" + mPrefabsList.Count);
            mNeedShadowsFlag = EditorGUILayout.Toggle("需要投影", mNeedShadowsFlag);
            mNeedAnimFlag = EditorGUILayout.Toggle("需要动画", mNeedAnimFlag);
            if (GUILayout.Button("更改预设", GUILayout.Width(100), GUILayout.Height(20)))
            {
                foreach (var item in mPrefabsList)
                {
                    GameObject prefab1 = PrefabUtility.InstantiatePrefab(item) as GameObject;
                    MeshRenderer[] renders = prefab1.GetComponentsInChildren<MeshRenderer>(true);
                    if (null != renders)
                    {
                        foreach (var ren in renders)
                        {
                            ren.castShadows = mNeedShadowsFlag;
                            ren.receiveShadows = mNeedShadowsFlag;
                        }
                    }
                    Animation[] anims = prefab1.GetComponentsInChildren<Animation>(true);
                    if (null != anims && !mNeedAnimFlag)
                    {
                        foreach (var anim in anims)
                        {
                            UnityEngine.Object.DestroyImmediate(anim);
                        }
                    }
                    PrefabUtility.ReplacePrefab(prefab1, item);
                    UnityEngine.Object.DestroyImmediate(prefab1);
                    prefab1 = null;
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            
        }
        if (null != mMeshList && mMeshList.Count > 0)
        {
            EditorGUILayout.LabelField("========================FBX处理=======================");
            EditorGUILayout.LabelField("FBX数量：" + mMeshList.Count);
            mMeshCompression = (ModelImporterMeshCompression)EditorGUILayout.EnumPopup("MeshCompression", mMeshCompression);
            mMeshReadOrWriteFlag = EditorGUILayout.Toggle("Read/Write Enabled", mMeshReadOrWriteFlag);
            mImportBlendShapes = EditorGUILayout.Toggle("ImportBlendShapes", mImportBlendShapes);
            /*mNormalsMode = (ModelImporterTangentSpaceMode)EditorGUILayout.EnumPopup("NormalsMode", mNormalsMode);
            mTangentsMode = (ModelImporterTangentSpaceMode)EditorGUILayout.EnumPopup("TangentsMode", mTangentsMode);
            mImportAnimation = EditorGUILayout.Toggle("ImportAnimation", mImportAnimation);
            mImportAnimationType = (ModelImporterAnimationType)EditorGUILayout.EnumPopup("ImportAnimationType", mImportAnimationType);*/
            if (GUILayout.Button("更改FBX", GUILayout.Width(100), GUILayout.Height(20)))
            {
                foreach (var item in mMeshList)
                {
                    string assetPath = AssetDatabase.GetAssetPath(item);
                    ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                    bool importFlag = false;
                    if (importer.meshCompression != mMeshCompression)
                    {
                        importFlag = true;
                        importer.meshCompression = mMeshCompression;
                    }
                    if (importer.isReadable != mMeshReadOrWriteFlag)
                    {
                        importFlag = true;
                        importer.isReadable = mMeshReadOrWriteFlag;
                    }
                    if (importer.importBlendShapes != mImportBlendShapes)
                    {
                        importFlag = true;
                        importer.importBlendShapes = mImportBlendShapes;
                    }
                    /*importer.normalImportMode = mNormalsMode;
                    importer.tangentImportMode = mTangentsMode;
                    importer.importAnimation = mImportAnimation;
                    importer.animationType = mImportAnimationType;*/
                    if (importFlag)
                    {
                        AssetDatabase.ImportAsset(assetPath);
                    }
                }
                AssetDatabase.Refresh();
            }
        }
    }


    void ReadAssets()
    {
        mPrefabsList = new List<GameObject>();
        mMeshList = new List<GameObject>();

        string[] prefabs = Directory.GetFiles(mFolderPath, "*.prefab", SearchOption.AllDirectories);
        if (null != prefabs)
        {
            for (int i = 0, imax = prefabs.Length; i < imax; ++i)
            {
                string path = prefabs[i].Replace("\\", "/");
                path = path.Substring(path.IndexOf("Assets"));
                GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
                MeshRenderer[] render = prefab.GetComponentsInChildren<MeshRenderer>(true);
                if (null != render && render.Length >= 1)
                {
                    mPrefabsList.Add(prefab);
                }
                
            }
        }
        string[] meshAry = Directory.GetFiles(mFolderPath, "*.fbx", SearchOption.AllDirectories);
        if (null != meshAry)
        {
            for (int i = 0, imax = meshAry.Length; i < imax; ++i)
            {
                string path = meshAry[i].Replace("\\", "/");
                path = path.Substring(path.IndexOf("Assets"));
                GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
                mMeshList.Add(prefab);
            }
        }
    }

}