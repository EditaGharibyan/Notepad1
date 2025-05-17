using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextEditorWpf.Core
{
    public abstract class DocumentElements
    {
        public abstract string Display();
        public virtual void RemoveItem(DocumentElements item)
        {
            
        }
        public virtual void Add(DocumentElements item) {

        }

    }
}
