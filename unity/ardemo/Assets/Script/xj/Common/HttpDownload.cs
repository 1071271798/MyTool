
using UnityEngine;
using System.IO;
using System.Collections;

public class HttpDownload : SingletonBehaviour<HttpDownload>
{
    public delegate void HttpCallback(string text);


    private WWW www;
    private string url;

    public void Download(string url, string savePath)
    {
        this.url = url;
        StartCoroutine(DownloadFile(url, savePath));
    }
    public void DownloadText(string url, HttpCallback callback)
    {
        this.url = url;
        StartCoroutine(WWWDownloadText(url, callback));
    }

    void Update()
    {
        if (null != www)
        {
            Debuger.Log(string.Format("DownloadFile url {0} progress = {1}", this.url, www.progress));
        }
    }

    IEnumerator DownloadFile(string url, string savePath)
    {
        www = new WWW(url);
        yield return www;
        if (null == www.error)
        {
            if (www.isDone)
            {
                if (null != www.bytes)
                {
                    File.WriteAllBytes(savePath, www.bytes);
                }
            }
        }
        else
        {
            Debuger.Log(string.Format("DownloadFile url {0} error = {1}", url, www.error));
        }
        www.Dispose();
    }

    IEnumerator WWWDownloadText(string url, HttpCallback callback)
    {
        WWW www = new WWW(url);
        
        yield return www;
        Debuger.Log(string.Format("DownloadFile url {0} progress = {1}", url, www.progress));
        if (null == www.error)
        {
            if (www.isDone)
            {
                if (null != callback)
                {
                    callback(www.text);
                }
            } else
            {
                if (null != callback)
                {
                    callback(null);
                }
            }
        }
        else
        {
            Debuger.Log(string.Format("DownloadFile url {0} error = {1}", url, www.error));
            if (null != callback)
            {
                callback(null);
            }
        }
        www.Dispose();
        www = null;
    }

}