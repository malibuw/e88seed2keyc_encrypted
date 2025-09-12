
# e88seed2keyc_encrypted

>This tool extracts and uses GM's internal `sa015bcr` seed-to-key algorithm from the `gmkeylib.dll` to generate valid security keys for unlocking ** E88 ECUs (Engine Control Modules). It supports decryption of seeds and returns matching keys for authentication over OBD2.
> Reverse engineered `keygen.exe` to extract E88 ECU key generation system.

## Currently

- Algorithm from binary blob (`sa015bcr`)
- Fully working DLL call interface
- Accepts 4-byte hexadecimal seed
- Returns valid unlock key for `E88` modules
- Pure native tooling (Mono + C#)

## Structure

```yaml
e88seed2keyc_encrypted/
├── ai.md # - Explanation for model.
│
├── bin/ # - Binary directory containing compiled or external resources.
│   └── gmkeylib.dll # — Native DLL likely containing the seed-key algorithm implementations.
├── gmkeylib.dll # - Root-level duplicate of the same DLL, maybe for convenience.
│
├── gmkeylib.lic # - License file required by gmkeylib.dll for functionality.
│
├── gmseedkeyalgs.md # - Markdown file listing known seed-key algorithms and observations.
│
├── LicGen.cs # - C# file for generating .lic license files compatible with the DLL.
│
├── src/ # - C# source code for interacting with the native DLL and performing seed-key calculations:
│   ├── Crypto.cs # — Contains cryptographic routines, possibly reverse-engineered.
│   ├── gmkey.cs # — Main logic for seed-key conversion and algorithm selection.
│   ├── Helpers.cs # — Utility functions (e.g., hex conversions, validation).
│   └── LibLoader.cs # — Dynamically loads gmkeylib.dll and invokes its methods.
└── TestHarness.cs # - Test driver to demonstrate usage, pass seeds, and receive computed keys.
```

## Run

### 1. Install Mono 

```bash
sudo pacman -S mono  # or sudo apt install mono-complete
```
2. Compile the Executable
```bash
cd FINISHED/src
```
```bash
mcs -out:../bin/LicGen.exe *.cs
```
3. Run It With a Seed
```bash
cd ../bin
```
```bash
mono LicGen.exe 0x12345678
```
Example Output
Input Seed: `12345678` → Generated Key: `ABCD1234`

## Supported Algorithms
This tool specifically supports algorithm 93 (internally named `sa015bcr`), which is known to work for many ECUs including E88.
