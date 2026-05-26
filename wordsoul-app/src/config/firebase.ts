import { initializeApp } from "firebase/app";
import { getMessaging, getToken, onMessage } from "firebase/messaging";

const firebaseConfig = {
  apiKey: "AIzaSyARowuD4tMrzwarl9U9knSr79mQQfiRnZw",
  authDomain: "vocamon-7932b.firebaseapp.com",
  projectId: "vocamon-7932b",
  storageBucket: "vocamon-7932b.firebasestorage.app",
  messagingSenderId: "767175855154",
  appId: "1:767175855154:web:422679e8f21f8ca0a8c02b",
  measurementId: "G-KJVT7K72Z4"
};

// Initialize Firebase
const app = initializeApp(firebaseConfig);

// Initialize Cloud Messaging and get a reference to the service
const messaging = getMessaging(app);

export { app, messaging, getToken, onMessage };
