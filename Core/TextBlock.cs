using System;
using System.Collections.Generic;
using TextEditorWpf.Logic;
using TextEditorWpf.Core;

namespace TextEditorWpf.Core
{
    public class TextBlock: DocumentElements
    {
        private string _text;
        public TextBlock(string text)
        {
            _text = text;
        }
        public override string  Display()
        {
            return _text;
        }
        public string Text
        {
            get => _text;
            set => _text = value;
        }
        public override bool Equals(object obj)
        {
            if (obj is TextBlock other)
            {
                return Text == other.Text;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Text?.GetHashCode() ?? 0;
        }
    }
}
