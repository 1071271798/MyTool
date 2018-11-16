using UnityEngine;
using System.Collections;

public class TextureAnim : MonoBehaviour {

     int framesPerSecond = 5;

    int scrollSpeed=10;
    int countX=16;
    int countY=1;
    int totalCount = 16;

    float offsetX=0.0f;
    float offsetY=0.0f;

    Vector2 singleTexSize;

    public Texture normalTexture;
    public Texture pickTexture;


	// Use this for initialization
	void Start () {
  
	    singleTexSize =new Vector2(1.0f/countX,1.0f/countY);
        this.transform.GetComponent<Renderer>().material.mainTextureScale = singleTexSize;
	}
	
	// Update is called once per frame
	void Update () {

        //var frame = Mathf.Floor(Time.time * scrollSpeed);
        //offsetX = frame / countX;
        //offsetY = -(frame - frame % countX) / countY / countX;
        //this.transform.renderer.material.SetTextureOffset("_MainTex", new Vector2(offsetX, offsetY));

        SetSpriteAnimation(countX, countY, 0, 0, totalCount, scrollSpeed);
	}


    public void SetSpriteAnimation(int colCount,int rowCount,int rowNumber,int colNumber,int totalCells,int fps)

   {
       float frames = Time.time * fps;
       int index=(int)frames;

       index=index%totalCells;

       //每个单元大小

       Vector2 size=new Vector2(1.0f/colCount,1.0f/rowCount);


       //分割成水平和垂直索引

       int uIndex=index%colCount;

       int vIndex=index/colCount;


       //颠倒V，让贴图正过来，所见即所得

       Vector2 offset=new Vector2((uIndex+colNumber)*size.x,(1.0f-size.y)-(vIndex+rowNumber)*size.y);


      GetComponent<Renderer>().material.SetTextureOffset("_MainTex",offset);

     // renderer.material.SetTextureScale("_MainTex",size);

  }

}
