using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ETC1SeperateRGBA
{
    static string getPath_NGUIAtlas()
    {
        return Application.dataPath + "/Textures/Atlas";
    }

    [MenuItem("MyTool/分离RGBA/分离NGUI图片通道")]
    static void SeperateNGUI_TexturesRGBandAlphaChannel()
    {
        Debug.Log("分离Alpha通道 Start.");
        string[] paths = Directory.GetFiles(getPath_NGUIAtlas(), "*.*", SearchOption.AllDirectories);
        foreach (string path in paths)
        {
            if (!string.IsNullOrEmpty(path) && !IsIgnorePath(path) && IsTextureFile(path) && !IsTextureConverted(path))   //full name  
            {
                //Debug.Log("path:" + path);  
                SeperateRGBAandlphaChannel(path);
            }
        }
        ReImportAsset();
        Debug.Log("分离Alpha通道 Finish.");
    }

    [MenuItem("MyTool/分离RGBA/检查图片格式")]
    public static void CheckAtlasImageFormat()
    {
        ETC1SeperateRGBA.SetImageFormat(TextureImporterFormat.AutomaticCompressed);return;
        string[] paths = Directory.GetFiles(getPath_NGUIAtlas(), "*_ETC_RGB.png", SearchOption.AllDirectories);
        foreach (var tmp in paths)
        {
            string path = tmp.Replace("\\", "/");
            path = path.Substring(path.IndexOf("Assets/"));
            Texture2D tex2d = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
            if (null != tex2d)
            {
                bool needChange = false;
                if (path.Contains("UIBuild_ETC_RGB"))
                {
                    if (tex2d.format != TextureFormat.RGB565)
                    {
                        needChange = true;
                    }
                }
                else
                {
                    if (tex2d.format != TextureFormat.RGB24)
                    {
                        needChange = true;
                    }
                }
                if (needChange)
                {
                    ReImport_Addlist(tmp, tex2d.width, tex2d.height);
                }
            }
        }
        ReImportAsset();
    }


    public static void SetImageFormat(TextureImporterFormat format)
    {
        string[] paths = Directory.GetFiles(getPath_NGUIAtlas(), "*_ETC_RGB.png", SearchOption.AllDirectories);
        foreach (var tmp in paths)
        {
            string path = tmp.Replace("\\", "/");
            path = path.Substring(path.IndexOf("Assets/"));
            Texture2D tex2d = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
            if (null != tex2d)
            {
                bool needChange = true;
                /*if (tex2d.format != (TextureFormat)format)
                {
                    needChange = true;
                }*/
                if (needChange)
                {
                    ReImport_Addlist(tmp, tex2d.width, tex2d.height);
                }
            }
        }
        ReImportAsset(format);
    }

    [MenuItem("MyTool/分离RGBA/改变NGUI材质")]
    static void ChangeNGUI_MaterialtoETC1()
    {
        //CalculateTexturesAlphaChannelDic();  
        string[] matpaths = Directory.GetFiles(getPath_NGUIAtlas(), "*.mat", SearchOption.AllDirectories);
        foreach (string matpath in matpaths)
        {
            string propermatpath = GetRelativeAssetPath(matpath);
            Material mat = (Material)AssetDatabase.LoadAssetAtPath(propermatpath, typeof(Material));
            if (mat != null)
            {
                ChangMaterial(mat, getPath_NGUIAtlas());
            }
            else
            {
                Debug.LogError("Load material failed : " + matpath);
            }
        }
        Debug.Log("材质改变完成 Finish!");
    }

    [MenuItem("MyTool/分离RGBA/分离选中NGUI图片通道")]
    static void SeperateSelectedNGUI_TexturesRGBandAlphaChannel()
    {
        Debug.Log("分离Alpha通道 Start.");
        Object[] args = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);
        string applicationPath = Application.dataPath;
        if (null != args && args.Length > 0)
        {
            for (int i = 0, imax = args.Length; i < imax; ++i)
            {
                if (args[i] is Texture2D)
                {
                    Texture2D tex2d = (Texture2D)args[i];
                    string texPath = AssetDatabase.GetAssetPath(tex2d);
                    SetTextureReadable(texPath);
                    tex2d = (Texture2D)AssetDatabase.LoadAssetAtPath(texPath, typeof(Texture2D));
                    string fullPath = applicationPath + texPath.Substring("Assets".Length);
                    if (!fullPath.Contains("_ETC_"))
                    {
                        WriteTexture(tex2d, fullPath);
                    }
                }
            }
        }
        ReImportAsset();
        Debug.Log("分离Alpha通道 Finish.");
    }


