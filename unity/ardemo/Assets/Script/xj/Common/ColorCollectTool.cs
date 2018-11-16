using Game.Resource;
using System.Collections.Generic;
using System.Text;
using Game;
using System.IO;
using LitJson;
using System;
using Game.Platform;
/// <summary>
/// 颜色采集工具
/// </summary>
public class ColorCollectTool : SingletonObject<ColorCollectTool>
{
    string mFilePath = ResourcesEx.persistentDataPath + "/color/color.txt";

    Dictionary<string, List<ColorCollectData>> mSuccessDict;
    Dictionary<string, List<ColorCollectData>> mFailedDict;

    Dictionary<string, int> mNowSuccessDict;
    Dictionary<string, int> mNowFailedDict;

    string mCollectColor;
    long mCollectIndex = -1;

    public ColorCollectTool()
    {
        mSuccessDict = new Dictionary<string, List<ColorCollectData>>();
        mFailedDict = new Dictionary<string, List<ColorCollectData>>();
        mNowSuccessDict = new Dictionary<string, int>();
        mNowFailedDict = new Dictionary<string, int>();
        ReadText();
    }

    public void StartCollect(string color)
    {
        StopCollect();
        this.mCollectColor = color;
        Robot robot = RobotManager.GetInst().GetCurrentRobot();
        if (null != robot)
        {
            mCollectIndex = Timer.Add(0, 0.5f, 0, delegate ()
            {
                robot.ReadAllSensorData();
            });
        }
        
    }

    public void StopCollect()
    {
        if (-1 != mCollectIndex)
        {
            Timer.Cancel(mCollectIndex);
            mCollectIndex = -1;
        }
        this.mCollectColor = null;
        SaveText();
    }

    public void AddColor(byte r, byte g, byte b, float[] hsv, string convertColor)
    {
        string key = this.mCollectColor;
        if (string.IsNullOrEmpty(key))
        {
            key = convertColor;
        }
        ColorCollectData data = new ColorCollectData(r, g, b, key, hsv[0], hsv[1], hsv[2], convertColor);
        AddColorData(data, key);
    }

    public void RestoreReadText()
    {
        mSuccessDict.Clear();
        mFailedDict.Clear();
        ReadText();
    }

    public void ApplicationQuit()
    {
        SaveText();
    }

    public void RestoreCalculation()
    {
        Dictionary<string, List<ColorCollectData>> successDict = new Dictionary<string, List<ColorCollectData>>();
        Dictionary<string, List<ColorCollectData>> failedDict = new Dictionary<string, List<ColorCollectData>>();
        foreach (var kvp in mSuccessDict)
        {
            List<ColorCollectData> list = kvp.Value;
            for (int i = 0, imax = list.Count; i < imax; ++i)
            {
                ColorCollectData data = list[i];
                data.convertColor = PublicFunction.ConverRgtToColor(data.r, data.g, data.b, false);
                if (!data.IsSuccess())
                {
                    if (!failedDict.ContainsKey(data.targetColor))
                    {
                        failedDict[data.targetColor] = new List<ColorCollectData>();
                    }
                    failedDict[data.targetColor].Add(data);
                }
            }
        }
        foreach (var kvp in mFailedDict)
        {
            List<ColorCollectData> list = kvp.Value;
            for (int i = 0, imax = list.Count; i < imax; ++i)
            {
                ColorCollectData data = list[i];
                data.convertColor = PublicFunction.ConverRgtToColor(data.r, data.g, data.b, false);
                if (data.IsSuccess())
                {
                    if (!successDict.ContainsKey(data.targetColor))
                    {
                        successDict[data.targetColor] = new List<ColorCollectData>();
                    }
                    successDict[data.targetColor].Add(data);
                }
            }
        }
        foreach (var kvp in successDict)
        {
            mNowSuccessDict[kvp.Key] = 0;
            mNowFailedDict[kvp.Key] = 0;
            for (int i = 0, imax = kvp.Value.Count; i < imax; ++i)
            {
                AddColorData(kvp.Value[i], kvp.Key);
                if (mFailedDict.ContainsKey(kvp.Key))
                {
                    mFailedDict[kvp.Key].Remove(kvp.Value[i]);
                    if (mFailedDict[kvp.Key].Count < 1)
                    {
                        mFailedDict.Remove(kvp.Key);
                        mNowFailedDict.Remove(kvp.Key);
                    }
                }
            }
        }
        foreach (var kvp in failedDict)
        {
            mNowSuccessDict[kvp.Key] = 0;
            mNowFailedDict[kvp.Key] = 0;
            for (int i = 0, imax = kvp.Value.Count; i < imax; ++i)
            {
                AddColorData(kvp.Value[i], kvp.Key);
                if (mSuccessDict.ContainsKey(kvp.Key))
                {
                    mSuccessDict[kvp.Key].Remove(kvp.Value[i]);
                    if (mSuccessDict[kvp.Key].Count < 1)
                    {
                        mSuccessDict.Remove(kvp.Key);
                        mNowSuccessDict.Remove(kvp.Key);
                    }
                }
            }
        }
    }

