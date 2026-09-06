#include "Communication.h"
#include "Config.h"
#include "Display.h"
#include "Actions.h"


String serialBuffer = "";
bool serialCommandOverflowed = false;
bool hostConnected = false;
unsigned long lastHostActivityTime = 0;


// INIT
void initCommunication()
{
  Serial.begin(SERIAL_BAUDRATE);
  serialBuffer.reserve(SERIAL_COMMAND_MAX_LENGTH);
}


void updateHostConnectionStatus()
{
  if (hostConnected && millis() - lastHostActivityTime >= HOST_CONNECTION_TIMEOUT_MS)
  {
    hostConnected = false;
    setDeviceDisplayStatus(DEVICE_STATUS_IDLE);
  }
}

// SERIAL INPUT
void handleSerialInput()
{
  while (Serial.available() > 0)
  {
    char c = Serial.read();

    if (c == '\n')
    {
      if (serialCommandOverflowed)
      {
        serialBuffer = "";
        serialCommandOverflowed = false;
        Serial.println("ERROR:COMMAND_TOO_LONG");

        continue;
      }

      serialBuffer.trim();

      if (serialBuffer.length() > 0)
      {
        String command = serialBuffer;

        serialBuffer = "";

        handleCommand(command);
      }
      else
      {
        serialBuffer = "";
      }
    }
    else if (c != '\r')
    {
      if (!serialCommandOverflowed)
      {
        if (serialBuffer.length() < SERIAL_COMMAND_MAX_LENGTH)
        {
          serialBuffer += c;
        }
        else
        {
          serialBuffer = "";
          serialCommandOverflowed = true;
        }
      }
    }
  }
}


void handleCommand(const String& command)
{
  if (command == "DEVICE:STATUS|DISCONNECTED")
  {
    hostConnected = false;
    setDeviceDisplayStatus(DEVICE_STATUS_DISCONNECTED);

    return;
  }

  hostConnected = true;
  lastHostActivityTime = millis();
  setDeviceDisplayStatus(DEVICE_STATUS_READY);

    if (command == "PING")
    {
        Serial.println("PONG");
    }
    else if (command == "SCREEN:CLEAR")
    {
        clearScreen();
        Serial.println("OK:SCREEN:CLEAR");
    }
    else if (command == "SCREEN:TEST")
    {
        showTestScreen();
        Serial.println("OK:SCREEN:TEST");
    }
    else if (command == "ACTION:CLEAR")
    {
        clearActions();
        Serial.println("OK:ACTION:CLEAR");
    }
    else if (command == "DEVICE:STATUS|SUCCESS")
    {
      setDeviceDisplayStatus(DEVICE_STATUS_SUCCESS);
      Serial.println("OK:DEVICE:STATUS");
    }
    else if (command == "DEVICE:STATUS|ERROR")
    {
      setDeviceDisplayStatus(DEVICE_STATUS_ERROR);
      Serial.println("OK:DEVICE:STATUS");
    }
    else if (command.startsWith("ACTION:SET|"))
    {
        setAction(command);
    }
    else if (command.startsWith("ACTION:SELECT|"))
    {
        selectAction(command);
    }
    else
    {
        Serial.print("ERROR:UNKNOWN_COMMAND:");
        Serial.println(command);
    }
}
