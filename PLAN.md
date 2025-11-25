## Disable Floor Adjuster
Add an option to disable the floor adjustment to maintain the avatar's body size (scale) regardless of footwear.

**Reason**:
When an avatar wears high heels, FloorAdjuster aligns the bottom of the heels to the floor, effectively increasing the ViewPoint height. If we scale this to a fixed target height, the body shrinks to compensate for the heels, which is unnatural compared to wearing sandals.
By disabling FloorAdjuster (or ignoring the height increase it causes), we can calculate the scale based on the body's inherent height. This allows the avatar to be naturally taller when wearing heels, while keeping the body proportions (scale) identical to the version without heels.

## Separate Scaler Object
Add a menu item to create a child GameObject named "AvatarScaler" under the selected avatar root and attach the `ViewPointScaler` component to it.

- **Menu Path**: `GameObject/VRChat Utility/Add ViewPoint Scaler Child` (Context menu on Hierarchy)
- **Behavior**:
    1. Create a new empty GameObject named "AvatarScaler" as a child of the selected object.
    2. Add `ViewPointScaler` component to the new object.
    3. Ensure the component is not added to the root if this method is used.

**Reason**:
Separating the scaler component into a child object allows users to easily copy and paste the scaling settings (the GameObject itself) between different avatars in the project.