using System;
using System.IO;
using System.Text;
using ImageMagick;

namespace imageresizer.Converter
{
    public class ConversionOptions
    {
        public const int MaxSize = 3000;

        public string Name { get; set; }
        public MagickFormat TargetFormat { get; set; }
        public int Width { get; set; } = MaxSize;
        public int Height { get; set; } = MaxSize;
        public int Quality { get; set; }

        public string TargetMimeType => GetTargetMimeType();

        public string GetCacheKey()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return string.Empty;
            }

            var fileExtension = Path.GetExtension(Name);
            var fileName = string.IsNullOrWhiteSpace(fileExtension) == false
                ? Name.Substring(0, Name.Length - fileExtension.Length)
                : Name;

            var widthKey = Width == MaxSize ? 0 : Width;
            var heightKey = Height == MaxSize ? 0 : Height;

            // http://localhost:32772/api/images/resize/?name=image.big.jpg&format=png&width=500            
            var builder = new StringBuilder();
            builder.Append($"{fileName}.");
            builder.Append($"w_{Width}");
            builder.Append($",h_{Height}");
            builder.Append($",q_{Quality}");
            builder.Append(GetExtension());
            // image.big.w_500,h_0,q_100.png

            return builder.ToString();
        }

        private string GetExtension()
        {
            switch(TargetFormat)
            {
                case MagickFormat.Png24:
                    return ".png";
                
                default:
                    return ".jpg";
            }
        }

        private string GetTargetMimeType()
        {
            switch(TargetFormat)
            {
                case MagickFormat.Png24:
                    return "image/png";
                
                default:
                    return "image/jpeg";
            }
        }
    }
}