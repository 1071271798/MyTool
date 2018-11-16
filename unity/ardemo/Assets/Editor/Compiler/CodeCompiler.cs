using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.IO;
using UnityEditor;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:CodeCompiler.cs
/// Description:代码编译
/// Time:2016/6/8 10:11:03
/// </summary>
public class CodeCompiler
{
    [MenuItem("MyTool/编译代码")]
    public static void CompileFile()
    {
        UnityEngine.Object[] objs = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets);
        if (null != objs)
        {
            for (int i = 0, imax = objs.Length; i < imax; ++i)
            {
                string path = AssetDatabase.GetAssetPath(objs[i]);
                if (Path.GetExtension(path).Equals(".cs"))
                {
                    path = Application.dataPath + path.Substring("Assets".Length);
                    BuildCode(path);
                }
            }
        }
    }

    /// <summary>
    /// 编译该路径的文件
    /// </summary>
    /// <param name="srcPath"></param>
    /// <returns></returns>
    public static CompilerResults BuildCode(string srcPath)
    {
        if (!Path.GetExtension(srcPath).Equals(".cs"))
        {//只编译.cs文件
            return null;
        }
        if (!File.Exists(srcPath))
        {
            Debug.Log("文件不存在 path = " + srcPath);
            return null;
        }
        // 1.CSharpCodePrivoder
        CSharpCodeProvider objCSharpCodePrivoder = new CSharpCodeProvider();

        // 2.ICodeComplier
        ICodeCompiler objICodeCompiler = objCSharpCodePrivoder.CreateCompiler();

        // 3.CompilerParameters
        CompilerParameters objCompilerParameters = new CompilerParameters();
        //objCompilerParameters.ReferencedAssemblies.Add("System.dll");
        objCompilerParameters.GenerateExecutable = false;
        objCompilerParameters.GenerateInMemory = false;
        string fileName = Path.GetFileNameWithoutExtension(srcPath);
        objCompilerParameters.OutputAssembly = fileName + ".dll";
        // 4.CompilerResults
        CompilerResults cr = objICodeCompiler.CompileAssemblyFromFile(objCompilerParameters, srcPath);
        if (cr.Errors.HasErrors)
        {
            Debug.LogError("编译错误：");
            foreach (CompilerError err in cr.Errors)
            {
                Debug.LogError(err.ErrorText);
            }
        }
        return cr;
    }

    /*static string[] GetFileReferenced(string srcPath)
    {
        FileStream fs = new FileStream(srcPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        StreamReader sr = new StreamReader(fs);
        sr.ReadLine();

    }*/
}