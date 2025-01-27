# gui
Mircosoft hololense GUI app made in Unity that aims to collect the mesh from the POV of hololense

## Usage
Copy the _Scenes_ and _Scripps_ into your unity project _Assets_ directory. Launch it and connect everything as in the Inspector instruction.

To build the app follow the appx instructions and then deploy it onto hololense in System -> Apps section at its homepage. 

## Used plugins
- MRTK 2
- [WebRTC plugin](https://github.com/SouprP/unity-webrtc-hololens) with dependencies - works only in Unity for now
- Nuget:
    - HtmlAgilityPack
    - Newtonsoft.Json
    - System.Net.Http
- [Unity glTFast](https://docs.unity3d.com/Packages/com.unity.cloud.gltfast@6.10/manual/index.html)

## Features

### Unity editor
- [x] Head, hand joints and eyes data collection
    - [x] writing to a file every 10 seconds
    - [x] sending through WebRTC
    - [x] sending via HTTP POST request - works very slowly
- [x] Collecting mesh from what the hololense sees every frame
    - [x] writing to a file every frame
    - [x] sending through WebRTC
    - [x] sending via HTTP POST request - works very slowly
- [x] Meshes initializaiton on the scene
    - [x] Obtaining files from the server via GET request
    - [x] OBJ files (only those smaller)
    - [x] GLB and GLTF
- [ ] Keyboard for setting up server IP address (need to be hardcoded)


### Hololense
- [x] Head, hand joints and eyes data collection
    - [x] writing to a file every 10 seconds
    - [ ] sending through WebRTC
    - [x] sending via HTTP POST request - works very slowly
- [x] Collecting mesh from what the hololense sees every frame
    - [x] writing to a file every frame
    - [ ] sending through WebRTC
    - [x] sending via HTTP POST request - works very slowly
- [x] Meshes initializaiton on the scene
    - [x] Obtaining files from the server via GET request
    - [x] OBJ files (only those smaller)
    - [ ] GLB and GLTF - they render on one ocular only, without shaders or textures (they're pink XD) and are manipulable only in parts (cannot resize and grab the whole model)
- [ ] Keyboard for setting up server IP address (need to be hardcoded)

## Known bugs
- buttons in model chooser sometimes are rendered off place
- the keyboard doesn't work - it can be shown but input said bye

