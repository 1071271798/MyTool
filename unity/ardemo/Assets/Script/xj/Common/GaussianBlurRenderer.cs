using UnityEngine;

public class GaussianBlurRenderer : SingletonBehaviour<GaussianBlurRenderer>
{
    [Range(1, 8)]
    [SerializeField]
    public int BlurSize = 1;      // 降采样率

    public static Shader GaussianBlurShader;
    public Material CurMaterial;
    //指定Shader名称
    private string ShaderName = "Custom/GaussianBlur";

    private UITexture mTexture;
    private EventDelegate.Callback onFinished;

    Material material
    {
        get
        {
            if (CurMaterial == null)
            {
                //CurMaterial = GameObject.Instantiate(Resources.Load("GaussianBlurMat") as Material) as Material;
                CurMaterial = new Material(GaussianBlurShader);
                CurMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
            return CurMaterial;
        }
    }

    void Start()
    {
        if (null == GaussianBlurShader)
        {
            GaussianBlurShader = Shader.Find(ShaderName);
        }
    }

    void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
    {
        if (BlurSize != 0 && material != null)
        {

            int rtW = sourceTexture.width / 8;
            int rtH = sourceTexture.height / 8;


            RenderTexture rtTempA = RenderTexture.GetTemporary(rtW, rtH, 0, sourceTexture.format);
            rtTempA.filterMode = FilterMode.Bilinear;


            Graphics.Blit(sourceTexture, rtTempA);

            for (int i = 0; i < 2; i++)
            {

                float iteraionOffs = i * 1.0f;
                material.SetFloat("_blurSize", BlurSize + iteraionOffs);

                //vertical blur  
                RenderTexture rtTempB = RenderTexture.GetTemporary(rtW, rtH, 0, sourceTexture.format);
                rtTempB.filterMode = FilterMode.Bilinear;
                Graphics.Blit(rtTempA, rtTempB, material, 0);
                RenderTexture.ReleaseTemporary(rtTempA);
                rtTempA = rtTempB;

                //horizontal blur  
                rtTempB = RenderTexture.GetTemporary(rtW, rtH, 0, sourceTexture.format);
                rtTempB.filterMode = FilterMode.Bilinear;
                Graphics.Blit(rtTempA, rtTempB, material, 1);
                RenderTexture.ReleaseTemporary(rtTempA);
                rtTempA = rtTempB;

            }
            Graphics.Blit(rtTempA, destTexture);

            RenderTexture.ReleaseTemporary(rtTempA);
        }

        else
        {
            Graphics.Blit(sourceTexture, destTexture);

        }
    }

    void OnDestroy()
    {
        if (CurMaterial)
        {
            //立即销毁材质实例
            DestroyImmediate(CurMaterial);
        }
    }

    public static void ShowGaussianBlur()
    {

        UICameraAttb cameraAttb = UIManager.GetInst().GetCameraData(eUICameraType.OrthographicTwo);
        if (null != cameraAttb && null != cameraAttb.camera)
        {
            ShowTexture.ShowMsg(null);
            SetGaussianBlur(ShowTexture.GetUITexture(), null);
        }
    }

    public static void SetGaussianBlur(UITexture uiTexture, EventDelegate.Callback onFinished)
    {
        UICameraAttb cameraAttb = UIManager.GetInst().GetCameraData(eUICameraType.OrthographicTwo);
        if (null != cameraAttb && null != cameraAttb.camera)
        {
            SingletonBehaviour<Screenshots>.GetInst().SetScreenshots(uiTexture, delegate ()
            {

                GaussianBlurRenderer effect = cameraAttb.camera.gameObject.GetComponent<GaussianBlurRenderer>();
                if (null != effect)
                {
                    GameObject.DestroyImmediate(effect);
                }
                effect = cameraAttb.camera.gameObject.AddComponent<GaussianBlurRenderer>();
                effect.mTexture = uiTexture;
                effect.onFinished = onFinished;
            });
        }
        else
        {
            if (null != onFinished)
            {
                onFinished();
            }
        }
    }
}