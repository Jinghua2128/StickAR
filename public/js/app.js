// --- Configuration ---
const firebaseConfig = {
    apiKey: "AIzaSyDfwu2FcWWz0wvFkqpjLH3Yxsuu7N5wkGs",
    authDomain: "stickar-36faf.firebaseapp.com",
    databaseURL: "https://stickar-36faf-default-rtdb.asia-southeast1.firebasedatabase.app",
    projectId: "stickar-36faf",
    storageBucket: "stickar-36faf.firebasestorage.app",
    messagingSenderId: "56412676389",
    appId: "1:56412676389:web:460928af7a345f4ae8facc",
    measurementId: "G-EJR99NS96K"
};

// Initialize Firebase
if (typeof firebase !== 'undefined') {
    firebase.initializeApp(firebaseConfig);
}
const db = firebase.database();

// --- Constants & Initial Data ---
const INITIAL_DATA = {
    "players": {
        "EEFZvci6hZhEjKwWXmZM7Ke20m83": {
            "AR_tokens": 20,
            "email": "lm2128@gmail.com",
            "experience": 450,
            "id": "99965597",
            "level": 3,
            "playerName": "lm2128",
            "selectedSkin": "Pink",
            "uid": "EEFZvci6hZhEjKwWXmZM7Ke20m83",
            "unLocked": {
                "rooms": ["Library", "Grass Patch"],
                "skins": ["Blue", "Yellow", "Pink"]
            }
        }
    },
    "rooms": {
        "Grass Patch": {
            "canvases": {
                "canvas_1": {
                    "stickARs": {
                        "40ef1d49-9ff1-4b41-ba9a-4cc24554139a": {
                            "authorName": "lm2128",
                            "authorSkin": "Pink",
                            "authorUid": "EEFZvci6hZhEjKwWXmZM7Ke20m83",
                            "content": "Hello world! This is my first stickAR.",
                            "gridX": 0,
                            "gridY": 2,
                            "id": "40ef1d49-9ff1-4b41-ba9a-4cc24554139a",
                            "likes": 2,
                            "timestamp": 1765789467
                        }
                    }
                },
                "canvas_2": {
                    "stickARs": {
                        "6cd403a1-4165-4b02-b657-19fede5f7f3d": {
                            "authorName": "lm2128",
                            "authorSkin": "Pink",
                            "authorUid": "EEFZvci6hZhEjKwWXmZM7Ke20m83",
                            "content": "hi there",
                            "gridX": 0,
                            "gridY": 2,
                            "id": "6cd403a1-4165-4b02-b657-19fede5f7f3d",
                            "likes": 4,
                            "timestamp": 1765798458
                        }
                    }
                },
                "canvas_3": {
                    "stickARs": {}
                },
                "canvas_4": {
                    "stickARs": {}
                }
            },
            "maxCanvases": 4,
            "name": "Grass Patch"
        },
        "Library": {
            "canvases": {
                "canvas_1": {
                    "stickARs": {
                        "stickar_lib_001": {
                            "authorName": "lm2128",
                            "authorSkin": "Pink",
                            "authorUid": "EEFZvci6hZhEjKwWXmZM7Ke20m83",
                            "content": "Welcome to the Library! 📚",
                            "gridX": 0,
                            "gridY": 0,
                            "id": "stickar_lib_001",
                            "likes": 12,
                            "timestamp": 1734307200
                        }
                    }
                },
                "canvas_2": {
                    "stickARs": {}
                }
            },
            "maxCanvases": 4,
            "name": "Library"
        }
    }
};

// --- Helper Functions ---
function generateId() {
    return Math.random().toString(36).substr(2, 9);
}

// --- Application State ---
const AppState = {
    data: JSON.parse(JSON.stringify(INITIAL_DATA)), // Fallback structure
    view: 'LOGIN',
    authLoading: true,
    authMode: 'LOGIN',
    authError: null,
    selectedRoomId: null,
    selectedCanvasId: 'canvas_1',
    isModalOpen: false,
    pendingCoords: null,
    newNoteContent: '',
    currentUser: null
};

