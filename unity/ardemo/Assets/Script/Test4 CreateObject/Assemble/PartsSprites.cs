using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.Resource;
using Game.Platform;

public class PartsSprites : MonoBehaviour
{
	
	private static PartsSprites _instance;
	public static PartsSprites Ins
	{
		get { _instance=new PartsSprites();return _instance;}
	}
	public List<string> sptPName = new List<string>();//零件panel的名称
	public Dictionary<string, GameObject> spritePanelGO = new Dictionary<string, GameObject>();
	public GameObject panelPrefab;//sprite的panel的预设
	
	public List<string> innerSprites = new List<string>();//内置在app中的贴图资源
	public Dictionary<string, GameObject> spritePrefab = new Dictionary<string, GameObject>();//两种贴图预设("innerSprite","outSprite")，内置的用UISprite(使用内置图集)，外部下载的用UITexture(直接用图片当贴图)
	public Dictionary<string, SpriteGO> spriteDetails = new Dictionary<string, SpriteGO>();//<spritePanel名称，当前版面的sprite详情>
	public Vector2 oriP1Size;
	
	Dictionary<string, Texture> outPics = new Dictionary<string, Texture>();//从外部加载的图片资源
	List<string> allSprName = new List<string>();//零件表中的所有图片
	List<string> outSprName = new List<string>();//需要从外部加载的图片点名称
	List<string> innerSprName = new List<string>();//零件表中的图片在innerSprites中有
	
	public Vector2 OriPSize
	{
		get { return oriP1Pos; }
	}
	
	public Vector3 oriP1Pos;
	public Vector2 newP1Size;
	
	Vector2 testSize;
	Vector3 testPos;
	
	Vector2 fixSize;  //在P1Sprite为原始尺寸时，sprite的尺寸
	Vector3 fixPos;
	
	string robotname;
	string nameNoType;
	
	string not1;//连接件
	string not2;//装饰件
	string not3;//连接线
	
	Dictionary<string, SpriteGO> spriteD = new Dictionary<string, SpriteGO>();  //<Panel名字，图片信息>
	void Start()
	{
		not1 = LauguageTool.GetIns().GetText("连接件");
		not2 = LauguageTool.GetIns().GetText("装饰件");
		not3 = LauguageTool.GetIns().GetText("连接线");
		
		
		
		testSize = new Vector2(144,144);
		testPos = new Vector3(-598.0f, 304.0f, 0);
		fixSize = new Vector2(144,144);
		sptPName = GetpSpritesData.Ins.FindPanels();
		
		GameObject spriteT = Resources.Load("Prefab/Test4/MainScene/pSprite") as GameObject;
		if(spritePrefab.ContainsKey("pSprite")==false)
		{
			spritePrefab.Add("pSprite",spriteT);
		}
		
		spriteT = Resources.Load("Prefab/Test4/MainScene/pTexture") as GameObject;
		if (spritePrefab.ContainsKey("pTexture") == false)
		{
			spritePrefab.Add("pTexture", spriteT);
		}
		
		spriteT = Resources.Load("Prefab/Test4/MainScene/pNoteLabel") as GameObject;
		if (spritePrefab.ContainsKey("pNoteLabel") == false)
		{
			spritePrefab.Add("pNoteLabel", spriteT);
		}
		
		
		innerSprites = GetInnerTexList.Instance.FindPicType();
		
		//Debug.Log("innerSpritesCount:"+innerSprites.Count);
		
		allSprName = GetpSpritesData.Ins.FindAllPicNames();
		
		
		
		
		robotname = RobotMgr.Instance.rbtnametempt;
		nameNoType = RobotMgr.NameNoType(robotname);
		foreach(string temp in allSprName)
		{
			if(innerSprites.Contains(temp)==false&&temp!=nameNoType)
			{
                if (temp.IndexOf('-') > 0)    //pic-WHT + 色号
                {
                    string[] picStr = temp.Split('-');
                    Dictionary<string, string> colorType = SingletonObject<PublicTools>.GetInst().PartsPicColor();
                    string picNameTemp = picStr[0] + "-WHT";
                    if (!colorType.ContainsKey(picStr[1]))
                    {
                        if (outSprName.Contains(temp) == false)
                        {
                            outSprName.Add(temp);
                        }
                    }
                }
                else
                {
                    if (outSprName.Contains(temp) == false)
                    {
                        outSprName.Add(temp);
                    }
                }
			}
		}
		
		
		
		StartCoroutine(AddModelPic(nameNoType));
		
		//CreateSpritePanel();
	}
	
