# Interactive Fiction Engine

A Unity-based engine for creating text-based interactive fiction games. The engine provides a flexible object system, action framework, and game control architecture.

## Core Objects

**BaseObject** is the foundation for all game entities. Every object in the world (rooms, items, characters) inherits from `BaseObject`. It provides:

- **Identity**: `mainName` and `aliases` for player references
- **Appearance**: `initialAppearance` text shown when examined
- **Sensory properties**: visibility, scent, taste, sound
- **Properties system**: attachable traits that define behavior
- **Action customization**: text responses, restrictions, overrides, and after-effects

Objects are Unity ScriptableObjects, authored in the Inspector and referenced at runtime.

**Action** is the abstract base class for all player verbs. Actions define what players can do in the game world. Each action specifies:
- A keyword and aliases (e.g., "take", "get", "pick up")
- Whether it requires an item, accepts one optionally, or doesn't use items
- Validation logic for whether it can apply to a specific object
- Execution logic that returns an `ActionStatus` (SUCCESSFUL, FAILED, INEFFECTIVE, RESTRICTED)

**Action Responses** allow objects to customize behavior when actions are performed on them:
- **Text Responses**: Simple text messages displayed when an action is performed (e.g., "The door creaks open")
- **Action Response Logic**: Custom ScriptableObject behaviors that execute after a successful action, enabling complex side effects like moving items, changing object states, or triggering events
- **Action Overrides**: Replace the default action implementation with a custom one for specific objects
- **Action Restrictions**: Block actions with custom messages (e.g., "The door is locked")


## Properties

Properties are reusable traits attached to objects. They come in three types:

- **BaseObjectProperty**: Abstract base for all properties
- **BooleanProperty**: Simple true/false states (e.g., `Lockable`, `Openable`, `Lightable`, `FixedInPlace`)
- **FunctionalProperty**: Rich state with custom fields (e.g., `HoldsContents`, `Writable`, `Aliveness`)

Properties are queried at runtime via `GetProperty<T>()` or `HasProperty<T>()`. They can be added/removed programmatically or authored in the Inspector.

**Usage**: Attach `HoldsContents` to make an object a container, `Lockable` to make it lockable, etc. Properties define what objects *can do* rather than what they *are*.

## Kinds

Kinds are specialized `BaseObject` subclasses for common game entity types:

- **Room**: Game spaces with exits and visit tracking
- **Creature**: Living actors (player/NPC) with gender-based pronouns
- **Container**: Objects that hold other objects
- **Exit**: Connections between rooms, optionally with doors
- **Door**: Lockable/openable barriers on exits
- **Supporter**: Surfaces that hold items (tables, shelves)
- **Scenery**: Background objects that can't be interacted with

Kinds provide convenience properties and auto-initialize default properties. For example, `Room` automatically gets `FixedInPlace`, `HoldsContents`, and `Lightable`.

## Registered Actions

Actions define player verbs. Each action:

- Declares a `Keyword` and `Aliases` (e.g., "take", "get", "pick up")
- Specifies `ItemApplicabilityLevel` (NA, OPTIONAL, REQUIRED)
- Implements `CanApplyToItem()` for item validation
- Implements `Execute()` which returns `ActionStatus` (SUCCESSFUL, FAILED, INEFFECTIVE, RESTRICTED)

**RegisteredActions** provides the core action set: `Attack`, `Close`, `Examine`, `Go`, `Listen`, `Lock`, `Look`, `Open`, `Smell`, `Take`, `Unlock`, `Inventory`.

**Custom actions** can be added via the Orchestrator's `customActions` list. Actions are registered with `ActionsManager` which handles keyword resolution and restrictions.

## Control Centre

The Control Centre manages game state and command execution:

- **GameController**: Central coordinator that processes player commands, executes actions, and emits events
- **ObjectsManager**: Tracks all objects, maintains room/inventory collections, handles object movement
- **ActionsManager**: Manages action registration, keyword lookup, and global restrictions
- **CommandParser**: Parses natural language input into structured commands

**Flow**: Player input → Parser → GameController → Action execution → Event generation → UI display

The controller validates objects at startup and throws early if configuration is invalid, making bugs easy to catch in testing.

## Orchestrator

The **Orchestrator** is a Unity MonoBehaviour that bridges the engine with Unity's UI system. It:

- Initializes the `GameController` with rooms, characters, and actions
- Handles player input from a text field
- Displays game events with typing effects
- Manages UI state (scrolling, input focus)

Configure it in the Unity Inspector by assigning starting room, all rooms, player character, NPCs, and custom actions.

## Tests

The test suite covers:

- **Runtime tests**: Action behavior, object properties, control centre logic, integration flows
- **Editor tests**: Compiler validation
- **Test helpers**: `TestObjectFactory` for creating test objects

Tests use NUnit and are organized by component (Actions, ControlCentre, CoreObjects, Properties, etc.). Run via Unity Test Runner.

