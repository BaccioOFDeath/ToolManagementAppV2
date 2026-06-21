# User Context Signed-Out Contract

## Completed

- Added focused regression coverage for `ApplicationUserContext` after the switch-user cancellation repair.
- Locked the signed-out display contract so missing current users expose blank user name and role values instead of a normal-looking fallback identity.
- Guarded the remaining role behavior so ordinary users with blank roles still display `User`, while admins still display `Admin`.

## Why it matters

The switch-user flow now shuts down when login is cancelled, and the shared user context should not present that transient signed-out state as a valid operator. These tests keep the authentication display contract explicit without adding another theme or visual customization layer.