// --- Auth & Database Listeners ---
if (typeof firebase !== 'undefined') {
    firebase.auth().onAuthStateChanged((user) => {
        if (user) {
            // User is signed in
            const uid = user.uid;

            // 1. Listen to Current User Data
            const userRef = db.ref(`players/${uid}`);
            userRef.on('value', (snapshot) => {
                const val = snapshot.val();
                if (val) {
                    AppState.data.players[uid] = val;
                    AppState.currentUser = val;
                    render();
                }
            });

            // 2. Listen to Rooms Data (Realtime)
            const roomsRef = db.ref('rooms');
            roomsRef.on('value', (snapshot) => {
                const val = snapshot.val();
                if (val) {
                    AppState.data.rooms = val;
                    render();
                } else {
                    // SEED DATABASE if empty (First run)
                    roomsRef.set(INITIAL_DATA.rooms);
                }
            });

            AppState.view = 'HOME';
        } else {
            // User is signed out - Unsubscribe
            db.ref('rooms').off();
            if (AppState.currentUser) {
                db.ref(`players/${AppState.currentUser.uid}`).off();
            }

            AppState.currentUser = null;
            AppState.view = 'LOGIN';
        }

        AppState.authLoading = false;
        render();
    });
} else {
    console.error("Firebase SDK not loaded");
    AppState.authLoading = false;
    AppState.view = 'LOGIN';
    AppState.authError = "System Error: Firebase not loaded";
}

// --- Actions ---
const Actions = {
    // Auth Actions
    toggleAuthMode: () => {
        AppState.authMode = AppState.authMode === 'LOGIN' ? 'SIGNUP' : 'LOGIN';
        AppState.authError = null;
        render();
    },

    handleAuthSubmit: async (e) => {
        e.preventDefault();
        const email = document.getElementById('auth-email').value;
        const password = document.getElementById('auth-password').value;

        AppState.authError = null;
        const button = document.getElementById('auth-submit-btn');
        if (button) {
            button.disabled = true;
            button.innerText = "Processing...";
        }

        try {
            if (AppState.authMode === 'LOGIN') {
                await firebase.auth().signInWithEmailAndPassword(email, password);
            } else {
                // Sign Up Flow
                const userCredential = await firebase.auth().createUserWithEmailAndPassword(email, password);
                const user = userCredential.user;

                // Create Player Profile in DB
                const newPlayer = {
                    AR_tokens: 10,
                    email: user.email,
                    experience: 0,
                    id: generateId(),
                    level: 1,
                    playerName: user.email.split('@')[0],
                    selectedSkin: "Blue",
                    uid: user.uid,
                    unLocked: {
                        rooms: ["Grass Patch"],
                        skins: ["Blue"]
                    }
                };

                await db.ref(`players/${user.uid}`).set(newPlayer);
            }
        } catch (error) {
            AppState.authError = error.message;
            if (button) {
                button.disabled = false;
                button.innerText = AppState.authMode === 'LOGIN' ? "Sign In" : "Sign Up";
            }
            render();
        }
    },

    logout: () => {
        firebase.auth().signOut();
    },

    // Navigation Actions
    setView: (viewName) => {
        AppState.view = viewName;
        if (viewName === 'HOME') {
            AppState.selectedRoomId = null;
        }
        render();
    },

    selectRoom: (roomId) => {
        AppState.selectedRoomId = roomId;
        AppState.selectedCanvasId = 'canvas_1';
        AppState.view = 'ROOM';
        render();
    },

    selectCanvas: (canvasId) => {
        AppState.selectedCanvasId = canvasId;
        render();
    },

    initAddNote: (x, y) => {
        AppState.pendingCoords = { x, y };
        AppState.isModalOpen = true;
        render();
    },

    updateNoteContent: (content) => {
        AppState.newNoteContent = content;
    },

    cancelAddNote: () => {
        AppState.isModalOpen = false;
        AppState.pendingCoords = null;
        AppState.newNoteContent = '';
        render();
    },

    confirmAddNote: () => {
        const inputEl = document.getElementById('note-input');
        const content = inputEl ? inputEl.value : '';

        if (!AppState.selectedRoomId || !AppState.selectedCanvasId || !AppState.pendingCoords || !content.trim()) return;

        const newNoteId = generateId();
        const newStickAR = {
            id: newNoteId,
            authorName: AppState.currentUser.playerName,
            authorSkin: AppState.currentUser.selectedSkin,
            authorUid: AppState.currentUser.uid,
            content: content,
            gridX: AppState.pendingCoords.x,
            gridY: AppState.pendingCoords.y,
            likes: 0,
            timestamp: Date.now() / 1000
        };

        // DB Update: Write to Firebase
        const path = `rooms/${AppState.selectedRoomId}/canvases/${AppState.selectedCanvasId}/stickARs/${newNoteId}`;
        db.ref(path).set(newStickAR)
            .then(() => {
                // Reset local UI state (DB listener will update the grid)
                AppState.isModalOpen = false;
                AppState.pendingCoords = null;
                AppState.newNoteContent = '';
                render();
            })
            .catch((error) => {
                alert("Failed to post: " + error.message);
            });
    },

    likeNote: (stickARId) => {
        if (!AppState.selectedRoomId || !AppState.selectedCanvasId) return;

        const path = `rooms/${AppState.selectedRoomId}/canvases/${AppState.selectedCanvasId}/stickARs/${stickARId}/likes`;

        // DB Update: Transaction to increment likes safely
        db.ref(path).transaction((currentLikes) => {
            return (currentLikes || 0) + 1;
        });
    }
};

