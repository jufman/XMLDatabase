using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace XMLDataBase
{
    public static class GetObjectLogic
    {

        public static List<T> GetItems<T>(string DataSet, string DefaultItemName, XMLDataBase.DataBase DataBase)
        {
            List<T> Items = new List<T>();

            OBJS.Data DataSetObject = GetDataSet(DataSet, DefaultItemName, typeof(T));

            Items = BuildObjects<T>(DataSetObject, DataBase);

            return Items;
        }

        private static List<T> BuildObjects<T>(OBJS.Data DataSetObject, XMLDataBase.DataBase DataBase)
        {
            List<T> Items = new List<T>();
            try 
            {
                List<OBJS.Data> DataItems = DataBase.LoadData(DataSetObject);

                foreach (OBJS.Data Data in DataItems)
                {
                    Items.Add((T)GetItem(Data, typeof(T)));
                }
            }
            catch (Exception e)
            {
                e.ToString();
            }

            return Items;
        }

        private static object GetItem(OBJS.Data Data, Type NewObject)
        {
            object Item = GetItemObject(NewObject, Data.GetDataValues());

            GetXmlDataDetails(Item, NewObject, Data);

            GetNestedSettings(Item, NewObject, Data);
            GetNestedItems(Item, NewObject, Data);
            GetNestedMulitNodeItems(Item, NewObject, Data);
            GetAttributeFillItems(Item, NewObject, Data);

            return Item;
        }

        private static void GetXmlDataDetails(object Item, Type NewObject, OBJS.Data Data)
        {
            List<PropertyInfo> NodedLocationProps = GetProprites(OBJS.Types.NodedLocation, NewObject);
            if (NodedLocationProps.Count != 0)
            {
                PropertyInfo NodedLocationProp = NodedLocationProps[0];
                NodedLocationProp.SetValue(Item, Data.Location, null);
            }

            List<PropertyInfo> NodeIDProps = GetProprites(OBJS.Types.NodeID, NewObject);
            if (NodeIDProps.Count != 0)
            {
                PropertyInfo NodeIDProp = NodeIDProps[0];
                NodeIDProp.SetValue(Item, Data.ID, null);
            }
        }

        private static void GetNestedSettings(object Item, Type NewObject, OBJS.Data Data)
        {
            List<PropertyInfo> NestedProps = GetProprites(OBJS.Types.NestedSettings, NewObject);

            foreach (PropertyInfo NestedProp in NestedProps)
            {
                OBJS.Data NestedDettingsData;
                OBJS.XMLDatabaseRetriveItem XMLDatabaseRetriveItem = (OBJS.XMLDatabaseRetriveItem)NestedProp.GetCustomAttributes(typeof(OBJS.XMLDatabaseRetriveItem), true).FirstOrDefault();
                if (XMLDatabaseRetriveItem.DataSet != string.Empty)
                {
                    NestedDettingsData = Data.GetSettingsNestedData(XMLDatabaseRetriveItem.DataSet);
                }
                else
                {
                    NestedDettingsData = Data.GetSettingsNestedData(NestedProp.Name);
                }

                object NestedSettingsObject = GetItem(NestedDettingsData, NestedProp.PropertyType);
                NestedProp.SetValue(Item, NestedSettingsObject, null);
            }
        }

        private static void GetNestedItems(object Item, Type NewObject, OBJS.Data Data)
        {
            List<PropertyInfo> NestedProps = GetProprites(OBJS.Types.NestedList, NewObject);

            foreach (PropertyInfo NestedProp in NestedProps)
            {
                OBJS.XMLDatabaseRetriveItem XMLDatabaseRetriveItem = (OBJS.XMLDatabaseRetriveItem)NestedProp.GetCustomAttributes(typeof(OBJS.XMLDatabaseRetriveItem), true).FirstOrDefault();

                string DataSetName = (XMLDatabaseRetriveItem.DataSet == string.Empty) ? NestedProp.Name : XMLDatabaseRetriveItem.DataSet;

                List<OBJS.Data> NestedDettingsData = Data.GetNestedData(DataSetName);

                try
                {
                    foreach (OBJS.Data DataValues in NestedDettingsData)
                    {
                        Type ListType = NestedProp.PropertyType.GetGenericArguments()[0];
                        System.Collections.IList IList = (System.Collections.IList)NestedProp.GetValue(Item, null);

                        IList.Add(GetItem(DataValues, ListType));
                    }
                }
                catch (Exception e)
                {
                    e.ToString();
                }
            }
        }

        private static void GetNestedMulitNodeItems(object Item, Type NewObject, OBJS.Data Data)
        {
            List<PropertyInfo> NestedProps = GetProprites(OBJS.Types.NestedMulitNode, NewObject);

            foreach (PropertyInfo NestedProp in NestedProps)
            {
                OBJS.XMLDatabaseRetriveItem XMLDatabaseRetriveItem = (OBJS.XMLDatabaseRetriveItem)NestedProp.GetCustomAttributes(typeof(OBJS.XMLDatabaseRetriveItem), true).FirstOrDefault();

                string DataSetName = (XMLDatabaseRetriveItem.DataSet == string.Empty) ? NestedProp.Name : XMLDatabaseRetriveItem.DataSet;

                List<OBJS.Data> NestedData = Data.GetNestedData(DataSetName);
                foreach (OBJS.Data NestedSettingsData in NestedData)
                {
                    List<OBJS.Data.DataValue> NestedDettingsData = NestedSettingsData.GetAllNodes(XMLDatabaseRetriveItem.ItemName);

                    Type ListType = NestedProp.PropertyType.GetGenericArguments()[0];

                    System.Collections.IList IList = (System.Collections.IList)NestedProp.GetValue(Item, null);
                    foreach (OBJS.Data.DataValue Node in NestedDettingsData)
                    {
                        IList.Add(GetItemObject(ListType, new List<OBJS.Data.DataValue>() { Node }));
                    }
                }
            }
        }

        private static void GetAttributeFillItems(object Item, Type NewObject, OBJS.Data Data)
        {
            List<PropertyInfo> NestedProps = GetProprites(OBJS.Types.AttributeFill, NewObject);

            foreach (PropertyInfo NestedProp in NestedProps)
            {
                OBJS.XMLDatabaseRetriveItem XMLDatabaseRetriveItem = (OBJS.XMLDatabaseRetriveItem)NestedProp.GetCustomAttributes(typeof(OBJS.XMLDatabaseRetriveItem), true).FirstOrDefault();

                List<OBJS.Data.DataValue> DataItem = Data.GetAllNodes(XMLDatabaseRetriveItem.NodeName);

                if (DataItem.Count == 0) 
                {
                    continue;
                }

                string DataSetName = (XMLDatabaseRetriveItem.DataSet == string.Empty) ? NestedProp.Name : XMLDatabaseRetriveItem.DataSet;

                List<OBJS.Data.DataValue> NestedDettingsData = Data.GetAllNodes(XMLDatabaseRetriveItem.NodeName);

                Type ListType = NestedProp.PropertyType.GetGenericArguments()[0];

                System.Collections.IList IList = (System.Collections.IList)NestedProp.GetValue(Item, null);
                foreach (OBJS.Data.DataValue Node in NestedDettingsData)
                {
                    IList.Add(GetItemObject(ListType, new List<OBJS.Data.DataValue>(){ Node }));
                }
                
            }
        }

        public static object GetItemObject(Type ListType, List<OBJS.Data.DataValue> DataValues)
        {
            object SubItem = Activator.CreateInstance(ListType);
            try
            {
                List<PropertyInfo> ListProp = GetProprites(OBJS.Types.Prop, ListType);

                foreach (PropertyInfo PropInfo in ListProp)
                {
                    OBJS.Data.DataValue Item = DataValues.FirstOrDefault(A => A.Name == PropInfo.Name);
                    if (Item == null)
                    {
                        continue;
                    }

                    OBJS.XMLDatabaseRetriveItem XMLDatabaseRetriveItem = (OBJS.XMLDatabaseRetriveItem)PropInfo.GetCustomAttributes(typeof(OBJS.XMLDatabaseRetriveItem), true).FirstOrDefault();

                    string DataValue = Item.Value;

                    object Value = GetValue(PropInfo.PropertyType, DataValue);
                    PropInfo.SetValue(SubItem, Value, null);
                    XMLDatabaseRetriveItem.Location = Item.Location;
                }

                List<PropertyInfo> AttributePop = GetProprites(OBJS.Types.Attribute, ListType);

                foreach (PropertyInfo PropInfo in AttributePop)
                {
                    OBJS.XMLDatabaseRetriveItem XMLDatabaseRetriveItem = (OBJS.XMLDatabaseRetriveItem)PropInfo.GetCustomAttributes(typeof(OBJS.XMLDatabaseRetriveItem), true).FirstOrDefault();

                    OBJS.Data.DataValue Item = DataValues.FirstOrDefault(A => A.Name == XMLDatabaseRetriveItem.NodeName);
                    if (Item == null)
                    {
                        continue;
                    }

                    OBJS.Data.DataValue DataValueItem = Item.Attributes.FirstOrDefault(B => B.Name == XMLDatabaseRetriveItem.AttributeName);

                    if (DataValueItem == null)
                    {
                        continue;
                    }

                    string DataValue = DataValueItem.Value;

                    object Value = GetValue(PropInfo.PropertyType, DataValue);
                    PropInfo.SetValue(SubItem, Value, null);
                    XMLDatabaseRetriveItem.Location = Item.Location;
                }
            }
            catch (Exception e)
            {
                e.ToString();
            }

            return SubItem;
        }

        private static object GetValue(Type type, string Value)
        {
            switch (type.Name)
            {
                case "Int32":
                    return Int32.Parse(Value);
                case "Int64":
                    return Int64.Parse(Value);
                case "Boolean":
                    return bool.Parse(Value);
                case "Double":
                    return Double.Parse(Value);
                case "DateTime":
                    return DateTime.Parse(Value);
                case "String":
                    return Value.ToString();
                default:
                    return Value.ToString();
            }
        }

        private static OBJS.Data GetDataSet(string DataSetString, string DefaultItemName, Type BaseType)
        {
            OBJS.Data DataSet = new OBJS.Data();
            DataSet.DataSet = DataSetString;
            DataSet.DefaultItemName = DefaultItemName;

            List<PropertyInfo> Props = GetProprites(OBJS.Types.Prop, BaseType);

            foreach (PropertyInfo Prop in Props)
            {
                DataSet.SetRetriveValue(Prop.Name);
            }

            AddAttributes(BaseType, DataSet);
            GetNestedSettingsDataSet(DefaultItemName, BaseType, DataSet);
            GetNestedDataSet(DefaultItemName, BaseType, DataSet);
            GetNestedMulitNodeDataSet(DefaultItemName, BaseType, DataSet);
            GetAttributeFillDataSet(BaseType, DataSet);

            return DataSet;
        }

        private static void AddAttributes(Type BaseType, OBJS.Data DataSet)
        {
            List<PropertyInfo> Props = GetProprites(OBJS.Types.Attribute, BaseType);

            List<string> Values = new List<string>();

            foreach (PropertyInfo Prop in Props)
            {
                OBJS.XMLDatabaseRetriveItem XMLDatabaseRetriveItem = (OBJS.XMLDatabaseRetriveItem)Prop.GetCustomAttributes(typeof(OBJS.XMLDatabaseRetriveItem), true).FirstOrDefault();

                if (Values.Contains(XMLDatabaseRetriveItem.NodeName) == false)
                {
                    Values.Add(XMLDatabaseRetriveItem.NodeName);
                }
            }

            foreach (string Value in Values)
            {
                DataSet.SetRetriveValue(Value);
            }
        }

        private static void GetNestedSettingsDataSet(string DefaultItemName, Type BaseType, OBJS.Data DataSet)
        {
            List<PropertyInfo> NestedSettings = GetProprites(OBJS.Types.NestedSettings, BaseType);

            foreach (PropertyInfo Prop in NestedSettings)
            {
                OBJS.XMLDatabaseRetriveItem XMLDatabaseRetriveItem = (OBJS.XMLDatabaseRetriveItem)Prop.GetCustomAttributes(typeof(OBJS.XMLDatabaseRetriveItem), true).FirstOrDefault();

                string DataSetName = Prop.Name;

                if (XMLDatabaseRetriveItem.DataSet != string.Empty)
                {
                    DataSetName = XMLDatabaseRetriveItem.DataSet;
                }

                Type PropType = Prop.PropertyType;

                OBJS.Data NestedDataSet = GetDataSet(DataSetName, DefaultItemName, PropType);
                NestedDataSet.IsSettingsItem = true;
                DataSet.SetRetriveValue(NestedDataSet);
            }
        }

        private static void GetNestedDataSet(string DefaultItemName, Type BaseType, OBJS.Data DataSet)
        {
            List<PropertyInfo> NestedSettings = GetProprites(OBJS.Types.NestedList, BaseType);

            foreach (PropertyInfo Prop in NestedSettings)
            {
                OBJS.XMLDatabaseRetriveItem XMLDatabaseRetriveItem = (OBJS.XMLDatabaseRetriveItem)Prop.GetCustomAttributes(typeof(OBJS.XMLDatabaseRetriveItem), true).FirstOrDefault();

                string DataSetName = (XMLDatabaseRetriveItem.DataSet == string.Empty) ? Prop.Name : XMLDatabaseRetriveItem.DataSet;
                DefaultItemName = (XMLDatabaseRetriveItem.ItemName != string.Empty) ? XMLDatabaseRetriveItem.ItemName : DefaultItemName;

                Type ListType = Prop.PropertyType.GetGenericArguments()[0];
                OBJS.Data NestedDataSet = GetDataSet(DataSetName, DefaultItemName, ListType);
                DataSet.SetRetriveValue(NestedDataSet);
            }
        }

        private static void GetNestedMulitNodeDataSet(string DefaultItemName, Type BaseType, OBJS.Data DataSet)
        {
            List<PropertyInfo> NestedMulitNode = GetProprites(OBJS.Types.NestedMulitNode, BaseType);

            foreach (PropertyInfo Prop in NestedMulitNode)
            {
                OBJS.XMLDatabaseRetriveItem XMLDatabaseRetriveItem = (OBJS.XMLDatabaseRetriveItem)Prop.GetCustomAttributes(typeof(OBJS.XMLDatabaseRetriveItem), true).FirstOrDefault();

                string DataSetName = (XMLDatabaseRetriveItem.DataSet == string.Empty) ? Prop.Name : XMLDatabaseRetriveItem.DataSet;
                DefaultItemName = (XMLDatabaseRetriveItem.ItemName != string.Empty) ? XMLDatabaseRetriveItem.ItemName : DefaultItemName;

                Type ListType = Prop.PropertyType.GetGenericArguments()[0];
                OBJS.Data NestedDataSet = GetDataSet(DataSetName, DefaultItemName, ListType);
                NestedDataSet.IsSettingsItem = true;
                DataSet.SetRetriveValue(NestedDataSet);
            }
        }

        private static void GetAttributeFillDataSet(Type BaseType, OBJS.Data DataSet)
        {
            List<PropertyInfo> AttributeFillNodes = GetProprites(OBJS.Types.AttributeFill, BaseType);

            foreach (PropertyInfo Prop in AttributeFillNodes)
            {
                OBJS.XMLDatabaseRetriveItem XMLDatabaseRetriveItem = (OBJS.XMLDatabaseRetriveItem)Prop.GetCustomAttributes(typeof(OBJS.XMLDatabaseRetriveItem), true).FirstOrDefault();

                DataSet.SetRetriveValue(XMLDatabaseRetriveItem.NodeName);
            }
        }

        private static List<PropertyInfo> GetProprites(OBJS.Types Type, Type BaseType)
        {
            List<PropertyInfo> Props = new List<PropertyInfo>();

            List<PropertyInfo> RawPropertyInfos = new List<PropertyInfo>(BaseType.GetProperties());

            List<PropertyInfo> PropertyInfo = new List<PropertyInfo>(RawPropertyInfos.Where(A => A.GetCustomAttributes(typeof(OBJS.XMLDatabaseRetriveItem), true).Where(B => ((OBJS.XMLDatabaseRetriveItem)B).Type == Type).Any()));

            return PropertyInfo;
        }

    }
}
