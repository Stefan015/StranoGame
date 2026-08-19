# StranoGame

A real-time, procedurally generated 3D world built in **Unity** (C#), focused on
atmosphere and visual aesthetics. Terrain is generated endlessly from noise and rendered
with custom shaders.

> Personal project exploring procedural generation and shader programming.

## Screenshots
<img width="1903" height="865" alt="strano1" src="https://github.com/user-attachments/assets/94ae1b28-ec07-4066-ad3c-bfc612b46275" />
<img width="1893" height="879" alt="strano2" src="https://github.com/user-attachments/assets/780ad011-9456-47ab-9e54-93658de5ed30" />
<img width="1905" height="865" alt="strano3" src="https://github.com/user-attachments/assets/248a9174-1f11-40b2-89b4-e550627ad487" />

## Features

- **Procedural terrain generation** driven by noise (`FastNoiseLite`).
- **Endless / streaming terrain** — chunks generate around the player as they move (`EndlessTerrain`).
- **Custom cuboid terrain meshing** (`CuboidGenerator`).
- **Custom shaders** — a per-object cuboid shader and a fullscreen (post-process) shader for atmosphere.
- **Free-look player movement and camera** controls.

## Tech Stack

- **Engine:** Unity `6000.1.14f1` (Unity 6.1)
- **Language:** C#
- **Graphics:** custom `.shader` files (ShaderLab / HLSL)