	//生成Sprite的Panel
	public GameObject p1;
	public void CreateSpritePanel()   //List<string> spriteP, GameObject panelPre
	{
		p1 = this.transform.GetChild(0).gameObject;
		
		spritePanelGO.Add(p1.name,p1);
		
		//oriP1Size = p1.GetComponent<UISprite>().localSize;
		
		JMSimulatorOnly.Instance.pswWidth = oriP1Size.x;
		//Debug.Log("oriP1Size:"+oriP1Size);
		p1.SetActive(false);
		StartCoroutine(EnableSprite(0.01f, p1));
	}
	
	IEnumerator EnableSprite(float t,GameObject p)
	{
		yield return new WaitForSeconds(t);
		p.SetActive(true);
		yield return new WaitForSeconds(0.02f);
		//newP1Size = p.GetComponent<UISprite>().localSize;
		
		
		
		for(int i=0;i<sptPName.Count;i++)
		{
			if(i!=0)
			{
				GameObject panelT = GameObject.Instantiate(p,Vector3.zero,Quaternion.identity) as GameObject;
				
				panelT.name=sptPName[i];
				panelT.transform.parent = this.transform;
				Vector3 posT = p.transform.localPosition;
				
				
				//panelT.transform.GetComponent<UISprite>().SetAnchor(panelT);//保证新生成的panel位置正确
				
				panelT.transform.localPosition = new Vector3(posT.x+1370*i,posT.y,posT.z);
				panelT.transform.localScale = new Vector3(1,1,1);
				if ((float)Screen.width / Screen.height > 2f)
				{
					panelT.transform.localScale = new Vector3(0.85f, 0.85f, 1);
				}
				spritePanelGO.Add(panelT.name, panelT);
			}
		}
		if ((float)Screen.width / Screen.height > 2f)
		{
			p.transform.localScale = new Vector3(0.85f, 0.85f, 1);
		}
		spriteD = GetpSpritesData.Ins.FindSpriteGO();
		
		for (int i = 0; i < sptPName.Count; i++)
		{
			CreateSprite(spritePanelGO[sptPName[i]], 9);//行，列
		}
		
	}
	
	//零件模型的路径
	public string ModelPicPath(string picName)
	{
		string pathTemp = "file:///" + ResourcesEx.persistentDataPath + "//default//" + picName + "//" + picName + ".png";
		return pathTemp;
	}
	

	//添加模型图片
	//如果app中没有内置图片，就需要加载
	Texture textureTT;
	IEnumerator AddModelPic(string picName)
	{
		#region 加载Clean图片
		textureTT = Resources.Load("Prefab/Test4/UI/Clean") as Texture;
		#endregion
		
		//模型名称的图片
		string pathTemp =ModelPicPath(picName);
		
		WWW www = new WWW(pathTemp);
		
		Texture textureT = null;
		yield return www;
		try
		{
			if (www != null && string.IsNullOrEmpty(www.error))
			{
				//获取Texture
				textureT = www.texture;
				//更多操作...    
				
				if (textureT != null)
				{
					outPics.Add(picName, textureT);
				}
			}
			else
			{
				outPics.Add(picName, textureTT);
			}
		}
		catch (System.Exception ex)
		{
			System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
			PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
		}
		
		
		if (outSprName.Count==0)
		{
			CreateSpritePanel();
		}
		else
		{
			StartCoroutine(AddOtherPic(outSprName,outPCount));
		}
		
	}
	
	
	int outPCount = 0;
	
