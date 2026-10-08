using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace NeoCalc
{
    // Galeria de temas + editor de personalizacion (los cambios se ven al momento en la calculadora)
    public class VentanaTemas : Window
    {
        readonly VentanaCalc calc;
        WrapPanel galIncluidos, galPropios;
        StackPanel editor;
        readonly Dictionary<string, Border> tarjetas = new Dictionary<string, Border>();
        DispatcherTimer aplicarLuego;
        readonly Random rnd = new Random();
        bool cargando;

        static readonly Color CFondo = Color.FromRgb(0x1B, 0x1C, 0x22);
        static readonly Color CPanel = Color.FromRgb(0x24, 0x25, 0x2D);
        static readonly Color CCampo = Color.FromRgb(0x2E, 0x30, 0x3A);
        static readonly Color CLinea = Color.FromRgb(0x3A, 0x3C, 0x48);
        static readonly Color CTexto = Color.FromRgb(0xEC, 0xEE, 0xF4);
        static readonly Color CSuave = Color.FromRgb(0x9A, 0x9F, 0xB0);
        static readonly Color CAcento = Color.FromRgb(0x22, 0xE3, 0xFF);

        Tema Actual { get { return calc.Tema; } }

        public VentanaTemas(VentanaCalc calc)
        {
            this.calc = calc;
            Title = L.T("NeoCalc · Temas y personalización");
            Width = 1040; Height = 720; MinWidth = 760; MinHeight = 480;
            Background = new SolidColorBrush(CFondo);
            Foreground = new SolidColorBrush(CTexto);
            FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI");
            FontSize = 13;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            try { Icon = Icono.Original(); } catch { }
            SourceInitialized += delegate
            {
                int on = 1;
                IntPtr h = new WindowInteropHelper(this).Handle;
                DwmSetWindowAttribute(h, 20, ref on, 4);
            };
            PreviewKeyDown += delegate (object s, KeyEventArgs e) { if (e.Key == Key.Escape) Close(); };

            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(390) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            Content = g;

            // --- galeria
            Grid izq = new Grid { Background = new SolidColorBrush(CPanel) };
            izq.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            izq.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            ScrollViewer sv = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            StackPanel gal = new StackPanel { Margin = new Thickness(16, 12, 8, 12) };
            gal.Children.Add(Titulo(L.T("Temas")));
            gal.Children.Add(Nota(L.T("Elige uno para aplicarlo. Para personalizar, cambia lo que quieras a la derecha.")));
            gal.Children.Add(Subtitulo(L.T("Mis temas")));
            galPropios = new WrapPanel();
            gal.Children.Add(galPropios);
            gal.Children.Add(Subtitulo(L.T("Incluidos")));
            galIncluidos = new WrapPanel();
            gal.Children.Add(galIncluidos);
            sv.Content = gal;
            izq.Children.Add(sv);
            WrapPanel acc = new WrapPanel { Margin = new Thickness(12, 8, 12, 12) };
            acc.Children.Add(Boton("", L.T("Aleatorio"), delegate { NuevoAleatorio(); }));
            acc.Children.Add(Boton("", L.T("Importar…"), delegate { Importar(); }));
            Grid.SetRow(acc, 1);
            izq.Children.Add(acc);
            g.Children.Add(izq);

            // --- editor
            Grid der = new Grid();
            der.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            der.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            ScrollViewer sve = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            editor = new StackPanel { Margin = new Thickness(24, 12, 24, 16) };
            sve.Content = editor;
            der.Children.Add(sve);
            WrapPanel pie = new WrapPanel { Margin = new Thickness(20, 8, 20, 12), HorizontalAlignment = HorizontalAlignment.Right };
            pie.Children.Add(Boton("", L.T("Duplicar"), delegate { Duplicar(); }));
            pie.Children.Add(Boton("", L.T("Exportar…"), delegate { Exportar(); }));
            pie.Children.Add(Boton("", L.T("Eliminar"), delegate { Eliminar(); }));
            Grid.SetRow(pie, 1);
            der.Children.Add(pie);
            Grid.SetColumn(der, 1);
            g.Children.Add(der);

            LlenarGaleria();
            LlenarEditor();
        }

        // los temas incluidos guardan el nombre en espanol y se traducen al mostrarlos
        public static string NombreDe(Tema t) { return t.Incluido ? L.T(t.Nombre) : t.Nombre; }

        [DllImport("dwmapi.dll")] static extern int DwmSetWindowAttribute(IntPtr h, int a, ref int v, int s);

        // ---------------------------------------------------------------- galeria

        void LlenarGaleria()
        {
            tarjetas.Clear();
            galIncluidos.Children.Clear();
            galPropios.Children.Clear();
            foreach (Tema t in Tema.Incluidos()) galIncluidos.Children.Add(Tarjeta(t));
            foreach (Tema t in calc.Cfg.Propios) galPropios.Children.Add(Tarjeta(t));
            if (calc.Cfg.Propios.Count == 0)
                galPropios.Children.Add(new TextBlock { Text = L.T("Aún no tienes temas propios: edita uno o pulsa «Aleatorio»."), Foreground = new SolidColorBrush(CSuave), TextWrapping = TextWrapping.Wrap, Width = 340, Margin = new Thickness(2, 0, 0, 8) });
            MarcarSeleccion();
        }

        Border Tarjeta(Tema t)
        {
            Border host = new Border { Width = 106, Margin = new Thickness(0, 0, 6, 10), Padding = new Thickness(4), CornerRadius = new CornerRadius(10), Cursor = Cursors.Hand, BorderThickness = new Thickness(2) };
            host.Tag = t.Id;
            host.Child = ContenidoTarjeta(t);
            Tema tt = t;
            Clic.En(host, delegate
            {
                Tema elegido = tt.Incluido ? calc.Cfg.Buscar(tt.Id) : tt;
                calc.AplicarTema(elegido);
                MarcarSeleccion();
                LlenarEditor();
            });
            host.MouseEnter += delegate { if (host.Tag as string != Actual.Id) host.BorderBrush = new SolidColorBrush(CLinea); };
            host.MouseLeave += delegate { MarcarSeleccion(); };
            tarjetas[t.Id] = host;
            return host;
        }

        // miniatura de la calculadora con los colores del tema
        static FrameworkElement ContenidoTarjeta(Tema t)
        {
            StackPanel sp = new StackPanel();
            Grid mini = new Grid { Height = 138 };
            Border fondo = new Border { CornerRadius = new CornerRadius(Math.Min(12, t.RadioVentana * 0.6)) };
            double ang = t.AnguloFondo * Math.PI / 180;
            fondo.Background = new LinearGradientBrush(Opaco(Tema.C(t.Fondo1)), Opaco(Tema.C(t.Fondo2)),
                new Point(0.5 - Math.Sin(ang) * 0.5, 0.5 - Math.Cos(ang) * 0.5), new Point(0.5 + Math.Sin(ang) * 0.5, 0.5 + Math.Cos(ang) * 0.5));
            fondo.BorderBrush = new SolidColorBrush(Color.FromArgb(40, 128, 128, 128));
            fondo.BorderThickness = new Thickness(1);
            mini.Children.Add(fondo);
            Grid in_ = new Grid { Margin = new Thickness(6) };
            in_.RowDefinitions.Add(new RowDefinition { Height = new GridLength(34) });
            in_.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            Border pant = new Border { Margin = new Thickness(0, 0, 0, 3), CornerRadius = new CornerRadius(3) };
            Color cp = Tema.C(t.Pantalla);
            if (cp.A > 0) pant.Background = new SolidColorBrush(cp);
            TextBlock num = new TextBlock { Text = "1.234", FontSize = 18, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Bottom, Margin = new Thickness(0, 0, 3, 0) };
            num.Foreground = Tema.B(t.PantallaTexto);
            try { num.FontFamily = new FontFamily(t.Fuente + ", Segoe UI"); } catch { }
            num.FontWeight = Tema.Peso(t.PesoPantalla);
            if (t.Brillo > 0) num.Effect = new DropShadowEffect { Color = Tema.C(t.PantallaTexto), BlurRadius = 6, ShadowDepth = 0, Opacity = Math.Min(1, t.Brillo) };
            pant.Child = num;
            in_.Children.Add(pant);
            UniformGrid teclas = new UniformGrid { Columns = 4, Rows = 5 };
            string cats = "ffffnnnonnnonnnonnn=";
            for (int i = 0; i < 20; i++)
            {
                char c = cats[i];
                string col = c == 'n' ? t.Num : (c == 'o' ? t.Op : (c == '=' ? t.Igual : t.Fun));
                Border k = new Border { Margin = new Thickness(Math.Max(0.5, t.Espacio / 4)) };
                double r = t.Radio > 100 ? 8 : t.Radio / 2.5;
                k.CornerRadius = new CornerRadius(r);
                Color cc = Tema.C(col);
                k.Background = new SolidColorBrush(cc);
                Color cb = Tema.C(t.Borde);
                if (cb.A > 0 && t.GrosorBorde > 0) { k.BorderBrush = new SolidColorBrush(cb); k.BorderThickness = new Thickness(0.7); }
                teclas.Children.Add(k);
            }
            Grid.SetRow(teclas, 1);
            in_.Children.Add(teclas);
            mini.Children.Add(in_);
            sp.Children.Add(mini);
            sp.Children.Add(new TextBlock { Text = NombreDe(t), Foreground = new SolidColorBrush(CTexto), FontSize = 12, Margin = new Thickness(2, 5, 2, 1), TextTrimming = TextTrimming.CharacterEllipsis, ToolTip = NombreDe(t) });
            return sp;
        }

        static Color Opaco(Color c) { return c.A < 255 ? Color.FromArgb((byte)Math.Max((int)c.A, 200), c.R, c.G, c.B) : c; }

        void MarcarSeleccion()
        {
            foreach (KeyValuePair<string, Border> kv in tarjetas)
            {
                bool sel = kv.Key == Actual.Id;
                kv.Value.BorderBrush = sel ? new SolidColorBrush(CAcento) : Brushes.Transparent;
                kv.Value.Background = sel ? new SolidColorBrush(Color.FromArgb(30, CAcento.R, CAcento.G, CAcento.B)) : Brushes.Transparent;
            }
        }

        void RefrescarTarjetaActual()
        {
            Border b;
            if (tarjetas.TryGetValue(Actual.Id, out b)) b.Child = ContenidoTarjeta(Actual);
        }

        // ---------------------------------------------------------------- editor

        void LlenarEditor()
        {
            cargando = true;
            editor.Children.Clear();
            Tema t = Actual;
            editor.Children.Add(Titulo(L.T("Personalizar")));
            TextBox nombre = Campo(NombreDe(t));
            nombre.FontSize = 16; nombre.Margin = new Thickness(0, 4, 0, 4); nombre.MaxWidth = 420; nombre.HorizontalAlignment = HorizontalAlignment.Left; nombre.Width = 420;
            nombre.TextChanged += delegate
            {
                if (cargando) return;
                Editable(); Actual.Nombre = nombre.Text;
                Border b;
                if (tarjetas.TryGetValue(Actual.Id, out b)) b.Child = ContenidoTarjeta(Actual);
                calc.GuardarLuego();
            };
            editor.Children.Add(nombre);
            if (t.Incluido)
                { TextBlock nota = Nota(L.T("Es un tema incluido: al cambiar cualquier cosa se crea una copia en «Mis temas» y el original queda intacto.")); nota.Tag = "nota-incluido"; editor.Children.Add(nota); }

            Seccion(L.T("Fondo de la ventana"));
            FilaColor(L.T("Color de arriba"), "Fondo1");
            FilaColor(L.T("Color de abajo"), "Fondo2");
            FilaDeslizador(L.T("Ángulo del degradado"), "AnguloFondo", 0, 360, 15, "0'°'");
            FilaImagen();
            FilaDeslizador(L.T("Opacidad de la imagen"), "OpacidadImagen", 0.05, 1, 0.05, "0%");
            FilaDeslizador(L.T("Opacidad de la ventana"), "Opacidad", 0.3, 1, 0.05, "0%");
            FilaDeslizador(L.T("Esquinas de la ventana"), "RadioVentana", 0, 32, 1, "0' px'");

            Seccion(L.T("Pantalla y textos"));
            FilaColor(L.T("Fondo de la pantalla"), "Pantalla");
            FilaColor(L.T("Números de la pantalla"), "PantallaTexto");
            FilaColor(L.T("Texto (título, menús)"), "Texto");
            FilaColor(L.T("Texto secundario"), "TextoSuave");
            FilaFuente(L.T("Fuente de la pantalla"), "Fuente");
            FilaPeso(L.T("Grosor de la fuente"), "PesoPantalla");
            FilaCasilla(L.T("Marco tipo LCD (calculadora clásica)"), "LCD");

            Seccion(L.T("Teclas"));
            FilaColor(L.T("Números"), "Num");
            FilaColor(L.T("Texto de los números"), "NumTexto");
            FilaColor(L.T("Funciones (%, CE, x²…)"), "Fun");
            FilaColor(L.T("Texto de las funciones"), "FunTexto");
            FilaColor(L.T("Operadores (+ − × ÷)"), "Op");
            FilaColor(L.T("Texto de los operadores"), "OpTexto");
            FilaColor(L.T("Tecla igual"), "Igual");
            FilaColor(L.T("Texto de la tecla igual"), "IgualTexto");
            FilaFuente(L.T("Fuente de las teclas"), "FuenteTeclas");
            FilaPeso(L.T("Grosor de la fuente"), "PesoTeclas");
            FilaDeslizador(L.T("Tamaño del texto"), "TamTeclas", 10, 28, 1, "0' pt'");

            Seccion(L.T("Forma y efectos"));
            FilaDeslizador(L.T("Redondeo de las teclas (máx = redondas)"), "Radio", 0, 40, 1, "0' px'");
            FilaDeslizador(L.T("Separación entre teclas"), "Espacio", 0, 16, 1, "0' px'");
            FilaColor(L.T("Borde de las teclas"), "Borde");
            FilaDeslizador(L.T("Grosor del borde"), "GrosorBorde", 0, 4, 0.5, "0.#' px'");
            FilaDeslizador(L.T("Brillo neón"), "Brillo", 0, 1, 0.05, "0%");
            FilaColor(L.T("Color de acento"), "Acento");
            cargando = false;
        }

        void Seccion(string t)
        {
            TextBlock s = Subtitulo(t);
            s.Margin = new Thickness(0, 22, 0, 8);
            editor.Children.Add(s);
        }

        Grid Fila(string etiqueta, UIElement control)
        {
            Grid g = new Grid { Margin = new Thickness(0, 3, 0, 3), MinHeight = 32 };
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.Children.Add(new TextBlock { Text = etiqueta, VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(CTexto), TextWrapping = TextWrapping.Wrap });
            Grid.SetColumn(control, 1);
            g.Children.Add(control);
            editor.Children.Add(g);
            return g;
        }

        static FieldInfo F(string campo) { return typeof(Tema).GetField(campo); }

        void Poner(string campo, object valor)
        {
            if (cargando) return;
            Editable();
            F(campo).SetValue(Actual, valor);
            AplicarLuego();
        }

        // Si el tema es incluido, crea una copia propia antes de cambiarlo
        void Editable()
        {
            if (!Actual.Incluido) return;
            Tema c = Actual.Copia();
            c.Incluido = false;
            c.Id = "mio-" + Guid.NewGuid().ToString("N").Substring(0, 8);
            c.Nombre = NombreDe(Actual) + " (" + L.T("mío") + ")";
            calc.Cfg.Propios.Add(c);
            calc.AplicarTema(c);
            LlenarGaleria();
            // el nombre del cuadro de texto se actualiza sin reconstruir todo el editor
            foreach (UIElement e in editor.Children)
            {
                TextBox tb = e as TextBox;
                if (tb != null) { bool k = cargando; cargando = true; tb.Text = c.Nombre; cargando = k; break; }
            }
            // quita la nota de "tema incluido"
            for (int i = 0; i < editor.Children.Count; i++)
            {
                TextBlock n = editor.Children[i] as TextBlock;
                if (n != null && (n.Tag as string) == "nota-incluido") { editor.Children.RemoveAt(i); break; }
            }
        }

        void AplicarLuego()
        {
            if (aplicarLuego == null)
            {
                aplicarLuego = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(60) };
                aplicarLuego.Tick += delegate
                {
                    aplicarLuego.Stop();
                    calc.AplicarTema(Actual);
                    RefrescarTarjetaActual();
                };
            }
            aplicarLuego.Stop(); aplicarLuego.Start();
        }

        // ---- color
        void FilaColor(string etiqueta, string campo)
        {
            StackPanel sp = new StackPanel { Orientation = Orientation.Horizontal };
            Border muestra = new Border { Width = 46, Height = 26, CornerRadius = new CornerRadius(5), BorderBrush = new SolidColorBrush(CLinea), BorderThickness = new Thickness(1), Cursor = Cursors.Hand, Background = Ajedrez() };
            Border color = new Border { CornerRadius = new CornerRadius(4) };
            muestra.Child = color;
            TextBox hex = Campo((string)F(campo).GetValue(Actual));
            hex.Width = 110; hex.Margin = new Thickness(10, 0, 0, 0); hex.FontFamily = new FontFamily("Cascadia Mono, Consolas");
            Action<string> mostrar = delegate (string h) { color.Background = Tema.B(h); };
            mostrar((string)F(campo).GetValue(Actual));
            bool desdeSelector = false;
            hex.TextChanged += delegate
            {
                if (cargando || desdeSelector) return;
                string h = hex.Text.Trim();
                if (!h.StartsWith("#")) h = "#" + h;
                try
                {
                    Color c = (Color)ColorConverter.ConvertFromString(h);
                    if (h.Length != 7 && h.Length != 9) return;
                    mostrar(Tema.Hex(c));
                    Poner(campo, Tema.Hex(c));
                }
                catch { }
            };
            Clic.En(muestra, delegate
            {
                SelectorColor.Abrir(muestra, Tema.C((string)F(campo).GetValue(Actual)), Paleta(), delegate (Color c)
                {
                    string h = Tema.Hex(c);
                    desdeSelector = true; hex.Text = h; desdeSelector = false;
                    mostrar(h);
                    Poner(campo, h);
                });
            });
            sp.Children.Add(muestra);
            sp.Children.Add(hex);
            Border transp = Boton(null, L.T("Transparente"), delegate { hex.Text = "#00000000"; });
            transp.Margin = new Thickness(8, 0, 0, 0);
            transp.Padding = new Thickness(8, 3, 8, 3);
            ((TextBlock)((StackPanel)transp.Child).Children[1]).FontSize = 11;
            if (campo == "Pantalla" || campo == "Borde" || campo == "Num" || campo == "Fun" || campo == "Op") sp.Children.Add(transp);
            Fila(etiqueta, sp);
        }

        List<Color> Paleta()
        {
            List<Color> l = new List<Color>();
            Tema t = Actual;
            foreach (string h in new[] { t.Fondo1, t.Fondo2, t.Num, t.Fun, t.Op, t.Igual, t.Acento, t.Texto })
            {
                Color c = Tema.C(h);
                if (!l.Contains(c)) l.Add(c);
            }
            return l;
        }

        static Brush Ajedrez()
        {
            DrawingGroup d = new DrawingGroup();
            d.Children.Add(new GeometryDrawing(Brushes.White, null, new RectangleGeometry(new Rect(0, 0, 8, 8))));
            d.Children.Add(new GeometryDrawing(new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC)), null, new RectangleGeometry(new Rect(0, 0, 4, 4))));
            d.Children.Add(new GeometryDrawing(new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC)), null, new RectangleGeometry(new Rect(4, 4, 4, 4))));
            DrawingBrush b = new DrawingBrush(d) { TileMode = TileMode.Tile, Viewport = new Rect(0, 0, 8, 8), ViewportUnits = BrushMappingMode.Absolute };
            return b;
        }

        // ---- deslizador
        void FilaDeslizador(string etiqueta, string campo, double min, double max, double paso, string formato)
        {
            Grid g = new Grid { MaxWidth = 330, HorizontalAlignment = HorizontalAlignment.Left, Width = 330 };
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
            double v = (double)F(campo).GetValue(Actual);
            bool redondas = campo == "Radio" && v > 100;
            if (redondas) v = max;
            if (campo == "Opacidad" && v <= 0) v = 1;
            Slider s = new Slider { Minimum = min, Maximum = max, Value = Math.Max(min, Math.Min(max, v)), SmallChange = paso, LargeChange = paso * 4, IsSnapToTickEnabled = true, TickFrequency = paso, VerticalAlignment = VerticalAlignment.Center };
            TextBlock val = new TextBlock { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Right, Foreground = new SolidColorBrush(CSuave) };
            Action mostrar = delegate
            {
                if (campo == "Radio" && s.Value >= max) val.Text = L.T("redondas");
                else val.Text = s.Value.ToString(formato, CultureInfo.CurrentCulture);
            };
            mostrar();
            s.ValueChanged += delegate
            {
                mostrar();
                double nv = s.Value;
                if (campo == "Radio" && nv >= max) nv = 999;
                Poner(campo, nv);
            };
            g.Children.Add(s);
            Grid.SetColumn(val, 1);
            g.Children.Add(val);
            Fila(etiqueta, g);
        }

        // ---- fuente
        static List<string> fuentesSistema;
        void FilaFuente(string etiqueta, string campo)
        {
            if (fuentesSistema == null)
                fuentesSistema = Fonts.SystemFontFamilies.Select(f => f.Source).Distinct().OrderBy(n => n).ToList();
            ComboBox cb = new ComboBox { IsEditable = true, Width = 280, HorizontalAlignment = HorizontalAlignment.Left, IsTextSearchEnabled = true };
            cb.ItemsSource = fuentesSistema;
            cb.Text = (string)F(campo).GetValue(Actual);
            cb.SelectionChanged += delegate { if (cb.SelectedItem != null) Poner(campo, (string)cb.SelectedItem); };
            cb.LostFocus += delegate { if (!string.IsNullOrWhiteSpace(cb.Text)) Poner(campo, cb.Text.Trim()); };
            cb.KeyDown += delegate (object s, KeyEventArgs e) { if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(cb.Text)) Poner(campo, cb.Text.Trim()); };
            Fila(etiqueta, cb);
        }

        void FilaPeso(string etiqueta, string campo)
        {
            ComboBox cb = new ComboBox { Width = 160, HorizontalAlignment = HorizontalAlignment.Left };
            string[] nombres = { "Muy fina", "Fina", "Semifina", "Normal", "Media", "Seminegrita", "Negrita", "Muy negrita" };
            for (int i = 0; i < nombres.Length; i++) cb.Items.Add(L.T(nombres[i]));
            string actual = (string)F(campo).GetValue(Actual) ?? "Normal";
            int idx = Array.FindIndex(Tema.Pesos, p => string.Equals(p, actual, StringComparison.OrdinalIgnoreCase));
            cb.SelectedIndex = idx < 0 ? 3 : idx;
            cb.SelectionChanged += delegate { Poner(campo, Tema.Pesos[cb.SelectedIndex]); };
            Fila(etiqueta, cb);
        }

        void FilaCasilla(string etiqueta, string campo)
        {
            CheckBox c = new CheckBox { IsChecked = (bool)F(campo).GetValue(Actual), VerticalAlignment = VerticalAlignment.Center };
            c.Checked += delegate { Poner(campo, true); };
            c.Unchecked += delegate { Poner(campo, false); };
            Fila(etiqueta, c);
        }

        void FilaImagen()
        {
            StackPanel sp = new StackPanel { Orientation = Orientation.Horizontal };
            TextBlock nombre = new TextBlock { VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(CSuave), Margin = new Thickness(10, 0, 0, 0), MaxWidth = 220, TextTrimming = TextTrimming.CharacterEllipsis };
            nombre.Text = string.IsNullOrEmpty(Actual.Imagen) ? L.T("ninguna") : System.IO.Path.GetFileName(Actual.Imagen);
            sp.Children.Add(Boton("", L.T("Elegir…"), delegate
            {
                Microsoft.Win32.OpenFileDialog d = new Microsoft.Win32.OpenFileDialog { Filter = L.T("Imágenes") + "|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.webp|" + L.T("Todos") + "|*.*", Title = L.T("Imagen de fondo") };
                if (d.ShowDialog(this) != true) return;
                // se copia a la carpeta del programa para que el tema no dependa del archivo original
                string destino = d.FileName;
                try
                {
                    string dir = System.IO.Path.Combine(Config.Carpeta, "imagenes");
                    Directory.CreateDirectory(dir);
                    destino = System.IO.Path.Combine(dir, Guid.NewGuid().ToString("N").Substring(0, 8) + System.IO.Path.GetExtension(d.FileName));
                    File.Copy(d.FileName, destino);
                }
                catch { destino = d.FileName; }
                nombre.Text = System.IO.Path.GetFileName(d.FileName);
                Poner("Imagen", destino);
            }));
            Border quitar = Boton("", L.T("Quitar"), delegate { nombre.Text = L.T("ninguna"); Poner("Imagen", null); });
            quitar.Margin = new Thickness(6, 0, 0, 0);
            sp.Children.Add(quitar);
            sp.Children.Add(nombre);
            Fila(L.T("Imagen de fondo"), sp);
        }

        // ---------------------------------------------------------------- acciones

        void NuevoAleatorio()
        {
            Tema t = Tema.Aleatorio(rnd);
            int n = calc.Cfg.Propios.Count(x => x.Nombre.StartsWith(L.T("Aleatorio"))) + 1;
            t.Nombre = L.T("Aleatorio") + " " + n;
            calc.Cfg.Propios.Add(t);
            calc.AplicarTema(t);
            LlenarGaleria();
            LlenarEditor();
        }

        void Duplicar()
        {
            Tema c = Actual.Copia();
            c.Incluido = false;
            c.Id = "mio-" + Guid.NewGuid().ToString("N").Substring(0, 8);
            c.Nombre = NombreDe(Actual) + " (" + L.T("copia") + ")";
            calc.Cfg.Propios.Add(c);
            calc.AplicarTema(c);
            LlenarGaleria();
            LlenarEditor();
        }

        void Eliminar()
        {
            if (Actual.Incluido) { MessageBox.Show(this, L.T("Los temas incluidos no se pueden eliminar."), "NeoCalc"); return; }
            if (MessageBox.Show(this, L.F("¿Eliminar el tema «{0}»?", NombreDe(Actual)), "NeoCalc", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            calc.Cfg.Propios.RemoveAll(x => x.Id == Actual.Id);
            calc.AplicarTema(Tema.Incluidos()[0]);
            LlenarGaleria();
            LlenarEditor();
        }

        void Exportar()
        {
            Microsoft.Win32.SaveFileDialog d = new Microsoft.Win32.SaveFileDialog { Filter = L.T("Tema de NeoCalc") + "|*.neocalc.json", FileName = Limpio(Actual.Nombre) + ".neocalc.json", Title = L.T("Exportar tema") };
            if (d.ShowDialog(this) != true) return;
            try
            {
                Tema c = Actual.Copia();
                c.Incluido = false;
                File.WriteAllText(d.FileName, new JavaScriptSerializer().Serialize(c));
            }
            catch (Exception ex) { MessageBox.Show(this, L.F("No se pudo exportar: {0}", ex.Message), "NeoCalc"); }
        }

        void Importar()
        {
            Microsoft.Win32.OpenFileDialog d = new Microsoft.Win32.OpenFileDialog { Filter = L.T("Tema de NeoCalc") + "|*.neocalc.json;*.json", Title = L.T("Importar tema"), Multiselect = true };
            if (d.ShowDialog(this) != true) return;
            Tema ultimo = null;
            foreach (string f in d.FileNames)
            {
                try
                {
                    Tema t = new JavaScriptSerializer().Deserialize<Tema>(File.ReadAllText(f));
                    if (t == null || string.IsNullOrEmpty(t.Fondo1)) continue;
                    Tema b = Tema.Incluidos()[0];
                    // completa lo que falte con el tema por defecto
                    foreach (FieldInfo fi in typeof(Tema).GetFields())
                        if (fi.FieldType == typeof(string) && fi.GetValue(t) == null && fi.Name != "Imagen") fi.SetValue(t, fi.GetValue(b));
                    t.Incluido = false;
                    t.Id = "mio-" + Guid.NewGuid().ToString("N").Substring(0, 8);
                    if (string.IsNullOrEmpty(t.Nombre)) t.Nombre = System.IO.Path.GetFileNameWithoutExtension(f);
                    calc.Cfg.Propios.Add(t);
                    ultimo = t;
                }
                catch (Exception ex) { MessageBox.Show(this, System.IO.Path.GetFileName(f) + ": " + ex.Message, "NeoCalc"); }
            }
            if (ultimo != null) { calc.AplicarTema(ultimo); LlenarGaleria(); LlenarEditor(); }
        }

        static string Limpio(string n)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars()) n = n.Replace(c, '_');
            return n;
        }

        // ---------------------------------------------------------------- controles con estilo oscuro

        TextBlock Titulo(string t) { return new TextBlock { Text = t, FontSize = 24, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 4, 0, 6), FontFamily = new FontFamily("Segoe UI Variable Display, Segoe UI") }; }
        TextBlock Subtitulo(string t) { return new TextBlock { Text = t, FontSize = 15, FontWeight = FontWeights.SemiBold, Foreground = new SolidColorBrush(CAcento), Margin = new Thickness(0, 14, 0, 8) }; }
        TextBlock Nota(string t) { return new TextBlock { Text = t, Foreground = new SolidColorBrush(CSuave), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 2, 0, 4), MaxWidth = 640, HorizontalAlignment = HorizontalAlignment.Left }; }

        TextBox Campo(string texto)
        {
            TextBox t = new TextBox { Text = texto ?? "", Background = new SolidColorBrush(CCampo), Foreground = new SolidColorBrush(CTexto), BorderBrush = new SolidColorBrush(CLinea), CaretBrush = new SolidColorBrush(CTexto), Padding = new Thickness(6, 4, 6, 4), VerticalContentAlignment = VerticalAlignment.Center };
            return t;
        }

        Border Boton(string glifo, string texto, Action accion)
        {
            StackPanel sp = new StackPanel { Orientation = Orientation.Horizontal };
            TextBlock ic = new TextBlock { Text = glifo ?? "", FontFamily = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets"), FontSize = 13, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, glifo == null ? 0 : 8, 0) };
            sp.Children.Add(ic);
            sp.Children.Add(new TextBlock { Text = texto, VerticalAlignment = VerticalAlignment.Center });
            Border b = new Border { Child = sp, Background = new SolidColorBrush(CCampo), BorderBrush = new SolidColorBrush(CLinea), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(6), Padding = new Thickness(12, 6, 12, 6), Margin = new Thickness(0, 0, 8, 4), Cursor = Cursors.Hand };
            b.MouseEnter += delegate { b.Background = new SolidColorBrush(Tema.Mezclar(CCampo, Colors.White, 0.08)); };
            b.MouseLeave += delegate { b.Background = new SolidColorBrush(CCampo); };
            Clic.En(b, delegate { accion(); });
            return b;
        }
    }

    // Selector de color (cuadro saturacion/brillo + tono + transparencia + hexadecimal)
    public static class SelectorColor
    {
        public static void Abrir(FrameworkElement ancla, Color inicial, List<Color> paleta, Action<Color> cambio)
        {
            double h, s, v;
            AHsv(inicial, out h, out s, out v);
            byte alfa = inicial.A;
            Popup p = new Popup { PlacementTarget = ancla, Placement = PlacementMode.Bottom, StaysOpen = false, AllowsTransparency = true };
            Border caja = new Border { Background = new SolidColorBrush(Color.FromRgb(0x2A, 0x2B, 0x34)), BorderBrush = new SolidColorBrush(Color.FromRgb(0x44, 0x46, 0x52)), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(10), Padding = new Thickness(12), Margin = new Thickness(8) };
            caja.Effect = new DropShadowEffect { BlurRadius = 16, ShadowDepth = 2, Opacity = 0.5 };
            StackPanel sp = new StackPanel { Width = 240 };

            // cuadro S/V
            Grid sv = new Grid { Height = 160, Cursor = Cursors.Cross, ClipToBounds = true };
            Border tono = new Border { CornerRadius = new CornerRadius(6) };
            Border blanco = new Border { CornerRadius = new CornerRadius(6), Background = new LinearGradientBrush(Colors.White, Color.FromArgb(0, 255, 255, 255), 0) };
            Border negro = new Border { CornerRadius = new CornerRadius(6), Background = new LinearGradientBrush(Color.FromArgb(0, 0, 0, 0), Colors.Black, 90) };
            Ellipse marca = new Ellipse { Width = 14, Height = 14, Stroke = Brushes.White, StrokeThickness = 2, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, IsHitTestVisible = false };
            marca.Effect = new DropShadowEffect { BlurRadius = 3, ShadowDepth = 0, Opacity = 0.8 };
            sv.Children.Add(tono); sv.Children.Add(blanco); sv.Children.Add(negro); sv.Children.Add(marca);
            sp.Children.Add(sv);

            // tono
            Grid hue = new Grid { Height = 16, Margin = new Thickness(0, 10, 0, 0), Cursor = Cursors.Hand };
            LinearGradientBrush arco = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(1, 0) };
            for (int i = 0; i <= 6; i++) arco.GradientStops.Add(new GradientStop(Tema.Hsl((i * 60) % 360 == 0 && i == 6 ? 359.9 : i * 60, 1, 0.5), i / 6.0));
            hue.Children.Add(new Border { CornerRadius = new CornerRadius(8), Background = arco });
            Border hueMarca = new Border { Width = 6, BorderBrush = Brushes.White, BorderThickness = new Thickness(2), CornerRadius = new CornerRadius(3), HorizontalAlignment = HorizontalAlignment.Left, IsHitTestVisible = false };
            hue.Children.Add(hueMarca);
            sp.Children.Add(hue);

            // transparencia
            Grid fa = new Grid { Margin = new Thickness(0, 10, 0, 0) };
            fa.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            fa.ColumnDefinitions.Add(new ColumnDefinition());
            fa.Children.Add(new TextBlock { Text = L.T("Opacidad"), Foreground = new SolidColorBrush(Color.FromRgb(0x9A, 0x9F, 0xB0)), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) });
            Slider sa = new Slider { Minimum = 0, Maximum = 255, Value = alfa, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(sa, 1); fa.Children.Add(sa);
            sp.Children.Add(fa);

            // hex + muestra
            Grid fh = new Grid { Margin = new Thickness(0, 10, 0, 0) };
            fh.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            fh.ColumnDefinitions.Add(new ColumnDefinition());
            Border muestra = new Border { CornerRadius = new CornerRadius(5), Height = 28, Margin = new Thickness(0, 0, 8, 0) };
            fh.Children.Add(muestra);
            TextBox hex = new TextBox { Background = new SolidColorBrush(Color.FromRgb(0x2E, 0x30, 0x3A)), Foreground = Brushes.White, BorderBrush = new SolidColorBrush(Color.FromRgb(0x44, 0x46, 0x52)), CaretBrush = Brushes.White, Padding = new Thickness(6, 4, 6, 4), FontFamily = new FontFamily("Cascadia Mono, Consolas"), VerticalContentAlignment = VerticalAlignment.Center };
            Grid.SetColumn(hex, 1); fh.Children.Add(hex);
            sp.Children.Add(fh);

            // paleta rapida
            WrapPanel pal = new WrapPanel { Margin = new Thickness(0, 10, 0, 0) };
            List<Color> todos = new List<Color>(paleta);
            foreach (string x in new[] { "#FFFFFF", "#000000", "#202020", "#3B3B3B", "#4CC2FF", "#005FB8", "#00F0FF", "#FF2BD6", "#00FF41", "#FFB000", "#FF4500", "#8BC34A", "#BD93F9", "#FF9F0A", "#D4AF37", "#00000000" })
            {
                Color c = Tema.C(x);
                if (!todos.Contains(c)) todos.Add(c);
            }
            sp.Children.Add(pal);
            caja.Child = sp;
            p.Child = caja;

            bool actualizando = false;
            Action pintar = null;
            Action<bool> emitir = delegate (bool cambiarHex)
            {
                Color c = DeHsv(h, s, v); c.A = alfa;
                pintar();
                if (cambiarHex) { actualizando = true; hex.Text = Tema.Hex(c); actualizando = false; }
                cambio(c);
            };
            pintar = delegate
            {
                tono.Background = new SolidColorBrush(DeHsv(h, 1, 1));
                double w = sv.ActualWidth > 0 ? sv.ActualWidth : 240, hh = sv.ActualHeight > 0 ? sv.ActualHeight : 160;
                marca.Margin = new Thickness(s * w - 7, (1 - v) * hh - 7, 0, 0);
                double hw = hue.ActualWidth > 0 ? hue.ActualWidth : 240;
                hueMarca.Margin = new Thickness(h / 360 * (hw - 6), 0, 0, 0);
                Color c = DeHsv(h, s, v); c.A = alfa;
                muestra.Background = new SolidColorBrush(c);
            };
            foreach (Color c0 in todos)
            {
                Color c = c0;
                Border b = new Border { Width = 22, Height = 22, CornerRadius = new CornerRadius(4), Margin = new Thickness(0, 0, 5, 5), Cursor = Cursors.Hand, BorderBrush = new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)), BorderThickness = new Thickness(1) };
                b.Background = new SolidColorBrush(c);
                if (c.A == 0) { b.Child = new TextBlock { Text = "∅", Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }; b.ToolTip = L.T("Transparente"); }
                Clic.En(b, delegate { AHsv(c, out h, out s, out v); alfa = c.A; sa.Value = alfa; emitir(true); });
                pal.Children.Add(b);
            }

            Action<Point> svEn = delegate (Point pt)
            {
                s = Math.Max(0, Math.Min(1, pt.X / sv.ActualWidth));
                v = Math.Max(0, Math.Min(1, 1 - pt.Y / sv.ActualHeight));
                if (alfa == 0) { alfa = 255; sa.Value = 255; }
                emitir(true);
            };
            sv.MouseLeftButtonDown += delegate (object o, MouseButtonEventArgs e) { sv.CaptureMouse(); svEn(e.GetPosition(sv)); };
            sv.MouseMove += delegate (object o, MouseEventArgs e) { if (sv.IsMouseCaptured) svEn(e.GetPosition(sv)); };
            sv.MouseLeftButtonUp += delegate { sv.ReleaseMouseCapture(); };
            Action<Point> hueEn = delegate (Point pt)
            {
                h = Math.Max(0, Math.Min(359.9, pt.X / hue.ActualWidth * 360));
                if (alfa == 0) { alfa = 255; sa.Value = 255; }
                emitir(true);
            };
            hue.MouseLeftButtonDown += delegate (object o, MouseButtonEventArgs e) { hue.CaptureMouse(); hueEn(e.GetPosition(hue)); };
            hue.MouseMove += delegate (object o, MouseEventArgs e) { if (hue.IsMouseCaptured) hueEn(e.GetPosition(hue)); };
            hue.MouseLeftButtonUp += delegate { hue.ReleaseMouseCapture(); };
            sa.ValueChanged += delegate { if (actualizando) return; byte na = (byte)sa.Value; if (na == alfa) return; alfa = na; emitir(true); };
            hex.TextChanged += delegate
            {
                if (actualizando) return;
                string t = hex.Text.Trim();
                if (!t.StartsWith("#")) t = "#" + t;
                if (t.Length != 7 && t.Length != 9) return;
                try
                {
                    Color c = (Color)ColorConverter.ConvertFromString(t);
                    AHsv(c, out h, out s, out v); alfa = c.A;
                    actualizando = true; sa.Value = alfa; actualizando = false;
                    emitir(false);
                }
                catch { }
            };
            sv.SizeChanged += delegate { pintar(); };
            hue.SizeChanged += delegate { pintar(); };
            actualizando = true; hex.Text = Tema.Hex(inicial); actualizando = false;
            pintar();
            p.IsOpen = true;
        }

        static void AHsv(Color c, out double h, out double s, out double v)
        {
            double r = c.R / 255.0, g = c.G / 255.0, b = c.B / 255.0;
            double max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b)), d = max - min;
            v = max; s = max == 0 ? 0 : d / max;
            if (d == 0) h = 0;
            else if (max == r) h = 60 * (((g - b) / d) % 6);
            else if (max == g) h = 60 * ((b - r) / d + 2);
            else h = 60 * ((r - g) / d + 4);
            if (h < 0) h += 360;
        }

        static Color DeHsv(double h, double s, double v)
        {
            double c = v * s, x = c * (1 - Math.Abs(h / 60 % 2 - 1)), m = v - c;
            double r = 0, g = 0, b = 0;
            if (h < 60) { r = c; g = x; } else if (h < 120) { r = x; g = c; } else if (h < 180) { g = c; b = x; }
            else if (h < 240) { g = x; b = c; } else if (h < 300) { r = x; b = c; } else { r = c; b = x; }
            return Color.FromRgb((byte)Math.Round((r + m) * 255), (byte)Math.Round((g + m) * 255), (byte)Math.Round((b + m) * 255));
        }
    }
}
