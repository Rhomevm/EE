Place a compatible `ddraw.dll` (e.g., DDrawCompat) here if you want the launcher to optionally install it in the game folder.

We do NOT include the DLL in this repository. Download from the project's official source and place the `ddraw.dll` file into this folder before running the launcher with the DDraw option enabled.

Purpose:
- Reduce stutter
- Improve frame pacing
- Help with compatibility on modern Windows

Installation behavior by the launcher:
- On enable: backup existing `ddraw.dll` to `ddraw.dll.bak` (if present) and copy the file into the game folder.
- On disable: restore backup if present, or delete the installed `ddraw.dll`.
