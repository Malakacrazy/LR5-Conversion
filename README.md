# L5R Conversion

Porting [ringteki](https://github.com/gryffon/ringteki) — a browser-based implementation of the *Legend of the Five Rings* LCG — from Node.js/TypeScript + React into C# for Unity.

## Source project

Ringteki is a fork of throneteki (the Game of Thrones LCG engine), archived since Jan 2020.

- **Backend:** Node.js/TypeScript, MongoDB, ZeroMQ for inter-process messaging
- **Frontend:** React
- **Architecture:** Lobby server (auth, matchmaking) + per-core game node processes (rules enforcement) + browser client
- **Card engine:** Event-driven — cards implement abilities by listening to a documented set of game events (attack declared, card played, ring claimed, etc.); adding a card means wiring new listeners into existing event contracts, not touching a rules engine core

## Goal

Reimplement the game engine and rules logic in C# inside Unity. Scope not yet decided: single-player only vs. networked multiplayer, whether to keep a server/client split or run rules client-authoritative, and how much of the card catalog to port initially.

## Status

Just started. No code yet — next step is deciding architecture (see Goal) before scaffolding.
