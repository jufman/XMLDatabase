using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XMLDataBase.OBJS
{
    public class XMLDataDetails
    {
        [XMLDatabaseRetriveItem(Type = Types.NodedLocation)]
        public string _ItemLocation { get; set; }
        [XMLDatabaseRetriveItem(Type = Types.NodeID)]
        public int _ItemID { get; set; } = -1;

    }
}
