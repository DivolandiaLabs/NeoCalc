using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeoCalc
{
    public static class App
    {
        [STAThread]
        public static int Main(string[] args)
        {
            L.Poner(Environment.GetEnvironmentVariable("NEOCALC_IDIOMA") ?? "");
            // build.ps1 llama a "/icono ruta.ico" para generar el icono del programa
            if (args.Length == 2 && args[0] == "/icono") { Icono.GuardarIco(args[1]); return 0; }
            // pruebas sin mostrar ventanas
            if (args.Length == 2 && args[0] == "/prueba") { File.WriteAllText(args[1], Pruebas.Ejecutar(), Encoding.UTF8); return 0; }
            if (args.Length >= 4 && args[0] == "/captura") { Captura(args[1], args[2] == "cientifica", args[3], args.Length > 4 ? args[4] : null); return 0; }
            if (args.Length == 2 && args[0] == "/galeria") { Galeria(args[1]); return 0; }
            if (args.Length == 2 && args[0] == "/capturatamanos") { CapturaTamanos(args[1]); return 0; }
            if (args.Length == 2 && args[0] == "/capturatemas")
            {
                Config ct = new Config(); ct.Tema = "synthwave"; ct.Historial = new List<string[]>();
                VentanaTemas vt = new VentanaTemas(new VentanaCalc(ct));
                FrameworkElement r = (FrameworkElement)vt.Content; vt.Content = null;
                Border b = new Border { Background = vt.Background, Child = r, Width = 1040, Height = 720 };
                b.Measure(new Size(1040, 720)); b.Arrange(new Rect(0, 0, 1040, 720)); b.UpdateLayout();
                RenderTargetBitmap bmp = new RenderTargetBitmap(1040, 720, 96, 96, PixelFormats.Pbgra32);
                bmp.Render(b); Guardar(bmp, args[1]);
                return 0;
            }
            if (args.Length == 3 && args[0] == "/acceso")
            {
                Tema t = new Config().Buscar(args[2]);
                string ic = Barra.IconoParaBarra(t, true);
                int c = ic.LastIndexOf(',');
                File.WriteAllText(args[1] + ".txt", Barra.ActualizarAcceso(args[1], ic.Substring(0, c), 0) + " " + ic);
                return 0;
            }
            if (args.Length == 2 && args[0] == "/propuestas") { IconoPropuestas.Hoja(args[1]); return 0; }
            if (args.Length == 2 && args[0] == "/iconos") { Iconos(args[1]); return 0; }

            Application app = new Application();
            app.ShutdownMode = ShutdownMode.OnMainWindowClose;
            app.DispatcherUnhandledException += delegate (object s, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
            {
                try { File.AppendAllText(Path.Combine(Config.Carpeta, "errores.log"), DateTime.Now + " " + e.Exception + "\r\n\r\n"); } catch { }
                e.Handled = true;
            };
            Config cfg = Config.Cargar();
            L.Poner(cfg.Idioma);
            VentanaCalc v = new VentanaCalc(cfg);
            app.MainWindow = v;
            v.Show();
            if (args.Length > 0 && args[0] == "/temas") v.AbrirTemas();
            return app.Run();
        }

        // Dibuja la calculadora con un tema en un PNG (para revisar los temas sin abrir ventanas)
        static void Captura(string temaId, bool cientifica, string png, string teclas)
        {
            Config cfg = new Config();
            cfg.Tema = temaId; cfg.Cientifica = cientifica;
            cfg.Historial = new List<string[]>();
            VentanaCalc v = new VentanaCalc(cfg);
            double w = cientifica ? 420 : 340, h = cientifica ? 640 : 540;
            if (teclas != null && teclas.StartsWith("ancha:")) { w = 720; teclas = teclas.Substring(6); }
            v.AnchoPrueba = w;
            if (!string.IsNullOrEmpty(teclas)) foreach (string a in teclas.Split(' ')) if (a.Length > 0) v.Ejecutar(a);
            FrameworkElement r = Render(v, w, h);
            Guardar(Foto(r, w, h), png);
        }

        static FrameworkElement Render(VentanaCalc v, double w, double h)
        {
            v.Construir();
            FrameworkElement r = (FrameworkElement)v.Content;
            v.Content = null;
            r.Width = w - 20; r.Height = h - 20;
            r.Measure(new Size(w, h)); r.Arrange(new Rect(0, 0, w, h)); r.UpdateLayout();
            v.Refrescar();
            r.UpdateLayout();
            return r;
        }

        static BitmapSource Foto(Visual r, double w, double h)
        {
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)(w * 1.5), (int)(h * 1.5), 144, 144, PixelFormats.Pbgra32);
            bmp.Render(r);
            return bmp;
        }

        static void Guardar(BitmapSource b, string png) { Icono.GuardarPng(b, png); }

        // La calculadora a varios tamanos, con el asa de la esquina y el boton "Fijar este tamano" (para la web)
        static void CapturaTamanos(string png)
        {
            double[] escalas = { 0.55, 0.8, 1.1 };
            StackPanel fila = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Bottom, Margin = new Thickness(30) };
            for (int i = 0; i < escalas.Length; i++)
            {
                Config cfg = new Config(); cfg.Tema = "synthwave"; cfg.Historial = new List<string[]>();
                VentanaCalc v = new VentanaCalc(cfg);
                v.AnchoPrueba = 340;
                foreach (string a in "1 2 3 + 4 5 =".Split(' ')) v.Ejecutar(a);
                v.Construir();
                FrameworkElement r = (FrameworkElement)v.Content;
                v.Content = null;
                r.Width = 320; r.Height = 520;
                Grid caja = new Grid { Width = 340, Height = 540, VerticalAlignment = VerticalAlignment.Bottom };
                caja.Children.Add(r);
                if (i == escalas.Length - 1)
                {
                    Tema t = cfg.Buscar("synthwave");
                    Color ca = Tema.C(t.Acento); ca.A = 255;
                    System.Windows.Shapes.Path rayas = new System.Windows.Shapes.Path
                    {
                        Data = Geometry.Parse("M 17,6 L 6,17 M 17,10.5 L 10.5,17 M 17,15 L 15,17"),
                        Stroke = new SolidColorBrush(ca), StrokeThickness = 1.8, StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round,
                        HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Bottom, Width = 22, Height = 22
                    };
                    caja.Children.Add(rayas);
                    Border pill = new Border { HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Bottom, Margin = new Thickness(0, 0, 24, 24), CornerRadius = new CornerRadius(16), Padding = new Thickness(12, 7, 14, 7), Background = new SolidColorBrush(ca) };
                    pill.Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 12, ShadowDepth = 2, Opacity = 0.45 };
                    StackPanel sp = new StackPanel { Orientation = Orientation.Horizontal };
                    sp.Children.Add(new TextBlock { Text = "\uE840", FontFamily = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets"), FontSize = 13, Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) });
                    sp.Children.Add(new TextBlock { Text = L.T("Fijar este tamaño"), FontSize = 13, FontWeight = FontWeights.SemiBold, Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center, FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI") });
                    pill.Child = sp;
                    caja.Children.Add(pill);
                }
                caja.LayoutTransform = new ScaleTransform(escalas[i], escalas[i]);
                fila.Children.Add(caja);
                if (i < escalas.Length - 1)
                    fila.Children.Add(new TextBlock { Text = "\u2192", FontSize = 34, Foreground = new SolidColorBrush(Color.FromRgb(0x5B, 0x64, 0x7A)), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(14, 0, 14, 0) });
            }
            Border fondo = new Border { Background = new SolidColorBrush(Color.FromRgb(0x0A, 0x0D, 0x14)), Child = fila };
            fondo.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Size t0 = fondo.DesiredSize;
            fondo.Arrange(new Rect(0, 0, t0.Width, t0.Height)); fondo.UpdateLayout();
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)Math.Ceiling(t0.Width * 2), (int)Math.Ceiling(t0.Height * 2), 192, 192, PixelFormats.Pbgra32);
            bmp.Render(fondo);
            Guardar(bmp, png);
        }

        // Todas las skins en una sola imagen
        static void Galeria(string png)
        {
            List<Tema> temas = Tema.Incluidos();
            int cols = 7;
            double w = 300, h = 470;
            int filas = (temas.Count + cols - 1) / cols;
            Grid g = new Grid { Background = new SolidColorBrush(Color.FromRgb(0x60, 0x66, 0x74)) };
            for (int c = 0; c < cols; c++) g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(w) });
            for (int f = 0; f < filas; f++) g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(h + 22) });
            for (int i = 0; i < temas.Count; i++)
            {
                Config cfg = new Config(); cfg.Tema = temas[i].Id; cfg.Historial = new List<string[]>();
                VentanaCalc v = new VentanaCalc(cfg);
                v.AnchoPrueba = w;
                foreach (string a in "1 2 3 4 * 5 6 =".Split(' ')) v.Ejecutar(a);
                v.Construir();
                FrameworkElement r = (FrameworkElement)v.Content;
                v.Content = null;
                r.Width = w - 20; r.Height = h - 20; r.Margin = new Thickness(10);
                StackPanel sp = new StackPanel();
                sp.Children.Add(r);
                sp.Children.Add(new TextBlock { Text = temas[i].Nombre, Foreground = Brushes.White, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center });
                Grid.SetColumn(sp, i % cols); Grid.SetRow(sp, i / cols);
                g.Children.Add(sp);
            }
            double W = cols * w, H = filas * (h + 22);
            g.Measure(new Size(W, H)); g.Arrange(new Rect(0, 0, W, H)); g.UpdateLayout();
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)W, (int)H, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(g);
            Guardar(bmp, png);
        }

        // El icono a todos sus tamanos (y con colores de varios temas) sobre fondo claro y oscuro
        static void Iconos(string png)
        {
            int[] tams = { 16, 20, 24, 32, 48, 64, 256 };
            DrawingVisual dv = new DrawingVisual();
            List<Tema> temas = Tema.Incluidos();
            string[] ids = { "neon", "synthwave", "matrix", "lava", "win-claro", "oro", "vapor" };
            double W = 760, H = 640;
            using (DrawingContext dc = dv.RenderOpen())
            {
                dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20)), null, new Rect(0, 0, W, H / 2));
                dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0)), null, new Rect(0, H / 2, W, H / 2));
                for (int fondo = 0; fondo < 2; fondo++)
                {
                    double x = 20, y = fondo * H / 2 + 20;
                    foreach (int s in tams)
                    {
                        dc.DrawImage(Icono.Imagen(s), new Rect(x, y + (s < 256 ? 0 : 0), s, s));
                        x += s + 18;
                    }
                    x = 20; y += 270;
                    foreach (string id in ids)
                    {
                        Tema t = temas.Find(q => q.Id == id);
                        BitmapFrame f = Icono.DelTema(t);
                        BitmapSource b = null;
                        foreach (BitmapFrame fr in f.Decoder.Frames) if (fr.PixelWidth == 32) b = fr;
                        dc.DrawImage(b ?? f, new Rect(x, y, 32, 32));
                        x += 50;
                    }
                }
            }
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)W, (int)H, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(dv);
            Guardar(bmp, png);
        }
    }

    // Comprobaciones del motor (NeoCalc.exe /prueba salida.txt)
    public static class Pruebas
    {
        public static string Ejecutar()
        {
            StringBuilder sb = new StringBuilder();
            int mal = 0, total = 0;
            Action<bool, string, string> P = delegate (bool cient, string teclas, string esperado)
            {
                Motor m = new Motor { Cientifica = cient };
                foreach (string a in teclas.Split(' ')) if (a.Length > 0) m.Pulsar(a);
                string r = m.Pantalla;
                total++;
                bool ok = r == esperado;
                if (!ok) mal++;
                sb.AppendLine((ok ? "OK   " : "FALLO") + "  [" + (cient ? "cie" : "est") + "] " + teclas + "  ->  " + r + (ok ? "" : "   (esperado " + esperado + ")") + "   expr: " + m.Expresion);
            };
            P(false, "3 + 5 * 2 =", "16");
            P(true, "3 + 5 * 2 =", "13");
            P(false, "0 . 1 + 0 . 2 =", "0,3");
            P(false, "5 + 3 = =", "11");
            P(false, "5 + =", "10");
            P(false, "1 0 0 + 1 0 %", "10");
            P(false, "2 0 0 * 1 0 %", "0,1");
            P(false, "9 sqrt", "3");
            P(false, "4 inv", "0,25");
            P(false, "1 / 0 =", "No se puede dividir por cero");
            P(false, "0 / 0 =", "Resultado indefinido");
            P(false, "1 2 3 4 5 6 7", "1.234.567");
            P(false, "1 2 back back back", "0");
            P(false, "5 neg * 2 =", "-10");
            P(false, "2 sqr sqr", "16");
            P(false, "7 * =", "49");
            P(false, "1 0 - 4 = 2 0 =", "16");
            P(true, "2 ^ 1 0 =", "1.024");
            P(true, "( 2 + 3 ) * 4 =", "20");
            P(true, "2 * ( 3 + 4 =", "14");
            P(true, "9 0 sin", "1");
            P(true, "1 8 0 cos", "-1");
            P(true, "5 fact", "120");
            P(true, "1 0 0 log", "2");
            P(true, "2 exp 3 =", "2.000");
            P(true, "1 7 mod 5 =", "2");
            P(true, "2 7 yroot 3 =", "3");
            P(true, "8 logy 2 =", "3");
            P(true, "2 + 3 * 4 ^ 2 =", "50");
            P(true, "2 ^ 3 ^ 2 =", "512");
            P(true, "1 0 0 0 0 0 0 0 0 * 1 0 0 0 0 0 0 0 0 * 1 0 0 =", "1,e+18");
            P(true, "2 + 3 *", "3");
            P(true, "2 * 3 +", "6");
            P(true, "pi", "3,141592653589793");
            P(false, "2 sqrt", "1,414213562373095");
            P(false, "1 / 3 * 3 =", "1");
            P(false, "1 neg sqrt", "Entrada no válida");
            P(false, "1 neg sqrt 5", "5");
            P(false, "0 . 3 - 0 . 1 - 0 . 2 =", "0");
            P(false, "2 + 3 = 4", "4");
            P(false, "1 2 MS C MR + 1 =", "13");
            P(true, "2 ( 3 ) =", "6");
            P(true, "0 . 5 fact", "0,88622692545276");
            sb.Insert(0, "Pruebas: " + (total - mal) + "/" + total + " correctas\r\n\r\n");
            return sb.ToString();
        }
    }
}
