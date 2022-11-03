# Changelog

## Version 0.5.1

### Bugfixes

- Fixed various bugs with the prediction window and keyboard shortcuts.
- Updated injected code to work with changes made to AppWriter in version 2.22.0.

## Version 0.5.0

### Features

- Now the AppWriter Toolbar now moves up or down if the cursor is underneath it.
- The program now supports multiple languages. It defaults to English but if the language that is selected as the display language in appWriter exits, then it uses that language instead.
- A miscellaneous program for creation and editing translations is created.
- Danish translation added.

## Version 0.4.0

### Features

- The prediction window settings under `Write Settings` is revamped.
- The keyboard shortcuts is completely rewritten, and all build-in shortcuts are now available in settings.
  - Fixes known bug: When using some AppWriter built in shortcuts, the key up event is not detected by the better AppWriter shortcut system.
- Changed build system to make it easier to build the whole project.
- Bundled most `*.dll`s into one single `*.dll` to make installation cleaner.

### Bugfixes

- There was some additional problems with the prediction window position, when the main screen does not have 100% scaling.
- Due to problems with the git ignore earlier versions could not build. If you want to build earlier versions include the folder `sharp_injector\sharp_injector\sharp_injector\Debug` from this version in the earlier version.

## Version 0.3.0

### Features

- Now the user can choose in the `Write Settings` menu whether they prefer to have the prediction window below or above the cursor.

### Bugfixes

- The prediction window position calculation is completely rewritten to fix problems with multiple screens with different scaling.

## Version 0.2.1

### Bugfixes

- Predictions window position now works better with multiple screens when using different scaling.

## Version 0.2.0

### Features

- Changed from .net Framework 4.0 to 4.8. (Apparently the version injected can differ from the version the host app is running which is 4.6)
- Enable keyboard shortcuts for languages toggling.
- Better detection of predictions window position. (This could use even more work though)

### Bugfixes

- Fixed a bug where only Write setting shortcuts was saved persistently.
- Fixed a bug where build number was printed as MajorRevision. (Bug not visible in x.x.0 versions).

### Known bugs

- When using some AppWriter built in shortcuts, the key up event is not detected by the better AppWriter shortcut system.

### Other

- Changelog added.

## Version 0.1.0

### Features

- Additonal keyboard shortcuts that are configurable
- Oxford Advanced Learners dictionary lookups when language set to english (Only avalible online.)
