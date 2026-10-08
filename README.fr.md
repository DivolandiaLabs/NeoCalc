# NeoCalc — la calculatrice de Windows, avec 26 thèmes et entièrement personnalisable

[Español](README.md) · [English](README.en.md) · [Português](README.pt.md) · **Français** · [Deutsch](README.de.md) · [Italiano](README.it.md) · [Polski](README.pl.md) · [Русский](README.ru.md) · [한국어](README.ko.md) · [日本語](README.ja.md)

Calculatrice standard et scientifique pour **Windows 10 et 11** qui fonctionne comme celle de Windows, mais avec
**26 thèmes inclus** et un éditeur pour créer les vôtres : couleurs, dégradés, image d'arrière-plan, polices,
touches rondes, lueur néon, écran LCD… Un seul `.exe` d'environ 200 Ko, sans installation.

**Site :** https://divolandialabs.github.io/neocalc/ · **Téléchargement :** [dernière version](https://github.com/DivolandiaLabs/NeoCalc/releases/latest)

![NeoCalc en mode scientifique avec le thème Néon cyber](docs/screenshots/en-cientifica.png)

## Fonctionnalités

- **Mode standard** identique à la calculatrice de Windows : les opérations s'enchaînent (3 + 5 × 2 = 16), `=` répète la dernière opération et `%` fonctionne de la même façon.
- **Mode scientifique** avec priorité des opérateurs et parenthèses (3 + 5 × 2 = 13), `2nd`, DEG/RAD/GRAD, notation scientifique (F-E), trigonométrie, logarithmes, puissances, racines, `n!` (y compris décimaux), `mod` et `exp`.
- **Historique et mémoire** (MC, MR, M+, M−, MS) : dans un panneau latéral si la fenêtre est large, ou par-dessus les touches si elle est étroite.
- **À la taille de votre choix** : faites glisser le coin et toute la calculatrice grandit ou rétrécit ; elle se rouvre ensuite à la dernière taille utilisée.
- **Clavier complet**, copier-coller et bouton **Toujours au premier plan**.
- **26 thèmes** : Windows sombre et clair, Néon cyber, Synthwave, Matrix, LCD rétro, Terminal ambre, Dracula, Nordique, Game Boy, Hologramme, Verre, Ronde (style iPhone), Or et noir, Vaporwave…
- **Éditeur de thèmes** avec aperçu instantané, sélecteur de couleur avec transparence, bouton **Aléatoire** et **import/export** de thèmes (`.neocalc.json`).
- **Icône de la barre des tâches** aux couleurs du thème, même quand le programme est épinglé.
- **10 langues** : espagnol, anglais, portugais, français, allemand, italien, polonais, russe, coréen et japonais.

![Les 26 thèmes inclus](docs/screenshots/en-galeria.png)

![La calculatrice à plusieurs tailles](docs/screenshots/en-tamanos.png)

## Installation

1. Téléchargez `NeoCalc.exe` depuis la [page des versions](https://github.com/DivolandiaLabs/NeoCalc/releases/latest).
2. Enregistrez-le où vous voulez (par exemple `Documents\NeoCalc`) et ouvrez-le. Aucune installation n'est nécessaire.
3. Pour l'avoir sous la main : clic droit sur son icône dans la barre des tâches → **Épingler à la barre des tâches**.

Windows SmartScreen peut afficher un avertissement la première fois car le programme n'est pas signé : cliquez sur **Informations complémentaires → Exécuter quand même**.

Pour le désinstaller, supprimez simplement le `.exe` et, si vous le souhaitez, le dossier `%APPDATA%\NeoCalc`.

## Thèmes et personnalisation

Ouvrez l'éditeur avec le bouton palette 🎨 ou **Ctrl+T**. Un thème choisi s'applique immédiatement ; si vous modifiez un
thème inclus, une copie est créée dans **Mes thèmes** et l'original reste intact.

![Éditeur de thèmes](docs/screenshots/en-temas.png)

Vous pouvez modifier : les couleurs de l'arrière-plan (dégradé et angle), une image d'arrière-plan, l'opacité de la fenêtre,
les coins, le fond et les chiffres de l'écran, un cadre style LCD, les couleurs de chaque type de touche, les polices et leur
graisse, la taille du texte, l'arrondi des touches (jusqu'à des touches rondes), l'espacement, la bordure et la lueur néon.

## Raccourcis clavier

| Touche | Action |
|---|---|
| `0`–`9`, `,` `.` | Saisir des nombres |
| `+` `-` `*` `/` `Entrée` | Opérateurs / égal |
| `Retour arrière` · `Suppr` · `Échap` | Effacer un chiffre · CE · C |
| `F9` · `R` · `@` · `Q` | +/− · 1/x · racine · x² |
| `(` `)` `^` `!` `%` | Parenthèses, puissance, factorielle, mod (scientifique) |
| `Alt+1` · `Alt+2` | Standard · Scientifique |
| `Ctrl+M` `Ctrl+R` `Ctrl+P` `Ctrl+Q` `Ctrl+L` | MS · MR · M+ · M− · MC |
| `Ctrl+H` · `Ctrl+T` | Historique · Thèmes |
| `Ctrl+C` · `Ctrl+V` | Copier · Coller |

## Compiler depuis le code source

Rien à installer : il se compile avec le compilateur C# déjà fourni avec Windows (.NET Framework 4.8).

```powershell
powershell -ExecutionPolicy Bypass -File build.ps1
```

L'icône est dessinée par le programme lui-même (`src\Icono.cs`). Pour vérifier le moteur de calcul : `NeoCalc.exe /prueba resultat.txt`.

## Confidentialité

NeoCalc ne se connecte jamais à Internet et ne collecte aucune donnée. Les réglages, l'historique et vos thèmes sont stockés
uniquement sur votre ordinateur, dans `%APPDATA%\NeoCalc\config.json`.

## Configuration requise

Windows 10 ou 11 (inclut .NET Framework 4.8).

---

© 2026 Divolandia Labs · [divolandialabs.github.io](https://divolandialabs.github.io/) · Des questions ou des idées ? [Ouvrez un ticket](https://github.com/DivolandiaLabs/NeoCalc/issues).
