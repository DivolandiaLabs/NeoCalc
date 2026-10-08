# NeoCalc — a calculadora do Windows, com 26 temas e totalmente personalizável

[Español](README.md) · [English](README.en.md) · **Português** · [Français](README.fr.md) · [Deutsch](README.de.md) · [Italiano](README.it.md) · [Polski](README.pl.md) · [Русский](README.ru.md) · [한국어](README.ko.md) · [日本語](README.ja.md)

Calculadora padrão e científica para **Windows 10 e 11** que funciona como a do Windows, mas com
**26 temas incluídos** e um editor para criar os seus: cores, degradês, imagem de fundo, fontes,
teclas redondas, brilho neon, visor LCD… Um único `.exe` de cerca de 200 KB, sem instalação.

**Site:** https://divolandialabs.github.io/neocalc/ · **Download:** [versão mais recente](https://github.com/DivolandiaLabs/NeoCalc/releases/latest)

![NeoCalc no modo científico com o tema Neon cyber](docs/screenshots/en-cientifica.png)

## O que faz

- **Modo padrão** igual à calculadora do Windows: as operações se encadeiam (3 + 5 × 2 = 16), `=` repete a última operação e `%` funciona da mesma forma.
- **Modo científico** com precedência de operadores e parênteses (3 + 5 × 2 = 13), `2nd`, DEG/RAD/GRAD, notação científica (F-E), trigonometria, logaritmos, potências, raízes, `n!` (também com decimais), `mod` e `exp`.
- **Histórico e memória** (MC, MR, M+, M−, MS): num painel lateral se a janela for larga, ou sobre as teclas se for estreita.
- **Teclado completo**, copiar e colar, e botão **Sempre visível**.
- **26 temas**: Windows escuro e claro, Neon cyber, Synthwave, Matrix, LCD retrô, Terminal âmbar, Drácula, Nórdico, Game Boy, Holograma, Vidro, Redonda (estilo iPhone), Ouro e preto, Vaporwave…
- **Editor de temas** com pré-visualização instantânea, seletor de cor com transparência, botão **Aleatório** e **importar/exportar** temas (`.neocalc.json`).
- **Ícone da barra de tarefas** com as cores do tema, mesmo com o programa fixado.
- **10 idiomas**: espanhol, inglês, português, francês, alemão, italiano, polonês, russo, coreano e japonês.

![Os 26 temas incluídos](docs/screenshots/en-galeria.png)

## Instalar

1. Baixe `NeoCalc.exe` da [página de versões](https://github.com/DivolandiaLabs/NeoCalc/releases/latest).
2. Salve onde quiser (por exemplo em `Documentos\NeoCalc`) e abra. Não precisa de instalação.
3. Para tê-lo à mão: clique com o botão direito no ícone da barra de tarefas → **Fixar na barra de tarefas**.

O Windows SmartScreen pode avisar na primeira vez porque o programa não é assinado: clique em **Mais informações → Executar assim mesmo**.

Para desinstalar, basta apagar o `.exe` e, se quiser, a pasta `%APPDATA%\NeoCalc`.

## Temas e personalização

Abra o editor com o botão da paleta 🎨 ou com **Ctrl+T**. Ao escolher um tema ele é aplicado na hora; se alterar algo de um
tema incluído, é criada uma cópia em **Meus temas** e o original não é modificado.

![Editor de temas](docs/screenshots/en-temas.png)

É possível alterar: as cores do fundo (degradê e ângulo), uma imagem de fundo, a opacidade da janela, os cantos, o fundo
e os números do visor, a moldura estilo LCD, as cores de cada tipo de tecla, as fontes e sua espessura, o tamanho do texto,
o arredondamento das teclas (até ficarem redondas), o espaçamento, a borda e o brilho neon.

## Atalhos de teclado

| Tecla | Ação |
|---|---|
| `0`–`9`, `,` `.` | Digitar números |
| `+` `-` `*` `/` `Enter` | Operadores / igual |
| `Backspace` · `Del` · `Esc` | Apagar dígito · CE · C |
| `F9` · `R` · `@` · `Q` | +/− · 1/x · raiz · x² |
| `(` `)` `^` `!` `%` | Parênteses, potência, fatorial, mod (científica) |
| `Alt+1` · `Alt+2` | Padrão · Científica |
| `Ctrl+M` `Ctrl+R` `Ctrl+P` `Ctrl+Q` `Ctrl+L` | MS · MR · M+ · M− · MC |
| `Ctrl+H` · `Ctrl+T` | Histórico · Temas |
| `Ctrl+C` · `Ctrl+V` | Copiar · Colar |

## Compilar a partir do código

Não é preciso instalar nada: compila com o compilador C# que já vem no Windows (.NET Framework 4.8).

```powershell
powershell -ExecutionPolicy Bypass -File build.ps1
```

O ícone é desenhado pelo próprio programa (`src\Icono.cs`). Para testar o motor de cálculo: `NeoCalc.exe /prueba resultado.txt`.

## Privacidade

O NeoCalc não se conecta à internet nem coleta nenhum dado. As configurações, o histórico e os seus temas ficam apenas no seu
computador, em `%APPDATA%\NeoCalc\config.json`.

## Requisitos

Windows 10 ou 11 (inclui o .NET Framework 4.8).

---

© 2026 Divolandia Labs · [divolandialabs.github.io](https://divolandialabs.github.io/) · Dúvidas ou ideias? [Abra uma issue](https://github.com/DivolandiaLabs/NeoCalc/issues).
