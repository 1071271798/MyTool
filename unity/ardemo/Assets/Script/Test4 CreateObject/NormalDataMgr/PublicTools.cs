using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PublicTools : MonoBehaviour {

    public PublicTools() { }
	public Dictionary<string,string> PartsPicColor()
    {
        Dictionary<string, string> color = new Dictionary<string, string>();
        color.Add("RED", "(255,24,24,255)");       //C4-RED
        color.Add("GRY", "(180,180,180,255)");    //C1-GRY
        color.Add("BLU", "(69,212,235,255)");      //P06-BLU
        color.Add("BLU2","(9,103,153,255)");      //C4-BLU2
        color.Add("LTBU", "(238,255,255,255)");      //P26-LTBU
        color.Add("GRN", "(170,226,88,255)");      //P02-GRN
        color.Add("GRN2","(78,117,52,255)");      //C4-GRN2
        color.Add("GRN3", "(49,116,61,255)");      //P02-GRN3
        color.Add("ORG", "(241,137,31,255)");      //P01-ORG
        color.Add("ORG1","(197,106,15,255)");      //C4-ORG1
        color.Add("ORG2", "(143,73,36,255)");      //C4-ORG2
        color.Add("PUR", "(144,74,153,255)");      //C4-PUR
        color.Add("YLW", "(238,234,63,255)");      //C8-YLW
        color.Add("YLW2", "(245,180,73,255)");      //P01-YLW2
        color.Add("BLK", "(88,90,87,255)");      //P02-BLK      
        return color;
    }

    //将字符串转换为Vector3
    public Vector3 StringToVector(string vect)
    {
        string[] num = vect.Split(new char[] { '(', ',', ')' });

        //Convert.ToSingle()将字符转换为float
        Vector3 newVect = new Vector3(float.Parse(num[1]), float.Parse(num[2]), float.Parse(num[3]));
        return newVect;
    }

    //将字符串转换为Vector3
    public Color StringToColor(string vect)
    {
        string[] num = vect.Split(new char[] { '(', ',', ')' });

        //Convert.ToSingle()将字符转换为float
        Color newColor = new Color(float.Parse(num[1]), float.Parse(num[2]), float.Parse(num[3]), float.Parse(num[4]));
        return newColor;
    }

    //将字符串转换为Vector3
    public Color StringToColor1(string vect)
    {
        string[] num = vect.Split(new char[] { '(', ',', ')' });

        //Convert.ToSingle()将字符转换为float
        Color newColor = new Color(float.Parse(num[1])/255, float.Parse(num[2])/255, float.Parse(num[3])/255, float.Parse(num[4])/255);
        return newColor;
    }

    public void ResetCameraPos(bool isReset)
    {
        Camera camTemp = GameObject.Find("MainCamera").GetComponent<Camera>();
        if (camTemp != null)
        {
            if (isReset)
            {
                camTemp.rect = new Rect(0, 0, 1, 1);
            }
            else
            {
                camTemp.rect = new Rect(0, 0, 0.6f, 1);
            }
        }
        
    }

}
