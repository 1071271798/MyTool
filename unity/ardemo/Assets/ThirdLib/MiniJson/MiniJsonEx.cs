using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

class MiniJsonEx
{
    private static BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    /// <summary>
    /// 把json字符串转换为指定类型的对象
    /// </summary>
    /// <param name="json"></param>
    /// <param name="objType"></param>
    /// <returns></returns>
    public static Object ToObject(string json, Type objType)
    {
        return ToObject(MiniJSON.Json.Deserialize(json) as Dictionary<string, object>, objType);
    }

    public static Object ToObject(Dictionary<string, object> dict, Type objType)
    {
        return ParseObj(dict, objType);
    }

    /// <summary>
    /// 把对象转换为json字符串
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static String ToJsonString(Object obj)
    {
        return MiniJSON.Json.Serialize(CreateJsonObj(obj));
    }

    private static void ParseField(Object obj, FieldInfo field, Object jsonObject, Type objType)
    {
        String typeStr = objType.ToString();

        if (typeStr.Equals("System.Int32"))
        {
            Int32 i = (Int32)((Int64)jsonObject);
            field.SetValue(obj, i);
        }
        else if (typeStr.Equals("System.Int64"))
        {
            Int64 i = (Int64)jsonObject;
            field.SetValue(obj, i);
        }
        else if (typeStr.Equals("System.Int16"))
        {
            Int16 i = (Int16)((Int64)jsonObject);
            field.SetValue(obj, i);
        }
        else if (typeStr.Equals("System.Byte"))
        {
            Byte i = (Byte)((Int64)jsonObject);
            field.SetValue(obj, i);
        }
        else if (typeStr.Equals("System.Single"))
        {
            string tstr = jsonObject.GetType().ToString();
            if (tstr.Equals("System.Int64"))
            {
                float i = (float)(Int64)jsonObject;
                field.SetValue(obj, i);
            }
            else
            {
                float i = (float)(double)(jsonObject);
                field.SetValue(obj, i);
            }
        }
        else if (typeStr.Equals("System.Double"))
        {
            string tstr = jsonObject.GetType().ToString();
            if (tstr.Equals("System.Int64"))
            {
                double i = (double)(Int64)(jsonObject);
                field.SetValue(obj, i);
            }
            else
            {
                double i = (double)(jsonObject);
                field.SetValue(obj, i);
            }
        }
        else if (typeStr.Equals("System.Boolean"))
        {
            bool b = (bool)jsonObject;
            field.SetValue(obj, b);
        }
        else if (typeStr.Equals("System.String"))
        {
            String str = (String)jsonObject;
            if (str != null)
            {
                field.SetValue(obj, str);
            }
        }
        else if (typeStr.StartsWith("System.Collections.Generic.List"))
        {
            String listType = typeStr.Substring(typeStr.IndexOf("[") + 1, (typeStr.Length - typeStr.IndexOf("[") - 2));
            field.SetValue(obj, CreateLst(listType, jsonObject as List<Object>));
        }
        else if (typeStr.StartsWith("System.Collections.Generic.Dictionary"))
        {
            String dictObjType = typeStr.Substring(typeStr.IndexOf(",") + 1, (typeStr.Length - typeStr.IndexOf(",") - 2));
            field.SetValue(obj, CreateDist(dictObjType, jsonObject as Dictionary<string, object>));
        }
        else
        {
            field.SetValue(obj, ParseObj(jsonObject as Dictionary<string, object>, objType));
        }
    }

    private static Object ParseObj(Dictionary<string, object> jObj, Type objType)
    {
        Object obj = Assembly.GetExecutingAssembly().CreateInstance(objType.ToString());
        FieldInfo[] fields = objType.GetFields(flag);
        foreach (FieldInfo subField in fields)
        {
            try
            {
                ParseField(obj, subField, jObj[subField.Name], subField.FieldType);
            }
            catch (KeyNotFoundException e)
            {
                //json里如果有没有定义的字段, 就不做处理
            }
        }
        return obj;
    }

