#include "Communication.h"
#include "Display.h"
#include "Actions.h"


// INIT
void initCommunication()
{
  Serial.begin(SERIAL_BAUDRATE);

}


String serialBuffer = "";

// SERIAL INPUT
void handleSerialInput()
{
  while (Serial.available() > 0)
  {
    char c = Serial.read();

    if (c == '\n')
    {
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
      serialBuffer += c;
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
