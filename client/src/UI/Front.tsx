import '../CSS/App.css'
import { useNavigate } from "react-router-dom";

function Front() {

    const rooms = ["Room1", "Room2", "Room3"];
    const navigate = useNavigate();

    return(
        <div className="front-container">
            <h1>Chat room</h1>
            {rooms.map((room) => (
                <button
                key={room}
                onClick={() => navigate(`/chat/${room}`)}
                style={{display: "block", margin: "10px 0"}}>
                    {room}
                </button>
            ))}
        </div>
    )
}

export default Front;