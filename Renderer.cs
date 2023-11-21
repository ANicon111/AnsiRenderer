using System.Globalization;

namespace AnsiRenderer
{
    public struct Color
    {
        private uint r, g, b;
        private double a;

        public static readonly Color Invalid = new()
        {
            r = 256,
            g = 256,
            b = 256,
            a = -1
        };

        public static readonly Color Reset = new()
        {
            r = 0,
            g = 0,
            b = 0,
            a = -1
        };

        public readonly uint R { get => uint.Clamp(r, 0, 255); }

        public readonly uint G { get => uint.Clamp(g, 0, 255); }

        public readonly uint B { get => uint.Clamp(b, 0, 255); }

        public readonly double A { get => double.Clamp(a, 0, 1); }

        public readonly double H
        {
            get
            {
                double R = this.R / 255.0d;
                double G = this.G / 255.0d;
                double B = this.B / 255.0d;

                double max = double.Max(double.Max(R, G), B);
                double min = double.Min(double.Min(R, G), B);

                if (max == min) return 0;

                if (R == max)
                {
                    return 60.0d * (G - B) / (max - min);
                }
                if (G == max)
                {
                    return 60.0d * (2.0d + (B - R) / (max - min));
                }
                return 60.0d * (4.0d + (R - G) / (max - min));
            }
        }
        public readonly double S
        {
            get
            {
                double max = uint.Max(uint.Max(R, G), B) / 255.0d;
                double min = uint.Min(uint.Min(R, G), B) / 255.0d;

                if (max == min) return 0;
                return (max - min) / (1 - double.Abs(2 * L - 1));
            }
        }
        public readonly double L
        {
            get { return (uint.Max(uint.Max(R, G), B) / 255.0d + uint.Min(uint.Min(R, G), B) / 255.0d) / 2; }
        }
        public Color(uint red, uint green, uint blue, double alpha = 1)
        {
            r = uint.Clamp(red, 0, 255);
            g = uint.Clamp(green, 0, 255);
            b = uint.Clamp(blue, 0, 255);
            a = double.Clamp(alpha, 0, 1);
        }
        public static Color FromHSLA(double hue, double saturation, double luminosity, double a = 1)
        {
            uint R, G, B;
            double A;
            if (hue < 0) hue = 360 - double.Abs(hue) % 360;
            else hue %= 360;

            static double hueCalc(double h)
            {
                return double.Clamp(double.Abs(3 - h % 360 / 60) - 1, 0, 1);
            }
            saturation = double.Clamp(saturation, 0, 1);
            luminosity = double.Clamp(luminosity, 0, 1);
            //chroma
            double c = saturation * (1 - double.Abs(1 - 2 * luminosity));
            //base color / lightness
            double b = luminosity - c / 2;
            R = uint.Clamp((uint)double.Round(255 * (b + c * hueCalc(hue + 0))), 0, 255);
            G = uint.Clamp((uint)double.Round(255 * (b + c * hueCalc(hue + 240))), 0, 255);
            B = uint.Clamp((uint)double.Round(255 * (b + c * hueCalc(hue + 120))), 0, 255);
            A = double.Clamp(a, 0, 1);
            return new(R, G, B, A);
        }

        public readonly Color WithAlpha(double alpha) => new(R, G, B, alpha);

        public readonly Color WithRed(uint red) => new(red, G, B, A);
        public readonly Color WithGreen(uint green) => new(R, green, B, A);
        public readonly Color WithBlue(uint blue) => new(R, G, blue, A);

        public readonly Color WithHue(double hue) => FromHSLA(hue, S, L, A);
        public readonly Color WithSaturation(double saturation) => FromHSLA(H, saturation, L, A);
        public readonly Color WithLuminosity(double lightness) => FromHSLA(H, S, lightness, A);

        public readonly Color WithOverlay(Color overlay)
        {
            uint r, g, b;
            double a;
            if (this == Invalid)
            {
                r = overlay.R;
                g = overlay.G;
                b = overlay.B;
                a = overlay.A;
            }
            else if (overlay == Invalid)
            {
                r = R;
                g = G;
                b = B;
                a = A;
            }
            else
            {
                r = (uint)double.Round(overlay.R * overlay.A + R * (1 - overlay.A));
                g = (uint)double.Round(overlay.G * overlay.A + G * (1 - overlay.A));
                b = (uint)double.Round(overlay.B * overlay.A + B * (1 - overlay.A));
                a = overlay.A + A * (1 - overlay.A);
            }
            return new(
                r,
                g,
                b,
                a
            );
        }

        public readonly Color Inverted() => new(255 - R, 255 - G, 255 - B, A);

