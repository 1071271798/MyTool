using UnityEngine;
using System.Collections;
/// <summary>
/// 获取，添加，删除组件的封装，避免了空异常或重复添加
/// </summary>
/// <typeparam name="?"></typeparam>
public class GetTCompent
{
    /// <summary>
    /// get compent avoid null
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T GetCompent<T>(Transform trans) where T : Component
    {
        T t = trans.GetComponent<T>();
        if (t == null)
        {
            t = trans.gameObject.AddComponent<T>();
        }
        return t;
    }
    /// <summary>
    /// add component avoid repeat
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    public static void AddCompent<T>(Transform trans) where T : Component
    {
        if (trans.GetComponent<T>() == null)
        {
            trans.gameObject.AddComponent<T>();
        }
    }
    /// <summary>
    /// deleta component avoid null
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    public static void RemoveCompent<T>(Transform trans) where T : Component
    {
        if (trans.GetComponent<T>() != null)
            GameObject.Destroy(trans.GetComponent<T>());
    }
}

