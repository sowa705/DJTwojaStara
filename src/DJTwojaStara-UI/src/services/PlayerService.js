import axios from 'axios';
let apiBase = "";
if (window.location.href.startsWith('http://localhost:5')) { // development hack to run asp net core on a different port than vue
    apiBase = "http://localhost:5000";
}
export async function getSessionData(id) {
    const response = await axios.get(apiBase+'/api/Player/'+id);
    return response.data;
}

export async function deleteSongFromQueue(sessionid, songid) {
    const response = await axios.post(apiBase+'/api/Player/'+sessionid+'/skip/'+songid);
}

export async function addSongToQueue(sessionid, url) {
    const response = await axios({
        method: 'post',
        url: `${apiBase}/api/Player/${sessionid}/addtrack/`,
        data: `"${url}"`,
        headers: {
            'Content-Type': 'text/json'
        }
    });
}

export async function skipCurrentSong(sessionid) {
    const response = await axios.post(apiBase+'/api/Player/'+sessionid+'/skip');
}