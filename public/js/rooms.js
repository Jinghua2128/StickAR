import { db, auth } from './firebase.js';
import { 
    ref, 
    onValue, 
    push, 
    set, 
    query, 
    orderByKey 
} from "https://www.gstatic.com/firebasejs/9.6.1/firebase-database.js";

// --- Global DOM Elements ---
const roomsContainer = document.getElementById('rooms-container');
const addRoomBtn = document.getElementById('add-room-btn');
const notesContainer = document.getElementById('notes-container');
const addNoteForm = document.getElementById('add-note-form');
const roomNameDisplay = document.getElementById('room-name-display');

// =================================================================
// 1. Rooms List Page Logic (/public/rooms.html)
// =================================================================

/**
 * Renders a single room card HTML element.
 * @param {string} roomId 
 * @param {object} roomData 
 * @returns {string} HTML string
 */
const createRoomCard = (roomId, roomData) => {
    return `
        <a href="room-detail.html?roomId=${roomId}" 
            class="block bg-white p-6 rounded-xl shadow-lg hover:shadow-2xl transition duration-300 transform hover:-translate-y-1">
            <h3 class="text-2xl font-bold text-indigo-600 mb-2">${roomData.name || 'Untitled Room'}</h3>
            <p class="text-gray-600">Created by: ${roomData.creatorEmail || 'Unknown'}</p>
            <p class="text-sm text-gray-500 mt-2">Notes: ${roomData.noteCount || 0}</p>
        </a>
    `;
};

/**
 * Realtime listener for the list of rooms.
 */
const listenForRooms = () => {
    const roomsRef = ref(db, 'rooms');
    
    // onValue creates a real-time listener
    onValue(roomsRef, (snapshot) => {
        const roomsData = snapshot.val();
        roomsContainer.innerHTML = ''; // Clear existing rooms
        
        if (roomsData) {
            // Loop through rooms data and render cards
            Object.entries(roomsData).forEach(([roomId, roomData]) => {
                roomsContainer.innerHTML += createRoomCard(roomId, roomData);
            });
        } else {
            roomsContainer.innerHTML = '<p class="text-gray-500 col-span-full">No rooms found. Create one to start!</p>';
        }
    }, (error) => {
        console.error("Failed to read rooms data:", error);
        roomsContainer.innerHTML = '<p class="text-red-500 col-span-full">Error loading rooms.</p>';
    });
};

/**
 * Handles the creation of a new room.
 */
const handleAddRoom = async () => {
    const roomName = prompt("Enter the name for the new room:");
    if (!roomName || roomName.trim() === "") return;

    if (!auth.currentUser) {
        alert("You must be logged in to create a room.");
        return;
    }

    try {
        const roomsListRef = ref(db, 'rooms');
        // Use push() to generate a unique key for the new room
        const newRoomRef = push(roomsListRef); 
        
        const newRoomData = {
            name: roomName.trim(),
            creatorId: auth.currentUser.uid,
            creatorEmail: auth.currentUser.email,
            createdAt: Date.now(),
            noteCount: 0 
        };

        // Use set() to write the data under the new unique key
        await set(newRoomRef, newRoomData);
        console.log("New room created successfully:", newRoomRef.key);
    } catch (error) {
        console.error("Error creating room:", error);
        alert("Failed to create room.");
    }
};

// Initialize rooms page listeners
if (roomsContainer) {
    listenForRooms();
    addRoomBtn.addEventListener('click', handleAddRoom);
}


// =================================================================
// 2. Room Detail Page Logic (/public/room-detail.html)
// =================================================================

/**
 * Extracts the room ID from the URL query parameters.
 * @returns {string | null}
 */
const getRoomIdFromUrl = () => {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get('roomId');
};

/**
 * Renders a single note element.
 * @param {object} noteData 
 * @returns {string} HTML string
 */
const createNoteElement = (noteId, noteData) => {
    const date = new Date(noteData.createdAt).toLocaleTimeString();
    return `
        <div class="bg-gray-50 p-4 rounded-lg shadow border-l-4 border-yellow-400">
            <p class="text-gray-800 font-medium">${noteData.content}</p>
            <p class="text-xs text-gray-500 mt-2">
                Posted by ${noteData.posterEmail} at ${date}
            </p>
        </div>
    `;
};


/**
 * Realtime listener for the room's notes.
 */
const listenForNotes = (roomId) => {
    const notesRef = ref(db, `rooms/${roomId}/notes`);
    
    // Listen for the room name once
    onValue(ref(db, `rooms/${roomId}/name`), (snapshot) => {
        const name = snapshot.val() || 'Room Not Found';
        roomNameDisplay.textContent = name;
        document.getElementById('room-title').textContent = `StickAR - ${name}`;
    }, { onlyOnce: true });

    // Listen for notes
    // Using a query to order by key (which is Firebase's timestamp-based push ID)
    const notesQuery = query(notesRef, orderByKey()); 

    onValue(notesQuery, (snapshot) => {
        const notesData = snapshot.val();
        notesContainer.innerHTML = ''; // Clear existing notes

        if (notesData) {
            // Loop through notes data and render them
            // Using reverse() to show latest notes first (since push keys are chronological)
            Object.entries(notesData).reverse().forEach(([noteId, noteData]) => {
                notesContainer.innerHTML += createNoteElement(noteId, noteData);
            });
        } else {
            notesContainer.innerHTML = '<p class="text-gray-500">No notes in this room yet. Be the first to post!</p>';
        }
    }, (error) => {
        console.error("Failed to read notes data:", error);
        notesContainer.innerHTML = '<p class="text-red-500">Error loading notes.</p>';
    });
};

/**
 * Handles posting a new note.
 */
const handleAddNote = async (e) => {
    e.preventDefault();
    const roomId = getRoomIdFromUrl();
    if (!roomId) return;
    
    const noteInput = document.getElementById('note-content');
    const content = noteInput.value.trim();

    if (!content) return;
    if (!auth.currentUser) {
        alert("You must be logged in to post a note.");
        return;
    }

    try {
        const notesListRef = ref(db, `rooms/${roomId}/notes`);
        const newNoteRef = push(notesListRef);
        
        const newNoteData = {
            content: content,
            posterId: auth.currentUser.uid,
            posterEmail: auth.currentUser.email,
            createdAt: Date.now()
        };

        // 1. Write the new note
        await set(newNoteRef, newNoteData);

        // 2. Update the room's noteCount (Optional but good practice)
        // This requires a transaction for robustness in a multi-user app, 
        // but for simplicity, we'll increment the counter directly.
        // A better solution would be to use a cloud function to count notes.
        const roomRef = ref(db, `rooms/${roomId}`);
        const currentCountRef = ref(db, `rooms/${roomId}/noteCount`);

        onValue(currentCountRef, (snapshot) => {
             const currentCount = snapshot.val() || 0;
             set(currentCountRef, currentCount + 1);
        }, { onlyOnce: true });

        noteInput.value = ''; // Clear the input
        console.log("Note posted successfully.");

    } catch (error) {
        console.error("Error posting note:", error);
        alert("Failed to post note.");
    }
};

// Initialize room detail page listeners
if (addNoteForm) {
    const currentRoomId = getRoomIdFromUrl();
    if (currentRoomId) {
        listenForNotes(currentRoomId);
        addNoteForm.addEventListener('submit', handleAddNote);
    } else {
        notesContainer.innerHTML = '<p class="text-red-500">Error: Room ID not found in URL.</p>';
    }
}