#include "Button.h"
#include "Display.h"

// BUTTON STATE
bool lastButtonReading = HIGH;
bool buttonState = HIGH;

unsigned long lastButtonChangeTime = 0;
unsigned long buttonPressTime = 0;

bool longPressTriggered = false;
uint32_t clickCount = 0;


// INIT
void initButton()
{
  pinMode(ENCODER_SW,INPUT_PULLUP);

}



// HANDLE BUTTON
void handleButton()
{
  bool reading = digitalRead(ENCODER_SW);


  // DEBOUNCE
  if (reading != lastButtonReading)
  {
    lastButtonChangeTime = millis();

    lastButtonReading = reading;
  }


  if (millis() - lastButtonChangeTime < BUTTON_DEBOUNCE_MS)
  {
    return;
  }


  
  // STATE CHANGE
  if (reading != buttonState)
  {
    buttonState = reading;


    // BUTTON PRESSED
    if (buttonState == LOW)
    {
      buttonPressTime = millis();

      longPressTriggered =false;
    }
    else  // BUTTON RELEASED
    {
      unsigned long pressDuration = millis() - buttonPressTime;

      // SHORT PRESS
      if (!longPressTriggered && pressDuration < BUTTON_LONG_PRESS_MS)
      {
        clickCount++;

        startActionAnimation();

        Serial.println("EVENT:BUTTON_CLICK");
      }

      Serial.println("EVENT:BUTTON_RELEASE");
    }
  }


  
  // LONG PRESS
  if (buttonState == LOW && !longPressTriggered)
  {
    unsigned long pressDuration = millis() - buttonPressTime;

    if (pressDuration > BUTTON_LONG_PRESS_MS)
    {
      longPressTriggered = true;


      Serial.println("EVENT:BUTTON_LONG_PRESS");
    }
  }
}