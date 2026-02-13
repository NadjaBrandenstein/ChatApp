import '../CSS/Front.css'
import { useNavigate } from "react-router-dom";
import { useState } from "react";
import toast from "react-hot-toast";

function Front() {

    const navigate = useNavigate();

    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const isLoggedIn = !!localStorage.getItem("token");

    const [newRoom, setNewRoom] = useState("");
    const [rooms, setRooms] = useState<string[]>([]);

    const createRoom = async () => {
        if (!newRoom.trim()) return;

        const token = localStorage.getItem("token");
        const res = await fetch("http://localhost:5050/rooms", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify({ RoomName: newRoom })
        });

        if(res.ok){
            toast.success("Room created successfully.");
            setRooms(prev => [...prev, newRoom]);
            setNewRoom("");
        }
        else{
            toast.error("Failed to create room!");
        }
    };

    const handleLogin = async () => {
        const res = await fetch("http://localhost:5050/login", {
            method: "POST",
            headers: { "content-type": "application/json" },
            body: JSON.stringify({ username, password })
        });

        if(!res.ok) {
            toast.error("Your username or password is wrong!");
            return;
        }

        const data = await res.json();
        localStorage.setItem("token", data.token);
        localStorage.setItem("username", username);
        toast.success("Login successful!");
    };

    const handleRegister = async () => {
        const res = await fetch("http://localhost:5050/register", {
            method: "POST",
            headers: { "content-type": "application/json" },
            body: JSON.stringify({ username, password })
        });

        if(!res.ok) {
            toast.error("Register not successful!");
            return;
        }

        toast.success("User created!");
    };

    return(
        <div className="">
            <h1>Login</h1><br/>

            <div className="login-card">
                <input
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                    placeholder="Username"
                />
                <input
                    type="password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    placeholder="Password"
                />
                <button onClick={handleLogin}>Login</button>
                <button onClick={handleRegister}>Register</button>
            </div>

            <h1>New Chat Room</h1><br/>
            <div className="new-room-card">
                <input
                    className="new-room-input"
                    value={newRoom}
                    onChange={(e) => setNewRoom(e.target.value)}
                    placeholder="Room Name"/>
                <button onClick={createRoom}>Create Room</button>
            </div>

            <h1>Available Rooms</h1><br/>
            <div className="rooms-card">
                {rooms.map((room: string) => (
                    <button
                        key={room}
                        onClick={() => navigate(`/chat/${room}`)}
                        disabled={!isLoggedIn}
                        className="room-button">
                        {room}
                    </button>
                ))}
            </div>
        </div>
    )
}

export default Front;
