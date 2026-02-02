import {createBrowserRouter, RouterProvider} from "react-router-dom";

import Front from "./UI/Front.tsx";
import ChatRoom from "./ChatRoom.tsx";

const App = () =>{

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

    return <RouterProvider router={router}/>
}

export default App;