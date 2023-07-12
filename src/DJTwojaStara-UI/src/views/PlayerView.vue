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
        <div class="currently-playing-controls">
          <button @click="skipSong">Skip</button>
        </div>
      </div>
    </div>
    <div class="four columns sidepanel">
      <h4>Queue</h4>
      <div class="queue-box">
        <QueueEntry v-for="song in queue" :song="song" :key="song.id" @delete-song="removeFromQueue" />
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
import { getSessionData, deleteSongFromQueue, addSongToQueue, skipCurrentSong } from "../services/PlayerService";
import QueueEntry from "../components/QueueEntry.vue";
const sessionId = ref(useRoute().params.id);

let songUrl = ref('');
let session = ref(null);
let queue = ref([]);
let currentSong = ref(null);

onMounted(async () => {
  updateSession();
  setInterval(updateSession, 2000);
});

const updateSession = async () => {
  session.value = await getSessionData(sessionId.value);
  queue.value = session.value.queue;
  currentSong.value = session.value.currentTrack;
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
  await skipCurrentSong(sessionId.value);
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
