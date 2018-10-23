using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSqueezer
{
    class ImageRepresenter
    {
        public string Title { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public string StringSize { get; set; }
        public ImageRepresenter(string path)
        {
            Path = path;
            FileInfo info = new FileInfo(path);
            Title = info.Name;
            Size = (info.Length / 1024);
            StringSize = Size.ToString() + "kb";
        }
    }
}
