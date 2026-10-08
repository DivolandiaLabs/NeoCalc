using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using System.Windows.Threading;

namespace NeoCalc
{
    // Una tecla de la calculadora (Border + texto) con los colores del tema
    public class Tecla : Border
    {
        public string Accion;
        public char Cat;      // n numero, f funcion, o operador, = igual, m memoria/plana
        public TextBlock Texto;
        public Color Base, Hover, Pulsado;
        public bool Activa = true;
        public bool Marcada;  // para F-E y 2nd
        public event Action<Tecla> Pulsada;
        ScaleTransform escala = new ScaleTransform(1, 1);

        public Tecla()
        {
            Texto = new TextBlock();
            Texto.HorizontalAlignment = HorizontalAlignment.Center;
            Texto.VerticalAlignment = VerticalAlignment.Center;
            Texto.TextAlignment = TextAlignment.Center;
            Child = Texto;
            RenderTransformOrigin = new Point(0.5, 0.5);
            RenderTransform = escala;
            Cursor = Cursors.Hand;
            SnapsToDevicePixels = true;
            MouseEnter += delegate { Pintar(); };
            MouseLeave += delegate { Pintar(); };
            MouseLeftButtonDown += delegate (object s, MouseButtonEventArgs e) { if (!Activa) return; CaptureMouse(); Pintar(); e.Handled = true; };
            MouseLeftButtonUp += delegate (object s, MouseButtonEventArgs e)
            {
                bool dentro = IsMouseCaptured && Clic.Dentro(this, e);
                ReleaseMouseCapture();
                Pintar();
                if (dentro && Activa && Pulsada != null) Pulsada(this);
                e.Handled = true;
            };
        }

        public void Pintar()
        {
            bool pulsada = IsMouseCaptured && IsMouseOver;
            Color c = pulsada ? Pulsado : (IsMouseOver && Activa ? Hover : Base);
            Background = new SolidColorBrush(c);
            escala.ScaleX = escala.ScaleY = pulsada ? 0.96 : 1;
            Opacity = Activa ? 1 : 0.45;
        }

