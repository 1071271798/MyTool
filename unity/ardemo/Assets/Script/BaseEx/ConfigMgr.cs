using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Game
{
    public static class ConfigMgr
    {
        readonly static string sm_textPath = "Text";
        readonly static string sm_textOldPath = "TextOld";
        private static Dictionary<string, object> sm_textConfig = null;
        public static Dictionary<string, object> sm_textOldConfig = null;
        public static void Init()
        {
            string text = Resources.Load<TextAsset>(sm_textPath).text;
            sm_textConfig = MiniJSON.Json.Deserialize(text) as Dictionary<string, object>;
            string textold = Resources.Load<TextAsset>(sm_textOldPath).text;
            sm_textOldConfig = MiniJSON.Json.Deserialize(textold) as Dictionary<string, object>;
        }
        /// <summary>
        /// 获得文字内容
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetText(string key)
        {
            if (null != sm_textConfig && sm_textConfig.ContainsKey(key))
            {
                return sm_textConfig[key].ToString();
            }
            return key;
        }

        public static string GetOldText(string key)
        {
            if (null != sm_textOldConfig && sm_textOldConfig.ContainsKey(key))
            {
                return sm_textOldConfig[key].ToString();
            }
            return key;
        }
    }
}