        public static Color FromUInt(uint rgba) => (Color)rgba;
        public static Color FromHTML(string html)
        {
            static double processPercentDouble(string percentDouble)
            {
                percentDouble = percentDouble.Trim();
                double alphaMod = 1;
                if (percentDouble.EndsWith('%'))
                {
                    percentDouble = percentDouble[..^1].Trim();
                    alphaMod = 0.01;
                }
                return Convert.ToDouble(percentDouble, CultureInfo.InvariantCulture) * alphaMod;
            }

            html = html.Trim();
            if (Enum.TryParse(html, true, out Colors getter))
            {
                return (Color)getter;
            }
            else
            {
                //#HEXADECIMAL
                if (html.StartsWith("#"))
                {
                    try
                    {
                        html = html[1..];
                        html = html.Trim();
                        //RRGGBBAA
                        if (html.Length == 8)
                            return (Color)Convert.ToUInt32(html, 16);
                        //RRGGBB
                        else if (html.Length == 6)
                        {
                            //convert to RRGGBBff
                            html += "ff";
                            return (Color)Convert.ToUInt32(html, 16);
                        }
                        //RGBA
                        else if (html.Length == 4)
                        {
                            //convert to RRGGBBAA
                            char[] l = html.ToCharArray();
                            html = $"{l[0]}{l[0]}{l[1]}{l[1]}{l[2]}{l[2]}{l[3]}{l[3]}";
                            return (Color)Convert.ToUInt32(html, 16);
                        }
                        //RGB
                        else if (html.Length == 3)
                        {
                            //convert to RRGGBBff
                            char[] l = html.ToCharArray();
                            html = $"{l[0]}{l[0]}{l[1]}{l[1]}{l[2]}{l[2]}ff";
                            return (Color)Convert.ToUInt32(html, 16);
                        }
                        else throw new Exception("Invalid color format. Supported hexadecimal formats are #RRGGBBAA, #RRGGBB, #RGBA and #RGB.");
                    }
                    catch
                    {
                        throw new Exception("Invalid color format. Supported hexadecimal formats are #RRGGBBAA, #RRGGBB, #RGBA and #RGB.");
                    }
                }
                //rgba(r,g,b,a)
                else if (html.StartsWith("rgba"))
                {
                    try
                    {
                        html = html[4..].Trim();
                        string[] rgba;
                        if (html.StartsWith('(') && html.EndsWith(')'))
                        {
                            rgba = html[1..^1].Trim().Split(',');
                        }
                        else
                        {
                            throw new("Invalid color format. The correct rgba(uint, uint, uint, double) function structure is rgba(r, g, b, a). Spaces don't matter.");
                        }
                        if (rgba.Length == 4)
                        {

                            return new(Convert.ToUInt32(rgba[0].Trim()), Convert.ToUInt32(rgba[1].Trim()), Convert.ToUInt32(rgba[2].Trim()), processPercentDouble(rgba[3].Trim()));
                        }
                        else
                        {
                            throw new("Invalid color format. The correct rgba(uint, uint, uint, double) function structure is rgba(r, g, b, a). Spaces don't matter.");
                        }
                    }
                    catch
                    {
                        throw new("Invalid color format. The correct rgba(uint, uint, uint, double) function structure is rgba(r, g, b, a). Spaces don't matter.");
                    }
                }
                //rgb(r,g,b)
                else if (html.StartsWith("rgb"))
                {
                    try
                    {
                        html = html[3..].Trim();
                        string[] rgb;
                        if (html.StartsWith('(') && html.EndsWith(')'))
                        {
                            rgb = html[1..^1].Trim().Split(',');
                        }
                        else
                        {
                            throw new("Invalid color format. The correct rgb(uint, uint, uint) function structure is rgb(r, g, b). Spaces don't matter.");
                        }
                        if (rgb.Length == 3)
                        {
                            return new(Convert.ToUInt32(rgb[0].Trim()), Convert.ToUInt32(rgb[1].Trim()), Convert.ToUInt32(rgb[2].Trim()));
                        }
                        else
                        {
                            throw new("Invalid color format. The correct rgb(uint, uint, uint) function structure is rgb(r, g, b). Spaces don't matter.");
                        }
                    }
                    catch
                    {
                        throw new("Invalid color format. The correct rgb(uint, uint, uint) function structure is rgb(r, g, b). Spaces don't matter.");
                    }
                }
                //hsla(h,s,l,a)
                else if (html.StartsWith("hsla"))
                {
                    try
                    {
                        html = html[4..].Trim();
                        string[] hsla;
                        if (html.StartsWith('(') && html.EndsWith(')'))
                        {
                            hsla = html[1..^1].Trim().Split(',');
                        }
                        else
                        {
                            throw new("Invalid color format. The correct hsla(double, double, double, double) function structure is hsla(h, s, l, a). Spaces don't matter.");
                        }
                        if (hsla.Length == 4)
                        {
                            return FromHSLA(Convert.ToDouble(hsla[0], CultureInfo.InvariantCulture), processPercentDouble(hsla[1]), processPercentDouble(hsla[2]), processPercentDouble(hsla[3]));
                        }
                        else
                        {
                            throw new("Invalid color format. The correct hsla(double, double, double, double) function structure is hsla(h, s, l, a). Spaces don't matter.");
                        }
                    }
                    catch
                    {
                        throw new("Invalid color format. The correct hsla(double, double, double, double) function structure is hsla(h, s, l, a). Spaces don't matter.");
                    }
                }
                //hsl(h,s,l)
                else if (html.StartsWith("hsl"))
                {
                    try
                    {
                        html = html[3..].Trim();
                        string[] hsl;
                        if (html.StartsWith('(') && html.EndsWith(')'))
                        {
                            hsl = html[1..^1].Trim().Split(',');
                        }
                        else
                        {
                            throw new("Invalid color format. The correct hsl(double, double, double) function structure is hsl(h, s, l). Spaces don't matter.");
                        }
                        if (hsl.Length == 3)
                        {
                            return FromHSLA(Convert.ToDouble(hsl[0], CultureInfo.InvariantCulture), processPercentDouble(hsl[1]), processPercentDouble(hsl[2]), 1);
                        }
                        else
                        {
                            throw new("Invalid color format. The correct hsl(double, double, double) function structure is hsl(h, s, l). Spaces don't matter.");
                        }
                    }
                    catch
                    {
                        throw new("Invalid color format. The correct hsl(double, double, double) function structure is hsl(h, s, l). Spaces don't matter.");
                    }
                }
                else throw new("Invalid color name or format. Make sure to use standard HTML color names or formats.");
            }
        }

