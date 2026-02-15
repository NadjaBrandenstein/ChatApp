import { createRoot } from 'react-dom/client'
import { Provider } from "jotai";
import App from "./App.tsx";
import {StreamProvider} from "./Hooks/useStream.tsx";

const token = localStorage.getItem("token");

createRoot(document.getElementById('root')!).render(
      <Provider>
          {token ? (
          <StreamProvider
            config={{
                connectEvent: "connected",
                urlForStreamEndpoint: `http://localhost:5050/connect?token=${token}`
            }}>
              <App />
          </StreamProvider>
          ) : (
              <App/>
          )}
      </Provider>
)