	//添加其他图片
	IEnumerator AddOtherPic(List<string> outNamT,int i)
	{
		
		//Debug.Log("i:" + i);  // + ";outNamT[i]:" + outNamT[i]
		if (outNamT.Count>i)
		{
			
			//零件图片
			// string pathTemp = OutPartsPicPath(outNamT[i]);
			
			//模型名称的图片
			//string pathTemp = "file:///"+Application.persistentDataPath + "//default//Meebot//Raph.png";
			
			//零件图片
			robotname = RobotMgr.Instance.rbtnametempt;
			nameNoType = RobotMgr.NameNoType(robotname);
			string pathTemp = "file:///" + ResourcesEx.persistentDataPath + "/default/" + nameNoType + "/partsPic/"+outNamT[i] + ".png";
			
			WWW www = new WWW(pathTemp);
			
			Texture textureT = null;
			yield return www;
			try
			{
				if (www != null && string.IsNullOrEmpty(www.error))
				{
					//获取Texture
					textureT = www.texture;
					//更多操作...    
					// Debug.Log("oOOOoutNamT[i:"+outNamT[i]);
					if (textureT != null && outPics.ContainsKey(outNamT[i]) == false)
					{
						// Debug.Log("iTT:" + i + ";name:" + outNamT[i]);
						outPics.Add(outNamT[i], textureT);
					}
					
				}
				else
				{
					if (textureTT != null)
					{
						outPics.Add(outNamT[i], textureTT);
					}
					
					
				}
			}
			catch (System.Exception ex)
			{
				System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
				PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
			}
			
			i++;
			
			// Debug.Log("outPics.Count:" + outPics.Count + ";outNamT.Count:" + outNamT.Count);
			if (outPics.Count == outNamT.Count+1)
			{
				//Debug.Log("craet");
				CreateSpritePanel();
			}
			else if (i <= outNamT.Count)
			{
				//Debug.Log("outNameT:"+outNamT[i]+";00outPics.Count:" + outPics.Count + ";00outNamT.Count:" + outNamT.Count);
				StartCoroutine(AddOtherPic(outNamT, i));
			}
			
		}
	}
	
	
	public void CreateSprite(GameObject p,int m)
	{
		int goCount=spriteD[p.name].sptD.Count;
		for (int t = 0; t < goCount;t++ )
		{
			
			string nameTemp=spriteD[p.name].sptD[t].name;
			if (nameTemp.Contains("label") == false)   //生成贴图
			{
				GameObject newgo = GameObject.Instantiate( spritePrefab["pSprite"], Vector3.zero, Quaternion.identity) as GameObject;
				newgo.transform.parent = p.transform;
				newgo.transform.localPosition = spriteD[p.name].sptD[t].pos;
				
				newgo.name = spriteD[p.name].sptD[t].name;
				newgo.transform.localScale = new Vector3(1, 1, 1);
				
				UISprite spritT =RobotMgr.Instance.FindSpriteComponent(newgo.transform);
				if (outPics.ContainsKey(nameTemp) == false)
				{
                    if (innerSprites.Contains(nameTemp))
                    {
                        spritT.spriteName = nameTemp;
                        spritT.color = new Color(1, 1, 1, 1);
                    }
                    else
                    {
						if (nameTemp != null && nameTemp != "" && nameTemp.IndexOf("-") > 0)
                        {
                            string[] picStr = nameTemp.Split('-');
                            Dictionary<string, string> colorType = SingletonObject<PublicTools>.GetInst().PartsPicColor();
                            string picNameTemp = picStr[0] + "-WHT";
                            spritT.spriteName = picNameTemp;
                            Color colorTemp = SingletonObject<PublicTools>.GetInst().StringToColor1(colorType[picStr[1]]);
                            spritT.color = colorTemp;
                        }
                        else
                        {
                            spritT.spriteName = "Clean";
                        }
                    }
					
					spritT.MakePixelPerfect();
				}
				else
				{
					Destroy(spritT);
					UITexture textureT=newgo.AddComponent<UITexture>();
					textureT.mainTexture = outPics[nameTemp];
					
					textureT.depth = 3;
					//Debug.Log("nameTemp:" + nameTemp);
					textureT.MakePixelPerfect();
				}
				
				
				
				
				UILabel spT = JMSimulatorOnly.FindLabel(newgo);
				
				if (newgo.name != "Clean" && newgo.name != nameNoType&&newgo.name.Contains("WIRE")==false)
				{
					int countT = spriteD[p.name].sptD[t].count;
					if (newgo.name.Contains("-"))
					{
						string nameT = RobotMgr.GoType(newgo.name);
						string colorT = RobotMgr.GONumbType(newgo.name);   //获取颜色
						
						if(colorT!="NULL")
						{
							spT.text = nameT + "x" + countT;// +"(" + colorT + ")";
						}
						else
						{
							spT.text = null;
						}
					}                   
					else
					{
						spT.text = newgo.name + "x" + countT;
						
					}
					
				}
				else
				{
					spT.text = null;
				}
			}
			else   //生成label
			{
				GameObject labelTGO = GameObject.Instantiate(spritePrefab["pNoteLabel"], Vector3.zero, Quaternion.identity) as GameObject;
				labelTGO.name=spriteD[p.name].sptD[t].name;
				labelTGO.transform.parent = p.transform;
				labelTGO.transform.localScale = new Vector3(1, 1, 1);
				
				labelTGO.transform.localPosition = spriteD[p.name].sptD[t].pos;
				string noticeT= RobotMgr.GONumbType(labelTGO.name);   //获取颜色
				//Debug.Log("noticeT:"+noticeT);
				
				if (noticeT == "CONNECTORS")
				{
					labelTGO.GetComponent<UILabel>().text = not1;
				}
				else if (noticeT == "PARTS")
				{
					labelTGO.GetComponent<UILabel>().text = not2;
				}
				else if (noticeT == "WIRES")
				{
					labelTGO.GetComponent<UILabel>().text = not3;
				}
				else 
				{
					labelTGO.GetComponent<UILabel>().text = noticeT;
				}
			}
			
		}
		
		
		
		
	}
	