        public static explicit operator Color(uint rgba)
        {
            return new(rgba / 0x1000000, rgba % 0x1000000 / 0x10000, rgba % 0x10000 / 0x100, rgba % 0x100 / 255.0d);
        }

        public static implicit operator Color(Colors color) => (Color)(uint)color;

        public static implicit operator Color(ConsoleColor consoleColor)
        {
            return consoleColor switch
            {
                ConsoleColor.Red => Colors.Red,
                ConsoleColor.Green => Colors.Green,
                ConsoleColor.Blue => Colors.Blue,
                ConsoleColor.Cyan => Colors.Cyan,
                ConsoleColor.Magenta => Colors.Magenta,
                ConsoleColor.Yellow => Colors.Yellow,
                ConsoleColor.DarkRed => Colors.DarkRed,
                ConsoleColor.DarkGreen => Colors.DarkGreen,
                ConsoleColor.DarkBlue => Colors.Navy,
                ConsoleColor.DarkCyan => Colors.DarkCyan,
                ConsoleColor.DarkMagenta => Colors.DarkMagenta,
                ConsoleColor.DarkYellow => Colors.Goldenrod,
                ConsoleColor.Black => Colors.Black,
                ConsoleColor.DarkGray => Colors.DarkGray,
                ConsoleColor.Gray => Colors.Gray,
                ConsoleColor.White => Colors.White,
                _ => Colors.Black
            };
        }

        public static explicit operator uint(Color color)
        {
            return color.R * 0x1000000 + color.G * 0x10000 + color.B * 0x100 + (uint)(color.A * 0xff);
        }

        public static bool operator ==(Color left, Color right) => left.r == right.r && left.g == right.g && left.b == right.b && left.a == right.a;

        public static bool operator !=(Color left, Color right) => !(left == right);

        public override readonly bool Equals(object? obj) => Equals(obj as Color?);

        public override readonly int GetHashCode() => (int)(uint)this;


        public override readonly string ToString()
        {
            List<string> name = new()
            {
                $"#{Convert.ToString((uint)this, 16).PadLeft(8, '0')}"
            };
            Colors value = (Colors)((uint)this / 0x100 * 0x100 + 0xff);
            if (Enum.IsDefined(value))
            {
                name.Add($" ({string.Join(", ", Enum.GetNames<Colors>().Where(n => Enum.Parse<Colors>(n).Equals(value)))}");
                if ((uint)value == (uint)this)
                {
                    name.Add(")");
                }
                else
                {
                    name.Add($" with transparency {(A * 100).ToString("0.00", CultureInfo.InvariantCulture)}%)");
                }
            }
            return string.Concat(name);
        }

        //supplementary ansi-based color representation for debugging
        public readonly string ToStringExtended()
        {
            Color dg = new(128, 128, 128);
            Color lg = new(192, 192, 192);
            Color dc = dg.WithOverlay(this);
            Color lc = lg.WithOverlay(this);
            return $"\x1B[38;2;{dg.R};{dg.G};{dg.B}m\x1B[48;2;{lg.R};{lg.G};{lg.B}m▄\x1B[38;2;{dc.R};{dc.G};{dc.B}m\x1B[48;2;{lc.R};{lc.G};{lc.B}m▀▄\x1B[38;2;{dg.R};{dg.G};{dg.B}m\x1B[48;2;{lg.R};{lg.G};{lg.B}m▀\x1B[0m "
                + ToString();
        }
    }


