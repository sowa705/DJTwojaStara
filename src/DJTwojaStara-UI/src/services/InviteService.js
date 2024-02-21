import axios from 'axios';
let apiBase = "";
if (window.location.href.startsWith('http://localhost:5')) { // development hack to run asp net core on a different port than vue
    apiBase = "http://localhost:5000";
}
export async function getInviteSupport() {
    const response = await axios.get(apiBase+'/api/Info/invite/enabled');
    return response.data;
}

export async function getInviteLink() {
    const response = await axios.get(apiBase+'/api/Info/invite/link');
    return response.data;
}