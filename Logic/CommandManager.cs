using System;
using System.Collections.Generic;
using TextEditorWpf.Command;
using TextEditorWpf.Logic;
using TextEditorWpf.Core;

namespace TextEditorWpf.Logic
{
    public  class CommandManager
    {
        private   Stack<ICommand>_undo=new Stack<ICommand>();
        public  void ExecuteeCommand(ICommand obj)
        {
            obj.Execute();
            if (obj is PrintDocument || obj is Command.ExitCom) return;
            _undo.Push(obj);
        }
        public  void UndoLastCommand()
        {
            if (_undo.Count == 0) return;
            ICommand temp = _undo.Pop();
            temp.Undo();
        }
    }

}