        // destello al usar el teclado fisico
        public void Destello()
        {
            Background = new SolidColorBrush(Pulsado);
            escala.ScaleX = escala.ScaleY = 0.96;
            DispatcherTimer t = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(110) };
            t.Tick += delegate { t.Stop(); Pintar(); };
            t.Start();
        }
    }

    // Clic fiable: captura el raton al pulsar y actua al soltar dentro del elemento.
    // (Sin esto, los botones de la barra de titulo perdian clics: la barra llama a DragMove al pulsar,
    // y el bucle de mover la ventana se quedaba con el "soltar")
    public static class Clic
    {
        public static void En(UIElement el, Action accion)
        {
            el.MouseLeftButtonDown += delegate (object s, MouseButtonEventArgs e) { el.CaptureMouse(); e.Handled = true; };
            el.MouseLeftButtonUp += delegate (object s, MouseButtonEventArgs e)
            {
                if (!el.IsMouseCaptured) return;
                bool dentro = Dentro(el, e);
                el.ReleaseMouseCapture();
                e.Handled = true;
                if (dentro) accion();
            };
        }

        public static bool Dentro(UIElement el, MouseEventArgs e)
        {
            Point p = e.GetPosition(el);
            Size t = el.RenderSize;
            return p.X >= 0 && p.Y >= 0 && p.X <= t.Width && p.Y <= t.Height;
        }
    }

    public class VentanaCalc : Window
    {
        public Config Cfg;
        public Tema Tema;
        public Motor Motor = new Motor();
        public double AnchoPrueba;  // para /captura (ventana sin mostrar)

        TextBlock txtPantalla, txtExpresion, txtTitulo;
        ScrollViewer svExpresion;
        Grid colLateral, sobrePanel;
        ColumnDefinition colDefLateral;
        StackPanel listaLateral;
        TextBlock pestHist, pestMem;
        bool verMemoria;
        bool sobreAbierto;
        bool segunda;
        readonly Dictionary<string, Tecla> teclas = new Dictionary<string, Tecla>();
        readonly List<Tecla> todas = new List<Tecla>();
        Popup menu;
        Border botonPin;
        static readonly FontFamily Iconos = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets");
        DispatcherTimer guardarLuego;
        VentanaTemas ventanaTemas;

        public VentanaCalc(Config cfg)
        {
            Cfg = cfg;
            Tema = cfg.Buscar(cfg.Tema);
            Motor.Cientifica = cfg.Cientifica;
            Motor.Angulo = cfg.Angulo ?? "DEG";
            Motor.Memoria.AddRange(cfg.Memoria);
            Motor.Historial.AddRange(cfg.Historial);

            Title = "NeoCalc";
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ResizeMode = ResizeMode.CanResize;
            MinWidth = 300; MinHeight = 470;
            Topmost = cfg.SiempreEncima;
            UseLayoutRounding = true;
            WindowChrome ch = new WindowChrome();
            ch.CaptionHeight = 0; ch.ResizeBorderThickness = new Thickness(8);
            ch.GlassFrameThickness = new Thickness(0); ch.CornerRadius = new CornerRadius(0);
            ch.UseAeroCaptionButtons = false;
            WindowChrome.SetWindowChrome(this, ch);

            Width = cfg.Cientifica ? cfg.AnchoCientifica : cfg.Ancho;
            Height = cfg.Cientifica ? cfg.AltoCientifica : cfg.Alto;
            if (cfg.X > -99000 && cfg.Y > -99000 && DentroDePantalla(cfg.X, cfg.Y))
            { WindowStartupLocation = WindowStartupLocation.Manual; Left = cfg.X; Top = cfg.Y; }
            else WindowStartupLocation = WindowStartupLocation.CenterScreen;

            SourceInitialized += delegate
            {
                IntPtr h = new WindowInteropHelper(this).Handle;
                int st = GetWindowLong(h, -16);
                SetWindowLong(h, -16, st | 0x00020000 | 0x00080000); // WS_MINIMIZEBOX | WS_SYSMENU: minimizar desde la barra
                // identidad para la barra de tareas antes de que salga el boton (icono al anclar)
                try { Barra.PrepararVentana(h, Barra.IconoParaBarra(Tema, Cfg.IconoDelTema)); } catch { }
                if (Topmost) PonerEncima(true);
            };
            SizeChanged += delegate { ActualizarLateral(); };
            PreviewKeyDown += Tecla_Abajo;
            TextInput += Texto_Entrada;
            Closing += delegate { GuardarAhora(); if (ventanaTemas != null) ventanaTemas.Close(); };
            LocationChanged += delegate { GuardarLuego(); };

            Construir();
        }

        static bool DentroDePantalla(double x, double y)
        {
            return x >= SystemParameters.VirtualScreenLeft - 50 && y >= SystemParameters.VirtualScreenTop - 10 &&
                   x < SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - 80 &&
                   y < SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - 80;
        }

        [DllImport("user32.dll")] static extern int GetWindowLong(IntPtr h, int i);
        [DllImport("user32.dll")] static extern int SetWindowLong(IntPtr h, int i, int v);

        // ---------------------------------------------------------------- tema

        public void AplicarTema(Tema t)
        {
            Tema = t;
            Cfg.Tema = t.Id;
            Construir();
            GuardarLuego();
        }

        void PonerIcono()
        {
            try { Icon = Cfg.IconoDelTema ? Icono.DelTema(Tema) : Icono.Original(); } catch { }
            if (AnchoPrueba > 0) return;
            // el boton anclado usa el icono del acceso directo: se actualiza un momento despues (no en cada cambio del editor)
            if (iconoBarraLuego == null)
            {
                iconoBarraLuego = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(700) };
                iconoBarraLuego.Tick += delegate { iconoBarraLuego.Stop(); ActualizarIconoBarra(); };
            }
            iconoBarraLuego.Stop(); iconoBarraLuego.Start();
        }

        DispatcherTimer iconoBarraLuego;

        void ActualizarIconoBarra()
        {
            string icono;
            try { icono = Barra.IconoParaBarra(Tema, Cfg.IconoDelTema); }
            catch (Exception ex) { Registro("icono barra: " + ex.Message); return; }
            IntPtr h = new WindowInteropHelper(this).Handle;
            if (h != IntPtr.Zero) Barra.PrepararVentana(h, icono);
            System.Threading.Thread hilo = new System.Threading.Thread(delegate ()
            {
                try { Barra.ActualizarAccesos(icono); } catch (Exception ex) { Registro("accesos: " + ex.Message); }
            });
            hilo.IsBackground = true;
            hilo.SetApartmentState(System.Threading.ApartmentState.STA);
            hilo.Start();
        }

        static void Registro(string t)
        {
            try { System.IO.File.AppendAllText(System.IO.Path.Combine(Config.Carpeta, "errores.log"), DateTime.Now + " " + t + Environment.NewLine); } catch { }
        }

        // ---------------------------------------------------------------- construccion de la interfaz

        public void Construir()
        {
            teclas.Clear(); todas.Clear();
            Content = ConstruirRaiz();
            Opacity = Math.Max(0.3, Math.Min(1, Tema.Opacidad <= 0 ? 1 : Tema.Opacidad));
            PonerIcono();
            Refrescar();
            ActualizarLateral();
        }

        public FrameworkElement ConstruirRaiz()
        {
            Tema t = Tema;
            Grid fuera = new Grid();
            fuera.Margin = new Thickness(10);
            CornerRadius rv = new CornerRadius(Math.Max(0, t.RadioVentana));

            // sombra de la ventana (separada para que el efecto no afecte al contenido)
            Border sombra = new Border { CornerRadius = rv, Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)) };
            sombra.Effect = new DropShadowEffect { BlurRadius = 16, ShadowDepth = 3, Opacity = 0.45, Direction = 270 };
            if (Tema.C(t.Fondo1).A < 250) sombra.Opacity = 0.25;
            fuera.Children.Add(sombra);

            Border fondo = new Border { CornerRadius = rv };
            double ang = t.AnguloFondo * Math.PI / 180;
            Point a = new Point(0.5 - Math.Sin(ang) * 0.5, 0.5 - Math.Cos(ang) * 0.5);
            Point b = new Point(0.5 + Math.Sin(ang) * 0.5, 0.5 + Math.Cos(ang) * 0.5);
            fondo.Background = new LinearGradientBrush(Tema.C(t.Fondo1), Tema.C(t.Fondo2), a, b);
            Color cBordeV = Tema.C(t.Borde);
            fondo.BorderBrush = new SolidColorBrush(cBordeV.A > 0 ? Color.FromArgb((byte)Math.Min(255, cBordeV.A + 30), cBordeV.R, cBordeV.G, cBordeV.B) : Color.FromArgb(40, 128, 128, 128));
            fondo.BorderThickness = new Thickness(1);
            if (Tema.C(t.Fondo1).A < 250)
            {
                // fondo translucido: le da algo de opacidad para que se pueda pulsar (si no, los clics lo atraviesan)
                fuera.Children.Add(new Border { CornerRadius = rv, Background = new SolidColorBrush(Color.FromArgb(8, 0, 0, 0)) });
            }
            fuera.Children.Add(fondo);

            if (!string.IsNullOrEmpty(t.Imagen) && System.IO.File.Exists(t.Imagen))
            {
                try
                {
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit(); bi.CacheOption = BitmapCacheOption.OnLoad; bi.UriSource = new Uri(t.Imagen); bi.EndInit();
                    Border img = new Border { CornerRadius = rv, Opacity = t.OpacidadImagen <= 0 ? 0.35 : t.OpacidadImagen };
                    img.Background = new ImageBrush(bi) { Stretch = Stretch.UniformToFill };
                    fuera.Children.Add(img);
                }
                catch { }
            }

            Grid g = new Grid();
            g.Margin = new Thickness(1, 1, 1, 4);
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            colDefLateral = new ColumnDefinition { Width = new GridLength(0) };
            g.ColumnDefinitions.Add(colDefLateral);
            g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });                      // titulo
            g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 96 }); // pantalla
            g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });                      // memoria
            g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(Motor.Cientifica ? 4.6 : 3.4, GridUnitType.Star) }); // teclado
            fuera.Children.Add(g);

            // --- barra de titulo (como la de Windows: franja con los botones de la ventana y debajo el modo)
            StackPanel titulo = new StackPanel { Background = Brushes.Transparent };
            titulo.MouseLeftButtonDown += delegate (object s, MouseButtonEventArgs e) { if (e.ClickCount == 1) try { DragMove(); } catch { } };
            Grid franja = new Grid { Height = 32 };
            franja.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            franja.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            StackPanel nombreApp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(10, 0, 0, 0), IsHitTestVisible = false };
            Image ico = new Image { Width = 16, Height = 16, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) };
            try { ico.Source = Icono.Imagen(32, Cfg.IconoDelTema ? Tema.C(t.Acento) : Icono.Cian, Cfg.IconoDelTema ? Tema.C(t.Igual) : Icono.Violeta, Color.FromRgb(0x0B, 0x0E, 0x1A), Color.FromRgb(0x1A, 0x1F, 0x38)); } catch { }
            nombreApp.Children.Add(ico);
            nombreApp.Children.Add(new TextBlock { Text = "NeoCalc", FontSize = 12, VerticalAlignment = VerticalAlignment.Center, Foreground = Tema.B(t.TextoSuave) });
            franja.Children.Add(nombreApp);
            StackPanel der = new StackPanel { Orientation = Orientation.Horizontal };
            Grid.SetColumn(der, 1);
            Border bTemas = BotonIcono("", L.T("Temas y personalización (Ctrl+T)"));
            Clic.En(bTemas, delegate { AbrirTemas(); });
            botonPin = BotonIcono(Topmost ? "" : "", L.T("Siempre encima"));
            Clic.En(botonPin, delegate { CambiarEncima(!Topmost); });
            Border bMin = BotonIcono("", L.T("Minimizar"));
            Clic.En(bMin, delegate { WindowState = WindowState.Minimized; });
            Border bCerrar = BotonIcono("", L.T("Cerrar"));
            Clic.En(bCerrar, delegate { Close(); });
            bCerrar.MouseEnter += delegate { bCerrar.Background = new SolidColorBrush(Color.FromRgb(0xC4, 0x2B, 0x1C)); ((TextBlock)bCerrar.Child).Foreground = Brushes.White; };
            bCerrar.MouseLeave += delegate { bCerrar.Background = Brushes.Transparent; ((TextBlock)bCerrar.Child).Foreground = Tema.B(Tema.Texto); };
            foreach (Border bb in new[] { bTemas, botonPin, bMin, bCerrar }) { bb.Height = 32; bb.Width = 42; ((TextBlock)bb.Child).FontSize = 11; der.Children.Add(bb); }
            botonPin.MouseLeave += delegate { PintarPin(); };
            PintarPin();
            bCerrar.CornerRadius = new CornerRadius(0, Math.Max(0, t.RadioVentana - 1), 0, 0);
            franja.Children.Add(der);
            titulo.Children.Add(franja);

            Grid modo = new Grid { Height = 42 };
            modo.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            modo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            modo.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Border hamb = BotonIcono("", L.T("Menú"));
            Clic.En(hamb, delegate { AbrirMenu(hamb); });
            modo.Children.Add(hamb);
            txtTitulo = new TextBlock { Text = Motor.Cientifica ? L.T("Científica") : L.T("Estándar"), FontSize = 20, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 0, 2), TextTrimming = TextTrimming.CharacterEllipsis };
            txtTitulo.Foreground = Tema.B(t.Texto);
            txtTitulo.FontFamily = new FontFamily("Segoe UI Variable Display, Segoe UI");
            txtTitulo.FontWeight = FontWeights.SemiBold;
            txtTitulo.IsHitTestVisible = false;
            Grid.SetColumn(txtTitulo, 1);
            modo.Children.Add(txtTitulo);
            Border bHist = BotonIcono("", L.T("Historial (Ctrl+H)"));
            Clic.En(bHist, delegate { AlternarHistorial(false); });
            Grid.SetColumn(bHist, 2);
            modo.Children.Add(bHist);
            titulo.Children.Add(modo);
            Grid.SetColumnSpan(titulo, 2);
            g.Children.Add(titulo);

            // --- pantalla
            Border pant = new Border();
            pant.Margin = new Thickness(6, 2, 6, 4);
            Color cp = Tema.C(t.Pantalla);
            if (cp.A > 0)
            {
                pant.Background = t.LCD
                    ? (Brush)new LinearGradientBrush(Tema.Mezclar(cp, Colors.Black, 0.08), cp, 90)
                    : new SolidColorBrush(cp);
                pant.CornerRadius = new CornerRadius(Math.Min(14, Math.Max(3, t.Radio > 100 ? 14 : t.Radio)));
                pant.Padding = new Thickness(12, 6, 12, 6);
                if (t.LCD)
                {
                    pant.BorderBrush = new SolidColorBrush(Tema.Mezclar(cp, Colors.Black, 0.45));
                    pant.BorderThickness = new Thickness(3);
                    pant.Margin = new Thickness(10, 4, 10, 10);
                }
            }
            else pant.Padding = new Thickness(8, 0, 8, 0);
            pant.MouseLeftButtonDown += delegate (object s, MouseButtonEventArgs e) { try { DragMove(); } catch { } };
            pant.Background = pant.Background ?? Brushes.Transparent;
            Grid pg = new Grid();
            pg.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            pg.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            pg.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            txtExpresion = new TextBlock { FontSize = 15, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 0, 2, 0) };
            txtExpresion.Foreground = cp.A > 0 ? new SolidColorBrush(Tema.Mezclar(Tema.C(t.PantallaTexto), cp, 0.35)) : Tema.B(t.TextoSuave);
            txtExpresion.FontFamily = new FontFamily(t.Fuente + ", Segoe UI");
            svExpresion = new ScrollViewer { HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden, VerticalScrollBarVisibility = ScrollBarVisibility.Disabled, Content = txtExpresion, HorizontalContentAlignment = HorizontalAlignment.Right };
            svExpresion.Focusable = false;
            Grid.SetRow(svExpresion, 1);
            pg.Children.Add(svExpresion);
            txtPantalla = new TextBlock { FontSize = 54, HorizontalAlignment = HorizontalAlignment.Right };
            txtPantalla.Foreground = Tema.B(t.PantallaTexto);
            txtPantalla.FontFamily = new FontFamily(t.Fuente + ", Segoe UI");
            txtPantalla.FontWeight = Tema.Peso(t.PesoPantalla);
            if (t.Brillo > 0)
                txtPantalla.Effect = new DropShadowEffect { Color = Tema.C(t.PantallaTexto), BlurRadius = 8 + 14 * t.Brillo, ShadowDepth = 0, Opacity = Math.Min(1, t.Brillo) };
            Viewbox vb = new Viewbox { StretchDirection = StretchDirection.DownOnly, Stretch = Stretch.Uniform, HorizontalAlignment = HorizontalAlignment.Right, Child = txtPantalla, MaxHeight = 76 };
            vb.Margin = new Thickness(0, 0, 0, 2);
            Grid.SetRow(vb, 2);
            pg.Children.Add(vb);
            pant.Child = pg;
            pant.ContextMenu = MenuCopiar();
            Grid.SetRow(pant, 1);
            g.Children.Add(pant);

            // --- fila de memoria
            Grid mem = new Grid { Margin = new Thickness(2, 0, 2, 2), Height = 34 };
            string[][] memT = { new[] { "MC", "MC" }, new[] { "MR", "MR" }, new[] { "M+", "M+" }, new[] { "M−", "M-" }, new[] { "MS", "MS" }, new[] { "M˅", "Mver" } };
            for (int i = 0; i < memT.Length; i++)
            {
                mem.ColumnDefinitions.Add(new ColumnDefinition());
                Tecla k = NuevaTecla(memT[i][0], memT[i][1], 'm');
                Grid.SetColumn(k, i);
                mem.Children.Add(k);
            }
            Grid.SetRow(mem, 2);
            g.Children.Add(mem);

            // --- teclado
            Grid tec = Teclado();
            Grid.SetRow(tec, 3);
            g.Children.Add(tec);

            // --- panel superpuesto (historial/memoria en modo estrecho)
            sobrePanel = new Grid { Visibility = Visibility.Collapsed };
            Border velo = new Border { Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0)) };
            velo.MouseLeftButtonDown += delegate { CerrarSobre(); };
            sobrePanel.Children.Add(velo);
            Grid.SetRow(sobrePanel, 2); Grid.SetRowSpan(sobrePanel, 2);
            g.Children.Add(sobrePanel);

            // --- columna lateral (ventana ancha)
            colLateral = new Grid();
            Grid.SetColumn(colLateral, 1); Grid.SetRow(colLateral, 1); Grid.SetRowSpan(colLateral, 3);
            g.Children.Add(colLateral);

            if (AnchoPrueba > 0) { fuera.Width = AnchoPrueba; }
            return fuera;
        }

        ContextMenu MenuCopiar()
        {
            ContextMenu cm = new ContextMenu();
            MenuItem copiar = new MenuItem { Header = L.T("Copiar"), InputGestureText = "Ctrl+C" };
            copiar.Click += delegate { Copiar(); };
            MenuItem pegar = new MenuItem { Header = L.T("Pegar"), InputGestureText = "Ctrl+V" };
            pegar.Click += delegate { Pegar(); };
            cm.Items.Add(copiar); cm.Items.Add(pegar);
            return cm;
        }

        Border BotonIcono(string glifo, string ayuda)
        {
            Border b = new Border { Width = 40, Height = 34, CornerRadius = new CornerRadius(5), Background = Brushes.Transparent, ToolTip = ayuda, Cursor = Cursors.Hand };
            b.VerticalAlignment = VerticalAlignment.Center;
            TextBlock tx = new TextBlock { Text = glifo, FontFamily = Iconos, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            tx.Foreground = Tema.B(Tema.Texto);
            b.Child = tx;
            Color h = Tema.C(Tema.Texto); h.A = 26;
            b.MouseEnter += delegate { b.Background = new SolidColorBrush(h); };
            b.MouseLeave += delegate { b.Background = Brushes.Transparent; };
            return b;
        }

        Tecla NuevaTecla(string etiqueta, string accion, char cat)
        {
            Tema t = Tema;
            Tecla k = new Tecla { Accion = accion, Cat = cat };
            string fondo, texto;
            switch (cat)
            {
                case 'n': fondo = t.Num; texto = t.NumTexto; break;
                case 'o': fondo = t.Op; texto = t.OpTexto; break;
                case '=': fondo = t.Igual; texto = t.IgualTexto; break;
                case 'm': fondo = "#00000000"; texto = t.Texto; break;
                default: fondo = t.Fun; texto = t.FunTexto; break;
            }
            Color cf = Tema.C(fondo);
            k.Base = cf;
            if (cat == 'm')
            {
                Color h = Tema.C(t.Texto);
                k.Hover = Color.FromArgb(22, h.R, h.G, h.B);
                k.Pulsado = Color.FromArgb(40, h.R, h.G, h.B);
                k.CornerRadius = new CornerRadius(4);
                k.Margin = new Thickness(1);
                k.Texto.FontSize = 13;
                k.Texto.FontWeight = FontWeights.SemiBold;
            }
            else
            {
                k.Hover = Tema.Variar(cf, 0.09);
                k.Pulsado = Tema.Variar(cf, 0.2);
                if (cf.A == 0) { Color h = Tema.C(texto); k.Hover = Color.FromArgb(25, h.R, h.G, h.B); k.Pulsado = Color.FromArgb(50, h.R, h.G, h.B); }
                k.Margin = new Thickness(Math.Max(0, t.Espacio) / 2);
                Color cb = Tema.C(t.Borde);
                if (cb.A > 0 && t.GrosorBorde > 0)
                {
                    k.BorderBrush = new SolidColorBrush(cb);
                    k.BorderThickness = new Thickness(t.GrosorBorde);
                }
                double tam = t.TamTeclas <= 0 ? 15 : t.TamTeclas;
                k.Texto.FontSize = (cat == 'n' && accion.Length == 1) || cat == 'o' || cat == '=' ? tam + 5 : tam;
                k.Texto.FontWeight = Tema.Peso(t.PesoTeclas);
                if (t.Brillo > 0)
                {
                    Color gc = cb.A > 0 ? cb : Tema.C(t.Acento);
                    if (cat == '=') gc = Tema.C(t.Igual);
                    gc.A = 255;
                    k.Effect = new DropShadowEffect { Color = gc, BlurRadius = 4 + 16 * t.Brillo, ShadowDepth = 0, Opacity = Math.Min(0.9, 0.25 + t.Brillo * 0.6) };
                }
            }
            k.Texto.Foreground = Tema.B(texto);
            if (etiqueta.Length == 1 && etiqueta[0] >= '' && etiqueta[0] <= '')
            {
                k.Texto.FontFamily = Iconos;
                k.Texto.FontSize = Math.Max(12, k.Texto.FontSize - 1);
            }
            else k.Texto.FontFamily = new FontFamily((cat == 'm' ? "Segoe UI" : t.FuenteTeclas) + ", Segoe UI, Segoe UI Symbol, Cambria Math");
            k.Texto.Text = etiqueta;
            k.Pulsada += Tecla_Pulsada;
            k.Loaded += delegate { AjustarRadio(k); };
            k.SizeChanged += delegate { AjustarRadio(k); };
            k.Pintar();
            if (!teclas.ContainsKey(accion)) teclas[accion] = k;
            todas.Add(k);
            return k;
        }

        void AjustarRadio(Tecla k)
        {
            if (k.Cat == 'm') return;
            double r = Tema.Radio;
            double max = Math.Min(k.ActualWidth, k.ActualHeight) / 2;
            if (r > max) r = max;
            k.CornerRadius = new CornerRadius(Math.Max(0, r));
        }

        Grid Teclado()
        {
            string dec = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            string[][] filas;
            if (!Motor.Cientifica)
            {
                filas = new string[][] {
                    new[] { "%|%|f", "CE|CE|f", "C|C|f", "|back|f" },
                    new[] { "1/x|inv|f", "x²|sqr|f", "²√x|sqrt|f", "÷|/|o" },
                    new[] { "7|7|n", "8|8|n", "9|9|n", "×|*|o" },
                    new[] { "4|4|n", "5|5|n", "6|6|n", "−|-|o" },
                    new[] { "1|1|n", "2|2|n", "3|3|n", "+|+|o" },
                    new[] { "+/−|neg|n", "0|0|n", dec + "|.|n", "=|=|=" } };
            }
            else
            {
                bool s = segunda;
                filas = new string[][] {
                    new[] { Motor.Angulo + "|ang|f", "F-E|fe|f", (s ? "sin⁻¹|asin" : "sin|sin") + "|f", (s ? "cos⁻¹|acos" : "cos|cos") + "|f", (s ? "tan⁻¹|atan" : "tan|tan") + "|f" },
                    new[] { "2ⁿᵈ|2nd|f", "π|pi|f", "e|e|f", "C|C|f", "|back|f" },
                    new[] { (s ? "x³|cube" : "x²|sqr") + "|f", "1/x|inv|f", "|x||abs|f", "exp|exp|f", "mod|mod|f" },
                    new[] { (s ? "∛x|cbrt" : "²√x|sqrt") + "|f", "(|(|f", ")|)|f", "n!|fact|f", "÷|/|o" },
                    new[] { (s ? "ʸ√x|yroot" : "xʸ|^") + "|f", "7|7|n", "8|8|n", "9|9|n", "×|*|o" },
                    new[] { (s ? "2ˣ|pow2" : "10ˣ|pow10") + "|f", "4|4|n", "5|5|n", "6|6|n", "−|-|o" },
                    new[] { (s ? "logᵧx|logy" : "log|log") + "|f", "1|1|n", "2|2|n", "3|3|n", "+|+|o" },
                    new[] { (s ? "eˣ|powe" : "ln|ln") + "|f", "+/−|neg|n", "0|0|n", dec + "|.|n", "=|=|=" } };
            }
            Grid g = new Grid { Margin = new Thickness(2, 0, 2, 0) };
            int cols = filas[0].Length;
            for (int c = 0; c < cols; c++) g.ColumnDefinitions.Add(new ColumnDefinition());
            for (int r = 0; r < filas.Length; r++)
            {
                g.RowDefinitions.Add(new RowDefinition());
                for (int c = 0; c < cols; c++)
                {
                    string def = filas[r][c];
                    // "|x||abs|f": la etiqueta puede llevar "|"
                    int u = def.LastIndexOf('|');
                    int p = def.LastIndexOf('|', u - 1);
                    string et = def.Substring(0, p), ac = def.Substring(p + 1, u - p - 1);
                    char cat = def[u + 1];
                    Tecla k = NuevaTecla(et, ac, cat);
                    Grid.SetRow(k, r); Grid.SetColumn(k, c);
                    g.Children.Add(k);
                }
            }
            return g;
        }

        // ---------------------------------------------------------------- refresco

        public void Refrescar()
        {
            if (txtPantalla == null) return;
            txtPantalla.Text = Motor.Pantalla;
            txtExpresion.Text = Motor.Expresion;
            svExpresion.UpdateLayout();
            svExpresion.ScrollToRightEnd();
            bool hayMem = Motor.Memoria.Count > 0;
            foreach (Tecla k in todas)
            {
                bool activa = true;
                if (k.Accion == "MC" || k.Accion == "MR" || k.Accion == "Mver") activa = hayMem;
                if (Motor.HayError && k.Cat != 'n' && k.Accion != "C" && k.Accion != "CE" && k.Accion != "back" && k.Cat != 'm') activa = false;
                if (Motor.HayError && (k.Accion == "neg" || k.Accion == ".")) activa = false;
                if (Motor.HayError && k.Cat == 'm') activa = false;
                if (k.Activa != activa) { k.Activa = activa; k.Pintar(); }
                if (k.Accion == "fe" || k.Accion == "2nd")
                {
                    bool on = k.Accion == "fe" ? Motor.NotacionFE : segunda;
                    k.Texto.Foreground = on ? Tema.B(Tema.Acento) : Tema.B(Tema.FunTexto);
                    k.Texto.TextDecorations = on ? TextDecorations.Underline : null;
                }
                if (k.Accion == "(")
                    k.Texto.Text = Motor.ParentesisAbiertos > 0 ? "(" + Sup(Motor.ParentesisAbiertos) : "(";
            }
            if (listaLateral != null) LlenarLista();
        }

        static string Sup(int n)
        {
            string d = "⁰¹²³⁴⁵⁶⁷⁸⁹", r = "";
            foreach (char c in n.ToString()) r += d[c - '0'];
            return r;
        }

        // ---------------------------------------------------------------- acciones

        void Tecla_Pulsada(Tecla k) { Ejecutar(k.Accion); }

        public void Ejecutar(string a)
        {
            switch (a)
            {
                case "2nd": segunda = !segunda; Construir(); return;
                case "ang":
                    Motor.Angulo = Motor.Angulo == "DEG" ? "RAD" : (Motor.Angulo == "RAD" ? "GRAD" : "DEG");
                    Cfg.Angulo = Motor.Angulo;
                    Tecla ka;
                    if (teclas.TryGetValue("ang", out ka)) ka.Texto.Text = Motor.Angulo;
                    GuardarLuego();
                    return;
                case "fe": Motor.NotacionFE = !Motor.NotacionFE; Refrescar(); return;
                case "Mver": AlternarHistorial(true); return;
            }
            Motor.Pulsar(a);
            if (segunda && Motor.Cientifica && a != "2nd" && EsDeSegunda(a)) { segunda = false; Construir(); }
            Refrescar();
            if (a.StartsWith("M") || a == "=") GuardarLuego();
        }

        static bool EsDeSegunda(string a)
        {
            switch (a) { case "asin": case "acos": case "atan": case "cube": case "cbrt": case "yroot": case "pow2": case "logy": case "powe": return true; }
            return false;
        }

        public void CambiarModo(bool cientifica)
        {
            if (cientifica == Motor.Cientifica) return;
            GuardarTamano();
            Motor.Cientifica = cientifica;
            Cfg.Cientifica = cientifica;
            Motor.Limpiar();
            segunda = false;
            Width = cientifica ? Cfg.AnchoCientifica : Cfg.Ancho;
            Height = cientifica ? Cfg.AltoCientifica : Cfg.Alto;
            Construir();
            GuardarLuego();
        }

        void GuardarTamano()
        {
            if (WindowState != WindowState.Normal) return;
            if (Motor.Cientifica) { Cfg.AnchoCientifica = Width; Cfg.AltoCientifica = Height; }
            else { Cfg.Ancho = Width; Cfg.Alto = Height; }
        }

        public void CambiarEncima(bool on)
        {
            Topmost = on;
            PonerEncima(on);
            Cfg.SiempreEncima = on;
            PintarPin();
            GuardarLuego();
        }

        // Topmost de WPF a veces no se aplica en ventanas sin marco: se refuerza con SetWindowPos
        void PonerEncima(bool on)
        {
            IntPtr h = new WindowInteropHelper(this).Handle;
            if (h == IntPtr.Zero) return;
            SetWindowPos(h, on ? new IntPtr(-1) : new IntPtr(-2), 0, 0, 0, 0, 0x0001 | 0x0002 | 0x0010); // NOSIZE|NOMOVE|NOACTIVATE
        }

        // el boton se ve encendido (color de acento) cuando esta activo
        void PintarPin()
        {
            if (botonPin == null) return;
            TextBlock tx = (TextBlock)botonPin.Child;
            tx.Text = Topmost ? "" : "";
            tx.Foreground = Topmost ? Tema.B(Tema.Acento) : Tema.B(Tema.Texto);
            botonPin.ToolTip = Topmost ? L.T("Siempre encima: activado (clic para desactivar)") : L.T("Siempre encima: desactivado");
            Color a = Tema.C(Tema.Acento); a.A = 40;
            botonPin.Background = Topmost ? (Brush)new SolidColorBrush(a) : Brushes.Transparent;
        }

        [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr h, IntPtr after, int x, int y, int cx, int cy, uint f);

        void Copiar()
        {
            try
            {
                string s = Motor.HayError ? Motor.Pantalla : Motor.ValorActual.ToString("R", CultureInfo.CurrentCulture);
                if (!Motor.HayError) s = Motor.Pantalla.Replace(CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator, "");
                Clipboard.SetText(s);
            }
            catch { }
        }

        void Pegar()
        {
            try { if (Clipboard.ContainsText() && Motor.Pegar(Clipboard.GetText())) Refrescar(); } catch { }
        }

        // ---------------------------------------------------------------- teclado fisico

        void Destello(string a)
        {
            Tecla k;
            if (teclas.TryGetValue(a, out k)) k.Destello();
        }

        void Pulsar(string a) { Destello(a); Ejecutar(a); }

        void Tecla_Abajo(object sender, KeyEventArgs e)
        {
            bool ctrl = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
            bool alt = (Keyboard.Modifiers & ModifierKeys.Alt) != 0;
            Key k = e.Key == Key.System ? e.SystemKey : e.Key;
            string a = null;
            if (ctrl)
            {
                switch (k)
                {
                    case Key.C: Copiar(); e.Handled = true; return;
                    case Key.V: Pegar(); e.Handled = true; return;
                    case Key.M: a = "MS"; break;
                    case Key.R: a = "MR"; break;
                    case Key.P: a = "M+"; break;
                    case Key.Q: a = "M-"; break;
                    case Key.L: a = "MC"; break;
                    case Key.H: AlternarHistorial(false); e.Handled = true; return;
                    case Key.T: AbrirTemas(); e.Handled = true; return;
                }
            }
            else if (alt)
            {
                if (k == Key.D1 || k == Key.NumPad1) { CambiarModo(false); e.Handled = true; return; }
                if (k == Key.D2 || k == Key.NumPad2) { CambiarModo(true); e.Handled = true; return; }
            }
            else
            {
                switch (k)
                {
                    case Key.Enter: a = "="; break;
                    case Key.Back: a = "back"; break;
                    case Key.Escape: if (sobreAbierto) { CerrarSobre(); e.Handled = true; return; } a = "C"; break;
                    case Key.Delete: a = "CE"; break;
                    case Key.F9: a = "neg"; break;
                    case Key.NumPad0: case Key.NumPad1: case Key.NumPad2: case Key.NumPad3: case Key.NumPad4:
                    case Key.NumPad5: case Key.NumPad6: case Key.NumPad7: case Key.NumPad8: case Key.NumPad9:
                        a = ((int)(k - Key.NumPad0)).ToString(); break;
                    case Key.Decimal: a = "."; break;
                    case Key.Add: a = "+"; break;
                    case Key.Subtract: a = "-"; break;
                    case Key.Multiply: a = "*"; break;
                    case Key.Divide: a = "/"; break;
                }
            }
            if (a != null) { Pulsar(a); e.Handled = true; }
        }

        // caracteres escritos (respeta la distribucion del teclado)
        void Texto_Entrada(object sender, TextCompositionEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Text)) return;
            char c = e.Text[0];
            string a = null;
            if (c >= '0' && c <= '9') a = c.ToString();
            else switch (c)
                {
                    case '.': case ',': a = "."; break;
                    case '+': a = "+"; break;
                    case '-': a = "-"; break;
                    case '*': case 'x': case 'X': a = "*"; break;
                    case '/': case ':': a = "/"; break;
                    case '=': a = "="; break;
                    case '%': a = Motor.Cientifica ? "mod" : "%"; break;
                    case '(': if (Motor.Cientifica) a = "("; break;
                    case ')': if (Motor.Cientifica) a = ")"; break;
                    case '^': if (Motor.Cientifica) a = "^"; break;
                    case '!': if (Motor.Cientifica) a = "fact"; break;
                    case 'r': case 'R': a = "inv"; break;
                    case '@': a = "sqrt"; break;
                    case 'q': case 'Q': a = "sqr"; break;
                    case 'p': case 'P': if (Motor.Cientifica) a = "pi"; break;
                    case 'e': case 'E': if (Motor.Cientifica) a = "exp"; break;
                    case 's': case 'S': if (Motor.Cientifica) a = "sin"; break;
                    case 'o': case 'O': if (Motor.Cientifica) a = "cos"; break;
                    case 't': case 'T': if (Motor.Cientifica) a = "tan"; break;
                    case 'l': case 'L': if (Motor.Cientifica) a = "log"; break;
                    case 'n': case 'N': if (Motor.Cientifica) a = "ln"; break;
                }
            if (a != null) { Pulsar(a); e.Handled = true; }
        }

        // ---------------------------------------------------------------- historial y memoria

        bool Ancha
        {
            get
            {
                double w = AnchoPrueba > 0 ? AnchoPrueba : ActualWidth;
                return w >= 600;
            }
        }

        void ActualizarLateral()
        {
            if (colDefLateral == null) return;
            bool lateral = Ancha && Cfg.PanelLateral;
            if (lateral)
            {
                if (sobreAbierto) CerrarSobre();
                colDefLateral.Width = new GridLength(Math.Min(320, Math.Max(240, (AnchoPrueba > 0 ? AnchoPrueba : ActualWidth) * 0.38)));
                if (colLateral.Children.Count == 0) colLateral.Children.Add(PanelLista(false));
            }
            else
            {
                colDefLateral.Width = new GridLength(0);
                if (colLateral.Children.Count > 0) { colLateral.Children.Clear(); listaLateral = null; }
            }
        }

        void AlternarHistorial(bool memoria)
        {
            if (Ancha)
            {
                if (memoria && Cfg.PanelLateral) { verMemoria = true; LlenarLista(); return; }
                Cfg.PanelLateral = !Cfg.PanelLateral;
                if (memoria) verMemoria = true;
                ActualizarLateral();
                GuardarLuego();
                return;
            }
            if (sobreAbierto && verMemoria == memoria) { CerrarSobre(); return; }
            verMemoria = memoria;
            if (sobrePanel.Children.Count > 1) sobrePanel.Children.RemoveAt(1);
            Border caja = new Border();
            Color f = Tema.C(Tema.Fondo2); f.A = 255;
            caja.Background = new SolidColorBrush(Tema.Mezclar(f, Tema.C(Tema.Texto), 0.05));
            caja.CornerRadius = new CornerRadius(Math.Min(12, Tema.RadioVentana), Math.Min(12, Tema.RadioVentana), Math.Min(12, Tema.RadioVentana), Math.Min(12, Tema.RadioVentana));
            caja.Margin = new Thickness(2, 0, 2, 0);
            caja.Effect = new DropShadowEffect { BlurRadius = 18, ShadowDepth = 0, Opacity = 0.4 };
            caja.Child = PanelLista(true);
            caja.VerticalAlignment = VerticalAlignment.Stretch;
            sobrePanel.Children.Add(caja);
            sobrePanel.Visibility = Visibility.Visible;
            sobreAbierto = true;
        }

        void CerrarSobre()
        {
            sobreAbierto = false;
            if (sobrePanel != null)
            {
                sobrePanel.Visibility = Visibility.Collapsed;
                if (sobrePanel.Children.Count > 1) sobrePanel.Children.RemoveAt(1);
            }
            listaLateral = null;
            ActualizarLateral();
            if (Ancha && Cfg.PanelLateral && colLateral.Children.Count == 0) colLateral.Children.Add(PanelLista(false));
        }

        FrameworkElement PanelLista(bool superpuesto)
        {
            Grid g = new Grid { Margin = new Thickness(superpuesto ? 6 : 10, 4, 6, 4) };
            g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            StackPanel pest = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(4, 0, 0, 6) };
            pestHist = Pestana(L.T("Historial"), false);
            pestMem = Pestana(L.T("Memoria"), true);
            pest.Children.Add(pestHist); pest.Children.Add(pestMem);
            g.Children.Add(pest);
            ScrollViewer sv = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
            sv.Focusable = false;
            listaLateral = new StackPanel();
            sv.Content = listaLateral;
            Grid.SetRow(sv, 1);
            g.Children.Add(sv);
            Border borrar = BotonIcono("", L.T("Borrar todo"));
            borrar.HorizontalAlignment = HorizontalAlignment.Right;
            Clic.En(borrar, delegate
            {
                if (verMemoria) Motor.Memoria.Clear(); else Motor.Historial.Clear();
                Refrescar(); GuardarLuego();
            });
            Grid.SetRow(borrar, 2);
            g.Children.Add(borrar);
            LlenarLista();
            return g;
        }

        TextBlock Pestana(string texto, bool memoria)
        {
            TextBlock t = new TextBlock { Text = texto, FontSize = 14, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 4, 18, 4), Cursor = Cursors.Hand };
            t.FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI");
            Clic.En(t, delegate { verMemoria = memoria; LlenarLista(); });
            return t;
        }

        void LlenarLista()
        {
            if (listaLateral == null) return;
            listaLateral.Children.Clear();
            Brush texto = Tema.B(Tema.Texto), suave = Tema.B(Tema.TextoSuave);
            if (pestHist != null)
            {
                pestHist.Foreground = verMemoria ? suave : texto;
                pestMem.Foreground = verMemoria ? texto : suave;
                pestHist.TextDecorations = verMemoria ? null : TextDecorations.Underline;
                pestMem.TextDecorations = verMemoria ? TextDecorations.Underline : null;
            }
            Color hc = Tema.C(Tema.Texto); hc.A = 20;
            if (!verMemoria)
            {
                if (Motor.Historial.Count == 0) listaLateral.Children.Add(Vacio(L.T("Todavía no hay historial")));
                foreach (string[] h in Motor.Historial)
                {
                    string[] item = h;
                    StackPanel sp = new StackPanel { Margin = new Thickness(8, 6, 8, 6) };
                    sp.Children.Add(new TextBlock { Text = item[0], Foreground = suave, FontSize = 13, TextAlignment = TextAlignment.Right, TextWrapping = TextWrapping.Wrap });
                    sp.Children.Add(new TextBlock { Text = item[1], Foreground = texto, FontSize = 22, FontWeight = FontWeights.SemiBold, TextAlignment = TextAlignment.Right, TextWrapping = TextWrapping.Wrap, FontFamily = new FontFamily(Tema.Fuente + ", Segoe UI") });
                    Border b = new Border { Child = sp, CornerRadius = new CornerRadius(6), Background = Brushes.Transparent, Cursor = Cursors.Hand };
                    b.MouseEnter += delegate { b.Background = new SolidColorBrush(hc); };
                    b.MouseLeave += delegate { b.Background = Brushes.Transparent; };
                    Clic.En(b, delegate
                    {
                        double v;
                        if (item.Length > 2 && double.TryParse(item[2], NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                        { Motor.UsarResultado(v, item[0]); if (sobreAbierto) CerrarSobre(); Refrescar(); }
                    });
                    listaLateral.Children.Add(b);
                }
            }
            else
            {
                if (Motor.Memoria.Count == 0) listaLateral.Children.Add(Vacio(L.T("No hay nada guardado en la memoria")));
                for (int i = 0; i < Motor.Memoria.Count; i++)
                {
                    int idx = i;
                    StackPanel sp = new StackPanel { Margin = new Thickness(8, 6, 8, 6) };
                    sp.Children.Add(new TextBlock { Text = Motor.Formatear(Motor.Memoria[i]), Foreground = texto, FontSize = 22, FontWeight = FontWeights.SemiBold, TextAlignment = TextAlignment.Right, TextWrapping = TextWrapping.Wrap, FontFamily = new FontFamily(Tema.Fuente + ", Segoe UI") });
                    StackPanel bs = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 4, 0, 0) };
                    bs.Children.Add(MiniBoton("MC", delegate { Motor.Memoria.RemoveAt(idx); }));
                    bs.Children.Add(MiniBoton("M+", delegate { Motor.Memoria[idx] += Motor.ValorActual; }));
                    bs.Children.Add(MiniBoton("M−", delegate { Motor.Memoria[idx] -= Motor.ValorActual; }));
                    sp.Children.Add(bs);
                    Border b = new Border { Child = sp, CornerRadius = new CornerRadius(6), Background = Brushes.Transparent, Cursor = Cursors.Hand };
                    b.MouseEnter += delegate { b.Background = new SolidColorBrush(hc); };
                    b.MouseLeave += delegate { b.Background = Brushes.Transparent; };
                    Clic.En(b, delegate
                    {
                        if (idx >= Motor.Memoria.Count) return;
                        Motor.UsarResultado(Motor.Memoria[idx], "");
                        if (sobreAbierto) CerrarSobre();
                        Refrescar();
                    });
                    listaLateral.Children.Add(b);
                }
            }
        }

        TextBlock Vacio(string t)
        {
            return new TextBlock { Text = t, Foreground = Tema.B(Tema.TextoSuave), Margin = new Thickness(8, 8, 8, 8), FontSize = 13, TextWrapping = TextWrapping.Wrap };
        }

        Border MiniBoton(string t, Action accion)
        {
            Border b = new Border { CornerRadius = new CornerRadius(4), Padding = new Thickness(8, 2, 8, 2), Margin = new Thickness(4, 0, 0, 0), Cursor = Cursors.Hand };
            Color c = Tema.C(Tema.Texto); c.A = 30;
            b.Background = new SolidColorBrush(c);
            b.Child = new TextBlock { Text = t, Foreground = Tema.B(Tema.Texto), FontSize = 11, FontWeight = FontWeights.SemiBold };
            Clic.En(b, delegate { accion(); Refrescar(); GuardarLuego(); });
            return b;
        }

        // ---------------------------------------------------------------- menu principal

        void AbrirMenu(FrameworkElement ancla)
        {
            menu = new Popup { PlacementTarget = ancla, Placement = PlacementMode.Bottom, AllowsTransparency = true, StaysOpen = false, PopupAnimation = PopupAnimation.Fade };
            Border caja = new Border { CornerRadius = new CornerRadius(8), Padding = new Thickness(4), Margin = new Thickness(6), MinWidth = 260 };
            Color f = Tema.C(Tema.Fondo1); f.A = 255;
            caja.Background = new SolidColorBrush(Tema.Mezclar(f, Tema.C(Tema.Texto), 0.07));
            caja.BorderBrush = new SolidColorBrush(Color.FromArgb(50, 128, 128, 128));
            caja.BorderThickness = new Thickness(1);
            caja.Effect = new DropShadowEffect { BlurRadius = 14, ShadowDepth = 2, Opacity = 0.4 };
            StackPanel sp = new StackPanel();
            sp.Children.Add(Cabecera(L.T("Calculadora")));
            sp.Children.Add(Opcion("", L.T("Estándar"), "Alt+1", !Motor.Cientifica, delegate { CambiarModo(false); }));
            sp.Children.Add(Opcion("", L.T("Científica"), "Alt+2", Motor.Cientifica, delegate { CambiarModo(true); }));
            sp.Children.Add(Separador());
            sp.Children.Add(Cabecera(L.T("Apariencia")));
            sp.Children.Add(Opcion("", L.T("Temas y personalización…"), "Ctrl+T", false, delegate { AbrirTemas(); }));
            sp.Children.Add(Opcion("", L.T("Siempre encima"), null, Topmost, delegate { CambiarEncima(!Topmost); }));
            sp.Children.Add(Opcion("", L.T("Icono de la barra con los colores del tema"), null, Cfg.IconoDelTema, delegate { Cfg.IconoDelTema = !Cfg.IconoDelTema; PonerIcono(); GuardarLuego(); }));
            sp.Children.Add(Opcion("", L.T("Historial al lado (ventana ancha)"), null, Cfg.PanelLateral, delegate { Cfg.PanelLateral = !Cfg.PanelLateral; ActualizarLateral(); GuardarLuego(); }));
            sp.Children.Add(Separador());
            string nombreIdioma = string.IsNullOrEmpty(Cfg.Idioma) ? L.T("Automático (idioma de Windows)") : L.Nombres[Array.IndexOf(L.Codigos, Cfg.Idioma)];
            sp.Children.Add(Opcion("\uE774", L.T("Idioma") + ": " + nombreIdioma, null, false, delegate { AbrirMenuIdioma(ancla); }));
            sp.Children.Add(Opcion("", L.T("Acerca de NeoCalc"), null, false, delegate { AcercaDe(); }));
            caja.Child = sp;
            menu.Child = caja;
            menu.IsOpen = true;
        }

        TextBlock Cabecera(string t)
        {
            return new TextBlock { Text = t, Foreground = Tema.B(Tema.TextoSuave), FontSize = 12, FontWeight = FontWeights.SemiBold, Margin = new Thickness(12, 8, 12, 4) };
        }

        Border Separador()
        {
            Color c = Tema.C(Tema.Texto); c.A = 30;
            return new Border { Height = 1, Background = new SolidColorBrush(c), Margin = new Thickness(8, 4, 8, 4) };
        }

        Border Opcion(string glifo, string texto, string atajo, bool marcada, Action accion)
        {
            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            TextBlock ic = new TextBlock { Text = glifo, FontFamily = Iconos, FontSize = 14, VerticalAlignment = VerticalAlignment.Center, Foreground = marcada ? Tema.B(Tema.Acento) : Tema.B(Tema.Texto) };
            g.Children.Add(ic);
            TextBlock tx = new TextBlock { Text = texto, FontSize = 14, VerticalAlignment = VerticalAlignment.Center, Foreground = Tema.B(Tema.Texto), FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI") };
            Grid.SetColumn(tx, 1); g.Children.Add(tx);
            if (marcada || atajo != null)
            {
                TextBlock d = new TextBlock { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 4, 0), FontSize = 12, Foreground = Tema.B(Tema.TextoSuave) };
                if (marcada) { d.Text = ""; d.FontFamily = Iconos; d.Foreground = Tema.B(Tema.Acento); }
                else d.Text = atajo;
                Grid.SetColumn(d, 2); g.Children.Add(d);
            }
            Border b = new Border { Child = g, Padding = new Thickness(10, 7, 10, 7), CornerRadius = new CornerRadius(5), Background = Brushes.Transparent, Cursor = Cursors.Hand };
            if (marcada)
            {
                Color m = Tema.C(Tema.Acento); m.A = 30;
                b.Background = new SolidColorBrush(m);
            }
            Brush bg0 = b.Background;
            Color h = Tema.C(Tema.Texto); h.A = 22;
            b.MouseEnter += delegate { b.Background = new SolidColorBrush(h); };
            b.MouseLeave += delegate { b.Background = bg0; };
            Clic.En(b, delegate { menu.IsOpen = false; accion(); });
            return b;
        }

        void AcercaDe()
        {
            string v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(2);
            MessageBox.Show(this, "NeoCalc " + v + "\n\n" + L.T("Calculadora estándar y científica con temas y personalización.") + "\n\n" +
                L.T("Atajos: Alt+1 / Alt+2 cambiar modo · Ctrl+T temas · Ctrl+H historial") + "\n" +
                L.T("Ctrl+M/R/P/Q/L memoria (MS, MR, M+, M−, MC) · F9 +/− · R 1/x · @ raíz · Q x²") + "\n\n" +
                L.T("Ajustes y temas propios en:") + "\n" + Config.Carpeta + "\n\n© 2026 Divolandia Labs · github.com/DivolandiaLabs/NeoCalc",
                L.T("Acerca de NeoCalc"));
        }

        // Submenu de idiomas (se abre en lugar del menu principal)
        void AbrirMenuIdioma(FrameworkElement ancla)
        {
            menu = new Popup { PlacementTarget = ancla, Placement = PlacementMode.Bottom, AllowsTransparency = true, StaysOpen = false, PopupAnimation = PopupAnimation.Fade };
            Border caja = new Border { CornerRadius = new CornerRadius(8), Padding = new Thickness(4), Margin = new Thickness(6), MinWidth = 260 };
            Color f = Tema.C(Tema.Fondo1); f.A = 255;
            caja.Background = new SolidColorBrush(Tema.Mezclar(f, Tema.C(Tema.Texto), 0.07));
            caja.BorderBrush = new SolidColorBrush(Color.FromArgb(50, 128, 128, 128));
            caja.BorderThickness = new Thickness(1);
            caja.Effect = new DropShadowEffect { BlurRadius = 14, ShadowDepth = 2, Opacity = 0.4 };
            StackPanel sp = new StackPanel();
            sp.Children.Add(Cabecera(L.T("Idioma")));
            string elegido = Cfg.Idioma ?? "";
            sp.Children.Add(Opcion("\uE774", L.T("Automático (idioma de Windows)"), null, elegido == "", delegate { CambiarIdioma(""); }));
            sp.Children.Add(Separador());
            for (int i = 0; i < L.Codigos.Length; i++)
            {
                string cod = L.Codigos[i];
                sp.Children.Add(Opcion("\uE8C1", L.Nombres[i], null, elegido == cod, delegate { CambiarIdioma(cod); }));
            }
            caja.Child = sp;
            menu.Child = caja;
            menu.IsOpen = true;
        }

        void CambiarIdioma(string codigo)
        {
            Cfg.Idioma = codigo;
            L.Poner(codigo);
            Construir();
            if (ventanaTemas != null) { ventanaTemas.Close(); AbrirTemas(); }
            GuardarLuego();
        }

        public void AbrirTemas()
        {
            if (ventanaTemas != null) { ventanaTemas.Activate(); return; }
            ventanaTemas = new VentanaTemas(this);
            ventanaTemas.Closed += delegate { ventanaTemas = null; };
            ventanaTemas.Show();
        }

        // ---------------------------------------------------------------- guardar

        public void GuardarLuego()
        {
            if (guardarLuego == null)
            {
                guardarLuego = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.5) };
                guardarLuego.Tick += delegate { guardarLuego.Stop(); GuardarAhora(); };
            }
            guardarLuego.Stop(); guardarLuego.Start();
        }

        public void GuardarAhora()
        {
            if (AnchoPrueba > 0) return;
            GuardarTamano();
            if (WindowState == WindowState.Normal) { Cfg.X = Left; Cfg.Y = Top; }
            Cfg.Memoria = new List<double>(Motor.Memoria);
            Cfg.Historial = Motor.Historial.Count > 60 ? Motor.Historial.GetRange(0, 60) : new List<string[]>(Motor.Historial);
            Cfg.Guardar();
        }
    }
}
