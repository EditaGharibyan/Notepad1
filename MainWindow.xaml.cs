using System.Windows;
using TextEditorWpf.Core;
using TextEditorWpf.Logic;
using TextEditorWpf.Command;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System;
using System.IO;
using Notepad.Visitor;
using System.Windows.Input;

namespace TextEditorWpf
{
    public partial class MainWindow : Window
    {
        private Document _document;
        private Logic.CommandManager _commandManager;

        public MainWindow()
        {
            InitializeComponent();
            _document = new Document("MyDoc");
            _commandManager = new Logic.CommandManager();
            RefreshDocumentList();
        }

        private void AddTextBlock_Click(object sender, RoutedEventArgs e)
        {
            string text = InputTextBox.Text;

            if (!string.IsNullOrWhiteSpace(text))
            {
                Core.TextBlock tb = new Core.TextBlock(text);
                Command.ICommand obj = new AddText(_document, tb);
                // _document.Add(tb);
                _commandManager.ExecuteeCommand(obj);
                RefreshDocumentList();
            }
        }
        private void Remove_Text(object sender, RoutedEventArgs e)
        {

            string text = InputTextBox.Text;
            
            if (!string.IsNullOrWhiteSpace(text))
            {
                Core.TextBlock tb = new Core.TextBlock(text);
                Command.ICommand obj = new RemoveItem(_document, tb);
                _commandManager.ExecuteeCommand(obj);
                RefreshDocumentList();
            }
        }
        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            _commandManager.UndoLastCommand();
            RefreshDocumentList();
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {

            this.Close();
        }
        private void Remove_Image(object sender, RoutedEventArgs e)
        {
            string text = InputTextBox.Text;

            if (!string.IsNullOrWhiteSpace(text))
            {
                Core.ImageBlock img = new Core.ImageBlock(text);
                Command.ICommand obj = new RemoveItem(_document, img);
                _commandManager.ExecuteeCommand(obj);
                RefreshDocumentList();
            }
        }

        private void AddImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = InputTextBox.Text;
                if (File.Exists(path))
                {
                    var imageElement = new ImageBlock(path);
                    AddImage img=new AddImage(_document, imageElement);
                    _commandManager.ExecuteeCommand(img);
                    RefreshDocumentList();
            }
                else
            {
                throw new Exception("File not excist");
            }
        }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        private void RefreshDocumentList()
        {
            ParagraphListBox.Items.Clear();

            foreach (var item in _document.GetElements)
            {
                ParagraphListBox.Items.Add(Visitor.Visit((dynamic)item));
            }
        }
    }
}
