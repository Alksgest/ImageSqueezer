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

namespace ImageSqueezer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ImageWorker ImageWorker;
        private string CurrentFile;
        public MainWindow()
        {
            InitializeComponent();
            TextBoxColorDepth.PreviewTextInput += TextBoxNumberValidation;
            TextBoxHeight.PreviewTextInput += TextBoxNumberValidation;
            TextBoxWidth.PreviewTextInput += TextBoxNumberValidation;

            ImageWorker = new ImageWorker();

            BitmapImage bitmapImage = new BitmapImage();
        }

        private void TextBoxNumberValidation(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ButtonApply_Click(object sender, RoutedEventArgs e)
        {
            Int32.TryParse(ComboBoxQuality.Text, out int quality);
            Int64.TryParse(ComboBoxCompression.Text, out long compression);
            Int64.TryParse(TextBoxColorDepth.Text, out long colorDepth);
            Int64.TryParse(ComboBoxTransform.Text, out long transform);

            Int32.TryParse(TextBoxWidth.Text, out int width);
            Int32.TryParse(TextBoxHeight.Text, out int height);

            ImageWorker.SetParametres(quality, compression, colorDepth, transform, width, height);
        }

        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if(dialog.ShowDialog() == true)
            {
                CurrentFile = dialog.FileName;
            }
        }

        private void ToggleButtonAsyncState_Checked(object sender, RoutedEventArgs e)
        {
            return;
        }

        private void ButtonHandleImages_Click(object sender, RoutedEventArgs e)
        {
            ImageWorker.DoWork(CurrentFile);
        }
    }
}
