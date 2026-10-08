using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NeoCalc
{
    // Motor de la calculadora. Imita el comportamiento de la calculadora de Windows:
    //  - Estandar: las operaciones se encadenan de izquierda a derecha (3 + 5 x 2 = 16).
    //  - Cientifica: respeta la prioridad de operadores y los parentesis (3 + 5 x 2 = 13).
    public class Motor
    {
        enum Estado { Escribiendo, Valor, TrasOperador, TrasParentesis, TrasIgual, Error }

        class Tok
        {
            public char Tipo;     // 'n' numero, 'o' operador, '(' , ')'
            public double Val;
            public string Op;     // + - * / ^ mod yroot logy
            public string Txt;    // como se ve en la linea de la expresion
        }

        public bool Cientifica;
        public string Angulo = "DEG";      // DEG, RAD, GRAD
        public bool NotacionFE;
        public readonly List<double> Memoria = new List<double>();
        public readonly List<string[]> Historial = new List<string[]>(); // {expresion, resultado}

        List<Tok> toks = new List<Tok>();
        string entrada = "0";       // texto que se esta escribiendo (formato invariante)
        double valor;               // valor de la entrada cuando no se esta escribiendo
        string valorTxt;            // como se escribe el valor en la expresion (sqr(5), negate(3)...)
        Estado estado = Estado.Escribiendo;
        int abiertos;
        string error;
        string expresionFinal;      // linea de expresion tras pulsar =
        string ultimoOp; double ultimoOperando; bool hayUltimo;

        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        // ---------------------------------------------------------------- lo que se ve

        public string Pantalla
        {
            get
            {
                if (estado == Estado.Error) return L.T(error);
                if (estado == Estado.Escribiendo) return FormatearEntrada(entrada);
                return Formatear(valor);
            }
        }

        public string Expresion
        {
            get
            {
                if (estado == Estado.Error) return expresionFinal ?? "";
                if (estado == Estado.TrasIgual) return expresionFinal ?? "";
                StringBuilder sb = new StringBuilder();
                foreach (Tok t in toks) { if (sb.Length > 0) sb.Append(' '); sb.Append(t.Txt); }
                if (estado == Estado.Valor && valorTxt != null) { if (sb.Length > 0) sb.Append(' '); sb.Append(valorTxt); }
                return sb.ToString();
            }
        }

        public bool HayError { get { return estado == Estado.Error; } }
        public int ParentesisAbiertos { get { return abiertos; } }

        public double ValorActual
        {
            get
            {
                if (estado == Estado.Escribiendo) return Leer(entrada);
                return valor;
            }
        }

        // ---------------------------------------------------------------- entrada de teclas

        // Ejecuta una accion por su nombre (el mismo que lleva cada tecla).
        public void Pulsar(string a)
        {
            if (a.Length == 1 && a[0] >= '0' && a[0] <= '9') { Digito(a[0]); return; }
            if (estado == Estado.Error && a != "C" && a != "CE" && a != "." ) { if (a == "back") Limpiar(); return; }
            switch (a)
            {
                case ".": Decimal(); break;
                case "back": Borrar(); break;
                case "CE": BorrarEntrada(); break;
                case "C": Limpiar(); break;
                case "neg": Negar(); break;
                case "%": Porcentaje(); break;
                case "=": Igual(); break;
                case "(": AbrirParentesis(); break;
                case ")": CerrarParentesis(); break;
                case "exp": Exponente(); break;
                case "pi": Constante(Math.PI, "π"); break;
                case "e": Constante(Math.E, "e"); break;
                case "+": case "-": case "*": case "/": case "^": case "mod": case "yroot": case "logy":
                    Operador(a); break;
                case "MC": Memoria.Clear(); break;
                case "MR": if (Memoria.Count > 0) PonerValor(Memoria[0]); break;
                case "MS": Memoria.Insert(0, ValorActual); TerminarEntrada(); break;
                case "M+": if (Memoria.Count == 0) Memoria.Add(0); Memoria[0] += ValorActual; TerminarEntrada(); break;
                case "M-": if (Memoria.Count == 0) Memoria.Add(0); Memoria[0] -= ValorActual; TerminarEntrada(); break;
                default: Unaria(a); break;
            }
        }

        void Digito(char d)
        {
            if (estado == Estado.Error) Limpiar();
            if (estado == Estado.TrasIgual) { toks.Clear(); abiertos = 0; }
            if (estado == Estado.TrasParentesis) Empujar(new Tok { Tipo = 'o', Op = "*", Txt = "×" });
            if (estado != Estado.Escribiendo) { entrada = "0"; estado = Estado.Escribiendo; valorTxt = null; }
            int e = entrada.IndexOf('e');
            if (e >= 0)
            {
                string exp = entrada.Substring(e + 2);
                if (exp.Length >= 4) return;
                if (exp == "0") entrada = entrada.Substring(0, e + 2) + d; else entrada += d;
                return;
            }
            if (Cifras(entrada) >= 16) return;
            if (entrada == "0") entrada = d.ToString();
            else if (entrada == "-0") entrada = "-" + d;
            else entrada += d;
        }

        void Decimal()
        {
            if (estado == Estado.Error) Limpiar();
            if (estado == Estado.TrasIgual) { toks.Clear(); abiertos = 0; }
            if (estado == Estado.TrasParentesis) Empujar(new Tok { Tipo = 'o', Op = "*", Txt = "×" });
            if (estado != Estado.Escribiendo) { entrada = "0"; estado = Estado.Escribiendo; valorTxt = null; }
            if (entrada.IndexOf('.') < 0 && entrada.IndexOf('e') < 0) entrada += ".";
        }

        void Exponente()
        {
            if (estado != Estado.Escribiendo) { entrada = "0"; estado = Estado.Escribiendo; valorTxt = null; }
            if (entrada.IndexOf('e') < 0) entrada += "e+0";
        }

        void Borrar()
        {
            if (estado == Estado.TrasIgual) { expresionFinal = ""; return; }
            if (estado != Estado.Escribiendo) return;
            int e = entrada.IndexOf('e');
            if (e >= 0)
            {
                string exp = entrada.Substring(e + 2);
                if (exp.Length > 1) { entrada = entrada.Substring(0, entrada.Length - 1); return; }
                if (exp != "0") { entrada = entrada.Substring(0, e + 2) + "0"; return; }
                entrada = entrada.Substring(0, e);
                return;
            }
            entrada = entrada.Substring(0, entrada.Length - 1);
            if (entrada == "" || entrada == "-") entrada = "0";
        }

        void BorrarEntrada()
        {
            if (estado == Estado.Error || estado == Estado.TrasIgual) { Limpiar(); return; }
            entrada = "0"; valorTxt = null;
            estado = Estado.Escribiendo;
        }

        public void Limpiar()
        {
            toks.Clear(); abiertos = 0;
            entrada = "0"; valor = 0; valorTxt = null; expresionFinal = null; error = null;
            estado = Estado.Escribiendo; hayUltimo = false;
        }

        void Negar()
        {
            if (estado == Estado.Escribiendo)
            {
                int e = entrada.IndexOf('e');
                if (e >= 0)
                {
                    char s = entrada[e + 1];
                    entrada = entrada.Substring(0, e + 1) + (s == '+' ? '-' : '+') + entrada.Substring(e + 2);
                }
                else if (entrada != "0") entrada = entrada.StartsWith("-") ? entrada.Substring(1) : "-" + entrada;
                return;
            }
            AplicarUnaria(-ValorActual, "negate");
        }

        void Constante(double v, string nombre)
        {
            if (estado == Estado.TrasIgual) { toks.Clear(); abiertos = 0; }
            if (estado == Estado.TrasParentesis) Empujar(new Tok { Tipo = 'o', Op = "*", Txt = "×" });
            valor = v; valorTxt = nombre; estado = Estado.Valor;
        }

        void PonerValor(double v)
        {
            if (estado == Estado.TrasIgual) { toks.Clear(); abiertos = 0; }
            if (estado == Estado.TrasParentesis) Empujar(new Tok { Tipo = 'o', Op = "*", Txt = "×" });
            valor = v; valorTxt = null; estado = Estado.Valor;
        }

        // Tras MS/M+/M-: el siguiente digito empieza un numero nuevo
        void TerminarEntrada()
        {
            if (estado == Estado.Escribiendo) { valor = Leer(entrada); valorTxt = null; estado = Estado.Valor; }
        }

        void Porcentaje()
        {
            double x = ValorActual, r;
            Tok op = UltimoOperadorPendiente();
            if (op == null) r = 0;
            else if (op.Op == "+" || op.Op == "-") r = ValorParcial() * x / 100.0;
            else r = x / 100.0;
            if (estado == Estado.TrasIgual) { toks.Clear(); r = 0; }
            valor = r; valorTxt = Formatear(r); estado = Estado.Valor;
        }

        Tok UltimoOperadorPendiente()
        {
            if (toks.Count == 0) return null;
            Tok t = toks[toks.Count - 1];
            return t.Tipo == 'o' ? t : null;
        }

        // Valor de lo que hay a la izquierda del ultimo operador (para el %)
        double ValorParcial()
        {
            List<Tok> l = toks.GetRange(0, toks.Count - 1);
            try { return Evaluar(l); } catch { return 0; }
        }

        // Funciones de una sola entrada: sqr, sqrt, 1/x, sin...
        void Unaria(string f)
        {
            double x = ValorActual;
            double r;
            string nombre;
            switch (f)
            {
                case "sqr": r = x * x; nombre = "sqr"; break;
                case "cube": r = x * x * x; nombre = "cube"; break;
                case "sqrt": if (x < 0) { Error("Entrada no válida"); return; } r = Math.Sqrt(x); nombre = "√"; break;
                case "cbrt": r = x < 0 ? -Math.Pow(-x, 1.0 / 3) : Math.Pow(x, 1.0 / 3); nombre = "cuberoot"; break;
                case "inv": if (x == 0) { Error("No se puede dividir por cero"); return; } r = 1 / x; nombre = "1/"; break;
                case "abs": r = Math.Abs(x); nombre = "abs"; break;
                case "pow10": r = Math.Pow(10, x); nombre = "10^"; break;
                case "pow2": r = Math.Pow(2, x); nombre = "2^"; break;
                case "powe": r = Math.Exp(x); nombre = "e^"; break;
                case "log": if (x <= 0) { Error("Entrada no válida"); return; } r = Math.Log10(x); nombre = "log"; break;
                case "ln": if (x <= 0) { Error("Entrada no válida"); return; } r = Math.Log(x); nombre = "ln"; break;
                case "fact":
                    if (x < 0 && x == Math.Floor(x)) { Error("Entrada no válida"); return; }
                    r = Factorial(x); nombre = "fact"; break;
                case "sin": r = Limpio(Math.Sin(ARad(x))); nombre = "sin" + SufAng(); break;
                case "cos": r = Limpio(Math.Cos(ARad(x))); nombre = "cos" + SufAng(); break;
                case "tan":
                    if (Math.Abs(Math.Cos(ARad(x))) < 1e-15) { Error("Entrada no válida"); return; }
                    r = Limpio(Math.Tan(ARad(x))); nombre = "tan" + SufAng(); break;
                case "asin": if (x < -1 || x > 1) { Error("Entrada no válida"); return; } r = Limpio(DeRad(Math.Asin(x))); nombre = "sin⁻¹" + SufAng(); break;
                case "acos": if (x < -1 || x > 1) { Error("Entrada no válida"); return; } r = Limpio(DeRad(Math.Acos(x))); nombre = "cos⁻¹" + SufAng(); break;
                case "atan": r = Limpio(DeRad(Math.Atan(x))); nombre = "tan⁻¹" + SufAng(); break;
                default: return;
            }
            AplicarUnaria(r, nombre);
        }

        void AplicarUnaria(double r, string nombre)
        {
            if (double.IsNaN(r)) { Error("Entrada no válida"); return; }
            if (double.IsInfinity(r)) { Error("Desbordamiento"); return; }
            string dentro;
            if (estado == Estado.Valor && valorTxt != null) dentro = valorTxt;
            else dentro = Formatear(ValorActual);
            if (estado == Estado.TrasIgual) { toks.Clear(); abiertos = 0; }
            valorTxt = nombre == "1/" ? "1/(" + dentro + ")" : nombre + "(" + dentro + ")";
            valor = r; estado = Estado.Valor;
        }

        void Operador(string op)
        {
            if (estado == Estado.TrasOperador && toks.Count > 0 && toks[toks.Count - 1].Tipo == 'o')
            {
                toks[toks.Count - 1] = new Tok { Tipo = 'o', Op = op, Txt = Simbolo(op) };
                return;
            }
            if (estado == Estado.TrasIgual) { toks.Clear(); abiertos = 0; }
            if (estado != Estado.TrasParentesis)
                Empujar(NumTok());
            Empujar(new Tok { Tipo = 'o', Op = op, Txt = Simbolo(op) });
            // En estandar la pantalla muestra el resultado acumulado; en cientifica, el de lo que ya se puede resolver
            if (!Cientifica)
            {
                double r;
                try { r = Evaluar(toks.GetRange(0, toks.Count - 1)); }
                catch (Exception ex) { Error(ex.Message); return; }
                valor = r;
            }
            else valor = ValorActualTrasOperador();
            valorTxt = null;
            estado = Estado.TrasOperador;
        }

        double ValorActualTrasOperador()
        {
            // Resuelve lo que la prioridad ya permite: 2 x 3 + -> 6 ; 2 + 3 x -> 3
            Tok last = toks[toks.Count - 1];
            int p = Prioridad(last.Op);
            int i = toks.Count - 2;
            int nivel = 0;
            int ini = i;
            while (i >= 0)
            {
                Tok t = toks[i];
                if (t.Tipo == ')') nivel++;
                else if (t.Tipo == '(') { if (nivel == 0) break; nivel--; }
                else if (t.Tipo == 'o' && nivel == 0 && Prioridad(t.Op) < p) break;
                ini = i;
                i--;
            }
            try { return Evaluar(toks.GetRange(ini, toks.Count - 1 - ini)); }
            catch { return ValorActual; }
        }

        void AbrirParentesis()
        {
            if (estado == Estado.TrasIgual) { toks.Clear(); abiertos = 0; }
            bool hayNumero = (estado == Estado.Escribiendo && entrada != "0") || estado == Estado.Valor || estado == Estado.TrasParentesis;
            if (hayNumero)
            {
                if (estado != Estado.TrasParentesis) Empujar(NumTok());
                Empujar(new Tok { Tipo = 'o', Op = "*", Txt = "×" });
            }
            Empujar(new Tok { Tipo = '(', Txt = "(" });
            abiertos++;
            entrada = "0"; valor = 0; valorTxt = null;
            estado = Estado.TrasOperador;
        }

        void CerrarParentesis()
        {
            if (abiertos <= 0) return;
            if (estado != Estado.TrasParentesis) Empujar(NumTok());
            Empujar(new Tok { Tipo = ')', Txt = ")" });
            abiertos--;
            // valor del grupo recien cerrado
            int nivel = 0, i = toks.Count - 1;
            for (; i >= 0; i--)
            {
                if (toks[i].Tipo == ')') nivel++;
                else if (toks[i].Tipo == '(') { nivel--; if (nivel == 0) break; }
            }
            try { valor = Evaluar(toks.GetRange(i, toks.Count - i)); }
            catch (Exception ex) { Error(ex.Message); return; }
            valorTxt = null;
            estado = Estado.TrasParentesis;
        }

        void Igual()
        {
            List<Tok> l;
            if (estado == Estado.TrasIgual)
            {
                if (!hayUltimo) return;
                l = new List<Tok>();
                l.Add(new Tok { Tipo = 'n', Val = valor, Txt = Formatear(valor) });
                l.Add(new Tok { Tipo = 'o', Op = ultimoOp, Txt = Simbolo(ultimoOp) });
                l.Add(new Tok { Tipo = 'n', Val = ultimoOperando, Txt = Formatear(ultimoOperando) });
                toks = l;
            }
            else
            {
                if (estado != Estado.TrasParentesis) Empujar(NumTok());
                // "10 - 4 =" y luego "20 =" -> 16 (repite la ultima operacion con el numero nuevo)
                if (toks.Count == 1 && hayUltimo)
                {
                    toks.Add(new Tok { Tipo = 'o', Op = ultimoOp, Txt = Simbolo(ultimoOp) });
                    toks.Add(new Tok { Tipo = 'n', Val = ultimoOperando, Txt = Formatear(ultimoOperando) });
                }
                while (abiertos > 0) { Empujar(new Tok { Tipo = ')', Txt = ")" }); abiertos--; }
                // operando para repetir con "=" otra vez (5 + 3 = = -> 11)
                hayUltimo = false;
                if (toks.Count >= 3 && toks[toks.Count - 2].Tipo == 'o' && toks[toks.Count - 1].Tipo == 'n')
                {
                    ultimoOp = toks[toks.Count - 2].Op;
                    ultimoOperando = toks[toks.Count - 1].Val;
                    hayUltimo = true;
                }
            }
            StringBuilder sb = new StringBuilder();
            foreach (Tok t in toks) { if (sb.Length > 0) sb.Append(' '); sb.Append(t.Txt); }
            sb.Append(" =");
            double r;
            try { r = Evaluar(toks); }
            catch (Exception ex) { expresionFinal = sb.ToString(); Error(ex.Message); toks.Clear(); return; }
            expresionFinal = sb.ToString();
            if (toks.Count > 1 || toks[0].Txt != Formatear(r))
                Historial.Insert(0, new string[] { expresionFinal, Formatear(r), r.ToString("R", Inv) });
            if (Historial.Count > 200) Historial.RemoveAt(Historial.Count - 1);
            toks.Clear();
            valor = r; valorTxt = null;
            estado = Estado.TrasIgual;
        }

        // Recupera un resultado del historial
        public void UsarResultado(double r, string expresion)
        {
            toks.Clear(); abiertos = 0;
            valor = r; valorTxt = null; expresionFinal = expresion;
            estado = Estado.TrasIgual; hayUltimo = false;
        }

        // Pega un numero desde el portapapeles
        public bool Pegar(string texto)
        {
            if (texto == null) return false;
            string t = texto.Trim().Replace(" ", "").Replace(" ", "");
            CultureInfo cur = CultureInfo.CurrentCulture;
            t = t.Replace(cur.NumberFormat.NumberGroupSeparator, "");
            t = t.Replace(cur.NumberFormat.NumberDecimalSeparator, ".");
            double v;
            if (!double.TryParse(t, NumberStyles.Float, Inv, out v)) return false;
            PonerValor(v);
            return true;
        }

        void Error(string msg)
        {
            error = msg; estado = Estado.Error;
            toks.Clear(); abiertos = 0;
        }

        Tok NumTok()
        {
            double v = ValorActual;
            string txt = (estado == Estado.Valor && valorTxt != null) ? valorTxt : Formatear(v);
            return new Tok { Tipo = 'n', Val = v, Txt = txt };
        }

        void Empujar(Tok t) { toks.Add(t); }

        // ---------------------------------------------------------------- evaluacion

        static int Prioridad(string op)
        {
            switch (op)
            {
                case "+": case "-": return 1;
                case "*": case "/": case "mod": return 2;
                default: return 3; // ^ yroot logy
            }
        }

        double Evaluar(List<Tok> l)
        {
            // Estandar: sin prioridad. Cientifica: precedencia con "shunting-yard".
            List<double> vals = new List<double>();
            List<Tok> ops = new List<Tok>();
            foreach (Tok t in l)
            {
                if (t.Tipo == 'n') vals.Add(t.Val);
                else if (t.Tipo == '(') ops.Add(t);
                else if (t.Tipo == ')')
                {
                    while (ops.Count > 0 && ops[ops.Count - 1].Tipo != '(') Reducir(vals, ops);
                    if (ops.Count > 0) ops.RemoveAt(ops.Count - 1);
                }
                else
                {
                    int p = Cientifica ? Prioridad(t.Op) : 1;
                    bool derecha = Cientifica && t.Op == "^";
                    while (ops.Count > 0 && ops[ops.Count - 1].Tipo == 'o')
                    {
                        int q = Cientifica ? Prioridad(ops[ops.Count - 1].Op) : 1;
                        if (q > p || (q == p && !derecha)) Reducir(vals, ops); else break;
                    }
                    ops.Add(t);
                }
            }
            while (ops.Count > 0) { if (ops[ops.Count - 1].Tipo == '(') ops.RemoveAt(ops.Count - 1); else Reducir(vals, ops); }
            if (vals.Count == 0) return 0;
            return vals[vals.Count - 1];
        }

        void Reducir(List<double> vals, List<Tok> ops)
        {
            Tok o = ops[ops.Count - 1]; ops.RemoveAt(ops.Count - 1);
            if (vals.Count < 2) return;
            double b = vals[vals.Count - 1], a = vals[vals.Count - 2];
            vals.RemoveRange(vals.Count - 2, 2);
            vals.Add(Operar(a, o.Op, b));
        }

        static double Operar(double a, string op, double b)
        {
            double r;
            switch (op)
            {
                case "+": r = a + b; break;
                case "-": r = a - b; break;
                case "*": r = a * b; break;
                case "/":
                    if (b == 0) throw new Exception(a == 0 ? "Resultado indefinido" : "No se puede dividir por cero");
                    r = a / b; break;
                case "mod":
                    if (b == 0) throw new Exception("No se puede dividir por cero");
                    r = a - b * Math.Floor(a / b); break;
                case "^": r = Math.Pow(a, b); break;
                case "yroot":
                    if (b == 0) throw new Exception("Entrada no válida");
                    if (a < 0 && Math.Abs(b % 2) == 1) r = -Math.Pow(-a, 1 / b);
                    else r = Math.Pow(a, 1 / b);
                    break;
                case "logy":
                    if (a <= 0 || b <= 0 || b == 1) throw new Exception("Entrada no válida");
                    r = Math.Log(a) / Math.Log(b); break;
                default: r = b; break;
            }
            if (double.IsNaN(r)) throw new Exception("Entrada no válida");
            if (double.IsInfinity(r)) throw new Exception("Desbordamiento");
            // 0,3 - 0,1 - 0,2 da 0 y no 2,7e-17
            if ((op == "+" || op == "-") && r != 0 && Math.Abs(r) < Math.Max(Math.Abs(a), Math.Abs(b)) * 1e-15) r = 0;
            return r;
        }

        // ---------------------------------------------------------------- utilidades

        static string Simbolo(string op)
        {
            switch (op)
            {
                case "+": return "+";
                case "-": return "−";
                case "*": return "×";
                case "/": return "÷";
                case "^": return "^";
                case "mod": return "mod";
                case "yroot": return "yroot";
                case "logy": return "log base";
            }
            return op;
        }

        double ARad(double x)
        {
            if (Angulo == "DEG") return x * Math.PI / 180;
            if (Angulo == "GRAD") return x * Math.PI / 200;
            return x;
        }
        double DeRad(double x)
        {
            if (Angulo == "DEG") return x * 180 / Math.PI;
            if (Angulo == "GRAD") return x * 200 / Math.PI;
            return x;
        }
        string SufAng() { return Angulo == "DEG" ? "₀" : (Angulo == "RAD" ? "ᵣ" : "g"); }

        static double Limpio(double r)
        {
            if (Math.Abs(r) < 1e-14) return 0;
            return double.Parse(r.ToString("G15", Inv), Inv);
        }

        static double Factorial(double x)
        {
            if (x == Math.Floor(x) && x >= 0)
            {
                if (x > 170) return double.PositiveInfinity;
                double r = 1;
                for (int i = 2; i <= (int)x; i++) r *= i;
                return r;
            }
            return double.Parse(Gamma(x + 1).ToString("G14", Inv), Inv); // Lanczos da ~15 cifras buenas
        }

        // Aproximacion de Lanczos (factorial de numeros con decimales, como hace Windows)
        static double Gamma(double z)
        {
            if (z < 0.5) return Math.PI / (Math.Sin(Math.PI * z) * Gamma(1 - z));
            double[] g = { 0.99999999999980993, 676.5203681218851, -1259.1392167224028, 771.32342877765313,
                           -176.61502916214059, 12.507343278686905, -0.13857109526572012, 9.9843695780195716e-6, 1.5056327351493116e-7 };
            z -= 1;
            double x = g[0];
            for (int i = 1; i < 9; i++) x += g[i] / (z + i);
            double t = z + 7.5;
            return Math.Sqrt(2 * Math.PI) * Math.Pow(t, z + 0.5) * Math.Exp(-t) * x;
        }

        static int Cifras(string s)
        {
            int n = 0;
            foreach (char c in s) if (c >= '0' && c <= '9') n++;
            return n;
        }

        static double Leer(string s)
        {
            double v;
            if (s.EndsWith(".")) s = s.Substring(0, s.Length - 1);
            if (double.TryParse(s, NumberStyles.Float, Inv, out v)) return v;
            return 0;
        }

        // Numero -> texto de pantalla con separadores del idioma del sistema
        public string Formatear(double v)
        {
            if (double.IsNaN(v) || double.IsInfinity(v)) return "Desbordamiento";
            // G15 si representa el numero exacto (evita 0,8862269254527601); si no, G16 (pi = 3,141592653589793)
            string s = v.ToString("G15", Inv);
            double back;
            if (!double.TryParse(s, NumberStyles.Float, Inv, out back) || back != v) s = v.ToString("G16", Inv);
            double a = Math.Abs(v);
            bool cientifico = s.Contains("E") || (NotacionFE && v != 0) || (a != 0 && (a >= 1e16 || a < 1e-15));
            if (cientifico)
            {
                string m = v.ToString("0.###############E+0", Inv);
                int e = m.IndexOf('E');
                string man = m.Substring(0, e), ex = m.Substring(e + 1);
                if (!man.Contains(".")) man += ".";
                return FormatearEntrada(man).Replace(" ", "") + "e" + ex;
            }
            return FormatearEntrada(s);
        }

        // Texto invariante ("-1234.5") -> "-1.234,5" segun el idioma
        public static string FormatearEntrada(string s)
        {
            NumberFormatInfo nf = CultureInfo.CurrentCulture.NumberFormat;
            string exp = "";
            int e = s.IndexOf('e');
            if (e >= 0) { exp = "e" + s.Substring(e + 1); s = s.Substring(0, e); }
            bool neg = s.StartsWith("-");
            if (neg) s = s.Substring(1);
            string ent = s, dec = null;
            int p = s.IndexOf('.');
            if (p >= 0) { ent = s.Substring(0, p); dec = s.Substring(p + 1); }
            StringBuilder sb = new StringBuilder();
            string sep = nf.NumberGroupSeparator;
            for (int i = 0; i < ent.Length; i++)
            {
                if (i > 0 && (ent.Length - i) % 3 == 0) sb.Append(sep);
                sb.Append(ent[i]);
            }
            if (dec != null) { sb.Append(nf.NumberDecimalSeparator); sb.Append(dec); }
            return (neg ? "-" : "") + sb.ToString() + exp;
        }
    }
}
