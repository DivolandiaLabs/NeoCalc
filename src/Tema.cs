using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Media;

namespace NeoCalc
{
    // Un tema (skin) completo. Los colores van en hexadecimal (#RRGGBB o #AARRGGBB) para guardarlos en JSON.
    public class Tema
    {
        public string Id, Nombre;
        public bool Incluido;                 // temas que vienen con el programa (no se borran)
        public string Fondo1, Fondo2;         // degradado de la ventana
        public double AnguloFondo;            // 0 = vertical, 90 = horizontal
        public string Imagen;                 // imagen de fondo opcional
        public double OpacidadImagen;
        public string Texto, TextoSuave;      // titulo, expresion, botones de memoria
        public string Pantalla, PantallaTexto;
        public bool LCD;                      // pantalla con marco (estilo calculadora antigua)
        public string Num, NumTexto;          // 0-9 , +/-
        public string Fun, FunTexto;          // CE C % 1/x ...
        public string Op, OpTexto;            // + - x /
        public string Igual, IgualTexto;
        public string Borde;
        public double GrosorBorde;
        public string Acento;                 // brillo / detalles
        public double Radio, RadioVentana, Espacio;
        public double Brillo;                 // 0 = sin brillo neon
        public double Opacidad;               // de toda la ventana
        public string Fuente, PesoPantalla;
        public string FuenteTeclas, PesoTeclas;
        public double TamTeclas;

        public Tema Copia()
        {
            Tema t = (Tema)MemberwiseClone();
            return t;
        }

        // ---------------------------------------------------------------- colores

        public static Color C(string hex)
        {
            try
            {
                if (string.IsNullOrEmpty(hex)) return Colors.Transparent;
                return (Color)ColorConverter.ConvertFromString(hex);
            }
            catch { return Colors.Magenta; }
        }

        public static SolidColorBrush B(string hex)
        {
            SolidColorBrush b = new SolidColorBrush(C(hex));
            b.Freeze();
            return b;
        }

