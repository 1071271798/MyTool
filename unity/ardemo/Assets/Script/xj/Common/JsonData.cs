using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace LitJson
{
	public class JsonData
	{
        #region Constructors


		public object m_value;

		public JsonData ()
		{
			m_value = null;
		}

		public JsonData (object value)
		{
			m_value = value;
		}

        #endregion

        #region Indexer

		public JsonData this [string key] {
			
			get {
				//dsy-json=null-dsy
				if (m_value == null) 
				{
					return null;
				}
				//dsy-类型匹配-dsy
				if (m_value.GetType () == typeof(Dictionary<string, object>))
				{
					//dsy-key exist-dsy
					if (((IDictionary)m_value).Contains (key)) 
					{						
						object val = ((IDictionary)m_value) [key];
						
						if (null != val) 
						{
							return new JsonData (val);
						}						
					} 
					else 
					{
						Debuger.Log ("Json:key not exist");
						return null;
					}					
				}
				//dsy-类型不匹配-dsy
                //add to tony
                if (null != m_value)
                {
                    Debuger.LogWarning("Error! Type = " + m_value.GetType().Name);
                }
				
				return null;
			}
			/*
			set
			{
				((IDictionary)m_value).Add(key,(string)value);
			}
			*/
		}

		public JsonData this [int index] 
		{
			get {
				if (m_value.GetType () == typeof(List<object>))
				{
					return new JsonData (((IList)m_value) [index]);
				} 
				else if (m_value.GetType () == typeof(Dictionary<string, object>)) 
				{
					int count = 0;
					foreach (object obj in (IDictionary)m_value)
					{
						if (count == index) 
						{
							return new JsonData (((DictionaryEntry)obj).Value);
						}

						count++;
					}
				} 
				else 
				{
					return new JsonData (m_value);
				}

				Debuger.LogWarning ("Error! Type = " + m_value.GetType ().Name);
				return null;
			}
		}

        #endregion

        #region Explicite Cast

		public static explicit operator String (JsonData data)
		{
			if (data != null && data.m_value != null)
				return data.m_value.ToString ();

			Debuger.LogWarning ("Not Containing a String");
			return "";
		}

		public static explicit operator Int32 (JsonData data)
		{
			int output = 0;
			if (data != null && data.m_value != null && System.Int32.TryParse (data.m_value.ToString (), out output))
				return output;

			Debuger.LogWarning ("Not Containing a Int32");
			return output;
		}

		public static explicit operator Int64 (JsonData data)
		{
			long output = 0;
			if (data.m_value != null && System.Int64.TryParse (data.m_value.ToString (), out output))
				return output;

			Debuger.LogWarning ("Not Containing a Int64");
			return output;
		}

		public static explicit operator Boolean (JsonData data)
		{
			bool output = false;
			if (data.m_value != null && System.Boolean.TryParse (data.m_value.ToString (), out output))
				return output;

			Debuger.LogWarning ("Not Containing a Boolean");
			return output;
		}

		public static explicit operator Double (JsonData data)
		{
			double output = 0;
			if (data.m_value != null && System.Double.TryParse (data.m_value.ToString (), out output))
				return output;

			Debuger.LogWarning ("Not Containing a Double");
			return output;
		}

        #endregion

		public int Count
		{
			get {
				if (m_value.GetType () == typeof(Dictionary<string, object>))
					return ((Dictionary<string, object>)m_value).Count;
				else if (m_value.GetType () == typeof(List<object>))
					return ((List<object>)m_value).Count;


				return 1;
			}
		}

		public override string ToString ()
		{
			if (m_value == null)
				return "";

			return m_value.ToString ();
		}

		public IDictionary Dictionary 
		{
			get {
				if (m_value.GetType () == typeof(Dictionary<string, object>)) {
					return (IDictionary)m_value;
				}

				return null;
			}
		}
	}
}
