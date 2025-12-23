# 3D CHESS GAME - PROJECT DOCUMENTATION

## SECTION 1: INTRODUCTION

### 1.1 Introduction

The 3D Chess Game is a comprehensive chess application developed using Unity 2021.3.8f1 LTS, combining classical game theory algorithms with modern game development to deliver an engaging chess experience. The application features a cartoonish art style with animated pieces, dynamic cameras, and visual effects that make chess more approachable and entertaining.

The project offers three distinct gameplay modes: **Local Multiplayer** (two players on same computer), **Player vs AI** (human against computer with four difficulty levels using Minimax algorithm with Alpha-Beta pruning), and **Online Multiplayer** (remote play via Python Flask REST API backend with unique game ID system). The chess engine strictly adheres to FIDE rules, implementing all standard and special moves including castling, en passant, pawn promotion, check, checkmate, and draw detection.

Originally developed in October 2021 for the CS412/512 - Computer Games Design course at Bishop's University, Canada (in collaboration with Julien Withfield), the current version has been enhanced with multiplayer networking capabilities and refined user interfaces.


### 1.2 Problem Statement

Traditional chess applications suffer from several key limitations: (1) **Fragmented Gameplay Options** - players must use multiple applications to access local play, AI opponents, and online multiplayer; (2) **Poor AI Scalability** - existing solutions offer either overly simplistic AI or resource-intensive chess engines with long calculation times unsuitable for casual gaming; (3) **Complex Network Setup** - multiplayer requires account creation, matchmaking systems, or technical server configuration; (4) **Limited Visual Engagement** - traditional apps feature bland 2D boards or overly complex 3D interfaces lacking personality; (5) **Incomplete Rule Implementation** - many implementations fail to correctly handle special moves like en passant, castling restrictions, and draw conditions.

This project addresses these gaps by providing an all-in-one solution with three integrated gameplay modes, configurable AI with appropriate difficulty scaling (depths 2, 4, and 6 for Easy, Medium, Hard), simple no-account multiplayer with shareable game IDs, engaging 3D visuals with cartoonish style, and complete FIDE-compliant rule implementation.


### 1.3 Motivation

The project is driven by multiple educational and technical objectives. From an **educational perspective**, it provides hands-on experience implementing the Minimax algorithm with Alpha-Beta pruning—a cornerstone of game theory and adversarial search. The project offers comprehensive exposure to Unity game development, including 3D graphics, animation systems, UI design, and performance optimization, while demonstrating software architecture patterns like Model-View-Controller separation and event-driven programming.

**Technical challenges** include network programming experience with RESTful API design, client-server communication, and state synchronization; full-stack development spanning Unity C# frontend and Python Flask backend; and algorithm optimization balancing search depth versus response time. The project makes **chess more accessible** through friendly visual design, entertaining animations and sound effects, and flexible playing options accommodating different player preferences (social players, solo learners, casual players, competitive players).

For **portfolio development**, the completed project showcases proficiency in Unity and C#, AI algorithm implementation, network programming, and full-stack capabilities using industry-relevant technologies (Unity, Python, Flask, REST APIs). The project also stems from **personal interest** in chess and provides opportunities for creative expression through visual style, sound design, and game feel while offering problem-solving satisfaction in debugging complex AI, resolving network synchronization, and implementing chess rule edge cases.


### 1.4 Objectives

**Primary Objectives:**

1. **Complete Chess Rule Implementation** - Implement all standard and special chess moves (castling, en passant, pawn promotion) with comprehensive game state detection (check, checkmate, stalemate, draw conditions) following FIDE rules.

2. **Multi-Mode Gameplay System** - Develop three fully functional modes: local multiplayer (same device), single-player vs AI, and online multiplayer with network synchronization.

3. **Artificial Intelligence Implementation** - Implement Minimax algorithm with Alpha-Beta pruning, develop position evaluation function, create four difficulty levels (Random, Easy depth-2, Medium depth-4, Hard depth-6) with AI response under 3 seconds.

4. **Network Multiplayer System** - Design RESTful API backend using Python Flask, implement game session management with unique game IDs, enable game state synchronization between clients, and handle turn management with graceful error handling.

**Secondary Objectives:**

5. **Engaging Visual Design** - Create 3D chess environment with cartoonish art style, smooth animations, visual effects for captures (explosions/particles), and dynamic camera system.

6. **Intuitive User Interface** - Design clean menus for mode selection, in-game UI for game state, pawn promotion interface, multiplayer lobby, and game-over screens.

7. **Performance Optimization** - Maintain 60 FPS gameplay, optimize AI calculation time, minimize network latency impact, and implement efficient memory management.


### 1.5 Scope and Relevance

**Project Scope:**

**In Scope:** Complete chess implementation (all piece movements, special moves: castling/en passant/promotion, game state detection); Three gameplay modes (Local PvP, Player vs AI with 4 difficulty levels, Online Multiplayer); AI system using Minimax with Alpha-Beta pruning and position evaluation; Network infrastructure with Python Flask REST API, game session management, and HTTP-based synchronization; 3D visuals with cartoonish art, animations, explosions, particle effects, and dynamic camera; User interface with menus, HUD, promotion selection, and multiplayer lobby.

**Out of Scope:** Advanced chess features (opening books, endgame tables, move analysis, PGN import/export, puzzles, ELO ratings); Online platform features (user accounts, matchmaking, chat, leaderboards, persistent storage); Advanced AI (neural networks, MCTS, difficulty beyond depth-6); Additional platforms (mobile, web, console, VR/AR); Customization (multiple themes, rule variants, time controls); Technical enhancements (database integration, WebSocket connections, anti-cheat).

**Relevance:**

**Academic:** Demonstrates core CS concepts (Minimax algorithm, data structures, complexity analysis), game development principles (3D graphics, animations, UI/UX design), and AI concepts (adversarial search, evaluation functions, optimization). **Industry:** Unity holds 40%+ game engine market share; skills demonstrated (Unity/C#, REST APIs, full-stack development, Python Flask) are in high demand with competitive salaries. **Social:** Chess popularity has surged (chess.com: 100M+ users), accelerated by COVID-19 and mainstream media interest. **Professional Development:** Project serves as comprehensive portfolio piece demonstrating end-to-end development, algorithm implementation, full-stack capabilities, and project completion skills valuable for job interviews and career advancement.

---

**Page 2 End**
