using TextEditorWpf.Command;
using TextEditorWpf.Logic;
using TextEditorWpf.Core;
namespace TextEditorWpf.Command
{
    class AddText: ICommand
    {
        private Document docs;
        private TextBlock txt;
        public AddText(Document d,TextBlock addtxt)
        {
            docs = d;
            txt=addtxt;
        }
        public void Execute()
        {

            docs.Add(txt);

        }
        public void Undo()
        {
            docs.Remove(txt);
        }
    }
}
