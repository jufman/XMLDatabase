﻿
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
namespace XMLDataBase
{
    public class DataBase
    {

        private string DataStoreLocation;

        private string Key;

        private string EncryptionKey = null;

        private bool Encryption;

        public XmlDocument Document;


        public DataBase(string DataStoreLocation = "DataStore.xml", bool UseEncryption = false, string EncryptionKey = null)
        {
            this.DataStoreLocation = DataStoreLocation;
            Encryption = UseEncryption; 

            this.EncryptionKey = EncryptionKey;

            Document = GetXMLDocument();
        }

        private void BuildKey()
        {
            if (EncryptionKey != null)
            {
                Key = EncryptionKey;
                return;
            }

            string AssemblyName = Assembly.GetEntryAssembly().GetName().Name;

            byte[] AssemblyNameBytes = Encoding.UTF8.GetBytes(AssemblyName);

            string AssemblyNameBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(AssemblyName));

            string AssemblyNameSHA = Convert.ToBase64String(SHA256.Create().ComputeHash(AssemblyNameBytes));

            Key = AssemblyNameBase64 + AssemblyNameSHA + AssemblyNameBase64;
        }

        public List<OBJS.Data> LoadData(OBJS.Data DataSet)
        {
            if (!DoesDataStoreFileExist())
            {
                CreateDataStore();
                return new List<OBJS.Data>();
            }

            XmlDocument Document = GetXMLDocument(true);

            if (DataSet.Location != null)
            {
                string ParrentNode = DataSet.Location + "/";

                return HandleLoadData(DataSet, Document, ParrentNode, DataSet.Location);
            }
            else
            {
                return HandleLoadData(DataSet, Document);
            }
        }

        public List<T> LoadDataSet<T>(string DataSet, string DefaultItemName = "Item", params object[] args)
        {
            return GetObjectLogic.GetItems<T>(DataSet, DefaultItemName, this, args);
        }

        public void AddNewDataSet<T>(string DataSet,List<T> Items ,string DefaultItemName = "Item", bool AutoSave = true)
        {
            if (Items.Count == 0) return;

            SetObjectLogic.AddNewObjects<T>(Items, DataSet, DefaultItemName, this, AutoSave);
        }

        public void UpdateDataSet<T>(string DataSet, List<T> Items, string DefaultItemName = "Item", bool AutoSave = true)
        {
            if (Items.Count == 0) return;

            SetObjectLogic.UpdateObjects<T>(Items, DataSet, DefaultItemName, this, AutoSave);
        }

        public void DeleteItems<T>(string DataSet, List<T> Items, string DefaultItemName = "Item", bool AutoSave = true)
        {
            if (Items.Count == 0) return;

            SetObjectLogic.DeleteDataItems<T>(Items, DataSet, DefaultItemName, this, AutoSave);
        }

        public OBJS.Data LoadSetting(OBJS.Data DataSet)
        {
            if (!DoesDataStoreFileExist())
            {
                CreateDataStore();
                return new OBJS.Data();
            }

            XmlDocument Document = GetXMLDocument();

            return HandleLoadSettings(DataSet, Document);
        }

        private OBJS.Data HandleLoadSettings(OBJS.Data DataSet, XmlNode Document, string ParentNode = "//DataStore/", string Location = "//DataStore")
        {
            string SearchClause = "";
            if (DataSet.SearchClause != string.Empty)
            {
                SearchClause = "[" + DataSet.SearchClause + "]";
            }

            OBJS.Data FoundData = new OBJS.Data();

            XmlNode node = Document.SelectSingleNode(ParentNode + DataSet.DataSet + SearchClause);


            FoundData.DataSet = DataSet.DataSet;
            FoundData.Location = Location + "/" + DataSet.DataSet;
            FoundData.IsSettingsItem = true;

            if (node == null)
            {
                return FoundData;
            }

            foreach (OBJS.Data.DataValue DataValue in DataSet.GetDataValues())
            {
                string value = string.Empty;
                XmlNodeList DataNodes = node.SelectNodes(DataValue.Name);
                foreach (XmlNode DataNode in DataNodes)
                {
                    OBJS.Data.DataValue DataValues = new OBJS.Data.DataValue();

                    if (DataNode != null)
                    {
                        value = DataNode.InnerText;
                        foreach (XmlAttribute XmlAttribute in DataNode.Attributes)
                        {
                            OBJS.Data.DataValue Attribute = new OBJS.Data.DataValue();

                            Attribute.Name = XmlAttribute.Name;
                            Attribute.Value = XmlAttribute.Value;

                            DataValues.Attributes.Add(Attribute);
                        }
                    }

                    DataValues.Value = value;
                    DataValues.Name = DataValue.Name;

                    FoundData.GetDataValues().Add(DataValues);
                }
            }

            foreach (OBJS.Data DataValue in DataSet.GetNestedData())
            {
                if (DataValue.IsSettingsItem)
                {
                    OBJS.Data NestedData = HandleLoadSettings(DataValue, node, FoundData.Location + "/", FoundData.Location);
                    FoundData.AddData(NestedData);
                }
                else
                {
                    List<OBJS.Data> NestedData = HandleLoadData(DataValue, node, FoundData.Location + "/", FoundData.Location);
                    FoundData.AddData(NestedData);
                }
            }

            return FoundData;
        }

