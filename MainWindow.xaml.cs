
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.Win32;
using System.Timers;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Linq;
using System.Collections.Generic;

namespace SmartEditor
{
    public partial class MainWindow : Window
    {
        private readonly string defaultKey = "mysecretkey"; // Fallback key
        private Timer autoSaveTimer;
        private string currentFilePath;
        private ObservableCollection<FileInfo> recentFiles;

        // DLL Imports (unchanged)
        [DllImport("SecurityDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Encrypt(IntPtr input, IntPtr output, int length, IntPtr key);

        [DllImport("SecurityDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Decrypt(IntPtr input, IntPtr output, int length, IntPtr key);

        public MainWindow()
        {
            InitializeComponent();
            recentFiles = new ObservableCollection<FileInfo>();
            DataContext = this;
            SetupAutoSave();
            UpdateWordCount();
            Editor.IsUndoEnabled = true;
        }

        public ObservableCollection<FileInfo> RecentFiles
        {
            get { return recentFiles; }
        }

        private void SetupAutoSave()
        {
            autoSaveTimer = new Timer(30000);
            autoSaveTimer.Elapsed += (s, e) =>
            {
                if (!string.IsNullOrEmpty(currentFilePath))
                {
                    Dispatcher.Invoke(() => SaveFile(currentFilePath));
                }
            };
            autoSaveTimer.Start();
        }

        private void NewFile_Click(object sender, RoutedEventArgs e)
        {
            Editor.Document.Blocks.Clear();
            currentFilePath = null;
            UpdateWordCount();
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "SmartEditor Files (*.smd)|*.smd" };
            if (openFileDialog.ShowDialog() == true)
            {
                string password = PasswordTextBox.Text.Trim();
                if (string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Please enter a password.");
                    return;
                }

                try
                {
                    byte[] fileData = File.ReadAllBytes(openFileDialog.FileName);
                    if (fileData.Length < 16)
                    {
                        MessageBox.Show("File is too small or corrupted.");
                        return;
                    }

                    // Separate encrypted content and hash
                    byte[] encryptedData = fileData.Take(fileData.Length - 32).ToArray();
                    byte[] storedHashBytes = fileData.Skip(fileData.Length - 32).ToArray();
                    string storedHash = Encoding.UTF8.GetString(storedHashBytes);

                    // Compute hash of encrypted data
                    string computedHash = ComputeMD5Hash(encryptedData);

                    if (computedHash != storedHash)
                    {
                        MessageBox.Show("File integrity check failed. File may be corrupted.");
                        return;
                    }

                    string encryptedText = Encoding.UTF8.GetString(encryptedData);
                    IntPtr inputPtr = Marshal.StringToHGlobalAnsi(encryptedText);
                    IntPtr outputPtr = Marshal.AllocHGlobal(encryptedText.Length);
                    IntPtr keyPtr = Marshal.StringToHGlobalAnsi(GetDynamicKey(password));

                    try
                    {
                        Decrypt(inputPtr, outputPtr, encryptedText.Length, keyPtr);
                        string decryptedText = Marshal.PtrToStringAnsi(outputPtr, encryptedText.Length);

                        Editor.Document.Blocks.Clear();
                        Editor.Document.Blocks.Add(new Paragraph(new Run(decryptedText)));
                        currentFilePath = openFileDialog.FileName;
                        UpdateWordCount();
                        AddToRecentFiles(currentFilePath);
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(inputPtr);
                        Marshal.FreeHGlobal(outputPtr);
                        Marshal.FreeHGlobal(keyPtr);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening file: {ex.Message}\nStack Trace: {ex.StackTrace}");
                }
            }
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                SaveAsFile_Click(sender, e);
            }
            else
            {
                SaveFile(currentFilePath);
            }
        }

        private void SaveAsFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog { Filter = "SmartEditor Files (*.smd)|*.smd" };
            if (saveFileDialog.ShowDialog() == true)
            {
                currentFilePath = saveFileDialog.FileName;
                SaveFile(currentFilePath);
                AddToRecentFiles(currentFilePath);
            }
        }

        private void SaveFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            string password = PasswordTextBox.Text.Trim();
            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter a password.");
                return;
            }

            try
            {
                TextRange range = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd);
                string text = range.Text;
                string dynamicKey = GetDynamicKey(password);

                IntPtr inputPtr = Marshal.StringToHGlobalAnsi(text);
                IntPtr outputPtr = Marshal.AllocHGlobal(text.Length);
                IntPtr keyPtr = Marshal.StringToHGlobalAnsi(dynamicKey);

                try
                {
                    Encrypt(inputPtr, outputPtr, text.Length, keyPtr);
                    string encryptedText = Marshal.PtrToStringAnsi(outputPtr, text.Length);
                    byte[] encryptedBytes = Encoding.UTF8.GetBytes(encryptedText);
                    string hash = ComputeMD5Hash(encryptedBytes);
                    byte[] hashBytes = Encoding.UTF8.GetBytes(hash);
                    byte[] finalData = encryptedBytes.Concat(hashBytes).ToArray();

                    File.WriteAllBytes(filePath, finalData);
                    StatusText.Text = $"Saved successfully at {DateTime.Now}";
                }
                finally
                {
                    Marshal.FreeHGlobal(inputPtr);
                    Marshal.FreeHGlobal(outputPtr);
                    Marshal.FreeHGlobal(keyPtr);
                }
            }
            catch (DllNotFoundException ex)
            {
                StatusText.Text = "Error: DLL not found.";
                MessageBox.Show($"DLL not found: {ex.Message}\nEnsure SecurityDLL.dll is in {AppDomain.CurrentDomain.BaseDirectory}");
            }
            catch (BadImageFormatException ex)
            {
                StatusText.Text = "Error: Platform mismatch.";
                MessageBox.Show($"Bad image format: {ex.Message}\nCheck platform compatibility (x86/x64) between DLL and app.");
            }
            catch (EntryPointNotFoundException ex)
            {
                StatusText.Text = "Error: Encrypt function not found.";
                MessageBox.Show($"Entry point not found: {ex.Message}\nVerify Encrypt function in SecurityDLL.dll.");
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error: Save failed.";
                MessageBox.Show($"Error saving file: {ex.Message}\nStack Trace: {ex.StackTrace}");
            }
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            Editor.Document.Blocks.Clear();
            UpdateWordCount();
        }

