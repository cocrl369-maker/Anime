# Sternenklingen – Anime-Fantasy-RPG für Android (Unity)

Ein rundenbasiertes Anime-Fantasy-RPG im Hochformat, gebaut für Unity und Android.

## Warum rundenbasiert statt 3D Open World?

| Kriterium | 3D Open World | Rundenbasiert |
|---|---|---|
| Entwicklungsaufwand (Solo/kleines Team) | sehr hoch: Terrain, Streaming, Animationen, KI, Kamera, Quests | überschaubar, schnell spielbar |
| Android-Performance | anspruchsvoll (LOD, Speicher, Akku, Überhitzung) | läuft auch auf günstigen Geräten flüssig |
| Touch-Steuerung | virtueller Joystick und Kamera, oft hakelig | Antippen, perfekt fürs Handy |
| Assets im Anime-Stil | geriggte 3D-Modelle und viele Animationen | 2D-Portraits und Illustrationen reichen |
| Bewährt auf Mobile | wenige Erfolge, meist große Studios | sehr erfolgreiches Genre (JRPGs, Gacha-RPGs) |

**Empfehlung:** Mit einem rundenbasierten Kampfsystem starten. Später kann eine kleine
2D- oder 3D-Oberwelt (Stadt, Dungeon-Karte) ergänzt werden, ohne das Kampfsystem neu zu bauen.

## Was ist enthalten?

- **3 Helden** mit eigenen Fähigkeiten und einer **Burst-Ultimate**:
  Akari (Feuer, Schwert), Ren (Wind, Magier), Yuki (Licht, Heilerin)
- **Elementsystem**: Feuer > Wind > Erde > Wasser > Feuer, Licht und Dunkel gegenseitig stark
- **Zugreihenfolge nach Tempo**, Angriff, Fähigkeiten (MP), Verteidigen, Burst-Leiste
- **Statuseffekte**: ATK+, DEF+, Brand, Regeneration, Wiederbelebung
- **5 Kapitel** mit Bosskampf, danach ein schwierigerer neuer Durchlauf (Gegner +5 Level)
- **EP und Level-Ups**, Training zum Leveln, **Spielstand** in den PlayerPrefs
- **Story-Szenen** im Visual-Novel-Stil vor und nach jedem Kapitel (Portraits, Schreibmaschinen-Text, Überspringen)
- **Weltkarte** mit Kapitel-Stationen; geschaffte Kapitel lassen sich zum Trainieren wiederholen
- **Gold, Shop und Ausrüstung**: 10 Items (Waffe, Rüstung, Accessoire), pro Held anlegbar
- **Anime-Portraits** für alle Figuren (in `Resources/Portraits`)
- **Auto-Kampf** und **x2-Tempo**, typische Mobile-Komfortfunktionen
- Hochformat-UI mit Safe Area (Notch), Android-Zurück-Taste, schwebende Schadenszahlen,
  Treffer-Effekte und Burst-Banner

Die gesamte Oberfläche wird **im Code erzeugt**: Du musst keine Szene, keine Prefabs und keine Grafiken
einrichten. Das Spiel startet automatisch in jeder Szene (`GameBootstrap`).

## Einrichtung

1. **Unity Hub** öffnen und **Unity 6 LTS** (oder 2022.3 LTS) mit dem Modul
   **Android Build Support** (inkl. OpenJDK und Android SDK/NDK) installieren.
2. Neues Projekt mit der Vorlage **2D (Built-In Render Pipeline)** oder **Universal 2D** anlegen.
3. Den Ordner `Assets/AnimeFantasy` aus diesem Repository in den `Assets`-Ordner des Projekts kopieren.
   (Alternativ: dieses Repository direkt im Unity Hub über **Add → Add project from disk** öffnen.)
4. Eine Szene öffnen (z. B. `SampleScene`) und **Play** drücken. Für eine Handy-Vorschau im Game-Fenster
   eine Hochformat-Auflösung wie 1080x1920 wählen.

## Android-Build

1. **File → Build Profiles** (bzw. **Build Settings**) → **Android** → **Switch Platform**
2. Die aktuelle Szene mit **Add Open Scenes** hinzufügen.
3. **Player Settings**:
   - *Resolution and Presentation* → Default Orientation: **Portrait**
   - *Other Settings* → Scripting Backend: **IL2CPP**, Target Architectures: **ARM64**
   - Company Name, Product Name („Sternenklingen“) und Package Name (z. B. `com.deinname.sternenklingen`) setzen
4. Handy per USB verbinden (USB-Debugging aktiv) → **Build And Run**.
   Für den Play Store als **.aab** bauen (Option *Build App Bundle*).

## Anime-Grafiken

Die mitgelieferten Portraits werden aus SVG erzeugt (`Tools/art/portraits.py` → `Tools/art/render.js`).
Neu rendern: `python3 Tools/art/portraits.py && node Tools/art/render.js Tools/art/svg Assets/AnimeFantasy/Resources/Portraits`

Zum Ersetzen durch eigene Bilder lege die Dateien unter `Assets/AnimeFantasy/Resources/Portraits/` ab. Der Import-Typ ist egal (Sprite oder Default). Der Dateiname muss der Charakter-ID entsprechen:

`akari.png`, `ren.png`, `yuki.png`, `schleim.png`, `kobold.png`, `golem.png`, `fuchs.png`, `noctis.png`

Ohne Bild wird automatisch ein farbiges Platzhalter-Portrait angezeigt (Datei löschen genügt).

## Projektstruktur

```
Assets/AnimeFantasy/Scripts
├── Core/    GameBootstrap (Autostart), GameManager (Bildschirme, Fortschritt), SaveSystem
├── Data/    GameDatabase (Helden, Gegner, Skills, Kapitel), Datenklassen, Elemente
├── Battle/  BattleController (Rundenablauf), BattleLogic (Schadensformeln), BattleAI, BattleUnit
└── UI/      BattleView, MenuView, UIFactory, SpriteFactory
```

**Inhalte erweitern:** Neue Helden, Gegner, Fähigkeiten und Kapitel werden in
`Data/GameDatabase.cs` eingetragen; dafür sind keine Änderungen an anderer Stelle nötig.

## Ideen für die nächsten Schritte

- Ganzkörper-Illustrationen, gezeichnete Hintergründe, Musik und Soundeffekte
- Weitere Helden zum Freischalten und eine Party-Auswahl
- Verbrauchsgegenstände (Tränke) im Kampf
- Daten auf ScriptableObjects umstellen, damit sie im Editor bearbeitet werden können
