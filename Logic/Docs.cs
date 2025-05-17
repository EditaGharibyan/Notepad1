using System;
using System.Collections.Generic;
using TextEditorWpf.Core;
using System.Windows.Controls;

namespace TextEditorWpf.Logic
{
    public class Document
    {
        List<DocumentElements> elements = new List<DocumentElements>();
        private string  _title;
        public Document( string title)
        {
            _title = title;
        }
        public List<DocumentElements> GetElements
        {
            // get => elements;
            get
            {
                return elements;
            }
        }
        public string Title
        {
            get => _title;
            set=>_title=value;

        }
        public  void Display()
        {
            for (int i = 0; i < elements.Count; i++)
            {
                elements[i].Display();
                Console.WriteLine();
            }
        }
        public  void Add(DocumentElements item)
        {
            elements.Add(item);
        }
        public  void Remove(DocumentElements item)
        {
            if(elements.Contains(item))
            {
                elements.Remove(item);
            }
            else
            {
                Console.WriteLine("No such Item");

            }

        }
    }
}
