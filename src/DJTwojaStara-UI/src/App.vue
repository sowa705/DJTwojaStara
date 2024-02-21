<script setup>
import { RouterLink, RouterView } from 'vue-router'
</script>

<template class="row">
  <div class="row">
    <header class="titlebar">
      <div class="title">
        <RouterLink to="/">DJTwojaStara</RouterLink>
      </div>
      <nav>
        <RouterLink to="/invite">Invite to server</RouterLink>
        <RouterLink to="/join">Join session</RouterLink>
      </nav>
    </header>
  </div>
  <br>
  <RouterView />
</template>

<style scoped>
.titlebar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  height: 3rem;
  background-color: #fff2;
  padding: 10px 10px;
  border-radius: 6px;
  color: #fff;
}
.titlebar .title {
  font-size: 2rem;
}
nav {
  display: flex;
  gap: 1.5rem;
}
nav a {
  font-size: 2rem;
  color: #fff;
  text-decoration: none;
}
</style>