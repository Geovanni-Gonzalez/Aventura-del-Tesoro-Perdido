# 🗺️ Aventura del Tesoro Perdido

> Una aventura gráfica inmersiva impulsada por lógica Prolog y una interfaz moderna en WPF.

![Status](https://img.shields.io/badge/Status-Completado-success?style=flat-square)
![Tech](https://img.shields.io/badge/Stack-WPF%20%7C%20C%23%20%7C%20Prolog-blue?style=flat-square)
![UI](https://img.shields.io/badge/UI-Glassmorphism-purple?style=flat-square)

## 📖 Descripción

**Aventura del Tesoro Perdido** es un juego interactivo de exploración y resolución de acertijos. El jugador asume el rol de un explorador que debe navegar por un mundo misterioso, recolectar objetos y superar obstáculos para encontrar el tesoro legendario.

Lo que hace único a este proyecto es su arquitectura híbrida:

- **Cerebro (Backend)**: Toda la lógica del juego, reglas, estado mundial y pathfinding residen en un servidor **SWI-Prolog**.
- **Cuerpo (Frontend)**: La presentación es una aplicación de escritorio nativa en **Windows Presentation Foundation (WPF)** con un diseño moderno y fluido.

## ✨ Características Principales

- **🧩 Motor de Lógica Simbólica**: Las reglas de juego (qué objeto abre qué puerta, conexiones entre mapas) están definidas declarativamente en Prolog.
- **🎨 Interfaz UI Moderna**: Diseño estilo "Glassmorphism" con paneles semitransparentes, desenfoques y gradientes vibrantes.
- **📡 Arquitectura Cliente-Servidor**: Comunicación robusta vía HTTP/JSON entre la UI (.NET) y el Motor Lógico (Prolog).
- **🎒 Sistema de Inventario**: Gestión dinámica de ítems y validación de requisitos para acceso a zonas.
- **📍 Feedback Visual y Hápitco**: Animaciones de desplazamiento, notificaciones visuales y controles reactivos.
- **🗺️ Pathfinding**: Cálculo automático de rutas posibles y sugerencias de movimiento.

## 🛠️ Tecnologías Utilizadas

| Componente | Tecnología | Descripción |
|------------|------------|-------------|
| **Frontend** | C# / WPF | Interfaz de usuario rica (XAML), animaciones y cliente HTTP. |
| **Backend** | SWI-Prolog | Servidor HTTP, base de conocimiento y reglas de inferencia. |
| **Comunicación** | REST API (JSON) | Protocolo de intercambio de datos entre capas. |
| **Diseño** | XAML | Estilos personalizados, Templates y Triggers para una UX premium. |

## 🚀 Instalación y Uso

### Requisitos Previos

* **SWI-Prolog**: Debe estar instalado y agregado a las variables de entorno (PATH).
- **.NET Framework / Visual Studio**: Para compilar y ejecutar el cliente C#.

### Paso 1: Iniciar el Motor Lógico

Antes de abrir el juego, debes iniciar el servidor de reglas.

1. Navega a `programa/src/AventuraDelTesoroPerdido/Aventura.View/PrologFiles`.
2. Ejecuta el archivo del servidor:

   ```bash
   swipl ServidorProlog.pl
   ```

   *El servidor debería iniciar en el puerto 5000.*

### Paso 2: Ejecutar el Cliente Gráfico

1. Abre la solución en **Visual Studio 2022**.
2. Compila la solución (Rebuild).
3. Ejecuta el proyecto `Aventura.View`.
4. ¡Disfruta la aventura!

## 📂 Estructura del Proyecto

```text
/
├── documentación/          # Documentos académicos y diagramas
├── programa/src/           # Código Fuente
│   ├── Aventura.Controlador # Lógica de conexión y DTOs
│   ├── Aventura.Model       # Modelos de datos compartidos
│   └── Aventura.View        # Interfaz Gráfica (WPF) y Archivos Prolog
│       ├── Assets/         # Imágenes y recursos
│       └── PrologFiles/    # Base de conocimiento (.pl)
└── project-info.json       # Metadatos para portafolio
```

## 👥 Créditos

Desarrollado como parte del curso **Lenguajes de Programación**, II Semestre 2025.
Con un enfoque en la integración de paradigmas de programación (Orientado a Objetos + Lógico).