window.app = Actions;

// --- Components ---

function renderLogin() {
    const isLogin = AppState.authMode === 'LOGIN';
    return `
        <div class="flex flex-col items-center justify-center min-h-screen p-6 bg-white w-full animate-fade-in">
        <div class="w-20 h-20 bg-blue-600 rounded-3xl flex items-center justify-center mb-6 shadow-xl shadow-blue-200 rotate-3">
            <i data-lucide="sticker" class="text-white w-10 h-10"></i>
        </div>
        
        <h1 class="text-3xl font-extrabold text-gray-900 mb-2">StickAR</h1>
        <p class="text-gray-500 mb-8 text-center">Leave your mark on the augmented world.</p>

        <form onsubmit="window.app.handleAuthSubmit(event)" class="w-full max-w-sm space-y-4">
            ${AppState.authError ? `
            <div class="bg-red-50 text-red-500 text-sm p-3 rounded-xl border border-red-100">
                ${AppState.authError}
            </div>
            ` : ''}
            
            <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Email</label>
            <input id="auth-email" type="email" required class="w-full p-3 rounded-xl border border-gray-200 bg-gray-50 focus:bg-white focus:ring-2 focus:ring-blue-500 outline-none transition-all">
            </div>
            
            <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Password</label>
            <input id="auth-password" type="password" required minlength="6" class="w-full p-3 rounded-xl border border-gray-200 bg-gray-50 focus:bg-white focus:ring-2 focus:ring-blue-500 outline-none transition-all">
            </div>

            <button id="auth-submit-btn" type="submit" class="w-full py-4 bg-blue-600 text-white font-bold rounded-xl shadow-lg shadow-blue-200 hover:bg-blue-700 active:scale-95 transition-all mt-4">
            ${isLogin ? 'Sign In' : 'Create Account'}
            </button>
        </form>

        <div class="mt-6 text-center">
            <button onclick="window.app.toggleAuthMode()" class="text-sm text-gray-500 hover:text-blue-600 font-medium">
            ${isLogin ? "New here? Create an account" : "Already have an account? Sign In"}
            </button>
        </div>
        </div>
    `;
}

function renderStickARItem(data) {
    let colorClasses = 'bg-gray-200 text-gray-900 border-gray-300 shadow-gray-200';

    switch (data.authorSkin?.toLowerCase()) {
        case 'pink': colorClasses = 'bg-pink-300 text-pink-900 border-pink-400 shadow-pink-200'; break;
        case 'blue': colorClasses = 'bg-blue-300 text-blue-900 border-blue-400 shadow-blue-200'; break;
        case 'yellow': colorClasses = 'bg-yellow-200 text-yellow-900 border-yellow-400 shadow-yellow-200'; break;
    }

    return `
        <div 
        class="w-full h-full p-2 relative rounded-lg border-2 shadow-lg flex flex-col justify-between transform transition-transform hover:scale-105 ${colorClasses}"
        style="box-shadow: 4px 4px 0px rgba(0,0,0,0.1)"
        onclick="event.stopPropagation(); window.app.likeNote('${data.id}')"
        >
        <div class="text-xs font-bold opacity-70 mb-1 truncate">@${data.authorName}</div>
        <div class="text-sm font-medium leading-tight overflow-hidden text-ellipsis flex-grow break-words">
            ${data.content}
        </div>
        <div class="flex items-center justify-end mt-2">
            <button 
            class="flex items-center space-x-1 text-xs bg-white/40 hover:bg-white/60 px-2 py-1 rounded-full transition-colors"
            >
            <i data-lucide="heart" class="w-3 h-3 ${data.likes > 0 ? "fill-current text-red-500" : ""}"></i>
            <span>${data.likes}</span>
            </button>
        </div>
        </div>
    `;
}

