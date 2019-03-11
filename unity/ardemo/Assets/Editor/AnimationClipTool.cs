using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class AnimationClipTool
{
    [MenuItem("AnimationClip/GetFilteredtoAnim &1", true)]
    static bool NotGetFiltered()
    {
        return Selection.activeObject;
    }

    [MenuItem("AnimationClip/GetFilteredtoAnim &1")]
    static void GetFiltered()
    {
        string targetPath = Application.dataPath + "/AnimationClip";
        if (!Directory.Exists(targetPath))
        {
            Directory.CreateDirectory(targetPath);
        }
        Object[] SelectionAsset = Selection.GetFiltered(typeof(Object), SelectionMode.Unfiltered);
        Debug.Log(SelectionAsset.Length);
        foreach (Object Asset in SelectionAsset)
        {
            if (Asset is AnimationClip)
            {
                AnimationClip newClip = new AnimationClip();
                EditorUtility.CopySerialized(Asset, newClip);
                AssetDatabase.CreateAsset(newClip, "Assets/AnimationClip/" + Asset.name + ".anim");
            }

        }
        AssetDatabase.Refresh();
    }
}