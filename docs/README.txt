Empire Earth LAN Launcher

Steps for players:
1) Install ZeroTier from https://www.zerotier.com/download/
2) Join network: f3797ba7a8ab2c2b (Private network, IPv4 auto-assign enabled)
3) Wait for authorization in ZeroTier Central (network admin must authorize you)
4) In the launcher, locate your EmpireEarth.exe (one time)
5) Always use LAN in-game (Host: Create LAN game; Join: Refresh LAN list)
6) Everyone must use the same game version (Patch 1.0.4.0)

Notes:
- The launcher does NOT modify EmpireEarth.exe.
- The launcher optionally installs DDrawCompat (ddraw.dll) into the game folder; existing ddraw.dll is backed up as ddraw.dll.bak.
- The launcher will attempt to set the ZeroTier adapter's network profile to Private and add firewall rules (requires Admin).
- Logs are in %AppData%\EE-LAN-Launcher\logs\
