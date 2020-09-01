# MangosMod
A relatively simple mod for the game Risk of Rain 2.

# Dependencies
- R2API (https://github.com/risk-of-thunder/R2API)
- BepInEx (https://github.com/BepInEx/BepInEx)

# Commands
- **mm** (lists information about the mod)
- **mmhelp** (lists all commands)
- **mmitems** *{name/id} {amount} {player(optional)}* (gives items to a player)
- **mmitemslist** (lists all items in the game)
- **mmtime** *{value}* (modifies the time scale depending on the integer given 0=pause, 1=normal)
- **mm$** *{value} {player(optional)}* (gives money to a player)
- **mmspawnas** *{body} {player(optional)}* (changes the player's character body)
- **mmequip** *{name/id} {player(optional)}* (gives equipment to a player)
- **mmequiplist** (lists available equipment in the game)

# TODO
- update **mmspawnlist** to contain new enemies and characters from the 1.0 update
- convert **mmspawnlist** into a function that lists all loaded bodies instead; should allow modded characters as well
