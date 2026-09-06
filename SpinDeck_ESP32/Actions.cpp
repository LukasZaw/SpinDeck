#include "Actions.h"
#include "Display.h"


// ACTION DATA
DeviceAction actions[MAX_ACTIONS];

int actionCount = 0;
int selectedAction = -1;


// INIT
void initActions()
{
  clearActions();
}


// CLEAR ALL ACTIONS
void clearActions()
{
  actionCount = 0;
  selectedAction = -1;


  for (int i = 0; i < MAX_ACTIONS; i++)
  {
    actions[i].id = -1;
    actions[i].name = "";
    actions[i].type = "";
    actions[i].value = "";
  }


  displayNoAction();
}



// SET ACTION

// Format:
// ACTION:SET|0|Google|Browser|https://google.com
void setAction(String command)
{
  // "ACTION:SET|" = 11 chars

  String data = command.substring(11);


  // SEPARATOR 1
  int separator1 = data.indexOf('|');

  if (separator1 == -1)
  {
    Serial.println("ERROR:INVALID_ACTION_SET");

    return;
  }


  
  // SEPARATOR 2
  int separator2 =data.indexOf('|', separator1 + 1);

  if (separator2 == -1)
  {
    Serial.println("ERROR:INVALID_ACTION_SET");

    return;
  }


  // SEPARATOR 3
  int separator3 = data.indexOf('|', separator2 + 1);

  if (separator3 == -1)
  {
    Serial.println("ERROR:INVALID_ACTION_SET");

    return;
  }


  // PARSE
  String indexString = data.substring(0, separator1);

  String name = data.substring(separator1 + 1, separator2);

  String type = data.substring(separator2 + 1, separator3);


  String value =data.substring(separator3 + 1 );


  int index =indexString.toInt();


  // VALIDATE INDEX
  if (index < 0 || index >= MAX_ACTIONS)
  {
    Serial.println("ERROR:ACTION_INDEX_OUT_OF_RANGE");

    return;
  }

  
  // SAVE ACTION
  actions[index].id = index;
  actions[index].name = name;
  actions[index].type = type;
  actions[index].value = value;


  // UPDATE ACTION COUNT
  if ( index >= actionCount)
  {
    actionCount = index + 1;
  }


  // RESPONSE
  Serial.print("OK:ACTION:SET|");

  Serial.println(index);
}



// SELECT ACTION

// Format:
// ACTION:SELECT|0
void selectAction(String command)
{
  // "ACTION:SELECT|" = 14 chars
  String indexString = command.substring(14);

  int index = indexString.toInt();


  // NO ACTION
  if (index == -1)
  {
    selectedAction = -1;

    displayNoAction();

    Serial.println("OK:ACTION:SELECT|-1");

    return;
  }

  
  // VALIDATE
  if (index < 0 || index >= actionCount)
  {
    Serial.println("ERROR:ACTION_INDEX_OUT_OF_RANGE");

    return;
  }

  // SELECT
  selectedAction = index;

  refreshActionDisplay();

  // RESPONSE
  Serial.print("OK:ACTION:SELECT|");

  Serial.println(selectedAction);
}


// REFRESH DISPLAY
void refreshActionDisplay()
{
  if (selectedAction < 0 || selectedAction >= actionCount)
  {
    displayNoAction();

    return;
  }


  displayAction(actions[selectedAction].name, actions[selectedAction].type, selectedAction,actionCount);
}