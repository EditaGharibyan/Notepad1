using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using TextEditorWpf.Core;
using TextEditorWpf.Logic;
using TextEditorWpf.Command;

namespace TextEditorWpf.Command
{
    public class RemoveItem: ICommand
    {
        private TextEditorWpf.Logic.Document docs;
        private DocumentElements t;
        public RemoveItem(Document docs, DocumentElements t)
        {
            this.docs = docs;
            this.t = t;
        }
        public void Execute()
        {
                docs.Remove(t);
        }
        public void Undo()
        {
            docs.Add(t);
        }

    }
}
