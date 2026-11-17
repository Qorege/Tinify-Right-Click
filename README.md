# Tiny Right Click (TinyRC) 🐼

A lightweight, modern Windows desktop utility to compress images directly from the Windows Explorer right-click context menu using the [Tinify (TinyPNG / TinyJPG) API](https://tinypng.com/developers).

![App Icon](icon.png)

## Features

- **Context Menu Integration**: One-click install/uninstall for the `TinyRC` option on all image files (`.png`, `.jpg`, `.jpeg`, `.webp`).
- **In-Place Replacement**: Simply right-click any image and select **TinyRC** — your original image is automatically replaced with the optimized version.
- **Batch Processing**: Supports selecting multiple images at once to compress them in parallel.
- **Native & Lightweight**: Built as a native Windows WPF application with C# and .NET 8, compiled into a single-file executable (~300 KB).
- **Modern Dark UI**: Clean dialog window with API key configuration and context menu status management.
- **Windows Notifications**: Real-time balloon notifications showing compression savings (e.g. `saved 65.0% (63.6 KB → 22.3 KB)`).

---

## Getting Started

### 1. Download or Build
Run `TinyRightClick.exe`.

### 2. Enter Your API Key
1. Get a free API key from [tinypng.com/developers](https://tinypng.com/developers) (500 free compressions per month).
2. Enter the key in the **Tinify API Key** field and click **Save**.

### 3. Install Context Menu
Click **Install Context Menu**. The `TinyRC` option will now appear when right-clicking image files.

### 4. Compress Images
- Right-click any `.png`, `.jpg`, `.jpeg`, or `.webp` file.
- Click **TinyRC**.
- The image will be compressed in-place and a notification will display the percentage saved.

---

## Building from Source

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- Windows 10 / 11

### Build Single-File Executable
```powershell
cd TinyRightClick
dotnet publish -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true -o publish
```

The output executable will be located in `TinyRightClick/publish/TinyRightClick.exe`.

---

## License
MIT License.
