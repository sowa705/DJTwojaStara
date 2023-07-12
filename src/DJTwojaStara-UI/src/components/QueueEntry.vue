<template>
  <div class="queue-entry">
    <div class="song-info">
      <img :src="song.coverUrl" alt="Album Art" class="album-art"/>
      <span class="song-name">{{ song.name }}</span>
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
  border-radius: 5px;
  border: none;
  cursor: pointer;
}
.delete-button:hover {
  background-color: #fff4;
}
</style>