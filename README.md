# Better AppWriter

**NB: WHEN INSTALLING DO NOT OVERWRITE ANY ORIGINAL FILES. READ THE INSTALL INSTRUCTIONS!**

This is an add-on to [Wizkids AppWriter](https://www.wizkids.dk/downloads/) that adds additional functionality to the application.

It is currently an early build.

What this app currently does add:

- Additional keyboard shortcuts that are configurable
- Oxford Advanced Learners dictionary lookups when the language is set to English (Only available online.)
- Better detection of predictions window position. (This could use even more work though)

## Installation

**NB: WHEN INSTALLING DO NOT OVERWRITE ANY ORIGINAL FILES. READ THE INSTALL INSTRUCTIONS!**

When installing this these steps need to be followed:

- Rename the file in `/<InstallPath>/Lib/nlp.dll` to `/<InstallPath>/Lib/real_nlp.dll`
- Copy files from the install zip to the install path. After this everything should work as expected.
- To check if everything is working check if the `Better AppWriter Version` is present in the menu to the right.

## How to use

The dictionary button is used in English just like it is used in Danish. Mark the text you want to look up and the dictionary will look up that word.

Setting the keyboard shortcuts is done by clicking on the `Better AppWriter Settings` button and then clicking on the A-ikon.

The other button in the `Better AppWriter Settings` is a terminal for developers. If any issues are found with this addon this output of this could be useful when submitting an issue.

## Planned features

These are the currently planned features. This list is not final, and features might be added and/or removed. Nor is a timeline for when a feature will be implemented given:

- Additional shortcuts e.g., for increasing/decreasing the reading speed.
- Translation support for all additional texts added by the addon so they follow the selected application language. (I'll only be able to do Danish and English myself)

## Building

Since CMake still does not support COM references in C# project this project uses a custom build pipeline, where some of the projects are from CMake and others comes from a Visual Studio 2022 solution.

To build the project run:

```powerShell
$ ./make.bat
```

For more information run:

```powerShell
$ ./make.bat Help
```

It is a primitive build system. If Visual Studio 2022 is not installed then it will try and use the newest version, but this is untested.

### Placement of built files

- `nlp.dll` should be placed in `/<InstallPath>/Lib/` **NOT** overwriting the original. (The original should be renamed `real_nlp.dll`)
- `Microsoft.Xaml.Behaviors.dll` should be placed in `/<InstallPath>/`.
- The rest of the DLL files should be placed in `/<InstallPath>/Lib/`.

## Licence

This product is released under the GPL 3.0 License. Microsoft.Xaml.Behaviors, Lib.Harmony, Newtonsoft.Json, and WarpDLL are under the MIT License. The program AppWriter that this is an add-on to, has a commercial license and is owned and distributed by Wizkids A/S.
