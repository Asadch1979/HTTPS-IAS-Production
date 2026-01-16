using System.Collections.Generic;

namespace AIS.Models
    {
    public class GroupMenuItemMapping
        {
        public int GROUPMENUITEMMAP_ID { get; set; }
        public int GROUP_ID { get; set; }
        public int MENU_ID { get; set; }
        public List<int> MENU_ITEM_IDs { get; set; }
        public List<int> UNLINK_MENU_ITEM_IDs { get; set; }

        }
    }
