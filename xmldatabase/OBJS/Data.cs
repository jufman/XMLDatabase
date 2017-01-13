
using System.Collections.Generic;
namespace XMLDataBase.OBJS
{
    public class Data
    {
        private List<DataValue> DataValues = new List<DataValue>();
        private List<Data> NestedData = new List<Data>();

        private string dataSet;
        private int iD;
        private string location;
        private string searchClause = string.Empty;

        public string Location
        {
            get { return location; }
            set { location = value; }
        }

        public int ID
        {
            get { return iD; }
            set { iD = value; }
        }

        public string DataSet
        {
            get { return dataSet; }
            set { dataSet = value; }
        }

        public string SearchClause
        {
            get
            {
                return searchClause;
            }

            set
            {
                searchClause = value;
            }
        }

        public void AddData(string Name, string Value)
        {
            DataValue DS = new DataValue();
            DS.Name = Name;
            DS.Value = Value;
            DataValues.Add(DS);
        }

        public void AddData(Data NestedData)
        {
            this.NestedData.Add(NestedData);
        }

        public void AddData(List<OBJS.Data> NestedData)
        {
            this.NestedData.AddRange(NestedData);
        }

        public void SetRetriveValue(string Name)
        {
            DataValue DS = new DataValue();
            DS.Name = Name;
            DataValues.Add(DS);
        }

        public void SetRetriveValue(Data NestedData)
        {
            this.NestedData.Add(NestedData);
        }

        public List<DataValue> GetDataValues()
        {
            return DataValues;
        }

        public List<Data> GetNestedData()
        {
            return NestedData;
        }

        public List<Data> GetNestedData(string DataSet)
        {
            List<Data> NestedDataSet = new List<Data>();

            foreach (Data data in NestedData)
            {
                if (data.DataSet == DataSet)
                {
                    NestedDataSet.Add(data);
                }
            }

            return NestedDataSet;
        }

        public string GetValue(string Name)
        {
            string Value = "";
            foreach (DataValue DataSet in DataValues)
            {
                if (DataSet.Name == Name)
                {
                    Value = DataSet.Value;
                }
            }
            return Value;
        }

        public class DataValue
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }
    }
}
