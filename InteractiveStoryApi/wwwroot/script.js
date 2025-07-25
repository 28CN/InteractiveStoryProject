let currentId = 1;
let history = [];
let lastChoiceText = null;

const storyTextDiv = document.getElementById("storyText");
const historyDiv = document.getElementById("history");
const leftBtn = document.getElementById("leftBtn");
const rightBtn = document.getElementById("rightBtn");

loadStory();

async function loadStory() {
    const response = await fetch("/story/start");
    const startNode = await response.json();
    displayNode(startNode);
}

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
        return;
    }

    storyTextDiv.textContent = node.text;

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

    // for debugging. solved
    //console.log("Choice clicked:", direction, "Current ID:", currentId);

    lastChoiceText = direction === "left" ? leftBtn.textContent : rightBtn.textContent;

    const response = await fetch("/story/next", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ currentId, choice: direction })
    });

    const nextNode = await response.json();
    //console.log("Next Node:", nextNode);
    displayNode(nextNode);
}

leftBtn.addEventListener("click", () => choose("left"));
rightBtn.addEventListener("click", () => choose("right"));

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