using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextEditorWpf.Command;
namespace TextEditorWpf.Command
{
    public class ExitCom:ICommand
    {
        public void Execute()
        {
            Console.WriteLine("Program End");
        }
        public void Undo() { }

    }
}
