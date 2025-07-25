# InteractiveStoryProject

## Project Overview

This is an interactive story system built with ASP.NET Core, consisting of two sub-projects:

- InteractiveStoryApi: A backend API that provides read/write access to story data. It also includes front-end HTML files (editor.html and index.html).
- LinkListStory: A console application that uses a linked list structure to store and test story node connections.

## Project Structure

InteractiveStoryProject.sln  
├── InteractiveStoryApi/  
│   ├── wwwroot/  
│   │   ├── editor.html  
│   │   ├── index.html  
│   │   └── script.js  
│   └── story.json  
├── LinkListStory/  
│   ├── Program.cs  
│   └── story.json  

## How to Use

1. Open the solution and run InteractiveStoryApi.
2. Open editor.html in your browser to create or edit story content.
3. Use the LinkListStory console app to verify story logic using a linked list structure.
4. Story data is saved in the story.json files.

## Technologies

- ASP.NET Core Web API
- HTML and JavaScript front-end
- Linked list for story data structure
