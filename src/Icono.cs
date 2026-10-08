using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeoCalc
{
    // Logo de NeoCalc: cuadrado oscuro redondeado con un fino borde degradado (cian -> violeta),
    // una barra luminosa como pantalla y un teclado de 3x3 puntos cuyo ultimo punto es la tecla "=" (LED violeta).
    // Se dibuja a cada tamano por separado para que a 16 px siga siendo nitido.
    public static class Icono
    {
        public static readonly Color Cian = Color.FromRgb(0x22, 0xE3, 0xFF);
        public static readonly Color Violeta = Color.FromRgb(0x9B, 0x5C, 0xFF);

        public static BitmapSource Imagen(int s) { return Imagen(s, Cian, Violeta, Color.FromRgb(0x0B, 0x0E, 0x1A), Color.FromRgb(0x1A, 0x1F, 0x38)); }

        public static BitmapSource Imagen(int s, Color c1, Color c2, Color f1, Color f2)
        {
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                double S = s;
                bool chico = s <= 24;
                LinearGradientBrush acento = new LinearGradientBrush(c1, c2, new Point(0, 0), new Point(1, 1));
                // fondo
                double rad = S * 0.24;
                LinearGradientBrush fondo = new LinearGradientBrush(f1, f2, new Point(0, 0), new Point(1, 1));
                double grosor = Math.Max(1, Math.Round(S * 0.035));
                Rect r = new Rect(grosor / 2, grosor / 2, S - grosor, S - grosor);
                dc.DrawRoundedRectangle(fondo, new Pen(acento, grosor), r, rad, rad);

                // brillo interior sutil arriba (cristal)
                if (!chico)
                {
                    LinearGradientBrush reflejo = new LinearGradientBrush(Color.FromArgb(28, 255, 255, 255), Color.FromArgb(0, 255, 255, 255), new Point(0, 0), new Point(0, 0.5));
                    dc.DrawRoundedRectangle(reflejo, null, new Rect(grosor, grosor, S - 2 * grosor, S * 0.5), rad * 0.9, rad * 0.9);
                }

                // pantalla = la barra larga del logo original (degradado + halo)
                double t = chico ? Math.Max(2, Math.Round(S * 0.12)) : S * 0.085;
                double x0 = S * 0.27, x1 = S * 0.73;
                double y1 = S * 0.30;
                if (chico) { x0 = Math.Round(x0); x1 = Math.Round(x1); y1 = Math.Round(y1) + (t % 2 == 1 ? 0.5 : 0); }
                if (s >= 32)
                {
                    for (int i = 3; i >= 1; i--)
                    {
                        byte a = (byte)(18 * (4 - i));
                        Pen halo = new Pen(new SolidColorBrush(Color.FromArgb(a, c1.R, c1.G, c1.B)), t + S * 0.035 * i);
                        halo.StartLineCap = halo.EndLineCap = PenLineCap.Round;
                        dc.DrawLine(halo, new Point(x0, y1), new Point(x1, y1));
                    }
                }
                LinearGradientBrush trazo = new LinearGradientBrush(c1, c2, new Point(0, 0), new Point(1, 0));
                trazo.MappingMode = BrushMappingMode.Absolute;
                trazo.StartPoint = new Point(x0, 0); trazo.EndPoint = new Point(x1, 0);
                Pen p = new Pen(trazo, t);
                p.StartLineCap = p.EndLineCap = chico ? PenLineCap.Flat : PenLineCap.Round;
                dc.DrawLine(p, new Point(x0 + (chico ? 0 : t / 2), y1), new Point(x1 - (chico ? 0 : t / 2), y1));

                // teclado: rejilla 3x3 de puntos; el ultimo es la tecla "=" (LED violeta)
                Brush tecla = new SolidColorBrush(Color.FromArgb(150, 0x9E, 0xB4, 0xE6));
                double[] xs = { 0.31, 0.5, 0.69 }, ys = { 0.5, 0.64, 0.78 };
                double rr = chico ? Math.Max(1, Math.Round(S * 0.05)) : S * 0.045;
                for (int f = 0; f < 3; f++)
                    for (int c = 0; c < 3; c++)
                    {
                        Point pt = new Point(S * xs[c], S * ys[f]);
                        if (chico) pt = new Point(Math.Round(pt.X), Math.Round(pt.Y));
                        if (f < 2 || c < 2) { dc.DrawEllipse(tecla, null, pt, rr, rr); continue; }
                        double lr = rr * 1.2;
                        if (s >= 32)
                            for (int i = 3; i >= 1; i--)
                            {
                                byte a = (byte)(18 * (4 - i));
                                dc.DrawEllipse(new SolidColorBrush(Color.FromArgb(a, c2.R, c2.G, c2.B)), null, pt, lr + S * 0.03 * i, lr + S * 0.03 * i);
                            }
                        dc.DrawEllipse(new SolidColorBrush(c2), null, pt, lr, lr);
                        if (s >= 32) dc.DrawEllipse(new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)), null, pt, lr * 0.4, lr * 0.4);
                    }
            }
            RenderTargetBitmap bmp = new RenderTargetBitmap(s, s, 96, 96, PixelFormats.Pbgra32);
            RenderOptions.SetEdgeMode(dv, EdgeMode.Unspecified);
            bmp.Render(dv);
            bmp.Freeze();
            return bmp;
        }

        static readonly int[] Tams = { 16, 20, 24, 32, 40, 48, 64, 256 };

        // Icono de la ventana (barra de tareas) con los colores del tema elegido
        public static BitmapFrame DelTema(Tema t)
        {
            using (MemoryStream ms = new MemoryStream(IcoDelTema(t)))
            {
                BitmapDecoder dec = BitmapDecoder.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                return dec.Frames[0];
            }
        }

        // archivo .ico completo con los colores del tema
        public static byte[] IcoDelTema(Tema t)
        {
            Color a1 = Tema.C(t.Acento), a2 = Tema.C(t.Igual);
            if (a1.A < 200) a1 = Color.FromRgb(a1.R, a1.G, a1.B);
            if (a2.A < 200) a2 = Color.FromRgb(a2.R, a2.G, a2.B);
            if (Math.Abs(Tema.Luz(a1) - Tema.Luz(a2)) < 0.02 && a1 == a2) a2 = Tema.Mezclar(a1, Colors.White, 0.35);
            Color f1 = Tema.C(t.Fondo1), f2 = Tema.C(t.Fondo2);
            f1.A = 255; f2.A = 255;
            // el icono siempre es oscuro: si el tema es claro se usa su color de acento sobre negro azulado
            if (Tema.Luz(f1) > 0.45) { f1 = Color.FromRgb(0x10, 0x12, 0x1C); f2 = Tema.Mezclar(f1, a1, 0.18); }
            if (Tema.Luz(a1) < 0.25) a1 = Tema.Mezclar(a1, Colors.White, 0.4);
            return Ico(a1, a2, f1, f2);
        }

        public static BitmapFrame Original()
        {
            using (MemoryStream ms = new MemoryStream(Ico(Cian, Violeta, Color.FromRgb(0x0B, 0x0E, 0x1A), Color.FromRgb(0x1A, 0x1F, 0x38))))
            {
                BitmapDecoder dec = BitmapDecoder.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                return dec.Frames[0];
            }
        }

        // ICO con imagenes PNG dentro (valido desde Windows Vista)
        static byte[] Ico(Color c1, Color c2, Color f1, Color f2)
        {
            byte[][] pngs = new byte[Tams.Length][];
            for (int i = 0; i < Tams.Length; i++)
            {
                PngBitmapEncoder enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(Imagen(Tams[i], c1, c2, f1, f2)));
                using (MemoryStream m = new MemoryStream()) { enc.Save(m); pngs[i] = m.ToArray(); }
            }
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter w = new BinaryWriter(ms))
            {
                w.Write((short)0); w.Write((short)1); w.Write((short)Tams.Length);
                int offset = 6 + 16 * Tams.Length;
                for (int i = 0; i < Tams.Length; i++)
                {
                    w.Write((byte)(Tams[i] >= 256 ? 0 : Tams[i]));
                    w.Write((byte)(Tams[i] >= 256 ? 0 : Tams[i]));
                    w.Write((byte)0); w.Write((byte)0);
                    w.Write((short)1); w.Write((short)32);
                    w.Write(pngs[i].Length); w.Write(offset);
                    offset += pngs[i].Length;
                }
                foreach (byte[] p in pngs) w.Write(p);
                w.Flush();
                return ms.ToArray();
            }
        }

        public static void GuardarIco(string ruta)
        {
            File.WriteAllBytes(ruta, Ico(Cian, Violeta, Color.FromRgb(0x0B, 0x0E, 0x1A), Color.FromRgb(0x1A, 0x1F, 0x38)));
        }

        public static void GuardarPng(BitmapSource b, string ruta)
        {
            PngBitmapEncoder enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(b));
            using (FileStream f = File.Create(ruta)) enc.Save(f);
        }
    }
}
