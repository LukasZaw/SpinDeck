#ifndef ACTIONS_H
#define ACTIONS_H

#include <Arduino.h>

#include "Config.h"

// ACTION STRUCTURE
struct DeviceAction
{
  int id;
  String name;
  String type;
  String value;
};


// ACTION DATA
extern DeviceAction actions[MAX_ACTIONS];

extern int actionCount;
extern int selectedAction;

// ACTION MANAGEMENT
void initActions();

void clearActions();

void setAction(String command);

void selectAction(String command);

// DISPLAY
void refreshActionDisplay();

#endif