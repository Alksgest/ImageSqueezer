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
        public enum MainWindowEncoderState
        {
            JPEG = 0,
            TIFF,
            PNG,
            GIF,
            BMP,
            NONE
        }
        private ImageWorker ImageWorker;
        private string CurrentFile;
        private List<string> ImageList = new List<string>();
        private List<string> ImageListToEncod = new List<string>();
        public MainWindowEncoderState MainState { get; private set; }
        private string EncoderStateString = null;
        public MainWindow()
        {
            InitializeComponent();
            RegisterOnEvents();

            LoadSettings();

            MainState = MainWindowEncoderState.NONE;
            ImageWorker = new ImageWorker();
            BitmapImage bitmapImage = new BitmapImage();
        }
        private void RegisterOnEvents()
        {
            TextBoxHeight.PreviewTextInput += TextBoxNumberValidation;
            TextBoxWidth.PreviewTextInput += TextBoxNumberValidation;
            ComboBoxTypeOfEncoders.SelectionChanged += ComboBoxTypeOfEncoders_SelectionChanged;
            ListViewHandledImages.SelectionChanged += ListViewHandledImages_SelectionChanged;
        }

        private void ListViewHandledImages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var uriPath = new Uri(((sender as ListView).SelectedItem as ImageRepresenter).Path);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = uriPath;
            bitmap.DecodePixelWidth = (int)ImageOverview.Width;
            bitmap.DecodePixelHeight = (int)ImageOverview.Height;
            bitmap.EndInit();

            ImageOverview.Source = bitmap;

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
            //Color Depth - 1, 4, 8, 24;
            long quality = 0;
            long colorDepth = 0;
            string compression = "";
            long transform = 0;
            int width = 0;
            int height = 0;



            if (ComboBoxQuality.IsEnabled)
                Int64.TryParse(ComboBoxQuality.Text, out quality);
            if (ComboBoxColorDepth.IsEnabled)
                Int64.TryParse(ComboBoxColorDepth.Text, out colorDepth);
            if (ComboBoxCompression.IsEnabled)
                compression = ComboBoxCompression.Text;

            if (ComboBoxTransform.IsEnabled)
            {
                Int64.TryParse(ComboBoxTransform.Text, out transform);
                if (transform == 0)
                    switch (ComboBoxTransform.Text)
                    {
                        case "Flip horizontal":
                            transform = 0;
                            break;
                        case "Flip vertical":
                            transform = 1;
                            break;
                    }
            }

            if (TextBoxWidth.IsEnabled)
                Int32.TryParse(TextBoxWidth.Text, out width);
            if (TextBoxHeight.IsEnabled)
                Int32.TryParse(TextBoxHeight.Text, out height);

            ImageWorker.SetParametres(quality, compression, colorDepth, transform, width, height);

            SaveSettings(quality, compression, colorDepth, transform, width, height);
        }

        private static void SaveSettings(long quality, string compression, long colorDepth, long transform, int width, int height)
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
            if (!CheckBoxEncodeAllFiles.IsChecked.Value)
            {
                foreach (ImageRepresenter item in ListViewHandledImages.SelectedItems)
                    ImageListToEncod.Add(item.ToString());
            }
            else
                ImageListToEncod = ImageList;

            if (ToggleButtonAsyncState.IsChecked.Value)
            {
                foreach (var file in ImageListToEncod)
                {
                    List<string> list = new List<string> { file, EncoderStateString };
                    ThreadPool.QueueUserWorkItem(ImageWorker.DoWork, list);
                }
            }
            else
            {
                foreach (var file in ImageListToEncod)
                {
                    List<string> list = new List<string> { file, EncoderStateString };
                    ImageWorker.DoWork(list);
                }
            }
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

        private void ComboBoxTypeOfEncoders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainWindowEncoderStateChanged();
        }

        private void MainWindowEncoderStateChanged()
        {
            LockControls();
            switch ((MainWindowEncoderState)ComboBoxTypeOfEncoders.SelectedIndex)
            {
                case MainWindowEncoderState.JPEG:
                    MainState = MainWindowEncoderState.JPEG;
                    ComboBoxQuality.IsEnabled = true;
                    ComboBoxTransform.IsEnabled = true;
                    TextBoxWidth.IsEnabled = true;
                    TextBoxHeight.IsEnabled = true;
                    EncoderStateString = "image/jpeg";
                    break;
                case MainWindowEncoderState.TIFF:
                    MainState = MainWindowEncoderState.TIFF;
                    ComboBoxCompression.IsEnabled = true;
                    ComboBoxColorDepth.IsEnabled = true;
                    TextBoxWidth.IsEnabled = true;
                    TextBoxHeight.IsEnabled = true;
                    EncoderStateString = "image/tiff";
                    break;
                case MainWindowEncoderState.PNG:
                    MainState = MainWindowEncoderState.PNG;
                    ComboBoxQuality.IsEnabled = true;
                    TextBoxWidth.IsEnabled = true;
                    TextBoxHeight.IsEnabled = true;
                    EncoderStateString = "image/png";
                    break;
                case MainWindowEncoderState.GIF:
                    MainState = MainWindowEncoderState.GIF;
                    ComboBoxQuality.IsEnabled = true;
                    TextBoxWidth.IsEnabled = true;
                    TextBoxHeight.IsEnabled = true;
                    EncoderStateString = "image/gif";
                    break;
                case MainWindowEncoderState.BMP:
                    MainState = MainWindowEncoderState.BMP;
                    ComboBoxQuality.IsEnabled = true;
                    TextBoxWidth.IsEnabled = true;
                    TextBoxHeight.IsEnabled = true;
                    EncoderStateString = "image/bmp";
                    break;
                default:
                    MainState = MainWindowEncoderState.NONE;
                    break;
            }
            ComboBoxTransform.IsEnabled = false;
        }
        private void LockControls()
        {
            ComboBoxQuality.IsEnabled = false;
            ComboBoxCompression.IsEnabled = false;
            ComboBoxColorDepth.IsEnabled = false;
            ComboBoxTransform.IsEnabled = false;
            TextBoxWidth.IsEnabled = false;
            TextBoxHeight.IsEnabled = false;
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            ImageList.Clear();
            ImageListToEncod.Clear();
            ListViewHandledImages.Items.Clear();
        } 

        private void ButtonTurnLeft_Click(object sender, RoutedEventArgs e)
        {
            if(ImageOverview.Source != null)
            {
                var rotation = ((BitmapImage)ImageOverview.Source).Rotation;

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = ((BitmapImage)ImageOverview.Source).UriSource;
                bitmap.BaseUri = ((BitmapImage)ImageOverview.Source).BaseUri;
                switch (rotation)
                {
                    case Rotation.Rotate0:
                        bitmap.Rotation = Rotation.Rotate90;
                        break;
                    case Rotation.Rotate90:
                        bitmap.Rotation = Rotation.Rotate180;
                        break;
                    case Rotation.Rotate180:
                        bitmap.Rotation = Rotation.Rotate270;
                        break;
                    case Rotation.Rotate270:
                        bitmap.Rotation = Rotation.Rotate0;
                        break;
                }
                bitmap.EndInit();
                ImageOverview.Source = bitmap;
            }
        }

        private void ButtonTurnRight_Click(object sender, RoutedEventArgs e)
        {
            var rotation = ((BitmapImage)ImageOverview.Source).Rotation;

            if (ImageOverview.Source != null)
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = ((BitmapImage)ImageOverview.Source).UriSource;
                bitmap.BaseUri = ((BitmapImage)ImageOverview.Source).BaseUri;
                switch (rotation)
                {
                    case Rotation.Rotate0:
                        bitmap.Rotation = Rotation.Rotate270;
                        break;
                    case Rotation.Rotate90:
                        bitmap.Rotation = Rotation.Rotate0;
                        break;
                    case Rotation.Rotate180:
                        bitmap.Rotation = Rotation.Rotate90;
                        break;
                    case Rotation.Rotate270:
                        bitmap.Rotation = Rotation.Rotate180;
                        break;
                }
                bitmap.EndInit();
                ImageOverview.Source = bitmap;
            }
        }
    }
}
