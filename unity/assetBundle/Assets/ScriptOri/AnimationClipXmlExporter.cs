using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace UGame
{
    public class AnimationClipXmlExporter
    {
        private string xmlName;
        private string sourceName;
        private string Path
        {
            get { return string.Concat(Application.persistentDataPath, "/", xmlName, ".xml"); }
        }

        public static void ExportXml(string xmlName, string sourceName, List<Fragment> fragments)
        {
            var exporter = new AnimationClipXmlExporter();
            exporter.CreateXml(xmlName, sourceName, fragments);
            EditorUtility.RevealInFinder(exporter.Path);
        }

        public static void MergeXml(string xmlName, string sourceName)
        {
            var exporter = new AnimationClipXmlExporter();
            exporter.MergeXmlElement(xmlName, sourceName);
            EditorUtility.RevealInFinder(exporter.Path);
        }

        private void CreateXml(string xmlName, string sourceName, List<Fragment> fragments)
        {
            this.xmlName = xmlName;
            this.sourceName = sourceName;
            XmlDocument document = new XmlDocument();

            //加入XML的声明段落,<?xml version="1.0" encoding="gb2312"?>
            var xml = document.CreateXmlDeclaration("1.0", "utf-8", "yes");
            document.AppendChild(xml);

            //加入一个根元素
            XmlElement node = document.CreateElement("root");
            document.AppendChild(node);//添加到文本中 
            CreateElements(document, node, fragments);
            MergeXmlElement(xmlName, sourceName);
        }

        private void CreateElements(XmlDocument document, XmlElement node, List<Fragment> fragments)
        {
            var count = fragments.Count;
            if (count <= 1) return;

            var type = sourceName;
            var source = sourceName;
            int index = 1;
            for (int i = 0; i < count; i++)
            {
                var fragment = fragments[i];
                var targetName = fragment.target;
                var offset = fragment.endIndex - fragment.startIndex;
                if (offset == 1)
                {
                    if (fragment.linkIndex > 1) continue;//抛弃当前帧
                    if (fragment.linkIndex == 1 && fragment.linkCount > 1) i++;//抛弃下一帧
                }

                var element = document.CreateElement("Anim");
                node.AppendChild(element);//添加到文本中 

                element.SetAttribute("id", index.ToString());
                element.SetAttribute("type", type);

                if (targetName.StartsWith("seivo_") && offset == 1)
                {
                    var strs = targetName.Split('_');
                    element.SetAttribute("source", "djid");
                    element.SetAttribute("djid", int.Parse(strs[1]).ToString());
                    AttachDJEffect(document, node, type, fragment, index);
                }
                else if (targetName.StartsWith("s0") && offset == 1)
                {
                    //var strs = targetName.Split('_');
                    //string sensorName = strs[0].Replace("s", "Sensor");
                    //int sensorId = int.Parse(strs[1]);
                    //element.SetAttribute("source", string.Concat(sensorName, "_", sensorId));
                }
                else
                {
                    element.SetAttribute("source", source);
                }

                element.SetAttribute("parts", targetName.ToUpper());
                element.SetAttribute("pic", "1");

                element.SetAttribute("start", fragment.startIndex.ToString());
                element.SetAttribute("end", fragment.endIndex.ToString());

                element.SetAttribute("step", "1");
                element.SetAttribute("name", string.Concat(type, "_", index));

                index++;
                source = sourceName;
                if (fragment.linkCount >= 3)
                {
                    if (offset <= 0)
                    {
                        element.SetAttribute("error", "");
                    }
                    else if (offset >= 2 && offset <= 18)
                    {
                        element.SetAttribute("short", "");
                    }
                    else if(offset >= 22)
                    {
                        element.SetAttribute("long", "");
                    }
                }
            }
            document.Save(Path);
        }

        private void AttachDJEffect(XmlDocument document, XmlElement node, string type, Fragment fragment, int index)
        {
            var element = document.CreateElement("Anim");
            node.AppendChild(element);//添加到文本中 

            element.SetAttribute("id", index.ToString());
            element.SetAttribute("type", type);

            element.SetAttribute("source", "shape");
            element.SetAttribute("shape", string.Concat(fragment.target, ":"));

            element.SetAttribute("parts", fragment.target.ToUpper());
            element.SetAttribute("pic", "1");

            element.SetAttribute("start", (fragment.endIndex).ToString());
            element.SetAttribute("end", (fragment.endIndex + 1).ToString());

            element.SetAttribute("step", "1");
            element.SetAttribute("name", string.Concat(type, "_", index));
        }

        private void MergeXmlElement(string xmlName, string sourceName)
        {
            this.xmlName = xmlName;
            this.sourceName = sourceName;
            var document = new XmlDocument();
            document.Load(Path);

            var key = sourceName;
            var elements = document.GetElementsByTagName("Anim");
            for (int i = 0; i < elements.Count; i++)
            {
                var element = (XmlElement)elements[i];
                var index = i + 1;
                element.SetAttribute("id", index.ToString());
                element.SetAttribute("name", string.Concat(key, "_", index));
                string parts = element.GetAttribute("parts");
                parts = parts.Split('_')[0];
                element.SetAttribute("parts", parts);
            }
            document.Save(Path);
        }

        private void MergeXmlElement(Object xmlObject)
        {
            var xmlAsset = xmlObject as TextAsset;
            if (xmlAsset == null) return;
            var xmlText = xmlAsset.text;
            this.xmlName = xmlObject.name;
            var document = new XmlDocument();
            document.LoadXml(xmlText);

            var key = sourceName;
            var elements = document.GetElementsByTagName("Anim");
            for (int i = 0; i < elements.Count; i++)
            {
                var element = (XmlElement)elements[i];
                var index = i + 1;
                element.SetAttribute("id", index.ToString());
                element.SetAttribute("name", string.Concat(key, "_", index));
            }
            document.Save(Path);
        }
    }
}
