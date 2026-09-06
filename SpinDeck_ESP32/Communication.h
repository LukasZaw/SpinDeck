#ifndef COMMUNICATION_H
#define COMMUNICATION_H

#include <Arduino.h>

void initCommunication();
void handleSerialInput();
void handleCommand(const String& command);
void updateHostConnectionStatus();

#endif