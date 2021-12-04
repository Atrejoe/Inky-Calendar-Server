# Inky-Calender server Changelog 

## Feature summary (December 2021) ##

### Features ###
- True Google calendar integration🎉 Adding Google calenders is now a matter of a few clicks.
- New York Times front-page panel, a real looker!
- Added some activity logging for panels (Date of creatiom, last modification, last access and the number of times displayed) ([e883c2f9](https://github.com/Atrejoe/Inky-Calendar-Server/commit/e883c2f9de7019ac1490251cedf70be1e9e70bc0))

## Features and fixes (November 2020) #

### Feature
- Included [Changelog](/changelog) in the server UI.
- Implemented starring of panels ⭐. Starred panels are listed first too. ([16cda669](https://github.com/Atrejoe/Inky-Calendar-Server/commit/16cda669a918f1f96e8d9264a00cb638b8abb7fa))

### UI
- Some text is more humanly pluralized ([3ae45516](https://github.com/Atrejoe/Inky-Calendar-Server/commit/3ae45516ebbf07199ba9b42e82fa6de37addac99))
- Added simpler way of removing a panel. Instead of dragging,you can just click the trash can ♻ . ([3ae45516](https://github.com/Atrejoe/Inky-Calendar-Server/commit/3ae45516ebbf07199ba9b42e82fa6de37addac99))
- Switched font in calendar panel. This font can show more accented characters and does not cause issues for unknown characters. Still does not support languages like 中文, عربى or اللغة العبرية. 
- Added explanation of relative panels heights. Allows more granular panel height configuration.

### Bugfixes
- Prevented errors in some calendar events with special characters.
- Recurring events are now properly calculated and displayed. Performance has been improved too.
## Code quality release (September 2020)

### Features

- Added internal error reporting
- Improvements to the calendar:
  - Better sorting
  - Improved display
  - Showed unique events only
  - Calendar deletion
  - Corrected time zones for UTC

## Winter is coming (May 2020)

### Features

- Added a basic weather panel

## Initial Release (March 2020)

### Features

- A UI for configuring panels for authenticated users

## Initial Release (Feb 2020)

### Features

First working setup.

Available panels:

- A weather panel - Shows the weather for a location

- A calendar (agenda) panel - Shows a unified agenda for multiple calendars

  

> This changelog was created using [CS.Changelog](https://github.com/cswebworks/CS.Changelog)