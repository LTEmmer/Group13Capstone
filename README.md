# Group13Capstone Flashcard Roguelike

UWM Capstone project: a 3D roguelike game that integrates flashcards into gameplay. Players can import or create flashcard sets and use them to fight enemies, progress through dungeon floors, and improve learning through gameplay.

---

# Download & Installation

## Releases
Download the latest build here:  
https://github.com/LTEmmer/Group13Capstone/releases

Choose the correct version for your system.

---

## Windows Installation

1. Download the Windows ZIP file from the latest release.
2. Extract the ZIP file.
3. Open the extracted folder.
4. Run the `.exe` file to start the game.

If Windows shows a security warning:
- Click **More Info**
- Click **Run Anyway**

---

## macOS Installation

1. Download the macOS ZIP file from the latest release.
2. Extract the ZIP file.
3. Open the extracted folder.
4. Run the `.app` file.

### If macOS blocks the app

macOS Gatekeeper may block the app because it is not notarized.

To open it:

**Method 1**
- Right click the app in Finder
- Click **Open**
- Click **Open** again in the confirmation dialog

**Method 2**
- Open System Settings
- Go to Security and Privacy
- Allow the app under General

If issues persist, use the Windows version if available.

---

## Godot Editor (Optional)

If the executable does not work:

1. Download Godot (.NET version): https://godotengine.org/download/
2. Clone or download the project:
   - git clone https://github.com/LTEmmer/Group13Capstone.git
3. Open Godot
4. Click **Import**
5. Select `project.godot`
6. Press **F5** to run

Main scene (if needed):
Project → Project Settings → Application → Run → `main_menu.tscn`

---

# Project Overview

Flashcard Roguelike is a dungeon crawler where:
- Players fight enemies using flashcards
- Users can import CSV flashcard sets
- Players can create custom flashcard sets in-game
- Difficulty increases as the player progresses deeper into the dungeon
- Players can exit and view stats after runs

---

# Controls
(Subject to change)

- WASD - Move
- Mouse - Look
- E - Interact
- Left Shift - Sprint (if applicable)
- ESC - Menu / Pause

---

# Developers

- Ademar Gamero
- Alexander Rui Xing Tong
- Lawrence Taro Emmer
- Logan Allen Duane Watson
- Mohsin Shah
- Nicholas David Tassone

---

# Version Control Conventions

- Commits: https://www.conventionalcommits.org/en/v1.0.0/
- Branch naming: https://graphite.com/guides/git-branch-naming-conventions
- Godot C# style guide: https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_style_guide.html

---

# Peer Testing Instructions

## Team Information

Group Name: Flashcard Roguelike  
Preferred Communication: Email (CC all members)

Emails:
- ltemmer@uwm.edu
- lawatson@uwm.edu
- aegamero@uwm.edu
- ntassone@uwm.edu
- artong@uwm.edu
- mohsin@uwm.edu

---

## System Overview

Flashcard Roguelike is a 3D roguelike game that uses flashcards as its core combat system. Players explore dungeon floors, fight enemies using flashcards, collect loot, and progress through increasingly difficult levels. Players can import CSV flashcard sets or create their own in-game. Runs end when the player exits or is defeated, and stats are shown after each run.

---

## Access Instructions

- Repository: https://github.com/LTEmmer/Group13Capstone.git  
- Latest builds: https://github.com/LTEmmer/Group13Capstone/releases  

---

## Required Test Scenarios

### 1. Import Flashcard CSV
- Open "View Imported Flashcards"
- Upload provided CSV
- Import and verify it appears in list
- Enable set and restart game
- Confirm persistence

### 2. Create Flashcard Set
- Open "View Imported Flashcards"
- Create new set
- Add cards (question + answer)
- Save set
- Enable and restart game
- Confirm persistence

### 3. Dungeon Progression
- Start game
- Progress through rooms
- Clear combat rooms to unlock paths
- Reach boss or exit room
- Confirm no softlocks

---

## Optional Test Scenarios

### Boss Fight
- Reach boss room
- Answer flashcards correctly to deal damage
- Defeat boss
- Verify victory screen and progression

### Equipment System
- Collect and equip items
- Fill multiple equipment slots
- Verify effects apply correctly

### Branch Exploration
- Enter optional green paths
- Clear combat rooms
- Collect treasure
- Return to main path

### Stress Testing
- Rapid inputs
- Invalid CSV imports
- Item spam
- Death / respawn behavior
- Mid-run menu exit and re-entry

---