    public enum Colors : uint
    {
        AliceBlue = 0xf0f8ffff,
        AntiqueWhite = 0xfaebd7ff,
        Aqua = 0x00ffffff,
        Aquamarine = 0x7fffd4ff,
        Azure = 0xf0ffffff,
        Beige = 0xf5f5dcff,
        Bisque = 0xffe4c4ff,
        Black = 0x000000ff,
        BlanchedAlmond = 0xffebcdff,
        Blue = 0x0000ffff,
        BlueViolet = 0x8a2be2ff,
        Brown = 0xa52a2aff,
        BurlyWood = 0xdeb887ff,
        CadetBlue = 0x5f9ea0ff,
        Chartreuse = 0x7fff00ff,
        Chocolate = 0xd2691eff,
        Coral = 0xff7f50ff,
        CornflowerBlue = 0x6495edff,
        CornSilk = 0xfff8dcff,
        Crimson = 0xdc143cff,
        Cyan = 0x00ffffff,
        DarkBlue = 0x00008bff,
        DarkCyan = 0x008b8bff,
        DarkGoldenrod = 0xb8860bff,
        DarkGray = 0xa9a9a9ff,
        DarkGreen = 0x006400ff,
        DarkGrey = 0xa9a9a9ff,
        DarkKhaki = 0xbdb76bff,
        DarkMagenta = 0x8b008bff,
        DarkOliveGreen = 0x556b2fff,
        DarkOrange = 0xff8c00ff,
        DarkOrchid = 0x9932ccff,
        DarkRed = 0x8b0000ff,
        DarkSalmon = 0xe9967aff,
        DarkSeaGreen = 0x8fbc8fff,
        DarkSlateBlue = 0x483d8bff,
        DarkSlateGray = 0x2f4f4fff,
        DarkSlateGrey = 0x2f4f4fff,
        DarkTurquoise = 0x00ced1ff,
        DarkViolet = 0x9400d3ff,
        DeepPink = 0xff1493ff,
        DeepSkyBlue = 0x00bfffff,
        DimGray = 0x696969ff,
        DimGrey = 0x696969ff,
        DodgerBlue = 0x1e90ffff,
        Firebrick = 0xb22222ff,
        FloralWhite = 0xfffaf0ff,
        ForestGreen = 0x228b22ff,
        Fuchsia = 0xff00ffff,
        Gainsboro = 0xdcdcdcff,
        GhostWhite = 0xf8f8ffff,
        Goldenrod = 0xdaa520ff,
        Gold = 0xffd700ff,
        Gray = 0x808080ff,
        Green = 0x008000ff,
        GreenYellow = 0xadff2fff,
        Grey = 0x808080ff,
        Honeydew = 0xf0fff0ff,
        HotPink = 0xff69b4ff,
        IndianRed = 0xcd5c5cff,
        Indigo = 0x4b0082ff,
        Ivory = 0xfffff0ff,
        Khaki = 0xf0e68cff,
        LavenderBlush = 0xfff0f5ff,
        Lavender = 0xe6e6faff,
        LawnGreen = 0x7cfc00ff,
        LemonChiffon = 0xfffacdff,
        LightBlue = 0xadd8e6ff,
        LightCoral = 0xf08080ff,
        LightCyan = 0xe0ffffff,
        LightGoldenrodYellow = 0xfafad2ff,
        LightGray = 0xd3d3d3ff,
        LightGreen = 0x90ee90ff,
        LightGrey = 0xd3d3d3ff,
        LightPink = 0xffb6c1ff,
        LightSalmon = 0xffa07aff,
        LightSeaGreen = 0x20b2aaff,
        LightSkyBlue = 0x87cefaff,
        LightSlateGray = 0x778899ff,
        LightSlateGrey = 0x778899ff,
        LightSteelBlue = 0xb0c4deff,
        LightYellow = 0xffffe0ff,
        Lime = 0x00ff00ff,
        LimeGreen = 0x32cd32ff,
        Linen = 0xfaf0e6ff,
        Magenta = 0xff00ffff,
        Maroon = 0x800000ff,
        MediumAquamarine = 0x66cdaaff,
        MediumBlue = 0x0000cdff,
        MediumOrchid = 0xba55d3ff,
        MediumPurple = 0x9370dbff,
        MediumSeaGreen = 0x3cb371ff,
        MediumSlateBlue = 0x7b68eeff,
        MediumSpringGreen = 0x00fa9aff,
        MediumTurquoise = 0x48d1ccff,
        MediumVioletRed = 0xc71585ff,
        MidnightBlue = 0x191970ff,
        MintCream = 0xf5fffaff,
        MistyRose = 0xffe4e1ff,
        Moccasin = 0xffe4b5ff,
        NavajoWhite = 0xffdeadff,
        Navy = 0x000080ff,
        OldLace = 0xfdf5e6ff,
        Olive = 0x808000ff,
        OliveDrab = 0x6b8e23ff,
        Orange = 0xffa500ff,
        OrangeRed = 0xff4500ff,
        Orchid = 0xda70d6ff,
        PaleGoldenrod = 0xeee8aaff,
        PaleGreen = 0x98fb98ff,
        PaleTurquoise = 0xafeeeeff,
        PaleVioletRed = 0xdb7093ff,
        PapayaWhip = 0xffefd5ff,
        PeachPuff = 0xffdab9ff,
        Peru = 0xcd853fff,
        Pink = 0xffc0cbff,
        Plum = 0xdda0ddff,
        PowderBlue = 0xb0e0e6ff,
        Purple = 0x800080ff,
        RebeccaPurple = 0x663399ff,
        Red = 0xff0000ff,
        RosyBrown = 0xbc8f8fff,
        RoyalBlue = 0x4169e1ff,
        SaddleBrown = 0x8b4513ff,
        Salmon = 0xfa8072ff,
        SandyBrown = 0xf4a460ff,
        SeaGreen = 0x2e8b57ff,
        Seashell = 0xfff5eeff,
        Sienna = 0xa0522dff,
        Silver = 0xc0c0c0ff,
        SkyBlue = 0x87ceebff,
        SlateBlue = 0x6a5acdff,
        SlateGray = 0x708090ff,
        SlateGrey = 0x708090ff,
        Snow = 0xfffafaff,
        SpringGreen = 0x00ff7fff,
        SteelBlue = 0x4682b4ff,
        Tan = 0xd2b48cff,
        Teal = 0x008080ff,
        Thistle = 0xd8bfd8ff,
        Tomato = 0xff6347ff,
        Turquoise = 0x40e0d0ff,
        Violet = 0xee82eeff,
        Wheat = 0xf5deb3ff,
        White = 0xffffffff,
        WhiteSmoke = 0xf5f5f5ff,
        Yellow = 0xffff00ff,
        YellowGreen = 0x9acd32,
    }

    public static class ColorsExtension
    {
        public static Color Value(this Colors color) => (Color)color;

