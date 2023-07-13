<template>
  <div class="queue-entry" v-bind:class="{ 'currently-playing-item': currentlyPlaying }">
    <div class="song-info">
      <img :src="song.coverUrl" alt="Album Art" class="album-art"/>
      <a target="_blank" :href="song.url" class="song-name">{{ song.name }}</a>
    </div>
    <button v-on:click="deleteSong" class="delete-button">Delete</button>
  </div>
</template>

<script>
export default {
  name: 'QueueEntry',
  props: {
    song: {
      type: Object,
      required: true
    },
    currentlyPlaying: {
      type: Boolean,
      default: false
    }
  },
  methods: {
    deleteSong() {
      this.$emit('delete-song', this.song);
    },
  },
}
</script>

<style scoped>
.queue-entry {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background-color: #fff1;
  padding: 5px;
  border-radius: 8px;
}

.queue-entry.currently-playing-item {
  background-color: #00ff0033;
}

.song-info {
  display: flex;
  align-items: center;
  font-size: 12px;
}

.song-info .album-art {
  width: 50px;
  height: 50px;
  object-fit: cover;
  margin-right: 10px;
  border-radius: 5px;
  filter: blur(0.5px);
}

.song-info .song-name {
  color: #fff;
}

.delete-button {
  background-color: #fff2;
  color: #fff;
  padding: 0px 10px;
  margin: 0;
  margin-left: 10px;
  border-radius: 5px;
  border: none;
  cursor: pointer;
}
.delete-button:hover {
  background-color: #fff4;
}
</style>