#region 分离 RGB & A  
    /// <summary>  
    /// 分离图片通道  
    /// </summary>  
    /// <param name="_texPath"></param>  
    static void SeperateRGBAandlphaChannel(string _texPath)
    {
        //获得相对路径  
        string assetRelativePath = GetRelativeAssetPath(_texPath);
        SetTextureReadable(assetRelativePath);
        Texture2D sourcetex = AssetDatabase.LoadAssetAtPath(assetRelativePath, typeof(Texture2D)) as Texture2D;  //not just the textures under Resources file  
        if (!sourcetex)
        {
            Debug.Log("读取图片失败 : " + assetRelativePath);
            return;
        }
        WriteTexture(sourcetex, _texPath);
    }

    static void WriteTexture(Texture2D sourcetex, string _texPath)
    {
        /*进化版本*/
        Color[] colors = sourcetex.GetPixels();
        Texture2D rgbTex2 = new Texture2D(sourcetex.width, sourcetex.height, TextureFormat.RGB24, false);
        rgbTex2.SetPixels(colors);
        rgbTex2.Apply();
        string strPath_RGB = GetRGBTexPath(_texPath);
        File.WriteAllBytes(strPath_RGB, rgbTex2.EncodeToPNG());
        ReImport_Addlist(strPath_RGB, rgbTex2.width, rgbTex2.height);

        Texture2D alphaTex2 = new Texture2D(sourcetex.width, sourcetex.height, TextureFormat.RGB24, false);
        Color[] alphacolors = new Color[colors.Length];
        for (int i = 0; i < colors.Length; ++i)
        {
            alphacolors[i].r = colors[i].a;
            alphacolors[i].g = colors[i].a;
            alphacolors[i].b = colors[i].a;
        }
        alphaTex2.SetPixels(alphacolors);
        alphaTex2.Apply();
        string strPath_Alpha = GetAlphaTexPath(_texPath);
        File.WriteAllBytes(strPath_Alpha, alphaTex2.EncodeToPNG());
        ReImport_Addlist(strPath_Alpha, alphaTex2.width, alphaTex2.height);
    }

    /// <summary>  
    /// 设置图片为可读格式  
    /// </summary>  
    /// <param name="_relativeAssetPath"></param>  
    static void SetTextureReadable(string _relativeAssetPath)
    {
        string postfix = GetFilePostfix(_relativeAssetPath);
        if (postfix == ".dds")    // no need to set .dds file.  Using TextureImporter to .dds file would get casting type error.  
        {
            return;
        }

        TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(_relativeAssetPath);
        ti.isReadable = true;
        AssetDatabase.ImportAsset(_relativeAssetPath);
    }

    static Dictionary<string, int[]> ReImportList = new Dictionary<string, int[]>();
    static void ReImport_Addlist(string path, int width, int height)
    {
        ReImportList.Add(path, new int[] { width, height });
    }

    /// <summary>  
    /// 设置图片格式  
    /// </summary>  
    static void ReImportAsset(TextureImporterFormat format = TextureImporterFormat.AutomaticCompressed)
    {
        foreach (var item in ReImportList)
        {
            TextureImporter importer = null;
            string assetpath = GetRelativeAssetPath(item.Key);
            try
            {
                importer = (TextureImporter)TextureImporter.GetAtPath(assetpath);
            }
            catch
            {
                Debug.LogError("Load Texture failed: " + assetpath);
                return;
            }

            if (importer == null)
            {
                Debug.Log("importer null:" + assetpath);
                return;
            }
            bool changeFlag = false;
            if (TextureImporterType.Default != importer.textureType)
            {
                changeFlag = true;
                importer.textureType = TextureImporterType.Default;
            }
            if (importer.isReadable)
            {
                changeFlag = true;
                importer.isReadable = false;  //increase memory cost if readable is true    
            }
            if (importer.mipmapEnabled)
            {
                changeFlag = true;
                importer.mipmapEnabled = false;
            }
            if (importer.wrapMode != TextureWrapMode.Clamp)
            {
                changeFlag = true;
                importer.wrapMode = TextureWrapMode.Clamp;
            }
            if (importer.filterMode != FilterMode.Bilinear)
            {
                changeFlag = true;
                importer.filterMode = FilterMode.Bilinear;
            }
            if (importer.anisoLevel != 1)
            {
                changeFlag = true;
                importer.anisoLevel = 1;
            }
            importer.maxTextureSize = Mathf.Max(item.Value[0], item.Value[1]);
            if (item.Key.Contains("ETC_Alpha"))
            {
                if (importer.textureFormat != TextureImporterFormat.AutomaticCompressed)
                {
                    changeFlag = true;
                    importer.textureFormat = TextureImporterFormat.AutomaticCompressed;
                }
            } else if (item.Key.Contains("actionEditAtlas_ETC_RGB") || item.Key.Contains("connectAtlas_ETC_RGB") || item.Key.Contains("publicAtlas_ETC_RGB"))
            {
                if (importer.textureFormat != TextureImporterFormat.RGB16)
                {
                    changeFlag = true;
                    importer.textureFormat = TextureImporterFormat.RGB16;
                }
            }
             else
            {
                //importer.textureFormat = TextureImporterFormat.RGB24;
                if (importer.textureFormat != format)
                {
                    changeFlag = true;
                    importer.textureFormat = format;
                }
            }
            if (changeFlag)
            {
                AssetDatabase.ImportAsset(assetpath);
            }
        }
        ReImportList.Clear();
        AssetDatabase.Refresh();
    }
