# Levada

## How to Clone

Follow these steps to clone the repository and get started:

### 1. Install DVC
If not already installed, download and install DVC from the [official DVC site](https://dvc.org/).

### 2. Clone the Repository

To clone the full repository:
```bash
git clone git@github.com:NeuroRehabilitation/Levada.git
```

Or to clone a specific branch:
```bash
git clone --branch <branch-name> --single-branch git@github.com:NeuroRehabilitation/Levada.git
```

### 3. Setup Google Drive Acess

1. Download ServiceAccount.json from [Neurorehab Shared Folder](https://drive.google.com/drive/folders/1n0E52KD4CWqNhatr6TcmdkqLG7lrINBe?usp=sharing) 
2. Copy the Downloaded file to **path/to/LevadaCloneRepo/.dvc**

### 4. Pull Large Files

1. Open your command prompt (CMD) or (PowerShel)
2. Navigate to the cloned repository location:
   ```bash
   cd path/to/LevadaCloneRepo
   ```
3. Pull the DVC-tracked files:
   ```bash
   dvc pull
   ```
   
   If the progress doesn't show, use verbose mode:
   ```bash
   dvc pull -v
   ```

### Troubleshooting

**Windows Path Length Issue**

If you encounter errors related to file paths, it's likely due to Windows' default 260-character path limit. To fix this:

1. Press `Win + R` and type `regedit` to open the Registry Editor
2. Navigate to: `HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\FileSystem`
3. Locate the DWORD value `LongPathsEnabled`
4. Set its value to `1`
5. Restart your computer for the changes to take effect

---

**Need Help?** If you continue experiencing issues, please check the repository's Issues page or contact the maintainers.
