// /public/js/firebase.js

// 1. Firebase Configuration (Specific to StickAR-36faf)
// Note: In a production environment, keys should be protected (e.g., using a proxy or environment variables).
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

// 2. Import Firebase SDKs (using ES Module syntax and CDN)
import { initializeApp } from "https://www.gstatic.com/firebasejs/9.6.1/firebase-app.js";
import { getAuth, onAuthStateChanged } from "https://www.gstatic.com/firebasejs/9.6.1/firebase-auth.js";
// 🚨 IMPORTANT: These are the functions for the REALTIME DATABASE
import { getDatabase, ref, onValue, push, set, remove } from "https://www.gstatic.com/firebasejs/9.6.1/firebase-database.js";


// 3. Initialize Firebase App and Services
const app = initializeApp(firebaseConfig);

// 4. Export services for use in other modules (auth.js, rooms.js)
export const auth = getAuth(app);
export const db = getDatabase(app); // Realtime Database instance

// 5. Simple Auth Check Listener (Global Utility)
onAuthStateChanged(auth, (user) => {
    // Check if the current page is one of the protected pages
    const protectedPages = ['rooms.html', 'room-detail.html', 'missions.html', 'profile.html'];
    const currentPage = window.location.pathname.split('/').pop();

    if (protectedPages.includes(currentPage) && !user) {
        // User is not logged in, redirect to login page
        window.location.href = 'index.html';
    } else if (currentPage === 'index.html' && user) {
        // User is logged in and is on the login/signup page, redirect to rooms
        window.location.href = 'rooms.html';
    }
});