# CMM Demo Application / KMM Demo App

[English](#english) | [Deutsch](#deutsch)

---

<a name="english"></a>
# CMM Demo Application

## Project Overview
A modern WPF application for visualizing and analyzing Coordinate Measuring Machine (CMM) data. The application demonstrates the integration of measurement data analysis in a modern UI environment, with planned 3D visualization capabilities.

## Current Features

### Measurement Operations
- **XML Data Loading**: Load and parse measurement data from XML files
- **Measurement Simulation**: Simulate measurements with configurable tolerances
- **Batch Processing**: "Measure All" functionality for processing multiple points
- **Status Tracking**: Real-time status updates (NotStarted, InProgress, Completed, Failed)
- **Progress Monitoring**: Visual progress tracking for individual and overall measurements

### User Interface
- **Tree View Navigation**: Hierarchical display of parts and measurement points
- **Advanced Visualizations**: Multiple statistical and analytical views:
  - Statistics Summary with key performance indicators
  - Individual Point Details with precise measurements
  - Tolerance Analysis with distribution graphs
  - Success/Failure Status overview
  - Deviation Measurements analysis
  - Statistical Graphs and trends
- **Reactive Status Bar**: Real-time progress monitoring with synchronized display of:
  - Currently selected measurement point
  - Live measurement progress
  - Adaptive visibility for measurement indicators
- **Multiple UI Themes**: Four distinct visual styles:
  - Minimalist Scientific - Clean, precise, light design for scientific work
  - Industrial Professional - Robust, industrial feel for manufacturing
  - Dark Technical - High contrast, precision-focused dark theme
  - Modern Fluent - Clean, light, Microsoft Fluent Design inspired
- **Theme Switching**: Real-time theme switching with visual style samples
- **Report Preview**: Interactive preview for all export formats
- **Modern UI**: Clean, modular interface with customizable layouts
- **Status Indicators**: Visual feedback for measurement status
- **Progress Tracking**: Progress bars for individual and overall progress
- **Error Handling**: Comprehensive error messages and status updates

### Data Processing and Export
- **Tolerance Checking**: Automatic verification against defined tolerances
- **Real-time Updates**: Immediate UI updates during measurement process
- **Data Persistence**: XML-based data storage and loading
- **Result Analysis**: Deviation calculations and pass/fail determination
- **Multiple Export Formats**:
  - PDF Reports with tables and graphs
  - XML Data for system integration
  - CSV Data for spreadsheet analysis
  - HTML Reports for web viewing
- **Customizable Reports**: Configure included data and visualizations

## Technical Stack
- **Framework**: .NET 8.0 with WPF
- **Architecture**: MVVM pattern with CommunityToolkit.Mvvm
- **Visualization**: ScottPlot for statistical graphs
- **PDF Generation**: iText7 for professional reports
- **UI Design**: Modern interface with customizable layouts
- **Version Control**: Git
- **Testing**: Unit tests for core functionality

## Planned Features

### 3D Visualization (Upcoming)
- **Native 3D Rendering**: Implementation with WPF Viewport3D
- **Interactive Camera Control**: Rotation, zoom, and pan functions
- **Dynamic Point Visualization**: Color-coded measurement points
- **Surface Projection**: Precise point positioning on model surfaces
- **STL Model Support**: Loading and display of STL files

### Advanced Features (Planned)
- **Reporting**: PDF/Excel export of measurement reports
- **Statistical Analysis**: Trend analysis and outlier detection
- **CAD Integration**: Direct import of various CAD formats
- **Cloud Connectivity**: Data synchronization and sharing
- **Live Data**: Real-time CMM hardware integration support
- **AR Integration**: Overlay of nominal/actual data in AR

## Project Structure
```
CMMDemoApp/
├── Models/             # Data models
├── Services/          # Business logic
├── ViewModels/        # MVVM view models
├── Views/             # XAML views
├── Converters/        # Value converters
├── Helpers/           # Utility classes
├── Themes/            # UI themes and styles
└── TestData/         # Sample data
```

---

<a name="deutsch"></a>
# KMM Demo App

## Projektübersicht
Eine moderne WPF-Anwendung zur Visualisierung und Analyse von Koordinatenmessmaschinen (KMM) Daten. Die Anwendung demonstriert die Integration von Datenanalyse in einer modernen UI-Umgebung, mit geplanter 3D-Visualisierung.

## Aktuelle Funktionen

### Messoperationen
- **XML-Daten-Laden**: Laden und Parsen von Messdaten aus XML-Dateien
- **Messsimulation**: Simulation von Messungen mit konfigurierbaren Toleranzen
- **Stapelverarbeitung**: "Alle messen" Funktionalität
- **Statusverfolgung**: Echtzeit-Statusaktualisierungen (Nicht gestartet, In Bearbeitung, Abgeschlossen, Fehlgeschlagen)
- **Fortschrittsüberwachung**: Visuelle Fortschrittsverfolgung

### Benutzeroberfläche
- **Baumansicht**: Hierarchische Anzeige von Teilen und Messpunkten
- **Erweiterte Visualisierungen**: Mehrere statistische und analytische Ansichten:
  - Statistik-Zusammenfassung mit Leistungsindikatoren
  - Detailansicht einzelner Messpunkte
  - Toleranzanalyse mit Verteilungsgraphen
  - Erfolgs-/Fehler-Statusübersicht
  - Abweichungsmessungsanalyse
  - Statistische Graphen und Trends
- **Reaktive Statusleiste**: Echtzeit-Fortschrittsüberwachung mit synchronisierter Anzeige von:
  - Aktuell ausgewähltem Messpunkt
  - Live-Messfortschritt
  - Adaptive Sichtbarkeit für Messindikatoren
- **Berichtvorschau**: Interaktive Vorschau für alle Exportformate
- **Moderne Benutzeroberfläche**: Klare, modulare Oberfläche
- **Statusindikatoren**: Visuelle Rückmeldung zum Messstatus
- **Fortschrittsanzeige**: Fortschrittsbalken für einzelne und Gesamtmessungen
- **Fehlerbehandlung**: Umfassende Fehlermeldungen

### Datenverarbeitung und Export
- **Toleranzprüfung**: Automatische Überprüfung gegen definierte Toleranzen
- **Echtzeit-Updates**: Sofortige UI-Aktualisierungen während des Messvorgangs
- **Datenpersistenz**: XML-basierte Datenspeicherung und -ladung
- **Ergebnisanalyse**: Abweichungsberechnungen und Gut/Schlecht-Bestimmung
- **Mehrere Exportformate**:
  - PDF-Berichte mit Tabellen und Graphen
  - XML-Daten für Systemintegration
  - CSV-Daten für Tabellenanalyse
  - HTML-Berichte für Webanzeige
- **Anpassbare Berichte**: Konfigurierbare Daten und Visualisierungen

## Technischer Stack
- **Framework**: .NET 8.0 mit WPF
- **Architektur**: MVVM-Pattern mit CommunityToolkit.Mvvm
- **Visualisierung**: ScottPlot für statistische Graphen
- **PDF-Generierung**: iText7 für professionelle Berichte
- **UI-Design**: Moderne Oberfläche mit anpassbaren Layouts
- **Versionskontrolle**: Git
- **Tests**: Unit-Tests für Kernfunktionalität

## Geplante Funktionen

### 3D-Visualisierung (Kommend)
- **Native 3D-Rendering**: Implementierung mit WPF Viewport3D
- **Interaktive Kamerasteuerung**: Rotation, Zoom und Pan-Funktionen
- **Dynamische Punktvisualisierung**: Farbkodierte Messpunkte
- **Oberflächenprojektion**: Präzise Punktpositionierung
- **STL-Modell-Unterstützung**: Laden und Anzeigen von STL-Dateien

### Erweiterte Funktionen (Geplant)
- **Berichterstattung**: PDF/Excel-Export von Messberichten
- **Statistische Analyse**: Trendanalyse und Ausreißererkennung
- **CAD-Integration**: Direkter Import verschiedener CAD-Formate
- **Cloud-Anbindung**: Datensynchronisation und -freigabe
- **Live-Daten**: Echtzeit-KMM-Hardware-Integration
- **AR-Integration**: Überlagerung von Soll/Ist-Daten in AR

