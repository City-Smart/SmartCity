# Progetto SmartCity

Questa repository contiene un progetto [**Unity**](https://unity.com/) che utilizza [**ML-Agents**](https://github.com/Unity-Technologies/ml-agents) (PPO) per addestrare un agente veicolare in una città appartenente al pacchetto di [SyntyStudios](https://syntystore.com/products/polygon-city-pack?srsltid=AfmBOoriNL1QoajRkkuC9Q3fZ1ubTe5owXqXCU2zCQ6T2U9Z6xrwHYkY) POLYGON - City Pack. Per quanto riguarda l'ostacolo è stato scelto un gatto di [Ladymito](https://assetstore.unity.com/packages/3d/characters/animals/mammals/free-chibi-cat-165490?srsltid=AfmBOop323Qj-8yxdLdo4QwAnP8-4AoLVmTT0-cbJUNVrprYdqGoFHt3). L'agente deve perseguire quattro obiettivi principali:

1. **Schivare gatti**
2. **Seguire la segnaletica orizzontale**
3. **Mantenere la corsia di sinistra**
4. **Effettuare parcheggi** nel punto di interesse più vicino

---
## 1. Requisiti e versioni

- [**Unity**](https://unity.com/): 2022.3.9f1
- [**ML-Agents**](https://github.com/Unity-Technologies/ml-agents): 1.1.0
- [SyntyStudios](https://syntystore.com/products/polygon-city-pack?srsltid=AfmBOoriNL1QoajRkkuC9Q3fZ1ubTe5owXqXCU2zCQ6T2U9Z6xrwHYkY): POLYGON - City Pack
- [**Python**](https://www.python.org/): 3.10.12
- [Conda](https://docs.conda.io/projects/conda/en/stable/user-guide/install/index.html): 24.11.3 o superiore
- [**Git**](https://git-scm.com/downloads)
## 2. Struttura del Progetto

```
SmartCity/
├── Assets/
│   ├── Scenes/
│   │   └── Smart-City.unity
│   ├── Scripts/
│   │   ├── CarAgent.cs
│   │   └── ...
│	├── results/
│   │   ├── 57CarRun/
│   │       └── CarBehavior.onnx
│   │   └── ...
│   ├── config/
│   │   └── ppo/
│   │       └── carBehavior.yaml
│   └── ...
├── .gitignore
└── README.md
```
## 3. Installazione e Setup

### 3.1 Clonare il repository

```bash
git clone https://github.com/City-Smart/SmartCity.git
cd SmartCity
```
### 3.2 Configurare l'ambiente Python

Crea un virtual environment:

```bash
    conda create --name mlagents --file requirements.txt
```
### 3.3 Importare il progetto in Unity

1. Avvia Unity Hub e seleziona **Add**
2. Punta alla cartella `SmartCity`
3. Apri il progetto con Unity 2022.3.9f1
4. Verifica che il pacchetto **ML-Agents** (v1.1.0) sia presente in **Window > Package Manager**
## 4. Configurazione Unity

1. Apri la scena `Assets/Scenes/CityScene.unity`
2. Controlla che lo script `CarAgent.cs` sia attaccato al GameObject **CarAgent** e che:
    - Gli **Observations** includano raycast per ostacoli (gatti, veicoli), rilevamento strada e target.
    - Le **Actions** permettano controllo della sterzata, accelerazione e frenata.
## 5. Training con ML-Agents

1. Verifica che il file `config/ppo/carBehavior.yaml` sia costruito come segue:
    ```yaml
	behaviors:
	  CarBehavior:
	    trainer_type: ppo
	    hyperparameters:
	      batch_size: 128
	      buffer_size: 1024
	      learning_rate: 3e-4
	      beta: 2e-3
	      epsilon: 0.2
	      lambd: 0.95
	      num_epoch: 4
	      learning_rate_schedule: linear
	      beta_schedule: constant
	      epsilon_schedule: linear
	    network_settings:  
	      normalize: true
	      hidden_units: 128
	      num_layers: 2
	    reward_signals:
	      extrinsic:
	        gamma: 0.99
	        strength: 1.0
	    max_steps: 2000000
	    time_horizon: 32
	    summary_freq: 5000
    ```
2. Lancia il processo di training dalla cartella Assets:
    ```bash
    mlagents-learn config/ppo/carBehavior.yaml --run-id=57CarRun --resume
    ```
3. Monitora le statistiche via TensorBoard:
    ```bash
    tensorboard --logdir=results
    ```
4. Al termine del training, il modello addestrato sarà salvato in `results/57CarRun`.
## 6. Valutazione e inferenza

1. Carica il modello addestrato in Unity:
    - In **Behavior Parameters** del `CarAgent`, imposta **Model** sul file `.onnx` generato situato in `Assets/results/57CarRun/CarBehavior.onxx`.
2. Esegui la scena in Unity:
    - L'agente guiderà seguendo il modello caricato.
3. Verifica i quattro obiettivi:
    - Nessuna collisione con gatti
    - Segnali rispettati
    - Posizionamento nella corsia sinistra
    - Parcheggio nel POI più vicino
## 7. Risoluzione dei Problemi

- **Il training diverge o le ricompense sono costantemente basse**
    - Controlla i parametri di learning_rate e buffer_size
    - Aumenta il numero di passi massimi (max_steps)
- **L’agente rimane fermo**: Verifica che gli input di azione siano collegati correttamente in Behavior Parameters
- **Collisioni frequenti**: Aumenta la penalità per collisione o regola il raggio dei raycast
## 8. Galleria

<p align="center">
<img src="https://raw.githubusercontent.com/City-Smart/SmartCity/refs/heads/main/Images/ParcheggioPizzeria.jpg">
</p>
<p align="center">
<img src="https://raw.githubusercontent.com/City-Smart/SmartCity/refs/heads/main/Images/ParcheggioPizzeriaRaycasts.jpg">
</p>
---

Creato da [Chiara Puglia](https://github.com/chiarapuglia99) e [Luigi Giacchetti](https://github.com/Rankoll), Giugno 2025
