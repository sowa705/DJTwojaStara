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

## Building

Running in docker takes care of all dependencies and is recommended.

```shell
docker build -t djtwojastara .
docker run -p 8080:80 djtwojastara -e TOKEN=your_token_here
```