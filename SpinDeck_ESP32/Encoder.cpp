#include "Encoder.h"

// ENCODER STATE
int encoderPosition = 0;

uint8_t lastEncoderState;

int8_t encoderAccumulator = 0;

// ENCODER TABLE
const int8_t encoderTable[16] =
{
   0, -1,  1,  0,
   1,  0,  0, -1,
  -1,  0,  0,  1,
   0,  1, -1,  0
};

// INIT
void initEncoder()
{
  pinMode(ENCODER_CLK,INPUT_PULLUP);

  pinMode(ENCODER_DT,INPUT_PULLUP);

  lastEncoderState = (digitalRead(ENCODER_CLK) << 1) | digitalRead(ENCODER_DT);
}


// HANDLE ENCODER
void handleEncoder()
{
  uint8_t currentState = (digitalRead(ENCODER_CLK) << 1) | digitalRead(ENCODER_DT);


  if (currentState != lastEncoderState)
  {
    uint8_t index = (lastEncoderState << 2) | currentState;

    int8_t movement = encoderTable[index];

    encoderAccumulator += movement;

    lastEncoderState = currentState;


  
    // RIGHT
    if (encoderAccumulator >= 4)
    {
      encoderPosition++;
      encoderAccumulator = 0;

      Serial.println("EVENT:ENCODER_RIGHT");
    }


    // LEFT
    else if (encoderAccumulator <= -4)
    {
      encoderPosition--;
      encoderAccumulator = 0;

      Serial.println("EVENT:ENCODER_LEFT");
    }
  }
}