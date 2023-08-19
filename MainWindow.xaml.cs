using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace myVid
{
    public partial class MainWindow : Window
    {
        private List<string> mediaFiles = new List<string>();
        private List<string> filteredMediaFiles = new List<string>();
        private List<string> selectedFiles = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            LoadMediaFilesFromDatabase();
            DisplayAllMediaFiles(); // Populate the mediaListBox with all media files at startup
        }


        private void DisplayAllMediaFiles()
        {
            mediaListBox.ItemsSource = null; // Clear the ItemsSource
            mediaListBox.ItemsSource = mediaFiles; // Assign the new ItemsSource
            searchTextBox.Text = ""; // Clear the search text
        }

        private void BackToListButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayAllMediaFiles();
            searchTextBox.Text = ""; // Clear the search text
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateFilteredMediaFiles();
        }

        private void UpdateFilteredMediaFiles()
        {
            try
            {
                if (searchTextBox != null && mediaFiles != null)
                {
                    string searchTerm = searchTextBox.Text?.ToLower() ?? "";
                    filteredMediaFiles = mediaFiles?.Where(file => file.ToLower().Contains(searchTerm)).ToList() ?? new List<string>();

                    if (mediaListBox != null)
                    {
                        mediaListBox.ItemsSource = filteredMediaFiles;

                        if (filteredMediaFiles.Count == 0)
                        {
                            MessageBox.Show("No matches found.", "Search Result", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadMediaFilesFromDatabase()
        {
            string json = File.ReadAllText(GetDatabasePath());
            mediaFiles = JsonConvert.DeserializeObject<List<string>>(json);
        }

        private string GetDatabasePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "myVidDatabase.json");
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateFilteredMediaFiles();
        }

        private void MediaListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string fileOrFolder in files)
                {
                    if (File.Exists(fileOrFolder) && IsMediaFile(fileOrFolder))
                    {
                        // Add individual media files
                        mediaFiles.Add(fileOrFolder);
                    }
                    else if (Directory.Exists(fileOrFolder))
                    {
                        // Add all media files from the folder
                        string[] folderFiles = Directory.GetFiles(fileOrFolder, "*", SearchOption.AllDirectories)
                                                      .Where(IsMediaFile).ToArray();
                        mediaFiles.AddRange(folderFiles);
                    }
                }

                SaveMediaFilesToDatabase();
                DisplayAllMediaFiles();
            }
        }

        private void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            if (mediaListBox.SelectedItem != null)
            {
                string selectedMediaFile = mediaListBox.SelectedItem.ToString();
                mediaFiles.Remove(selectedMediaFile);
                filteredMediaFiles.Remove(selectedMediaFile);
                SaveMediaFilesToDatabase();
                UpdateFilteredMediaFiles();
            }
        }

        private void ClearListButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to clear the list?", "Confirm Clear", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                mediaFiles.Clear();
                filteredMediaFiles.Clear();
                SaveMediaFilesToDatabase();
                DisplayAllMediaFiles();
            }
        }

        private bool IsMediaFile(string filePath)
        {
            string extension = Path.GetExtension(filePath)?.ToLower();
            return extension == ".mp4" || extension == ".avi" || extension == ".mkv";
        }

        private void MediaListBoxItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            string selectedItem = (sender as ListBoxItem)?.Content?.ToString();
            if (!string.IsNullOrEmpty(selectedItem) && IsMediaFile(selectedItem))
            {
                try
                {
                    // Update this path to point to the VLC Player executable on your system
                    string vlcPlayerPath = @"C:\Program Files\VideoLAN\VLC\vlc.exe";

                    // Launch VLC Player with the selected video file
                    System.Diagnostics.Process.Start(vlcPlayerPath, $"\"{selectedItem}\"");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveMediaFilesToDatabase()
        {
            string json = JsonConvert.SerializeObject(mediaFiles, Formatting.Indented);
            File.WriteAllText(GetDatabasePath(), json);
        }
    }
}
