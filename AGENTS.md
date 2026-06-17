# AGENTS.md

## Project Rules

This is a Unity 2D learning project. The goal is to improve architecture, maintainability, and performance without breaking existing gameplay.

## Refactor Rules

- Do not make large-scale rewrites unless explicitly requested.
- Prefer small, incremental, reviewable changes.
- Preserve existing gameplay behavior unless the user approves behavior changes.
- Before modifying files, explain the planned changes and affected files.
- Avoid over-engineering. Use architecture patterns that are suitable for a beginner-to-intermediate Unity project.
- Do not introduce unnecessary third-party packages.
- Do not rename public serialized fields lightly because Unity scene and prefab references may break.
- Be careful when moving scripts because Unity meta files and serialized references may be affected.
- Keep MonoBehaviour lifecycle methods clear and minimal.
- Avoid circular references between gameplay, UI, and manager classes.
- Prefer event-driven communication or interface-based dependencies where appropriate.
- UI should separate View display/input from business logic using MVP, MVC, or a simplified variant.
- Use object pooling for frequently spawned temporary objects.
- Avoid renderer.material unless a unique material instance is intentionally required.
- Avoid expensive calls such as FindObjectOfType, GameObject.Find, or repeated GetComponent in Update.
- Always mention how to verify the change inside Unity Editor.

## Review Guidelines

When reviewing code, prioritize:

1. Bugs or behavior regressions
2. Broken Unity references
3. Lifecycle and initialization order issues
4. Circular dependencies and tight coupling
5. Memory leaks from events, timers, coroutines, or object pools
6. Runtime allocations and performance issues
7. UI architecture problems
8. Maintainability and extensibility issues