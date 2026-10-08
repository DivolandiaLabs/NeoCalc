# NeoCalc — the Windows calculator, with 26 themes and fully customizable

[Español](README.md) · **English** · [Português](README.pt.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Italiano](README.it.md) · [Polski](README.pl.md) · [Русский](README.ru.md) · [한국어](README.ko.md) · [日本語](README.ja.md)

A standard and scientific calculator for **Windows 10 and 11** that works just like the Windows one, but with
**26 built-in themes** and an editor to create your own: colors, gradients, background image, fonts,
round keys, neon glow, LCD display… A single `.exe` of about 200 KB, no installation needed.

**Website:** https://divolandialabs.github.io/neocalc/ · **Download:** [latest release](https://github.com/DivolandiaLabs/NeoCalc/releases/latest)

![NeoCalc in scientific mode with the Cyber neon theme](docs/screenshots/en-cientifica.png)

## What it does

- **Standard mode** exactly like the Windows calculator: operations chain (3 + 5 × 2 = 16), `=` repeats the last operation and `%` behaves the same.
- **Scientific mode** with operator precedence and parentheses (3 + 5 × 2 = 13), `2nd`, DEG/RAD/GRAD, scientific notation (F-E), trigonometry, logarithms, powers, roots, `n!` (decimals too), `mod` and `exp`.
- **History and memory** (MC, MR, M+, M−, MS): in a side panel when the window is wide, or over the keypad when it's narrow.
- **Any size you like**: drag the corner and the whole calculator grows or shrinks; press **Keep this size** and it will always open that way.
- **Full keyboard support**, copy and paste, and an **Always on top** button.
- **26 themes**: Windows dark and light, Cyber neon, Synthwave, Matrix, Retro LCD, Amber terminal, Dracula, Nordic, Game Boy, Hologram, Glass, Round (iPhone style), Gold and black, Vaporwave…
- **Theme editor** with live preview, a color picker with transparency, a **Random** button and theme **import/export** (`.neocalc.json`).
- **Taskbar icon** in the theme's colors, even when the app is pinned.
- **10 languages**: Spanish, English, Portuguese, French, German, Italian, Polish, Russian, Korean and Japanese.

![The 26 built-in themes](docs/screenshots/en-galeria.png)

![The calculator at several sizes with the Keep this size button](docs/screenshots/en-tamanos.png)

## Install

1. Download `NeoCalc.exe` from the [releases page](https://github.com/DivolandiaLabs/NeoCalc/releases/latest).
2. Save it anywhere you like (for example `Documents\NeoCalc`) and open it. No installation needed.
3. To keep it handy: right-click its taskbar icon → **Pin to taskbar**.

Windows SmartScreen may warn you the first time because the app isn't signed: click **More info → Run anyway**.

To uninstall, just delete the `.exe` and, if you want, the `%APPDATA%\NeoCalc` folder.

## Themes and customization

Open the editor with the palette button 🎨 or **Ctrl+T**. Picking a theme applies it instantly; if you change anything in a
built-in theme, a copy is created under **My themes** and the original stays untouched.

![Theme editor](docs/screenshots/en-temas.png)

You can change: the background colors (gradient and angle), a background image, window opacity, corners, the display
background and digits, an LCD-style frame, the colors of each kind of key, fonts and their weight, text size,
key rounding (all the way to round keys), spacing, border and neon glow.

## Keyboard shortcuts

| Key | Action |
|---|---|
| `0`–`9`, `,` `.` | Type numbers |
| `+` `-` `*` `/` `Enter` | Operators / equals |
| `Backspace` · `Del` · `Esc` | Delete digit · CE · C |
| `F9` · `R` · `@` · `Q` | +/− · 1/x · square root · x² |
| `(` `)` `^` `!` `%` | Parentheses, power, factorial, mod (scientific) |
| `Alt+1` · `Alt+2` | Standard · Scientific |
| `Ctrl+M` `Ctrl+R` `Ctrl+P` `Ctrl+Q` `Ctrl+L` | MS · MR · M+ · M− · MC |
| `Ctrl+H` · `Ctrl+T` | History · Themes |
| `Ctrl+C` · `Ctrl+V` | Copy · Paste |

## Build from source

Nothing to install: it builds with the C# compiler that already ships with Windows (.NET Framework 4.8).

```powershell
powershell -ExecutionPolicy Bypass -File build.ps1
```

The icon is drawn by the program itself (`src\Icono.cs`). To check the calculation engine: `NeoCalc.exe /prueba result.txt`.

## Privacy

NeoCalc never connects to the internet and collects no data. Settings, history and your themes are stored only on your
computer, in `%APPDATA%\NeoCalc\config.json`.

## Requirements

Windows 10 or 11 (includes .NET Framework 4.8).

---

© 2026 Divolandia Labs · [divolandialabs.github.io](https://divolandialabs.github.io/) · Questions or ideas? [Open an issue](https://github.com/DivolandiaLabs/NeoCalc/issues).
