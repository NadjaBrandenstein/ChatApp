import {createBrowserRouter, RouterProvider} from "react-router-dom";

import Front from "./UI/Front.tsx";
import ChatRoom from "./ChatRoom.tsx";
import {Toaster} from "react-hot-toast";

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

    return (
        <div>
            <Toaster position="top-center"/>
            <RouterProvider router={router}/>
        </div>
    )

}

export default App;