function renderGridMap(canvas) {
    const GRID_SIZE = 3;
    let cellsHTML = '';

    for (let y = 0; y < GRID_SIZE; y++) {
        for (let x = 0; x < GRID_SIZE; x++) {
            let item = null;
            if (canvas && canvas.stickARs) {
                item = Object.values(canvas.stickARs).find(s => s.gridX === x && s.gridY === y);
            }

            cellsHTML += `
            <div class="relative w-full h-full min-h-[80px]">
            ${item
                    ? renderStickARItem(item)
                    : `<button 
                    onclick="window.app.initAddNote(${x}, ${y})"
                    class="w-full h-full rounded-lg border-2 border-dashed border-gray-300 flex items-center justify-center text-gray-300 hover:text-gray-400 hover:border-gray-400 hover:bg-gray-100 transition-all"
                >
                    <i data-lucide="plus" class="w-6 h-6"></i>
                </button>`
                }
            </div>
        `;
        }
    }

    return `
        <div class="w-full aspect-square max-w-sm mx-auto bg-gray-50 rounded-xl p-4 shadow-inner border border-gray-200 grid grid-cols-3 grid-rows-3 gap-3">
        ${cellsHTML}
        </div>
    `;
}

function renderRoomList() {
    const rooms = AppState.data.rooms || {};
    const roomsHTML = Object.entries(rooms).map(([key, room]) => {
        const canvases = room.canvases || {};
        const canvasBars = Object.keys(canvases).map(() => `
        <div class="h-1 flex-1 rounded-full bg-gray-100 overflow-hidden">
            <div class="h-full bg-blue-400 w-3/4 opacity-50"></div>
        </div>
        `).join('');

        return `
        <div 
            onclick="window.app.selectRoom('${key}')"
            class="group relative overflow-hidden bg-white rounded-2xl shadow-sm border border-gray-200 p-6 transition-all active:scale-95 hover:shadow-md cursor-pointer mb-4"
        >
            <div class="absolute top-0 right-0 p-4 opacity-10 group-hover:opacity-20 transition-opacity">
            <i data-lucide="map" class="w-20 h-20"></i>
            </div>
            
            <div class="relative z-10 flex items-center justify-between">
            <div>
                <h3 class="text-xl font-bold text-gray-800 mb-1">${room.name}</h3>
                <div class="flex items-center text-sm text-gray-500">
                <i data-lucide="box" class="w-3.5 h-3.5 mr-1"></i>
                <span>${room.maxCanvases} Canvases available</span>
                </div>
            </div>
            <div class="w-10 h-10 rounded-full bg-blue-50 flex items-center justify-center text-blue-600 group-hover:bg-blue-600 group-hover:text-white transition-colors">
                <i data-lucide="arrow-right" class="w-5 h-5"></i>
            </div>
            </div>
            
            <div class="mt-4 flex gap-2">
            ${canvasBars}
            </div>
        </div>
        `;
    }).join('');

    return `
        <div class="p-6 pb-24">
        <div class="mb-8">
            <h1 class="text-3xl font-extrabold text-gray-900 mb-2">Explore</h1>
            <p class="text-gray-500">Find a space to leave your mark.</p>
        </div>
        <div class="grid gap-4">
            ${roomsHTML}
        </div>
        </div>
    `;
}

