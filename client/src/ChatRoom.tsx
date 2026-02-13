import { useParams } from "react-router-dom";
import { Chat } from "./UI/Chat.tsx";

function ChatRoom() {
    const { roomName } = useParams<{ roomName: string }>();

    if (!roomName) {
        return <div>No room selected</div>;
    }

    return <Chat room={roomName} />;
}

export default ChatRoom;
