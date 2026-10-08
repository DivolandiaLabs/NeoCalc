# NeoCalc — 26가지 테마와 자유로운 사용자 지정을 갖춘 Windows 계산기

[Español](README.md) · [English](README.en.md) · [Português](README.pt.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Italiano](README.it.md) · [Polski](README.pl.md) · [Русский](README.ru.md) · **한국어** · [日本語](README.ja.md)

**Windows 10 및 11**용 표준·공학용 계산기입니다. Windows 계산기와 똑같이 동작하면서
**26가지 기본 제공 테마**와 나만의 테마를 만드는 편집기를 제공합니다: 색상, 그라데이션, 배경 이미지, 글꼴,
원형 키, 네온 광채, LCD 디스플레이… 약 200KB의 `.exe` 파일 하나로, 설치가 필요 없습니다.

**웹사이트:** https://divolandialabs.github.io/neocalc/ · **다운로드:** [최신 버전](https://github.com/DivolandiaLabs/NeoCalc/releases/latest)

![사이버 네온 테마를 적용한 공학용 모드의 NeoCalc](docs/screenshots/en-cientifica.png)

## 주요 기능

- **표준 모드**는 Windows 계산기와 동일합니다: 연산이 순서대로 이어지고(3 + 5 × 2 = 16), `=`는 마지막 연산을 반복하며 `%`도 같은 방식으로 동작합니다.
- **공학용 모드**는 연산자 우선순위와 괄호를 지원합니다(3 + 5 × 2 = 13). `2nd`, DEG/RAD/GRAD, 지수 표기(F-E), 삼각함수, 로그, 거듭제곱, 제곱근, `n!`(소수 포함), `mod`, `exp`.
- **기록 및 메모리**(MC, MR, M+, M−, MS): 창이 넓으면 옆 패널에, 좁으면 키패드 위에 표시됩니다.
- **원하는 크기로**: 모서리를 끌면 계산기 전체가 커지거나 작아지며, 다음에는 마지막으로 사용한 크기로 열립니다.
- **키보드 완전 지원**, 복사·붙여넣기, **항상 위** 버튼.
- **26가지 테마**: Windows 어둡게/밝게, 사이버 네온, Synthwave, Matrix, 레트로 LCD, 앰버 터미널, 드라큘라, 노르딕, Game Boy, 홀로그램, 유리, 원형(iPhone 스타일), 골드 & 블랙, Vaporwave…
- **테마 편집기**: 실시간 미리 보기, 투명도를 지원하는 색 선택기, **무작위** 버튼, 테마 **가져오기/내보내기**(`.neocalc.json`).
- 프로그램을 고정해도 테마 색상이 적용되는 **작업 표시줄 아이콘**.
- **10개 언어**: 스페인어, 영어, 포르투갈어, 프랑스어, 독일어, 이탈리아어, 폴란드어, 러시아어, 한국어, 일본어.

![기본 제공 테마 26가지](docs/screenshots/en-galeria.png)

![여러 크기의 계산기](docs/screenshots/en-tamanos.png)

## 설치

1. [릴리스 페이지](https://github.com/DivolandiaLabs/NeoCalc/releases/latest)에서 `NeoCalc.exe`를 다운로드합니다.
2. 원하는 위치(예: `문서\NeoCalc`)에 저장하고 실행합니다. 설치는 필요 없습니다.
3. 손쉽게 사용하려면 작업 표시줄 아이콘을 마우스 오른쪽 버튼으로 클릭 → **작업 표시줄에 고정**.

프로그램이 서명되지 않았기 때문에 처음 실행할 때 Windows SmartScreen 경고가 나타날 수 있습니다: **추가 정보 → 실행**을 클릭하세요.

제거하려면 `.exe` 파일과, 원한다면 `%APPDATA%\NeoCalc` 폴더를 삭제하면 됩니다.

## 테마 및 사용자 지정

팔레트 버튼 🎨 또는 **Ctrl+T**로 편집기를 엽니다. 테마를 선택하면 바로 적용되며, 기본 제공 테마를 수정하면
**내 테마**에 사본이 만들어지고 원본은 그대로 유지됩니다.

![테마 편집기](docs/screenshots/en-temas.png)

변경할 수 있는 항목: 배경 색상(그라데이션과 각도), 배경 이미지, 창 불투명도, 모서리, 디스플레이 배경과 숫자, LCD 스타일
테두리, 키 종류별 색상, 글꼴과 두께, 텍스트 크기, 키 둥글기(원형까지), 간격, 테두리, 네온 광채.

## 바로 가기 키

| 키 | 동작 |
|---|---|
| `0`–`9`, `,` `.` | 숫자 입력 |
| `+` `-` `*` `/` `Enter` | 연산자 / 등호 |
| `Backspace` · `Delete` · `Esc` | 숫자 지우기 · CE · C |
| `F9` · `R` · `@` · `Q` | +/− · 1/x · 제곱근 · x² |
| `(` `)` `^` `!` `%` | 괄호, 거듭제곱, 팩토리얼, mod (공학용) |
| `Alt+1` · `Alt+2` | 표준 · 공학용 |
| `Ctrl+M` `Ctrl+R` `Ctrl+P` `Ctrl+Q` `Ctrl+L` | MS · MR · M+ · M− · MC |
| `Ctrl+H` · `Ctrl+T` | 기록 · 테마 |
| `Ctrl+C` · `Ctrl+V` | 복사 · 붙여넣기 |

## 소스 코드로 빌드

따로 설치할 것이 없습니다. Windows에 기본 포함된 C# 컴파일러(.NET Framework 4.8)로 빌드합니다.

```powershell
powershell -ExecutionPolicy Bypass -File build.ps1
```

아이콘은 프로그램이 직접 그립니다(`src\Icono.cs`). 계산 엔진 확인: `NeoCalc.exe /prueba result.txt`.

## 개인정보 보호

NeoCalc는 인터넷에 연결하지 않으며 어떤 데이터도 수집하지 않습니다. 설정, 기록, 사용자 테마는
`%APPDATA%\NeoCalc\config.json`에 내 컴퓨터에만 저장됩니다.

## 요구 사항

Windows 10 또는 11 (.NET Framework 4.8 포함).

---

© 2026 Divolandia Labs · [divolandialabs.github.io](https://divolandialabs.github.io/) · 질문이나 아이디어가 있나요? [이슈 등록](https://github.com/DivolandiaLabs/NeoCalc/issues).
