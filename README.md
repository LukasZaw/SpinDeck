# SpinDeck

SpinDeck is an open-source Stream Deck-style controller built from an ESP32 device and a Windows desktop application.

The project currently contains two parts:

- `SpinDeck_ESP32` - Arduino firmware for the ESP32, TFT display, rotary encoder and push button.
- `SpinDeck_Win_app` - WPF desktop application for Windows, written in C# on .NET 8.

## Features

- Configure browser and Windows application actions.
- Select actions with the rotary encoder.
- Execute the selected action with a button press.
- Display the selected action on the ESP32 screen.
- Synchronize actions over USB serial communication.
- Save actions and application settings locally.
- Start with Windows and minimize to the system tray.

## Repository structure

```text
SpinDeck/
|-- SpinDeck_ESP32/
|-- SpinDeck_Win_app/
|-- .gitignore
`-- README.md
```

## Windows application

### Requirements

- Windows 10 or newer.
- .NET 8 SDK.
- Visual Studio 2022 or VS Code with the C# extension.

### Run

Open `SpinDeck_Win_app/SpinDeck_Win_app.sln` in Visual Studio and run the project, or use:

```powershell
dotnet restore SpinDeck_Win_app/SpinDeck_Win_app.csproj
dotnet run --project SpinDeck_Win_app/SpinDeck_Win_app.csproj
```

The application uses `115200` baud for serial communication. Select the ESP32 COM port in the Device view and connect.

## ESP32 firmware

### Requirements

- Arduino IDE 2.x or PlatformIO.
- ESP32 board support installed in the Arduino IDE.
- An ESP32 board with a compatible ST7735S TFT display.
- The display and input libraries used by the source code.

Open `SpinDeck_ESP32/SpinDeck_ESP32.ino`, select the correct ESP32 board and port, then upload the firmware.

The default serial speed is `115200`. Hardware pin assignments are documented in [`SpinDeck_ESP32/Config.h`](SpinDeck_ESP32/Config.h).

## Default hardware mapping

| Component | ESP32 pin |
| --- | ---: |
| TFT CS | GPIO 5 |
| TFT RST | GPIO 4 |
| TFT DC | GPIO 2 |
| Encoder CLK | GPIO 32 |
| Encoder DT | GPIO 33 |
| Encoder switch | GPIO 25 |

## First start

1. Flash the firmware to the ESP32.
2. Connect the ESP32 to Windows with USB.
3. Start the Windows application.
4. Open **Device**, select the COM port and connect.
5. Open **Actions** and create a browser or application action.
6. Turn the encoder to select an action and press it to execute the action.

## Serial protocol

The current protocol is line-based text over USB serial. The main commands are:

```text
PING
SCREEN:TEST
ACTION:CLEAR
ACTION:SET|index|name|type|value
ACTION:SELECT|index
```

The device reports readiness with `DEVICE:READY` and sends input events such as `EVENT:ENCODER_RIGHT`, `EVENT:ENCODER_LEFT` and `EVENT:BUTTON_CLICK`.

## Notes

- The ESP32 currently keeps synchronized actions in RAM. The Windows application is the source of the saved configuration and resynchronizes the device after reconnecting.
- The firmware supports up to 30 actions (`MAX_ACTIONS` in `Config.h`).
- Do not commit `bin`, `obj`, `.vs` or local user settings to the repository.

## License

No license has been selected yet. Add a `LICENSE` file before publishing the project for public reuse.
