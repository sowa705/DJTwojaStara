<script setup>
import QrcodeVue from 'qrcode.vue'
import { computed, onMounted, ref } from 'vue'
import { getInviteLink, getInviteSupport } from "../services/InviteService"

const qrOptions = computed(() => ({
  typeNumber: 0,
  colorDark: '#000000',
  colorLight: '#ffffff',
  correctLevel: 'L',
}))
const inviteEnabled = ref(false)
const inviteLink = ref('')
onMounted(async () => {
  inviteEnabled.value = await getInviteSupport()
  inviteLink.value = await getInviteLink()
})
</script>

<template>
  <div class="row" v-if="inviteEnabled">
    <div class="six columns">
      <h1>Invite this instance to your server</h1>
      
      <p>Invites are enabled on this instance. You can invite this instance to your server by clicking the button below or scanning the QR code.</p>
      
      <a :href="inviteLink" target="_blank">
        <button class="button-primary">Invite this instance to your server</button>
      </a>
      
    </div>
    <div class="six columns qr-container">
      <div class="qr-image">
        <qrcode-vue :value="inviteLink" :options="qrOptions" :size="400" />
      </div>
    </div>
  </div>
  <div class="row" v-else>
    <h1>Invite this instance to your server</h1>
    <p>Invites are disabled on this instance :(</p>
    <p>Contact the administrator or consider self-hosting your own instance: <a href="https://github.com/sowa705/DJTwojaStara">https://github.com/sowa705/DJTwojaStara</a></p>
  </div>
</template>

<style scoped>
.qr-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}
.qr-image {
  box-sizing: border-box;
  padding: 10px;
  background-color: white;
  border-radius: 15px;
  height: 420px;
}
</style>