#endregion

#region 材质  
    static void ChangMaterial(Material _mat, string _texPath)
    {
        Shader shader;
        switch (_mat.shader.name)
        {
            case "Unlit/Transparent Colored":
                {
                    shader = Shader.Find("Unlit/Transparent Colored ETC");
                }
                break;
            default:
                return;
        }

        string[] mainPath = Directory.GetFiles(_texPath, _mat.mainTexture.name + "_ETC_RGB.png", SearchOption.AllDirectories);
        Texture mainTex = AssetDatabase.LoadAssetAtPath(GetRelativeAssetPath(mainPath[0]), typeof(Texture)) as Texture;
        string[] alphaPath = Directory.GetFiles(_texPath, _mat.mainTexture.name + "_ETC_Alpha.png", SearchOption.AllDirectories);
        Texture alphaTex = AssetDatabase.LoadAssetAtPath(GetRelativeAssetPath(alphaPath[0]), typeof(Texture)) as Texture;

        _mat.shader = shader;
        _mat.SetTexture("_MainTex", mainTex);
        _mat.SetTexture("_MainTex_A", alphaTex);
    }
#endregion

#region Path or 后缀  
    /// <summary>  
    /// 获得相对路径  
    /// </summary>  
    /// <param name="_fullPath"></param>  
    /// <returns></returns>  
    static string GetRelativeAssetPath(string _fullPath)
    {
        _fullPath = GetRightFormatPath(_fullPath);
        int idx = _fullPath.IndexOf("Assets");
        string assetRelativePath = _fullPath.Substring(idx);
        return assetRelativePath;
    }

    /// <summary>  
    /// 转换斜杠  
    /// </summary>  
    /// <param name="_path"></param>  
    /// <returns></returns>  
    static string GetRightFormatPath(string _path)
    {
        return _path.Replace("\\", "/");
    }

    /// <summary>  
    /// 获取文件后缀  
    /// </summary>  
    /// <param name="_filepath"></param>  
    /// <returns></returns>  
    static string GetFilePostfix(string _filepath)   //including '.' eg ".tga", ".dds"  
    {
        string postfix = "";
        int idx = _filepath.LastIndexOf('.');
        if (idx > 0 && idx < _filepath.Length)
            postfix = _filepath.Substring(idx, _filepath.Length - idx);
        return postfix;
    }

    static bool IsIgnorePath(string _path)
    {
        return _path.Contains("\\UI\\");
    }

    /// <summary>  
    /// 是否为图片  
    /// </summary>  
    /// <param name="_path"></param>  
    /// <returns></returns>  
    static bool IsTextureFile(string _path)
    {
        string path = _path.ToLower();
        return path.EndsWith(".psd") || path.EndsWith(".tga") || path.EndsWith(".png") || path.EndsWith(".jpg") || path.EndsWith(".dds") || path.EndsWith(".bmp") || path.EndsWith(".tif") || path.EndsWith(".gif");
    }

    /// <summary>  
    /// 是否为自动生成的ETC图片  
    /// </summary>  
    /// <param name="_path"></param>  
    /// <returns></returns>  
    static bool IsTextureConverted(string _path)
    {
        return _path.Contains("_ETC_RGB.") || _path.Contains("_ETC_Alpha.");
    }

    static string GetRGBTexPath(string _texPath)
    {
        return GetTexPath(_texPath, "_ETC_RGB.");
    }

    static string GetAlphaTexPath(string _texPath)
    {
        return GetTexPath(_texPath, "_ETC_Alpha.");
    }

    static string GetTexPath(string _texPath, string _texRole)
    {
        string result = _texPath.Replace(".", _texRole);
        string postfix = GetFilePostfix(_texPath);
        return result.Replace(postfix, ".png");
    }
#endregion
}