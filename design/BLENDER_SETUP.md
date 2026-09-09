# Blender Setup Guide for LAST-GOD Lab Canvas

## Step 1: Install Blender

Download from: https://www.blender.org/download/
- Recommended: **Blender 4.2 LTS** (most stable for Grease Pencil work)
- Install to default path: `C:\Program Files\Blender Foundation\Blender 4.2\`

## Step 2: Run the Lab Canvas Script

1. Open Blender
2. Click the **Scripting** tab at the top of the Blender window
3. Click **Open** in the text editor panel
4. Navigate to: `c:\projects\LAST-GOD\design\LabCanvas.py`
5. Press **Run Script** (▶ button or Alt+P)

The script will:
- Configure the canvas (480×160, 12fps, transparent background)
- Create 6 Grease Pencil layers with the lab colour palette
- Load `frame-1.png` and `frame-2.png` as tracing references
- Create 4 animated prop objects (monitor, flask, alarm, steam)
- Save `design/LabCanvas.blend` automatically

## Step 3: Connect Blender MCP (optional, for AI control)

If you want me (the AI) to draw directly in Blender:

1. Open Blender
2. Go to **Edit → Preferences → Add-ons**
3. Search "MCP" — if not found, install via:
   ```
   uvx blender-mcp install-addon
   ```
4. Enable the **Blender MCP** addon
5. The addon starts a server on port 9000
6. I can then control Blender directly via MCP tools

## Step 4: Draw the Background Layers

After the script runs, in Blender:

1. Select `LabBG` in the outliner
2. Switch to **Draw mode** (Tab key)
3. Select layer `BG_Far` in the GP layers panel
4. Use the **Fill** tool to block in dark background shapes
5. Trace loosely over `REF_frame-1.png` (the reference image at 30% opacity)
6. Repeat for `BG_Mid` (consoles, railings) and `FG_Silhouette` (pipes, foreground)

## Step 5: Export PNG Sequences

1. In Blender: **Render → Render Animation** (Ctrl+F12)
2. PNG sequences output to: `Assets/Art/Backgrounds/Lab_Animated/`
3. Unity auto-imports these when the Editor opens

## Colour Palette Reference

| Name | Hex | Use |
|---|---|---|
| Dark Wall | `#0a0a1a` | Far background walls |
| Steel Gray | `#2a2a3a` | Mid structures, columns |
| Cyan Glow | `#00d4ff` | Stasis tubes, holographics |
| Amber Warn | `#ff6600` | Warning lights, alerts |
| Monitor Green | `#00ff41` | Console screens |
| Shadow Black | `#050508` | Foreground silhouettes |

