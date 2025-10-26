# 🎙️🌾 AIDay: An AI-Remake 

> 3rd Place at Supercell Global AI Game Hackathon, *AI-Remake* Category.

<center><img src="/Assets/Resources/image.png" height=500></center>


## 🚀 The Project

This project is a proof-of-concept that reimagines classic mobile farming games (like *HayDay*) with an accessibility-first approach. Many mobile games rely heavily on precise and repetitive touch interactions, which can be a barrier for players with physical disabilities.

Our solution is an **AI-Remake** that uses natural voice commands to perform all key game actions. The player can simply speak, and an AI model parses their intent, translating it directly into game logic.

### Hackathon Challenge
* **Challenge:** **`AI-Remake`**
*  **Prompt:** "Reimagine a classic game or mechanic with Al, your creation should be something that couldn't exist without Al."   
* **Our Fit:** This project fits perfectly, as it uses generative AI's natural language understanding (NLU) to create a new, voice-native control scheme that simply couldn't exist with traditional game mechanics.

## ✨ Features

* **Natural Language Commands:** No rigid keywords! Say "Plant 3 pumpkins" or "Can you harvest carrots?" and the AI understands.
* **Core Game Actions:**
    * **Plant:** Plant different crops (wheat, corn, carrots, pumpkins) on specific plots.
    * **Harvest:** Harvest a specific plot, which automatically adds the item to your inventory.
    * **Bake:** Use your harvested crops to bake food.
* **Real-time UI:** The on-screen UI updates instantly to show the contents of your inventory.

---

### Demo Video
   
[![Watch the video](https://i9.ytimg.com/vi/NisjA_rUI50/mqdefault.jpg?v=68fe43db&sqp=CNSI-ccG&rs=AOn4CLAN8bUJrpXPkpukA61FoOHIC9lFWw)](https://www.youtube.com/watch?v=NisjA_rUI50)

---

## 🛠️ How It Works (Architecture)

 This project's "magic" comes from connecting a game engine (Unity) to a powerful conversational AI platform (Neocortex), which was one of the tools provided for the hackathon.

1.  **Voice Input (Player):** The player speaks a command (e.g., "Plant 5 carrots") into their microphone.
2.  **NLU (Neocortex):** The audio is streamed to the **Neocortex** platform. Neocortex's AI parses the audio and identifies:
    * **The Intent (Action):** `PlantCrop`
    * **The Entities (Details):** ` [{"crop_name": "carrot"}`, `{"quantity": "5"}]`
3.  **Command (SDK):** Neocortex sends this structured data (the intent and its entities) back to our game via its Unity SDK.
4.  **Game Logic (Unity):**
    * A `VoiceCommandHandler.cs` script receives the event from the SDK.
    * It safely parses the entities (e.g., converting "carrot" into the corresponding CropData object).
    * It calls the appropriate function on our singleton `FarmManager.cs`.
    * `FarmManager.cs` updates the game state (e.g., changing the plot's color, updating the `inventory` dictionary) and refreshes the UI text.

---

## 💻 Technology Used

* **Game Engine:** **Unity** (to create a 3D mock-up of the game environment)
* **Voice & NLU Platform:** **Neocortex**
    * This was a hackathon-provided tool.
    * We used the Neocortex web platform to define our `PlantCrop`, `HarvestCrop` and `BakeBread` actions and train the AI on sample "intents"
    * We used the Neocortex Unity SDK to connect our game to the AI in real-time[cite: 148, 156].
* **Language:** **C#**

---

## 🔧 How to Run

1.  **Clone Repository:**
    ```bash
    git clone https://github.com/lucamazzza/AIDay.git
    ```
2.  **Open in Unity:** Open the project in Unity Hub (tested with `Unity 6000.2.4f1`).
3.  **Install Neocortex SDK:**
    * Go to `Window > Package Manager`.
    * Click the `+` icon and select `Add package from git URL...`.
    * Paste the SDK Git URL from the Neocortex documentation: `https://github.com/neocortex-link/neocortex-unity-sdk`
4.  **Configure Neocortex:**
    *  Create a [Neocortex account](https://neocortex.link/).
    *  Generate an API Key in the Neocortex dashboard.
    *  Add the file `prompt.txt` into the knowledge tab.
5.  **Link to Unity:**
    * In the Unity top menu bar, navigate to Tools > Neocortex > API Key Setup.
    * Paste your generated API Key into the window that appears and save.
    * In the Unity AIDay scene, find the `NeocortexHandler` GameObject.
    * On its `Character` component, paste the **Project ID**: cmh7dt2bt0001l404pepwl4hi
6.  **Run:**
    * Press **Play** in the Unity Editor.
    * Allow microphone access when prompted.
    * Start speaking commands!

---

## 💡 Future Ideas

With more time, we would love to:
* **Expand Commands:** Add logic for feeding animals, selling items from the inventory, and crafting other goods.
* **Add Seed Inventory:** Modify `PlantCrop` to check if the player *has* a "seed" in their inventory before allowing them to plant.
* **Query Commands:** Implement "read" commands like "What's in my inventory?" or "What's growing on plot 3?"

---

## 🧑‍💻 The Team

* **Luca Mazza**: [GitHub](https://github.com/lucamazzza) | [LinkedIn](https://linkedin.com/in/lucamazzza)
* **Giada Galdiolo**: [GitHub](https://github.com/giadagaldiolo) | [LinkedIn](https://linkedin.com/in/giadagaldiolo)
* **Andre Peiti Ocampo**: [GitHub](https://github.com/linkswitch19) | [LinkedIn](https://www.linkedin.com/in/andre-peiti-ocampo-87a1a9313/) | [Youtube](https://www.youtube.com/@linkswitch19/)

*Thank you for judging our project!*
