using UnityEngine;
using System.Collections;
using Game.Scene;

public class SceneMgrTest
{
    private static SceneMgrTest instance;
    static object sLock = new object();
    public static SceneMgrTest Instance
    {
        get
        {
            if (null == instance)
            {
                lock (sLock)
                {
                    if (instance == null)
                    {

                        instance = new SceneMgrTest();
                    }
                }
            }

            return instance;
        }
    }
    public SceneMgrTest()
    {
        Init();
    }
    public void Init()
    {
        lastScene = SceneType.StartScene;
    }

    private SceneType lastScene;

    public SceneType LastScene
    {
        get { return lastScene; }
        set{lastScene=value;}
    }
}
