ValheimCompletionistStub/Checklist/readme.txt

The plan for the Checklist:

------------------------------------------------------------

Player Load:
    scan player inventory

Player Item Interactions:
    scan player inventory
        Certain event happens:
        → scan the player's inventory
        → compare inventory item prefab names against items.csv
        → mark matching checklist entries complete
        → save progress

Bosses:
    Kill boss
    → Check global key
    → mark complete
    
Enemy Kills:
    Kill enemy
    → Compare enemy name against enemies.csv
    → mark matching checklist entries complete
    → save progress

