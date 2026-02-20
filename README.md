# BuoniPastoCS

Applicazione desktop cross-platform sviluppata in C# (.NET + Avalonia) che analizza un PDF di presenze e individua i buoni pasto maturati secondo regole prefissate:

- buono pasto **pranzo**: venerdì, sabato o domenica se si esce dalle 15:30 in poi;
- buono pasto **cena**: tutti i giorni se si esce dalle 20:30 in poi.

## Dipendenze principali

- Avalonia UI
- iText7 (per leggere i PDF)

## Installazione di .NET su macOS

```
brew install dotnet
dotnet --version
```
È un cross-compilatore.

## Compilazione per macOS

Dalla directory principale del progetto, lanciare
```
dotnet restore
```
che gestisce/installa/ripristina le dipendenze, e
```
dotnet run
```
per esegure localmente, o in alternativa
```
dotnet build
```
per compilare localmente a scopo di debug.

Infine, lanciare
```
dotnet publish -c Release -r osx-arm64 --self-contained true
```
Opzione facoltativa: `-p:PublishSingleFile=true` (crea un file singolo).

Il binario viene creato dentro `bin/Release/net*/osx-arm64/publish/BuoniPasto`.

Per impostare l'icona dell'app su macOS, seguire le istruzioni nel file `Help-icone.txt`.

Ora creare il pacchetto `.app`:
```
PUBLISH_DIR=$(ls -d bin/Release/net9.0/osx-arm64/publish | head -n 1)
mkdir -p "BuoniPasto.app/Contents/MacOS"
mkdir -p "BuoniPasto.app/Contents/Resources"
cp -R "$PUBLISH_DIR/"* "BuoniPasto.app/Contents/MacOS/"
cp AppIcon.icns "BuoniPasto.app/Contents/Resources/AppIcon.icns"
```
Accertarsi che il file `Info.plst` sia già presente nel progetto. \
Accertarsi che `BuoniPasto.app/Contents/MacOS/BuoniPasto` abbia il permesso `+x`.

### Avviso per l'app compilata su GitHub

Se macOS si rifiuta di eseguire l'app visualizzando un avviso di file insicuro oppure danneggiato, sbloccare Gatekeeper:
```
xattr -cr /path/BuoniPasto.app
```

## Compilazione per Windows

Per impostare l'icona dell'app su Windows, seguire le istruzioni nel file `Help-icone.txt`.

Dopo i summenzionati `dotnet restore`, `dotnet run` e/o `dotnet build`, compilare con
```
dotnet publish -c Release -r win-x64 --self-contained true
```
Opzione facoltativa: `-p:PublishSingleFile=true` (crea un file singolo).

Se non si crea un file singolo, è necessario distribuire tutta la cartella `publish` (anche zippata), non il solo file EXE:
```
publish/
├── BuoniPasto.exe
├── BuoniPasto.dll
├── Avalonia.*
├── runtimes/
└── *.json
```