    public Dictionary<string, Int2> GetColorResult()
    {
        Dictionary<string, Int2> dict = new Dictionary<string, Int2>();
        foreach (var kvp in mSuccessDict)
        {
            Int2 result;
            List<ColorCollectData> list = kvp.Value;
            result.num1 = list.Count;
            result.num2 = list.Count;
            dict[kvp.Key] = result;
        }
        foreach (var kvp in mFailedDict)
        {
            if (dict.ContainsKey(kvp.Key))
            {
                Int2 result = dict[kvp.Key];
                result.num2 += kvp.Value.Count;
                dict[kvp.Key] = result;
            } else
            {
                Int2 result;
                List<ColorCollectData> list = kvp.Value;
                result.num1 = 0;
                result.num2 = list.Count;
                dict[kvp.Key] = result;
            }
            
        }
        return dict;
    }

    public List<string> GetColorList()
    {
        List<string> list = new List<string>();
        foreach (var kvp in mSuccessDict)
        {
            list.Add(kvp.Key);
        }
        foreach (var kvp in mFailedDict)
        {
            if (!list.Contains(kvp.Key))
            {
                list.Add(kvp.Key);
            }
        }
        return list;
    }

    public bool HaveSuccess(string color)
    {
        if (mSuccessDict.ContainsKey(color))
        {
            return true;
        }
        return false;
    }

    public void MoveSuccessNext(string color)
    {
        if (mNowSuccessDict.ContainsKey(color))
        {
            if (!mNowSuccessDict.ContainsKey(color))
            {
                mNowSuccessDict[color] = 0;
            }
            mNowSuccessDict[color]++;
            if (mNowSuccessDict[color] == mSuccessDict[color].Count)
            {
                mNowSuccessDict[color] = 0;
            }
        }
    }

    public void MoveSuccessFront(string color)
    {
        if (mNowSuccessDict.ContainsKey(color))
        {
            if (!mNowSuccessDict.ContainsKey(color))
            {
                mNowSuccessDict[color] = 0;
            }
            mNowSuccessDict[color]--;
            if (mNowSuccessDict[color] < 0)
            {
                mNowSuccessDict[color] = mSuccessDict[color].Count - 1;
            }
        }
    }

    public ColorCollectData GetNowSuccessColor(string color)
    {
        if (mSuccessDict.ContainsKey(color))
        {
            if (mNowSuccessDict[color] >= 0 && mNowSuccessDict[color] < mSuccessDict[color].Count)
            {
                return mSuccessDict[color][mNowSuccessDict[color]];
            }
        }
        return null;
    }

    public int GetNowSuccessColorIndex(string color)
    {
        if (mNowSuccessDict.ContainsKey(color))
        {
            return mNowSuccessDict[color];
        }
        return -1;
    }

    public bool HaveFailed(string color)
    {
        if (mFailedDict.ContainsKey(color))
        {
            return true;
        }
        return false;
    }

