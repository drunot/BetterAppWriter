# Better AppWriter

**NB: WHEN INSTALLING DO NOT OVERWRITE ANY ORIGINAL FILES. READ THE INSTALL INSTRUCTIONS!**

This is a addon to [Wizkids AppWriter](https://www.wizkids.dk/downloads/) that adds additonal functionality to the application.

It is currently a very early build and does only add very basic functionality.

What this app currently does add:

- Additonal keyboard shortcuts that are configurable
- Oxford Advanced Learners dictionary lookups when language set to english (Only avalible online.)
- Better detection of predictions window position. (This could use even more work though)

## Installation

**NB: WHEN INSTALLING DO NOT OVERWRITE ANY ORIGINAL FILES. READ THE INSTALL INSTRUCTIONS!**

When installing this these steps needs to be followed:

- Rename the file in `/<InstallPath>/Lib/nlp.dll` to `/<InstallPath>/Lib/real_nlp.dll`
- Copy files from the install zip to the install path. After this everything should work as expected.
- To check if everything is working check if the `Better AppWriter Verison` is present in the menu to the right.

## How to use

The dictonary button is used in english just like it is used in danish. Mark the text you want to look up and the dictionary will look up that word.

Setting the keyboard shortcuts is done by clicking on the `Better AppWriter Settings` button, and then clicking on the A-ikon.

The other button in the `Better AppWriter Settings` is a terminal for developers. If any issuses is found with this addon this output of this could be usefull when subitting a issue.

## Planed features

This is the currently planed features. This list is not final and features might be added and/or removed. Nor is a timeline for when a feature will be impleneted given:

- Rebinding of exiting shortcuts
- Addition shortcuts e.g. increasing/decreasing the reading speed.
- Option to have the system ignore all inputs that are assigned to a shortcut in AppWriter.
- Opetion for the writing prediction bar to prefering beeing on top of the curor instead of on buttom.
- Adding the Better AppWriter settings to the toolbar menu.
- Translation support for all aditional texts added by the addon so they follow the selected application language. (I'll only be able to do Danish and English myself)

## Building

Right now the building process is not very organised:

- `nlp_loader` is build by a CMake and will generate `nlp.dll`.
- `sharp_injector` holds `sharp_injector.sln` and will build `BetterAW.dll`, `sharp_injector.dll`, `DictionaryHandler.dll`, and `WinAPIHooks.dll`. It also uses `Newtonsoft.Json.dll`, `0Harmony.dll` and `System.Windows.Interactivity.dll`, the latter two beeing NuGet packages. (Named `Lib.Harmony` and `Expression.Blend.Sdk` respectively) All build file from the sharp_injector solution will end up in the sharp_injector project build folder. Right now only the debug build of all C# projects have been tested.

### Placement of builded files

- `nlp.dll` should be placed in `/<InstallPath>/Lib/` **NOT** overwriting the original. (The original should be renamed `real_nlp.dll`)
- `System.Windows.Interactivity.dll` should be placed in `/<InstallPath>/`.
- The rest of the dll files should be placed in should be placed in `/<InstallPath>/Lib/`.

## Licence

This product is under the GPL 3.0 Licence. Microsoft.Xaml.Behaviors, Lib.Harmony, Newtonsoft.Json and WarpDLL is under the MIT Licence.
