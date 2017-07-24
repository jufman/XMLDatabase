using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XMLDataBase.OBJS
{
    public class XMLDatabaseRetriveItem : Attribute
    {
        public Types Type { get; set; } = Types.Prop;
        public string DataSet { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string NodeName { get; set; } = string.Empty;
        public string AttributeName { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;
        public int ID { get; set; } = -1;

    }

    public enum Types
    {
        NestedSettings,
        NestedList,
        NestedMulitNode,
        Prop,
        MultiProp,
        AttributeFill,
        Attribute,
        NestedItems,
        NodedLocation,
        NodeID
    }
}
