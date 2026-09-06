#include "Communication.h"
#include "Config.h"
#include "Display.h"
#include "Actions.h"


String serialBuffer = "";
bool serialCommandOverflowed = false;


// INIT
void initCommunication()
{
  Serial.begin(SERIAL_BAUDRATE);
  serialBuffer.reserve(SERIAL_COMMAND_MAX_LENGTH);
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