    public void MoveFailedNext(string color)
    {
        if (mNowFailedDict.ContainsKey(color))
        {
            if (!mNowFailedDict.ContainsKey(color))
            {
                mNowFailedDict[color] = 0;
            }
            mNowFailedDict[color]++;
            if (mNowFailedDict[color] == mFailedDict[color].Count)
            {
                mNowFailedDict[color] = 0;
            }
        }
    }

    public void MoveFailedFront(string color)
    {
        if (mNowFailedDict.ContainsKey(color))
        {
            if (!mNowFailedDict.ContainsKey(color))
            {
                mNowFailedDict[color] = 0;
            }
            mNowFailedDict[color]--;
            if (mNowFailedDict[color] < 0)
            {
                mNowFailedDict[color] = mFailedDict[color].Count - 1;
            }
        }
    }

    public ColorCollectData GetNowFailedColor(string color)
    {
        if (mFailedDict.ContainsKey(color))
        {
            if (mNowFailedDict[color] >= 0 && mNowFailedDict[color] < mFailedDict[color].Count)
            {
                return mFailedDict[color][mNowFailedDict[color]];
            }
        }
        return null;
    }

    public int GetNowFailedColorIndex(string color)
    {
        if (mNowFailedDict.ContainsKey(color))
        {
            return mNowFailedDict[color];
        }
        return -1;
    }

    public void ChangeNowSuccessTargetColor(string oldColor, string targetColor)
    {
        if (string.Equals(oldColor, targetColor))
        {
            return;
        }
        if (mSuccessDict.ContainsKey(oldColor) && mNowSuccessDict.ContainsKey(oldColor))
        {
            ColorCollectData data = GetNowSuccessColor(oldColor);
            mSuccessDict[oldColor].RemoveAt(mNowSuccessDict[oldColor]);
            if (mSuccessDict[oldColor].Count < 1)
            {
                mSuccessDict.Remove(oldColor);
                mNowSuccessDict.Remove(oldColor);
            } else
            {
                mNowSuccessDict[oldColor]--;
                if (mNowSuccessDict[oldColor] < 0)
                {
                    mNowSuccessDict[oldColor] = 0;
                }
            }
            data.targetColor = targetColor;
            if (data.IsSuccess())
            {
                if (!mSuccessDict.ContainsKey(targetColor))
                {
                    mSuccessDict[targetColor] = new List<ColorCollectData>();
                    mNowSuccessDict[targetColor] = 0;
                }
                mSuccessDict[targetColor].Add(data);
            }
            else
            {
                if (!mFailedDict.ContainsKey(targetColor))
                {
                    mFailedDict[targetColor] = new List<ColorCollectData>();
                    mNowFailedDict[targetColor] = 0;
                }
                mFailedDict[targetColor].Add(data);
            }
        }
    }

    public void ChangeNowFailedTargetColor(string oldColor, string targetColor)
    {
        if (string.Equals(oldColor, targetColor))
        {
            return;
        }
        if (mFailedDict.ContainsKey(oldColor) && mNowFailedDict.ContainsKey(oldColor))
        {
            ColorCollectData data = GetNowFailedColor(oldColor);
            mFailedDict[oldColor].RemoveAt(mNowFailedDict[oldColor]);
            if (mFailedDict[oldColor].Count < 1)
            {
                mFailedDict.Remove(oldColor);
                mNowFailedDict.Remove(oldColor);
            } else
            {
                mNowFailedDict[oldColor]--;
                if (mNowFailedDict[oldColor] < 0)
                {
                    mNowFailedDict[oldColor] = 0;
                }
            }
            data.targetColor = targetColor;
            if (data.IsSuccess())
            {
                if (!mSuccessDict.ContainsKey(targetColor))
                {
                    mSuccessDict[targetColor] = new List<ColorCollectData>();
                    mNowSuccessDict[targetColor] = 0;
                }
                mSuccessDict[targetColor].Add(data);
            } else
            {
                if (!mFailedDict.ContainsKey(targetColor))
                {
                    mFailedDict[targetColor] = new List<ColorCollectData>();
                    mNowFailedDict[targetColor] = 0;
                }
                mFailedDict[targetColor].Add(data);
            }
        }
    }