function renderDashboard() {
    const player = AppState.currentUser;

    if (!player) return ''; // Safety

    let avatarColor = 'bg-gray-500';
    if (player.selectedSkin === 'Pink') avatarColor = 'bg-pink-500';
    if (player.selectedSkin === 'Blue') avatarColor = 'bg-blue-500';
    if (player.selectedSkin === 'Yellow') avatarColor = 'bg-yellow-500';

    const roomsHTML = (player.unLocked?.rooms || []).map(room => `
        <div class="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
        <span class="font-medium text-gray-700">${room}</span>
        <span class="text-xs bg-green-100 text-green-700 px-2 py-1 rounded-full">Unlocked</span>
        </div>
    `).join('');

    const skinsHTML = (player.unLocked?.skins || []).map(skin => `
        <div class="px-4 py-2 rounded-lg text-sm font-medium border-2 ${skin === player.selectedSkin ? 'border-blue-500 bg-blue-50 text-blue-700' : 'border-gray-200 bg-gray-50 text-gray-600'}">
        ${skin}
        </div>
    `).join('');

    return `
        <div class="p-6 space-y-6 pb-24 animate-fade-in">
        <!-- Profile Header -->
        <div class="flex flex-col items-center relative">
            <button onclick="window.app.logout()" class="absolute top-0 right-0 p-2 text-gray-400 hover:text-red-500 transition-colors bg-white rounded-full shadow-sm border border-gray-100">
            <i data-lucide="log-out" class="w-5 h-5"></i>
            </button>

            <div class="w-24 h-24 rounded-full ${avatarColor} flex items-center justify-center shadow-xl mb-4 ring-4 ring-white">
            <i data-lucide="user" class="text-white w-12 h-12"></i>
            </div>
            <h2 class="text-2xl font-bold text-gray-800">${player.playerName}</h2>
            <p class="text-gray-500 text-sm">Level ${player.level} Explorer</p>
        </div>

        <!-- Stats Cards -->
        <div class="grid grid-cols-2 gap-4">
            <div class="bg-white p-4 rounded-xl shadow-sm border border-gray-100 flex flex-col items-center">
            <i data-lucide="zap" class="text-yellow-500 mb-2 w-6 h-6"></i>
            <span class="text-xl font-bold text-gray-800">${player.experience}</span>
            <span class="text-xs text-gray-400 uppercase tracking-wider">XP</span>
            </div>
            <div class="bg-white p-4 rounded-xl shadow-sm border border-gray-100 flex flex-col items-center">
            <i data-lucide="hexagon" class="text-purple-500 mb-2 w-6 h-6"></i>
            <span class="text-xl font-bold text-gray-800">${player.AR_tokens}</span>
            <span class="text-xs text-gray-400 uppercase tracking-wider">Tokens</span>
            </div>
        </div>

        <!-- Unlocked Rooms -->
        <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-5">
            <div class="flex items-center mb-4 text-gray-700">
            <i data-lucide="map-pin" class="mr-2 text-blue-500 w-5 h-5"></i>
            <h3 class="font-bold">Discovered Locations</h3>
            </div>
            <div class="space-y-3">
            ${roomsHTML}
            </div>
        </div>

        <!-- Unlocked Skins -->
        <div class="bg-white rounded-xl shadow-sm border border-gray-100 p-5">
            <div class="flex items-center mb-4 text-gray-700">
            <i data-lucide="trophy" class="mr-2 text-orange-500 w-5 h-5"></i>
            <h3 class="font-bold">Skin Collection</h3>
            </div>
            <div class="flex gap-2 flex-wrap">
            ${skinsHTML}
            </div>
        </div>
        </div>
    `;
}

function renderModal() {
    if (!AppState.isModalOpen) return '';

    return `
        <div class="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4 animate-fade-in">
        <div class="bg-white w-full max-w-sm rounded-2xl p-6 shadow-2xl transform transition-all scale-100">
            <h3 class="text-lg font-bold mb-4 text-gray-800">Leave a note</h3>
            <textarea
            id="note-input"
            class="w-full h-32 p-3 border border-gray-200 rounded-xl bg-gray-50 focus:bg-white focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none resize-none mb-4 text-gray-700"
            placeholder="Write something nice..."
            maxlength="60"
            >${AppState.newNoteContent}</textarea>
            <div class="flex gap-3">
            <button 
                onclick="window.app.cancelAddNote()"
                class="flex-1 py-3 text-gray-600 font-medium bg-gray-100 rounded-xl hover:bg-gray-200 transition-colors"
            >
                Cancel
            </button>
            <button 
                onclick="window.app.confirmAddNote()"
                class="flex-1 py-3 text-white font-bold bg-blue-600 rounded-xl hover:bg-blue-700 transition-colors shadow-lg shadow-blue-200"
            >
                Post it!
            </button>
            </div>
        </div>
        </div>
    `;
}

