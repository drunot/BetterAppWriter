# Changelog

## Version 0.3.0

### Features
- Now the user can choose in the `Write Settings` menu weather they prefer to have the prediction window below or above the cursor.

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
