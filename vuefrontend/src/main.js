import "./assets/main.css";

import App from "./App.vue";
import { createApp } from "vue";
import { createBootstrap } from "bootstrap-vue-next";

// Add the necessary CSS
import "bootstrap/dist/css/bootstrap.css";
import "bootstrap-vue-next/dist/bootstrap-vue-next.css";

// FontAwesomeIcon 설정
import { library } from "@fortawesome/fontawesome-svg-core";
import { fas } from "@fortawesome/free-solid-svg-icons";
import { faR } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/vue-fontawesome";

library.add(fas);
library.add(faR);

const app = createApp(App);
app.component("font-awesome-icon", FontAwesomeIcon);
app.use(createBootstrap()); // Important
app.mount("#app");
