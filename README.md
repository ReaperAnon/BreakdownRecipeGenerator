#Breakdown Recipe Generator
A simple patcher meant to generate breakdown recipes for all craftable items in a load order. It also provides the option of creating custom breakdown recipes for miscellaneous items, weapons and armors.

##Options
- Yield Percentage: The percentage of the original materials the recipe should give back. The resulting number is rounded down.
- Generate Recipe For Each Component: By default the patcher only makes a breakdown recipe for the "highest grade" materials in a given crafting recipe. With this option it will make one for every insignificant component, like leather strips.
- Recipes: A user-defined list of breakdown recipes. The usage should be self-explanatory, look at the miscellaneous section to get an idea.
- Excluded Crafting Stations: This is a list of keywords for every crafting station that has tempering recipes. Without this tempering recipes would also result in a breakdown recipe while they obviously shouldn't. If you have any mod-added crafting stations that shouldn't have recipes generated or are only for improving items, put them in here.
