<script setup>
import PerformanceChart from '../components/PerformanceChart.vue'
let currentSessionCount = 0;
</script>
<template>
  <main>
    <div class="row">
      <div class="six columns">
        <h1>Welcome to DJTwojaStara</h1>
        <p>
          DJTwojaStara is a Discord bot that allows you to play music from Youtube on your Discord server.
        </p>
        <div class="box-container">
          <h4>Instance stats</h4>
          Currently serving {{ currentSessionCount }} sessions
          <PerformanceChart />
        </div>
      </div>
      <div class="six columns">
        <h4>Bot features</h4>
        <div class="box-container">
          <h5>Youtube support</h5>
          Play music from youtube on your Discord server
        </div>
        <div class="box-container">
          <h5>Slash commands</h5>
          Control the bot effortlessly with slash commands
        </div>
        <div class="box-container">
          <h5>Web UI</h5>
          Don't want to use slash commands? Use the WebUI instead (like you are doing right now :D)
        </div>
        <div class="box-container">
          <h5>Equalizer</h5>
          Adjust the sound to your liking with the equalizer
        </div>
        <div class="box-container">
          <h5>Free and open source</h5>
          DJTwojaStara is free and open source software licensed under the GPL-3.0 license
        </div>
      </div>
    </div>
  </main>
</template>

<style scoped>
.box-container {
  background-color: #fff1;
  padding: 10px;
  border-radius: 10px;
  margin-bottom: 10px;
}
</style>