# ADR 0003: Camera-Delta Pixel-Snapped Parallax System

## Status
Accepted

## Context
Multi-layered backgrounds in a retro 2D platformer can exhibit jitter, sub-pixel shearing, and misalignment if layers move independently with floating-point drift or desynchronize from the primary gameplay camera.

## Decision
Parallax movement is implemented in `LateUpdate` by computing camera position deltas (`targetCamera.transform.position - _previousCameraPosition`) and multiplying by a per-layer scalar `parallaxFactor`. Positions are snapped to the 16 PPU grid:
`snapped = Mathf.Round(pos * pixelsPerUnit) / pixelsPerUnit`.
Ground and gameplay physics surfaces remain static on the Midground layer (`parallaxFactor = 1.0` or strictly attached to physics geometry) so collision never shifts relative to the player.

## Consequences
- **Positive**: Perfectly stable depth perception without sub-pixel swimming; seamless compatibility with Unity's Pixel Perfect Camera.
- **Negative**: Extremely fast camera movement requires sufficient background canvas dimensions to avoid edge clipping.