        private List<OBJS.Data> HandleLoadData(OBJS.Data DataSet, XmlNode Document, string ParentNode = "//DataStore/", string Location = "//DataStore")
        {
            List<OBJS.Data> Data = new List<OBJS.Data>();

            int id = 1;

            string SearchClause = "";
            if (DataSet.SearchClause != string.Empty)
            {
                SearchClause = "[" + DataSet.SearchClause + "]";
            }

            XmlNodeList nodes = Document.SelectNodes(ParentNode + DataSet.DataSet + "/" + DataSet.DefaultItemName + SearchClause);
            foreach (XmlNode node in nodes)
            {
                OBJS.Data FoundData = new OBJS.Data();

                FoundData.DataSet = DataSet.DataSet;
                FoundData.ID = id;
                FoundData.Location = Location + "/" + DataSet.DataSet + "/" + DataSet.DefaultItemName + "["+ id+"]";

                foreach (OBJS.Data.DataValue DataValue in DataSet.GetDataValues())
                {
                    string value = string.Empty;
                    XmlNodeList DataNodes = node.SelectNodes(DataValue.Name);

                    foreach (XmlNode DataNode in DataNodes)
                    {
                        OBJS.Data.DataValue DataValues = new OBJS.Data.DataValue();

                        if (DataNode != null)
                        {
                            value = DataNode.InnerText;
                            foreach (XmlAttribute XmlAttribute in DataNode.Attributes)
                            {
                                OBJS.Data.DataValue Attribute = new OBJS.Data.DataValue();

                                Attribute.Name = XmlAttribute.Name;
                                Attribute.Value = XmlAttribute.Value;
                                Attribute.Location = Location + "/" + DataSet.DataSet + "/" + DataSet.DefaultItemName + "[" + id + "]/" + DataValue.Name;

                                DataValues.Attributes.Add(Attribute);
                            }
                        }

                        DataValues.Value = value;
                        DataValues.Name = DataValue.Name;
                        DataValues.Location = Location + "/" + DataSet.DataSet + "/" + DataSet.DefaultItemName + "[" + id + "]/"+ DataValue.Name;

                        FoundData.GetDataValues().Add(DataValues);
                    }
                }

                foreach (OBJS.Data DataValue in DataSet.GetNestedData())
                {
                    if (DataValue.IsSettingsItem)
                    {
                        OBJS.Data NestedData = HandleLoadSettings(DataValue, node, FoundData.Location + "/", FoundData.Location);
                        FoundData.AddData(NestedData);
                    }
                    else
                    {
                        List<OBJS.Data> NestedData = HandleLoadData(DataValue, node, FoundData.Location + "/", FoundData.Location);
                        FoundData.AddData(NestedData);
                    }
                }

                Data.Add(FoundData);
                id++;
            }

            return Data;
        }

        public void SetData(OBJS.Data Data, XmlDocument Document = null, bool AutoSave = true)
        {
            if (!DoesDataStoreFileExist())
            {
                CreateDataStore();
            }

            if (Document == null)
            {
                Document = GetXMLDocument();
            }

            HandleSetData(Data, Document.SelectSingleNode("//DataStore"), Document);
            if (AutoSave)
            { 
                SaveXMLDocument(Document);
            }
        }

        public void SetData(List<OBJS.Data> Data, XmlDocument Document = null, bool AutoSave = true)
        {
            if (!DoesDataStoreFileExist())
            {
                CreateDataStore();
            }

            if (Document == null)
            {
                Document = GetXMLDocument();
            }

            Data.ForEach(X => {
                SetData(X, Document, false);
            });

            if (AutoSave)
            {
                SaveXMLDocument(Document);
            }
        }