function render() {
    const root = document.getElementById('root');

    if (AppState.authLoading) {
        root.innerHTML = `
        <div class="flex items-center justify-center h-screen w-full bg-gray-50">
            <div class="w-12 h-12 border-4 border-blue-200 border-t-blue-600 rounded-full animate-spin"></div>
        </div>
        `;
        return;
    }

    if (AppState.view === 'LOGIN' || !AppState.currentUser) {
        root.innerHTML = renderLogin();
        if (window.lucide) window.lucide.createIcons();
        return;
    }

    // Safe Access to Rooms Data
    const rooms = AppState.data.rooms || {};
    const currentRoom = AppState.selectedRoomId ? rooms[AppState.selectedRoomId] : null;
    const currentCanvas = (currentRoom && currentRoom.canvases && AppState.selectedCanvasId)
        ? currentRoom.canvases[AppState.selectedCanvasId]
        : undefined;

    const headerHTML = `
        <header class="h-16 bg-white/80 backdrop-blur-md border-b border-gray-100 flex items-center px-4 sticky top-0 z-30">
        ${AppState.view === 'ROOM'
            ? `<button onclick="window.app.setView('HOME')" class="p-2 -ml-2 text-gray-600 hover:text-gray-900 rounded-full hover:bg-gray-100">
                <i data-lucide="chevron-left" class="w-6 h-6"></i>
            </button>`
            : '<div class="w-8"></div>'
        }
        
        <div class="flex-1 text-center font-bold text-lg">
            ${AppState.view === 'ROOM' ? (currentRoom?.name || 'Loading...') : (AppState.view === 'HOME' ? 'StickAR' : 'Profile')}
        </div>
        
        <div class="w-8"></div>
        </header>
    `;

    let mainContentHTML = '';
    if (AppState.view === 'HOME') {
        mainContentHTML = renderRoomList();
    } else if (AppState.view === 'DASHBOARD') {
        mainContentHTML = renderDashboard();
    } else if (AppState.view === 'ROOM' && currentRoom && currentRoom.canvases) {
        const tabsHTML = Object.keys(currentRoom.canvases).map((canvasKey, idx) => `
        <button
            onclick="window.app.selectCanvas('${canvasKey}')"
            class="flex items-center space-x-2 px-4 py-2 rounded-full text-sm font-medium whitespace-nowrap transition-all ${AppState.selectedCanvasId === canvasKey
                ? 'bg-blue-600 text-white shadow-md shadow-blue-200'
                : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
            }"
        >
            <i data-lucide="layers" class="w-3.5 h-3.5"></i>
            <span>Area ${idx + 1}</span>
        </button>
        `).join('');

        mainContentHTML = `
        <div class="p-4 pb-24">
            <div class="flex gap-2 overflow-x-auto pb-4 no-scrollbar mb-2">
            ${tabsHTML}
            </div>
            <div class="mt-4 animate-fade-in-up">
            ${renderGridMap(currentCanvas)}
            </div>
            <div class="mt-8 text-center text-sm text-gray-400 px-8">
            Tap an empty space to leave a note. Tap a note to like it.
            </div>
        </div>
        `;
    } else if (AppState.view === 'ROOM') {
        mainContentHTML = `<div class="p-8 text-center text-gray-400">Room loading or not found...</div>`;
    }

    const bottomNavHTML = `
        <nav class="h-20 bg-white border-t border-gray-100 flex items-center justify-around px-6 absolute bottom-0 w-full z-40 pb-2">
        <button 
            onclick="window.app.setView('HOME')"
            class="flex flex-col items-center space-y-1 p-2 rounded-xl transition-all ${AppState.view === 'HOME' || AppState.view === 'ROOM' ? 'text-blue-600' : 'text-gray-400 hover:text-gray-600'
        }"
        >
            <i data-lucide="home" class="w-6 h-6" stroke-width="${AppState.view === 'HOME' || AppState.view === 'ROOM' ? 2.5 : 2}"></i>
        </button>
        <button 
            onclick="window.app.setView('DASHBOARD')"
            class="flex flex-col items-center space-y-1 p-2 rounded-xl transition-all ${AppState.view === 'DASHBOARD' ? 'text-blue-600' : 'text-gray-400 hover:text-gray-600'
        }"
        >
            <i data-lucide="user" class="w-6 h-6" stroke-width="${AppState.view === 'DASHBOARD' ? 2.5 : 2}"></i>
        </button>
        </nav>
    `;

    root.innerHTML = `
        <div class="w-full max-w-md bg-white min-h-screen shadow-2xl relative flex flex-col overflow-hidden">
        ${headerHTML}
        <main class="flex-1 overflow-y-auto no-scrollbar bg-white">
            ${mainContentHTML}
        </main>
        ${bottomNavHTML}
        ${renderModal()}
        </div>
    `;

    if (window.lucide) {
        window.lucide.createIcons();
    }
}

document.addEventListener('DOMContentLoaded', render);