    private static Object CreateNodeObj(Object jsonObj, Type t)
    {
        Object obj = null;
        String typeStr = t.ToString();

        if (typeStr.Equals("System.Int32"))
        {
            obj = (Int32)jsonObj;
        }
        else if (typeStr.Equals("System.Int64"))
        {
            obj = (Int64)jsonObj;
        }
        else if (typeStr.Equals("System.Int16"))
        {
            obj = (Int16)jsonObj;
        }
        else if (typeStr.Equals("System.Byte"))
        {
            obj = (Byte)jsonObj;
		}
		else if (typeStr.Equals("System.Single"))
		{
			string tstr = jsonObj.GetType().ToString();
			if (tstr.Equals("System.Int64"))
			{
				obj = (float)(Int64)jsonObj;
			}
			else
			{
				obj = (float)(double)jsonObj;
			}
		}
		else if (typeStr.Equals("System.Double"))
		{
			string tstr = jsonObj.GetType().ToString();
			if (tstr.Equals("System.Int64"))
			{
				obj = (double)(Int64)jsonObj;
			}
			else
			{
				obj = (double)jsonObj;
			}
		}
        else if (typeStr.Equals("System.Boolean"))
        {
            obj = (bool)jsonObj;
        }
        else if (typeStr.Equals("System.String"))
        {
            obj = jsonObj.ToString();
        }
        else if (typeStr.StartsWith("System.Collections.Generic.List"))
        {
            String listType = typeStr.Substring(typeStr.IndexOf("[") + 1, (typeStr.Length - typeStr.IndexOf("[") - 2));
            obj = CreateLst(listType, jsonObj as List<Object>);
        }
        else if (typeStr.StartsWith("System.Collections.Generic.Dictionary"))
        {
            String dictObjType = typeStr.Substring(typeStr.IndexOf(",") + 1, (typeStr.Length - typeStr.IndexOf(",") - 2));
            obj = CreateDist(dictObjType, jsonObj as Dictionary<string, object>);
        }
        else
        { //自定义类型
            obj = ParseObj(jsonObj as Dictionary<String, Object>, t);
        }

        return obj;
    }

    private static IList CreateLst(String listType, List<Object> jsonLst)
    {
        Type t = Type.GetType(listType);
        Type generic = typeof(List<>);
        generic = generic.MakeGenericType(new Type[] { t });
        var objLst = Activator.CreateInstance(generic) as IList;
        foreach (Object jobj in jsonLst)
        {
            objLst.Add(CreateNodeObj(jobj, t));
        }

        return objLst;
    }

    private static IDictionary CreateDist(String listType, Dictionary<String, Object> jObjs)
    {
        Type t = Type.GetType(listType);

        Type strType = typeof(string);
        IDictionary objDist = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(strType, t));

        foreach (String key in jObjs.Keys)
        {
            objDist.Add(key, CreateNodeObj(jObjs[key], t));
        }

        return objDist;
    }

    private static Object CreateJsonObj(Object obj)
    {
        Dictionary<String, object> dict = new Dictionary<string, object>();
        Type objType = obj.GetType();
        FieldInfo[] fields = objType.GetFields(flag);

        foreach (FieldInfo field in fields)
        {
			if (field.GetValue(obj) != null)
			{
				String fieldTypeStr = field.FieldType.ToString();
				if (fieldTypeStr.IndexOf("System.") >= 0)
				{
					if (fieldTypeStr.StartsWith("System.Collections.Generic.List"))
					{
						dict.Add(field.Name, CreateListJsonObj(field.GetValue(obj)));
					}
					else if (fieldTypeStr.StartsWith("System.Collections.Generic.Dictionary"))
					{
						dict.Add(field.Name, CreateDictJsonObj(field.GetValue(obj)));
					}
					else if (fieldTypeStr.IndexOf(".Collections") < 0)
					{
						dict.Add(field.Name, field.GetValue(obj));
					}
				}
				else
				{
					Dictionary<String, object> subDict = CreateJsonObj(field.GetValue(obj)) as Dictionary<String, object>;
					dict.Add(field.Name, subDict);
				}
			}
        }

        return dict;
    }

    private static IList CreateListJsonObj(object obj)
    {
        IList objList = obj as IList;
        IList jsonLst = new List<object>();
        foreach (object subObj in objList)
        {
            jsonLst.Add(CreateSubObj(subObj.GetType().ToString(), subObj));
        }

        return jsonLst;
    }

    private static IDictionary CreateDictJsonObj(object obj)
    {
        IDictionary objDict = obj as IDictionary;
        Dictionary<String, object> jsonDict = new Dictionary<string, object>();
        foreach (String key in objDict.Keys)
        {
            Object dictObj = objDict[key];
            jsonDict.Add(key, CreateSubObj(dictObj.GetType().ToString(), dictObj));
        }

        return jsonDict;
    }

    private static object CreateSubObj(String typeStr, object obj)
    {
        Object tmpObj = null;
        if (typeStr.IndexOf("System.") >= 0)
        {
			if (typeStr.StartsWith("System.Collections.Generic.List"))
			{
				tmpObj = CreateListJsonObj(obj);
			}
			else if (typeStr.StartsWith("System.Collections.Generic.Dictionary"))
			{
				tmpObj = CreateDictJsonObj(obj);
			}
			else if (typeStr.IndexOf(".Collections") < 0)
			{
				tmpObj = obj;
			}
        }
        else
        {//自定义类型
            tmpObj = CreateJsonObj(obj);
        }

        return tmpObj;
    }
}
