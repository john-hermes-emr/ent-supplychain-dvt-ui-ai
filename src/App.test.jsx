import React from 'react';
import ReactDOM from 'react-dom';
import { MemoryRouter } from 'react-router-dom';
import { act } from 'react-dom/test-utils';
import App from './App';

let container;

beforeEach(() => {
  container = document.createElement('div');
  document.body.appendChild(container);
});

afterEach(() => {
  document.body.removeChild(container);
  container = null;
});

it('renders title link', async () => {
  await act(async () => {
    ReactDOM.render(
      <MemoryRouter initialEntries={["/"]}>
        <App />
      </MemoryRouter>,
      container
    );
  });

  const linkElement = container.querySelector('h1');
  expect(linkElement.textContent.trim()).toBe('Click on login to visit website');
});
