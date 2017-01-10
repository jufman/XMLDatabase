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

            XMLDataBase.OBJS.Data DataSet = new XMLDataBase.OBJS.Data();
            DataSet.DataSet = "ThisCanBe";
            DataSet.AddData("Name", textBox1.Text);
            DataSet.AddData("Date", dateTimePicker1.Value.ToLongDateString());
            DataSet.AddData("Enviroment", comboBox1.Text);
            DataSet.AddData("FucksGiven", "None");

            XMLDataBase.OBJS.Data NestedDataSet = new XMLDataBase.OBJS.Data();
            NestedDataSet.DataSet = "ThisCanBeNested";
            NestedDataSet.AddData("Over", textBox1.Text);
            NestedDataSet.AddData("Out", dateTimePicker1.Value.ToLongDateString());
            NestedDataSet.AddData("To", comboBox1.Text);

            DataSet.AddData(NestedDataSet);

            DataBase.SetData(DataSet);

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
    }
}
