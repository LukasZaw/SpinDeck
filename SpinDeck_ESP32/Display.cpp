#include "Display.h"
#include "Actions.h"

// TFT OBJECT
Adafruit_ST7735 tft =
  Adafruit_ST7735(
    TFT_CS,
    TFT_DC,
    TFT_RST
  );

bool actionAnimationActive = false;
unsigned long actionAnimationStartTime = 0;
uint8_t actionAnimationFrame = 0;


// INIT
void initDisplay()
{
  tft.initR(INITR_MINI160x80);

  tft.setRotation(1);

  tft.fillScreen(ST77XX_BLACK);
}


// ACTION EXECUTION ANIMATION
void startActionAnimation()
{
  actionAnimationActive = true;
  actionAnimationStartTime = millis();
  actionAnimationFrame = 0;

  clearScreen();
  tft.setTextColor(ST77XX_WHITE);
  tft.setTextSize(2);
  tft.setCursor(17, 10);
  tft.println("Executing");
}


void updateActionAnimation()
{
  if (!actionAnimationActive)
  {
    return;
  }

  if (millis() - actionAnimationStartTime >= 1000)
  {
    actionAnimationActive = false;
    refreshActionDisplay();

    return;
  }

  static unsigned long lastFrameTime = 0;

  if (millis() - lastFrameTime < 90)
  {
    return;
  }

  lastFrameTime = millis();

  tft.fillRect(52, 38, 56, 24, ST77XX_BLACK);
  tft.drawCircle(80, 50, 10, ST77XX_WHITE);

  const int frameX = 80 + ((actionAnimationFrame % 8) * 7) - 24;
  tft.fillCircle(frameX, 50, 3, ST77XX_WHITE);

  actionAnimationFrame++;
}


// CLEAR
void clearScreen()
{
  tft.fillScreen(ST77XX_BLACK);
}


// DEFAULT SCREEN
void showDefaultScreen()
{
  clearScreen();

  tft.setTextColor(ST77XX_WHITE);
  tft.setTextSize(2);
  tft.setCursor(20, 15);
  tft.println("SpinDeck");

  tft.drawLine(10, 40, 149, 40,ST77XX_WHITE);

  tft.setTextSize(1);
  tft.setCursor(35, 52);
  tft.println("Ready");

  tft.setCursor(20, 68);
  tft.println("Connected to PC");
}


// TEST SCREEN
void showTestScreen()
{
  clearScreen();

  tft.setTextColor(ST77XX_WHITE);

  tft.setTextSize(2);
  tft.setCursor(10, 25);
  tft.println("PC OK");
}


// BROWSER ICON
void drawBrowserIcon(int x,int y)
{
  tft.drawCircle(
    x + 8,
    y + 8,
    7,
    ST77XX_WHITE
  );

  tft.drawLine(
    x + 8,
    y + 1,
    x + 8,
    y + 15,
    ST77XX_WHITE
  );

  tft.drawLine(
    x + 1,
    y + 8,
    x + 15,
    y + 8,
    ST77XX_WHITE
  );

  tft.drawLine(
    x + 4,
    y + 3,
    x + 12,
    y + 3,
    ST77XX_WHITE
  );

  tft.drawLine(
    x + 4,
    y + 13,
    x + 12,
    y + 13,
    ST77XX_WHITE
  );
}


// APPLICATION ICON
void drawApplicationIcon(int x, int y)
{
  tft.drawRoundRect(
    x,
    y,
    16,
    16,
    2,
    ST77XX_WHITE
  );

  tft.drawLine(
    x + 1,
    y + 4,
    x + 14,
    y + 4,
    ST77XX_WHITE
  );

  tft.fillCircle(
    x + 3,
    y + 2,
    1,
    ST77XX_WHITE
  );

  tft.fillCircle(
    x + 6,
    y + 2,
    1,
    ST77XX_WHITE
  );

  tft.fillCircle(
    x + 9,
    y + 2,
    1,
    ST77XX_WHITE
  );

  tft.drawRect(
    x + 4,
    y + 7,
    8,
    6,
    ST77XX_WHITE
  );
}



// NO ACTION
void displayNoAction()
{
  showDefaultScreen();
}

// DEFAULT SCREEN
void showScreen()
{
  showDefaultScreen();
}

// DISPLAY ACTION
void displayAction(const String& name, const String& type, int selectedIndex, int actionCount)
{
  actionAnimationActive = false;
  clearScreen();

  // HEADER
  tft.setTextColor(ST77XX_WHITE);
  tft.setTextSize(1);
  tft.setCursor(5, 5);
  tft.println("SPINDECK");

  tft.drawLine(0, 18, 159, 18, ST77XX_WHITE);


  // ACTION NAME
  tft.setTextSize(2);
  tft.setCursor(5, 27);

  if (name.length() <= 12)
  {
    tft.println(name);
  }
  else
  {
    String line1 = name.substring(0, 12);

    String line2 =name.substring(12);


    if (line2.length() > 12)
    {
      line2 = line2.substring(0, 12);
    }

    tft.println(line1);
    tft.setCursor(5, 46);

    tft.println(line2);
  }


  
  // ACTION ICON
  if (type == "Browser")
  {
    drawBrowserIcon(5, 63);
  }
  else if (type == "Application")
  {
    drawApplicationIcon(5, 63);
  }

  // ACTION NUMBER
  tft.setTextSize(1);
  tft.setCursor(125,67);
  tft.print(selectedIndex + 1);

  tft.print("/");
  tft.print(actionCount);
}
