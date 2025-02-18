import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  root: '',
  build: {
    outDir: '../My.Ai.Web/wwwroot', // change this to your desired output folder
    emptyOutDir: true, // also necessary
  },
  plugins: [react()],
})
