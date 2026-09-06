#ifndef DISPLAY_H
#define DISPLAY_H

#include <Arduino.h>
#include <Adafruit_GFX.h>
#include <Adafruit_ST7735.h>

#include "Config.h"

// TFT
extern Adafruit_ST7735 tft;

enum DeviceDisplayStatus
{
  DEVICE_STATUS_IDLE,
  DEVICE_STATUS_DISCONNECTED,
  DEVICE_STATUS_READY,
  DEVICE_STATUS_WAITING,
  DEVICE_STATUS_SUCCESS,
  DEVICE_STATUS_ERROR
};

void setDeviceDisplayStatus(DeviceDisplayStatus status);



// INIT
void initDisplay();



// SCREENS
void showDefaultScreen();

void displayNoAction();

void displayAction(
  const String& name,
  const String& type,
  int selectedIndex,
  int actionCount
);

void startActionAnimation();

void updateActionAnimation();



// BASIC DISPLAY
void clearScreen();

void showTestScreen();



// ICONS
void drawBrowserIcon(
  int x,
  int y
);

void drawApplicationIcon(
  int x,
  int y
);

#endif