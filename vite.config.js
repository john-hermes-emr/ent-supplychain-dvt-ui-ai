import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { visualizer } from 'rollup-plugin-visualizer';
import path from 'path';
import getEnvModule from './env'

getEnvModule().setEnvironmentVarsFromTestEnv(__dirname);

process.env.CLIENT_ID = process.env.SPA_CLIENT_ID || process.env.CLIENT_ID;

const env = {};

// List of environment variables made available to the app
[
    'ISSUER',
    'CLIENT_ID',
    'REACT_APP_DVT_API_ROOT_PATH',
].forEach((key) => {
    if (!process.env[key]) {
        throw new Error(`Environment variable ${key} must be set. See README.md`);
    }
    env[key] = process.env[key];
});

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [
        react({
            jsxRuntime: 'classic',
        }),
        visualizer({
            open: true, // Automatically open the report in the browser
            filename: 'bundle-analysis.html', // Output file for the visualization
            gzipSize: true, // Show gzip size
            brotliSize: true, // Show brotli size
        }),
    ],
    define: {
        'process.env': env
    },
    resolve: {
        alias: {
            '@okta/okta-auth-js': path.resolve(__dirname, 'node_modules/@okta/okta-auth-js/dist/okta-auth-js.umd.js'),
            'react-router-dom': path.resolve(__dirname, 'node_modules/react-router-dom')
        }
    },
    server: {
        proxy: {
            '/api': {
                target: 'https://dvt-dev.emerson.com',
                // target: 'http://localhost:24407',
                changeOrigin: true,
                secure: false,
                ws: true,
                rewrite: (path) => path.replace(/^\/api/, ''),
            },
        },
        port: process.env.PORT || 8080,
        host: '0.0.0.0',
        hmr: true,
        reload: false

    },
    build: {
        commonjsOptions: {
            include: [/node_modules/, /@microsoft\/signalr/]
        },
        rollupOptions: {
            onwarn(warning, warn) {
                // Suppress PURE annotation warnings in signalr
                if (
                    warning.code === 'ANNOTATION' &&
                    warning.message.includes('node_modules/@microsoft/signalr/dist/esm/Utils.js')
                ) {
                    return;
                }
                warn(warning);
            }
        },
    }
})