        public static Color WithAlpha(this Colors color, double a) => ((Color)color).WithAlpha(a);

        public static Color WithRed(this Colors color, uint r) => ((Color)color).WithRed(r);
        public static Color WithGreen(this Colors color, uint g) => ((Color)color).WithGreen(g);
        public static Color WithBlue(this Colors color, uint b) => ((Color)color).WithBlue(b);

        public static Color WithHue(this Colors color, double h) => ((Color)color).WithHue(h);
        public static Color WithSaturation(this Colors color, double s) => ((Color)color).WithSaturation(s);
        public static Color WithLuminosity(this Colors color, double l) => ((Color)color).WithLuminosity(l);

        public static Color WithOverlay(this Colors color, Color overlay) => ((Color)color).WithOverlay(overlay);

        public static Color Inverted(this Colors color) => ((Color)color).Inverted();
    }

    public struct Rectangle
    {
        public int X, Y;
        public readonly int Width, Height;
        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Rectangle(Rectangle? rect)
        {
            this = rect ?? new();
        }
    }

    public struct Pixel
    {
        public Color BG = Color.Invalid, FG = Color.Invalid;
        public char Ch = '\0';

        public Pixel() { }

        public Pixel(char ch = '\0', Color? bg = null, Color? fg = null)
        {
            if (ch >= ' ')
            {
                Ch = ch;
                BG = bg ?? Color.Invalid;
                FG = fg ?? Color.Invalid;
            }
        }

        public readonly Pixel WithOverlay(Pixel overlay)
        {
            Color bg, fg;
            char ch;
            if (overlay.Ch > ' ')
            {
                ch = overlay.Ch;
                //large character automatic background replacement
                if ("▀▄▅▆▇█▉▊▋▌▐▙▛▜▟▚▞▓▒".Contains(Ch))
                {
                    bg = FG.WithOverlay(overlay.BG);
                    fg = FG.WithOverlay(overlay.FG);
                }
                else
                {
                    bg = BG.WithOverlay(overlay.BG);
                    fg = BG.WithOverlay(overlay.FG);
                }
            }
            else if (overlay.Ch == ' ')
            {
                if (Ch >= ' ')
                    ch = Ch;
                else ch = ' ';
                bg = BG.WithOverlay(overlay.BG);
                fg = FG.WithOverlay(overlay.BG);
            }
            else
            {
                ch = Ch;
                bg = BG;
                fg = FG;
            }
            return new(ch, bg, fg);
        }

        public static bool operator ==(Pixel left, Pixel right) => left.Ch == right.Ch && left.BG == right.BG && left.FG == right.FG;

        public static bool operator !=(Pixel left, Pixel right) => !(left == right);

        public override readonly bool Equals(object? obj) => Equals(obj as Pixel?);

        public override readonly int GetHashCode() => Ch;
    }

    public struct ColorArea
    {
        public Color Color;
        public Rectangle? Geometry;
        public bool Foreground;
        public Alignment? AlignmentX, AlignmentY;

        //the foreground bool decides if you are coloring the text or the background.
        //the overlayColors bool decides if you are setting or overlaying the color over the existing valid pixels
        public ColorArea(Color color, bool foreground = false, Rectangle? geometry = null, Alignment? alignmentX = null, Alignment? alignmentY = null)
        {
            Color = color;
            Geometry = geometry;
            Foreground = foreground;
            AlignmentX = alignmentX;
            AlignmentY = alignmentY;
        }
    }

    public enum Alignment : byte
    {
        Beginning = 0,
        Start = 0,
        Left = 0,
        Top = 0,
        Center = 1,
        Middle = 1,
        End = 2,
        Right = 2,
        Bottom = 2,

    }

    public class BufferedConsole
    {
        private List<string> buffer;

        public BufferedConsole()
        {
            buffer = new();
        }

        public void Write(char c) => buffer.Add(c.ToString());

        public void Write(string str) => buffer.Add(str);

        public void SetCursorPosition(int left, int top) => buffer.Add($"\x1B[{top + 1};{left + 1}H");

        public void SetFgColor(Color color)
        {
            if (color == Color.Reset)
                Write($"\x1B[39m");
            else
                Write($"\x1B[38;2;{color.R};{color.G};{color.B}m");
        }

        public void SetBgColor(Color color)
        {
            if (color == Color.Reset)
                Write($"\x1B[49m");
            else
                Write($"\x1B[48;2;{color.R};{color.G};{color.B}m");
        }

        public void Flush()
        {
            if (buffer.Count == 0)
                return;
            Console.Write(string.Concat(buffer));
            SetBgColor(ConsoleColor.Black);
            SetFgColor(ConsoleColor.White);
            SetCursorPosition(0, 0);
            buffer = new();
        }
    }

    public class RendererObject
    {
        private int x;
        private int y;
        private int width;
        private int height;
        private char defaultCharacter;

        private readonly string[] lines;
        private readonly List<string[]> animation = new();
        private int frame = 0;
        private Pixel[,] pixels;
        private bool update = true;
        private List<RendererObject> subObjects = new();
        private List<ColorArea> colorAreas = new();

        private Alignment internalAlignmentX = Alignment.Start;
        private Alignment internalAlignmentY = Alignment.Start;
        private Alignment? externalAlignmentX;
        private Alignment? externalAlignmentY;

        private readonly List<RendererObject> Parents = new();
        private bool sizeChanged = false;

