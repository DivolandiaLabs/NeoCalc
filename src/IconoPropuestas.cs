using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeoCalc
{
    // Propuestas de icono basadas en el logo actual que hacen referencia a la pantalla y las teclas.
    // NeoCalc.exe /propuestas hoja.png
    public static class IconoPropuestas
    {
        public static readonly string[] Nombres = {
            "Nuevo icono (4 con la barra del original)",
            "1 · Pantalla + tecla igual",
            "2 · Igual dentro de la pantalla",
            "3 · Rejilla 2×2",
            "4 · Calculadora de frente",
            "5 · Pantalla LCD",
            "6 · Igual sobre teclado de puntos" };

        static Color C1 { get { return Icono.Cian; } }
        static Color C2 { get { return Icono.Violeta; } }

        public static BitmapSource Imagen(int estilo, int s)
        {
            if (estilo == 0) return Icono.Imagen(s);
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                double S = s;
                bool chico = s <= 24;
                Base(dc, S, chico);
                switch (estilo)
                {
                    case 1: Estilo1(dc, S, chico); break;
                    case 2: Estilo2(dc, S, chico); break;
                    case 3: Estilo3(dc, S, chico); break;
                    case 4: Estilo4(dc, S, chico); break;
                    case 5: Estilo5(dc, S, chico); break;
                    case 6: Estilo6(dc, S, chico); break;
                }
            }
            RenderTargetBitmap bmp = new RenderTargetBitmap(s, s, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(dv);
            bmp.Freeze();
            return bmp;
        }

        // ---------------------------------------------------------------- piezas comunes

        static void Base(DrawingContext dc, double S, bool chico)
        {
            LinearGradientBrush acento = new LinearGradientBrush(C1, C2, new Point(0, 0), new Point(1, 1));
            double rad = S * 0.24;
            LinearGradientBrush fondo = new LinearGradientBrush(Color.FromRgb(0x0B, 0x0E, 0x1A), Color.FromRgb(0x1A, 0x1F, 0x38), new Point(0, 0), new Point(1, 1));
            double g = Math.Max(1, Math.Round(S * 0.035));
            dc.DrawRoundedRectangle(fondo, new Pen(acento, g), new Rect(g / 2, g / 2, S - g, S - g), rad, rad);
            if (!chico)
            {
                LinearGradientBrush reflejo = new LinearGradientBrush(Color.FromArgb(28, 255, 255, 255), Color.FromArgb(0, 255, 255, 255), new Point(0, 0), new Point(0, 0.5));
                dc.DrawRoundedRectangle(reflejo, null, new Rect(g, g, S - 2 * g, S * 0.5), rad * 0.9, rad * 0.9);
            }
        }

        // degradado horizontal en coordenadas absolutas (todas las piezas comparten el mismo color de izquierda a derecha)
        static Brush Grad(double S)
        {
            LinearGradientBrush b = new LinearGradientBrush(C1, C2, new Point(S * 0.2, 0), new Point(S * 0.8, 0));
            b.MappingMode = BrushMappingMode.Absolute;
            return b;
        }

        static Rect R(double S, double x0, double y0, double x1, double y1, bool chico)
        {
            Rect r = new Rect(S * x0, S * y0, S * (x1 - x0), S * (y1 - y0));
            if (chico) r = new Rect(Math.Round(r.X), Math.Round(r.Y), Math.Max(1, Math.Round(r.Width)), Math.Max(1, Math.Round(r.Height)));
            return r;
        }

        static void Halo(DrawingContext dc, Geometry geo, Color c, double S, bool chico)
        {
            if (chico || S < 32) return;
            for (int i = 3; i >= 1; i--)
            {
                byte a = (byte)(18 * (4 - i));
                Pen p = new Pen(new SolidColorBrush(Color.FromArgb(a, c.R, c.G, c.B)), S * 0.035 * i);
                p.LineJoin = PenLineJoin.Round;
                dc.DrawGeometry(new SolidColorBrush(Color.FromArgb(a, c.R, c.G, c.B)), p, geo);
            }
        }

        static void Led(DrawingContext dc, Point p, double r, double S, bool chico)
        {
            if (!chico)
                for (int i = 3; i >= 1; i--)
                {
                    byte a = (byte)(18 * (4 - i));
                    dc.DrawEllipse(new SolidColorBrush(Color.FromArgb(a, C2.R, C2.G, C2.B)), null, p, r + S * 0.03 * i, r + S * 0.03 * i);
                }
            dc.DrawEllipse(new SolidColorBrush(C2), null, p, r, r);
            if (S >= 32) dc.DrawEllipse(new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)), null, p, r * 0.4, r * 0.4);
        }

        // el "=" asimetrico del logo actual dentro de un rectangulo
        static void Igual(DrawingContext dc, Rect caja, Brush trazo, bool chico, bool conPunto, double S, Color? punto)
        {
            double t = chico ? Math.Max(1, Math.Round(caja.Height * 0.2)) : caja.Height * 0.2;
            double x0 = caja.Left, x1 = caja.Right, x1b = caja.Left + caja.Width * 0.6;
            double y1 = caja.Top + caja.Height * 0.25, y2 = caja.Top + caja.Height * 0.75;
            if (chico) { y1 = Math.Round(y1) + (t % 2 == 1 ? 0.5 : 0); y2 = Math.Round(y2) + (t % 2 == 1 ? 0.5 : 0); }
            Pen p = new Pen(trazo, t);
            p.StartLineCap = p.EndLineCap = chico ? PenLineCap.Flat : PenLineCap.Round;
            dc.DrawLine(p, new Point(x0 + (chico ? 0 : t / 2), y1), new Point(x1 - (chico ? 0 : t / 2), y1));
            dc.DrawLine(p, new Point(x0 + (chico ? 0 : t / 2), y2), new Point(x1b, y2));
            if (conPunto)
            {
                double r = t * 0.62;
                Point c = new Point(x1 - r, y2);
                if (punto.HasValue) dc.DrawEllipse(new SolidColorBrush(punto.Value), null, c, r, r);
                else Led(dc, c, r, S, chico);
            }
        }

        static Brush Tenue(byte a) { return new SolidColorBrush(Color.FromArgb(a, 0x9E, 0xB4, 0xE6)); }

        // ---------------------------------------------------------------- estilos

        // 1: pantalla con un numero (barra a la derecha) y debajo dos teclas + tecla igual luminosa
        static void Estilo1(DrawingContext dc, double S, bool chico)
        {
            double g = chico ? Math.Max(1, Math.Round(S * 0.06)) : S * 0.04;
            Rect pant = R(S, 0.22, 0.22, 0.78, 0.46, chico);
            dc.DrawRoundedRectangle(null, new Pen(Grad(S), g), Desplazar(pant, g / 2), S * 0.06, S * 0.06);
            // numero en la pantalla: barra corta alineada a la derecha
            Rect num = R(S, 0.48, 0.31, 0.68, 0.37, chico);
            Halo(dc, new RectangleGeometry(num, num.Height / 2, num.Height / 2), C1, S, chico);
            dc.DrawRoundedRectangle(new SolidColorBrush(C1), null, num, chico ? 0 : num.Height / 2, chico ? 0 : num.Height / 2);
            // teclas
            Rect k1 = R(S, 0.22, 0.56, 0.38, 0.78, chico), k2 = R(S, 0.42, 0.56, 0.58, 0.78, chico), k3 = R(S, 0.62, 0.56, 0.78, 0.78, chico);
            double kr = S * 0.05;
            dc.DrawRoundedRectangle(Tenue(60), null, k1, kr, kr);
            dc.DrawRoundedRectangle(Tenue(60), null, k2, kr, kr);
            Halo(dc, new RectangleGeometry(k3, kr, kr), C2, S, chico);
            dc.DrawRoundedRectangle(Grad(S), null, k3, kr, kr);
            if (S >= 32) Igual(dc, Inset(k3, 0.28, 0.33), new SolidColorBrush(Color.FromRgb(0x0B, 0x0E, 0x1A)), chico, false, S, null);
        }

        // 2: pantalla llena con el degradado y el "=" del logo en oscuro; debajo una fila de teclas
        static void Estilo2(DrawingContext dc, double S, bool chico)
        {
            Rect pant = R(S, 0.2, 0.2, 0.8, 0.54, chico);
            double r = S * 0.08;
            Halo(dc, new RectangleGeometry(pant, r, r), C1, S, chico);
            LinearGradientBrush f = new LinearGradientBrush(C1, C2, new Point(0, 0), new Point(1, 1));
            dc.DrawRoundedRectangle(f, null, pant, r, r);
            Igual(dc, Inset(pant, 0.2, 0.26), new SolidColorBrush(Color.FromRgb(0x0B, 0x0E, 0x1A)), chico, true, S, Color.FromRgb(0x0B, 0x0E, 0x1A));
            for (int i = 0; i < 4; i++)
            {
                double x = 0.2 + i * 0.16;
                Rect k = R(S, x, 0.64, x + 0.12, 0.78, chico);
                Brush b = i == 3 ? (Brush)new SolidColorBrush(C2) : Tenue(70);
                if (i == 3) Halo(dc, new RectangleGeometry(k, S * 0.03, S * 0.03), C2, S, chico);
                dc.DrawRoundedRectangle(b, null, k, S * 0.03, S * 0.03);
            }
        }

        // 3: linea de pantalla arriba y 4 teclas grandes; la de abajo a la derecha es el igual luminoso
        static void Estilo3(DrawingContext dc, double S, bool chico)
        {
            double t = chico ? Math.Max(1, Math.Round(S * 0.07)) : S * 0.055;
            Pen linea = new Pen(Grad(S), t) { StartLineCap = chico ? PenLineCap.Flat : PenLineCap.Round, EndLineCap = chico ? PenLineCap.Flat : PenLineCap.Round };
            double y = chico ? Math.Round(S * 0.27) + (t % 2 == 1 ? 0.5 : 0) : S * 0.27;
            dc.DrawLine(linea, new Point(S * 0.5, y), new Point(S * 0.76, y));
            double g = chico ? 1 : S * 0.03;
            Rect[] k = { R(S, 0.24, 0.39, 0.48, 0.58, chico), R(S, 0.52, 0.39, 0.76, 0.58, chico), R(S, 0.24, 0.62, 0.48, 0.81, chico), R(S, 0.52, 0.62, 0.76, 0.81, chico) };
            double kr = S * 0.06;
            for (int i = 0; i < 3; i++) dc.DrawRoundedRectangle(null, new Pen(Tenue(110), g), Desplazar(k[i], g / 2), kr, kr);
            Halo(dc, new RectangleGeometry(k[3], kr, kr), C2, S, chico);
            dc.DrawRoundedRectangle(new LinearGradientBrush(C1, C2, new Point(0, 0), new Point(1, 1)), null, k[3], kr, kr);
            Igual(dc, Inset(k[3], 0.26, 0.3), new SolidColorBrush(Color.FromRgb(0x0B, 0x0E, 0x1A)), chico, false, S, null);
        }

        // 4: cuerpo de calculadora de frente: pantalla con el "=" y rejilla 3x3 de puntos, el ultimo es el LED
        static void Estilo4(DrawingContext dc, double S, bool chico)
        {
            Rect pant = R(S, 0.25, 0.2, 0.75, 0.4, chico);
            double r = S * 0.05;
            dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromArgb(60, C1.R, C1.G, C1.B)), new Pen(Grad(S), chico ? 1 : S * 0.025), pant, r, r);
            if (S >= 24) Igual(dc, Inset(pant, 0.22, 0.28), Grad(S), chico, false, S, null);
            for (int f = 0; f < 3; f++)
                for (int c = 0; c < 3; c++)
                {
                    Point p = new Point(S * (0.31 + c * 0.19), S * (0.53 + f * 0.13));
                    double rr = S * 0.045;
                    if (chico) { p = new Point(Math.Round(p.X), Math.Round(p.Y)); rr = Math.Max(1, rr); }
                    if (f == 2 && c == 2) Led(dc, p, rr * 1.15, S, chico);
                    else dc.DrawEllipse(Tenue(150), null, p, rr, rr);
                }
        }

        // 5: pantalla LCD grande con el logo actual brillando; debajo una barra de teclas tipo pastilla
        static void Estilo5(DrawingContext dc, double S, bool chico)
        {
            Rect pant = R(S, 0.17, 0.19, 0.83, 0.6, chico);
            double r = S * 0.07;
            dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromArgb(255, 0x07, 0x09, 0x12)), new Pen(new SolidColorBrush(Color.FromArgb(90, C1.R, C1.G, C1.B)), chico ? 1 : S * 0.02), pant, r, r);
            Rect caja = Inset(pant, 0.16, 0.26);
            if (!chico)
            {
                double t = caja.Height * 0.2;
                for (int i = 3; i >= 1; i--)
                {
                    byte a = (byte)(18 * (4 - i));
                    Pen h = new Pen(new SolidColorBrush(Color.FromArgb(a, C1.R, C1.G, C1.B)), t + S * 0.035 * i) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
                    dc.DrawLine(h, new Point(caja.Left + t / 2, caja.Top + caja.Height * 0.25), new Point(caja.Right - t / 2, caja.Top + caja.Height * 0.25));
                    dc.DrawLine(h, new Point(caja.Left + t / 2, caja.Top + caja.Height * 0.75), new Point(caja.Left + caja.Width * 0.6, caja.Top + caja.Height * 0.75));
                }
            }
            Igual(dc, caja, Grad(S), chico, true, S, null);
            Rect barra = R(S, 0.17, 0.69, 0.83, 0.8, chico);
            double br = chico ? 0 : barra.Height / 2;
            dc.DrawRoundedRectangle(Tenue(55), null, barra, br, br);
            Rect fin = R(S, 0.6, 0.69, 0.83, 0.8, chico);
            Halo(dc, new RectangleGeometry(fin, br, br), C2, S, chico);
            dc.DrawRoundedRectangle(new SolidColorBrush(C2), null, fin, br, br);
        }

        // 6: el "=" del logo como contenido de la pantalla (subrayado = borde de la pantalla) sobre un teclado de puntos 4x2
        static void Estilo6(DrawingContext dc, double S, bool chico)
        {
            Rect caja = R(S, 0.27, 0.22, 0.73, 0.42, chico);
            if (!chico)
            {
                double t = caja.Height * 0.2;
                for (int i = 3; i >= 1; i--)
                {
                    byte a = (byte)(18 * (4 - i));
                    Pen h = new Pen(new SolidColorBrush(Color.FromArgb(a, C1.R, C1.G, C1.B)), t + S * 0.035 * i) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
                    dc.DrawLine(h, new Point(caja.Left + t / 2, caja.Top + caja.Height * 0.25), new Point(caja.Right - t / 2, caja.Top + caja.Height * 0.25));
                    dc.DrawLine(h, new Point(caja.Left + t / 2, caja.Top + caja.Height * 0.75), new Point(caja.Left + caja.Width * 0.6, caja.Top + caja.Height * 0.75));
                }
            }
            Igual(dc, caja, Grad(S), chico, true, S, null);
            double yl = chico ? Math.Round(S * 0.52) + 0.5 : S * 0.52;
            dc.DrawLine(new Pen(Tenue(90), chico ? 1 : S * 0.015), new Point(S * 0.22, yl), new Point(S * 0.78, yl));
            for (int f = 0; f < 2; f++)
                for (int c = 0; c < 4; c++)
                {
                    Point p = new Point(S * (0.29 + c * 0.14), S * (0.63 + f * 0.13));
                    double rr = S * 0.035;
                    if (chico) { p = new Point(Math.Round(p.X), Math.Round(p.Y)); rr = Math.Max(1, rr); }
                    dc.DrawEllipse(Tenue(140), null, p, rr, rr);
                }
        }

        static Rect Desplazar(Rect r, double d) { return new Rect(r.X + d, r.Y + d, Math.Max(0, r.Width - 2 * d), Math.Max(0, r.Height - 2 * d)); }
        static Rect Inset(Rect r, double fx, double fy) { return new Rect(r.X + r.Width * fx, r.Y + r.Height * fy, r.Width * (1 - 2 * fx), r.Height * (1 - 2 * fy)); }

        // Hoja comparativa: cada propuesta a 256/64/48/32/24/16 sobre fondo oscuro y claro
        public static void Hoja(string png)
        {
            int[] tams = { 64, 48, 32, 24, 16 };
            double fila = 300, W = 900, H = Nombres.Length * fila;
            DrawingVisual dv = new DrawingVisual();
            Typeface tf = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal);
            using (DrawingContext dc = dv.RenderOpen())
            {
                dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(0x18, 0x19, 0x1F)), null, new Rect(0, 0, W, H));
                for (int e = 0; e < Nombres.Length; e++)
                {
                    double y = e * fila;
                    if (e > 0) dc.DrawLine(new Pen(new SolidColorBrush(Color.FromRgb(0x33, 0x35, 0x40)), 1), new Point(0, y), new Point(W, y));
                    dc.DrawText(new FormattedText(Nombres[e], System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, tf, 20, Brushes.White, 1.0), new Point(20, y + 12));
                    dc.DrawImage(Imagen(e, 256), new Rect(20, y + 40, 240, 240));
                    // barra de tareas oscura y clara con los tamanos reales
                    dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20)), null, new Rect(290, y + 60, 590, 100));
                    dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE)), null, new Rect(290, y + 170, 590, 100));
                    double x = 310;
                    foreach (int s in tams)
                    {
                        BitmapSource b = Imagen(e, s);
                        dc.DrawImage(b, new Rect(x, y + 110 - s / 2.0, s, s));
                        dc.DrawImage(b, new Rect(x, y + 220 - s / 2.0, s, s));
                        dc.DrawText(new FormattedText(s + " px", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, tf, 11, Brushes.Gray, 1.0), new Point(x, y + 146));
                        x += s + 70;
                    }
                }
            }
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)W, (int)H, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(dv);
            Icono.GuardarPng(bmp, png);
        }
    }
}
