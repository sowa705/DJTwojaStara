<template>
  <div class="row player">
    <div class="eight columns">
      <h3>Currently playing</h3>
      <div class="currently-playing">
        <div class="currently-playing-image">
          <div class="blur-background" :style="{ backgroundImage: `url(${currentSong?.coverUrl})` }"></div>
          <img :src="currentSong?.coverUrl" />
        </div>
        <div class="currently-playing-info">
          <h4>{{ currentSong?.name }}</h4>
        </div>
        <div class="currently-playing-stats">
          <p>{{formattedPosition()}}</p>
        </div>
        <div class="currently-playing-controls">
          <button @click="previousSong">Prev</button>
          <button @click="skipSong">Next</button>
        </div>
      </div>
    </div>
    <div class="four columns sidepanel">
      <h4>Playlist</h4>
      <div class="queue-box">
        <QueueEntry v-for="song in queue" :song="song" :key="song.id" @delete-song="removeFromQueue" :currentlyPlaying="song.id === currentSong.id"  />
      </div>
      <br>
      <div class="song-add">
        <input v-model="songUrl" type="text" placeholder="Song URL" @keyup.enter="addSong" />
        <button @click="addSong">Add</button>
      </div>
      <button @click="deleteAllSongs">Delete all</button>
      <i>{{ queue.length }} songs in the queue</i>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import { getSessionData, deleteSongFromQueue, addSongToQueue, nextSong, prevSong } from "../services/PlayerService";
import QueueEntry from "../components/QueueEntry.vue";
const sessionId = ref(useRoute().params.id);

let songUrl = ref('');
let session = ref(null);
let queue = ref([]);
let currentSong = ref(null);

onMounted(async () => {
  updateSession();
  setInterval(updateSession, 1000);
});

const formattedPosition = () => {
  if (!session.value) return "";

  if (session.value.playList.currentPosition) {
    let minutes = Math.floor(session.value.playList.currentPosition / 60);
    let seconds = Math.round(session.value.playList.currentPosition % 60);
    return minutes.toString().padStart(2, '0') + ':' + seconds.toFixed(0).toString().padStart(2, '0');
  } else {
    return "";
  }
}

const updateSession = async () => {
  session.value = await getSessionData(sessionId.value);
  queue.value = session.value.playList.songs;
  currentSong.value = queue.value[session.value.playList.currentSong];
  document.title = "DJ - "+currentSong.value.name;
}

const removeFromQueue = async (song) => {
  const songId = queue.value.find(s => s.id === song.id).id;

  await deleteSongFromQueue(sessionId.value, songId);
  await updateSession();
}

const deleteAllSongs = async () => {
  await Promise.all(queue.value.map(song => deleteSongFromQueue(sessionId.value, song.id)));
  await updateSession();
}

const addSong = async () => {
  const url = songUrl.value;
  songUrl.value = '';
  await addSongToQueue(sessionId.value, url);
  
  await updateSession();
}

const skipSong = async () => {
  await nextSong(sessionId.value);
  await updateSession();
}

const previousSong = async () => {
  await prevSong(sessionId.value);
  await updateSession();
}
</script>
<style scoped>
.sidepanel {
  background-color: #fff1;
  color: #fff;
  padding: 10px;
  border-radius: 6px;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
}
.queue-box {
  display: flex;
  flex-direction: column;
  overflow: auto;
  flex-grow: 1;
  gap: 10px;
}
.player {
  display: flex;
  height: calc(100vh - 110px);
}
.song-add {
  display: flex;
  gap: 10px;
}
.song-add input {
  flex-grow: 1;
}

.currently-playing {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
}
.currently-playing-image img {
  width: 400px;
  height: 400px;
  object-fit: cover;
  border-radius: 5px;
}
.blur-background {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  filter: blur(100px);
  z-index: -1;
  background-size: cover; /* or use 100% 100% if you want the image to stretch */
  background-position: center; /* Keep this if you want the image to stay centered after scaling */
  overflow: hidden;
}
.blur-background::after {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  width: 200%;
  height: 200%;
  background: rgba(0,0,0,0.8);
  filter: blur(100px);
}
</style>
