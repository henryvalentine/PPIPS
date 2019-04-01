
namespace ImportPermitPortal.DataObjects
{
    using System;
    using System.Collections.Generic;
    
    public partial class MessageTemplateObject 
    {
        public int Id { get; set; }
        public int EventTypeId { get; set; }
        public string Subject { get; set; }
        public string MessageContent { get; set; }
        public string Footer { get; set; }
    
        public virtual ICollection<MessageObject> MessageObjects { get; set; }
    }
}

