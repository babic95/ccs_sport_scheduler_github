using PdfSharpCore.Fonts;
using System.IO;
using System.Reflection;

namespace CcsSportScheduler_API.Helpers
{
    public class FontResolver : IFontResolver
    {
        public static readonly FontResolver Instance = new FontResolver();

        public byte[] GetFont(string faceName)
        {
            switch (faceName)
            {
                case "Verdana#":
                    return LoadFontData("CcsSportScheduler_API.Resources.Fonts.Verdana.ttf");
                case "Verdana#b":
                    return LoadFontData("CcsSportScheduler_API.Resources.Fonts.Verdana.ttf");
                    // Dodaj ostale fontove koje koristiš
            }
            return null;
        }

        public string DefaultFontName => "Verdana";

        private byte[] LoadFontData(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames();
            foreach (var resource in resourceNames)
            {
                Console.WriteLine(resource); // Dodaj logovanje svih resursa
            }

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException($"Font resource '{resourceName}' not found.");
                }

                byte[] fontData = new byte[stream.Length];
                stream.Read(fontData, 0, fontData.Length);
                return fontData;
            }
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            // Map the font family and style to the correct font face name
            string faceName = familyName.ToLower() switch
            {
                "verdana" when isBold => "Verdana#b",
                "verdana" => "Verdana#",
                _ => null
            };

            if (faceName != null)
            {
                return new FontResolverInfo(faceName);
            }

            // If the requested font is not found, return the default font
            return new FontResolverInfo("Verdana#");
        }
    }
}