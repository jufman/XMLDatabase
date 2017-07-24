using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace XMLDataBase
{
    public static class SetObjectLogic
    {

        public static void AddNewObjects<T>(List<T> Itens, string DataSet, string DefaultItemName, XMLDataBase.DataBase DataBase)
        {
            List<OBJS.Data> DataSetItems = GetDataObject(DataSet, DefaultItemName, Itens);

            DataBase.SetData(DataSetItems);
        }

        public static void UpdateObjects<T>(List<T> Items, string DataSet, string DefaultItemName, XMLDataBase.DataBase DataBase)
        {
            foreach (T Item in Items)
            {
                DeleteDataItem(typeof(T), Item, DataBase);
            }

            AddNewObjects(Items, DataSet, DefaultItemName, DataBase);
        }

        public static void DeleteDataItems<T>(List<T> Items, string DataSet, string DefaultItemName, XMLDataBase.DataBase DataBase)
        {
            foreach (T Item in Items)
            {
                DeleteDataItem(typeof(T), Item, DataBase);
            }
        }

        private static void DeleteDataItem(Type BaseType, object Item, XMLDataBase.DataBase DataBase)
        {
            List<PropertyInfo> NodedLocationProps = GetProprites(OBJS.Types.NodedLocation, BaseType);
            if (NodedLocationProps.Count != 0)
            {
                PropertyInfo NodeLocation = NodedLocationProps[0];

                object ItemObject = NodeLocation.GetValue(Item, null);
                string ItemValue = (ItemObject != null) ? ItemObject.ToString() : "";

                DataBase.DeleteItem(ItemValue);
            }

            List<PropertyInfo> NodeIDProps = GetProprites(OBJS.Types.NodeID, BaseType);
            if (NodeIDProps.Count != 0)
            {
                PropertyInfo NodeIDProp = NodeIDProps[0];

                object ItemObject = NodeIDProp.GetValue(Item, null);
                int ItemID = (ItemObject != null) ? (int) ItemObject : -1;

            }
        }

        private static List<OBJS.Data> GetDataObject<T>(string DataSetString, string DefaultItemName, List<T> Items)
        {
            List<OBJS.Data> DataSet = new List<OBJS.Data>();

            foreach (T Item in Items)
            {
                DataSet.Add(BuildData(typeof(T), Item, DataSetString, DefaultItemName));
            }

            return DataSet;
        }

        private static OBJS.Data BuildData(Type BaseType, object Item, string DataSetString, string DefaultItemName)
        {
            OBJS.Data DataSet = new OBJS.Data();
            DataSet.DataSet = DataSetString;
            DataSet.DefaultItemName = DefaultItemName;

            FillData(DataSet, BaseType, Item);

            FillNestedSettings(DataSet, BaseType, Item, DefaultItemName);
            FillNestedData(DataSet, BaseType, Item, DefaultItemName);
            FillNestedMultiNode(DataSet, BaseType, Item, DefaultItemName);
            FillAttributeFillData(DataSet, BaseType, Item, DefaultItemName);

            return DataSet;
        }

        private static void FillAttributeFillData(OBJS.Data DataSet, Type BaseType, object Item, string DefaultItemName)
        {
            List<PropertyInfo> NestedProps = GetProprites(OBJS.Types.AttributeFill, BaseType);

            if (NestedProps.Count == 0)
            {
                return;
            }

            foreach (PropertyInfo NestedProp in NestedProps)
            {
                OBJS.XMLDatabaseRetriveItem XMLDatabaseRetriveItem = (OBJS.XMLDatabaseRetriveItem)NestedProp.GetCustomAttributes(typeof(OBJS.XMLDatabaseRetriveItem), true).FirstOrDefault();

                string DataSetName = (XMLDatabaseRetriveItem.DataSet == string.Empty) ? NestedProp.Name : XMLDatabaseRetriveItem.DataSet;
                DefaultItemName = (XMLDatabaseRetriveItem.ItemName != string.Empty) ? XMLDatabaseRetriveItem.ItemName : DefaultItemName;

                Type ListType = NestedProp.PropertyType.GetGenericArguments()[0];

                System.Collections.IList IList = (System.Collections.IList)NestedProp.GetValue(Item, null);
                if (IList != null)
                {
                    foreach (Object ListItem in IList)
                    {
                        FillData(DataSet, ListType, ListItem);
                    }
                }
            }
        }

        private static void FillNestedMultiNode(OBJS.Data DataSet, Type BaseType, object Item, string DefaultItemName)
        {
            List<PropertyInfo> NestedProps = GetProprites(OBJS.Types.NestedMulitNode, BaseType);

            foreach (PropertyInfo NestedProp in NestedProps)
            {
                OBJS.XMLDatabaseRetriveItem XMLDatabaseRetriveItem = (OBJS.XMLDatabaseRetriveItem)NestedProp.GetCustomAttributes(typeof(OBJS.XMLDatabaseRetriveItem), true).FirstOrDefault();

                string DataSetName = (XMLDatabaseRetriveItem.DataSet == string.Empty) ? NestedProp.Name : XMLDatabaseRetriveItem.DataSet;
                DefaultItemName = (XMLDatabaseRetriveItem.ItemName != string.Empty) ? XMLDatabaseRetriveItem.ItemName : DefaultItemName;

                Type ListType = NestedProp.PropertyType.GetGenericArguments()[0];

                System.Collections.IList IList = (System.Collections.IList)NestedProp.GetValue(Item, null);
                if (IList != null)
                {
                    OBJS.Data Data = new OBJS.Data();
                    Data.DataSet = DataSetName;
                    Data.IsSettingsItem = true;

                    foreach (Object ListItem in IList)
                    {
                        FillData(Data, ListType, ListItem);
                    }
                    DataSet.SetRetriveValue(Data);
                }
            }
        }

        private static void FillNestedData(OBJS.Data DataSet, Type BaseType, object Item, string DefaultItemName)
        {
            List<PropertyInfo> NestedSettings = GetProprites(OBJS.Types.NestedList, BaseType);

            foreach (PropertyInfo Prop in NestedSettings)
            {
                OBJS.XMLDatabaseRetriveItem XMLDatabaseRetriveItem = (OBJS.XMLDatabaseRetriveItem)Prop.GetCustomAttributes(typeof(OBJS.XMLDatabaseRetriveItem), true).FirstOrDefault();

                string DataSetName = (XMLDatabaseRetriveItem.DataSet == string.Empty) ? Prop.Name : XMLDatabaseRetriveItem.DataSet;
                DefaultItemName = (XMLDatabaseRetriveItem.ItemName != string.Empty) ? XMLDatabaseRetriveItem.ItemName : DefaultItemName;

                Type ListType = Prop.PropertyType.GetGenericArguments()[0];

                System.Collections.IList IList = (System.Collections.IList)Prop.GetValue(Item, null);
                if (IList != null)
                {
                    foreach (Object ListItem in IList)
                    {
                        OBJS.Data NestedDataSet = BuildData(ListType, ListItem, DataSetName, DefaultItemName);
                        DataSet.SetRetriveValue(NestedDataSet);
                    }
                }
            }
        }

        private static void FillNestedSettings(OBJS.Data DataSet, Type BaseType, object Item, string DefaultItemName)
        {
            List<PropertyInfo> NestedSettings = GetProprites(OBJS.Types.NestedSettings, BaseType);

            foreach (PropertyInfo Prop in NestedSettings)
            {
                OBJS.XMLDatabaseRetriveItem XMLDatabaseRetriveItem = (OBJS.XMLDatabaseRetriveItem)Prop.GetCustomAttributes(typeof(OBJS.XMLDatabaseRetriveItem), true).FirstOrDefault();

                string DataSetName = (XMLDatabaseRetriveItem.DataSet == string.Empty) ? Prop.Name : XMLDatabaseRetriveItem.DataSet;
                DefaultItemName = (XMLDatabaseRetriveItem.ItemName != string.Empty) ? XMLDatabaseRetriveItem.ItemName : DefaultItemName;

                Type PropType = Prop.PropertyType;
                object NestedObject = Prop.GetValue(Item, null);

                if (NestedObject == null)
                {
                    continue;
                }

                OBJS.Data NestedDataSet = BuildData(PropType, NestedObject, DataSetName, DefaultItemName);
                NestedDataSet.IsSettingsItem = true;
                DataSet.AddData(NestedDataSet);
            }
        }

        private static void FillData(OBJS.Data DataSet, Type BaseType, object Item)
        {
            List<PropertyInfo> Props = GetProprites(OBJS.Types.Prop, BaseType);

            List<OBJS.Data.DataValue> DataValues = new List<OBJS.Data.DataValue>();

            foreach (PropertyInfo Prop in Props)
            {
                object ItemObject = Prop.GetValue(Item, null);
                string ItemValue = (ItemObject != null) ? ItemObject.ToString() : "";

                OBJS.Data.DataValue DataValue = new OBJS.Data.DataValue();
                DataValue.Name = Prop.Name;
                DataValue.Value = ItemValue;

                DataValues.Add(DataValue);
            }


            List<PropertyInfo> AttributePop = GetProprites(OBJS.Types.Attribute, BaseType);
            
            foreach (PropertyInfo PropInfo in AttributePop)
            {
                OBJS.XMLDatabaseRetriveItem XMLDatabaseRetriveItem = (OBJS.XMLDatabaseRetriveItem)PropInfo.GetCustomAttributes(typeof(OBJS.XMLDatabaseRetriveItem), true).FirstOrDefault();
                object ItemObject = PropInfo.GetValue(Item, null);
                string ItemValue = (ItemObject != null) ? ItemObject.ToString() : "";

                OBJS.Data.DataValue DataValue = DataValues.Find(A => A.Name == XMLDatabaseRetriveItem.NodeName);

                if (DataValue != null)
                {
                    DataValue.Attributes.Add(new OBJS.Data.DataValue()
                    {
                        Name = XMLDatabaseRetriveItem.AttributeName,
                        Value = ItemValue
                    });
                }
                else
                {
                    DataValue = new OBJS.Data.DataValue();
                    DataValue.Name = XMLDatabaseRetriveItem.NodeName;

                    DataValue.Attributes.Add(new OBJS.Data.DataValue()
                    {
                        Name = XMLDatabaseRetriveItem.AttributeName,
                        Value = ItemValue
                    });
                    DataValues.Add(DataValue);
                }
            }

            DataValues.ForEach(T =>
            {
                DataSet.AddData(T);
            });
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
