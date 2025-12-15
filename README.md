# StickAR

## Overview
StickAR is a cross-platform social experience connecting users through a virtual bulletin board. Whether via the immersive Mobile AR app or the accessible Web Dashboard, users can place customized Sticky Notes, leave personal messages, and discover hidden digital content in physical locations.

We aim to give underutilized areas around schools and communities a new digital purpose.

---

## New Features: Gamification & Immersion
We have expanded the StickAR ecosystem with new mechanics to reward exploration and interaction:

### Experience (XP) & Leveling
*   **Gain XP:** Earn experience points by posting notes, receiving likes, and discovering new rooms.
*   **Level Up:** As you gain XP, your **Explorer Level** increases, unlocking prestige badges on your profile.

### StickAR Tokens
*   **Currency:** Earn Tokens by completing Daily Tasks and maintaining login streaks.
*   **Shop:** Use tokens to unlock special **Note Skins** (e.g., Pink, Blue, Yellow, and rare animated skins).

### Audio & Atmosphere
*   **Sound Effects:** Satisfying UI sounds when posting, liking, or unlocking achievements.
*   **BGM:** Ambient background music that changes based on the "Room" you are currently exploring (e.g., quiet lo-fi for the Library, nature sounds for the Grass Patch).

### Daily Tasks
*   **Engage:** Specific goals such as "Like 5 posts" or "Post in 2 different rooms" to keep explorers active.

---

## StickAR Web (Browser Version)
The Web Client serves as a companion dashboard and a "Lite" version of the experience.

### Key Features
*   **Grid Map View:** Navigate rooms via a 2D grid interface.
*   **Dashboard:** View your Stats, XP, Token Count, and unlocked Skin Collection.
*   **Real-time Updates:** See notes appear instantly as mobile users post them.

### Technology Stack
*   **Frontend:** HTML5, Vanilla JavaScript.
*   **Styling:** Tailwind CSS.
*   **Icons:** Lucide Icons.
*   **Backend:** Firebase Realtime Database & Authentication.

### How to Run
1.  Simply open `https://stickar-36faf.web.app/` in any modern web browser.

---

## StickAR Mobile (Unity AR App)
The full Augmented Reality experience available on Android.

### Features
*   **AR Scanning:** Scan physical images/markers to unlock Virtual Rooms.
*   **3D Environment:** 4 interactive boards on 3D walls.
*   **Proximity Interaction:** Walk closer to notes to read them.

### How to Use
1.  **Scan:** Use the 'Scan Page' to find markers in the real world.
2.  **Enter Room:** Click the prompt button to enter the virtual space.
3.  **Interact:**
    *   **Post:** Click an empty space, type your message, and select a skin.
    *   **Like/Comment:** Tap existing notes to interact.
    *   **Change Variant:** Use the bottom-left button to swap Note Skins.

---

## Account & Settings
*   **Universal Login:** Your account works on both Web and Mobile.
*   **Profile:** Manage your Username, Email, and Password.
*   **Privacy:** Toggle anonymity on notes or email notifications via the Settings page.

---

## Bugs & Limitations
*   *Web:* Grid view is an abstraction of the 3D space; precise positioning may vary slightly from AR view.
*   *Status:* Beta release. Feedback is welcomed.

---

## References and Credits
*   **Database:** [Google Firebase](https://firebase.google.com/)
*   **Icons:** [Lucide](https://lucide.dev/) & [Figma Game Icons](https://www.figma.com/design/6SAPrziKbA4BK7oN2RH7d8/Ultimate-Discord-Library--Community-)
*   **Assets:** [Digital Sticky Notes Template](https://www.figma.com/design/e80CbPR3Tk5S1FawAKlRcp/Digital-Sticky-Notes--Community-)
*   **Website** [StickAR](https://stickar-36faf.web.app/)
*   **Developers:** Lim Guang Xuan & Ng Kiang Hwee
