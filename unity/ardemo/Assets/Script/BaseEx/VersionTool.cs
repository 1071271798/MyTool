
using System.Collections;
using UnityEngine;
using Game.Platform;
using System.Text;
using System.IO;

public class VersionTool : SingletonObject<VersionTool>
{
    private string version;
    private int[] versions;
    private string fileName = "version.txt";
    private string filePath = Application.dataPath + "/Resources/";


    public VersionTool()
    {
        LoadVersionText();
    }

    public string GetVersion()
    {
        return version;
    }


    public void AddLowVersion()
    {
        if (null != versions && versions.Length == 3)
        {
            versions[2]++;
            version = ConvertToVersion(versions);
            SaveText();
        }
    }

    public void AddMidVersion()
    {
        if (null != versions && versions.Length == 3)
        {
            versions[1]++;
            versions[2] = 0;
            version = ConvertToVersion(versions);
            SaveText();
        }
    }

    public void AddHightVersion()
    {
        if (null != versions && versions.Length == 3)
        {
            versions[0]++;
            versions[1] = 0;
            versions[2] = 0;
            version = ConvertToVersion(versions);
            SaveText();
        }
    }


    void SaveText()
    {
        try
        {
            File.WriteAllText(filePath + fileName, version);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }


    string ConvertToVersion(int[] ary)
    {
        StringBuilder tmp = new StringBuilder();
        for (int i = 0, imax = ary.Length; i < imax; ++i)
        {
            if (tmp.Length > 0)
            {
                tmp.Append('.');
            }
            tmp.Append(ary[i]);
        }
        return tmp.ToString();
    }

    void LoadVersionText()
    {
        try
        {
            TextAsset text = Resources.Load("version", typeof(TextAsset)) as TextAsset;
            if (null != text)
            {
                version = text.text.Trim().Replace("\r", "").Replace("\n", "");
                string[] ary = version.Split('.');
                versions = new int[ary.Length];
                int num = 0;
                for (int i = 0, imax = ary.Length; i < imax; ++i)
                {
                    if (int.TryParse(ary[i], out num))
                    {
                        versions[i] = num;
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}

