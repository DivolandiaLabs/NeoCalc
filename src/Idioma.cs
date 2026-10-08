using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace NeoCalc
{
    // Traducciones. La clave es el propio texto en espanol; Idiomas.txt (recurso incrustado) tiene una linea por texto:
    // "es | en | pt | fr | de | it | pl | ru | ko | ja". Si falta una traduccion se muestra el espanol.
    public static class L
    {
        public static readonly string[] Codigos = { "es", "en", "pt", "fr", "de", "it", "pl", "ru", "ko", "ja" };
        public static readonly string[] Nombres = { "Español", "English", "Português", "Français", "Deutsch", "Italiano", "Polski", "Русский", "한국어", "日本語" };

        static Dictionary<string, string[]> tabla;
        static int actual;

        public static string Actual { get { return Codigos[actual]; } }

        // "" = idioma de Windows (si no esta entre los nuestros, ingles)
        public static void Poner(string codigo)
        {
            if (string.IsNullOrEmpty(codigo)) codigo = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            int i = Array.IndexOf(Codigos, codigo);
            actual = i < 0 ? 1 : i;
        }

        public static string T(string es)
        {
            if (es == null || actual == 0) return es;
            Cargar();
            string[] t;
            if (tabla.TryGetValue(es, out t) && actual < t.Length && t[actual].Length > 0) return t[actual];
            return es;
        }

        public static string F(string es, params object[] datos) { return string.Format(T(es), datos); }

        static void Cargar()
        {
            if (tabla != null) return;
            tabla = new Dictionary<string, string[]>();
            try
            {
                using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("Idiomas.txt"))
                using (StreamReader r = new StreamReader(s, System.Text.Encoding.UTF8))
                {
                    string linea;
                    bool primera = true;
                    while ((linea = r.ReadLine()) != null)
                    {
                        if (primera) { primera = false; continue; }
                        if (linea.Trim().Length == 0) continue;
                        string[] p = linea.Split(new[] { " | " }, StringSplitOptions.None);
                        for (int i = 0; i < p.Length; i++) p[i] = p[i].Trim();
                        tabla[p[0]] = p;
                    }
                }
            }
            catch { }
        }
    }
}
