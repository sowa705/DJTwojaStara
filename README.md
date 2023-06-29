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

You can use a docker one-liner to run the bot.

```shell
docker run sowa705/djtwojastara:latest -p 8080:80 -e TOKEN=your_token_here
```

## Building

Running in docker takes care of all dependencies and is recommended.

```shell
docker build -t djtwojastara .
docker run -p 8080:80 djtwojastara -e TOKEN=your_token_here
```