	//生成Sprite,零件贴图
	int t = 1;
	
	public void CreateSpriteOld(GameObject p,GameObject prefabgo,int n,int m)
	{
		//GetpSpritesData.Ins.creatPath();
		for (int j = 0; j < n; j++)
		{
			
			for (int i = 0; i < m;i++ )
			{
				Vector3 pos = new Vector3(-550.0f + 184 * i, 250.0f- 205 * j, 0);
				//Debug.Log("POS:"+pos);
				GameObject newgo= GameObject.Instantiate(prefabgo,Vector3.zero,Quaternion.identity) as GameObject;
				newgo.transform.parent = p.transform;
				newgo.transform.localPosition = pos;
				newgo.name = "go" +j+i;
				newgo.transform.localScale = new Vector3(1,1,1);
				
				UISprite spritT = RobotMgr.Instance.FindSpriteComponent(newgo.transform);
				spritT.spriteName = "Clean";
				UILabel spT = JMSimulatorOnly.FindLabel(newgo);
				spT.text = newgo.name;
				
				
			}
		}
	}
	
}

public class SpriteGO     //P01x2(BLK)
{
	public Dictionary<int, SpriteDetails> sptD = new Dictionary<int, SpriteDetails>(); //<id,spriteDetails>
}

public class SpriteDetails     //P01x2(BLK)
{
	public int id;//生成的顺序
	public string name;  //贴图的名字 如：P01-BLK
	public string type;  //贴图代表的模型的类型：P01
	public string color;  //贴图代表的模型的颜色：BLK
	public int count;    //贴图代表的模型的数量
	public Vector3 pos;//贴图生成的坐标
}
