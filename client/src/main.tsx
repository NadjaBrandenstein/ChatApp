import { createRoot } from 'react-dom/client'
import { Provider } from "jotai";
import App from "./App.tsx";

createRoot(document.getElementById('root')!).render(
      <Provider>
          <App />
      </Provider>
)
