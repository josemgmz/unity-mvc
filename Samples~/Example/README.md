# UnityMVC Examples

Example usage of UnityMVC across multiple scenarios. Import this sample to explore scene setups and scripts demonstrating different controller/view/model interactions.

## Available Examples

### 1. Base MVC Example (`1_Example_Base_MVC`)

Demonstrates the fundamental MVC pattern using a **Circle** GameObject. Shows basic separation of concerns:
- **Model**: Stores data (rotation speed, pulse parameters, colors, movement settings)
- **View**: Connects Model and Controller to the GameObject (no logic)
- **Controller**: Handles all behavior (rotation, pulsing scale, horizontal movement)

Ideal starting point for understanding UnityMVC architecture.

### 2. Events + MVC Example (`2_Example_Events_MVC`)

Demonstrates Unity event handling within MVC using a **Triangle** GameObject. Shows:
- **Unity Events**: Triggers, collisions, and input handling in Controllers
- **2D Physics**: `OnTriggerEnter2D/Exit2D` and `OnCollisionEnter2D/Exit2D` events
- **State Management**: Runtime flags tracking interaction state
- **Input**: Space key for jump mechanics

Perfect for understanding how Unity's event system integrates with the MVC pattern.
