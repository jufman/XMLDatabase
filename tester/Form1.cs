using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace Tester
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            XMLDataBase.DataBase DataBase = new XMLDataBase.DataBase();

            List<TestObject> TestObjects = new List<TestObject>();

            TestObjects.Add(new TestObject()
            {
                Name = "Hello",
                MultiNodes = new List<MultiNode>()
                    {
                        new MultiNode() {FileName = "jus" },
                        new MultiNode() {FileName = "jus"}
                    },
                TestNestedSetting = new NestedSettings()
                {
                    FileName = "I am a Nested Item",
                    MultiNodes = new List<MultiNode>()
                    {
                        new MultiNode() {FileName = "jus" },
                        new MultiNode() {FileName = "jus"}
                    }
                },
               
                And = new List<And>()
                {
                    new And()
                    {
                        AttFill = new List<AttFill>() {
                            new AttFill() {Attr ="First 555", Sec = "Hello" },
                            new AttFill() {Attr ="First 444", Sec = "Hello Again" }, 
                            new AttFill() {Attr ="First 333", Sec = "Hello one more" }
                        },
                        AttFill2 = new List<AttFill2>() {
                            new AttFill2() {Attr ="Secound 555" },
                            new AttFill2() {Attr ="Secound 444" },
                            new AttFill2() {Attr ="Secound 333" }
                        }
                    },
                    new And()
                    {
                        AttFill = new List<AttFill>() {
                            new AttFill() {Attr ="Round 2 555" },
                            new AttFill() {Attr ="Round 2 444" },
                        },
                        AttFill2 = new List<AttFill2>() {
                            new AttFill2() {Attr ="Fight 555" },
                            new AttFill2() {Attr ="Fight 444" },
                        }
                    },

                }
            });

            DataBase.AddNewDataSet<TestObject>("Test", TestObjects);
        }

        public class TestObject : XMLDataBase.OBJS.XMLDataDetails
        {
            [XMLDataBase.OBJS.XMLDatabaseRetriveItem]
            public string Name { get; set; }
            [XMLDataBase.OBJS.XMLDatabaseRetriveItem(Type = XMLDataBase.OBJS.Types.NestedSettings, DataSet = "NestedSetting")]
            public NestedSettings TestNestedSetting { get; set; }
            [XMLDataBase.OBJS.XMLDatabaseRetriveItem(Type = XMLDataBase.OBJS.Types.NestedMulitNode, DataSet = "MultiNodes", ItemName = "FileName")]
            public List<MultiNode> MultiNodes { get; set; } = new List<MultiNode>();
            [XMLDataBase.OBJS.XMLDatabaseRetriveItem(Type = XMLDataBase.OBJS.Types.NestedList, DataSet = "NestedAttraFill", ItemName = "Group")]
            public List<And> And { get; set; } = new List<Form1.And>();

        }

        public class MultiNode
        {
            [XMLDataBase.OBJS.XMLDatabaseRetriveItem]
            public string FileName { get; set; }

        }

        public class And
        {
            [XMLDataBase.OBJS.XMLDatabaseRetriveItem(Type = XMLDataBase.OBJS.Types.AttributeFill, DataSet = "Group", NodeName = "AttFill")]
            public List<AttFill> AttFill { get; set; } = new List<Form1.AttFill>();
            [XMLDataBase.OBJS.XMLDatabaseRetriveItem(Type = XMLDataBase.OBJS.Types.AttributeFill, DataSet = "Group", NodeName = "AttFill2")]
            public List<AttFill2> AttFill2 { get; set; } = new List<Form1.AttFill2>();
        }

        public class AttFill
        {
            [XMLDataBase.OBJS.XMLDatabaseRetriveItem(Type = XMLDataBase.OBJS.Types.Attribute, NodeName = "AttFill", AttributeName = "Attr")]
            public string Attr { get; set; }
            [XMLDataBase.OBJS.XMLDatabaseRetriveItem(Type = XMLDataBase.OBJS.Types.Attribute, NodeName = "AttFill", AttributeName = "Sec")]
            public string Sec { get; set; }
        }

        public class AttFill2
        {
            [XMLDataBase.OBJS.XMLDatabaseRetriveItem(Type = XMLDataBase.OBJS.Types.Attribute, NodeName = "AttFill2", AttributeName = "Attr")]
            public string Attr { get; set; }
        }

        public class NestedSettings : XMLDataBase.OBJS.XMLDataDetails
        {
            [XMLDataBase.OBJS.XMLDatabaseRetriveItem]
            public string FileName { get; set; }
            [XMLDataBase.OBJS.XMLDatabaseRetriveItem]
            public string FolderName { get; set; } = "";
            [XMLDataBase.OBJS.XMLDatabaseRetriveItem(Type = XMLDataBase.OBJS.Types.NestedMulitNode, DataSet = "MultiNodes", ItemName = "FileName")]
            public List<MultiNode> MultiNodes { get; set; } = new List<MultiNode>();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            listBox2.Items.Clear();

            XMLDataBase.DataBase DataBase = new XMLDataBase.DataBase();

            XMLDataBase.OBJS.Data DataSet = new XMLDataBase.OBJS.Data();
            DataSet.DataSet = "ThisCanBe";
            DataSet.SetRetriveValue("Name");
            DataSet.SetRetriveValue("Date");
            DataSet.SetRetriveValue("Enviroment");
            DataSet.SetRetriveValue("FucksGiven");

            XMLDataBase.OBJS.Data NestedDataSet = new XMLDataBase.OBJS.Data();
            NestedDataSet.DataSet = "ThisCanBeNested";
            NestedDataSet.SetRetriveValue("Over");
            NestedDataSet.SetRetriveValue("Out");
            NestedDataSet.SetRetriveValue("To");

            DataSet.SetRetriveValue(NestedDataSet);


            List<XMLDataBase.OBJS.Data> FoundData = DataBase.LoadData(DataSet);

            foreach (XMLDataBase.OBJS.Data Data in FoundData)
            {
                listBox1.Items.Add("Name: " + Data.GetValue("Name") + ", Date: " + Data.GetValue("Date") + ", Enviroment " + Data.GetValue("Enviroment") + ", Fucks Given " + Data.GetValue("FucksGiven"));

                foreach (XMLDataBase.OBJS.Data NestedData in Data.GetNestedData())
                {
                    listBox2.Items.Add("Over: " + NestedData.GetValue("Over") + ", Out: " + NestedData.GetValue("Out") + ", To " + NestedData.GetValue("To"));
                }
            }
        }

        private void Load_Click(object sender, EventArgs e)
        {
            XMLDataBase.DataBase DataBase = new XMLDataBase.DataBase();
            List<TestObject> TestObjects = DataBase.LoadDataSet<TestObject>("Test");
            
            foreach (TestObject TestObject in TestObjects)
            {
                TestObject.Name = "This is a new Name";
                TestObject.MultiNodes.Add(new MultiNode() { FileName = "Hello New Home" });
            }

            DataBase.UpdateDataSet<TestObject>("Test", TestObjects);
        }
    }
}
