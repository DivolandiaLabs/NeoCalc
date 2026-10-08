# NeoCalc — la calcolatrice di Windows, con 26 temi e completamente personalizzabile

[Español](README.md) · [English](README.en.md) · [Português](README.pt.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · **Italiano** · [Polski](README.pl.md) · [Русский](README.ru.md) · [한국어](README.ko.md) · [日本語](README.ja.md)

Calcolatrice standard e scientifica per **Windows 10 e 11** che funziona come quella di Windows, ma con
**26 temi inclusi** e un editor per creare i tuoi: colori, sfumature, immagine di sfondo, caratteri,
tasti rotondi, bagliore neon, display LCD… Un solo `.exe` di circa 200 KB, senza installazione.

**Sito:** https://divolandialabs.github.io/neocalc/ · **Download:** [ultima versione](https://github.com/DivolandiaLabs/NeoCalc/releases/latest)

![NeoCalc in modalità scientifica con il tema Neon cyber](docs/screenshots/en-cientifica.png)

## Cosa fa

- **Modalità standard** identica alla calcolatrice di Windows: le operazioni si concatenano (3 + 5 × 2 = 16), `=` ripete l'ultima operazione e `%` funziona allo stesso modo.
- **Modalità scientifica** con precedenza degli operatori e parentesi (3 + 5 × 2 = 13), `2nd`, DEG/RAD/GRAD, notazione scientifica (F-E), trigonometria, logaritmi, potenze, radici, `n!` (anche con decimali), `mod` ed `exp`.
- **Cronologia e memoria** (MC, MR, M+, M−, MS): in un pannello laterale se la finestra è larga, o sopra i tasti se è stretta.
- **Della dimensione che vuoi**: trascina l'angolo e tutta la calcolatrice si ingrandisce o si rimpicciolisce; con **Mantieni questa dimensione** si aprirà sempre così.
- **Tastiera completa**, copia e incolla, e pulsante **Sempre in primo piano**.
- **26 temi**: Windows scuro e chiaro, Neon cyber, Synthwave, Matrix, LCD retrò, Terminale ambra, Dracula, Nordico, Game Boy, Ologramma, Vetro, Rotonda (stile iPhone), Oro e nero, Vaporwave…
- **Editor dei temi** con anteprima immediata, selettore di colore con trasparenza, pulsante **Casuale** e **importazione/esportazione** dei temi (`.neocalc.json`).
- **Icona della barra delle applicazioni** con i colori del tema, anche quando il programma è aggiunto alla barra.
- **10 lingue**: spagnolo, inglese, portoghese, francese, tedesco, italiano, polacco, russo, coreano e giapponese.

![I 26 temi inclusi](docs/screenshots/en-galeria.png)

## Installazione

1. Scarica `NeoCalc.exe` dalla [pagina delle versioni](https://github.com/DivolandiaLabs/NeoCalc/releases/latest).
2. Salvalo dove vuoi (per esempio in `Documenti\NeoCalc`) e aprilo. Non serve installazione.
3. Per averlo sempre a portata di mano: clic destro sulla sua icona nella barra delle applicazioni → **Aggiungi alla barra delle applicazioni**.

Windows SmartScreen potrebbe avvisarti la prima volta perché il programma non è firmato: fai clic su **Ulteriori informazioni → Esegui comunque**.

Per disinstallarlo basta eliminare il `.exe` e, se vuoi, la cartella `%APPDATA%\NeoCalc`.

## Temi e personalizzazione

Apri l'editor con il pulsante tavolozza 🎨 o con **Ctrl+T**. Scegliendo un tema lo applichi subito; se modifichi qualcosa di un
tema incluso, viene creata una copia in **I miei temi** e l'originale resta intatto.

![Editor dei temi](docs/screenshots/en-temas.png)

Puoi cambiare: i colori dello sfondo (sfumatura e angolo), un'immagine di sfondo, l'opacità della finestra, gli angoli, lo sfondo
e le cifre del display, una cornice stile LCD, i colori di ogni tipo di tasto, i caratteri e il loro spessore, la dimensione del
testo, l'arrotondamento dei tasti (fino a renderli rotondi), la spaziatura, il bordo e il bagliore neon.

## Scorciatoie da tastiera

| Tasto | Azione |
|---|---|
| `0`–`9`, `,` `.` | Inserire numeri |
| `+` `-` `*` `/` `Invio` | Operatori / uguale |
| `Backspace` · `Canc` · `Esc` | Cancella cifra · CE · C |
| `F9` · `R` · `@` · `Q` | +/− · 1/x · radice · x² |
| `(` `)` `^` `!` `%` | Parentesi, potenza, fattoriale, mod (scientifica) |
| `Alt+1` · `Alt+2` | Standard · Scientifica |
| `Ctrl+M` `Ctrl+R` `Ctrl+P` `Ctrl+Q` `Ctrl+L` | MS · MR · M+ · M− · MC |
| `Ctrl+H` · `Ctrl+T` | Cronologia · Temi |
| `Ctrl+C` · `Ctrl+V` | Copia · Incolla |

## Compilare dal codice sorgente

Non serve installare nulla: si compila con il compilatore C# già incluso in Windows (.NET Framework 4.8).

```powershell
powershell -ExecutionPolicy Bypass -File build.ps1
```

L'icona la disegna il programma stesso (`src\Icono.cs`). Per verificare il motore di calcolo: `NeoCalc.exe /prueba risultato.txt`.

## Privacy

NeoCalc non si connette mai a Internet e non raccoglie alcun dato. Impostazioni, cronologia e i tuoi temi restano solo sul tuo
computer, in `%APPDATA%\NeoCalc\config.json`.

## Requisiti

Windows 10 o 11 (include .NET Framework 4.8).

---

© 2026 Divolandia Labs · [divolandialabs.github.io](https://divolandialabs.github.io/) · Domande o idee? [Apri una segnalazione](https://github.com/DivolandiaLabs/NeoCalc/issues).
