import { defineConfig } from "vite";
import plugin from "@vitejs/plugin-vue";
import Components from "unplugin-vue-components/vite";
import { BootstrapVueNextResolver } from "bootstrap-vue-next";

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    plugin(),
    Components({
      resolvers: [BootstrapVueNextResolver()],
    }),
  ],
  server: {
    port: 50142,
    proxy: {
      "/api": {
        target: "https://localhost:44363", // ASP.NET MVC5 서버 주소
        changeOrigin: true,
        secure: false,
      },
    },
  },
});
