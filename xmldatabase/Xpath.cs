
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
namespace XMLDataBase
{
    public static class Xpath
    {
        public static ArrayList phaseXml(string xpath, XmlDocument XMLDocument)
        {
            ArrayList nodesA = new ArrayList();
            try
            {
                XmlNodeList nodes = XMLDocument.DocumentElement.SelectNodes(xpath);
                foreach (XmlNode node in nodes)
                {
                    Dictionary<string, string> nodes_ = new Dictionary<string, string>();
                    node.ChildNodes.Count.ToString();
                    foreach (XmlNode node_ in node.ChildNodes)
                    {
                        if (nodes_.ContainsKey(node_.Name)) continue;
                        nodes_.Add(node_.Name, node_.InnerText);
                    }
                    nodesA.Add(nodes_);
                }
            }
            catch (Exception e)
            {
                e.ToString();
            }

            return nodesA;
        }

        public static ArrayList phaseXmlRawNode(string xpath, XmlDocument XMLDocument)
        {
            ArrayList nodesA = new ArrayList();
            try
            {
                XmlNodeList nodes = XMLDocument.SelectNodes(xpath);
                foreach (XmlNode node in nodes)
                {
                    nodesA.Add(node);
                }
            }
            catch (Exception e)
            {
                e.ToString();
            }

            return nodesA;
        }

        public static ArrayList phaseXmlRawNode(string xpath, XmlNode XmlNode)
        {
            ArrayList nodesA = new ArrayList();
            try
            {
                XmlNodeList nodes = XmlNode.SelectNodes(xpath);
                foreach (XmlNode node in nodes)
                {
                    nodesA.Add(node);
                }
            }
            catch (Exception e)
            {
                e.ToString();
            }

            return nodesA;
        }

        public static XmlDocument GetXmlDocument(string SaveLocation)
        {
            XmlDocument Document = new XmlDocument();

            Document.LoadXml(GetXMLString(SaveLocation));

            return Document;
        }

        private static string GetXMLString(string SaveLocation)
        {
            string SaveFile = File.ReadAllText(SaveLocation);
            return SaveFile;
        }
    }
}
