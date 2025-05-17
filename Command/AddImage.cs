using TextEditorWpf.Core;
using TextEditorWpf.Logic;
using TextEditorWpf.Command;

namespace TextEditorWpf.Command
{
    public class AddImage: ICommand
    {
        private Document docs;
        private ImageBlock img;
        public AddImage(Document d, ImageBlock img)
        {
            docs = d;
            this.img = img;
        }
        public void Execute()
        {

            docs.Add(img);

        }
        public void Undo()
        {
            docs.Remove(img);
        }
    }
}
