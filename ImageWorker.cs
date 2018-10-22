using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ImageSqueezer
{
    class ImageWorker
    {
        private Encoder ImageQualityEncoder;
        private Encoder CompressionTypeEcnoder;
        private Encoder ColorDepthEncoder;
        private Encoder TransformationEncoder;

        private EncoderParameters EncoderParemeters;

        public Bitmap BitmapBuffer { get; set; }
        public BitmapImage BitmapImageBuffer { get; set; }

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

        public ImageWorker(int quality, long compression, long colorDepth, long transform, int width, int height)
        {
            SetEncoders();
            SetParametres(quality, compression, colorDepth, transform, width, height);
        }
        public ImageWorker()
        {
            SetEncoders();
        }
        public void SetParametres(int quality, long compression, long colorDepth, long transform, int width, int height)
        {
            EncoderParemeters = new EncoderParameters();

            quality = quality == 0 ? 100 : quality;
            compression = compression == 0 ? 6 : compression;
            colorDepth = colorDepth == 0 ? 4L : colorDepth;
            ImageWidth = width == 0 ? BitmapBuffer.Width : width;
            ImageHeight = height == 0 ? BitmapBuffer.Height : height;

            if (transform == 0)
            {
                EncoderParameter[] parametres = new EncoderParameter[]
                {
                new EncoderParameter(ImageQualityEncoder, quality),
                new EncoderParameter(CompressionTypeEcnoder, compression),
                new EncoderParameter(ColorDepthEncoder, colorDepth),
                };
                //var pararmQ = new EncoderParameter(ImageQualityEncoder, quality);
                //var pararmC = new EncoderParameter(CompressionTypeEcnoder, compression);
                //var paravCD = new EncoderParameter(ColorDepthEncoder, colorDepth);
                //EncoderParemeters.Param[0] = pararmQ;
                //EncoderParemeters.Param[1] = pararmC;
                //EncoderParemeters.Param[2] = paravCD;
                EncoderParemeters.Param = parametres;
            }
            else
            {
                EncoderValue transformEncoder = (EncoderValue)transform;
                EncoderParameter[] parametres = new EncoderParameter[]
                {
                new EncoderParameter(ImageQualityEncoder, quality),
                new EncoderParameter(CompressionTypeEcnoder, compression),
                new EncoderParameter(ColorDepthEncoder, colorDepth),
                new EncoderParameter(TransformationEncoder, (long)transformEncoder)
                };
                EncoderParemeters.Param = parametres;
            }
        }
        public void DoWork(string inPath, string outPath = "")
        {
            outPath = @"F:\tmp_res.jpg";
            BitmapBuffer = new Bitmap(inPath);
            BitmapBuffer.Save(outPath, GetCodecInfo(inPath), EncoderParemeters);
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
        private ImageCodecInfo GetCodecInfo(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
            {
                string encoderInfoString = @"image/";
                encoderInfoString += fileInfo.Extension;
                return GetCodecInfo(encoderInfoString);
            }
            return null;
        }
        private  BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var outStream = new MemoryStream())
            {
                bitmap.Save(outStream, ImageFormat.Png);
                outStream.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = outStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }
    }
}