        public static string Hex(Color c)
        {
            if (c.A == 255) return string.Format("#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);
            return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", c.A, c.R, c.G, c.B);
        }

        public static double Luz(Color c)
        {
            return (0.2126 * c.R + 0.7152 * c.G + 0.0722 * c.B) / 255.0;
        }

        // aclara (o oscurece si el color ya es claro) para hover/pulsado
        public static Color Variar(Color c, double cuanto)
        {
            bool claro = Luz(c) > 0.6;
            double f = claro ? -cuanto : cuanto;
            Func<byte, byte> m = delegate (byte v)
            {
                double r = f > 0 ? v + (255 - v) * f : v * (1 + f);
                return (byte)Math.Max(0, Math.Min(255, r));
            };
            byte a = c.A;
            if (a < 255) a = (byte)Math.Min(255, a + 40 * cuanto * 5);
            return Color.FromArgb(a, m(c.R), m(c.G), m(c.B));
        }

        public static Color Mezclar(Color a, Color b, double t)
        {
            return Color.FromArgb((byte)(a.A + (b.A - a.A) * t), (byte)(a.R + (b.R - a.R) * t),
                                  (byte)(a.G + (b.G - a.G) * t), (byte)(a.B + (b.B - a.B) * t));
        }

        public static FontWeight Peso(string p)
        {
            switch ((p ?? "").ToLowerInvariant())
            {
                case "thin": return FontWeights.Thin;
                case "light": return FontWeights.Light;
                case "semilight": return FontWeight.FromOpenTypeWeight(350);
                case "medium": return FontWeights.Medium;
                case "semibold": return FontWeights.SemiBold;
                case "bold": return FontWeights.Bold;
                case "black": return FontWeights.Black;
            }
            return FontWeights.Normal;
        }

        public static readonly string[] Pesos = { "Thin", "Light", "SemiLight", "Normal", "Medium", "SemiBold", "Bold", "Black" };

        // ---------------------------------------------------------------- temas incluidos

        static Tema T(string id, string nombre, string f1, string f2, string texto, string suave,
                      string pantalla, string pantallaTxt, string num, string numTxt, string fun, string funTxt,
                      string op, string opTxt, string igual, string igualTxt, string borde, string acento,
                      double radio, double brillo, string fuente, string peso)
        {
            Tema t = new Tema();
            t.Id = id; t.Nombre = nombre; t.Incluido = true;
            t.Fondo1 = f1; t.Fondo2 = f2; t.AnguloFondo = 0;
            t.Texto = texto; t.TextoSuave = suave;
            t.Pantalla = pantalla; t.PantallaTexto = pantallaTxt;
            t.Num = num; t.NumTexto = numTxt; t.Fun = fun; t.FunTexto = funTxt;
            t.Op = op; t.OpTexto = opTxt; t.Igual = igual; t.IgualTexto = igualTxt;
            t.Borde = borde; t.GrosorBorde = borde == "#00000000" ? 0 : 1; t.Acento = acento;
            t.Radio = radio; t.RadioVentana = 10; t.Espacio = 2; t.Brillo = brillo; t.Opacidad = 1;
            t.Fuente = fuente; t.PesoPantalla = peso;
            t.FuenteTeclas = fuente; t.PesoTeclas = "Normal"; t.TamTeclas = 15;
            t.OpacidadImagen = 0.35;
            return t;
        }

        const string N = "#00000000"; // sin color

        public static List<Tema> Incluidos()
        {
            List<Tema> l = new List<Tema>();
            Tema t;

            t = T("win-oscuro", "Windows oscuro", "#202020", "#202020", "#FFFFFF", "#C8C8C8", N, "#FFFFFF",
                  "#3B3B3B", "#FFFFFF", "#323232", "#FFFFFF", "#323232", "#FFFFFF", "#4CC2FF", "#000000", "#0FFFFFFF", "#4CC2FF",
                  4, 0, "Segoe UI Variable Display", "SemiBold");
            l.Add(t);

            t = T("win-claro", "Windows claro", "#F3F3F3", "#EEEEEE", "#1B1B1B", "#5F5F5F", N, "#1B1B1B",
                  "#FFFFFF", "#1B1B1B", "#F9F9F9", "#1B1B1B", "#F9F9F9", "#1B1B1B", "#005FB8", "#FFFFFF", "#0F000000", "#005FB8",
                  4, 0, "Segoe UI Variable Display", "SemiBold");
            l.Add(t);

            t = T("neon", "Neón cyber", "#07070F", "#140A26", "#E6FBFF", "#7FA9C9", N, "#00F0FF",
                  "#0F1022", "#BFF8FF", "#0C0D1C", "#FF4FD8", "#0C0D1C", "#00F0FF", "#FF2BD6", "#FFFFFF", "#5500F0FF", "#00F0FF",
                  8, 0.6, "Bahnschrift", "Light");
            t.FuenteTeclas = "Bahnschrift"; t.Espacio = 5; t.RadioVentana = 14;
            l.Add(t);

            t = T("synthwave", "Synthwave", "#241046", "#5A1A5E", "#FFE9FF", "#E59AD9", N, "#FFD319",
                  "#3A1A66", "#FFE9FF", "#2C1450", "#FF8AD8", "#2C1450", "#FFD319", "#FF2E97", "#FFFFFF", "#40FF2E97", "#FF2E97",
                  10, 0.45, "Bahnschrift", "SemiBold");
            t.Espacio = 4; t.RadioVentana = 16;
            l.Add(t);

            t = T("matrix", "Matrix", "#000000", "#001006", "#00FF41", "#00A82B", N, "#00FF41",
                  "#000F05", "#00FF41", "#000A03", "#00C832", "#000A03", "#7CFF9C", "#00FF41", "#000000", "#3300FF41", "#00FF41",
                  2, 0.5, "Consolas", "Normal");
            t.FuenteTeclas = "Consolas"; t.Espacio = 3; t.RadioVentana = 4;
            l.Add(t);

            t = T("lcd", "LCD retro", "#3B3D40", "#2A2C2F", "#E8E8E8", "#A9ACB1", "#B9C7A0", "#1E2715",
                  "#1D1F22", "#F1F1F1", "#53575D", "#F1F1F1", "#53575D", "#F1F1F1", "#E8662C", "#FFFFFF", "#22000000", "#E8662C",
                  6, 0, "Consolas", "Normal");
            t.LCD = true; t.Espacio = 6; t.RadioVentana = 18; t.FuenteTeclas = "Segoe UI"; t.PesoTeclas = "SemiBold";
            l.Add(t);

            t = T("ambar", "Terminal ámbar", "#0B0700", "#140D00", "#FFB000", "#B37B00", N, "#FFB000",
                  "#1A1100", "#FFB000", "#120C00", "#D99600", "#120C00", "#FFC940", "#FFB000", "#0B0700", "#33FFB000", "#FFB000",
                  0, 0.35, "Lucida Console", "Normal");
            t.FuenteTeclas = "Lucida Console"; t.RadioVentana = 2;
            l.Add(t);

            t = T("dracula", "Drácula", "#282A36", "#21222C", "#F8F8F2", "#6272A4", N, "#F8F8F2",
                  "#44475A", "#F8F8F2", "#343746", "#8BE9FD", "#343746", "#BD93F9", "#FF79C6", "#282A36", N, "#BD93F9",
                  8, 0, "Cascadia Mono", "Normal");
            t.FuenteTeclas = "Cascadia Mono"; t.Espacio = 4;
            l.Add(t);

            t = T("nord", "Nórdico", "#2E3440", "#3B4252", "#ECEFF4", "#9AA5B8", N, "#ECEFF4",
                  "#434C5E", "#ECEFF4", "#3B4252", "#88C0D0", "#3B4252", "#81A1C1", "#88C0D0", "#2E3440", N, "#88C0D0",
                  10, 0, "Segoe UI", "Light");
            t.Espacio = 4; t.RadioVentana = 14;
            l.Add(t);

            t = T("solar", "Solarizado", "#002B36", "#073642", "#EEE8D5", "#839496", N, "#EEE8D5",
                  "#0A4150", "#EEE8D5", "#073642", "#2AA198", "#073642", "#B58900", "#CB4B16", "#FDF6E3", N, "#B58900",
                  6, 0, "Segoe UI", "Normal");
            l.Add(t);

            t = T("monokai", "Monokai", "#272822", "#1E1F1A", "#F8F8F2", "#75715E", N, "#F8F8F2",
                  "#3E3D32", "#F8F8F2", "#31322B", "#66D9EF", "#31322B", "#F92672", "#A6E22E", "#272822", N, "#A6E22E",
                  6, 0, "Consolas", "Normal");
            t.FuenteTeclas = "Consolas"; t.Espacio = 3;
            l.Add(t);

            t = T("oceano", "Océano profundo", "#0F2027", "#2C5364", "#E8F6FF", "#8FB8CC", N, "#FFFFFF",
                  "#26FFFFFF", "#FFFFFF", "#14FFFFFF", "#BDEBFF", "#14FFFFFF", "#7FDBFF", "#00C6FF", "#00141F", "#22FFFFFF", "#00C6FF",
                  12, 0.25, "Segoe UI", "Light");
            t.Espacio = 5; t.RadioVentana = 18;
            l.Add(t);

            t = T("atardecer", "Atardecer", "#FF7E5F", "#6A3093", "#FFFFFF", "#FFE0D6", N, "#FFFFFF",
                  "#30FFFFFF", "#FFFFFF", "#1AFFFFFF", "#FFFFFF", "#1AFFFFFF", "#FFFFFF", "#FFFFFF", "#C2456D", "#33FFFFFF", "#FFD3A5",
                  14, 0, "Segoe UI", "Light");
            t.Espacio = 6; t.RadioVentana = 20;
            l.Add(t);

            t = T("bosque", "Bosque", "#14261A", "#24402C", "#EAF5E4", "#9DB89C", N, "#EAF5E4",
                  "#2F4F37", "#EAF5E4", "#223B29", "#B5D99C", "#223B29", "#B5D99C", "#8BC34A", "#14261A", N, "#8BC34A",
                  8, 0, "Georgia", "Normal");
            t.FuenteTeclas = "Segoe UI"; t.Espacio = 4;
            l.Add(t);

            t = T("lava", "Lava", "#120300", "#3D0C02", "#FFE6D9", "#D08A6E", N, "#FFB38A",
                  "#2A0A03", "#FFE6D9", "#1E0702", "#FF8A50", "#1E0702", "#FF6A2B", "#FF4500", "#FFFFFF", "#44FF4500", "#FF4500",
                  6, 0.5, "Bahnschrift", "SemiBold");
            t.Espacio = 4;
            l.Add(t);

            t = T("hielo", "Hielo", "#E6F7FF", "#FFFFFF", "#0B3D5C", "#5C8AA6", N, "#0B3D5C",
                  "#FFFFFF", "#0B3D5C", "#D9F1FF", "#0B3D5C", "#D9F1FF", "#0077B6", "#00A3E0", "#FFFFFF", "#3300A3E0", "#00A3E0",
                  12, 0.2, "Segoe UI", "Light");
            t.Espacio = 5; t.RadioVentana = 16;
            l.Add(t);

            t = T("pastel", "Rosa pastel", "#FFE4EC", "#FFF4E6", "#5A3946", "#A5798A", N, "#5A3946",
                  "#FFFFFF", "#5A3946", "#FFD6E2", "#7A4A5C", "#FFD6E2", "#C2557E", "#FF8FB1", "#FFFFFF", N, "#FF8FB1",
                  18, 0, "Segoe UI", "SemiLight");
            t.Espacio = 6; t.RadioVentana = 22;
            l.Add(t);

            t = T("oro", "Oro y negro", "#0B0B0B", "#1A1A1A", "#F5E6B8", "#A8955A", N, "#F5E6B8",
                  "#151515", "#F5E6B8", "#101010", "#D4AF37", "#101010", "#D4AF37", "#D4AF37", "#0B0B0B", "#66D4AF37", "#D4AF37",
                  3, 0.25, "Georgia", "Normal");
            t.FuenteTeclas = "Georgia"; t.Espacio = 4;
            l.Add(t);

            t = T("cristal", "Cristal", "#99161B2E", "#661B2A4A", "#FFFFFF", "#C4CCE0", N, "#FFFFFF",
                  "#22FFFFFF", "#FFFFFF", "#14FFFFFF", "#FFFFFF", "#14FFFFFF", "#FFFFFF", "#88A6C8FF", "#FFFFFF", "#33FFFFFF", "#A6C8FF",
                  10, 0, "Segoe UI Variable Display", "Light");
            t.Espacio = 5; t.RadioVentana = 18;
            l.Add(t);

            t = T("ios", "Redonda (estilo iPhone)", "#000000", "#000000", "#FFFFFF", "#A5A5A5", N, "#FFFFFF",
                  "#333333", "#FFFFFF", "#A5A5A5", "#000000", "#FF9F0A", "#FFFFFF", "#FF9F0A", "#FFFFFF", N, "#FF9F0A",
                  999, 0, "Segoe UI", "Light");
            t.Espacio = 8; t.RadioVentana = 26; t.TamTeclas = 20;
            l.Add(t);

            t = T("holo", "Holograma", "#030812", "#0A1A2E", "#CFFBFF", "#6FA7B5", N, "#7DF9FF",
                  "#0A7DF9FF", "#CFFBFF", "#057DF9FF", "#7DF9FF", "#057DF9FF", "#7DF9FF", "#337DF9FF", "#FFFFFF", "#997DF9FF", "#7DF9FF",
                  0, 0.7, "Bahnschrift", "Light");
            t.Espacio = 6; t.RadioVentana = 0; t.FuenteTeclas = "Bahnschrift"; t.PesoTeclas = "Light";
            l.Add(t);

            t = T("papel", "Papel", "#F5F0E6", "#ECE4D3", "#3B3024", "#8C7B66", N, "#3B3024",
                  "#FFFDF7", "#3B3024", "#EFE7D6", "#5C4B37", "#EFE7D6", "#8B4513", "#8B4513", "#FFFDF7", "#22000000", "#8B4513",
                  4, 0, "Georgia", "Normal");
            t.FuenteTeclas = "Georgia"; t.Espacio = 4;
            l.Add(t);

            t = T("gameboy", "Game Boy", "#8BAC0F", "#9BBC0F", "#0F380F", "#306230", "#9BBC0F", "#0F380F",
                  "#306230", "#9BBC0F", "#306230", "#9BBC0F", "#0F380F", "#9BBC0F", "#0F380F", "#9BBC0F", N, "#0F380F",
                  2, 0, "Consolas", "Bold");
            t.LCD = true; t.FuenteTeclas = "Consolas"; t.PesoTeclas = "Bold"; t.Espacio = 5; t.RadioVentana = 8;
            l.Add(t);

            t = T("contraste", "Alto contraste", "#000000", "#000000", "#FFFFFF", "#FFFF00", N, "#FFFFFF",
                  "#000000", "#FFFFFF", "#000000", "#FFFF00", "#000000", "#00FFFF", "#FFFF00", "#000000", "#FFFFFF", "#FFFF00",
                  0, 0, "Segoe UI", "Bold");
            t.GrosorBorde = 2; t.PesoTeclas = "Bold"; t.Espacio = 3; t.RadioVentana = 0;
            l.Add(t);

            t = T("cafe", "Café", "#2B1D16", "#3E2A20", "#F3E5D8", "#BFA08A", N, "#F3E5D8",
                  "#4A3428", "#F3E5D8", "#3A281F", "#E0B48C", "#3A281F", "#E0B48C", "#C8875A", "#2B1D16", N, "#C8875A",
                  10, 0, "Segoe UI", "SemiLight");
            t.Espacio = 4; t.RadioVentana = 14;
            l.Add(t);

            t = T("vapor", "Vaporwave", "#1A0B2E", "#2B0F45", "#FFFFFF", "#B967FF", N, "#01CDFE",
                  "#2A1450", "#FFFFFF", "#22103F", "#05FFA1", "#22103F", "#01CDFE", "#FF71CE", "#1A0B2E", "#55B967FF", "#B967FF",
                  6, 0.5, "Bahnschrift", "SemiBold");
            t.Espacio = 4; t.AnguloFondo = 45;
            l.Add(t);

            return l;
        }

        // Tema al azar (boton "Aleatorio" del editor)
        public static Tema Aleatorio(Random rnd)
        {
            double h = rnd.NextDouble() * 360, h2 = (h + 120 + rnd.NextDouble() * 120) % 360;
            bool oscuro = rnd.NextDouble() < 0.7;
            Tema t = new Tema();
            t.Id = "mio-" + Guid.NewGuid().ToString("N").Substring(0, 8);
            t.Nombre = "Aleatorio";
            Func<double, double, double, string> hsl = delegate (double hh, double s, double l) { return Hex(Hsl(hh, s, l)); };
            if (oscuro)
            {
                t.Fondo1 = hsl(h, 0.35, 0.08); t.Fondo2 = hsl(h, 0.45, 0.16);
                t.Texto = hsl(h, 0.2, 0.95); t.TextoSuave = hsl(h, 0.2, 0.65);
                t.Num = hsl(h, 0.3, 0.2); t.NumTexto = hsl(h, 0.1, 0.96);
                t.Fun = hsl(h, 0.3, 0.15); t.FunTexto = hsl(h2, 0.7, 0.75);
                t.Op = hsl(h, 0.3, 0.15); t.OpTexto = hsl(h2, 0.8, 0.7);
                t.Igual = hsl(h2, 0.8, 0.6); t.IgualTexto = hsl(h2, 0.3, 0.08);
                t.PantallaTexto = hsl(h2, 0.5, 0.9);
            }
            else
            {
                t.Fondo1 = hsl(h, 0.5, 0.94); t.Fondo2 = hsl(h, 0.45, 0.88);
                t.Texto = hsl(h, 0.4, 0.15); t.TextoSuave = hsl(h, 0.2, 0.45);
                t.Num = "#FFFFFF"; t.NumTexto = hsl(h, 0.4, 0.15);
                t.Fun = hsl(h, 0.5, 0.97); t.FunTexto = hsl(h2, 0.6, 0.35);
                t.Op = hsl(h, 0.5, 0.97); t.OpTexto = hsl(h2, 0.7, 0.4);
                t.Igual = hsl(h2, 0.7, 0.5); t.IgualTexto = "#FFFFFF";
                t.PantallaTexto = hsl(h, 0.4, 0.15);
            }
            t.Pantalla = N; t.Borde = N; t.GrosorBorde = 0; t.Acento = t.Igual;
            t.Radio = new double[] { 0, 4, 8, 12, 999 }[rnd.Next(5)];
            t.RadioVentana = rnd.Next(0, 24); t.Espacio = rnd.Next(2, 8);
            t.Brillo = oscuro && rnd.NextDouble() < 0.4 ? 0.5 : 0;
            t.Opacidad = 1; t.OpacidadImagen = 0.35; t.AnguloFondo = rnd.Next(0, 4) * 45;
            string[] fuentes = { "Segoe UI", "Bahnschrift", "Consolas", "Georgia", "Cascadia Mono", "Segoe UI Variable Display" };
            t.Fuente = fuentes[rnd.Next(fuentes.Length)]; t.FuenteTeclas = t.Fuente;
            t.PesoPantalla = new string[] { "Light", "Normal", "SemiBold" }[rnd.Next(3)]; t.PesoTeclas = "Normal";
            t.TamTeclas = 15;
            return t;
        }

        public static Color Hsl(double h, double s, double l)
        {
            double c = (1 - Math.Abs(2 * l - 1)) * s, x = c * (1 - Math.Abs(h / 60 % 2 - 1)), m = l - c / 2;
            double r = 0, g = 0, b = 0;
            if (h < 60) { r = c; g = x; } else if (h < 120) { r = x; g = c; } else if (h < 180) { g = c; b = x; }
            else if (h < 240) { g = x; b = c; } else if (h < 300) { r = x; b = c; } else { r = c; b = x; }
            return Color.FromRgb((byte)((r + m) * 255), (byte)((g + m) * 255), (byte)((b + m) * 255));
        }
    }