        private void HandleSetData(OBJS.Data Data, XmlNode ParentRoot, XmlDocument Document, string ParentNode = "//DataStore")
        {
            XmlNode NewNode = ParentRoot.SelectSingleNode(ParentNode + "/" + Data.DataSet);
            if (!DoesDataStoreHaveDataSet(ParentNode, Data.DataSet, ParentRoot))
            {
                NewNode = CreateDataSet(ParentNode, Data.DataSet, ParentRoot, Document);
            }

            if (Data.IsSettingsItem)
            {
                AppendSettingsNode(Data, NewNode, Document);
            }
            else
            {
                AppendNode(Data, NewNode, Document);
            }
        }

        private void AppendSettingsNode(OBJS.Data Data, XmlNode ParentRoot, XmlDocument Document)
        {
            foreach (OBJS.Data.DataValue DataValue in Data.GetDataValues())
            {
                XmlElement ItemValue = Document.CreateElement(DataValue.Name);
                if (DataValue.Value == string.Empty || DataValue.Value == null)
                {
                    ItemValue.IsEmpty = true;
                }
                else
                {
                    ItemValue.InnerText = DataValue.Value;
                }

                if (DataValue.Attributes.Count != 0)
                {
                    foreach (OBJS.Data.DataValue Attributes in DataValue.Attributes)
                    {
                        XmlAttribute typeAttr = Document.CreateAttribute(Attributes.Name);
                        typeAttr.InnerText = Attributes.Value;
                        ItemValue.Attributes.Append(typeAttr);
                    }
                }
                ParentRoot.AppendChild(ItemValue);
            }

            foreach (OBJS.Data NestedData in Data.GetNestedData())
            {
                HandleSetData(NestedData, ParentRoot, Document, "");
            }
        }

        private bool AppendNode(OBJS.Data Data, XmlNode ParentRoot, XmlDocument Document)
        {
            XmlNode NewItem = Document.CreateNode(XmlNodeType.Element, Data.DefaultItemName, null);

            foreach (OBJS.Data.DataValue DataValue in Data.GetDataValues())
            {
                XmlElement ItemValue = Document.CreateElement(DataValue.Name);
                if (DataValue.Value == string.Empty || DataValue.Value == null)
                {
                    ItemValue.IsEmpty = true;
                }
                else
                {
                    ItemValue.InnerText = DataValue.Value;
                }

                if (DataValue.Attributes.Count != 0)
                {
                    foreach (OBJS.Data.DataValue Attributes in DataValue.Attributes)
                    {
                        XmlAttribute typeAttr = Document.CreateAttribute(Attributes.Name);
                        typeAttr.InnerText = Attributes.Value;
                        ItemValue.Attributes.Append(typeAttr);
                    }
                }
                NewItem.AppendChild(ItemValue);
            }

            foreach (OBJS.Data NestedData in Data.GetNestedData())
            {
                HandleSetData(NestedData, NewItem, Document, "");
            }

            ParentRoot.AppendChild(NewItem);

            return true;
        }

        private XmlNode CreateDataSet(string ParentNode, string DataSetName, XmlNode Document, XmlDocument RootDocument)
        {
            XmlNode DataSetNode = RootDocument.CreateNode(XmlNodeType.Element, DataSetName, null);
            return Document.AppendChild(DataSetNode);
        }

        private bool CreateDataStore()
        {
            StreamWriter SW = new StreamWriter(DataStoreLocation);

            string XMLTemplate = "<?xml version=\"1.0\"?><DataStore></DataStore>";

            if (Encryption)
            {
                Encryption EncryptionLogic = new Encryption();

                BuildKey();
                string CryptKey = Key;

                string DecryptedString = XMLTemplate;

                string EncryptedString = EncryptionLogic.EncryptText(DecryptedString, CryptKey);

                SW.Write(EncryptedString);
            }
            else
            {
                SW.Write(XMLTemplate);
            }

            SW.Close();

            return true;
        }

        private bool DoesDataStoreHaveDataSet(string ParentNode, string DataSetName, XmlNode Document)
        {
            bool HasDataSet = false;

            XmlNode Node = Document.SelectSingleNode(ParentNode + "/" + DataSetName);

            if (Node != null)
            {
                HasDataSet = true;
            }

            return HasDataSet;
        }

        public void DeleteDataSet(string DataSet, bool AutoSave = true)
        {
            if (!DoesDataStoreFileExist())
            {
                CreateDataStore();
                return;
            }

            XmlDocument Document = GetXMLDocument();

            if(DoesDataStoreHaveDataSet("//DataStore", DataSet, Document))
            {
                XmlNode Node = Document.SelectSingleNode("//DataStore/"+ DataSet);
                if (Node != null)
                {
                    Node.ParentNode.RemoveChild(Node);
                }
            }

            if (AutoSave)
            {
                SaveXMLDocument(Document);
            }
        }

