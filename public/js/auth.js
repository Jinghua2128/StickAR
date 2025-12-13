import { auth } from './firebase.js';
import { 
    createUserWithEmailAndPassword, 
    signInWithEmailAndPassword, 
    signOut 
} from "https://www.gstatic.com/firebasejs/9.6.1/firebase-auth.js";

// Global state for authentication mode (login or signup)
let isLoginMode = true;

// --- DOM Elements ---
const authForm = document.getElementById('auth-form');
const authButton = document.getElementById('auth-button');
const toggleAuthModeButton = document.getElementById('toggle-auth-mode');
const formTitle = document.getElementById('form-title');
const errorMessage = document.getElementById('error-message');
const emailInput = document.getElementById('email');
const passwordInput = document.getElementById('password');
const logoutButton = document.getElementById('logout-button');

// Function to update the UI based on the current mode
const updateAuthUI = () => {
    formTitle.textContent = isLoginMode ? 'Login' : 'Signup';
    authButton.textContent = isLoginMode ? 'Log In' : 'Sign Up';
    toggleAuthModeButton.textContent = isLoginMode 
        ? 'Need an account? Switch to Signup' 
        : 'Already have an account? Switch to Login';
    errorMessage.classList.add('hidden'); // Clear error on mode switch
};

// --- Handlers ---

// Toggle between Login and Signup modes
const handleToggleAuthMode = () => {
    isLoginMode = !isLoginMode;
    updateAuthUI();
};

// Handle Form Submission (Login or Signup)
const handleAuthFormSubmit = async (e) => {
    e.preventDefault();
    errorMessage.classList.add('hidden'); // Hide previous error
    
    const email = emailInput.value;
    const password = passwordInput.value;
    
    try {
        if (isLoginMode) {
            // LOGIN logic
            await signInWithEmailAndPassword(auth, email, password);
            console.log("User logged in successfully.");
        } else {
            // SIGNUP logic
            await createUserWithEmailAndPassword(auth, email, password);
            console.log("User signed up successfully.");
        }
        // Redirection handled by the onAuthStateChanged listener in firebase.js
    } catch (error) {
        console.error("Authentication Error:", error);
        errorMessage.textContent = `Error: ${error.message}`;
        errorMessage.classList.remove('hidden');
    }
};

// Handle Logout
const handleLogout = async () => {
    try {
        await signOut(auth);
        console.log("User signed out successfully.");
        // Redirection handled by the onAuthStateChanged listener in firebase.js
        window.location.href = 'index.html'; // Fallback redirect
    } catch (error) {
        console.error("Logout Error:", error);
        alert("Failed to log out.");
    }
};


// --- Event Listeners and Initialization ---

// Only run on index.html
if (authForm) {
    authForm.addEventListener('submit', handleAuthFormSubmit);
    toggleAuthModeButton.addEventListener('click', handleToggleAuthMode);
    updateAuthUI(); // Initial UI setup
}

// Only run on pages with the logout button (rooms, profile, missions)
if (logoutButton) {
    logoutButton.addEventListener('click', handleLogout);
}