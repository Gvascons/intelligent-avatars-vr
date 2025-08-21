# Avataredu: Intelligent Avatars in Virtual Reality Environments

This is the official repository for **Avataredu**, an open-source framework for creating intelligent, historically themed avatars in virtual reality (VR) environments, as presented in our paper.

Contributors: João Gabriel Vasconcelos, Maria Eduarda Mota, Glenda Malta, Cleber Zanchettin, Germano Vasconcelos, Francisco Simões

Avataredu features an end-to-end architecture that combines on-device AI inference with the **Ubiq** multiplayer VR stack. Our prototype instantiates an Avatar powered by a local Large Language Model (LLM) served via **Ollama**, **Whisper** speech-to-text, and a lightweight text-to-speech (TTS) engine, running entirely on commodity hardware. To validate its applicability, we conducted a quantitative cross-persona analysis using avatars such as **Marie Curie**, **Malala Yousafzai**, and **Martin Luther King Jr**, which revealed low error rates (mean of ~9.3%) and demonstrated that persona swap requires minimal additional computational power while maintaining answer quality.

This repository contains:
* The full source code, including **Unity** project files and Python backend scripts.
* Detailed instructions for replication.
* The dataset of interaction logs generated during our tests.

  
---

## 🛠 Installation

### 1. Clone the repository

```bash
git clone https://github.com/Gvascons/intelligent-avatars-vr
```

### 2. Open the project in Unity

Download and install [Unity Hub](https://unity.com/).

### 3. Create and open the project

- Open Unity Hub
- Click on "New Project"
- Choose the **3D (Core)** template
- Name it (e.g., `IntelligentAvatarsVR`)
- Choose the location to save the project
- Click **Create**

---

## 📦 Unity Setup

### 1. Add Required Unity Packages

#### 🔹 Add Ubiq

1. Open **Window > Package Manager**
2. Click the **`+`** in the top-left → **Add package from git URL...**
3. Paste:
   ```
   https://github.com/UCL-VR/ubiq.git#upm
   ```
4. Click **Add**

#### 🔹 Add glTF Utility (for .glb avatars)

Same steps, but use:

```
https://github.com/Siccity/GLTFUtility.git
```

This allows Unity to import `.glb` avatars downloaded from ReadyPlayerMe.

### 2. Import the Ubiq Demo

In **Package Manager**:
- Select `Ubiq` in the left panel
- Go to the **Samples** tab
- Click **Import** next to **Demo (XRI)**

### 3. Open and Run the Demo Scene

- In the **Project** tab, navigate to:
  `Assets > Samples > Ubiq > [version] > Demo (XRI)`
- Double-click on `Demo.unity`
- Click **Play** to test the scene

---

## 👤 Avatar Creation

### 1. Ready Player Me

- Go to [https://readyplayer.me](https://readyplayer.me)
- Create an avatar (Full Body recommended)
- Download it as `.glb` (for Unity)

### 2. Import into Unity

- Drag the `.glb` file into the `Assets` folder
- Wait for Unity to import it
- Drag the avatar into the **Hierarchy**
- Set **Position** to `(0, 1, 0)`
- If too small, scale to `(10, 10, 10)`

---

## 🎙 Add Scripts

### 1. Add `LipSyncFromAudioClip.cs` and `AskMarie.cs`

- Drag them into the `Assets` folder **or**
- Right-click → Create → C# Script and paste the code

Unity will auto-compile the scripts.

### 2. Attach to GameObject

- Create an **Empty GameObject** (right-click in Hierarchy → Create Empty)
- Rename to `AvatarAssistant`

With `AvatarAssistant` selected:

- Add **Audio Source** component
- Add **LipSyncFromAudioClip** and **AskMarie** scripts

### 3. Fill Script Fields

#### LipSyncFromAudioClip:

- `Face Mesh`: drag the `Wolf3D_Head` from your avatar
- `Mouth Blend Shape Index`: 0
- `Audio Source`: assign the same AudioSource

#### AskMarie:

- `Audio Source`: assign the same AudioSource
- `Server Url` → write http://localhost:5000/speech-to-text-and-respond

> ⚠️ `recordAction` is now **automatically bound to R** in code — no need to assign Input Actions manually.

---

## 🧠 Backend Setup

### 1. Python Requirements

Create and activate a virtual environment (i'd recommend *uv*), then run:

```bash
pip install -r requirements.txt
```

> Includes: `flask`, `pyttsx3`, `soundfile` and `numpy`
> You must install `whisper` via git package (e.g. pip install git+https://github.com/openai/whisper.git)
> In order to install torch and ffmeg, refer to the end of this document.

Make sure [ffmpeg](https://ffmpeg.org/download.html) is installed and available in your system `PATH`.

### 2. Run the server

```bash
python llm_server.py
```

> This will:
> - Transcribe voice using Whisper
> - Generate response using `llama3` via Ollama
> - Convert response to speech with pyttsx3
> - Serve audio for playback in Unity

> ✅ Ollama and the model (`llama3`) must be installed. Run `ollama list` to confirm.

---

## ▶️ Running Everything

1. Start your Python server:
   ```bash
   python llm_server.py
   ```

2. In Unity, click **Play**.

3. Press **R** to start recording.

4. The avatar will:
   - Record your audio
   - Send it to the server
   - Receive a spoken response back
   - Animate lips using blend shapes

Enjoy your intelligent virtual avatar! 🎧🧠✨


---

## ⚠️ Additional Setup Notes

### 🔥 Installing PyTorch (torch) Correctly

Do **not** install torch with just `pip install torch`.

Instead, visit the [official PyTorch installation page](https://pytorch.org/get-started/locally/) and choose the correct command for your environment.

Examples:

- **CPU-only (no GPU)**:

```bash
pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cpu
```

- **GPU (CUDA 11.8)**:

```bash
pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu118
```

⚠️ Make sure the versions match your system’s Python version and CUDA setup.

---

### 🎬 Installing ffmpeg (Required by Whisper)

Whisper depends on `ffmpeg` to load and convert audio. You must install it and ensure it is in your system’s `PATH`.

#### ✅ Option A: Using Chocolatey (Windows)

```bash
choco install ffmpeg
```

Then confirm it's available:

```bash
ffmpeg -version
```

You can also install in your venv via (on Windows) via:

```bash
winget install ffmpeg
```

#### ✅ Option B: Manual Installation

1. Go to [https://ffmpeg.org/download.html](https://ffmpeg.org/download.html)
2. Download a Windows static build and extract it (e.g., to `C:\ffmpeg`)
3. Add `C:\ffmpeg\bin` to your **System Environment PATH**
4. Restart your terminal or PC

---

After completing these steps, everything should run smoothly. 🎯