        public void DeleteData(OBJS.Data Data)
        {
            if (!DoesDataStoreFileExist())
            {
                CreateDataStore();
                return;
            }

            XmlDocument Document = GetXMLDocument();

            XmlNode Node = Document.SelectSingleNode(Data.Location);

            if (Node != null)
            {
                DeleteNode(Node);
            }
            SaveXMLDocument(Document);
        }

        public void DeleteDataFromID(OBJS.Data Data)
        {
            if (!DoesDataStoreFileExist())
            {
                CreateDataStore();
                return;
            }

            XmlDocument Document = GetXMLDocument();

            XmlNode Node = Document.SelectSingleNode("//DataStore/" + Data.DataSet + "/" + Data.DefaultItemName + "["+Data.ID+"]");

            if (Node != null)
            {
                DeleteNode(Node);
            }
            SaveXMLDocument(Document);
        }

        public void DeleteItem(string ItemLocation, bool AutoSave = true)
        {
            if (!DoesDataStoreFileExist())
            {
                CreateDataStore();
                return;
            }

            XmlDocument Document = GetXMLDocument();

            XmlNode Node = Document.SelectSingleNode(ItemLocation);

            if (Node != null)
            {
                DeleteNode(Node);
            }

            if (AutoSave)
            {
                SaveXMLDocument(Document);
            }
        }

        private void DeleteNode(XmlNode Node)
        {
            Node.ParentNode.RemoveChild(Node);
        }

        public void UpdateData(OBJS.Data CurrentData, OBJS.Data NewData)
        {
            if (!DoesDataStoreFileExist())
            {
                CreateDataStore();
            }

            XmlDocument Document = GetXMLDocument();

            XmlNode Node = Document.SelectSingleNode(CurrentData.Location);
            if (Node != null)
            {
                foreach (OBJS.Data.DataValue DV in NewData.GetDataValues())
                {
                    XmlNode NodeToUpdate = Node.SelectSingleNode(DV.Name);
                    if (NodeToUpdate != null)
                    {
                        NodeToUpdate.InnerText = DV.Value;
                    }
                    else
                    {
                        XmlNode ItemValue = Document.CreateNode(XmlNodeType.Element, DV.Name, null);
                        ItemValue.InnerText = DV.Value;
                        Node.AppendChild(ItemValue);
                    }
                }
            }

            SaveXMLDocument(Document);
        }

        public XmlDocument GetXMLDocument(bool GetNew = false)
        {
            if (Document == null || GetNew)
            {
               return GetXmlDocument(DataStoreLocation);
            }
            return Document;
        }

        public bool SaveXMLDocument(XmlDocument Document)
        {

            if (Encryption)
            {
                Encryption EncryptionLogic = new Encryption();

                BuildKey();
                string CryptKey = Key;

                string DecryptedString = GetXMLAsString(Document);

                string EncryptedString = EncryptionLogic.EncryptText(DecryptedString, CryptKey);

                File.WriteAllText(DataStoreLocation, EncryptedString);
            }
            else
            {
                Document.Save(DataStoreLocation);
            }

            return true;
        }

        private bool DoesDataStoreFileExist()
        {
            return File.Exists(DataStoreLocation);
        }

        public XmlDocument GetXmlDocument(string SaveLocation)
        {
            Document = new XmlDocument();

            if (!DoesDataStoreFileExist())
            {
                CreateDataStore();
            }

            if (Encryption)
            {
                Encryption EncryptionLogic = new Encryption();

                BuildKey();
                string CryptKey = Key;

                string EncryptedString = GetXMLString(SaveLocation);

                string DecryptedString = EncryptionLogic.DecryptText(EncryptedString, CryptKey);

                Document.LoadXml(DecryptedString);
            }
            else
            {
                Document.LoadXml(GetXMLString(SaveLocation));
            }
            return Document;
        }

        public string GetEncryptedXMLString()
        {
            Encryption EncryptionLogic = new Encryption();

            BuildKey();
            string CryptKey = Key;

            string DecryptedString = GetXMLAsString(Document);

            string EncryptedString = EncryptionLogic.EncryptText(DecryptedString, CryptKey);

            return EncryptedString;
        }

        public string GetXMLString()
        {
            string DecryptedString = GetXMLAsString(Document);

            return DecryptedString;
        }

        public string GetXMLAsString(XmlDocument myxml)
        {
            return myxml.OuterXml;
        }

        private string GetXMLString(string SaveLocation)
        {
            string SaveFile = File.ReadAllText(SaveLocation);
            return SaveFile;
        }

    }
}
