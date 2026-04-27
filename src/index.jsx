import React from 'react';
import ReactDOM from 'react-dom';
import { BrowserRouter as Router } from 'react-router-dom';
import './polyfills';
import App from './App';
import style from './index.module.css';
import registerServiceWorker from './registerServiceWorker';
import './utils/styles/dist/vars.css';
import './index.css';

/* eslint-disable react/jsx-filename-extension */
ReactDOM.render(
  <Router>
    <App className={style.app} />
  </Router>,
  document.getElementById("root")
);
registerServiceWorker();
