using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary
{
    public class TECToDoItem : TECObject
    {
        private string _description = "";
        private bool _isDone = false;

        public string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    var old = _description;
                    _description = value;
                    notifyCombinedChanged(Change.Edit, "Description", this, value, old);
                }
            }
        }
        public bool IsDone
        {
            get { return _isDone; }
            set
            {
                if (_isDone != value)
                {
                    _isDone = value;
                    notifyCombinedChanged(Change.Edit, "IsDone", this, value, !value);
                }
            }
        }

        public TECToDoItem(Guid guid) : base(guid) { }
        public TECToDoItem(Guid guid, string desc) : this(guid)
        {
            this.Description = desc;
        }
    }

    public static class ToDoList
    {
        private static List<string> bidToDoList
        {
            get
            {
                return new List<string>
                {
                    "Fill out bid info",
                    "Populate riser",
                    "Create systems from sequences",
                    "Verify valve selections",
                    "Verify electrical runs",
                    "Verify proposal",
                    "Add systems' miscellaneous costs",
                    "Create system instances",
                    "Create and verify controllers",
                    "Create network electrical runs",
                    "Add any miscellaneous costs for the bid",
                    "Submit quotes"
                };
            }
        }

        public static List<TECToDoItem> BidList
        {
            get
            {
                List<TECToDoItem> list = new List<TECToDoItem>();
                bidToDoList.ForEach((item) => list.Add(new TECToDoItem(Guid.NewGuid(), item)));
                return list;
            }
        }
    }
}