        private void FindReplace_Click(object sender, RoutedEventArgs e)
        {
            Window findReplaceWindow = new Window
            {
                Title = "Find and Replace",
                Width = 300,
                Height = 150,
                Content = new StackPanel
                {
                    Children =
                    {
                        new TextBox { Name = "FindText", Margin = new Thickness(5) },
                        new TextBox { Name = "ReplaceText", Margin = new Thickness(5) },
                        new Button { Content = "Replace All", Margin = new Thickness(5) }
                    }
                }
            };
            var replaceButton = ((StackPanel)findReplaceWindow.Content).Children[2] as Button;
            var findTextBox = ((StackPanel)findReplaceWindow.Content).Children[0] as TextBox;
            var replaceTextBox = ((StackPanel)findReplaceWindow.Content).Children[1] as TextBox;

            replaceButton.Click += (s, ev) =>
            {
                TextRange range = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd);
                range.Text = range.Text.Replace(findTextBox.Text, replaceTextBox.Text);
                UpdateWordCount();
            };
            findReplaceWindow.ShowDialog();
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            if (Editor.CanUndo)
            {
                Editor.Undo();
            }
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            if (Editor.CanRedo)
            {
                Editor.Redo();
            }
        }

        private void RecentFilesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RecentFilesComboBox.SelectedItem is FileInfo selectedFile)
            {
                try
                {
                    byte[] fileData = File.ReadAllBytes(selectedFile.FullName);
                    if (fileData.Length < 32)
                    {
                        MessageBox.Show("File is too small or corrupted.");
                        return;
                    }

                    byte[] encryptedData = fileData.Take(fileData.Length - 32).ToArray();
                    byte[] storedHashBytes = fileData.Skip(fileData.Length - 32).ToArray();
                    string storedHash = Encoding.UTF8.GetString(storedHashBytes);

                    string computedHash = ComputeMD5Hash(encryptedData);

                    if (computedHash != storedHash)
                    {
                        MessageBox.Show("File integrity check failed. File may be corrupted.");
                        return;
                    }

                    string password = PasswordTextBox.Text.Trim();
                    if (string.IsNullOrEmpty(password))
                    {
                        MessageBox.Show("Please enter a password.");
                        return;
                    }

                    string encryptedText = Encoding.UTF8.GetString(encryptedData);
                    IntPtr inputPtr = Marshal.StringToHGlobalAnsi(encryptedText);
                    IntPtr outputPtr = Marshal.AllocHGlobal(encryptedText.Length);
                    IntPtr keyPtr = Marshal.StringToHGlobalAnsi(GetDynamicKey(password));

                    try
                    {
                        Decrypt(inputPtr, outputPtr, encryptedText.Length, keyPtr);
                        string decryptedText = Marshal.PtrToStringAnsi(outputPtr, encryptedText.Length);

                        Editor.Document.Blocks.Clear();
                        Editor.Document.Blocks.Add(new Paragraph(new Run(decryptedText)));
                        currentFilePath = selectedFile.FullName;
                        UpdateWordCount();
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(inputPtr);
                        Marshal.FreeHGlobal(outputPtr);
                        Marshal.FreeHGlobal(keyPtr);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening recent file: {ex.Message}\nStack Trace: {ex.StackTrace}");
                }
            }
        }

        private void PasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PasswordPlaceholder.Visibility = string.IsNullOrEmpty(PasswordTextBox.Text) ? Visibility.Visible : Visibility.Hidden;
        }

        private void AddToRecentFiles(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var existingFile = recentFiles.FirstOrDefault(f => f.FullName == fileInfo.FullName);
            if (existingFile != null)
            {
                recentFiles.Remove(existingFile);
            }
            recentFiles.Insert(0, fileInfo);
            if (recentFiles.Count > 5)
            {
                recentFiles.RemoveAt(recentFiles.Count - 1);
            }
        }

        private void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateWordCount();
            SuggestCorrections();
        }

        private void UpdateWordCount()
        {
            TextRange range = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd);
            string text = range.Text.Trim();
            int wordCount = string.IsNullOrEmpty(text) ? 0 : Regex.Split(text, @"\s+").Length;
            int charCount = text.Length;
            StatusText.Text = $"Words: {wordCount} | Characters: {charCount}";
        }

        private void SuggestCorrections()
        {
            TextRange range = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd);
            string text = range.Text;

            var corrections = new Dictionary<string, string>
            {
                { "teh", "the" },
                { "dont", "don't" },
                { "i ", "I " },
                { "im ", "I'm " },
                { "cant", "can't" }
            };

            foreach (var correction in corrections)
            {
                if (text.Contains(correction.Key))
                {
                    text = text.Replace(correction.Key, correction.Value);
                    range.Text = text;
                }
            }
        }

        private string GetDynamicKey(string password)
        {
            // Use only the password to generate the key, removing timestamp dependency
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash).Substring(0, 16); // Fixed length for XOR
            }
        }

        private string ComputeMD5Hash(byte[] data)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(data);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
