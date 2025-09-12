
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
├── bin/
│ └── gmkeylib.dll # Decompiled GM DLL with embedded algorithm
├── src/
 ├── gmkey.cs # Main interface
 ├── Crypto.cs # Decryption logic
 ├── Helpers.cs # Seed/key helpers
 ├── LibLoader.cs # DLL loader wrapper
 └── LicGen.cs # Executable to run the algorithm
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