    // Ajustes del programa + temas propios, en %APPDATA%\NeoCalc\config.json
    public class Config
    {
        public string Tema = "win-oscuro";
        public string Idioma = "";            // "" = idioma de Windows
        public bool Cientifica;
        public string Angulo = "DEG";
        public double X = -99999, Y = -99999, Ancho = 340, Alto = 540;
        public double AnchoCientifica = 400, AltoCientifica = 620;
        public bool SiempreEncima;
        public bool IconoDelTema = true;
        public bool PanelLateral = true;
        public List<Tema> Propios = new List<Tema>();
        public List<double> Memoria = new List<double>();
        public List<string[]> Historial = new List<string[]>();

        public static string Carpeta
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NeoCalc"); }
        }
        static string Ruta { get { return Path.Combine(Carpeta, "config.json"); } }

        public static Config Cargar()
        {
            try
            {
                if (File.Exists(Ruta))
                {
                    Config c = new JavaScriptSerializer().Deserialize<Config>(File.ReadAllText(Ruta));
                    if (c != null)
                    {
                        if (c.Propios == null) c.Propios = new List<Tema>();
                        if (c.Memoria == null) c.Memoria = new List<double>();
                        if (c.Historial == null) c.Historial = new List<string[]>();
                        foreach (Tema t in c.Propios) t.Incluido = false;
                        return c;
                    }
                }
            }
            catch { }
            return new Config();
        }

        public void Guardar()
        {
            try
            {
                Directory.CreateDirectory(Carpeta);
                string tmp = Ruta + ".tmp";
                File.WriteAllText(tmp, new JavaScriptSerializer().Serialize(this));
                if (File.Exists(Ruta)) File.Delete(Ruta);
                File.Move(tmp, Ruta);
            }
            catch { }
        }

        public List<Tema> Todos()
        {
            List<Tema> l = NeoCalc.Tema.Incluidos();
            l.AddRange(Propios);
            return l;
        }

        public Tema Buscar(string id)
        {
            foreach (Tema t in Todos()) if (t.Id == id) return t;
            return NeoCalc.Tema.Incluidos()[0];
        }
    }
}
