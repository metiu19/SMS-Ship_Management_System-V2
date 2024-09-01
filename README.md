# SMS-Ship_Management_System-V2

This is a script to simulate a more real expirence when interacting with some in-game blocks (game: Space Engineers).
This scripts aims to improve realism by making possible to simulate block's subsystems, instructions on how to use will be found in the first sections of the final script available on steam workshop (WIP).

## Dependencies

- [SE Scripts Utils](https://github.com/metiu19/SE-Scripts-Utils) (Included as submodule)

## How to build

1. Clone this repo with this command `git clone --recurse-submodules`
2. If you can't/didn't use the previous command then pull the submodule code with this command: `git submodule update --init --recursive`
4. Launch [Setup (run before opening solution).bat](Setup%20(run%20before%20opening%20solution).bat) and follow the instructions
5. Choose either Release build or Debug (Debug logs enabled)
6. Build project `[SMS] Ship Management System V2 Script`

## How to update the submodule

If a new version of the submodule will be required by this project then use the following command to pull both this repo and the submodule: `git pull --recurse-submodules`

---

# To-Do List

## First Relase
- [x] Streamline (As Much As Possible) and Manage INI Config
- [x] Add Block Tag support
- [ ] Multi Tick System Init
- [x] Develop Property Management
- [x] Develop Parse and Action Management by Topic
- [ ] Develop Storage Logger System

## UI Interface
- [ ] Develop INI Generator for Display Panel and Set
- [ ] Agree and develop communication standards between the two PBs
- [ ] Develop Parse and Action Management by IGC
- [ ] Receive Commands From Interface
- [ ] Response of Results to Interface

## Features
- [ ] Develop Subtypes System
- [ ] Develop Property with Numeric Value
- [ ] Develop Threshold Acceleration System
- [ ] Remote Tamper Management


# Design Board
The live Design Board can be found at this [link](https://www.tldraw.com/ro/dP5dSsz9P3M5qiZGDzCyl?v=-9,-20,1920,1052&p=JESMoZBO6S0kI4gtO4vbP)
