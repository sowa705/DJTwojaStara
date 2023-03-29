# DJ Twoja stara
*Shitty discord bot with support for Youtube*

## Features

* Play music from youtube
* Play entire playlists
* Slash commands
* Equalizer
* *Interrupt* feature (interrupt current song with another one and resume playback)
* AI chatbot based on llama.cpp

## Usage

Add your bot token to the TOKEN environment variable

## AI chatbot (`/ask`)

The chatbot is based on the [TCP fork of the LLaMa.cpp](https://github.com/tarruda/llama.cpp) project. I recommend using the Alpaca 13B model for performance reasons.

### Requirements

AI chat feature is very demanding. You need at least a quad core arm64/x86_64 CPU with at least 16GB of ram. The model itself is 7.5GB. (protip: Oracle has free ARM vm's that work well for this)

### Setup

Use the included `prepareAI.sh` script to download the model, llama.cpp repo and add the systemd service.

### Alignment and Safety

lol, lmao even