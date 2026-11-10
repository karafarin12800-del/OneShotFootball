# Runtime Character Prefabs

The game looks up optional prefabs through Unity Resources. Put these prefabs in this folder:

- Kicker.prefab
- ProForward.prefab
- StreetLegend.prefab
- Goalkeeper.prefab

Required runtime resource keys are Characters/Kicker, Characters/ProForward, Characters/StreetLegend and Characters/Goalkeeper.

Each prefab should use a Humanoid Avatar with a valid Animator. Keep root scale at one and local pivot at ground level. The kicker should face +Z; the goalkeeper prefab is rotated 180 degrees by the current loader. Keep Animator root motion disabled: the game owns the root transform and shot physics.

Until real prefabs are added, the game uses its existing procedural fallback characters. No third-party model files are included in this folder yet.
