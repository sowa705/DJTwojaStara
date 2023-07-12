<template>
  <div class="player">
    <h1>Join session</h1>
    <input v-model="sessionCode" type="text" placeholder="Session code" />
    <button @click="joinSession">Join</button>
  </div>
</template>

<script>
import { ref } from 'vue';

export default {
  setup() {
    const sessionCode = ref("");

    const joinSession = () => {
      console.log("Joining session");
      console.log("Session code: " + sessionCode.value);
      window.location.href = "/player/" + sessionCode.value;
    }

    return {
      sessionCode,
      joinSession
    }
  }
}
</script>

<style scoped>
</style>