    private void AddColorData(ColorCollectData data, string targetColor)
    {
        if (data.IsSuccess())
        {
            if (!mSuccessDict.ContainsKey(targetColor))
            {
                mSuccessDict[targetColor] = new List<ColorCollectData>();
                mNowSuccessDict[targetColor] = 0;
            }
            mSuccessDict[targetColor].Add(data);
        } else
        {
            if (!mFailedDict.ContainsKey(targetColor))
            {
                mFailedDict[targetColor] = new List<ColorCollectData>();
                mNowFailedDict[targetColor] = 0;
            }
            mFailedDict[targetColor].Add(data);
        }
        
    }

    private void SaveText()
    {
        try
        {
            FileInfo fileInfo = new FileInfo(ResourcesEx.persistentDataPath + "/color/color_" + DateTime.Now.Ticks + ".txt");
            StreamWriter sw = new StreamWriter(fileInfo.Open(FileMode.OpenOrCreate));
            sw.WriteLine("==================================success========================================");
            foreach (var kvp in mSuccessDict)
            {
                sw.WriteLine();
                sw.WriteLine(kvp.Key);
                sw.WriteLine();
                for (int i = 0, imax = kvp.Value.Count; i < imax; ++i)
                {
                    sw.WriteLine(kvp.Value[i].ToString());
                }
            }
            sw.WriteLine();
            sw.WriteLine("===================================failed=====================================");
            foreach (var kvp in mFailedDict)
            {
                sw.WriteLine();
                sw.WriteLine(kvp.Key);
                sw.WriteLine();
                for (int i = 0, imax = kvp.Value.Count; i < imax; ++i)
                {
                    sw.WriteLine(kvp.Value[i].ToString());
                }
            }
            sw.Flush();
            sw.Dispose();
            sw.Close();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    private void ReadText()
    {
        try
        {
            if (!File.Exists(mFilePath))
            {
                return;
            }
            FileInfo fileInfo = new FileInfo(mFilePath);
            StreamReader sr = new StreamReader(fileInfo.Open(FileMode.Open));
            string readStr = null;
            mSuccessDict.Clear();
            while (null != (readStr = sr.ReadLine()))
            {
                if (!string.IsNullOrEmpty(readStr) && readStr.StartsWith("{"))
                {
                    ColorCollectData data = new ColorCollectData(readStr);
                    AddColorData(data, data.targetColor);
                }
            }
            sr.Dispose();
            sr.Close();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
}


public class ColorCollectData
{
    public byte r;
    public byte g;
    public byte b;
    public string convertColor;
    public string targetColor;
    public float h;
    public float s;
    public float v;

    public ColorCollectData(byte r, byte g, byte b, string targetColor, float h, float s, float v, string convertColor)
    {
        this.r = r;
        this.g = g;
        this.b = b;
        this.targetColor = targetColor;
        this.h = h;
        this.s = s;
        this.v = v;
        this.convertColor = convertColor;
    }

    public ColorCollectData(string data)
    {
        Dictionary<string, object> dict = (Dictionary<string, object>)Json.Deserialize(data);
        this.r = byte.Parse(dict["r"].ToString());
        this.g = byte.Parse(dict["g"].ToString());
        this.b = byte.Parse(dict["b"].ToString());
        this.h = float.Parse(dict["h"].ToString());
        this.s = float.Parse(dict["s"].ToString());
        this.v = float.Parse(dict["v"].ToString());
        this.targetColor = dict["targetColor"].ToString();
        this.convertColor = dict["convertColor"].ToString();
    }

    public bool IsSuccess()
    {
        if (!string.IsNullOrEmpty(targetColor) && targetColor.Equals(convertColor))
        {
            return true;
        }
        return false;
    }

    public override string ToString()
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict["r"] = r;
        dict["g"] = g;
        dict["b"] = b;
        dict["h"] = h;
        dict["s"] = s;
        dict["v"] = v;
        dict["targetColor"] = targetColor;
        dict["convertColor"] = convertColor;
        return Json.Serialize(dict);
    }
}