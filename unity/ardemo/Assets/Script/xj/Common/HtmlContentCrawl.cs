using System.Net;

public class HtmlContentCrawl
{
    private string url;
    
    public HtmlContentCrawl(string url)
    {
        this.url = url;
        LoadHtml(this.url);
    }


    private void LoadHtml(string url)
    {
        SingletonBehaviour<HttpDownload>.GetInst().DownloadText(url, delegate (string text) {
            Debuger.Log(text);
        });
    }
}

