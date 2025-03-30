using System;
using System.IO;
using System.Windows;
//comment
Console.WriteLine("hgvbkj");
namespace NotepadApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void NewFile_Click(object sender, RoutedEventArgs e)
        {
            TextBoxEditor.Clear();
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                string content = File.ReadAllText(filePath);
                TextBoxEditor.Text = content;
            }
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                File.WriteAllText(filePath, TextBoxEditor.Text);
            }
        }
    }
}
