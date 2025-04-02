import { defineConfig } from 'astro/config';
import { loadEnv } from "vite";

// https://astro.build/config
export default defineConfig({});
const { NODE_TLS_REJECT_UNAUTHORIZED } = loadEnv(process.env.NODE_ENV, process.cwd(), "");
process.env.NODE_TLS_REJECT_UNAUTHORIZED = NODE_TLS_REJECT_UNAUTHORIZED;