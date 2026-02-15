import {createBrowserRouter, RouterProvider} from "react-router-dom";

import Front from "./UI/Front.tsx";
import ChatRoom from "./ChatRoom.tsx";
import {Toaster} from "react-hot-toast";
import {useEffect, useState} from "react";
import {StreamProvider} from "./Hooks/useStream.tsx";

const App = () =>{

    const [token, setToken] = useState<string | null>(() =>
        sessionStorage.getItem("token"));

    useEffect(()=>{
        const syncAuth = () => {
            setToken(sessionStorage.getItem("token"));
        };

        window.addEventListener("authChanged", syncAuth);
        return () => window.removeEventListener("authChanged", syncAuth);
    }, []);


    const router = createBrowserRouter([
        {
            path: "/",
            element: <Front/>
        },
        {
            path: "/chat/:roomName",
            element: <ChatRoom/>
        },
    ]);

    const content = (
        <>
            <Toaster position="top-center"/>
            <RouterProvider router={router}/>
        </>
    );

    if(!token){
        return content;
    }

    return (
        <StreamProvider
            key={token}
        config={{
            connectEvent: "connected",
            urlForStreamEndpoint: `http://localhost:5050/connect?token=${token}`
        }}>
            {content}
        </StreamProvider>
    );

}

export default App;