        public void UpdateParents()
        {
            foreach (RendererObject parent in Parents)
            {
                parent.update = true;
                parent.UpdateParents();
            }
        }

        public void Update()
        {
            update = true;
            UpdateParents();
        }

        public RendererObject(
            Rectangle? geometry = null,
            int x = 0,
            int y = 0,
            string text = "",
            string[]? animation = null,
            int startFrame = 0,
            char defaultCharacter = '\0',
            RendererObject[]? subObjects = null,
            ColorArea[]? colorAreas = null,
            Alignment internalAlignmentX = Alignment.Start,
            Alignment internalAlignmentY = Alignment.Start,
            Alignment? externalAlignmentX = null,
            Alignment? externalAlignmentY = null
            )
        {
            lines = text.Split(new string[] { "\r\n", "\n\r", "\r", "\n" }, StringSplitOptions.None);
            if (geometry != null)
            {
                Rectangle Geometry = (Rectangle)geometry;
                width = Geometry.Width;
                height = Geometry.Height;
                X = Geometry.X;
                Y = Geometry.Y;
            }
            else
            {
                //geometry guesser in case it isn't provided
                int width = 0;
                int height = lines.Length;
                if (animation != null)
                    foreach (string animationText in animation)
                    {
                        string[] animationLines = animationText.Split(new string[] { "\r\n", "\n\r", "\r", "\n" }, StringSplitOptions.None);
                        height = int.Max(height, animationLines.Length);
                        foreach (string line in animationLines) width = int.Max(width, line.Length);
                        this.animation.Add(animationLines);
                    }
                foreach (string line in lines) width = int.Max(width, line.Length);

                if (subObjects != null)
                {
                    this.subObjects = subObjects.ToList();
                    foreach (RendererObject subObject in subObjects)
                    {
                        width = int.Max(width, subObject.width + subObject.X);
                        height = int.Max(height, subObject.height + subObject.Y);
                        subObject.Parents.Add(this);
                    }
                }
                if (colorAreas != null)
                    this.colorAreas = colorAreas.ToList();
                X = x;
                Y = y;
                this.width = width;
                this.height = height;
            }
            Frame = startFrame;
            this.defaultCharacter = defaultCharacter;
            pixels = new Pixel[width, height];
            ExternalAlignmentX = externalAlignmentX;
            ExternalAlignmentY = externalAlignmentY;
            InternalAlignmentX = internalAlignmentX;
            InternalAlignmentY = internalAlignmentY;
        }
        public Pixel[,] Pixels
        {
            get
            {
                if (!update)
                    return pixels;
                update = false;

                if (sizeChanged)
                {
                    pixels = new Pixel[width, height];
                    sizeChanged = false;
                }

                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        pixels[i, j] = new(defaultCharacter);
                    }
                }

                //text drawing
                int yStart = 0;
                int yEnd = lines.Length;
                if (InternalAlignmentY == Alignment.Center)
                {
                    yStart += height / 2 - lines.Length / 2;
                    yEnd += height / 2 - lines.Length / 2;
                }
                if (InternalAlignmentY == Alignment.Bottom)
                {
                    yStart = height - lines.Length;
                    yEnd = height;
                }
                for (int j = int.Max(0, yStart); j < int.Min(height, yEnd); j++)
                {
                    int xStart = 0;
                    int xEnd = lines[j - yStart].Length;
                    if (InternalAlignmentX == Alignment.Center)
                    {
                        xStart += width / 2 - lines[j - yStart].Length / 2;
                        xEnd += width / 2 - lines[j - yStart].Length / 2;
                    }
                    if (InternalAlignmentX == Alignment.Right)
                    {
                        xStart = width - lines[j - yStart].Length;
                        xEnd = width;
                    }
                    for (int i = int.Max(0, xStart); i < int.Min(width, xEnd); i++)
                    {
                        pixels[i, j].Ch = lines[j - yStart][i - xStart];
                    }
                }

                //animation drawing
                if (animation.Count != 0)
                {
                    string[] animationLines = animation[frame];
                    int animationYStart = 0;
                    int animationYEnd = animationLines.Length;
                    if (InternalAlignmentY == Alignment.Center)
                    {
                        animationYStart += height / 2 - animationLines.Length / 2;
                        animationYEnd += height / 2 - animationLines.Length / 2;
                    }
                    if (InternalAlignmentY == Alignment.Bottom)
                    {
                        animationYStart += height - animationLines.Length;
                        animationYEnd += height;
                    }
                    for (int j = int.Max(0, animationYStart); j < int.Min(height, animationYEnd); j++)
                    {
                        int animationXStart = 0;
                        int animationXEnd = animationLines[j - animationYStart].Length;
                        if (InternalAlignmentX == Alignment.Center)
                        {
                            animationXStart += width / 2 - animationLines[j - animationYStart].Length / 2;
                            animationXEnd += width / 2 - animationLines[j - animationYStart].Length / 2;
                        }
                        if (InternalAlignmentX == Alignment.Right)
                        {
                            animationXStart += width - animationLines[j - animationYStart].Length;
                            animationXEnd += width;
                        }
                        for (int i = int.Max(0, animationXStart); i < int.Min(width, animationXEnd); i++)
                        {
                            pixels[i, j].Ch = animationLines[j - animationYStart][i - animationXStart];
                        }
                    }
                }


