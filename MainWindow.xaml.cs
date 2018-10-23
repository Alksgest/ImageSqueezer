using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.Win32;
using System.Threading;

namespace ImageSqueezer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ImageWorker ImageWorker;
        private string CurrentFile;
        private List<string> ImageList = new List<string>();
        public MainWindow()
        {
            InitializeComponent();
            RegisterOnEvents();

            LoadSettings();

            ImageWorker = new ImageWorker();
            BitmapImage bitmapImage = new BitmapImage();
        }
        private void RegisterOnEvents()
        {
            TextBoxColorDepth.PreviewTextInput += TextBoxNumberValidation;
            TextBoxHeight.PreviewTextInput += TextBoxNumberValidation;
            TextBoxWidth.PreviewTextInput += TextBoxNumberValidation;

        }

        private void LoadSettings()
        {
        }


        private void TextBoxNumberValidation(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ButtonApply_Click(object sender, RoutedEventArgs e) => ApplySettings();

        private void ApplySettings()
        {
            Int32.TryParse(ComboBoxQuality.Text, out int quality);
            string compression = ComboBoxCompression.Text;
            Int64.TryParse(TextBoxColorDepth.Text, out long colorDepth);
            Int64.TryParse(ComboBoxTransform.Text, out long transform);
            Int32.TryParse(TextBoxWidth.Text, out int width);
            Int32.TryParse(TextBoxHeight.Text, out int height);

            ImageWorker.SetParametres(quality, compression, colorDepth, transform, width, height);

            SaveSettings(quality, compression, colorDepth, transform, width, height);
        }

        private static void SaveSettings(int quality, string compression, long colorDepth, long transform, int width, int height)
        {
            Properties.Settings.Default.Quality = quality;
            Properties.Settings.Default.Compression = compression;
            Properties.Settings.Default.ColorDepth = colorDepth;
            Properties.Settings.Default.Transform = transform;
            Properties.Settings.Default.Width = width;
            Properties.Settings.Default.Height = height;

            Properties.Settings.Default.Save();
        }

        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e) => OpenFile();
        private void OpenFolderButton_Click(object sender, RoutedEventArgs e) => OpenFolder();

        private void OpenFile()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                CurrentFile = dialog.FileName;
            }
            ListViewHandledImages.Items.Clear();
            ListViewHandledImages.Items.Add(new ImageRepresenter(CurrentFile));

            ImageList = new List<string> { CurrentFile };
        }
        private void ButtonHandleImages_Click(object sender, RoutedEventArgs e)
        {
            if (ImageList.Count == 0)
                return;

            foreach (var file in ImageList)
                if (ToggleButtonAsyncState.IsChecked.Value)
                    ThreadPool.QueueUserWorkItem(ImageWorker.DoWork, file);
                else
                    ImageWorker.DoWork(file);
        }

        private void OpenFolder()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ListViewHandledImages.Items.Clear();

                    ImageList = Directory.EnumerateFiles(dialog.SelectedPath, "*.*", SearchOption.TopDirectoryOnly)
                               .Where(fileName =>
                               {
                                   fileName = fileName.ToLower();
                                   return fileName.EndsWith(".jpg") || fileName.EndsWith(".jpeg") || fileName.EndsWith(".png") ||
                                            fileName.EndsWith(".gif") || fileName.EndsWith(".jpe") || fileName.EndsWith(".jfif") ||
                                            fileName.EndsWith(".bmp") || fileName.EndsWith(".dib") || fileName.EndsWith(".rle") ||
                                            fileName.EndsWith(".tiff") || fileName.EndsWith(".tif");
                               }).ToList();

                    foreach (var str in ImageList)
                        ListViewHandledImages.Items.Add(new ImageRepresenter(str));
                }
            }
        }
    }
}
