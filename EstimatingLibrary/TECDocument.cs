using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary
{
    public class TECDocument : TECObject
    {
        private DocumentType documentType = DocumentType.Drawing;
        private string title = "";
        private string date = "";

        public DocumentType DocumentType
        {
            get { return documentType; }
            set
            {
                var old = documentType;
                documentType = value;
                notifyCombinedChanged(Change.Edit, "DocumentType", this, value, old);
            }
        }
        public string Title
        {
            get { return title; }
            set
            {
                var old = title;
                title = value;
                notifyCombinedChanged(Change.Edit, "Title", this, value, old);
            }
        }
        public string Date
        {
            get { return date; }
            set
            {
                var old = date;
                date = value;
                notifyCombinedChanged(Change.Edit, "Date", this, value, old);
            }
        }

        public TECDocument(Guid guid) : base(guid) { }
        public TECDocument() : base(Guid.NewGuid()) { }
    }

    public enum DocumentType {
        Drawing=1,
        Specification
    };

}
