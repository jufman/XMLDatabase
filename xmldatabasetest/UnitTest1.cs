using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XMLDataBaseTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            XMLDataBase.DataBase DataBase = new XMLDataBase.DataBase();

            XMLDataBase.OBJS.Data DataSet = new XMLDataBase.OBJS.Data();
            DataSet.DataSet = "ThisIsNew";
            DataSet.AddData("Name", "Test Name");
            DataSet.AddData("ID", "22");


            XMLDataBase.OBJS.Data NestedDataSet = new XMLDataBase.OBJS.Data();
            NestedDataSet.DataSet = "NestedTest";
            NestedDataSet.AddData("SecoundName", "Nake");
            NestedDataSet.AddData("SecoundID", "44");

            //DataSet.AddData(NestedDataSet);


            DataBase.SetData(DataSet);

            DataSet = new XMLDataBase.OBJS.Data();
            DataSet.DataSet = "ThisIsNew";
            DataSet.AddData("Name", "Test TEST");
            DataSet.AddData("ID", "55");

            DataBase.SetData(DataSet);

        }
    }
}
