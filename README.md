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
cd DRAM-Simulator
