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

namespace ImageSqueezer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Encoder ImageQualityEncoder;
        private Encoder CompressionTypeEcnoder;
        private Encoder ColorDepthEncoder;
        private Encoder TransformationEncoder;

        private EncoderParameters EncoderParemeters;

        private Bitmap BitmapBuffer;

        private int ImageWidth = 0;
        private int ImageHeight = 0;

        private List<string> TypeOfCompression = new List<string>
        {
            "CompressionNONE",
            "CompressionLZW",
            "CompressionCCITT3",
            "CompressionCCITT4",
            "CompressionRLE"
        };
        private List<string> TypeOfTransformation = new List<string>
        {
            "TransformFlipHorizontal",
            "TransformFlipVertical",
            "TransformRotate90",
            "TransformRotate180",
            "TransformRotate270",
        };
        public MainWindow()
        {
            InitializeComponent();
            TextBoxColorDepth.PreviewTextInput += TextBoxNumberValidation;
            TextBoxHeight.PreviewTextInput += TextBoxNumberValidation;
            TextBoxWidth.PreviewTextInput += TextBoxNumberValidation;
        }
        private EncoderValue? GetCompressionType(long? compression)
        {
            return (EncoderValue)compression;
        }

        private void SetParametres(int? quality, long? compression, long? colorDepth, long? transform, int? width, int? height)
        {
            quality = quality ?? 100;
            EncoderValue compressionEncoder = GetCompressionType(compression) ?? EncoderValue.CompressionNone;
            colorDepth = colorDepth ?? 4L;
            ImageWidth = width ?? BitmapBuffer.Width;
            ImageHeight = height ?? BitmapBuffer.Height;

            SetEncoders();

            if (transform == null)
            {
                EncoderParameter[] parametres =
                {
                new EncoderParameter(ImageQualityEncoder, quality.Value),
                new EncoderParameter(CompressionTypeEcnoder, (long)compressionEncoder),
                new EncoderParameter(ColorDepthEncoder, colorDepth.Value),
                };
                EncoderParemeters.Param = parametres;
            }
            else
            {
                EncoderValue transformEncoder = (EncoderValue)transform;
                EncoderParameter[] parametres =
                {
                new EncoderParameter(ImageQualityEncoder, quality.Value),
                new EncoderParameter(CompressionTypeEcnoder, (long)compressionEncoder),
                new EncoderParameter(ColorDepthEncoder, colorDepth.Value),
                new EncoderParameter(TransformationEncoder, (long)transformEncoder)
                };
            }
        }

        private void SetEncoders()
        {
            ImageQualityEncoder = Encoder.Quality;
            CompressionTypeEcnoder = Encoder.Compression;
            ColorDepthEncoder = Encoder.ColorDepth;
            TransformationEncoder = Encoder.Transformation;
        }

        private Bitmap ApplySettings(Bitmap bitmap, string inFilePath, string outFilePath)
        {
            Bitmap result = new Bitmap(bitmap, ImageWidth, ImageHeight);
            result.Save(outFilePath, GetCodecInfo(inFilePath), EncoderParemeters);
            return result;
        }
        private void TextBoxNumberValidation(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private ImageCodecInfo GetCodecInfo(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
            {
                string encoderInfoString = @"image/";
                encoderInfoString += fileInfo.Extension;
            }
            return null;
        }
    }
}
