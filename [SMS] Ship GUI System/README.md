
# SMS-Ship_GUI_System

Questo scipt serve per aggiungere una interfaccia Bash Dialog oppure Sprite allo script [SMS-Ship_Management_System-V2](https://github.com/metiu19/SMS-Ship_Management_System-V2/tree/master) 
Lo scambio di informazioni avviene tramite IGC in modalita Brodcast per la sincronizzazione delle informazioni e Unicast per le singole azioni

## Dependencies

- [SE Scripts Utils](https://github.com/metiu19/SE-Scripts-Utils) (Included as submodule)

## How to build

1. Clone this repo with this command `git clone --recurse-submodules`
2. If you can't/didn't use the previous command then pull the submodule code with this command: `git submodule update --init --recursive`

## How to update the submodule

If a new version of the submodule will be required by this project then use the following command to pull both this repo and the submodule: `git pull --recurse-submodules`

---

# To-Do List

## UI Interface
- [ ] Develop INI Generator for Display Panel and Set
- [ ] Agree and develop communication standards between the two PBs
- [ ] Develop Parse and Action Management by IGC
- [ ] Receive Commands From Interface
- [ ] Response of Results to Interface

# Design Board
The live Design Board can be found at this [link](https://www.tldraw.com/ro/dP5dSsz9P3M5qiZGDzCyl?v=-9,-20,1920,1052&p=JESMoZBO6S0kI4gtO4vbP)
