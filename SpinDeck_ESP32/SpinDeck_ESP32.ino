#include "Config.h"

#include "Display.h"
#include "Encoder.h"
#include "Button.h"
#include "Actions.h"
#include "Communication.h"


// =====================================================
// SETUP
// =====================================================

void setup()
{
  initCommunication();

  initDisplay();

  initEncoder();

  initButton();

  initActions();

  Serial.println("DEVICE:READY");
}


// =====================================================
// LOOP
// =====================================================

void loop()
{
  handleEncoder();

  handleButton();

  handleSerialInput();

  updateActionAnimation();

  delay(1);
}