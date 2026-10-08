using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace NeoCalc
{
    // Integracion con la barra de tareas.
    // Un boton ANCLADO no usa el icono de la ventana: usa el del acceso directo anclado (.lnk).
    // Por eso: 1) la ventana lleva AppUserModelID + RelaunchIconResource, asi al anclarla Windows crea el acceso
    // con el icono del tema; 2) al cambiar de tema se reescribe el icono de los accesos anclados que apuntan a NeoCalc.
    public static class Barra
    {
        public const string Aumid = "DivolandiaLabs.NeoCalc";

        public static string Exe { get { return System.Reflection.Assembly.GetExecutingAssembly().Location; } }

        // Carpeta de los .ico: junto al programa si se puede escribir, si no en %APPDATA%\NeoCalc
        public static string CarpetaIconos
        {
            get
            {
                string d = Path.Combine(Path.GetDirectoryName(Exe), "iconos");
                try
                {
                    Directory.CreateDirectory(d);
                    string prueba = Path.Combine(d, ".escritura");
                    File.WriteAllText(prueba, "");
                    File.Delete(prueba);
                    return d;
                }
                catch
                {
                    d = Path.Combine(Config.Carpeta, "iconos");
                    Directory.CreateDirectory(d);
                    return d;
                }
            }
        }

        // Escribe el .ico del tema (nombre distinto por colores para que Windows no use su cache) y devuelve "ruta,0"
        public static string IconoParaBarra(Tema t, bool delTema)
        {
            if (!delTema) return Exe + ",0";
            string clave = (t.Acento + t.Igual + t.Fondo1 + t.Fondo2 + "v2").ToUpperInvariant();
            uint h = 2166136261;
            foreach (char c in clave) { h ^= c; h *= 16777619; }
            string dir = CarpetaIconos;
            string ruta = Path.Combine(dir, "neocalc-" + h.ToString("x8") + ".ico");
            if (!File.Exists(ruta)) File.WriteAllBytes(ruta, Icono.IcoDelTema(t));
            // borra iconos de temas antiguos (se dejan los 3 mas recientes por si algun acceso tarda en refrescarse)
            try
            {
                List<FileInfo> viejos = new List<FileInfo>(new DirectoryInfo(dir).GetFiles("neocalc-*.ico"));
                viejos.Sort(delegate (FileInfo a, FileInfo b) { return b.LastWriteTimeUtc.CompareTo(a.LastWriteTimeUtc); });
                for (int i = 3; i < viejos.Count; i++) if (!string.Equals(viejos[i].FullName, ruta, StringComparison.OrdinalIgnoreCase)) viejos[i].Delete();
            }
            catch { }
            File.SetLastWriteTimeUtc(ruta, DateTime.UtcNow);
            return ruta + ",0";
        }

        // Propiedades de la ventana: a que programa pertenece y con que icono se ancla
        public static void PrepararVentana(IntPtr hwnd, string icono)
        {
            try
            {
                IPropertyStore ps;
                Guid iid = typeof(IPropertyStore).GUID;
                if (SHGetPropertyStoreForWindow(hwnd, ref iid, out ps) != 0 || ps == null) return;
                Poner(ps, PKEY(5), Aumid);
                Poner(ps, PKEY(2), "\"" + Exe + "\"");
                Poner(ps, PKEY(3), icono);
                Poner(ps, PKEY(4), "NeoCalc");
                Marshal.ReleaseComObject(ps);
            }
            catch { }
        }

        // Cambia el icono de los accesos anclados (y de Inicio/escritorio) que abren NeoCalc
        public static int ActualizarAccesos(string icono)
        {
            int n = 0;
            string ap = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            List<string> carpetas = new List<string> {
                Path.Combine(ap, @"Microsoft\Internet Explorer\Quick Launch\User Pinned"),
                Path.Combine(ap, @"Microsoft\Windows\Start Menu\Programs"),
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) };
            string ruta; int idx;
            Partir(icono, out ruta, out idx);
            foreach (string c in carpetas)
            {
                if (!Directory.Exists(c)) continue;
                string[] lnks;
                try { lnks = Directory.GetFiles(c, "*.lnk", SearchOption.AllDirectories); } catch { continue; }
                foreach (string l in lnks)
                {
                    try { if (ActualizarAcceso(l, ruta, idx)) n++; } catch { }
                }
            }
            if (n > 0) SHChangeNotify(0x08000000, 0, IntPtr.Zero, IntPtr.Zero); // SHCNE_ASSOCCHANGED: refresca iconos
            return n;
        }

        public static bool ActualizarAcceso(string lnk, string icono, int idx)
        {
            IShellLinkW sl = (IShellLinkW)new CShellLink();
            try
            {
                ((IPersistFile)sl).Load(lnk, 2);
                StringBuilder sb = new StringBuilder(1024);
                sl.GetPath(sb, sb.Capacity, IntPtr.Zero, 0);
                if (!string.Equals(Path.GetFullPath(sb.ToString()), Path.GetFullPath(Exe), StringComparison.OrdinalIgnoreCase)) return false;
                StringBuilder ic = new StringBuilder(1024); int i0;
                sl.GetIconLocation(ic, ic.Capacity, out i0);
                IPropertyStore ps = (IPropertyStore)sl;
                string aumidActual = Leer(ps, PKEY(5));
                if (string.Equals(ic.ToString(), icono, StringComparison.OrdinalIgnoreCase) && i0 == idx && aumidActual == Aumid) return false;
                sl.SetIconLocation(icono, idx);
                // mismo AppUserModelID que la ventana: asi la ventana abierta se agrupa con el boton anclado
                Poner(ps, PKEY(5), Aumid);
                ps.Commit();
                ((IPersistFile)sl).Save(lnk, true);
                IntPtr p = Marshal.StringToHGlobalUni(lnk);
                SHChangeNotify(0x00002000, 0x0005, p, IntPtr.Zero); // SHCNE_UPDATEITEM, SHCNF_PATHW
                Marshal.FreeHGlobal(p);
                return true;
            }
            finally { Marshal.ReleaseComObject(sl); }
        }

        static void Partir(string icono, out string ruta, out int idx)
        {
            ruta = icono; idx = 0;
            int c = icono.LastIndexOf(',');
            if (c > 2 && int.TryParse(icono.Substring(c + 1), out idx)) ruta = icono.Substring(0, c);
        }

        // ---------------------------------------------------------------- COM

        static PropertyKey PKEY(int pid) { return new PropertyKey { fmtid = new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), pid = pid }; }

        static void Poner(IPropertyStore ps, PropertyKey k, string valor)
        {
            PropVariant v = new PropVariant { vt = 31, p = Marshal.StringToCoTaskMemUni(valor) }; // VT_LPWSTR
            try { ps.SetValue(ref k, ref v); }
            finally { PropVariantClear(ref v); }
        }

        static string Leer(IPropertyStore ps, PropertyKey k)
        {
            PropVariant v;
            if (ps.GetValue(ref k, out v) != 0) return null;
            try { return v.vt == 31 ? Marshal.PtrToStringUni(v.p) : null; }
            finally { PropVariantClear(ref v); }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        struct PropertyKey { public Guid fmtid; public int pid; }

        [StructLayout(LayoutKind.Sequential)]
        struct PropVariant { public ushort vt; public ushort r1, r2, r3; public IntPtr p; public IntPtr p2; }

        [ComImport, Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IPropertyStore
        {
            [PreserveSig] int GetCount(out uint c);
            [PreserveSig] int GetAt(uint i, out PropertyKey k);
            [PreserveSig] int GetValue(ref PropertyKey k, out PropVariant v);
            [PreserveSig] int SetValue(ref PropertyKey k, ref PropVariant v);
            [PreserveSig] int Commit();
        }

        [ComImport, Guid("000214F9-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IShellLinkW
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder f, int c, IntPtr fd, int flags);
            void GetIDList(out IntPtr p);
            void SetIDList(IntPtr p);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder s, int c);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string s);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder s, int c);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string s);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder s, int c);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string s);
            void GetHotkey(out short h);
            void SetHotkey(short h);
            void GetShowCmd(out int c);
            void SetShowCmd(int c);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder s, int c, out int i);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string s, int i);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string s, int r);
            void Resolve(IntPtr hwnd, int flags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string s);
        }

        [ComImport, Guid("00021401-0000-0000-C000-000000000046")]
        class CShellLink { }

        [DllImport("shell32.dll")] static extern int SHGetPropertyStoreForWindow(IntPtr hwnd, ref Guid iid, out IPropertyStore ps);
        [DllImport("shell32.dll")] static extern void SHChangeNotify(int e, int f, IntPtr a, IntPtr b);
        [DllImport("ole32.dll")] static extern int PropVariantClear(ref PropVariant v);
    }
}
