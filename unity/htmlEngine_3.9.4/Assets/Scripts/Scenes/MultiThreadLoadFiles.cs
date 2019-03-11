using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class MultiThreadLoadFiles : SingletonBehaviour<MultiThreadLoadFiles>
{
    public delegate void LoadFileCallback(byte[] bytes);
    readonly int Max_Thread_Num = 2;
    Dictionary<string, LoadFileCallback> mFilesDict;
    static object mLock;
    static Queue<string> mLoadFilePathList;
    ThreadLoadFiles[] mThreadLoadArray;

    bool isLoadThread;

    public MultiThreadLoadFiles()
    {
        mFilesDict = new Dictionary<string, LoadFileCallback>();
        mThreadLoadArray = new ThreadLoadFiles[Max_Thread_Num];
        mLoadFilePathList = new Queue<string>();
        isLoadThread = false;
        mLock = new object();
    }

    public void LoadFile(string path, LoadFileCallback callback)
    {
        mFilesDict[path] = callback;
        lock(mLock)
        {
            mLoadFilePathList.Enqueue(path);
        }
        OpenThread();
    }
    void OpenThread()
    {
        if (isLoadThread)
        {
            return;
        }
        for (int i = 0; i < Max_Thread_Num; ++i)
        {
            if (null == mThreadLoadArray[i])
            {
                mThreadLoadArray[i] = new ThreadLoadFiles("Thread" + i);
            }
        }
        isLoadThread = true;
        SingletonObject<TimeStatisticsTool>.GetInst().EventStart("MultiThreadLoadFiles");
    }
    static string PopLoadPath()
    {
        lock(mLock)
        {
            if (mLoadFilePathList.Count > 0)
            {
                return mLoadFilePathList.Dequeue();
            }
            return null;
        }
    }

    static void CheckLoadPathCompleted()
    {
        lock(mLock)
        {
            if (mLoadFilePathList.Count <= 0)
            {
                SingletonObject<TimeStatisticsTool>.GetInst().EventFinished("MultiThreadLoadFiles", delegate(long time) {
                    Debug.Log(time);
                });
            }
        }
    }

    void FixedUpdate()
    {
        for (int i = 0; i < Max_Thread_Num; ++i)
        {
            List<LoadFileCompleted> list = mThreadLoadArray[i].PopCompleted();
            if (null != list)
            {
                foreach (var item in list)
                {
                    if (mFilesDict.ContainsKey(item.path) && null != mFilesDict[item.path])
                    {
                        mFilesDict[item.path](item.bytes);
                    }
                    mFilesDict.Remove(item.path);
                }
                list.Clear();
            }
        }
    }

    void OnDestroy()
    {
        for (int i = 0; i < Max_Thread_Num; ++i)
        {
            mThreadLoadArray[i].Dispose();
        }
        mThreadLoadArray = null;
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// 
    /// </summary>
    private class ThreadLoadFiles
    {
        private Thread mThread;
        private string name;
        private object mLockCompleted;
        private bool isWork;
        private List<LoadFileCompleted> mCompletedList;

        public ThreadLoadFiles(string name)
        {
            this.name = name;
            mThread = new Thread(OnThreadStart);
            mLockCompleted = new object();
            isWork = true;
            mThread.Start();
        }

        public List<LoadFileCompleted> PopCompleted()
        {
            lock(mLockCompleted)
            {
                List<LoadFileCompleted> list = null;
                if (null != mCompletedList && mCompletedList.Count > 0)
                {
                    list = mCompletedList;
                    mCompletedList = null;
                }
                return list;
            }
        }

        public void Dispose()
        {
            mThread.Abort();
        }

        void OnThreadStart()
        {
            //Debug.Log("Thread " + name + " start");
            while (isWork)
            {
                string loadPath = PopLoadPath();
                if (!string.IsNullOrEmpty(loadPath))
                {
                    LoadFile(loadPath);
                }
            }
        }

        void AddCompleted(string path, byte[] bytes)
        {
            lock(mLockCompleted)
            {
                if (null == mCompletedList)
                {
                    mCompletedList = new List<LoadFileCompleted>();
                }
                mCompletedList.Add(new LoadFileCompleted(path, bytes));
            }
        }
        void LoadFile(string path)
        {
            FileStream fileStream = null;
            byte[] bytes = null;
            try
            {
                //创建文件读取流
                fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                fileStream.Seek(0, SeekOrigin.Begin);
                //创建文件长度缓冲区
                bytes = new byte[fileStream.Length];
                //读取文件
                fileStream.Read(bytes, 0, (int)fileStream.Length);
                //释放文件读取流
                fileStream.Close();
                fileStream.Dispose();
                fileStream = null;
            }
            catch (System.Exception ex)
            {
                if (null != fileStream)
                {
                    fileStream.Close();
                    fileStream.Dispose();
                    fileStream = null;
                }
                Debug.Log("ThreadLoadFiles name = " + this.name + "-LoadFile:FileStream error:" + ex.ToString());
            }
            //Debug.Log(this.name + "LoadFile Completed:" + path);
            AddCompleted(path, bytes);
            //CheckLoadPathCompleted();
        }
    }

    private class LoadFileCompleted
    {
        public string path;
        public byte[] bytes;

        public LoadFileCompleted(string path, byte[] bytes)
        {
            this.path = path;
            this.bytes = bytes;
        }
    }
}