                //sub-objects and colors drawing
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        foreach (ColorArea colorArea in colorAreas)
                        {
                            if (colorArea.Geometry != null)
                            {
                                int extraX = 0;
                                int extraY = 0;

                                if (colorArea.AlignmentX != null)
                                {
                                    if (colorArea.AlignmentX == Alignment.Center)
                                    {
                                        extraX = width / 2 - new Rectangle(colorArea.Geometry).Width / 2;
                                    }
                                    if (colorArea.AlignmentX == Alignment.Right)
                                    {
                                        extraX = width - (colorArea.Geometry ?? new Rectangle()).Width;
                                    }
                                }
                                else
                                {
                                    if (InternalAlignmentX == Alignment.Center)
                                    {
                                        extraX = width / 2 - new Rectangle(colorArea.Geometry).Width / 2;
                                    }
                                    if (InternalAlignmentX == Alignment.Right)
                                    {
                                        extraX = width - (colorArea.Geometry ?? new Rectangle()).Width;
                                    }
                                }

                                if (colorArea.AlignmentY != null)
                                {
                                    if (colorArea.AlignmentY == Alignment.Center)
                                    {
                                        extraY = height / 2 - new Rectangle(colorArea.Geometry).Height / 2;
                                    }
                                    if (colorArea.AlignmentY == Alignment.Right)
                                    {
                                        extraY = height - (colorArea.Geometry ?? new Rectangle()).Height;
                                    }
                                }
                                else
                                {
                                    if (InternalAlignmentY == Alignment.Center)
                                    {
                                        extraY = height / 2 - (colorArea.Geometry ?? new Rectangle()).Height / 2;
                                    }
                                    if (InternalAlignmentY == Alignment.Bottom)
                                    {
                                        extraY = height - (colorArea.Geometry ?? new Rectangle()).Height;
                                    }
                                }

                                if (
                                    (colorArea.Geometry ?? new Rectangle()).X + extraX <= i && i < (colorArea.Geometry ?? new Rectangle()).X + extraX + (colorArea.Geometry ?? new Rectangle()).Width &&
                                    (colorArea.Geometry ?? new Rectangle()).Y + extraY <= j && j < (colorArea.Geometry ?? new Rectangle()).Y + extraY + (colorArea.Geometry ?? new Rectangle()).Height
                                )
                                    if (colorArea.Foreground)
                                        pixels[i, j].FG = pixels[i, j].FG.WithOverlay(colorArea.Color);
                                    else
                                        pixels[i, j].BG = pixels[i, j].BG.WithOverlay(colorArea.Color);
                            }
                            else
                            {
                                if (colorArea.Foreground)
                                    pixels[i, j].FG = pixels[i, j].FG.WithOverlay(colorArea.Color);
                                else
                                    pixels[i, j].BG = pixels[i, j].BG.WithOverlay(colorArea.Color);
                            }
                        }
                        //if color isn't set, default to white on transparent(black)
                        if (pixels[i, j].FG == Color.Invalid) pixels[i, j].FG = new(255, 255, 255, 1);
                        if (pixels[i, j].BG == Color.Invalid) pixels[i, j].BG = new(0, 0, 0, 0);
                        foreach (RendererObject subObject in subObjects)
                        {
                            Pixel[,] subPixels = subObject.Pixels;
                            int extraX = 0;
                            int extraY = 0;
                            if (subObject.ExternalAlignmentX != null)
                            {
                                if (subObject.ExternalAlignmentX == Alignment.Center)
                                {
                                    extraX = width / 2 - subObject.width / 2;
                                }
                                if (subObject.ExternalAlignmentX == Alignment.Right)
                                {
                                    extraX = width - subObject.width;
                                }
                            }
                            else
                            {
                                if (InternalAlignmentX == Alignment.Center)
                                {
                                    extraX = width / 2 - subObject.width / 2;
                                }
                                if (InternalAlignmentX == Alignment.Right)
                                {
                                    extraX = width - subObject.width;
                                }
                            }

                            if (subObject.ExternalAlignmentY != null)
                            {
                                if (subObject.ExternalAlignmentY == Alignment.Center)
                                {
                                    extraY = height / 2 - subObject.height / 2;
                                }
                                if (subObject.ExternalAlignmentY == Alignment.Right)
                                {
                                    extraY = height - subObject.height;
                                }
                            }
                            else
                            {
                                if (InternalAlignmentY == Alignment.Center)
                                {
                                    extraY = height / 2 - subObject.height / 2;
                                }
                                if (InternalAlignmentY == Alignment.Bottom)
                                {
                                    extraY = height - subObject.height;
                                }
                            }
                            if (
                                subObject.X + extraX <= i && i < subObject.X + extraX + subObject.width &&
                                subObject.Y + extraY <= j && j < subObject.Y + extraY + subObject.height
                            )
                            {
                                pixels[i, j] = pixels[i, j].WithOverlay(subPixels[i - subObject.X - extraX, j - subObject.Y - extraY]);
                            }
                        }
                    }
                }
                return pixels;
            }
        }



        public int X
        {
            get => x; set { x = value; UpdateParents(); }
        }
        public int Y
        {
            get => y; set { y = value; UpdateParents(); }
        }
        public int Width
        {
            get => width; set { width = value; Update(); sizeChanged = true; }
        }
        public int Height
        {
            get => height; set { height = value; Update(); sizeChanged = true; }
        }
        public char DefaultCharacter
        {
            get => defaultCharacter; set { defaultCharacter = value; Update(); }
        }
        public List<RendererObject> SubObjects
        {
            get { UpdateParents(); update = true; return subObjects; }
            set { UpdateParents(); update = true; subObjects = value; }
        }
        public List<ColorArea> ColorAreas
        {
            get { Update(); return colorAreas; }
            set { Update(); colorAreas = value; }
        }
        public int Frame
        {
            get => frame;
            set
            {
                int count = animation.Count;
                if (count > 0)
                    frame = (value > 0 ? value : count + value % count) % count;
                Update();
            }
        }
        public Alignment InternalAlignmentX
        {
            get => internalAlignmentX; set { internalAlignmentX = value; Update(); }
        }
        public Alignment InternalAlignmentY
        {
            get => internalAlignmentY; set { internalAlignmentY = value; Update(); }
        }
        public Alignment? ExternalAlignmentX
        {
            get => externalAlignmentX; set { externalAlignmentX = value; UpdateParents(); }
        }
        public Alignment? ExternalAlignmentY
        {
            get => externalAlignmentY; set { externalAlignmentY = value; UpdateParents(); }
        }
    }

    public class Renderer
    {
        private int terminalWidth = Console.WindowWidth;
        public int TerminalWidth { get => terminalWidth; }
        private int terminalHeight = Console.WindowHeight;
        public int TerminalHeight { get => terminalHeight; }

        public Color? FGOverride = null;
        public Color? BGOverride = null;

        private static readonly BufferedConsole Terminal = new();

        private Pixel[,] frameBuffer = new Pixel[Console.WindowWidth, Console.WindowHeight];
        private Pixel[,] oldFrameBuffer = new Pixel[Console.WindowWidth, Console.WindowHeight];

        public RendererObject Object = new();

        //clear the screen and buffers, reset the terminal size and set the background color to black
        public void Reset()
        {
            terminalWidth = Console.WindowWidth;
            terminalHeight = Console.WindowHeight;
            frameBuffer = new Pixel[terminalWidth, terminalHeight];
            oldFrameBuffer = new Pixel[terminalWidth, terminalHeight];
            Terminal.SetCursorPosition(0, 0);
            for (int i = 0; i < terminalWidth; i++)
            {
                for (int j = 0; j < terminalHeight; j++)
                {
                    WritePixel(i, j, new(' '));
                }
            }
            Terminal.Flush();
        }

        private void WritePixel(int x, int y, Pixel pixel)
        {
            Color FG = pixel.FG;
            Color BG = pixel.BG;
            if (FG == Color.Invalid) FG = Colors.White;
            if (BG == Color.Invalid) BG = Colors.Black;
            if (FGOverride != null) FG = (Color)FGOverride;
            if (BGOverride != null) BG = (Color)BGOverride;

            if (pixel.Ch >= ' ')
            {
                Terminal.SetFgColor(FG);
                Terminal.SetBgColor(BG);
                Terminal.Write(pixel.Ch);
            }
            else
            {
                Terminal.SetBgColor(Colors.Black);
                Terminal.Write(' ');
            }

            frameBuffer[x, y] = pixel;
        }

        public void Update(bool forceRedraw = true, bool preventBackgroundChange = false)
        {
            oldFrameBuffer = (Pixel[,])frameBuffer.Clone();
            int x = Object.X;
            int y = Object.Y;
            if (Object.ExternalAlignmentX == Alignment.Center) x += TerminalWidth / 2 - Object.Width / 2;
            if (Object.ExternalAlignmentX == Alignment.End) x += TerminalWidth - Object.Width;
            if (Object.ExternalAlignmentY == Alignment.Center) y += TerminalHeight / 2 - Object.Height / 2;
            if (Object.ExternalAlignmentY == Alignment.End) y += TerminalHeight - Object.Height;
            for (int j = 0; j < terminalHeight; j++)
            {
                for (int i = 0; i < terminalWidth; i++)
                    if (x <= i && i < Object.Width + x && y <= j && j < Object.Height + y)
                        frameBuffer[i, j] = Object.Pixels[i - x, j - y];
            }
            for (int j = 0; j < terminalHeight; j++)
            {
                bool updateRow = false;
                for (int i = 0; i < terminalWidth; i++)
                    if (frameBuffer[i, j] != oldFrameBuffer[i, j] || updateRow || forceRedraw)
                    {
                        if (!updateRow)
                        {
                            updateRow = true;
                            Terminal.SetCursorPosition(i, j);
                        }
                        if (x <= i && i < Object.Width + x && y <= j && j < Object.Height + y)
                            WritePixel(i, j, frameBuffer[i, j]);
                        else if (!preventBackgroundChange || oldFrameBuffer[i, j].Ch > ' ') WritePixel(i, j, new(' ', Color.Reset, Color.Reset));
                    }
            }
            Terminal.SetCursorPosition(0, 0);
            Terminal.Flush();
        }

        public bool UpdateScreenSize()
        {
            bool updated = false;
            if (Console.WindowWidth != terminalWidth || Console.WindowHeight != terminalHeight)
            {
                Reset();
                Update(true);
                updated = true;
            }
            return updated;
        }

        public ConsoleKeyInfo ReadKey()
        {
            ConsoleKeyInfo key = Console.ReadKey();
            for (int i = 0; i < terminalWidth; i++)
            {
                WritePixel(i, 0, frameBuffer[i, 0]);
            }
            Terminal.SetCursorPosition(0, 0);
            Terminal.Flush();
            return key;
        }
    }
}