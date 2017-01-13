
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
        private string DefaultItemName = "Item";

        private string Key;

        private string EncryptionKey = null;

        private bool Encryption;


        public DataBase(string DataStoreLocation = "DataStore.xml", bool UseEncryption = false, string EncryptionKey = null)
        {
            this.DataStoreLocation = DataStoreLocation;
            Encryption = UseEncryption; 

            this.EncryptionKey = EncryptionKey;
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
                return new List<OBJS.Data>();
            }

            XmlDocument Document = GetXMLDocument();

            return HandleLoadData(DataSet, Document);
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

            XmlNodeList nodes = Document.SelectNodes(ParentNode + DataSet.DataSet + "/" + DefaultItemName + SearchClause);
            foreach (XmlNode node in nodes)
            {
                OBJS.Data FoundData = new OBJS.Data();

                FoundData.DataSet = DataSet.DataSet;
                FoundData.ID = id;
                FoundData.Location = Location + "/" + DataSet.DataSet + "/" +DefaultItemName +"["+ id+"]";

                foreach (OBJS.Data.DataValue DataValue in DataSet.GetDataValues())
                {
                    string value = "";

                    if (node.SelectSingleNode(DataValue.Name) != null)
                    {
                        value = node.SelectSingleNode(DataValue.Name).InnerText;
                    }
                    FoundData.AddData(DataValue.Name, value);
                }

                foreach (OBJS.Data DataValue in DataSet.GetNestedData())
                {
                    List<OBJS.Data> NestedData = HandleLoadData(DataValue, node, "", FoundData.Location);
                    FoundData.AddData(NestedData);
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

        private void HandleSetData(OBJS.Data Data, XmlNode ParentRoot, XmlDocument Document, string ParentNode = "//DataStore")
        {
            XmlNode NewNode = ParentRoot.SelectSingleNode(ParentNode + "/" + Data.DataSet);
            if (!DoesDataStoreHaveDataSet(ParentNode, Data.DataSet, ParentRoot))
            {
                NewNode = CreateDataSet(ParentNode, Data.DataSet, ParentRoot, Document);
            }

            AppendNode(Data, NewNode, Document);
        }

        private bool AppendNode(OBJS.Data Data, XmlNode ParentRoot, XmlDocument Document)
        {
            XmlNode NewItem = Document.CreateNode(XmlNodeType.Element, DefaultItemName, null);

            foreach (OBJS.Data.DataValue DataValue in Data.GetDataValues())
            {
                XmlNode ItemValue = Document.CreateNode(XmlNodeType.Element, DataValue.Name, null);
                ItemValue.InnerText = DataValue.Value;
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

        public void DeleteDataSet(string DataSet)
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

            SaveXMLDocument(Document);
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

        public XmlDocument GetXMLDocument()
        {
            return GetXmlDocument(DataStoreLocation);
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
            XmlDocument Document = new XmlDocument();

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

        public string GetXMLAsString(XmlDocument myxml)
        {

            StringWriter sw = new StringWriter();
            XmlTextWriter tx = new XmlTextWriter(sw);
            myxml.WriteTo(tx);

            string str = sw.ToString();// 
            return str;
        }

        private string GetXMLString(string SaveLocation)
        {
            string SaveFile = File.ReadAllText(SaveLocation);
            return SaveFile;
        }

    }
}
