using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
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

        public Bitmap BitmapBuffer { get; private set; }
        public BitmapImage BitmapImageBuffer { get; private set; }

        private int ImageWidth = 0;
        private int ImageHeight = 0;

        private bool isSizeSetted = false;

        //private List<string> TypeOfCompression = new List<string>
        //{
        //    "CompressionNONE",
        //    "CompressionLZW",
        //    "CompressionCCITT3",
        //    "CompressionCCITT4",
        //    "CompressionRLE"
        //};
        //private List<string> TypeOfTransformation = new List<string>
        //{
        //    "TransformFlipHorizontal",
        //    "TransformFlipVertical",
        //    "TransformRotate90",
        //    "TransformRotate180",
        //    "TransformRotate270",
        //};
        private EncoderValue GetTypeOfTransformation(long transform)
        {
            switch (transform)
            {
                case 90:
                    return EncoderValue.TransformRotate90;
                case 180:
                    return EncoderValue.TransformRotate180;
                case 270:
                    return EncoderValue.TransformRotate270;
                default:
                    break;
            }
            return 0;
        }

        private EncoderValue GetCompressionFromString(string compression)
        {
            switch (compression)
            {
                case "CompressionNONE":
                    return EncoderValue.CompressionNone;
                case "CompressionLZW":
                    return EncoderValue.CompressionLZW;
                case "CompressionCCITT3":
                    return EncoderValue.CompressionCCITT3;
                case "CompressionCCITT4":
                    return EncoderValue.CompressionCCITT4;
                case "CompressionRLE":
                    return EncoderValue.CompressionRle;
            }
            return EncoderValue.CompressionNone;
        }

        public ImageWorker(int quality, string compression, long colorDepth, long transform, int width, int height)
        {
            SetEncoders();
            SetParametres(quality, compression, colorDepth, transform, width, height);
        }
        public ImageWorker()
        {
            SetEncoders();
        }
        public void SetParametres(int quality, string compression, long colorDepth, long transform, int width, int height)
        {
            colorDepth = colorDepth == 0 ? 4L : colorDepth;

            if (width != 0 || height != 0)
                isSizeSetted = true;

            ImageWidth = width;
            ImageHeight = height;

            if (transform == 0)
            {
                EncoderParemeters = new EncoderParameters(3);
                EncoderParameter[] parametres = new EncoderParameter[]
                {
                new EncoderParameter(ImageQualityEncoder, quality),
                new EncoderParameter(CompressionTypeEcnoder, (long)GetCompressionFromString(compression)),
                new EncoderParameter(ColorDepthEncoder, colorDepth),
                };
                EncoderParemeters.Param = parametres;
            }
            else
            {
                EncoderParemeters = new EncoderParameters(4);
                EncoderParameter[] parametres = new EncoderParameter[]
                {
                new EncoderParameter(ImageQualityEncoder, quality),
                new EncoderParameter(CompressionTypeEcnoder, (long)GetCompressionFromString(compression)),
                new EncoderParameter(ColorDepthEncoder, colorDepth),
                new EncoderParameter(TransformationEncoder, (long)GetTypeOfTransformation(transform))

                };
                EncoderParemeters.Param = parametres;
            }
        }
        static Mutex doWorkMutex = new Mutex();
        public void DoWork(object InPath)
        {
            doWorkMutex.WaitOne();
            string inPath = (string)InPath;

            var fileInfo = new FileInfo(inPath);
            if (!Directory.Exists(fileInfo.DirectoryName + @"\" + "Converted"))
                Directory.CreateDirectory(fileInfo.DirectoryName + @"\" + "Converted");

            string outPath = fileInfo.DirectoryName + @"\" + "Converted" + @"\" + fileInfo.Name + "_converted" + fileInfo.Extension;

            var splitedPath = inPath.Split('.');
            var codecInfo = GetCodecInfo(splitedPath[splitedPath.Length - 1]);


            using (Bitmap currentImage = new Bitmap(inPath))
            {
                ImageWidth = ImageWidth == 0 ? currentImage.Width : ImageWidth;
                ImageHeight = ImageHeight == 0 ? currentImage.Height : ImageHeight;
                try
                {

                    using (BitmapBuffer = new Bitmap(currentImage, ImageWidth, ImageHeight))
                    {
                        BitmapBuffer.Save(outPath, codecInfo, EncoderParemeters);
                    }
                }
                catch (Exception e)
                {
                    LogErrors(e.Message);
                }


                if (!isSizeSetted)
                {
                    ImageWidth = 0;
                    ImageHeight = 0;
                }
            }
            doWorkMutex.ReleaseMutex();
        }

        private void SetEncoders()
        {
            ImageQualityEncoder = Encoder.Quality;
            CompressionTypeEcnoder = Encoder.Compression;
            ColorDepthEncoder = Encoder.ColorDepth;
            TransformationEncoder = Encoder.Transformation;            
        }

        private ImageCodecInfo GetCodecInfo(string extention)
        {
            ImageCodecInfo[] codecInfo = ImageCodecInfo.GetImageEncoders();
            for (int j = 0; j < codecInfo.Length; ++j)
            {
                if (codecInfo[j].FilenameExtension.Contains(extention.ToUpper()))
                    return codecInfo[j];
            }
            return null;
        }
        private BitmapImage ToBitmapImage(Bitmap bitmap)

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
        static Mutex logErrorMutex = new Mutex();
        void LogErrors(string error)
        {
            logErrorMutex.WaitOne();
            if (!File.Exists("Errors.log"))
                File.Create("Errors.log");
            using (FileStream stream = new FileStream("Errors.log", FileMode.Append, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.WriteLine(error);
                }
            }
            logErrorMutex.ReleaseMutex();
        }
    }
}
