# Test Plan

Scenarios to validate:

1. ZeroTier not installed
   - Launcher should show blocking state and message: "ZeroTier is required for LAN multiplayer." and provide button "Install ZeroTier" that opens the site.

2. ZeroTier installed but not joined
   - Launcher should indicate installed but not joined
   - Allow user to click Recheck/Join (UI guides)

3. ZeroTier installed but waiting for authorization
   - Launcher should show: "Waiting for network authorization. Ask the host to approve your device in ZeroTier Central." and not allow play until online.

4. Authorized and online (IPv4 assigned)
   - Launcher displays: "LAN network active. Your LAN IP: 10.x.x.x"
   - Copy IP button works
   - PLAY validates and launches game

5. No admin rights
   - Launcher must still run and show warnings that firewall/profile steps require Administrator
   - PLAY should ask to continue without firewall/profile changes

6. Admin rights
   - Launcher should attempt to set profile to Private and add firewall rules for `EmpireEarth.exe` on Private profile
   - Rules should be idempotent

7. Multiple network adapters
   - Ensure ZeroTier IPv4 is detected even when many adapters are present

8. DDraw handling
   - Install: backup existing `ddraw.dll` and copy provided one
   - Disable: restore backup or remove installed dll

9. Windows 10 and 11
   - Validate UAC and PowerShell behavior

Logs
- Validate logs are written to `%AppData%\EE-LAN-Launcher\logs\` containing zerotier-cli output and actions.

Manual tests
- Verify user is shown the Play confirmation modal (can be disabled via checkbox)
- Verify SHA-256 is computed and can be copied
