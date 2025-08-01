let currentId = 1;
let history = [];
let lastChoiceText = null;
let userName = null;
let totalScore = 0; // Added to track the user's score

const storyTextDiv = document.getElementById("storyText");
const historyDiv = document.getElementById("history");
const leftBtn = document.getElementById("leftBtn");
const rightBtn = document.getElementById("rightBtn");
const userInputDiv = document.getElementById("userInput");
const storyContainerDiv = document.getElementById("storyContainer");
const userNameInput = document.getElementById("userNameInput");
const startStoryBtn = document.getElementById("startStoryBtn");
const restartBtn = document.getElementById("restartBtn");
const exitBtn = document.getElementById("exitBtn");

// Ensure the user input form is displayed initially
document.addEventListener("DOMContentLoaded", () => {
    userInputDiv.style.display = "flex";
    storyContainerDiv.style.display = "none";
    document.getElementById("endButtons").style.display = "none"; // Ensure end buttons are hidden initially
});

startStoryBtn.addEventListener("click", async () => {
    const name = userNameInput.value.trim();
    if (!name) {
        alert("Please enter your name to start the story.");
        return;
    }

    userName = name;

    // Register the user
    const response = await fetch("http://localhost:51441/user/register", { // Updated URL
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ name: userName, score: 0 })
    });

    if (response.ok) {
        userInputDiv.style.display = "none";
        storyContainerDiv.style.display = "block";
        loadStory();
    } else {
        alert("Failed to register user. Please try again.");
    }
});

restartBtn.addEventListener("click", () => {
    userName = null;
    totalScore = 0;
    history = [];
    userNameInput.value = "";
    userInputDiv.style.display = "flex";
    storyContainerDiv.style.display = "none";
});

exitBtn.addEventListener("click", () => {
    alert("Thank you for playing!");
    window.location.reload();
});

// Modify the loadStory function to handle the response structure correctly
async function loadStory() {
    const response = await fetch("http://localhost:51441/story/start");
    const data = await response.json();

    if (response.ok && data.node) {
        totalScore = data.totalScore; // Initialize score from server
        displayNode(data.node);
    } else {
        alert(data.Message || "Failed to load the story.");
    }
}

// Update the displayNode function to show the score at the end
function displayNode(node) {
    if (!node || !node.text || (node.nextLeft == null && node.nextRight == null)) {
        if (lastChoiceText) {
            history.push(" YOU CHOSE : " + lastChoiceText);
            lastChoiceText = null;
        }
        history.push(node.text);

        storyTextDiv.textContent = "🎬 " + (node?.text || "The story ends here.") + " 🎬";
        historyDiv.textContent = history.join("\n\n");

        storyTextDiv.textContent = history.at(-1);
        leftBtn.style.display = "none";
        rightBtn.style.display = "none";
        document.getElementById("endButtons").style.display = "flex";

        // Save the user's score
        saveUserScore();
        alert(`Your score: ${totalScore}`); // Display the score
        document.getElementById("storyText").innerHTML +=
            `<p style="margin-top:20px;"><strong>Your score: ${totalScore}</strong></p>`;
        return;
    }

    storyTextDiv.textContent = node.text;

    leftBtn.style.display = "inline-block";         
    rightBtn.style.display = "inline-block";      
    document.getElementById("endButtons").style.display = "none"; 

    if (lastChoiceText) {
        history.push(" YOU CHOSE : " + lastChoiceText);
        lastChoiceText = null;
    }
    history.push(node.text);
    historyDiv.textContent = history.join("\n\n");

    currentId = node.id;

    leftBtn.disabled = node.nextLeft == null;
    rightBtn.disabled = node.nextRight == null;

    const [leftText, rightText] = extractChoices(node.text);
    leftBtn.textContent = leftText;
    rightBtn.textContent = rightText;
}

async function choose(direction) {
    lastChoiceText = direction === "left" ? leftBtn.textContent : rightBtn.textContent;

    const res = await fetch("http://localhost:51441/story/next", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ currentId, choice: direction })
    });

    const data = await res.json();               // data = { Node, TotalScore }
    if (res.ok && data.node) {
        totalScore = data.totalScore;            
        displayNode(data.node);
    } else {
        alert(data.Message || "Failed to load next node.");
    }
}

async function saveUserScore() {
    await fetch("http://localhost:51441/user/score", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ name: userName, score: totalScore })
    });
}

function extractChoices(text) {
    const pattern = /Do you (.+?) or (.+?)\?/i;
    const match = text.match(pattern);
    if (match) {
        return [capitalize(match[1]), capitalize(match[2])];
    }
    return ["Left", "Right"]; // fallback
}

function capitalize(s) {
    return s.charAt(0).toUpperCase() + s.slice(1);
}

leftBtn.addEventListener("click", () => choose("left"));
rightBtn.addEventListener("click", () => choose("right"));