# DRAM Architectural Simulator (C#)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Reproduction Status](https://img.shields.io/badge/Reproducibility-Verified-success)](https://github.com/Tesfamichael-G)

A high-performance, cycle-approximate C# simulation framework designed for modeling, configuring, and evaluating Dynamic Random-Access Memory (DRAM) architectures. This tool enables researchers and systems engineers to analyze the complex trade-offs between custom memory topologies, timing constraints, and power consumption profiles under realistic workloads.

---

## 🔬 Scientific & Architectural Features

Unlike high-level abstract models, this framework enforces strict structural hierarchies and hardware timing constraints:

* **Hierarchical Topology Modeling:** Explicit structural emulation of memory Channels, Ranks, Banks, Rows, and Columns.
* **Granular Timing Constraints:** Fully configurable JEDEC-standard timing parameters including $t_{RCD}$, $t_{RP}$, $t_{RAS}$, and $t_{CL}$.
* **Trace-Driven Evaluation:** Accepts standard memory access trace inputs to reproduce specific application workloads.
* **Multi-Dimensional Telemetry:** Outputs comprehensive execution analytics including cycle-by-cycle latency profiles, effective bandwidth/throughput, and estimated power consumption.

---

## 🚀 Quick Start & Installation

### Prerequisites
* [.NET SDK 8.0+](https://dotnet.microsoft.com/download) or higher.

### 1. Installation
Clone the repository and navigate to the project root:
```bash
git clone [https://github.com/Tesfamichael-G/DDR_Sharp-2.0.git](https://github.com/Tesfamichael-G/DDR_Sharp-2.0.git)
cd DDR_Sharp-2.0
```
### 2. Build the Project
Compile the simulation framework using the dotnet CLI:
```bash
dotnet build -c Release
```
### 3. Execution Example
Run a simulation by passing a configuration file and a workload trace:
```bash
dotnet run --project DRAMSimulator.CLI --config ./configs/ddr4_default.json --trace ./traces/sample_workload.txt
```
### Input Trace Format
The simulator expects a trace file where each line denotes an access operation using the following format:
```plaintext
# [Timestamp_Cycles] [Access_Type: R/W] [Hexadecimal_Memory_Address]
100 R 0x7FFF0000
142 W 0x7FFF0004
```
### Extensible Architecture
The framework is designed around dependency injection principles, allowing researchers to easily swap out or implement custom:
#### 1. Scheduling Policies: (e.g., FR-FCFS vs. FCFS memory controllers).
#### 1. Address Mapping Schemes: (e.g., RoBaRaCoCh vs. RoCoBaRaCh configurations).
#### 1. Power Models: Custom analytical power calculators based on state transitions.

### License & Open Science Commitment
This project is open-source and licensed under the Apache License — see the LICENSE file for details. We highly encourage extension, reproduction, and modification by the computer architecture community.

✍️ Citation / Contact
If you utilize this simulator or its architectural models in your academic research, please cite it as follows:
```code
@software{tesfamichael_dram_sim,
  author = {Gebrehiwot, T. G., Andargie, F. A. & Ismail, M.},
  title = {DDRSHARP: A Fast and Extensible DRAM Simulator.},
  doi = {https://doi.org/10.3844/jcssp.2023.836.846},
  year = {2023},
  journal = {Journal of Computer Science},
  voulume = {19},
  issue = {7},
  page = {836--846}
  url = {[https://github.com/Tesfamichael-G/DDR_Sharp-2.0](https://github.com/Tesfamichael-G/DDR_Sharp-2.0)}
}
```
