let userList = [];

window.onload = async function () {
    const res = await fetch("http://localhost:51441/user/list");
    userList = await res.json();
    renderTable(userList);
};

function renderTable(users) {
    const tbody = document.querySelector("#scoreTable tbody");
    tbody.innerHTML = "";
    users.forEach(u => {
        const tr = document.createElement("tr");
        tr.innerHTML = `<td>${u.name}</td><td>${u.score}</td>`;
        tbody.appendChild(tr);
    });
}

async function loadAndRenderScores() {
    const sortBy = document.getElementById("sortBy").value;
    const order = document.querySelector("input[name='sortOrder']:checked").value;
    const algo = document.getElementById("sortAlgo").value;

    const res = await fetch(`http://localhost:51441/user/list?sort=${sortBy}&order=${order}&algo=${algo}`);
    const users = await res.json();
    renderTable(users);
}
